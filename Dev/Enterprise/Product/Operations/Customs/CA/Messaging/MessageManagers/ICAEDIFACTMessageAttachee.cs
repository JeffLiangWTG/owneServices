namespace Enterprise.Customs.CA.Messaging
{
	public interface ICAEDIFACTMessageAttachee : Common.MessageBuilders.IEDIFACTMessageAttachee
	{
		bool IsCancelled { get; }
	}
}
