using System.Collections.Immutable;

namespace Enterprise.Customs.IE.Business
{
	public interface IEMCSResponseMessageDetails
	{
		ResponseDetail GetResponseDetail(string messageType, string messageText = null);
		ImmutableDictionary<string, ResponseDetail[]> ResponseDetails { get; }
		ResponseDetail AcknowledgementResponseDetail { get; }
	}
}
