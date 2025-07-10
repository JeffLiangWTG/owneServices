using System;
using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class SealDataProvider : ISeal
{
	public static IEnumerable<ISeal> NewCollection(ZString seal) => seal.IsEmpty ? Array.Empty<ISeal>() : new[] { new SealDataProvider(1, seal) };

	SealDataProvider(int sequenceNumber, string identifier)
	{
		SequenceNumber = sequenceNumber;
		Identifier = identifier;
	}

	public int SequenceNumber { get; }

	public string Identifier { get; }

	public string UnloadingRemarkText => string.Empty;
}
