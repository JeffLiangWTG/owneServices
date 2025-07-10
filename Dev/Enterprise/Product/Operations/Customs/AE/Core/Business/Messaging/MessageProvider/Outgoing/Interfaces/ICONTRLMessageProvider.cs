using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public interface ICONTRLMessageProvider : IEDIFACTMessageProvider
{
	ActionCodedList ActionCoded { get; }

	string InterchangeControlReference { get; }

	EDIMessage RequestMessage { get; }

	AEEDIMessage AddNewEDIMessage();
}
