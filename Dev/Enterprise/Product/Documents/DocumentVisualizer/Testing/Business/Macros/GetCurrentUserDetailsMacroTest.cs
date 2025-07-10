using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json.Linq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class GetCurrentUserDetailsMacroTestTest : TestCaseWithFactory
	{
		public void TestGetCurrentUserDetails_FromBranch()
		{
			AssertGetCurrentUserDetailsMacro(
@"{
  ""AddressType"": ""CurrentUser"",
  ""AddressShortCode"": ""address 1"",
  ""OrganizationCode"": ""MONSTESYD"",
  ""OrganizationCategory"": null,
  ""AdditionalAddressInformation"": null,
  ""Address1"": ""address 1"",
  ""Address2"": ""address 2"",
  ""AddressOverride"": null,
  ""City"": ""Parramatta"",
  ""CompanyName"": ""Monsters Inc"",
  ""Contact"": ""Russel Hassel"",
  ""Port"": null,
  ""Country"": {
    ""Code"": ""AU"",
    ""Name"": ""Australia""
  },
  ""Email"": ""rus@ty.co"",
  ""Fax"": ""02 9999 9999"",
  ""GovRegNum"": null,
  ""GovRegNumType"": null,
  ""Mobile"": null,
  ""Phone"": ""02 8888 8888"",
  ""Postcode"": ""2015"",
  ""ScreeningStatus"": null,
  ""ValidationStatus"": null,
  ""State"": {
    ""Code"": ""NSW"",
    ""Description"": ""New South Wales""
  },
  ""UniversalNettingCode"": null,
  ""UniversalOfficeCode"": null,
  ""RegistrationNumberCollection"": null,
  ""IsResidential"": null,
  ""SuppressAddressValidationError"": null,
  ""LocalAddressCollection"": null,
  ""GeoLocation"": null
}");
		}

		public void TestGetCurrentUserDetails_FromCompany()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			AssertGetCurrentUserDetailsMacro(
@"{
  ""AddressType"": ""CurrentUser"",
  ""AddressShortCode"": ""address 1"",
  ""OrganizationCode"": ""CHICHIMDW"",
  ""OrganizationCategory"": null,
  ""AdditionalAddressInformation"": null,
  ""Address1"": ""address 1"",
  ""Address2"": ""address 2"",
  ""AddressOverride"": null,
  ""City"": ""Brisbane"",
  ""CompanyName"": ""Chicken Chipotle"",
  ""Contact"": ""Russel Hassel"",
  ""Port"": null,
  ""Country"": {
    ""Code"": ""US"",
    ""Name"": ""United States""
  },
  ""Email"": ""rus@ty.co"",
  ""Fax"": ""02 9999 9999"",
  ""GovRegNum"": null,
  ""GovRegNumType"": null,
  ""Mobile"": null,
  ""Phone"": ""02 8888 8888"",
  ""Postcode"": ""454545"",
  ""ScreeningStatus"": null,
  ""ValidationStatus"": null,
  ""State"": {
    ""Code"": ""IL"",
    ""Description"": ""Illinois""
  },
  ""UniversalNettingCode"": null,
  ""UniversalOfficeCode"": null,
  ""RegistrationNumberCollection"": null,
  ""IsResidential"": null,
  ""SuppressAddressValidationError"": null,
  ""LocalAddressCollection"": null,
  ""GeoLocation"": null
}
");
		}

		void AssertGetCurrentUserDetailsMacro(string expected)
		{
			const string macro = "GetCurrentUserDetails";

			var expr = macro.With<DataLibrary>().CreateExpression();

			using (var scope = new MacroScope())
			{
				var result = (MacroMap)expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertMultilineASCIIEquals("user details", expected, Format(result));
			}
		}

		#region Implementation

		IDisposable tempContext;

		protected override void SetUp()
		{
			base.SetUp();

			var companyOrg = Factory.New<OrgHeader>();
			companyOrg.OH_FullName = "Chicken Chipotle";
			companyOrg.OH_RL_NKClosestPort = "USCHI";
			companyOrg.MainAddress.Address1 = "address 1";
			companyOrg.MainAddress.Address2 = "address 2";
			companyOrg.MainAddress.State = "IL";
			companyOrg.MainAddress.City = "Brisbane";
			companyOrg.MainAddress.Postcode = "454545";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZZZ";
			company.GC_Name = "ZZZ Comp";
			company.GC_RN_NKCountryCode = "US";
			company.GC_OH_OrgProxy = companyOrg.PK;

			var branchOrg = Factory.New<OrgHeader>();
			branchOrg.OH_FullName = "Monsters Inc";
			branchOrg.OH_RL_NKClosestPort = "AUSYD";
			branchOrg.MainAddress.Address1 = "address 1";
			branchOrg.MainAddress.Address2 = "address 2";
			branchOrg.MainAddress.City = "Parramatta";
			branchOrg.MainAddress.State = "NSW";
			branchOrg.MainAddress.Postcode = "2015";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "Los Branchos";
			branch.GB_Code = "AAA";
			branch.GB_OH_OrgProxy = branchOrg.PK;

			var user = Factory.New<GlbStaff>();
			user.GS_FullName = "Russel Hassel";
			user.GS_EmailAddress = "rus@ty.co";
			user.GS_WorkPhone = "02 8888 8888";
			user.GS_FaxNum = "02 9999 9999";

			Factory.Save();

			tempContext = Env.SetTemporaryUserContext(user.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK);
		}

		protected override void TearDown()
		{
			base.TearDown();

			tempContext?.Dispose();
		}

		string Format(MacroMap map)
		{
			var json = map.ToJSON();
			var jsonObj = JObject.Parse(json);

			return jsonObj.ToString();
		}

		#endregion
	}
}
