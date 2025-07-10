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
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TCPGAHeader :
		AutoTCPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		IPurgeValueParent,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter
	{
		public TCPGAHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTCPGAHeader.Schema
		{
			public const string OA_Manufacturer = "OA_Manufacturer";
			public const string RN_NKCountryOfOrigin = "RN_NKCountryOfOrigin";
			public const string JI_BrandName = "JI_BrandName";
			public const string JI_Model = "JI_Model";
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
		[PurgeValue(nameof(IsLPCOsDisabled))]
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

		void HookParentLPCOViewsCountChange(JobDeclaration dec)
		{
			if (dec != null && dec.LPCOViews != null)
			{
				dec.LPCOViews.TCCountChanged -= ParentTCCountChanged;
				dec.LPCOViews.TCCountChanged += ParentTCCountChanged;
			}
		}

		void ParentTCCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		LPCOViewCollection lpcoViews;

		#region CA_ImporterDeclarationCode

		[BusinessObjectTestExclude]
		public ZBool IsUSImporterDeclared
		{
			get => CA_ImporterDeclarationCode == TCComplicanceStatements.Codes.TC02 || CA_ImporterDeclarationCode == TCComplicanceStatements.Codes.TC08;
			set
			{
				if (value)
				{
					if (IsNewOrRetreadedTiresNeedDeclaraion)
					{
						CA_ImporterDeclarationCode = TCComplicanceStatements.Codes.TC02;
					}
					else if (IsUsedTiresNeedDeclaraion)
					{
						CA_ImporterDeclarationCode = TCComplicanceStatements.Codes.TC08;
					}
				}
				else
				{
					CA_ImporterDeclarationCode = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsZZImporterDeclared();
					Validation.ValidateIsUSImporterDeclared();
				}

				IsUSImporterDeclaredInfo.RefreshBinding();
				IsZZImporterDeclaredInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsUSImporterDeclaredInfo => GetZPropertyInfo(nameof(IsUSImporterDeclared));

		[BusinessObjectTestExclude]
		public ZBool IsZZImporterDeclared
		{
			get => CA_ImporterDeclarationCode == TCComplicanceStatements.Codes.TC01 || CA_ImporterDeclarationCode == TCComplicanceStatements.Codes.TC07;
			set
			{
				if (value)
				{
					if (IsNewOrRetreadedTiresNeedDeclaraion)
					{
						CA_ImporterDeclarationCode = TCComplicanceStatements.Codes.TC01;
					}
					else if (IsUsedTiresNeedDeclaraion)
					{
						CA_ImporterDeclarationCode = TCComplicanceStatements.Codes.TC07;
					}
				}
				else
				{
					CA_ImporterDeclarationCode = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsZZImporterDeclared();
					Validation.ValidateIsUSImporterDeclared();
				}

				IsZZImporterDeclaredInfo.RefreshBinding();
				IsUSImporterDeclaredInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsZZImporterDeclaredInfo => GetZPropertyInfo(nameof(IsZZImporterDeclared));

		[BusinessObjectTestExclude]
		public ZBool IsVPRImporterDeclared
		{
			get => CA_ImporterDeclarationCode == TCComplicanceStatements.Codes.TC04 || CA_ImporterDeclarationCode == TCComplicanceStatements.Codes.TC06;
			set
			{
				if (value)
				{
					if (CA_VPRProgramInd == YesNoList.Codes.Yes)
					{
						switch (CA_SubProgram)
						{
							case TCPGAVehicleProgramCodes.Codes.VFS:
							case TCPGAVehicleProgramCodes.Codes.VFC:
								CA_ImporterDeclarationCode = TCComplicanceStatements.Codes.TC06;
								break;
							case TCPGAVehicleProgramCodes.Codes.VVP:
								CA_ImporterDeclarationCode = TCComplicanceStatements.Codes.TC04;
								break;
						}
					}
				}
				else
				{
					CA_ImporterDeclarationCode = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsVPRImporterDeclared();
				}

				IsVPRImporterDeclaredInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsVPRImporterDeclaredInfo => GetZPropertyInfo(nameof(IsVPRImporterDeclared));

		public ZBool USImporterDeclarationVisibility => (IsNewOrRetreadedTiresNeedDeclaraion || IsUsedTiresNeedDeclaraion)
														&& ParentCountryOfOrigin == Core.Constants.CountryCodes.UnitedStates;
		public ZBool ZZImporterDeclarationVisibility => IsNewOrRetreadedTiresNeedDeclaraion || IsUsedTiresNeedDeclaraion;

		public bool IsNewOrRetreadedTiresNeedDeclaraion => CA_ImportReasonCode == TCIntendedUseCodes.Codes.TC01
														&& CA_ProductType == TCProductCategories.Codes.TC04
														&& (CA_ProductClass == TCProductCategories.Codes.TC01 || CA_ProductClass == TCProductCategories.Codes.TC02)
														&& NewOrRetreadedTiresNeedDeclaraionList.Contains(ParentTariffNumber.Substring(0, 8));
		public bool IsUsedTiresNeedDeclaraion => CA_ImportReasonCode == TCIntendedUseCodes.Codes.TC01
												&& CA_ProductType == TCProductCategories.Codes.TC04
												&& CA_ProductClass == TCProductCategories.Codes.TC03
												&& UsedTiresNeedDeclaraionList.Contains(ParentTariffNumber.Substring(0, 8));

		public bool VPRImporterShouldProvided => CA_VPRProgramInd == YesNoList.Codes.Yes
			&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS || CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC || CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP);

		public ZString ImporterDeclarationStateDescForBinding
		{
			get
			{
				var code = IsNewOrRetreadedTiresNeedDeclaraion ? TCComplicanceStatements.Codes.TC01 : TCComplicanceStatements.Codes.TC07;
				return AddInfoLookups.ImporterDeclarationList.GetDescriptionFromCode(code);
			}
		}
		public ZPropertyInfo ImporterDeclarationStateDescForBindingInfo => GetZPropertyInfo(nameof(ImporterDeclarationStateDescForBinding));

		public ZString USImporterDeclarationStateDescForBinding
		{
			get
			{
				var code = IsNewOrRetreadedTiresNeedDeclaraion ? TCComplicanceStatements.Codes.TC02 : TCComplicanceStatements.Codes.TC08;
				return AddInfoLookups.ImporterDeclarationList.GetDescriptionFromCode(code);
			}
		}
		public ZPropertyInfo USImporterDeclarationStateDescForBindingInfo => GetZPropertyInfo(nameof(ImporterDeclarationStateDescForBinding));

		internal readonly List<string> UsedTiresNeedDeclaraionList = new List<string>
		{
			"40122010",
			"40122020",
			"40122090"
		};

		internal readonly List<string> NewOrRetreadedTiresNeedDeclaraionList = new List<string>
		{
			"40111000",
			"40112000",
			"40114000",
			"40121100",
			"40111000",
			"40121900",
		};

		void SetDefaultImporterDeclarationCode()
		{
			if (CA_TPRProgramInd == YesNoList.Codes.Yes)
			{
				if (USImporterDeclarationVisibility)
				{
					IsUSImporterDeclared = true;
				}
				else if (ZZImporterDeclarationVisibility)
				{
					IsZZImporterDeclared = true;
				}
			}
		}

		#endregion

		public ZBool ManufacturerLetterAttached
		{
			get => CA_CriteriaConformance == TCComplicanceStatements.Codes.TC05;
			set
			{
				CA_CriteriaConformance = value ? (ZString)TCComplicanceStatements.Codes.TC05 : ZString.Empty;

				if (!IsValidationSuspended)
				{
					Validation.ValidateManufacturerLetterAttached();
					Validation.ValidateStatementLabelAttached();
				}

				if (value)
				{
					LPCOs.AddIfTypeNotExist(LPCODocumentTypeQualifier.Codes._4003);
				}

				ManufacturerLetterAttachedInfo.RefreshBinding();
				StatementLabelAttachedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ManufacturerLetterAttachedInfo => GetZPropertyInfo(nameof(ManufacturerLetterAttached));

		public ZBool StatementLabelAttached
		{
			get => CA_CriteriaConformance == TCComplicanceStatements.Codes.TC03;
			set
			{
				CA_CriteriaConformance = value ? (ZString)TCComplicanceStatements.Codes.TC03 : ZString.Empty;

				if (!IsValidationSuspended)
				{
					Validation.ValidateManufacturerLetterAttached();
					Validation.ValidateStatementLabelAttached();
				}

				ManufacturerLetterAttachedInfo.RefreshBinding();
				StatementLabelAttachedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StatementLabelAttachedInfo => GetZPropertyInfo(nameof(StatementLabelAttached));

		#endregion

		#region Overrides

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

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.CountryOfOriginsLookup))]
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

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.TC;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			TCPGAHeader header = (TCPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
		}

		#endregion

		#region Properties

		public override ZString CA_TPRProgramInd
		{
			get => base.CA_TPRProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_TPRProgramInd))
				{
					var oldValue = base.CA_TPRProgramInd;
					if (oldValue != value)
					{
						base.CA_TPRProgramInd = value;
						if (!IsCopying && oldValue == YesNoList.Codes.Yes)
						{
							((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
						}
					}
				}
			}
		}

		public override ZString CA_VPRProgramInd
		{
			get => base.CA_VPRProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_VPRProgramInd))
				{
					var oldValue = base.CA_VPRProgramInd;
					if (oldValue != value)
					{
						base.CA_VPRProgramInd = value;
						if (!IsCopying && oldValue == YesNoList.Codes.Yes)
						{
							((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
						}
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.ProductClassList))]
		[PurgeValue(nameof(IsProductClassDisabled))]
		public override ZString CA_ProductClass
		{
			get => base.CA_ProductClass;
			set
			{
				if (value != base.CA_ProductClass)
				{
					base.CA_ProductClass = value;
					SetDefaultImporterDeclarationCode();
					if (!IsValidationSuspended)
					{
						Validation.ValidateIsZZImporterDeclared();
						Validation.ValidateIsUSImporterDeclared();
					}

					ImporterDeclarationStateDescForBindingInfo.RefreshBinding();
					USImporterDeclarationStateDescForBindingInfo.RefreshBinding();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.ProductTypeList))]
		[PurgeValue(nameof(IsTPRDisabled))]
		public override ZString CA_ProductType
		{
			get => base.CA_ProductType;
			set
			{
				if (value != base.CA_ProductType)
				{
					base.CA_ProductType = value;
					SetDefaultImporterDeclarationCode();
					if (!IsValidationSuspended)
					{
						Validation.ValidateIsZZImporterDeclared();
						Validation.ValidateIsUSImporterDeclared();
					}

					ImporterDeclarationStateDescForBindingInfo.RefreshBinding();
					USImporterDeclarationStateDescForBindingInfo.RefreshBinding();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.ProductSizeList))]
		[PurgeValue(nameof(IsTPRDisabled))]
		public override ZString CA_ProductSize
		{
			get => base.CA_ProductSize;
			set => base.CA_ProductSize = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.ImportReasonCodeList))]
		[PurgeValue(nameof(IsTPRDisabled))]
		public override ZString CA_ImportReasonCode
		{
			get => base.CA_ImportReasonCode;
			set
			{
				if (value != base.CA_ImportReasonCode)
				{
					base.CA_ImportReasonCode = value;
					SetDefaultImporterDeclarationCode();
					if (!IsValidationSuspended)
					{
						Validation.ValidateIsZZImporterDeclared();
						Validation.ValidateIsUSImporterDeclared();
					}

					ImporterDeclarationStateDescForBindingInfo.RefreshBinding();
					USImporterDeclarationStateDescForBindingInfo.RefreshBinding();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.TitleStatusList))]
		[PurgeValue(nameof(IsTitleStatusDisabled))]
		public override ZString CA_TitleStatus
		{
			get => base.CA_TitleStatus;
			set => base.CA_TitleStatus = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.VehicleConditionList))]
		[PurgeValue(nameof(IsVehicleConditionDisabled))]
		public override ZString CA_VehicleCondition
		{
			get => base.CA_VehicleCondition;
			set => base.CA_VehicleCondition = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.VehicleStatusList))]
		[PurgeValue(nameof(IsVehicleStatusDisabled))]
		public override ZString CA_VehicleStatus
		{
			get => base.CA_VehicleStatus;
			set => base.CA_VehicleStatus = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.CriteriaConformanceList))]
		[PurgeValue(nameof(IsCriteriaConformanceDisabled))]
		public override ZString CA_CriteriaConformance
		{
			get => base.CA_CriteriaConformance;
			set => base.CA_CriteriaConformance = value;
		}

		[PurgeValue(nameof(IsChassisDisabled))]
		public override ZString CA_ChassisMake
		{
			get => base.CA_ChassisMake;
			set => base.CA_ChassisMake = value;
		}

		[PurgeValue(nameof(IsChassisDisabled))]
		public override ZString CA_ChassisManufacturerName
		{
			get => base.CA_ChassisManufacturerName;
			set => base.CA_ChassisManufacturerName = value;
		}

		[PurgeValue(nameof(IsChassisDisabled))]
		public override ZString CA_ChassisModel
		{
			get => base.CA_ChassisModel;
			set => base.CA_ChassisModel = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.ChassisYearList))]
		[PurgeValue(nameof(IsChassisDisabled))]
		public override ZString CA_ChassisYear
		{
			get => base.CA_ChassisYear;
			set => base.CA_ChassisYear = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.SubProgramCodesList))]
		[PurgeValue(nameof(IsVPRDisabled))]
		public override ZString CA_SubProgram
		{
			get => base.CA_SubProgram;
			set
			{
				if (value != base.CA_SubProgram)
				{
					base.CA_SubProgram = value;

					if (VPRImporterShouldProvided)
					{
						IsVPRImporterDeclared = true;
					}

					LPCOViews.AddDefaultLPCOs();
					if (!IsCopying && (CA_SubProgram.IsEmpty || AddInfoLookups.SubProgramCodesList.ContainsCode(CA_SubProgram)))
					{
						((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.ManufactureYearList))]
		[PurgeValue(nameof(IsVPRDisabledOrPIGEnabled))]
		public override ZString CA_ManufactureYear
		{
			get => base.CA_ManufactureYear;
			set
			{
				if (value != base.CA_ManufactureYear)
				{
					base.CA_ManufactureYear = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_ManufactureMonth();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(TCPGAHeaderAddInfoLookups.ManufactureMonthList))]
		[PurgeValue(nameof(IsVPRDisabledOrPIGEnabled))]
		public override ZString CA_ManufactureMonth
		{
			get => base.CA_ManufactureMonth;
			set
			{
				if (value != base.CA_ManufactureMonth)
				{
					base.CA_ManufactureMonth = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_ManufactureYear();
					}
				}
			}
		}

		[PurgeValue(nameof(IsImporterDeclarationDisabled))]
		public override ZString CA_ImporterDeclarationCode
		{
			get => base.CA_ImporterDeclarationCode;
			set => base.CA_ImporterDeclarationCode = value;
		}

		[PurgeValue(nameof(IsODOReadingDisabled))]
		public override ZString CA_ODOReading
		{
			get => base.CA_ODOReading;
			set => base.CA_ODOReading = value;
		}

		[PurgeValue(nameof(IsVPRDisabledOrPIGEnabled))]
		public override ZString CA_AssemblerName
		{
			get => base.CA_AssemblerName;
			set => base.CA_AssemblerName = value;
		}

		public bool IsVPR => CA_VPRProgramInd == YesNoList.Codes.Yes && AddInfoLookups.SubProgramCodesList.ContainsCode(CA_SubProgram);

		#endregion

		#region IPGAProgramRequirementProvider Members

		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case TCPGADepartmentCodes.Codes.TPR:
					return CA_TPRProgramIndInfo;
				case TCPGADepartmentCodes.Codes.VPR:
					return CA_VPRProgramIndInfo;
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
				case TCPGADepartmentCodes.Codes.TPR:
					AddInfoValidation.ValidateCA_TPRProgramInd();
					break;
				case TCPGADepartmentCodes.Codes.VPR:
					AddInfoValidation.ValidateCA_VPRProgramInd();
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

		#region Validation

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new TCPGAHeaderValidation(this);
		}

		public new TCPGAHeaderValidation Validation => (TCPGAHeaderValidation)base.Validation;

		#endregion

		#region Properties For Purge

		bool IsProductClassEnabled => IsTPREnabled
									|| IsVPREnabled
										&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP);

		bool IsProductClassDisabled => !IsProductClassEnabled;

		bool IsImporterDeclarationEnabled => IsTPREnabled
											|| IsVPREnabled
											&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
												|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
												|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP);

		bool IsImporterDeclarationDisabled => !IsImporterDeclarationEnabled;

		bool IsChassisEnabled => IsVPREnabled
								&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL
									|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
									|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
									|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
									|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
									|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR
									|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VUV);

		bool IsChassisDisabled => !IsChassisEnabled;

		bool IsCriteriaConformanceEnabled => IsVPREnabled
											&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL
												|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
												|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
												|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
												|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
												|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR);

		bool IsCriteriaConformanceDisabled => !IsCriteriaConformanceEnabled;

		bool IsVehicleConditionEnabled => IsVPREnabled
										&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
											|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR);

		bool IsVehicleConditionDisabled => !IsVehicleConditionEnabled;

		bool IsODOReadingEnabled => IsVPREnabled && (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS || CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC);

		bool IsODOReadingDisabled => !IsODOReadingEnabled;

		bool IsTitleStatusEnabled => IsVPREnabled && (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS || CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP);

		bool IsTitleStatusDisabled => !IsTitleStatusEnabled;

		bool IsVehicleStatusEnabled => IsVPREnabled && CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP;

		bool IsVehicleStatusDisabled => !IsVehicleStatusEnabled;

		bool IsLPCOsEnabled => IsVPREnabled
							&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
								|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
								|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
								|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
								|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR
								|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VUV
								|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP);

		bool IsLPCOsDisabled => !IsLPCOsEnabled;

		bool IsTPREnabled => CA_TPRProgramInd == YesNoList.Codes.Yes;

		bool IsTPRDisabled => !IsTPREnabled;

		bool IsVPREnabled => CA_VPRProgramInd == YesNoList.Codes.Yes;

		bool IsVPRDisabled => !IsVPREnabled;

		internal bool IsVPREnabledExceptPIG => IsVPREnabled
									&& (CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL
										|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
										|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
										|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
										|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
										|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR
										|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VUV
										|| CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP);

		bool IsVPRDisabledOrPIGEnabled => !IsVPREnabledExceptPIG;

		#endregion

		#region IPurgeValueParent

		bool IPurgeValueParent.IsPurging { get; set; }

		IPurgeValueHelper IPurgeValueParent.PurgeHelper => purgeHelper ?? (purgeHelper = new PurgeValueHelper<TCPGAHeader>(this));
		IPurgeValueHelper purgeHelper;

		#endregion

		#region Parent
		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (cachedInvoiceLine == null && RequirementsParent is JobComInvoiceLine line)
				{
					cachedInvoiceLine = line;
				}
				return cachedInvoiceLine;
			}
		}
		JobComInvoiceLine cachedInvoiceLine;

		public ZString ParentCountryOfOrigin => RequirementsParent?.RN_NKCountryOfOrigin ?? ZString.Empty;

		ZString ParentTariffNumber => RequirementsParent?.Tariff ?? ZString.Empty;

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

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => IsLPCOsEnabled;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry,
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
