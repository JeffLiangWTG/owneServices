using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface ICC007ADeclaration : EU.NCTS.Messaging.ICC007ADeclaration
	{
		ZString AgreementNumber { get; }
	}
}
