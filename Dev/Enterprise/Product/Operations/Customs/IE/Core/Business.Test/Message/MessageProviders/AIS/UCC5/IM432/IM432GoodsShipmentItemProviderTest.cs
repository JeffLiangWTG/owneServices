using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class IM432GoodsShipmentItemProviderTest : DataProviderTestCase<IM432GoodsShipmentItemProvider>
	{
		public void TestGoodsItemNumber()
		{
			AssertEquals("GoodsItemNumber=>CL_LineNumber", "1", Provider.GoodsItemNumber);
		}

		public void TestDocumentsAuthorisations()
		{
			AssertType<ISimplifiedDeclarationDocumentWritingOff[]>("Type of DocumentsAuthorisations", Provider.DocumentsAuthorisations);
		}

		public void TestGoodsInformation()
		{
			AssertType<IM432GoodsShipmentItemProvider>("Type of GoodsInformation", Provider.GoodsInformation);
		}

		public void TestGrossMass()
		{
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_WeightUQ = "KG";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 13;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals("GrossMass=>line EffectiveGrossWeight", 25m, Provider.GrossMass);
		}

		public void TestPackaging()
		{
			AssertType<IPackaging[]>("Type of Packaging", Provider.Packaging);
		}

		public void TestContainerId()
		{
			AssertType<string[]>("Type of ContainerId", Provider.ContainerId);
		}

		protected override void SetUp() => SetUpTestDataIfNeeded();

		protected override IM432GoodsShipmentItemProvider GetProvider()
		{
			var headerProvider = new IM432GoodsShipmentProvider(new EntryHeaderWrapper(entryHeader));
			return (IM432GoodsShipmentItemProvider)headerProvider.GoodsShipmentItems.First();
		}

		void SetUpTestDataIfNeeded()
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
				entryLine.CL_LineNumber = 1;
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
