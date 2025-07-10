using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeForLoadAirCTO()
		{
			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			Factory.Save();
			AssertEquals(typeof(AirCTOExportCustomsManifestHeader), new BusinessObjectFactory().Load<ExportCustomsManifestHeader>(header.PK).GetType());
		}

		public void TestTypeForLoadDefault()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Factory.Save();
			AssertEquals(typeof(ExportCustomsManifestHeader), new BusinessObjectFactory().Load<ExportCustomsManifestHeader>(header.PK).GetType());
		}

		public void TestTypeForNew()
		{
			AssertEquals(typeof(ExportCustomsManifestHeader), typeDecider.GetTypeForNew());
			AssertEquals(typeof(ExportCustomsManifestHeader), new BusinessObjectFactory().New<ExportCustomsManifestHeader>().GetType());
		}

		public void TestTypeForBinding()
		{
			AssertEquals(typeof(ExportCustomsManifestHeader), typeDecider.GetTypeForBinding());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			Factory.Save();
		}

		readonly TypeDecider typeDecider = new ExportCustomsManifestHeaderTypeDecider();
		ExportCustomsManifestHeader header;
	}
}
