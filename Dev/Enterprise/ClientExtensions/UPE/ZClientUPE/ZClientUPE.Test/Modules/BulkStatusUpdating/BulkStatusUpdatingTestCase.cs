using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Module.Testing
{
	abstract class BulkStatusUpdatingTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestNonPersistentQueue()
		{
			Assert("Correct type of NonPersistentProcessQueue", ExpectedNonPersistentProcessQueueType.IsInstanceOfType(BulkUpdateBizObj.NonPersistentQueue));
			AssertEquals("Has to be registered as editable child object", true, BulkUpdateBizObj.IsRegisteredEditableChildObject(BulkUpdateBizObj.NonPersistentQueue));
			AssertEquals("AssignedTo should Read-Only if disabled", !ExpectedAssignedToEnabled, BulkUpdateBizObj.NonPersistentQueue.AssignedToInfo.ReadOnly);
		}

		#region BulkUpdate
		public void TestBulkUpdate()
		{
			BulkUpdateBizObj.NonPersistentQueue.QueueName = ValidQueueNameToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.Status = ValidStatusToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.SubStatus = ValidSubStatusToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.AssignedTo = GlbStaff.CurrentUser.GS_Code;
			BulkUpdateBizObj.NonPersistentQueue.Reason = "NewRemarks";
			Factory.ThrowOnSave = true;
			NotificationBuffer notify = new NotificationBuffer();
			BulkUpdateBizObj.BulkUpdate(notify);
			Factory.ThrowOnSave = false;
			AssertEquals("Should update QueueName", ValidQueueNameToBulkUpdateTo, ItemQueueToBulkUpdate.QueueName);
			AssertEquals("Should update Status", ValidStatusToBulkUpdateTo, ItemQueueToBulkUpdate.Status);
			AssertEquals("Should update SubStatus", ValidSubStatusToBulkUpdateTo, ItemQueueToBulkUpdate.SubStatus);
			AssertEquals("Should update AssignedTo", GlbStaff.CurrentUser.GS_Code, ItemQueueToBulkUpdate.AssignedTo);
			AssertEquals("Should update Remarks", "NewRemarks", ItemQueueToBulkUpdate.Reason);
		}

		public void TestBulkUpdate_RecoverOnConcurrencyError()
		{
			BulkUpdateBizObj.NonPersistentQueue.QueueName = ValidQueueNameToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.Status = ValidStatusToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.SubStatus = ValidSubStatusToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.AssignedTo = GlbStaff.CurrentUser.GS_Code;
			BulkUpdateBizObj.NonPersistentQueue.Reason = "NewRemarks";
			BulkUpdateBizObj.BeforeBulkUpdateSave += new EventHandler(OnBeforeBulkUpdateSave_ConcurrentlyModifyItemToBulkUpdate);
			NotificationBuffer notify = new NotificationBuffer();
			int updateCount = BulkUpdateBizObj.BulkUpdate(notify);
			AssertEquals("There should be an error raised about the concurrency error, but no exception", true, notify.HasErrors);
			AssertEquals("There should be an error raised about the concurrency error, but no exception", true, notify.AsString.IndexOf("Another user") != -1);
			AssertEquals("There should be no items updated", 0, updateCount);
		}

		void OnBeforeBulkUpdateSave_ConcurrentlyModifyItemToBulkUpdate(object sender, EventArgs e)
		{
			DbCommand concurrentUserCommand = Db.Connection.Command(string.Format("UPDATE dbo.ProcessQueue set P4_CustomsQueue='XXX', P4_QueueName='XXX' WHERE P4_PK='{0}'", ItemQueueToBulkUpdate.ProcessQueue.PK.ToString()));
			concurrentUserCommand.ExecuteNonQuery();
		}

		public void TestBulkUpdate_QueueNameAndStatusesEmpty()
		{
			BulkUpdateBizObj.NonPersistentQueue.QueueName = "";
			BulkUpdateBizObj.NonPersistentQueue.Status = "";
			BulkUpdateBizObj.NonPersistentQueue.SubStatus = "";
			BulkUpdateBizObj.NonPersistentQueue.AssignedTo = "";
			BulkUpdateBizObj.NonPersistentQueue.Reason = "NewRemarks";
			ItemQueueToBulkUpdate.QueueName = ValidQueueNameToBulkUpdateTo;
			ItemQueueToBulkUpdate.Status = ValidStatusToBulkUpdateTo;
			ItemQueueToBulkUpdate.SubStatus = ValidSubStatusToBulkUpdateTo;
			ItemQueueToBulkUpdate.AssignedTo = "";
			ItemQueueToBulkUpdate.Reason = "";
			Factory.Save();
			Factory.ThrowOnSave = true;
			NotificationBuffer notify = new NotificationBuffer();
			BulkUpdateBizObj.BulkUpdate(notify);
			Factory.ThrowOnSave = false;
			if (ExpectedQueueNameMandatory)
			{
				AssertEquals("Expected an error due to no queue name being entered", true, BulkUpdateBizObj.NonPersistentQueue.QueueNameInfo.HasErrors());
			}
			else
			{
				AssertEquals("There should be no errors despite no queue name/reason/status entered", false, BulkUpdateBizObj.NonPersistentQueue.HasErrors);
				AssertEquals("QueueName does not need to be updated", ValidQueueNameToBulkUpdateTo, ItemQueueToBulkUpdate.QueueName);
				AssertEquals("Status does not need to be updated", ValidStatusToBulkUpdateTo, ItemQueueToBulkUpdate.Status);
				AssertEquals("SubStatus does not need to be updated", ValidSubStatusToBulkUpdateTo, ItemQueueToBulkUpdate.SubStatus);
				AssertEquals("AssignedTo does not need to be updated", "", ItemQueueToBulkUpdate.AssignedTo);
				AssertEquals("Should update Remarks", "NewRemarks", ItemQueueToBulkUpdate.Reason);
			}
		}

		public void TestBulkUpdate_UpdateReasonAndStatusEvenIfEmpty()
		{
			BulkUpdateBizObj.NonPersistentQueue.QueueName = ValidQueueNameToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.Status = "";
			BulkUpdateBizObj.NonPersistentQueue.SubStatus = "";
			ItemQueueToBulkUpdate.Status = "XX";
			ItemQueueToBulkUpdate.SubStatus = "XX";
			Factory.Save();
			Factory.ThrowOnSave = true;
			NotificationBuffer notify = new NotificationBuffer();
			BulkUpdateBizObj.BulkUpdate(notify, false);
			Factory.ThrowOnSave = false;
			AssertEquals("Should update QueueName", ValidQueueNameToBulkUpdateTo, ItemQueueToBulkUpdate.QueueName);
			AssertEquals("Should update Status", "", ItemQueueToBulkUpdate.Status);
			AssertEquals("Should update SubStatus", "", ItemQueueToBulkUpdate.SubStatus);
		}

		public void TestBulkUpdate_DontUpdateAssignedToIfEmpty()
		{
			BulkUpdateBizObj.NonPersistentQueue.QueueName = ValidQueueNameToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.Status = ValidStatusToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.SubStatus = ValidSubStatusToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.AssignedTo = "";
			ItemQueueToBulkUpdate.AssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			Factory.ThrowOnSave = true;
			NotificationBuffer notify = new NotificationBuffer();
			BulkUpdateBizObj.BulkUpdate(notify, false);
			Factory.ThrowOnSave = false;
			AssertEquals("Should update QueueName", ValidQueueNameToBulkUpdateTo, ItemQueueToBulkUpdate.QueueName);
			AssertEquals("Should update Status", ValidStatusToBulkUpdateTo, ItemQueueToBulkUpdate.Status);
			AssertEquals("Should update SubStatus", ValidSubStatusToBulkUpdateTo, ItemQueueToBulkUpdate.SubStatus);
			AssertEquals("Should NOT update AssignedTo", GlbStaff.CurrentUser.GS_Code, ItemQueueToBulkUpdate.AssignedTo);
		}

		public void TestBulkUpdate_ResetRemarksIfStatusChanges()
		{
			BulkUpdateBizObj.NonPersistentQueue.QueueName = ValidQueueNameToBulkUpdateTo;
			ItemQueueToBulkUpdate.Reason = "OriginalRemarks";
			Factory.Save();
			Factory.ThrowOnSave = true;
			NotificationBuffer notify = new NotificationBuffer();
			BulkUpdateBizObj.BulkUpdate(notify, false);
			Factory.ThrowOnSave = false;
			AssertEquals("Should reset remarks to zero-length string", "", ItemQueueToBulkUpdate.Reason);
		}

		public void TestBulkUpdate_DontResetRemarksIfStatusDoesntChange()
		{
			BulkUpdateBizObj.NonPersistentQueue.QueueName = ValidQueueNameToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.Status = ValidStatusToBulkUpdateTo;
			BulkUpdateBizObj.NonPersistentQueue.SubStatus = ValidSubStatusToBulkUpdateTo;
			ItemQueueToBulkUpdate.QueueName = ValidQueueNameToBulkUpdateTo;
			ItemQueueToBulkUpdate.Status = ValidStatusToBulkUpdateTo;
			ItemQueueToBulkUpdate.SubStatus = ValidSubStatusToBulkUpdateTo;
			ItemQueueToBulkUpdate.Reason = "OriginalRemarks";
			Factory.Save();
			Factory.ThrowOnSave = true;
			NotificationBuffer notify = new NotificationBuffer();
			BulkUpdateBizObj.BulkUpdate(notify, false);
			Factory.ThrowOnSave = false;
			AssertEquals("Status didn't change, so remarks should not be reset", "OriginalRemarks", ItemQueueToBulkUpdate.Reason);
		}

		public void TestBulkUpdate_WhenNoDataIsEntered()
		{
			NotificationBuffer notify = new NotificationBuffer();
			BulkUpdateBizObj.NonPersistentQueue.QueueName = "";
			BulkUpdateBizObj.NonPersistentQueue.Status = "";
			BulkUpdateBizObj.BulkUpdate(notify);
			AssertEquals("Correct error message to be shown to the user", true, notify.AsString.IndexOf("Error: You must enter some data to bulk update") != -1);
		}

		public void TestBulkUpdate_WhenValidationErrors()
		{
			BulkStatusUpdating bulkUpdateBizObj = GetNewBulkUpdatingBusinessObject(new IProcessQueueParent[] { ItemToBulkUpdate });
			bulkUpdateBizObj.NonPersistentQueue.Status = "|)";
			AssertHasErrors("Expected validation error for the test", bulkUpdateBizObj.NonPersistentQueue.StatusInfo);
			NotificationBuffer notify = new NotificationBuffer();
			bulkUpdateBizObj.BulkUpdate(notify);
			AssertEquals("Correct exception message to be shown to the user", "Error: There are errors that need to be corrected before you can proceed", notify.AsString.Trim());
		}

		public void TestBulkUpdate_UpdatedCountReturned()
		{
			IProcessQueueParent[] itemsToBulkUpdate = new IProcessQueueParent[25];
			for (int i = 0; i < 25; i++)
			{
				itemsToBulkUpdate[i] = GetNewItemToBulkUpdate();
				GetItemQueueToBulkUpdate(itemsToBulkUpdate[i].CurrentQueue).Reason = "OriginalRemarks";
			}

			UPEActiveProcessQueue processQueue9 = GetItemQueueToBulkUpdate(itemsToBulkUpdate[9].CurrentQueue);
			UPEActiveProcessQueue processQueue24 = GetItemQueueToBulkUpdate(itemsToBulkUpdate[24].CurrentQueue);
			processQueue9.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			processQueue9.Reason = "NewRemarks";
			processQueue24.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			processQueue24.Reason = "NewRemarks";
			Factory.Save();
			BulkStatusUpdating bizObj = GetNewBulkUpdatingBusinessObject(itemsToBulkUpdate);
			bizObj.NonPersistentQueue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			bizObj.NonPersistentQueue.Status = "";
			bizObj.NonPersistentQueue.Reason = "NewRemarks";
			NotificationBuffer notify = new NotificationBuffer();
			int updatedCount = bizObj.BulkUpdate(notify);
			AssertEquals("23 / 25 items have been updated", 23, updatedCount);
		}

		#endregion
		#region GetBulkUpdateCompletedMessage
		public void TestGetBulkUpdateCompletedMessage()
		{
			AssertEquals("No items were updated", BulkUpdateBizObj.GetBulkUpdateCompletedMessage(0, 3));
			AssertEquals("1 / 2 items successfully updated", BulkUpdateBizObj.GetBulkUpdateCompletedMessage(1, 2));
			AssertEquals("1 item successfully updated", BulkUpdateBizObj.GetBulkUpdateCompletedMessage(1, 1));
			AssertEquals("2 items successfully updated", BulkUpdateBizObj.GetBulkUpdateCompletedMessage(2, 2));
		}

		#endregion
		#region Test Classes
		protected class BusinessObjectFactoryThrowOnSave : BusinessObjectFactory
		{
			public BusinessObjectFactoryThrowOnSave()
			{
			}

			public BusinessObjectFactoryThrowOnSave(DbConnection connection) : base(connection)
			{
			}

			public bool ThrowOnSave;
			protected override IChangedTableNames SaveInTransactionCore()
			{
				if (ThrowOnSave)
				{
					throw new InvalidOperationException("The bulk update save should occur in a different factory because the items are in the grid");
				}

				return base.SaveInTransactionCore();
			}
		}

		#endregion
		#region Implementation
		new protected BusinessObjectFactoryThrowOnSave Factory = new BusinessObjectFactoryThrowOnSave();
		IProcessQueueParent ItemToBulkUpdate
		{
			get
			{
				return fItemToBulkUpdate;
			}

			set
			{
				fItemToBulkUpdate = value;
				fItemQueueToBulkUpdate = null;
			}
		}

		protected UPEActiveProcessQueue ItemQueueToBulkUpdate
		{
			get
			{
				if (fItemQueueToBulkUpdate == null && ItemToBulkUpdate != null)
				{
					fItemQueueToBulkUpdate = GetItemQueueToBulkUpdate(ItemToBulkUpdate.CurrentQueue);
				}

				return fItemQueueToBulkUpdate;
			}
		}

		UPEActiveProcessQueue GetItemQueueToBulkUpdate(ProcessQueue queue)
		{
			UPEActiveProcessQueue result = new UPEActiveProcessQueue(queue);
			result.QueueType = ExpectedQueueTypeToBulkUpdate;
			return result;
		}

		IProcessQueueParent fItemToBulkUpdate;
		UPEActiveProcessQueue fItemQueueToBulkUpdate;
		protected BulkStatusUpdating BulkUpdateBizObj;
		protected abstract ZString ValidQueueNameToBulkUpdateTo { get; }

		protected abstract ZString ValidStatusToBulkUpdateTo { get; }

		protected abstract ZString ValidSubStatusToBulkUpdateTo { get; }

		protected abstract IQueueFilterBusinessObject GetNewQueueFilterBusinessObject();
		protected abstract IProcessQueueParent GetNewItemToBulkUpdate();
		protected abstract ProcessQueueType.Enum ExpectedQueueTypeToBulkUpdate { get; }

		protected abstract Type ExpectedNonPersistentProcessQueueType { get; }

		protected abstract bool ExpectedAssignedToEnabled { get; }

		protected virtual bool ExpectedQueueNameMandatory
		{
			get
			{
				return false;
			}
		}

		protected virtual BulkStatusUpdating GetNewBulkUpdatingBusinessObject(IProcessQueueParent[] itemsToBulkUpdate)
		{
			return BulkStatusUpdating.New(GetNewQueueFilterBusinessObject(), itemsToBulkUpdate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBulkUpdatingBusinessObject(Array.Empty<IProcessQueueParent>());
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			ItemToBulkUpdate = GetNewItemToBulkUpdate();
			Factory.Save();
			BulkUpdateBizObj = GetNewBulkUpdatingBusinessObject(new IProcessQueueParent[] { ItemToBulkUpdate });
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
		}
		#endregion
	}
}
