using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IReceptionMessageDataProvider : IESEDIMessageCollectionProvider
	{
		IReceptionHeader Header { get; }
		IReadOnlyCollection<IT2LLineCommon> Lines { get; }
	}

	public interface IReceptionHeader : IT2LHeaderCommon
	{
		ZString ReceptionCustomsOffice { get; }
		ZString ReceptionT2LReference { get; }
		ZDateTime ExpeditionDate { get; }
	}
}
