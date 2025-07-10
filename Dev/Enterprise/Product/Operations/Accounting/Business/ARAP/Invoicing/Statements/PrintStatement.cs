using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PrintStatement : StatementBase, IDocumentSupportable, ISourceIdentifierProvider
	{
		public class Constants : Statement.Constants
		{
			public const string CalculatedMatchedAmount = "CalculatedMatchedAmount";
			public const string DueDate = "DueDate";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
			public const string Description = "Description";
			public const string Statement = "STATEMENT";
		}

		public PrintStatement(BusinessObjectFactory factory, GlbBranch branch)
			: base(factory)
		{
			fBranch = branch;
		}

		public static new string TableName
		{
			get { return "PrintStatement"; }
		}

		public string GetDocumentName()
		{
			var result = string.Empty;

			switch (DocumentToPrint)
			{
				case Core.Constants.StatementCollectionLetterType.StatementOfAccount:
					result = AccountingMasterFilesRegistry.Instance.StatementDocumentName.Value;
					break;

				case Core.Constants.StatementCollectionLetterType.FirstReminder:
					result = AccountingMasterFilesRegistry.Instance.CollectionAndDemandFirstReminderDocumentName.Value;
					break;

				case Core.Constants.StatementCollectionLetterType.SecondReminder:
					result = AccountingMasterFilesRegistry.Instance.CollectionAndDemandSecondReminderDocumentName.Value;
					break;

				case Core.Constants.StatementCollectionLetterType.CollectionLetter:
					result = AccountingMasterFilesRegistry.Instance.CollectionLetterDocumentName.Value;
					break;

				case Core.Constants.StatementCollectionLetterType.DemandLetter:
					result = AccountingMasterFilesRegistry.Instance.DemandLetterDocumentName.Value;
					break;
			}

			return result;
		}

		#region Transaction Collection

		public virtual TransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = LoadTransactions<TransactionHeader>();
				}
				return fTransactions;
			}
		}
		protected TransactionHeaderCollection fTransactions;

		protected TransactionHeaderCollection LoadTransactions<T>()
			where T : TransactionHeader
		{
			DynamicBusinessObjectCollection dynamicTransactions = GetDynamicBusinessObjectCollection();
			var transactions = new TransactionHeaderCollection(Factory);

			var transactionPKs = from DynamicBusinessObject transaction in dynamicTransactions select new ZGuid(transaction[Constants.AH_PK]);

			if (transactionPKs.Any())
			{
				int batchSize =
#if DEBUG
							Globals.IsTest ? 5 :
#endif
							500;

				List<ZGuid> pks = transactionPKs.ToList();
				for (int i = 0; i < pks.Count; i = i + batchSize)
				{
					ZGuid[] batch = new ZGuid[i <= pks.Count - batchSize ? batchSize : pks.Count % batchSize];
					pks.CopyTo(i, batch, 0, batch.Length);
					transactions.AddRange(Factory.Load<T>(new ZQuery(AccTransactionHeaderSchema.PK, batch)));
				}

				// Sort Transactions collection to restore the original order of the DynamicTransactions
				Dictionary<ZGuid, int> sortDictionary = new Dictionary<ZGuid, int>(transactionPKs.Count());
				int j = 0;
				foreach (ZGuid pk in pks)
				{
					sortDictionary.Add(pk, j);
					j++;
				}

				transactions.Sort<TransactionHeader>((x, y) => sortDictionary[x.PK].CompareTo(sortDictionary[y.PK]));
			}

			return transactions;
		}

		public TransactionHeaderCollection LoadAttachmentInvoices()
		{
			return LoadTransactions<TransactionHeader>();
		}

		internal static string[] AttachmentTransactionTypes
		{
			get
			{
				return new string[] {
					ZArchitecture.Core.TransactionTypes.Invoice,
					ZArchitecture.Core.TransactionTypes.AdjustmentNote,
					ZArchitecture.Core.TransactionTypes.CreditNote,
					ZArchitecture.Core.TransactionTypes.InvoiceBatch
				};
			}
		}

		internal int TransactionCountForAttachment
		{
			get
			{
				return Transactions.Cast<TransactionHeader>().Count(x => AttachmentTransactionTypes.Any(y => y == x.AH_TransactionType));
			}
		}

		DynamicBusinessObjectCollection GetDynamicBusinessObjectCollection()
		{
			DynamicBusinessObjectCollection businessObjCollection = new DynamicBusinessObjectCollection(Factory);

			ZStringBuilder sqlString = new ZStringBuilder();
			ZSqlParameterCollection sqlParamaters = new ZSqlParameterCollection();
			AddStandardSQLString(sqlString, sqlParamaters);

			businessObjCollection.Load(sqlString.ToString(), sqlParamaters);

			return businessObjCollection;
		}

		protected virtual void AddAdditionalFilterForStatementOfAccount(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
		}

		protected virtual void AddStandardSQLString(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			AddUnmatchedNonBatchHeaderTransactionsSelect(sqlString, sqlParams);
			AddMatchedNonBatchHeaderTransactionsSelect(sqlString, sqlParams);
			AddBatchHeaderTranasctionsSelect(sqlString, sqlParams);

			sqlString.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"ORDER BY {0}.{1}, {2} , ", Constants.AccTransactionHeader, TransactionHeader.Schema.AH_InvoiceDate, Constants.AH_TransTypeIndex));
			sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_TransactionNum + " ASC");
		}

		#region Helper methods to build SQL

		bool isPreviousPeriod
		{
			get { return !EndOfPeriod.IsEmpty && EndOfPeriod < ZDateTime.Today; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddUnmatchedNonBatchHeaderTransactionsSelect(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			AddTransactionHeaderSelect(sqlString, sqlParams, "0");

			sqlString.Append("LEFT JOIN " + Constants.AccTransactionHeader + " AccTransactionHeaderInvoiceBatch ");
			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "ON {0}.{1} = AccTransactionHeaderInvoiceBatch.{2} ", Constants.AccTransactionHeader, Constants.AH_PK, Constants.AH_AH_InvoiceStatement));

			if (isPreviousPeriod)
			{
				sqlString.Append("LEFT JOIN " + Constants.AccTransactionMatchLink + " ON ");
				AddFilterForMatchDateOnOrBefore(sqlString, sqlParams);
				sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_PK + " = " + Constants.AP_AH + " ");
			}

			AddWhereClause(sqlString, sqlParams);

			if (isPreviousPeriod)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0} IS NULL ", Constants.AP_PK));
			}
			else
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} = {0}.{2} ", Constants.AccTransactionHeader, Constants.AH_LocalTotal, Constants.AH_OutstandingAmount));
			}

			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND AccTransactionHeaderInvoiceBatch.{0} IS NULL ", Constants.AH_AH_InvoiceStatement));

			if (!isPreviousPeriod)
			{
				AddWhereFilterForOutstandingAmount(sqlString, sqlParams);
			}

			sqlString.Append("UNION ALL ");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddMatchedNonBatchHeaderTransactionsSelect(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			string sumExpression = isPreviousPeriod ? Constants.AP_Amount :
				string.Format(CultureInfo.InvariantCulture, "{0}.{1} - {0}.{2}", Constants.AccTransactionHeader, Constants.AH_LocalTotal, Constants.AH_OutstandingAmount);
			AddTransactionHeaderSelect(sqlString, sqlParams, string.Format(CultureInfo.InvariantCulture, "SUM({0})", sumExpression));

			if (isPreviousPeriod)
			{
				sqlString.Append("INNER JOIN " + Constants.AccTransactionMatchLink + " ON ");
				AddFilterForMatchDateOnOrBefore(sqlString, sqlParams);
				sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_PK + " = " + Constants.AP_AH + " ");
			}

			sqlString.Append("LEFT JOIN " + Constants.AccTransactionHeader + " AccTransactionHeaderInvoiceBatch ");
			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "ON {0}.{1} = AccTransactionHeaderInvoiceBatch.{2} ", Constants.AccTransactionHeader, Constants.AH_PK, Constants.AH_AH_InvoiceStatement));

			AddWhereClause(sqlString, sqlParams);

			if (!isPreviousPeriod)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} <> {0}.{2} ", Constants.AccTransactionHeader, Constants.AH_LocalTotal, Constants.AH_OutstandingAmount));
				AddWhereFilterForOutstandingAmount(sqlString, sqlParams);
			}

			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND AccTransactionHeaderInvoiceBatch.{0} IS NULL ", Constants.AH_AH_InvoiceStatement));

			AddGroupBy(sqlString, sqlParams);

			if (isPreviousPeriod)
			{
				AddHavingFilterForOutstandingAmount(sqlString, sqlParams);
			}

			sqlString.Append("UNION ALL ");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddBatchHeaderTranasctionsSelect(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			string sumExpression = isPreviousPeriod ? string.Format(CultureInfo.InvariantCulture, "ISNULL({0}, 0)", Constants.AP_Amount) :
				string.Format(CultureInfo.InvariantCulture, "{0}.{1} - {0}.{2}", "AccTransactionHeaderInvoiceBatch", Constants.AH_LocalTotal, Constants.AH_OutstandingAmount);
			AddTransactionHeaderSelect(sqlString, sqlParams, string.Format(CultureInfo.InvariantCulture, "SUM({0})", sumExpression));

			sqlString.Append("INNER JOIN " + Constants.AccTransactionHeader + " AccTransactionHeaderInvoiceBatch ");
			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "ON {0}.{1} = AccTransactionHeaderInvoiceBatch.{2} ", Constants.AccTransactionHeader, Constants.AH_PK, Constants.AH_AH_InvoiceStatement));

			if (isPreviousPeriod)
			{
				sqlString.Append("LEFT JOIN " + Constants.AccTransactionMatchLink + " ON ");
				AddFilterForMatchDateOnOrBefore(sqlString, sqlParams);
				sqlString.Append("AccTransactionHeaderInvoiceBatch." + Constants.AH_PK + " = " + Constants.AP_AH + " ");
			}

			AddWhereClause(sqlString, sqlParams);

			AddGroupBy(sqlString, sqlParams);

			if (isPreviousPeriod)
			{
				AddHavingFilterForOutstandingAmount(sqlString, sqlParams);
			}
			else
			{
				sqlString.AppendLine();
				if (OutStandingAmountGreaterThan != 0)
				{
					sqlString.Append(string.Format(CultureInfo.InvariantCulture, "HAVING {0}.{1} ", Constants.AccTransactionHeader, Constants.AH_LocalTotal));
					sqlString.Append(string.Format(CultureInfo.InvariantCulture, "- SUM({0}.{1} - {0}.{2}) >= @OutstandingAmount ", "AccTransactionHeaderInvoiceBatch", Constants.AH_LocalTotal, Constants.AH_OutstandingAmount));
					AddParameter(sqlParams, ref OutstandingAmountParam, delegate { return ZSqlParameter.New("@OutstandingAmount", OutStandingAmountGreaterThan, AccTransactionHeaderSchema.AH_OutstandingAmount); });
				}
				else
				{
					sqlString.Append(string.Format(CultureInfo.InvariantCulture, "HAVING ({0}.{1} ", Constants.AccTransactionHeader, Constants.AH_LocalTotal));
					sqlString.Append(string.Format(CultureInfo.InvariantCulture, "- SUM({0}.{1} - {0}.{2})) <> 0 ", "AccTransactionHeaderInvoiceBatch", Constants.AH_LocalTotal, Constants.AH_OutstandingAmount));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddTransactionHeaderSelect(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams, string sumExpression)
		{
			sqlString.Append("SELECT " + Constants.AccTransactionHeader + "." + Constants.AH_PK + ", " + Constants.AccTransactionHeader + "." + Constants.AH_InvoiceDate + ", ");
			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "CHARINDEX({0}.{1}, @TransactionTypes) AS {2}, ", Constants.AccTransactionHeader, Constants.AH_TransactionType, Constants.AH_TransTypeIndex));
			AddParameter(sqlParams, ref TransactionTypesParam, delegate { return ZSqlParameter.New("@TransactionTypes", string.Format(CultureInfo.InvariantCulture, "'INV CRD ADJ CTR DSC EXX JNL OVP REC TRF'"), CargoWise.Schema.Schema.GenericStringSchemaColumn); });
			sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_TransactionNum + ", " + sumExpression + " AS " + Constants.CalculatedMatchedAmount + " ");

			sqlString.Append("FROM " + Constants.AccTransactionHeader + " ");

			sqlString.Append("INNER JOIN " + Constants.GlbCompany + " ");
			sqlString.Append("ON " + Constants.GlbCompany + "." + Constants.GC_PK + " = " + Constants.AccTransactionHeader + "." + Constants.AH_GC + " ");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddWhereClause(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			sqlString.Append("WHERE " + Constants.AccTransactionHeader + "." + Constants.AH_Ledger + " = @Ledger ");
			AddParameter(sqlParams, ref LedgerParam, delegate { return ZSqlParameter.New("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger); });
			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} IS NULL ", Constants.AccTransactionHeader, AccTransactionHeaderSchema.Constants.AH_AH_InvoiceStatement));

			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} IN (SELECT @Organisation ", Constants.AccTransactionHeader, Constants.AH_OH));
			AddParameter(sqlParams, ref OrganisationParam, delegate { return ZSqlParameter.New("@Organisation", OrganisationPK, AccTransactionHeaderSchema.AH_OH); });
			if (IssueBySettlementGroup)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, " UNION ALL SELECT {0} FROM {1} WHERE {2} = @Organisation AND {3} = @Company AND {4} = @ARS", Constants.PR_OH_Parent, Constants.RelatedParty, Constants.PR_OH_RelatedParty, Constants.PR_GC, Constants.PR_PartyType));
				AddParameter(sqlParams, ref ARSettlementParam, delegate { return ZSqlParameter.New("@ARSettlement", RelatedPartyTypeList.Codes.ARSettlementGroup, OrgRelatedPartySchema.PR_PartyType); });
			}
			sqlString.Append(") ");

			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} = @Currency ", Constants.AccTransactionHeader, Constants.AH_RX_NKTransactionCurrency));
			AddParameter(sqlParams, ref CurrencyParam, delegate { return ZSqlParameter.New("@Currency", CurrencyNK, AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency); });
			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0} = @Company ", Constants.GC_PK));

			AddParameter(sqlParams, ref CompanyParam, delegate { return ZSqlParameter.New("@Company", Company.PK, GlbCompanySchema.PK); });

			AddParameter(sqlParams, ref ARSRelatedPartyType, delegate { return ZSqlParameter.New("@ARS", RelatedPartyTypeList.Codes.ARSettlementGroup, OrgRelatedPartySchema.PR_PartyType); });

			AddFilterForTransactionBranch(sqlString, sqlParams);
			AddFilterForTransactionDepartment(sqlString, sqlParams);

			if (DocumentToPrint == Core.Constants.StatementCollectionLetterType.StatementOfAccount)
			{
				AddFilterForInvoiceDateOnOrBefore(sqlString, sqlParams);
				AddFilterForPostDateOnOrBefore(sqlString, sqlParams);
				AddAdditionalFilterForStatementOfAccount(sqlString, sqlParams);
			}
			else
			{
				AddFilterForDueDatesOnOrBefore(sqlString, sqlParams);
			}

			AddFilterForDisbursementInvoicesOnly(sqlString, sqlParams);

			AddFilterForTransactionsInActiveBatch(sqlString, sqlParams);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddGroupBy(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			sqlString.Append(string.Format(CultureInfo.InvariantCulture, "GROUP BY {0}.{1}, ", Constants.AccTransactionHeader, Constants.AH_PK));
			sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_InvoiceDate + ", ");
			sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_TransactionType + ", ");
			sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_TransactionNum + ", ");
			sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_LocalTotal + " ");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddWhereFilterForOutstandingAmount(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (OutStandingAmountGreaterThan != 0)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} >= @OutstandingAmount ", Constants.AccTransactionHeader, Constants.AH_OutstandingAmount));
				AddParameter(sqlParams, ref OutstandingAmountParam, delegate { return ZSqlParameter.New("@OutstandingAmount", OutStandingAmountGreaterThan, AccTransactionHeaderSchema.AH_OutstandingAmount); });
			}
			else
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} IS NULL ", Constants.AccTransactionHeader, Constants.AH_FullyPaidDate));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddFilterForTransactionBranch(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (IssueByTransactionBranch && TransactionBranchPK.IsValid)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} = @TransactionBranch ", Constants.AccTransactionHeader, Constants.AH_GB));
				AddParameter(sqlParams, ref TransactionBranchParam, delegate { return ZSqlParameter.New("@TransactionBranch", TransactionBranchPK, AccTransactionHeaderSchema.AH_GB); });
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddFilterForTransactionDepartment(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (IssueByTransactionDepartment && TransactionDepartmentPK.IsValid)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} = @TransactionDepartment ", Constants.AccTransactionHeader, Constants.AH_GE));
				AddParameter(sqlParams, ref TransactionDepartmentParam, delegate { return ZSqlParameter.New("@TransactionDepartment", TransactionDepartmentPK, AccTransactionHeaderSchema.AH_GE); });
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddFilterForInvoiceDateOnOrBefore(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (CutOffDate.IsValid)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} < @CutOffDate ", Constants.AccTransactionHeader, Constants.AH_InvoiceDate));
				AddParameter(sqlParams, ref CutOffDateParam, delegate { return ZSqlParameter.New("@CutOffDate", CutOffDate, AccTransactionHeaderSchema.AH_InvoiceDate); });
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddFilterForPostDateOnOrBefore(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (EndOfPeriod.IsValid)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} < @PostDateOnOrBefore ", Constants.AccTransactionHeader, Constants.AH_PostDate));
				AddParameter(sqlParams, ref PostDateOnOrBeforeParam, delegate { return ZSqlParameter.New("@PostDateOnOrBefore", EndOfPeriod, AccTransactionHeaderSchema.AH_PostDate); });
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddFilterForMatchDateOnOrBefore(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (EndOfPeriod.IsValid)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "{0} < @MatchDateOnOrBefore AND ", Constants.AP_MatchDate));
				AddParameter(sqlParams, ref MatchDateOnOrBeforeParam, delegate { return ZSqlParameter.New("@MatchDateOnOrBefore", EndOfPeriod, AccTransactionMatchLinkSchema.AP_MatchDate); });
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddFilterForDueDatesOnOrBefore(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (!CutOffDate.IsEmpty)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} < @CutOffDate ", Constants.AccTransactionHeader, Constants.AH_DueDate));
				AddParameter(sqlParams, ref CutOffDateParam, delegate { return ZSqlParameter.New("@CutOffDate", CutOffDate, AccTransactionHeaderSchema.AH_DueDate); });
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddFilterForDisbursementInvoicesOnly(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (DisbursementInvoicesOnly)
			{
				ZStringBuilder disbursementInStatement = new ZStringBuilder();
				string[] disbursementTypes = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes;
				for (int i = 1; i <= disbursementTypes.Length; i++)
				{
					string parameterName = "@Disbursement" + i;
					disbursementInStatement.Append(parameterName);
					if (i < disbursementTypes.Length)
					{
						disbursementInStatement.Append(", ");
					}
					IsDisbursementParam = null;
					AddParameter(sqlParams, ref IsDisbursementParam, delegate { return ZSqlParameter.New(parameterName, disbursementTypes[i - 1], AccTransactionHeaderSchema.AH_TransactionCategory); });
				}
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "AND {0}.{1} IN ({2}) ", Constants.AccTransactionHeader, Constants.AH_TransactionCategory, disbursementInStatement));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL command")]
		void AddHavingFilterForOutstandingAmount(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			if (OutStandingAmountGreaterThan != 0)
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "HAVING {0}.{1} ", Constants.AccTransactionHeader, Constants.AH_LocalTotal));
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, " - SUM(ISNULL({0}, 0)) >= @OutstandingAmount ", Constants.AP_Amount));
				AddParameter(sqlParams, ref OutstandingAmountParam, delegate { return ZSqlParameter.New("@OutstandingAmount", OutStandingAmountGreaterThan, AccTransactionHeaderSchema.AH_OutstandingAmount); });
			}
			else
			{
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "HAVING {0}.{1} ", Constants.AccTransactionHeader, Constants.AH_LocalTotal));
				sqlString.Append(string.Format(CultureInfo.InvariantCulture, "!= SUM(ISNULL({0}, 0)) ", Constants.AP_Amount));
			}
		}

		#endregion

		#region Parameters support

		protected ZSqlParameter TransactionTypesParam;
		protected ZSqlParameter LedgerParam;
		protected ZSqlParameter OrganisationParam;
		protected ZSqlParameter CurrencyParam;
		protected ZSqlParameter CompanyParam;
		protected ZSqlParameter ARSRelatedPartyType;
		protected ZSqlParameter PostDateOnOrBeforeParam;

		ZSqlParameter ARSettlementParam;
		ZSqlParameter MatchDateOnOrBeforeParam;
		ZSqlParameter TransactionBranchParam;
		ZSqlParameter TransactionDepartmentParam;
		ZSqlParameter CutOffDateParam;
		ZSqlParameter IsDisbursementParam;
		ZSqlParameter OutstandingAmountParam;

		#endregion

		#endregion

		#region Properties

		#region IssueBySettlementGroup

		public bool IssueBySettlementGroup
		{
			get { return fIssueBySettlementGroup; }
			set { fIssueBySettlementGroup = value; }
		}

		bool fIssueBySettlementGroup;

		#endregion

		#region IssueByTransactionBranch

		public bool IssueByTransactionBranch
		{
			get { return fIssueByTransactionBranch; }
			set { fIssueByTransactionBranch = value; }
		}

		bool fIssueByTransactionBranch;

		#endregion

		#region IssueByTransactionDepartment

		public bool IssueByTransactionDepartment
		{
			get { return fIssueByTransactionDepartment; }
			set { fIssueByTransactionDepartment = value; }
		}

		bool fIssueByTransactionDepartment;

		#endregion

		#region Statement Display Date

		public ZDateTime StatementDisplayDate
		{
			get { return fStatementDisplayDate; }
			set { fStatementDisplayDate = value; }
		}

		ZDateTime fStatementDisplayDate;

		#endregion

		#region DocumentToPrint

		public ZString DocumentToPrint
		{
			get { return fDocumentToPrint; }
			set { fDocumentToPrint = value; }
		}

		ZString fDocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
		#endregion

		#region Currency

		public ZString CurrencyNK
		{
			get { return fCurrencyNK; }
			set { fCurrencyNK = value; }
		}

		protected ZString fCurrencyNK = ZString.Empty;

		#endregion

		#region TransactionBranch

		public ZGuid TransactionBranchPK
		{
			get { return fTransactionBranchPK; }
			set { fTransactionBranchPK = value; }
		}

		protected ZGuid fTransactionBranchPK = ZGuid.Empty;

		#endregion

		#region TransactionDepartment

		public ZGuid TransactionDepartmentPK
		{
			get { return fTransactionDepartmentPK; }
			set { fTransactionDepartmentPK = value; }
		}

		protected ZGuid fTransactionDepartmentPK = ZGuid.Empty;

		#endregion

		#region Company

		public GlbCompany Company
		{
			get { return Branch.Company; }
		}

		#endregion

		#region Branch

		public GlbBranch Branch
		{
			get { return fBranch; }
		}
		readonly GlbBranch fBranch;

		#endregion

		#region Organisation

		public ZGuid OrganisationPK
		{
			get { return fOrganisationPK; }
			set { fOrganisationPK = value; }
		}

		protected ZGuid fOrganisationPK = ZGuid.Empty;

		public OrgHeader Organisation
		{
			get
			{
				return Factory.Load<OrgHeader>(OrganisationPK);
			}
		}

		#endregion

		#region CutOffDate

		public ZDateTime CutOffDate
		{
			get { return fCutOffDate; }
			set { fCutOffDate = value; }
		}

		protected ZDateTime fCutOffDate = ZDateTime.Empty;

		#endregion

		#region EndOfPeriod

		public ZDateTime EndOfPeriod
		{
			get { return fEndOfPeriod; }
			set { fEndOfPeriod = value; }
		}

		protected ZDateTime fEndOfPeriod = ZDateTime.Today.AddDays(1);

		#endregion

		#region OutStandingAmountGreaterThan

		public ZDecimal OutStandingAmountGreaterThan
		{
			get { return fOutStandingAmountGreaterThan; }
			set { fOutStandingAmountGreaterThan = value; }
		}

		ZDecimal fOutStandingAmountGreaterThan = 0;

		#endregion

		#region Disbursement Invoices only

		public bool DisbursementInvoicesOnly
		{
			get { return fDisbursementInvoicesOnly; }
			set { fDisbursementInvoicesOnly = value; }
		}

		bool fDisbursementInvoicesOnly;

		#endregion

		#endregion

		#region Function
		public virtual TransactionHeaderCollection GetInvoicesForAttachment()
		{
			return Transactions;
		}
		#endregion

		#region IDocumentSupportable Members
		public DocumentSupporter DocumentSupporter
		{
			get { return new PrintStatementDocumentSupporter(this); }
		}
		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => OrganisationPK;
	}

	public class PrintStatementDocumentSupporter : DocumentSupporter
	{
		public PrintStatementDocumentSupporter(PrintStatement statement)
			: base(statement)
		{
		}

		public static PrintStatementDocumentSupporter New(PrintStatement statement)
		{
			PrintStatementDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(statement);
			}
			else if (statement != null)
			{
				result = new PrintStatementDocumentSupporter(statement);
			}

			return result;
		}

		public PrintStatement PrintStatement
		{
			get { return (PrintStatement)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Statement; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, PrintStatement);
			}
			else if (dataContext == Core.Constants.DataContext.Statement)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(dataContext, PrintStatement) };
			}
			else
			{
				return null;
			}
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.Statement };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			OrgHeader orgHeader = Factory.Load<OrgHeader>(PrintStatement.OrganisationPK);
			return new OrgHeaderContact(orgHeader, null);
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair titleCopy = null;

			var value = PrintStatement.GetDocumentName();
			if (!string.IsNullOrEmpty(value))
			{
				titleCopy = new TitleCopyCountPair(value);
			}

			return titleCopy;
		}

		#endregion

		protected delegate PrintStatementDocumentSupporter NewDelegate(PrintStatement printStatement);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
	}
}
