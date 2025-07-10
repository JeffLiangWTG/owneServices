using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalHeaderContainerCollection : DependentBusinessObjectCollection<NctsArrivalHeaderContainer, NctsHeader>, ISequenceNumberHeader
	{
		public NctsArrivalHeaderContainerCollection(NctsHeader master)
			: base(master)
		{ }

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusInBondContainerSchema.BC_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusInBondContainerSchema.BC_TypeOfService, ContainerTypeOfServiceList.Codes.DepartureContainer);
			query.IsNoResultQuery = !Master.IsPhase5Arrival;
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var headerContainer = (NctsArrivalHeaderContainer)child;
			SequenceGenerator.RecalculateWhenAdded(headerContainer);
			if (Count > 0 && Master.IsPhase5)
			{
				headerContainer.BC_Mode = this[0].BC_Mode;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			SequenceGenerator.ReCalculateAll();
		}

		public override void Load()
		{
			if (Master.IsPhase5Arrival)
			{
				base.Load();
			}
		}

		public IEnumerable<ZString> AllSeals
		{
			get
			{
				foreach (NctsArrivalHeaderContainer cont in this)
				{
					if (!cont.BC_Seal1.IsEmpty)
					{
						yield return cont.BC_Seal1;
					}

					if (!cont.BC_Seal2.IsEmpty)
					{
						yield return cont.BC_Seal2;
					}
				}
			}
		}

		ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ??= new ShortSequenceNumberGenerator(this);
		ShortSequenceNumberGenerator sequenceGenerator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(this);

		protected override bool AllowNewCore => Master.IsPhase5Arrival;
	}
}
