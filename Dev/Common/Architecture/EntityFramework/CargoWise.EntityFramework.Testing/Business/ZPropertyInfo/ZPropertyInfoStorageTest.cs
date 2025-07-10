using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoStorageTest : TestCase
	{
		public void TestValueChangedIsActive()
		{
			ZPropertyInfoStorage storage = new ZPropertyInfoStorage();
			AssertEquals(false, storage.ValueChangedIsActive);
			object x = storage.ValueChangedDictionary;
			AssertEquals(true, storage.ValueChangedIsActive);
		}

		public void TestGetAndSetHumanReadableName()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessObject>();
			var storage = new ZPropertyInfoStorage();
			AssertEquals(ZString.Empty, storage.GetHumanReadableName(bizO.Z0_GuidInfo).Value);
			AssertEquals(false, storage.GetHumanReadableName(bizO.Z0_GuidInfo).IsGuiCaption);

			storage.SetHumanReadableName(bizO.Z0_GuidInfo, "test", true);
			AssertEquals("test", storage.GetHumanReadableName(bizO.Z0_GuidInfo).Value);
			AssertEquals(true, storage.GetHumanReadableName(bizO.Z0_GuidInfo).IsGuiCaption);
			AssertNotEquals("test", storage.GetHumanReadableName(bizO.Z0_AnotherDateInfo).Value);

			storage.SetHumanReadableName(bizO.Z0_AnotherDateInfo, "test1");
			AssertEquals("test1", storage.GetHumanReadableName(bizO.Z0_AnotherDateInfo).Value);
			AssertEquals(false, storage.GetHumanReadableName(bizO.Z0_AnotherDateInfo).IsGuiCaption);

			storage.SetHumanReadableName(bizO.Z0_GuidInfo, ZString.Empty);
			AssertEquals("should remove empty values from dictionary", false, storage.HumanReadableNameDictionary.ContainsKey(bizO.Z0_GuidInfo));
		}

		public void TestValueChangedDictionary()
		{
			ZPropertyInfoStorage storage = new ZPropertyInfoStorage();
			AssertEquals(storage.ValueChangedDictionary, storage.ValueChangedDictionary);
		}

		public void TestAdditionalValidationDictionaryReturnsSameInstance()
		{
			ZPropertyInfoStorage storage = new ZPropertyInfoStorage();
			AssertEquals(storage.AdditionalValidationDictionary, storage.AdditionalValidationDictionary);
		}
	}
}
