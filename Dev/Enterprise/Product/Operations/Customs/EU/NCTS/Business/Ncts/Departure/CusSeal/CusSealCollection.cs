using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusSealCollection : DependentBusinessObjectCollection<CusSeal, BusinessObject>
	{
		public CusSealCollection(NctsDepartureHeaderContainer master) : base(master)
		{
			if (master.Header is NctsHeader nctsHeader
				&& nctsHeader.IsPhase5Departure
				&& nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleTR0025Active)
			{
				this.EnableMaxCountValidationWithMessageError(maxAdditionalSealCount, warnAtHalfway: false, Res.GetString("B20089CC-C403-4E6B-BA7F-8874ADAF6509", "[{0}] The maximum number of {1} Additional Seals has been exceeded.", ValidationRuleCodeConstants.TR0025, maxSealCount));
			}
		}

		public CusSealCollection(NctsArrivalHeaderContainer master) : base(master)
		{ }

		public CusSealCollection(NctsHeader master) : base(master)
		{ }

		public CusSealCollection(EnRouteIncident master) : base(master)
		{ }

		public CusSealCollection(NctsContainer master) : base(master)
		{
			var incident = master.Parent as EnRouteIncident;
			if (incident?.Header is NctsHeader header
				&& header.IsPhase5
				&& header.Configuration.ValidationRuleConfiguration.IsRuleTR0016Active)
			{
				this.EnableMaxCountValidationWithMessageError(maxAdditionalSealCount, warnAtHalfway: false, Res.GetString("95DFAFE5-D641-4A9E-A815-2E9B60788055", "[{0}] Max. Number of Seals per Container/Equipment is limited to {1}.", ValidationRuleCodeConstants.TR0016, maxSealCount));
			}
		}

		readonly int maxSealCount = 99;
		readonly int maxAdditionalSealCount = 97;

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent => CusSealSchema.BK_ParentID;

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			var seal = (CusSeal)base.CreateBusinessObjectFromRow(row);
			seal.ParentType = Master.GetType();
			return seal;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			var seal = (CusSeal)dependent;
			seal.ParentType = Master.GetType();
			base.SetCollectionRelationships(dependent);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var seal = (CusSeal)child;
			if (Master is NctsDepartureHeaderContainer departureContainer)
			{
				departureContainer.AdditionalSealsLineNumberGenerator.RecalculateWhenAdded(seal);
			}
			else if (Master is NctsArrivalHeaderContainer arrivalContainer)
			{
				arrivalContainer.SealsLineNumberGenerator.RecalculateWhenAdded(seal);
			}
			else
			{
				seal.BK_SequenceNumber = Count == 0 ? (ZShort)1 : Select(x => x.BK_SequenceNumber).Max() + 1;
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			((CusSeal)bizOAdded).ParentType = Master.GetType();
			base.OnAdded(bizOAdded);
			if (Master is NctsDepartureHeaderContainer departureContainer)
			{
				departureContainer.MarkAsNeedingValidation();
			}
			if (Master is NctsArrivalHeaderContainer arrivalContainer)
			{
				arrivalContainer.MarkAsNeedingValidation();
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (Master is NctsDepartureHeaderContainer departureContainer)
			{
				departureContainer.AdditionalSealsLineNumberGenerator.ReCalculateAll();
			}
			else if (Master is NctsArrivalHeaderContainer arrivalContainer)
			{
				arrivalContainer.SealsLineNumberGenerator.ReCalculateAll();
			}
		}
	}
}
