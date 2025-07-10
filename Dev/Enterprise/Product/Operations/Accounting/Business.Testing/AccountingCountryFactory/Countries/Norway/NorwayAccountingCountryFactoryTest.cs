using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class NorwayAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIReportSAFTWriter()
		{
			var builder = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Norway) as IInstanceProvider<IReportSAFTWriter>).Get();

			AssertNotNull(builder);
		}

		public void TestIReportModeAndCreditorSelectorDefault()
		{
			var builder = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Norway) as IInstanceProvider<IReportModeAndCreditorSelectorDefault>).Get();

			AssertNotNull(builder);
		}

		public void TestIComplianceReportGUIActionProvider()
		{
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Norway) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get();

			AssertNotNull(provider);
		}
	}
}
