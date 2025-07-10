using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class BusinessObjectReader : IEnumerable<BusinessObject>
	{
		protected BusinessObjectReader(BusinessObjectFactoryProvider factoryProvider)
		{
			this.FactoryProvider = factoryProvider;
		}

		public BusinessObjectFactory Factory
		{
			get
			{
				return FactoryProvider.Current;
			}
		}

		public BusinessObject[] LoadNextBatchInANewFactory(ZGuid lastBusinessObjectReadPK)
		{
			if (SaveBeforeLoadNextEnabled)
			{
				FactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
			}
			else
			{
				FactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
			}
			return LoadNextBatchCore(lastBusinessObjectReadPK);
		}

		public BusinessObject[] LoadNextBatchInANewFactory(BusinessObject lastBusinessObjectRead)
		{
			if (SaveBeforeLoadNextEnabled)
			{
				FactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
			}
			else
			{
				FactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
			}
			return LoadNextBatchCore(lastBusinessObjectRead);
		}

		public bool SaveBeforeLoadNextEnabled;

		public virtual int ApproximateCount
		{
			get { return -1; }
		}

		public abstract bool HasRecords
		{
			get;
		}

		public abstract Type BusinessObjectType
		{
			get;
		}

		protected abstract BusinessObject[] LoadNextBatchCore(ZGuid pk);
		protected abstract BusinessObject[] LoadNextBatchCore(BusinessObject lastBusinessObjectRead);

		public BusinessObjectFactoryProvider FactoryProvider { get; private set; }

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		IEnumerator<BusinessObject> IEnumerable<BusinessObject>.GetEnumerator() => GetEnumerator();

		public virtual BusinessObjectEnumerator GetEnumerator()
		{
			return new BusinessObjectEnumerator(LoadNextBatchInANewFactory);
		}

		#endregion

		#region BusinessObjectEnumerator

		public delegate T[] GetNextBatch<T>(BusinessObject lastBizo);

		public sealed class BusinessObjectEnumerator : IEnumerator<BusinessObject>
		{
			internal BusinessObjectEnumerator(GetNextBatch<BusinessObject> getNextBatch)
			{
				this.getNextBatch = getNextBatch;
				Reset();
			}

			#region IEnumerator Members

			public void Reset()
			{
				currentBatch = Array.Empty<BusinessObject>();
				IndexOfBatchArray = 0;
			}

			public BusinessObject Current
			{
				get { return currentBatch[IndexOfBatchArray]; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public bool MoveNext()
			{
				IndexOfBatchArray++;
				if (CurrentBatch == null || IndexOfBatchArray >= currentBatch.Length)
				{
					IndexOfBatchArray = 0;

					BusinessObject lastBusinessObjectRead = currentBatch.Length == 0 ? null : currentBatch[currentBatch.Length - 1];
					currentBatch = getNextBatch(lastBusinessObjectRead);
				}
				return currentBatch.Length > 0;
			}

			public void Dispose()
			{
				// Do nothing. Nothing to dispose.
			}

			#endregion

			public IEnumerable<BusinessObject> CurrentBatch => currentBatch;
			BusinessObject[] currentBatch;
			readonly GetNextBatch<BusinessObject> getNextBatch;
			int IndexOfBatchArray;
		}

		#endregion
	}
}
