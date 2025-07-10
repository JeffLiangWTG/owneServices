using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.CH.NCTS.Business;

public class TransportDocumentDataProvider : IDocument
{
	public static IEnumerable<IDocument> NewCollection(ICusSupportingInfoCollection<AdditionalInfo> additionalDocuments)
		=> additionalDocuments?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select((transportDocument, index) => new TransportDocumentDataProvider(transportDocument, index + 1));

	TransportDocumentDataProvider(AdditionalInfo additionalInfo, int sequenceNumber)
	{
		this.additionalInfo = additionalInfo;
		SequenceNumber = sequenceNumber;
	}
	readonly AdditionalInfo additionalInfo;

	public int SequenceNumber { get; }

	public string ReferenceNumber => additionalInfo.CSI_ReferenceNumber;

	public string Type => additionalInfo.CSI_Code;

	public int? LineItemNumber => null;

	public int GoodsItemNumber => -1;

	public string ComplementOfInformation => null;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => Array.Empty<ITransportEquipment>();
}
