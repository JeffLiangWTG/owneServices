using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CustomerService.Business
{
	internal sealed class IncidentApprovalLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Static

		public void TestGetOtherModuleCode()
		{
			AssertEquals(MandatoryCustomerServiceMenuSectionList.Codes.Other, IncidentApprovalLookups.GetOtherModuleCode(ModuleListType.MenuSection));
			AssertEquals(Cr8ModuleList.Codes.OtherComplianceIssue, IncidentApprovalLookups.GetOtherModuleCode(ModuleListType.Cr8));
			AssertEquals(Cr9ModuleList.Codes.OtherConsultingPleaseDescribeClearly, IncidentApprovalLookups.GetOtherModuleCode(ModuleListType.Cr9));
		}

		#endregion

		public void TestLists()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();
			AssertEquals("14 statuses", 14, approval.Lookups.StatusList.Count);
			AssertEquals("9 criticalities", 9, approval.Lookups.CriticalityList.Count);
			Assert("LicenceCompanyList", approval.Lookups.LicenceCompanyList.ContainsCode(approval.IA_LicenceCode));
		}

		#region ModuleList

		public void TestMenuSectionList()
		{
			var approval = Factory.New<IncidentApproval>();
			var lookups = new IncidentApprovalLookups(approval);

			var actualMenuSectionList = lookups.MenuSectionListIncludingHidden;
			var notReleasedModules = new HashSet<string> { "OCS", "EQM" }; // OCS & EQM is not releases as of 2025-04-02

			var mandatoryCustomerServiceMenuSectionList = new MandatoryCustomerServiceMenuSectionList();
			CombineAssertions("Should contain mandatory menu sections", () =>
			{
				foreach (var menuSection in mandatoryCustomerServiceMenuSectionList.Values)
				{
					if (!notReleasedModules.Contains(menuSection.Code))
					{
						AssertContainsModule(actualMenuSectionList, menuSection.Code, menuSection.Description);
					}
				}
			});

			var moduleTreeCustomerServiceMenuSectionList = new ModuleTreeCustomerServiceMenuSectionList();
			CombineAssertions("Should contain all module tree sections", () =>
			{
				foreach (var category in ModuleTree.Tree.Categories.ValuesIncludingHidden)
				{
					foreach (var section in category.Sections.ValuesIncludingHidden)
					{
						var menuSectionCode = section.CustomerServiceMenuSectionCode;
						if (!string.IsNullOrEmpty(menuSectionCode)
							&& !notReleasedModules.Contains(menuSectionCode))
						{
							var menuSectionDescription = mandatoryCustomerServiceMenuSectionList.ContainsKey(menuSectionCode) ? mandatoryCustomerServiceMenuSectionList.GetDescriptionFromCode(menuSectionCode) : moduleTreeCustomerServiceMenuSectionList.GetDescriptionFromCode(menuSectionCode);
							AssertContainsModule(actualMenuSectionList, menuSectionCode, menuSectionDescription);
						}
					}
				}
			});
		}

		public void TestMenuSectionList_UniqueCodes()
		{
			var approval = Factory.New<IncidentApproval>();
			var lookups = new IncidentApprovalLookups(approval);
			TestModuleList_UniqueCodes(lookups.MenuSectionList);
		}

		public void TestCr8ModuleList()
		{
			var approval = Factory.New<IncidentApproval>();
			var lookups = approval.Lookups;
			AssertModulesInAnyOrder(new Cr8ModuleList(), lookups.Cr8ModuleList);
		}

		public void TestCr8ModuleList_UniqueCodes()
		{
			var approval = Factory.New<IncidentApproval>();
			var lookups = new IncidentApprovalLookups(approval);
			TestModuleList_UniqueCodes(lookups.Cr8ModuleList);
		}

		public void TestCr9ModuleList()
		{
			var approval = Factory.New<IncidentApproval>();
			var lookups = approval.Lookups;
			AssertModulesInAnyOrder(new Cr9ModuleList(), lookups.Cr9ModuleList);
		}

		public void TestCr9ModuleList_UniqueCodes()
		{
			var approval = Factory.New<IncidentApproval>();
			var lookups = new IncidentApprovalLookups(approval);
			TestModuleList_UniqueCodes(lookups.Cr9ModuleList);
		}

		static void TestModuleList_UniqueCodes(CodeDescriptionPairList moduleList)
		{
			var codesUsedByMultipleModules =
				from ICodeDescription module in moduleList
				group module by module.Code into moduleGroup
				where moduleGroup.Count() > 1
				select new { Code = moduleGroup.Key, Modules = moduleGroup.ToList() };

			if (codesUsedByMultipleModules.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var codeUsedByMultipleModules in codesUsedByMultipleModules)
					{
						Fail(string.Format("Code [{0}] is not unique. The following modules have this code: {1}", codeUsedByMultipleModules.Code, string.Join(", ", codeUsedByMultipleModules.Modules.Select(m => string.Format("'{0}'", m.Description)))));
					}
				});
			}
			else
			{
				Assert("All modules have unique codes", true);
			}
		}

		#endregion

		public void TestCreateLicenceCompanyList()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "TTA";
			company1.GC_Name = "ZZZ";
			Assert(company1.LicenceKeyIdentifier.Length == 9);

			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "TTZ";
			company2.GC_Name = "AAA";
			Assert(company1.LicenceKeyIdentifier.Length == 9);

			CodeDescriptionPairList licenceCompanyList = IncidentApprovalLookups.CreateLicenceCompanyList(Factory);
			Assert(licenceCompanyList.ContainsCode(approval.IA_LicenceCode));
			Assert(licenceCompanyList.ContainsCode(company1.LicenceKeyIdentifier));
			Assert(licenceCompanyList.ContainsCode(company2.LicenceKeyIdentifier));

			GlbCompany[] list = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));

			foreach (GlbCompany company in list)
			{
				ZString code = company.LicenceKeyIdentifier;
				if (code.Length == 9)
				{
					AssertEquals("description is name", licenceCompanyList[code].Description, company.GC_Name.ToString());
				}
			}

			for (int i = 0; i < licenceCompanyList.Count - 1; ++i)
			{
				Assert("sorted by description", 0 <= string.Compare(licenceCompanyList[i + 1].Description, licenceCompanyList[i].Description));
			}
		}

		public void TestModuleList_NoExceptions()
		{
			IncidentApproval approval = Factory.New<IncidentApproval>();

			foreach (var menuSection in approval.Lookups.MenuSectionList)
			{
				Assert("Should be a CodeDescriptionPair", menuSection is CodeDescriptionPair);
			}

			foreach (var cr8Module in approval.Lookups.Cr8ModuleList)
			{
				Assert("Should be a CodeDescriptionPair", cr8Module is CodeDescriptionPair);
			}

			foreach (var cr9Module in approval.Lookups.Cr9ModuleList)
			{
				Assert("Should be a CodeDescriptionPair", cr9Module is CodeDescriptionPair);
			}
		}

		#region Implementation

		void AssertContainsModule(CodeDescriptionPairList moduleList, string expectedCode, string expectedDescription)
		{
			var containsModule = false;
			foreach (ICodeDescription module in moduleList)
			{
				if (module.Code.Equals(expectedCode, StringComparison.OrdinalIgnoreCase) && module.Description.Equals(expectedDescription, StringComparison.OrdinalIgnoreCase))
				{
					containsModule = true;
					break;
				}
			}

			Assert("Contains module Code:[" + expectedCode + "] with Description:[" + expectedDescription + "]", containsModule);
		}

		void AssertModulesInAnyOrder(ICodeDescriptionPairList expectedModules, ICodeDescriptionPairList actualModules)
		{
			AssertEquals(expectedModules.Count, actualModules.Count);
			foreach (ICodeDescription module in expectedModules)
			{
				Assert("Contains Code", actualModules.ContainsCode(module.Code));
				AssertEquals("Description", module.Description, actualModules.GetDescriptionFromCode(module.Code));
			}
		}

		#endregion
	}
}
