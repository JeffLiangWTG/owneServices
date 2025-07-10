using CargoWise.EntityFramework;
using CargoWise.Main.Navigation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class ModuleMenuItemTest : TransactionedTestCase
	{
		public void TestGetMenuItemText_OpenInNewWindow_IsZFilterModule()
		{
			var module = new MainFormModule(ModuleIDs.AcceptabilityBand);
			using (var testCase1 = new ModuleMenuItem(module, null, null))
			{
				Assert(ZModuleFactory.Instance.IsZFilterModule(module.ModuleID));
				AssertEquals("Open Acceptability Bands in a &New Window", testCase1.Text);
			}
		}

		public void TestGetMenuItemText_OpenInNewWindow_IsZPopupModuleNonSingleton()
		{
			var module = new MainFormModule(ModuleIDs.CartageLegPlanner);
			using (var testCase1 = new ModuleMenuItem(module, null, null))
			{
				Assert(ZModuleFactory.Instance.IsZPopupModuleNonSingleton(module.ModuleID));
				AssertEquals("Open Leg Planner in a &New Window", testCase1.Text);
			}
		}

		public void TestGetMenuItemText_OpenInCargoWiseClassic_IsZFilterModule_BlazorWinFormsHybridMode_StaffInGroupWithFeature()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;

			var module = new MainFormModule(ModuleIDs.AcceptabilityBand);

			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var featureTest = factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_GG_Group = group.PK;
			featureTest.SFT_FeatureName = StmFeatureTest.WinzorFeatureCode;

			var acceptabilityBandFeature = factory.NewWithValidTestData<StmFeatureTest>();
			acceptabilityBandFeature.SFT_GG_Group = group.PK;
			acceptabilityBandFeature.SFT_FeatureName = "ACCEPTABILITYBAND";

			factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var testCase1 = new ModuleMenuItem(module, null, null))
			{
				Assert(ZModuleFactory.Instance.IsZFilterModule(module.ModuleID));
				AssertEquals($"Open Acceptability Bands {(Globals.IsWinzor ? "in a &New Window" : "i&n CargoWise Classic")}", testCase1.Text);
			}
		}

		public void TestGetMenuItemText_OpenInNewWindow_IsZFilterModule_BlazorWinFormsHybridMode_StaffNotInGroupWithFeature()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;

			var module = new MainFormModule(ModuleIDs.AcceptabilityBand);
			using (var testCase1 = new ModuleMenuItem(module, null, null))
			{
				Assert(ZModuleFactory.Instance.IsZFilterModule(module.ModuleID));
				AssertEquals("Open Acceptability Bands in a &New Window", testCase1.Text);
			}
		}

		#region Test_MenuItemText_OpenInNewWindow

		public void Test_NewWindowMenuItemText_WithoutWinzorFeature_NeverShowClassic()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			Assert_MenuItemText(staff, isAcceptabilityBandCW1Classic: false, isRefAirlineCW1Classic: false);
		}

		public void Test_NewWindowMenuItemText_WithWinzorAllAndMainFormFeature_NeverShowClassic()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var winzorFeature = factory.NewWithValidTestData<StmFeatureTest>();
			winzorFeature.SFT_GG_Group = group.PK;
			winzorFeature.SFT_FeatureName = StmFeatureTest.WinzorFeatureCode;

			var winzorAllFeature = factory.NewWithValidTestData<StmFeatureTest>();
			winzorAllFeature.SFT_GG_Group = group.PK;
			winzorAllFeature.SFT_FeatureName = StmFeatureTest.WinzorAllFeaturesCode;

			var winzorMainFormFeature = factory.NewWithValidTestData<StmFeatureTest>();
			winzorMainFormFeature.SFT_GG_Group = group.PK;
			winzorMainFormFeature.SFT_FeatureName = StmFeatureTest.WinzorMainFormFeatureCode;

			factory.Save();

			Assert_MenuItemText(staff, isAcceptabilityBandCW1Classic: false, isRefAirlineCW1Classic: false);
		}

		public void Test_NewWindowMenuItemText_WithWinzorAllFeature()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var winzorFeature = factory.NewWithValidTestData<StmFeatureTest>();
			winzorFeature.SFT_GG_Group = group.PK;
			winzorFeature.SFT_FeatureName = StmFeatureTest.WinzorFeatureCode;

			var winzorAllFeature = factory.NewWithValidTestData<StmFeatureTest>();
			winzorAllFeature.SFT_GG_Group = group.PK;
			winzorAllFeature.SFT_FeatureName = StmFeatureTest.WinzorAllFeaturesCode;

			factory.Save();

			Assert_MenuItemText(staff, isAcceptabilityBandCW1Classic: !Globals.IsWinzor, isRefAirlineCW1Classic: !Globals.IsWinzor);
		}

		public void Test_NewWindowMenuItemText_WithWinzorAndModuleFeature()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var group = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var winzorFeature = factory.NewWithValidTestData<StmFeatureTest>();
			winzorFeature.SFT_GG_Group = group.PK;
			winzorFeature.SFT_FeatureName = StmFeatureTest.WinzorFeatureCode;

			var acceptabilityBandFeature = factory.NewWithValidTestData<StmFeatureTest>();
			acceptabilityBandFeature.SFT_GG_Group = group.PK;
			acceptabilityBandFeature.SFT_FeatureName = "ACCEPTABILITYBAND";

			factory.Save();

			Assert_MenuItemText(staff, isAcceptabilityBandCW1Classic: !Globals.IsWinzor, isRefAirlineCW1Classic: false);
		}

		void Assert_MenuItemText(GlbStaff staff, bool isAcceptabilityBandCW1Classic, bool isRefAirlineCW1Classic)
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;

			var moduleAcceptabilityBand = new MainFormModule(ModuleIDs.AcceptabilityBand);
			var moduleRefAirline = new MainFormModule(ModuleIDs.RefAirline);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var menuItemAcceptabilityBand = new ModuleMenuItem(moduleAcceptabilityBand, null, null))
			using (var menuItemRefAirline = new ModuleMenuItem(moduleRefAirline, null, null))
			{
				AssertEquals($"Open Acceptability Bands {(isAcceptabilityBandCW1Classic ? "i&n CargoWise Classic" : "in a &New Window")}", menuItemAcceptabilityBand.Text);
				AssertEquals($"Open Airlines {(isRefAirlineCW1Classic ? "i&n CargoWise Classic" : "in a &New Window")}", menuItemRefAirline.Text);
			}
		}

		#endregion

		public void TestGetMenuItemTextOpenFilterLayout()
		{
			var dummyfilter = new DummyFilterStripBusinessObject();
			var filter = dummyfilter.Layouts.AddNew();
			filter.S9_FilterName = "Leg Planner";
			using (var testCase1 = new ModuleMenuItem(null, null, filter))
			{
				AssertEquals("Open Leg Planner filter layout", testCase1.Text);
			}
		}

		public void TestGetMenuItemTextOpen()
		{
			var module = new MainFormModule(ModuleIDs.RoutingLookups);
			using (var testCase1 = new ModuleMenuItem(module, null, null))
			{
				AssertEquals("Open Global Flight Schedules", testCase1.Text);
			}
		}
	}
}
