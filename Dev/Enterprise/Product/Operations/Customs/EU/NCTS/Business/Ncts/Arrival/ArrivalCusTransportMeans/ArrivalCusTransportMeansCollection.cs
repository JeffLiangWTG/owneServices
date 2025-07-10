using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IArrivalCusTransportMeansCollection<out T> : IBusinessObjectCollection<T>, ISequenceNumberHeader
		where T : ArrivalCusTransportMeans
	{
		new T this[int index] { get; }

		new T AddNew();
		void SetReadOnlyIncludingChildren(bool readOnly);
		void RefreshBinding();
	}

	public sealed class ArrivalCusTransportMeansCollection<T> : DependentBusinessObjectCollection<T, BusinessObject>
		, IArrivalCusTransportMeansCollection<T>
		where T : ArrivalCusTransportMeans
	{
		public ArrivalCusTransportMeansCollection(NctsBill master) : base(master)
		{
		}

		public ArrivalCusTransportMeansCollection(NctsArrivalMovementHeader master) : base(master)
		{
		}

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent => CusTransportMeansSchema.TPM_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var cusTransportMeans = (T)child;
			SequenceGenerator.RecalculateWhenAdded(cusTransportMeans);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			SequenceGenerator.ReCalculateAll();
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var transportMeans = (T)dependent;
			transportMeans.TPM_ParentTableCode = Master.TablePrefix;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();

			if (Master is NctsBill bill && (!bill.Header?.IsArrivalMovement ?? true))
			{
				result.AddToFilter(ZQuery.NoResultQuery);
			}
			return result;
		}

		ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ?? (sequenceGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator sequenceGenerator;

		public IEnumerable<ISequenceNumberLine> Lines => this;

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => Elements.Cast<T>().GetEnumerator();
	}
}
