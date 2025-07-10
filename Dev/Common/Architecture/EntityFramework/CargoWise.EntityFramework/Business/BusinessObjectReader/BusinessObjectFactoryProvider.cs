using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectFactoryProvider : IFactoryProvider
	{
		public BusinessObjectFactoryProvider()
		{
		}

		public BusinessObjectFactoryProvider(BusinessObjectFactory initialFactory)
		{
			current = initialFactory;
		}

		public BusinessObjectFactoryProvider(DbConnection connection) : this(new BusinessObjectFactory(connection))
		{
		}

		public BusinessObjectFactory Current => current ?? (current = new BusinessObjectFactory());

		public void SaveCurrentAndUpdateRecordCounts()
		{
			if (current != null)
			{
				UpdateRecordCounts(current);
				SaveCurrent();
			}
		}

		protected virtual void SaveCurrent()
		{
			current.Save();
		}

		public void SaveCurrentAndCreateNew()
		{
			CreateNew(true, false);
		}

		public void SaveCurrentReclaimMemoryAndCreateNew()
		{
			CreateNew(true, true);
		}

		public void CreateNewWithoutSave()
		{
			CreateNew(false, false);
		}

		public void CreateNewAndReclaimMemoryWithoutSave()
		{
			CreateNew(false, true);
		}

		public void RemoveCurrent()
		{
			current = null;
		}

		public event EventHandler<EventArgs> CurrentFactoryChanged;

		void CreateNew(bool save, bool reclaimMemory)
		{
			if (current != null)
			{
				if (!current.IsCleanedUp)
				{
					UpdateRecordCounts(current);
					if (save)
					{
						SaveCurrent();
					}
				}
				current = CreateNew(reclaimMemory);
				OnCurrentFactoryChanged();
			}
		}

		protected virtual BusinessObjectFactory CreateNew(bool reclaimMemory)
		{
			var wasValidationSuspended = Current.IsValidationSuspended;
			var refreshEnabled = Current.RefreshEnabled;
			var shouldPerformFullQueryCacheCleanOnSave = Current.ShouldPerformFullQueryCacheCleanOnSave;
			var previousNameForDebugging = current.NameForDebugging;

			var newCurrent = GetNewFactoryFromOldFactory(current);
			newCurrent.NameForDebugging = previousNameForDebugging;

			if (reclaimMemory)
			{
				GCWrapper.ReclaimMemory(ref current);
			}

			if (wasValidationSuspended)
			{
				newCurrent.SuspendValidation();
			}

			newCurrent.RefreshEnabled = refreshEnabled;
			newCurrent.ShouldPerformFullQueryCacheCleanOnSave = shouldPerformFullQueryCacheCleanOnSave;

			return newCurrent;
		}

		protected virtual BusinessObjectFactory GetNewFactoryFromOldFactory(BusinessObjectFactory old)
		{
			return old.CreateNewFactory();
		}

		void OnCurrentFactoryChanged()
		{
			CurrentFactoryChanged?.Invoke(this, EventArgs.Empty);
		}

		public int RecordsAdded => fRecordsAdded;

		public int RecordsModified => fRecordsModified;

		int fRecordsAdded;
		int fRecordsModified;

		#region Implementation

		BusinessObjectFactory current;

		protected internal bool HasCurrentFactory => current != null;

		protected void UpdateRecordCounts()
		{
			if (current != null)
			{
				UpdateRecordCounts(current);
			}
		}

		void UpdateRecordCounts(BusinessObjectFactory factory)
		{
			fRecordsAdded += CountRowStatesInDataSet(((INeedDataSet)factory).Data, DataRowState.Added);
			fRecordsModified += CountRowStatesInDataSet(((INeedDataSet)factory).Data, DataRowState.Modified);
		}

		#region CountRowStatesInFactories

		int CountRowStatesInDataSet(DataSet data, DataRowState rowState)
		{
			int result = 0;
			foreach (DataTable table in data.Tables)
			{
				foreach (DataRow row in table.Rows)
				{
					if (row.RowState == rowState)
					{
						result++;
					}
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region IFactoryProvider Members

		BusinessObjectFactory IFactoryProvider.Factory => Current;

		#endregion
	}
}
