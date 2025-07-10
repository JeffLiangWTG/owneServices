using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class ConsigneeJobDocAddressValidation : JobDocAddressValidation
	{
		public ConsigneeJobDocAddressValidation(AutoJobDocAddress parent, NctsHeader nctsHeader)
			: base(parent)
		{
			NctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		protected NctsHeader NctsHeader { get; }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			var validationDecider = NctsHeader.ValidationDecider as INctsHeaderDeparturePhase5ValidationDecider;
			if (validationDecider is null)
			{
				return;
			}

			var isRuleB1823Applicable = validationDecider.IsRuleB1823Active
				&& NctsHeader.IsInPhase5TransitionPeriod;

			if (validationDecider.IsRuleC0001Active
				&& !isRuleB1823Applicable)
			{
				CheckRuleC0001();
			}
		}

		void CheckRuleC0001()
		{
			if (Parent.IsEmpty
				&& NctsHeader.MovementHeader?.BM_RL_NKDestinationPort is ZString destinationCountry
				&& !destinationCountry.IsEmpty
				&& IsRelevantConsigneeEmpty()
				&& NctsHeaderValidationHelper.IsNctsContractingParty(destinationCountry))
			{
				Parent.OrganisationPKInfo.AddMessageError(Res.GetString("272372BE-23B1-4FA3-AAC0-5013505C8B9D", "[C0001] You have not entered Consignee. It is required either on Declaration or House Consignment."));
			}
		}

		protected abstract bool IsRelevantConsigneeEmpty();
	}
}
