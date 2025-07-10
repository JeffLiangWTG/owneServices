
namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class NctsDepartureMovementHeaderValidation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation
{
	public NctsDepartureMovementHeaderValidation(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	protected override bool ShouldCheckGuaranteeForTIRDeclaration => false;

	protected override void CheckBM_TOLCarrierCode()
	{
	}
}
