using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface ICC015BDeclaration : EU.NCTS.Messaging.ICC015BDeclaration
	{
		ZString NatureOfSeals { get; }
		ZString AgreementNumber { get; }
		ZBool IsPrelodge { get; }
	}
}
