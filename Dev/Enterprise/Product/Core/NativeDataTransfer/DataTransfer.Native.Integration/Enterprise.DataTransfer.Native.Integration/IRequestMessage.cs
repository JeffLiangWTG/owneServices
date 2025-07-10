namespace Enterprise.DataTransfer.Native.Integration
{
	/// <summary>
	/// Request Message for EHub Service
	/// </summary>
	public interface IRequestMessage
	{
		string Message { get; set; }
		string MessageEntity { get; set; }
		string MessageID { get; set; }
		string RecipientID { get; set; }
		string RecipientType { get; set; }
		string SenderID { get; set; }
		string SenderType { get; set; }
		string SenderUsername { get; set; }
	}
}
