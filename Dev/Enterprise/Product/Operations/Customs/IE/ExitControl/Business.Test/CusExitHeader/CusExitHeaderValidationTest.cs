using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class CusExitHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXH_OA_Carrier_EORI()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var report = exitHeader.CusExitReports.AddNew();
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			var targetInfo = exitHeader.CXH_OA_CarrierInfo;

			var eoriRequiredMessage = "The selected Organization must have a valid EORI code.";
			var eoriMismatchMessage = "Carrier is the Declarant on a Exit Notification (IE590). Carrier EORI must match the Message Sender EORI associated with your company's Revenue Online Services Credentials. This is entered on the company record.";
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			exitHeader.CXH_OA_Carrier = carrier.MainAddress.PK;
			AssertHasMessageError("EORI code needed.", targetInfo, eoriRequiredMessage);
			AssertNoMessageError("EORI mismatch should not take place if no EORI is entered.", exitHeader.CXH_OA_CarrierInfo, eoriMismatchMessage);

			var cusCode = carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DOESNOTMATCH", Core.Constants.CountryCodes.Ireland);

			exitHeader.Validation.ValidateCXH_OA_Carrier();
			AssertNoMessageError("EORI code needed.", targetInfo, eoriRequiredMessage);
			AssertNoMessageError("EORI mismatch should not take place if no company EORI is entered.", exitHeader.CXH_OA_CarrierInfo, eoriMismatchMessage);

			var ieWrapper = IE.Business.GlbCompanyWrapper.Get(exitHeader.Company);
			ieWrapper.GlbExternalPassword.GP_MailBoxID = "IECOMPANYPASS123";
			exitHeader.Validation.ValidateCXH_OA_Carrier();
			AssertNoMessageError("EORI code needed.", targetInfo, eoriRequiredMessage);
			AssertHasMessageError("EORI does not match company EORI", exitHeader.CXH_OA_CarrierInfo, eoriMismatchMessage);

			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			exitHeader.Validation.ValidateCXH_OA_Carrier();
			AssertNoMessageError("EORI code needed.", targetInfo, eoriRequiredMessage);
			AssertNoMessageError("EORI validation should not take place with no EXT reports.", exitHeader.CXH_OA_CarrierInfo, eoriMismatchMessage);

			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			cusCode.OK_CustomsRegNo = "COMPANYPASS123";
			exitHeader.Validation.ValidateCXH_OA_Carrier();
			AssertNoMessageError("EORI code needed.", targetInfo, eoriRequiredMessage);
			AssertNoMessageError("EORI code of carrier matches company.", exitHeader.CXH_OA_CarrierInfo, eoriMismatchMessage);
		}

		public void TestCXH_GS_NKCustomsAgent()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_LoginName = "Current User";
			staff.GS_IsActive = true;

			var exitHeader = Factory.New<CusExitHeader>();
			var report = exitHeader.CusExitReports.AddNew();
			var targetInfo = exitHeader.CXH_GS_NKCustomsAgentInfo;

			const string error = "The code you have selected is not in the list.";

			CombineAssertions(() =>
			{
				exitHeader.CXH_GS_NKCustomsAgent = "000";
				AssertHasMessageError(targetInfo, error);

				exitHeader.CXH_GS_NKCustomsAgent = "XYZ";
				exitHeader.Validation.ValidateCXH_GS_NKCustomsAgent();
				AssertNoMessageError(targetInfo, error);
			});
		}
	}
}
