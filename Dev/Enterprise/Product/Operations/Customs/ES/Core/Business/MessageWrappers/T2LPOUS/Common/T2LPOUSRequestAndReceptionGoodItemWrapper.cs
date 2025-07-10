using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSRequestAndReceptionGoodItemWrapper : T2LPOUSCommonGoodsItemWrapper, IT2LPOUSRequestAndReceptionGoodItem
{
	public T2LPOUSRequestAndReceptionGoodItemWrapper(CusEntryLine entryLine) : base(entryLine)
	{
		randomLine = entryLine.RandomLine;
	}
	readonly JobComInvoiceLine randomLine;

	const int descriptionMaxLength = 512;
	const string csiCodePrefixY = "Y";

	public ICommodityCodeCommon CommodityCode => commodityCode ??= new CommodityCodeCommonWrapper(randomLine);
	CommodityCodeCommonWrapper commodityCode;

	public ZString Description => randomLine.JI_Description.SubstringSafe(0, descriptionMaxLength).Replace("\r\n", " ").Replace("\n", " ");

	public ZString CusCode => randomLine.ZG_CusNumber;

	public IGoodsMeasureCommon GoodsMeasure => goodsMeasure ??= new GoodsMeasureCommonWrapper(entryLine);
	GoodsMeasureCommonWrapper goodsMeasure;

	public IReadOnlyCollection<IT2LPOUSRequestAndReceptionPackaging> Package =>
		package ??= T2LPOUSRequestAndReceptionPackagingWrapper.GetPackagesList(entryLine).AsReadOnly();
	IReadOnlyCollection<T2LPOUSRequestAndReceptionPackagingWrapper> package;

	public IReadOnlyCollection<IDocumentsCommon> AdditionalInformation =>
		additionalInformation ??=
			entryLine.AdditionalInfos.Cast<AdditionalInfo>()
			.Concat(randomLine.InvoiceHeader.AdditionalInfos.Cast<AdditionalInfo>())
			.Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation))
			.ToReadOnlyWrapperCollection();
	IReadOnlyCollection<DocumentCommonWrapper> additionalInformation;

	public IReadOnlyCollection<IDocumentsCommon> PreviousDocument =>
		previousDocument ??=
			entryLine.InvoiceLines.Cast<JobComInvoiceLine>()
			.SelectMany(l => l.PreviousDocuments.Cast<PreviousDocument>())
			.Concat(randomLine.InvoiceHeader.PreviousDocuments.Cast<PreviousDocument>())
			.ToReadOnlyWrapperCollection();
	IReadOnlyCollection<DocumentCommonWrapper> previousDocument;

	public IReadOnlyCollection<IDocumentsCommon> SupportingDocument =>
		supportingDocument ??=
			GetInvoiceLinesSupportingDocuments()
			.Concat(GetInvoiceHeaderSupportingDocuments())
			.Where(doc => !doc.CSI_Code.StartsWith(csiCodePrefixY))
			.ToReadOnlyWrapperCollection();
	IReadOnlyCollection<DocumentCommonWrapper> supportingDocument;

	public IReadOnlyCollection<IDocumentsCommon> AdditionalReference
	{
		get
		{
			if (additionalReference == null)
			{
				var supDocs =
					GetInvoiceLinesSupportingDocuments()
					.Concat(GetInvoiceHeaderSupportingDocuments())
					.Where(doc => doc.CSI_Code.StartsWith(csiCodePrefixY))
					.ToReadOnlyWrapperCollection();

				var addRefs =
					entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(line => line.AdditionalInfos.Cast<AdditionalInfo>())
					.Concat(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(line => line.InvoiceHeader).SelectMany(invoice => invoice.AdditionalInfos.Cast<AdditionalInfo>()))
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

	IEnumerable<SupportingDocument> GetInvoiceLinesSupportingDocuments() =>
		entryLine.InvoiceLines.Cast<JobComInvoiceLine>()
		.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>());

	IEnumerable<SupportingDocument> GetInvoiceHeaderSupportingDocuments() =>
		EntryHasMultipleInvoices()
		? entryLine.InvoiceLines.Cast<JobComInvoiceLine>()
			.SelectMany(line => line.InvoiceHeader.SupportingDocuments.Cast<SupportingDocument>())
		: Enumerable.Empty<SupportingDocument>();

	bool EntryHasMultipleInvoices() => entryLine.Header.InvoiceHeaders.Length > 1;
}
