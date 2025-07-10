using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var invoiceCharge = Factory.New<JobDeclaration>().Invoices.AddNew().Charges.AddNew();
			AssertType<InvoiceChargeLookups>(invoiceCharge.Lookups);
		}

		public void TestDefaultValuesForMethodOne()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			CombineAssertions("Case CFR (same as CPT)", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndFreight;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A102, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A104, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A105, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A106, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A107, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A108, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A109, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A110, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A111, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A112, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A114, true, true, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A115, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A116, false, true, false, true);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A118, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A119, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A120, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A121, true, false, false, false);
			});

			CombineAssertions("CIN", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndInsurance;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A102, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A104, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A105, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A106, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A107, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A108, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A109, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A110, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A111, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A112, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A114, false, true, false, true);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A115, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A116, true, true, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A118, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A119, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A120, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A121, true, false, false, false);
			});

			CombineAssertions("CIF (same as CIP, DAF, DAP, DAT, DDP, DDU, DEQ, DES, DPU)", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.CostInsuranceAndFreight;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A102, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A104, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A105, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A106, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A107, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A108, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A109, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A110, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A111, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A112, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A114, true, true, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A115, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A116, true, true, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A118, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A119, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A120, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A121, true, false, false, false);
			});

			CombineAssertions("EXW (same as FAS, FCA, FOB)", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.ExWorks;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A102, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A104, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A105, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A106, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A107, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A108, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A109, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A110, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A111, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A112, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A114, false, true, false, true);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A115, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A116, false, true, false, true);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A118, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A119, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A120, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodOneCodeList.Codes.A121, true, false, false, false);
			});
		}

		public void TestDefaultValuesForMethodTwoAndThree()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;

			CombineAssertions("Case CFR (same as CPT)", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndFreight;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B303, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B304, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B305, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B306, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B307, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B309, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B310, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B311, true, true, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B312, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B313, false, true, false, true);
			});

			CombineAssertions("CIN", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndInsurance;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B303, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B304, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B305, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B306, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B307, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B309, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B310, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B311, false, true, false, true);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B312, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B313, true, true, false, false);
			});

			CombineAssertions("CIF (same as CIP, DAF, DAP, DAT, DDP, DDU, DEQ, DES, DPU)", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.CostInsuranceAndFreight;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B303, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B304, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B305, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B306, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B307, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B309, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B310, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B311, true, true, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B312, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B313, true, true, false, false);
			});

			CombineAssertions("EXW (same as FAS, FCA, FOB)", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.ExWorks;

				var charge = Factory.New<InvoiceChargeForTest>();
				charge.Parent = invoice;
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B303, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B304, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B305, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B306, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B307, true, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B309, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B310, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B311, false, true, false, true);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B312, false, false, false, false);
				AssertPropertiesAboutInvoiceCharge(charge, ImportChargeMethodTwoAndThreeCodeList.Codes.B313, false, true, false, true);
			});
		}
		void AssertPropertiesAboutInvoiceCharge(InvoiceChargeForTest charge, ZString chargeCode, bool isIncludedInInvoiceAmount, bool isIncludedInInvoiceAmount_ReadOnly, bool isIncludedInInvoiceLine, bool isIncludedInInvoiceLine_ReadOnly)
		{
			charge.J7_ChargeType = chargeCode;
			AssertEquals(isIncludedInInvoiceAmount, charge.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals(isIncludedInInvoiceAmount_ReadOnly, charge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
			AssertEquals(isIncludedInInvoiceLine, charge.J7_IsIncludedInITOT);
			AssertEquals(isIncludedInInvoiceLine_ReadOnly, charge.J7_IsIncludedInITOT_ReadOnly);
		}

		public void TestDefaultValuesForMethodFour()
		{
			var errorMsg = "You can't enter a percentage for this charge type.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoiceCharge = Factory.New<InvoiceChargeForTest>();
			invoiceCharge.Parent = invoice;
			AssertNoError(invoiceCharge.J7_PercentageInfo, errorMsg);

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B404;
			invoiceCharge.J7_Percentage = 60m;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B405;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B406;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B407;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B408;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B409;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B410;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFourCodeList.Codes.B411;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", false, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});
		}

		public void TestDefaultValuesForMethodFiveAndSix()
		{
			var errorMsg = "You can't enter a percentage for this charge type.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoiceCharge = Factory.New<InvoiceChargeForTest>();
			invoiceCharge.Parent = invoice;
			AssertNoError(invoiceCharge.J7_PercentageInfo, errorMsg);

			invoiceCharge.J7_ChargeType = ImportChargeMethodFiveAndSixCodeList.Codes.B501;
			invoiceCharge.J7_Percentage = 60m;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", true, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", true, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFiveAndSixCodeList.Codes.B502;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", true, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", true, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});

			invoiceCharge.J7_ChargeType = ImportChargeMethodFiveAndSixCodeList.Codes.B503;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_IsIncludedInITOT_ReadOnly", true, invoiceCharge.J7_IsIncludedInITOT_ReadOnly);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount_ReadOnly", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
				AssertEquals("J7_IsDutiable", true, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsDutiable_ReadOnly", true, invoiceCharge.J7_IsDutiable_ReadOnly);
				AssertHasError(invoiceCharge.J7_PercentageInfo, errorMsg);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().Invoices.AddNew().Charges.AddNew();
	}

	class InvoiceChargeForTest : InvoiceCharge
	{
		public InvoiceChargeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new bool J7_IsIncludedInITOT_ReadOnly => base.J7_IsIncludedInITOT_ReadOnly;
		public new bool J7_Calc_IsIncludedInInvoiceAmount_ReadOnly => base.J7_Calc_IsIncludedInInvoiceAmount_ReadOnly;
		public new bool J7_IsDutiable_ReadOnly => base.J7_IsDutiable_ReadOnly;
	}
}
