using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSGoodsShipmentWrapper : IT2LPOUSGoodsShipment
{
	public T2LPOUSGoodsShipmentWrapper(CusEntryHeader cusEntryHeader)
	{
		entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = entryHeader.EntryInstruction;
	}
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly CusEntryInstruction entryInstruction;

	const string csiCodePrefixY = "Y";

	public IT2LPOUSCommonContainerIndicator ContainerIndication =>
		containerIndication ??= new T2LPOUSCommonContainerIndicatorWrapper(entryHeader.IsContainerised());
	T2LPOUSCommonContainerIndicatorWrapper containerIndication;

	public IReadOnlyCollection<IT2LPOUSTransportEquipment> TransportEquipment =>
		transportEquipment ??= T2LPOUSTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);
	IReadOnlyCollection<T2LPOUSTransportEquipmentWrapper> transportEquipment;

	public IReadOnlyCollection<IDocumentsCommon> AdditionalInformation =>
		additionalInformation ??=
			entryInstruction.AdditionalInfos.Cast<AdditionalInfo>()
			.Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation))
			.ToReadOnlyWrapperCollection();
	IReadOnlyCollection<DocumentCommonWrapper> additionalInformation;

	public IReadOnlyCollection<IDocumentsCommon> PreviousDocument =>
		previousDocument ??=
			declaration.PreviousDocuments.Cast<PreviousDocument>()
			.ToReadOnlyWrapperCollection();
	IReadOnlyCollection<DocumentCommonWrapper> previousDocument;

	public IReadOnlyCollection<IDocumentsCommon> SupportingDocument =>
		supportingDocument ??=
			declaration.SupportingDocuments.Cast<SupportingDocument>()
			.Concat(entryInstruction.SupportingDocuments.Cast<SupportingDocument>())
			.Concat(GetInvoiceHeaderSupportingDocuments())
			.Where(doc => !doc.CSI_Code.StartsWith(csiCodePrefixY))
			.ToReadOnlyWrapperCollection();
	IReadOnlyCollection<DocumentCommonWrapper> supportingDocument;

	public IReadOnlyCollection<IDocumentsCommon> TransportDocument =>
		transportDocument ??=
			declaration.AdditionalInfos.Cast<AdditionalInfo>()
			.Concat(entryInstruction.AdditionalInfos.Cast<AdditionalInfo>())
			.Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments))
			.ToReadOnlyWrapperCollection();
	IReadOnlyCollection<DocumentCommonWrapper> transportDocument;

	public IReadOnlyCollection<IDocumentsCommon> AdditionalReference
	{
		get
		{
			if (additionalReference == null)
			{
				var supDocs = declaration.SupportingDocuments.Cast<SupportingDocument>()
					.Concat(entryInstruction.SupportingDocuments.Cast<SupportingDocument>())
					.Concat(GetInvoiceHeaderSupportingDocuments())
					.Where(doc => doc.CSI_Code.StartsWith(csiCodePrefixY))
					.ToReadOnlyWrapperCollection();

				var addRefs = entryInstruction.AdditionalInfos.Cast<AdditionalInfo>()
					.Concat(declaration.AdditionalInfos.Cast<AdditionalInfo>())
					.Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalReference))
					.ToReadOnlyWrapperCollection();

				var list = new List<DocumentCommonWrapper>();
				supDocs.ForEach(doc => list.Add(doc));
				addRefs.ForEach(doc => list.Add(doc));
				additionalReference = list.ToArray();
			}

			return additionalReference;
		}
	}
	IReadOnlyCollection<DocumentCommonWrapper> additionalReference;

	public IReadOnlyCollection<IT2LPOUSRequestAndReceptionGoodItem> GoodItems =>
		lines ??=
			entryHeader.MergedLines.Cast<CusEntryLine>()
			.Select(x => new T2LPOUSRequestAndReceptionGoodItemWrapper(x)).ToList().AsReadOnly();
	IReadOnlyCollection<T2LPOUSRequestAndReceptionGoodItemWrapper> lines;

	IEnumerable<SupportingDocument> GetInvoiceHeaderSupportingDocuments() =>
		EntryHasMultipleInvoices() ? Enumerable.Empty<SupportingDocument>() : entryHeader.InvoiceHeaders.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>());

	bool EntryHasMultipleInvoices() => entryHeader.InvoiceHeaders.Length > 1;
}
