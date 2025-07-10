using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsDepartureMovementHeaderG0090Validation
	{
		public NctsDepartureMovementHeaderG0090Validation(NctsDepartureMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}

		public void Validate()
		{
			var carrier = movementHeader.Carrier;

			if (movementHeader.IsPhase5
				&& movementHeader.Header is NctsHeader nctsHeader
				&& nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleG0090Active
				&& nctsHeader.Principal is JobDocAddress principal
				&& carrier.Organisation.HasSameEoriOrTcu(principal.Organisation))
			{
				carrier.OrganisationPKInfo.AddWarning(
					Res.GetString(
						"E0FBD535-3779-4D97-87B0-32F2E9AA7EB9",
						"{0} Carrier EOR/TCU code used is the same as Principal EOR/TCU code used: Carrier data will not be reported in the Message.",
						ValidationRuleCodeConstants.G0090.GetRuleCodeMessagePrefix()));
			}
		}

		readonly NctsDepartureMovementHeader movementHeader;
	}
}
