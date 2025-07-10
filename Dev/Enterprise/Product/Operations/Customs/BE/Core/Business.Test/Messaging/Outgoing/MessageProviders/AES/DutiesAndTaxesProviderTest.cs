using System;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class DutiesAndTaxesProviderTest : Customs.Business.Testing.DataProviderTestCase<DutiesAndTaxesProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DutiesAndTaxesProvider(null, 0));
	}

	public void TestSequenceNumber()
	{
		AssertEquals("999", Provider.SequenceNumber);
	}

	public void TestTaxType()
	{
		entryLineFee.CF_ChargeType = "Tax";
		AssertEquals("Tax", Provider.TaxType);
	}

	public void TestPayableTaxAmount()
	{
		entryLineFee.CF_ChargeAmount = 1334;
		AssertEquals(1334m, Provider.PayableTaxAmount);
	}

	public void TestMethodOfPayment()
	{
		entryLineFee.EntryLine.Header.Declaration.JE_PaymentMethod = "M";
		AssertEquals("M", Provider.MethodOfPayment);
	}

	public void TestTaxBase()
	{
		AssertEquals(1, Provider.TaxBase.Count);
	}

	protected override DutiesAndTaxesProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		entryLineFee = declaration.CustomsEntryHeaders.AddNew().AllEntryLines.AddNew().Fees.AddNew();
		provider = new DutiesAndTaxesProvider(entryLineFee, 999);
	}

	CusEntryLineFee entryLineFee;
	DutiesAndTaxesProvider provider;
}
