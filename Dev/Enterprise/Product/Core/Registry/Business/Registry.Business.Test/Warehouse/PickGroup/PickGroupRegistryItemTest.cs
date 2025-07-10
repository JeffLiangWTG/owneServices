using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PickGroupRegistryItem))]
	sealed class PickGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<PickGroupCollection>
	{
		protected override StronglyTypedRegistryItem<PickGroupCollection, PickGroupCollection> GetNewRegistryItem()
		{
			return new PickGroupRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		public void TestTranslatable()
		{
			var registryItem = new PickGroupRegistryItem("", null, null, null, RegistryStorageFlags.System);
			var value = registryItem.Value;
			var pickGroup = value.AddNew();
			pickGroup.EnglishDescription = "Group 1";
			pickGroup.PickSequence = 1;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockGrm = Res.UseMockData())
			{
				AssertType(typeof(ResourceString), registryItem.Value[0].Description);
				var key = ((ResourceString)registryItem.Value[0].Description).ResourceKey;
				mockGrm.Put(key, new ResourceStringData(key, "1 puorg"));
				AssertEquals("1 puorg", registryItem.Value[0].Description);
			}
		}
	}
}
