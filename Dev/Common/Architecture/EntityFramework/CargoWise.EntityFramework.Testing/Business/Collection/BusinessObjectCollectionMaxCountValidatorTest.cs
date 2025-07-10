using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCollectionMaxCountValidatorTest : TestCaseWithFactory
	{
		public void TestValidate_WarningsAtHalfway()
		{
			Validator.MaxCount = 4;
			Validator.WarnAtHalfway = true;

			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			AssertNotifications(collection[0], 0, 0);
			AssertNotifications(collection[1], 0, 0);
			AssertNotifications(collection[2], 1, 0);
			AssertNotifications(collection[3], 1, 0);
			AssertNotifications(collection[4], 0, 1);
		}

		void AssertNotifications(BusinessObject bizo, int expectedWarnings, int expectedErrors)
		{
			AssertEquals(expectedWarnings, bizo.RowWarnings.Count());
			AssertEquals(expectedErrors, bizo.RowErrors.Count());
		}

		public void TestPluralForBusinessObject()
		{
			var mouseCollection = new ActiveBusinessObjectCollection<MouseForTest>(Factory);
			var mouseValidator = new BusinessObjectCollectionMaxCountValidator(mouseCollection);
			mouseValidator.MaxCount = 1;
			var bo1 = mouseCollection.AddNew();
			AssertEquals(0, mouseCollection[0].RowNotifications.Count());
			var bo2 = mouseCollection.AddNew();
			AssertContains("Mouse ", mouseCollection[1].RowNotifications.GetFirst().Message);
			mouseValidator.MaxCount = 2;
			var bo3 = mouseCollection.AddNew();
			AssertContains("Mice ", mouseCollection[2].RowNotifications.GetFirst().Message);
		}

		public void TestValidate()
		{
			TestValidateCore();
		}

		public void TestValidate_WithCustomNotification()
		{
			Validator.Notification = new Notification(NotificationType.Error, "Error");
			TestValidateCore();
		}

		void TestValidateCore()
		{
			Validator.MaxCount = 2;
			Collection.AdditionalFilter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.NotEqual, "Exclude");

			DummyBusinessObject dummy1 = Collection.AddNew();
			DummyBusinessObject dummy2 = Collection.AddNew();
			DummyBusinessObject dummy3 = Collection.AddNew();
			DummyBusinessObject dummy4 = Collection.AddNew();

			AssertEquals(4, Collection.Count);
			AssertEquals(0, Collection[0].RowNotifications.Count());
			AssertEquals(0, Collection[1].RowNotifications.Count());
			AssertEquals(1, Collection[2].RowNotifications.Count());
			AssertEquals(1, Collection[3].RowNotifications.Count());

			dummy1.Z0_Description = "Exclude";
			AssertEquals(3, Collection.Count);
			AssertEquals(0, Collection[0].RowNotifications.Count());
			AssertEquals(0, Collection[1].RowNotifications.Count());
			AssertEquals(1, Collection[2].RowNotifications.Count());

			dummy1.Z0_Description = "XXX";
			AssertEquals(4, Collection.Count);
			AssertEquals(0, Collection[0].RowNotifications.Count());
			AssertEquals(0, Collection[1].RowNotifications.Count());
			AssertEquals(1, Collection[2].RowNotifications.Count());
			AssertEquals(1, Collection[3].RowNotifications.Count());

			DummyBusinessObject dummy5 = Collection.AddNew();
			AssertEquals(5, Collection.Count);
			AssertEquals(0, Collection[0].RowNotifications.Count());
			AssertEquals(0, Collection[1].RowNotifications.Count());
			AssertEquals(1, Collection[2].RowNotifications.Count());
			AssertEquals(1, Collection[3].RowNotifications.Count());
			AssertEquals(1, Collection[4].RowNotifications.Count());

			dummy3.Z0_Description = "Exclude";
			dummy4.Z0_Description = "Exclude";
			dummy5.Z0_Description = "Exclude";
			DummyBusinessObject dummy6 = Collection.AddNew();
			AssertEquals(3, Collection.Count);
			AssertEquals(0, Collection[0].RowNotifications.Count());
			AssertEquals(0, Collection[1].RowNotifications.Count());
			AssertEquals(1, Collection[2].RowNotifications.Count());
		}

		public void TestValidate_WhenMaxCountChanges()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			DummyBusinessObject dummy1 = collection.AddNew();
			DummyBusinessObject dummy2 = collection.AddNew();
			DummyBusinessObject dummy3 = collection.AddNew();

			Validator.MaxCount = 2;
			AssertEquals(0, collection[0].RowNotifications.Count());
			AssertEquals(0, collection[1].RowNotifications.Count());
			AssertEquals(1, collection[2].RowNotifications.Count());

			Validator.MaxCount = 1;
			AssertEquals(0, collection[0].RowNotifications.Count());
			AssertEquals(1, collection[1].RowNotifications.Count());
			AssertEquals(1, collection[2].RowNotifications.Count());
		}

		public void TestGetMaxCount()
		{
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			Validator.MaxCount = 3;
			AssertEquals(0, collection[0].RowNotifications.Count());
			AssertEquals(0, collection[1].RowNotifications.Count());
			AssertEquals(0, collection[2].RowNotifications.Count());
			AssertEquals(1, collection[3].RowNotifications.Count());
			AssertEquals(1, collection[4].RowNotifications.Count());

			Validator.GetMaxCountReduction = () => 1;
			Validator.Refresh();
			AssertEquals(0, collection[0].RowNotifications.Count());
			AssertEquals(0, collection[1].RowNotifications.Count());
			AssertEquals(1, collection[2].RowNotifications.Count());
			AssertEquals(1, collection[3].RowNotifications.Count());
			AssertEquals(1, collection[4].RowNotifications.Count());

			Validator.GetMaxCountReduction = () => 2;
			Validator.Refresh();
			AssertEquals(0, collection[0].RowNotifications.Count());
			AssertEquals(1, collection[1].RowNotifications.Count());
			AssertEquals(1, collection[2].RowNotifications.Count());
			AssertEquals(1, collection[3].RowNotifications.Count());
			AssertEquals(1, collection[4].RowNotifications.Count());

			Validator.GetMaxCountReduction = () => 5;
			Validator.Refresh();
			AssertEquals(1, collection[0].RowNotifications.Count());
			AssertEquals(1, collection[1].RowNotifications.Count());
			AssertEquals(1, collection[2].RowNotifications.Count());
			AssertEquals(1, collection[3].RowNotifications.Count());
			AssertEquals(1, collection[4].RowNotifications.Count());
		}

		public void TestRefreshResult()
		{
			Validator.MaxCount = 3;
			Validator.WarnAtHalfway = true;

			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			var bizo01 = collection.AddNew();
			var bizo02 = collection.AddNew();
			var bizo03 = collection.AddNew();
			var bizo04 = collection.AddNew();
			var bizo05 = collection.AddNew();
			var bizo06 = collection.AddNew();

			AssertEquals(0, bizo01.RowNotifications.Count());
			AssertEquals(1, bizo02.RowNotifications.Count());
			AssertEquals(1, bizo03.RowNotifications.Count());
			AssertEquals(1, bizo04.RowNotifications.Count());
			AssertEquals(1, bizo05.RowNotifications.Count());
			AssertEquals(1, bizo06.RowNotifications.Count());

			AssertEquals(NotificationType.Warning, bizo02.RowNotifications.ToArray()[0].Type);
			AssertEquals(NotificationType.Warning, bizo03.RowNotifications.ToArray()[0].Type);
			AssertEquals(NotificationType.Error, bizo04.RowNotifications.ToArray()[0].Type);
			AssertEquals(NotificationType.Error, bizo05.RowNotifications.ToArray()[0].Type);
			AssertEquals(NotificationType.Error, bizo06.RowNotifications.ToArray()[0].Type);

			Validator.GetMaxCountReduction = () => -2;
			Validator.Refresh();

			AssertEquals(0, bizo01.RowNotifications.Count());
			AssertEquals(0, bizo02.RowNotifications.Count());
			AssertEquals(1, bizo03.RowNotifications.Count());
			AssertEquals(1, bizo04.RowNotifications.Count());
			AssertEquals(1, bizo05.RowNotifications.Count());
			AssertEquals(1, bizo06.RowNotifications.Count());

			AssertEquals(NotificationType.Warning, bizo03.RowNotifications.ToArray()[0].Type);
			AssertEquals(NotificationType.Warning, bizo04.RowNotifications.ToArray()[0].Type);
			AssertEquals(NotificationType.Warning, bizo05.RowNotifications.ToArray()[0].Type);
			AssertEquals(NotificationType.Error, bizo06.RowNotifications.ToArray()[0].Type);
		}

		BusinessObjectCollectionMaxCountValidator Validator
		{
			get { return validator ?? (validator = new BusinessObjectCollectionMaxCountValidator(Collection)); }
		}
		BusinessObjectCollectionMaxCountValidator validator;

		ActiveBusinessObjectCollection<DummyBusinessObject> Collection
		{
			get { return collection ?? (collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory)); }
		}
		ActiveBusinessObjectCollection<DummyBusinessObject> collection;

		class MouseForTest : DummyBusinessObject
		{
			public MouseForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override Types.ZString HumanReadableNameCore
			{
				get { return "Mouse"; }
			}

			protected override Types.ZString HumanReadableNameForPluralCore
			{
				get { return "Mice"; }
			}
		}
	}
}
