using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class TransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", TransportEquipmentProvider.NewOrNull(entryInstruction, null));
		}

		public void TestContainerIdentificationNumber()
		{
			AssertEquals("12345678", dataProvider.ContainerIdentificationNumber);
		}

		public void TestSealIdentifiers()
		{
			AssertEquals(0, dataProvider.SealIdentifiers.Count);
		}

		public void TestDeclarationGoodsItemNumbers()
		{
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2 }, dataProvider.DeclarationGoodsItemNumbers.Select(x => x));
		}

		public void TestNumberOfSeals()
		{
			AssertExceptionThrown<NotImplementedException>("property not used here", () => _ = Provider.NumberOfSeals);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "12345678";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "23456789";

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_JE = declaration.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			var entryLine4 = entryHeader.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 4;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			var pivot = invoiceLine.ContainersPivot.AddNew();
			pivot.C2_CO = container.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			var pivot2 = invoiceLine2.ContainersPivot.AddNew();
			pivot2.C2_CO = container.PK;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CL = entryLine3.PK;
			var pivot3 = invoiceLine3.ContainersPivot.AddNew();
			pivot3.C2_CO = container2.PK;

			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_CL = entryLine4.PK;

			dataProvider = TransportEquipmentProvider.NewOrNull(entryInstruction, container);
		}
		CusEntryInstruction entryInstruction;
		ITransportEquipment dataProvider;

		protected override TransportEquipmentProvider GetProvider() => (TransportEquipmentProvider)dataProvider;
	}
}
