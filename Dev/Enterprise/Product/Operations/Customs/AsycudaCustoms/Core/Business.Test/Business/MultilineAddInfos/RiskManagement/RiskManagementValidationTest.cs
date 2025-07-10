using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class RiskManagementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code_Mandatory_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertErrorIfNotEntered(CreateRiskManagement().CSI_CodeInfo);
			}
		}

		public void TestCheckCSI_Code_InvalidCode_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertErrorIfInvalidCode(CreateRiskManagement().CSI_CodeInfo, "~!@", EntryPermitTypeList.Codes.TransitPermit);
			}
		}

		public void TestCheckCSI_ReferenceNumber_Mandatory_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertErrorIfNotEntered(CreateRiskManagement().CSI_ReferenceNumberInfo);
			}
		}

		public void TestCheckCSI_DateOfIssue_Mandatory_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertErrorIfNotEntered(CreateRiskManagement().CSI_DateOfIssueInfo);
			}
		}

		public void TestCheckCSI_Value_Mandatory_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(CreateRiskManagement().CSI_ValueInfo);
			}
		}

		public void TestCheckCSI_Value_Negative_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertErrorIfValueIsNegative(CreateRiskManagement().CSI_ValueInfo);
			}
		}

		public void TestCheckCSI_ValueValidMoney()
		{
			var riskManagement = CreateRiskManagement();
			riskManagement.CSI_Value = 99999999999999999999m;
			AssertHasError(riskManagement.CSI_ValueInfo, "The number 99,999,999,999,999,999,999 is too large, the value's range of Customs Value is between -922337203685477.5808 and 922337203685477.5807.");
		}

		public void TestCheckCSI_Quantity_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(CreateRiskManagement().CSI_QuantityInfo);
			}
		}

		public void TestCSI_QuantityIsValidDecimal()
		{
			var riskManagement = CreateRiskManagement();
			riskManagement.CSI_Quantity = 99999999999999999999m;
			AssertHasError(riskManagement.CSI_QuantityInfo, "The number 99,999,999,999,999,999,999 is too large, the maximum value allowed for Net Weight in KG is 9,999,999,999,999,999.999.");
		}

		public void TestCheckCSI_Quantity2_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(CreateRiskManagement().CSI_Quantity2Info);
			}
		}

		public void TestCheckCSI_Code_Mandatory_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_Code = ZString.Empty;
				riskManagement.Validation.ValidateCSI_Code();
				AssertEquals(false, riskManagement.CSI_CodeInfo.HasNotifications());
			}
		}

		public void TestCheckCSI_Code_InvalidCode_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_Code = "~!@";
				AssertEquals(false, riskManagement.CSI_CodeInfo.HasNotifications());
			}
		}

		public void TestCheckCSI_ReferenceNumber_Mandatory_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_ReferenceNumber = ZString.Empty;
				riskManagement.Validation.ValidateCSI_ReferenceNumber();
				AssertEquals(false, riskManagement.CSI_ReferenceNumberInfo.HasNotifications());
			}
		}

		public void TestCheckCSI_DateOfIssue_Mandatory_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_DateOfIssue = ZDateTime.Empty;
				riskManagement.Validation.ValidateCSI_DateOfIssue();
				AssertEquals(false, riskManagement.CSI_DateOfIssueInfo.HasNotifications());
			}
		}

		public void TestCheckCSI_Value_Mandatory_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_Value = ZDecimal.Zero;
				riskManagement.Validation.ValidateCSI_Code();
				AssertEquals(false, riskManagement.CSI_ValueInfo.HasNotifications());
			}
		}

		public void TestCheckCSI_Value_Negative_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_Value = -1m;
				AssertEquals(false, riskManagement.CSI_ValueInfo.HasNotifications());
			}
		}

		public void TestCheckCSI_Quantity_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_Quantity = ZDecimal.Zero;
				riskManagement.Validation.ValidateCSI_Quantity();
				AssertEquals(false, riskManagement.CSI_QuantityInfo.HasNotifications());
			}
		}

		public void TestCheckCSI_Quantity2_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var riskManagement = CreateRiskManagement();
				riskManagement.CSI_Quantity2 = ZDecimal.Zero;
				riskManagement.Validation.ValidateCSI_Quantity2();
				AssertEquals(false, riskManagement.CSI_Quantity2Info.HasNotifications());
			}
		}

		RiskManagement CreateRiskManagement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			return instruction.RiskManagements.AddNew();
		}
	}
}
