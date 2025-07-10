using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AsycudaManifestHeader : EU.H7.Business.AsycudaManifestHeader, Integration.Customs.ESH7.IAsycudaManifestHeader, ICusGoodsLocationProvider, ILRNGenerator, IESMessageInfoProvider, IESResponseBOMessageStatus
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Spain;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		#region GenAddOn

		public static class GenAddOnColumnConstants
		{
			public const string TrainingEntryColumnName = "ES_H7_TrainingEntry";
			public const string EntryLineNumberColumnName  = "ES_H7_DSDTEntryLine";
			public const int EntryLineNumberMaxLength  = 5;
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetDefaultTrainingEntry();
		}

		#region Properties

		[ResourceStringData("Enterprise.Customs.ES.H7.Business.AsycudaManifestHeader.AMA_AgentType", Caption = "Rep. Status", ShortCaption = "Rep. Status", MediumCaption = "Rep. Status", FullDescription = "The relevant code representing the status of the representative.")]
		public override ZString AMA_AgentType
		{
			get => base.AMA_AgentType;
			set => base.AMA_AgentType = value;
		}

		[ResourceStringData("Enterprise.Customs.ES.H7.Business.AsycudaManifestHeader.AMA_MasterInformation", Caption = "DSDT MRN/Flight No.", ShortCaption = "DSDT MRN/Flight No.", MediumCaption = "DSDT MRN/Flight No.", FullDescription = "Flight number or Movement Reference Number of the associated Discharge Summary Declaration.")]
		[MaxLength(70)]
		public override ZString AMA_MasterInformation
		{
			get => base.AMA_MasterInformation;
			set => base.AMA_MasterInformation = value;
		}

		[ResourceStringData("06f92a08-9269-4f45-953a-8c3d5c4c719c", Caption = "Broker")]
		public override ZString AMA_GS_NKCustomsAgent
		{
			get => base.AMA_GS_NKCustomsAgent;
			set
			{
				var oldValue = base.AMA_GS_NKCustomsAgent;
				base.AMA_GS_NKCustomsAgent = value;

				if (!IsCopying && oldValue != value)
				{
					SetDefaultAMA_CustomsProfile();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CertificateNames))]
		[ResourceStringData("fe3e697c-9326-461a-a9c3-5dc75edcc6ad", Caption = "Certificate", ShortCaption = "Cert.", MediumCaption = "Certif.", FullDescription = "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job.")]
		public override ZString AMA_CustomsProfile
		{
			get => base.AMA_CustomsProfile;
			set => base.AMA_CustomsProfile = value;
		}

		[ResourceStringData("e0662c02-9878-420e-ad4f-89972b827dbc", Caption = "Training Entry", ShortCaption = "Training Entry", MediumCaption = "Training Entry", FullDescription = "When checked the declaration will be sent to Test.")]
		public ZBool TrainingEntry
		{
			get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.TrainingEntryColumnName);
			set
			{
				var oldValue = TrainingEntry;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(GenAddOnColumnConstants.TrainingEntryColumnName, value);
					TrainingEntryInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo TrainingEntryInfo => GetZPropertyInfo(nameof(TrainingEntry));

		[ResourceStringData("2446da72-943d-424a-b4cb-9d337b7f241e", Caption = "Entry Line Number", MediumCaption = "Ent. Line No.", ShortCaption = "E. Line No.", FullDescription = "The Entry Line Number within the DSDT.")]
		[MaxLength(GenAddOnColumnConstants.EntryLineNumberMaxLength)]
		public ZString EntryLineNumber
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.EntryLineNumberColumnName);
			set
			{
				var oldValue = EntryLineNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(EntryLineNumberInfo, value);
					this.SetSystemDefinedValue(GenAddOnColumnConstants.EntryLineNumberColumnName, value);
					EntryLineNumberInfo.RefreshBinding(oldValue);
					if (!base.IsValidationSuspended)
					{
						Validation.ValidateEntryLineNumber();
					}
				}
			}
		}

		public ZPropertyInfo EntryLineNumberInfo => GetZPropertyInfo(nameof(EntryLineNumber));

		[MaxLength(35)]
		[List(nameof(Lookups) + "." + nameof(Lookups.G3MRNToRevokeList))]
		[ResourceStringData("f3e422b9-4821-4910-b4e5-2242ae8527cf", Caption = "G3 MRN To Revoke", ShortCaption = "G3 MRN Rev.", MediumCaption = "G3 MRN Revoke", FullDescription = "MRN of the G3 Declaration linked to the H7 bills intending to revoke.")]
		public ZString G3MRNToRevoke
		{
			get
			{
				if(g3MRNToRevoke == null || g3MRNToRevoke.IsDeleted)
				{
					g3MRNToRevoke = LoadOrCreateG3MRNToRevoke();
					RegisterEditableChildObject(g3MRNToRevoke);
				}

				return g3MRNToRevoke?.CE_EntryNum ?? ZString.Empty;
			}
			set
			{
				var currentG3MRNToRevoke = g3MRNToRevoke;
				var oldValue = currentG3MRNToRevoke?.CE_EntryNum ?? ZString.Empty;
				if (oldValue != value)
				{
					if (currentG3MRNToRevoke == null)
					{
						currentG3MRNToRevoke = LoadOrCreateG3MRNToRevoke(true);
					}

					currentG3MRNToRevoke.CE_EntryNum = value;
					G3MRNToRevokeInfo.RefreshBinding(oldValue);
				}
			}
		}

		CusEntryNumber g3MRNToRevoke;

		public ZPropertyInfo G3MRNToRevokeInfo => GetZPropertyInfo(nameof(G3MRNToRevoke));

		const string G3ToRevoke = "G3TOREVOKE";

		#endregion

		#region Additional Documents

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalDocument);
			return result;
		}

		protected override EU.H7.Business.IAdditionalDocumentCollection<EU.H7.Business.AdditionalDocument> CreateNewAdditionalDocumentCollection() => new EU.H7.Business.AdditionalDocumentCollection<AdditionalDocument>(this);

		public new EU.H7.Business.IAdditionalDocumentCollection<AdditionalDocument> AdditionalDocuments => (EU.H7.Business.IAdditionalDocumentCollection<AdditionalDocument>)base.AdditionalDocuments;

		#endregion

		#region Transport Document

		[MaxLength(70)]
		[ResourceStringData("Enterprise.Customs.ES.H7.Business.AsycudaManifestHeader.TransportDocumentReference", Caption = "Transport Document Reference", ShortCaption = "T.Doc.Ref.", MediumCaption = "T. Doc. Reference")]
		public ZString TransportDocumentReference
		{
			get => TransportDocument?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				var currentTransportDocument = TransportDocument;
				var oldValue = currentTransportDocument?.CSI_ReferenceNumber ?? ZString.Empty;
				if (oldValue != value)
				{
					if (currentTransportDocument == null)
					{
						currentTransportDocument = LoadTransportDocument(createIfMissing: true);
					}
					CheckMaximumLength(TransportDocumentReferenceInfo, value);
					TransportDocument.CSI_ReferenceNumber = value;
					TransportDocumentReferenceInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TransportDocumentReferenceInfo => TransportDocument == null ?
			GetZPropertyInfo(nameof(TransportDocumentReference)) :
			GetWrappedZPropertyInfo(nameof(TransportDocumentReference), x => TransportDocument.CSI_ReferenceNumberInfo);

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TransportDocumentTypes))]
		[ResourceStringData("Enterprise.Customs.ES.H7.Business.AsycudaManifestHeader.TransportDocumentType", Caption = "Transport Document Type", ShortCaption = "T.Doc.Type", MediumCaption = "T. Doc. Type")]
		public ZString TransportDocumentType
		{
			get => TransportDocument?.CSI_Code ?? ZString.Empty;
			set
			{
				var currentTransportDocument = TransportDocument;
				var oldValue = currentTransportDocument?.CSI_Code ?? ZString.Empty;
				if (oldValue != value)
				{
					if (currentTransportDocument == null)
					{
						currentTransportDocument = LoadTransportDocument(createIfMissing: true);
					}
					TransportDocument.CSI_Code = value;
					TransportDocumentTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TransportDocumentTypeInfo => TransportDocument == null ?
			GetZPropertyInfo(nameof(TransportDocumentType)) :
			GetWrappedZPropertyInfo(nameof(TransportDocumentType), x => TransportDocument.CSI_CodeInfo);

		AdditionalDocument TransportDocument => transportDocument ??= LoadTransportDocument(createIfMissing: false);

		AdditionalDocument LoadTransportDocument(bool createIfMissing = true)
		{
			transportDocument = AdditionalDocuments.FirstOrDefault(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument);
			if (transportDocument == null && createIfMissing)
			{ 
				transportDocument = AdditionalDocuments.AddNew();
				RegisterEditableChildObject(transportDocument);
				transportDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			}
			return transportDocument;
		}

		AdditionalDocument transportDocument;

		#endregion

		#region ICusGoodsLocationProvider

		public CusGoodsLocation CusGoodsLocation
		{
			get
			{
				if (cusGoodsLocation == null)
				{
					cusGoodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Departure);
					RegisterEditableChildObject(cusGoodsLocation);
				}
				return cusGoodsLocation;
			}
		}
		CusGoodsLocation cusGoodsLocation;

		[ResourceStringData("82364d20-d958-4695-87e4-9dde083d8038", Caption = "Location of Goods (G3)", ShortCaption = "Location of Goods (G3)", MediumCaption = "Location of Goods (G3)", FullDescription = "Location where the goods may be examined. The location must be precise enough to allow Customs to carry out the physical control of the goods.")]
		public ZString GoodsLocationDescription
		{
			get
			{
				if (cusGoodsLocation == null || cusGoodsLocation.IsDeleted)
				{
					cusGoodsLocation = Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Departure);
					if (cusGoodsLocation != null)
					{
						RegisterEditableChildObject(cusGoodsLocation);
					}
				}
				return cusGoodsLocation?.DisplayText ?? ZString.Empty;
			}
		}

		EU.Business.CusGoodsLocation ICusGoodsLocationProvider.GoodsLocation => CusGoodsLocation;

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		ZString CountryCode => !AMA_RN_NKCountry.IsEmpty ? AMA_RN_NKCountry : ZString.Empty;

		public ZString ProviderKey => CountryCode + GoodsLocationProviderApplications.Codes.H7Declaration;

		public void ValidateGoodsLocationDescription()
		{
			if (Validation is AsycudaManifestHeaderValidation manifestHeaderValidation)
			{
				manifestHeaderValidation.ValidateGoodsLocationDescription();
			}
		}

		#endregion

		#region ILRNGenerator

		public INumberFountainProxy LrnNumberFountain => Environment.Env.NumberFountains.G3LocalReferenceNumber(Branch.Company.PK.ToGuid());

		#endregion

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var asycudaManifestHeader = (AsycudaManifestHeader)base.CloneInternal(args);

			var clonedGoodsLocation = (CusGoodsLocation)CusGoodsLocation.Clone();
			clonedGoodsLocation.CGL_ParentID = asycudaManifestHeader.PK;

			return asycudaManifestHeader;
		}

		public new EU.H7.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (EU.H7.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		protected override EU.H7.Business.IAsycudaBillCollection<EU.H7.Business.AsycudaBill, EU.H7.Business.AsycudaManifestHeader> CreateNewEUH7AsycudaBillCollection() => new EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		public bool LodgementCustomsOfficeInCanaryIsland => Factory.GetValue(ref lodgementCustomsOfficeInCanaryIslandCached, () => AMA_CustomsOffice.StartsWith(CustomOfficeCanaryIslandWith35)
		|| AMA_CustomsOffice.StartsWith(CustomOfficeCanaryIslandWith38) || AMA_CustomsOffice.Equals(CustomOfficeCanaryIslandTest));

		CachedProperty<bool> lodgementCustomsOfficeInCanaryIslandCached;

		const string CustomOfficeCanaryIslandWith35 = "ES0035";
		const string CustomOfficeCanaryIslandWith38 = "ES0038";
		const string CustomOfficeCanaryIslandTest = "ES009998";

		void SetDefaultAMA_CustomsProfile()
		{
			var certificateNamesList = Lookups.CertificateNames;
			var customsProfile = AMA_CustomsProfile;
			if (customsProfile.IsEmpty || !certificateNamesList.GetAllCodesZString().Contains(customsProfile))
			{
				if (certificateNamesList.Count == 1)
				{
					AMA_CustomsProfile = certificateNamesList[0].Code;
				}
				else
				{
					AMA_CustomsProfile = ZString.Empty;
				}
			}
		}

		public CertificateObject Certificate => certificate ??= new CertificateObject(CustomsAgent, AMA_CustomsProfile, ZString.Empty);
		CertificateObject certificate;

		public CurrencyConverter CurrencyConverter
		{
			get
			{
				currencyConverter ??= CurrencyConverter.New(Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, true);
				return currencyConverter;
			}
		}

		public GlbStaff Broker => CustomsAgent;

		public ZString MRN => G3ToRevoke;

		public ZString DocumentJobReference => G3ToRevoke;

		public ZString EntryReference => AMA_JobReference;

		CurrencyConverter currencyConverter;

		[ChildEditable]
		public new EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = new G3EDIMessageCollection(this, Factory);
					ediMessages.Load();
					ediMessages.SetReadOnlyIncludingChildren(readOnly: true);
					RegisterEditableChildObject(ediMessages);
				}

				return ediMessages;
			}
		}

		EDIMessageCollection ediMessages;

		public bool IsAgentTypeINDOrDCA => Factory.GetValue(ref isAgentTypeINDOrDCACached, () => AMA_AgentType == EU.H7.Business.EUH7AgentTypes.Codes.IND || AMA_AgentType == ESH7AgentTypes.Codes.DCA);

		CachedProperty<bool> isAgentTypeINDOrDCACached;

		public bool IsAgentTypeDIROrICA => Factory.GetValue(ref isAgentTypeDIROrICACached, () => AMA_AgentType == EU.H7.Business.EUH7AgentTypes.Codes.DIR || AMA_AgentType == ESH7AgentTypes.Codes.ICA);

		CachedProperty<bool> isAgentTypeDIROrICACached;

		void DefaultCustomsAgentIfApplicable()
		{
			if (!IsInDatabase && AMA_GS_NKCustomsAgent.IsEmpty && !GlbStaff.CurrentUser.GS_IsSystemAccount)
			{
				AMA_GS_NKCustomsAgent = GlbStaff.CurrentUser.GS_Code;
			}
		}

		CusEntryNumber LoadOrCreateG3MRNToRevoke(bool createIfMissing = false)
		{
			return createIfMissing
				? CusEntryNumber.LoadOrCreate<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry, G3ToRevoke)
				: CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry, G3ToRevoke);
		}

		void SetDefaultTrainingEntry()
		{
			TrainingEntry = true;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			DefaultCustomsAgentIfApplicable();
		}
		#region IESResponseBOMessageStatus Members

		ZString IESResponseBOMessageStatus.MessageStatus { set => AMA_MessageStatus = value; }

		ZGuid IESResponseBusinessObject.BranchPK => Branch?.PK ?? GlbBranch.CurrentBranch.PK;

		EDIMessageCollection IESResponseBusinessObject.MessageCollection => Messages;

		#endregion
	}
}
