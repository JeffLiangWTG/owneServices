using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCTOExportMessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageManagers()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			var line = header.Lines.AddNew();
			line.EL_CAN = "weeaboo";
			var manager = new AirCTOExportMessageManagerForTest(header);
			var managers = manager.GetAllMessageManagers();
			AssertEquals("CTO Removal Report - #1 - weeaboo", managers[0].MessageFriendlyName);
			AssertEquals("CTO Receival Report - #1 - weeaboo", managers[1].MessageFriendlyName);
			AssertEquals(2, managers.Length);
		}

		sealed class AirCTOExportMessageManagerForTest : AirCTOExportMessageManager
		{
			public AirCTOExportMessageManagerForTest(AirCTOExportCustomsManifestHeader exportCustomsManifestHeader) : base(exportCustomsManifestHeader)
			{
			}

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();
		}
	}
}
