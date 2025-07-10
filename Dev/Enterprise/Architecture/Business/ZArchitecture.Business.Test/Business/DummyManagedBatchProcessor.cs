using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyManagedBatchProcessor : ManagedBatchProcessor<DummyBusinessObject>
	{
		public Action<DummyBusinessObject> ProcessRow_Override;
		public Action<DummyBusinessObject, (int, int)> ProcessRowWithIndex_Override;
		public Func<BusinessObjectFactory, IList<DummyBusinessObject>, object, IDisposable> WithBatch_Override;
		public Action<INotifications, DummyBusinessObject, BusinessObjectFactory> MarkRowAsBad_Override;

		public int DisposeTrackingVariable { get; set; }

		public override int BatchSize => BatchSizeOverride ?? base.BatchSize;
		public int? BatchSizeOverride;
		public IBatchGrouper GrouperOverride { get; set; }

		protected override ZQuery GetQuery() => new ZQuery(DummyBizoSchema.Z0_Number, 0);
		protected override ZQuery GetSingularQuery(DummyBusinessObject row) => new ZQuery(DummyBizoSchema.PK, row.PK);
		protected override void MarkRowAsBadCore(INotifications notifications, DummyBusinessObject row, BusinessObjectFactory factory)
		{
			MarkRowAsBad_Override?.Invoke(notifications, row, factory);
			row.Z0_Number = 2;
		}
		protected override IBatchGrouper GetGrouper() => GrouperOverride ?? base.GetGrouper();

		protected override IDisposable WithBatch(BusinessObjectFactory factory, IList<DummyBusinessObject> batch, object groupKey)
		{
			if (WithBatch_Override != null)
			{
				return WithBatch_Override.Invoke(factory, batch, groupKey);
			}
			else
			{
				return base.WithBatch(factory, batch, groupKey);
			}
		}

		protected override IList<DummyBusinessObject> LoadBatchCore(BusinessObjectFactory factory, ZQuery query)
		{
			DisposeTrackingVariable++;
			return factory.Load<DummyBusinessObject>(query);
		}

		protected override void OnAfterBatch(IList<DummyBusinessObject> batch)
		{
			DisposeTrackingVariable--;
		}

		protected override void ProcessRowCore(INotifications notifications, CancellationToken token, DummyBusinessObject row, BusinessObjectFactory factory, (int, int) index)
		{
			ProcessRowWithIndex_Override?.Invoke(row, index);
			ProcessRow_Override?.Invoke(row);

			row.Z0_Number = 1;
		}
	}
}
