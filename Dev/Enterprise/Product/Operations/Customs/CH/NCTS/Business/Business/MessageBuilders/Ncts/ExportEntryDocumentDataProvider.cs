using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;
using DeclarationTransportEquipmentDataProvider = Enterprise.Customs.CH.Business.TransportEquipmentDataProvider;

namespace Enterprise.Customs.CH.NCTS.Business;

public class ExportEntryDocumentDataProvider : IDocument
{
	public static IEnumerable<ExportEntryDocumentDataProvider> NewCollection(RelatedExportEntryHeaderGenPivotCollection relatedExportEntries)
		=> relatedExportEntries?.Cast<RelatedExportEntryHeaderGenPivot>().OrderBy(x => x.XX_Sequence).Select(x => new ExportEntryDocumentDataProvider(x));

	ExportEntryDocumentDataProvider(RelatedExportEntryHeaderGenPivot exportEntry)
	{
		this.exportEntry = exportEntry;
		isExportDeclarationActivation = exportEntry.EntryHeader.Declaration.JE_MessageType == CHJobMessageTypeList.Codes.ExportDeclarationActivation;
	}
	readonly RelatedExportEntryHeaderGenPivot exportEntry;
	readonly bool isExportDeclarationActivation;

	public int SequenceNumber => exportEntry.XX_Sequence;

	public string Type => PreviousDocumentCodes.Export;

	public string ReferenceNumber => CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(exportEntry.EntryNumber);

	public string ComplementOfInformation => null;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ??= isExportDeclarationActivation ? GetTransportEquipments().ToArray() : null;
	IReadOnlyCollection<ITransportEquipment> transportEquipments;

	IEnumerable<ITransportEquipment> GetTransportEquipments() => DeclarationTransportEquipmentDataProvider.NewCollection(exportEntry.EntryHeader.Declaration.CusContainers);

	public int? LineItemNumber => null;

	public int GoodsItemNumber => 0;
}
