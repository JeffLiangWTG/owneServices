using System.Collections.Immutable;

namespace Enterprise.Customs.IE.Business;

public interface IPBNResponseMessageDetails
{
	ResponseDetail GetResponseDetail(string messageType, string messageText);
	ImmutableDictionary<string, ResponseDetail> ResponseDetails { get; }
}
