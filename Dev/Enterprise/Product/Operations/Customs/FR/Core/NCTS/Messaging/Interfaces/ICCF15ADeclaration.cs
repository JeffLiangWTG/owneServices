using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface ICCF15ADeclaration : EU.NCTS.Messaging.ICCF15ADeclaration
	{
		ZString AgreementNumber { get; }
	}
}
