using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryPrioritySetting))]
	sealed class HBLDeliveryPrioritySettingTest : RegistryBusinessObjectTemplateTestCase<HBLDeliveryPrioritySetting>
	{
		public void TestHBLDeliveryPriorityList()
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

			foreach (var keyValue in hblDeliveryModes)
			{
				var hblModesBasedOnContainer = keyValue.Value;

				for (var i = 0; i < hblModesBasedOnContainer.Count; i++)
				{
					AssertCodeDescriptionPairList(keyValue.Key, hblModesBasedOnContainer[i].Code, hblModesBasedOnContainer, x => x.HBLDeliveryModePriorityList);
				}
			}
		}

		void AssertCodeDescriptionPairList(string containerMode,
			string hblDeliveryMode,
			CodeDescriptionPairList expectedList,
			Func<HBLDeliveryPrioritySetting, CodeDescriptionPairList> actualListGetter)
		{
			var setting = GetNewBusinessObject(containerMode, hblDeliveryMode);

			var actual = actualListGetter(setting);
			AssertEquals(expectedList.Count, actual.Count);

			for (var i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actual[i].Code);
				AssertEquals(expectedList[i].Description, actual[i].Description);
			}
		}

		public void TestRunPreSaveValidation()
		{
			string originalValue;
			var setting = GetNewBusinessObject("BBK", "DOOR/DOOR");
			setting.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			// HBLDeliveryModePriority Validation
			originalValue = setting.HBLDeliveryModePriority;
			setting.HBLDeliveryModePriority = "CFS/CY";
			setting.ClearAllNotifications();
			AssertNoErrors("Precondition: HBLDeliveryModePriority should not have errors", setting);
			setting.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated HBLDeliveryModePriority", setting.HBLDeliveryModePriorityInfo); // CFS/CY is not valid for BBK
			setting.HBLDeliveryModePriority = originalValue;

			// CompositeKey Validation
			AssertNoRowErrors("Precondition: Setting should not have row errors", setting);

			var parentCollection = setting.ParentConfiguration.Settings;
			AssertEquals(1, parentCollection.Count);

			var newSetting = parentCollection.AddNew();
			newSetting.HBLDeliveryModePriority = setting.HBLDeliveryModePriority;

			setting.RunPreSaveValidation();
			AssertHasRowError("Precondition: Setting should have row error", newSetting, HBLDeliveryPrioritySetting.IdenticalSettingExists);

			newSetting.HBLDeliveryModePriority = "DOOR/PORT";
			setting.RunPreSaveValidation();
			AssertNoRowErrors("Precondition: Setting should not have row errors", setting);

			parentCollection.RemoveAndDelete(newSetting);
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
			=> GetNewBusinessObject("", "");

		HBLDeliveryPrioritySetting GetNewBusinessObject(string containerMode, string hblDeliveryMode)
		{
			var currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var parentConfiguration = new HBLDeliveryPriorityConfig(currentFallbackLevel, Factory);
			parentConfiguration.ContainerMode = containerMode;
			parentConfiguration.HBLDeliveryMode = hblDeliveryMode;

			var setting = parentConfiguration.Settings.AddNew();
			return setting;
		}

		protected override HBLDeliveryPrioritySetting GetBusinessObjectToClone()
			=> (HBLDeliveryPrioritySetting)GetNewBusinessObject();

		protected override HBLDeliveryPrioritySetting GetBusinessObjectToSerialise()
			=> (HBLDeliveryPrioritySetting)GetNewBusinessObject();

		#endregion
	}
}
