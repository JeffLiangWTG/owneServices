using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class ShipperDetailsLineTest : TestCaseWithFactory
	{
		public void TestSetOwnerName()
		{
			Mapper.SetOwnerName("ALLIED PICKFORDS PTY LTD");
			AssertEquals("ALLIED PICKFORDS PTY LTD", Header.Lines[0].EL_GoodsOwner);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.New<ExportCustomsManifestHeader>();
			Header.LoadOrCreateLineWithCANIntoCurrentManifestLine("NEWCAN101");
			Mapper = new ShipperDetailsLine(null, Header, null);
		}

		protected ShipperDetailsLine Mapper;
		protected ExportCustomsManifestHeader Header;
		#endregion
	}
}
