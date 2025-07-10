namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using CargoWise.Types;
	using Enterprise.Customs.CA.Messaging;

	public interface IACIForwarderMessageProvider : ICAEDIFACTMessageAttachee
	{
		bool IsPostArrival { get; }
		ZString AmendmentReason { get; }
	}
}
