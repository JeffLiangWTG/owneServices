using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(SystemProductRegistryItem))]
	internal sealed class SystemProductRegistryItemTest : StronglyTypedRegistryItemTestCase<SystemProductCollection>
	{
		public void TestTranslatable()
		{
			var registryItem = new SystemProductRegistryItem("", null, null, null, new SystemProductRegistryEditorInfo(ModuleListType.MenuSection, true), RegistryStorageFlags.System, new SystemProductCollection());
			var value = registryItem.Value;
			var systemProduct = value.AddNew();
			systemProduct.Code = "P1";
			systemProduct.Description = "Desc 1";
			var moduleMapping = systemProduct.ModuleMappings.AddNew();
			moduleMapping.ModuleCode = "M1";
			moduleMapping.ModuleDescriptionMultilingual = (NoResString)"Type 1";
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				using (var mockChs = Res.UseMockData())
				{
					AssertType(typeof(ResourceString), registryItem.Value[0].ModuleMappings[0].ModuleDescriptionMultilingual);
					var key = ((ResourceString)registryItem.Value[0].ModuleMappings[0].ModuleDescriptionMultilingual).ResourceKey;
					mockChs.Put(key, new ResourceStringData(key, "类型1"));
					AssertEquals("类型1", registryItem.Value[0].ModuleMappings[0].ModuleDescriptionMultilingual);
				}
		}

#region Implementation
		protected override StronglyTypedRegistryItem<SystemProductCollection, SystemProductCollection> GetNewRegistryItem()
		{
			return new SystemProductRegistryItem("", null, null, null, new SystemProductRegistryEditorInfo(ModuleListType.MenuSection, true), RegistryStorageFlags.System, new SystemProductCollection());
		}
#endregion
	}
}
