using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public class MalaysiaEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch batch, GlbBranch branch, TransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var items = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var transaction = batch.TransactionPivots[0]?.ParentTransactionHeader;

			if (transaction != null)
			{
				AddItem(items, "OrgISOAlpha3Code", GetISOAlpha3Code("AccountingTransactionExportGetAddressesSingle", transaction.PK.ToGuid()));
				AddItem(items, "BraISOAlpha3Code", GetISOAlpha3Code("AccountingTransactionExportGetSenderAddressesSingle", transaction.PK.ToGuid()));

				var orgHeader = transaction.Factory.Load<OrgHeader>(transaction.AH_OH);

				var categoryString = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? "DebtorCategory" : transaction.AH_Ledger == LedgerTypes.AccountsPayable ? "CreditorCategory" : string.Empty;

				AddItem(items, categoryString, GetCategory(orgHeader));
				AddItem(items, "UNLOCOCountry", orgHeader?.UNLOCO?.RL_RN_NKCountryCode ?? string.Empty);

				var eInvoicingHeader = transaction as IEInvoicingEligibilityLiteTransaction;
				var originalTransaction = eInvoicingHeader.OriginalTransactionIfExists;

				if ((originalTransaction?.GovernmentAllocatedID ?? string.Empty) != string.Empty)
				{
					AddItem(items, "OriginalGovt", originalTransaction.GovernmentAllocatedID);
				}

				AddItem(items, "UUID", transaction.AH_GovernmentAllocatedID);
				AddStaffPhone(transaction, items);
				AddContactPhone(transaction, items);

				var registryValue = AccountingElectronicMessagingRegistry.Instance.ElectronicInvoiceDocumentType.GetFallBackValueAtAllLevels(transaction.AH_GC.ToGuid(), transaction.AH_GB.ToGuid(), transaction.AH_GE.ToGuid());
				AddItem(items, "EINVDocumentType", registryValue);

				var codeDescriptionpairList = ((CodePairRegistryDataType)AccountingElectronicMessagingRegistry.Instance.ElectronicInvoiceDocumentType.DataType).LookUpList;
				AddItem(items, "EINVDocumentDesc", codeDescriptionpairList?.GetDescriptionFromCode(registryValue) ?? codeDescriptionpairList?.GetDescriptionFromCode(Constants.RefDocTypes.Invoice));

				AddMSICDescription("SupplierMSICDesc", transaction, items);
				AddItem(items, "GEIDateTime", ZDateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
			}

			return items;
		}

		string GetCategory(OrgHeader orgHeader)
		{
			if (orgHeader == null)
			{
				return OrgConstants.Category.Business;
			}

			return orgHeader.OH_Category;
		}

		string GetISOAlpha3Code(ZString dbFunctionName, Guid transactionHeaderPK)
		{
			var sqlText = SqlTextForProcedureCall(dbFunctionName, transactionHeaderPK);

			using (var reader = GetCommand(sqlText, Transaction).ExecuteReader())
			{
				while (reader.Read())
				{
					if (reader["RN_IsoAlpha3Code"] != DBNull.Value)
					{
						return (ZString?)((string)reader["RN_IsoAlpha3Code"]).Trim();
					}
				}
			}

			return string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		System.Data.Common.DbCommand GetCommand(string sqlText, System.Data.Common.DbTransaction transaction)
		{
			var result = Connection.CreateCommand();
			result.CommandText = sqlText;
			result.Transaction = Transaction;

			return result;
		}

		System.Data.Common.DbConnection Connection => ((IDbConnectionInternals)(Db.Connection)).ADOConnection;
		System.Data.Common.DbTransaction Transaction => ((IDbConnectionInternals)(Db.Connection)).ADOTransaction;

		static string SqlTextForProcedureCall(string baseProcedureName, Guid transactionHeaderPK)
		{
			return FormattableString.Invariant($"EXEC {baseProcedureName} '{transactionHeaderPK}'");
		}

		void AddItem(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items, ZString key, ZString value)
		{
			var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = key, Value = value };
			items.Add(item);
		}

		void AddStaffPhone(TransactionHeader transaction, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items)
		{
			var query = new ZQuery(GlbStaffSchema.GS_Code, transaction.AH_SystemCreateUser);
			var staff = transaction.Factory.LoadTop1<GlbStaff>(query);

			var contactName = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? "SupplierPhone" : transaction.AH_Ledger == LedgerTypes.AccountsPayable ? "BuyerPhone" : string.Empty;

			AddItem(items, contactName, staff?.GS_WorkPhone ?? ZString.Empty);
		}

		void AddContactPhone(TransactionHeader transaction, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items)
		{
			OrgContact contact;

			if (transaction.AH_OC_InvoiceContactOverride.IsEmpty)
			{
				var contactType = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? ContactType.Receivables.Code : transaction.AH_Ledger == LedgerTypes.AccountsPayable ? ContactType.Payables.Code : string.Empty;
				contact = transaction.Header?.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.OC_IsActive && x.Documents.Cast<OrgDocument>().Any(y => y.OD_DefaultContact && y.OD_DocumentGroup == contactType));
			}
			else
			{
				contact = transaction.InvoiceContactOverride;
			}

			var contactName = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? "BuyerPhone" : transaction.AH_Ledger == LedgerTypes.AccountsPayable ? "SupplierPhone" : string.Empty;

			AddItem(items, contactName, contact?.OC_Phone ?? ZString.Empty);
		}

		void AddMSICDescription(ZString key, TransactionHeader transaction, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items)
		{
			OrgHeader orgHeader = null;
			if (transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				orgHeader = transaction.Branch.OrgProxy ?? transaction.Company.OrgProxy;
			}
			else if (transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				orgHeader = transaction.Header;
			}

			if (orgHeader != null)
			{
				var code = orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.StandardIndustrialClassification, Constants.CountryCodes.Malaysia)?.OK_CustomsRegNo ?? string.Empty;
				var desc = AccountingMasterFilesUtils.GetRegistrationNumberDescriptionInFeatureControl(Constants.CountryCodes.Malaysia, code, OrgCusCode.CodeTypes.StandardIndustrialClassification);
				if (desc.IsNullOrEmpty())
				{
					desc = new MYMSICCodeDescriptionPairList().GetDescriptionFromCode(code);
				}

				AddItem(items, key, desc);
			}
		}
	}
}
