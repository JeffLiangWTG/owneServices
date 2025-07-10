using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class InvoiceDetailsLineTest : TestCaseWithFactory
	{
		public void TestSetDescription()
		{
			Mapper.SetDescription("DIAGNOSTIC KITS FOR LAB TESTING");
			AssertEquals("DIAGNOSTIC KITS FOR LAB TESTING", header.Lines[0].EL_GoodsDescription);
		}

		public void TestSetDestinationCountry()
		{
			Mapper.SetDestinationCountry("ID");
			AssertEquals("ID", header.Lines[0].EL_RN_NKCountryOfDestination);
		}

		public void TestSetMultipleDescription()
		{
			for (int i = 0; i < 100; i++)
			{
				Mapper.SetDescription("0123456789");
			}

			StringBuilder descBuilder = new StringBuilder("0123456789");
			for (int i = 0; i < 9; i++)
			{
				descBuilder.Append("; 0123456789");
			}

			descBuilder.Append("; 01234567");
			AssertEquals(descBuilder.ToString(), header.Lines[0].EL_GoodsDescription);
		}

		public void TestFindingCANFromTranshipmentNumber()
		{
			string transhipmentNumber = "JAY";
			var cusHAWB = Factory.NewWithValidTestData<Customs.Business.CusHAWB>();
			cusHAWB.CS_TranshipmentEntryNum = transhipmentNumber;
			string hawb = "1ZA178826647781798";
			cusHAWB.CS_HAWB = hawb;
			Mapper.PackageTrackingNumber = hawb;
			AssertEquals(transhipmentNumber, Mapper.GetCANFromTranshipmentNumber());
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			header.LoadOrCreateLineWithCANIntoCurrentManifestLine("NEWCAN101");
			Mapper = new InvoiceDetailsLine(null, header, null);
		}

		protected InvoiceDetailsLine Mapper;
		protected ExportCustomsManifestHeader header;
		#endregion
	}
}
