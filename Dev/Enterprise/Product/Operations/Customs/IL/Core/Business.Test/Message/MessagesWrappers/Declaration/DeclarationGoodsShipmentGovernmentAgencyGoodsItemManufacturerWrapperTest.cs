using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper.NewOrNull(invoiceLine));
		}

		public void TestID()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var wrapper = DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper.NewOrNull(invoiceLine);
			AssertNull("When header have invalid Manufacturer", wrapper.ID);

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "MAN1";
			orgHeader.CustomsCodes.AddNew("VAT", "1", "IL");
			orgAddress.OA_OH = orgHeader.PK;
			invoiceHeader.JZ_OA_ManufacturerAddress = orgAddress.PK;
			wrapper = DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper.NewOrNull(invoiceLine);
			AssertNull("When header have invalid Manufacturer", wrapper.ID);

			orgHeader.CustomsCodes.AddNew("CSC", "11212", "IL");
			wrapper = DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper.NewOrNull(invoiceLine);
			AssertEquals("When header have invalid Manufacturer", "11212", wrapper.ID.Value);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
		}

		JobComInvoiceLine invoiceLine;
	}
}
