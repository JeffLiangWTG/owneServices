using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentSupplierWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentSupplier>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceHeader is null", DeclarationGoodsShipmentSupplierWrapper.NewOrNull(null));
			AssertNotNull("When invoiceHeader is not null and supplier has Israeli VAT", DeclarationGoodsShipmentSupplierWrapper.NewOrNull(supplier));
		}

		public void TestID()
		{
			var wrapper = GetProvider();
			AssertEquals("When supplier has Israeli Supplier Code", "560122458", wrapper.ID.Value);
		}

		protected override IDeclarationGoodsShipmentSupplier GetProvider() => DeclarationGoodsShipmentSupplierWrapper.NewOrNull(supplier);

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();

			supplier = factory.New<OrgHeader>();
			supplier.OH_FullName = "SUP";
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "560122458", Core.Constants.CountryCodes.Israel);
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
		}

		JobComInvoiceHeader invoiceHeader;
		OrgHeader supplier;
	}
}
