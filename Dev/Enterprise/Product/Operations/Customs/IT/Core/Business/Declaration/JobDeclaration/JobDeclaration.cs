using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobDeclaration : AutoITJobDeclaration
	, Integration.Customs.IT.IJobDeclaration
	, ICustomsProfileDataProvider
	, IDeclarantProvider
	, IAutHeaderWithCusOfficeProvider
	, IInvoicesProvider
	, IUCC6AndTransitionPeriodProvider
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.JobDeclaration.Schema
	{
		public const string ZG_PreClearing = "ZG_PreClearing";
		public new const int JE_CustomsOfficeMaxLength = 8;
		public new const int JE_LocationQualifierMaxLength = 2;
		public const int ImportJE_LocationOtherInformationMaxLength = 3;
		public const int JE_LocationOfGoodsAsCustomsOfficeMaxLength = 8;
		public const int JE_LocationOfGoodsAsCodeMaxLength = 7;
		public const int JE_LocationOfGoodsAsAuthorizedLocationMaxLength = 20;
		public const int ImportJE_SubLocationOfGoodsMaxLength = 2;
		public const int ZG_AuthorisationNumberDropEditMaxLength = 7;
		public new const int JE_CustomsProfileMaxLength = 20;
		public const string MessageVersion = "MessageVersion";
		public const int MessageVersionMaxLength = 3;
	}

	public static class GenAddOnColumnConstants
	{
		public const string MessageVersionColumnName = "IT_MessageVersion";
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.JobDeclaration|JE_CustomsProfile", Caption = "Node")]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.JobDeclaration|JE_CustomsProfile|UCC6", Caption = "Account", MultipleKey = JobDeclaration.CaptionKeyUCC)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.JobDeclaration|JE_CustomsProfile|Export|UCC6", Caption = "Account", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ProfileList))]
	[MaxLength(Schema.JE_CustomsProfileMaxLength)]
	public override ZString JE_CustomsProfile
	{
		get => base.JE_CustomsProfile;
		set
		{
			var oldValue = JE_CustomsProfile;
			base.JE_CustomsProfile = value;
			if (!IsCopying && oldValue != JE_CustomsProfile)
			{
				node = null;
				DefaultSubscriberFromNode();
			}
		}
	}

	public ZString Node => node ?? (node = CustomsCredentialHelper.GetNodeFromInternalCode(JE_CustomsProfile));
	string node;

	public override EU.Business.Declaration.EntryCreationStrategy CreateEntryCreationStrategy()
	{
		return new EntryCreationStrategy(this);
	}

	protected override ZString BarrierPortCore => IsExport ? JE_RL_NKPortOfLoading : base.BarrierPortCore;

	protected override ZBool IsReciprocalRatesCore => IsReciprocalRatesConstant;

	protected override bool ZG_AgreedPlaceCodeValidationSupportCore => false;

	protected override bool EUD_AgreedPlaceCodeValidationSupportCore => true;

	protected override ZBool AgreedPlaceCodeSupportAndVisibleCore => IsImport || (IsUCC6 && JE_ShipmentIncoTerm != Core.Constants.IncoTerms.Other);

	internal static bool IsReciprocalRatesConstant => false;

	[ResourceStringData("849FEFE5-F155-40B4-8352-E614A43B3E82", Caption = "Country of Export")]
	public override ZString JE_GoodsOrigin
	{
		get => base.JE_GoodsOrigin;
		set
		{
			var oldValue = JE_GoodsOrigin;
			base.JE_GoodsOrigin = value;
			if (!IsCopying && oldValue != JE_GoodsOrigin && !IsMarkingAsNeedingValidationSuspended)
			{
				MarkInvoiceLinesAsNeedingValidation();
			}
		}
	}

	[MaxLength(Schema.ImportJE_SubLocationOfGoodsMaxLength)]
	public ZString ImportJE_SubLocationOfGoods { get => base.JE_SubLocationOfGoods; set => base.JE_SubLocationOfGoods = value; }

	public ZPropertyInfo ImportJE_SubLocationOfGoodsInfo => JE_SubLocationOfGoodsInfo;

	[MaxLength(Schema.ImportJE_LocationOtherInformationMaxLength)]
	public ZString ImportJE_LocationOtherInformation { get => base.JE_LocationOtherInformation; set => base.JE_LocationOtherInformation = value; }

	[MaxLength(nameof(JE_LocationOfGoodsMaxLength))]
	public override ZString JE_LocationOfGoods
	{
		get => base.JE_LocationOfGoods;
		set
		{
			var oldValue = JE_LocationOfGoods;
			base.JE_LocationOfGoods = value;
			if (!IsCopying && oldValue != JE_LocationOfGoods)
			{
				DefaultJE_CustomsOfficeIfNeeded();
			}
		}
	}

	int JE_LocationOfGoodsMaxLength => IsImport
		&& ZG_AuthorisationNumber.IsEmpty
		&& JE_LocationQualifier == GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation
		? Schema.JE_LocationOfGoodsAsAuthorizedLocationMaxLength : Schema.JE_LocationOfGoodsAsCustomsOfficeMaxLength;

	protected override void LogEventIfJE_EntryStatusChangedCore()
	{
		bool shouldLogEntryStatusChange = JE_EntryStatus != ITEntryStatusList.Codes.ExportCleared && JE_EntryStatus != ITEntryStatusList.Codes.ImportCleared;

		if (shouldLogEntryStatusChange)
		{
			base.LogEventIfJE_EntryStatusChangedCore();
		}
	}

	protected override ZString LocalCurrencyCodeCore => LocalCurrencyConstantCode;

	internal static ZString LocalCurrencyConstantCode => Enterprise.Core.Constants.CurrencyCodes.Italy;

