using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class MConsignment02ProviderTest : DataProviderTestCase<MConsignment02Provider>
	{
		public void TestLocationOfGoods()
		{
			SetUpTestData();
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_Type = "A";
			var locationOfGoods = Provider.LocationOfGoods;
			AssertEquals("Has been constructed with entryInstruction", "A", locationOfGoods.TypeOfLocation);
			AssertSame("Cached", locationOfGoods, Provider.LocationOfGoods);
		}

		public void TestTransportEquipments()
		{
			SetUpTestData();

			entryLine.CL_LineNumber = 1;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT1";
			var package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			invoiceLine.PackagesPivot.AddPivotFor(package);

			var transportEquipments = Provider.TransportEquipments;
			AssertEquals("Count", 1, transportEquipments.Count);
			Assert("Element type", transportEquipments.All(x => x is TransportEquipmentProvider));
		}

		protected override MConsignment02Provider GetProvider()
		{
			SetUpTestData();
			return new MConsignment02Provider(entryHeaderWrapper);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		EntryHeaderWrapper entryHeaderWrapper;
	}
}
