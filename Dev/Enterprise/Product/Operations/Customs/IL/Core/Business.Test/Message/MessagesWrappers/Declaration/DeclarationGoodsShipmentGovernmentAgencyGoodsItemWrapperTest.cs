using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItem>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is not null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper.NewOrNull(invoiceLine));
		}

		public void TestAdditionalDocument()
		{
			AssertNotNull(Provider.AdditionalDocument);
			AssertEquals(0, Provider.AdditionalDocument.Count);

			invoiceLine.Permits.AddNew();
			var wrapper = GetProvider();
			AssertEquals(1, wrapper.AdditionalDocument.Count);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper>(wrapper.AdditionalDocument.Single());
		}

		public void TestCommodity()
		{
			AssertNotNull(Provider.Commodity);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityWrapper>(Provider.Commodity);
		}

		public void TestDmExtensions()
		{
			AssertNotNull(Provider.DmExtensions);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper>(Provider.DmExtensions);
		}

		public void TestGoodsMeasure()
		{
			AssertNotNull(Provider.GoodsMeasure);
			AssertEquals(0, Provider.GoodsMeasure.Count);

			invoiceLine.JI_CustomsThirdQuantity = 1.04m;
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			var wrapper = GetProvider();
			AssertEquals(1, wrapper.GoodsMeasure.Count);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper>(wrapper.GoodsMeasure.Single());
		}

		public void TestManufacturer()
		{
			AssertNotNull(Provider.Manufacturer);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper>(Provider.Manufacturer);
		}

		public void TestOrigin()
		{
			AssertNotNull(Provider.Origin);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper>(Provider.Origin);
		}

		public void TestPreviousDocument() => AssertNull(Provider.PreviousDocument);

		public void TestSequenceNumeric()
			=> AssertEquals(12m, Provider.SequenceNumeric);

		public void TestValuationAdjustment()
		{
			AssertNotNull(Provider.ValuationAdjustment);
			AssertEquals(0, Provider.ValuationAdjustment.Count);

			invoiceLine.Charges.AddNew();
			var wrapper = GetProvider();
			AssertEquals(1, wrapper.ValuationAdjustment.Count);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper>(wrapper.ValuationAdjustment.Single());
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItem GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";
			var entryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.CustomsEntryInstructions.Add(entryInstruction);

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 12;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}

		JobComInvoiceLine invoiceLine;
	}
}