#if DEBUG

	protected override ZDateTime GetDateOfClearanceCore()// For CreateDeclarationForFetchHintTest test
	{
		return ZDateTime.BrettsBirthday;
	}

#endif

	protected override EUAddInfoValidation GetAddInfoJobComInvoiceLineValidationCore(EU.Business.Declaration.AddInfoJobComInvoiceLine addInfo) => new AddInfoJobComInvoiceLineValidation(addInfo);

	[MaxLength(Schema.JE_CustomsOfficeMaxLength)]
	[ReadOnlyMember(nameof(HasAtLeastOneEntryInAmendingStatus))]
	public override ZString JE_CustomsOffice
	{
		get => base.JE_CustomsOffice;
		set
		{
			var oldValue = JE_CustomsOffice;
			base.JE_CustomsOffice = value;
			if (!IsCopying && oldValue != JE_CustomsOffice)
			{
				RefreshPortTaxRateBinding();
			}
		}
	}

	#region GoodsLocationAddress

	public JobDocAddress GoodsLocationAddress
	{
		get
		{
			if (goodsLocationJobDocAddress == null || goodsLocationJobDocAddress.IsDeleted)
			{
				goodsLocationJobDocAddress = DocAddresses.FindOrCreateWithRequirement(GoodsLocationJobDocAddressRequirement);
			}
			return goodsLocationJobDocAddress;
		}
	}

	JobDocAddress goodsLocationJobDocAddress;

	JobDocAddressRequirement GoodsLocationJobDocAddressRequirement
	{
		get
		{
			if (goodsLocationJobDocAddressRequirement == null)
			{
				goodsLocationJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Location, ContactType.All);
				goodsLocationJobDocAddressRequirement.ValidateOrganisationPK = ValidateGoodsLocationTrader;
				goodsLocationJobDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateGoodsLocationAddress;
				goodsLocationJobDocAddressRequirement.CanOverride = false;
				DocAddressManager.AddRequirement(goodsLocationJobDocAddressRequirement);
			}
			return goodsLocationJobDocAddressRequirement;
		}
	}
	JobDocAddressRequirement goodsLocationJobDocAddressRequirement;

	protected override DocAddressType[] SupportedAddressTypesCore
	{
		get
		{
			var docAddressTypes = base.SupportedAddressTypesCore.ToList();
			docAddressTypes.Add(DocAddressType.Location);

			return docAddressTypes.ToArray();
		}
	}

	void ValidateGoodsLocationTrader(JobDocAddressValidation validation)
		=> GoodsLocationAddressValidation.ValidateOrganisation(GoodsLocationAddress.OrganisationPKInfo);

	void ValidateGoodsLocationAddress(JobDocAddressValidation validation)
		=> GoodsLocationAddressValidation.ValidateAddress(GoodsLocationAddress.E2_OA_AddressInfo);

	JobDeclarationGoodsLocationAddressValidation GoodsLocationAddressValidation => goodsLocationAddressValidation ?? (goodsLocationAddressValidation = new JobDeclarationGoodsLocationAddressValidation(this));
	JobDeclarationGoodsLocationAddressValidation goodsLocationAddressValidation;

	#endregion

	public override ZString JE_MessageType
	{
		get => base.JE_MessageType;
		set
		{
			var oldValue = JE_MessageType;
			base.JE_MessageType = value;
			if (!IsCopying && oldValue != JE_MessageType)
			{
				DefaultPreClearingIfNeeded();
				ClearZG_CTStatusIDIfImport();
				ApplyDeclarationOfIntentDefaulting();
				DefaultEntryInstructions();
				JobDeclarationFieldsCleaner.CleanUpMessageDependentFieldsIfNoLongerApplicable();
				MarkPackagesAsNeedingValidation();
				SetMessageVersionAsApplicable();
				RefreshPortTaxSupportingDocuments();
			}
		}
	}

	public override ZString JE_ApplicationCode
	{
		get => base.JE_ApplicationCode;
		set
		{
			var oldValue = JE_ApplicationCode;
			base.JE_ApplicationCode = value;
			if (!IsCopying && oldValue != JE_ApplicationCode)
			{
				SetMessageVersionAsApplicable();
			}
		}
	}

	[ResourceStringData("E05D0A61-16B7-46A3-883E-1304A4DA76EF", Caption = "[21] Code", FullDescription = "[21] Border Transport Code 19 08 061 000", MediumCaption = "[21] Border Transport Code", ShortCaption = "[21] Code")]
	public override ZString ZG_BorderTransportMeans { get => base.ZG_BorderTransportMeans; set => base.ZG_BorderTransportMeans = value; }

	[ReadOnlyMember(nameof(IsImport))]
	[ResourceStringData("D36C000F-F5AD-4212-96F4-A558A8C9F3A8", Caption = "[1c] CT Status")]
	public override ZString ZG_CTStatusID { get => base.ZG_CTStatusID; set => base.ZG_CTStatusID = value; }

	#region Message Version

	[ReadOnlyMember(nameof(MessageVersionReadOnly))]
	[MaxLength(Schema.MessageVersionMaxLength)]
	[ResourceStringData("78E81402-A601-40D3-B1D3-84F8AF86051C", Caption = "Message Version", ShortCaption = "Msg. Version")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageVersionList))]
	public ZString MessageVersion
	{
		get => GenAddOnForMessageVersion?.XA_Data ?? ZString.Empty;
		set
		{
			var oldValue = MessageVersion;
			if (!value.IsEmpty)
			{
				if (GenAddOnForMessageVersion == null)
				{
					GenAddOnHelper.FindOrMakeNewAddOn(GenAddOnColumnConstants.MessageVersionColumnName, this, out genAddOnForMessageVersion);
					genAddOnForMessageVersion.XA_Type = AddOnColumnDataType.Codes.String;
					genAddOnForMessageVersion.XA_DataInfo.ValueChanged -= MessageVersion_XA_DataInfo_ValueChanged;
					genAddOnForMessageVersion.XA_DataInfo.ValueChanged += MessageVersion_XA_DataInfo_ValueChanged;
					RegisterEditableChildObject(genAddOnForMessageVersion);
				}
				CheckMaximumLength(MessageVersionInfo, value);
				GenAddOnForMessageVersion.XA_Data = value;
			}
			else
			{
				if (GenAddOnForMessageVersion != null)
				{
					GenAddOnForMessageVersion.XA_Data = ZString.Empty;
					GenAddOnForMessageVersion.XA_DataInfo.ValueChanged -= MessageVersion_XA_DataInfo_ValueChanged;
					GenAddOnForMessageVersion.Delete();
					genAddOnForMessageVersion = null;
				}
			}
			ValidateMessageVersion();
			MessageVersionInfo.RefreshBinding(oldValue);
			if (!IsCopying && oldValue != MessageVersion)
			{
				JobDeclarationFieldsCleaner.CleanUpMessageDependentFieldsIfNoLongerApplicable();
			}
		}
	}

	GenAddOnColumn GenAddOnForMessageVersion
	{
		get
		{
			if (genAddOnForMessageVersion == null || genAddOnForMessageVersion.IsDeleted)
			{
				GenAddOnHelper.Find(GenAddOnColumnConstants.MessageVersionColumnName, this, out genAddOnForMessageVersion);
				if (genAddOnForMessageVersion != null)
				{
					genAddOnForMessageVersion.XA_DataInfo.ValueChanged -= MessageVersion_XA_DataInfo_ValueChanged;
					genAddOnForMessageVersion.XA_DataInfo.ValueChanged += MessageVersion_XA_DataInfo_ValueChanged;
					RegisterEditableChildObject(genAddOnForMessageVersion);
				}
			}
			return genAddOnForMessageVersion;
		}
	}

	GenAddOnColumn genAddOnForMessageVersion;

	void MessageVersion_XA_DataInfo_ValueChanged(object sender, EventArgs e)
	{
		CustomsOffices.MarkAsNeedingValidation();
		CustomsEntryHeaders.MarkAsNeedingValidation();
		Invoices.MarkAsNeedingValidation();
		InvoiceLines.MarkAsNeedingValidation();
		Packages.MarkAsNeedingValidation();
	}

	ZBool MessageVersionReadOnly => !ITCustomsDataRegistry.Instance.IsExportMessageVersionEnabled;

	public ZPropertyInfo MessageVersionInfo => GetZPropertyInfo(Schema.MessageVersion);

	public ZBool IsMessageVersionApplicable => IsExport && JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin;

	void SetMessageVersionAsApplicable()
	{
		SetMessageVersionDefaultValueIfRequired();
		ClearMessageVersionIfNotVisible();
	}

	void SetMessageVersionDefaultValueIfRequired()
	{
		if (IsMessageVersionApplicable && MessageVersion.IsEmpty)
		{
			var exportMessageVersion = ITCustomsDataRegistry.Instance.ExportMessageVersion.Value;
			if (exportMessageVersion == ExportMessageVersionList.Codes.TXT || exportMessageVersion == ExportMessageVersionList.Codes.BTX)
			{
				MessageVersion = MessageVersionList.Codes.TXT;
			}
			else if (exportMessageVersion == ExportMessageVersionList.Codes.XML || exportMessageVersion == ExportMessageVersionList.Codes.BXM)
			{
				MessageVersion = MessageVersionList.Codes.XML;
			}
		}
	}

	void ClearMessageVersionIfNotVisible()
	{
		if (!IsMessageVersionApplicable && !MessageVersion.IsEmpty)
		{
			MessageVersion = ZString.Empty;
		}
	}

	void ValidateMessageVersion()
	{
		if (!IsValidationSuspended && Validation is CommonJobDeclarationValidation validation)
		{
			validation.ValidateMessageVersion();
		}
	}

	#endregion

	protected override ZBool IsSecurityAllowedCore() => base.IsSecurityAllowedCore() && !IsEntryStyleExportToSpecialTerritory;

	IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
	{
		return new DeclarationValueChangedAnnouncer(this);
	}

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		SetMessageVersionDefaultValueIfRequired();
	}

	void MarkPackagesAsNeedingValidation() => Packages?.ForEach(p => p.MarkAsNeedingValidation());

	#region JobDeclarationFieldsCleaner

	IJobDeclarationFieldsCleaner JobDeclarationFieldsCleaner => jobDeclarationFieldsCleaner ?? (jobDeclarationFieldsCleaner = GetNewJobDeclarationFieldsCleaner());
	IJobDeclarationFieldsCleaner jobDeclarationFieldsCleaner;

	protected virtual IJobDeclarationFieldsCleaner GetNewJobDeclarationFieldsCleaner() => new JobDeclarationFieldsCleaner(this);

	#endregion

	void DefaultEntryInstructions()
	{
		var entryInstructions = CustomsEntryInstructions;
		entryInstructions.ResetOrDefaultParticipantType();
	}

	public override ZString JE_TransportMode
	{
		get => base.JE_TransportMode;
		set
		{
			var oldValue = JE_TransportMode;
			base.JE_TransportMode = value;
			if (!IsCopying && oldValue != JE_TransportMode)
			{
				DefaultPreClearingIfNeeded();
				EmptyInvoiceLinesPortTaxRateIfApplicable();
				RefreshPortTaxRateBinding();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.JobDeclaration|JE_GS_NKCusAgent", Caption = "Subscriber")]
	public override ZString JE_GS_NKCusAgent
	{
		get => base.JE_GS_NKCusAgent;
		set => base.JE_GS_NKCusAgent = value;
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.JobDeclaration|ZG_PreClearing", Caption = "[PRE.1] Pre-clearing")]
	public override ZBool ZG_PreClearing
	{
		get => base.ZG_PreClearing;
		set => base.ZG_PreClearing = value;
	}

	public ZBool IsPreClearingEditable => IsImport && IsSea;

	public override ZString JE_TransportModeInland
	{
		get => base.JE_TransportModeInland;
		set
		{
			var oldValue = JE_TransportModeInland;
			base.JE_TransportModeInland = value;
			if (!IsCopying && oldValue != JE_TransportModeInland)
			{
				SetDefaultTransportMeansIfRequired();
				JobDeclarationFieldsCleaner.CleanUpTransportModeInlandDependentFieldsIfNoLongerApplicable();
			}
		}
	}

	void SetDefaultTransportMeansIfRequired()
	{
		if (IsUCC6)
		{
			JE_TransportMeans = Lookups.TransportMeansList.DefaultCode;
		}
	}

	protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => IsImport ? EUCommonConstants.TransportModeSource.InlandTransportMode : base.TransportMeansDependencyCore;

	public override ZGuid JE_OH_Importer
	{
		get => base.JE_OH_Importer;
		set
		{
			var oldValue = JE_OH_Importer;
			base.JE_OH_Importer = value;
			if (!IsCopying && oldValue != JE_OH_Importer)
			{
				ApplyDeclarationOfIntentDefaulting();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.JobDeclaration|JE_OH_Buyer", Caption = "Buyer", FullDescription = "Buyer's name 3/26")]
	public override ZGuid JE_OH_Buyer { get => base.JE_OH_Buyer; set => base.JE_OH_Buyer = value; }

	public override ZString JE_RL_NKOrigin
	{
		get => base.JE_RL_NKOrigin;
		set
		{
			var oldValue = JE_RL_NKOrigin;
			base.JE_RL_NKOrigin = value;
			if (!IsCopying && oldValue != JE_RL_NKOrigin && Invoices != null)
			{
				Invoices.Cast<BaseJobComInvoiceHeader>().ForEach(x => x.MarkAsNeedingValidation());
			}
		}
	}

	protected override DocumentSupporter CreateNewDocumentSupporter()
	{
		return new JobDeclarationDocumentSupporter(this);
	}

	protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
	{
		return new JobDeclarationSynchroniser(this);
	}

	public new JobDeclarationCustomsOfficeRequirementHelper CustomsOfficeRequirementHelper => (JobDeclarationCustomsOfficeRequirementHelper)base.CustomsOfficeRequirementHelper;

	protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

	protected override IValueSetStrategy GetValueSetStrategy() => new JobDeclarationValueSetStrategy(this);

	protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SetMessageVersionDefaultValueIfRequired();

		ZG_PreClearing = ZBool.False;
	}

	void DefaultPreClearingIfNeeded()
	{
		if (!IsPreClearingEditable)
		{
			ZG_PreClearing = ZBool.False;
		}
	}

	void EmptyInvoiceLinesPortTaxRateIfApplicable()
	{
		if (!NeedsPortTax)
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ZG_PortTaxRate = ZString.Empty);
		}
	}

	public void RefreshPortTaxRateBinding()
	{
		InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ZG_PortTaxRateInfo.RefreshBinding());
	}

	#region JE_DeclarantType Defaulting Disabled

	protected override void SetRepresentationTypeIfMatchingEORICodes()
	{
	}

	protected override string GetDeclarantTypeByDefault() => ZString.Empty;

	#endregion

	public override bool IsDeclarantAddressRequired => IsUCC6AndIsExport || base.IsDeclarantAddressRequired;

	public override ZGuid JE_OA_DeclarantAddress
	{
		get => base.JE_OA_DeclarantAddress;
		set
		{
			var oldValue = JE_OA_DeclarantAddress;
			base.JE_OA_DeclarantAddress = value;
			if (!IsCopying && oldValue != JE_OA_DeclarantAddress)
			{
				DefaultNodeFromDeclarant();
			}
		}
	}

	[ResourceStringData("55499B93-C392-437B-9D0C-A6B7AB7DFDD3", Caption = "Flight", MultipleKey = CaptionKeyAirVoyageFlightNo)]
	[ResourceStringData("A58830C4-81CB-463A-A6A3-254BC2A6415C", Caption = "Voyage", MultipleKey = CaptionKeySeaVoyageFlightNo)]
	public override ZString JE_VoyageFlightNo
	{
		get => base.JE_VoyageFlightNo;
		set
		{
			var oldValue = JE_VoyageFlightNo;
			base.JE_VoyageFlightNo = value;
			if (!IsCopying && oldValue != JE_VoyageFlightNo && IsSea)
			{
				Validation.ValidateJE_VesselName();
			}
		}
	}

	[ResourceStringData("F34EB969-3F3E-4EF5-89F5-2C95C2D18B46", Caption = "[21] Transport ID")]
	[ResourceStringData("DD80F214-B2EB-4761-9B53-60FCF0FD6CEA", Caption = "[21] Vessel", MultipleKey = CaptionKeySeaVesselName)]
	public override ZString JE_VesselName => base.JE_VesselName;

	public override ZString JE_RL_NKPortOfArrival
	{
		get => base.JE_RL_NKPortOfArrival;
		set
		{
			var oldValue = JE_RL_NKPortOfArrival;
			base.JE_RL_NKPortOfArrival = value;
			if (!IsCopying && oldValue != JE_RL_NKPortOfArrival)
			{
				EmptyInvoiceLinesPortTaxRateIfApplicable();
				RefreshPortTaxRateBinding();
				RefreshPortTaxSupportingDocuments();
			}
		}
	}

	public override ZString JE_RL_NKPortOfLoading
	{
		get => base.JE_RL_NKPortOfLoading;
		set
		{
			var oldValue = JE_RL_NKPortOfLoading;
			base.JE_RL_NKPortOfLoading = value;
			if (!IsCopying && oldValue != JE_RL_NKPortOfLoading)
			{
				EmptyInvoiceLinesPortTaxRateIfApplicable();
				RefreshPortTaxRateBinding();
				RefreshPortTaxSupportingDocuments();
			}
		}
	}

	#region MultipleKeysToUseCore

	protected override IReadOnlyList<string> MultipleKeysToUseCore => base.MultipleKeysToUseCore.Concat(GetMultipleKeysToUse()).ToArray();

	internal const string CaptionKeySeaVesselName = "SEA56CAD-4FAB-4058-BB85-1A911AC36E66";
	internal const string CaptionKeyAirVoyageFlightNo = "AIR9D737-FE41-4A4F-965E-3388D6090189";
	internal const string CaptionKeySeaVoyageFlightNo = "SEA3FB6D-C962-4117-B53D-9B0430A41137";

	IEnumerable<string> GetMultipleKeysToUse()
	{
		if (IsAir)
		{
			yield return CaptionKeyAirVoyageFlightNo;
		}
		else if (IsSea)
		{
			yield return CaptionKeySeaVesselName;
			yield return CaptionKeySeaVoyageFlightNo;
		}
	}

	#endregion

	public PreviousDocumentApportionedCollection ApportionedDocumentCollection => apportionedDocumentCollection ?? (apportionedDocumentCollection = PreviousDocumentApportionedCollection.LoadNew(this));
	PreviousDocumentApportionedCollection apportionedDocumentCollection;

	public virtual void ResetApportionedPreviousDocuments()
	{
		apportionedDocumentCollection = null;
		foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
		{
			entryHeader.ResetAllGroupedPreviousDocuments();
		}
	}

	protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
	{
		base.ResetValuesOnTemplateCopyAfterClone(declaration, cloneType);

		foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
		{
			instruction.CEI_DateForDuty = ZDate.Today;
		}
	}

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		return new JobDeclarationPiggyBackedDocAddressValidationFactory(this, x => base.PiggyBackedDocAddressValidation(x))
			.GetNewPiggyBackedDocAddressValidation(addressToValidate);
	}

	public ZString GetApplicationReference()
	{
		return ApplicationReferenceHelper.GetNew(Node, JE_GS_NKCusAgent, JE_CustomsOffice);
	}

	public ZString MeansOfTransportCrossingBorderIdentity
	{
		get
		{
			ZString identity;
			switch (TransportMode)
			{
				case TransportTypeList.Codes.Sea:
					identity = ZString.Format("{0}{1}", JE_VesselName, JE_VoyageFlightNo);
					break;

				case TransportTypeList.Codes.Air:
					identity = JE_VoyageFlightNo;
					break;

				default:
					identity = JE_VesselName;
					break;
			}
			return identity;
		}
	}

	public bool NeedsPortTax => (needsPortTaxCachedProperty ?? (needsPortTaxCachedProperty = new CachedProperty<bool>(Factory, GetNeedsPortTax))).Value;

	CachedProperty<bool> needsPortTaxCachedProperty;

	bool GetNeedsPortTax()
	{
		return ITCustomsDataRegistry.Instance.IsPortTaxesCalculationEnabled && IsSea && BarrierPortUNLOCO != null && !IsBarrierPortTrieste() && (!IsExport || JE_CustomsOffice == OfficeOfExit);

		bool IsBarrierPortTrieste() => BarrierPortUNLOCO.Code == TriestePortCode;
	}

	public RefUNLOCO BarrierPortUNLOCO => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, BarrierPort);

	public ZString CountryOfDestinationCode => JE_GoodsDestination;

	public CusAuthorisationHeader Authorization => AuthorizationProvider.Authorization;

	public CusAuthorisationHeader DeclarationOfIntentAuthorisation
	{
		get
		{
			if (declarationOfIntentAuthorisationCached == null)
			{
				declarationOfIntentAuthorisationCached = new CachedProperty<CusAuthorisationHeader>(Factory, () =>
				{
					CusAuthorisationHeader cusAuthorisation = null;
					var importerPK = JE_OH_Importer;
					if (!importerPK.IsEmpty)
					{
						cusAuthorisation = CusAuthorisationHeader.Loader
							.GetAuthorisations(Factory, Core.Constants.CountryCodes.Italy, new ZString[] { CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent }, ZDate.Today, importerPK)
							.OrderBy(x => x.CPH_StartDate).ThenBy(x => x.CPH_Number)
							.FirstOrDefault();
					}
					return cusAuthorisation;
				});
			}
			return declarationOfIntentAuthorisationCached.Value;
		}
	}
	CachedProperty<CusAuthorisationHeader> declarationOfIntentAuthorisationCached;

	public DeclarationOfIntentRefresher DeclarationOfIntentRefresher => declarationOfIntentRefresher ?? (declarationOfIntentRefresher = new DeclarationOfIntentRefresher(this));
	DeclarationOfIntentRefresher declarationOfIntentRefresher;

	public ZBool ShouldOverrideC100SupportingDocumentRexCode { get; set; }

	public void ApplyEntryLineDefaultLogicForC100SupportingDocuments()
	{
		ActiveEntryHeaders
			.Cast<CusEntryHeader>()
			.SelectMany(x => x.MergedLines)
			.ForEach(x => x.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable());
	}

	IEnumerable<ZString> GetDistinctSupplierCountryCodes()
	{
		List<ZString> supplierCountryCodes = new List<ZString>();
		if (Invoices != null)
		{
			var suppliers = Invoices.Select(x => x.Supplier);

			foreach (OrgHeader supplier in suppliers)
			{
				var countryCode = supplier?.CountryCode ?? ZString.Empty;

				supplierCountryCodes.Add(countryCode);
			}
		}

		return supplierCountryCodes.Distinct().ToList();
	}

	public bool AreDeclarationAndSupplierCountryCodesCompatible()
	{
		bool areDeclarationAndSupplierCountryCodesCompatible = true;
		var countryCodes = GetDistinctSupplierCountryCodes();
		var isDistinctCountryCodeOneAndValid = countryCodes.Count() == 1 && !countryCodes.FirstOrDefault().IsEmpty;

		var originCountryCode = JE_RL_NKOrigin.IsEmpty ? ZString.Empty : JE_RL_NKOrigin.Left(2);
		if (isDistinctCountryCodeOneAndValid && !originCountryCode.IsEmpty && !countryCodes.FirstOrDefault().Equals(originCountryCode))
		{
			areDeclarationAndSupplierCountryCodesCompatible = false;
		}

		return areDeclarationAndSupplierCountryCodesCompatible;
	}

	public ZString SupplierRexCode => supplierRexCode?.Value ?? (supplierRexCode = new CachedProperty<ZString>(Factory, () => Supplier?.GetRexCode() ?? ZString.Empty)).Value;
	CachedProperty<ZString> supplierRexCode;

	protected override MessageChangedStatusDeterminerToDictateWhetherSavingAllowed GetMessageChangedStatusDeterminerForDictatingWhetherSavingAllowed()
	{
		return JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin
			? new ITMessageChangedStatusDeterminerToDictateWhetherSavingAllowed(this)
			: null;
	}

	protected override bool RequiresMessageSaveValidation => CustomsEntryHeaders.HasAnyEntryWhichMessagesCannotBeChanged && CustomsEntryHeaders.Any(x => !x.IsFailedFromTransmission);

	public void AddMissingSupportingDocumentsForThoseInvoiceLinesHaveSameCondition(IEnumerable<MissingSupportingDocument> missingSupportingDocuments, ZString tariffCode, IZZConditionSelectionCriteria conditionSelectionCriteria)
	{
		var invoiceLinesWithSameTariff = InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.JI_Tariff == tariffCode && x.HasSameConditionSelectionCriteria(conditionSelectionCriteria)).ToList();
		invoiceLinesWithSameTariff.ForEach(x => x.AddMissingSupportingDocuments(missingSupportingDocuments));
	}

	protected override ZString SupplierTraderIdCore => Supplier.GetCustomsCodeInfo(SupplierDocumentaryAddress?.Country).FullId;

	protected override ZString ImporterTraderIdCore => Importer.GetCustomsCodeInfo(ImporterDocumentaryAddress?.Country).FullId;

	protected override bool IsOfficeOfExitMeaningfulForDeclaration => IsExport;

	#region VATDeferStrategy

	protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore() => new VATDeferStrategy(this);

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DefermentApprovalNumberList))]
	public override ZString JE_DefermentAccountNumber
	{
		get => base.JE_DefermentAccountNumber;
		set => base.JE_DefermentAccountNumber = value;
	}

	public ZBool IsDefermentAccountNumberEqualToDatCode()
	{
		var impoterDatCode = VATDeferStrategy
			.GetPaymentMethodSourceWrapper()
			?.GetDefermentApprovalNumberForTriesteCode() ?? ZString.Empty;

		return !impoterDatCode.IsEmpty && impoterDatCode == JE_DefermentAccountNumber;
	}

	#endregion

	#region Implementation

	void RefreshPortTaxSupportingDocuments()
	{
		foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>())
		{
			invoiceLine.RefreshPortTaxSupportingDocument();
		}
	}

	void DefaultNodeFromDeclarant()
	{
		JE_CustomsProfile = Lookups.ProfileList.DefaultCode;
	}

	void DefaultSubscriberFromNode()
	{
		if (IsUCC6)
		{
			return;
		}

		var lookupsCusAgents = Lookups.CusAgents;
		if (lookupsCusAgents.Count == 1)
		{
			JE_GS_NKCusAgent = lookupsCusAgents[0].GS_Code;
		}
	}

	void ClearZG_CTStatusIDIfImport()
	{
		if (IsImport)
		{
			ZG_CTStatusID = ZString.Empty;
		}
	}

	void DefaultJE_CustomsOfficeIfNeeded()
	{
		var currentLocationRule = Authorization?.CusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location && x.CPR_ValueFrom == JE_LocationOfGoods);
		var customsOfficeLinkedRule = currentLocationRule?.LinkedCusAuthorisationRules.SingleOrDefault(x => x.CPR_RuleCode == Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice);
		if (customsOfficeLinkedRule != null && JE_CustomsOffice.IsEmpty)
		{
			JE_CustomsOffice = customsOfficeLinkedRule.CPR_ValueFrom.Left(Schema.JE_CustomsOfficeMaxLength);
		}
	}

	void ApplyDeclarationOfIntentDefaulting()
	{
		DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent();
		DeclarationOfIntentRefresher.DefaultSupportingDocument01DI();
	}

	AuthorizationHeaderProvider AuthorizationProvider => authorizationProvider ?? (authorizationProvider = new AuthorizationHeaderProvider(new JobDeclarationAuthorizationDataProvider(this), Factory));
	AuthorizationHeaderProvider authorizationProvider;

	#endregion

	const string TriestePortCode = "ITTRS";

	#region ICustomsProfileDataProvider Members

	ZString ICustomsProfileDataProvider.CustomsProfile => JE_CustomsProfile;

	#endregion

	#region SADH C88

	public event EventHandler<CancelEventArgs> OnGetEntryToPrintSadHC88;

	public CancelEventArgs PerformOnGetEntryToPrintSadHC88()
	{
		var cancelEventArgs = new CancelEventArgs();
		OnGetEntryToPrintSadHC88?.Invoke(this, cancelEventArgs);
		return cancelEventArgs;
	}

	#endregion

	#region IDeclarantProvider Members

	OrgAddress IDeclarantProvider.DeclarantAddress => DeclarantAddress;

	ZString IDeclarantProvider.RepresentativeType => JE_DeclarantType;

	#endregion

	public JobDeclarationAeoCertificateSupporterWrapper AeoCertificateSupporter => aeoCertificateSupporter ?? (aeoCertificateSupporter = new JobDeclarationAeoCertificateSupporterWrapper(this));
	JobDeclarationAeoCertificateSupporterWrapper aeoCertificateSupporter;

	#region IAutHeaderWithCusOfficeProvider members

	CusAuthorisationHeader IAutHeaderWithCusOfficeProvider.Authorization => Authorization;

	ZString IAutHeaderWithCusOfficeProvider.AuthorizationNumber => ZG_AuthorisationNumber;

	ZString IAutHeaderWithCusOfficeProvider.CustomsOffice => JE_CustomsOffice;

	#endregion

	#region LocationQualifier

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationQualifierList))]
	[MaxLength(Schema.JE_LocationQualifierMaxLength)]
	public override ZString JE_LocationQualifier { get => base.JE_LocationQualifier; set => base.JE_LocationQualifier = value; }

	#endregion

	#region CustomsOffices

	[ChildEditable(true)]
	public new OfficeCodeCollection CustomsOffices => (OfficeCodeCollection)base.CustomsOffices;

	protected override EuOfficeCodeCollection GetCustomsOffices() => new OfficeCodeCollection(this);

	#endregion

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = new Dictionary<ZString, Type>();
		result.Add(CusCodeDataTypeList.Codes.OfficeCode, typeof(OfficeCode));
		return result;
	}

	[ReadOnlyMember(nameof(HasAtLeastOneEntryInAmendingStatus))]
	public override ZString JE_EntryStyle
	{
		get => base.JE_EntryStyle;
		set => base.JE_EntryStyle = value;
	}

	public bool HasAtLeastOneEntryInAmendingStatus
	{
		get
		{
			if (hasAtLeastOneEntryInAmendingStatusCachedProperty is null)
			{
				hasAtLeastOneEntryInAmendingStatusCachedProperty = new CachedProperty<bool>(Factory, () => CustomsEntryHeaders.Any(x => x.IsInAmendingStatus));
			}
			return hasAtLeastOneEntryInAmendingStatusCachedProperty.Value;
		}
	}
	CachedProperty<bool> hasAtLeastOneEntryInAmendingStatusCachedProperty;

	[ResourceStringData("03955DDF-4968-49FD-BB19-2211A7E999D7", Caption = "Country of Destination")]
	public override ZString JE_GoodsDestination
	{
		get => base.JE_GoodsDestination;
		set => base.JE_GoodsDestination = value;
	}

	bool IUCC6AndTransitionPeriodProvider.IsImport => base.IsImport;

	[ResourceStringData("85CBDC26-2AF8-492C-BCCB-C393E4B85B5B", Caption = "Delivery Terms")]
	public override ZString ZG_AdditionalDeliveryTerms
	{
		get => base.ZG_AdditionalDeliveryTerms;
		set => base.ZG_AdditionalDeliveryTerms = value;
	}

	public override ZString JE_ShipmentIncoTerm
	{
		get => base.JE_ShipmentIncoTerm;
		set
		{
			var oldValue = JE_ShipmentIncoTerm;
			base.JE_ShipmentIncoTerm = value;
			if (!IsCopying && oldValue != JE_ShipmentIncoTerm)
			{
				JobDeclarationFieldsCleaner.CleanUpShipmentIncoTermFieldsIfNoLongerApplicable();
			}
		}
	}

	public bool IsUcc6ExportAndIsShipmentIncoTermOther => IsUCC6AndIsExport && JE_ShipmentIncoTerm == Core.Constants.IncoTerms.Other;

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

	protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader() => new InventorySelectionHeader(this);

	public override ZGuid JE_OA_Representative
	{
		get => base.JE_OA_Representative;
		set
		{
			var oldValue = JE_OA_Representative;
			base.JE_OA_Representative = value;
			if (!IsCopying && oldValue != JE_OA_Representative)
			{
				DefaultPaymentMethodAndDefermentAccountNumberIfApplicable();
			}
		}
	}

	protected override void SetDefaultValuesForDeclarantAndRepresentativeWithOrgProxy(OrgHeader orgHeaderOfOrgProxy)
	{
		// Intentionally kept blank: Default Declarant from 'Organization Proxy' is opt-out
	}

	protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
	{
		base.ImporterDocumentaryAddressChanged(sender, e);

		if (IsImport && JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin)
		{
			ApplyDeclarantAndRepresentativeDefaulting(ImporterDocumentaryAddress);
		}
	}

	protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
	{
		base.SupplierDocumentaryAddressChanged(sender, e);

		if (IsExport && JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin)
		{
			ApplyDeclarantAndRepresentativeDefaulting(SupplierDocumentaryAddress);
		}
	}

	void DefaultPaymentMethodAndDefermentAccountNumberIfApplicable()
	{
		if (IsImport && JE_OA_Representative.IsValid)
		{
			JE_PaymentMethod = ImportDefermentMethodList.Codes.RepresentativesAccountFromCustomsDecisions;
			JE_DefermentAccountNumber = GetDefaultDefermentAccountNumber();
		}
	}

	ZString GetDefaultDefermentAccountNumber()
	{
		var defermentApprovalNumberList = Lookups.DefermentApprovalNumberList;
		return defermentApprovalNumberList.Count == 1
			? (ZString)defermentApprovalNumberList[0].Code
			: ZString.Empty;
	}

	void ApplyDeclarantAndRepresentativeDefaulting(JobDocAddress supplierOrImporterJobAddress)
	{
		if (supplierOrImporterJobAddress.IsEmpty)
		{
			JE_OA_DeclarantAddress_ZAddress.OrgPK = ZGuid.Empty;
			JE_OA_Representative_ZAddress.OrgPK = ZGuid.Empty;
			return;
		}

		var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
		var orgProxyMainAddressPK = orgProxy.MainAddress.PK;

		if (supplierOrImporterJobAddress.OrganisationPK == orgProxy.PK)
		{
			JE_OA_Representative_ZAddress.OrgPK = ZGuid.Empty;
			JE_OA_DeclarantAddress = supplierOrImporterJobAddress.E2_OA_Address;
			return;
		}

		JE_OA_Representative = orgProxyMainAddressPK;
		JE_OA_DeclarantAddress = supplierOrImporterJobAddress.E2_OA_Address;
	}
}
