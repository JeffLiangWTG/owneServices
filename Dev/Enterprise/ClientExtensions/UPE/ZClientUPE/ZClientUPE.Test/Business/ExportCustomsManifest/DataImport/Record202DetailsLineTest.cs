using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Record202DetailsLineTest : TestCaseWithFactory
	{
		public void TestSetEL_AirWayBill()
		{
			Mapper.SetEL_AirWayBill("1ZAA00350498771908");
			AssertEquals("1ZAA00350498771908", header.Lines[0].EL_AirWayBill);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			header.LoadOrCreateLineWithCANIntoCurrentManifestLine("NEWCAN101");
			Mapper = new Record202DetailsLine(null, header, null);
		}

		protected Record202DetailsLine Mapper;
		protected ExportCustomsManifestHeader header;
		#endregion
	}
}
