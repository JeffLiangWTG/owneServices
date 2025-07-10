using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IDepartureCusTransportMeansCollection<out T> : IDependentBusinessObjectCollection, IBusinessObjectCollection<T>
		where T : DepartureCusTransportMeans
	{
		new T this[int index] { get; }

		new T AddNew();
	}

	public class DepartureCusTransportMeansCollection<T> : DependentBusinessObjectCollection<T, BusinessObject>, IDepartureCusTransportMeansCollection<T>, ISequenceNumberHeader
		where T : DepartureCusTransportMeans
	{
		public DepartureCusTransportMeansCollection(NctsBill master) : base(master)
		{
			MaxCountValidationEnable(MaxAllowedLines);
		}

		public DepartureCusTransportMeansCollection(NctsDepartureMovementHeader master) : base(master)
		{
			MaxCountValidationWithMessageErrorEnable(MaxAllowedLines, Res.GetString("D7BFEF1D-E5DB-4BD5-8F73-45B47A18664C", "[R0789-2] If a Customs Office of Transit is present then the multiplicity of additional Active Border Transport Means must be up to {0}", MaxAllowedLines));
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (Master is NctsBill bill && (!bill.Header?.IsDepartureMovement ?? true))
			{
				result.AddToFilter(ZQuery.NoResultQuery);
			}
			return result;
		}

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent => CusTransportMeansSchema.TPM_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var cusTransportMeans = (DepartureCusTransportMeans)child;
			SequenceGenerator.RecalculateWhenAdded(cusTransportMeans);
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var transportMeans = (DepartureCusTransportMeans)dependent;
			transportMeans.TPM_ParentTableCode = Master.TablePrefix;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (Master is NctsDepartureMovementHeader departureHeader)
			{
				departureHeader.MarkAsNeedingValidation();
			}
		}

		int MaxAllowedLines => Master is NctsBill ? 999 : 8;

		protected override bool AllowNewCore => Count < MaxAllowedLines;

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			if (!bizO.IsDeleting && bizO is DepartureCusTransportMeans removedLine && removedLine.TPM_SequenceNumber >= 3)
			{
				SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(removedLine);
			}
		}

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ?? (sequenceGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator sequenceGenerator;

		public IEnumerable<ISequenceNumberLine> Lines => this.Cast<DepartureCusTransportMeans>();
	}
}
