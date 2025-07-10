using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public interface INCTSResponseMessageDetails
	{
		ResponseDetail GetResponseDetail(string messageType);
		ImmutableDictionary<string, ResponseDetail> ResponseDetails { get; }
		ResponseDetail AcknowledgementResponseDetail { get; }
		int CompareMessageType(ZString messageXType, ZString messageYType);
	}
}
