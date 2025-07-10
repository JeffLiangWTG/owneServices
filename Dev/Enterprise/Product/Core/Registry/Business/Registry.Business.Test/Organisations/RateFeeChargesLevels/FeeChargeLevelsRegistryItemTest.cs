using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FeeChargeLevelsRegistryItem))]
	sealed class FeeChargeLevelsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<FeeChargeLevelsSection>
	{
		protected override StronglyTypedRegistryItem<FeeChargeLevelsSection, FeeChargeLevelsSection> GetNewRegistryItem()
		{
			return new FeeChargeLevelsRegistryItem("", null, null, null, RegistryStorageFlags.System, new FeeChargeLevelsSection());
		}

		#region TestTranslatable

		public void TestTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = new FeeChargeLevelsRegistryItem("", null, null, null, RegistryStorageFlags.System, new FeeChargeLevelsSection());
				var levels = registryItem.Value;
				var type = levels.FeeChargeTypes.AddNew();
				type.Code = "TES";
				type.EnglishDescription = "Test Type";
				var level = type.FeeChargeLevels.AddNew();
				level.Code = "TSL";
				level.EnglishDescription = "Level Description";
				level.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.None;
				level.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.None;

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, levels);
				var key = ((ResourceString)registryItem.Value.FeeChargeTypes[0].Description).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "测试"));
				AssertEquals("测试", registryItem.Value.FeeChargeTypes[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var keyForLevel = ((ResourceString)registryItem.Value.FeeChargeTypes[0].FeeChargeLevels[0].Description).ResourceKey;
				mockChs.Put(keyForLevel, new ResourceStringData(key, "测试等级"));
				AssertEquals("测试等级", registryItem.Value.FeeChargeTypes[0].FeeChargeLevels[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		#endregion
	}
}
