using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

class FreightWrapperTest : DataProviderTestCase<FreightWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new FreightWrapper(null as JobDeclaration));
		AssertExceptionThrown<ArgumentNullException>(() => new FreightWrapper(null as JobComInvoiceHeader));
	}

	public void TestPaymentMethodCode_Declaration()
	{
		AssertEquals("PaymentMethodCode", "A", wrapperDeclaration.PaymentMethodCode);
	}

	public void TestPaymentMethodCode_Invoice()
	{
		AssertEquals("PaymentMethodCode", "X", wrapperInvoice.PaymentMethodCode);
	}

	protected override FreightWrapper GetProvider() => wrapperInvoice;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_PaymentMethod = "A";
		invoice = declaration.Invoices.AddNew();
		invoice.ZG_TransportChargesMethodOfPayment = "X";
		wrapperInvoice = new FreightWrapper(invoice);
		wrapperDeclaration = new FreightWrapper(declaration);
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	FreightWrapper wrapperInvoice;
	FreightWrapper wrapperDeclaration;
}
