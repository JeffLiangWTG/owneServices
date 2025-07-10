using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public abstract class DocBaseCusEntryHeader : DocumentWrappers.Customs.Base.DocBaseCusEntryHeader
{
	public static DocBaseCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
	{
		if (cusEntryHeader == null)
		{
			return null;
		}

		if (cusEntryHeader.IsExport)
		{
			return DocPassarCusEntryHeader.New(cusEntryHeader, factoryToWrap);
		}
		else
		{
			return DocEdecCusEntryHeader.New(cusEntryHeader, factoryToWrap);
		}
	}

	protected DocBaseCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap) : base(cusEntryHeader, factoryToWrap)
	{
		entryInstruction = cusEntryHeader.EntryInstruction;
	}
	protected readonly CusEntryInstruction entryInstruction;

	protected CusEntryHeader CusEntryHeader => (CusEntryHeader)WrappedObject;

	public ZDateTime AcceptanceDateTime => CusEntryHeader.CH_EntryReleaseDate;

	public ZString SelectionResult => CusEntryHeader.SelectionResult;

	public ZString SelectionResultDescription => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult, ZDateTime.Today, languageCode: DocumentWrapperHelper.SelectedPrinterLanguage).GetDescriptionFromCode(SelectionResult);

	public ZString DeclarationType => entryInstruction?.Lookups.StyleList.GetDescriptionFromCode(entryInstruction.CEI_Style);

	public ZString DeclarationTime => entryInstruction?.Lookups.EntrySubStyleList.GetDescriptionFromCode(entryInstruction.CEI_SubStyle);

	public ZString TraderReference => (ZString)(!CusEntryHeader.Declaration?.JE_OwnerRef.IsEmpty ?? false ? CusEntryHeader.Declaration?.JE_OwnerRef : CusEntryHeader.Declaration?.JE_DeclarationReference);

	public ZString CountryOfDestination => CusEntryHeader.Declaration?.JE_GoodsDestination ?? ZString.Empty;

	public ZString Currency => CusEntryHeader.Declaration?.Invoices?.MaxBy(i => i.JZ_InvoiceAmount)?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;

	public ZString PlaceOfUnloading => CusEntryHeader.Declaration?.JE_LocationOfGoods ?? ZString.Empty;

	public ZString CustomsOfficeName => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, (ZString)(CusEntryHeader.Declaration?.JE_CustomsOffice), Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CustomsOffice, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;

	public ZString PrevDocReferences => GetPreviousDocumentsReferences();

	public DocTraderDataWrapper Exporter => exporter ??= DocTraderDataWrapper.New(CusEntryHeader.Declaration?.SupplierDocumentaryAddress, Factory, AddressAttributeMaxLength);
	DocTraderDataWrapper exporter;

	public DocTraderDataWrapper Consignor => consignor ??= DocTraderDataWrapper.New(CusEntryHeader.Declaration?.ConsignorDocAddress, Factory, AddressAttributeMaxLength);
	DocTraderDataWrapper consignor;

	public virtual DocTraderDataWrapper Consignee => consignee ??= DocTraderDataWrapper.New(CusEntryHeader.Declaration?.ImporterDocumentaryAddress, Factory, AddressAttributeMaxLength);
	DocTraderDataWrapper consignee;

	public DocTraderDataWrapper AuthorizedConsignee => authorizedConsignee ??= DocTraderDataWrapper.New(CusEntryHeader.Declaration?.Representative, Factory, AddressAttributeMaxLength);
	DocTraderDataWrapper authorizedConsignee;

	public DocTraderDataWrapper Declarant => declarant ??= DocTraderDataWrapper.New(CusEntryHeader.Declaration?.DeclarantAddress, Factory, AddressAttributeMaxLength);
	DocTraderDataWrapper declarant;

	public ZString BrokerName => CusEntryHeader.Declaration?.CHDPassword?.Staff?.GS_FullName ?? ZString.Empty;

	public DocFinanceDataWrapper FinanceData => financeData ??= DocFinanceDataWrapper.New(CusEntryHeader.InvoiceHeaders?.FirstOrDefault(), Factory);
	DocFinanceDataWrapper financeData;

	public ZString TransportationNumber => CusEntryHeader.Declaration?.IsAir ?? false ? CusEntryHeader.Declaration.JE_VoyageFlightNo : CusEntryHeader.Declaration?.JE_VesselName ?? ZString.Empty;

	#region Collections

	public DocCusEntryLineCollection EntryLines => entryLines ??= CreateNewDocCusEntryLineCollection();
	DocCusEntryLineCollection entryLines;

	DocCusEntryLineCollection CreateNewDocCusEntryLineCollection()
	{
		var entryLines = new DocCusEntryLineCollection(CusEntryHeader.MergedLines, Factory);
		entryLines.Cast<DocCusEntryLine>().ForEach(x => x.EntryHeader = this);
		entryLines.Sort(nameof(DocCusEntryLine.LineNumber), System.ComponentModel.ListSortDirection.Ascending);
		return entryLines;
	}

	public DocDocumentCollection PreviousDocuments => previousDocuments ??= CreatePrevDocDocumentCollection();
	DocDocumentCollection previousDocuments;

	DocDocumentCollection CreatePrevDocDocumentCollection()
	{
#if NETFRAMEWORK
		var previousDocuments = entryInstruction?.PreviousDocuments
			.Cast<CusSupportingInfo>()
			.Concat(CusEntryHeader.InvoiceHeaders.SelectMany(x => x.PreviousDocuments.Cast<CusSupportingInfo>()))
			.DistinctBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber });
#else
		var previousDocuments = IEnumerableExtensions.DistinctBy(entryInstruction?.PreviousDocuments
			.Cast<CusSupportingInfo>()
			.Concat(CusEntryHeader.InvoiceHeaders.SelectMany(x => x.PreviousDocuments.Cast<CusSupportingInfo>())),
			x => new { x.CSI_Code, x.CSI_ReferenceNumber });
#endif

		return new DocDocumentCollection(previousDocuments, Factory);
	}

	ZString GetPreviousDocumentsReferences()
	{
		var result = string.Join(", ", PreviousDocuments.Cast<DocDocumentDataWrapper>().Select(doc => $"{doc.Type} - {doc.ReferenceNumber}"));
		return result.Length > PrevDocReferencesMaxLength ? result.Substring(0, PrevDocReferencesMaxLength) + "..." : result;
	}

	protected virtual int AddressAttributeMaxLength => 29;
	protected virtual int PrevDocReferencesMaxLength => 129;

	#endregion
}
