using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using DateTime = System.DateTime;
using JobComInvoiceHeader = Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public sealed class EXPDATHeaderProvider : AESHeaderProvider, IEXPDATHeader
	{
		public EXPDATHeaderProvider(ExportEntryMessageSendingAction action)
			: base(action?.MessagingObject)
		{
			this.action = Argument.NotNull(action, nameof(action));
			Argument.NotNull(EntryInstruction, nameof(EntryInstruction));
		}
		readonly ExportEntryMessageSendingAction action;

		bool IAESHeader.IsContainerized => GetIsContainerizedCore;

		bool GetIsContainerizedCore => base.IsContainerized && Declaration.JE_ContainerMode != Core.Constants.ContainerModes.LCL;

		IReadOnlyCollection<ITransportEquipment> IAESHeader.TransportEquipments => transportEquipments ?? (transportEquipments = GetIsContainerizedCore ? Declaration.CusContainers
			.Where(x => !x.CO_ContainerNumber.IsEmpty).Select(x => TransportEquipmentProvider.NewOrNull(EntryInstruction, x)).ToArray() : Array.Empty<ITransportEquipment>());
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		public ZString DeclarationType => Declaration.JE_EntryStyle;

		public string ExportDeclarationType => EntryInstruction.CEI_SubStyle + EntryInstruction.CEI_Style;

		public ZString PartyConstellation => EntryInstruction.ZG_PartyConstellation;

		public ZDate DecisiveDate => EntryInstruction.SubStyle1stDigitIs1() || (ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(EntryInstruction.CEI_SubStyle) && ExportDeclarationTypeProcedureList.Is000000(EntryInstruction.CEI_Style)) ? EntryInstruction.CEI_DateForDuty.Date : ZDate.Empty;

		public ZDate ExitDate
		{
			get
			{
				var result = ZDate.Empty;
				var subStyle = EntryInstruction.CEI_SubStyle;
				if ((ExportDeclarationTypeTimeList.Is10(subStyle) && EntryInstruction.Style2ndDigitIs0()) ||
					(ExportDeclarationTypeTimeList.Is11Or12Or13(subStyle) && ExportDeclarationTypeProcedureList.Is000000(EntryInstruction.CEI_Style)))
				{
					result = EntryInstruction.ZG_ExitDate.Date;
				}
				return result;
			}
		}

		public DateTime PresentationStartDateAndTimeUtc => EntryInstruction.Style4thDigitIs2() || EntryInstruction.Style4thDigitIs4() ? Declaration.ZG_PresentationStartDate.ToUniversalBranchTime(Declaration.Factory).SafeDateTime().ZeroFromSecond() : default;

		public DateTime LoadingEndDateAndTimeUtc => EntryInstruction.Style4thDigitIs2() ? Declaration.ZG_PresentationEndDate.ToUniversalBranchTime(Declaration.Factory).SafeDateTime().ZeroFromSecond() : default;

		public string Security => action.SecurityType;

		public ZString SpecificCircumstanceIndicator => Declaration.ZG_SpecificCircumstanceIndicator;

		public IReadOnlyCollection<IAuthorisation> Authorisations
		{
			get
			{
				if (authorisations == null)
				{
					if (ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(EntryInstruction.CEI_SubStyle) ||
						(EntryInstruction.Style2ndDigitIs0() && EntryInstruction.Style4thDigitIs4()) ||
						(EntryInstruction.Style1stDigitIs1() && EntryInstruction.Style2ndDigitIs1()) ||
						(EntryInstruction.Style2ndDigitIs0() && EntryInstruction.Style3rdDigitIs1()))
					{
						authorisations = AuthorisationProvider.New(EntryInstruction);
					}
					else
					{
						authorisations = Array.Empty<IAuthorisation>();
					}
				}
				return authorisations;
			}
		}
		IReadOnlyCollection<IAuthorisation> authorisations;

		public string CustomsOfficeOfPresentation => EntryInstruction.Style4thDigitIs4() ? Declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.OfficeOfPresentation) : ZString.Empty;

		string IAESHeader.SupplementaryDeclarationCustomsOffice => EntryInstruction.Style5thDigitIs1() ? SupplementaryDeclarationCustomsOffice : null;

		public ZString IntendedExitCustomsOffice => EntryInstruction.SubStyle1stDigitIs0() ? Declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.OfficeOfExit) : ZString.Empty;

		public ZString ActualExitCustomsOffice
		{
			get
			{
				var result = ZString.Empty;
				var subStyle = EntryInstruction.CEI_SubStyle;
				if ((ExportDeclarationTypeTimeList.Is10(subStyle) && EntryInstruction.Style2ndDigitIs0()) ||
					(ExportDeclarationTypeTimeList.Is11Or12Or13(subStyle) && ExportDeclarationTypeProcedureList.Is000000(EntryInstruction.CEI_Style)))
				{
					result = Declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.ActualExitOffice);
				}
				return result;
			}
		}

		public IAESParty ContractualPartner => CachedValueHelper.GetValue(ref contractualPartnerCached, () => EntryInstruction.Constellation1stDigitIs1() ? PartyProvider.NewOrNull(Declaration.ContractualPartnerDocAddress.Address) : null);
		CachedValue<IAESParty> contractualPartnerCached;

		public IAESParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => EntryInstruction.Constellation2ndDigitIs1() ? PartyProvider.NewOrNull(Declaration.ExporterDocAddress.Address) : null);
		CachedValue<IAESParty> exporterCached;

		IAESParty IAESHeader.Declarant => CachedValueHelper.GetValue(ref declarantCached, () => EntryInstruction.Constellation3rdDigitIs0() ? PartyProvider.NewOrNull(Declaration.Declarant, GlbStaff.CurrentUser) : PartyProvider.NewOrNull(Declaration.Declarant));
		CachedValue<IAESParty> declarantCached;

		public IAESParty SubContractor => CachedValueHelper.GetValue(ref subContractorCached, () => EntryInstruction.Constellation4thDigitIs1() ? PartyProvider.NewOrNull(Declaration.SellerAddress) : null);
		CachedValue<IAESParty> subContractorCached;

		public ZString ExportCountry
		{
			get
			{
				var entryInstruction = EntryInstruction;
				var countryOfExport = ZString.Empty;
				var goodsOrigin = Declaration.JE_GoodsOrigin;
				if (entryInstruction.Style4thDigitIs9())
				{
					if (entryInstruction.InvoiceLines.AllSame(x => x.JI_RN_NKCountryOfExport))
					{
						countryOfExport = goodsOrigin.IsEmpty ? entryInstruction.InvoiceLines.Select(x => x.JI_RN_NKCountryOfExport).FirstOrDefault() : goodsOrigin;
					}
				}
				else
				{
					countryOfExport = goodsOrigin;
				}

				return countryOfExport;
			}
		}

		public ZString DestinationCountry => Declaration.JE_GoodsDestination;

		public ZString DeclarationProcedure => EntryInstruction.CEI_Style;

		public ZString DeclarationVariant => EntryInstruction.CEI_SubStyle;

		public IReadOnlyCollection<ISupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = EntryInstruction.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>()
			.Select(SupplyChainActorProvider.NewOrNull).ToArray());
		IReadOnlyCollection<ISupplyChainActor> additionalSupplyChainActors;

		public IOutwardProcessing OutwardProcessing => CachedValueHelper.GetValue(ref outwardProcessingCached, () => EntryInstruction.Style1stDigitIs1() ? OutwardProcessingProvider.NewOrNull(EntryInstruction) : null);
		CachedValue<IOutwardProcessing> outwardProcessingCached;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = DistinctInvoiceHeaders.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>()).Select(PreviousDocumentProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??= DistinctInvoiceHeaders.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).OrderBy(d => d.CSI_LineNo).Select(SupportingDocumentProvider.NewOrNull).ToArray();
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IReference> AdditionalReferences
			=> additionalReferences ?? (additionalReferences = DistinctInvoiceHeaders.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()).Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalReference).Select(AdditionalInfoProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IReference> additionalReferences;

		public IReadOnlyCollection<IReference> AdditionalInformations
			=> additionalInformations ?? (additionalInformations = DistinctInvoiceHeaders.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()).Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalInformation).Select(AdditionalInfoProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IReference> additionalInformations;

		public IPartyID Carrier => CachedValueHelper.GetValue(ref carrierCached, () => PartyIDProvider.NewOrNull(Declaration.CarrierEUBorderDocAddress));
		CachedValue<IPartyID> carrierCached;

		public IAESParty Consignor => CachedValueHelper.GetValue(ref consignorCached, GetConsignor);
		CachedValue<IAESParty> consignorCached;

		IAESParty GetConsignor()
		{
			IAESParty result = null;
			var invoiceLinesHaveConsignor = EntryHeader.InvoiceLines.Where(x => !x.JI_OA_ExporterAddress.IsEmpty);
			if (invoiceLinesHaveConsignor.AllSame(x => x.JI_OA_ExporterAddress))
			{
				if (Declaration.SupplierDocumentaryAddress.Address != null)
				{
					result = PartyProvider.NewOrNull(Declaration.SupplierDocumentaryAddress.Address);
				}
				else
				{
					result = PartyProvider.NewOrNull(invoiceLinesHaveConsignor.FirstOrDefault()?.ExporterAddress);
				}
			}
			return result;
		}

		public IAESParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, GetConsignee);
		CachedValue<IAESParty> consigneeCached;

		IAESParty GetConsignee()
		{
			IAESParty result = null;

			// Fallback consignee for invoice line is one from invoice header
			var invoiceLinesHaveConsignee = EntryHeader.InvoiceLines.Select(x =>
				new
				{
					JI_OA_ConsigneeAddress = !x.JI_OA_ConsigneeAddress.IsEmpty ? x.JI_OA_ConsigneeAddress : x.InvoiceHeader.JZ_OA_ConsigneeAddress,
					ConsigneeAddress = !x.JI_OA_ConsigneeAddress.IsEmpty ? x.ConsigneeAddress : x.InvoiceHeader.ConsigneeAddress,
				})
				.Where(x => !x.JI_OA_ConsigneeAddress.IsEmpty);

			if (invoiceLinesHaveConsignee.AllSame(x => x.JI_OA_ConsigneeAddress))
			{
				result = PartyProvider.NewOrNull(invoiceLinesHaveConsignee.FirstOrDefault()?.ConsigneeAddress);
			}
			return result;
		}

		public bool LocationOfGoodsSpecified => !ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(EntryInstruction.CEI_SubStyle)
					&& !EntryInstruction.GoodsLocation.CGL_Qualifier.IsEmpty;

		public string TypeOfLocation => EntryInstruction.GoodsLocation.CGL_Type;

		public string QualifierOfIdentification => EntryInstruction.GoodsLocation.CGL_Qualifier.ToString();

		public string AuthorisationNumber
		{
			get
			{
				var result = string.Empty;
				if (QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
					&& !EntryInstruction.Style3rdDigitIs1()
					&& !EntryInstruction.Style4thDigitIs4())
				{
					var cusAuthorizationUsage = EntryInstruction.CusAuthorizationUsages.SingleOrDefault(a => a.AGC_Code == CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
					result = cusAuthorizationUsage?.AGC_Number ?? string.Empty;
				}
				return result;
			}
		}

		public string AdditionalIdentifier => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.AuthorizationNumber ? EntryInstruction.GoodsLocation.CGL_AdditionalIdentifier : string.Empty;

		public string UNLocode => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? EntryInstruction.GoodsLocation.CGL_AdditionalIdentifier : string.Empty;

		public bool GNSSSpecified => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates;

		public double GNSSLatitude => (double)(EntryInstruction.GoodsLocation.Address?.E2_GeoLocation.Latitude ?? 0d);

		public double GNSSLongitude => (double)(EntryInstruction.GoodsLocation.Address?.E2_GeoLocation.Longitude ?? 0d);

		public IPartyDocAddress LocationOfGoodsParty => CachedValueHelper.GetValue(ref locationOfGoodsPartyCached, () => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.Address ? PartyDocAddressProvider.NewOrNull(EntryInstruction.GoodsLocation.Address) : null);
		CachedValue<IPartyDocAddress> locationOfGoodsPartyCached;

		public IAESPartyContactPerson LocationOfGoodsContactPerson => CachedValueHelper.GetValue(ref locationOfGoodsContactPersonCached, GetLocationOfGoodsContactPerson);
		CachedValue<IAESPartyContactPerson> locationOfGoodsContactPersonCached;

		IAESPartyContactPerson GetLocationOfGoodsContactPerson()
		{
			IAESPartyContactPerson result = null;
			if (QualifierOfIdentification != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier)
			{
				var supplierPickupAddress = EntryInstruction.GoodsLocation.Address;
				if (!supplierPickupAddress.E2_AddressOverride)
				{
					result = PartyContactPersonProvider.NewOrNull(supplierPickupAddress.Contact);
				}
				else
				{
					result = PartyContactPersonJobDocAddressProvider.NewOrNull(supplierPickupAddress);
				}
			}
			return result;
		}

		public IReadOnlyCollection<IReference> TransportDocuments => transportDocuments ?? (transportDocuments = DistinctInvoiceHeaders.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()).Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.TransportDocuments).Select(AdditionalInfoProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IReference> transportDocuments;

		public ZString TransportChargesPaymentMethod => CachedValueHelper.GetValue(ref transportChargesPaymentMethodCached, () => EntryHeader.GetAllSameValue(x => x.ZG_TransportChargesMethodOfPayment));
		CachedValue<ZString> transportChargesPaymentMethodCached;

		public IReadOnlyCollection<IEXPDATLine> Lines => lines ?? (lines = EntryHeader.MergedLines.OrderBy(x => x.CL_LineNumber).ThenBy(x => x.PK).Select(x => new EXPDATLineProvider(x, this)).ToArray());
		IReadOnlyCollection<IEXPDATLine> lines;

		public ZString RegistrationNumber => Declaration.JE_TransportMode == Core.Constants.TransportModes.Sea || Declaration.JE_TransportMode == Core.Constants.TransportModes.Air ? Declaration.JE_MasterBill : ZString.Empty;

		public ZString Annotation => EntryInstruction.AdditionalInformation;

		public ZInt GoodsItemQuantity => EntryHeader.MergedLines.Count;

		public ZString LocalClearanceOutwardProcessingIDNumber => ZString.Empty;

		public ZString OutwardProcessingIDNumber => CachedValueHelper.GetValue(ref outwardProcessingIDNumber, () => ZString.Empty);
		CachedValue<ZString> outwardProcessingIDNumber;

		public ZString AccreditedExporterIDNumber => ZString.Empty;

		public IGoodsLoadingPlace GoodsLoadingPlace => CachedValueHelper.GetValue(ref goodsLoadingPlaceCached, () => GoodsLoadingPlaceProvider.NewOrNull(EntryInstruction, Declaration.SupplierPickupAddress.Address));
		CachedValue<IGoodsLoadingPlace> goodsLoadingPlaceCached;

		public IReadOnlyCollection<ZString> ItineraryCountries => itineraryCountries ?? (itineraryCountries = GetItineraryCountriesCore().ToArray());
		IReadOnlyCollection<ZString> itineraryCountries;

		IEnumerable<ZString> GetItineraryCountriesCore()
		{
			var country = Declaration.JE_RL_NKOrigin.Left(2);
			if (!country.IsEmpty)
			{
				yield return country;
			}
			var transports = Declaration.Shipment?.Transports ?? Declaration.Transports;
			if (transports.Count > 0)
			{
				foreach (var transportCountry in transports.Cast<Transport>()
					.OrderBy(x => x.JW_LegOrder)
					.ThenBy(x => x.PK)
					.Select(x => x.JW_RL_NKDiscPort.Left(2))
					.Where(x => !x.IsEmpty))
				{
					if (country != transportCountry)
					{
						country = transportCountry;
						yield return country;
					}
				}
			}

			var finalDestinationCountry = Declaration.JE_RL_NKFinalDestination.Left(2);
			if (!finalDestinationCountry.IsEmpty && finalDestinationCountry != country)
			{
				yield return finalDestinationCountry;
			}

			country = Declaration.JE_GoodsDestination;
			if (country.In(new ZString[] { CountryCodes.CountryCodeQP, CountryCodes.CountryCodeQQ, CountryCodes.CountryCodeQR, CountryCodes.CountryCodeQS, CountryCodes.CountryCodeQU, CountryCodes.CountryCodeQV, CountryCodes.CountryCodeQW, CountryCodes.CountryCodeQX, CountryCodes.CountryCodeQY, CountryCodes.CountryCodeQZ }))
			{
				yield return country;
			}
		}

		public IDateTimeRange PresentationPackingLoading => null;

		public IAESParty Contractor => CachedValueHelper.GetValue(ref contractorCached, () => EntryInstruction.Constellation3rdDigitIs1() ? PartyProvider.NewOrNull(Declaration.SellerAddress, Declaration.Seller?.ContactsActive?.Cast<OrgContact>().FirstOrDefault()) : null);
		CachedValue<IAESParty> contractorCached;

		public IAESParty OutwardProcessingOwner => CachedValueHelper.GetValue(ref outwardProcessingOwnerCached, () => EntryInstruction.Constellation4thDigitIs3() ? PartyProvider.NewOrNull(Declaration.ManufacturerAddress, Declaration.Manufacturer?.ContactsActive?.Cast<OrgContact>().FirstOrDefault()) : null);
		CachedValue<IAESParty> outwardProcessingOwnerCached;

		public string InternalCurrency => string.Empty;

		public decimal ExchangeRate => 0;

		public string DeferredPayment => string.Empty;

		public string WarehouseType => string.Empty;

		public string WarehouseIdentifier => string.Empty;

		protected override IDeliveryTerms GetDeliveryTermsFromInvoice(JobComInvoiceHeader invoice)
		{
			return new AESHeaderDeliveryTermsProvider(invoice);
		}
	}
}
