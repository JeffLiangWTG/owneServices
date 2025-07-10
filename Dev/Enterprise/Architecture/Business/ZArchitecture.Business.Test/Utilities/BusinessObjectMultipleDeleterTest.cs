using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class BusinessObjectMultipleDeleterTest : TestCaseWithFactory
	{
		public void TestObjectsCorrectlyCaclulatedOverMax()
		{
			List<BusinessObject> list = new List<BusinessObject>();
			DummyCancellable obj1 = Factory.New<DummyCancellable>();
			DummyBusinessObject obj2 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			for (int i = 0; i < 30; i++)
			{
				list.Add(obj1);
				list.Add(obj2);
			}
			list.Add(obj1);

			var deleter = new BusinessObjectMultipleDeleter(list.ToArray());

			AssertMultilineASCIIEquals("There should be 31 cancellable and 30 deletable objects",
				@"You are about to delete 30 objects.", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Delete));

			AssertMultilineASCIIEquals("There should be 31 cancellable and 30 deletable objects",
				@"If some objects are already inactive, no action will be performed on them.
You are about to deactivate 31 objects.", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Deactivate));

			AssertMultilineASCIIEquals("There should be 31 cancellable and 30 deletable objects",
				@"If some objects are already active, no action will be performed on them.
You are about to activate 31 objects.", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Activate));
		}

		public void TestObjectsCorrectlyCaclulated()
		{
			DummyCancellable obj1 = Factory.New<DummyCancellable>();
			DummyBusinessObject obj2 = Factory.New<DummyBusinessObject>();
			DummyCancellable obj3 = Factory.New<DummyCancellable>();
			DummyBusinessObject obj4 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject obj5 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var deleter = new BusinessObjectMultipleDeleter(new BusinessObject[] { obj1, obj2, obj3, obj4, obj5 });

			AssertMultilineASCIIEquals("There should be 2 cancellable and 3 deletable objects",
				"If some objects are already inactive, no action will be performed on them." +
"\r\nThese objects will be deactivated:\r\nDummyBizoCancellable\r\nDummyBizoCancellable", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Deactivate));

			AssertMultilineASCIIEquals("There should be 2 cancellable and 3 deletable objects",
				"The selected records will be deleted:\r\nDummyBizo\r\nDummyBizo\r\nDummyBizo", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Delete));
		}

		public void TestAllApplicableSelectedObjectsAreDeleted()
		{
			DummyBusinessObject obj1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject obj2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject obj3 = Factory.New<DummyBusinessObject>();
			DummyCancellable obj4 = Factory.New<DummyCancellable>();
			DummyCantDelete obj5 = Factory.New<DummyCantDelete>();
			DummyDependantBusinessObject dep = Factory.New<DummyDependantBusinessObject>();
			dep.ZD1_Z0 = obj1.PK;
			Factory.Save();

			new BusinessObjectMultipleDeleter(new BusinessObject[] { obj1, obj2, obj3, obj4 }).Process(BusinessObjectMultipleDeleterAction.Deactivate);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			obj1 = factory.Load<DummyBusinessObject>(obj1.PK);
			obj2 = factory.Load<DummyBusinessObject>(obj2.PK);
			obj3 = factory.Load<DummyBusinessObject>(obj3.PK);
			obj4 = factory.Load<DummyCancellable>(obj4.PK);
			obj5 = factory.Load<DummyCantDelete>(obj5.PK);
			AssertNotNull("Nothing is deleted as action was Deactivate", obj1);
			AssertNotNull("Nothing is deleted as action was Deactivate", obj2);
			AssertNotNull("Nothing is deleted as action was Deactivate", obj3);
			AssertNotNull("Nothing is deleted as action was Deactivate", obj4);
			AssertNotNull("Nothing is deleted as action was Deactivate", obj5);

			string message = new BusinessObjectMultipleDeleter(new BusinessObject[] { obj1, obj2, obj3, obj4 }).Process(BusinessObjectMultipleDeleterAction.Delete);
			factory = new BusinessObjectFactory();
			obj1 = factory.Load<DummyBusinessObject>(obj1.PK);
			obj2 = factory.Load<DummyBusinessObject>(obj2.PK);
			obj3 = factory.Load<DummyBusinessObject>(obj3.PK);
			obj4 = factory.Load<DummyCancellable>(obj4.PK);
			obj5 = factory.Load<DummyCantDelete>(obj5.PK);
			AssertNotNull(obj1);
			AssertNull(obj2);
			AssertNull(obj3);
			AssertNotNull(obj4);
			AssertNotNull(obj5);
			Assert(message.Contains("The DummyBizo cannot be deleted, because there is at least one DummyDependentBizo referencing it."));
		}

		public void TestAllApplicableSelectedObjectsAreNotDeletedIfTheyCant()
		{
			DummyBusinessObject obj1 = Factory.New<DummyBusinessObject>();
			DummyCancellable obj2 = Factory.New<DummyCancellable>();
			DummyCantDelete obj3 = Factory.New<DummyCantDelete>();
			DummyCantDelete obj4 = Factory.New<DummyCantDelete>();
			Factory.Save();

			string message = new BusinessObjectMultipleDeleter(new BusinessObject[] { obj1, obj2, obj3, obj4 }).Process(BusinessObjectMultipleDeleterAction.Deactivate);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			obj1 = factory.Load<DummyBusinessObject>(obj1.PK);
			obj2 = factory.Load<DummyCancellable>(obj2.PK);
			obj3 = factory.Load<DummyCantDelete>(obj3.PK);
			obj4 = factory.Load<DummyCantDelete>(obj4.PK);
			AssertNotNull(obj1);
			AssertNotNull(obj2);
			AssertNotNull(obj3);
			AssertNotNull(obj4);
			AssertMultilineASCIIEquals("Only one should be deleted. Second one is cancellable. Others cannot be deleted.",
				@"This Dummy BizO can't be cancelled.", message);

			message = new BusinessObjectMultipleDeleter(new BusinessObject[] { obj1, obj2, obj3, obj4 }).Process(BusinessObjectMultipleDeleterAction.Delete);

			factory = new BusinessObjectFactory();
			obj1 = factory.Load<DummyBusinessObject>(obj1.PK);
			obj2 = factory.Load<DummyCancellable>(obj2.PK);
			obj3 = factory.Load<DummyCantDelete>(obj3.PK);
			obj4 = factory.Load<DummyCantDelete>(obj4.PK);
			AssertNull(obj1);
			AssertNotNull(obj2);
			AssertNotNull(obj3);
			AssertNotNull(obj4);
			AssertMultilineASCIIEquals("Only one should be deleted. Second one is cancellable. Others cannot be deleted.",
				@"Object cannot be deleted. Reason: Because its Friday tomorrow!
Object cannot be deleted. Reason: Because its Friday tomorrow!", message);
		}

		public void TestAllApplicableSelectedObjectsAreCancelled()
		{
			DummyCancellable obj1 = Factory.New<DummyCancellable>();
			DummyCancellable obj2 = Factory.New<DummyCancellable>();
			DummyCancellable obj3 = Factory.New<DummyCancellable>();
			DummyBusinessObject obj4 = Factory.New<DummyBusinessObject>();
			DummyDependantBusinessObject dep = Factory.New<DummyDependantBusinessObject>();
			dep.ZD1_Z0 = obj1.PK;
			DummyCantDelete obj5 = Factory.New<DummyCantDelete>();
			DummyCantDelete obj6 = Factory.New<DummyCantDelete>();
			Factory.Save();

			string message = new BusinessObjectMultipleDeleter(new BusinessObject[] { obj1, obj2, obj3, obj4 }).Process(BusinessObjectMultipleDeleterAction.Deactivate);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			obj1 = factory.Load<DummyCancellable>(obj1.PK);
			obj2 = factory.Load<DummyCancellable>(obj2.PK);
			obj3 = factory.Load<DummyCancellable>(obj3.PK);
			obj4 = factory.Load<DummyBusinessObject>(obj4.PK);
			obj5 = factory.Load<DummyCantDelete>(obj5.PK);
			obj6 = factory.Load<DummyCantDelete>(obj6.PK);
			AssertNotNull(obj1);
			AssertNotNull(obj2);
			AssertNotNull(obj3);
			AssertNotNull("Action was Deactivate", obj4);
			AssertNotNull(obj5);
			AssertNotNull(obj6);
			AssertMultilineASCIIEquals("Must try to cancel, but fail due to DummyCancellable options",
@"This Dummy BizO can't be cancelled.
This Dummy BizO can't be cancelled.
This Dummy BizO can't be cancelled.", message);
		}

		public void TestAllApplicableSelectedObjectsAreReactivated()
		{
			DummyCancellable obj1 = Factory.New<DummyCancellable>();
			DummyCancellable obj2 = Factory.New<DummyCancellable>();
			DummyCancellable obj3 = Factory.New<DummyCancellable>();
			obj1.IsCancelled = true;
			obj2.IsCancelled = true;
			obj3.IsCancelled = true;
			DummyBusinessObject obj4 = Factory.New<DummyBusinessObject>();
			DummyDependantBusinessObject dep = Factory.New<DummyDependantBusinessObject>();
			dep.ZD1_Z0 = obj1.PK;
			DummyCantDelete obj5 = Factory.New<DummyCantDelete>();
			DummyCantDelete obj6 = Factory.New<DummyCantDelete>();
			Factory.Save();

			string message = new BusinessObjectMultipleDeleter(new BusinessObject[] { obj1, obj2, obj3, obj4 }).Process(BusinessObjectMultipleDeleterAction.Activate);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			obj1 = factory.Load<DummyCancellable>(obj1.PK);
			obj2 = factory.Load<DummyCancellable>(obj2.PK);
			obj3 = factory.Load<DummyCancellable>(obj3.PK);
			obj4 = factory.Load<DummyBusinessObject>(obj4.PK);
			obj5 = factory.Load<DummyCantDelete>(obj5.PK);
			obj6 = factory.Load<DummyCantDelete>(obj6.PK);
			AssertNotNull(obj1);
			AssertNotNull(obj2);
			AssertNotNull(obj3);
			AssertNotNull("Action was Deactivate", obj4);
			AssertNotNull(obj5);
			AssertNotNull(obj6);
			AssertMultilineASCIIEquals("Must try to activate, but fail due to DummyCancellable options",
@"This Dummy BizO can't be cancelled.
This Dummy BizO can't be cancelled.
This Dummy BizO can't be cancelled.", message);
		}

		public void TestGetConfirmationMessage_WithNoObjectsListing()
		{
			List<BusinessObject> list = new List<BusinessObject>();
			DummyCancellable obj1 = Factory.New<DummyCancellable>();
			DummyBusinessObject obj2 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			for (int i = 0; i < 30; i++)
			{
				list.Add(obj1);
				list.Add(obj2);
			}
			list.Add(obj1);

			var deleter = new BusinessObjectMultipleDeleter(list.ToArray());

			AssertMultilineASCIIEquals("There should be 31 cancellable and 30 deletable objects",
				@"You are about to delete 30 objects.", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Delete, false));

			AssertMultilineASCIIEquals("There should be no objects listing",
				@"Would you like to deactivate the selected items? If some objects are already inactive, no action will be performed on them.", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Deactivate, false));

			AssertMultilineASCIIEquals("There should be no objects listing",
				@"Would you like to activate the selected items? If some objects are already active, no action will be performed on them.", deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Activate, false));
		}

		[PreventDelete(true)]
		class DummyCancellable : DummyBusinessObject, ICancellable
		{
			public DummyCancellable(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get
				{
					return base.HumanReadableNameCore + "Cancellable";
				}
			}

			#region ICancellable Members

			public string CanCancel()
			{
				return "This Dummy BizO can't be cancelled.";
			}

			public string CanReactivate()
			{
				return "This Dummy BizO can't be reactivated.";
			}

			public bool IsCancelled
			{
				get
				{
					return isCancelled;
				}
				set
				{
					isCancelled = value;
				}
			}
			bool isCancelled;

			public bool IsCancelledHasChanged
			{
				get { return false; }
			}

			#endregion
		}

		class DummyCantDelete : DummyBusinessObject
		{
			public DummyCantDelete(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get
				{
					return base.HumanReadableNameCore + "Can't Delete";
				}
			}

			public override bool CanDelete
			{
				get
				{
					return false;
				}
			}

			public override MultilingualString ReasonForNotAbleToDelete
			{
				get
				{
					return (NoResString)"Because its Friday tomorrow!";
				}
			}
		}
	}
}
