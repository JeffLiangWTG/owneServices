using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Business;

public interface ISadOutgoingCustomsMessageGeneratorValuesProvider : IOutgoingCustomsMessageGeneratorValuesProvider
{
	IEnumerable<ISadCustomsMessage> GetCustomsMessageObjects();

	ICustomsMessageFountainProvider FountainProvider { get; }
}
