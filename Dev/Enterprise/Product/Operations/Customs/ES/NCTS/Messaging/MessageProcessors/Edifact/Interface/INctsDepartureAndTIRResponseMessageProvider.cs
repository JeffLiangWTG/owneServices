using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors
{
	public interface INctsDepartureAndTIRResponseMessageProvider : ICUSRESV921ESMessageProvider
	{
		ZString CustomsClearanceCriteria { get; }
	}
}
