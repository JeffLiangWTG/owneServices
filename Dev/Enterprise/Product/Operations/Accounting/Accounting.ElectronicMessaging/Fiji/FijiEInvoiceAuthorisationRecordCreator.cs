using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing.Fiji;
using Enterprise.Accounting.Business.EInvoicing.TaxCore;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;

namespace Enterprise.Accounting.ElectronicMessaging.Fiji
{
	public class FijiEInvoiceAuthorisationRecordCreator : TaxCoreEInvoiceAuthorisationRecordCreator
	{
		protected override TaxCoreAccTransactionHeaderAuthorisationRecord GetNewAuthorisationRecordInstance(BusinessObjectFactory factory) => factory.New<FijiAccTransactionHeaderAuthorisationRecord>();
	}
}
