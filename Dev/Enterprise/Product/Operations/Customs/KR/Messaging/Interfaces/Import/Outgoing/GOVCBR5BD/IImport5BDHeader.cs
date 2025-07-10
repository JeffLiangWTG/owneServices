using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5BDHeader : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		ZString RequestReason { get; }
		ZString SecurityType { get; }
		ZDate SecurityStartDate { get; }
		ZDate SecurityEndDate { get; }
		ZDecimal SecurityAmount { get; }
		ZString OtherSecurityType { get; }
		ZString ReasonForEarlyRemoval { get; }
	}
}
