using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var invoiceCLineharge = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().Charges.AddNew();
			AssertType<InvoiceLineChargeLookups>(invoiceCLineharge.Lookups);
		}

		public void TestDefaultValuesForMethodFiveAndSix()
		{
			var errorMsg = "You can't enter a percentage for this charge type.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCharge = Factory.New<InvoiceLineCharge>();
			invoiceLineCharge.Parent = invoiceLine;
			AssertNoError(invoiceLineCharge.J7_PercentageInfo, errorMsg);

			invoiceLineCharge.J7_ChargeType = ImportChargeMethodFiveAndSixCodeList.Codes.B501;
			invoiceLineCharge.J7_Percentage = 60m;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceLineCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", true, invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly);
				AssertEquals("J7_IsNotIncludedInInvoice", false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
				AssertEquals("J7_IsDutiable", true, invoiceLineCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceLineCharge.J7_IsDutiableInfo.ReadOnly);
				AssertHasError(invoiceLineCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceLineCharge.J7_ChargeType = ImportChargeMethodFiveAndSixCodeList.Codes.B502;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceLineCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", true, invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly);
				AssertEquals("J7_IsNotIncludedInInvoice", false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
				AssertEquals("J7_IsDutiable", true, invoiceLineCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceLineCharge.J7_IsDutiableInfo.ReadOnly);
				AssertHasError(invoiceLineCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceLineCharge.J7_ChargeType = ImportChargeMethodFiveAndSixCodeList.Codes.B503;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceLineCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", true, invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly);
				AssertEquals("J7_IsNotIncludedInInvoice", false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
				AssertEquals("J7_IsDutiable", true, invoiceLineCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceLineCharge.J7_IsDutiableInfo.ReadOnly);
				AssertHasError(invoiceLineCharge.J7_PercentageInfo, errorMsg);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<InvoiceLineCharge>();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().Charges.AddNew();
	}
}
