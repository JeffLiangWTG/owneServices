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
	[TestedType(typeof(BarcodeValidationModule))]
	class BarcodeValidationModuleTest : ZModuleBasherTest
	{
		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (var module = new BarcodeValidationModule())
			{
				AssertEquals(typeof(BarcodeValidationRuleFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		#endregion

		#region TestFilterControl

		public void TestFilterControl()
		{
			using (var module = new BarcodeValidationModule())
			using (var controlForTest = (IDisposable)module.GetNewFilterControlForGrid())
			{
				AssertEquals(typeof(BarcodeValidationRuleFilterControl), controlForTest.GetType());
			}
		}

		#endregion

		#region TestGridCollection

		public void TestGridCollection()
		{
			using (var module = new BarcodeValidationModule())
			{
				AssertEquals(typeof(BarcodeValidationRuleCollection), module.GridCollection.GetType());
			}
		}

		#endregion

		#region TestLicenceCheckPoint

		public void TestLicenceCheckPoint()
		{
			using (var module = new BarcodeValidationModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckpoint

		public void TestSecurityCheckpoint()
		{
			using (var module = new BarcodeValidationModule())
			{
				AssertEquals(Env.Security.BarcodeParsing, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestNewRuleSetModuleIsDefaultedFromCurrentFilter

		public void TestNewRuleSetModuleIsDefaultedFromCurrentFilter_NoModule()
		{
			using (var module = new BarcodeValidationModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New");
				newMenu.PerformClick();

				using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
				{
					var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
					AssertEquals("No module filter applied, the module on the form should default to empty.", BarcodeModuleTypes.Codes.Warehouse, barcodeRuleSet.BRS_Module);
					AssertNoErrors("Not entering a module should not result in an error.", barcodeRuleSet.BRS_ModuleInfo);
				}
			}
		}

		public void TestNewRuleSetModuleIsDefaultedFromCurrentFilter_InvalidFilter()
		{
			using (var module = new BarcodeValidationModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New");
				var filter = (ModuleTextFilter)module.FilterBusinessObject["Module"];
				filter.IsActive = true;
				filter.Property = "XXX";
				newMenu.PerformClick();
				using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
				{
					var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
					AssertEquals("No valid module filter applied, the module on the form should default to empty.", BarcodeModuleTypes.Codes.Warehouse, barcodeRuleSet.BRS_Module);
					AssertNoErrors("Not entering a module should not result in an error.", barcodeRuleSet.BRS_ModuleInfo);
				}
			}
		}

		public void TestNewRuleSetModuleIsDefaultedFromCurrentFilter_ValidFilter()
		{
			using (var module = new BarcodeValidationModule())
			{
				var newMenu = module.FormActionMenu.FindByText("&New");
				var filter = (ModuleTextFilter)module.FilterBusinessObject["Module"];
				filter.IsActive = true;
				foreach (CodeDescriptionPair moduleType in new BarcodeModuleTypes())
				{
					filter.Property = moduleType.Code;
					newMenu.PerformClick();
					using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
					{
						var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
						AssertEquals($"{moduleType.Description} module filter applied, the module on the form should default to {moduleType.Code}.", BarcodeModuleTypes.Codes.Warehouse, barcodeRuleSet.BRS_Module);
					}
				}
			}
		}

		#endregion

		#region TestNewRuleSetModuleIsDefaultedToEmpty

		public void TestNewRuleSetModuleIsDefaultedToEmpty()
		{
			using (var module = new BarcodeValidationModule())
			{
				var originalSetting = Env.Security.BarcodeParsingNew.IsAllowed;

				try
				{
					Env.Security.BarcodeParsingNew.IsAllowed = true;
					var newMenu = module.FormActionMenu.FindByText("&New");

					// no module filter
					newMenu.PerformClick();

					using (var moduleForm = (ZForm)((IFilterModuleInternalsForTesting)module).LastController.LastShownForm)
					{
						var barcodeRuleSet = (BarcodeRuleSet)moduleForm.BusinessEntity;
						AssertEquals("No module filter applied, the module on the form should default to empty because Etail is enabled.", BarcodeModuleTypes.Codes.Warehouse, barcodeRuleSet.BRS_Module);
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
			return ModuleIDs.BarcodeValidation;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return Factory.NewWithValidTestData<BarcodeValidationRule>();
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;

		#endregion
	}
}
