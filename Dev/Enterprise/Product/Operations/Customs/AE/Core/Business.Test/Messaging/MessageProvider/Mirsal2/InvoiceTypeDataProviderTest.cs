using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using CargoWise.Types;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class InvoiceTypeDataProviderTest : Mirsal2InvoiceTypeDataProviderAbstractClassBase
{
	public override void TestInvoiceNumber()
	{
		invoiceHeader.JZ_InvoiceNumber = "TEST1";
		AssertEquals(invoiceHeader.JZ_InvoiceNumber, CreateDataProvider().InvoiceNumber);
	}

	public override void TestInvoiceDate()
	{
		invoiceHeader.JZ_InvoiceDate = ZDateTime.Now;
		AssertEquals(invoiceHeader.JZ_InvoiceDate.ToDateTime(), CreateDataProvider().InvoiceDate);
	}

	public override void TestTotalNumberOfInvoicePages()
	{
		invoiceHeader.JZ_TotNoOfInvPages = 10;
		AssertEquals(invoiceHeader.JZ_TotNoOfInvPages, CreateDataProvider().TotalNumberOfInvoicePages);
	}

	public override void TestInvoiceTypeProperty()
	{
		invoiceHeader.JZ_InvoiceType = 5;
		AssertEquals(invoiceHeader.JZ_InvoiceType, CreateDataProvider().InvoiceTypeProperty);
	}

	public override void TestSellerName()
	{ 
		var supplier = Factory.New<OrgHeader>();
		supplier.OH_FullName = "Seller1";
		invoiceHeader.JZ_OH_Supplier = supplier.PK;
		AssertEquals(supplier.OH_FullName, CreateDataProvider().SellerName);
	}

	public override void TestBuyerName()
	{
		var buyer = Factory.New<OrgHeader>();
		buyer.OH_FullName = "Buyer1";
		invoiceHeader.JZ_OH_Buyer = buyer.PK;
		AssertEquals(buyer.OH_FullName, CreateDataProvider().BuyerName);
	}

	public override void TestPaymentInstrumentType()
	{
		invoiceHeader.JZ_PaymentMethod = "1";
		AssertEquals((short)1, CreateDataProvider().PaymentInstrumentType);
	}

	public override void TestValuationMethod()
	{
		invoiceHeader.JZ_ValuationCode = "2";
		AssertEquals((short)2, CreateDataProvider().ValuationMethod);
	}

	public override void TestInvoiceCurrency()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
		AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, CreateDataProvider().InvoiceCurrency);
	}

	public override void TestInvoiceValue()
	{
		invoiceHeader.JZ_InvoiceAmount = 999.999m;
		AssertEquals(invoiceHeader.JZ_InvoiceAmount, CreateDataProvider().InvoiceValue);
	}

	public override void TestINCOTermsCode()
	{
		invoiceHeader.JZ_IncoTerm = "ABC";
		AssertEquals(invoiceHeader.JZ_IncoTerm, CreateDataProvider().INCOTermsCode);
	}

	public override void TestFreightCurrencyCode()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
		AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, CreateDataProvider().FreightCurrencyCode);
	}

	public override void TestFreightCharges() => CombineAssertions(() =>
	{
		var charge = invoiceHeader.Charges.AddNew();
		charge.J7_ChargeType = "OFT";
		charge.J7_Amount = 100;
		AssertEquals(charge.J7_Amount, CreateDataProvider().FreightCharges);

		charge.J7_ChargeType = "XXX";
		AssertEquals(null, CreateDataProvider().FreightCharges);
	});

	public override void TestInsuranceCurrencyCode()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
		AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, CreateDataProvider().InsuranceCurrencyCode);
	}

	public override void TestInsuranceChargesCost()
	{
		var charge = invoiceHeader.Charges.AddNew();
		charge.J7_ChargeType = "ONS";
		charge.J7_Amount = 1000;
		AssertEquals(charge.J7_Amount, CreateDataProvider().InsuranceChargesCost);

		charge.J7_ChargeType = "XXX";
		AssertEquals(null, CreateDataProvider().InsuranceChargesCost);
	}

	public override void TestEDASAttestationNumber()
	{
		invoiceHeader.JZ_AttestationNo = "TEST2";
		AssertEquals(invoiceHeader.JZ_AttestationNo, CreateDataProvider().EDASAttestationNumber);
	}

	public override void TestInvoiceItemsDetail()
	{
		AssertEquals($"{nameof(InvoiceItemsDetailsTypeDataProviderAbstractClass)} Type", "InvoiceItemsDetailsTypeDataProvider", CreateDataProvider().InvoiceItemsDetail.Single().GetType().Name);
	}

	protected override InvoiceTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.ShippingDetails.Invoices.First();
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

		var cusEntryLine = header.MergedLines.AddNew();
		cusEntryLine.InvoiceLines.Add(invoiceLine);
	}

	JobComInvoiceHeader invoiceHeader;
}
