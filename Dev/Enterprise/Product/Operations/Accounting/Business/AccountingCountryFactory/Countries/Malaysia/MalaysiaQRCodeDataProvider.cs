using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class MalaysiaQRCodeDataProvider : IQRCodeDataProvider
	{
		public string GetTransactionQRCodeString(InvoicingBase invoicing)
		{
			var result = string.Empty;

			if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				var baseUrl = AccountingConfigurationRegistry.Instance.MalaysiaEInvoicingPortalBaseUrl.Value;
				if (baseUrl.IsNullOrEmpty() || invoicing.AH_GovernmentAllocatedID.IsEmpty)
				{
					return result;
				}

				var transactionHeaderAuthorisationRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(invoicing.Factory, invoicing.PK, invoicing.Company.Country.RN_Code);
				var longId = transactionHeaderAuthorisationRecord?.AHF_Number ?? ZString.Empty;
				if (!longId.IsEmpty)
				{
					result = $"{baseUrl}/{invoicing.AH_GovernmentAllocatedID}/share/{longId}";
				}
			}

			return result;
		}
	}
}
