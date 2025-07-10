using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.FR.Business.Declaration.Testing.CusEntryInstructionTest;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class EcoRegimeDatasWrapperTest : TestCaseWithFactory
	{
		public void TestErrorCollected_WhenSTOAuthorisationValueExceedsRange()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";

			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "100");

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = customer.PK;

			CombineAssertions(() =>
			{
				var errorCollector = new EU.Business.ErrorCollector();
				var wrapper = new EcoRegimeDatasWrapper(entryLine);
				var numberDaysOfDischarge = wrapper.NumberDaysOfDischarge;
				AssertEquals("EcoRegimeDatasWrapper.NumberDaysOfDischarge should return sbyte.MinValue if the value of STO authorisation is greater than 99.", sbyte.MinValue, numberDaysOfDischarge);
				AssertEquals(0, errorCollector.ErrorCount);

				authHeader.CusAuthorisationRules.Find(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.STO).First().CPR_ValueFrom = "1";
				errorCollector = new EU.Business.ErrorCollector();
				wrapper = new EcoRegimeDatasWrapper(entryLine);
				numberDaysOfDischarge = wrapper.NumberDaysOfDischarge;
				AssertEquals("EcoRegimeDatasWrapper.NumberDaysOfDischarge should return sbyte.MinValue if the value of STO authorisation is less than 2.", sbyte.MinValue, numberDaysOfDischarge);
				AssertEquals(0, errorCollector.ErrorCount);
			});
		}

		public void TestNoErrorsCollected_WhenSTOAuthorisationValueIsValid()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";

			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "95");

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = customer.PK;

			var errorCollector = new EU.Business.ErrorCollector();
			var wrapper = new EcoRegimeDatasWrapper(entryLine);
			CombineAssertions(() =>
			{
				var numberDaysOfDischarge = wrapper.NumberDaysOfDischarge;
				AssertEquals("EcoRegimeDatasWrapper.NumberDaysOfDischarge should return the configured value if STO authorisation configuration is valid.", (SByte)95, numberDaysOfDischarge);
				Assert("No errors are collected if the STO authorisation is configured correctly.", errorCollector.ErrorCount == 0);
			});
		}

		public void TestGuaranteeAmountAndNumberDaysOfDischarge()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);

			var entryHeader = EU.Business.Declaration.Testing.CusEntryHeaderTestHelper.SetupEntryForFeesTest<CusEntryHeader>(Factory);
			var entryLine = entryHeader.MergedLines.First();
			var indirectFee = entryLine.Fees.AddNew();
			indirectFee.CF_ChargeType = "X220";
			indirectFee.CF_ChargeAmount = 70m;
			indirectFee.CF_MethodOfPayment = "X";

			var dec = Factory.New<JobDeclaration>();
			entryHeader.CH_JE = dec.PK;
			dec.ActiveEntryHeaders.ReloadFromLocalCache();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";
			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = customer.PK;

			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "9");

			var wrapper = new EcoRegimeDatasWrapper(entryLine);
			AssertEquals("GuaranteeAmount", 200m, wrapper.GuaranteeAmount);
			AssertEquals("NumberDaysOfDischarge", (sbyte)9, wrapper.NumberDaysOfDischarge);
		}

		public void TestDecEcos()
		{
			var cle1 = declaration.PreviousDocuments.AddNew();
			cle1.CSI_Code = PreviousDocumentCodeList.Codes.CLE;
			cle1.CSI_ReferenceNumber = "CLE1";

			var cle2 = declaration.PreviousDocuments.AddNew();
			cle2.CSI_Code = PreviousDocumentCodeList.Codes.CLE;
			cle2.CSI_ReferenceNumber = "CLE2";

			var ist1 = declaration.PreviousDocuments.AddNew();
			ist1.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			ist1.CSI_ReferenceNumber = "IST1";

			var cle3 = invoiceHeader.PreviousDocuments.AddNew();
			cle3.CSI_Code = PreviousDocumentCodeList.Codes.CLE;
			cle3.CSI_ReferenceNumber = "CLE3";

			var previousDeclaration = Factory.New<JobDeclaration>();
			var previousEntryHeader = previousDeclaration.CustomsEntryHeaders.AddNew();
			previousEntryHeader.EntryNumber = "PRE_001";

			var im1 = invoiceLine.PreviousDocuments.AddNew();
			im1.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			im1.CSI_SubType = "Z";
			im1.CSI_ReferenceNumber = "PRE_001";

			invoiceLine.JI_Procedure = "1021";

			AssertNotNull(invoiceLine.PreviousInbondMovement);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"CLE1", "CLE2", "PRE_001"
			}, wrapper.DecEcos.Select(x => x.CusDeclarationID));
		}

		protected override void SetUp()
		{
			base.SetUp();

			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			wrapper = new EcoRegimeDatasWrapper(entryLine);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		EcoRegimeDatasWrapper wrapper;
	}
}
