using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IExpeditionAmendmentMessageDataProvider : IESEDIMessageCollectionProvider
	{
		IExpeditionAmendmentHeader Header { get; }
		IReadOnlyCollection<IExpeditionLine> Lines { get; }
	}

	public interface IExpeditionAmendmentHeader : IExpeditionHeader
	{
		ZString ExpeditionT2LReference { get; }
	}

	public interface IReceptionAmendmentMessageDataProvider : IReceptionMessageDataProvider
	{
	}
}
