using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryPriorityConfig))]
	sealed class HBLDeliveryPriorityConfigTest : RegistryBusinessObjectTemplateTestCase<HBLDeliveryPriorityConfig>
	{
		public void TestSettings()
		{
			var setting = Configuration.Settings.AddNew();

			AssertEquals("Settings.ParentLocationsChargesGroup", BizObj, BizObj.Settings.ParentConfiguration);
			AssertEquals("Settings.Factory", BizObj.Factory, BizObj.Settings.Factory);
			AssertEquals("Settings.CurrentFallbackLevel", BizObj.CurrentFallbackLevel, setting.CurrentFallbackLevel);
		}

		public void TestContainerModeList()
		{
			(string code, string description)[] expected =
			{
				("BBK", "Break Bulk"),
				("BCN", "Buyer's Consolidation"),
				("BLK", "Bulk"),
				("FCL", "Full Container Load"),
				("LCL", "Less Container Load"),
				("LQD", "Liquid"),
				("LSE", "Loose"),
				("ROR", "Roll On/Roll Off"),
				("ULD", "Unit Load Device")
			};

			var configuration = (HBLDeliveryPriorityConfig)GetNewBusinessObject();

			AssertEquals(expected.Length, configuration.ContainerModeList.Count);
			AssertContainsExactElementsInAnyOrder(expected, configuration.ContainerModeList.ToArray().Select(x => (code: x.Code, description: x.Description)));
		}

		public void TestHBLDeliveryModeList()
		{
			var hblDeliveryModes = new Dictionary<string, CodeDescriptionPairList>
			{
				{ ContainerModes.FCL, FreightDataRegistry.Instance.HBLDeliveryMode_FCL.Value.HBLDeliveryModesList },
				{ ContainerModes.LCL, FreightDataRegistry.Instance.HBLDeliveryMode_LCL.Value.HBLDeliveryModesList },
				{ ContainerModes.BuyersConsol, FreightDataRegistry.Instance.HBLDeliveryMode_BCN.Value.HBLDeliveryModesList },
				{ ContainerModes.Loose, FreightDataRegistry.Instance.HBLDeliveryMode_LSE.Value.HBLDeliveryModesList },
				{ ContainerModes.ULD, FreightDataRegistry.Instance.HBLDeliveryMode_ULD.Value.HBLDeliveryModesList },
				{ ContainerModes.RollOnRollOff, FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value.HBLDeliveryModesList },
				{ ContainerModes.BreakBulk, FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value.HBLDeliveryModesList },
				{ ContainerModes.Bulk, FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value.HBLDeliveryModesList },
				{ ContainerModes.Liquid, FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value.HBLDeliveryModesList }
			};

			var configuration = (HBLDeliveryPriorityConfig)GetNewBusinessObject();

			foreach (var keyValue in hblDeliveryModes)
			{
				configuration.ContainerMode = keyValue.Key;

				var hblModesBasedOnContainer = keyValue.Value;

				AssertEquals(hblModesBasedOnContainer.Count, configuration.HBLDeliveryModeList.Count);
				AssertContainsExactElementsInAnyOrder(hblModesBasedOnContainer, configuration.HBLDeliveryModeList);
			}
		}

		public void TestRunPreSaveValidation()
		{
			string originalValue;
			var configuration = (HBLDeliveryPriorityConfig)GetNewBusinessObject();

			// ContainerMode Validation
			originalValue = configuration.ContainerMode;
			configuration.ContainerMode = "XXX";
			configuration.ClearAllNotifications();
			AssertNoErrors("Precondition: ContainerMode should not have errors", configuration);
			configuration.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated ContainerMode", configuration.ContainerModeInfo);
			configuration.ContainerMode = originalValue;

			// HBLDeliveryMode Validation
			originalValue = configuration.HBLDeliveryMode;
			configuration.HBLDeliveryMode = "XXX";
			configuration.ClearAllNotifications();
			AssertNoErrors("Precondition: HBLDeliveryMode should not have errors", configuration);
			configuration.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated HBLDeliveryMode", configuration.HBLDeliveryModeInfo);
			configuration.HBLDeliveryMode = originalValue;
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var configuration = new HBLDeliveryPriorityConfig(currentFallbackLevel, Factory);
			configuration.ContainerMode = "FCL";
			configuration.HBLDeliveryMode = "DOOR/DOOR";

			return configuration;
		}

		protected override HBLDeliveryPriorityConfig GetBusinessObjectToClone()
			=> (HBLDeliveryPriorityConfig)GetNewBusinessObject();

		protected override HBLDeliveryPriorityConfig GetBusinessObjectToSerialise()
			=> (HBLDeliveryPriorityConfig)GetNewBusinessObject();

		#endregion

		HBLDeliveryPriorityConfig Configuration => BizObj;
	}
}
