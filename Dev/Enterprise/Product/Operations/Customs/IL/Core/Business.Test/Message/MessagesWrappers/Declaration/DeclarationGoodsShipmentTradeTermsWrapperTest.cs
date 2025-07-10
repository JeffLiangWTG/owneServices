using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentTradeTermsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentTradeTerms>
	{
		public void TestNewOrNull()
		{
			AssertNull("when invoiceHeader is null", DeclarationGoodsShipmentTradeTermsWrapper.NewOrNull(null));

			AssertNotNull("when invoiceHeader is not null", DeclarationGoodsShipmentTradeTermsWrapper.NewOrNull(invoiceHeader));
		}

		public void TestConditionCode()
		{
			invoiceHeader.JZ_IncoTerm = "";
			var wrapper = GetProvider();
			AssertNull("When Inco Term is empty", wrapper.ConditionCode);

			invoiceHeader.JZ_IncoTerm = "FOB";
			wrapper = GetProvider();
			AssertEquals("When Inco Term is not empty", "FOB", wrapper.ConditionCode.Value);
		}

		public void TestLocationID()
		{
			invoiceHeader.JZ_IncoTermPlace = "";
			var wrapper = GetProvider();
			AssertNull("When Inco Term place is empty", wrapper.LocationID);
			invoiceHeader.JZ_IncoTermPlace = "PLACE1";
			wrapper = GetProvider();
			AssertEquals("When Inco Term place is not empty", "PLACE1", wrapper.LocationID.Value);
		}

		protected override IDeclarationGoodsShipmentTradeTerms GetProvider()
			=> DeclarationGoodsShipmentTradeTermsWrapper.NewOrNull(invoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_IncoTermPlace = "PLACE1";
		}

		JobComInvoiceHeader invoiceHeader;
	}
}
