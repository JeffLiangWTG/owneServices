using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml;

public class MrnAndIutMessageWrapper : IMrnAndIutProvider
{
	public MrnAndIutMessageWrapper(CusEntryHeader provider, string messageType)
	{
		Mrn = Argument.NotNull(provider, nameof(provider)).MovementReferenceNumber;
		lazyIut = new Lazy<string>(() => FetchIut(provider, messageType));
	}

	string FetchIut(CusEntryHeader provider, string messageType)
	{
		var iut = provider.Messages.Where(x => x.EM_MessageType == messageType && !x.IsTransmitMessage).OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
		return UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier(iut.EM_MessageText);
	}

	public string Mrn { get; }

	public string Iut => lazyIut.Value;

	readonly Lazy<string> lazyIut;
}
