using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Business
{
	class AccountingEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.eNett, new InterchangeObjCollection() { NewInterchangeConfigObj(6, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(eNettMessageSubTypeList.Codes.CancelInvoice, eNettMessageSubTypeList.Descriptions.CancelInvoice, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.CreateNewInvoice, eNettMessageSubTypeList.Descriptions.CreateNewInvoice, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.GetNewCancellations, eNettMessageSubTypeList.Descriptions.GetNewCancellations, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.GetNewInvoices, eNettMessageSubTypeList.Descriptions.GetNewInvoices, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.GetNewPayments, eNettMessageSubTypeList.Descriptions.GetNewPayments, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.OfflinePayment, eNettMessageSubTypeList.Descriptions.OfflinePayment, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.ProcessCreditCard, eNettMessageSubTypeList.Descriptions.ProcessCreditCard, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.ProcessDirectDebit, eNettMessageSubTypeList.Descriptions.ProcessDirectDebit, 6, TimeUnit.Month)
				.Add(eNettMessageSubTypeList.Codes.Response, eNettMessageSubTypeList.Descriptions.Response, 6, TimeUnit.Month)
			);
		}
	}
}
