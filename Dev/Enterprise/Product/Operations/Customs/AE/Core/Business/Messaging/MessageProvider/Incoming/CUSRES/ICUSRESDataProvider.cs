using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

public interface ICUSRESDataProvider : IInboundMessageDataProvider
{
	ZString DocumentIdentifier { get; }

	ZString EntryStatus { get; }

	IReadOnlyCollection<IInformationRequest> InformationRequests { get; }

	bool IsParsed { get; }
}

public interface IInformationRequest
{
	ZString ErrorSegment { get; }

	ZString RequestType { get; }

	IReadOnlyCollection<ZString> ResponseDetails { get; }
}
