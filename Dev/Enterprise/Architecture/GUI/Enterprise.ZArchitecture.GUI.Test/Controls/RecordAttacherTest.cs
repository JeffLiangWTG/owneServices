using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZRecordAttacherTest : TestCaseWithDummy
	{
		public void TestErrorReporterWhenListProviderIsNull()
		{
			var mockBusinessObjectCollection = new Mock<IBusinessObjectCollection>().Object;
			var attacher = new ZRecordAttacher(Dummy.Collection, mockBusinessObjectCollection, DummyModuleIDs.Dummy);

			_ = (attacher as IFindBox).ListProvider;
			AssertEquals("ZRecordAttacher.ListProviderIsNullWhileFindBoxListIsNotNull key should be reported", "ZRecordAttacher.ListProviderIsNullWhileFindBoxListIsNotNull", ErrorReporter.LastKeyReported);
			AssertEquals("ZRecordAttacher.ListProviderIsNullWhileFindBoxListIsNotNull message should be reported", mockBusinessObjectCollection.GetType().FullName, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestModuleDecisionProviderGetSet()
		{
			var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
			AssertNotNull("Lazy created module decision provider", attacher.ModuleDecisionProvider);

			IModuleDecisionProvider provider = new Modules.Internal.DirectToFormModuleDecisionProvider(new DummyFindBox());
			attacher.ModuleDecisionProvider = provider;
			AssertEquals("Get/set", attacher.ModuleDecisionProvider, provider);
		}

		public void TestShowAndAttach()
		{
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				Dummy.Collection.AddNew();
				AssertEquals("Added one", 1, Dummy.Collection.Count);
				Dummy.Factory.Save();
				AssertEquals("No changes", false, Dummy.HasChanges);

				var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacher.Show(form);

				try
				{
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.Factory.Save();
					attacher.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0] });
					AssertEquals("Should be another attached", 2, Dummy.Collection.Count);
					AssertEquals("Existing Element HasChanges", false, Dummy.Collection[0].HasChanges);
					AssertEquals("Added Element HasChanges", false, Dummy.Collection[1].HasChanges);
				}
				finally
				{
					attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		public void TestAttachWithMultiSelect()
		{
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				Dummy.Collection.AddNew();
				AssertEquals("Added one", 1, Dummy.Collection.Count);
				Dummy.Factory.Save();
				AssertEquals("No changes", false, Dummy.HasChanges);

				var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacher.Show(form);

				try
				{
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.Factory.Save();
					attacher.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0], Dummy.FilteredCollection[1] });
					AssertEquals("Should be 2 more attached", 3, Dummy.Collection.Count);
					AssertEquals("Existing Element HasChanges", false, Dummy.Collection[0].HasChanges);
					AssertEquals("Added Element (1) HasChanges", false, Dummy.Collection[1].HasChanges);
					AssertEquals("Added Element (2) HasChanges", false, Dummy.Collection[2].HasChanges);
				}
				finally
				{
					attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		public void TestAttachHasFiredOnAttachedEvent()
		{
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();

				var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacher.OnAttach += Attacher_OnAttach;
				attacher.Show(form);

				try
				{
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.Factory.Save();
					attacher.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0], Dummy.FilteredCollection[1] });
					AssertEquals("Should be two new objects attached", 2, Dummy.Collection.Count);
				}
				finally
				{
					attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		void Attacher_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			AssertEquals("OnAttach event was fired, two added object has been passed to event args", 2, e.AttachedBusinessObjects.Length);
		}

		public void TestOnAttachEventIsFiredBeforeActualAttaching()
		{
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				attacherForTesting = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacherForTesting.OnAttach += Attacher_OnAttachForTestingOrder;
				attacherForTesting.Show(form);

				try
				{
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.Factory.Save();
					attacherForTesting.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0], Dummy.FilteredCollection[1] });
					AssertEquals("All child objects should be added after selected is called. ", 2, attacherForTesting.DestinationCollectionCount);
				}
				finally
				{
					attacherForTesting.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		ZRecordAttacher attacherForTesting;

		void Attacher_OnAttachForTestingOrder(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			AssertEquals("When OnAttach is called, collection should only have initial 1 row (automatically created when form is created).", 1, attacherForTesting.DestinationCollectionCount);
		}

		public void TestErrorForAlreadyAddedItemsInAttachFindBox()
		{
			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			TestCaseHelper.ClearTable(Dummy.TableName);
			var dummy1 = (DummyBaseBusinessObject)Dummy.Collection.AddNew();
			dummy1.Z0_Number = 5;
			var dummy2 = (DummyBaseBusinessObject)Dummy.Collection.AddNew();
			dummy2.Z0_Number = 5;
			Dummy.Collection.Remove(dummy1);
			AssertEquals("One only", 1, Dummy.Collection.Count);
			Dummy.Factory.Save();

			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacher.Show(form);
				try
				{
					AssertEquals("Should have both items to show", 2, Dummy.FilteredCollection.Count);
					var objectWithError = Dummy.FilteredCollection[0].HasRowErrors ? Dummy.FilteredCollection[0] : Dummy.FilteredCollection[1];
					Assert("Exactly one item has an error", objectWithError.HasErrors && (Dummy.FilteredCollection[0].HasRowErrors ^ Dummy.FilteredCollection[1].HasRowErrors));
					AssertEquals("Error", true, objectWithError.RowErrors.ContainsNotificationContaining("This record has already been selected. Please ensure you select only records that have not already been used."));
				}
				finally
				{
					attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}

			Dummy.Collection.Remove(dummy2);

			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacher.Show(form);
				try
				{
					AssertEquals("Should have both items to show", 2, Dummy.FilteredCollection.Count);
					AssertEquals("No error on Dummy1", false, Dummy.FilteredCollection[0].HasRowErrors);
					AssertEquals("No error on Dummy2", false, Dummy.FilteredCollection[1].HasRowErrors);
				}
				finally
				{
					attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		public void TestDefaultWordingIsRecord()
		{
			AssertEquals("record", new ZRecordAttacher(null, null, null).NameOfAnElementInDestinationCollection);
		}

		public void TestCannotAddMessage()
		{
			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			TestCaseHelper.ClearTable(Dummy.TableName);

			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				var dummyKid = Factory.New<DummyBaseBusinessObject>();
				dummyKid.Z0_Number = 5;
				Factory.Save();

				var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacher.Show(form);

				try
				{
					AssertEquals("Should have dummyKid as it is not attached", 1, Dummy.FilteredCollection.Count);
					dummyKid.Delete();

					attacher.NameOfAnElementInDestinationCollection = "teapot";
					attacher.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0] });
					AssertEquals("Correct message shown", "Unable to attach 1 of 1 teapot(s) as they are no longer in the database.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		[RequiresSTA]
		public void TestSuspendCellNotification()
		{
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Grid.InnerGrid.IsTestingUpdateNonCellNotifications = true;
				form.Show();
				Dummy.Collection.CountChanged += Collection_CountChanged;

				var attacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				attacher.Show(form);

				try
				{
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.AddNew();
					Dummy.FilteredCollection.Factory.Save();
					form.Grid.InnerGrid.UpdateNonCellNotificationsCountForTesting = 0;
					form.Grid.InnerGrid.UpdateGridNotificationTypeCountForTesting = 0;

					attacher.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0], Dummy.FilteredCollection[1], Dummy.FilteredCollection[2] });

					AssertEquals("UpdateNonCellNotifications should be called 12 times", 12, form.Grid.InnerGrid.UpdateNonCellNotificationsCountForTesting);
					AssertEquals("Grid Notification Type should be updated only once", 1, form.Grid.InnerGrid.UpdateGridNotificationTypeCountForTesting);
					AssertEquals("Grid Notification Type should be updated", CargoWise.ComponentModel.NotificationType.Error, form.Grid.InnerGrid.NotificationTypeForTesting);
				}
				finally
				{
					attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var dummyChild = e.BizObject as DummyChildBusinessObject;
				dummyChild.Z0_Date = new ZDateTime(1900, 1, 1);
				dummyChild.Z0_AnotherDate = new ZDateTime(1900, 1, 1);
			}
		}
	}
}
