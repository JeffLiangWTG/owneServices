using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	public interface IDialog
	{
		string TransmissionCode { get; }
		string ResponseCode { get; }
		string TransmittedMessageText { get; }
		StmALog[] Logs { get; }
	}
}
