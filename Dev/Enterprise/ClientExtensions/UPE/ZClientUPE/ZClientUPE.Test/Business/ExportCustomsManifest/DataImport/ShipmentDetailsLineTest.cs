using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class ShipmentDetailsLineTest : TestCaseWithFactory
	{
		public void TestSetNoOfContainers()
		{
			Mapper.SetNoOfContainers();
			AssertEquals((short)0, Header.Lines[0].EL_NumberOfContainers);
		}

		public void TestSetNoOfPacks()
		{
			Mapper.SetNoOfPacks("340");
			AssertEquals(340, Header.Lines[0].EL_NumberOfPackages);
			Header.Lines[0].EL_NumberOfPackages = 0;

			Mapper.SetNoOfPacks("Error");
			AssertEquals(0, Header.Lines[0].EL_NumberOfPackages);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.New<ExportCustomsManifestHeader>();
			Header.LoadOrCreateLineWithCANIntoCurrentManifestLine("NEWCAN101");
			Mapper = new ShipmentDetailsLine(null, Header, null);
		}

		protected ShipmentDetailsLine Mapper;
		protected ExportCustomsManifestHeader Header;

		#endregion
	}
}
