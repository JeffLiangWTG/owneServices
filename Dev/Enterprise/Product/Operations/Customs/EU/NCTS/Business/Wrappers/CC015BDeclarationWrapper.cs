using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CC015BDeclarationWrapper : DeclarationWrapper, ICC015BDeclaration
	{
		public CC015BDeclarationWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{
			departureMovementHeader = (NctsDepartureMovementHeader)commonMovementHeader;
		}
		readonly NctsDepartureMovementHeader departureMovementHeader;

		public ZString TypeOfDeclaration => departureMovementHeader.BM_InBondEntryType;

		public ZBool IsTIRDeclaration => departureMovementHeader.IsTIRDeclaration;

		public ZString CountryOfDestinationCode => departureMovementHeader.BM_RL_NKDestinationPort;

		public ZString AgreedLocationOfGoodsCode => departureMovementHeader.BM_LocationOfGoodsCode;

		public ZString AgreedLocationOfGoods => departureMovementHeader.BM_LocationOfGoods;

		public ZString AgreedLocationOfGoodsLanguage => ZString.Empty;

		public ZString AuthorisedLocationOfGoodsCode => departureMovementHeader.BM_LocationOfGoodsCode;

		public ZString PlaceOfLoadingCode => departureMovementHeader.BM_RL_NKForeignDestPort;

		public ZString PlaceOfLoading => GetPlaceOfLoading();

		public ZString CountryOfDispatchExportCode => nctsHeader.BH_RL_NKImportLoadPort.SubstringSafe(0, 2);

		public ZString CustomsSubPlace => departureMovementHeader.BM_CustomsSubPlace;

		public ZString InlandTransportMode => departureMovementHeader.BM_InlandTransportMode;

		public ZString TransportModeAtBorder => departureMovementHeader.BM_ExportTransportMode;

		public ZString IdentityOfMeansOfTransportAtDeparture => departureMovementHeader.BM_TransportAtDeparture;

		public ZString IdentityOfMeansOfTransportAtDepartureLanguage => ZString.Empty;

		public ZString NationalityOfMeansOfTransportAtDeparture => departureMovementHeader.BM_RN_NKTransportAtDepartureCountry;

		public ZString IdentityOfMeansOfTransportCrossingBorder => departureMovementHeader.BM_TOLCarrierID;

		public ZString IdentityOfMeansOfTransportCrossingBorderLanguage => ZString.Empty;

		public ZString NationalityOfMeansOfTransportCrossingBorder => departureMovementHeader.BM_TOLCarrierCode;

		public ZString TypeOfMeansOfTransportCrossingBorder => departureMovementHeader.BM_ExportTransportMode;

		public ZBool IsContainerised => departureMovementHeader.IsContainerised;

		public ZString DialogLanguageIndicatorAtDeparture => ZString.Empty;

		public ZString AccompanyingDocumentLanguage
		{
			get
			{
				var language = nctsHeader.Consignee.Organisation?.OH_Language ?? ZString.Empty;
				return language.IsEmpty ? GlbCompany.CurrentCompany.Language.Substring(0, 2) : language.Substring(0, 2);
			}
		}

		public ZInt TotalNumberOfItems => nctsHeader.TotalNumberOfItems;

		public ZLong TotalNumberOfPackages => nctsHeader.TotalNumberOfPackages;

		public ZDecimal TotalGrossMass => nctsHeader.TotalGrossMassInKilograms;

		public ZDecimal TotalInvoiceValue => nctsHeader.TotalInvoiceValue;

		public ZString DeclarationDate
		{
			get
			{
				var entryDate = departureMovementHeader.BM_EntryDate;
				var declarationDate = entryDate.IsEmpty ? ZDateTime.Now : entryDate;
				return declarationDate.GetLongDate();
			}
		}

		public ZString DeclarationPlace => nctsHeader.DeclarationPlace;

		public ZString DeclarationPlaceLanguage => ZString.Empty;

		public ZString SpecificCircumstanceIndicator => departureMovementHeader.BM_BTAIndicator;

		public ZString TransportChargesMethodOfPayment => departureMovementHeader.BM_MethodOfPayment;

		public ZString CommercialReferenceNumber => departureMovementHeader.BM_AdditionalText;

		public ZBool SecurityIndicator => nctsHeader.HasSecurityAtHeaderLevel;

		public ZBool SafetyAndSecurityData => nctsHeader.BH_FTZMove;

		public ZString ProvisionalTransitDepartureDate
		{
			get
			{
				var customsOffices = isPhase5 ? departureMovementHeader.CustomsOffices : nctsHeader.CustomsOffices;
				return WrapperHelper.GetLongDate(customsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)?.CY_Date ?? ZDateTime.Empty);
			}
		}

		public ZString ModeOfRepresentation => nctsHeader.ModeOfRepresentationCalculator.GetModeOfRepresentation();

		public ZString ConveyanceReferenceNumber => departureMovementHeader.BM_ConveyanceNumber;

		public ZString TransportReferenceNumber => nctsHeader.BH_VoyageNumber;

		public ZString PlaceOfUnloadingCode => departureMovementHeader.BM_PlaceOfUnloading;

		public ZString PlaceOfUnloading => GetPlaceOfUnLoading();

		public ZString PlaceOfUnloadingCodeLanguage => ZString.Empty;

		public ITrader Consignor => CachedValueHelper.GetValue(ref consignor, () => TraderWrapper.New(nctsHeader.Consignor, false, IsAddressExtended));
		CachedValue<ITrader> consignor;

		public ITrader Declarant => CachedValueHelper.GetValue(ref declarant, () => TraderWrapper.New(nctsHeader.Declarant, false, IsAddressExtended));
		CachedValue<ITrader> declarant;

		public ZString DeclarantTIN => CachedValueHelper.GetValue(ref declarantTIN, () => nctsHeader.DepartureDeclarantTIN());
		CachedValue<ZString> declarantTIN;

		public ZString PrincipalTIN => CachedValueHelper.GetValue(ref principalTIN, () => nctsHeader.DeparturePrincipalTIN());
		CachedValue<ZString> principalTIN;

		public ZString AuthorisedConsigneeTIN => CachedValueHelper.GetValue(ref authorisedConsigneeTIN, () => nctsHeader.AuthorisedConsigneeTIN());
		CachedValue<ZString> authorisedConsigneeTIN;

		public IReadOnlyCollection<ICustomsOffice> TransitCustomsOffices
		{
			get
			{
				var transitCustomsOfficeCodeList = isPhase5 ? departureMovementHeader.TransitCustomsOfficeCodeList : nctsHeader.TransitCustomsOfficeCodeList;
				return transitCustomsOffices ?? (transitCustomsOffices = transitCustomsOfficeCodeList.Select(x => new CustomsOfficeWrapper(x)).Cast<ICustomsOffice>().ToArray());
			}
		}
		IReadOnlyCollection<ICustomsOffice> transitCustomsOffices;

		public ZString ControlResultCode => departureMovementHeader.BM_GONumber;

		public ZString ControlResultDateLimit => WrapperHelper.GetLongDate(departureMovementHeader.BM_ExportDate);

		public ZString Representative => departureMovementHeader.BM_GS_NKCusAgent;

		public ZInt NumberOfSeals => Seals.Count;

		public IReadOnlyCollection<ISealID> Seals => seals ?? (seals = nctsHeader.GetSealIDs(SealsSelected));
		IReadOnlyCollection<ISealID> seals;

		public IReadOnlyCollection<IGuarantee> Guarantees => guarantees ?? (guarantees = nctsHeader.GetEffectiveGuarantees().Cast<NctsGuarantee>().Select(x => new GuaranteeWrapper(x)).Cast<IGuarantee>().ToArray());
		IReadOnlyCollection<IGuarantee> guarantees;

		public IReadOnlyCollection<IDepartureGoodsItem> GoodsItems => goodsItems ??= EffectiveDepartureGoodsItems.Select(line => new DepartureGoodsItemWrapper(line)).OrderBy(line => line.ItemNumber).ToArray<IDepartureGoodsItem>();
		IReadOnlyCollection<IDepartureGoodsItem> goodsItems;

		public IReadOnlyCollection<ZString> Itinerary => itinerary ?? (itinerary = nctsHeader.Itinerary.Cast<NonPersistentItineraryCountry>().Select(leg => leg.CountryCode).ToArray());
		IReadOnlyCollection<ZString> itinerary;

		public ITrader SecurityConsignor => CachedValueHelper.GetValue(ref securityConsignor, () => TraderWrapper.New(nctsHeader.SecurityConsignor, false, IsAddressExtended));
		CachedValue<ITrader> securityConsignor;

		public ITrader SecurityConsignee => CachedValueHelper.GetValue(ref securityConsignee, () => TraderWrapper.New(nctsHeader.SecurityConsignee, false, IsAddressExtended));
		CachedValue<ITrader> securityConsignee;

		public SecurityTraderCountryGroup SecurityConsignorCountryGroup => CachedValueHelper.GetValue(ref securityConsignorCountryGroup, () => SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(nctsHeader.SecurityConsignor));
		CachedValue<SecurityTraderCountryGroup> securityConsignorCountryGroup;

		public SecurityTraderCountryGroup SecurityConsigneeCountryGroup => CachedValueHelper.GetValue(ref securityConsigneeCountryGroup, () => SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(nctsHeader.SecurityConsignee));
		CachedValue<SecurityTraderCountryGroup> securityConsigneeCountryGroup;

		protected IReadOnlyList<ZString> SealsSelected => sealsSelected ?? (sealsSelected = nctsHeader.GetSealsSelected());
		ZString[] sealsSelected;

		ZString GetPlaceOfLoading()
		{
			var unloco = nctsHeader.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, PlaceOfLoadingCode);
			return unloco != null ? unloco.RL_PortName.SubstringSafe(0, 17) : ZString.Empty;
		}

		ZString GetPlaceOfUnLoading()
		{
			var unloco = nctsHeader.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, PlaceOfUnloadingCode);
			return unloco != null ? unloco.RL_PortName.SubstringSafe(0, 17) : ZString.Empty;
		}

		public ZString SecurityCarrierEORI => CachedValueHelper.GetValue(ref carrierEORI, () => WrapperHelper.GetCarrierEORI(nctsHeader));
		CachedValue<ZString> carrierEORI;
	}
}
