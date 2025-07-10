using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportInvoiceChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJ7_ChargeType_RuleBR4010()
		{
			const string messageError = "[BR4010] In all circumstances the respective amount declared for 1X must equal the respective amount declared for BA.";
			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			declaration.JE_ApplicationCode = "V1";
			instrction.CEI_Style = "H1";

			invoiceCharge.J7_ChargeType = "$X";
			AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);

			var charge1X = invoice.Charges.AddNew();
			charge1X.J7_ChargeType = AISChargeCodeList.Codes._1X;
			CombineAssertions("A charge 1X is present but no BA.", () =>
			{
				AssertHasMessageErrorContaining(charge1X.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			var chargeBA = invoice.Charges.AddNew();
			chargeBA.J7_ChargeType = AISChargeCodeList.Codes.BA;
			CombineAssertions("Both charges 1X and BA are present with amount = 0.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				charge1X.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(charge1X.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			chargeBA.J7_Amount = 2;
			charge1X.J7_Amount = 1;
			CombineAssertions("Both charges 1X and BA are present with different amounts.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertHasMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				charge1X.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(charge1X.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			charge1X.J7_Amount = 2;
			CombineAssertions("Both charges 1X and BA are present with same amounts.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				charge1X.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(charge1X.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			invoice.Charges.RemoveAndDelete(charge1X);
			var groupCharge1X = invoice.GroupCharges.AddNew();
			groupCharge1X.J7_ChargeType = AISChargeCodeList.Codes._1X;
			groupCharge1X.J7_Amount = 1;
			CombineAssertions("Both charges 1X (group) and BA are present with different amounts.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertHasMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			groupCharge1X.J7_Amount = 2;
			CombineAssertions("Both charges 1X (group) and BA are present with same amounts.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			invoice.GroupCharges.RemoveAndDelete(groupCharge1X);
			CombineAssertions("A charge BA is present but no 1X.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertHasMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			var groupHeaderZeroCharge1X = invoice.GroupHeader.Charges.AddNew();
			groupHeaderZeroCharge1X.J7_ChargeType = AISChargeCodeList.Codes._1X;
			chargeBA.J7_Amount = 0;
			CombineAssertions("Both charges 1X (group header - all invoices) and BA are present with amount = 0", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			chargeBA.J7_Amount = 2;
			CombineAssertions("Both charges 1X (group header - all invoices) and BA are present but BA amount > 0.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertHasMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});

			instrction.CEI_Style = "H2";
			CombineAssertions("No Message error when UCC5 and H2, H3, H4 or I1.", () =>
			{
				chargeBA.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(chargeBA.J7_ChargeTypeInfo, messageError);
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageErrorContaining(invoiceCharge.J7_ChargeTypeInfo, messageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceCharge = invoice.Charges.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		InvoiceCharge invoiceCharge;
	}
}
