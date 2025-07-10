using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IAnnexNCTSMessageDataProvider : INCTSCommonDataProvider
	{
		INCTSCommonTransitOperationMRN TransitOperation { get; }
		ZString DispatchRequestCode { get; }
		IReadOnlyCollection<IAnnexDocCommon> Documents { get; }
	}
}
