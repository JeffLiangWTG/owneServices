using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Provider;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	sealed class RootSecurityInfoProviderTest : TestCaseWithFactory
	{
		public void TestFetchForCategorySecurityInfoProvider()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			var provider = new RootSecurityInfoProviderForTest(security);
			_ = provider.GetChildren().ToArray();
			AssertEquals(58, provider.FactoryForTest.ActiveFetchHintsForTable(StmMenuItem.Schema.TableName));
		}

		public void TestGetChidren()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			var provider = new RootSecurityInfoProvider(security);
			var nodes = provider.GetChildren().ToArray();

			var expectedOrder = new[] {
				"Operate",
				"Manage",
				"Maintain",
				"Notes",
				"Documents/Reports",
				"Specialized Rights",
				"Login Branches and Departments",
				"Edit User-Defined Filters",
				"Publish Global Filter Layouts",
				"Publish Global Color Schemes",
				"Edit All Global Color Schemes",
				"Publish Global Universal Copy Templates",
				"Auto-Refresh Module Grids",
				"Publish Global Text Templates",
				"Ignore Mandatory To Read News and Announcements",
				"Save Data Import Wizard Settings",
				"Contains in Numbers and References Filters"
			};

			var expectedValue = expectedOrder
				.Select((name, index) => "Nodes[" + index + "]\t" + name)
				.Aggregate("Length: " + expectedOrder.Length, (l, r) => l + "\r\n" + r);

			var actualValue = nodes
				.Select((node, index) => "Nodes[" + index + "]\t" + node.Name)
				.Aggregate("Length: " + nodes.Length, (l, r) => l + "\r\n" + r);

			AssertMultilineASCIIEquals("As this is forward facing, the ordering should be consistent", expectedValue, actualValue);
		}

		//This test has to be here because Enterprise.Security references Enterprise.Masterfiles.Business, and this test uses both assemblies.
		public void TestGlbGroupDefaultSecurityRights()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			AssertEquals("Default Security Permissions should be added", 20, group.SecurityPermissions.Count);
			foreach (GlbSecurity securityPermission in group.SecurityPermissions)
			{
				AssertEquals("All Security permissions added should be denied", "No", securityPermission.IsAllowed);
			}

			Assert("Has changes should be false on Group", !group.HasChanges);
			Assert("Has changes should be false on SecurityPermissions", !group.SecurityPermissions.HasChanges);

			var security = new SecurityCore(new GlbSecurityCollection(Factory), group, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			var provider = new RootSecurityInfoProvider(security);
			var nodes = provider.GetChildren().ToArray();

			foreach (SecurityInfoProvider node in nodes)
			{
				if (node.Checkpoint.Code == "ContainsInNumbersAndReferences")
				{
					return;
				}
				Assert(node.Checkpoint.Code + " is not allowed", group.SecurityPermissions.Any(x => ((GlbSecurity)x).GU_SecurityRight == node.Checkpoint.Code));
			}
		}

		//This test has to be here because Enterprise.Security references Enterprise.Masterfiles.Business, and this test uses both assemblies.
		public void TestNewTopLevelSecurityRights_DeniedByDefaultInALL_ExcludingGrandfatheredList()
		{
			var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));

			var security = new SecurityCore(new GlbSecurityCollection(Factory), group, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			var provider = new RootSecurityInfoProvider(security);
			var nodes = provider.GetChildren().ToArray();

			foreach (SecurityInfoProvider node in nodes)
			{
				var securityName = node.Checkpoint.Code;
				if (!GrandfatheredSecurityRights.Contains(securityName))
				{
					Assert(string.Format(NotDeniedInALLMessage, securityName), group.SecurityPermissions.Any(x => ((GlbSecurity)x).GU_SecurityRight == securityName && !((GlbSecurity)x).GU_SecurityItemIsAllowed));
				}
			}
		}

		//DO NOT ADD/REMOVE SECURITY RIGHTS FROM THIS LIST! These top level Security Rights were added before we started to enforce the rule that all new Security Rights must be denied by default in ALL.
		//We do not wish to retroactively deny them by default on ALL, because many clients may be relying on these being granted by default, and we don't wish to make them angry :)
		internal List<String> GrandfatheredSecurityRights = new List<String>() {
			"AutoRefreshModuleGrids",
			"PublishGlobalUniversalCopyTemplates",
			"PublishGlobalGridColorSchemes",
			"Login",
			"SpecializedRights",
			"DocumentsReports",
			"Notes",
			"ContainsInNumbersAndReferences" //Hey guess who's grandfathering in a new security right because they don't want THIS one denied by default either
		};

		internal string NotDeniedInALLMessage = @"You have added a new top level Security Right, {0}, without adding a corresponding data transformation to deny it.
Go to DenyRootSecurityRightsByDefaultForGroups.cs, and make a subclass that denies the new Security Right(s) you have added, and map it.
(Note that top level Security Rights that existed before this unit test are 'grandfathered', and are not required to be denied by default.)";

		#region RootSecurityInfoProviderForTest

		class RootSecurityInfoProviderForTest : RootSecurityInfoProvider
		{
			public RootSecurityInfoProviderForTest(SecurityCore security) : base(security)
			{
			}

			public BusinessObjectFactory FactoryForTest => Factory;
		}

		#endregion
	}
}
