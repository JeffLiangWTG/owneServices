using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipment>
	{
		public void TestNewOrNull()
		{
			AssertNull("When Invoice Header is null", DeclarationGoodsShipmentWrapper.NewOrNull(cusEntryInstruction, null));
			AssertNull("When Entry Instruction is null", DeclarationGoodsShipmentWrapper.NewOrNull(null, invoiceHeader));
			AssertNotNull(DeclarationGoodsShipmentWrapper.NewOrNull(cusEntryInstruction, invoiceHeader));
		}

		public void TestSequenceNumeric()
		{
			var wrapper = GetProvider();
			AssertEquals(1m, wrapper.SequenceNumeric);
			invoiceHeader = declaration.Invoices.AddNew();
			wrapper = GetProvider();
			AssertEquals(2m, wrapper.SequenceNumeric);
		}

		public void TestInvoice()
		{
			var wrapper = GetProvider();
			var invoice = wrapper.Invoice;
			AssertNotNull(nameof(IDeclarationGoodsShipment.Invoice), invoice);
		}

		public void TestSupplier()
		{
			var wrapper = GetProvider();
			AssertNull("When invoice header has not supplier with Israeli VAT", wrapper.Supplier);
			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "SUP1";
			orgAddress.OA_OH = orgHeader.PK;
			orgHeader.CustomsCodes.AddNew("VAT", "1", "IL");

			invoiceHeader.JZ_OH_Supplier = orgHeader.PK;

			wrapper = GetProvider();
			AssertNotNull("When invoice header has supplier with Israeli VAT", wrapper.Supplier);
		}

		public void TestTradeTerms()
		{
			var wrapper = GetProvider();
			AssertNotNull("Even if invoice header has any incoterms/place ", wrapper.TradeTerms);
		}
		public void TestAdditionalDocument()
		{
			AssertNull(Provider.AdditionalDocument);
		}

		public void TestConsignment()
		{
			AssertNotNull("Primary invoice will generate a consignment", Provider.Consignment);
			AssertEquals("There should be exactly 1 consignment", 1, Provider.Consignment.Count);
			AssertType<DeclarationGoodsShipmentConsignmentWrapper>("The consigment wrapper should be of the expected type", Provider.Consignment.First());

			var secondaryInvoiceHeader = invoiceHeader.JobDeclaration.Invoices.AddNew();
			IDeclarationGoodsShipment secondaryInvoiceWrapper = DeclarationGoodsShipmentWrapper.NewOrNull(cusEntryInstruction, secondaryInvoiceHeader);
			AssertNull("Only the primary invoice will generate a consignment", secondaryInvoiceWrapper.Consignment);
		}

		public void TestCustomsValuation()
		{
			var chargeOFT = invoiceHeader.Charges.AddNew();
			chargeOFT.J7_ChargeType = "OFT";
			chargeOFT.J7_Amount = 10.2m;
			chargeOFT.J7_RX_NKCurrency = "ILS";

			var wrapper = GetProvider();
			var customsValuation = wrapper.CustomsValuation;

			AssertEquals("Have Charges", 1, customsValuation.Count);

			var declarationGoodsShipmentCustomsValuation = wrapper.CustomsValuation.Single() as DeclarationGoodsShipmentCustomsValuationWrapper;
			AssertNotNull("Type is DeclarationGoodsShipmentCustomsValuationWrapper", declarationGoodsShipmentCustomsValuation);
		}

		public void TestGovernmentAgencyGoodsItem()
		{
			AssertNotNull(Provider.GovernmentAgencyGoodsItem);
			AssertEquals("When In the absence of Invoice Lines", 0, Provider.GovernmentAgencyGoodsItem.Count);

			invoiceHeader.InvoiceLines.AddNew();
			var wrapper = GetProvider();
			AssertEquals("When has Invoice Lines", 1, wrapper.GovernmentAgencyGoodsItem.Count);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper>(wrapper.GovernmentAgencyGoodsItem.Single());
		}

		public void TestUcr()
		{
			invoiceHeader.JZ_UCR = "90";
			var wrapper = GetProvider();

			AssertEquals("When UCR is not empty", 1, wrapper.Ucr.Count);
			AssertType<DeclarationGoodsShipmentUcrWrapper>(wrapper.Ucr.Single());
		}

		protected override IDeclarationGoodsShipment GetProvider() => DeclarationGoodsShipmentWrapper.NewOrNull(cusEntryInstruction, invoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var factory = Factory;
			declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";
			cusEntryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			invoiceHeader = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryInstruction cusEntryInstruction;
	}
}
