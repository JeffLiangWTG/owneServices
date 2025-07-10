using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusBondDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRemaining()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			CombineAssertions(() =>
			{
				guarantee.PW_BondAmount = 1m;
				guarantee.Validation.ValidateRemaining();
				AssertNoErrorContaining(guarantee.RemainingInfo, MandatoryValidation.ValueCannotBeNegative);
				var releaseGuarantee = instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee.PW_BondAmount = 2m;
				guarantee.Validation.ValidateRemaining();
				AssertHasErrorContaining(guarantee.RemainingInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckPW_BondType_NotRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			ValidationTestHelper.AssertFieldIsNotMandatory(guarantee.PW_BondTypeInfo);
		}

		public void TestCheckPW_BondType_Mandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondAmount = 1m;
			ValidationTestHelper.AssertErrorIfNotEntered(guarantee.PW_BondTypeInfo);
		}

		public void TestCheckPW_BondAmount_NotRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			ValidationTestHelper.AssertFieldIsNotMandatory(guarantee.PW_BondAmountInfo);
		}

		public void TestCheckPW_BondAmount_Mandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(guarantee.PW_BondAmountInfo);
		}

		public void TestCheckPW_ActivityCode_AllCustomsProceduresShouldUseConsumedGuarantee()
		{
			AssertAllCustomsProceduresShouldUseGuarantee(GuaranteeActivityCodeList.Codes.ConsumesGuarantee, YesNoList.Codes.Yes, YesNoList.Codes.No, "All invoice lines must have a Procedure and all of these Procedures should consume a guarantee and not release a guarantee.");
		}

		void AssertAllCustomsProceduresShouldUseGuarantee(string activityCode, string isGuaranteeConsumed, string isGuaranteeReleased, string messageError)
		{
			var procedure = CreateNotConsumedAndNotReleasedProcedure(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var detail = instruction.Guarantee;
			detail.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
			detail.PW_CPH_Guarantee = CreateGuaranteeHeader(Factory).PK;
			detail.PW_ActivityCode = activityCode;
			CombineAssertions(() =>
			{
				AssertHasMessageError("Invoice Line is not using procedure", detail.PW_ActivityCodeInfo, messageError);
				invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode;
				detail.Validation.ValidatePW_ActivityCode();
				AssertHasMessageError("Procedure is not using guarantee", detail.PW_ActivityCodeInfo, messageError);
				procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
				procedure.ZZ6_IsGuaranteeReleased = isGuaranteeReleased;
				detail.Validation.ValidatePW_ActivityCode();
				AssertNoMessageError("All procedures are using guarantee", detail.PW_ActivityCodeInfo, messageError);
			});
		}

		public static RefCusProcedure CreateNotConsumedAndNotReleasedProcedure(BusinessObjectFactory factory)
		{
			var procedure = factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "XX";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.No;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.No;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return procedure;
		}

		public static CusGuaranteeHeader CreateGuaranteeHeader(BusinessObjectFactory factory, bool isSingleUse = false)
		{
			var guaranteeHeader = factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			guaranteeHeader.CPH_IsSingleUse = isSingleUse;
			return guaranteeHeader;
		}
	}
}
