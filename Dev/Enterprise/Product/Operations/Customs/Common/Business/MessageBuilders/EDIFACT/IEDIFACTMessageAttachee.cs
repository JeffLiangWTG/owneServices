namespace Enterprise.Customs.Common.MessageBuilders
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Messaging.Business;

	public interface IEDIFACTMessageAttachee : IEDIMessageCollectionProvider
	{
		void AddMessage(EDIMessage message);
		ZString MessageStatus { get; set; }
		ZString JobStatus { get; set; }
		bool HasChanges { get; }
		ZString JobIdentification { get; }
		bool RefreshValidationBeforeSendMessage { get; }
		BusinessObject TopLevelBusinessObject { get; }
	}
}
