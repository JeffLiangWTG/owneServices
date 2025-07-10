using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(UOMPackTypeRegistryItem))]
	sealed class UOMPackTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<UOMPackTypeCollection>
	{
		#region TestTranslatable

		public void TestTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = new UOMPackTypeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new UOMPackTypeCollection());
				AssertEquals("Precondition", true, registryItem.IsTranslatable);
				var collection = registryItem.Value;
				var packType = collection.AddNew();
				packType.Code = "CAS";
				packType.EnglishDescription = "Case";
				packType.NumberOfLabels = 1;
				AssertEquals(UOMPackType.MaxDescriptionLength, registryItem.MaxLength);
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				AssertType(typeof(ResourceString), registryItem.Value[0].Description);
				var keyDescription = ((ResourceString)registryItem.Value[0].Description).ResourceKey;
				mockChs.Put(keyDescription, new ResourceStringData(keyDescription, "废话"));
				AssertEquals("废话", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		#endregion

		#region Implmentation

		protected override StronglyTypedRegistryItem<UOMPackTypeCollection, UOMPackTypeCollection> GetNewRegistryItem()
		{
			return new UOMPackTypeRegistryItem("UOMPackType", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new UOMPackTypeCollection());
		}

		#endregion
	}
}
