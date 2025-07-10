using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface ICC014ADeclaration : EU.NCTS.Messaging.ICC014ADeclaration
	{
		ZString AgreementNumber { get; }
		JustificationReglementaireInvalidation CancellationRegularJustification { get; }
		ZString CancellationDate { get; }
	}
}
