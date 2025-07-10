using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.FR;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[UniversalCopyAddInfo(JobDeclarationSchema.Constants.Prefix, EU.Business.AddInfo.Schema.Prefix)]
	public class JobDeclaration : AutoFRJobDeclaration
		, IJobDeclaration, IDeltaSupporter, IHarbourJob, IJobDocAddressOverrideSupporter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string Multiple = "Multiple";

		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			AutoCreateChargesBasedOnIncoTerm = false;
		}

		public new class Schema : AutoFRJobDeclaration.Schema
		{
			public const int ZG_BypassCodeMaxLength = 1;
			public const int ZG_BypassReasonMaxLength = 255;
			public new const int JE_LocationOfGoodsMaxLength = 30;

			public const string JE_DeltaMode = "JE_DeltaMode";
			public const string DeltaG = "DeltaG";

			public const string FallbackEntryNumber = "FallbackEntryNumber";
			public const string FallbackEntryDate = "FallbackEntryDate";
			public const string FallbackEntryStatus = "FallbackEntryStatus";
			public const string IsDeltaDStepOneSentOK = "IsDeltaDStepOneSentOK";
			public const string IsDeltaDStepTwoSentOK = "IsDeltaDStepTwoSentOK";
			public const string IsDeltaDStepTwoSentOKButZeroLiquidation = "IsDeltaDStepTwoSentOKButZeroLiquidation";

			public const string ChargePaymentOrDestinationID = "ChargePaymentOrDestinationID";

			public const string EntryExitedStatus = "EntryExitedStatus";
			public const string AssessmentDate = "AssessmentDate";
			public const string CustomsLastEntryStatusDate = "CustomsLastEntryStatusDate";
			public const string TriggeringPointForValidation = "TriggeringPointForValidation";
			public const string CorrelationID = "CorrelationID";
		}

		protected override ZString EntryStyleForInwardProcessingVATPayment => "FR";

		Type IJobDocAddressOverrideSupporter.ZDocAddressControlType
		{
			get { return ObjectFactory.GetType<IFRDocAddressControl>(); }
		}

		JobDocAddressCollectionForPlugin IJobDocAddressOverrideSupporter.GetJobDocAddressCollectionForPlugin(BusinessObjectFactory factory)
		{
			return null;
		}

		#region Lookups

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.AirRouteTypeList))]
		[MaxLength(JobDeclaration.Schema.JE_AirRouteTypeMaxLength)]
		public override ZString JE_AirRouteType
		{
			get => base.JE_AirRouteType;
			set
			{
				var oldValue = JE_AirRouteType;
				base.JE_AirRouteType = value;
				if (oldValue != JE_AirRouteType)
				{
					JE_AirRouteTypeInfo.RefreshBinding();
				}
			}
		}

		[MaxLength(Schema.JE_LocationOfGoodsMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GoodsLocations))]
		public override ZString JE_LocationOfGoods
		{
			get { return base.JE_LocationOfGoods; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_LocationOfGoods))
				{
					base.JE_LocationOfGoods = value;
				}
			}
		}
		public CusAuthorisationHeader JE_LocationOfGoodsRelatedCusAuthorisation => JE_LocationOfGoods.IsEmpty ? null : Lookups.AuthorizedLocations.FirstOrDefault(x => x.CPH_Number == JE_LocationOfGoods);

		[MaxLength(Schema.JE_SubLocationOfGoodsMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SubGoodsLocations))]
		public override ZString JE_SubLocationOfGoods
		{
			get { return base.JE_SubLocationOfGoods; }
			set
			{
				base.JE_SubLocationOfGoods = value;
			}
		}

		public override ZString JE_CustomsProfile
		{
			get => base.JE_CustomsProfile;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_CustomsProfile))
				{
					base.JE_CustomsProfile = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_OA_DeclarantAddress();
					}
				}
			}
		}

		protected override int JE_OwnerRefMaxLength => 35;

		public OrgCusAccount JE_CustomsProfileRelatedAccount => ApplicationExtender.GetCustomsProfileRelatedAccount(this);

		public IEnumerable<ZString> JE_CustomsProfileAuthorizedLocations => JE_CustomsProfileRelatedAccount?.Header?.GetCusAuthorisationHeadersNumberWithType(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation, CountryCode) ?? Enumerable.Empty<ZString>();

		public ZString CustomsProfileRelatedAccountRepresentativeID => JE_CustomsProfileRelatedAccount?.CZ_RepresentativeID ?? ZString.Empty;

		public bool CustomsProfileAndDeltaModeMatch => (JE_CustomsProfile.IsEmpty && JE_DeltaMode.IsEmpty) || JE_CustomsProfileRelatedAccount != null;

		#endregion

		public ZPropertyInfo ChargePaymentOrDestinationIDInfo => GetZPropertyInfo(Schema.ChargePaymentOrDestinationID);

		[ResourceStringData("FR.JobDeclaration.ChargePaymentOrDestinationID", Caption = "Payment/Destination")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ChargePaymentOrDestinationIDs))]
		[MaxLength(3)]
		public ZString ChargePaymentOrDestinationID
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.ChargePaymentOrDestinationIDType);
			set
			{
				CheckMaximumLength(ChargePaymentOrDestinationIDInfo, value);
				var oldValue = ChargePaymentOrDestinationID;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ChargePaymentOrDestinationIDType, value);

				if (value != oldValue)
				{
					var valueSetStrategy = GetValueSetStrategy();
					if (valueSetStrategy != null)
					{
						valueSetStrategy.ValueSet(ChargePaymentOrDestinationIDInfo, oldValue);
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateChargePaymentOrDestinationID();
				}

				ChargePaymentOrDestinationIDInfo.RefreshBinding(oldValue);
			}
		}

		protected override ZString GetFormattedDUCR(ZString year, ZString eori, ZString reference)
		{
			return IsUCC6 ? (ZString)string.Format("{0}{1}{2}", year, eori, reference) : base.GetFormattedDUCR(year, eori, reference);
		}

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		public ZString GetVatCanaSpecialMention(ZString vATCanaCode) => Factory.GetCachedValue($"VATCanaSpecialMentionCode_{Lookups.DataGroupingForVATCANA}_{vATCanaCode}", delegate
		{
			var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, vATCanaCode, Lookups.DataGroupingForVATCANA, UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, ZDateTime.Today);
			var attribute = cusCodeList?.Attributes?.Cast<ZZRefCusCodeListAttributeCombined>().FirstOrDefault(x => string.Compare(x.ZZE_ZXE_NKName, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.SpecialMention, StringComparison.OrdinalIgnoreCase) == 0);
			return attribute?.ZZE_Value ?? ZString.Empty;
		});

		public ZZRefCusCodeListCombined IATALoadPort => Factory.GetCachedValue("FRIATALoadPort_" + JE_IATALoadPort, delegate
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JE_IATALoadPort, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, ZDateTime.Today, null, new ZString[] { UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentOutEU, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentInEu, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentDomestic });
		});

		public static List<ZString> NonStandardCountryCodes => new List<ZString>
		{
			Core.Constants.NonStandardCountryCodes.Codes.QP,
			Core.Constants.NonStandardCountryCodes.Codes.QR,
			Core.Constants.NonStandardCountryCodes.Codes.QS,
			Core.Constants.NonStandardCountryCodes.Codes.QU,
		};

		public bool HasNonStandardCountryOfDestination => NonStandardCountryCodes.Contains(JE_GoodsDestination);

		public bool HasNonStandardCountryOfOrigin => NonStandardCountryCodes.Contains(JE_GoodsOrigin);

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new FRDeclarationJobDocAddressValidation(addressToValidate, this);

		#region Validation and Lookups

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		protected override Customs.Business.JobDeclarationValidation GetNewValidation() => ApplicationExtender.GetNewJobDeclarationValidation(this);

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		protected override Customs.Business.JobDeclarationLookups GetNewLookups() => ApplicationExtender.GetNewJobDeclarationLookups(this);

		protected override bool IsLookupsCachedInBase => false;

		#endregion

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(this);

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

		[ChildEditable(true)]
		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}

		protected override EU.Business.Declaration.CusEntryHeaderDocumentSupporter GetCusEntryHeaderDocumentSupporterCore(EU.Business.Declaration.CusEntryHeader entryHeader) => new CusEntryHeaderDocumentSupporter((CusEntryHeader)entryHeader);

		public new JobComInvoiceGroupHeader TopGroupInvoice => (JobComInvoiceGroupHeader)base.TopGroupInvoice;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		[ChildEditable(true)]
		public new BaseDeclarationLevelPackageCollection<Package> Packages => (BaseDeclarationLevelPackageCollection<Package>)base.Packages;

		protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

		protected override bool EUD_AgreedPlaceCodeValidationSupportCore => false;

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

		public new CusEntryInstructionCollection CustomsEntryInstructions => (CusEntryInstructionCollection)base.CustomsEntryInstructions;

		[ResourceStringData("FR.JobDeclaration.AssessmentDate", Caption = "Assessment Date")]
		public ZString AssessmentDate
		{
			get
			{
				var listOfDate = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(x => x.EntryInstruction).Where(x => x != null && !x.CEI_DateForDuty.IsEmpty).Select(x => x.CEI_DateForDuty.Date).Distinct().ToList();
				var result = ZString.Empty;

				if (listOfDate.Count == 1)
				{
					result = listOfDate[0].ToString("dd/MM/yyyy");
				}
				else if (listOfDate.Count > 1)
				{
					result = "MLT";
				}

				return result;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JE_TariffType = Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff;
			AddDefaultEntryInstruction();
		}

		void AddDefaultEntryInstruction()
		{
			var entryInstruction = CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = ZString.Empty;
		}

		protected override CusAuthorizationUsageUpdater GetNewCusAuthorisationUsageUpdaterCore() => new FRCusAuthorizationUsageUpdater(this);

		protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
		{
			base.ResetValuesOnTemplateCopyAfterClone(declaration, cloneType);

			foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
			{
				instruction.CEI_DateForDuty = ZDateTime.Today;
			}

			var frDeclaration = (JobDeclaration)declaration;

			if (frDeclaration.Lookups.ApplicationCodeList.GetDescriptionFromCode(frDeclaration.JE_ApplicationCode).IsNullOrEmpty())
			{
				if (frDeclaration.IsDeltaIEEnable)
				{
					frDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				}
				else if (frDeclaration.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interface)
				{
					frDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
				}
				else
				{
					frDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				}
			}

			frDeclaration.JE_TariffType = Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff;
		}

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		public override ZString JE_EntryStatusDescription => GetEntryStatusDescription(base.JE_EntryStatus);

		public ZString GetEntryStatusDescription(ZString entryStatus)
		{
			var entryStatusDescription = ApplicationExtender.GetEntryStatusDescription(this, entryStatus);
			return entryStatusDescription.IsEmpty ? base.JE_EntryStatusDescription : entryStatusDescription;
		}

		#region AdditionalInfos

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		#endregion

		#region SupportingDocuments

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		#endregion

		#region PreviousDocuments

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		#endregion

		#region GenAddOn

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeltaModeList))]
		[MaxLength(20)]
		public ZString JE_DeltaMode
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.JE_DeltaMode);
			}
			set
			{
				var oldValue = JE_DeltaMode;

				CheckMaximumLength(JE_DeltaModeInfo, value);

				GetValueSetStrategy();

				this.SetSystemDefinedValue(Schema.JE_DeltaMode, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_DeltaMode();
					Validation.ValidateJE_OA_DeclarantAddress();
				}

				JE_DeltaModeInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo JE_DeltaModeInfo
		{
			get { return GetZPropertyInfo(Schema.JE_DeltaMode); }
		}

		#endregion

		public override EU.Business.Declaration.EntryCreationStrategy CreateEntryCreationStrategy()
		{
			return new EntryCreationStrategy(this);
		}

		protected override IValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(EU.Business.Declaration.JobComInvoiceLine line) => ApplicationExtender.GetJobComInvoiceLineValueSetStrategy((JobComInvoiceLine)line);

		protected override IValueSetStrategy GetSupportingDocumentValueSetStrategyCore(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument) => new SupportingDocumentValueSetStrategy((SupportingDocument)supportingDocument);

		protected override ZString Box30LocationOfGoodsForDocumentsAndMessagingCore
		{
			get
			{
				ZString locationNumber = ZString.Empty;
				ZString subLocation = ZString.Empty;
				if (!JE_LocationOfGoods.IsEmpty)
				{
					locationNumber = JE_LocationOfGoods;
				}
				if (!JE_SubLocationOfGoods.IsEmpty)
				{
					subLocation = JE_SubLocationOfGoods;
				}
				return subLocation + " : " + locationNumber;
			}
		}

		#region Export Exit Type and Export Exit Type Reason

		[ReadOnlyMember(nameof(IsExportExitTypeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ExportExitTypeList))]
		public override ZString JE_ExportExitType
		{
			get { return base.JE_ExportExitType; }
			set
			{
				var oldValue = JE_ExportExitType;
				base.JE_ExportExitType = value;
				if (oldValue != JE_ExportExitType)
				{
					JE_ExportExitTypeInfo.RefreshBinding();
				}
			}
		}

		ZBool IsExportExitTypeReadOnly => IsOfficeOfLodgementDifferentFromOfficeOfExit;

		[ReadOnlyMember(nameof(JE_ExportExitTypeReasonReadOnly))]
		public override ZString JE_ExportExitTypeReason
		{
			get { return base.JE_ExportExitTypeReason; }
			set
			{
				var oldValue = JE_ExportExitTypeReason;
				base.JE_ExportExitTypeReason = value;
				if (oldValue != JE_ExportExitTypeReason)
				{
					JE_ExportExitTypeReasonInfo.RefreshBinding();
				}
			}
		}

		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set
			{
				var oldValue = JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination && IsImport)
				{
					SetRegionOrTerritoryOfDestinationDefaultValue();
				}
			}
		}

		protected override ZString ConvertTerritory(string country) => CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction.ToList().Contains(country) ? new ZString(country) : base.ConvertTerritory(country);

		public override ZString JE_RL_NKOrigin
		{
			get => base.JE_RL_NKOrigin;
			set
			{
				var oldValue = JE_RL_NKOrigin;
				base.JE_RL_NKOrigin = value;
				if (!IsCopying && oldValue != JE_RL_NKOrigin && IsExport)
				{
					SetRegionOrTerritoryOfDestinationDefaultValue();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.RegionOrTerritoryOfDestinationList))]
		[ResourceStringData("FR.JobDeclaration.ZG_RegionOrTerritoryOfDestination", Caption = "Region or Territory of Destination", MediumCaption = "Destination Region/Territory", ShortCaption = "Region")]
		public override ZString JE_RegionOrTerritoryOfDestination
		{
			get => base.JE_RegionOrTerritoryOfDestination;
			set
			{
				var oldValue = JE_RegionOrTerritoryOfDestination;
				base.JE_RegionOrTerritoryOfDestination = value;
				if (!IsCopying && oldValue != JE_RegionOrTerritoryOfDestination)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public void SetRegionOrTerritoryOfDestinationDefaultValue()
		{
			var result = ZString.Empty;

			var unlocoExtractedCountryCode = IsImport ? JE_RL_NKFinalDestination.Left(2) : JE_RL_NKOrigin.Left(2);

			if (FRDomesticOverseasTerritories.GetTerritoriesFromCountryCode().TryGetValue(unlocoExtractedCountryCode, out var frDOMCodes))
			{
				var territoryFromStateList = FRDomesticOverseasTerritories.GetTerritoryFromState();
				var state = (IsImport ? FinalDestination?.CountryStates?.RW_Code : Origin?.CountryStates?.RW_Code) ?? ZString.Empty;
				if (!state.IsEmpty)
				{
					if (territoryFromStateList.TryGetValue(state, out var frDOMCode))
					{
						if (frDOMCodes.Contains(frDOMCode))
						{
							result = frDOMCode;
						}
					}
				}

				if (result.IsEmpty)
				{
					var codesWithStates = territoryFromStateList.Select(x => x.Value);
					result = frDOMCodes.FirstOrDefault(x => !codesWithStates.Contains(x));
				}
			}

			JE_RegionOrTerritoryOfDestination = result;
		}

		public ZBool HasValidPreviousDocumentForExportExitType
		{
			get
			{
				var allowedPreviousDocs = GetAllowedPreviousDocsForExportExitType().ToList();

				if (allowedPreviousDocs.Count > 0)
				{
					var isRequiredDocOnDeclaration = PreviousDocuments.Cast<PreviousDocument>()?.Any(p => allowedPreviousDocs.Contains(p.CSI_Code) && !p.CSI_ReferenceNumber.IsEmpty) ?? false;
					var isRequiredDocOnInvoice = Invoices.Cast<JobComInvoiceHeader>()?.Any(x => x.PreviousDocuments.Cast<PreviousDocument>().Any(q => allowedPreviousDocs.Contains(q.CSI_Code) && !q.CSI_ReferenceNumber.IsEmpty)
						|| x.InvoiceLines.Cast<JobComInvoiceLine>().Any(y => y.PreviousDocuments.Cast<PreviousDocument>().Any(r => allowedPreviousDocs.Contains(r.CSI_Code) && !r.CSI_ReferenceNumber.IsEmpty))) ?? false;

					return isRequiredDocOnDeclaration || isRequiredDocOnInvoice;
				}
				return true;
			}
		}

		public ZBool HasValidExportExitTypeCTStatusCombination
		{
			get
			{
				return (JE_ExportExitType != ExportExitTypeList.Codes.TRA)
					|| (JE_ExportExitType == ExportExitTypeList.Codes.TRA
						&& (ZG_CTStatusID == ExportCommunityTransitStatusList.Codes.T1 || ZG_CTStatusID == ExportCommunityTransitStatusList.Codes.T2 || ZG_CTStatusID == "T-"));
			}
		}

		public ImmutableArray<string> GetAllowedPreviousDocsForExportExitType()
		{
			var allowedPreviousDocs = ImmutableArray.Create<string>();

			if (JE_ExportExitType == ExportExitTypeList.Codes.TRA)
			{
				allowedPreviousDocs = AllowedPreviousDocsForTRAExportExitType;
			}
			else if (JE_ExportExitType == ExportExitTypeList.Codes.EMC)
			{
				allowedPreviousDocs = AllowedPreviousDocsForEMCExportExitType;
			}

			return allowedPreviousDocs;
		}

		public ImmutableArray<string> AllowedPreviousDocsForEMCExportExitType => (allowedPreviousDocsForEMCExportExitType ?? (allowedPreviousDocsForEMCExportExitType = new CachedValue<ImmutableArray<string>>(() => ImmutableArray.Create(
				FRConstants.ExportPreviousDocuments.AAD
			)))).Value;
		CachedValue<ImmutableArray<string>> allowedPreviousDocsForEMCExportExitType;

		public ImmutableArray<string> AllowedPreviousDocsForTRAExportExitType => (allowedPreviousDocsForTRAExportExitType ?? (allowedPreviousDocsForTRAExportExitType = new CachedValue<ImmutableArray<string>>(() => ImmutableArray.Create(
				new PreviousDocumentCodeList().GetAllCodes()
			)))).Value;
		CachedValue<ImmutableArray<string>> allowedPreviousDocsForTRAExportExitType;

		public ZBool JE_ExportExitTypeReasonReadOnly => JE_ExportExitType != ExportExitTypeList.Codes.OTH;

		#endregion

		#region Implementation

		#region protected override

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;
		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.France;

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("FR"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		#endregion

		public override ZBool AreMultipleEntryInstructionsAllowed => true;

		#endregion

		protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

		public JobDeclarationValueSetStrategy ValueSetStrategy => (JobDeclarationValueSetStrategy)GetValueSetStrategy();

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = ApplicationExtender.GetJobDeclarationValueSetStrategy(this));

		IValueSetStrategy valueSetStrategy;

		#region VATDeferStrategy

		public new VATDeferStrategy VATDeferStrategy => (VATDeferStrategy)base.VATDeferStrategy;

		protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore() => ApplicationExtender.GetVATDeferStrategy(this);

		[ResourceStringData("FR.JobDeclaration.JE_PaymentMethod", Caption = "Method of Payment")]
		public override ZString JE_PaymentMethod
		{
			get => base.JE_PaymentMethod;
			set => base.JE_PaymentMethod = value;
		}

		[ResourceStringData("FR.JobDeclaration.JE_DefermentAccountNumber", Caption = "Approval Defer No.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DefermentAccountNumberList))]
		public override ZString JE_DefermentAccountNumber
		{
			get => base.JE_DefermentAccountNumber;
			set => base.JE_DefermentAccountNumber = value;
		}

		[ResourceStringData("FR.JobDeclaration.ZG_VATDeferType", Caption = "VAT Procedure")]
		public override ZString ZG_VATDeferType
		{
			get => base.ZG_VATDeferType;
			set
			{
				var oldValue = ZG_VATDeferType;
				if (oldValue != value)
				{
					base.ZG_VATDeferType = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateZG_VATDeferNumber();
					}
				}
			}
		}

		[MaxLength(AutoCusPermitHeader.Schema.CPH_NumberMaxLength)]
		public override ZString JE_VATDeferNumber
		{
			get => base.JE_VATDeferNumber;
			set => base.JE_VATDeferNumber = value;
		}

		[ResourceStringData("FR.JobDeclaration.ZG_VATDeferNumber", Caption = "VAT Authorization")]
		public override ZString ZG_VATDeferNumber
		{
			get => base.ZG_VATDeferNumber;
			set => base.ZG_VATDeferNumber = value;
		}

		public override ZString ZG_CTStatusID
		{
			get => base.ZG_CTStatusID;
			set
			{
				var oldValue = ZG_CTStatusID;
				base.ZG_CTStatusID = value;
				if (!IsCopying && oldValue != ZG_CTStatusID)
				{
					if (ZG_CTStatusID == ExportCommunityTransitStatusList.Codes.T2LF)
					{
						if (!Invoices.Any())
						{
							var invoice = Invoices.AddNew();
							invoice.JZ_InvoiceNumber = (NoResString)"Facture Default";
						}

						Invoices.Cast<JobComInvoiceHeader>().ForEach(x =>
						{
							if (!x.SupportingDocuments.Cast<SupportingDocument>().Any(y => y.CSI_Code == "C620" && y.CSI_ReferenceNumber == ExportCommunityTransitStatusList.Codes.T2LF))
							{
								var supportingDocument = x.SupportingDocuments.AddNew();
								supportingDocument.CSI_Code = "C620";
								supportingDocument.CSI_ReferenceNumber = ExportCommunityTransitStatusList.Codes.T2LF;
								supportingDocument.CSI_DateOfIssue = JE_SystemCreateTimeUtc;
							}
						});
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VatCanaList))]
		public override ZString JE_VATCANACode { get => base.JE_VATCANACode; set => base.JE_VATCANACode = value; }

		[ResourceStringData("FR.JobDeclaration.ZG_VATCANACode", Caption = "VAT CANA Code")]
		public override ZString ZG_VATCANACode { get => base.ZG_VATCANACode; set => base.ZG_VATCANACode = value; }

		#endregion

		public ZBool IsOfficeOfExitCodeTheSameAsOfficeOfDeclaration => IsExport && !OfficeOfExitCode.IsEmpty && OfficeOfExitCode == OfficeOfDeclaration;

		public new FROfficeCodeCollection CustomsOffices => (FROfficeCodeCollection)base.CustomsOffices;

		protected override EuOfficeCodeCollection GetCustomsOffices() => new FROfficeCodeCollection(this);

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(OfficeCode);
			return result;
		}

		public ZString OfficeOfExitCode => this.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit)?.CY_Data ?? ZString.Empty;

		public ZString SiretCode => DeltaAccountOrgHeader?.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.Siret, Core.Constants.CountryCodes.France) ?? ZString.Empty;
		public ZString EoriCode => DeltaAccountOrgHeader?.GetUnprefixedEORI() ?? ZString.Empty;

		public ZString OfficeOfDeclaration
		{
			get => CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep)?.CY_Data ?? ZString.Empty;
			set => (CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep) ?? CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep)).CY_Data = value;
		}

		public ZBool IsDCN => JE_CustomsOffice == OfficeOfDeclaration;

		#region Delta

		public ZBool IsDeltaC => JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G1;

		public ZBool IsDeltaD => JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G2;

		public ZBool IsDeltaDStepOneSentOK => ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.IsDeltaDStepOneSentOK);

		public ZBool IsDeltaDStepTwoSentOK => ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.IsDeltaDStepTwoSentOK);

		public ZBool IsDeltaDStepTwoSentOKButZeroLiquidation => ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.IsDeltaDStepTwoSentOKButZeroLiquidation);

		public ZBool IsDeltaIE => JE_ApplicationCode == DeclarationApplicationCodeList.Codes.DeltaIE;

		public ZBool IsDeltaG => JE_ApplicationCode == DeclarationApplicationCodeList.Codes.DeltaG;

		public IEnumerable<OrgCusAccount> DeltaAccounts => ApplicationExtender.GetDeltaAccounts(this);

		#region Delta G Account

		public ZString DeltaGAccountCode => IsImport ? OrgCusAccountCodeList.Codes.DGI : OrgCusAccountCodeList.Codes.DGE;

		public OrgHeader ActualClient => IsImport ? Importer : Supplier;

		public IEnumerable<OrgHeader> ActualClientAndDeclarant
		{
			get
			{
				var actualClient = ActualClient;
				if (actualClient != null)
				{
					yield return actualClient;
				}

				var declarant = Declarant?.Header;
				if (declarant != null)
				{
					yield return declarant;
				}
			}
		}

		public OrgHeader DefaultDeltaAccountOrgHeader => ActualClient ?? Declarant?.Header;

		public OrgHeader DeltaAccountOrgHeader => JE_CustomsProfileRelatedAccount?.Header ?? DefaultDeltaAccountOrgHeader;

		#endregion

		public override ZString JE_CustomsOffice
		{
			get => base.JE_CustomsOffice;
			set
			{
				base.JE_CustomsOffice = value;
				ActiveEntryHeaders.Cast<CusEntryHeader>().ToList().ForEach(x => x.Charges.MarkAsNeedingValidation());
			}
		}

		public override ZGuid JE_GB
		{
			get => base.JE_GB;
			set
			{
				base.JE_GB = value;
				ActiveEntryHeaders.Cast<CusEntryHeader>().ToList().ForEach(x => x.Charges.MarkAsNeedingValidation());
			}
		}

		public override ZGuid JE_GC
		{
			get => base.JE_GC;
			set
			{
				var oldValue = JE_GC;
				base.JE_GC = value;
				if (!IsCopying && oldValue != JE_GC)
				{
					ActiveEntryHeaders.Cast<CusEntryHeader>().ToList().ForEach(x => x.Charges.MarkAsNeedingValidation());
				}
			}
		}

		public override ZString JE_RL_NKPortOfLoading
		{
			get => base.JE_RL_NKPortOfLoading;
			set
			{
				base.JE_RL_NKPortOfLoading = value;
				ActiveEntryHeaders.Cast<CusEntryHeader>().ToList().ForEach(x => x.Charges.MarkAsNeedingValidation());
			}
		}

		#endregion

		#region JE_LandedPieces

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration|JE_LandedPieces", Caption = "Pieces Landed")]
		public override ZInt JE_LandedPieces { get => base.JE_LandedPieces; set => base.JE_LandedPieces = value; }

		#endregion

		#region JE_TransportMode

		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				var oldValue = JE_TransportMode;
				base.JE_TransportMode = value;
				if (!IsCopying && oldValue != JE_TransportMode && !IsValidationSuspended)
				{
					Validation.ValidateJE_LandedPieces();
				}
			}
		}

		#endregion

		#region JE_MessageType

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);
			RefreshIncotermAndChargeFactory();

			Invoices.MarkAsNeedingValidation();

			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_LandedPieces();
			}
			if (!IsDeltaIEEnable)
			{
				DefaultJE_ApplicationCode();
			}
		}

		#endregion

		#region JE_TotalNoOfPacks

		public override ZInt JE_TotalNoOfPacks
		{
			get => base.JE_TotalNoOfPacks;
			set
			{
				var oldValue = JE_TotalNoOfPacks;
				base.JE_TotalNoOfPacks = value;
				if (!IsCopying && oldValue != JE_TotalNoOfPacks && !IsValidationSuspended)
				{
					Validation.ValidateJE_LandedPieces();
				}
			}
		}

		#endregion

		#region JE_ShipmentIncoTerm

		public override ZString JE_ShipmentIncoTerm
		{
			get => base.JE_ShipmentIncoTerm;
			set
			{
				var oldvalue = JE_ShipmentIncoTerm;
				if (!IsCopying && oldvalue != value)
				{
					base.JE_ShipmentIncoTerm = value;
					Invoices.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		public bool IsDeltaIEEnable => (IsImport && FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.Value)
			|| (IsExport && FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.Value);

		#region JE_ApplicationCode

		protected override bool SupportMultipleBuiltInTypes => IsDeltaIEEnable;
		protected override string SubmissionTypeBuiltinCode => DeclarationApplicationCodeList.Codes.DeltaG;
		protected override bool IsBuiltinSubmissionType(string type)
		{
			return IsDeltaIEEnable ? type.ToUpperInvariant().In(DeclarationApplicationCodeList.Codes.DeltaG, DeclarationApplicationCodeList.Codes.DeltaIE) : type.Equals(DeclarationApplicationCodeList.Codes.DeltaG, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		[ResourceStringData("FR.JobDeclaration.JE_DeclarationLanguage", Caption = "Language", ShortCaption = "Lang.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsLanguageList))]
		[ReadOnly(true)]
		public override ZString JE_DeclarationLanguage { get => base.JE_DeclarationLanguage; set => base.JE_DeclarationLanguage = value; }

		internal void PopulateInvoiceCharges() => Invoices.Cast<JobComInvoiceHeader>().ForEach(inv => inv.PopulateCharges());

		protected override ZBool SupportValidateCustomsMessagingCore => true;

		public ZString CorrelationID
		{
			get
			{
				var distinctCorrelationIDs = ActiveEntryHeaders.Cast<CusEntryHeader>().Where(c => !c.CorrelationID.IsEmpty).Select(c => c.CorrelationID).Distinct();
				return distinctCorrelationIDs.Count() <= 1 ? distinctCorrelationIDs.FirstOrDefault().ToString() : Multiple;
			}
		}

		protected override ZQuery GetValidCusEntryNumFilter()
		{
			var result = base.GetValidCusEntryNumFilter();
			result.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, CusEntryHeader.Schema.FallbackEntryType);
			return result;
		}

		#region Fallback

		public ZString FallbackEntryNumber
		{
			get
			{
				var distinctNumbers = ActiveEntryHeaders.Cast<CusEntryHeader>().Where(c => !c.FRCustomsFallbackNumber.IsEmpty).Select(c => c.FRCustomsFallbackNumber).Distinct();
				return distinctNumbers.Count() <= 1 ? distinctNumbers.FirstOrDefault().ToString() : Multiple;
			}
		}

		public ZString FallbackEntryDate
		{
			get
			{
				var distinctDates = ActiveEntryHeaders.Cast<CusEntryHeader>().Where(c => !c.DeltaGFallbackIssueDate.IsEmpty).Select(c => c.DeltaGFallbackIssueDate.ToString(DateTimeFormatStrings.ShortDateFormat, CultureInfo.CurrentCulture)).Distinct();
				return distinctDates.Count() <= 1 ? distinctDates.FirstOrDefault() : Multiple;
			}
		}

		public ZString FallbackEntryStatus
		{
			get
			{
				var distinctStatus = ActiveEntryHeaders.Cast<CusEntryHeader>().Where(c => !c.DeltaGFallbackStatus.IsEmpty).Select(c => c.DeltaGFallbackStatus).Distinct();
				return distinctStatus.Count() <= 1 ? distinctStatus.FirstOrDefault().ToString() : Multiple;
			}
		}

		public ZBool DeltaGFallbackAnnounced => IsRefCusCodeListCombinedExistInDataBase(Schema.DeltaG, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, Core.Constants.CountryCodes.France, ZDateTime.Today, ZDateTime.Today);

		ZBool IsRefCusCodeListCombinedExistInDataBase(ZString deltaCode, ZString code, ZString countryCode, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, countryCode);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, code);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, deltaCode);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, startDate);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, endDate);
			return Factory.Exists(typeof(ZZRefCusCodeListCombined), query);
		}

		public ZBool DeltaGFallbackAnnouncedButNotActive => DeltaGFallbackAnnounced && !FRCustomsDataRegistry.DeltaGFallbackIsActive;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDeclarationFetchStrategy(this);
		}

		public CusGuaranteeHeader Ai2Permit
		{
			get
			{
				CusGuaranteeHeader cusGuaranteeHeader = null;
				if (ZG_VATDeferType == VATProcedureList.Codes._2)
				{
					var query = GetAi2PermitQueryWithNumber(ZG_VATDeferNumber, JE_OH_Importer, Declarant?.OA_OH ?? ZGuid.Empty);
					cusGuaranteeHeader = Factory.LoadTop1<CusGuaranteeHeader>(query);
				}
				return cusGuaranteeHeader;
			}
		}

		public ZQuery GetAi2PermitQueryWithNumber(ZString permitNumber, params ZGuid[] holders)
		{
			var query = GetAi2PermitQuery(holders);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Number, ZG_VATDeferNumber);
			return query;
		}

		public ZQuery GetAi2PermitQuery(params ZGuid[] holders)
		{
			var filteredHolders = holders.Where(x => !x.IsEmpty).ToArray();

			if (filteredHolders.Any())
			{
				var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
				var ohQuery = new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, holders);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, GuaranteeTypeList.Codes.AI2);
				query.AddToFilter(ohQuery);
				query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);
				var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
				endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, ZDate.Empty);
				query.AddToFilter(endDateQuery);
				query.OrderBy = CusPermitHeaderSchema.Constants.CPH_StartDate + OrderByClause.Descending + "," + CusPermitHeaderSchema.Constants.CPH_SystemCreateTimeUtc + OrderByClause.Descending;
				return query;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		public Money RemainingAi2PermitBalance => new Money(Ai2Permit?.CPH_Calc_TotalBalanceIncludingPending.Amount ?? ZDecimal.Zero, RefCurrency.LoadFromCurrencyCode(Factory, Ai2Permit?.CPH_UnitOfMeasure ?? ZString.Empty) ?? LocalCurrency);

		public ZDecimal RemainingAi2PermitBalanceAmountInLocalCurrency => ((ICurrencyConverterProvider)this).CurrencyConverter.ConvertExact(RemainingAi2PermitBalance, LocalCurrency).Amount;

		#region CustomsGuarantee

		protected override EU.Business.CusGuaranteeHeader GetCustomsGuaranteeCore => Lookups.CODCustomsGuarantees.Cast<EU.Business.CusGuaranteeHeader>().FirstOrDefault(x => x.CPH_Number == JE_CustomsGuaranteeNumber);

		public new CusGuaranteeHeader CustomsGuarantee => (CusGuaranteeHeader)base.CustomsGuarantee;

		public ZString GetVariousOperationCreditNumber()
		{
			var guaranteeNumber = CustomsGuarantee?.GetApplicationSpecificReference(JE_DeltaMode) ?? ZString.Empty;
			return guaranteeNumber;
		}

		[ResourceStringData("FR.JobDeclaration.ZG_CustomsGuaranteeNumber", Caption = "COD No.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsGuaranteeNumberList))]
		[MaxLength(JobDeclaration.Schema.JE_CustomsGuaranteeNumberMaxLength)]
		public override ZString JE_CustomsGuaranteeNumber
		{
			get => base.JE_CustomsGuaranteeNumber;
			set
			{
				base.JE_CustomsGuaranteeNumber = value;
			}
		}

		public CusGuaranteeHeader CustomsGuaranteeIncludingExpired => (CusGuaranteeHeader)Lookups.CODCustomsGuaranteesIncludingExpired.Cast<EU.Business.CusGuaranteeHeader>().FirstOrDefault(x => x.CPH_Number == JE_CustomsGuaranteeNumber);

		public ZString GetVariousOperationCreditNumberIncludingExpired()
		{
			var guaranteeNumber = CustomsGuaranteeIncludingExpired?.GetApplicationSpecificReference(JE_DeltaMode) ?? ZString.Empty;
			return guaranteeNumber;
		}

		#endregion

		#region Override IJobDeclarationMessageSupporter Members

		protected override ZBool SupportEntryDeclarationMessageCore => true;

		protected override IProcessor GetEntryDeclarationMessageProcessorCore() => IsUCC5 ? new DeltaGAutoSendCustomsMessageProcessor(this) : new DeltaIEAutoSendCustomsMessageProcessor(this);

		#endregion

		protected override EU.Business.Declaration.EntryFeePaymentPartyUnderstander GetEntryFeePaymentPartyUnderstanderCore(EU.Business.Declaration.CusEntryHeader header) => new EntryFeePaymentPartyUnderstander(this);

		protected override bool IsInventorySelectionEnabledCore => CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.WarehouseIsInventoryManagementOn);

		protected override bool SupportInwardProcessingCore => true;

		protected override bool IsBondedWhsQuantityRequiredForBondedWarehouse => !IsAllocatedQuantityRequiredForBondedWarehouse;

		protected override bool IsAllocatedQuantityRequiredForBondedWarehouse => SupportInwardProcessing && CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.IsEntryStyleOutOfInward);

		public ZBool IsOfficeOfLodgementDifferentFromOfficeOfExit => !OfficeOfExitCode.IsEmpty && JE_CustomsOffice != OfficeOfExit;

		protected override NotificationTypes ErrorTypeForCWPackQtyExceededErrorCore => NotificationTypes.Warning;

		protected override string GetIApportionInvoiceHolderCountryContextCore()
		{
			return CountryCode + this.GetIncoTermChargeFactoryCacheKey();
		}

		protected override bool IsSupplementaryMenuVisibleCore => IsUCC6;

		public bool IsG2WithMixedStatusEntries =>
											IsDeltaD &&
											ActiveEntryHeaders.Take(2).Count() > 1 &&
											ActiveEntryHeaders.Cast<CusEntryHeader>().Any(e => e.IsBAE) &&
											ActiveEntryHeaders.Cast<CusEntryHeader>().Any(e => !e.IsBAE);

		public ZString DeltaMode => JE_DeltaMode;

		public ApplicationExtender ApplicationExtender => applicationExtender ?? (applicationExtender = ApplicationExtender.New(JE_ApplicationCode));
		ApplicationExtender applicationExtender;

		public override ZString JE_ApplicationCode
		{
			get => base.JE_ApplicationCode;
			set
			{
				var hasChanged = JE_ApplicationCode != value;
				if (hasChanged)
				{
					applicationExtender = null;
					valueSetStrategy = null;
					base.JE_ApplicationCode = value;
					JE_DeltaModeInfo.RefreshBinding();
				}
			}
		}

		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var oldValue = JE_MessageType;
				base.JE_MessageType = value;
				if (oldValue != value && !IsCopying)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		protected override ZString PackageMarksAndNumbersAlwaysRequiredValidationMessageCore => !IsImport ? base.PackageMarksAndNumbersAlwaysRequiredValidationMessageCore : ZString.Empty;

		protected override bool SupportEntrySnpashotsCore => true;

		#region JobDocAddresses

		public override JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new FRJobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		FRJobDocAddressDependentCollection fDocAddresses;

		#region ImporterDocumentaryAddress

		public new FRJobDocAddress ImporterDocumentaryAddress => base.ImporterDocumentaryAddress as FRJobDocAddress;
		public ZString ImporterDocumentaryAddressCode => ImporterDocumentaryAddress.Address?.AddressCode ?? ZString.Empty;

		protected override JobDocAddressRequirement AddImporterDocAddressRequirement()
		{
			var result = base.AddImporterDocAddressRequirement();
			var importer = Importer;
			if (importer != null)
			{
				result.DefaultAddressType = GetOrgHeaderDefaultAddressType(importer);
			}

			return result;
		}

		protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_ImporterChanged(oldValue, newValue);

			var importer = Importer;
			if (importer != null)
			{
				ImporterDocumentaryAddress.DefaultAddressType = GetOrgHeaderDefaultAddressType(importer);
				ImporterDocumentaryAddress.SetDefaultAddressFromOrg();
			}
		}

		#endregion

		#region SupplierDocumentaryAddress

		public new FRJobDocAddress SupplierDocumentaryAddress => base.SupplierDocumentaryAddress as FRJobDocAddress;
		public ZString SupplierDocumentaryAddressCode => SupplierDocumentaryAddress.Address?.AddressCode ?? ZString.Empty;

		protected override JobDocAddressRequirement AddSupplierDocAddressRequirement()
		{
			var result = base.AddSupplierDocAddressRequirement();
			var supplier = Supplier;
			if (supplier != null)
			{
				result.DefaultAddressType = GetOrgHeaderDefaultAddressType(supplier);
			}

			return result;
		}

		protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_SupplierChanged(oldValue, newValue);

			var supplier = Supplier;
			if (supplier != null)
			{
				SupplierDocumentaryAddress.DefaultAddressType = GetOrgHeaderDefaultAddressType(supplier);
				SupplierDocumentaryAddress.SetDefaultAddressFromOrg();
			}
		}

		#endregion

		AddressType GetOrgHeaderDefaultAddressType(OrgHeader orgHeader)
		{
			var fallBackAddress = orgHeader?.GetAddressWithFallback(AddressType.ECA);
			var isECAEnabled = fallBackAddress?.AddressCapability?.GetCapabilityEnabled(OrgConstants.AddressType.EUCustomsAddress) ?? false;

			return isECAEnabled ? AddressType.ECA : AddressType.OFC;
		}

		#endregion

		#region IHarbourJob Implementation

		ZDateTime IHarbourJob.ValuationDate => FirstActiveEntryHeaderWithEntryNum?.EffectiveValuationDate ?? ZDateTime.Today;

		ZString IHarbourJob.HarbourType => JE_MessageType;

		ZString IHarbourJob.DataGrouping => GetDefaultDataGroupingCode();

		ZString IHarbourJob.CustomsOffice => JE_CustomsOffice;

		ZString IHarbourJob.ContainerMode => ContainerMode;
		ZBool IHarbourJob.IsDCN => IsDCN;

		#endregion

		protected override ZBool ShouldDefaultContainerModeAndIsContainerisedCore(ZString containerMode) => Core.Constants.ContainerModes.IsContainerised(containerMode);

		[ResourceStringData("FR.JobDeclaration.EntryExitedStatus", Caption = "Export Control Status", ShortCaption = "ECS Status")]
		public ZString EntryExitedStatus
		{
			get
			{
				var result = ZString.Empty;
				var entryHeaders = CustomsEntryHeaders;
				var defaultEntry = entryHeaders.FirstOrDefault();
				if (defaultEntry != null)
				{
					result = defaultEntry.CH_ExitedStatus;
					if (entryHeaders.Any(entry => entry.CH_ExitedStatus != result))
					{
						result = Multiple;
					}
				}

				return result;
			}
		}
		public ZPropertyInfo EntryExitedStatusInfo => GetZPropertyInfo(Schema.EntryExitedStatus);

		#region ManageVatSupportingDocuments

		public VATNumberSupporter VATNumberSupporter => ApplicationExtender.GetVATNumberSupporter(this);

		internal void ManageVATSupportingDocuments()
		{
			var vatDeferNumber = Importer != null ? VATNumberSupporter.GetVATDeferNumberForAutoliquidation(Importer) : ZString.Empty;
			switch (ZG_VATDeferType)
			{
				case VATProcedureList.Codes.L:
					if (vatDeferNumber.IsEmpty)
					{
						VATNumberSupporter.SetUnidentifiedVATNumber();
					}
					else
					{
						VATNumberSupporter.SetIdentifiedVATNumber();
					}
					RemoveFromCusSupportingCollection(VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode);
					break;
				case VATProcedureList.Codes._2:
					if (!vatDeferNumber.IsEmpty)
					{
						VATNumberSupporter.SetIdentifiedVATNumber();
						AddNewToCusSupportingCollection(VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode);
					}
					break;
			}
		}

		void AddNewToCusSupportingCollection(ZString codeToAdd)
		{
			var cusSupportingCollection = SupportingDocuments;
			var cusSupportingInfo = cusSupportingCollection.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == codeToAdd);
			if (cusSupportingInfo == null)
			{
				cusSupportingInfo = cusSupportingCollection.AddNew();
				cusSupportingInfo.CSI_Code = codeToAdd;
			}
		}

		void RemoveFromCusSupportingCollection(ZString codeToRemove)
		{
			var cusSupportingCollection = SupportingDocuments;
			var cusSupportingInfo = cusSupportingCollection.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == codeToRemove);
			if (cusSupportingInfo != null)
			{
				SupportingDocuments.RemoveAndDelete(cusSupportingInfo);
			}
		}

		#endregion

		protected override void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
			base.DecorateDocAddressRequirement(requirement, addressType);

			switch (addressType)
			{
				case DocAddressType.SupplierDocumentaryAddress:
					requirement.ValidateOrganisationPK += Validation.ValidateSupplierDocumentaryAddress;
					break;

				case DocAddressType.ImporterDocumentaryAddress:
					requirement.ValidateOrganisationPK += Validation.ValidateImporterDocumentaryAddress;
					break;
			}
		}

		protected override bool IsDeclarationIntegratedCore() => IsInterface;

		protected override bool ShowSubmitMenuItemCore() => IsInterface;

		public ZDateTime CustomsLastEntryStatusDate
		{
			get
			{
				if (customsLastEntryStatusDateCache == null)
				{
					customsLastEntryStatusDateCache = new CachedProperty<ZDateTime>(Factory, () =>
					{
						ZDateTime result = ZDateTime.Empty;

						ZString[] array = (from CusEntryHeader entry in ActiveEntryHeaders
										   select entry.CH_EntryStatus
							into x
										   where !x.IsEmpty
										   select x).Distinct().ToArray();

						if (array.Length == 1)
						{
							foreach (CusEntryHeader entry in ActiveEntryHeaders)
							{
								var lastEntryStatusDate = entry.CustomsLastEntryStatusDate;
								if (result.IsEmpty || lastEntryStatusDate > result)
								{
									result = lastEntryStatusDate;
								}
							}
						}

						return result;
					});
				}
				return customsLastEntryStatusDateCache.Value;
			}
		}

		CachedProperty<ZDateTime> customsLastEntryStatusDateCache;

		[ResourceStringData("FRJobDeclaration|ZG_AgreedPlaceCode", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code")]
		public override ZString ZG_AgreedPlaceCode { get => base.ZG_AgreedPlaceCode; set => base.ZG_AgreedPlaceCode = value; }

		protected override ZString DefaultDataGroupingCore => Core.Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(CountryCode) ? Core.Constants.CountryCodes.France : CountryCode.ToString();

		protected override ZString DefaultDataGroupingForCusProcedureCore
		{
			get
			{
				var procedureDataGrouping = ApplicationExtender.GetDataGroupingForCusProcedure(this);
				return procedureDataGrouping.IsEmpty ? base.DefaultDataGroupingForCusProcedureCore : procedureDataGrouping;
			}
		}

		protected override ZString DefaultDataGroupingForAdditionalDocumentCodesCore
		{
			get
			{
				var additionalDocumentCodesDataGrouping = ApplicationExtender.GetDataGroupingForAdditionalDocumentCodes(this);
				return additionalDocumentCodesDataGrouping.IsEmpty ? base.DefaultDataGroupingForAdditionalDocumentCodesCore : additionalDocumentCodesDataGrouping;
			}
		}

		public override bool IsCreditCheckEnabledForValidateCustomsMessaging
		{
			get
			{
				var creditCheckEnabled = false;
				if (ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(this, x, ZString.Empty)))
				{
					creditCheckEnabled = base.IsCreditCheckEnabledForValidateCustomsMessaging;
				}
				return creditCheckEnabled;
			}
		}

		public bool IsContainerizedAndHasContainer
		{
			get
			{
				var result = false;

				if (CusContainers.Cast<CusContainer>().Any(x => !x.CO_ContainerNumber.IsEmpty))
				{
					switch (JE_ContainerMode)
					{
						case Core.Constants.ContainerModes.FCL:
						case Core.Constants.ContainerModes.Containerised:
						case Core.Constants.ContainerModes.ULD:
						case Core.Constants.ContainerModes.Liquid:
							result = true;
							break;
					}
				}
				return result;
			}
		}

		public bool HasSimplifiedEntry => CustomsEntryInstructions?.Cast<CusEntryInstruction>().Any(entryInstruction => entryInstruction.IsSimplified) ?? false;

		public ZString TriggeringPointForValidation
		{
			get
			{
				var distinctEntryTriggerPointForValidation = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(c => c.CH_TriggeringPointForValidation).Distinct();
				return distinctEntryTriggerPointForValidation.Count() <= 1 ? distinctEntryTriggerPointForValidation.FirstOrDefault().ToString() : Multiple;
			}
		}

		protected override bool IsIntegrationWithAccountingSupported => true;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					result.AddRange(entryHeader.BusinessObjectsWithRelatedEvents);
				}
				return result.ToArray();
			}
		}

		protected override void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
			if (Supplier != null && Importer != null && !JE_MessageTypeInfo.ReadOnly && !IsNonTransportDeclarationType)
			{
				var provider = new EuropeanUnionCustomsMembersProvider();
				var supplierCountryCode = Supplier.CountryCode;
				var importerCountryCode = Importer.CountryCode;

				var isImporterInEuOrFRButNotDrom = provider.IsInEuropeanCustomsUnion(importerCountryCode) && (!CountryCodes.IsUnderFrenchCustomsJurisdiction(importerCountryCode) || importerCountryCode == CountryCodes.France);
				var isSupplierOutsideEuOrDrom = !provider.IsInEuropeanCustomsUnion(supplierCountryCode) || (CountryCodes.IsUnderFrenchCustomsJurisdiction(supplierCountryCode) && supplierCountryCode != CountryCodes.France);
				var isFrenchCompany = Company.Country.Code == CountryCodes.France;
				var isCompanyCountryMatchingImporterButNotSupplier = Company.Country.Code == importerCountryCode && Company.Country.Code != supplierCountryCode;

				var result = EUJobMessageTypeList.Codes.Export;

				if (isFrenchCompany)
				{
					if (isSupplierOutsideEuOrDrom && isImporterInEuOrFRButNotDrom)
					{
						result = EUJobMessageTypeList.Codes.Import;
					}
				}
				else
				{
					if (isCompanyCountryMatchingImporterButNotSupplier)
					{
						result = EUJobMessageTypeList.Codes.Import;
					}
				}

				JE_MessageType = result;
			}
		}

		protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => EUCommonConstants.TransportModeSource.TransportModeAtBorder;

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new InventorySelectionHeader(this);
		}

		public override bool UseTariffDescriptionForEntryLine => false;

		protected override void SetRepresentationTypeIfMatchingEORICodes()
		{
			if (IsUCC6AndIsImport && GetValueSetStrategy() is DeltaIEJobDeclarationValueSetStrategy valueSetStrategy)
			{
				valueSetStrategy.SetRepresentationMode(null);
			}
			else
			{
				base.SetRepresentationTypeIfMatchingEORICodes();
			}
		}

		protected override bool SupportsCalculateInsuranceCore => false;

		public bool IsDeclarationStandard => AdditionalInfos.OfType<AdditionalInfo>().Any(x => x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration);
	}
}
