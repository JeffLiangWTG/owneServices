using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json.Linq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class EnvironmentVariableTest : TestCaseWithFactory
	{
		public void TestEnvironment_CurrentUser()
		{
			using (var scope = new MacroScope())
			{
				scope.SetVariable(VariableNames.Environment, new MasterFiles.Business.Macros.Environment());

				var result = "@env.CurrentUser".Evaluate(scope).ToMap();

				AssertMultilineASCIIEquals("CurrentUser",
@"{
  ""Code"": ""RH"",
  ""Name"": ""Russel Hassel"",
  ""Phone"": ""02 8888 8888"",
  ""Email"": ""rus@ty.co"",
  ""Fax"": ""02 9999 9999"",
  ""IsDeveloper"": ""N"",
  ""Signature"": null,
  ""Certificates"": [],
  ""Groups"": [
    {
      ""Code"": ""ALL"",
      ""Description"": ""ALL STAFF"",
      ""DomainName"": """",
      ""Category"": """",
      ""ParentGroup"": null,
      ""IsActive"": ""Y"",
      ""IsNonSecurity"": ""N""
    }
  ]
}", Format(result));
			}
		}

		public void TestEnvironment_Company()
		{
			using (var scope = new MacroScope())
			{
				scope.SetVariable(VariableNames.Environment, new MasterFiles.Business.Macros.Environment());

				var companyResult = (ICompany)"@env.Company".Evaluate(scope);

				AssertEquals("Company.Code", "ZZZ", companyResult.Code);
				AssertEquals("Company.Name", "ZZZ Comp", companyResult.Name);
				AssertEquals("Company.Country.Code", "US", companyResult.Country.Code);
				AssertEquals("Company.Country.Name", "United States", companyResult.Country.Name);
				AssertEquals("Company.Country.LicenceCode", "EDIZZZDAT", companyResult.LicenceCode);
				AssertEquals("Company.Organization.Code", "CHICHIMDW", companyResult.Organization.Code);
				AssertEquals("Company.Organization.Name", "Chicken Chipotle", companyResult.Organization.Name);
				AssertEquals("Company.Organization.Unloco.Code", "USCHI", companyResult.Organization.Unloco.Code);
				AssertEquals("Company.Organization.Unloco.Name", "Chicago", companyResult.Organization.Unloco.Name);
				AssertEquals("Company.Organization.Unloco.Country.Code", "US", companyResult.Organization.Unloco.Country.Code);
				AssertEquals("Company.Organization.Unloco.Country.Name", "United States", companyResult.Organization.Unloco.Country.Name);
				AssertEquals("Company.Organization.MainAddress.CompanyName", "Chicken Chipotle", companyResult.Organization.MainAddress.CompanyName);
				AssertEquals("Company.Organization.MainAddress.AdditionalAddressInformation", "", companyResult.Organization.MainAddress.AdditionalAddressInformation);
				AssertEquals("Company.Organization.MainAddress.AddressLine1", "address 1", companyResult.Organization.MainAddress.AddressLine1);
				AssertEquals("Company.Organization.MainAddress.AddressLine2", "address 2", companyResult.Organization.MainAddress.AddressLine2);
				AssertEquals("Company.Organization.MainAddress.City", "", companyResult.Organization.MainAddress.City);
				AssertEquals("Company.Organization.MainAddress.State", "IL", companyResult.Organization.MainAddress.State);
				AssertEquals("Company.Organization.MainAddress.Postcode", "454545", companyResult.Organization.MainAddress.Postcode);
				AssertEquals("Company.Organization.MainAddress.Unloco.Code", "USCHI", companyResult.Organization.MainAddress.Unloco.Code);
				AssertEquals("Company.Organization.MainAddress.Unloco.Name", "Chicago", companyResult.Organization.MainAddress.Unloco.Name);
				AssertEquals("Company.Organization.MainAddress.Unloco.Country.Code", "US", companyResult.Organization.MainAddress.Unloco.Country.Code);
				AssertEquals("Company.Organization.MainAddress.Unloco.Country.Name", "United States", companyResult.Organization.MainAddress.Unloco.Country.Name);
				AssertEquals("Company.Organization.RegistrationNumbers.Count", 0, companyResult.Organization.RegistrationNumbers.Count);
			}
		}

		public void TestEnvironment_Branch()
		{
			using (var scope = new MacroScope())
			{
				scope.SetVariable(VariableNames.Environment, new MasterFiles.Business.Macros.Environment());

				var branchResult = (IBranch)"@env.Branch".Evaluate(scope);

				AssertEquals("Branch.Name", "Los Branchos", branchResult.Name);
				AssertEquals("Branch.City", "", branchResult.City);
				AssertEquals("Branch.HomePort.Code", "", branchResult.HomePort.Code);
				AssertEquals("Branch.HomePort.Name", "", branchResult.HomePort.Name);
				AssertEquals("Branch.HomePort.Country.Code", "", branchResult.HomePort.Country.Code);
				AssertEquals("Branch.HomePort.Country.Name", "", branchResult.HomePort.Country.Name);
				AssertEquals("Branch.Country.Code", "", branchResult.Country.Code);
				AssertEquals("Branch.Country.Name", "", branchResult.Country.Name);
				AssertEquals("Branch.Organization.Code", "MONSTESYD", branchResult.Organization.Code);
				AssertEquals("Branch.Organization.Name", "Monsters Inc", branchResult.Organization.Name);
				AssertEquals("Branch.Organization.Unloco.Code", "AUSYD", branchResult.Organization.Unloco.Code);
				AssertEquals("Branch.Organization.Unloco.Name", "Sydney", branchResult.Organization.Unloco.Name);
				AssertEquals("Branch.Organization.Unloco.Country.Code", "AU", branchResult.Organization.Unloco.Country.Code);
				AssertEquals("Branch.Organization.Unloco.Country.Name", "Australia", branchResult.Organization.Unloco.Country.Name);
				AssertEquals("Branch.Organization.MainAddress.CompanyName", "Monsters Inc", branchResult.Organization.MainAddress.CompanyName);
				AssertEquals("Branch.Organization.MainAddress.AdditionalAddressInformation", "", branchResult.Organization.MainAddress.AdditionalAddressInformation);
				AssertEquals("Branch.Organization.MainAddress.AddressLine1", "address 1", branchResult.Organization.MainAddress.AddressLine1);
				AssertEquals("Branch.Organization.MainAddress.AddressLine2", "address 2", branchResult.Organization.MainAddress.AddressLine2);
				AssertEquals("Branch.Organization.MainAddress.City", "", branchResult.Organization.MainAddress.City);
				AssertEquals("Branch.Organization.MainAddress.State", "NSW", branchResult.Organization.MainAddress.State);
				AssertEquals("Branch.Organization.MainAddress.Postcode", "2015", branchResult.Organization.MainAddress.Postcode);
				AssertEquals("Branch.Organization.MainAddress.Unloco.Code", "AUSYD", branchResult.Organization.MainAddress.Unloco.Code);
				AssertEquals("Branch.Organization.MainAddress.Unloco.Name", "Sydney", branchResult.Organization.MainAddress.Unloco.Name);
				AssertEquals("Branch.Organization.MainAddress.Unloco.Country.Code", "AU", branchResult.Organization.MainAddress.Unloco.Country.Code);
				AssertEquals("Branch.Organization.MainAddress.Unloco.Country.Name", "Australia", branchResult.Organization.MainAddress.Unloco.Country.Name);
				AssertEquals("Branch.Organization.RegistrationNumbers.Count", 0, branchResult.Organization.RegistrationNumbers.Count);

				var result = "@env.Branch.PortCode".Evaluate(scope);
				Assert("Branch.PortCode", result.Equals(""));

				result = "@env.Branch.PortName".Evaluate(scope);
				Assert("Branch.PortName", result.Equals(""));

				result = "@env.Branch.PortCountry".Evaluate(scope);
				Assert("Branch.PortCountry", result.Equals("Australia"));
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
