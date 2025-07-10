using System;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsing.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Module.Testing
{
	[TestedType(typeof(BarcodeParsingModule))]
	class BarcodeParsingModuleTest : ZModuleBasherTest
	{
		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (var module = new BarcodeParsingModule())
			{
				AssertEquals(typeof(BarcodeRuleFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		#endregion

		#region TestFilterControl

		public void TestFilterControl()
		{
			using (var module = new BarcodeParsingModule())
			using (var controlForTest = (IDisposable)module.GetNewFilterControlForGrid())
			{
				AssertEquals(typeof(BarcodeRuleFilterControl), controlForTest.GetType());
			}
		}

		#endregion

		#region TestNewMenuItem

		public void TestNewMenuItem()
		{
			using (var module = new BarcodeParsingModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New");
				AssertEquals("New menu items should contain 2 sub items", 2, newMenu.MenuItems.Count);
				AssertEquals("&New", newMenu.MenuItems[0].Text);
				AssertEquals("New Warehouse GS1 Rule Set with System Default Rules", newMenu.MenuItems[1].Text);
			}
		}

		#endregion

		#region TestGridCollection

		public void TestGridCollection()
		{
			using (var module = new BarcodeParsingModule())
			{
				AssertEquals(typeof(BarcodeRuleCollection), module.GridCollection.GetType());
			}
		}

		#endregion

		#region TestLicenceCheckPoint

		public void TestLicenceCheckPoint()
		{
			using (var module = new BarcodeParsingModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckpoint

		public void TestSecurityCheckpoint()
		{
			using (var module = new BarcodeParsingModule())
			{
				AssertEquals(Env.Security.BarcodeParsing, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestNewRuleSetModuleIsDefaultedFromCurrentFilter

		public void TestNewRuleSetModuleIsDefaultedFromCurrentFilter()
		{
			using (var module = new BarcodeParsingModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New").MenuItems.FindByText("&New");

				// no module filter
				newMenu.PerformClick();

				using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
				{
					var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
					AssertEquals("No module filter applied, the module on the form should default to empty.", "", barcodeRuleSet.BRS_Module);
					AssertNoErrors("Not entering a module should not result in an error.", barcodeRuleSet.BRS_ModuleInfo);
				}

				var filter = (ModuleTextFilter)module.FilterBusinessObject["Module"];
				filter.IsActive = true;

				// invalid module filter
				filter.Property = "XXX";
				newMenu.PerformClick();
				using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
				{
					var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
					AssertEquals("No valid module filter applied, the module on the form should default to empty.", "", barcodeRuleSet.BRS_Module);
					AssertNoErrors("Not entering a module should not result in an error.", barcodeRuleSet.BRS_ModuleInfo);
				}

				// invalid module filter
				foreach (CodeDescriptionPair moduleType in new BarcodeModuleTypes())
				{
					filter.Property = moduleType.Code;
					newMenu.PerformClick();
					using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
					{
						var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
						AssertEquals($"{moduleType.Description} module filter applied, the module on the form should default to {moduleType.Code}.", moduleType.Code, barcodeRuleSet.BRS_Module);
					}
				}
			}
		}

		#endregion

		#region TestNewRuleSetModuleIsDefaultedToEmpty

		public void TestNewRuleSetModuleIsDefaultedToEmpty()
		{
			using (var module = new BarcodeParsingModule())
			{
				var originalSetting = Env.Security.BarcodeParsingNew.IsAllowed;

				try
				{
					Env.Security.BarcodeParsingNew.IsAllowed = true;
					var newMenu = module.FormActionMenu.FindByText("&New").MenuItems.FindByText("&New");

					// no module filter
					newMenu.PerformClick();

					using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
					{
						var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
						AssertEquals("No module filter applied, the module on the form should default to empty because Etail is enabled.", "", barcodeRuleSet.BRS_Module);
						AssertNoErrors("Not entering a module should not result in an error.", barcodeRuleSet.BRS_ModuleInfo);
					}
				}
				finally
				{
					Env.Security.BarcodeParsingNew.IsAllowed = originalSetting;
				}
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BarcodeParsing;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return Factory.NewWithValidTestData<BarcodeRule>();
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;

		#endregion
	}
}
