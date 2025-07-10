using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceRollupAndGroupDescriptionRegistryItem))]
	class InvoiceRollupAndGroupDescriptionRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<InvoiceRollupAndGroupDescriptionCollection>
	{
		protected override StronglyTypedRegistryItem<InvoiceRollupAndGroupDescriptionCollection, InvoiceRollupAndGroupDescriptionCollection> GetNewRegistryItem()
		{
			return new InvoiceRollupAndGroupDescriptionRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceRollupAndGroupDescriptionCollection());
		}

		#region TestTranslatable

		public void TestTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = new InvoiceRollupAndGroupDescriptionRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceRollupAndGroupDescriptionCollection());
				var descriptions = registryItem.Value;
				var type = descriptions.AddNew();
				type.Style = "TES";
				type.Group = "TEST";
				type.EnglishDescription = "Test Type";

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, descriptions);
				var key = ((ResourceString)registryItem.Value[0].Description).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "测试"));
				AssertEquals("测试", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		#endregion

		public void TestGetDescription()
		{
			var registryItem = new InvoiceRollupAndGroupDescriptionRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceRollupAndGroupDescriptionCollection());
			var descriptions = new InvoiceRollupAndGroupDescriptionCollection();
			var type1 = descriptions.AddNew();
			type1.Style = "TES";
			type1.Group = "TEST";
			type1.EnglishDescription = "Test Type 1";
			var type2 = descriptions.AddNew();
			type2.Style = "TES";
			type2.Group = "DIFFERENT";
			type2.EnglishDescription = "Test Type 2";
			var type3 = descriptions.AddNew();
			type3.Style = "ABC";
			type3.Group = "TEST";
			type3.EnglishDescription = "Test Type 3";
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, descriptions);
			AssertEquals("Test Type 1", registryItem.GetDescription("TES", "TEST"));
			AssertEquals("Test Type 2", registryItem.GetDescription("TES", "DIFFERENT"));
			AssertEquals("Test Type 3", registryItem.GetDescription("ABC", "TEST"));
			AssertNull(registryItem.GetDescription("NUN", "Not a thing"));
		}
	}
}
