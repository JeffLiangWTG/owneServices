namespace Enterprise.Interop.OutlookIntegration
{
	public interface IOutlookMailItem
	{
		object CustomData { get; }
		string FileNameToSaveOnSend { get; set; }
		void AddRecipient(string recipient);
		void AddReplyRecipient(string replyRecipient);
		void AddAttachment(string fileName);
		void Display(bool modal);
		string Subject { get; set; }
		string Body { get; set; }
		event OutlookMailItemSendEventHandler MailItemSend;
	}

	public delegate void OutlookMailItemSendEventHandler(object data);
}
