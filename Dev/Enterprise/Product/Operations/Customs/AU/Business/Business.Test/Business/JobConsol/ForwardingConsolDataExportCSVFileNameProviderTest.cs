using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ForwardingConsolDataExportCSVFileNameProviderTest : TestCaseWithFactory
	{
		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C111";
			consol.JK_MasterBillNum = "MB111";
			var wrapper = new ForwardingConsolDataExportCSVFileNameProvider(consol);
			var fileNameProvider = wrapper as IDataExportCSVFileNameProvider;
			AssertEquals("File name suffix", "C111_MB111", fileNameProvider.FileNameSuffix);
		}
	}
}
