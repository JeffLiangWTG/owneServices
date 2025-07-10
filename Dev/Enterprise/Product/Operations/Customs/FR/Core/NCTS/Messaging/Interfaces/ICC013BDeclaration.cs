using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface ICC013BDeclaration : EU.NCTS.Messaging.ICC013BDeclaration
	{
		ZString NatureOfSeals { get; }
		ZString AgreementNumber { get; }
		ZBool PrelodgeDeclarationIndicator { get; }
		ZString AmendmentDate { get; }
	}
}
