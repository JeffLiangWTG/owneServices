using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Testing;

public class RomaniaEInvoicingPivotsToRequeueInfoTest : TestCaseWithFactory
{
	public void TestGetPivotsToRequeueMessage()
	{
		AssertEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:
- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights), or
- 'DLV' - Delivered (when you have appropriate security rights).", GetInstance().GetPivotsToRequeueMessage());
	}

	IEInvoicingPivotsToRequeueFilterProvider GetInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Romania) as IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>).Get();
}
