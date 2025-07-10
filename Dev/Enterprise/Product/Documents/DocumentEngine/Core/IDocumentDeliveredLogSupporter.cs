using System;
using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	public interface IDocumentDeliveredLogSupporter
	{
		Type BusinessObjectTypeToLogAgainst { get; }
		ZGuid Identifier { get; }
	}
}
