using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PortugaIReportSAFTWriterProviderTest : TestCaseWithFactory
	{
		public void TestReportSAFTWriter()
		{
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Portugal) as IInstanceProvider<IReportSAFTWriter>).Get();

			AssertNotNull(result);
			AssertEquals(SAFTVersion.SAFT1_04, result.GetSAFTVersion);
			AssertEquals(ZString.Empty, result.GetLocalLanguage);
		}
	}
}
