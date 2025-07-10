using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.Netting
{
	public static class NettingHelper
	{
		public static Dictionary<ZString, ZDecimal> ReadExchangeRatesFromDatabase(NettingSystemPeriod period, ZString rateType, BusinessObjectFactory factory)
		{
			var query = new ZQuery(NettingSystemExchangeRateSchema.NER_NS_NettingSystem, period.NSP_NS_NettingSystem);
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RateType, rateType);
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_NSP_Period, period.PK.ToGuid());
			query.OrderBy = NettingSystemExchangeRateSchema.Constants.NER_NSP_Period + " DESC";

			var exchangeRates = factory.Load<NettingSystemExchangeRate>(query);
			var exchangeRatesDictionary = new Dictionary<ZString, ZDecimal>();

			exchangeRatesDictionary[period.NettingSystem.Company.GC_RX_NKLocalCurrency] = 1m;

			foreach (var exchangeRate in exchangeRates)
			{
				if (!exchangeRatesDictionary.ContainsKey(exchangeRate.NER_RX_NKCurrency))
				{
					exchangeRatesDictionary.Add(exchangeRate.NER_RX_NKCurrency, exchangeRate.NER_Rate);
				}
			}

			return exchangeRatesDictionary;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void MoveUnmatchedTransactionsToTheNextPeriod(NettingSystemPeriod currentPeriod, BusinessObjectFactory factory, DbConnection connection)
		{
			var nextPeriod = NettingPeriodHelper.GetNextOpenPeriod(currentPeriod, factory);

			if (nextPeriod != null)
			{
				using (var cmd = connection.Command("NettingMoveUnmatchedTransactionsToNextPeriod"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@CurrentPeriod", SqlDbType.UniqueIdentifier, currentPeriod.PK.ToGuid());
					cmd.AddParameter("@NextPeriod", SqlDbType.UniqueIdentifier, nextPeriod.PK.ToGuid());
					cmd.AddParameter("@CurrentUser", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());

					cmd.ExecuteProcedureWithReturnValue();
				}
			}
			else
			{
				throw new IncorrectDataSetupException(Res.GetString("8b5252bc-81d6-4e9c-9c1d-522515ce8fa2", "No Netting Cycle found after current open Cycle: {0}. The next Netting Cycle is required for moving unmatched transactions from current Cycle since Latest Approval Date has elapsed for the current cycle.", currentPeriod.NSP_Period));
			}
		}

		public static ZString GetRecipienteHubIDFromUniversalTransaction(UniversalTransaction universalTransaction)
		{
			var receivingParticipantEHubId = ZString.Empty;
			if (universalTransaction != null && universalTransaction.OrganizationAddress != null && universalTransaction.OrganizationAddress.RegistrationNumberCollection != null)
			{
				var receivingParticipantSettings = universalTransaction.OrganizationAddress.RegistrationNumberCollection.Where(x => x.Type.Code.Value == OrgCusCode.CodeTypes.EHubOrganisationID);

				foreach (var receivingParticipantSetting in receivingParticipantSettings)
				{
					receivingParticipantEHubId = receivingParticipantSetting.Value ?? ZString.Empty;

					if (!receivingParticipantEHubId.IsEmpty)
					{
						break;
					}
				}

				if (receivingParticipantEHubId.IsEmpty)
				{
					throw new MalformedUniversalXmlException(Res.GetString("794180cb-6588-41a8-90d1-f0de5d7c2a68", "Participant eHub ID is missing in Universal XML."));
				}
			}

			return receivingParticipantEHubId;
		}

		public static NettingOrganisation GetNettingOrganization(ZGuid orgPK, BusinessObjectFactory factory)
		{
			var nettingOrgQuery = new ZQuery(NettingOrganisationSchema.NSO_OH_Organisation, orgPK);
			return factory.LoadTop1<NettingOrganisation>(nettingOrgQuery);
		}

		public static NettingOrganisation GetNettingOrganizationByEHubID(ZString eHubID, BusinessObjectFactory factory)
		{
			var nettingOrgQuery = new ZDBOnlyQuery(typeof(NettingOrganisation));
			nettingOrgQuery.AddSubQuery(NettingOrganisationSchema.NSO_OH_Organisation, GetNettingOrgSubQueryFromEHubID(eHubID), JoinCondition.And);

			return factory.LoadTop1<NettingOrganisation>(nettingOrgQuery);
		}

		public static NettingReceivableTransaction GetNettingReceivableTransaction(ZString transactionType, ZString invoiceNumber, ZString eHubID, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(NettingReceivableTransaction));
			query.AddToFilter(NettingReceivableTransactionSchema.NRT_TransactionType, transactionType);
			query.AddToFilter(NettingReceivableTransactionSchema.NRT_Reference, invoiceNumber);

			var issuerSubQuery = new ZDBOnlySubQuery(typeof(NettingOrganisation), NettingOrganisationSchema.PK);
			issuerSubQuery.AddSubQuery(NettingOrganisationSchema.NSO_OH_Organisation, GetNettingOrgSubQueryFromEHubID(eHubID), JoinCondition.And);

			query.AddSubQuery(NettingReceivableTransactionSchema.NRT_NSO_Issuer, issuerSubQuery, JoinCondition.And);

			return factory.LoadTop1<NettingReceivableTransaction>(query);
		}

		public static NettingPayableTransaction GetNettingPayableTransaction(ZString transactionType, ZString internalReferenceNumber, ZString eHubID, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(NettingPayableTransaction));
			query.AddToFilter(NettingPayableTransactionSchema.NPT_TransactionType, transactionType);

			var internalRefSubquery = new ZDBOnlySubQuery(typeof(NettingPayableTransactionRef), NettingPayableTransactionRefSchema.NPR_NPT_Transaction);
			internalRefSubquery.AddToFilter(NettingPayableTransactionRefSchema.NPR_Type, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber);
			internalRefSubquery.AddToFilter(NettingPayableTransactionRefSchema.NPR_Reference, internalReferenceNumber);

			query.AddSubQuery(internalRefSubquery, JoinCondition.And);

			var recipientSubQuery = new ZDBOnlySubQuery(typeof(NettingOrganisation), NettingOrganisationSchema.PK);
			recipientSubQuery.AddSubQuery(NettingOrganisationSchema.NSO_OH_Organisation, GetNettingOrgSubQueryFromEHubID(eHubID), JoinCondition.And);

			query.AddSubQuery(NettingPayableTransactionSchema.NPT_NSO_Recipient, recipientSubQuery, JoinCondition.And);

			return factory.LoadTop1<NettingPayableTransaction>(query);
		}

		public static NettingReceivableTransaction GetNettingReceivableTransaction(ZGuid issuerPK, ZGuid recipientPK, ZString primaryReference, ZString currency, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(NettingReceivableTransaction));
			query.AddToFilter(NettingReceivableTransactionSchema.NRT_Reference, primaryReference);
			query.AddToFilter(NettingReceivableTransactionSchema.NRT_NSO_Issuer, issuerPK.ToGuid());
			query.AddToFilter(NettingReceivableTransactionSchema.NRT_NSO_Recipient, recipientPK.ToGuid());
			query.AddToFilter(NettingReceivableTransactionSchema.NRT_RX_NKInvoiceCurrency, currency);

			return factory.LoadTop1<NettingReceivableTransaction>(query);
		}

		public static NettingPayableTransaction GetNettingPayableTransaction(ZGuid issuerPK, ZGuid recipientPK, ZString primaryReference, ZString currency, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(NettingPayableTransaction));
			query.AddToFilter(NettingPayableTransactionSchema.NPT_Reference, primaryReference);
			query.AddToFilter(NettingPayableTransactionSchema.NPT_NSO_Issuer, issuerPK.ToGuid());
			query.AddToFilter(NettingPayableTransactionSchema.NPT_NSO_Recipient, recipientPK.ToGuid());
			query.AddToFilter(NettingPayableTransactionSchema.NPT_RX_NKInvoiceCurrency, currency);

			return factory.LoadTop1<NettingPayableTransaction>(query);
		}

		static ZDBOnlySubQuery GetNettingOrgSubQueryFromEHubID(ZString eHubID)
		{
			var cusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, eHubID);
			cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.EHubOrganisationID);

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddSubQuery(cusCodeSubQuery, JoinCondition.And);

			return orgHeaderSubQuery;
		}

		public static OrgHeader GetOrgHeaderByEhubID(ZString eHubID, BusinessObjectFactory factory)
		{
			var nettingParticipant = GetNettingOrganizationByEHubID(eHubID, factory);
			return nettingParticipant?.Organisation;
		}

		public static Guid GetNettingPeriod(NettingSystem nettingSystem, ZDateTime transactionDate, bool isReceivable)
		{
			var period = NettingPeriodHelper.GetNettingPeriod(nettingSystem, transactionDate, isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable);

			if (period == Guid.Empty)
			{
				throw new IncorrectDataSetupException(Res.GetString("9e2ef9cb-14b4-43fe-92ba-c4c39312f356",
					"No Open Netting Period found for Netting System '{0}' at UTC date: '{1}'.", nettingSystem.NS_Code, transactionDate));
			}

			return period;
		}

		public static NettingSystem GetNettingSystem(BusinessObjectFactory factory, ZString eHubID)
		{
			var eHubCusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			eHubCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.EHubOrganisationID);
			eHubCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, eHubID);

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddSubQuery(eHubCusCodeQuery, JoinCondition.And);

			var nettingOrgSubQuery = new ZDBOnlySubQuery(typeof(NettingOrganisation), NettingOrganisationSchema.NSO_NS_NettingSystem);
			nettingOrgSubQuery.AddSubQuery(NettingOrganisationSchema.NSO_OH_Organisation, orgHeaderSubQuery, JoinCondition.And);

			var nettingSystemQuery = new ZDBOnlyQuery(typeof(NettingSystem));
			nettingSystemQuery.AddSubQuery(NettingSystemSchema.PK, nettingOrgSubQuery, JoinCondition.And);

			return factory.LoadTop1<NettingSystem>(nettingSystemQuery);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static IEnumerable<NettingClearingJournal> GetAllTransactionsForParticipantJournalCreation(ZGuid nettingPeriod, DbConnection connection, NettingStatement statement = null)
		{
			var result = new List<NettingClearingJournal>();

			using (var table = new DataTable())
			{
				table.Locale = CultureInfo.InvariantCulture;
				var sql = @"SELECT Ledger, OrgCode, ParticipantOrgCode, TransactionCurrency, TransactionAmount, TransactionReference, OrgPK, ParticipantOrgPK, TransactionType, Description, OfferOrRequest, TransactionPK, CompanyCode, ParticipantCompanyCode
							FROM fn_NettingGetTransactionsForClearingJournal (@NettingPeriod)";

				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, nettingPeriod.ToGuid());
					using (var dataAdpter = cmd.NewDataAdapter())
					{
						dataAdpter.Fill(table);
					}
				}

				foreach (DataRow row in table.Rows)
				{
					result.Add(GetTransactionInfo(row, statement));
				}
			}

			return result;
		}

		static NettingClearingJournal GetTransactionInfo(DataRow row, NettingStatement statement = null)
		{
			return new NettingClearingJournal(statement)
			{
				Ledger = row[0] != DBNull.Value ? row[0].ToString() : string.Empty,
				OrgCode = row[1] != DBNull.Value ? row[1].ToString() : string.Empty,
				ParticipatingOrgCode = row[2] != DBNull.Value ? row[2].ToString() : string.Empty,
				TransactionCurrency = row[3] != DBNull.Value ? row[3].ToString() : string.Empty,
				TransactionAmount = row[4] != DBNull.Value ? ZDecimal.ParseSafe(row[4].ToString(), 0) : ZDecimal.Zero,
				TransactionReference = row[5] != DBNull.Value ? row[5].ToString() : string.Empty,
				OrgPK = row[6] != DBNull.Value ? Guid.Parse(row[6].ToString()) : Guid.Empty,
				ParticipantOrgPK = row[7] != DBNull.Value ? Guid.Parse(row[7].ToString()) : Guid.Empty,
				TransactionType = row[8] != DBNull.Value ? row[8].ToString() : string.Empty,
				Description = row[9] != DBNull.Value ? row[9].ToString() : string.Empty,
				OfferOrRequest = row[10] != DBNull.Value && Convert.ToBoolean(row[10], CultureInfo.InvariantCulture),
				TransactionPK = row[11] != DBNull.Value ? Guid.Parse(row[11].ToString()) : Guid.Empty,
				CompanyCode = row[12] != DBNull.Value ? row[12].ToString() : string.Empty,
				ParticipatingCompanyCode = row[13] != DBNull.Value ? row[13].ToString() : string.Empty
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static IEnumerable<NettingMovement> GetNettingMovements(BusinessObjectFactory factory, NettingSystemPeriod period, bool isFinal)
		{
			var nettingMovements = new List<NettingMovement>();
			var sql = @"SELECT 
	CompanyCode,
	Currency,
	Direction,
	Amount,
	SignedAmount,
	NettingCurrency,
	ReportingRate,
	ReportingAmount,
	DealtRate,
	DealtAmount,
	NettingType,
	OfferOrRequest
FROM 
	fn_NettingGetNetMovementDetails(@NettingPeriod, @IsFinal)";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, period != null ? period.PK.ToGuid() : Guid.Empty);
				cmd.AddParameter("@IsFinal", SqlDbType.Bit, isFinal);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						nettingMovements.Add(new NettingMovement(factory)
						{
							CompanyCode = !reader.IsDBNull(0) ? reader.GetString(0) : string.Empty,
							Currency = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
							Direction = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
							MovementAmount = !reader.IsDBNull(3) ? reader.GetDecimal(3) : 0,
							SignedMovementAmount = !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0,
							NettingCurrency = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty,
							ReportingRate = !reader.IsDBNull(6) ? reader.GetDecimal(6) : 0,
							NettingCurrencyReportingAmount = !reader.IsDBNull(7) ? reader.GetDecimal(7) : 0,
							Dealtrate = !reader.IsDBNull(8) ? reader.GetDecimal(8) : 0,
							NettingCurrencyDealtAmount = !reader.IsDBNull(9) ? reader.GetDecimal(9) : 0,
							NettingType = !reader.IsDBNull(10) ? reader.GetString(10) : string.Empty,
							MovementType = !reader.IsDBNull(11) ? !string.IsNullOrEmpty(reader.GetString(11)) ? string.Equals(reader.GetString(11), "O", StringComparison.OrdinalIgnoreCase) ? MovementType.Offer : MovementType.Request : MovementType.Transaction : MovementType.Transaction
						});
					}
				}
			}

			return nettingMovements;
		}

		public static List<ZString> GetIssuerParticipantEHubIds(NettingSystemPeriod period)
		{
			var sql = @"select OK_CustomsRegNo IssuerEHubID
	from dbo.NettingReceivableTransaction
		INNER JOIN dbo.NettingOrganisation ON NSO_PK = NRT_NSO_Issuer		
		LEFT JOIN dbo.OrgCusCode ON OK_OH = NSO_OH_Organisation AND OK_CodeType = 'HID'
	where NRT_NSP_Period = @NettingPeriod
	group by OK_CustomsRegNo";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@NettingPeriod", period != null ? period.PK.ToGuid() : Guid.Empty, NettingReceivableTransactionSchema.NRT_NSP_Period);

			var collection = new DynamicBusinessObjectCollection(period.Factory);
			collection.Load(sql, parameters);

			var issuerEHubIds = new List<ZString>();
			issuerEHubIds = collection.Select(x => (ZString)x["IssuerEHubID"]).ToList();

			return issuerEHubIds;
		}

		public static List<ZString> GetRecipientParticipantEHubIds(NettingSystemPeriod period)
		{
			var sql = @"select OK_CustomsRegNo RecipientEHubID
	from dbo.NettingReceivableTransaction
		INNER JOIN dbo.NettingOrganisation ON NSO_PK = NRT_NSO_Recipient
		LEFT JOIN dbo.OrgCusCode ON OK_OH = NSO_OH_Organisation AND OK_CodeType = 'HID'
	where NRT_NSP_Period = @NettingPeriod
	group by OK_CustomsRegNo";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@NettingPeriod", period != null ? period.PK.ToGuid() : Guid.Empty, NettingReceivableTransactionSchema.NRT_NSP_Period);

			var collection = new DynamicBusinessObjectCollection(period.Factory);
			collection.Load(sql, parameters);

			var recipientEHubIds = new List<ZString>();
			recipientEHubIds = collection.Select(x => (ZString)x["RecipientEHubID"]).ToList();

			return recipientEHubIds;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void UnmatchTransaction(DbConnection connection, Guid period, INettingTransaction transactionToUnmatch, ZString newStatus)
		{
			using (var cmd = connection.Command("NettingUnmatchTransaction"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, period);
				cmd.AddParameter("@TransactionToUnmatch", SqlDbType.UniqueIdentifier, transactionToUnmatch.PK.ToGuid());
				cmd.AddParameter("@Ledger", SqlDbType.VarChar, transactionToUnmatch is NettingReceivableTransaction ? "AR" : "AP");
				cmd.AddParameter("@NewStatus", SqlDbType.Char, newStatus.ToString());

				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void NettingMatchTransactions(DbConnection connection, Guid periodPK, ZString issuerEhubID, ZString recipientEhubID, Guid companyPK, string staffCode)
		{
			ExecuteDbOperation("NettingHeaderToHeaderLevelMatch", issuerEhubID, recipientEhubID);
			ExecuteDbOperation("NettingHeaderReferenceToHeaderReferenceLevelMatch", issuerEhubID, recipientEhubID);
			ExecuteDbOperation("NettingLineToLineLevelMatch", issuerEhubID, recipientEhubID);
			ExecuteDbOperation("NettingLineToLineLevelMatchWithoutGroupingByJobReference", issuerEhubID, recipientEhubID);
			ExecuteDbOperation("NettingLineReferenceToLineReferenceLevelMatch", issuerEhubID, recipientEhubID);
			ExecuteDbOperation("NettingReceivableLineToPayableLineReferenceLevelMatch", issuerEhubID, recipientEhubID);
			ExecuteDbOperation("NettingPayableLineToReceivableLineReferenceLevelMatch", issuerEhubID, recipientEhubID);

			void ExecuteDbOperation(ZString dbOperationName, ZString issuer, ZString recipient)
			{
				using (var cmd = connection.Command(dbOperationName))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, periodPK);
					cmd.AddParameter("@IssuerEHubId", SqlDbType.Char, issuer.ToString());
					cmd.AddParameter("@RecipientEHubId", SqlDbType.Char, recipient.ToString());
					cmd.AddParameter("@NettingThreshold", SqlDbType.Decimal, AccountingConfigurationRegistry.Instance.NettingThresholdValue.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
					cmd.AddParameter("@MatchingUser", SqlDbType.VarChar, staffCode);

					cmd.ExecuteNonQuery();
				}
			}
		}

		public static void HandleSettledOutOfNettingEvent(DbConnection connection, INettingTransaction originalTransaction, ZString matchStatus)
		{
			if (originalTransaction != null)
			{
				if (matchStatus == InvoiceAdditionalReference.FullyMatched || matchStatus == InvoiceAdditionalReference.PostedAndFullyMatched)
				{
					if (originalTransaction.ApprovalStatus == NettingTransactionApprovalStatus.Approved)
					{
						originalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.SettledOutOfNetting;
					}
					else if (originalTransaction.ApprovalStatus == NettingTransactionApprovalStatus.Matched)
					{
						UnmatchTransaction(connection, originalTransaction.NettingPeriodPK.ToGuid(), originalTransaction, NettingTransactionApprovalStatus.SettledOutOfNetting);
					}
				}
				else if (matchStatus == InvoiceAdditionalReference.UndoFullyMatched)
				{
					if (originalTransaction.ApprovalStatus == NettingTransactionApprovalStatus.SettledOutOfNetting)
					{
						originalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Approved;
					}
				}
			}
		}
	}
}
