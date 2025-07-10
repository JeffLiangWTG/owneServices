using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface ICC141ADeclaration : EU.NCTS.Messaging.ICC141ADeclaration
	{
		ZString AgreementNumber { get; }
		ZBool IsTC11DeliveredByCustoms { get; }
		ZString TC11Date { get; }
		ZString QueryInformation { get; }
		ZBool IsQueryAvailableOnPaper { get; }
	}
}
