namespace Enterprise.DataTransfer.Native.Integration
{
	public interface IResponseMessage
	{
		string ErrorMessage { get; set; }
		bool HasError { get; set; }
		string MessageID { get; set; }
		string ResponseMessage { get; set; }
	}
}
