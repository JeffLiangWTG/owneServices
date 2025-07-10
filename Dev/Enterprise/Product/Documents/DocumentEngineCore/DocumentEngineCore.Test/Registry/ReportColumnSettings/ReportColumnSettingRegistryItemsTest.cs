using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	class ReportColumnSettingRegistryItemsTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestSaveAndRetreiveDelete()
		{
			string description1 = "Description with fancy bits \"1\"";
			string description2 = "Description and other #$%2";

			ReportColumnSettingRegistryItem registryItem = new ReportColumnSettingRegistryItem();

			Guid currentBranch = Env.CurrentBranch.PK;
			Guid currentDepartment = Env.CurrentDepartment.PK;
			string currentStaff = Env.CurrentUser.LoginName;

			try
			{
				Guid company2 = Factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK.ToGuid();
				Guid branch2 = ((BusinessObject)Factory.LoadTop1<IGlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, company2))).PK.ToGuid();

				ZGuid reportID1 = Guid.NewGuid();
				ZGuid reportID2 = Guid.NewGuid();
				ZGuid link1 = Guid.NewGuid();
				ZGuid link2 = Guid.NewGuid();

				string description1Report1Link1 = "Company1 Description1 Report1 Link1";
				string description1Report1Link2 = "Company1 Description1 Report1 Link2";
				string description1Report2Link1 = "Company1 Description1 Report2 Link1";
				string description1Report2Link2 = "Company1 Description1 Report2 Link2";

				string description1Report1Link1Code1 = "Company1 Description1 Report1 Link1 Code1";
				string description1Report1Link2Code1 = "Company1 Description1 Report1 Link2 Code1";
				string description1Report2Link1Code2 = "Company1 Description1 Report2 Link1 Code2";
				string description1Report2Link2Code2 = "Company1 Description1 Report2 Link2 Code2";

				string description1Report1NoLink = "Company1 Description1 Report1 no Link";
				string description1Report2NoLink = "Company1 Description1 Report2 no Link";
				string description2Report2NoLink = "Company1 Description2 Report2 no Link";
				string description2Report2Link1 = "Company 1 Description2 Report2 Link1";
				string description2Report2NoLinkCompany2 = "Company2 Description2 Report2 no Link";
				string description2Report2Link1Company2 = "Company 2 Description2 Report2 Link1";

				Env.SetUserContext(new UserContext(currentStaff, currentBranch, currentDepartment));
				registryItem.SetValue(description1, reportID1, link1, description1Report1Link1);
				registryItem.SetValue(description1, reportID2, link1, description1Report1Link2);
				registryItem.SetValue(description1, reportID1, link2, description1Report2Link1);
				registryItem.SetValue(description1, reportID2, link2, description1Report2Link2);

				registryItem.SetValue(description1, reportID1, link1, "Code1", description1Report1Link1Code1);
				registryItem.SetValue(description1, reportID2, link1, "Code1", description1Report1Link2Code1);
				registryItem.SetValue(description1, reportID1, link2, "Code2", description1Report2Link1Code2);
				registryItem.SetValue(description1, reportID2, link2, "Code2", description1Report2Link2Code2);

				registryItem.SetValue(description1, reportID1, ZGuid.Empty, description1Report1NoLink);
				registryItem.SetValue(description1, reportID2, ZGuid.Empty, description1Report2NoLink);
				registryItem.SetValue(description2, reportID2, ZGuid.Empty, description2Report2NoLink);
				registryItem.SetValue(description2, reportID2, link1, description2Report2Link1);

				Env.SetUserContext(new UserContext(currentStaff, branch2, currentDepartment));
				registryItem.SetValue(description2, reportID2, link1, description2Report2Link1Company2);
				registryItem.SetValue(description2, reportID2, ZGuid.Empty, description2Report2NoLinkCompany2);

				Env.SetUserContext(new UserContext(currentStaff, currentBranch, currentDepartment));
				AssertEquals("Should handle non existant value", "", registryItem.GetValueWithoutFallback("Descriptoins that does not exist", reportID1, link1));
				AssertEquals("Same description should be useable for unique company, report and link", description1Report1Link1, registryItem.GetValueWithoutFallback(description1, reportID1, link1));
				AssertEquals("Same description should be useable for unique company, report and link", description1Report1Link2, registryItem.GetValueWithoutFallback(description1, reportID2, link1));
				AssertEquals("Same description should be useable for unique company, report and link", description1Report2Link1, registryItem.GetValueWithoutFallback(description1, reportID1, link2));
				AssertEquals("Same description should be useable for unique company, report and link", description1Report2Link2, registryItem.GetValueWithoutFallback(description1, reportID2, link2));

				AssertEquals("Same description should be useable for unique company, report and link", description1Report1Link1Code1, registryItem.GetValueWithoutFallback(description1, reportID1, link1, "Code1"));
				AssertEquals("Same description should be useable for unique company, report and link", description1Report1Link2Code1, registryItem.GetValueWithoutFallback(description1, reportID2, link1, "Code1"));
				AssertEquals("Same description should be useable for unique company, report and link", description1Report2Link1Code2, registryItem.GetValueWithoutFallback(description1, reportID1, link2, "Code2"));
				AssertEquals("Same description should be useable for unique company, report and link", description1Report2Link2Code2, registryItem.GetValueWithoutFallback(description1, reportID2, link2, "Code2"));

				AssertEquals("Same description should be useable for unique company", description1Report1NoLink, registryItem.GetValueWithoutFallback(description1, reportID1, Guid.Empty));
				AssertEquals("Same description should be useable for unique company ", description1Report2NoLink, registryItem.GetValueWithoutFallback(description1, reportID2, Guid.Empty));
				AssertEquals("differnt description already used Company and reportid", description2Report2NoLink, registryItem.GetValueWithoutFallback(description2, reportID2, Guid.Empty));
				AssertEquals("differnt description already used Company and reportid and link", description2Report2Link1, registryItem.GetValueWithoutFallback(description2, reportID2, link1));

				Env.SetUserContext(new UserContext(currentStaff, branch2, currentDepartment));
				AssertEquals("differnt description, defferent company already used reportid and link", description2Report2Link1Company2, registryItem.GetValueWithoutFallback(description2, reportID2, link1));
				AssertEquals("differnt description, defferent company already used reportid", description2Report2NoLinkCompany2, registryItem.GetValueWithoutFallback(description2, reportID2, Guid.Empty));

				registryItem.DeleteRecord(description2, reportID2, Guid.Empty);
				AssertEquals("Should get nothing back for deleted item", "", registryItem.GetValueWithoutFallback(description2, reportID2, Guid.Empty));

				Env.SetUserContext(new UserContext(currentStaff, currentBranch, currentDepartment));
				AssertEquals("SHould get something back for this company even tho item deleted in other company", description2Report2NoLink, registryItem.GetValueWithoutFallback(description2, reportID2, Guid.Empty));
			}
			finally
			{
				Env.SetUserContext(new UserContext(currentStaff, currentBranch, currentDepartment));
			}
		}
	}
}
