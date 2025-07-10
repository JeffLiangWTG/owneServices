using System.Collections.Generic;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface ITIRLine : INctsBaseDepartureLineMessageProvider
	{
		ITIRInternalPackagesInfo InternalPackages { get; }
		IReadOnlyCollection<IDocumentsCommon> Documents { get; }
	}
}
