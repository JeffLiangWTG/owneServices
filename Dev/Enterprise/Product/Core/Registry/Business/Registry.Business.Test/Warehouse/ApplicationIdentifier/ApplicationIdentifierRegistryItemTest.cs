using System;
using System.Collections.Generic;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ApplicationIdentifierRegistryItem))]
	sealed class ApplicationIdentifierRegistryItemTest : StronglyTypedRegistryItemTestCase<ApplicationIdentifierCollection>
	{
		protected override StronglyTypedRegistryItem<ApplicationIdentifierCollection, ApplicationIdentifierCollection> GetNewRegistryItem()
		{
			return new ApplicationIdentifierRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new ApplicationIdentifierCollection());
		}

		#region TestTranslatable

		public void TestTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = new ApplicationIdentifierRegistryItem("", null, null, null, RegistryStorageFlags.System, new ApplicationIdentifierCollection());
				var aiCollection = registryItem.Value;
				AddNewApplicationIdentifier(aiCollection, "T123", "Test Full Title", "Test Data Title");

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aiCollection);
				var keyFullTitle = ((ResourceString)registryItem.Value[0].FullTitle).ResourceKey;
				mockChs.Put(keyFullTitle, new ResourceStringData(keyFullTitle, "测试全称"));
				AssertEquals("测试全称", registryItem.Value[0].FullTitle.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var keyDataTitle = ((ResourceString)registryItem.Value[0].DataTitle).ResourceKey;
				mockChs.Put(keyDataTitle, new ResourceStringData(keyDataTitle, "数据名称"));
				AssertEquals("数据名称", registryItem.Value[0].DataTitle.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var captions = new List<string>(registryItem.GetCaptions(aiCollection));
				int numOfCaptions = captions.Count;
				AddNewApplicationIdentifier(aiCollection, "T321", "Full Title", " ");
				captions = new List<string>(registryItem.GetCaptions(aiCollection));
				AssertEquals(numOfCaptions + 1, captions.Count);
			}
		}

		void AddNewApplicationIdentifier(ApplicationIdentifierCollection aiCollection, string id, string englishFullTitle, string englishDataTitle)
		{
			var identifier = aiCollection.AddNew();
			identifier.EnglishFullTitle = englishFullTitle;
			identifier.EnglishDataTitle = englishDataTitle;
			identifier.ApplicationID = id;
			identifier.DataType = "D";
		}

		#endregion

		#region TestGetCaptions_NoDuplicates

		public void TestGetCaptions_NoDuplicates()
		{
			var registryItem = new ApplicationIdentifierRegistryItem("", null, null, null, RegistryStorageFlags.System, new ApplicationIdentifierCollection());
			var aiCollection = registryItem.Value;
			AddNewApplicationIdentifier(aiCollection, "T123", "Test Full Title", "Test Data Title");
			AddNewApplicationIdentifier(aiCollection, "T456", "Test Full Title", "Test Data Title");
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aiCollection);

			var captions = registryItem.GetCaptions(aiCollection);
			AssertEquals(2, captions.Count());
			AssertContainsExactElementsInAnyOrder(new[] { "Test Full Title", "Test Data Title" }, captions);
		}

		#endregion
	}
}
