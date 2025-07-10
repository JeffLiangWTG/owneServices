using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoRatingRequiredFieldsRegistryItem))]
	sealed class AutoRatingRequiredFieldsRegistryItemTest : StronglyTypedRegistryItemTestCase<IAutoRatingRequiredFields, AutoRatingRequiredFields>
	{
		public void TestEditorInfo()
		{
			AutoRatingRequiredFieldsRegistryItem registryItem = new AutoRatingRequiredFieldsRegistryItem("", "", null, null, null, RegistryStorageFlags.System, true);
			AssertEquals("EditorInfo.ShowIncoterm", true, ((AutoRatingRequiredFieldsRegistryEditorInfo)registryItem.EditorInfo).ShowIncoterm);

			registryItem = new AutoRatingRequiredFieldsRegistryItem("", "", null, null, null, RegistryStorageFlags.System, false);
			AssertEquals("EditorInfo.ShowIncoterm", false, ((AutoRatingRequiredFieldsRegistryEditorInfo)registryItem.EditorInfo).ShowIncoterm);
		}

		public void TestGetValue()
		{
			AutoRatingRequiredFieldsRegistryItem registryItem = new AutoRatingRequiredFieldsRegistryItem("Test!", "", null, null, null, RegistryStorageFlags.System, true);

			AutoRatingRequiredFields requiredFields = new AutoRatingRequiredFields();
			requiredFields.RequireCommodityCode = true;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, requiredFields);

			IAutoRatingRequiredFields obtainedValue = registryItem.TypedValue;
			AssertEquals("RatingHeaderType", "Test!", obtainedValue.RatingHeaderType);
			AssertEquals("RequireServiceLevel", false, obtainedValue.RequireServiceLevel);
			AssertEquals("RequireCommodityCode", true, obtainedValue.RequireCommodityCode);
			AssertEquals("RequireFrequency", false, obtainedValue.RequireFrequency);
			AssertEquals("RequireTransitTime", false, obtainedValue.RequireTransitTime);
		}

		protected override StronglyTypedRegistryItem<IAutoRatingRequiredFields, AutoRatingRequiredFields> GetNewRegistryItem()
		{
			return new AutoRatingRequiredFieldsRegistryItem("", "", null, null, null, RegistryStorageFlags.System, false);
		}
	}
}
