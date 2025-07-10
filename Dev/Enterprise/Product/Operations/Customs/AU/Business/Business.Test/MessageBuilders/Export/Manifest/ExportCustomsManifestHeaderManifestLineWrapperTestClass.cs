using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestHeaderManifestLineWrapperTestClass : TestCaseWithFactory
	{
		public void TestAirWaybillNumber()
		{
			var line = new ExportCustomsManifestHeaderManifestLineWrapper(Factory.New<ExportCustomsManifestHeader>());
			Assert(line.AirWaybillNumber.IsEmpty);
		}
	}
}
