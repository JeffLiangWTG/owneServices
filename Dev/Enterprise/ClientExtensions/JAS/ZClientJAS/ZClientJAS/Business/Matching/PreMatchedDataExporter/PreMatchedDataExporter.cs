using System;
using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Matching
{
	public class PreMatchedDataExporter : JASDataExporterBizO
	{
		#region Schema

		public new class Schema : JASDataExporterBizO.Schema
		{
			public const string NettingCycle = "NettingCycle";
		}

		#endregion

		public void CancelExport()
		{
			IsCancelled = true;
		}

		#region Validation

		public new PreMatchedDataExporterValidation Validation
		{
			get { return (PreMatchedDataExporterValidation)base.Validation; }
		}

		protected override JASDataExporterBizOValidation GetNewJASDataExporterBizOValidation()
		{
			return new PreMatchedDataExporterValidation(this);
		}

		#endregion

		#region Export

		public virtual void Export()
		{
			string aPFileFullPath = Path.Combine(Env.TempPath, string.Format("{0}{1:yMM}.txt", apNettingCodeForFileName, NettingCycleDate));
			string aRFileFullPath = Path.Combine(Env.TempPath, string.Format("{0}{1:yMM}.txt", arNettingCodeForFileName, NettingCycleDate));

			using (StreamWriter aPWriter = new StreamWriter(aPFileFullPath))
			using (StreamWriter aRWriter = new StreamWriter(aRFileFullPath))
			{
				ExportLines(accTransactions, aPWriter, aRWriter);
			}

			DeliverFiles(aRFileFullPath, aPFileFullPath);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override string EmailSubject
		{
			get { return "AR and AP Pre-Matching files from CargoWise One"; }
		}

		#endregion

		#region NettingCycle

		[MaxLength(9)]
		public ZString NettingCycle
		{
			get { return fNettingCycle; }
			set
			{
				CheckMaximumLength(NettingCycleInfo, value);
				SetNonPersistentPropertyValue(NettingCycleInfo, ref fNettingCycle, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNettingCycle();
				}
			}
		}

		public ZPropertyInfo NettingCycleInfo
		{
			get { return GetZPropertyInfo(nameof(NettingCycle)); }
		}

		public ReadOnlyCodeDescriptionPairList NettingCycleList
		{
			get { return JASDataRegistry.Instance.NettingCycleListItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "not an issue with Globalization as InvariantCulture is used")]
		public ZDateTime NettingCycleDate
		{
			get
			{
				return (NettingCycleInfo.HasErrors())
					? ZDateTime.Empty
					: new ZDateTime(DateTime.ParseExact(NettingCycle, "dd-MMM-yy", CultureInfo.InvariantCulture));
			}
		}

		public ZDateTime CycleClosingDate
		{
			get
			{
				return (NettingCycleDate.IsEmpty)
					? ZDateTime.Empty
					: NettingCycleDate.AddDays(-NettingCycleDate.Day + JASDataRegistry.Instance.NettingPaymentTerms);
			}
		}

		ZString fNettingCycle;

		#endregion

		#region Implementation

		virtual public bool HasMinimumRequirements
		{
			get
			{
				arNettingCodeForFileName = (GlbCompany.CurrentCompany.OrgProxy != null) ? GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetUNC() : ZString.Empty;
				apNettingCodeForFileName = GetAPNettingCodeForFileName(arNettingCodeForFileName);
				accTransactions = GetTransactions();

				return !arNettingCodeForFileName.IsEmpty && !apNettingCodeForFileName.IsEmpty && accTransactions != null;
			}
		}
		ZString arNettingCodeForFileName;
		ZString apNettingCodeForFileName;
		AccTransactionHeaderWithJobInfo[] accTransactions;

		AccTransactionHeaderWithJobInfo[] GetTransactions()
		{
			ZQuery filter = ConstructTransactionFilter();
			return (AccTransactionHeaderWithJobInfo[])Factory.Load(typeof(AccTransactionHeaderWithJobInfo), filter);
		}

		ZQuery ConstructTransactionFilter()
		{
			ZQuery result = new ZQuery();

			ZQuery ledgerQuery = new ZQuery();
			ledgerQuery.AddToFilter(JoinCondition.Or, ClientAccTransactionHeaderWithJobInfoSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			ledgerQuery.AddToFilter(JoinCondition.Or, ClientAccTransactionHeaderWithJobInfoSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);

			ZDBOnlyQuery orgQuery = new ZDBOnlyQuery(typeof(AccTransactionHeaderWithJobInfo));
			ZDBOnlySubQuery nettingOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			nettingOrgSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.UniversalNettingCode);
			nettingOrgSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, SQLComparisonOperator.NotEqual, "");
			orgQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, OrgCusCodeSchema.OK_OH, nettingOrgSubQuery, JoinCondition.And);

			ZQuery transactionTypeQuery = new ZQuery(ClientAccTransactionHeaderWithJobInfoSchema.AH_TransactionType, TransactionTypes.Invoice);
			transactionTypeQuery.DefaultJoinCondition = JoinCondition.Or;
			transactionTypeQuery.AddToFilter(ClientAccTransactionHeaderWithJobInfoSchema.AH_TransactionType, TransactionTypes.CreditNote);
			transactionTypeQuery.AddToFilter(ClientAccTransactionHeaderWithJobInfoSchema.AH_TransactionType, TransactionTypes.AdjustmentNote);

			ZQuery currentCompanyQuery = new ZQuery();
			currentCompanyQuery.DefaultJoinCondition = JoinCondition.Or;
			foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
			{
				currentCompanyQuery.AddToFilter(ClientAccTransactionHeaderWithJobInfoSchema.AH_GB, branch.PK);
			}

			result.AddToFilter(ClientAccTransactionHeaderWithJobInfoSchema.AH_DueDate, SQLComparisonOperator.LessThanOrEqualTo, CycleClosingDate);
			result.AddToFilter(ClientAccTransactionHeaderWithJobInfoSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, 0);
			result.AddToFilter(ledgerQuery);
			result.AddToFilter(orgQuery);
			result.AddToFilter(transactionTypeQuery);
			result.AddToFilter(currentCompanyQuery);

			return result;
		}

		protected ZString GetAPNettingCodeForFileName(ZString code)
		{
			ZString result;
			if (code.EndsWith("COR"))
			{
				result = "COR" + code.Replace("COR", "");
			}
			else
			{
				result = code.Right(3) + ZArchitecture.Core.LedgerTypes.AccountsPayable;
			}

			return result;
		}

		void ExportLines(AccTransactionHeaderWithJobInfo[] transactions, StreamWriter aPWriter, StreamWriter aRWriter)
		{
			for (int i = 0; i < transactions.Length; i++)
			{
				AccTransactionHeaderWithJobInfo transaction = transactions[i];
				if (IsCancelled)
				{
					break;
				}

				PreMatchedDataLine line;
				StreamWriter writerToUse;
				if (transaction.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					line = new APDataLine(transaction, NettingCycleDate);
					writerToUse = aPWriter;
				}
				else
				{
					line = new ARDataLine(transaction);
					writerToUse = aRWriter;
				}

				writerToUse.WriteLine(line.ToString());
				PreMatchedDataExporter_OnProgress(this, new SharedProgressEventArgs(i, transactions.Length, "Updating delcaration flight details..."));
			}
		}

		protected bool IsCancelled;

		#endregion

		void PreMatchedDataExporter_OnProgress(object sender, SharedProgressEventArgs e)
		{
			if (OnProgress != null)
			{
				OnProgress(this, e);
			}
		}

		public event SharedProgressEventHandler OnProgress;
	}
}
