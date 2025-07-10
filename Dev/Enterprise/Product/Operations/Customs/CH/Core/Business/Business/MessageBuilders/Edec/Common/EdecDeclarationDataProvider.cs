using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class EdecDeclarationDataProvider<T> : IEdecDeclaration where T : JobDeclarationMessageSendingObject
{
	public EdecDeclarationDataProvider(T sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	protected readonly T sendingObject;

	protected CusEntryHeader EntryHeader => (CusEntryHeader)sendingObject.Header;

	protected CusEntryInstruction EntryInstruction => EntryHeader?.EntryInstruction;

	protected JobDeclaration Declaration => EntryHeader?.Declaration;

	public string TraderDeclarationNumber => EntryHeader?.CH_BGMReference;

	public string TraderReference => !Declaration?.JE_OwnerRef.IsEmpty ?? false ? Declaration?.JE_OwnerRef : Declaration?.JE_DeclarationReference;

	public string ClearanceLocation => Declaration?.JE_ClearanceLocation;

	public string DeclarationType => EntryInstruction?.CEI_Style;

	public string DeclarationTime => EntryInstruction?.CEI_SubStyle;

	public string CorrectionCode
	{
		get
		{
			switch (sendingObject.MessageType)
			{
				case PassarMessageTypeList.Codes.NI015:
					return MessagingConstants.CustomsCorrectionCode.Original;
				case PassarMessageTypeList.Codes.NI013:
					return MessagingConstants.CustomsCorrectionCode.Correction;
				case PassarMessageTypeList.Codes.NI014:
					return MessagingConstants.CustomsCorrectionCode.Cancellation;
				case PassarMessageTypeList.Codes.NI016:
					return MessagingConstants.CustomsCorrectionCode.RequestLastResponse;
				default:
					return string.Empty;
			}
		}
	}

	public string CorrectionReason
	{
		get
		{
			switch (sendingObject.MessageType)
			{
				case PassarMessageTypeList.Codes.NI013:
				case PassarMessageTypeList.Codes.NI014:
					return sendingObject.VOCReason;
				default:
					return string.Empty;
			}
		}
	}

	public string Language => (Declaration?.JE_DeclarationLanguage.ToLower()).ReturnDefaultValueIfNullOrEmpty(UndefinedCustomsLanguage);

	public virtual string CustomsOfficeNumber => Declaration?.JE_CustomsOffice;

	public virtual string DispatchCountry => null;

	public virtual bool DispatchCountryConfirmation => false;

	public virtual string PlaceofUnloading => null;

	public virtual string InjunctionType => null;

	public string Reason => EntryInstruction?.CEI_DeclarationReason;

	public bool TransportInContainer
	{
		get
		{
			switch (Declaration?.JE_ContainerMode)
			{
				case Core.Constants.ContainerModes.FCL:
				case Core.Constants.ContainerModes.LCL:
				case Core.Constants.ContainerModes.Containerised:
					return true;
				default:
					return false;
			}
		}
	}

	public virtual string ServiceType => null;

	public virtual string WarehouseCoded => null;

	public virtual string PlaceOfLoading => null;

	public virtual string AgreedLocationOfGoods => null;

	public virtual string DeliveryDestination => null;

	public virtual string UniqueConsignmentReferenceNumber => null;

	public virtual bool? Security => null;

	public virtual string SpecificCircumstanceIndicator => null;

	public IEdecTransportMeans TransportMeans => transportMeans ?? (transportMeans = EdecTransportMeansDataProvider.New(Declaration));
	IEdecTransportMeans transportMeans;

	public IEdecDeclarant Declarant => declarant ?? (declarant = EdecDeclarantDataProvider.New(Declaration));
	IEdecDeclarant declarant;

	public IEdecBusiness Business => business ?? (business = NewEdecBusiness(EntryHeader));
	IEdecBusiness business;

	protected abstract IEdecBusiness NewEdecBusiness(CusEntryHeader entryHeader);

	public IEdecAddress ConsignorAddress => consignorAddress ?? (consignorAddress = EdecAddressDataProvider.New(Declaration?.SupplierDocumentaryAddress));
	IEdecAddress consignorAddress;

	public virtual IEdecAddress ImporterAddress => null;

	public abstract IEdecAddress ConsigneeAddress { get; }

	public virtual IEdecAddress AuthorizedConsigneeAddress => null;

	public virtual IEdecAddress ConsignorSecurityAddress => null;

	public virtual IEdecAddress ConsigneeSecurityAddress => null;

	public virtual IEdecAddress CarrierAddress => null;

	public virtual IEdecAddress VendeeAddress => null;

	public virtual IEdecAddress BailorAddress => null;

#if NETFRAMEWORK
	public IEnumerable<IEdecContainer> Containers => containers ?? (containers = EntryHeader.Containers.Cast<CusContainer>().Select(x => EdecContainerDataProvider.New(x)).DistinctBy(x => x.ContainerNumber).ToArray());
#else
	public IEnumerable<IEdecContainer> Containers => containers ?? (containers = IEnumerableExtensions.DistinctBy(EntryHeader.Containers.Cast<CusContainer>().Select(x => EdecContainerDataProvider.New(x)), x => x.ContainerNumber).ToArray());
#endif
	IEnumerable<IEdecContainer> containers;

	public IEnumerable<IEdecPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = LoadPreviousDocuments());
	IEnumerable<IEdecPreviousDocument> previousDocuments;

	IEdecPreviousDocument[] LoadPreviousDocuments()
	{
		var previousDocuments = EntryHeader.InvoiceHeaders.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>());
		return previousDocuments.Select(x => EdecPreviousDocumentDataProvider.New(x)).ToArray();
	}

	public IEnumerable<IEdecSpecialMention> SpecialMentions => specialMentions ?? (specialMentions = EdecSpecialMentionDataProvider.NewCollection(EntryHeader.InvoiceHeaders.AsEnumerable())).ToArray();
	IEnumerable<IEdecSpecialMention> specialMentions;

	public IEnumerable<IEdecGoodsItem> GoodsItems => goodsItems ?? (goodsItems = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(entryLine => NewEdecGoodsItem(entryLine)).ToArray());
	IEnumerable<IEdecGoodsItem> goodsItems;

	protected abstract IEdecGoodsItem NewEdecGoodsItem(CusEntryLine entryLine);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string UndefinedCustomsLanguage = "xx";
}
