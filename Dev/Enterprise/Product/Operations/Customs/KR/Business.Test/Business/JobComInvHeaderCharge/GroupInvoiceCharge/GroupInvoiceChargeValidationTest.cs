using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_RX_NKCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			var groupInvoiceCharge = groupInvoice.Charges.AddNew();
			ChargeValidationHelperTest.TestCheckJ7_ExchangeRate(groupInvoiceCharge);
		}

		public void TestCheckJ7_ChargeType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();

			var groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			var groupInvoiceCharge = (GroupInvoiceCharge)groupInvoice.Charges.AddNew();
			groupInvoiceCharge.Validation.ValidateJ7_ChargeType();
			AssertHasMessageErrorContaining(groupInvoiceCharge.J7_ChargeTypeInfo, "Please enter a valid Charge code");

			groupInvoiceCharge.J7_ChargeType = ImportChargeMethodOneCodeList.Codes.A114;
			AssertHasMessageErrorContaining(groupInvoiceCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list");

			invoice1.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice2.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice3.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			groupInvoiceCharge.Validation.ValidateJ7_ChargeType();
			AssertNoMessageErrorContaining(groupInvoiceCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list");

			invoice1.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
			groupInvoiceCharge.Validation.ValidateJ7_ChargeType();
			AssertHasMessageErrorContaining(groupInvoiceCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list");

			groupInvoiceCharge.J7_ChargeType = ImportChargeMethodTwoAndThreeCodeList.Codes.B311;
			AssertHasMessageErrorContaining(groupInvoiceCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list");

			invoice2.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
			invoice3.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
			groupInvoiceCharge.Validation.ValidateJ7_ChargeType();
			AssertNoMessageErrorContaining(groupInvoiceCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list");

			invoice3.JZ_ValuationCode = string.Empty;
			groupInvoiceCharge.Validation.ValidateJ7_ChargeType();
			AssertHasMessageErrorContaining(groupInvoiceCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list");
		}
	}
}
