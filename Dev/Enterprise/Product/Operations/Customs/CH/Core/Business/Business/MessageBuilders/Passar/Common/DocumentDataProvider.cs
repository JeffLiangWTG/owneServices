using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class DocumentDataProvider : IDocument
{
	public static IEnumerable<IDocument> NewCollection(IEnumerable<CusSupportingInfo> documentList, int sequenceNumberOffset = 0)
		=> documentList?.Select((document, index) => new DocumentDataProvider(document, index + 1 + sequenceNumberOffset))
		?? Enumerable.Empty<DocumentDataProvider>();

	DocumentDataProvider(CusSupportingInfo document, int sequenceNumber)
	{
		this.document = document;
		SequenceNumber = sequenceNumber;
	}
	readonly CusSupportingInfo document;

	public int? LineItemNumber => !document.CSI_ItemNumber.IsEmpty ? (int?)document.CSI_ItemNumber : null;

	public int SequenceNumber { get; }

	public string Type => document.CSI_Code;

	public string ReferenceNumber => document.CSI_ReferenceNumber;

	public int GoodsItemNumber => document.CSI_ItemNumber;

	public string ComplementOfInformation => document.CSI_ReferenceNumber2;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => Array.Empty<ITransportEquipment>();
}
