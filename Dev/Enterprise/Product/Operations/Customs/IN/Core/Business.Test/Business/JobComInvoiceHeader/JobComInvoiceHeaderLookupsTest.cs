using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using UniversalHelper = Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceHeaderLookups))]
sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
{
	public override void TestExporters()
	{
		var header = Factory.New<JobComInvoiceHeader>();
		var exporters = header.Lookups.Exporters;
		var propertyName = "Country/Region:Property";
		CombineAssertions(() =>
		{
			AssertEquals(typeof(ConsignorCollection), exporters.GetType());
			Assert("Filter", exporters.FilterBusinessObjectDefaults.ContainsDefaultFor(propertyName));
			AssertEquals("Filter Value", Core.Constants.CountryCodes.India, (ZString)exporters.FilterBusinessObjectDefaults[propertyName].Value);
		});
	}

	public void TestJZ_IncoTerm_List()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var incoTermList = invoiceHeader.Lookups.JZ_IncoTerm_List;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("JZ_IncoTerm_List codes", new[] { "FOB", "CIF", "CFR", "C&I" }, incoTermList.GetAllCodes());
			AssertSame("JZ_IncoTerm_List cached", incoTermList, invoiceHeader.Lookups.JZ_IncoTerm_List);
		});
	}

	public void TestJZ_GSTPaymentStatus_List()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		var gstPaymentStatusList = invoice.Lookups.IGSTPaymentStatusCodeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInExactOrder("IGSTPaymentStatusCodeList codes", new[] { "P", "LUT", "NA" }, gstPaymentStatusList.GetAllCodes());
			AssertSame("JZ_GSTPaymentStatus_List cached", gstPaymentStatusList, invoice.Lookups.IGSTPaymentStatusCodeList);
		});
	}

	public override void TestPaymentMethodList()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ValuationDate = ZDate.Today;
		var header = declaration.Invoices.AddNew();
		var today = ZDate.Today;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);
		var helper = new UniversalHelper.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IndiaNatureOfPayment, "Payment Method List");

		var codelist1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IndiaNatureOfPayment, "LC", "Letter of Credit", yesterday, tomorrow);
		var codelist2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IndiaNatureOfPayment, "DP", "Direct Payment", yesterday, tomorrow);
		var codelist3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IndiaNatureOfPayment, "NA", "Not Applicable", yesterday, tomorrow);
		Factory.Save();

		var paymentMethodList = header.Lookups.PaymentMethodList;
		AssertContainsExactElementsInAnyOrder("JZ_PaymentMethod codes", new[] { "LC", "DP", "NA" }, paymentMethodList.GetAllCodes());
		AssertSame("JZ_PaymentMethod Cached", paymentMethodList, header.Lookups.PaymentMethodList);
	}

	public void TestBuyers()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<ConsigneeCollection>(invoiceHeader.Lookups.Buyers);
	}

	public void TestAuthorizedEconomicOperatorsList()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<OrgHeaderCollection>(invoiceHeader.Lookups.AuthorizedEconomicOperatorsList);
	}
}
