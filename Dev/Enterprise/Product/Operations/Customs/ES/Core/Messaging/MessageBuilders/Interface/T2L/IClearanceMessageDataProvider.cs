using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IClearanceMessageDataProvider : IESEDIMessageCollectionProvider
	{
		IClearanceHeader Header { get; }
		IReadOnlyCollection<IClearanceLine> Lines { get; }
	}

	public interface IClearanceHeader
	{
		ZString ReceptionCustomsOffice { get; }
		ZString ReceptionT2LReference { get; }
		IPartyNameProvider Declarant { get; }
		ZString GoodsLocation { get; }
		ZInt TotalLinesNum { get; }
		ZBool ContainersIndicator { get; }
		IT2LCommunicationsCommon Communications { get; }
	}

	public interface IClearanceLine
	{
		ZInt LineNumber { get; }
		ZString SummaryDeclaration { get; }
		ZInt LineNumberReferenced { get; }
		ZDecimal GrossWeightInKG { get; }
		ZInt PackageQty { get; }
		IReadOnlyCollection<ZString> Containers { get; }
	}
}
