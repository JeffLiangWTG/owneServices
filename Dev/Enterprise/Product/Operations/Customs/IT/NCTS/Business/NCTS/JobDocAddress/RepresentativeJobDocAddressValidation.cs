using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class RepresentativeJobDocAddressValidation : TraderJobDocAddressValidation
{
	public RepresentativeJobDocAddressValidation(AutoJobDocAddress parent, NctsDepartureMovementHeader departureMovement)
		: base(parent, ValidationCaptions.Shared.RepresentativeCaption, departureMovement, isMandatory: true)
	{
		this.departureMovement = Argument.NotNull(departureMovement, nameof(departureMovement));
	}
	readonly NctsDepartureMovementHeader departureMovement;

	protected override void CheckOrganisationPK()
	{
		if (departureMovement.IsPhase5Departure || departureMovement.IsTIRDeclaration)
		{
			return;
		}

		base.CheckOrganisationPK();
	}

	protected override ZString[] GetRequiredCodeTypeListForEuropeanTrader(OrgHeader organisation) => organisation.GetCustomsCodeTypeListRequiredForEUTrader(ignoreEoriCusCode: true);

	protected override string GetMissingCustomsCodeMessageError() => !departureMovement.IsPhase5Departure ? ValidationCaptions.TraderJobDocAddressValidation.VatCodeOrFiscalCodeIsRequired : string.Empty;
}
