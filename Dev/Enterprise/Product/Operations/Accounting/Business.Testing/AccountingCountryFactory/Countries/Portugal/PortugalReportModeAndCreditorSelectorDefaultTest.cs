using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PortugalReportModeAndCreditorSelectorDefaultTest : TestCaseWithFactory
	{
		public void TestReportModeAndCreditorSelectorDefault()
		{
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Portugal) as IInstanceProvider<IReportModeAndCreditorSelectorDefault>).Get();

			AssertNotNull(result);
			AssertEquals(true, result.GenerateSalesInvoices);
			AssertEquals(false, result.GenerateCreditorInvoices);
			AssertEquals(false, result.GenerateAllInvoices);
			AssertEquals(true, result.ShouldPopup);
		}
	}
}
