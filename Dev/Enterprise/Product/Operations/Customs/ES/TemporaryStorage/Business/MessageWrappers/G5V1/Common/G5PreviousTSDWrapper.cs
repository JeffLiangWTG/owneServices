using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers;

public class G5PreviousTSDWrapper : IG5PreviousTSD
{
	public G5PreviousTSDWrapper(TemporaryStoragePreviousDocument doc)
	{
		document = Argument.NotNull(doc, nameof(doc));
	}
	readonly TemporaryStoragePreviousDocument document;

	public ZString MRN => document.CSI_Code == MRNDocumentCode ? document.CSI_ReferenceNumber : ZString.Empty;

	public ICommonArrivalTransportMeans TransportMeans => transportMeans ??= AcceptedDocumentList.Contains(document.CSI_Code) ? new CommonArrivalTransportMeansWrapper(TransportMeansIdCode, document.CSI_ReferenceNumber2) : null;
	CommonArrivalTransportMeansWrapper transportMeans;

	public IDocumentsCommon TransportDocument => transportDocument ??= AcceptedDocumentList.Contains(document.CSI_Code) ? new DocumentCommonWrapper(document.CSI_Code, document.CSI_ReferenceNumber) : null;
	DocumentCommonWrapper transportDocument;

	public ZString GoodsItemId => document.CSI_Code == MRNDocumentCode
													? document.CSI_LineNo.IsEmpty ? ZString.Empty : document.CSI_LineNo.ToString()
													: ZString.Empty;

	readonly static ZString MRNDocumentCode = "337";
	readonly static ZString TransportMeansIdCode = "40";
	static readonly ImmutableHashSet<ZString> AcceptedDocumentList = new ZString[] { "C624", "C625", "C664", "C665", "N703", "N704", "N705", "N714", "N720", "N722", "N730", "N740", "N741", "N750", "N760" }.ToImmutableHashSet();
}
