using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class DFOPGAHeader :
		AutoDFOPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		IPurgeValueParent,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter
	{
		public DFOPGAHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoDFOPGAHeader.Schema
		{
			public const string OA_Manufacturer = "OA_Manufacturer";
			public const string RN_NKCountryOfOrigin = "RN_NKCountryOfOrigin";
		}

		#region Related

		[ChildEditable]
		CusCALPCOCollection LPCOs
		{
			get
			{
				if (lpcos == null)
				{
					lpcos = new CusCALPCOCollection(this);
					lpcos.Load();
					RegisterEditableChildObject(lpcos);
				}
				return lpcos;
			}
		}

		CusCALPCOCollection lpcos;

		[ChildEditable]
		[PurgeValue(nameof(AllProgramsDisabled))]
		public LPCOViewCollection LPCOViews
		{
			get
			{
				if (lpcoViews == null)
				{
					var dec = (InvoiceLine as IDeclarationProvider)?.Declaration as JobDeclaration;
					lpcoViews = new LPCOViewCollection(LPCOs, dec?.LPCOs, this);
					RegisterEditableChildObject(lpcoViews);
					HookParentLPCOViewsCountChange(dec);
				}
				return lpcoViews;
			}
		}
		LPCOViewCollection lpcoViews;

		void HookParentLPCOViewsCountChange(JobDeclaration dec)
		{
			if (dec != null && dec.LPCOViews != null)
			{
				dec.LPCOViews.DFOCountChanged -= ParentDFOCountChanged;
				dec.LPCOViews.DFOCountChanged += ParentDFOCountChanged;
			}
		}

		void ParentDFOCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		#endregion

		#region Properties

		public override ZString CA_ABIProgramInd
		{
			get => base.CA_ABIProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_ABIProgramInd))
				{
					var oldValue = base.CA_ABIProgramInd;
					if (oldValue != value)
					{
						base.CA_ABIProgramInd = value;
						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_AISProgramInd
		{
			get => base.CA_AISProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_AISProgramInd))
				{
					var oldValue = base.CA_AISProgramInd;
					if (oldValue != value)
					{
						base.CA_AISProgramInd = value;
						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_TTPProgramInd
		{
			get => base.CA_TTPProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_TTPProgramInd))
				{
					var oldValue = base.CA_TTPProgramInd;
					if (oldValue != value)
					{
						base.CA_TTPProgramInd = value;
						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		void PurgeValuesIfNeed(ZString oldValue)
		{
			if (!IsCopying && oldValue == YesNoList.Codes.Yes)
			{
				((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.CommissionList))]
		public override ZString CA_Commission
		{
			get => base.CA_Commission;
			set => base.CA_Commission = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.CommonNameCodes))]
		public override ZString CA_CommonNameCode
		{
			get => base.CA_CommonNameCode;
			set
			{
				var oldValue = base.CA_CommonNameCode;

				if (oldValue != value)
				{
					base.CA_CommonNameCode = value;

					if (CA_TTPProgramInd == YesNoList.Codes.Yes && !string.IsNullOrWhiteSpace(value))
					{
						var invoiceLine = InvoiceLine;
						if (invoiceLine != null)
						{
							invoiceLine.JI_Model = AddInfoLookups.CommonNameCodes.GetDescriptionFromCode(value);
						}
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.ScientificNames))]
		public override ZString CA_SpeciesCode
		{
			get => base.CA_SpeciesCode;
			set
			{
				var oldValue = base.CA_SpeciesCode;

				if (oldValue != value)
				{
					base.CA_SpeciesCode = value;

					if (CA_AISProgramInd == YesNoList.Codes.Yes && !string.IsNullOrWhiteSpace(value))
					{
						CA_GenusOrSpecies = AddInfoLookups.ScientificNames.GetDescriptionFromCode(value);
					}
				}
			}
		}

		[ReadOnlyMember(nameof(CA_GenusOrSpecies_ReadOnly))]
		public override ZString CA_GenusOrSpecies
		{
			get => base.CA_GenusOrSpecies;
			set => base.CA_GenusOrSpecies = value;
		}

		bool CA_GenusOrSpecies_ReadOnly
		{
			get
			{
				if (cachedGenusOrSpeciesReadOnly == null)
				{
					cachedGenusOrSpeciesReadOnly = new CachedProperty<bool>(Factory,
						() => CA_AISProgramInd == YesNoList.Codes.Yes && AddInfoLookups.ScientificNames.ContainsCode(CA_SpeciesCode));
				}

				return cachedGenusOrSpeciesReadOnly.Value;
			}
		}
		CachedProperty<bool> cachedGenusOrSpeciesReadOnly;

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.HarvestingParties))]
		public override ZGuid CA_OA_HarvestingParty
		{
			get => base.CA_OA_HarvestingParty;
			set
			{
				var oldValue = base.CA_OA_HarvestingParty;

				if (oldValue != value)
				{
					base.CA_OA_HarvestingParty = value;

					if (((IPurgeValueParent)this).IsPurging && value.IsEmpty)
					{
						CA_OA_HarvestingParty_ZAddress.OrgPKInfo.ClearValue();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.Processors))]
		[PurgeValue(nameof(AreAISAndTTPDisabled))]
		public override ZGuid CA_OA_Processor
		{
			get => base.CA_OA_Processor;
			set
			{
				var oldValue = base.CA_OA_Processor;

				if (oldValue != value)
				{
					base.CA_OA_Processor = value;

					if (((IPurgeValueParent)this).IsPurging && value.IsEmpty)
					{
						CA_OA_Processor_ZAddress.OrgPKInfo.ClearValue();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.DirectionList))]
		public override ZString CA_Direction
		{
			get => base.CA_Direction;
			set => base.CA_Direction = value;
		}

		public ZAddress CA_OA_HarvestingParty_ZAddress
		{
			get
			{
				if (fCA_OA_HarvestingParty_ZAddress == null)
				{
					fCA_OA_HarvestingParty_ZAddress = new ZAddress(CA_OA_HarvestingPartyInfo)
					{
						IsOrgVisible = true,
						DefaultAddressType = AddressType.NoDefault,
						GetDefaultAddress = header => GetAddressPk(header as OrgHeader)
					};

					RegisterEditableChildObject(fCA_OA_HarvestingParty_ZAddress);
				}
				return fCA_OA_HarvestingParty_ZAddress;
			}
		}
		ZAddress fCA_OA_HarvestingParty_ZAddress;

		public ZAddress CA_OA_Processor_ZAddress
		{
			get
			{
				if (fCA_OA_Processor_ZAddress == null)
				{
					fCA_OA_Processor_ZAddress = new ZAddress(CA_OA_ProcessorInfo)
					{
						IsOrgVisible = true,
						DefaultAddressType = AddressType.NoDefault,
						GetDefaultAddress = header => GetAddressPk(header as OrgHeader)
					};

					RegisterEditableChildObject(fCA_OA_Processor_ZAddress);
				}
				return fCA_OA_Processor_ZAddress;
			}
		}
		ZAddress fCA_OA_Processor_ZAddress;

		ZGuid GetAddressPk(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		#region CA_Category

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.CategoryList))]
		public override ZString CA_Category
		{
			get => base.CA_Category;
			set => base.CA_Category = value;
		}

		public ZBool HasAsianCarpSpeciesOrQuaggaOrZebraMussels
		{
			get
			{
				return CA_AISProgramInd == YesNoList.Codes.Yes
					&& (CA_SpeciesCode == DFOScientificNames.Codes.FO11
						|| CA_SpeciesCode == DFOScientificNames.Codes.FO12
						|| CA_SpeciesCode == DFOScientificNames.Codes.FO13
						|| CA_SpeciesCode == DFOScientificNames.Codes.FO14
						|| CA_SpeciesCode == DFOScientificNames.Codes.FO16
						|| CA_SpeciesCode == DFOScientificNames.Codes.FO17);
			}
		}

		#endregion

		public bool IsCurrentLPCOReadOnly => NSNLPCOManager != null && NSNLPCOManager.CurrentLPCO != null && NSNLPCOManager.CurrentLPCO.ReadOnly;

		#region NSNNumber

		[MaxLength(AutoCusCALPCO.Schema.CLP_RefNoMaxLength)]
		[ReadOnlyMember(nameof(IsCurrentLPCOReadOnly))]
		public ZString NSNNumber
		{
			get => NSNLPCOManager.GetValueSafe<ZString>(AutoCusCALPCO.Schema.CLP_RefNo);
			set
			{
				NSNLPCOManager.SetValueSafe(AutoCusCALPCO.Schema.CLP_RefNo, value);
				NSNNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NSNNumberInfo
		{
			get => GetZPropertyInfo(nameof(NSNNumber));
		}

		LPCOValueManager NSNLPCOManager
		{
			get
			{
				if (nsnLPCOManager == null)
				{
					Func<LPCOView> getter = () => LPCOViews.OfType<LPCOView>()
					.FirstOrDefault(c => c.CLP_Type == DFODocumentTypes.Codes.AquaticBiotechnologyNewSubstancesNotification && !c.IsDeleted);

					var lazyCollection = new Lazy<LPCOViewCollection>(() => LPCOViews);

					nsnLPCOManager = new LPCOValueManager(lazyCollection, DFODocumentTypes.Codes.AquaticBiotechnologyNewSubstancesNotification, getter);
				}

				return nsnLPCOManager;
			}
		}
		LPCOValueManager nsnLPCOManager;

		#endregion

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_IUA
		{
			get => base.CA_IUA;
			set => base.CA_IUA = value;
		}

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_IUEA
		{
			get => base.CA_IUEA;
			set => base.CA_IUEA = value;
		}

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_IUF
		{
			get => base.CA_IUF;
			set => base.CA_IUF = value;
		}

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_IUO
		{
			get => base.CA_IUO;
			set => base.CA_IUO = value;
		}

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_IUOTH
		{
			get => base.CA_IUOTH;
			set => base.CA_IUOTH = value;
		}

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_IURAD
		{
			get => base.CA_IURAD;
			set => base.CA_IURAD = value;
		}

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_IUSCP
		{
			get => base.CA_IUSCP;
			set => base.CA_IUSCP = value;
		}

		[PurgeValue(nameof(AreABIAndTTPDisabled))]
		public override ZBool CA_SexHermaphrodite
		{
			get => base.CA_SexHermaphrodite;
			set => base.CA_SexHermaphrodite = value;
		}

		[PurgeValue(nameof(AreAISAndTTPDisabled))]
		public override ZBool CA_Eviscerated
		{
			get => base.CA_Eviscerated;
			set => base.CA_Eviscerated = value;
		}

		[PurgeValue(nameof(AreAISAndTTPDisabled))]
		public override ZBool CA_LifeStageDead
		{
			get => base.CA_LifeStageDead;
			set => base.CA_LifeStageDead = value;
		}

		[PurgeValue(nameof(AreAISAndTTPDisabled))]
		public override ZBool CA_SexUnknown
		{
			get => base.CA_SexUnknown;
			set => base.CA_SexUnknown = value;
		}

		#endregion

		#region Properties For Purge

		bool IsABIDisabled => CA_ABIProgramInd != YesNoList.Codes.Yes;

		bool IsAISDisabled => CA_AISProgramInd != YesNoList.Codes.Yes;

		bool IsTTPDisabled => CA_TTPProgramInd != YesNoList.Codes.Yes;

		bool AreAISAndTTPDisabled => IsAISDisabled && IsTTPDisabled;

		bool AreABIAndTTPDisabled => IsABIDisabled && IsTTPDisabled;

		bool AllProgramsDisabled => IsABIDisabled && IsAISDisabled && IsTTPDisabled;

		#endregion

		#region Overrides

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LPCOViews.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		#region IHasPGARequirements

		[List(nameof(OA_Manufacturer_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid OA_Manufacturer
		{
			get => RequirementsParent?.OA_Manufacturer ?? ZGuid.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.OA_Manufacturer = value;
				}
			}
		}
		public ZPropertyInfo OA_ManufacturerInfo => GetWrappedZPropertyInfo(nameof(OA_Manufacturer), x => RequirementsParent?.OA_ManufacturerInfo ?? GetZPropertyInfo(Schema.OA_Manufacturer));

		public ZAddress OA_Manufacturer_ZAddress
		{
			get
			{
				if (cachedOA_Manufacturer_ZAddress == null)
				{
					cachedOA_Manufacturer_ZAddress = new CachedProperty<ZAddress>(Factory, () =>
					{
						if (RequirementsParent is IHasPGARequirements requirementsParent)
						{
							return requirementsParent.OA_ManufacturerAddress_ZAddress;
						}
						else
						{
							return new ZAddress(OA_ManufacturerInfo);
						}
					});
				}
				return cachedOA_Manufacturer_ZAddress.Value;
			}
		}
		CachedProperty<ZAddress> cachedOA_Manufacturer_ZAddress;

		[List(nameof(AddInfoLookups) + "." + nameof(DFOPGAHeaderAddInfoLookups.CountryOfOriginsLookup))]
		[MaxLength(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CountryOfOriginMaxLength)]
		public ZString RN_NKCountryOfOrigin
		{
			get => RequirementsParent?.RN_NKCountryOfOrigin ?? ZString.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.RN_NKCountryOfOrigin = value;
				}
			}
		}
		public ZPropertyInfo RN_NKCountryOfOriginInfo => GetWrappedZPropertyInfo(nameof(RN_NKCountryOfOrigin), x => RequirementsParent?.RN_NKCountryOfOriginInfo ?? GetZPropertyInfo(Schema.RN_NKCountryOfOrigin));

		public IHasPGARequirements RequirementsParent => (IHasPGARequirements)Parent;

		#endregion

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.DFO;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			DFOPGAHeader header = (DFOPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
		}

		#endregion

		#region IPGAProgramRequirementProvider Members

		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case DFOPGADepartmentCodes.Codes.ABI:
					return CA_ABIProgramIndInfo;
				case DFOPGADepartmentCodes.Codes.AIS:
					return CA_AISProgramIndInfo;
				case DFOPGADepartmentCodes.Codes.TTP:
					return CA_TTPProgramIndInfo;
			}

			return null;
		}

		CodeDescriptionPairList IPGAProgramRequirementProvider.GetProgramCodesList()
		{
			return AddInfoLookups.ProgramCodesList;
		}

		void IPGAProgramRequirementProvider.ValidateProgramIndicator(ZString programCode)
		{
			switch (programCode)
			{
				case DFOPGADepartmentCodes.Codes.ABI:
					AddInfoValidation.ValidateCA_ABIProgramInd();
					break;
				case DFOPGADepartmentCodes.Codes.AIS:
					AddInfoValidation.ValidateCA_AISProgramInd();
					break;
				case DFOPGADepartmentCodes.Codes.TTP:
					AddInfoValidation.ValidateCA_TTPProgramInd();
					break;
			}
		}

		IDisposable IPGAProgramRequirementProvider.SuspendSettingDefaultValues()
		{
			return null;
		}

		SetterSuspender IPGAProgramRequirementProvider.SetterSuspender => SetterSuspender;

		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusAddInfoTypeSupporterFetchStrategy(this, true);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			return new Dictionary<ZString, Type>();
		}

		#endregion

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (cachedInvoiceLine == null || cachedInvoiceLine.PK != B7_ParentID || B7_ParentTableCode != JobComInvoiceLineSchema.Constants.Prefix)
				{
					cachedInvoiceLine = B7_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? (JobComInvoiceLine)Parent : null;
				}
				return cachedInvoiceLine;
			}
		}
		JobComInvoiceLine cachedInvoiceLine;

		public CusClassPartPivot PartPivot
		{
			get
			{
				if (cachedPartPivot == null || cachedPartPivot.PK != B7_ParentID || B7_ParentTableCode != CusClassPartPivotSchema.Constants.Prefix)
				{
					cachedPartPivot = B7_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix ? (CusClassPartPivot)Parent : null;
				}
				return cachedPartPivot;
			}
		}
		CusClassPartPivot cachedPartPivot;

		#region IPurgeValueParent

		bool IPurgeValueParent.IsPurging { get; set; }

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get => purgeHelper ?? (purgeHelper = new PurgeValueHelper<DFOPGAHeader>(this));
		}
		IPurgeValueHelper purgeHelper;

		#endregion

		#region IDeclarationProvider
		BaseJobDeclaration IDeclarationProvider.Declaration
		{
			get { return declaration ?? (declaration = (InvoiceLine as IDeclarationProvider)?.Declaration); }
		}
		BaseJobDeclaration declaration;
		ZBool ICADeclarationProvider.IsValidationEnabled => this.IsPGAValidationEnabled();
		#endregion

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && ((ICADeclarationProvider)this).IsValidationEnabled;
		}

		#region ILPCODefaulter
		IEnumerable<ZString> ILPCODefaulter.LPCOFieldsDefaultFromURN
		{
			get
			{
				return new List<ZString>
				{
					CusCALPCO.Schema.CLP_Type,
					CusCALPCO.Schema.CLP_RefNo
				};
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => CA_TTPProgramInd == YesNoList.Codes.Yes;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_Type
		);

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion
	}
}
