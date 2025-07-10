using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuaranteeCollection<T> : DependentBusinessObjectCollection<T, BusinessObject>, INctsGuaranteeCollection<T>
		where T : NctsGuarantee
	{
		public NctsGuaranteeCollection(NctsHeader header)
			: base(header)
		{
		}

		public NctsGuaranteeCollection(NctsDepartureMovementHeader departureMovementHeader)
			: base(departureMovementHeader)
		{
			if (departureMovementHeader.Header is NctsHeader nctsHeader && nctsHeader.IsPhase5Departure && nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleTR0023Active)
			{
				const int maxGuaranteesAllowed = 9;
				this.EnableMaxCountValidationWithMessageError(maxGuaranteesAllowed, warnAtHalfway: false, GetMaxCountValidationError(maxGuaranteesAllowed));
			}
		}

		public NctsGuaranteeCollection(NctsArrivalMovementHeader arrivalMovementHeader)
			: base(arrivalMovementHeader)
		{
			if (arrivalMovementHeader.Header is NctsHeader nctsHeader && nctsHeader.IsPhase5Arrival && nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleTR0023Active)
			{
				const int maxGuaranteesAllowed = 1;
				this.EnableMaxCountValidationWithMessageError(maxGuaranteesAllowed, warnAtHalfway: false, GetMaxCountValidationError(maxGuaranteesAllowed));
			}
		}

		ZString GetMaxCountValidationError(int maxGuaranteesAllowed) => Res.GetString("F5D2B9DC-37AF-4C02-9E65-919049A7AE2E", "[{0}] The maximum number of {1} Guarantees has been exceeded.", ValidationRuleCodeConstants.TR0023, maxGuaranteesAllowed);

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var guarantee = (T)child;
			guarantee.SetDefaultsAfterParentIsSet();
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.IsNoResultQuery |= Master is NctsHeader header && header.IsPhase5;
			return query;
		}

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		protected override string FkColumnName => CusBondDetailSchema.PW_ParentID.Name;
	}
}
