using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class LogFileRecordCollection : NonPersistentBusinessObjectCollection<LogFileRecord>
	{
		public LogFileRecordCollection() { }

		public void Load(IEnumerable<ILogViewerDataProvider> hostLogProviderCollection)
		{
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				foreach (var logViewerDataProvider in hostLogProviderCollection)
				{
					var files = logViewerDataProvider.GetFileNames();
					var filesToAdd = new List<string>(files);

					foreach (var fileName in filesToAdd)
					{
						var record = new LogFileRecord(fileName, logViewerDataProvider.Hostname);
						record.LogViewerDataProvider = logViewerDataProvider;
						Add(record);
					}

					var bindingListView = (IBindingListView)this;
					if (bindingListView.SortDescriptions != null && bindingListView.SortDescriptions.Count > 0)
					{
						bindingListView.ApplySort(bindingListView.SortDescriptions);
					}
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
			return new LogFileRecord("", "");
		}

		#endregion
	}
}

