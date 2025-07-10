using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Module
{
	public abstract class BulkStatusUpdating : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected BulkStatusUpdating(BusinessObjectFactory factory, IProcessQueueParent[] itemsToBulkUpdate)
			: base(factory)
		{
			this.ItemsToBulkUpdate = itemsToBulkUpdate;
		}

		public readonly IProcessQueueParent[] ItemsToBulkUpdate;

		public static BulkStatusUpdating New(IQueueFilterBusinessObject filterBizObj, IProcessQueueParent[] itemsToBulkUpdate)
		{
			BulkStatusUpdating result;
			if (filterBizObj is CalloutFilterBusinessObject)
			{
				result = new CalloutBulkStatusUpdating(filterBizObj.Factory, itemsToBulkUpdate);
			}
			else if (filterBizObj is EnquiryFilterBusinessObject)
			{
				result = new ForceToCalloutBulkStatusUpdating(filterBizObj.Factory, itemsToBulkUpdate);
			}
			else if (filterBizObj is UPEAirCargoFilterBusinessObject)
			{
				result = new AirCargoBulkStatusUpdating(filterBizObj.Factory, itemsToBulkUpdate);
			}
			else if (filterBizObj is UPEJobDeclarationFilterBusinessObject)
			{
				result = new JobDecBulkStatusUpdating(filterBizObj.Factory, itemsToBulkUpdate);
			}
			else
			{
				throw new ArgumentException("FilterBizObj", "Unknown type of filter business object '" + filterBizObj.GetType().FullName + "'");
			}
			return result;
		}

		#region BulkUpdate

		public int BulkUpdate(INotifications notify)
		{
			int result = 0;
			try
			{
				result = BulkUpdate(notify, true);
			}
			catch (ZSaveConcurrencyException)
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, "Another user or the system updated one or more of these shipments while attempting to bulk update them. Try again later."));
			}
			return result;
		}

		internal int BulkUpdate(INotifications notify, bool validate)
		{
			int updatedCount = 0;
			if (CheckCanBulkUpdate(validate, notify))
			{
				Queue itemQueue = new Queue(ItemsToBulkUpdate);
				BusinessObjectFactory bulkUpdateFactory = new BusinessObjectFactory();
				updatedCount = BulkUpdateToFactory(bulkUpdateFactory, itemQueue);

				if (Globals.IsDebugMode)
				{
					OnBeforeBulkUpdateSaveDebugMode(bulkUpdateFactory);
				}

				bulkUpdateFactory.Save();
				notify.Notify(new InfoNotification(GetBulkUpdateCompletedMessage(updatedCount, ItemsToBulkUpdate.Length)));
			}
			return updatedCount;
		}

		protected virtual void OnBeforeBulkUpdateSaveDebugMode(BusinessObjectFactory factory)
		{
			if (BeforeBulkUpdateSave != null)
			{
				BeforeBulkUpdateSave(this, EventArgs.Empty);
			}
		}

		public event EventHandler BeforeBulkUpdateSave;

		int BulkUpdateToFactory(BusinessObjectFactory bulkUpdateFactory, Queue itemQueue)
		{
			int updatedCount = 0;
			while (itemQueue.Count > 0)
			{
				List<ZGuid> itemPks = new List<ZGuid>(10);
				for (int i = 0; i < 10 && itemQueue.Count > 0; i++)
				{
					IProcessQueueParent next = (IProcessQueueParent)itemQueue.Dequeue();
					itemPks.Add(next.PK);
				}
				ZQuery filter = new ZQuery(ItemPKColumn, itemPks);
				updatedCount += BulkUpdateToFactory(bulkUpdateFactory, filter);
			}
			return updatedCount;
		}

		int BulkUpdateToFactory(BusinessObjectFactory bulkUpdateFactory, ZQuery filter)
		{
			int updatedCount = 0;
			IProcessQueueParent[] items = (IProcessQueueParent[])bulkUpdateFactory.Load(ItemType, filter);
			foreach (IProcessQueueParent item in items)
			{
				UpdateItem(item);
				if (item.HasChanges)
				{
					updatedCount++;
				}
			}
			return updatedCount;
		}

		protected virtual void UpdateItem(IProcessQueueParent item)
		{
			UPEActiveProcessQueue queueToUpdate = new UPEActiveProcessQueue(item.CurrentQueue);
			queueToUpdate.QueueType = QueueTypeToUpdate;
			if (!QueueName.IsEmpty)
			{
				if (QueueName != queueToUpdate.QueueName ||
					Status != queueToUpdate.Status ||
					SubStatus != queueToUpdate.SubStatus)
				{
					queueToUpdate.QueueName = QueueName;
					queueToUpdate.Status = Status;
					queueToUpdate.SubStatus = SubStatus;
					queueToUpdate.Reason = "";
				}
			}
			if (!AssignedTo.IsEmpty)
			{
				queueToUpdate.AssignedTo = AssignedTo;
			}
			if (!Reason.IsEmpty)
			{
				queueToUpdate.Reason = Reason;
			}
		}

		bool CheckCanBulkUpdate(bool validate, INotifications notify)
		{
			bool result = true;
			if (validate)
			{
				RunPreSaveValidation();
				if (HasErrors)
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, "There are errors that need to be corrected before you can proceed"));
					result = false;
				}
			}
			if (QueueName.IsEmpty && Status.IsEmpty && SubStatus.IsEmpty && AssignedTo.IsEmpty && Reason.IsEmpty)
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, "You must enter some data to bulk update"));
				result = false;
			}
			return result;
		}

		protected abstract ProcessQueueType.Enum QueueTypeToUpdate { get; }

		#endregion

		#region GetBulkUpdateCompletedMessage

		public string GetBulkUpdateCompletedMessage(int updatedCount, int selectedCount)
		{
			string result;
			if (updatedCount == 0)
			{
				result = "No items were updated";
			}
			else if (updatedCount != selectedCount)
			{
				result = updatedCount + " / " + selectedCount + " items successfully updated";
			}
			else
			{
				result = updatedCount + " item" + (updatedCount == 1 ? "" : "s") + " successfully updated";
			}
			return result;
		}

		#endregion

		#region NonPersistentProcessQueue

		ZString QueueName
		{
			get { return NonPersistentQueue.QueueName; }
		}

		ZString Status
		{
			get { return NonPersistentQueue.Status; }
		}

		ZString SubStatus
		{
			get { return NonPersistentQueue.SubStatus; }
		}

		ZString Reason
		{
			get { return NonPersistentQueue.Reason; }
		}

		ZString AssignedTo
		{
			get { return NonPersistentQueue.AssignedTo; }
		}

		public NonPersistentProcessQueue NonPersistentQueue
		{
			get
			{
				if (fNonPersistentQueue == null)
				{
					fNonPersistentQueue = GetNewNonPersistentProcessQueue();
					RegisterEditableChildObject(fNonPersistentQueue);
					((IZPropertyInfoObsolete)NonPersistentQueue.AssignedToInfo).ReadOnly = !EnableAssignedTo;
				}
				return fNonPersistentQueue;
			}
		}

		protected abstract NonPersistentProcessQueue GetNewNonPersistentProcessQueue();
		protected abstract bool EnableAssignedTo { get; }
		NonPersistentProcessQueue fNonPersistentQueue;

		#endregion

		#region Implementation

		Type ItemType
		{
			get
			{
				if (fItemType == null)
				{
					fItemType = ItemsToBulkUpdate[0].GetType();
				}
				return fItemType;
			}
		}
		Type fItemType;

		SchemaGuidColumn ItemPKColumn
		{
			get
			{
				if (fItemPKColumn == null)
				{
					string tableName = BusinessObjectFactory.GetTableNameFromType(ItemType);
					fItemPKColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(tableName);
				}
				return fItemPKColumn;
			}
		}
		SchemaGuidColumn fItemPKColumn;

		#endregion
	}
}
