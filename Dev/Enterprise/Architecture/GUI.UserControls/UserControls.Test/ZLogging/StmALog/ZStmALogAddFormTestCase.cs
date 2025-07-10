using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmALogAddFormTestCase : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			using (new ZStmALogAddForm(new StmALogCollection(Factory).AddNew(typeof(StmALogAsAddedByUser)) as StmALogAsAddedByUser, false))
			{
			}
		}

		[ExpectNoExceptions]
		public void TestShowingForm()
		{
			using (var eventForm = new ZStmALogAddForm(new StmALogCollection(Factory).AddNew(typeof(StmALogAsAddedByUser)) as StmALogAsAddedByUser, false))
			{
				eventForm.Show();
			}
		}

		[ExpectNoExceptions]
		public void TestAddNewEvent_CancelButtonClicked()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log = new BusinessObjectFactory().New<StmALogAsAddedByUser>();
			log.SL_SE_NKEvent = Events.ArrivalCode;

			using (var addForm = new ZStmALogAddForm(log, dummy))
			{
				Assert("Precondition", !log.HasErrors);
				addForm.Show();
				var cancelButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["CancelAddButton"];
				cancelButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Should not add event (StmALog) to collection if adding was cancelled", 0, dummy.Logs.GetAllLogs().Count);
				AssertEquals("Parent bizo should have no changes", false, dummy.HasChanges);
			}
		}

		[ExpectNoExceptions]
		public void TestAddNewEvent_AddButtonClicked()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log = new BusinessObjectFactory().New<StmALogAsAddedByUser>();
			log.SL_SE_NKEvent = Events.ArrivalCode;
			log.Master = dummy;

			using (var addForm = new ZStmALogAddForm(log, dummy))
			{
				Assert("Precondition", !log.HasErrors);
				addForm.Show();
				var addEventButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
				addEventButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Should add event (StmALog) to collection if adding was confirmed and StmALog has no validation errors", 1, dummy.Logs.GetAllLogs().Count);
				AssertEquals("If log is added to BusinessObject's Logs then log's SL_Parent should point to BusinessObject", dummy.PK, dummy.Logs.GetAllLogs()[0].SL_Parent);
				AssertEquals("Parent bizo should have changes", true, dummy.HasChanges);
			}
		}

		public void TestAddNewEvent_AddButtonClickedWithInvalidCode()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log = new BusinessObjectFactory().New<StmALogAsAddedByUser>();

			using (var addForm = new ZStmALogAddForm(log, dummy))
			{
				addForm.Show();
				var addEventButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
				var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
				const string invalidEventCode = "GG";
				addFormBusinessEntity.SL_SE_NKEvent = invalidEventCode;
				addFormBusinessEntity.SL_EventTime = ZDateTime.UtcNow;
				addFormBusinessEntity.Master = null;
				addFormBusinessEntity.SL_Parent = ZGuid.NewZGuid();
				addEventButton.PerformClick();
				Application.DoEvents();
				addForm.Close();
				Application.DoEvents();
				Assert("Precondition", log.HasNotifications());
				AssertEquals("It should have validation error as the property Master is null", 0, dummy.Logs.GetAllLogs().Count);
			}
		}

		[ExpectNoExceptions]
		public void TestAddNewEvent_AddButtonClickedWithAppend_ValidString()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();

			using (var addForm = new ZStmALogAddForm(new StmALogCollectionView(dummy), false, "Added|LOC=AUMLB"))
			{
				using (ZFormModaliser.SuspendDispose())
				{
					addForm.Show();
					var addEventButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
					var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
					addFormBusinessEntity.SL_SE_NKEvent = Events.ArrivalCode;
					addFormBusinessEntity.SL_Reference = "TEST";
					addEventButton.PerformClick();
				}

				AssertEquals("Should add event (StmALog) to collection if adding was confirmed and StmALog has no validation errors", 1, dummy.Logs.GetAllLogs().Count);
				AssertEquals("If log is added to BusinessObject's Logs then log's SL_Parent should point to BusinessObject", dummy.PK, dummy.Logs.GetAllLogs()[0].SL_Parent);
				AssertEquals("Should have appended test to the reference", "TEST Added|LOC=AUMLB", dummy.Logs.GetAllLogs()[0].SL_Reference);
				AssertEquals("Parent bizo should have changes", true, dummy.HasChanges);
			}

			using (var addForm = new ZStmALogAddForm(new StmALogCollectionView(dummy), false, "test"))
			{
				using (ZFormModaliser.SuspendDispose())
				{
					addForm.Show();
					var addEventButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
					var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
					addFormBusinessEntity.SL_SE_NKEvent = Events.ArrivalCode;
					addFormBusinessEntity.SL_Reference = "TEST";
					addEventButton.PerformClick();
				}

				AssertEquals("Should add event (StmALog) to collection if adding was confirmed and StmALog has no validation errors", 2, dummy.Logs.GetAllLogs().Count);
				AssertEquals("If log is added to BusinessObject's Logs then log's SL_Parent should point to BusinessObject", dummy.PK, dummy.Logs.GetAllLogs()[1].SL_Parent);
				AssertEquals("Should not have appended test to the reference", "TEST", dummy.Logs.GetAllLogs()[1].SL_Reference);
				AssertEquals("Parent bizo should have changes", true, dummy.HasChanges);
			}
		}

		public void TestAddNewEvent_AddButtonClickedWithAppend_InvalidString()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();

			using (new DisposableAction(() => ErrorReporter.Clear()))
			using (var addForm = new ZStmALogAddForm(new StmALogCollectionView(dummy), false, "TryAdd|SYD=Value"))
			{
				using (ZFormModaliser.SuspendDispose())
				{
					addForm.Show();
					var addEventButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
					var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
					addFormBusinessEntity.SL_SE_NKEvent = Events.ArrivalCode;
					addFormBusinessEntity.SL_Reference = "TEST";
					addEventButton.PerformClick();
				}

				AssertContains("parameter codes are not valid: SYD", ErrorReporter.LastMessageReported);
			}
		}

		[ExpectNoExceptions]
		public void TestAddNewEvent_AddButtonClickedLogHasValidationErrors()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log = new BusinessObjectFactory().New<StmALogAsAddedByUser>();
			log.Master = dummy;
			using (var addForm = new ZStmALogAddForm(log, dummy))
			{
				addForm.Show();
				var addEventButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
				addEventButton.PerformClick();
				Application.DoEvents();
				addForm.Close();
				Application.DoEvents();
				Assert("Precondition", log.HasNotifications());
				AssertEquals("Should not add event (StmALog) to collection if adding was confirmed but StmALog has validation errors", 0, dummy.Logs.GetAllLogs().Count);
			}
		}

		[ExpectNoExceptions]
		public void TestAddNewEvent_AddButtonClickedLogHasInvalidCodeError()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log = new BusinessObjectFactory().New<StmALogAsAddedByUser>();
			log.Master = dummy;

			using (var addForm = new ZStmALogAddForm(log, dummy))
			{
				addForm.Show();
				var addEventButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
				var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
				var invalidEventCode = "GG";
				addFormBusinessEntity.SL_SE_NKEvent = invalidEventCode;
				addFormBusinessEntity.SL_EventTime = ZDateTime.UtcNow;
				addEventButton.PerformClick();
				Application.DoEvents();
				addForm.Close();
				Application.DoEvents();
				Assert("Precondition", log.HasNotifications());
				AssertEquals("It should have validation error", 0, dummy.Logs.GetAllLogs().Count);
			}
		}

		public void TestEvents_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var log = new BusinessObjectFactory().New<StmALogAsAddedByUser>();

			log.SL_SE_NKEvent = Events.Arrival.Code;
			AssertNoErrors("Valid PW event, Valid CW1 event", log.SL_SE_NKEventInfo);

			log.SL_SE_NKEvent = Events.CargoAcceptedAtOriginDepot.Code;
			AssertHasError("Invalid PW event, Valid CW1 event", log.SL_SE_NKEventInfo, "Enter a valid Event Code.");

			log.SL_SE_NKEvent = "Z0Z"; // This is a code for a non-existent event for testing
			AssertHasError("Invalid PW event, Invalid CW1 event", log.SL_SE_NKEventInfo, "Enter a valid Event Code.");
		}

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyEnterpriseBusinessObject); }
		}
	}
}
