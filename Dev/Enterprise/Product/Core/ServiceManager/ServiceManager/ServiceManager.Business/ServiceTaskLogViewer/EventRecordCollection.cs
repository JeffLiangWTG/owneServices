using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ServiceManager.Business
{
	public class EventRecordCollection : NonPersistentBusinessObjectCollection<EventRecord>
	{
		public EventRecordCollection() { }

		public override void RemoveAndDeleteAll()
		{
			using (SuspendListChanged())
			{
				foreach (var element in Elements.Reverse())
				{
					RemoveCollectionRelationships(element, true);
					UnHookElementFromCollection(element);
					RemoveFromElements(element, false);
				}
			}
		}

		public void Load(Stream stream)
		{
			RemoveAndDeleteAll();

			using (SuspendListChanged())
			{
				try
				{
					using (var sr = new StreamReader(stream))
					{
						var stringInterner = new StringInterner();
						string line;
						var sequenceId = 0;
						while ((line = sr.ReadLine()) != null)
						{
							var er = EventRecord.FromString(stringInterner, line);
							er.SequenceNumber = sequenceId++;
							Add(er);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Add(new EventRecord(ZDateTime.UtcNow, nameof(LogType.Error), "LogViewer Exception: " + ex.Message, 0, null, 0));
				}

				var bindingListView = (IBindingListView)this;
				if (bindingListView.SortDescriptions != null && bindingListView.SortDescriptions.Count > 0)
				{
					bindingListView.ApplySort(bindingListView.SortDescriptions);
				}
			}
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EventRecord(ZDateTime.UtcNow, ZString.Empty, ZString.Empty, 0, null, 0);
		}

		#endregion
	}
}

