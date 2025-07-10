using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVTaxationDocumentWrapper : GenericEVVDocumentWrapper<IEvvTaxationDecisionProvider>
{
	public static EVVTaxationDocumentWrapper New(CHEDIMessage message, BusinessObjectFactory factory) => new EVVTaxationDocumentWrapper(message, factory);

	EVVTaxationDocumentWrapper(CHEDIMessage message, BusinessObjectFactory factory) : base(message, factory)
	{
	}
	
	public ZString SchemaVersion => responseData?.SchemaVersion ?? ZString.Empty;

	public ZString RequestorTraderIdentificationNumber => responseData?.RequestorTraderIdentificationNumber ?? ZString.Empty;

	public ZString DeclarationType => responseData?.DeclarationType ?? ZString.Empty;

	public ZDateTime AcceptanceDateTime => responseData?.AcceptanceDateTime.DateTime ?? ZDateTime.Empty;

	public ZString Ensty => ensty ??= EnstyCodeList.GetDescriptionFromCode(DeclarationType.PadLeft(2, '0'));
	ZString? ensty;

	CodeDescriptionPairList EnstyCodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, ZDateTime.Now);

	public ZString TraderDeclarationNumber => responseData?.TraderDeclarationNumber ?? ZString.Empty;

	public ZString TraderReference => responseData?.TraderReference ?? ZString.Empty;

	public ZString AccessCode => responseData?.AccessCode ?? ZString.Empty;

	public EVVAddressWrapper ConsignorAddress => consignorAddress ?? (consignorAddress = EVVAddressWrapper.New(responseData?.Consignor));
	EVVAddressWrapper consignorAddress;

	public EVVAddressWrapper ImporterAddress => importerAddress ?? (importerAddress = EVVAddressWrapper.New(responseData?.Importer));
	EVVAddressWrapper importerAddress;

	public ZString ImporterTIN => responseData?.Importer.TraderIdentificationNumber ?? ZString.Empty;

	public EVVAddressWrapper ConsigneeAddress => consigneeAddress ?? (consigneeAddress = EVVAddressWrapper.New(responseData?.Consignee));
	EVVAddressWrapper consigneeAddress;

	public ZString ConsigneeTIN => responseData?.Consignee.TraderIdentificationNumber ?? ZString.Empty;

	public EVVAddressWrapper DeclarantAddress => declarantAddress ?? (declarantAddress = EVVAddressWrapper.New(responseData?.Declarant));
	EVVAddressWrapper declarantAddress;

	public ZString DeclarantTIN => responseData?.Declarant.TraderIdentificationNumber ?? ZString.Empty;

	public ZString DeclarantNumber => responseData?.Declarant.DeclarantNumber ?? ZString.Empty;

	public ZString BodereauNumber => responseData?.BordereauNumber ?? ZString.Empty;

	public ZString DispatchCountry => responseData?.DispatchCountry ?? ZString.Empty;

	public ZInt NumberOfGoods => responseData?.NumberOfGoods ?? 0;

	public ZString AccountNumber => responseData?.Account?.Number ?? ZString.Empty;

	public ZString AccountName => responseData?.Account?.Name ?? ZString.Empty;

	public ZString VATNumber => responseData?.VATNumber ?? ZString.Empty;

	public ZString VATSuffix => DocumentWrapperHelper.GetVATSuffixText(responseData?.VATSuffix ?? false);

	public ZString InvoiceCurrencyType => invoiceCurrencyType ??= Factory.GetCachedValue<EVVInvoiceCurrencyType>().GetMultilingualDescriptionFromCode(responseData?.InvoiceCurrencyType)?.ToString(DocumentLanguage);
	ZString? invoiceCurrencyType;

	public ZString Incoterms => responseData?.Incoterms ?? ZString.Empty;

	public ZString TransportMode => transportMode ??= Factory.GetCachedValue<EVVTransportModeList>().GetMultilingualDescriptionFromCode(responseData?.TransportMeans?.TransportMode)?.ToString(DocumentLanguage);
	ZString? transportMode;

	public ZString TransportationType => transportationType ??= GetTransportationType();
	ZString? transportationType;

	ZString GetTransportationType()
	{
		string description = null;
		var transportationType = responseData?.TransportMeans?.TransportationType;
		if (!transportationType.IsNullOrEmpty())
		{
			description = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportationType, ZDateTime.Now, languageCode: DocumentLanguage.GetLanguageCode()).GetDescriptionFromCode(transportationType);
		}
		return description ?? NoValueText;
	}

	public ZString TransportationCountry => responseData?.TransportMeans?.TransportationCountry ?? new ZString(NoValueText);

	public ZString TransportationNumber => responseData?.TransportMeans?.TransportationNumber ?? ZString.Empty;

	public ZString Containers => responseData?.Containers != null ? string.Join(", ", responseData.Containers) : string.Empty;

	public ZString SpecialMentions => responseData != null ? new ZStringBuilder(responseData.SpecialMentions).ToStringWithNewLineBetweenAppends() : string.Empty;

	public EVVPreviousDocumentWrapperCollection PreviousDocuments => previousDocuments ?? (previousDocuments = EVVPreviousDocumentWrapperCollection.New(responseData?.PreviousDocuments, Factory));
	EVVPreviousDocumentWrapperCollection previousDocuments;

	public EVVDutyAmountWrapperCollection Duties => duties ??= EVVDutyAmountWrapperCollection.New(responseData?.Duties, Factory, DocumentLanguage);
	EVVDutyAmountWrapperCollection duties;

	public ZDecimal TotalAmount => responseData?.TotalAmount ?? ZDecimal.Zero;

	public EVVGoodsItemWrapperCollection GoodsItems => goodsItems ??= EVVGoodsItemWrapperCollection.New(responseData?.GoodsItems, Factory, DocumentLanguage);
	EVVGoodsItemWrapperCollection goodsItems;

	public EVVLegalAdvisoryWrapperCollection LegalAdvisories => legalAdvisories ??= EVVLegalAdvisoryWrapperCollection.New(responseData?.LegalAdvisories.OrderBy(l => l.SequenceNumber), Factory);
	EVVLegalAdvisoryWrapperCollection legalAdvisories;

	const string NoValueText = "--";
}

