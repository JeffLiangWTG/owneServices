using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	static class MessageTestHelper
	{
		internal static GlbCompany CreateCompany(BusinessObjectFactory factory, string code = "IE1")
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = $"TEST {code} COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			return company;
		}

		internal static GlbBranch CreateBranch(GlbCompany company, string code = "IEB")
		{
			var branch = company.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_BranchName = $"TEST {code} BRANCH";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			return branch;
		}

		internal const string ReleaseNotification1 = "Full release of goods (as per declaration) - Movement closed";
		internal const string ReleaseNotification2 = "Partial release of goods";
		internal const string ReleaseNotification3 = "Partial release of goods - Movement closed";
		internal const string ReleaseNotification4 = "No release of goods";

		internal static void SetupReleaseCodes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);

			helper.CreateNewOrGetExistingCusCodeType("CL163", "Release Type Code");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL163",
				code: "1",
				description: "Partial release",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL163",
				code: "2",
				description: "Full release",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			helper.CreateNewOrGetExistingCusCodeType("CL164", "Release Notification Code");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL164",
				code: "1",
				description: ReleaseNotification1,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL164",
				code: "2",
				description: ReleaseNotification2,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL164",
				code: "3",
				description: ReleaseNotification3,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL164",
				code: "4",
				description: ReleaseNotification4,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			factory.Save();
		}

		internal static void SetupCL215Types(BusinessObjectFactory factory)
		{
			IE.Business.Testing.MessageTestHelper.SetupCL215Types(factory);
		}

		internal static void SetupCL384Types(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL384, "Notification Type");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL384,
				code: "0",
				description: "Decision to Control (and requested documents if needed)",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL384,
				code: "1",
				description: "Additional documents request",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL384,
				code: "2",
				description: "Intention to Control",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			factory.Save();
		}

		internal static void SetupCL716Types(BusinessObjectFactory factory)
		{
			IE.Business.Testing.MessageTestHelper.SetupCL716Types(factory);
		}
	}
}
