using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomsReferenceNumberTypesRegistryItem))]
	sealed class CustomsReferenceNumbersRegistryItemTest : StronglyTypedRegistryItemTestCase<CustomsReferenceNumberTypeCollection>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<CustomsReferenceNumberTypeCollection, CustomsReferenceNumberTypeCollection> GetNewRegistryItem()
		{
			return new CustomsReferenceNumberTypesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new CustomsReferenceNumberTypeCollection());
		}

		#endregion

		#region TestRegistryOptionsConstructor

		public void TestRegistryOptionsConstructor()
		{
			var registryItem = new CustomsReferenceNumberTypesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new CustomsReferenceNumberTypeCollection(), RegistryOptions.IsHidden);
			Assert(registryItem.HasOption(RegistryOptions.IsHidden));
		}

		#endregion

		#region TestAddsMissingDefaults

		public void TestAddsMissingDefaults()
		{
			var list = new CustomsReferenceNumberTypeCollection();
			list.Add("AAA", (NoResString)"A Desc");
			list.Add("BBB", (NoResString)"B Desc");

			var defaults = new CustomsReferenceNumberTypeCollection();
			defaults.AddSystemDefined("AAA", (NoResString)"A Desc", true);
			defaults.AddSystemDefined("CCC", (NoResString)"C Desc", true);

			var item = new CustomsReferenceNumberTypesRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, defaults);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertContainsExactElementsInAnyOrder("Should have added the missing default 'CCC' type",
				new[] { "AAA", "BBB", "CCC" },
				item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Cast<CustomsReferenceNumberType>()
				.Select(cusRef => cusRef.Code.ToString()));
		}

		public void TestAddsMissingDefaults_OnlyAddsIfReadOnly()
		{
			// Clients are free to edit/modify non system defined types as per old functionality
			var list = new CustomsReferenceNumberTypeCollection();
			list.Add("AAA", (NoResString)"A Desc");
			list.Add("BBB", (NoResString)"B Desc");

			var defaults = new CustomsReferenceNumberTypeCollection();
			defaults.Add("AAA", (NoResString)"A Desc", true);
			defaults.Add("CCC", (NoResString)"C Desc", true);

			var item = new CustomsReferenceNumberTypesRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, defaults);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertContainsExactElementsInAnyOrder("Should not have added the missing default 'CCC' type",
				new[] { "AAA", "BBB" },
				item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Cast<CustomsReferenceNumberType>()
				.Select(cusRef => cusRef.Code.ToString()));
		}

		#endregion

		#region TestAddsMissingDefaults_PartiallyCorrectRecords

		public void TestAddsMissingDefaults_PartiallyCorrectRecords()
		{
			var defaults = new CustomsReferenceNumberTypeCollection();
			defaults.AddSystemDefined("AAA", (NoResString)"A Desc");
			defaults.AddSystemDefined("BBB", (NoResString)"B Desc");

			var deserialisedList = new CustomsReferenceNumberTypeCollection();
			deserialisedList.Add("AAA", (NoResString)"A Desc22", true); // SystemDefined is not serialised
			deserialisedList.Add("BBB", (NoResString)"B Desc22", false); // SystemDefined is not serialised

			var item = new CustomsReferenceNumberTypesRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, defaults);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deserialisedList);

			var aaa = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Cast<CustomsReferenceNumberType>().Single(c => c.Code == "AAA");
			var bbb = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Cast<CustomsReferenceNumberType>().Single(c => c.Code == "BBB");
			AssertEquals("Description should be same as system defined.", "A Desc", aaa.Description);
			AssertEquals("Description should be same as system defined.", "B Desc", bbb.Description);
			AssertEquals("IsUnique should be same as system defined.", true, aaa.IsUnique);
			AssertEquals("IsUnique should be same as system defined.", true, bbb.IsUnique);
			AssertEquals("Should set system defined.", true, aaa.SystemDefined);
			AssertEquals("Should set system defined.", true, bbb.SystemDefined);
		}

		#endregion
	}
}
