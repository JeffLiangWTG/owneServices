using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CustomerService.Business
{
	internal sealed class IncidentApprovalValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCriticality()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			approval.IA_Criticality = "";
			AssertHasErrors("Criticality is mandatory", approval.IA_CriticalityInfo);

			approval.IA_Criticality = "CR1";
			AssertNoErrors("Criticality is mandatory", approval.IA_CriticalityInfo);

			approval.IA_Criticality = "XXX";
			AssertHasErrors("Criticality is invalid", approval.IA_CriticalityInfo);
		}

		public void TestReportingStaffMember()
		{
			SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, IncidentApprovalLookups.EmailRecipientSettings.ThirdPartyNotifyOnly);

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			IncidentApproval approval = Factory.New<IncidentApproval>();

			approval.IA_GS_NKReportingStaff = "";
			AssertHasErrors("StaffMember is mandatory if registry not set", approval.IA_GS_NKReportingStaffInfo);

			approval.IA_GS_NKReportingStaff = staff.GS_Code;
			AssertNoErrors("StaffMember is set", approval.IA_GS_NKReportingStaffInfo);

			SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, IncidentApprovalLookups.EmailRecipientSettings.ApprovingStaff);
			approval.IA_GS_NKReportingStaff = "";
			AssertNoErrors("StaffMember is not mandatory if registry already set", approval.IA_GS_NKReportingStaffInfo);
		}

		public void TestModule()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			approval.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			approval.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding;
			AssertNoErrors(approval.IA_ModuleInfo);

			approval.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			approval.IA_Module = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;
			AssertHasErrors("Invalid Menu Section", approval.IA_ModuleInfo);

			approval.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			approval.IA_Module = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;
			AssertNoErrors(approval.IA_ModuleInfo);

			approval.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			approval.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding;
			AssertHasErrors("Invalid CR8 Module", approval.IA_ModuleInfo);

			approval.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			approval.IA_Module = Cr9ModuleList.Codes.AccountingDataTakeOn;
			AssertNoErrors(approval.IA_ModuleInfo);

			approval.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			approval.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding;
			AssertHasErrors("Invalid CR9 Module", approval.IA_ModuleInfo);

			approval.IA_Module = "";
			AssertHasErrors("Module is mandatory", approval.IA_ModuleInfo);

			approval.IA_Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			approval.Validation.ValidateIA_Module();
			AssertEquals("Precondition", true, approval.IA_Module_ReadOnly);
			AssertNoErrors("Module is not validated when readonly", approval.IA_ModuleInfo);
		}

		public void TestIncidentSummary()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			approval.IA_IncidentSummary = "";
			AssertHasErrors("IncidentSummary is mandatory", approval.IA_IncidentSummaryInfo);

			approval.IA_IncidentSummary = "Test Summary Data";
			AssertNoErrors("IncidentSummary is mandatory", approval.IA_IncidentSummaryInfo);
		}

		public void TestIncidentDetails()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			approval.IA_IncidentDetails = "";
			AssertHasErrors("IncidentDetails is mandatory", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails = "Test Details Data";
			AssertHasErrors("IncidentDetails too short", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails = "Test Details Data Is OK";
			AssertNoErrors("IncidentDetails is long enough", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails = "問題ある";
			AssertHasErrors("IncidentDetails too short", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails = "問題がある";
			AssertNoErrors("IncidentDetails is long enough", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails =
@"Line
Break
Should
Count
Too";
			AssertNoErrors("IncidentDetails is long enough", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails =
@"LineBreakAfterSpace

  ";
			AssertHasErrors("Empty lines do not count", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails =
@"問題
がある";
			AssertNoErrors("IncidentDetails is long enough", approval.IA_IncidentDetailsInfo);

			approval.IA_IncidentDetails =
@"問題

  ";
			AssertHasErrors("Empty lines do not count", approval.IA_IncidentDetailsInfo);
		}

		public void TestClientSpecifiedStatus()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("BUG", "It's a bug");
			list.AddPair("FTR", "It's a feature");
			SystemDataRegistry.Instance.CustomerStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			approval.IA_ClientSpecifiedStatus = "";
			AssertNoErrors("Field is not mandatory", approval.IA_ClientSpecifiedStatusInfo);

			approval.IA_ClientSpecifiedStatus = "FTR";
			AssertNoErrors("There is such value in the list", approval.IA_ClientSpecifiedStatusInfo);

			approval.IA_ClientSpecifiedStatus = "666";
			AssertHasErrors("There is no such value in the list", approval.IA_ClientSpecifiedStatusInfo);
		}

		public void TestLicenceCode()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			Assert(approval.IA_LicenceCode.Length > 0);
			AssertNoErrors("LicenceCode is mandatory", approval.IA_LicenceCodeInfo);

			approval.IA_LicenceCode = "";
			AssertHasErrors("LicenceCode is mandatory", approval.IA_LicenceCodeInfo);

			approval.IA_LicenceCode = "987654321";
			AssertHasErrors("LicenceCode is invalid", approval.IA_LicenceCodeInfo);

			approval.IA_LicenceCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			AssertNoErrors("Valid LicenceCode", approval.IA_LicenceCodeInfo);

			approval.IA_Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_Code = "XXX";
			GlbCompany.CurrentCompany.GC_Code = "XXX";

			Assert(approval.IA_LicenceCode_ReadOnly);
			AssertNotEquals(company.LicenceKeyIdentifier, approval.IA_LicenceCode);
			AssertNotEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, approval.IA_LicenceCode);
			approval.Validation.ValidateIA_LicenceCode();
			AssertNoErrors("No error for LicenceCode once incident is sent", approval.IA_LicenceCodeInfo);

			approval.IA_LicenceCode = "";
			approval.Validation.ValidateIA_LicenceCode();
			AssertNoErrors("No error for LicenceCode once incident is sent", approval.IA_LicenceCodeInfo);
		}
	}
}
