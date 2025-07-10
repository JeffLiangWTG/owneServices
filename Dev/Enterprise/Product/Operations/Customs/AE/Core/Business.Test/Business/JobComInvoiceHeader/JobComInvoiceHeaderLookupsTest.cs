using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
{
	public void TestInvoiceTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeList("AED", "INVTP", "1", "INVTP for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList("AED", "INVTP", "3", "INVTP for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = "AED";
		var header = declaration.Invoices.AddNew();
		CombineAssertions(() =>
		{
			var invoiceTypeList = header.Lookups.InvoiceTypeList;
			AssertEquals("invoiceTypeList", "1, 3", invoiceTypeList.CodesAsString);
			NUnit.Framework.Assert.That(invoiceTypeList, Is.SameAs(header.Lookups.InvoiceTypeList), "Cached");
		});
	}

	public override void TestPaymentMethodList() => CombineAssertions(() =>
	{
		var paymentMethodList = lookups.PaymentMethodList;
		var expectedList = new PaymentMethodList();
		expectedList.SortNumerically();
		AssertContainsExactElementsInExactOrder("PaymentMethodList values", paymentMethodList.GetAllCodes(), expectedList.GetAllCodes());
		AssertSame("PaymentMethodList is cached", paymentMethodList, lookups.PaymentMethodList);
	});

	public void TestValuationCode() => CombineAssertions(() =>
	{
		var valuationCodeList = lookups.ValuationCodeList;
		var listCodes = ((CodeDescriptionPairList)valuationCodeList).GetAllCodes();
		AssertContainsExactElementsInExactOrder("ValuationCodeList values", listCodes, new ValuationCodeList().GetAllCodes());
		AssertSame("PaymentMethodList is cached", valuationCodeList, lookups.ValuationCodeList);
	});

	protected override void SetUp()
	{
		base.SetUp();
		invoiceHeader = Factory.New<JobComInvoiceHeader>();
		lookups = new JobComInvoiceHeaderLookups(invoiceHeader);
	}

	JobComInvoiceHeaderLookups lookups;
	JobComInvoiceHeader invoiceHeader;
}
