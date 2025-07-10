using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class NorwayReportModeAndCreditorSelectorDefaultTest : TestCaseWithFactory
	{
		public void TestReportModeAndCreditorSelectorDefault()
		{
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Norway) as IInstanceProvider<IReportModeAndCreditorSelectorDefault>).Get();

			AssertNotNull(result);
			AssertEquals(false, result.GenerateSalesInvoices);
			AssertEquals(false, result.GenerateCreditorInvoices);
			AssertEquals(true, result.GenerateAllInvoices);
			AssertEquals(false, result.ShouldPopup);
		}
	}
}
