using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.eInvoicing.China;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	internal static class ChinaEInvoiceHelper
	{
		public static OrgAddress GetMainARMailingAddress(OrgHeader orgHeader)
		{
			return orgHeader.Addresses.Cast<OrgAddress>().FirstOrDefault(
				x =>
					x.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Receivables) &&
					x.OA_Language == Core.SharedConstants.Languages.ChineseSimplified);
		}

		public static AccARAccountDetails GetDefaultTaxBankAccount(OrgHeader orgHeader, ZString transactionCurrency)
		{
			var eligibleBankAccount = orgHeader.CompanyData?.ARAccountDetailsCollection.Cast<AccARAccountDetails>().Where(
				x =>
					x.A1_IsDefaultAccount &&
					x.A1_PaymentMethod == AccARAccountDetails.ARBankAccPayment &&
					x.A1_RN_NKCountryCode == Enterprise.Core.Constants.CountryCodes.China &&
					CheckBankCurrencyEligibleForTransaction(x.A1_RX_NKAccountCurrency));

			return eligibleBankAccount.FirstOrDefault(x => x.A1_RX_NKAccountCurrency == transactionCurrency) ?? eligibleBankAccount.FirstOrDefault(x => x.A1_RX_NKAccountCurrency == Core.Constants.CurrencyCodes.China);

			bool CheckBankCurrencyEligibleForTransaction(ZString bankCurrency)
			{
				var isBankWithLocalCurrency = bankCurrency == Enterprise.Core.Constants.CurrencyCodes.China;
				if (transactionCurrency == Enterprise.Core.Constants.CurrencyCodes.China)
				{
					return isBankWithLocalCurrency;
				}
				else
				{
					return isBankWithLocalCurrency || bankCurrency == transactionCurrency;
				}
			}
		}

		public static void UpdateDDIReferenceAndFileName(InvoicingBase invoice, IEnumerable<AttachedDocument> attachedDocuments)
		{
			if (attachedDocuments?.Any() ?? false)
			{
				var fileNames = attachedDocuments.Select(x => x.FileName).ToArray();
				var logsToUpdate = invoice.Logs.GetAllLogs().Cast<StmALog>()
					.Where(x => !x.IsInDatabase && x.SL_SE_NKEvent == AutoEvents.DocumentImportedCode).ToArray();
				var fileNamesCount = fileNames.Length;
				var eDocs = invoice.DocManagerInfo.AllEDocs.OfType<StorageDocsBase>();

				var logToDelete = new HashSet<StmALog>();
				for (int i = 0; i < logsToUpdate.Length; i++)
				{
					if (fileNamesCount > i && fileNames[i].HasValue)
					{
						var fileName = fileNames[i].ToString().Replace(".pdf", "").Replace(".ofd", "").Replace(".xml", "");
						var eDoc = eDocs.FirstOrDefault(x => !x.IsInDatabase && x.SC_FileName.StartsWith(fileName));
						if (eDoc != null)
						{
							fileName = "VAT_" + invoice.AH_TransactionNum + "_" + eDoc.SC_FileName;
							var fileNameWithExtension = "VAT_" + invoice.AH_TransactionNum + "_" + eDoc.SC_FileNameWithExtension;
							if (eDoc.SC_DescMultilingual.ToString() == "China eInvoice file" &&
								eDocs.Any(x => x.IsInDatabase && x.SC_FileNameWithExtension == fileNameWithExtension && x.SC_DescMultilingual.Equals(eDoc.SC_DescMultilingual)))
							{
								invoice.DocManagerInfo.AllEDocs.Remove(eDoc);
								logToDelete.Add(logsToUpdate[i]);
							}
							else
							{
								eDoc.SC_FileName = fileName;
							}
						}
						logsToUpdate[i].UpdateReference(fileName);
					}
				}
				logToDelete.ForEach(x => invoice.Logs.GetAllLogs().RemoveAndDelete(x));
			}
		}

		public static string GetRemark(BusinessObjectFactory factory, ZGuid transactionPK, ZGuid companyPK, ZGuid branchPK)
		{
			var result = string.Empty;

			var sql = @"
			SELECT AH_TransactionNum,
				AH_Desc,
				AH_RX_NKTransactionCurrency,
				AH_ExchangeRate,
				AH_OSTotal,
				AH_ConsolidatedInvoiceRef AS JobInvoiceNumber,
				JS_HouseBill AS HouseBill,
				dbo.CLRConcatenateAgg(CONVERT(varchar(100), MainConTrans.JW_ETD, 120), '/', 1) AS ETD,
				dbo.CLRConcatenateAgg(CONVERT(varchar(100), MainConTrans.JW_ETA, 120), '/', 1) AS ETA,
				dbo.CLRConcatenateAgg(MainConTrans.JW_RL_NKLoadPort, '/', 1) AS LoadPort,
				dbo.CLRConcatenateAgg(MainConTrans.JW_RL_NKDiscPort, '/', 1) AS DischargePort,
				dbo.CLRConcatenateAgg(JK_MasterBillNum, '/', 1) AS MasterBill,
				dbo.CLRConcatenateAgg(MainConTrans.JW_Vessel, '/', 1) AS Vessel,
				dbo.CLRConcatenateAgg(MainConTrans.JW_VoyageFlight, '/', 1) AS Voyage,
				RepSales.GS_FullName AS RepSalesFullName,
				RepSales.GS_FriendlyName AS RepSalesFriendlyName,
				Operator.GS_FullName AS OperatorFullName,
				Operator.GS_FriendlyName AS OperatorFriendlyName
			FROM
				dbo.AccTransactionHeader
				LEFT JOIN dbo.JobHeader  ON JobHeader.JH_PK = AH_JH
				LEFT JOIN dbo.GLBStaff As RepSales ON JH_GS_NKRepSales = RepSales.GS_Code
				LEFT JOIN dbo.GLBStaff As Operator ON JH_GS_NKRepOps = Operator.GS_Code
				LEFT JOIN  dbo.JobShipment ON JS_PK = JH_ParentId
				LEFT OUTER JOIN dbo.JobConShipLink ON JS_PK = JN_JS
				LEFT OUTER JOIN dbo.JobConsol ON JK_PK = JN_JK
				LEFT JOIN dbo.csfn_MainConsolTransport('CN') AS MainConTrans ON MainConTrans.JW_JK = JK_PK
			WHERE AH_PK  = @TransactionPK
			GROUP BY AH_TransactionNum, AH_Desc, AH_RX_NKTransactionCurrency, AH_ExchangeRate,
				AH_OSTotal, AH_ConsolidatedInvoiceRef, JS_HouseBill, RepSales.GS_FullName, RepSales.GS_FriendlyName,
				Operator.GS_FullName, Operator.GS_FriendlyName";

			var command = Db.Connection.Command(sql);
			command.AddParameter("@TransactionPK", SqlDbType.UniqueIdentifier, transactionPK.ToGuid());

			var invoiceCollection = new DataTable() { Locale = CultureInfo.InvariantCulture };

			using (var reader = command.ExecuteReader())
			{
				invoiceCollection.Load(reader);
			}

			if (invoiceCollection.Rows.Count > 0)
			{
				var invoice = invoiceCollection.Rows[0];

				result = ChinaEInvoicingHelper.CreateRemarkString(factory, invoice, companyPK.ToGuid(), branchPK.ToGuid());
			}

			return result;
		}

		public static string GetSerialNumber(TransactionHeader transaction)
		{
			return transaction.PK.ToString() + "|" + transaction.Company.GetLicenceCode();
		}

		public static void UpdateDiscountedLineRowType(List<OrderDetail> items)
		{
			for (var index = 0; index < items.Count - 1; index++)
			{
				if (items[index].RowType == "0" && items[index + 1].RowType == "1")
				{
					items[index].RowType = "2";
				}
			}
		}

		public static string GetExtend(TransactionHeader transaction)
		{
			var formats = new List<string>() { "PDF" };
			var registry = AccountingConfigurationRegistry.Instance.ChinaEInvoicingReceivingFileType.Value;

			if (registry.Count > 0)
			{
				var fileTypeConfigs = registry.Cast<EInvoicingReceivingFileTypeConfiguration>().ToList();
				formats.AddRange(fileTypeConfigs.Where(x => x.DebtorType == "ALL").Select(x => x.FileFormat.ToString()).OrderBy(x => x));
				var debtorGroupConfigs = fileTypeConfigs.Where(x => x.DebtorType == Core.Constants.DebtorTypes.Code.DebtorGroup && x.DebtorCode == transaction.Header.CompanyData.OB_OJ_ARDebtorGroup);
				formats.AddRange(debtorGroupConfigs.Select(x => x.FileFormat.ToString()));
				var debtorConfigs = fileTypeConfigs.Where(x => x.DebtorType == Core.Constants.DebtorTypes.Code.DebtorOrganisation && x.DebtorCode == transaction.Header.PK);
				formats.AddRange(debtorConfigs.Select(x => x.FileFormat.ToString()));
			}

			return string.Join(",", formats.OrderBy(x => x));
		}
	}
}
