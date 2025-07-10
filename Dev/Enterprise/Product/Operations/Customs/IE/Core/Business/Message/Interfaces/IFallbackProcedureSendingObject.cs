using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IFallbackProcedureSendingObject
	{
		ZDate AlternativeDateOfAcceptance { get; }
		ZString CustomsReferenceNumber { get; }
		ZString CustomsJustification { get; }
	}
}
