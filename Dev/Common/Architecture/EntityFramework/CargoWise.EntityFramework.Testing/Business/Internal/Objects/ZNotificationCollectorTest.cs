using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZNotificationCollectorTest : TestCaseWithDummyForValidationTesting
	{
		public void TestColumnNameUsedInDescriptionByDefault()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, false, false);

			AssertEquals("PreCondition: Initial error count should be zero", 0, errors.Count());
			Dummy.Z0_DescriptionInfo.AddError("Test Error");
			AssertEquals("Failed to add test error", 1, errors.Count());
			string expMessage = String.Format("{0}: {1}", Dummy.Z0_DescriptionInfo.Name, "Test Error");
			AssertEquals("Column name should be used to prefix error by default", expMessage, errors.GetFirstMessage());
		}

		public void TestHumanReadableDescriptionName()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

			AssertEquals("PreCondition: Initial error count should be zero", 0, errors.Count());
			Dummy.Z0_DescriptionInfo.AddError("Test Error");
			AssertEquals("Failed to add test error", 1, errors.Count());
			string expMessage = String.Format("{0}: {1}", Dummy.Z0_DescriptionInfo.HumanReadableName, "Test Error");
			Assert(Dummy.Z0_DescriptionInfo.HasHumanReadableName);
			AssertEquals("HumanReadableName should be used to prefix error message", expMessage, errors.GetFirstMessage());
		}

		public void TestNoDescriptionName()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, false, false, ZNotificationCollector.PropertyDescriptionType.None);

			AssertEquals("PreCondition: Initial error count should be zero", 0, errors.Count());
			Dummy.Z0_DescriptionInfo.AddError("Test Error");
			AssertEquals("Failed to add test error", 1, errors.Count());
			AssertEquals("No prefix should be used in error message", "Test Error", errors.GetFirstMessage());
		}

		public void TestIgnoresKidsWhenToldTo()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, false, true, ZNotificationCollector.PropertyDescriptionType.ColumnName);

			Dummy.RegisterEditableChildObject(Dummy.Collection);
			Dummy.Collection.AddNew().AddRowError("noodle");
			AssertEquals("ignores notification in kid", 0, errors.Count());
		}

		public void TestIgnoreTopLevelObject()
		{
			DummyBusinessObject dummyMaster = Factory.New<DummyBusinessObject>();
			dummyMaster.AddRowError("Master Error");
			Dummy.RegisterEditableChildObject(dummyMaster);

			ZNotificationCollector errors = new ZNotificationCollector(Dummy, true, false, ZNotificationCollector.PropertyDescriptionType.None);
			AssertEquals("All errors by default", 1, errors.Count());
			AssertEquals("DummyBizo: Master Error", errors.GetFirstMessage());

			dummyMaster.IsTopLevel = true;

			errors = new ZNotificationCollector(Dummy, true, false, ZNotificationCollector.PropertyDescriptionType.None);
			AssertEquals("No errors found on Top Level object", 0, errors.Count());

			errors = new ZNotificationCollector(dummyMaster, true, false, ZNotificationCollector.PropertyDescriptionType.None);
			AssertEquals("Errors on Top Level object should be found when it is root", 1, errors.Count());
			AssertEquals("DummyBizo: Master Error", errors.GetFirstMessage());
		}

		public void TestFindsErrors()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, false, true, ZNotificationCollector.PropertyDescriptionType.ColumnName);

			AssertEquals("initial state", 0, errors.Count());

			Dummy.AddRowError("blah");
			Dummy.Z0_AnotherDateInfo.AddError("teapot");
			Dummy.Z0_DecimalInfo.AddError("teapot");
			AssertEquals("Errors", 3, errors.Count());
		}

		public void TestFindsErrorsInKids()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, true, true, ZNotificationCollector.PropertyDescriptionType.ColumnName);
			Dummy.AddRowError("yeah");

			Dummy.RegisterEditableChildObject(Dummy.Collection);
			DummyChildBusinessObject child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_ByteInfo.AddError("noodle");
				child.AddRowError("angle");
				AssertEquals("finds notifications in kids", 3, errors.Count());
			}
		}

		public void TestIEnumerable()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, true, true, ZNotificationCollector.PropertyDescriptionType.ColumnName);
			Dummy.AddRowError("yeah");

			Dummy.RegisterEditableChildObject(Dummy.Collection);
			DummyChildBusinessObject child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_ByteInfo.AddError("noodle");
				child.AddRowError("angle");

				foreach (INotification error in errors)
				{
					Assert("Valid error",
						error.Message.IndexOf("yeah") != -1 || error.Message.IndexOf("noodle") != -1 || error.Message.IndexOf("angle") != -1);
				}
			}
		}

		public void TestToString()
		{
			ZNotificationCollector errors = new ZNotificationCollector(Dummy, true, true, ZNotificationCollector.PropertyDescriptionType.ColumnName);
			Dummy.AddRowError("yeah");

			Dummy.RegisterEditableChildObject(Dummy.Collection);
			DummyChildBusinessObject child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_ByteInfo.AddError("noodle");
				child.AddRowError("angle");

				string error = errors.ToUniqueMessageListString();
				Assert("All errors",
					error.IndexOf("yeah") != -1 && error.IndexOf("noodle") != -1 && error.IndexOf("angle") != -1);
			}
		}

		public void TestIncludesNotificationType()
		{
			ZNotificationCollector errorsWithType = new ZNotificationCollector(Dummy, true, true, ZNotificationCollector.PropertyDescriptionType.ColumnName);
			Dummy.AddRowError("yeah");
			Dummy.Z0_DescriptionInfo.AddError("teapot");

			Assert("Has type", errorsWithType.GetUniqueMessageList()[0].IndexOf("Error") != -1);
			Assert("Has type", errorsWithType.GetUniqueMessageList()[1].IndexOf("Error") != -1);
		}

		#region ShouldIncludeNotificationsFromXXX

		public void TestShouldIncludeNotificationsFromObject()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_DateInfo.AddError("Parent");
			}
			using (Child.SuspendValidationTesting())
			{
				Child.Z0_DateInfo.AddError("Child");
			}
			Assert("Does include error on parent", Collector.ToUniqueMessageListString().Contains("Parent"));
			Assert("Does include error on child", Collector.ToUniqueMessageListString().Contains("Child"));
			Collector.IgnorePK = Dummy.PK;
			Assert("Does not include error on parent", !Collector.ToUniqueMessageListString().Contains("Parent"));
			Assert("Does include error on child", !Collector.ToUniqueMessageListString().Contains("Child"));
			Collector.IgnorePK = Child.PK;
			Assert("Does include error on parent", Collector.ToUniqueMessageListString().Contains("Parent"));
			Assert("Does not include error on child", !Collector.ToUniqueMessageListString().Contains("Child"));
		}

		public void TestShouldIncludeNotificationsFromInfo()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_DateInfo.AddError("Date");
				Dummy.Z0_VarCharMaxInfo.AddError("Text");
			}
			Assert("Does include error on date", Collector.ToUniqueMessageListString().Contains(Dummy.Z0_DateInfo.Name));
			Assert("Does include error on text", Collector.ToUniqueMessageListString().Contains(Dummy.Z0_VarCharMaxInfo.Name));
			Collector.IgnorePropertyName = Dummy.Z0_DateInfo.Name;
			Assert("Does not include error on date", !Collector.ToUniqueMessageListString().Contains(Dummy.Z0_DateInfo.Name));
			Assert("Does include error on text", Collector.ToUniqueMessageListString().Contains(Dummy.Z0_VarCharMaxInfo.Name));
		}

		public void TestShouldIncludeNotificationsFromWrappedInfo()
		{
			ZWrappedPropertyInfo wrappedProperty = Wrapper.ZPropertyInfoHash["WrappedZ0_Description"] as ZWrappedPropertyInfo;
			AssertNotNull("Wrapped Property", wrappedProperty);
			AssertEquals("IsLoaded", false, wrappedProperty.IsLoaded);

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_DescriptionInfo.AddError("Wrapped");
			}
			AssertEquals("IsLoaded", false, wrappedProperty.IsLoaded);

			Collector = new DummyNotificationCollector(Wrapper);
			Assert("Does not include error on wrapped", !Collector.ToUniqueMessageListString().Contains("Wrapped"));
			AssertNotNull(wrappedProperty.InnerInfo);
			AssertEquals("IsLoaded", true, wrappedProperty.IsLoaded);
			Assert("Does include error on wrapped", Collector.ToUniqueMessageListString().Contains("Wrapped"));
		}

		#endregion

		public void TestNotificationsFromObjectContainsTheObjectHumanReadableName()
		{
			Dummy.HumanReadableNameForTest = "Dummy Human Readable Name";
			Dummy.AddRowError("Row Error");
			AssertContains("Dummy Human Readable Name: Row Error", Collector.ToUniqueMessageListString());
		}

		public void TestWrappedPropertyNotifications()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_DescriptionInfo.AddError("Description Error");
				dummy.Z0_CodeInfo.AddError("Code Error");
			}

			DummyBusinessObjectWrapperWithInfo wrapper = new DummyBusinessObjectWrapperWithInfo(dummy);
			AssertNotNull(((ZWrappedPropertyInfo)wrapper.WrappedZ0_DescriptionInfo).InnerInfo);
			AssertNotNull(((ZWrappedPropertyInfo)wrapper.WrappedZ0_CodeInfo).InnerInfo);

			DummyNotificationCollector collector = new DummyNotificationCollector(wrapper);

			Assert("Should contain WrappedZ0_Description notifications", collector.ToUniqueMessageListString().Contains("Description Error"));
			Assert("Should not contain Wrapped+Z0_code notifications", !collector.ToUniqueMessageListString().Contains("Code Error"));
		}

		public void TestEnumeratingCycledChildren()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();

			dummy1.Z0_Code = "D01";
			dummy2.Z0_Code = "D02";
			dummy3.Z0_Code = "D03";

			var dummyCollection = new DummyActiveCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, dummy3.Z0_Code));

			Factory.SuspendValidation(); // To prevent OnNotificationChanged on ActiveBusinessObjectCollection

			using (dummy1.SuspendSettingHasChanges()) // To prevent HandleUpdateOfChildHasChanges calling OnHasChangesChanged
			using (dummy2.SuspendSettingHasChanges())
			using (dummy3.SuspendSettingHasChanges())
			{
				dummy1.Z0_DescriptionInfo.AddError("Error 1");
				dummy2.Z0_DescriptionInfo.AddError("Error 2");
				dummy3.Z0_DescriptionInfo.AddError("Error 3");
				ErrorReporter.Clear(); // Remove error about setting notification outside of validation method

				dummy1.RegisterEditableChildObject(dummy2);
				dummy2.RegisterEditableChildObject(dummyCollection);
				dummy3.RegisterEditableChildObject(dummy1);
			}

			AssertEquals(1, dummyCollection.Count); // Pre-load collection

			AssertEquals(3, new ZNotificationCollector(dummy1, true, true).Count());

			AssertEquals("Cycle references found on the businessObject(Type: DummyBusinessObject), please check and fix.", ErrorReporter.LastMessageReported);

			AssertEquals(3, new ZNotificationCollector(dummy1, true, true).Count()); // Enumerate again to ensure the hashtable is cleared correctly
			ErrorReporter.Clear();
		}

		class DummyActiveCollection : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public DummyActiveCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter) { }
		}

		#region Implementation

		DummyNotificationCollector Collector;
		DummyChildBusinessObject Child;
		DummyBusinessObjectWrapperWithInfo Wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			Dummy.RegisterEditableChildObject(Dummy.Collection);
			Child = Dummy.Collection.AddNew();
			Collector = new DummyNotificationCollector(Dummy);
			Wrapper = new DummyBusinessObjectWrapperWithInfo(Dummy);
		}

		#endregion
	}
}
