using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentUcrWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentUniqueConsignmentReference>
	{
		public void TestNewOrNull()
		{
			AssertNull("when invoiceHeader is null", DeclarationGoodsShipmentUcrWrapper.NewOrNull(null));
			AssertNotNull("when invoiceHeader is not null", DeclarationGoodsShipmentUcrWrapper.NewOrNull(invoiceHeader));
		}

		public void TestId()
		{
			var wrapper = GetProvider();
			AssertEquals("90", wrapper.Id.Value);
		}

		public void TestTraderAssignedReferenceId()
		{
			AssertNull(Provider.TraderAssignedReferenceId);
		}

		protected override IDeclarationGoodsShipmentUniqueConsignmentReference GetProvider()
			=> DeclarationGoodsShipmentUcrWrapper.NewOrNull(invoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_UCR = "90";
		}

		JobComInvoiceHeader invoiceHeader;
	}
}
