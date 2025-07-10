using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteIncidentCollection : CusInBondEventCollection<EnRouteIncident>
	{
		public EnRouteIncidentCollection(NctsHeader master) : base(master, CusInBondEventTypes.Codes.Incident)
		{
			SetReadOnlyWhenOutsideTransitionPeriod();

			if (master.IsPhase5 && master.Configuration.ValidationRuleConfiguration.IsRuleTR0011Active)
			{
				this.EnableMaxCountValidationWithMessageError(9, warnAtHalfway: false, Res.GetString("AF8A7EB5-D555-48B2-8150-86874B29F753", "[TR0011] Only 9 Incidents are allowed."));
			}
		}

		void SetReadOnlyWhenOutsideTransitionPeriod()
		{
			if (Relationship.Master is NctsHeader nctsHeader && nctsHeader.IsPhase5 && !nctsHeader.IsInPhase5TransitionPeriod)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		protected override void SetDefaultsForNewElementCore(EnRouteIncident newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (newElement.IsPhase5)
			{
				newElement.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			}
		}

		public ZBool AnyCreatedByCustomsMessage => this.Any(x => x.BN_CustomsStatus == IncidentCustomsStatusList.Codes.CUS);
	}
}
