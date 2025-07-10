using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing.Samoa;
using Enterprise.Accounting.Business.EInvoicing.TaxCore;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;

namespace Enterprise.Accounting.ElectronicMessaging.Samoa
{
	public class SamoaEInvoiceAuthorisationRecordCreator : TaxCoreEInvoiceAuthorisationRecordCreator
	{
		protected override TaxCoreAccTransactionHeaderAuthorisationRecord GetNewAuthorisationRecordInstance(BusinessObjectFactory factory) => factory.New<SamoaAccTransactionHeaderAuthorisationRecord>();
	}
}
