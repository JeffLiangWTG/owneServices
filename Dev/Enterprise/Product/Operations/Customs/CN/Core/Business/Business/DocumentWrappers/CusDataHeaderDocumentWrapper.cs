using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business
{
	public class CusDataHeaderDocumentWrapper : DocumentWrapper, IBODocDataProvider, IDocumentWrapper
	{
		public CusDataHeaderDocumentWrapper(CusEntryHeader entryHeader) : base(entryHeader, entryHeader.Factory)
		{
			cusDataHeader = entryHeader;
		}
		readonly ICustomsEntryHeader cusDataHeader;
		public CusEntryHeader EntryHeader => ParentBusinessObject as CusEntryHeader;

		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => EntryHeader;
		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;
		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
		string IBODocDataProvider.ToString() => EntryHeader.HumanReadableName;
		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);
		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);
		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));

		IBODocDataProvider basicBODocDataProvider;

		#endregion

		#region Entry Numbers

		public ZString EntryNumber => cusDataHeader.EntryNumber;
		public ZString EntryNumberSpaced => EntryNumber.ToString().ToCharArray().JoinAsString(" ", false);
		public ZString LocalReferenceNumber => cusDataHeader.LocalReferenceNumber;
		public ZString DeclarationUnifiedNumber => cusDataHeader.DeclarationUnifiedNumber;
		public ZString PreEntryNumber => cusDataHeader.PreEntryNumber;

		#endregion

		#region Parties

		public AddressWrapper TradeParty => fTradeParty ?? (fTradeParty = new AddressWrapper(EntryHeader.TradeParty, Factory));
		AddressWrapper fTradeParty;
		public AddressWrapper CargoOwner => fCargoOwner ?? (fCargoOwner = new AddressWrapper(EntryHeader.CargoOwner, Factory));
		AddressWrapper fCargoOwner;
		public AddressWrapper Declarant => fDeclarant ?? (fDeclarant = new AddressWrapper(EntryHeader.Declarant.MainAddress, ContactType.NoContactType, Factory));
		AddressWrapper fDeclarant;
		public AddressWrapper OverseasParty => fOverseasParty ?? (fOverseasParty = new AddressWrapper(EntryHeader.OverseasParty, Factory));
		AddressWrapper fOverseasParty;

		public ZString TradePartyName => cusDataHeader.TradePartyName;
		public ZString TradePartyCCD => cusDataHeader.TradePartyCCD;
		public ZString TradePartyUSCI => cusDataHeader.TradePartyUSCI;
		public ZString TradePartyCIQ => cusDataHeader.TradePartyCIQ;
		public ZString TradePartyEnglishName => EntryHeader.TradeParty?.EnglishCompanyName ?? ZString.Empty;
		public ZString CargoOwnerName => cusDataHeader.CargoOwnerName;
		public ZString CargoOwnerCCD => cusDataHeader.CargoOwnerCCD;
		public ZString CargoOwnerUSCI => cusDataHeader.CargoOwnerUSCI;
		public ZString CargoOwnerCIQ => cusDataHeader.CargoOwnerCIQ;
		public ZString DeclarantName => cusDataHeader.DeclarantName;
		public ZString DeclarantCCD => cusDataHeader.DeclarantCCD;
		public ZString DeclarantUSCI => cusDataHeader.DeclarantUSCI;
		public ZString DeclarantCIQ => cusDataHeader.DeclarantCIQ;
		public ZString OverseasPartyCode => cusDataHeader.OverseasPartyCode;
		public ZString OverseasPartyName => cusDataHeader.OverseasPartyName;
		public ZString OverseasPartyChineseName => EntryHeader.OverseasParty?.ChineseCompanyName ?? ZString.Empty;

		#endregion

		#region Dates

		public ZDateTime ImportOrExportDate => cusDataHeader.ImportOrExportDate;
		public ZDateTime DeclarantDate => cusDataHeader.DeclarantDate;

		public ZDateTime DepartureDate => cusDataHeader.DepartureDate;
		public ZDateTime DateOfUnloadComplete => cusDataHeader.DateOfUnloadComplete;

		#endregion

		#region Code and Description

		public CodeAndDescriptionWrapper CustomsOffice => fCustomsOffice ?? (fCustomsOffice = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.CustomsOfficeCode, RefCusCodeListTypesCodes.CustomsOffice, cusDataHeader.DateOfValuation));
		CodeAndDescriptionWrapper fCustomsOffice;

		public CodeAndDescriptionWrapper OfficeOfEntryOrExit => fOfficeOfEntryOrExit ?? (fOfficeOfEntryOrExit = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.OfficeOfEntryOrExitCode, RefCusCodeListTypesCodes.CustomsOffice, cusDataHeader.DateOfValuation));
		CodeAndDescriptionWrapper fOfficeOfEntryOrExit;

		public CodeAndDescriptionWrapper CIQOfficeOfEntryOrExit => fCIQOfficeOfEntryOrExit ?? (fCIQOfficeOfEntryOrExit = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.CIQOfficeOfEntryOrExitCode, RefCusCodeListTypesCodes.CNCIQPortOffices, cusDataHeader.DateOfValuation));
		CodeAndDescriptionWrapper fCIQOfficeOfEntryOrExit;

		public CodeAndDescriptionWrapper TransportMode => fTransportMode ?? (fTransportMode = CodeAndDescriptionWrapper.New(cusDataHeader.TransportModeCode, Factory.GetCachedValue<CNTransportModeList>(), Factory));
		CodeAndDescriptionWrapper fTransportMode;

		public CodeAndDescriptionWrapper CustomsProcedure
		{
			get
			{
				if (fCustomsProcedure == null)
				{
					var code = cusDataHeader.CustomsProcedureCode;
					fCustomsProcedure = CodeAndDescriptionWrapper.New(code, CNRefCusProcedure.GetRefCusProcedure(Factory, code, cusDataHeader.DateOfValuation)?.ZZ6_Description ?? ZString.Empty, Factory);
				}
				return fCustomsProcedure;
			}
		}
		CodeAndDescriptionWrapper fCustomsProcedure;

		public CodeAndDescriptionWrapper LevyType => fLevyType ?? (fLevyType = CodeAndDescriptionWrapper.New(cusDataHeader.LevyTypeCode, Factory.GetCachedValue<LevyTypeList>(), Factory));
		CodeAndDescriptionWrapper fLevyType;

		public CodeAndDescriptionWrapper CountryOfTrade => fCountryOfTrade ?? (fCountryOfTrade = EntryHeader.CountryOfTrade.CreateCodeAndDescriptionWrapper(Factory));
		CodeAndDescriptionWrapper fCountryOfTrade;

		public CodeAndDescriptionWrapper CountryOfLoadOrDischarge => fCountryOfLoadOrDischarge ?? (fCountryOfLoadOrDischarge = EntryHeader.CountryOfLoadOrDischarge.CreateCodeAndDescriptionWrapper(Factory));
		CodeAndDescriptionWrapper fCountryOfLoadOrDischarge;

		public CodeAndDescriptionWrapper PortOfOriginOrDest => fPortOfOriginOrDest ?? (fPortOfOriginOrDest = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.PortOfOriginOrDestCode, RefCusCodeListTypesCodes.Port, cusDataHeader.DateOfValuation));
		CodeAndDescriptionWrapper fPortOfOriginOrDest;

		public CodeAndDescriptionWrapper PortOfStopover => fPortOfStopover ?? (fPortOfStopover = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.PortOfStopoverCode, RefCusCodeListTypesCodes.Port, cusDataHeader.DateOfValuation));
		CodeAndDescriptionWrapper fPortOfStopover;

		public CodeAndDescriptionWrapper IncoTerm => fIncoTerm ?? (fIncoTerm = CodeAndDescriptionWrapper.New(cusDataHeader.IncoTermCode, Factory.GetCachedValue<ShipmentIncoTerm>(), Factory));
		CodeAndDescriptionWrapper fIncoTerm;

		public CodeAndDescriptionWrapper DocumentSubmissionType => fDocumentSubmissionType ?? (fDocumentSubmissionType = CodeAndDescriptionWrapper.New(cusDataHeader.DocumentSubmissionTypeCode, Factory.GetCachedValue<EntryDocumentSubmissionTypes>(), Factory));
		CodeAndDescriptionWrapper fDocumentSubmissionType;

		#endregion

		#region Fees

		public CustomsFeeWrapper FreightFee => fFreightFee ?? (fFreightFee = new CustomsFeeWrapper(EntryHeader.FreightFee, cusDataHeader.DateOfValuation, Factory));
		CustomsFeeWrapper fFreightFee;

		public CustomsFeeWrapper InsuranceFee => fInsuranceFee ?? (fInsuranceFee = new CustomsFeeWrapper(EntryHeader.InsuranceFee, cusDataHeader.DateOfValuation, Factory));
		CustomsFeeWrapper fInsuranceFee;

		public CustomsFeeWrapper OtherFee => fOtherFee ?? (fOtherFee = new CustomsFeeWrapper(EntryHeader.OtherFee, cusDataHeader.DateOfValuation, Factory));
		CustomsFeeWrapper fOtherFee;

		#endregion

		#region Weights & Packs

		public ZDecimal GrossWeightInKG => cusDataHeader.GrossWeightInKG;
		public ZDecimal NetWeightInKG => cusDataHeader.NetWeightInKG;

		public ZInt NoOfPacks => cusDataHeader.NoOfPacks;

		public CodeAndDescriptionWrapper PackType => fPackType ?? (fPackType = CodeAndDescriptionWrapper.New(cusDataHeader.PackTypeCode, Factory.GetCachedValue<PackageType>(), Factory));
		CodeAndDescriptionWrapper fPackType;

		public CodeAndDescriptionWrapperCollection OtherPackages => fOtherPackages ?? (fOtherPackages = CodeAndDescriptionWrapperCollection.New(cusDataHeader.OtherPackageCodes, Factory.GetCachedValue<PackageType>(), Factory));
		CodeAndDescriptionWrapperCollection fOtherPackages;

		public CodeAndDescriptionWrapperCollection SpecialBusinessIdentifiers => fSpecialBusinessIdentifiers ?? (fSpecialBusinessIdentifiers = CodeAndDescriptionWrapperCollection.New(cusDataHeader.SpecialBusinessIdentifiers, Factory.GetCachedValue<SpecialBusinessList>(), Factory));
		CodeAndDescriptionWrapperCollection fSpecialBusinessIdentifiers;

		#endregion

		#region Confirms

		public CodeAndDescriptionWrapper SpecialRelationshipConfirm => fSpecialRelationshipConfirm ?? (fSpecialRelationshipConfirm = CodeAndDescriptionWrapper.New(cusDataHeader.SpecialRelationshipConfirmCode, Factory.GetCachedValue<ConfirmationTypeList>(), Factory));
		public CodeAndDescriptionWrapper fSpecialRelationshipConfirm;

		public CodeAndDescriptionWrapper PriceAffectConfirm => fPriceAffectConfirm ?? (fPriceAffectConfirm = CodeAndDescriptionWrapper.New(cusDataHeader.PriceAffectConfirmCode, Factory.GetCachedValue<ConfirmationTypeList>(), Factory));
		public CodeAndDescriptionWrapper fPriceAffectConfirm;

		public CodeAndDescriptionWrapper PaymentOfRoyaltyConfirm => fPaymentOfRoyaltyConfirm ?? (fPaymentOfRoyaltyConfirm = CodeAndDescriptionWrapper.New(cusDataHeader.PaymentOfRoyaltyConfirmCode, Factory.GetCachedValue<ConfirmationTypeList>(), Factory));
		public CodeAndDescriptionWrapper fPaymentOfRoyaltyConfirm;

		public CodeAndDescriptionWrapper IsPaperlessTaxForm => fIsPaperlessTaxForm ?? (fIsPaperlessTaxForm = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.IsPaperlessTaxForm));
		CodeAndDescriptionWrapper fIsPaperlessTaxForm;

		public CodeAndDescriptionWrapper IsAutonomousTaxFiling => fIsAutonomousTaxFiling ?? (fIsAutonomousTaxFiling = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.IsAutonomousTaxFiling));
		CodeAndDescriptionWrapper fIsAutonomousTaxFiling;

		public CodeAndDescriptionWrapper IsAssuredInspectClearance => fAssuredInspectClearance ?? (fAssuredInspectClearance = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.IsAssuredInspectClearance));
		CodeAndDescriptionWrapper fAssuredInspectClearance;

		#endregion

		#region Pricing Confirms

		public CodeAndDescriptionWrapper FormulaPricingConfirm => fFormulaPricingConfirm ?? (fFormulaPricingConfirm = CodeAndDescriptionWrapper.New(cusDataHeader.FormulaPricingConfirmCode, Factory.GetCachedValue<ConfirmationTypeList>(), Factory));
		CodeAndDescriptionWrapper fFormulaPricingConfirm;

		public CodeAndDescriptionWrapper TemporaryPricingConfirm => fTemporaryPricingConfirm ?? (fTemporaryPricingConfirm = CodeAndDescriptionWrapper.New(cusDataHeader.TemporaryPricingConfirmCode, Factory.GetCachedValue<ConfirmationTypeList>(), Factory));
		CodeAndDescriptionWrapper fTemporaryPricingConfirm;

		#endregion

		#region References

		public ZString VesselName => cusDataHeader.VesselName;
		public ZString Voyage => cusDataHeader.Voyage;
		public ZString LocationOfGoods => cusDataHeader.LocationOfGoods;
		public ZString BillOfLading => cusDataHeader.BillOfLading;

		public ZString ManualNo => cusDataHeader.ManualNo;
		public ZString LicenseNo => cusDataHeader.LicenseNo;
		public ZString ContractNo => cusDataHeader.ContractNo;
		public ZString BondedAreaCode => cusDataHeader.BondedAreaCode;
		public ZString FreightYardCode => cusDataHeader.FreightYardCode;
		public ZString Remarks => cusDataHeader.Remarks;
		public ZString MarksAndNumbers => cusDataHeader.MarksAndNumbers;

		public ZString RelatedEntryNumber => cusDataHeader.RelatedEntryNumber;
		public ZString RelatedManualNumber => cusDataHeader.RelatedManualNumber;

		#endregion

		#region CIQ Data

		public CodeAndDescriptionWrapper OfficeOfDestination => fOfficeOfDestination ?? (fOfficeOfDestination = CodeAndDescriptionWrapper.New(Factory, cusDataHeader.OfficeOfDestination, RefCusCodeListTypesCodes.CustomsOffice, cusDataHeader.DateOfValuation));
		CodeAndDescriptionWrapper fOfficeOfDestination;

		public CodeAndDescriptionWrapper IsOriginalContainerLoading => fIsOriginalContainerLoading ?? (fIsOriginalContainerLoading = CodeAndDescriptionWrapper.New(cusDataHeader.IsOriginalContainerLoading, Factory.GetCachedValue<ConfirmationTypeList>(), Factory));
		public CodeAndDescriptionWrapper fIsOriginalContainerLoading;

		public ZString BillNumber => cusDataHeader.BillNumber;
		public ZString CIQRelatedNumber => cusDataHeader.CIQRelatedNumber;
		public CodeAndDescriptionWrapper CIQRelatedReason => fCIQRelatedReason ?? (fCIQRelatedReason = CodeAndDescriptionWrapper.New(cusDataHeader.CIQRelatedReason, Factory.GetCachedValue<CIQRelation>(), Factory));
		CodeAndDescriptionWrapper fCIQRelatedReason;

		public ZString ConsumerContactName => cusDataHeader.ConsumerContactName;
		public ZString ConsumerContactPhone => cusDataHeader.ConsumerContactPhone;

		#endregion

		#region Collections

		public BusinessObjectCollectionWrapper<CusDataLineDocumentWrapper> Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new BusinessObjectCollectionWrapper<CusDataLineDocumentWrapper>(EntryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new CusDataLineDocumentWrapper(x)));
				}
				return fLines;
			}
		}
		BusinessObjectCollectionWrapper<CusDataLineDocumentWrapper> fLines;

		public BusinessObjectCollectionWrapper<EntryHeaderContainer> Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new BusinessObjectCollectionWrapper<EntryHeaderContainer>(EntryHeader.EntryHeaderContainers.Cast<EntryHeaderContainer>());
				}
				return fContainers;
			}
		}
		BusinessObjectCollectionWrapper<EntryHeaderContainer> fContainers;

		public BusinessObjectCollectionWrapper<EntryHeaderSupportingDocument> SupportingDocuments
		{
			get
			{
				if (fSupportingDocuments == null)
				{
					fSupportingDocuments = new BusinessObjectCollectionWrapper<EntryHeaderSupportingDocument>(EntryHeader.SupportingDocuments.Cast<EntryHeaderSupportingDocument>());
				}
				return fSupportingDocuments;
			}
		}
		BusinessObjectCollectionWrapper<EntryHeaderSupportingDocument> fSupportingDocuments;

		public IEnumerable<EntryHeaderSupportingDocument> SupportingDocumentsWithoutCOO => SupportingDocuments.Cast<EntryHeaderSupportingDocument>().Where(x => !x.IsCertificateOfOrigin);

		public IEnumerable<EnterpriseQualification> EnterpriseQualifications => cusDataHeader.EnterpriseQualifications.Cast<EnterpriseQualification>() ?? Enumerable.Empty<EnterpriseQualification>();

		public BusinessObjectCollectionWrapper<EntryHeaderRequiredDocument> RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new BusinessObjectCollectionWrapper<EntryHeaderRequiredDocument>(EntryHeader.RequiredDocuments.Cast<EntryHeaderRequiredDocument>());
				}
				return fRequiredDocuments;
			}
		}
		BusinessObjectCollectionWrapper<EntryHeaderRequiredDocument> fRequiredDocuments;

		public BusinessObjectCollectionWrapper<EntryLineVINData> VINs
		{
			get
			{
				if (fVINs == null)
				{
					var vins = Lines.Cast<CusDataLineDocumentWrapper>().OrderBy(x => x.EntryLineNo)
						.SelectMany(x => x.ProductQualifications).Cast<EntryLineProductQualification>().OrderBy(x => x.Sequence)
						.SelectMany(x => x.VINs).Cast<EntryLineVINData>();

					int sequence = 1;
					fVINs = new BusinessObjectCollectionWrapper<EntryLineVINData>(vins);
					fVINs.Cast<EntryLineVINData>().ForEach(vin => vin.Sequence = sequence++);
				}
				return fVINs;
			}
		}
		BusinessObjectCollectionWrapper<EntryLineVINData> fVINs;

		#endregion

		#region For Customs Invoice Document

		public JobComInvoiceHeader InvoiceHeader => EntryHeader.RandomHeader;

		public AddressDocWrapper InvoiceOwner => EntryHeader.IsEntering ? SoldTo : Seller;

		public AddressDocWrapper SoldTo => fSoldTo ?? (fSoldTo = CreateAddressDocWrapper(OrganisationUsageType.Buyer, InvoiceHeader?.Buyer?.MainAddress, InvoiceHeader?.JobDeclaration?.ImporterDocumentaryAddress));
		AddressDocWrapper fSoldTo;

		public AddressDocWrapper Seller => fSeller ?? (fSeller = CreateAddressDocWrapper(OrganisationUsageType.Supplier, InvoiceHeader?.Supplier?.MainAddress, InvoiceHeader?.JobDeclaration?.SupplierDocumentaryAddress));
		AddressDocWrapper fSeller;

		AddressDocWrapper CreateAddressDocWrapper(OrganisationUsageType usageType, OrgAddress addressOnInvoice, JobDocAddress addressOnDeclaration)
		{
			return addressOnInvoice != null ? new AddressDocWrapper(usageType, addressOnInvoice, Factory) : new AddressDocWrapper(usageType, addressOnDeclaration, Factory);
		}

		public ZDateTime InvoiceDate => InvoiceHeader?.JZ_InvoiceDate ?? ZDateTime.Empty;

		public ZString DeliveryTerm => (InvoiceHeader?.JobDeclaration?.WillGenerateBothEntries ?? true) ? ZString.Empty : IncoTerm.Description;

		public Image ClientLogo
		{
			get
			{
				if (fClientLogo == null)
				{
					var clientPK = InvoiceHeader == null ? ZGuid.Empty : (EntryHeader.IsEntering ? InvoiceHeader.JZ_OH_Buyer_Effective : InvoiceHeader.JZ_OH_Supplier_Effective);
					fClientLogo = clientPK.IsValid
					? SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(clientPK.ToGuid(), Guid.Empty, Guid.Empty)
					: SystemDataRegistry.Instance.CompanyLogo.Value;
				}
				return fClientLogo;
			}
		}
		Image fClientLogo;

		#endregion
	}
}
