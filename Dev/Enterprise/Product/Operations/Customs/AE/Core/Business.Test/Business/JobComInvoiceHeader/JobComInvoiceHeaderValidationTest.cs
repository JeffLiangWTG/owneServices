using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

class JobComInvoiceHeaderValidationTest : TestCaseWithFactory
{
	public void TestInvoiceCurrency()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = RefCurrency.New(Factory).RX_Code;
		AssertNoErrors(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo);
		invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
		AssertHasMessageError(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, "An invoice currency must be entered");
	}

	public void TestJZ_OH_Supplier()
	{
		var qantas = Factory.New<OrgHeader>();
		invoiceHeader.JobDeclaration.JE_OH_Supplier = ZGuid.Empty;
		invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
		Assert(invoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
		invoiceHeader.JZ_OH_Supplier = qantas.PK;
		Assert(!invoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
		invoiceHeader.JobDeclaration.JE_OH_Supplier = qantas.PK;
		invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
		Assert(!invoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
		invoiceHeader.JZ_OH_Supplier = qantas.PK;
		Assert(!invoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
	}

	public void TestCheckJZ_InvoiceType() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeList("AED", "INVTP", "1", "INVTP for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		invoiceHeader.JobDeclaration.JE_ApplicationCode = "AED";
		invoiceHeader.Validation.ValidateJZ_InvoiceType();
		AssertHasMessageError("Not enter a value", invoiceHeader.JZ_InvoiceTypeInfo, "You have not entered an Invoice Type.");

		invoiceHeader.JZ_InvoiceType = 10;
		invoiceHeader.Validation.ValidateJZ_InvoiceType();
		AssertHasMessageError("Value not in the list", invoiceHeader.JZ_InvoiceTypeInfo, "The code you have selected is not in the list.");

		invoiceHeader.JZ_InvoiceType = 1;
		invoiceHeader.Validation.ValidateJZ_InvoiceType();
		Assert("No message errors", !invoiceHeader.JZ_InvoiceTypeInfo.HasMessageErrors());
	});

	public void TestCheckJZ_TotNoOfInvPages() => CombineAssertions(() =>
	{
		invoiceHeader.JZ_TotNoOfInvPages = 0;
		invoiceHeader.Validation.ValidateJZ_TotNoOfInvPages();
		AssertHasMessageError("value cannot be zero", invoiceHeader.JZ_TotNoOfInvPagesInfo, "Total No of Pages cannot be zero.");

		invoiceHeader.JZ_TotNoOfInvPages = 1;
		invoiceHeader.Validation.ValidateJZ_TotNoOfInvPages();
		Assert("No message errors", !invoiceHeader.JZ_TotNoOfInvPagesInfo.HasMessageErrors());
	});

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
		invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
	}

	protected JobComInvoiceHeader invoiceHeader;
}
