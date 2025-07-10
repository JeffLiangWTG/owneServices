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
	public class ECCCPGAHeader :
		AutoECCCPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		IPurgeValueParent,
		ILPCOContactParent,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter,
		ISetterSuspenderSupporter
	{
		public ECCCPGAHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoECCCPGAHeader.Schema
		{
			public const string OA_Manufacturer = "OA_Manufacturer";
			public const string JI_BrandName = "JI_BrandName";
			public const string JI_Model = "JI_Model";
		}

		public ZBool EngineGroupBoxVisible
		{
			get
			{
				var processCode = CA_ProcessCode;
				return processCode == ProcessCodes.Codes.XE01 || processCode == ProcessCodes.Codes.XE02 || processCode == ProcessCodes.Codes.XE03 || processCode == ProcessCodes.Codes.XE04;
			}
		}

		public ZBool VehicleGroupBoxVisible
		{
			get
			{
				var processCode = CA_ProcessCode;
				return processCode == ProcessCodes.Codes.XE01 || processCode == ProcessCodes.Codes.XE04;
			}
		}

		public ZBool MachineGroupBoxVisible
		{
			get
			{
				var processCode = CA_ProcessCode;
				return processCode == ProcessCodes.Codes.XE02 || processCode == ProcessCodes.Codes.XE03;
			}
		}

		public ZBool TestGroupTextBoxVisible => CA_ProcessCode == ProcessCodes.Codes.XE01;

		public ZBool PowerCalcDropEditVisible => MachineGroupBoxVisible;

		public ZBool EvaporativeFamilyTextBoxVisible => CA_ProcessCode == ProcessCodes.Codes.XE02 || CA_ProcessCode == ProcessCodes.Codes.XE03 || CA_ProcessCode == ProcessCodes.Codes.XE04;

		public ZBool MachineModelYearDropEditVisible => CA_ProcessCode == ProcessCodes.Codes.XE02;

		public ZBool TransitionCheckBoxIncompleteCheckBoxVisible => !XE02SpecificControlsVisible;

		public ZBool XE02SpecificControlsVisible => CA_ProcessCode == ProcessCodes.Codes.XE02;

		public ZBool EngineComplianceStatementGroupBoxVisible
		{
			get
			{
				var processCode = CA_ProcessCode;
				return processCode == ProcessCodes.Codes.XE01 || processCode == ProcessCodes.Codes.XE04;
			}
		}

		public bool IsVehicleDetailsRequiredForXE01 => CA_VEEProgramInd == YesNoList.Codes.Yes && CA_ProcessCode == ProcessCodes.Codes.XE01 && CA_Incomplete && !CA_VehicleClass.IsEmpty;

		public bool IsEngineDetailsRequiredForXE01 => CA_VEEProgramInd == YesNoList.Codes.Yes && CA_ProcessCode == ProcessCodes.Codes.XE01 && CA_Incomplete && !CA_EngineClass.IsEmpty;

		public bool IsXE04Process => CA_ProcessCode == ProcessCodes.Codes.XE04;

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
		[PurgeValue(nameof(AreOSDAndWENAndWRMDisabledAndVEEDisabledWhenIID402NotXE02))]
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
				dec.LPCOViews.ECCCCountChanged -= ParentECCCCountChanged;
				dec.LPCOViews.ECCCCountChanged += ParentECCCCountChanged;
			}
		}

		void ParentECCCCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		[ChildEditable]
		[PurgeValue(nameof(AreOSDAndWENDisabled))]
		public ComponentCollection Components
		{
			get
			{
				if (components == null)
				{
					components = new ComponentCollection(this);
					components.Load();
					RegisterEditableChildObject(components);
				}
				return components;
			}
		}
		ComponentCollection components;

		#endregion

		#region Overrides

		[PurgeValue(nameof(AreWENAndWRMDisabled))]
		public override ZString CA_IntendedUseCode
		{
			get { return base.CA_IntendedUseCode; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_IntendedUseCode))
				{
					base.CA_IntendedUseCode = value;
				}
			}
		}

		[PurgeValue(nameof(IsODSDisabled))]
		public override ZString CA_CASNumber
		{
			get { return base.CA_CASNumber; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_CASNumber))
				{
					base.CA_CASNumber = value;
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(ECCCPGAHeaderAddInfoLookups.EngineModelYearList))]
		[PurgeValue(nameof(IsVEEDisabled))]
		public override ZString CA_EngineModelYear
		{
			get { return base.CA_EngineModelYear; }
			set { base.CA_EngineModelYear = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(ECCCPGAHeaderAddInfoLookups.MachineModelYearList))]
		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZString CA_MachineModelYear
		{
			get { return base.CA_MachineModelYear; }
			set { base.CA_MachineModelYear = value; }
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE01AndXE04))]
		public override ZBool CA_ENGCanadaUnique
		{
			get => base.CA_ENGCanadaUnique;
			set => base.CA_ENGCanadaUnique = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE01AndXE04))]
		public override ZBool CA_ENGEPACertified
		{
			get => base.CA_ENGEPACertified;
			set => base.CA_ENGEPACertified = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE01AndXE04))]
		public override ZBool CA_ENGIncomplete
		{
			get => base.CA_ENGIncomplete;
			set => base.CA_ENGIncomplete = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE01AndXE04))]
		public override ZBool CA_ENGNationalMark
		{
			get => base.CA_ENGNationalMark;
			set => base.CA_ENGNationalMark = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02AndXE03))]
		public override ZDecimal CA_EnginePowerRating
		{
			get => base.CA_EnginePowerRating;
			set => base.CA_EnginePowerRating = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02AndXE03))]
		public override ZString CA_MakeOfMachine
		{
			get => base.CA_MakeOfMachine;
			set => base.CA_MakeOfMachine = value;
		}

		[PurgeValue(nameof(IsVEEDisabled))]
		public override ZString CA_ModelOfEngine
		{
			get => base.CA_ModelOfEngine;
			set => base.CA_ModelOfEngine = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02AndXE03))]
		public override ZString CA_PowerRatingUQ
		{
			get => base.CA_PowerRatingUQ;
			set => base.CA_PowerRatingUQ = value;
		}

		public override ZString CA_WRMProgramInd
		{
			get => base.CA_WRMProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_WRMProgramInd))
				{
					var oldValue = base.CA_WRMProgramInd;
					if (oldValue != value)
					{
						base.CA_WRMProgramInd = value;

						if (value == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						if (!IsCopying && oldValue == YesNoList.Codes.Yes)
						{
							((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
						}
					}
				}
			}
		}

		public override ZString CA_ODSProgramInd
		{
			get => base.CA_ODSProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_ODSProgramInd))
				{
					var oldValue = base.CA_ODSProgramInd;
					if (oldValue != value)
					{
						base.CA_ODSProgramInd = value;
						if (!IsCopying && oldValue == YesNoList.Codes.Yes)
						{
							((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
						}
					}
				}
			}
		}

		public override ZString CA_VEEProgramInd
		{
			get => base.CA_VEEProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_VEEProgramInd))
				{
					var oldValue = base.CA_VEEProgramInd;
					if (oldValue != value)
					{
						base.CA_VEEProgramInd = value;
						if (!IsCopying && oldValue == YesNoList.Codes.Yes)
						{
							((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
						}
					}
				}
			}
		}

		public override ZString CA_WENProgramInd
		{
			get => base.CA_WENProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_WENProgramInd))
				{
					var oldValue = base.CA_WENProgramInd;
					base.CA_WENProgramInd = value;
					if (oldValue != value)
					{
						InvoiceLine?.InvoiceHeader?.RefreshInvoiceLinesWithWENIndOnECCCPGA();
						if (!IsCopying && oldValue == YesNoList.Codes.Yes)
						{
							((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
						}
					}
				}
			}
		}

		public override ZString CA_ProcessCode
		{
			get => base.CA_ProcessCode;
			set
			{
				var oldValue = base.CA_ProcessCode;
				if (oldValue != value)
				{
					base.CA_ProcessCode = value;

					if (!IsCopying && !IsVEEDisabled)
					{
						((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
					}

					if (MachineGroupBoxVisible && CA_MachineManufacturer.IsEmpty && RequirementsParent is IHasPGARequirements requirementsParent)
					{
						CA_MachineManufacturer = requirementsParent.OA_Manufacturer;
					}

					if (CA_ProcessCode == ProcessCodes.Codes.XE02 && InvoiceLine is JobComInvoiceLine invoiceLine && invoiceLine.Declaration is JobDeclaration declaration)
					{
						CA_OA_EngineLocation = invoiceLine.ConsigneeAddress?.PK ?? ZGuid.Empty;
						CA_OA_EvidenceOfConformityLocation = !declaration.JE_OA_DeclarantAddress.IsEmpty ? declaration.JE_OA_DeclarantAddress : (invoiceLine.Importer?.MainAddress.PK ?? ZGuid.Empty);
					}
					else
					{
						CA_OA_EngineLocation = ZGuid.Empty;
						CA_OA_EvidenceOfConformityLocation = ZGuid.Empty;
					}

					CA_OA_EngineLocationInfo.RefreshBinding();
					CA_OA_EvidenceOfConformityLocationInfo.RefreshBinding();

					JI_BrandNameInfo.RefreshBinding();
					JI_ModelInfo.RefreshBinding();
				}
			}
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE01AndXE02AndXE04))]
		public override ZBool CA_BulkReporting
		{
			get => base.CA_BulkReporting;
			set
			{
				if (base.CA_BulkReporting != value)
				{
					base.CA_BulkReporting = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_NationalMark();
						AddInfoValidation.ValidateCA_NonCommercialImport();
					}
				}
			}
		}

		public override ZBool CA_NationalMark
		{
			get => base.CA_NationalMark;
			set
			{
				if (base.CA_NationalMark != value)
				{
					base.CA_NationalMark = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_ProcessCode();
						AddInfoValidation.ValidateCA_BulkReporting();
						AddInfoValidation.ValidateCA_NonCommercialImport();
					}
				}
			}
		}

		public override ZBool CA_EPACertified
		{
			get => base.CA_EPACertified;
			set
			{
				if (base.CA_EPACertified != value)
				{
					base.CA_EPACertified = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_ProcessCode();
					}
				}
			}
		}

		public override ZBool CA_CanadaUnique
		{
			get => base.CA_CanadaUnique;
			set
			{
				if (base.CA_CanadaUnique != value)
				{
					base.CA_CanadaUnique = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_ProcessCode();
					}
				}
			}
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsXE02))]
		public override ZBool CA_Incomplete
		{
			get => base.CA_Incomplete;
			set
			{
				if (base.CA_Incomplete != value)
				{
					base.CA_Incomplete = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_ProcessCode();
					}
				}
			}
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsXE02))]
		public override ZBool CA_Transition
		{
			get => base.CA_Transition;
			set
			{
				if (base.CA_Transition != value)
				{
					base.CA_Transition = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_ProcessCode();
					}
				}
			}
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE01AndXE04))]
		public override ZString CA_VehicleClass
		{
			get => base.CA_VehicleClass;
			set
			{
				if (base.CA_VehicleClass != value)
				{
					base.CA_VehicleClass = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_Incomplete();
					}
				}

				JI_BrandNameInfo.RefreshBinding();
				JI_ModelInfo.RefreshBinding();
			}
		}

		public override ZString CA_EngineClass
		{
			get => base.CA_EngineClass;
			set
			{
				if (base.CA_EngineClass != value)
				{
					base.CA_EngineClass = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_Incomplete();
					}
				}
			}
		}

		public override ZBool CA_NonCommercialImport
		{
			get { return base.CA_NonCommercialImport; }
			set
			{
				if (base.CA_NonCommercialImport != value)
				{
					base.CA_NonCommercialImport = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_BulkReporting();
						AddInfoValidation.ValidateCA_NationalMark();
					}
				}
			}
		}

		public OrgAddress MachineManufacturer
		{
			get { return Factory.Load<OrgAddress>(CA_MachineManufacturer); }
		}

		[List(nameof(CA_MachineManufacturer_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid CA_MachineManufacturer
		{
			get
			{
				return base.CA_MachineManufacturer;
			}
			set
			{
				base.CA_MachineManufacturer = value;
			}
		}

		public ZAddress CA_MachineManufacturer_ZAddress
		{
			get
			{
				if (fCA_MachineManufacturer_ZAddress == null)
				{
					fCA_MachineManufacturer_ZAddress = new ZAddress(CA_MachineManufacturerInfo)
					{
						IsOrgVisible = true,
						DefaultAddressType = AddressType.NoDefault,
						GetDefaultAddress = header => GetAddressPk(header as OrgHeader)
					};

					RegisterEditableChildObject(fCA_MachineManufacturer_ZAddress);
				}
				return fCA_MachineManufacturer_ZAddress;
			}
		}
		ZAddress fCA_MachineManufacturer_ZAddress;

		public OrgAddress EngineLocation
		{
			get { return Factory.Load<OrgAddress>(CA_OA_EngineLocation); }
		}

		public ZAddress CA_OA_EngineLocation_ZAddress
		{
			get
			{
				if (fCA_OA_EngineLocation_ZAddress == null)
				{
					fCA_OA_EngineLocation_ZAddress = new ZAddress(CA_OA_EngineLocationInfo)
					{
						IsOrgVisible = true,
						DefaultAddressType = AddressType.NoDefault,
						GetDefaultAddress = header => GetAddressPk(header as OrgHeader)
					};
					RegisterEditableChildObject(fCA_OA_EngineLocation_ZAddress);
				}
				return fCA_OA_EngineLocation_ZAddress;
			}
		}
		ZAddress fCA_OA_EngineLocation_ZAddress;

		public OrgAddress EvidenceOfConformityLocation
		{
			get { return Factory.Load<OrgAddress>(CA_OA_EvidenceOfConformityLocation); }
		}

		public ZAddress CA_OA_EvidenceOfConformityLocation_ZAddress
		{
			get
			{
				if (fCA_OA_EvidenceOfConformityLocation_ZAddress == null)
				{
					fCA_OA_EvidenceOfConformityLocation_ZAddress = new ZAddress(CA_OA_EvidenceOfConformityLocationInfo)
					{
						IsOrgVisible = true,
						DefaultAddressType = AddressType.NoDefault,
						GetDefaultAddress = header => GetAddressPk(header as OrgHeader)
					};
					RegisterEditableChildObject(fCA_OA_EvidenceOfConformityLocation_ZAddress);
				}
				return fCA_OA_EvidenceOfConformityLocation_ZAddress;
			}
		}
		ZAddress fCA_OA_EvidenceOfConformityLocation_ZAddress;

		ZGuid GetAddressPk(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE01))]
		public override ZString CA_TestGroupName
		{
			get => base.CA_TestGroupName;
			set => base.CA_TestGroupName = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZString CA_AlternativeStandardOfEngineClass
		{
			get => base.CA_AlternativeStandardOfEngineClass;
			set => base.CA_AlternativeStandardOfEngineClass = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZString CA_AOSConformity
		{
			get => base.CA_AOSConformity;
			set => base.CA_AOSConformity = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZString CA_AOSEvidence
		{
			get => base.CA_AOSEvidence;
			set => base.CA_AOSEvidence = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZString CA_AOSReplacement
		{
			get => base.CA_AOSReplacement;
			set => base.CA_AOSReplacement = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZString CA_AOSRetention
		{
			get => base.CA_AOSRetention;
			set => base.CA_AOSRetention = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZGuid CA_OA_EngineLocation
		{
			get => base.CA_OA_EngineLocation;
			set => base.CA_OA_EngineLocation = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZGuid CA_OA_EvidenceOfConformityLocation
		{
			get => base.CA_OA_EvidenceOfConformityLocation;
			set => base.CA_OA_EvidenceOfConformityLocation = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02))]
		public override ZBool CA_ReplacementEngines
		{
			get => base.CA_ReplacementEngines;
			set => base.CA_ReplacementEngines = value;
		}

		[PurgeValue(nameof(IsVEEDisabledOrProcessIsNotXE02AndXE04))]
		public override ZString CA_EvaporativeFamily
		{
			get => base.CA_EvaporativeFamily;
			set => base.CA_EvaporativeFamily = value;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LPCOViews.RemoveAndDeleteAll();
				Components.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#endregion

		#region Properties For Purge

		bool IsODSDisabled => CA_ODSProgramInd != YesNoList.Codes.Yes;

		bool IsVEEDisabled => CA_VEEProgramInd != YesNoList.Codes.Yes;

		bool IsWENDisabled => CA_WENProgramInd != YesNoList.Codes.Yes;

		bool IsWRMDisabled => CA_WRMProgramInd != YesNoList.Codes.Yes;

		bool AreWENAndWRMDisabled => IsWENDisabled && IsWRMDisabled;

		bool AreOSDAndWENDisabled => IsODSDisabled && IsWENDisabled;

		bool AreOSDAndWENAndWRMDisabledAndVEEDisabledWhenIID402NotXE02 => IsODSDisabled && IsWENDisabled && IsWRMDisabled && IsVEEDisabledOrProcessIsNotXE02;

		bool IsVEEDisabledOrProcessIsNotXE01AndXE04 => IsVEEDisabled || (CA_ProcessCode != ProcessCodes.Codes.XE01 && CA_ProcessCode != ProcessCodes.Codes.XE04);

		bool IsVEEDisabledOrProcessIsNotXE02AndXE03 => IsVEEDisabled || (CA_ProcessCode != ProcessCodes.Codes.XE02 && CA_ProcessCode != ProcessCodes.Codes.XE03);

		bool IsVEEDisabledOrProcessIsNotXE01 => IsVEEDisabled || CA_ProcessCode != ProcessCodes.Codes.XE01;

		bool IsVEEDisabledOrProcessIsNotXE02 => IsVEEDisabled || CA_ProcessCode != ProcessCodes.Codes.XE02;

		bool IsVEEDisabledOrProcessIsXE02 => IsVEEDisabled || CA_ProcessCode == ProcessCodes.Codes.XE02;

		bool IsVEEDisabledOrProcessIsNotXE01AndXE02AndXE04 => IsVEEDisabled || (CA_ProcessCode != ProcessCodes.Codes.XE01 && CA_ProcessCode != ProcessCodes.Codes.XE02 && CA_ProcessCode != ProcessCodes.Codes.XE04);

		bool IsVEEDisabledOrProcessIsNotXE02AndXE04 => IsVEEDisabled || (CA_ProcessCode != ProcessCodes.Codes.XE02 && CA_ProcessCode != ProcessCodes.Codes.XE04);

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

		public IHasPGARequirements RequirementsParent => (IHasPGARequirements)Parent;

		public ZString JI_BrandName
		{
			get => RequirementsParent?.JI_BrandName ?? ZString.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.JI_BrandName = value;
				}
			}
		}

		public ZPropertyInfo JI_BrandNameInfo => GetWrappedZPropertyInfo(nameof(JI_BrandName), x => RequirementsParent?.JI_BrandNameInfo ?? GetZPropertyInfo(Schema.JI_BrandName));

		public ZString JI_Model
		{
			get => RequirementsParent?.JI_Model ?? ZString.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.JI_Model = value;
				}
			}
		}

		public ZPropertyInfo JI_ModelInfo => GetWrappedZPropertyInfo(nameof(JI_Model), x => RequirementsParent?.JI_ModelInfo ?? GetZPropertyInfo(Schema.JI_Model));

		#endregion

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.ECCC;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			ECCCPGAHeader header = (ECCCPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
			Components.CopyPersistentValuesFrom(header.Components);
		}

		#endregion

		#region IPGAProgramRequirementProvider Members

		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case ECCCPGADepartmentCodes.Codes.ODS:
					return CA_ODSProgramIndInfo;
				case ECCCPGADepartmentCodes.Codes.WEN:
					return CA_WENProgramIndInfo;
				case ECCCPGADepartmentCodes.Codes.WRM:
					return CA_WRMProgramIndInfo;
				case ECCCPGADepartmentCodes.Codes.VEE:
					return CA_VEEProgramIndInfo;
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
				case ECCCPGADepartmentCodes.Codes.ODS:
					AddInfoValidation.ValidateCA_ODSProgramInd();
					break;
				case ECCCPGADepartmentCodes.Codes.WEN:
					AddInfoValidation.ValidateCA_WENProgramInd();
					break;
				case ECCCPGADepartmentCodes.Codes.WRM:
					AddInfoValidation.ValidateCA_WRMProgramInd();
					break;
				case ECCCPGADepartmentCodes.Codes.VEE:
					AddInfoValidation.ValidateCA_VEEProgramInd();
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
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CAComponent, typeof(Component));
			return result;
		}

		#endregion

		#region ILPCOContactParent

		IPGAContactDetails ILPCOContactParent.GetContactDetails(ZString type)
		{
			return (IPGAContactDetails)InvoiceLine?.GetContactDetails(type) ?? new PGAEmptyContactDetails();
		}

		#endregion

		#region IPurgeValueParent

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get => purgeWithMacroHelper ?? (purgeWithMacroHelper = new PurgeValueHelper<ECCCPGAHeader>(this));
		}
		PurgeValueHelper<ECCCPGAHeader> purgeWithMacroHelper;

		bool IPurgeValueParent.IsPurging { get; set; }

		#endregion

		#region Related BO

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
				var list = new List<ZString>
				{
					CusCALPCO.Schema.CLP_Type,
					CusCALPCO.Schema.CLP_RefNo
				};

				if (CA_WENProgramInd == YesNoList.Codes.Yes)
				{
					list.Add(CusCALPCO.Schema.CLP_EndDate);
					list.Add(CusCALPCO.Schema.CLP_HolderType);
				}
				return list;
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => CA_WRMProgramInd == YesNoList.Codes.Yes || CA_ODSProgramInd == YesNoList.Codes.Yes
			 || CA_WENProgramInd == YesNoList.Codes.Yes;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_IsHolderOverridden,
			CusCALPCO.Schema.CLP_HolderContactEmail,
			CusCALPCO.Schema.CLP_HolderContactName,
			CusCALPCO.Schema.CLP_HolderContactPhone,
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_EndDate,
			CusCALPCO.Schema.CLP_HolderType,
			CusCALPCO.Schema.LPCOHolderOrgPK,
			CusCALPCO.Schema.CLP_OA_Holder,
			CusCALPCO.Schema.CLP_HolderName,
			CusCALPCO.Schema.CLP_IssueDate,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_Type,
			CusCALPCO.Schema.CLP_AlternativeQuotaQuantity,
			CusCALPCO.Schema.CLP_AlternativeQuotaUQ
		);

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		public IEnumerable<string> SupportedFields => supportedFields ?? (supportedFields = new string[] {
			ECCCPGAHeader.Schema.CA_IntendedUseCode,
			ECCCPGAHeader.Schema.CA_CASNumber,
			ECCCPGAHeader.Schema.CA_ODSProgramInd,
			ECCCPGAHeader.Schema.CA_VEEProgramInd,
			ECCCPGAHeader.Schema.CA_WENProgramInd,
			ECCCPGAHeader.Schema.CA_WRMProgramInd
		});
		IEnumerable<string> supportedFields;

		#endregion
	}
}
