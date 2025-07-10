using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.AutomaticProcessRegistryBusinessObject;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutomaticProcessRegistryItem))]
	sealed class AutomaticProcessRegistryItemTest : StronglyTypedRegistryItemTestCase<AutomaticProcessRegistryBusinessObject>
	{
		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestCasting()
		{
			base.TestCasting();
		}

		public void TestUpdateLastRun()
		{
			ZDateTime testDate = new DateTime(2006, 1, 1, 1, 1, 50);
			AutomaticProcessRegistryItem itemAtSystemLevel = new AutomaticProcessRegistryItem("", null, null, null, RegistryStorageFlags.System);
			AutomaticProcessRegistryBusinessObject bizObj = ValidValue;
			itemAtSystemLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bizObj);
			AssertEquals("At system level", testDate, itemAtSystemLevel.Value.LastRunDateTime);
			itemAtSystemLevel.UpdateLastRun(testDate.AddDays(1));
			AssertEquals("At system level", testDate.AddDays(1), itemAtSystemLevel.Value.LastRunDateTime);
		}

		public void TestDeserialiseCore_ShouldCheckIntervalNotLessThanMinimumValue()
		{
			AssertShouldCheckIntervalNotLessThanMinimumValueAfterDeserialise(true);

			AssertShouldCheckIntervalNotLessThanMinimumValueAfterDeserialise(false);

			void AssertShouldCheckIntervalNotLessThanMinimumValueAfterDeserialise(bool shouldCheck)
			{
				var item = new AutomaticProcessRegistryItem("", null, null, null, RegistryStorageFlags.System, shouldCheck);
				Assert("Precondition", !ValidValue.ShouldCheckIntervalNotLessThanMinimumValue);

				var serialisedValue = item.DataType.Serialise(ValidValue);
				var bizObj = (AutomaticProcessRegistryBusinessObject)item.DataType.Deserialise(serialisedValue);

				AssertEquals(shouldCheck, bizObj.ShouldCheckIntervalNotLessThanMinimumValue);
			}
		}

		public void TestDeserialiseCore_ShouldMaintainMinimumInterval()
		{
			var minimumIntervalType = IntervalTypes.Hours;
			var minimumInterval = 5;
			var item = new AutomaticProcessRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, minimumIntervalType, minimumInterval);
			Assert("Precondition", !ValidValue.ShouldCheckIntervalNotLessThanMinimumValue);

			var serialisedValue = item.DataType.Serialise(ValidValue);
			var bizObj = (AutomaticProcessRegistryBusinessObject)item.DataType.Deserialise(serialisedValue);

			Assert("Should check minimum", bizObj.ShouldCheckIntervalNotLessThanMinimumValue);
			AssertEquals("Should maintain minimum interval type", minimumIntervalType, bizObj.MinimumIntervalType);
			AssertEquals("Should maintain minimum interval", minimumInterval, bizObj.MinimumInterval);
		}

		public void TestCloneValue_ShouldCheckIntervalNotLessThanMinimumValue()
		{
			AssertShouldCheckIntervalNotLessThanMinimumValueAfterClone(true);

			AssertShouldCheckIntervalNotLessThanMinimumValueAfterClone(false);

			void AssertShouldCheckIntervalNotLessThanMinimumValueAfterClone(bool shouldCheck)
			{
				var item = new AutomaticProcessRegistryItem("", null, null, null, RegistryStorageFlags.System, shouldCheck);
				Assert("Precondition", !ValidValue.ShouldCheckIntervalNotLessThanMinimumValue);

				var bizObj = (AutomaticProcessRegistryBusinessObject)item.DataType.CloneValue(ValidValue);
				AssertEquals(shouldCheck, bizObj.ShouldCheckIntervalNotLessThanMinimumValue);
			}
		}

		public void TestCloneValue_ShouldMaintainMinimumInterval()
		{
			var minimumIntervalType = IntervalTypes.Hours;
			var minimumInterval = 5;
			var item = new AutomaticProcessRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, minimumIntervalType, minimumInterval);
			Assert("Precondition", !ValidValue.ShouldCheckIntervalNotLessThanMinimumValue);

			var bizObj = (AutomaticProcessRegistryBusinessObject)item.DataType.CloneValue(ValidValue);
			Assert("Should check minimum", bizObj.ShouldCheckIntervalNotLessThanMinimumValue);
			AssertEquals("Should maintain minimum interval type", minimumIntervalType, bizObj.MinimumIntervalType);
			AssertEquals("Should maintain minimum interval", minimumInterval, bizObj.MinimumInterval);
		}

		protected override AutomaticProcessRegistryBusinessObject ValidValue
		{
			get
			{
				AutomaticProcessRegistryBusinessObject bizObj = new AutomaticProcessRegistryBusinessObject();
				bizObj.Interval = 1;
				bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
				bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);

				return bizObj;
			}
		}

		protected override StronglyTypedRegistryItem<AutomaticProcessRegistryBusinessObject, AutomaticProcessRegistryBusinessObject> GetNewRegistryItem()
		{
			return new AutomaticProcessRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		public static AutomaticProcessRegistryBusinessObject GetNewValueWithValidDataForTesting(BusinessObjectFactory factory)
		{
			AutomaticProcessRegistryBusinessObject newValue = new AutomaticProcessRegistryBusinessObject(factory);
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			factory.Save();

			return newValue;
		}
	}
}
