using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class NRCanPGAHeader :
		AutoNRCanPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		IPurgeValueParent,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter
	{
		public NRCanPGAHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoNRCanPGAHeader.Schema
		{
			public const string RN_NKCountryOfOrigin = "RN_NKCountryOfOrigin";
			public const string RW_NKOriginState = "RW_NKOriginState";
		}

		[List(nameof(AddInfoLookups) + "." + nameof(NRCanPGAHeaderAddInfoLookups.AuthorizedPartyTypeCodes))]
		public override ZString CA_AuthorizedParty { get => base.CA_AuthorizedParty; set => base.CA_AuthorizedParty = value; }

		#region Implemented

		[List(nameof(AddInfoLookups) + "." + nameof(NRCanPGAHeaderAddInfoLookups.UNDGCodeList))]
		public ZGuid DangerousGoodsDGSubs
		{
			get
			{
				return InvoiceLine?.DangerousGoodsDGSubs ?? ZGuid.Empty;
			}
			set
			{
				if (InvoiceLine != null)
				{
					var oldValue = InvoiceLine.DangerousGoodsDGSubs;

					InvoiceLine.DangerousGoodsDGSubs = value;
					DangerousGoodsDGSubsInfo.RefreshBinding();

					if (oldValue != InvoiceLine.DangerousGoodsDGSubs && !IsCopying)
					{
						Validation.ValidateDangerousGoodsDGSubs();
					}
				}
			}
		}

		public UNDGDataItem DangerousGoods => InvoiceLine?.DangerousGoods;

		public ZPropertyInfo DangerousGoodsDGSubsInfo => GetZPropertyInfo(nameof(DangerousGoodsDGSubs));

		public ZBool CA_IsNotRegulatedByOEE // Category
		{
			get => base.CA_Category == NRCanCategoryCodes.Codes.NR01;
			set
			{
				if (value.IsValid && value == ZBool.True)
				{
					base.CA_Category = NRCanCategoryCodes.Codes.NR01; // Product Not Regulated by OEE
				}
				else
				{
					base.CA_Category = NRCanCategoryCodes.Codes.NR02; // Product Regulated by OEE
				}

				CA_IsNotRegulatedByOEEInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IsNotRegulatedByOEEInfo
		{
			get { return GetZPropertyInfo(nameof(CA_IsNotRegulatedByOEE)); }
		}

		public ZBool CA_IsNotRegulatedByExplosives // IntendedUseCodes for Explosive Program
		{
			get => base.CA_IntendedUseCode == NRCanIntendedUseCodes.Codes.NR04;
			set
			{
				if (value.IsValid && value == ZBool.True)
				{
					base.CA_IntendedUseCode = NRCanIntendedUseCodes.Codes.NR04;
				}
				else
				{
					base.CA_IntendedUseCode = ZString.Empty;
				}

				CA_IsNotRegulatedByExplosivesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IsNotRegulatedByExplosivesInfo
		{
			get { return GetZPropertyInfo(nameof(CA_IsNotRegulatedByExplosives)); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(NRCanPGAHeaderAddInfoLookups.IntendedUseCodeList))]
		[PurgeValue(nameof(AreEEFAndEXPDisabled))]
		public override ZString CA_IntendedUseCode
		{
			get { return base.CA_IntendedUseCode; }
			set { base.CA_IntendedUseCode = value; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LPCOViews.RemoveAndDeleteAll();
			}
			base.Delete();
		}

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
				dec.LPCOViews.NRCanCountChanged -= ParentNRCanCountChanged;
				dec.LPCOViews.NRCanCountChanged += ParentNRCanCountChanged;
			}
		}

		void ParentNRCanCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		#endregion

		#region Overrides

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override ZString CA_EEFProgramInd
		{
			get => base.CA_EEFProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_EEFProgramInd))
				{
					var oldValue = base.CA_EEFProgramInd;
					if (oldValue != value)
					{
						base.CA_EEFProgramInd = value;
						PurgeValuesIfNeeded(oldValue);
					}
				}
			}
		}

		public override ZString CA_EXPProgramInd
		{
			get => base.CA_EXPProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_EXPProgramInd))
				{
					var oldValue = base.CA_EXPProgramInd;
					if (oldValue != value)
					{
						base.CA_EXPProgramInd = value;
						PurgeValuesIfNeeded(oldValue);
						Validation.ValidateDangerousGoodsDGSubs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(NRCanPGAHeaderAddInfoLookups.PGAIndicatorList))]
		public override ZString CA_RDAProgramInd
		{
			get => base.CA_RDAProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_RDAProgramInd))
				{
					var oldValue = base.CA_RDAProgramInd;
					if (base.CA_RDAProgramInd != value)
					{
						base.CA_RDAProgramInd = value;
						PurgeValuesIfNeeded(oldValue);
					}
				}
			}
		}

		#endregion

		#region IHasPGARequirements

		[List(nameof(AddInfoLookups) + "." + nameof(NRCanPGAHeaderAddInfoLookups.CountryOfOriginsLookup))]
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

		[List(nameof(AddInfoLookups) + "." + nameof(NRCanPGAHeaderAddInfoLookups.StateCodeListLookup))]
		[ResourceStringData("0cb6388b-0c0b-41b5-a804-6c1de9b19771", Caption = "State of Origin", ShortCaption = "State", MediumCaption = "Org. State", FullDescription = "The state for Country/Region of Origin.")]
		[MaxLength(2)]
		public ZString RW_NKOriginState
		{
			get => RequirementsParent?.RW_NKOriginState ?? ZString.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.RW_NKOriginState = value;
				}
			}
		}
		public ZPropertyInfo RW_NKOriginStateInfo => GetWrappedZPropertyInfo(nameof(RW_NKOriginState), x => RequirementsParent?.RW_NKOriginStateInfo ?? GetZPropertyInfo(Schema.RW_NKOriginState));

		public IHasPGARequirements RequirementsParent => (IHasPGARequirements)Parent;

		#endregion

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.NRCan;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			NRCanPGAHeader header = (NRCanPGAHeader)source;

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
				case NRCanPGADepartmentCodes.Codes.EEF:
					return CA_EEFProgramIndInfo;
				case NRCanPGADepartmentCodes.Codes.EXP:
					return CA_EXPProgramIndInfo;
				case NRCanPGADepartmentCodes.Codes.RDA:
					return CA_RDAProgramIndInfo;
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
				case NRCanPGADepartmentCodes.Codes.EEF:
					AddInfoValidation.ValidateCA_EEFProgramInd();
					break;
				case NRCanPGADepartmentCodes.Codes.EXP:
					AddInfoValidation.ValidateCA_EXPProgramInd();
					break;
				case NRCanPGADepartmentCodes.Codes.RDA:
					AddInfoValidation.ValidateCA_RDAProgramInd();
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

		#region Properties for Purge

		protected bool IsEEFDisabled => CA_EEFProgramInd != YesNoList.Codes.Yes;

		protected bool IsEXPDisabled => CA_EXPProgramInd != YesNoList.Codes.Yes;

		protected bool IsRDADisabled => CA_RDAProgramInd != YesNoList.Codes.Yes;

		protected bool AreEXPAndRDADisabled => IsEXPDisabled && IsRDADisabled;

		protected bool AreEEFAndEXPDisabled => IsEEFDisabled && IsEXPDisabled;

		void PurgeValuesIfNeeded(ZString oldValue)
		{
			if (!IsCopying && oldValue == YesNoList.Codes.Yes)
			{
				((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
			}
		}

		#endregion

		#region IPurgeValueParent

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get { return purgeWithMacroHelper ?? (purgeWithMacroHelper = new PurgeValueHelper<NRCanPGAHeader>(this)); }
		}
		PurgeValueHelper<NRCanPGAHeader> purgeWithMacroHelper;

		bool IPurgeValueParent.IsPurging { get; set; }

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

		#region Validation

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new NRCanPGAHeaderValidation(this);
		}

		public new NRCanPGAHeaderValidation Validation => (NRCanPGAHeaderValidation)base.Validation;

		#endregion

		#region ILPCODefaulter
		IEnumerable<ZString> ILPCODefaulter.LPCOFieldsDefaultFromURN
		{
			get
			{
				return new List<ZString>
				{
					CusCALPCO.Schema.CLP_Type,
					CusCALPCO.Schema.CLP_RefNo,
					CusCALPCO.Schema.CLP_EndDate,
					CusCALPCO.Schema.CLP_IssueDate
				};
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => CA_RDAProgramInd == YesNoList.Codes.Yes;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_IsApplicantOverridden,
			CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode,
			CusCALPCO.Schema.CLP_RN_NKOriginCountryCode,
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_IsMixedCountryOfOrigin,
			CusCALPCO.Schema.CLP_ApplicantType,
			CusCALPCO.Schema.LPCOApplicantOrgPK,
			CusCALPCO.Schema.CLP_ApplicantName,
			CusCALPCO.Schema.CLP_OA_Applicant,
			CusCALPCO.Schema.CLP_EndDate,
			CusCALPCO.Schema.CLP_IssueDate,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_Type
		);

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion
	}
}
