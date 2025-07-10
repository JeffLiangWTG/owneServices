using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.CAJobComInvoiceLine)]
	public partial class JobComInvoiceLine : AutoJobComInvoiceLine
		, IUltimateDistributee
		, ISecondCustomsQuantity
		, ICusAddInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, IHaveAdditionalDataForBorderWise
		, ISynchroniserReadOnlyMembersProvider
		, ICasualImportTaxData
		, ITemplateCopyable
		, IHasPGARequirements
		, IPGARequirementSupporter
		, IDisposable
		, Integration.Customs.CA.IJobComInvoiceLine
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (IsB2AsAccountForSeededLine)
			{
				this.SetReadOnlyIncludingChildren(true);
			}

			pgaHeaders = new CusAddInfoChildrenCollection(this);
			pgaHeaders.Register<HCPGAHeader>();
			pgaHeaders.Register<PHACPGAHeader>();
			pgaHeaders.Register<NRCanPGAHeader>();
			pgaHeaders.Register<DFOPGAHeader>();
			pgaHeaders.Register<GACPGAHeader>();
			pgaHeaders.Register<CFIAPGAHeader>();
			pgaHeaders.Register<CNSCPGAHeader>();
			pgaHeaders.Register<ECCCPGAHeader>();
			pgaHeaders.Register<TCPGAHeader>();
		}

		#region Schema

		public new class Schema : AutoJobComInvoiceLine.Schema
		{
			public const string CA_ImportReasonCodeTC = "CA_ImportReasonCodeTC";
			public const string CA_ImportReasonCodeNR = "CA_ImportReasonCodeNR";
			public const string CA_ImportReasonCodeSITT = "CA_ImportReasonCodeSITT";
			public const string CA_BrandNameTC = "CA_BrandNameTC";
			public const string CA_BrandNameNR = "CA_BrandNameNR";
			public const string CA_BrandNameSITT = "CA_BrandNameSITT";
			public const string CA_TypeSizeTC = "CA_TypeSizeTC";
			public const string CA_TypeSizeNR = "CA_TypeSizeNR";
			public const string CA_TypeSizeSITT = "CA_TypeSizeSITT";
			public const string CA_ModelNR = "CA_ModelNR";
			public const string CA_ModelSITT = "CA_ModelSITT";
			public const string CA_ModelNumberNR = "CA_ModelNumberNR";
			public const string CA_ModelNumberSITT = "CA_ModelNumberSITT";
			public const string CA_OGDStatusDescription = "CA_OGDStatusDescription";
			public const string CA_SIMADumpingDesc = "CA_SIMADumpingDesc";
			public const string CA_CFIAAllProgramInd = "CA_CFIAAllProgramInd";
			public const string CA_RN_NKCountryOfSourceCFIA = "CA_RN_NKCountryOfSourceCFIA";
			public const string CA_AIRSEndUseCFIA = "CA_AIRSEndUseCFIA";
			public const string CA_AIRSExtensionCodeCFIA = "CA_AIRSExtensionCodeCFIA";
			public const string CA_DeliveryLocationCFIA = "CA_DeliveryLocationCFIA";
			public const string CA_OA_ConsigneeAddressCFIA = "CA_OA_ConsigneeAddressCFIA";
			public const string CA_RW_NKSourceStateCFIA = "CA_RW_NKSourceStateCFIA";
			public const string CA_AIRSMiscellaneousCFIA = "CA_AIRSMiscellaneousCFIA";
			public const string CA_APIProgramInd = "CA_APIProgramInd";
			public const string CA_IntendedUseCodeAPI = "CA_IntendedUseCodeAPI";
			public const string CA_CategoryAPI = "CA_CategoryAPI";
			public const string CA_BrandNameAPI = "CA_BrandNameAPI";
			public const string CA_BrandNameCPR = "CA_BrandNameCPR";
			public const string CA_BrandNameHDR = "CA_BrandNameHDR";
			public const string CA_BrandNameOCS = "CA_BrandNameOCS";
			public const string CA_BrandNameMDE = "CA_BrandNameMDE";
			public const string CA_BrandNamePES = "CA_BrandNamePES";
			public const string CA_BrandNameVET = "CA_BrandNameVET";
			public const string CA_GTINNumber = "CA_GTINNumber";
			public const string CA_GTINNumberAPI = "CA_GTINNumberAPI";
			public const string CA_GTINNumberBBC = "CA_GTINNumberBBC";
			public const string CA_GTINNumberCTO = "CA_GTINNumberCTO";
			public const string CA_GTINNumberCPR = "CA_GTINNumberCPR";
			public const string CA_GTINNumberHDR = "CA_GTINNumberHDR";
			public const string CA_GTINNumberMDE = "CA_GTINNumberMDE";
			public const string CA_GTINNumberNHP = "CA_GTINNumberNHP";
			public const string CA_GTINNumberVET = "CA_GTINNumberVET";
			public const string CA_BatchLotNumber = "CA_BatchLotNumber";
			public const string CA_BatchLotNumberAPI = "CA_BatchLotNumberAPI";
			public const string CA_BatchLotNumberCPR = "CA_BatchLotNumberCPR";
			public const string CA_BatchLotNumberHDR = "CA_BatchLotNumberHDR";
			public const string CA_BatchLotNumberOCS = "CA_BatchLotNumberOCS";
			public const string CA_BatchLotNumberMDE = "CA_BatchLotNumberMDE";
			public const string CA_BatchLotNumberNHP = "CA_BatchLotNumberNHP";
			public const string CA_BatchLotNumberPES = "CA_BatchLotNumberPES";
			public const string CA_BatchLotNumberVET = "CA_BatchLotNumberVET";
			public const string CA_ProductionDateAPI = "CA_ProductionDateAPI";
			public const string CA_ProductionDateCPR = "CA_ProductionDateCPR";
			public const string CA_ProductionDateHDR = "CA_ProductionDateHDR";
			public const string CA_ProductionDateMDE = "CA_ProductionDateMDE";
			public const string CA_ProductionDateNHP = "CA_ProductionDateNHP";
			public const string CA_ProductionDatePES = "CA_ProductionDatePES";
			public const string CA_ProductionDateVET = "CA_ProductionDateVET";
			public const string CA_TradeNameCPR = "CA_TradeNameCPR";
			public const string CA_TradeNamePES = "CA_TradeNamePES";
			public const string CA_ModelNameMDE = "CA_ModelNameMDE";
			public const string CA_ModelNameRED = "CA_ModelNameRED";
			public const string CA_ExpiryDateBBC = "CA_ExpiryDateBBC";
			public const string CA_ExpiryDateCTO = "CA_ExpiryDateCTO";
			public const string CA_BBCProgramInd = "CA_BBCProgramInd";
			public const string CA_IntendedUseCodeBBC = "CA_IntendedUseCodeBBC";
			public const string CA_CategoryBBC = "CA_CategoryBBC";
			public const string CA_CTOProgramInd = "CA_CTOProgramInd";
			public const string CA_IntendedUseCodeCTO = "CA_IntendedUseCodeCTO";
			public const string CA_CategoryCTO = "CA_CategoryCTO";
			public const string CA_CTO_LCO = "CA_CTO_LCO";
			public const string CA_CPRProgramInd = "CA_CPRProgramInd";
			public const string CA_IntendedUseCodeCPR = "CA_IntendedUseCodeCPR";
			public const string CA_CategoryCPR = "CA_CategoryCPR";
			public const string CA_ManufacturerOrgPKCPR = "CA_ManufacturerOrgPKCPR";
			public const string CA_OA_ManufacturerAddressCPR = "CA_OA_ManufacturerAddressCPR";
			public const string CA_DSEProgramInd = "CA_DSEProgramInd";
			public const string CA_IntendedUseCodeDSE = "CA_IntendedUseCodeDSE";
			public const string CA_CategoryDSE = "CA_CategoryDSE";
			public const string CA_ComplianceStatement = "CA_ComplianceStatement";
			public const string CA_HDRProgramInd = "CA_HDRProgramInd";
			public const string CA_IntendedUseCodeHDR = "CA_IntendedUseCodeHDR";
			public const string CA_CategoryHDR = "CA_CategoryHDR";
			public const string CA_OCSProgramInd = "CA_OCSProgramInd";
			public const string CA_IntendedUseCodeOCS = "CA_IntendedUseCodeOCS";
			public const string CA_CategoryOCS = "CA_CategoryOCS";
			public const string CA_ManufacturerOrgPKOCS = "CA_ManufacturerOrgPKOCS";
			public const string CA_OA_ManufacturerAddressOCS = "CA_OA_ManufacturerAddressOCS";
			public const string CA_MDEProgramInd = "CA_MDEProgramInd";
			public const string CA_IntendedUseCodeMDE = "CA_IntendedUseCodeMDE";
			public const string CA_CategoryMDE = "CA_CategoryMDE";
			public const string CA_UniqueDeviceIDNumber = "CA_UniqueDeviceIDNumber";
			public const string CA_MDE_LEX = "CA_MDE_LEX";
			public const string CA_NHPProgramInd = "CA_NHPProgramInd";
			public const string CA_IntendedUseCodeNHP = "CA_IntendedUseCodeNHP";
			public const string CA_CategoryNHP = "CA_CategoryNHP";
			public const string CA_PESProgramInd = "CA_PESProgramInd";
			public const string CA_IntendedUseCodePES = "CA_IntendedUseCodePES";
			public const string CA_CategoryPES = "CA_CategoryPES";
			public const string CA_ManufacturerOrgPKPES = "CA_ManufacturerOrgPKPES";
			public const string CA_OA_ManufacturerAddressPES = "CA_OA_ManufacturerAddressPES";
			public const string CA_CASNumber = "CA_CASNumber";
			public const string CA_DangerousGoodsDGSubsPES = "CA_DangerousGoodsDGSubsPES";
			public const string CA_PES_SPCP = "CA_PES_SPCP";
			public const string CA_PES_EPCP = "CA_PES_EPCP";
			public const string CA_REDProgramInd = "CA_REDProgramInd";
			public const string CA_CategoryRED = "CA_CategoryRED";
			public const string CA_FDANumber = "CA_FDANumber";
			public const string CA_VETProgramInd = "CA_VETProgramInd";
			public const string CA_IntendedUseCodeVET = "CA_IntendedUseCodeVET";
			public const string CA_CategoryVET = "CA_CategoryVET";
			public const string JI_CustomsValueInUSD = "JI_CustomsValueInUSD";
			public const string ConsigneePK = "ConsigneePK";
			public const string DangerousGoodsDGSubs = "DangerousGoodsDGSubs";
			public const string CommoditySequence = "CommoditySequence";
			public const string GoodsShipmentSequence = "GoodsShipmentSequence";
			public const string JI_B3LineNumber = "JI_B3LineNumber";
			public const string CA_ADD_Description = "CA_ADD_Description";
			public const string CA_ADD_ExemptCode = "CA_ADD_ExemptCode";
			public const string CA_ADD_Code = "CA_ADD_Code";
			public const string CA_ADD_Override = "CA_ADD_Override";
			public const string CA_ADD_Amount = "CA_ADD_Amount";
			public const string CA_ADD_Rate = "CA_ADD_Rate";
			public const string CA_ADD_RateType = "CA_ADD_RateType";
			public const string CA_CPT_Description = "CA_CPT_Description";
			public const string CA_CPT_ExemptCode = "CA_CPT_ExemptCode";
			public const string CA_CPT_Code = "CA_CPT_Code";
			public const string CA_CPT_Override = "CA_CPT_Override";
			public const string CA_CPT_Amount = "CA_CPT_Amount";
			public const string CA_CPT_Rate = "CA_CPT_Rate";
			public const string CA_CPT_RateType = "CA_CPT_RateType";
			public const string CA_CTA_Description = "CA_CTA_Description";
			public const string CA_CTA_ExemptCode = "CA_CTA_ExemptCode";
			public const string CA_CTA_Code = "CA_CTA_Code";
			public const string CA_CTA_Override = "CA_CTA_Override";
			public const string CA_CTA_Amount = "CA_CTA_Amount";
			public const string CA_CTA_Rate = "CA_CTA_Rate";
			public const string CA_CTA_RateType = "CA_CTA_RateType";
			public const string CA_EXCDTY_Description = "CA_EXCDTY_Description";
			public const string CA_EXCDTY_ExemptCode = "CA_EXCDTY_ExemptCode";
			public const string CA_EXCDTY_Code = "CA_EXCDTY_Code";
			public const string CA_EXCDTY_Override = "CA_EXCDTY_Override";
			public const string CA_EXCDTY_Amount = "CA_EXCDTY_Amount";
			public const string CA_EXCDTY_Rate = "CA_EXCDTY_Rate";
			public const string CA_EXCDTY_RateType = "CA_EXCDTY_RateType";
			public const string CA_CLSDTY_Description = "CA_CLSDTY_Description";
			public const string CA_CLSDTY_ExemptCode = "CA_CLSDTY_ExemptCode";
			public const string CA_CLSDTY_Code = "CA_CLSDTY_Code";
			public const string CA_CLSDTY_Override = "CA_CLSDTY_Override";
			public const string CA_CLSDTY_Amount = "CA_CLSDTY_Amount";
			public const string CA_CLSDTY_Rate = "CA_CLSDTY_Rate";
			public const string CA_CLSDTY_RateType = "CA_CLSDTY_RateType";
			public const string CA_CVD_Description = "CA_CVD_Description";
			public const string CA_CVD_ExemptCode = "CA_CVD_ExemptCode";
			public const string CA_CVD_Code = "CA_CVD_Code";
			public const string CA_CVD_Override = "CA_CVD_Override";
			public const string CA_CVD_Amount = "CA_CVD_Amount";
			public const string CA_CVD_Rate = "CA_CVD_Rate";
			public const string CA_CVD_RateType = "CA_CVD_RateType";
			public const string CA_EXS_Description = "CA_EXS_Description";
			public const string CA_EXS_ExemptCode = "CA_EXS_ExemptCode";
			public const string CA_EXS_Code = "CA_EXS_Code";
			public const string CA_EXS_Override = "CA_EXS_Override";
			public const string CA_EXS_Amount = "CA_EXS_Amount";
			public const string CA_EXS_Rate = "CA_EXS_Rate";
			public const string CA_EXS_RateType = "CA_EXS_RateType";
			public const string CA_GST_Description = "CA_GST_Description";
			public const string CA_GST_ExemptCode = "CA_GST_ExemptCode";
			public const string CA_GST_Code = "CA_GST_Code";
			public const string CA_GST_Override = "CA_GST_Override";
			public const string CA_GST_Amount = "CA_GST_Amount";
			public const string CA_GST_Rate = "CA_GST_Rate";
			public const string CA_GST_RateType = "CA_GST_RateType";
			public const string CA_SAF_Description = "CA_SAF_Description";
			public const string CA_SAF_ExemptCode = "CA_SAF_ExemptCode";
			public const string CA_SAF_Code = "CA_SAF_Code";
			public const string CA_SAF_Override = "CA_SAF_Override";
			public const string CA_SAF_Amount = "CA_SAF_Amount";
			public const string CA_SAF_Rate = "CA_SAF_Rate";
			public const string CA_SAF_RateType = "CA_SAF_RateType";
			public const string CA_SUR_Description = "CA_SUR_Description";
			public const string CA_SUR_ExemptCode = "CA_SUR_ExemptCode";
			public const string CA_SUR_Code = "CA_SUR_Code";
			public const string CA_SUR_Override = "CA_SUR_Override";
			public const string CA_SUR_Amount = "CA_SUR_Amount";
			public const string CA_SUR_Rate = "CA_SUR_Rate";
			public const string CA_SUR_RateType = "CA_SUR_RateType";
		}

		#endregion

		#region Public New Properties

		#region ADDDutyAndTax

		DutyAndTax ADDDutyAndTax => Factory.GetValue(ref cachedADDDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.ADD));

		CachedProperty<DutyAndTax> cachedADDDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|ADD|Description", Caption = "Duty & Tax ADD Description", ShortCaption = "ADD Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_ADD_Description => ADDDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_ADD_DescriptionInfo => GetZPropertyInfo(Schema.CA_ADD_Description);

		[ReadOnlyMember(nameof(CA_ADD_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|ADD|ExemptCode", Caption = "Duty & Tax ADD Exempt/Code", ShortCaption = "ADD Exempt")]
		[List(nameof(CA_ADD_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_ADD_ExemptCode
		{
			get => ADDDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (ADDDutyAndTax != null)
				{
					ADDDutyAndTax.C1_ExemptCode = value;
				}
				CA_ADD_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_ADD_ExemptCode_ReadOnly => ADDDutyAndTax == null || ADDDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_ADD_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_ADD_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_ADD_AddInfoLookups => ADDDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_ADD_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|ADD|Code", Caption = "Duty & Tax ADD Code")]
		[List(nameof(CA_ADD_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_ADD_Code
		{
			get => ADDDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (ADDDutyAndTax != null)
				{
					ADDDutyAndTax.C1_Code = value;
				}
				CA_ADD_CodeInfo.RefreshBinding();
			}
		}

		bool CA_ADD_Code_ReadOnly => ADDDutyAndTax == null || ADDDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_ADD_CodeInfo => GetZPropertyInfo(Schema.CA_ADD_Code);

		[ResourceStringData("DutiesAndTaxes|ADD|Override", Caption = "Duty & Tax ADD Override", ShortCaption = "ADD Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_ADD_Override
		{
			get => ADDDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = ADDDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
				dutyAndTax.C1_Override = value;
				CA_ADD_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ADD_OverrideInfo => GetZPropertyInfo(Schema.CA_ADD_Override);

		[ReadOnlyMember(nameof(CA_ADD_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|ADD|Amount", Caption = "Duty & Tax ADD Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_ADD_Amount
		{
			get => ADDDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (ADDDutyAndTax != null)
				{
					ADDDutyAndTax.C1_Amount = value;
				}
				CA_ADD_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ADD_AmountInfo => GetZPropertyInfo(Schema.CA_ADD_Amount);

		bool CA_ADD_NotOverridenReadOnly => ADDDutyAndTax == null || ADDDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_ADD_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|ADD|Rate", Caption = "Duty & Tax ADD Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_ADD_Rate
		{
			get => ADDDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (ADDDutyAndTax != null)
				{
					ADDDutyAndTax.C1_Rate = value;
				}
				CA_ADD_RateInfo.RefreshBinding();
			}
		}

		bool CA_ADD_SIMADutyReadOnly => ADDDutyAndTax == null || ADDDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_ADD_RateInfo => GetZPropertyInfo(Schema.CA_ADD_Rate);

		[ReadOnlyMember(nameof(CA_ADD_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|ADD|RateType", Caption = "Duty & Tax ADD Rate Type", ShortCaption = "ADD Type")]
		[List(nameof(CA_ADD_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_ADD_RateType
		{
			get => ADDDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (ADDDutyAndTax != null)
				{
					ADDDutyAndTax.C1_RateType = value;
				}
				CA_ADD_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ADD_RateTypeInfo => GetZPropertyInfo(Schema.CA_ADD_RateType);

		#endregion

		#region CPTDutyAndTax

		DutyAndTax CPTDutyAndTax => Factory.GetValue(ref cachedCPTDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.CPT));

		CachedProperty<DutyAndTax> cachedCPTDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|CPT|Description", Caption = "Duty & Tax CPT Description", ShortCaption = "CPT Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_CPT_Description => CPTDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_CPT_DescriptionInfo => GetZPropertyInfo(Schema.CA_CPT_Description);

		[ReadOnlyMember(nameof(CA_CPT_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CPT|ExemptCode", Caption = "Duty & Tax CPT Exempt/Code", ShortCaption = "CPT Exempt")]
		[List(nameof(CA_CPT_CPTInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_CPT_ExemptCode
		{
			get => CPTDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (CPTDutyAndTax != null)
				{
					CPTDutyAndTax.C1_ExemptCode = value;
				}
				CA_CPT_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_CPT_ExemptCode_ReadOnly => CPTDutyAndTax == null || CPTDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_CPT_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_CPT_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_CPT_CPTInfoLookups => CPTDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_CPT_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CPT|Code", Caption = "Duty & Tax CPT Code")]
		[List(nameof(CA_CPT_CPTInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_CPT_Code
		{
			get => CPTDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (CPTDutyAndTax != null)
				{
					CPTDutyAndTax.C1_Code = value;
				}
				CA_CPT_CodeInfo.RefreshBinding();
			}
		}

		bool CA_CPT_Code_ReadOnly => CPTDutyAndTax == null || CPTDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_CPT_CodeInfo => GetZPropertyInfo(Schema.CA_CPT_Code);

		[ResourceStringData("DutiesAndTaxes|CPT|Override", Caption = "Duty & Tax CPT Override", ShortCaption = "CPT Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_CPT_Override
		{
			get => CPTDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = CPTDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CPT);
				dutyAndTax.C1_Override = value;
				CA_CPT_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CPT_OverrideInfo => GetZPropertyInfo(Schema.CA_CPT_Override);

		[ReadOnlyMember(nameof(CA_CPT_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CPT|Amount", Caption = "Duty & Tax CPT Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_CPT_Amount
		{
			get => CPTDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (CPTDutyAndTax != null)
				{
					CPTDutyAndTax.C1_Amount = value;
				}
				CA_CPT_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CPT_AmountInfo => GetZPropertyInfo(Schema.CA_CPT_Amount);

		bool CA_CPT_NotOverridenReadOnly => CPTDutyAndTax == null || CPTDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_CPT_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CPT|Rate", Caption = "Duty & Tax CPT Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_CPT_Rate
		{
			get => CPTDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (CPTDutyAndTax != null)
				{
					CPTDutyAndTax.C1_Rate = value;
				}
				CA_CPT_RateInfo.RefreshBinding();
			}
		}

		bool CA_CPT_SIMADutyReadOnly => CPTDutyAndTax == null || CPTDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_CPT_RateInfo => GetZPropertyInfo(Schema.CA_CPT_Rate);

		[ReadOnlyMember(nameof(CA_CPT_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CPT|RateType", Caption = "Duty & Tax CPT Rate Type", ShortCaption = "CPT Type")]
		[List(nameof(CA_CPT_CPTInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_CPT_RateType
		{
			get => CPTDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (CPTDutyAndTax != null)
				{
					CPTDutyAndTax.C1_RateType = value;
				}
				CA_CPT_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CPT_RateTypeInfo => GetZPropertyInfo(Schema.CA_CPT_RateType);

		#endregion

		#region CTADutyAndTax

		DutyAndTax CTADutyAndTax => Factory.GetValue(ref cachedCTADutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.CTA));

		CachedProperty<DutyAndTax> cachedCTADutyAndTax;

		[ResourceStringData("DutiesAndTaxes|CTA|Description", Caption = "Duty & Tax CTA Description", ShortCaption = "CTA Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_CTA_Description => CTADutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_CTA_DescriptionInfo => GetZPropertyInfo(Schema.CA_CTA_Description);

		[ReadOnlyMember(nameof(CA_CTA_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CTA|ExemptCode", Caption = "Duty & Tax CTA Exempt/Code", ShortCaption = "CTA Exempt")]
		[List(nameof(CA_CTA_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_CTA_ExemptCode
		{
			get => CTADutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (CTADutyAndTax != null)
				{
					CTADutyAndTax.C1_ExemptCode = value;
				}
				CA_CTA_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_CTA_ExemptCode_ReadOnly => CTADutyAndTax == null || CTADutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_CTA_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_CTA_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_CTA_AddInfoLookups => CTADutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_CTA_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CTA|Code", Caption = "Duty & Tax CTA Code")]
		[List(nameof(CA_CTA_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_CTA_Code
		{
			get => CTADutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (CTADutyAndTax != null)
				{
					CTADutyAndTax.C1_Code = value;
				}
				CA_CTA_CodeInfo.RefreshBinding();
			}
		}

		bool CA_CTA_Code_ReadOnly => CTADutyAndTax == null || CTADutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_CTA_CodeInfo => GetZPropertyInfo(Schema.CA_CTA_Code);

		[ResourceStringData("DutiesAndTaxes|CTA|Override", Caption = "Duty & Tax CTA Override", ShortCaption = "CTA Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_CTA_Override
		{
			get => CTADutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = CTADutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
				dutyAndTax.C1_Override = value;
				CA_CTA_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CTA_OverrideInfo => GetZPropertyInfo(Schema.CA_CTA_Override);

		[ReadOnlyMember(nameof(CA_CTA_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CTA|Amount", Caption = "Duty & Tax CTA Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_CTA_Amount
		{
			get => CTADutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (CTADutyAndTax != null)
				{
					CTADutyAndTax.C1_Amount = value;
				}
				CA_CTA_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CTA_AmountInfo => GetZPropertyInfo(Schema.CA_CTA_Amount);

		bool CA_CTA_NotOverridenReadOnly => CTADutyAndTax == null || CTADutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_CTA_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CTA|Rate", Caption = "Duty & Tax CTA Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_CTA_Rate
		{
			get => CTADutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (CTADutyAndTax != null)
				{
					CTADutyAndTax.C1_Rate = value;
				}
				CA_CTA_RateInfo.RefreshBinding();
			}
		}

		bool CA_CTA_SIMADutyReadOnly => CTADutyAndTax == null || CTADutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_CTA_RateInfo => GetZPropertyInfo(Schema.CA_CTA_Rate);

		[ReadOnlyMember(nameof(CA_CTA_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CTA|RateType", Caption = "Duty & Tax CTA Rate Type", ShortCaption = "CTA Type")]
		[List(nameof(CA_CTA_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_CTA_RateType
		{
			get => CTADutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (CTADutyAndTax != null)
				{
					CTADutyAndTax.C1_RateType = value;
				}
				CA_CTA_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CTA_RateTypeInfo => GetZPropertyInfo(Schema.CA_CTA_RateType);

		#endregion

		#region EXCDTYDutyAndTax

		DutyAndTax EXCDTYDutyAndTax => Factory.GetValue(ref cachedEXCDTYDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && dat.C1_DutyType == DutyAndTaxManager.CombinedDuty.Excise));

		CachedProperty<DutyAndTax> cachedEXCDTYDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|EXCDTY|Description", Caption = "Duty & Tax Excise DTY Description", ShortCaption = "EXC DTY Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_EXCDTY_Description => EXCDTYDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_EXCDTY_DescriptionInfo => GetZPropertyInfo(Schema.CA_EXCDTY_Description);

		[ReadOnlyMember(nameof(CA_EXCDTY_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXCDTY|ExemptCode", Caption = "Duty & Tax Excise DTY Exempt/Code", ShortCaption = "EXC DTY Exempt")]
		[List(nameof(CA_EXCDTY_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_EXCDTY_ExemptCode
		{
			get => EXCDTYDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (EXCDTYDutyAndTax != null)
				{
					EXCDTYDutyAndTax.C1_ExemptCode = value;
				}
				CA_EXCDTY_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_EXCDTY_ExemptCode_ReadOnly => EXCDTYDutyAndTax == null || EXCDTYDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_EXCDTY_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_EXCDTY_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_EXCDTY_AddInfoLookups => EXCDTYDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_EXCDTY_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXCDTY|Code", Caption = "Duty & Tax Excise DTY Code")]
		[List(nameof(CA_EXCDTY_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_EXCDTY_Code
		{
			get => EXCDTYDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (EXCDTYDutyAndTax != null)
				{
					EXCDTYDutyAndTax.C1_Code = value;
				}
				CA_EXCDTY_CodeInfo.RefreshBinding();
			}
		}

		bool CA_EXCDTY_Code_ReadOnly => EXCDTYDutyAndTax == null || EXCDTYDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_EXCDTY_CodeInfo => GetZPropertyInfo(Schema.CA_EXCDTY_Code);

		[ResourceStringData("DutiesAndTaxes|EXCDTY|Override", Caption = "Duty & Tax DTY Excise Override", ShortCaption = "EXC DTY Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_EXCDTY_Override
		{
			get => EXCDTYDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = EXCDTYDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
				dutyAndTax.C1_Override = value;
				dutyAndTax.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
				CA_EXCDTY_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_EXCDTY_OverrideInfo => GetZPropertyInfo(Schema.CA_EXCDTY_Override);

		[ReadOnlyMember(nameof(CA_EXCDTY_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXCDTY|Amount", Caption = "Duty & Tax DTY Excise Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_EXCDTY_Amount
		{
			get => EXCDTYDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (EXCDTYDutyAndTax != null)
				{
					EXCDTYDutyAndTax.C1_Amount = value;
				}
				CA_EXCDTY_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_EXCDTY_AmountInfo => GetZPropertyInfo(Schema.CA_EXCDTY_Amount);

		bool CA_EXCDTY_NotOverridenReadOnly => EXCDTYDutyAndTax == null || EXCDTYDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_EXCDTY_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXCDTY|Rate", Caption = "Duty & Tax Excise DTY Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_EXCDTY_Rate
		{
			get => EXCDTYDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (EXCDTYDutyAndTax != null)
				{
					EXCDTYDutyAndTax.C1_Rate = value;
				}
				CA_EXCDTY_RateInfo.RefreshBinding();
			}
		}

		bool CA_EXCDTY_SIMADutyReadOnly => EXCDTYDutyAndTax == null || EXCDTYDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_EXCDTY_RateInfo => GetZPropertyInfo(Schema.CA_EXCDTY_Rate);

		[ReadOnlyMember(nameof(CA_EXCDTY_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXCDTY|RateType", Caption = "Duty & Tax Excise DTY Rate Type", ShortCaption = "EXC DTY Type")]
		[List(nameof(CA_EXCDTY_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_EXCDTY_RateType
		{
			get => EXCDTYDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (EXCDTYDutyAndTax != null)
				{
					EXCDTYDutyAndTax.C1_RateType = value;
				}
				CA_EXCDTY_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_EXCDTY_RateTypeInfo => GetZPropertyInfo(Schema.CA_EXCDTY_RateType);

		#endregion

		#region CLSDTYDutyAndTax

		DutyAndTax CLSDTYDutyAndTax => Factory.GetValue(ref cachedCLSDTYDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && dat.C1_DutyType == DutyAndTaxManager.CombinedDuty.Classification));

		CachedProperty<DutyAndTax> cachedCLSDTYDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|CLSDTY|Description", Caption = "Duty & Tax Classification DTY Description", ShortCaption = "CLS DTY Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_CLSDTY_Description => CLSDTYDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_CLSDTY_DescriptionInfo => GetZPropertyInfo(Schema.CA_CLSDTY_Description);

		[ReadOnlyMember(nameof(CA_CLSDTY_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CLSDTY|ExemptCode", Caption = "Duty & Tax Classification DTY Exempt/Code", ShortCaption = "CLS DTY Exempt")]
		[List(nameof(CA_CLSDTY_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_CLSDTY_ExemptCode
		{
			get => CLSDTYDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (CLSDTYDutyAndTax != null)
				{
					CLSDTYDutyAndTax.C1_ExemptCode = value;
				}
				CA_CLSDTY_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_CLSDTY_ExemptCode_ReadOnly => CLSDTYDutyAndTax == null || CLSDTYDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_CLSDTY_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_CLSDTY_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_CLSDTY_AddInfoLookups => CLSDTYDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_CLSDTY_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CLSDTY|Code", Caption = "Duty & Tax Classification DTY Code")]
		[List(nameof(CA_CLSDTY_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_CLSDTY_Code
		{
			get => CLSDTYDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (CLSDTYDutyAndTax != null)
				{
					CLSDTYDutyAndTax.C1_Code = value;
				}
				CA_CLSDTY_CodeInfo.RefreshBinding();
			}
		}

		bool CA_CLSDTY_Code_ReadOnly => CLSDTYDutyAndTax == null || CLSDTYDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_CLSDTY_CodeInfo => GetZPropertyInfo(Schema.CA_CLSDTY_Code);

		[ResourceStringData("DutiesAndTaxes|CLSDTY|Override", Caption = "Duty & Tax DTY Classification Override", ShortCaption = "CLS DTY Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_CLSDTY_Override
		{
			get => CLSDTYDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = CLSDTYDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
				dutyAndTax.C1_Override = value;
				dutyAndTax.C1_DutyType = DutyAndTaxManager.CombinedDuty.Classification;
				CA_CLSDTY_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CLSDTY_OverrideInfo => GetZPropertyInfo(Schema.CA_CLSDTY_Override);

		[ReadOnlyMember(nameof(CA_CLSDTY_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CLSDTY|Amount", Caption = "Duty & Tax DTY Classification Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_CLSDTY_Amount
		{
			get => CLSDTYDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (CLSDTYDutyAndTax != null)
				{
					CLSDTYDutyAndTax.C1_Amount = value;
				}
				CA_CLSDTY_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CLSDTY_AmountInfo => GetZPropertyInfo(Schema.CA_CLSDTY_Amount);

		bool CA_CLSDTY_NotOverridenReadOnly => CLSDTYDutyAndTax == null || CLSDTYDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_CLSDTY_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CLSDTY|Rate", Caption = "Duty & Tax Classification DTY Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_CLSDTY_Rate
		{
			get => CLSDTYDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (CLSDTYDutyAndTax != null)
				{
					CLSDTYDutyAndTax.C1_Rate = value;
				}
				CA_CLSDTY_RateInfo.RefreshBinding();
			}
		}

		bool CA_CLSDTY_SIMADutyReadOnly => CLSDTYDutyAndTax == null || CLSDTYDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_CLSDTY_RateInfo => GetZPropertyInfo(Schema.CA_CLSDTY_Rate);

		[ReadOnlyMember(nameof(CA_CLSDTY_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CLSDTY|RateType", Caption = "Duty & Tax Classification DTY Rate Type", ShortCaption = "CLS DTY Type")]
		[List(nameof(CA_CLSDTY_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_CLSDTY_RateType
		{
			get => CLSDTYDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (CLSDTYDutyAndTax != null)
				{
					CLSDTYDutyAndTax.C1_RateType = value;
				}
				CA_CLSDTY_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CLSDTY_RateTypeInfo => GetZPropertyInfo(Schema.CA_CLSDTY_RateType);

		#endregion

		#region CVDDutyAndTax

		DutyAndTax CVDDutyAndTax => Factory.GetValue(ref cachedCVDDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.CVD));

		CachedProperty<DutyAndTax> cachedCVDDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|CVD|Description", Caption = "Duty & Tax CVD Description", ShortCaption = "CVD Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_CVD_Description => CVDDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_CVD_DescriptionInfo => GetZPropertyInfo(Schema.CA_CVD_Description);

		[ReadOnlyMember(nameof(CA_CVD_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CVD|ExemptCode", Caption = "Duty & Tax CVD Exempt/Code", ShortCaption = "CVD Exempt")]
		[List(nameof(CA_CVD_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_CVD_ExemptCode
		{
			get => CVDDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (CVDDutyAndTax != null)
				{
					CVDDutyAndTax.C1_ExemptCode = value;
				}
				CA_CVD_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_CVD_ExemptCode_ReadOnly => CVDDutyAndTax == null || CVDDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_CVD_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_CVD_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_CVD_AddInfoLookups => CVDDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_CVD_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CVD|Code", Caption = "Duty & Tax CVD Code")]
		[List(nameof(CA_CVD_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_CVD_Code
		{
			get => CVDDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (CVDDutyAndTax != null)
				{
					CVDDutyAndTax.C1_Code = value;
				}
				CA_CVD_CodeInfo.RefreshBinding();
			}
		}

		bool CA_CVD_Code_ReadOnly => CVDDutyAndTax == null || CVDDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_CVD_CodeInfo => GetZPropertyInfo(Schema.CA_CVD_Code);

		[ResourceStringData("DutiesAndTaxes|CVD|Override", Caption = "Duty & Tax CVD Override", ShortCaption = "CVD Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_CVD_Override
		{
			get => CVDDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = CVDDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
				dutyAndTax.C1_Override = value;
				CA_CVD_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CVD_OverrideInfo => GetZPropertyInfo(Schema.CA_CVD_Override);

		[ReadOnlyMember(nameof(CA_CVD_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CVD|Amount", Caption = "Duty & Tax CVD Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_CVD_Amount
		{
			get => CVDDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (CVDDutyAndTax != null)
				{
					CVDDutyAndTax.C1_Amount = value;
				}
				CA_CVD_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CVD_AmountInfo => GetZPropertyInfo(Schema.CA_CVD_Amount);

		bool CA_CVD_NotOverridenReadOnly => CVDDutyAndTax == null || CVDDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_CVD_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CVD|Rate", Caption = "Duty & Tax CVD Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_CVD_Rate
		{
			get => CVDDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (CVDDutyAndTax != null)
				{
					CVDDutyAndTax.C1_Rate = value;
				}
				CA_CVD_RateInfo.RefreshBinding();
			}
		}

		bool CA_CVD_SIMADutyReadOnly => CVDDutyAndTax == null || CVDDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_CVD_RateInfo => GetZPropertyInfo(Schema.CA_CVD_Rate);

		[ReadOnlyMember(nameof(CA_CVD_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|CVD|RateType", Caption = "Duty & Tax CVD Rate Type", ShortCaption = "CVD Type")]
		[List(nameof(CA_CVD_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_CVD_RateType
		{
			get => CVDDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (CVDDutyAndTax != null)
				{
					CVDDutyAndTax.C1_RateType = value;
				}
				CA_CVD_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CVD_RateTypeInfo => GetZPropertyInfo(Schema.CA_CVD_RateType);

		#endregion

		#region EXSDutyAndTax

		DutyAndTax EXSDutyAndTax => Factory.GetValue(ref cachedEXSDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax));

		CachedProperty<DutyAndTax> cachedEXSDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|EXS|Description", Caption = "Duty & Tax EXS Description", ShortCaption = "EXS Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_EXS_Description => EXSDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_EXS_DescriptionInfo => GetZPropertyInfo(Schema.CA_EXS_Description);

		[ReadOnlyMember(nameof(CA_EXS_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXS|ExemptCode", Caption = "Duty & Tax EXS Exempt/Code", ShortCaption = "EXS Exempt")]
		[List(nameof(CA_EXS_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_EXS_ExemptCode
		{
			get => EXSDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (EXSDutyAndTax != null)
				{
					EXSDutyAndTax.C1_ExemptCode = value;
				}
				CA_EXS_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_EXS_ExemptCode_ReadOnly => EXSDutyAndTax == null || EXSDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_EXS_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_EXS_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_EXS_AddInfoLookups => EXSDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_EXS_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXS|Code", Caption = "Duty & Tax EXS Code")]
		[List(nameof(CA_EXS_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_EXS_Code
		{
			get => EXSDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (EXSDutyAndTax != null)
				{
					EXSDutyAndTax.C1_Code = value;
				}
				CA_EXS_CodeInfo.RefreshBinding();
			}
		}

		bool CA_EXS_Code_ReadOnly => EXSDutyAndTax == null || EXSDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_EXS_CodeInfo => GetZPropertyInfo(Schema.CA_EXS_Code);

		[ResourceStringData("DutiesAndTaxes|EXS|Override", Caption = "Duty & Tax EXS Override", ShortCaption = "EXS Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_EXS_Override
		{
			get => EXSDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = EXSDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
				dutyAndTax.C1_Override = value;
				CA_EXS_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_EXS_OverrideInfo => GetZPropertyInfo(Schema.CA_EXS_Override);

		[ReadOnlyMember(nameof(CA_EXS_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXS|Amount", Caption = "Duty & Tax EXS Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_EXS_Amount
		{
			get => EXSDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (EXSDutyAndTax != null)
				{
					EXSDutyAndTax.C1_Amount = value;
				}
				CA_EXS_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_EXS_AmountInfo => GetZPropertyInfo(Schema.CA_EXS_Amount);

		bool CA_EXS_NotOverridenReadOnly => EXSDutyAndTax == null || EXSDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_EXS_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXS|Rate", Caption = "Duty & Tax EXS Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_EXS_Rate
		{
			get => EXSDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (EXSDutyAndTax != null)
				{
					EXSDutyAndTax.C1_Rate = value;
				}
				CA_EXS_RateInfo.RefreshBinding();
			}
		}

		bool CA_EXS_SIMADutyReadOnly => EXSDutyAndTax == null || EXSDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_EXS_RateInfo => GetZPropertyInfo(Schema.CA_EXS_Rate);

		[ReadOnlyMember(nameof(CA_EXS_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|EXS|RateType", Caption = "Duty & Tax EXS Rate Type", ShortCaption = "EXS Type")]
		[List(nameof(CA_EXS_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_EXS_RateType
		{
			get => EXSDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (EXSDutyAndTax != null)
				{
					EXSDutyAndTax.C1_RateType = value;
				}
				CA_EXS_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_EXS_RateTypeInfo => GetZPropertyInfo(Schema.CA_EXS_RateType);

		#endregion

		#region GSTDutyAndTax

		DutyAndTax GSTDutyAndTax => Factory.GetValue(ref cachedGSTDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.GST));

		CachedProperty<DutyAndTax> cachedGSTDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|GST|Description", Caption = "Duty & Tax GST Description", ShortCaption = "GST Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_GST_Description => GSTDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_GST_DescriptionInfo => GetZPropertyInfo(Schema.CA_GST_Description);

		[ReadOnlyMember(nameof(CA_GST_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|GST|ExemptCode", Caption = "Duty & Tax GST Exempt/Code", ShortCaption = "GST Exempt")]
		[List(nameof(CA_GST_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_GST_ExemptCode
		{
			get => GSTDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (GSTDutyAndTax != null)
				{
					GSTDutyAndTax.C1_ExemptCode = value;
				}
				CA_GST_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_GST_ExemptCode_ReadOnly => GSTDutyAndTax == null || GSTDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_GST_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_GST_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_GST_AddInfoLookups => GSTDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_GST_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|GST|Code", Caption = "Duty & Tax GST Code")]
		[List(nameof(CA_GST_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_GST_Code
		{
			get => GSTDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (GSTDutyAndTax != null)
				{
					GSTDutyAndTax.C1_Code = value;
				}
				CA_GST_CodeInfo.RefreshBinding();
			}
		}

		bool CA_GST_Code_ReadOnly => GSTDutyAndTax == null || GSTDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_GST_CodeInfo => GetZPropertyInfo(Schema.CA_GST_Code);

		[ResourceStringData("DutiesAndTaxes|GST|Override", Caption = "Duty & Tax GST Override", ShortCaption = "GST Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_GST_Override
		{
			get => GSTDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = GSTDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
				dutyAndTax.C1_Override = value;
				CA_GST_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_GST_OverrideInfo => GetZPropertyInfo(Schema.CA_GST_Override);

		[ReadOnlyMember(nameof(CA_GST_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|GST|Amount", Caption = "Duty & Tax GST Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_GST_Amount
		{
			get => GSTDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (GSTDutyAndTax != null)
				{
					GSTDutyAndTax.C1_Amount = value;
				}
				CA_GST_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_GST_AmountInfo => GetZPropertyInfo(Schema.CA_GST_Amount);

		bool CA_GST_NotOverridenReadOnly => GSTDutyAndTax == null || GSTDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_GST_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|GST|Rate", Caption = "Duty & Tax GST Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_GST_Rate
		{
			get => GSTDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (GSTDutyAndTax != null)
				{
					GSTDutyAndTax.C1_Rate = value;
				}
				CA_GST_RateInfo.RefreshBinding();
			}
		}

		bool CA_GST_SIMADutyReadOnly => GSTDutyAndTax == null || GSTDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_GST_RateInfo => GetZPropertyInfo(Schema.CA_GST_Rate);

		[ReadOnlyMember(nameof(CA_GST_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|GST|RateType", Caption = "Duty & Tax GST Rate Type", ShortCaption = "GST Type")]
		[List(nameof(CA_GST_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_GST_RateType
		{
			get => GSTDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (GSTDutyAndTax != null)
				{
					GSTDutyAndTax.C1_RateType = value;
				}
				CA_GST_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_GST_RateTypeInfo => GetZPropertyInfo(Schema.CA_GST_RateType);

		#endregion

		#region SAFDutyAndTax

		DutyAndTax SAFDutyAndTax => Factory.GetValue(ref cachedSAFDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.SAF));

		CachedProperty<DutyAndTax> cachedSAFDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|SAF|Description", Caption = "Duty & Tax SAF Description", ShortCaption = "SAF Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_SAF_Description => SAFDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_SAF_DescriptionInfo => GetZPropertyInfo(Schema.CA_SAF_Description);

		[ReadOnlyMember(nameof(CA_SAF_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SAF|ExemptCode", Caption = "Duty & Tax SAF Exempt/Code", ShortCaption = "SAF Exempt")]
		[List(nameof(CA_SAF_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_SAF_ExemptCode
		{
			get => SAFDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (SAFDutyAndTax != null)
				{
					SAFDutyAndTax.C1_ExemptCode = value;
				}
				CA_SAF_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_SAF_ExemptCode_ReadOnly => SAFDutyAndTax == null || SAFDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_SAF_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_SAF_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_SAF_AddInfoLookups => SAFDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_SAF_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SAF|Code", Caption = "Duty & Tax SAF Code")]
		[List(nameof(CA_SAF_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_SAF_Code
		{
			get => SAFDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (SAFDutyAndTax != null)
				{
					SAFDutyAndTax.C1_Code = value;
				}
				CA_SAF_CodeInfo.RefreshBinding();
			}
		}

		bool CA_SAF_Code_ReadOnly => SAFDutyAndTax == null || SAFDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_SAF_CodeInfo => GetZPropertyInfo(Schema.CA_SAF_Code);

		[ResourceStringData("DutiesAndTaxes|SAF|Override", Caption = "Duty & Tax SAF Override", ShortCaption = "SAF Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_SAF_Override
		{
			get => SAFDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = SAFDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SAF);
				dutyAndTax.C1_Override = value;
				CA_SAF_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_SAF_OverrideInfo => GetZPropertyInfo(Schema.CA_SAF_Override);

		[ReadOnlyMember(nameof(CA_SAF_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SAF|Amount", Caption = "Duty & Tax SAF Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_SAF_Amount
		{
			get => SAFDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (SAFDutyAndTax != null)
				{
					SAFDutyAndTax.C1_Amount = value;
				}
				CA_SAF_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_SAF_AmountInfo => GetZPropertyInfo(Schema.CA_SAF_Amount);

		bool CA_SAF_NotOverridenReadOnly => SAFDutyAndTax == null || SAFDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_SAF_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SAF|Rate", Caption = "Duty & Tax SAF Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_SAF_Rate
		{
			get => SAFDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (SAFDutyAndTax != null)
				{
					SAFDutyAndTax.C1_Rate = value;
				}
				CA_SAF_RateInfo.RefreshBinding();
			}
		}

		bool CA_SAF_SIMADutyReadOnly => SAFDutyAndTax == null || SAFDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_SAF_RateInfo => GetZPropertyInfo(Schema.CA_SAF_Rate);

		[ReadOnlyMember(nameof(CA_SAF_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SAF|RateType", Caption = "Duty & Tax SAF Rate Type", ShortCaption = "SAF Type")]
		[List(nameof(CA_SAF_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_SAF_RateType
		{
			get => SAFDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (SAFDutyAndTax != null)
				{
					SAFDutyAndTax.C1_RateType = value;
				}
				CA_SAF_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_SAF_RateTypeInfo => GetZPropertyInfo(Schema.CA_SAF_RateType);

		#endregion

		#region SURDutyAndTax

		DutyAndTax SURDutyAndTax => Factory.GetValue(ref cachedSURDutyAndTax, () => DutiesAndTaxes.FirstOrDefault(dat => dat.C1_TaxType == DutyAndTaxTypes.Codes.SUR));

		CachedProperty<DutyAndTax> cachedSURDutyAndTax;

		[ResourceStringData("DutiesAndTaxes|SUR|Description", Caption = "Duty & Tax SUR Description", ShortCaption = "SUR Desc.")]
		[MaxLength(DutyAndTax.Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString CA_SUR_Description => SURDutyAndTax?.Description ?? ZString.Empty;

		public ZPropertyInfo CA_SUR_DescriptionInfo => GetZPropertyInfo(Schema.CA_SUR_Description);

		[ReadOnlyMember(nameof(CA_SUR_ExemptCode_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SUR|ExemptCode", Caption = "Duty & Tax SUR Exempt/Code", ShortCaption = "SUR Exempt")]
		[List(nameof(CA_SUR_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.ExemptCodes))]
		[BusinessObjectTestExclude]
		public ZString CA_SUR_ExemptCode
		{
			get => SURDutyAndTax?.C1_ExemptCode ?? ZString.Empty;
			set
			{
				if (SURDutyAndTax != null)
				{
					SURDutyAndTax.C1_ExemptCode = value;
				}
				CA_SUR_ExemptCodeInfo.RefreshBinding();
			}
		}

		bool CA_SUR_ExemptCode_ReadOnly => SURDutyAndTax == null || SURDutyAndTax.C1_ExemptCode_ReadOnly;

		public ZPropertyInfo CA_SUR_ExemptCodeInfo => GetZPropertyInfo(Schema.CA_SUR_ExemptCode);

		public CADutyAndTaxAddInfoLookups CA_SUR_AddInfoLookups => SURDutyAndTax?.AddInfoLookups;

		[ReadOnlyMember(nameof(CA_SUR_Code_ReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SUR|Code", Caption = "Duty & Tax SUR Code")]
		[List(nameof(CA_SUR_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.Rates))]
		[BusinessObjectTestExclude]
		public ZString CA_SUR_Code
		{
			get => SURDutyAndTax?.C1_Code ?? ZString.Empty;
			set
			{
				if (SURDutyAndTax != null)
				{
					SURDutyAndTax.C1_Code = value;
				}
				CA_SUR_CodeInfo.RefreshBinding();
			}
		}

		bool CA_SUR_Code_ReadOnly => SURDutyAndTax == null || SURDutyAndTax.C1_Code_ReadOnly;

		public ZPropertyInfo CA_SUR_CodeInfo => GetZPropertyInfo(Schema.CA_SUR_Code);

		[ResourceStringData("DutiesAndTaxes|SUR|Override", Caption = "Duty & Tax SUR Override", ShortCaption = "SUR Ovr.")]
		[BusinessObjectTestExclude]
		public ZBool CA_SUR_Override
		{
			get => SURDutyAndTax?.C1_Override ?? ZBool.False;
			set
			{
				var dutyAndTax = SURDutyAndTax ?? DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
				dutyAndTax.C1_Override = value;
				CA_SUR_OverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_SUR_OverrideInfo => GetZPropertyInfo(Schema.CA_SUR_Override);

		[ReadOnlyMember(nameof(CA_SUR_NotOverridenReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SUR|Amount", Caption = "Duty & Tax SUR Amount")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(2)]
		public ZDecimal CA_SUR_Amount
		{
			get => SURDutyAndTax?.C1_Amount ?? ZDecimal.Zero;
			set
			{
				if (SURDutyAndTax != null)
				{
					SURDutyAndTax.C1_Amount = value;
				}
				CA_SUR_AmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_SUR_AmountInfo => GetZPropertyInfo(Schema.CA_SUR_Amount);

		bool CA_SUR_NotOverridenReadOnly => SURDutyAndTax == null || SURDutyAndTax.NotOverridenReadOnly;

		[ReadOnlyMember(nameof(CA_SUR_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SUR|Rate", Caption = "Duty & Tax SUR Rate")]
		[BusinessObjectTestExclude]
		[DecimalPlaces(5)]
		public ZDecimal CA_SUR_Rate
		{
			get => SURDutyAndTax?.C1_Rate ?? ZDecimal.Zero;
			set
			{
				if (SURDutyAndTax != null)
				{
					SURDutyAndTax.C1_Rate = value;
				}
				CA_SUR_RateInfo.RefreshBinding();
			}
		}

		bool CA_SUR_SIMADutyReadOnly => SURDutyAndTax == null || SURDutyAndTax.SIMADutyReadOnly;

		public ZPropertyInfo CA_SUR_RateInfo => GetZPropertyInfo(Schema.CA_SUR_Rate);

		[ReadOnlyMember(nameof(CA_SUR_SIMADutyReadOnly))]
		[ResourceStringData("DutiesAndTaxes|SUR|RateType", Caption = "Duty & Tax SUR Rate Type", ShortCaption = "SUR Type")]
		[List(nameof(CA_SUR_AddInfoLookups) + "." + nameof(CADutyAndTaxAddInfoLookups.RateTypes))]
		[BusinessObjectTestExclude]
		public ZString CA_SUR_RateType
		{
			get => SURDutyAndTax?.C1_RateType ?? ZString.Empty;
			set
			{
				if (SURDutyAndTax != null)
				{
					SURDutyAndTax.C1_RateType = value;
				}
				CA_SUR_RateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_SUR_RateTypeInfo => GetZPropertyInfo(Schema.CA_SUR_RateType);

		#endregion

		#region CA_ImportReasonCodeTC

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ImportReasonCodes))]
		[MaxLength(AutoCAAddInfo.Schema.CA_ImportReasonCodeMaxLength)]
		public ZString CA_ImportReasonCodeTC
		{
			get { return CA_ImportReasonCode; }
			set
			{
				CA_ImportReasonCode = value;
				CA_ImportReasonCodeTCInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_ImportReasonCodeTCInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ImportReasonCodeTC); }
		}

		#endregion

		#region CA_ImportReasonCodeNR

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ImportReasonCodes))]
		[MaxLength(AutoCAAddInfo.Schema.CA_ImportReasonCodeMaxLength)]
		public ZString CA_ImportReasonCodeNR
		{
			get { return CA_ImportReasonCode; }
			set
			{
				CA_ImportReasonCode = value;
				CA_ImportReasonCodeNRInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_ImportReasonCodeNRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ImportReasonCodeNR); }
		}

		#endregion

		#region CA_ImportReasonCodeSITT

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ImportReasonCodes))]
		[MaxLength(AutoCAAddInfo.Schema.CA_ImportReasonCodeMaxLength)]
		public ZString CA_ImportReasonCodeSITT
		{
			get { return CA_ImportReasonCode; }
			set
			{
				CA_ImportReasonCode = value;
				CA_ImportReasonCodeSITTInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_ImportReasonCodeSITTInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ImportReasonCodeSITT); }
		}

		#endregion

		#region CA_BrandNameTC

		[MaxLength(Schema.JI_BrandNameMaxLength)]
		public ZString CA_BrandNameTC
		{
			get { return JI_BrandName; }
			set
			{
				JI_BrandName = value;
				CA_BrandNameTCInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_BrandNameTCInfo
		{
			get { return GetZPropertyInfo(Schema.CA_BrandNameTC); }
		}

		#endregion

		#region CA_BrandNameNR

		[MaxLength(Schema.JI_BrandNameMaxLength)]
		public ZString CA_BrandNameNR
		{
			get { return JI_BrandName; }
			set
			{
				JI_BrandName = value;
				CA_BrandNameNRInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_BrandNameNRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_BrandNameNR); }
		}

		#endregion

		#region CA_BrandNameSITT

		[MaxLength(Schema.JI_BrandNameMaxLength)]
		public ZString CA_BrandNameSITT
		{
			get { return JI_BrandName; }
			set
			{
				JI_BrandName = value;
				CA_BrandNameSITTInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_BrandNameSITTInfo
		{
			get { return GetZPropertyInfo(Schema.CA_BrandNameSITT); }
		}

		#endregion

		#region CA_TypeSizeTC

		[MaxLength(AutoCAAddInfo.Schema.CA_TypeSizeMaxLength)]
		public ZString CA_TypeSizeTC
		{
			get { return CA_TypeSize; }
			set
			{
				CA_TypeSize = value;
				CA_TypeSizeTCInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_TypeSizeTCInfo
		{
			get { return GetZPropertyInfo(Schema.CA_TypeSizeTC); }
		}

		#endregion

		#region CA_TypeSizeNR

		[MaxLength(AutoCAAddInfo.Schema.CA_TypeSizeMaxLength)]
		public ZString CA_TypeSizeNR
		{
			get { return CA_TypeSize; }
			set
			{
				CA_TypeSize = value;
				CA_TypeSizeNRInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_TypeSizeNRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_TypeSizeNR); }
		}

		#endregion

		#region CA_ModelNR

		[MaxLength(AutoCAAddInfo.Schema.CA_ModelMaxLength)]
		public ZString CA_ModelNR
		{
			get { return CA_Model; }
			set
			{
				CA_Model = value;
				CA_ModelNRInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_ModelNRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ModelNR); }
		}

		#endregion

		#region CA_ModelSITT

		[MaxLength(AutoCAAddInfo.Schema.CA_ModelMaxLength)]
		public ZString CA_ModelSITT
		{
			get { return CA_Model; }
			set
			{
				CA_Model = value;
				CA_ModelSITTInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_ModelSITTInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ModelSITT); }
		}

		#endregion

		#region CA_ModelNumberNR

		[MaxLength(AutoCAAddInfo.Schema.CA_ModelNumberMaxLength)]
		public ZString CA_ModelNumberNR
		{
			get { return CA_ModelNumber; }
			set
			{
				CA_ModelNumber = value;
				CA_ModelNumberNRInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_ModelNumberNRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ModelNumberNR); }
		}

		#endregion

		#region CA_ModelSITT

		[MaxLength(AutoCAAddInfo.Schema.CA_ModelNumberMaxLength)]
		public ZString CA_ModelNumberSITT
		{
			get { return CA_ModelNumber; }
			set
			{
				CA_ModelNumber = value;
				CA_ModelNumberSITTInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CA_ModelNumberSITTInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ModelNumberSITT); }
		}

		#endregion

		#region CA_OGDStatusDescription

		public ZString CA_OGDStatusDescription
		{
			get { return AddInfoLookups.OGDStatusCodes.GetDescriptionFromCode(CA_OGDStatus); }
		}

		public ZPropertyInfo CA_OGDStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CA_OGDStatusDescription); }
		}

		#endregion

		#region JI_CustomsValueInUSD

		public ZDecimal JI_CustomsValueInUSD
		{
			get
			{
				if (cachedCustomsValueInUsd == null)
				{
					cachedCustomsValueInUsd = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var currency = IsImport ? RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Canada) : LocalCurrency;
						var destinationCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);

						return currency != null && destinationCurrency != null
							? CurrencyConverter.ConvertRounded(new Money(JI_CustomsValue, currency), destinationCurrency).Amount
							: ZDecimal.Zero;
					});
				}

				return cachedCustomsValueInUsd.Value;
			}
		}
		CachedProperty<ZDecimal> cachedCustomsValueInUsd;

		public ZPropertyInfo JI_CustomsValueInUSDInfo => GetZPropertyInfo(Schema.JI_CustomsValueInUSD);

		#endregion

		public bool IsGSTDirectPayment
		{
			get
			{
				if (Declaration.IsLVSTotalConsolidation && InvoiceHeader?.Buyer is OrgHeader buyer)
				{
					return Declaration.IsGSTDirectPaymentCore(OrgImpAddInfo.Get(buyer));
				}
				return Declaration.IsGSTDirectPayment;
			}
		}

		public ZDecimal JI_Calc_ExciseTaxesAmount
		{
			get
			{
				if (totalExciseTaxesAmount == null)
				{
					totalExciseTaxesAmount = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return (!IsImport && !IsB2OrB3XAdjustments) ? ZDecimal.Zero : DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.ExciseTax);
					});
				}
				return totalExciseTaxesAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalExciseTaxesAmount;

		public ZDecimal JI_Calc_NormalDutyAmount
		{
			get
			{
				if (totalNormalDutyAmount == null)
				{
					totalNormalDutyAmount = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return (!IsImport && !IsB2OrB3XAdjustments) ? ZDecimal.Zero : DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty);
					});
				}
				return totalNormalDutyAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalNormalDutyAmount;

		public ZDecimal JI_Calc_SIMADutyAmount
		{
			get
			{
				if (totalSIMADutyAmount == null)
				{
					totalSIMADutyAmount = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return (!IsImport && !IsB2OrB3XAdjustments) ? ZDecimal.Zero : DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty);
					});
				}
				return totalSIMADutyAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalSIMADutyAmount;

		public ZDecimal JI_Calc_GSTAmount
		{
			get
			{
				if (totalGSTAmount == null)
				{
					totalGSTAmount = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return (!IsImport && !IsB2OrB3XAdjustments) ? ZDecimal.Zero : DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST);
					});
				}
				return totalGSTAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalGSTAmount;

		public ZDecimal JI_Calc_PayableSIMADutyAmount
		{
			get
			{
				if (totalPayableSIMADutyAmount == null)
				{
					totalPayableSIMADutyAmount = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return IDutyAndTaxDataExtensions.IsSimaAmountPayable(DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SIMADuty)) ? JI_Calc_SIMADutyAmount : ZDecimal.Zero;
					});
				}
				return totalPayableSIMADutyAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalPayableSIMADutyAmount;

		public bool IsImportIncludingB2
		{
			get { return Declaration?.IsImportIncludingB2 ?? false; }
		}

		public ZString EffectiveProvinceOfOrigin
		{
			get { return (JI_StateOrRegionOfOrigin.IsEmpty && InvoiceHeader != null) ? InvoiceHeader.JZ_RW_NKOriginState : JI_StateOrRegionOfOrigin; }
		}

		public ZString EffectiveValueForDutyCode
		{
			get { return (CA_ValueForDutyCode.IsEmpty && InvoiceHeader != null) ? InvoiceHeader.CA_ValueForDutyCode : CA_ValueForDutyCode; }
		}

		public ZBool IsEffectiveCasualImport
		{
			get { return CA_IsCasualImport || InvoiceHeader != null && InvoiceHeader.CA_IsCasualImport; }
		}

		public ZDate EffectiveDate => EffectiveDateForDutyRate;

		public ZBool IsExempt
		{
			get { return CA_IsExempt; }
		}

		#region CusEntryLine

		public new CusEntryLine CusEntryLine
		{
			get
			{
				if (fCusEntryLine == null || fCusEntryLine.IsDeleted || fCusEntryLine.PK != JI_CL)
				{
					fCusEntryLine = (CusEntryLine)Factory.Load(CusEntryLineType, JI_CL);
				}
				return fCusEntryLine;
			}
		}

		CusEntryLine fCusEntryLine;

		#endregion

		public ZString EffectiveCasualImportCommodity
		{
			get
			{
				return CA_CasualImportCommodity;
			}
		}

		[ResourceStringData("CAAddInfo|CA_ValueForTax", Caption = "Value for Tax", ShortCaption = "Tax", MediumCaption = "Tax. Value", FullDescription = "The value in the currency of the invoice that will be converted to CAD for tax calculation.")]
		[ReadOnly(true)]
		public override ZDecimal CA_ValueForTax { get => base.CA_ValueForTax; set => base.CA_ValueForTax = value; }

		public ZString EffectiveCasualImportDestinationProvince
		{
			get
			{
				return CA_CasualImportDestinationProvince;
			}
		}

		public ZString EffectiveCasualImportClearanceProvince
		{
			get { return InvoiceHeader?.EffectiveImportClearanceProvince ?? ZString.Empty; }
		}

		public bool IsDataLoadingModule
		{
			get { return new CachedProperty<bool>(Factory, () => Declaration?.IsDataLoadingModule ?? false).Value; }
		}

		public
#if DEBUG
			virtual // for mock
#endif
			CusEntryLine B3EntryLine
		{
			get { return (Declaration?.IsLVX ?? ZBool.False) ? CusEntryLine : (AdditionalEntryLineLinks.Count > 0 ? (CusEntryLine)AdditionalEntryLineLinks[0].EntryLine : null); }
		}

		public ZString JI_B3LineNumber
		{
			get
			{
				if (Declaration?.IsIM2 ?? ZBool.False)
				{
					return B3EntryLine?.CA_B2LineNo ?? ZString.Empty;
				}
				else
				{
					return B3EntryLine?.CL_LineNumber.ToString() ?? ZString.Empty;
				}
			}
		}

		public ZString LVXB3LineNumber
		{
			get
			{
				return (Declaration?.IsLVX ?? ZBool.False) ? (CusEntryLine?.CL_LineNumber.ToString() ?? string.Empty) : string.Empty;
			}
		}

		#region CA_ApplyLuxuryTax

		[ReadOnlyMember(nameof(IsLuxuryTaxInvoiceLine))]
		public override ZBool CA_ApplyLuxuryTax
		{
			get
			{
				return base.CA_ApplyLuxuryTax;
			}
			set
			{
				var oldValue = CA_ApplyLuxuryTax;
				base.CA_ApplyLuxuryTax = value;
				if (!IsCopying && oldValue != CA_ApplyLuxuryTax)
				{
					if (value)
					{
						PopulateLuxuryTaxInvoiceLine();
					}
					else if (ShouldDeleteLuxuryTaxInvoiceLine != null && ShouldDeleteLuxuryTaxInvoiceLine())
					{
						DeleteLuxuryTaxInvoiceLine();
					}
					else
					{
						base.CA_ApplyLuxuryTax = oldValue;
					}
				}
			}
		}

		void PopulateLuxuryTaxInvoiceLine()
		{
			var invHeader = InvoiceHeader;
			var line = LuxuryTaxInvoiceLine;
			if (invHeader != null && line == null)
			{
				line = invHeader.JobComInvoiceLines.AddNew();

				if (line.EffectiveTreatmentCode.IsEmpty)
				{
					line.CA_TreatmentCode = this.CA_TreatmentCode;
				}

				if (line.EffectiveCountryOfOrigin.IsEmpty)
				{
					line.JI_CountryOfOrigin = this.JI_CountryOfOrigin;
				}

				if (line.EffectiveProvinceOfOrigin.IsEmpty)
				{
					line.JI_StateOrRegionOfOrigin = this.JI_StateOrRegionOfOrigin;
				}

				line.JI_ParentID = this.PK;
				line.JI_ParentTableCode = this.TablePrefix;
				line.JI_Tariff = LuxuryTaxTariffCode;
				line.JI_Description = "LUXURY TAX";
				line.ReadOnly = true;
				line.LuxuryTaxInvoiceLineSynchroniser.Synchronise(true);

				var exsTax = line.dutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
				exsTax.C1_Override = true;
			}
		}

		void DeleteLuxuryTaxInvoiceLine()
		{
			var line = LuxuryTaxInvoiceLine;
			if (line != null)
			{
				InvoiceHeader.JobComInvoiceLines.RemoveAndDelete(line);
			}
		}

		LuxuryTaxInvoiceLineSynchroniser LuxuryTaxInvoiceLineSynchroniser
		{
			get
			{
				if (IsLuxuryTaxInvoiceLine && luxuryTaxInvoiceLineSynchroniser == null)
				{
					luxuryTaxInvoiceLineSynchroniser = new LuxuryTaxInvoiceLineSynchroniser(this, (JobComInvoiceLine)ParentTariffLine);
				}
				return luxuryTaxInvoiceLineSynchroniser;
			}
		}
		LuxuryTaxInvoiceLineSynchroniser luxuryTaxInvoiceLineSynchroniser;

		public Func<bool> ShouldDeleteLuxuryTaxInvoiceLine;

		public JobComInvoiceLine LuxuryTaxInvoiceLine
		{
			get { return InvoiceHeader?.JobComInvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_ParentID == this.PK && x.JI_Tariff == LuxuryTaxTariffCode); }
		}

		public ZBool IsLuxuryTaxInvoiceLine
		{
			get { return !JI_ParentID.IsEmpty && JI_Tariff == LuxuryTaxTariffCode; }
		}

		public const string LuxuryTaxTariffCode = "0000999969";

		#endregion

		[ReadOnly(true)]
		public override ZInt CA_PreviousB3LineNo
		{
			get { return base.CA_PreviousB3LineNo; }
			set { base.CA_PreviousB3LineNo = value; }
		}

		[ReadOnly(true)]
		public override ZInt CA_PreviousB3SubHeaderNo
		{
			get { return base.CA_PreviousB3SubHeaderNo; }
			set { base.CA_PreviousB3SubHeaderNo = value; }
		}

		ZBool CA_IsCasualImport_ReadOnly
		{
			get { return InvoiceHeader?.CA_IsCasualImport ?? ZBool.True; }
		}

		ZBool CA_IsCasualImportValues_ReadOnly
		{
			get { return !CA_IsCasualImport; }
		}

		ZBool CA_PageRelativeLineNumber_ReadOnly
		{
			get { return !Declaration.IsOtherWarehouseEntry; }
		}

		ZBool CA_AuthorityNumber_ReadOnly
		{
			get
			{
				bool result;
				if (InvoiceHeader == null)
				{
					result = true;
				}
				else
				{
					var declaration = InvoiceHeader.FirstAdditionalOrOnlyDeclaration;
					result = declaration != null && declaration.IsConsolidatedLVS && !declaration.CA_AllowOIC;
				}
				return result;
			}
		}

		public ZString Origin
		{
			get { return JI_CountryOfOrigin == Core.Constants.CountryCodes.UnitedStates ? new ZString("U" + JI_StateOrRegionOfOrigin) : JI_CountryOfOrigin; }
		}

		#region CA_SIMADumpingDesc

		[ResourceStringData("CAAddInfo|CA_SIMADumpingDesc", Caption = "SIMA Measure")]
		public ZString CA_SIMADumpingDesc
		{
			get
			{
				return DutyAndTaxManager.GetSIMADumpingDescription(Factory, JI_Tariff, CA_SIMADumpingNum, EffectiveDateForDutyRate);
			}
		}

		public virtual ZPropertyInfo CA_SIMADumpingDescInfo
		{
			get { return GetZPropertyInfo(Schema.CA_SIMADumpingDesc); }
		}

		#endregion

		#region PGAReadOnlyForNonIID

		bool IsIIDDeclaration => Declaration?.IsIID ?? ZBool.False;

		bool PGAReadOnlyForNonIID => !IsIIDDeclaration;

		#endregion

		#region CA_CFIA_ReadOnly

		bool CA_CFIA_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_CFIAAllProgramInd; }
		}

		#endregion

		PGAProgramRequirement GetPGAProgramRequirement(ZString agencyCode, ZString programCode)
		{
			if (!IsIIDDeclaration)
			{
				return null;
			}
			var requirments = PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == agencyCode);
			return requirments?.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == programCode);
		}

		#region CA_CFIAAllProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_CFIAAllProgramInd
		{
			get
			{
				var allProgram = GetPGAProgramRequirement(PGACodes.Codes.CFIA, CFIAPGADepartmentCodes.Codes.ALL);
				return allProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = CFIAPGAHeader.CA_AllProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var allProgram = GetPGAProgramRequirement(PGACodes.Codes.CFIA, CFIAPGADepartmentCodes.Codes.ALL);
					if (allProgram != null)
					{
						allProgram.DeclareYes = value;
					}
				}
				CA_CFIAAllProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CFIAAllProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CFIAAllProgramInd); }
		}

		#endregion

		#region CA_RN_NKCountryOfSourceCFIA

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.DefaultOrigins))]
		[MaxLength(AutoCAAddInfo.Schema.CA_CFIACountryOfSourceMaxLength)]
		[ReadOnlyMember(nameof(CA_CFIA_ReadOnly))]
		public ZString CA_RN_NKCountryOfSourceCFIA
		{
			get { return CA_RN_NKSource; }
			set
			{
				CA_RN_NKSource = value;
				CA_RN_NKCountryOfSourceCFIAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_RN_NKCountryOfSourceCFIAInfo
		{
			get { return GetZPropertyInfo(Schema.CA_RN_NKCountryOfSourceCFIA); }
		}

		#endregion

		#region CA_AIRSEndUseCFIA

		[List(nameof(CFIAAddInfoLookups) + "." + nameof(CFIAPGAHeaderAddInfoLookups.EndUseCodes))]
		[MaxLength(AutoCFIAPGAHeaderAddInfo.Schema.CA_AIRSEndUseMaxLength)]
		[ReadOnlyMember(nameof(CA_CFIA_ReadOnly))]
		public ZString CA_AIRSEndUseCFIA
		{
			get { return CFIAPGAHeader.CA_AIRSEndUse; }
			set
			{
				CFIAPGAHeader.CA_AIRSEndUse = value;
				CA_AIRSEndUseCFIAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_AIRSEndUseCFIAInfo
		{
			get { return GetZPropertyInfo(Schema.CA_AIRSEndUseCFIA); }
		}

		#endregion

		#region CA_AIRSExtensionCodeCFIA

		[MaxLength(AutoCFIAPGAHeaderAddInfo.Schema.CA_AIRSExtensionCodeMaxLength)]
		[ReadOnlyMember(nameof(CA_CFIA_ReadOnly))]
		public ZString CA_AIRSExtensionCodeCFIA
		{
			get { return CFIAPGAHeader.CA_AIRSExtensionCode; }
			set
			{
				CFIAPGAHeader.CA_AIRSExtensionCode = value;
				CA_AIRSExtensionCodeCFIAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_AIRSExtensionCodeCFIAInfo
		{
			get { return GetZPropertyInfo(Schema.CA_AIRSExtensionCodeCFIA); }
		}

		#endregion

		#region CA_DeliveryLocationCFIA

		[RelatedBusinessObject("Consignee")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Consignees))]
		[ReadOnlyMember(nameof(CA_CFIA_ReadOnly))]
		public ZGuid CA_DeliveryLocationCFIA
		{
			get { return JI_OA_ConsigneeAddress_ZAddress.OrgPK; }
			set { JI_OA_ConsigneeAddress_ZAddress.OrgPK = value; }
		}
		#endregion

		#region CA_OA_ConsigneeAddressCFIA

		[List(nameof(JI_OA_ConsigneeAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ReadOnlyMember(nameof(CA_CFIA_ReadOnly))]
		public ZGuid CA_OA_ConsigneeAddressCFIA
		{
			get { return JI_OA_ConsigneeAddress; }
			set
			{
				JI_OA_ConsigneeAddress = value;
				CA_OA_ConsigneeAddressCFIAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_OA_ConsigneeAddressCFIAInfo
		{
			get => GetZPropertyInfo(Schema.CA_OA_ConsigneeAddressCFIA);
		}

		#endregion

		#region CA_RW_NKSourceStateCFIA_ReadOnly

		bool CA_RW_NKSourceStateCFIA_ReadOnly
		{
			get { return CA_CFIA_ReadOnly || CA_StateOfSource_ReadOnly; }
		}

		#endregion

		#region CA_RW_NKSourceStateCFIA

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.StatesOfExport))]
		[MaxLength(2)]
		[ReadOnlyMember(nameof(CA_RW_NKSourceStateCFIA_ReadOnly))]
		public ZString CA_RW_NKSourceStateCFIA
		{
			get { return CA_StateOfSource; }
			set
			{
				CA_StateOfSource = value;
				CA_RW_NKSourceStateCFIAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_RW_NKSourceStateCFIAInfo
		{
			get { return GetZPropertyInfo(Schema.CA_RW_NKSourceStateCFIA); }
		}
		#endregion

		#region CA_AIRSMiscellaneousCFIA

		[List(nameof(CFIAAddInfoLookups) + "." + nameof(CFIAPGAHeaderAddInfoLookups.AirsMiscellaneous))]
		[MaxLength(AutoCFIAPGAHeaderAddInfo.Schema.CA_AIRSMiscellaneousMaxLength)]
		[ReadOnlyMember(nameof(CA_CFIA_ReadOnly))]
		public ZString CA_AIRSMiscellaneousCFIA
		{
			get { return CFIAPGAHeader.CA_AIRSMiscellaneous; }
			set
			{
				CFIAPGAHeader.CA_AIRSMiscellaneous = value;
				CA_AIRSMiscellaneousCFIAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_AIRSMiscellaneousCFIAInfo
		{
			get { return GetZPropertyInfo(Schema.CA_AIRSMiscellaneousCFIA); }
		}

		#endregion

		#region CA_HC_API_ReadOnly

		bool CA_HC_API_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_APIProgramInd; }
		}

		#endregion

		#region CA_APIProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_APIProgramInd
		{
			get
			{
				var apiProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.API);
				return apiProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_APIProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var apiProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.API);
					if (apiProgram != null)
					{
						apiProgram.DeclareYes = value;
					}
				}
				CA_APIProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_APIProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_APIProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeAPI

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesAPI))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeAPIMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_API_ReadOnly))]
		public ZString CA_IntendedUseCodeAPI
		{
			get { return HCPGAHeader.CA_IntendedUseCodeAPI; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeAPI = value;
				CA_IntendedUseCodeAPIInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeAPIInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeAPI); }
		}

		#endregion

		#region CA_CategoryAPI

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesAPI))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryAPIMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_API_ReadOnly))]
		public ZString CA_CategoryAPI
		{
			get { return HCPGAHeader.CA_CategoryAPI; }
			set
			{
				HCPGAHeader.CA_CategoryAPI = value;
				CA_CategoryAPIInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryAPIInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryAPI); }
		}

		#endregion

		#region CA_GTINNumber_ReadOnly

		bool CA_GTINNumber_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || (!CA_APIProgramInd && !CA_BBCProgramInd && !CA_CTOProgramInd && !CA_CPRProgramInd && !CA_HDRProgramInd && !CA_MDEProgramInd && !CA_NHPProgramInd && !CA_VETProgramInd); }
		}

		#endregion

		#region CA_GTINNumber

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumber
		{
			get { return HCPGAHeader.CA_GTINNumber; }
			set
			{
				HCPGAHeader.CA_GTINNumber = value;
				CA_GTINNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_GTINNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CA_GTINNumber); }
		}

		#endregion

		#region CA_GTINNumberAPI

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberAPI
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_ProductionDate_ReadOnly

		bool CA_ProductionDate_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || (!CA_APIProgramInd && !CA_CPRProgramInd && !CA_HDRProgramInd && !CA_MDEProgramInd && !CA_NHPProgramInd && !CA_PESProgramInd && !CA_VETProgramInd); }
		}

		#endregion

		#region CA_ProductionDate

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public override ZDateTime CA_ProductionDate
		{
			get { return base.CA_ProductionDate; }
			set
			{
				base.CA_ProductionDate = value;
				RefreshBindingForDeclaredPGAHeaders();
			}
		}

		#endregion

		#region CA_ProductionDateAPI

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public ZDateTime CA_ProductionDateAPI
		{
			get { return CA_ProductionDate; }
			set
			{
				CA_ProductionDate = value;
				CA_ProductionDateAPIInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ProductionDateAPIInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ProductionDateAPI); }
		}

		#endregion

		#region CA_BatchLotNumber_ReadOnly

		bool CA_BatchLotNumber_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || (!CA_APIProgramInd && !CA_CPRProgramInd && !CA_HDRProgramInd && !CA_OCSProgramInd && !CA_MDEProgramInd && !CA_NHPProgramInd && !CA_PESProgramInd && !CA_VETProgramInd); }
		}

		#endregion

		#region CA_BatchLotNumber

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumber
		{
			get { return HCPGAHeader.CA_BatchLotNumber; }
			set
			{
				HCPGAHeader.CA_BatchLotNumber = value;
				CA_BatchLotNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_BatchLotNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CA_BatchLotNumber); }
		}

		#endregion

		#region CA_BrandName_ReadOnly

		bool CA_BrandName_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || (!CA_APIProgramInd && !CA_CPRProgramInd && !CA_HDRProgramInd && !CA_OCSProgramInd && !CA_MDEProgramInd && !CA_PESProgramInd && !CA_VETProgramInd); }
		}

		#endregion

		#region CA_BrandNameAPI

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameAPI
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameAPIInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameAPI), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberAPI

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberAPI
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		#region CA_HC_BBC_ReadOnly

		bool CA_HC_BBC_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_BBCProgramInd; }
		}

		#endregion

		#region CA_BBCProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_BBCProgramInd
		{
			get
			{
				var bbcProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.BBC);
				return bbcProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_BBCProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var bbcProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.BBC);
					if (bbcProgram != null)
					{
						bbcProgram.DeclareYes = value;
					}
				}
				CA_BBCProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_BBCProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_BBCProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeBBC

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesBBC))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeBBCMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_BBC_ReadOnly))]
		public ZString CA_IntendedUseCodeBBC
		{
			get { return HCPGAHeader.CA_IntendedUseCodeBBC; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeBBC = value;
				CA_IntendedUseCodeBBCInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeBBCInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeBBC); }
		}

		#endregion

		#region CA_CategoryBBC

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesBBC))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryBBCMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_BBC_ReadOnly))]
		public ZString CA_CategoryBBC
		{
			get { return HCPGAHeader.CA_CategoryBBC; }
			set
			{
				HCPGAHeader.CA_CategoryBBC = value;
				CA_CategoryBBCInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryBBCInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryBBC); }
		}

		#endregion

		#region CA_GTINNumberBBC

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberBBC
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_BrandNameBBC

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameBBC
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameBBCInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameBBC), x => JI_BrandNameInfo);

		#endregion

		#region CA_ExpiryDate_ReadOnly

		bool CA_ExpiryDate_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || (!CA_BBCProgramInd && !CA_CTOProgramInd); }
		}

		#endregion

		#region CA_ExpiryDate

		[ReadOnlyMember(nameof(CA_ExpiryDate_ReadOnly))]
		public override ZDateTime CA_ExpiryDate
		{
			get { return base.CA_ExpiryDate; }
			set
			{
				base.CA_ExpiryDate = value;
				RefreshBindingForDeclaredPGAHeaders();
			}
		}

		#endregion

		#region CA_ExpiryDateBBC

		[ReadOnlyMember(nameof(CA_ExpiryDate_ReadOnly))]
		public ZDateTime CA_ExpiryDateBBC
		{
			get { return CA_ExpiryDate; }
			set
			{
				CA_ExpiryDate = value;
				CA_ExpiryDateBBCInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ExpiryDateBBCInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ExpiryDateBBC); }
		}

		#endregion

		#region CA_HC_CTO_ReadOnly

		bool CA_HC_CTO_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_CTOProgramInd; }
		}

		#endregion

		#region CA_CTOProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_CTOProgramInd
		{
			get
			{
				var ctoProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.CTO);
				return ctoProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_CTOProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var ctoProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.CTO);
					if (ctoProgram != null)
					{
						ctoProgram.DeclareYes = value;
					}
				}
				CA_CTOProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CTOProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CTOProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeCTO

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesCTO))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeCTOMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_CTO_ReadOnly))]
		public ZString CA_IntendedUseCodeCTO
		{
			get { return HCPGAHeader.CA_IntendedUseCodeCTO; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeCTO = value;
				CA_IntendedUseCodeCTOInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeCTOInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeCTO); }
		}

		#endregion

		#region CA_CategoryCTO

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesCTO))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryCTOMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_CTO_ReadOnly))]
		public ZString CA_CategoryCTO
		{
			get { return HCPGAHeader.CA_CategoryCTO; }
			set
			{
				HCPGAHeader.CA_CategoryCTO = value;
				CA_CategoryCTOInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryCTOInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryCTO); }
		}

		#endregion

		#region CA_ExpiryDateCTO

		[ReadOnlyMember(nameof(CA_ExpiryDate_ReadOnly))]
		public ZDateTime CA_ExpiryDateCTO
		{
			get { return CA_ExpiryDate; }
			set
			{
				CA_ExpiryDate = value;
				CA_ExpiryDateCTOInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ExpiryDateCTOInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ExpiryDateCTO); }
		}

		#endregion

		#region CA_GTINNumberCTO

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberCTO
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_CTO_LCO

		[ReadOnlyMember(nameof(CA_HC_CTO_ReadOnly))]
		public ZBool CA_CTO_LCO
		{
			get { return HCPGAHeader.CA_CTO_LCO; }
			set
			{
				HCPGAHeader.CA_CTO_LCO = value;
				CA_CTO_LCOInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CTO_LCOInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CTO_LCO); }
		}

		#endregion

		#region CA_HC_CPR_ReadOnly

		bool CA_HC_CPR_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_CPRProgramInd; }
		}

		#endregion

		#region CA_CPRProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_CPRProgramInd
		{
			get
			{
				var cprProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.CPR);
				return cprProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_CPRProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var cprProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.CPR);
					if (cprProgram != null)
					{
						cprProgram.DeclareYes = value;
					}
				}
				CA_CPRProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CPRProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CPRProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeCPR

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesCPR))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeCPRMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_CPR_ReadOnly))]
		public ZString CA_IntendedUseCodeCPR
		{
			get { return HCPGAHeader.CA_IntendedUseCodeCPR; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeCPR = value;
				CA_IntendedUseCodeCPRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeCPRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeCPR); }
		}

		#endregion

		#region CA_CategoryCPR

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesCPR))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryCPRMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_CPR_ReadOnly))]
		public ZString CA_CategoryCPR
		{
			get { return HCPGAHeader.CA_CategoryCPR; }
			set
			{
				HCPGAHeader.CA_CategoryCPR = value;
				CA_CategoryCPRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryCPRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryCPR); }
		}

		#endregion

		#region CA_Manufacturer_ReadOnly

		bool CA_Manufacturer_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || (!CA_CPRProgramInd && !CA_OCSProgramInd && !CA_PESProgramInd); }
		}

		#endregion

		#region CA_ManufacturerOrgPKCPR

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupplierList))]
		[ReadOnlyMember(nameof(CA_Manufacturer_ReadOnly))]
		public ZGuid CA_ManufacturerOrgPKCPR
		{
			get { return JI_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { JI_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		#endregion

		#region CA_OA_ManufacturerAddressCPR

		[List(nameof(JI_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ReadOnlyMember(nameof(CA_Manufacturer_ReadOnly))]
		public ZGuid CA_OA_ManufacturerAddressCPR
		{
			get { return JI_OA_ManufacturerAddress; }
			set
			{
				JI_OA_ManufacturerAddress = value;
				CA_OA_ManufacturerAddressCPRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_OA_ManufacturerAddressCPRInfo
		{
			get => GetZPropertyInfo(Schema.CA_OA_ManufacturerAddressCPR);
		}

		#endregion

		#region CA_ProductionDateCPR

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public ZDateTime CA_ProductionDateCPR
		{
			get { return CA_ProductionDate; }
			set
			{
				CA_ProductionDate = value;
				CA_ProductionDateCPRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ProductionDateCPRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ProductionDateCPR); }
		}

		#endregion

		#region CA_GTINNumberCPR

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberCPR
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_BrandNameCPR

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameCPR
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameCPRInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameCPR), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberCPR

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberCPR
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		#region IsTradeNameReadOnlyForCPRAndPES

		bool IsTradeNameReadOnlyForCPRAndPES
		{
			get { return PGAReadOnlyForNonIID || (!CA_CPRProgramInd && !CA_PESProgramInd); }
		}

		#endregion

		#region CA_TradeNameCPR

		[MaxLength(AddInfoJobComInvoiceLine.Schema.CA_TradeNameMaxLength)]
		[ReadOnlyMember(nameof(IsTradeNameReadOnlyForCPRAndPES))]
		public ZString CA_TradeNameCPR
		{
			get { return CA_TradeName; }
			set
			{
				CA_TradeName = value;
				CA_TradeNameCPRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_TradeNameCPRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_TradeNameCPR); }
		}

		#endregion

		#region CA_HC_DSE_ReadOnly

		bool CA_HC_DSE_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_DSEProgramInd; }
		}

		#endregion

		#region CA_DSEProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_DSEProgramInd
		{
			get
			{
				var dseProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.DSE);
				return dseProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_DSEProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var dseProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.DSE);
					if (dseProgram != null)
					{
						dseProgram.DeclareYes = value;
					}
				}
				CA_DSEProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_DSEProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_DSEProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeDSE

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesDSE))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeDSEMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_DSE_ReadOnly))]
		public ZString CA_IntendedUseCodeDSE
		{
			get { return HCPGAHeader.CA_IntendedUseCodeDSE; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeDSE = value;
				CA_IntendedUseCodeDSEInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeDSEInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeDSE); }
		}

		#endregion

		#region CA_CategoryDSE

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesDSE))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryDSEMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_DSE_ReadOnly))]
		public ZString CA_CategoryDSE
		{
			get { return HCPGAHeader.CA_CategoryDSE; }
			set
			{
				HCPGAHeader.CA_CategoryDSE = value;
				CA_CategoryDSEInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryDSEInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryDSE); }
		}

		#endregion

		#region CA_ComplianceStatement

		[ReadOnlyMember(nameof(CA_HC_DSE_ReadOnly))]
		public ZBool CA_ComplianceStatement
		{
			get { return HCPGAHeader.CA_ComplianceStatement; }
			set
			{
				HCPGAHeader.CA_ComplianceStatement = value;
				CA_ComplianceStatementInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ComplianceStatementInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ComplianceStatement); }
		}

		#endregion

		#region CA_HC_HDR_ReadOnly

		bool CA_HC_HDR_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_HDRProgramInd; }
		}

		#endregion

		#region CA_HDRProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_HDRProgramInd
		{
			get
			{
				var hdrProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.HDR);
				return hdrProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_HDRProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var hdrProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.HDR);
					if (hdrProgram != null)
					{
						hdrProgram.DeclareYes = value;
					}
				}
				CA_HDRProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_HDRProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_HDRProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeHDR

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesHDR))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeHDRMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_HDR_ReadOnly))]
		public ZString CA_IntendedUseCodeHDR
		{
			get { return HCPGAHeader.CA_IntendedUseCodeHDR; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeHDR = value;
				CA_IntendedUseCodeHDRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeHDRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeHDR); }
		}

		#endregion

		#region CA_CategoryHDR

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesHDR))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryHDRMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_HDR_ReadOnly))]
		public ZString CA_CategoryHDR
		{
			get { return HCPGAHeader.CA_CategoryHDR; }
			set
			{
				HCPGAHeader.CA_CategoryHDR = value;
				CA_CategoryHDRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryHDRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryHDR); }
		}

		#endregion

		#region CA_ProductionDateHDR

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public ZDateTime CA_ProductionDateHDR
		{
			get { return CA_ProductionDate; }
			set
			{
				CA_ProductionDate = value;
				CA_ProductionDateHDRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ProductionDateHDRInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ProductionDateHDR); }
		}

		#endregion

		#region CA_GTINNumberHDR

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberHDR
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_BrandNameHDR

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameHDR
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameHDRInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameHDR), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberHDR

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberHDR
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		#region CA_HC_OCS_ReadOnly

		bool CA_HC_OCS_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_OCSProgramInd; }
		}

		#endregion

		#region CA_OCSProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_OCSProgramInd
		{
			get
			{
				var ocsProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.OCS);
				return ocsProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_OCSProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var ocsProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.OCS);
					if (ocsProgram != null)
					{
						ocsProgram.DeclareYes = value;
					}
				}
				CA_OCSProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_OCSProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_OCSProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeOCS

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesOCS))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeOCSMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_OCS_ReadOnly))]
		public ZString CA_IntendedUseCodeOCS
		{
			get { return HCPGAHeader.CA_IntendedUseCodeOCS; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeOCS = value;
				CA_IntendedUseCodeOCSInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeOCSInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeOCS); }
		}

		#endregion

		#region CA_CategoryOCS

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesOCS))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryOCSMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_OCS_ReadOnly))]
		public ZString CA_CategoryOCS
		{
			get { return HCPGAHeader.CA_CategoryOCS; }
			set
			{
				HCPGAHeader.CA_CategoryOCS = value;
				CA_CategoryOCSInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryOCSInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryOCS); }
		}

		#endregion

		#region CA_ManufacturerOrgPKOCS

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupplierList))]
		[ReadOnlyMember(nameof(CA_Manufacturer_ReadOnly))]
		public ZGuid CA_ManufacturerOrgPKOCS
		{
			get { return JI_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { JI_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		#endregion

		#region CA_OA_ManufacturerAddressOCS

		[List(nameof(JI_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ReadOnlyMember(nameof(CA_Manufacturer_ReadOnly))]
		public ZGuid CA_OA_ManufacturerAddressOCS
		{
			get { return JI_OA_ManufacturerAddress; }
			set
			{
				JI_OA_ManufacturerAddress = value;
				CA_OA_ManufacturerAddressOCSInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_OA_ManufacturerAddressOCSInfo
		{
			get => GetZPropertyInfo(Schema.CA_OA_ManufacturerAddressOCS);
		}

		#endregion

		#region CA_BrandNameOCS

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameOCS
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameOCSInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameOCS), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberOCS

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberOCS
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		#region CA_HC_MDE_ReadOnly

		bool CA_HC_MDE_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_MDEProgramInd; }
		}

		#endregion

		#region CA_MDEProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_MDEProgramInd
		{
			get
			{
				var mdeProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.MDE);
				return mdeProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_MDEProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var mdeProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.MDE);
					if (mdeProgram != null)
					{
						mdeProgram.DeclareYes = value;
					}
				}
				CA_MDEProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_MDEProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_MDEProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeMDE

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesMDE))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeMDEMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_MDE_ReadOnly))]
		public ZString CA_IntendedUseCodeMDE
		{
			get { return HCPGAHeader.CA_IntendedUseCodeMDE; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeMDE = value;
				CA_IntendedUseCodeMDEInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeMDEInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeMDE); }
		}

		#endregion

		#region CA_CategoryMDE

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesMDE))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryMDEMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_MDE_ReadOnly))]
		public ZString CA_CategoryMDE
		{
			get { return HCPGAHeader.CA_CategoryMDE; }
			set
			{
				HCPGAHeader.CA_CategoryMDE = value;
				CA_CategoryMDEInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryMDEInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryMDE); }
		}

		#endregion

		#region CA_UniqueDeviceIDNumber

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_UniqueDeviceIDNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_MDE_ReadOnly))]
		public ZString CA_UniqueDeviceIDNumber
		{
			get { return HCPGAHeader.CA_UniqueDeviceIDNumber; }
			set
			{
				HCPGAHeader.CA_UniqueDeviceIDNumber = value;
				CA_UniqueDeviceIDNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_UniqueDeviceIDNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CA_UniqueDeviceIDNumber); }
		}

		#endregion

		#region CA_ProductionDateMDE

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public ZDateTime CA_ProductionDateMDE
		{
			get { return CA_ProductionDate; }
			set
			{
				CA_ProductionDate = value;
				CA_ProductionDateMDEInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ProductionDateMDEInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ProductionDateMDE); }
		}

		#endregion

		#region CA_GTINNumberMDE

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberMDE
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_BrandNameMDE

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameMDE
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameMDEInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameMDE), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberMDE

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberMDE
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		#region CA_ModelName_ReadOnly

		bool CA_ModelName_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || (!CA_MDEProgramInd && !CA_REDProgramInd); }
		}

		#endregion

		#region CA_ModelNameMDE

		[ReadOnlyMember(nameof(CA_ModelName_ReadOnly))]
		public ZString CA_ModelNameMDE
		{
			get { return JI_Model; }
			set { JI_Model = value; }
		}

		#endregion

		#region CA_MDE_LEX

		[ReadOnlyMember(nameof(CA_HC_MDE_ReadOnly))]
		public ZBool CA_MDE_LEX
		{
			get { return HCPGAHeader.CA_MDE_LEX; }
			set
			{
				HCPGAHeader.CA_MDE_LEX = value;
				CA_MDE_LEXInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_MDE_LEXInfo
		{
			get { return GetZPropertyInfo(Schema.CA_MDE_LEX); }
		}

		#endregion

		#region CA_HC_NHP_ReadOnly

		bool CA_HC_NHP_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_NHPProgramInd; }
		}

		#endregion

		#region CA_NHPProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_NHPProgramInd
		{
			get
			{
				var nhpProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.NHP);
				return nhpProgram.DeclareYes;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_NHPProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var nhpProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.NHP);
					if (nhpProgram != null)
					{
						nhpProgram.DeclareYes = value;
					}
				}
				CA_NHPProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_NHPProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_NHPProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeNHP

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesNHP))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeNHPMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_NHP_ReadOnly))]
		public ZString CA_IntendedUseCodeNHP
		{
			get { return HCPGAHeader.CA_IntendedUseCodeNHP; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeNHP = value;
				CA_IntendedUseCodeNHPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeNHPInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeNHP); }
		}

		#endregion

		#region CA_CategoryNHP

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesNHP))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryNHPMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_NHP_ReadOnly))]
		public ZString CA_CategoryNHP
		{
			get { return HCPGAHeader.CA_CategoryNHP; }
			set
			{
				HCPGAHeader.CA_CategoryNHP = value;
				CA_CategoryNHPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryNHPInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryNHP); }
		}

		#endregion

		#region CA_ProductionDateNHP

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public ZDateTime CA_ProductionDateNHP
		{
			get { return CA_ProductionDate; }
			set
			{
				CA_ProductionDate = value;
				CA_ProductionDateNHPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ProductionDateNHPInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ProductionDateNHP); }
		}

		#endregion

		#region CA_GTINNumberNHP

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberNHP
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_BrandNameNHP

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameNHP
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameNHPInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameNHP), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberNHP

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberNHP
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		#region CA_HC_PES_ReadOnly

		bool CA_HC_PES_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_PESProgramInd; }
		}

		#endregion

		#region CA_PESProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_PESProgramInd
		{
			get
			{
				var pesProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.PES);
				return pesProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_PESProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var pesProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.PES);
					if (pesProgram != null)
					{
						pesProgram.DeclareYes = value;
					}
				}
				CA_PESProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_PESProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_PESProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodePES

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesPES))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodePESMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_PES_ReadOnly))]
		public ZString CA_IntendedUseCodePES
		{
			get { return HCPGAHeader.CA_IntendedUseCodePES; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodePES = value;
				CA_IntendedUseCodePESInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodePESInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodePES); }
		}

		#endregion

		#region CA_CategoryPES

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesPES))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryPESMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_PES_ReadOnly))]
		public ZString CA_CategoryPES
		{
			get { return HCPGAHeader.CA_CategoryPES; }
			set
			{
				HCPGAHeader.CA_CategoryPES = value;
				CA_CategoryPESInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryPESInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryPES); }
		}

		#endregion

		#region CA_ManufacturerOrgPKPES

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupplierList))]
		[ReadOnlyMember(nameof(CA_Manufacturer_ReadOnly))]
		public ZGuid CA_ManufacturerOrgPKPES
		{
			get { return JI_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { JI_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		#endregion

		#region CA_OA_ManufacturerAddressPES

		[List(nameof(JI_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ReadOnlyMember(nameof(CA_Manufacturer_ReadOnly))]
		public ZGuid CA_OA_ManufacturerAddressPES
		{
			get { return JI_OA_ManufacturerAddress; }
			set
			{
				JI_OA_ManufacturerAddress = value;
				CA_OA_ManufacturerAddressPESInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_OA_ManufacturerAddressPESInfo
		{
			get => GetZPropertyInfo(Schema.CA_OA_ManufacturerAddressPES);
		}

		#endregion

		#region CA_ProductionDatePES

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public ZDateTime CA_ProductionDatePES
		{
			get { return CA_ProductionDate; }
			set
			{
				CA_ProductionDate = value;
				CA_ProductionDatePESInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ProductionDatePESInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ProductionDatePES); }
		}

		#endregion

		#region CA_DangerousGoodsDGSubsPES

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.UNDGs))]
		[ReadOnlyMember(nameof(CA_HC_PES_ReadOnly))]
		public ZGuid CA_DangerousGoodsDGSubsPES
		{
			get { return DangerousGoodsDGSubs; }
			set { DangerousGoodsDGSubs = value; }
		}

		#endregion

		#region CA_BrandNamePES

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNamePES
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNamePESInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNamePES), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberPES

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberPES
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		#region CA_TradeNamePES

		[MaxLength(AddInfoJobComInvoiceLine.Schema.CA_TradeNameMaxLength)]
		[ReadOnlyMember(nameof(IsTradeNameReadOnlyForCPRAndPES))]
		public ZString CA_TradeNamePES
		{
			get { return CA_TradeName; }
			set
			{
				CA_TradeName = value;
				CA_TradeNamePESInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_TradeNamePESInfo
		{
			get { return GetZPropertyInfo(Schema.CA_TradeNamePES); }
		}

		#endregion

		#region CA_CASNumber

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CASNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_PES_ReadOnly))]
		public ZString CA_CASNumber
		{
			get { return HCPGAHeader.CA_CASNumber; }
			set
			{
				HCPGAHeader.CA_CASNumber = value;
				CA_CASNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CASNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CASNumber); }
		}

		#endregion

		#region CA_PES_SPCP

		[ReadOnlyMember(nameof(CA_HC_PES_ReadOnly))]
		public ZBool CA_PES_SPCP
		{
			get { return HCPGAHeader.CA_PES_SPCP; }
			set
			{
				HCPGAHeader.CA_PES_SPCP = value;
				CA_PES_SPCPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_PES_SPCPInfo
		{
			get { return GetZPropertyInfo(Schema.CA_PES_SPCP); }
		}

		#endregion

		#region CA_PES_EPCP

		[ReadOnlyMember(nameof(CA_HC_PES_ReadOnly))]
		public ZBool CA_PES_EPCP
		{
			get { return HCPGAHeader.CA_PES_EPCP; }
			set
			{
				HCPGAHeader.CA_PES_EPCP = value;
				CA_PES_EPCPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_PES_EPCPInfo
		{
			get { return GetZPropertyInfo(Schema.CA_PES_EPCP); }
		}

		#endregion

		#region CA_HC_RED_ReadOnly

		bool CA_HC_RED_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_REDProgramInd; }
		}

		#endregion

		#region CA_REDProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_REDProgramInd
		{
			get
			{
				var redProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.RED);
				return redProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_REDProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var redProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.RED);
					if (redProgram != null)
					{
						redProgram.DeclareYes = value;
					}
				}
				CA_REDProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_REDProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_REDProgramInd); }
		}

		#endregion

		#region CA_CategoryRED

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesRED))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryREDMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_RED_ReadOnly))]
		public ZString CA_CategoryRED
		{
			get { return HCPGAHeader.CA_CategoryRED; }
			set
			{
				HCPGAHeader.CA_CategoryRED = value;
				CA_CategoryREDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryREDInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryRED); }
		}

		#endregion

		#region CA_FDANumber

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_FDANumberMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_RED_ReadOnly))]
		public ZString CA_FDANumber
		{
			get { return HCPGAHeader.CA_FDANumber; }
			set
			{
				HCPGAHeader.CA_FDANumber = value;
				CA_FDANumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_FDANumberInfo
		{
			get { return GetZPropertyInfo(Schema.CA_FDANumber); }
		}

		#endregion

		#region CA_ModelNameRED

		[ReadOnlyMember(nameof(CA_ModelName_ReadOnly))]
		public ZString CA_ModelNameRED
		{
			get { return JI_Model; }
			set { JI_Model = value; }
		}

		#endregion

		#region CA_HC_VET_ReadOnly

		bool CA_HC_VET_ReadOnly
		{
			get { return PGAReadOnlyForNonIID || !CA_VETProgramInd; }
		}

		#endregion

		#region CA_VETProgramInd

		[ReadOnlyMember(nameof(PGAReadOnlyForNonIID))]
		public ZBool CA_VETProgramInd
		{
			get
			{
				var vetProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.VET);
				return vetProgram?.DeclareYes ?? ZBool.False;
			}
			set
			{
				var oldValue = HCPGAHeader.CA_VETProgramInd == YesNoList.Codes.Yes;
				if (oldValue != value)
				{
					var vetProgram = GetPGAProgramRequirement(PGACodes.Codes.HC, HCPGADepartmentCodes.Codes.VET);
					if (vetProgram != null)
					{
						vetProgram.DeclareYes = value;
					}
				}
				CA_VETProgramIndInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_VETProgramIndInfo
		{
			get { return GetZPropertyInfo(Schema.CA_VETProgramInd); }
		}

		#endregion

		#region CA_IntendedUseCodeVET

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesVET))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_IntendedUseCodeVETMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_VET_ReadOnly))]
		public ZString CA_IntendedUseCodeVET
		{
			get { return HCPGAHeader.CA_IntendedUseCodeVET; }
			set
			{
				HCPGAHeader.CA_IntendedUseCodeVET = value;
				CA_IntendedUseCodeVETInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_IntendedUseCodeVETInfo
		{
			get { return GetZPropertyInfo(Schema.CA_IntendedUseCodeVET); }
		}

		#endregion

		#region CA_CategoryVET

		[List(nameof(HCAddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesVET))]
		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_CategoryVETMaxLength)]
		[ReadOnlyMember(nameof(CA_HC_VET_ReadOnly))]
		public ZString CA_CategoryVET
		{
			get { return HCPGAHeader.CA_CategoryVET; }
			set
			{
				HCPGAHeader.CA_CategoryVET = value;
				CA_CategoryVETInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CategoryVETInfo
		{
			get { return GetZPropertyInfo(Schema.CA_CategoryVET); }
		}

		#endregion

		#region CA_ProductionDateVET

		[ReadOnlyMember(nameof(CA_ProductionDate_ReadOnly))]
		public ZDateTime CA_ProductionDateVET
		{
			get { return CA_ProductionDate; }
			set
			{
				CA_ProductionDate = value;
				CA_ProductionDateVETInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_ProductionDateVETInfo
		{
			get { return GetZPropertyInfo(Schema.CA_ProductionDateVET); }
		}

		#endregion

		#region CA_GTINNumberVET

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_GTINNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_GTINNumber_ReadOnly))]
		public ZString CA_GTINNumberVET
		{
			get { return CA_GTINNumber; }
			set { CA_GTINNumber = value; }
		}

		#endregion

		#region CA_BrandNameVET

		[ReadOnlyMember(nameof(CA_BrandName_ReadOnly))]
		public ZString CA_BrandNameVET
		{
			get { return JI_BrandName; }
			set { JI_BrandName = value; }
		}

		public ZPropertyInfo CA_BrandNameVETInfo => GetWrappedZPropertyInfo(nameof(CA_BrandNameVET), x => JI_BrandNameInfo);

		#endregion

		#region CA_BatchLotNumberVET

		[MaxLength(AutoHCPGAHeaderAddInfo.Schema.CA_BatchLotNumberMaxLength)]
		[ReadOnlyMember(nameof(CA_BatchLotNumber_ReadOnly))]
		public ZString CA_BatchLotNumberVET
		{
			get { return CA_BatchLotNumber; }
			set { CA_BatchLotNumber = value; }
		}

		#endregion

		public CFIAPGAHeaderAddInfoLookups CFIAAddInfoLookups
		{
			get { return cfiaAddInfoLookups ?? (cfiaAddInfoLookups = CFIAPGAHeader.AddInfoLookups); }
		}
		CFIAPGAHeaderAddInfoLookups cfiaAddInfoLookups;

		public HCPGAHeaderAddInfoLookups HCAddInfoLookups
		{
			get { return hcAddInfoLookups ?? (hcAddInfoLookups = HCPGAHeader.AddInfoLookups); }
		}
		HCPGAHeaderAddInfoLookups hcAddInfoLookups;

		public ZBool IsSIMADutyRequired => Factory.GetValue(ref isSIMADutyRequiredCached, () =>
		{
			var result = false;

			if (!HasSIMADuty && InvoiceHeader is JobComInvoiceHeader invoiceHeader)
			{
				var isImportIncludingB2 = IsImportIncludingB2;
				var effectiveDateForDutyRate = EffectiveDateForDutyRate;
				result = DutyAndTaxManager.IsSIMADutyRequired(Factory, isImportIncludingB2, JI_Tariff, JI_CountryOfOrigin, CA_SIMADumpingNum, effectiveDateForDutyRate, true);
				if (!result && Declaration is JobDeclaration declaration)
				{
					var countryOfExport = declaration.IsLVS ? CA_RN_NKExport : invoiceHeader.CA_RN_NKExport;
					result = DutyAndTaxManager.IsSIMADutyRequired(Factory, isImportIncludingB2, JI_Tariff, countryOfExport, CA_SIMADumpingNum, effectiveDateForDutyRate, false);
				}
			}

			return result;
		});
		CachedProperty<ZBool> isSIMADutyRequiredCached;

		bool HasSIMADuty => Factory.GetValue(ref hasSIMADutyCached, () => DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SIMADuty));
		CachedProperty<bool> hasSIMADutyCached;

		public ZBool IsHSCodeContainsOGDCFIA
		{
			get
			{
				if (isHSCodeContainsOGDCFIACached == null)
				{
					isHSCodeContainsOGDCFIACached = new CachedProperty<ZBool>(Factory, () => HasPGACodeInTariff(PGACodes.Codes.CFIA));
				}

				return isHSCodeContainsOGDCFIACached.Value;
			}
		}
		CachedProperty<ZBool> isHSCodeContainsOGDCFIACached;

		public ZBool IsHSCodeContainsOGDNRCan
		{
			get
			{
				if (isHSCodeContainsOGDNRCanCached == null)
				{
					isHSCodeContainsOGDNRCanCached = new CachedProperty<ZBool>(Factory, () => HasPGACodeInTariff(PGACodes.Codes.NRCan));
				}

				return isHSCodeContainsOGDNRCanCached.Value;
			}
		}
		CachedProperty<ZBool> isHSCodeContainsOGDNRCanCached;

		public ZBool IsHSCodeContainsOGDTC
		{
			get
			{
				if (isHSCodeContainsOGDTCCached == null)
				{
					isHSCodeContainsOGDTCCached = new CachedProperty<ZBool>(Factory, () => HasPGACodeInTariff(PGACodes.Codes.TC));
				}

				return isHSCodeContainsOGDTCCached.Value;
			}
		}
		CachedProperty<ZBool> isHSCodeContainsOGDTCCached;

		ZBool IsCommoditySequenceEmpty
		{
			get
			{
				if (isCommoditySequenceEmpty == null)
				{
					isCommoditySequenceEmpty = new CachedProperty<ZBool>(Factory, () => !Regex.Match(CommoditySequence, @"^\[\d+,[1-9]\d*\]$").Success);
				}
				return isCommoditySequenceEmpty.Value;
			}
		}
		CachedProperty<ZBool> isCommoditySequenceEmpty;

		ZBool HasPGACodeInTariff(ZString pgaType)
		{
			var declaration = Declaration;
			return declaration != null && declaration.IsImport && !declaration.IsIID && CARefTariffDataLoader.DoesTariffHasPGAType(Factory, JI_Tariff, pgaType, EffectiveDateForDutyRate);
		}

		#endregion

		#region Overrides

		#region Properties

		public ZShort InvoiceHeaderSequence => InvoiceHeader?.JZ_InvoiceDisplaySequence ?? ZShort.Zero;

		public ZString CommoditySequence
		{
			get
			{
				if (commoditySequence == null)
				{
					commoditySequence = new CachedProperty<ZString>(Factory, () => B3EntryLine?.SequenceNumber ?? "[0,0]");
				}
				return commoditySequence.Value;
			}
		}
		CachedProperty<ZString> commoditySequence;

		public ZPropertyInfo CommoditySequenceInfo
		{
			get { return GetZPropertyInfo(Schema.CommoditySequence); }
		}

		public ZShort GoodsShipmentSequence
		{
			get
			{
				if (goodsShipmentSequence == null)
				{
					goodsShipmentSequence = new CachedProperty<ZShort>(Factory, delegate
					{
						return B3EntryLine?.CL_GoodsShipmentSequence ?? ZShort.Zero;
					});
				}
				return goodsShipmentSequence.Value;
			}
		}
		CachedProperty<ZShort> goodsShipmentSequence;

		public ZPropertyInfo GoodsShipmentSequenceInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsShipmentSequence); }
		}

		#region JI_OA_ConsigneeAddress

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DeliveredParties))]
		public override ZGuid JI_OA_ConsigneeAddress
		{
			get { return base.JI_OA_ConsigneeAddress; }

			set
			{
				var oldValue = JI_OA_ConsigneeAddress;
				base.JI_OA_ConsigneeAddress = value;
				if (!IsCopying && oldValue != JI_OA_ConsigneeAddress)
				{
					SetAVSStatusIfNeeded();
					if (CA_IsCasualImport)
					{
						DefaultCasualImportDestinationProvinceIfNeeded();
					}
				}
			}
		}

		public OrgHeader Consignee
		{
			get => ConsigneeAddress?.Header;
		}

		[RelatedBusinessObject("Consignee")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Consignees))]
		public ZGuid ConsigneePK
		{
			get => JI_OA_ConsigneeAddress_ZAddress.OrgPK;
			set
			{
				JI_OA_ConsigneeAddress_ZAddress.OrgPK = value;
				ConsigneePKInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ConsigneePKInfo
		{
			get => GetZPropertyInfo(Schema.ConsigneePK);
		}

		public void DefaultJI_OA_ConsigneeAddress(ZGuid oldConsigneeAddress, ZGuid newConsigneeAddress)
		{
			if (JI_OA_ConsigneeAddress != newConsigneeAddress && (JI_OA_ConsigneeAddress.IsEmpty || oldConsigneeAddress == JI_OA_ConsigneeAddress))
			{
				JI_OA_ConsigneeAddress = newConsigneeAddress;
				var invoiceConsigneePK = ConsigneeAddress?.OA_OH ?? ZGuid.Empty;
				if (JI_OA_ConsigneeAddress_ZAddress.OrgPK != invoiceConsigneePK)
				{
					JI_OA_ConsigneeAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(invoiceConsigneePK);
				}
			}
		}
		#endregion

		#region JI_OA_ManufacturerAddress

		[List(nameof(JI_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JI_OA_ManufacturerAddress
		{
			get
			{
				var manufacturerAddress = base.JI_OA_ManufacturerAddress;
				return manufacturerAddress.IsEmpty ? InvoiceHeader?.JZ_OA_ManufacturerAddress ?? ZGuid.Empty : manufacturerAddress;
			}
			set
			{
				base.JI_OA_ManufacturerAddress = value;
				RefreshBindingForDeclaredPGAHeaders();
			}
		}

		protected override ZAddress GetNewJI_OA_ManufacturerAddress_ZAddress()
		{
			var address = base.GetNewJI_OA_ManufacturerAddress_ZAddress();
			address.IsOrgVisible = true;
			address.GetDefaultAddress = (header) => header?.MainAddress?.PK ?? ZGuid.Empty;
			return address;
		}

		#endregion

		#region DangerousGoods

		public UNDGDataItem DangerousGoods => UNDGs.FirstItemForBinding[0];

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.UNDGs))]
		public ZGuid DangerousGoodsDGSubs
		{
			get
			{
				return (dangerousGoodsDGSubsCached
						?? (dangerousGoodsDGSubsCached =
							new CachedProperty<ZGuid>(Factory, () => GetUNDGValue(undg => undg.DI_DG)))).Value;
			}
			set
			{
				SetUNDGValue(x => x.DI_DG = value);
				base.JI_HazMatCode = DangerousGoods?.UNDGSubstance != null ? DangerousGoods.UNDGSubstance.DG_Code.ToUpperInvariant() : ZString.Empty;
				DangerousGoodsDGSubsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DangerousGoodsDGSubsInfo => GetZPropertyInfo(Schema.DangerousGoodsDGSubs);

		CachedProperty<ZGuid> dangerousGoodsDGSubsCached;

		T GetUNDGValue<T>(Func<UNDGDataItem, T> valueGetter)
		{
			return UNDGs.Count > 1 ? default : UNDGs.Select(valueGetter).FirstOrDefault();
		}

		void SetUNDGValue(Action<UNDGDataItem> valueSetter)
		{
			if (UNDGs.Count <= 1)
			{
				var undg = (UNDGs.FirstOrDefault() ?? UNDGs.AddNew());
				using (undg.GetValidationSuspender())
				{
					var oldDI_DGValue = undg.DI_DG;
					valueSetter(undg);
					if (undg.DI_DG.IsEmpty && undg.DI_OC_DGContact.IsEmpty)
					{
						undg.Delete();
					}
					else if (!IsCopying && oldDI_DGValue != DangerousGoodsDGSubs && YesNoList.IsYesOrNo(CA_NRCanInd))
					{
						var nrCanPGAHeader = NRCanPGAHeader;
						nrCanPGAHeader.DangerousGoodsDGSubsInfo.RefreshBinding();
						nrCanPGAHeader.Validation.ValidateDangerousGoodsDGSubs();
					}
				}
			}
		}

		#endregion

		#region JI_Calc_Invoice

		public override ZString JI_Calc_Invoice
		{
			get { return base.JI_Calc_Invoice; }
			set
			{
				var hasChange = base.CA_OriginalLineNo != value;
				var oldIncoTerm = InvoiceHeader.IncoTerm;
				base.JI_Calc_Invoice = value;
				var newInvoiceHeader = InvoiceHeader;
				var newIncoTerm = newInvoiceHeader.IncoTerm;
				if (hasChange && oldIncoTerm != newIncoTerm)
				{
					ResetCalculationMethod(newInvoiceHeader.CalculationMethod);
				}
			}
		}

		#endregion

		#region CA_OriginalLineNo

		[ReadOnlyMember(nameof(CA_OriginalLineNo_ReadOnly))]
		public override ZString CA_OriginalLineNo
		{
			get
			{
				var result = ZString.Empty;
				var lineAccountFor = LineAccountFor;
				if (lineAccountFor != null)
				{
					if (this == lineAccountFor.ReadOnlyAsClaimForFilteredInvoiceLines.Cast<JobComInvoiceLine>().OrderBy(c => c.JI_LineNo).FirstOrDefault())
					{
						result = lineAccountFor.CA_OriginalLineNo;
					}
					else
					{
						result = lineAccountFor.CA_OriginalLineNo + SplitLine;
					}
				}
				else
				{
					result = base.CA_OriginalLineNo;
				}
				return result;
			}
			set
			{
				var hasChange = base.CA_OriginalLineNo != value;
				base.CA_OriginalLineNo = value;
				if (hasChange && IsB2OrB3XAdjustments)
				{
					if (CA_IsAccountForLine)
					{
						foreach (JobComInvoiceLine claimLine in ReadOnlyAsClaimForFilteredInvoiceLines)
						{
							claimLine.CA_IsAccountForLineInfo.RefreshBinding();
						}
					}
					else if (!CA_IsSeeded)
					{
						var asAccountedLineOriginalNo = GetEffectiveOriginalLineNo(CA_OriginalLineNo).ToString();
						var asAccountedLine = InvoiceHeader?.CorrespondingAsAccountedForInvoice?.AsAccountForFilteredInvoiceLines?.Cast<JobComInvoiceLine>()
							.Where(x => JI_ParentID.IsEmpty).Select(s => (s.CA_OriginalLineNo, Line: s)).Where(w => w.CA_OriginalLineNo == asAccountedLineOriginalNo).OrderBy(o => o.CA_OriginalLineNo).FirstOrDefault().Line;
						if (asAccountedLine != null)
						{
							JI_ParentID = asAccountedLine.PK;
							JI_ParentTableCode = asAccountedLine.TablePrefix;
						}
					}
				}
				EnableAndSynchronise(true);
			}
		}

		bool CA_OriginalLineNo_ReadOnly
		{
			get { return !CA_IsAccountForLine && !CA_OriginalLineNo.IsEmpty; }
		}

		internal const string SplitLine = "/SL";

		public static ZInt GetEffectiveOriginalLineNo(ZString originalLineNo)
		{
			var lineStrWithoutSplit = originalLineNo.Trim().EndsWith(SplitLine, StringComparison.OrdinalIgnoreCase) ? originalLineNo.Substring(0, originalLineNo.Length - 3) : originalLineNo;
			ZInt result = ZInt.Zero;
			ZInt.TryParse(lineStrWithoutSplit, out result);
			return result;
		}

		#endregion

		#region CA_IsSeeded

		public override ZBool CA_IsSeeded
		{
			get { return base.CA_IsSeeded; }
			set
			{
				base.CA_IsSeeded = value;
				if (IsB2AsAccountForSeededLine)
				{
					var originalDec = Declaration?.OriginalDeclaration;
					if (originalDec == null || originalDec.JE_MessageType != JobMessageTypeList.Codes.LowValueShipments)
					{
						this.SetReadOnlyIncludingChildren(true);
					}
				}
			}
		}

		#endregion

		#region CA_CVforCurrConvOvr

		[ResourceStringData("CAAddInfo|CA_CVforCurrConvOvr", Caption = "Override", ShortCaption = "Ovr.", FullDescription = "Checking this box allows manual override of the Value for Currency Conversion.")]
		public override ZBool CA_CVforCurrConvOvr
		{
			get { return IsB2OrB3XAdjustments ? ZBool.True : base.CA_CVforCurrConvOvr; }
			set { base.CA_CVforCurrConvOvr = value; }
		}

		#endregion

		#region JI_CC/Classification

		public new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}

		public override ZGuid JI_CC
		{
			get { return base.JI_CC; }
			set
			{
				var hasChanges = base.JI_CC != value;
				if (hasChanges)
				{
					base.JI_CC = value;
					if (!IsCopying && Classification != null && IsImportIncludingB2)
					{
						AddClassificationInfoToLine();
					}
				}
			}
		}

		void AddClassificationInfoToLine()
		{
			try
			{
				disableSIMAMeasuresCollectionRefreshForCusClassification = true;

				CA_99TariffCode = Classification.CCA_99TariffCode;
				CA_ValueForDutyCode = Classification.CCA_ValueForDutyCode;
				CA_AuthorityNumber = Classification.CCA_AuthorityNumber;
				if (!IsB2OrB3XAdjustments)
				{
					CA_TRSNumber = Classification.CCA_TRSNumber;
					CA_TreatmentCode = Classification.CCA_TreatmentCode;
				}
				UpdatePGADetailsFromClassification(Classification);
				UpdateDutiesAndTaxesFromCusClassification(Classification);
			}
			finally
			{
				disableSIMAMeasuresCollectionRefreshForCusClassification = false;
			}
		}
		ZBool disableSIMAMeasuresCollectionRefreshForCusClassification;

		#endregion

		#region JI_StateOrRegionOfOrigin

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.StateCodesList))]
		[ReadOnlyMember(nameof(JI_StateOrRegionOfOrigin_ReadOnly))]
		[ResourceStringData("0cb6388b-0c0b-41b5-a804-6c1de9b19771", Caption = "State of Origin", ShortCaption = "State", MediumCaption = "Org. State", FullDescription = "The state for Country/Region of Origin.")]
		[MaxLength(2)]
		public override ZString JI_StateOrRegionOfOrigin
		{
			get
			{
				var originState = base.JI_StateOrRegionOfOrigin;
				var invoiceOriginCountry = InvoiceHeader?.JZ_RN_NKDefaultOrigin ?? ZString.Empty;

				return originState.IsEmpty
						&& InvoiceHeader != null
						&& JI_CountryOfOrigin == invoiceOriginCountry
					? InvoiceHeader.JZ_RW_NKOriginState
					: originState;
			}
			set
			{
				var oldValue = JI_StateOrRegionOfOrigin;
				var truncatedValue = value.Left(2);
				base.JI_StateOrRegionOfOrigin = truncatedValue;
				if (!IsCopying && oldValue != JI_StateOrRegionOfOrigin)
				{
					if (!truncatedValue.IsEmpty && base.CA_StateOfSource.IsEmpty && !CA_StateOfSource_ReadOnly && IsRegulatedByIIDCFIA)
					{
						CA_StateOfSource = truncatedValue;
					}
					RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		#endregion

		#region JI_CountryOfOrigin

		public override ZString JI_CountryOfOrigin
		{
			get
			{
				var countryOfOrigin = base.JI_CountryOfOrigin;
				return countryOfOrigin.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_RN_NKDefaultOrigin : countryOfOrigin;
			}
			set
			{
				var oldValue = JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = value;
				if (!IsCopying && oldValue != JI_CountryOfOrigin)
				{
					if (JI_StateOrRegionOfOrigin_ReadOnly)
					{
						JI_StateOrRegionOfOrigin = ZString.Empty;
					}
					RefreshSIMAMeasuresCollection();
					if (!value.IsEmpty && base.CA_RN_NKSource.IsEmpty && IsRegulatedByIIDCFIA)
					{
						CA_RN_NKSource = JI_CountryOfOrigin;
					}
					RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		#endregion

		#region JI_FormattedTariff

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Tariffs))]
		public override ZString JI_FormattedTariff
		{
			get { return base.JI_FormattedTariff; }
			set
			{
				var oldValue = JI_FormattedTariff;
				base.JI_FormattedTariff = value;
				if (!IsCopying && JI_FormattedTariff != oldValue)
				{
					RefreshDutiesAndTaxesWhenTariffChangedIfNeeded(true);
				}
			}
		}

		public TariffPropertyInfo JI_FormattedTariffCodeTariffInfo
		{
			get
			{
				return new TariffPropertyInfo
				{
					TariffType = IsDataLoadingModule ? TariffType.Export : TariffType.Import,
					DateForDutyRate = EffectiveDateForDutyRate
				};
			}
		}

		#endregion

		#region Line Price & Currency

		public override ZDecimal JI_LinePrice
		{
			get { return base.JI_LinePrice; }
			set
			{
				var oldValue = JI_LinePrice;
				base.JI_LinePrice = value;
				if (!IsCopying && oldValue != JI_LinePrice && IsImport)
				{
					CalculateAMMVCharge();
				}
			}
		}

		public override ZDecimal JI_InvoiceQuantity
		{
			get { return base.JI_InvoiceQuantity; }
			set
			{
				var oldValue = JI_InvoiceQuantity;
				base.JI_InvoiceQuantity = value;
				if (!IsCopying && oldValue != JI_InvoiceQuantity && IsImport)
				{
					CalculateAMMVCharge();
				}
			}
		}

		public override ZDecimal LinePriceForBalanceCalc
		{
			get { return IsSoftwareRemissionLine ? ZDecimal.Zero : base.LinePriceForBalanceCalc; }
		}

		#endregion

		#region JI_Calc_DutyAmount

		public override ZDecimal JI_Calc_DutyAmount
		{
			get
			{
				return (!IsImport && !IsB2OrB3XAdjustments) ? base.JI_Calc_DutyAmount : JI_Calc_NormalDutyAmount;
			}
		}

		#endregion

		#region JI_CustomsValue

		public override ZDecimal JI_CustomsValue
		{
			get { return IsImport ? CA_CustomsValue : base.JI_CustomsValue; }
		}

		#endregion

		[ReadOnlyMember(nameof(CA_AMMVPerUnit_ReadOnly))]
		public override ZDecimal CA_AMMVPerUnit
		{
			get { return base.CA_AMMVPerUnit; }
			set
			{
				var oldValue = CA_AMMVPerUnit;
				base.CA_AMMVPerUnit = value;
				if (!IsCopying && oldValue != CA_AMMVPerUnit && IsImport)
				{
					if (CA_AMMVPerUnit.IsEmpty || CA_AMMVPercentage.IsEmpty)
					{
						CalculateAMMVCharge();
					}
					else
					{
						CA_AMMVPercentage = ZDecimal.Zero;
					}
				}
			}
		}

		bool CA_AMMVPerUnit_ReadOnly => !CA_AMMVPercentage.IsEmpty && CA_AMMVPerUnit.IsEmpty;

		[ReadOnlyMember(nameof(CA_AMMVPercentage_ReadOnly))]
		public override ZDecimal CA_AMMVPercentage
		{
			get { return base.CA_AMMVPercentage; }
			set
			{
				var oldValue = CA_AMMVPercentage;
				base.CA_AMMVPercentage = value;
				if (!IsCopying && oldValue != CA_AMMVPercentage && IsImport)
				{
					if (CA_AMMVPerUnit.IsEmpty || CA_AMMVPercentage.IsEmpty)
					{
						CalculateAMMVCharge();
					}
					else
					{
						CA_AMMVPerUnit = ZDecimal.Zero;
					}
				}
			}
		}

		bool CA_AMMVPercentage_ReadOnly => !CA_AMMVPerUnit.IsEmpty && CA_AMMVPercentage.IsEmpty;

		#region CA_ADJCode

		[ResourceStringData("CAAddInfo|CA_ADJCode", Caption = "Adjustment Type", ShortCaption = "$/%", MediumCaption = "Adj. Type", FullDescription = "Indicates if the adjustment value is a Dollar ($) amount or a percentage (%) amount.")]
		public override ZString CA_ADJCode
		{
			get { return base.CA_ADJCode; }
			set
			{
				var oldValue = CA_ADJCode;
				base.CA_ADJCode = value;
				if (!IsCopying && oldValue != CA_ADJCode && CA_ADJCode.IsEmpty)
				{
					CA_ADJValue = ZDecimal.Zero;
				}
			}
		}

		#endregion

		#region CA_CalculationMethod

		[ReadOnlyMember(nameof(CA_CalculationMethod_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_CalculationMethod", Caption = "Remission")]
		public override ZString CA_CalculationMethod
		{
			get { return base.CA_CalculationMethod; }
			set
			{
				var oldValue = CA_CalculationMethod;

				if (!IsCopying && oldValue != value)
				{
					var shouldSetValue = true;
					var oldRemission = GetCalculationMethodDescription(oldValue);
					var newRemission = GetCalculationMethodDescription(value);
					if (RepairRemissions.Contains(oldValue))
					{
						if (!isResettingCalculationMethod && RepairRemissions.Contains(value))
						{
							Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation(Res.GetString("05411adc-fd01-44d9-958c-6a626f8dfbbf", "Remission type cannot be changed from {0} to {1} directly", oldRemission, newRemission));
							shouldSetValue = false;
						}
						else
						{
							shouldSetValue = isResettingCalculationMethod || ShowDeleteRepairLineConfirmation(oldRemission);
							if (shouldSetValue)
							{
								DeleteRemissionLine(oldValue);
							}
						}
					}

					if (shouldSetValue && JI_ParentID.IsEmpty)
					{
						if (RepairRemissions.Contains(value))
						{
							shouldSetValue = isResettingCalculationMethod || ShowAddRepairLineConfirmation(newRemission);
							if (shouldSetValue)
							{
								AddRemissionLine(value);
							}
						}
					}

					if (shouldSetValue)
					{
						base.CA_CalculationMethod = value;
						CA_AuthorityNumberInfo.RefreshBinding();
						ResetGSTExemptCode(oldValue);
						var dec = Declaration;
						if (dec != null)
						{
							dec.MarkAsNeedingValidation();
						}
						if (value != RefCusRulingTypeList.Codes.T)
						{
							RulingConfigurations.RemoveAndDeleteAll();
						}
						RulingConfigurations.SetReadOnlyIncludingChildren(ConfigurationsReadOnly);
					}
					ValidateCalculationMethodRelatedProperties();
					InvoiceHeader?.InvalidateJZ_Calc_LinesEnteredCache();
				}
			}
		}

		internal ZString GetCalculationMethodDescription(ZString code) => Lookups.CalculationMethods.GetDescriptionFromCode(code);

		[ReadOnlyMember(nameof(JI_ParentID_ReadOnly))]
		public override ZGuid JI_ParentID
		{
			get { return base.JI_ParentID; }
			set
			{
				var oldValue = JI_ParentID;
				base.JI_ParentID = value;
				if (!IsCopying && oldValue != JI_ParentID)
				{
					InvoiceHeader?.InvalidateJZ_Calc_LinesEnteredCache();
				}
			}
		}

		bool JI_ParentID_ReadOnly
		{
			get { return IsB2OrB3XAdjustments && CA_IsSeeded; }
		}

		public override ZGuid JI_JZ
		{
			get { return base.JI_JZ; }
			set
			{
				var oldValue = JI_JZ;
				var oldInvoiceHeaderSeq = InvoiceHeader?.JZ_InvoiceDisplaySequence ?? new ZShort(0);
				base.JI_JZ = value;
				if (!IsCopying && oldValue != value)
				{
					var dec = Declaration;
					if (dec != null)
					{
						dec.MarkAsNeedingValidation();

						if (dec.IsImport && !dec.IsLVS && InvoiceHeader is JobComInvoiceHeader invoiceHeader)
						{
							DefaultJI_OA_ConsigneeAddress(ZGuid.Empty, invoiceHeader.FinalConsigneeAddress?.RealAddress?.PK ?? ZGuid.Empty);
							if (!((ISupportDataImporting)this).IsImportingData)
							{
								PageNumberCalculator.RecalculatePageNumber(dec, Math.Min(oldInvoiceHeaderSeq, invoiceHeader.JZ_InvoiceDisplaySequence));
							}
						}
					}
				}
			}
		}

		public override ZInt CA_PageNumber
		{
			get
			{
				return base.CA_PageNumber;
			}
			set
			{
				var oldValue = CA_PageNumber;
				base.CA_PageNumber = value;
				if (!IsCopying && oldValue != value && !((ISupportDataImporting)this).IsImportingData)
				{
					if (!IsRecalculatePageNumbersSuspended && !IsLuxuryTaxInvoiceLine && InvoiceHeader is JobComInvoiceHeader invoiceHeader && invoiceHeader.JobDeclaration is JobDeclaration dec && dec.IsImport)
					{
						PageNumberCalculator.RecalculatePageNumber(dec, invoiceHeader.JZ_InvoiceDisplaySequence);
					}
				}
			}
		}

		public IDisposable SuspendRecalculatePageNumbers()
		{
			return new DisposableAction(() => recalculatePageNumbersSuspenderIndex++, () => recalculatePageNumbersSuspenderIndex--);
		}

		public bool IsRecalculatePageNumbersSuspended => recalculatePageNumbersSuspenderIndex > 0;
		byte recalculatePageNumbersSuspenderIndex;

		bool CA_CalculationMethod_ReadOnly
		{
			get
			{
				bool result;
				if (InvoiceHeader == null)
				{
					result = true;
				}
				else
				{
					var dec = InvoiceHeader.FirstAdditionalOrOnlyDeclaration;
					result = dec != null && dec.IsConsolidatedLVS && !dec.CA_AllowOIC;
				}
				return result;
			}
		}

		internal void ResetCalculationMethod(ZString value)
		{
			isResettingCalculationMethod = true;
			CA_CalculationMethod = value;
			isResettingCalculationMethod = false;
		}

		bool isResettingCalculationMethod;

		#region Remission

		internal bool IsRemissionRepairLine => IsRepairLine || IsWarrantyRepairLine || IsDutyDeferralLine || IsSoftwareRemissionLine;
		internal bool IsRemissionRepairLineIncludingParent => RepairRemissions.Contains(CA_CalculationMethod);
		internal static IEnumerable<ZString> RepairRemissions => new List<ZString>() { CalculationMethods.Codes.RepairsRemission, CalculationMethods.Codes.WarrantyRepairsRemission, CalculationMethods.Codes.DutyDeferral, CalculationMethods.Codes.SoftwareRemission };

		internal void AddRemissionLine(ZString calculationMethod)
		{
			JobComInvoiceLine line = null;
			if (InvoiceHeader != null)
			{
				line = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
				line.JI_ParentID = PK;
				line.JI_ParentTableCode = TablePrefix;
				line.JI_LineNo = JI_LineNo + 1;
				line.CA_CalculationMethod = calculationMethod;
				line.RepairLineSynchroniser.Synchronise(true);

				if (calculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission)
				{
					if (ValueForDutyCodes.IsRelatedFirms(CA_ValueForDutyCode))
					{
						line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsResidualMethodValue;
					}
					else if (ValueForDutyCodes.IsUnrelatedFirms(CA_ValueForDutyCode))
					{
						line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsResidualMethodValue;
					}
				}
				else if (calculationMethod == CalculationMethods.Codes.DutyDeferral)
				{
					line.JI_LinePrice = JI_LinePrice * 0.6m;
					RecalculateDutyDeferralPrice(true);
				}
				else if (calculationMethod == CalculationMethods.Codes.SoftwareRemission)
				{
					line.JI_Description = Res.GetString("5BCEF83C-FF97-4C13-AC5A-4ED29AC37CD0", "MEDIA");
				}
			}
		}

		void RecalculateDutyDeferralPrice(bool isAddingDutyDeferralLine)
		{
			if (isAddingDutyDeferralLine)
			{
				JI_LinePrice *= 0.4m;
			}
			else
			{
				JI_LinePrice /= 0.4m;
			}
		}

		bool ShowAddRepairLineConfirmation(string remissionType)
		{
			var caption = Res.GetString("afa900b0-b0ec-4968-a522-20ab254ff41a", "{0} Line Creation", remissionType);
			var message = Res.GetString("1e6010fc-f76b-43cc-8eb1-7f9e08c11cbb", "The System is about to add a {0} line.\r\nYou will be able to override Price and Description for {0} calculations.\r\nDo you want to continue?", remissionType);
			return ShowRemissionLineConfirmation(caption, message);
		}

		bool ShowDeleteRepairLineConfirmation(string remissionType)
		{
			var caption = Res.GetString("66062fde-1f86-4a1c-a0af-62cf03a1ddc7", "{0} Line Deletion", remissionType);
			var message = Res.GetString("b30c11bd-78ed-4232-8771-4edbfd2f736f", "The System is about to delete the {0} line.\r\nDo you want to continue?", remissionType);
			return ShowRemissionLineConfirmation(caption, message);
		}

		bool ShowRemissionLineConfirmation(string caption, string message)
		{
			return Declaration.MessageInitiator.ContinueWithAction(message, caption);
		}

		void DeleteRemissionLine(ZString remissionType)
		{
			if (InvoiceHeader?.InvoiceLines is JobComInvoiceLineViewCollection invoiceLines)
			{
				var repairLine = (from JobComInvoiceLine line in invoiceLines
								  where line.JI_ParentID == PK && line.CA_CalculationMethod == CA_CalculationMethod
								  select line).FirstOrDefault();

				if (repairLine != null)
				{
					repairLine.RepairLineSynchroniser.SetEnabled(false, repairLine.RepairLineSynchroniser.DetectEnabled);
					invoiceLines.RemoveAndDelete(repairLine);

					if (remissionType == CalculationMethods.Codes.DutyDeferral)
					{
						RecalculateDutyDeferralPrice(false);
					}
				}
			}
		}

		void ResetGSTExemptCode(ZString oldCalculationMethod)
		{
			if (oldCalculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission)
			{
				var gstDuties = dutiesAndTaxes.Where(x => x.IsGST);
				foreach (var gst in gstDuties)
				{
					gst.C1_ExemptCode = ((IDutyAndTaxData)this).DefaultGSTStatusCode;
				}
			}
		}

		internal RepairLineSynchroniser RepairLineSynchroniser
		{
			get
			{
				if (IsRemissionRepairLine && repairLineSynchroniser == null)
				{
					var parent = (JobComInvoiceLine)ParentTariffLine;
					if (parent != null)
					{
						repairLineSynchroniser = new RepairLineSynchroniser(this, parent);
					}
				}
				return repairLineSynchroniser;
			}
		}

		RepairLineSynchroniser repairLineSynchroniser;

		internal bool IsRepairLine
		{
			get { return CA_CalculationMethod == CalculationMethods.Codes.RepairsRemission && !JI_ParentID.IsEmpty; }
		}

		bool IsRepairLineParent
		{
			get { return CA_CalculationMethod == CalculationMethods.Codes.RepairsRemission && JI_ParentID.IsEmpty; }
		}

		internal bool IsWarrantyRepairLine => CA_CalculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission && !JI_ParentID.IsEmpty;

		bool IsWarrantyRepairLineParent => CA_CalculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission && JI_ParentID.IsEmpty;

		internal bool IsDutyDeferralLine => CA_CalculationMethod == CalculationMethods.Codes.DutyDeferral && !JI_ParentID.IsEmpty;

		bool IsDutyDeferralLineParent => CA_CalculationMethod == CalculationMethods.Codes.DutyDeferral && JI_ParentID.IsEmpty;

		internal bool IsSoftwareRemissionLine
		{
			get { return CA_CalculationMethod == CalculationMethods.Codes.SoftwareRemission && !JI_ParentID.IsEmpty; }
		}

		internal bool IsRemissionLine
		{
			get
			{
				return new ZString[]
				{
					CalculationMethods.Codes.RegularRemission,
					CalculationMethods.Codes.RepairsRemission,
					CalculationMethods.Codes.OneSixtiethRemission,
					CalculationMethods.Codes.OneOneTwentiethRemission,
					CalculationMethods.Codes.WarrantyRepairsRemission,
					CalculationMethods.Codes.DutyDeferral,
					CalculationMethods.Codes.SoftwareRemission
				}.Contains(CA_CalculationMethod);
			}
		}

		#endregion

		#region ValidateCalculationMethodRelatedProperties

		void ValidateCalculationMethodRelatedProperties()
		{
			if (Declaration != null)
			{
				if (InvoiceHeader != null)
				{
					InvoiceHeader.AddInfoValidation.ValidateCA_TimeLimit();
				}

				AddInfoValidation.ValidateCA_99TariffCode();
			}
		}

		#endregion

		#endregion

		#region CA_CasualImportDestinationProvince

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CanadianProvinces))]
		[ReadOnlyMember(nameof(CA_IsCasualImportValues_ReadOnly))]
		public override ZString CA_CasualImportDestinationProvince
		{
			get
			{
				var result = base.CA_CasualImportDestinationProvince;
				if (result.IsEmpty && InvoiceHeader is JobComInvoiceHeader invoice)
				{
					result = invoice.CA_CasualImportDestinationProvince;
				}

				return result;
			}
			set { base.CA_CasualImportDestinationProvince = value; }
		}

		#endregion

		#region CA_CasualImportCommodity

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CasualImportCommodity))]
		[ReadOnlyMember(nameof(CA_IsCasualImportValues_ReadOnly))]
		public override ZString CA_CasualImportCommodity
		{
			get
			{
				var result = base.CA_CasualImportCommodity;
				if (result.IsEmpty)
				{
					if (InvoiceHeader is JobComInvoiceHeader invoice)
					{
						result = invoice.CA_CasualImportCommodity;
					}
				}

				return result;
			}
			set { base.CA_CasualImportCommodity = value; }
		}

		#endregion

		internal void DefaultCasualImportDestinationProvinceIfNotPreviouslyDefaulted()
		{
			if (!base.CA_IsCasualImport) // only when it was not previously ticked on line level
			{
				DefaultCasualImportDestinationProvinceIfNeeded();
			}
		}

		public ZString DefaultCasualImportDestinationProvince
		{
			get
			{
				if (defaultCasualImportDestinationProvince == null)
				{
					defaultCasualImportDestinationProvince = new CachedProperty<ZString>(Factory, () =>
					{
						var destProvince = ZString.Empty;
						if (ConsigneeAddress is OrgAddress consigneeAddress)
						{
							destProvince = GetCanadianState(consigneeAddress);
							if (destProvince.IsEmpty)
							{
								destProvince = GetCanadianState(consigneeAddress.Header?.MainAddress);
							}
						}
						return destProvince;
					});
				}
				return defaultCasualImportDestinationProvince.Value;
			}
		}
		CachedProperty<ZString> defaultCasualImportDestinationProvince;

		void DefaultCasualImportDestinationProvinceIfNeeded()
		{
			if (ConsigneeAddress != null)
			{
				var destProvince = DefaultCasualImportDestinationProvince;
				if (!destProvince.IsEmpty)
				{
					CA_CasualImportDestinationProvince = destProvince;
				}
			}
			else
			{
				var headerDestProvince = InvoiceHeader?.CA_CasualImportDestinationProvince ?? ZString.Empty;
				if (!headerDestProvince.IsEmpty)
				{
					var baseDestProvince = base.CA_CasualImportDestinationProvince;
					if (!baseDestProvince.IsEmpty && baseDestProvince != headerDestProvince)
					{
						base.CA_CasualImportDestinationProvince = ZString.Empty;
					}
				}
			}
		}

		ZString GetCanadianState(OrgAddress address)
		{
			return address != null && address.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Canada ? address.OA_State.Left(2) : ZString.Empty;
		}

		#region CA_IsCasualImport

		[ReadOnlyMember(nameof(CA_IsCasualImport_ReadOnly))]
		public override ZBool CA_IsCasualImport
		{
			get { return base.CA_IsCasualImport || InvoiceHeader != null && InvoiceHeader.CA_IsCasualImport; }
			set
			{
				var oldValue = CA_IsCasualImport;
				base.CA_IsCasualImport = value;

				if (!IsCopying)
				{
					var newValue = CA_IsCasualImport;
					if (oldValue != newValue)
					{
						if (newValue)
						{
							DefaultCasualImportDestinationProvinceIfNeeded();
						}
						else
						{
							CA_CasualImportCommodity = ZString.Empty;
							CA_CasualImportDestinationProvince = ZString.Empty;
							CA_IsExempt = ZBool.False;
						}
					}
				}
			}
		}

		#endregion

		#region CA_IsExempt

		[ReadOnlyMember(nameof(CA_IsCasualImportValues_ReadOnly))]
		public override ZBool CA_IsExempt
		{
			get { return base.CA_IsExempt; }
			set { base.CA_IsExempt = value; }
		}

		#endregion

		#region CA_RN_NKExport

		[ResourceStringData("CAAddInfo|CA_RN_NKExport", Caption = "Country/Region of Export", ShortCaption = "Ctry/Rgn. of Export", MediumCaption = "Export Country/Region", FullDescription = "The country/region from which the goods were exported for importation into Canada.")]
		public override ZString CA_RN_NKExport
		{
			get { return base.CA_RN_NKExport; }
			set
			{
				var oldValue = CA_RN_NKExport;
				base.CA_RN_NKExport = value;
				if (!IsCopying && oldValue != CA_RN_NKExport)
				{
					if (InvoiceHeader != null && InvoiceHeader.IsImport && CA_RN_NKExport != Core.Constants.CountryCodes.UnitedStates)
					{
						CA_USStateOfExport = ZString.Empty;
					}
					RefreshSIMAMeasuresCollection();
				}
			}
		}

		#endregion

		#region CA_TreatmentCode

		[ResourceStringData("CAAddInfo|CA_TreatmentCode", Caption = "Tariff Treatment Code", ShortCaption = "TT", MediumCaption = "Treatment  Code",
			FullDescription = "A means by which normal rates of duty may be modified according to the Customs Tariff. Refer to the Customs Tariff for information on the applicability of these tariff treatments.")]
		public override ZString CA_TreatmentCode
		{
			get
			{
				var treatmentCode = base.CA_TreatmentCode;

				return IsChildLine && !IsB2OrB3XAdjustments
					? ((JobComInvoiceLine)ParentTariffLine).CA_TreatmentCode
					: (treatmentCode.IsEmpty && InvoiceHeader is JobComInvoiceHeader invoiceHeader ? invoiceHeader.CA_TreatmentCode : treatmentCode);
			}
			set
			{
				var oldValue = CA_TreatmentCode;
				base.CA_TreatmentCode = value;
				if (!IsCopying && oldValue != CA_TreatmentCode)
				{
					if (TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(oldValue)
						!= TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(CA_TreatmentCode))
					{
						AddInfoValidation.ValidateCA_RN_NKExport();
						Validation.ValidateJI_CountryOfOrigin();
					}
				}
				if (InvoiceHeader != null && !IsChildLine)
				{
					foreach (var line in InvoiceHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.JI_ParentID == PK))
					{
						line.CA_TreatmentCodeInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region CA_ADJValue

		[DecimalPlaces(nameof(CA_ADJValueDecimalPlaces))]
		[ReadOnlyMember(nameof(CA_ADJValue_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_ADJValue", Caption = "Adjustment Value", ShortCaption = "Adj.", MediumCaption = "Adjustment", FullDescription = "The amount by which the FOB Value is to be adjusted to get the Value for Currency Conversion to be declared.")]
		public override ZDecimal CA_ADJValue
		{
			get { return base.CA_ADJValue; }
			set { base.CA_ADJValue = value; }
		}

		bool CA_ADJValue_ReadOnly
		{
			get { return CA_ADJCode.IsEmpty; }
		}

		public int CA_ADJValueDecimalPlaces
		{
			get { return CA_ADJCode == AmountTypes.Codes.Dollar ? 2 : 5; }
		}

		#endregion

		#region CA_PageRelativeLineNumber

		[ReadOnlyMember(nameof(CA_PageRelativeLineNumber_ReadOnly))]
		[ResourceStringData("fbc98018-48c9-4070-9c21-b181a18518fc", Caption = "Line Number on Page", ShortCaption = "PLNO", MediumCaption = "LNO on Pg.")]
		public override ZInt CA_PageRelativeLineNumber
		{
			get { return base.CA_PageRelativeLineNumber; }
			set { base.CA_PageRelativeLineNumber = value; }
		}

		#endregion

		#region CA_RemissionType

		[ResourceStringData("CAAddInfo|CA_RemissionType", Caption = "Remission Type")]
		public override ZString CA_RemissionType
		{
			get { return base.CA_RemissionType; }
			set { base.CA_RemissionType = value; }
		}

		#endregion

		#region JI_PartNo

		public override ZString JI_PartNo
		{
			get => base.JI_PartNo;
			set
			{
				base.JI_PartNo = value;

				if (!value.IsEmpty && CA_TradeName.IsEmpty)
				{
					CA_TradeName = value;
				}
			}
		}

		void UpdatePGADetailsFromPivot(CusClassPartPivot pivot)
		{
			var pivotAsIHasPGARequirements = pivot as IHasPGARequirements;
			var classificationAsIHasPGARequirements = pivot.Classification as IHasPGARequirements;

			SetFieldFromPivot((x) => x.OA_Manufacturer, (y) => JI_OA_ManufacturerAddress = y);
			SetFieldFromPivot((x) => x.RN_NKCountryOfOrigin, (y) => JI_CountryOfOrigin = y);
			SetFieldFromPivot((x) => x.RW_NKOriginState, (y) => JI_StateOrRegionOfOrigin = y);
			SetFieldFromPivot((x) => x.RN_NKCountryOfSource, (y) => CA_RN_NKSource = y);
			SetFieldFromPivot((x) => x.RW_NKCountryOfSourceState, (y) => CA_StateOfSource = y);
			SetFieldFromPivot((x) => x.JI_BrandName, (y) => JI_BrandName = y);
			SetFieldFromPivot((x) => x.JI_Model, (y) => JI_Model = y);

			void SetFieldFromPivot<T>(Func<IHasPGARequirements, T> valueGetter, Action<T> setter) where T : IZType
			{
				var pivotValue = valueGetter(pivotAsIHasPGARequirements);
				var classificationValue = classificationAsIHasPGARequirements == null ? default(T) : valueGetter(classificationAsIHasPGARequirements);

				var effectiveValue = pivotValue.IsEmpty ? classificationValue : pivotValue;
				if (!effectiveValue.IsEmpty)
				{
					setter(effectiveValue);
				}
			}

			if (pivot.PGARequirements.Cast<PGARequirement>().Any(x => YesNoList.IsYesOrNo(x.Indicator)))
			{
				PGARequirements.CopyPersistentValuesFrom(pivot.PGARequirements);
			}
		}

		void UpdatePGADetailsFromClassification(CusClassification classification)
		{
			var classManufacturer = classification.CCA_OA_Manufacturer;
			if (!classManufacturer.IsEmpty)
			{
				JI_OA_ManufacturerAddress = classManufacturer;
			}

			var classOrigin = classification.CCA_RN_NKOrigin;
			if (!classOrigin.IsEmpty)
			{
				JI_CountryOfOrigin = classOrigin;
			}

			var classOriginProvince = classification.CCA_ProvinceOfOrigin;
			if (!classOriginProvince.IsEmpty)
			{
				JI_StateOrRegionOfOrigin = classOriginProvince;
			}

			var classSourceCountry = classification.CCA_RN_NKSource;
			if (!classSourceCountry.IsEmpty)
			{
				CA_RN_NKSource = classSourceCountry;
			}

			var classSourceState = classification.CCA_StateOfSource;
			if (!classSourceState.IsEmpty)
			{
				CA_StateOfSource = classSourceState;
			}

			var brandName = classification.CCA_BrandName;
			if (!brandName.IsEmpty)
			{
				JI_BrandName = brandName;
			}

			var model = classification.CCA_Model;
			if (!model.IsEmpty)
			{
				JI_Model = model;
			}

			if (IsIIDDeclaration && classification.PGARequirements.Cast<PGARequirement>().Any(x => YesNoList.IsYesOrNo(x.Indicator)))
			{
				PGARequirements.CopyPersistentValuesFrom(classification.PGARequirements);
			}
		}

		void UpdateDutiesAndTaxesFromProduct(CusClassPartPivot pivot)
		{
			if (CA_SIMADumpingNum.IsEmpty || !pivot.CCA_SIMADumpingNumber.IsEmpty)
			{
				CA_SIMADumpingNum = pivot.CCA_SIMADumpingNumber;
			}

			var dutiesAndTaxesInPivot = pivot.Classification == null ? pivot.DutiesAndTaxes.ToArray() : pivot.DutiesAndTaxesForCC.Cast<DutyAndTax>().ToArray();
			if (dutiesAndTaxesInPivot.Any())
			{
				var query = DutyAndTaxCollection.GetQuery(TablePrefix);
				query.AddToFilter(CusAddInfoSchema.B7_ParentID, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				var dutiesAndTaxesList = Factory.Load<DutyAndTax>(query).ToList(); // need to load it from factory as ActiveBusinessCollection doesn't show it correctly.
				foreach (var dutyInProduct in dutiesAndTaxesInPivot)
				{
					var shouldCloneDuty = false;
					var matchedDuty = dutiesAndTaxesList.FirstOrDefault(x => x.C1_TaxType == dutyInProduct.C1_TaxType);
					if (matchedDuty == null)
					{
						shouldCloneDuty = true;
					}
					else if (!matchedDuty.C1_Override && dutyInProduct.C1_Override)
					{
						dutiesAndTaxesList.Remove(matchedDuty);
						matchedDuty.Delete();
						shouldCloneDuty = true;
					}

					if (shouldCloneDuty)
					{
						var clonedDuty = (DutyAndTax)dutyInProduct.Clone();
						clonedDuty.C1_Amount = ZDecimal.Zero;
						clonedDuty.C1_PreviousTranNumber = ZString.Empty;
						clonedDuty.C1_PreviousTranLine = ZInt.Zero;
						clonedDuty.C1_ExemptCode = ZString.Empty;
						DutiesAndTaxes.Add(clonedDuty);
						dutiesAndTaxesList.Add(clonedDuty);
						if (clonedDuty.C1_Rate.IsEmpty && !clonedDuty.C1_ForeignRate.IsEmpty)
						{
							clonedDuty.CalculateRateFromForeighRate();
						}
						clonedDuty.C1_ExemptCode = dutyInProduct.C1_ExemptCode;
						clonedDuty.C1_Code = dutyInProduct.C1_Code;
					}
				}
			}
		}

		void UpdateDutiesAndTaxesFromCusClassification(CusClassification classification)
		{
			if (!AreClassificationDetailsBeingUpdated)
			{
				if (CA_SIMADumpingNum.IsEmpty || !classification.CCA_SIMADumpingNumber.IsEmpty || classification.SIMAMeasures.Count == 0)
				{
					CA_SIMADumpingNum = classification.CCA_SIMADumpingNumber;
				}

				if (classification.DutiesAndTaxes.Count > 0)
				{
					var query = DutyAndTaxCollection.GetQuery(TablePrefix);
					query.AddToFilter(CusAddInfoSchema.B7_ParentID, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					var dutiesAndTaxesList = Factory.Load<DutyAndTax>(query).ToList(); // need to load it from factory as ActiveBusinessCollection doesn't show it correctly.
					foreach (var dutyInProduct in classification.DutiesAndTaxes)
					{
						var shouldCloneDuty = false;
						var matchedDuty = dutiesAndTaxesList.FirstOrDefault(x => x.C1_TaxType == dutyInProduct.C1_TaxType);
						if (matchedDuty == null)
						{
							shouldCloneDuty = true;
						}
						else if (!matchedDuty.C1_Override && dutyInProduct.C1_Override)
						{
							dutiesAndTaxesList.Remove(matchedDuty);
							matchedDuty.Delete();
							shouldCloneDuty = true;
						}

						if (shouldCloneDuty)
						{
							var clonedDuty = (DutyAndTax)dutyInProduct.Clone();
							clonedDuty.C1_Amount = ZDecimal.Zero;
							clonedDuty.C1_PreviousTranNumber = ZString.Empty;
							clonedDuty.C1_PreviousTranLine = ZInt.Zero;
							clonedDuty.C1_ExemptCode = ZString.Empty;
							DutiesAndTaxes.Add(clonedDuty);
							dutiesAndTaxesList.Add(clonedDuty);
							if (clonedDuty.C1_Rate.IsEmpty && !clonedDuty.C1_ForeignRate.IsEmpty)
							{
								clonedDuty.CalculateRateFromForeighRate();
							}
							clonedDuty.C1_ExemptCode = dutyInProduct.C1_ExemptCode;
						}
					}
				}
			}
		}

		#endregion

		#region JI_Model

		[ReadOnlyMember(nameof(JI_Model_ReadOnly))]
		public override ZString JI_Model
		{
			get => base.JI_Model;
			set => base.JI_Model = value;
		}

		bool JI_Model_ReadOnly
		{
			get
			{
				if (cachedModel_ReadOnly == null)
				{
					cachedModel_ReadOnly = new CachedProperty<bool>(Factory, () =>
					{
						var dfoPAGHeader = DFOPGAHeader;
						if (dfoPAGHeader != null && dfoPAGHeader.CA_TTPProgramInd == YesNoList.Codes.Yes)
						{
							return !string.IsNullOrWhiteSpace(dfoPAGHeader.CA_CommonNameCode);
						}

						return false;
					});
				}

				return cachedModel_ReadOnly.Value;
			}
		}
		CachedProperty<bool> cachedModel_ReadOnly;

		#endregion

		public override bool UseImportClassification
		{
			get { return base.UseImportClassification || IsB2OrB3XAdjustments; }
		}

		public override ZString JI_CustomsUnitQty
		{
			get { return base.JI_CustomsUnitQty; }
			set { base.JI_CustomsUnitQty = value.ToUpper(); }
		}

		public override ZString JI_InvoiceUQ
		{
			get { return base.JI_InvoiceUQ; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					base.JI_InvoiceUQ = value.ToUpper();
				}
			}
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(invoiceLine => (ITariff)invoiceLine.Tariff, defaultFirstUnitOnly: true);

		public override void SetInvoiceUQWhenPartNoChanged(ZString newUQ)
		{
			if (IsIIDDeclaration)
			{
				base.JI_InvoiceUQ = CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits(newUQ, Factory, true);
			}
			else
			{
				base.SetInvoiceUQWhenPartNoChanged(newUQ);
			}
		}

		#region JI_Tariff

		[MaxLength(15)]
		public override ZString JI_Tariff
		{
			get { return base.JI_Tariff; }
			set
			{
				var oldValue = JI_Tariff;
				base.JI_Tariff = value;
				if (!IsCopying && oldValue != JI_Tariff)
				{
					SetAVSStatusIfNeeded();
					RefreshDutiesAndTaxesWhenTariffChangedIfNeeded(AreClassificationDetailsBeingUpdated);
					RefreshSIMAMeasuresCollection();

					if (IsIIDDeclaration)
					{
						PGARequirements.SetDefaultValueForIndicatorWhenTariffChanged();
					}
				}
			}
		}

		protected override IDisposable OnUpdatingDetailsFromPart()
		{
			return new DisposableAction(() =>
			{
				disableSIMAMeasuresCollectionRefresh = true;
				needToRefreshSIMAMeasuresCollection = false;
			}, () =>
			{
				disableSIMAMeasuresCollectionRefresh = false;
				if (needToRefreshSIMAMeasuresCollection && !ProductHasSIMAData())
				{
					RefreshSIMAMeasuresCollection();
				}
			});
		}

		bool ProductHasSIMAData()
		{
			var result = false;
			if (Pivot is CusClassPartPivot pivot)
			{
				result = !pivot.CCA_SIMADumpingNumber.IsEmpty
					|| pivot.DutiesAndTaxes.Any(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType));
			}
			return result;
		}
		bool disableSIMAMeasuresCollectionRefresh;
		bool needToRefreshSIMAMeasuresCollection;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal void RefreshSIMAMeasuresCollection()
		{
			if (!AreClassificationDetailsBeingUpdated && !disableSIMAMeasuresCollectionRefresh && !disableSIMAMeasuresCollectionRefreshForCusClassification)
			{
				SIMAMeasures.RemoveAndDeleteAll();
				CA_SIMADumpingNum = ZString.Empty;
				DutyAndTaxManager.RemoveNotOverriddenSIMADuties();

				if (!HasSIMADuty)
				{
					if (IsImportIncludingB2 && !JI_Tariff.IsEmpty)
					{
						var dumpingNumbers = new List<Tuple<ZString, ZString>>();
						if (!JI_CountryOfOrigin.IsEmpty)
						{
							var numbersFromOriginCountry = DutyAndTaxManager.GetSIMADumpingNumbersFromTariffAndCountry(Factory, JI_Tariff, JI_CountryOfOrigin, EffectiveDateForDutyRate);
							if (numbersFromOriginCountry != null && numbersFromOriginCountry.Count > 0)
							{
								dumpingNumbers = numbersFromOriginCountry;
							}
						}

						if (dumpingNumbers.Count == 0)
						{
							var countryOfExport = CA_RN_NKExport.IsEmpty && InvoiceHeader != null ? InvoiceHeader.CA_RN_NKExport : CA_RN_NKExport;
							if (!countryOfExport.IsEmpty)
							{
								var numbersFromExportCountry = DutyAndTaxManager.GetSIMADumpingNumbersFromTariffAndCountry(Factory, JI_Tariff, countryOfExport, EffectiveDateForDutyRate);
								if (numbersFromExportCountry != null && numbersFromExportCountry.Count > 0)
								{
									dumpingNumbers = numbersFromExportCountry;
								}
							}
						}

						if (dumpingNumbers.Count > 0)
						{
							dumpingNumbers.ForEach(d => SIMAMeasures.AddNew(d.Item1, d.Item2));
						}
					}

					SIMADumpingNumber dumpingToDefault = null;
					if (SIMAMeasures.Count == 1)
					{
						dumpingToDefault = SIMAMeasures[0];
					}
					else if (SIMAMeasures.Count > 1 && OnRefreshSIMAMeasureEvent != null && !((ISupportDataImporting)this).IsImportingData)
					{
						dumpingToDefault = OnRefreshSIMAMeasureEvent();
					}

					if (dumpingToDefault != null)
					{
						CA_SIMADumpingNum = dumpingToDefault.CA_DumpingNumber;
					}

					DutyAndTaxManager.AddSIMADutyIfNotExists();
				}
			}
			else
			{
				needToRefreshSIMAMeasuresCollection = true;
			}
		}
		public Func<SIMADumpingNumber> OnRefreshSIMAMeasureEvent;

		internal ZBool ShouldRefreshSIMAMeasuresFromInvoiceHeader
		{
			get { return base.JI_CountryOfOrigin.IsEmpty; }
		}

		void RefreshDutiesAndTaxesWhenTariffChangedIfNeeded(bool shouldRefresh = true)
		{
			if (shouldRefresh && IsImport && !AreClassificationDetailsBeingUpdated)
			{
				DutyAndTaxManager.RemoveNotOverriddenDutiesAndTaxes();
				dutyAndTaxManager.PopulateDutiesAndTaxes();
			}
		}

		protected override ZDateTime UniversalTariffValuationDate => EffectiveDateForDutyRate;

		public override ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected override ZString EffectivePrimaryPreferenceCore => CA_TreatmentCode;

		public override bool ShouldWipeNKTaxType => false;

		#endregion

		#region CFIA Properties

		#region CA_RequirementID

		public override ZString CA_RequirementID
		{
			get { return base.CA_RequirementID; }
			set
			{
				var oldValue = CA_RequirementID;
				base.CA_RequirementID = value;
				if (!IsCopying && oldValue != CA_RequirementID)
				{
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region CA_RequirementVer

		public override ZString CA_RequirementVer
		{
			get { return base.CA_RequirementVer; }
			set
			{
				var oldValue = CA_RequirementVer;
				base.CA_RequirementVer = value;
				if (!IsCopying && oldValue != CA_RequirementVer)
				{
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region CA_AirsCode

		public override ZString CA_AirsCode
		{
			get { return base.CA_AirsCode; }
			set
			{
				var oldValue = CA_AirsCode;
				base.CA_AirsCode = value;
				if (!IsCopying && oldValue != CA_AirsCode)
				{
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region CA_DestinationProvince

		public override ZString CA_DestinationProvince
		{
			get { return base.CA_DestinationProvince; }
			set
			{
				var oldValue = CA_DestinationProvince;
				base.CA_DestinationProvince = value;
				if (!IsCopying && oldValue != CA_DestinationProvince)
				{
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region CA_EndUse

		public override ZString CA_EndUse
		{
			get { return base.CA_EndUse; }
			set
			{
				var oldValue = CA_EndUse;
				base.CA_EndUse = value;
				if (!IsCopying && oldValue != CA_EndUse)
				{
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region CA_MiscID

		public override ZString CA_MiscID
		{
			get { return base.CA_MiscID; }
			set
			{
				var oldValue = CA_MiscID;
				base.CA_MiscID = value;
				if (!IsCopying && oldValue != CA_MiscID)
				{
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region CA_RN_NKCFIAOrigin

		public override ZString CA_RN_NKCFIAOrigin
		{
			get { return base.CA_RN_NKCFIAOrigin; }
			set
			{
				var oldValue = CA_RN_NKCFIAOrigin;
				base.CA_RN_NKCFIAOrigin = value;
				if (!IsCopying && oldValue != CA_RN_NKCFIAOrigin)
				{
					if (InvoiceHeader != null && InvoiceHeader.IsImport && CA_RN_NKCFIAOrigin != Core.Constants.CountryCodes.UnitedStates)
					{
						CA_CFIAUSStateOfOrigin = ZString.Empty;
					}
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region CA_CFIAUSStateOfOrigin

		[ReadOnlyMember(nameof(CA_CFIAUSStateOfOrigin_ReadOnly))]
		public override ZString CA_CFIAUSStateOfOrigin
		{
			get { return base.CA_CFIAUSStateOfOrigin; }
			set
			{
				var oldValue = CA_CFIAUSStateOfOrigin;
				base.CA_CFIAUSStateOfOrigin = value;
				if (!IsCopying && oldValue != CA_CFIAUSStateOfOrigin)
				{
					SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		public bool IsRegulatedByCFIA
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsPersistent && declaration.IsOGD &&
					!JI_Tariff.IsEmpty && IsHSCodeContainsOGDCFIA;
			}
		}

		public bool IsRegulatedByIIDCFIA
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsPersistent && declaration.IsIID && !JI_Tariff.IsEmpty && CA_CFIAInd == YesNoList.Codes.Yes;
			}
		}

		internal void SetAVSStatusIfNeeded()
		{
			if (IsRegulatedByCFIA || IsRegulatedByIIDCFIA)
			{
				CA_OGDStatus = AVSStatusList.Codes.NotValidated;
				Declaration.AVSEventRequired = true;
			}
			else if (CA_OGDStatus != AVSStatusList.Codes.Blank)
			{
				CA_OGDStatus = AVSStatusList.Codes.Blank;
			}
		}

		IEnumerable<ZPropertyInfo> OGDCFIAFieldsPropertyInfos
		{
			get
			{
				var result = new List<ZPropertyInfo>();
				result.AddRange(new[] { CA_RequirementIDInfo, CA_RequirementVerInfo, CA_AirsCodeInfo, CA_DestinationProvinceInfo, CA_EndUseInfo, CA_MiscIDInfo, CA_RN_NKCFIAOriginInfo, CA_CFIAUSStateOfOriginInfo });
				return result;
			}
		}

		public bool IsOGDCFIABlank => OGDCFIAFieldsPropertyInfos.All(x => x.Value.IsEmpty)
				&& CFIARegistrationNumbers.Count == 0;

		#endregion

		[ResourceStringData("JobComInvoiceLine|JI_CustomsSecondUnitQty", Caption = "Customs UQ 2", ShortCaption = "UQ 2", MediumCaption = "Cust. UQ 2", FullDescription = "Second Customs Unit Quantity (Statistical) as indicated by the tariff item entered.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString JI_CustomsSecondUnitQty
		{
			get { return base.JI_CustomsSecondUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsSecondUnitQty))
				{
					var oldValue = JI_CustomsSecondUnitQty;
					base.JI_CustomsSecondUnitQty = value.ToUpper();
					if (IsImport && oldValue != JI_CustomsSecondUnitQty && !IsCopying
						&& !JI_CustomsSecondUnitQty.IsEmpty && JI_CustomsSecondQuantity.IsEmpty
						&& !JI_CustomsUnitQty.IsEmpty && !JI_CustomsQuantity.IsEmpty)
					{
						JI_CustomsSecondQuantity = DutyAndTaxUnitConverter.GetConvertedQuantity(JI_CustomsQuantity, JI_CustomsUnitQty, JI_CustomsSecondUnitQty);
					}
				}
			}
		}

		[ResourceStringData("JobComInvoiceLine|JI_CustomsThirdUnitQty", Caption = "Customs UQ 3", ShortCaption = "UQ 3", MediumCaption = "Cust. UQ 3", FullDescription = "Third Customs Unit Quantity (Statistical) as indicated by the tariff item entered.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString JI_CustomsThirdUnitQty
		{
			get { return base.JI_CustomsThirdUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsThirdUnitQty))
				{
					var oldValue = JI_CustomsThirdUnitQty;
					base.JI_CustomsThirdUnitQty = value.ToUpper();
					if (IsImport && oldValue != JI_CustomsThirdUnitQty && !IsCopying
						&& !JI_CustomsThirdUnitQty.IsEmpty && JI_CustomsThirdQuantity.IsEmpty
						&& !JI_CustomsUnitQty.IsEmpty && !JI_CustomsQuantity.IsEmpty)
					{
						JI_CustomsThirdQuantity = DutyAndTaxUnitConverter.GetConvertedQuantity(JI_CustomsQuantity, JI_CustomsUnitQty, JI_CustomsThirdUnitQty);
					}
				}
			}
		}

		[DecimalPlaces(3)]
		public override ZDecimal JI_CustomsQuantity
		{
			get { return base.JI_CustomsQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsQuantity))
				{
					var oldValue = JI_CustomsQuantity;
					base.JI_CustomsQuantity = value;
					if (oldValue != JI_CustomsQuantity && !IsCopying && !JI_CustomsUnitQty.IsEmpty && !JI_CustomsQuantity.IsEmpty)
					{
						if (!JI_CustomsSecondUnitQty.IsEmpty)
						{
							JI_CustomsSecondQuantity = DutyAndTaxUnitConverter.GetConvertedQuantity(JI_CustomsQuantity, JI_CustomsUnitQty, JI_CustomsSecondUnitQty);
						}

						if (!JI_CustomsThirdUnitQty.IsEmpty)
						{
							JI_CustomsThirdQuantity = DutyAndTaxUnitConverter.GetConvertedQuantity(JI_CustomsQuantity, JI_CustomsUnitQty, JI_CustomsThirdUnitQty);
						}
					}
				}
			}
		}

		protected override ZBool ShouldReCalculateCustomsQtyOnLineQuantityChange => !IsRemissionRepairLine;

		public override ZString CustomsUQ
		{
			get { return Tariff != null ? Tariff.TariffUnits : ZString.Empty; }
		}

		protected override bool IsValidForLineTotalCalculation
		{
			get { return !IsRepairLineParent && !IsWarrantyRepairLine && !IsWarrantyRepairLineParent; }
		}

		public override bool IsContainerLinkMandatory
		{
			get { return false; }
		}

		public override bool NeedsCustomsQuantity => !IsWarrantyRepairLine && base.NeedsCustomsQuantity;

		public bool HasABVInUnits => JI_CustomsUnitQty == CustomsUnitOfMeasureList.Codes.AlcoholByVolume
						|| JI_CustomsSecondUnitQty == CustomsUnitOfMeasureList.Codes.AlcoholByVolume
						|| JI_CustomsThirdUnitQty == CustomsUnitOfMeasureList.Codes.AlcoholByVolume;

		#region Part Synchronization process members

		protected override OrgHeader ImporterCore
		{
			get
			{
				var lvsShipment = GetLVSShipment();
				return lvsShipment != null ? lvsShipment.Importer_Effective : base.ImporterCore;
			}
		}

		public override OrgHeader Supplier
		{
			get
			{
				var lvsShipment = GetLVSShipment();
				return lvsShipment != null ? lvsShipment.Supplier_Effective : base.Supplier;
			}
		}

		JobComInvoiceHeader GetLVSShipment()
		{
			JobComInvoiceHeader result = null;
			var declaration = !IsDeleted ? Declaration : null;
			if (declaration != null && !declaration.IsDeleted && declaration.IsLVS)
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null && !invoiceHeader.IsDeleted)
				{
					result = invoiceHeader;
				}
			}
			return result;
		}

		#endregion

		[ResourceStringData("CAAddInfo|CA_ValueForDutyCode", Caption = "Value For Duty Code", ShortCaption = "VFD", MediumCaption = "VFD Code",
			FullDescription = "A code used to indicate the basis on which the value for duty was determined. The first digit is the relationship, while the second digit is the valuation method used.")]
		public override ZString CA_ValueForDutyCode
		{
			get
			{
				var valueForDutyCode = base.CA_ValueForDutyCode;
				return valueForDutyCode.IsEmpty && InvoiceHeader != null ? InvoiceHeader.CA_ValueForDutyCode : valueForDutyCode;
			}
			set { base.CA_ValueForDutyCode = value; }
		}

		[ResourceStringData("JobComInvoiceLine|JI_CustomsSecondQuantity", Caption = "Customs Qty 2", ShortCaption = "Qty 2", FullDescription = "Customs Quantity as indicated by the Tariff item entered.")]
		[DecimalPlaces(3)]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get { return base.JI_CustomsSecondQuantity; }
			set { base.JI_CustomsSecondQuantity = value; }
		}

		[ResourceStringData("JobComInvoiceLine|JI_CustomsThirdQuantity", Caption = "Customs Qty 3", ShortCaption = "Qty 3", FullDescription = "Customs Quantity as indicated by the Tariff item entered.")]
		[DecimalPlaces(3)]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get { return base.JI_CustomsThirdQuantity; }
			set { base.JI_CustomsThirdQuantity = value; }
		}

		[ResourceStringData("CAAddInfo|CA_TRSNumber", Caption = "TRS Number", ShortCaption = "TRS #", FullDescription = "Technical Reference System ruling number.")]
		public override ZString CA_TRSNumber
		{
			get { return base.CA_TRSNumber; }
			set { base.CA_TRSNumber = value; }
		}

		[ResourceStringData("CAAddInfo|CA_99TariffCode", Caption = "Tariff Code", ShortCaption = "Tariff", FullDescription = "Applicable if the conditions specified in the Chapter 99 (special classification provisions) tariff item apply.")]
		public override ZString CA_99TariffCode
		{
			get { return base.CA_99TariffCode; }
			set
			{
				if (value != base.CA_99TariffCode)
				{
					base.CA_99TariffCode = value;
					if (value == DutyAndTaxManager.A99TariffCode0017)
					{
						CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
					}
					ResetCalculationMethod();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AuthorityNumberList))]
		[ReadOnlyMember(nameof(CA_AuthorityNumber_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_AuthorityNumber", Caption = "Special Authority/Permit", ShortCaption = "Auth/Pmt", MediumCaption = "Special Auth/Pmt",
			FullDescription = "A permit number or Order in Council (OIC) authorization number to import goods under special conditions.")]
		public override ZString CA_AuthorityNumber
		{
			get { return base.CA_AuthorityNumber; }
			set
			{
				if (value != base.CA_AuthorityNumber)
				{
					base.CA_AuthorityNumber = value;
					needReloadRuling = true;
					RulingDescriptionInfo.RefreshBinding();

					if (!IsRemissionRepairLine)
					{
						DefaultRemissionType(Ruling);
					}

					SyncConfigurationsFromRuling();
					ResetCalculationMethod();
				}
			}
		}

		void ResetCalculationMethod()
		{
			if ((Declaration?.IsLVX ?? false) && CA_99TariffCode.IsEmpty && CA_AuthorityNumber.IsEmpty)
			{
				CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			}
		}

		void DefaultRemissionType(ZZRefCusRulingCombined cusRuling)
		{
			if (cusRuling != null)
			{
				if (CA_CalculationMethod.IsEmpty || CA_CalculationMethod == CalculationMethods.Codes.NoRemission)
				{
					CA_CalculationMethod = cusRuling.ZZX_RulingType.Left(CA_CalculationMethodInfo.MaxLength);
				}
			}
		}

		void SyncConfigurationsFromRuling()
		{
			RulingConfigurations.RemoveAndDeleteAll();

			var configs = Ruling?.Configurations.Cast<CusRulingConfigCombined>() ?? Enumerable.Empty<CusRulingConfigCombined>();

			foreach (var config in configs.Where(x => x.ZZY_Category != RefCusRulingConfigCategories.Codes.DAT))
			{
				RulingConfigurations.AddNew(config.ZZY_Category, config.ZZY_Type, config.ZZY_Rate, config.ZZY_Value);
			}
		}

		[ResourceStringData("CAAddInfo|CA_CustomsValueOvr", Caption = "Override", ShortCaption = "Ovr.", FullDescription = "Checking this box allows manual override of the Value for Duty.")]
		public override ZBool CA_CustomsValueOvr
		{
			get { return base.CA_CustomsValueOvr; }
			set { base.CA_CustomsValueOvr = value; }
		}

		[ReadOnlyMember(nameof(CA_CustomsValue_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_CustomsValue", Caption = "Value for Duty", ShortCaption = "Cust. Value", FullDescription = "Customs Value for current line item converted to CAD.")]
		public override ZDecimal CA_CustomsValue
		{
			get { return base.CA_CustomsValue; }
			set
			{
				var oldValue = CA_CustomsValue;
				base.CA_CustomsValue = value;
				if (oldValue != CA_CustomsValue && InvoiceHeader != null)
				{
					InvoiceHeader.Validation.ValidateTotalValueForDuty();
					InvoiceHeader.TotalValueForDutyInfo.RefreshBinding();
				}
			}
		}

		[ReadOnlyMember(nameof(CA_USStateOfExport_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_USStateOfExport", Caption = "US State of Export", ShortCaption = "State", MediumCaption = "Exp. State", FullDescription = "The US state code if the Country/Region of Export is US.")]
		public override ZString CA_USStateOfExport
		{
			get { return base.CA_USStateOfExport; }
			set { base.CA_USStateOfExport = value; }
		}

		public override ZString CA_CFIACountryOfSource
		{
			get { return base.CA_CFIACountryOfSource; }
			set
			{
				base.CA_CFIACountryOfSource = value;
				if (CA_CFIAStateOfSource_ReadOnly)
				{
					CA_CFIAStateOfSource = ZString.Empty;
				}
			}
		}

		#region Bonded Warehouse

		[ResourceStringData("CAAddInfo|JI_PreviousEntryNumber", Caption = "Previous Trans. Number", ShortCaption = "Prev. Tran. #", FullDescription = "Previous Transaction Number")]
		public override ZString JI_PreviousEntryNumber
		{
			get { return base.JI_PreviousEntryNumber; }
			set
			{
				EntryLineCodeParser parser = new EntryLineCodeParser(value);
				if (parser.IsCompleteCode)
				{
					base.JI_PreviousEntryNumber = parser.EntryNumber;
					JI_PreviousEntryLineNumber = parser.LineNumber;
				}
				else
				{
					base.JI_PreviousEntryNumber = value;
				}
			}
		}

		[ResourceStringData("CAAddInfo|JI_PreviousEntryLineNumber", Caption = "Previous Trans. Line Number", ShortCaption = "PTLN", FullDescription = "Previous Transaction Line Number")]
		public override ZShort JI_PreviousEntryLineNumber
		{
			get { return base.JI_PreviousEntryLineNumber; }
			set { base.JI_PreviousEntryLineNumber = value; }
		}

		internal void ClearBondedWarehouseFieldsIfNeeded()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				if (!declaration.IsExWarehouseEntry)
				{
					JI_PreviousEntryNumber = ZString.Empty;
					JI_PreviousEntryLineNumber = ZShort.Zero;
				}
			}
		}

		#endregion

		public override ZString CA_HCInd
		{
			get { return base.CA_HCInd; }
			set
			{
				var oldValue = CA_HCInd;
				base.CA_HCInd = value;
				if (oldValue != CA_HCInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_HCInd))
					{
						pgaHeaders.Delete<HCPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		public override ZString CA_PHACInd
		{
			get { return base.CA_PHACInd; }
			set
			{
				var oldValue = CA_PHACInd;
				base.CA_PHACInd = value;
				if (oldValue != CA_PHACInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_PHACInd))
					{
						pgaHeaders.Delete<PHACPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		public override ZString CA_NRCanInd
		{
			get { return base.CA_NRCanInd; }
			set
			{
				var oldValue = CA_NRCanInd;
				base.CA_NRCanInd = value;
				if (oldValue != CA_NRCanInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_NRCanInd))
					{
						pgaHeaders.Delete<NRCanPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		public override ZString CA_DFOInd
		{
			get { return base.CA_DFOInd; }
			set
			{
				var oldValue = CA_DFOInd;
				base.CA_DFOInd = value;
				if (oldValue != CA_DFOInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_DFOInd))
					{
						pgaHeaders.Delete<DFOPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		public override ZString CA_GACInd
		{
			get { return base.CA_GACInd; }
			set
			{
				var oldValue = CA_GACInd;
				base.CA_GACInd = value;
				if (oldValue != CA_GACInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_GACInd))
					{
						pgaHeaders.Delete<GACPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		public override ZString CA_CFIAInd
		{
			get { return base.CA_CFIAInd; }
			set
			{
				var oldValue = CA_CFIAInd;
				base.CA_CFIAInd = value;
				if (oldValue != CA_CFIAInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_CFIAInd))
					{
						pgaHeaders.Delete<CFIAPGAHeader>();
					}
					if (YesNoList.IsYes(CA_CFIAInd))
					{
						if (CA_RN_NKSource.IsEmpty)
						{
							CA_RN_NKSource = JI_CountryOfOrigin;
						}
						if (CA_StateOfSource.IsEmpty && !CA_StateOfSource_ReadOnly)
						{
							CA_StateOfSource = JI_StateOrRegionOfOrigin.Left(2);
						}
						RefreshBindingForDeclaredPGAHeaders();
					}
					RefreshInvoiceLinesWithPGAs();
					SetAVSStatusIfNeeded();
				}
			}
		}

		public override ZString CA_CNSCInd
		{
			get { return base.CA_CNSCInd; }
			set
			{
				var oldValue = CA_CNSCInd;
				base.CA_CNSCInd = value;
				if (oldValue != CA_CNSCInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_CNSCInd))
					{
						pgaHeaders.Delete<CNSCPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		public override ZString CA_ECCCInd
		{
			get { return base.CA_ECCCInd; }
			set
			{
				var oldValue = CA_ECCCInd;
				base.CA_ECCCInd = value;
				if (oldValue != CA_ECCCInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_ECCCInd))
					{
						pgaHeaders.Delete<ECCCPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		public override ZString CA_TCInd
		{
			get { return base.CA_TCInd; }
			set
			{
				var oldValue = CA_TCInd;
				base.CA_TCInd = value;
				if (oldValue != CA_TCInd && !IsCopying)
				{
					if (YesNoList.IsYesOrNo(oldValue) && !YesNoList.IsYesOrNo(CA_TCInd))
					{
						pgaHeaders.Delete<TCPGAHeader>();
					}
					RefreshInvoiceLinesWithPGAs();
				}
			}
		}

		[ResourceStringData("CAAddInfo|CA_IIDRegion", Caption = "Region of Origin", ShortCaption = "Region", MediumCaption = "Region")]
		public override ZString CA_IIDRegion
		{
			get
			{
				var region = base.CA_IIDRegion;
				return region.IsEmpty ? (InvoiceHeader?.CA_IIDRegion ?? ZString.Empty) : region;
			}
			set
			{
				base.CA_IIDRegion = value;
			}
		}

		protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore()
		{
			return new InvoiceLineCusLinkPackageCollection(this);
		}

		public override ZString CA_RN_NKSource
		{
			get
			{
				var result = base.CA_RN_NKSource;
				return result.IsEmpty ? (InvoiceHeader?.CA_RN_NKSource ?? ZString.Empty) : result;
			}
			set
			{
				var oldValue = CA_RN_NKSource;
				base.CA_RN_NKSource = value;
				if (!IsCopying && oldValue != CA_RN_NKSource)
				{
					SetAVSStatusIfNeeded();
					if (!CA_StateOfSource.IsEmpty && CA_StateOfSource_ReadOnly)
					{
						CA_StateOfSource = ZString.Empty;
					}
					RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		[ReadOnlyMember(nameof(CA_StateOfSource_ReadOnly))]
		public override ZString CA_StateOfSource
		{
			get
			{
				var result = base.CA_StateOfSource;
				if (result.IsEmpty)
				{
					var invoiceSourceCountry = InvoiceHeader?.CA_RN_NKSource ?? ZString.Empty;
					if (invoiceSourceCountry == CA_RN_NKSource)
					{
						result = InvoiceHeader?.CA_StateOfSource ?? ZString.Empty;
					}
				}

				return result;
			}
			set
			{
				var oldValue = CA_StateOfSource;
				base.CA_StateOfSource = value;
				if (!IsCopying && oldValue != CA_StateOfSource)
				{
					SetAVSStatusIfNeeded();
					RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		internal bool CA_StateOfSource_ReadOnly => CA_RN_NKSource != Core.Constants.CountryCodes.UnitedStates;

		[ReadOnlyMember(nameof(CA_CFIAStateOfSource_ReadOnly))]
		public override ZString CA_CFIAStateOfSource
		{
			get => base.CA_CFIAStateOfSource;
			set => base.CA_CFIAStateOfSource = value;
		}

		#endregion

		#region Methods

		protected override bool AllowParentTariffLineAndThisLineHavingDifferentHeader
		{
			get { return IsB2OrB3XAdjustments; }
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (IsImportIncludingB2)
			{
				OnMarkApportionmentDirty += JobComInvoiceLine_OnMarkApportionmentDirty;
			}
		}

		protected override void RefreshDetailsAfterPartInitialisation()
		{
			base.RefreshDetailsAfterPartInitialisation();
			if (IsLuxuryTaxInvoiceLine)
			{
				LuxuryTaxInvoiceLineSynchroniser.Synchronise(true);
			}
		}

		protected override ZDecimal GetGSTVATAmountCore()
		{
			return !IsImport
				? base.GetGSTVATAmountCore()
				: DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST);
		}

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return (!IsExport && JI_CustomsUnitQty.IsEmpty) || IsB2AsAccountForSeededLine;
		}

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return JI_Tariff.IsEmpty || (Tariff != null && !Tariff.TariffUnits.IsEmpty && !CACustomsDataRegistry.Instance.EnableDebugHooks.Value) || IsB2AsAccountForSeededLine;
		}

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return ((IFindBoxListProvider)Lookups.Tariffs).DescriptionFromCode(tariffCode) ?? ZString.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (GetAddInfo().SuspendSettingHasChanges())
			using (GetAddInfo().GetValidationSuspender())
			{
				CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			}
		}

		protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		{
			return new CustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsQuantityInfo, (ZPropertyInfoString)JI_CustomsUnitQtyInfo);
		}

		protected override ZAddress GetNewJI_OA_ConsigneeAddress_ZAddress()
		{
			var result = base.GetNewJI_OA_ConsigneeAddress_ZAddress();
			result.GetDefaultAddress = header =>
			{
				var orgHeader = header as OrgHeader;
				return orgHeader != null ? orgHeader.GetAddressWithFallback(AddressType.DLV).PK : ZGuid.Empty;
			};

			return result;
		}

		#region ClassificationPartPivot

		protected override void InitialisePartSyncManager()
		{
			if (fPartSyncManager == null)
			{
				fPartSyncManager = new JobComInvoiceLinePartSynchronisationManager(this);
			}
		}

		protected override BaseCusClassPartPivot GetPivotCore()
		{
			if (Declaration != null && Part != null && InvoiceHeader != null)
			{
				if (IsImportIncludingB2)
				{
					return Part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Canada).GetImportMatch(Core.Constants.CountryCodes.Canada, InvoiceHeader.JZ_OH_Buyer_Effective, InvoiceHeader.JZ_OH_Supplier, EffectiveDateForDutyRate, GetPartAttribs());
				}
				else if (Declaration.IsExport)
				{
					return Part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Canada).GetExportMatch(Core.Constants.CountryCodes.Canada, !Declaration.IsG7ExportDeclaration, InvoiceHeader.JZ_OH_Buyer_Effective, InvoiceHeader.JZ_OH_Supplier, EffectiveDateForDutyRate);
				}
			}
			return base.GetPivotCore();
		}

		public override ZDate EffectiveDateForDutyRate
		{
			get
			{
				return Factory.GetValue(ref effectiveDateForDutyRateCached, () =>
				{
					var result = ZDate.Today;
					var declaration = Declaration;
					if (declaration != null)
					{
						if (declaration.IsImportIncludingB2)
						{
							if (declaration.JE_EntryAuthorisationDate.IsValid)
							{
								result = declaration.JE_EntryAuthorisationDate.Date;
							}
							else if (declaration.CA_EstReleaseDate.IsValid)
							{
								result = declaration.CA_EstReleaseDate.Date;
							}
							else if (declaration.JE_DateOfFirstArrival.IsValid)
							{
								result = declaration.JE_DateOfFirstArrival.Date;
							}
						}
						else if (declaration.IsExport)
						{
							if (declaration.JE_ExportDate.IsValid)
							{
								result = declaration.JE_ExportDate.Date;
							}
						}
					}

					return result;
				});
			}
		}
		CachedProperty<ZDate> effectiveDateForDutyRateCached;

		#region UpdateDetailsOnPartChange

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();

			if (Declaration is JobDeclaration declaration && Pivot is CusClassPartPivot pivot)
			{
				if (declaration.IsImport)
				{
					SetImportDefaulsFromPivot(pivot);
					UpdateDutiesAndTaxesFromProduct(pivot);

					if (!PartWasJustUpdatedByDataRefresh(pivot))
					{
						if (!declaration.IsIID)
						{
							UpdateOGDDetails(pivot);
						}
						else
						{
							UpdatePGADetailsFromPivot(pivot);
						}
					}
				}
				else if (IsB2OrB3XAdjustments)
				{
					SetB2DefaulsFromPivot(pivot);
				}
				else if (declaration.IsExport)
				{
					SetExportDefaulsFromPivot(pivot);
				}
			}
		}

		bool PartWasJustUpdatedByDataRefresh(CusClassPartPivot pivot)
		{
			var part = !pivot.IsNull ? pivot.Part : null;
			return part != null && part.JustUpdatedByDataRefresh;
		}

		protected override void ASNRefereshDataCountrySpecific(IEnumerable<ZString> refreshOptions, BaseCusClassPartPivot pivot)
		{
			base.ASNRefereshDataCountrySpecific(refreshOptions, pivot);
			var caPivot = (CusClassPartPivot)pivot;
			if (refreshOptions.Contains(DefaultOptions.Codes.CountryOfOrigin))
			{
				JI_CountryOfOrigin = caPivot.CCA_RN_NKOrigin;
			}
			if (refreshOptions.Contains(DefaultOptions.Codes.Preference))
			{
				CA_TreatmentCode = caPivot.CI_CC_CA_TreatmentCode;
			}
		}

		void SetImportDefaulsFromPivot(CusClassPartPivot pivot)
		{
			if (!pivot.CCA_ValueForDutyCode.IsEmpty)
			{
				CA_ValueForDutyCode = pivot.CCA_ValueForDutyCode;
			}

			if (!pivot.CCA_TreatmentCode.IsEmpty)
			{
				CA_TreatmentCode = pivot.CCA_TreatmentCode;
			}

			if (!pivot.CCA_99TariffCode.IsEmpty)
			{
				CA_99TariffCode = pivot.CCA_99TariffCode;
			}

			if (!pivot.CCA_AuthorityNumber.IsEmpty)
			{
				CA_AuthorityNumber = pivot.CCA_AuthorityNumber;
			}

			if (!pivot.CCA_TRSNumber.IsEmpty)
			{
				CA_TRSNumber = pivot.CCA_TRSNumber;
			}

			if (!pivot.CCA_OA_Manufacturer.IsEmpty)
			{
				JI_OA_ManufacturerAddress = pivot.CCA_OA_Manufacturer;
			}

			var invoiceHeader = InvoiceHeader;
			if (invoiceHeader == null || pivot.CCA_RN_NKOrigin != invoiceHeader.JZ_RN_NKDefaultOrigin || pivot.CCA_ProvinceOfOrigin != invoiceHeader.JZ_RW_NKOriginState)
			{
				if (!pivot.CCA_RN_NKOrigin.IsEmpty)
				{
					JI_CountryOfOrigin = pivot.CCA_RN_NKOrigin;
				}

				if (!pivot.CCA_ProvinceOfOrigin.IsEmpty)
				{
					JI_StateOrRegionOfOrigin = pivot.CCA_ProvinceOfOrigin;
				}
			}

			if (invoiceHeader == null || pivot.CCA_RN_NKSource != invoiceHeader.CA_RN_NKSource || pivot.CCA_StateOfSource != invoiceHeader.CA_StateOfSource)
			{
				if (!pivot.CCA_RN_NKSource.IsEmpty)
				{
					CA_RN_NKSource = pivot.CCA_RN_NKSource;
				}

				if (!pivot.CCA_StateOfSource.IsEmpty)
				{
					CA_StateOfSource = pivot.CCA_StateOfSource;
				}
			}

			var pivotAsIHasPGARequirements = pivot as IHasPGARequirements;
			var classificationAsIHasPGARequirements = pivot.Classification as IHasPGARequirements;
			var effectiveBrandName = pivotAsIHasPGARequirements.JI_BrandName.IsEmpty ? (classificationAsIHasPGARequirements?.JI_BrandName ?? ZString.Empty) : pivotAsIHasPGARequirements.JI_BrandName;
			if (!effectiveBrandName.IsEmpty)
			{
				JI_BrandName = effectiveBrandName;
			}

			var effectiveModel = pivotAsIHasPGARequirements.JI_Model.IsEmpty ? (classificationAsIHasPGARequirements?.JI_Model ?? ZString.Empty) : pivotAsIHasPGARequirements.JI_Model;
			if (!effectiveModel.IsEmpty)
			{
				JI_Model = effectiveModel;
			}

			CA_AMMVPerUnit = pivot.CCA_AMMVPerUnit;
			CA_AMMVPercentage = pivot.CCA_AMMVPercentage;
		}

		void SetB2DefaulsFromPivot(CusClassPartPivot pivot)
		{
			if (!pivot.CCA_ValueForDutyCode.IsEmpty)
			{
				CA_ValueForDutyCode = pivot.CCA_ValueForDutyCode;
			}

			if (!pivot.CCA_99TariffCode.IsEmpty)
			{
				CA_99TariffCode = pivot.CCA_99TariffCode;
			}

			if (!pivot.CCA_AuthorityNumber.IsEmpty)
			{
				CA_AuthorityNumber = pivot.CCA_AuthorityNumber;
			}
		}

		void UpdateOGDDetails(CusClassPartPivot pivot)
		{
			//CFIA
			if (!pivot.CCA_RequirementID.IsEmpty)
			{
				CA_RequirementID = pivot.CCA_RequirementID;
			}

			if (!pivot.CCA_RequirementVersion.IsEmpty)
			{
				CA_RequirementVer = pivot.CCA_RequirementVersion;
			}

			if (!pivot.CCA_AirsCode.IsEmpty)
			{
				CA_AirsCode = pivot.CCA_AirsCode;
			}

			if (!pivot.CCA_DestinationProvince.IsEmpty)
			{
				CA_DestinationProvince = pivot.CCA_DestinationProvince;
			}

			if (!pivot.CCA_EndUse.IsEmpty)
			{
				CA_EndUse = pivot.CCA_EndUse;
			}

			if (!pivot.CCA_MiscID.IsEmpty)
			{
				CA_MiscID = pivot.CCA_MiscID;
			}

			if (!pivot.CCA_RN_NKCFIAOrigin.IsEmpty)
			{
				CA_RN_NKCFIAOrigin = pivot.CCA_RN_NKCFIAOrigin;
			}

			if (!pivot.CCA_CFIAUSStateOfOrigin.IsEmpty)
			{
				CA_CFIAUSStateOfOrigin = pivot.CCA_CFIAUSStateOfOrigin;
			}

			foreach (CFIARegistrationNumber regNumber in pivot.CFIARegistrationNumbers)
			{
				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, PK);
				query.AddToFilter(CusCodeDataSchema.CY_Code, regNumber.CY_Code);
				foreach (var existingNumber in CFIARegistrationNumbers.Find(query).ToArray())
				{
					CFIARegistrationNumbers.RemoveAndDelete(existingNumber);
				}
				CFIARegistrationNumbers.Add(regNumber.Clone());
			}
			//SITT
			if (!pivot.CCA_ImportReasonCode.IsEmpty)
			{
				CA_ImportReasonCode = pivot.CCA_ImportReasonCode;
			}

			if (!pivot.CCA_Model.IsEmpty)
			{
				CA_Model = pivot.CCA_Model.SubstringSafe(0, CAAddInfoSchema.CA_Model.MaxLength);
			}

			if (!pivot.CCA_ModelNumber.IsEmpty)
			{
				CA_ModelNumber = pivot.CCA_ModelNumber;
			}

			if (!pivot.CCA_BrandName.IsEmpty)
			{
				JI_BrandName = pivot.CCA_BrandName.SubstringSafe(0, JobComInvoiceLineSchema.JI_BrandName.MaxLength);
			}

			foreach (SITTCertificationNumber number in pivot.SITTCertificationNumbers)
			{
				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, PK);
				query.AddToFilter(CusCodeDataSchema.CY_Data, number.CY_Data);
				foreach (var existingNumber in SITTCertificationNumbers.Find(query).ToArray())
				{
					SITTCertificationNumbers.RemoveAndDelete(existingNumber);
				}
				SITTCertificationNumbers.Add(number.Clone());
			}

			//NRCAN
			if (!pivot.CCA_TypeSize.IsEmpty)
			{
				CA_TypeSize = pivot.CCA_TypeSize;
			}

			//Tires
			if (!pivot.CCA_TIIN.IsEmpty)
			{
				CA_TIIN = pivot.CCA_TIIN;
			}

			if (!pivot.CCA_CompliantCompletion.IsEmpty)
			{
				CA_CompliantCompletion = pivot.CCA_CompliantCompletion;
			}

			if (!pivot.CCA_CompliantImportDateIndicator.IsEmpty)
			{
				CA_CompliantImportDate = pivot.CCA_CompliantImportDateIndicator;
			}
		}

		void SetExportDefaulsFromPivot(CusClassPartPivot pivot)
		{
			if (!pivot.CCA_ProvinceOfOrigin.IsEmpty)
			{
				JI_StateOrRegionOfOrigin = pivot.CCA_ProvinceOfOrigin;
			}

			if (!pivot.CCA_RN_NKOrigin.IsEmpty)
			{
				JI_CountryOfOrigin = pivot.CCA_RN_NKOrigin;
			}
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region Collections

		#region Export Permits

		[ChildEditable(true)]
		public InvoiceLineExportPermitCollection Permits
		{
			get
			{
				if (permits == null)
				{
					permits = new InvoiceLineExportPermitCollection(this);
					permits.Load();
					RegisterEditableChildObject(permits);
				}
				return permits;
			}
		}

		InvoiceLineExportPermitCollection permits;

		#endregion

		#region CFIARegistrationNumbers

		[ChildEditable(true)]
		public CFIARegistrationNumberCollection CFIARegistrationNumbers
		{
			get
			{
				if (fCFIARegistrationNumbers == null)
				{
					fCFIARegistrationNumbers = new CFIARegistrationNumberCollection(this);
					fCFIARegistrationNumbers.Load();
					RegisterEditableChildObject(fCFIARegistrationNumbers);
					fCFIARegistrationNumbers.HasChangesChanged += FCFIARegistrationNumbers_HasChangesChanged;
				}
				return fCFIARegistrationNumbers;
			}
		}

		void FCFIARegistrationNumbers_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged)
			{
				SetAVSStatusIfNeeded();
			}
		}

		CFIARegistrationNumberCollection fCFIARegistrationNumbers;

		#endregion

		#region SITTCertificationNumbers

		[ChildEditable(true)]
		public SITTCertificationNumberCollection SITTCertificationNumbers
		{
			get
			{
				if (fSITTCertificationNumbers == null)
				{
					fSITTCertificationNumbers = new SITTCertificationNumberCollection(this);
					fSITTCertificationNumbers.Load();
					RegisterEditableChildObject(fSITTCertificationNumbers);
					fSITTCertificationNumbers.CountChanged += fSITTCertificationNumbers_CountChanged;
				}
				return fSITTCertificationNumbers;
			}
		}

		SITTCertificationNumberCollection fSITTCertificationNumbers;

		void fSITTCertificationNumbers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var collection = (SITTCertificationNumberCollection)sender;
			if ((e.ItemAdded && collection.Count == 1) || (e.ItemRemoved && collection.Count == 0))
			{
				AddInfoValidation.ValidateCA_ImportReasonCode();
			}
		}

		#endregion

		#region DutiesAndTaxes

		[ChildEditable(true)]
		public DutyAndTaxCollection DutiesAndTaxes
		{
			get
			{
				if (dutiesAndTaxes == null)
				{
					if (IsImportIncludingB2)
					{
						SetDutiesAndTaxes();
					}
					else
					{
						dutiesAndTaxes = new DutyAndTaxCollection(Factory, this);
					}
				}
				return dutiesAndTaxes;
			}
		}

		void SetDutiesAndTaxes()
		{
			dutiesAndTaxes = new DutyAndTaxCollection(this);
			dutiesAndTaxes.HasChangesChanged += JobComInvoiceLine_OnMarkApportionmentDirty;
			_ = DutyAndTaxManager;
			RegisterEditableChildObject(dutiesAndTaxes);
		}

		protected DutyAndTaxCollection dutiesAndTaxes;

		public void RefreshDutiesAndTaxes()
		{
			dutiesAndTaxes = null;
		}

		#endregion

		#region PGARequirements

		[ChildEditable]
		public PGARequirementCollection PGARequirements
		{
			get
			{
				if (pgaAgencyRequirements == null)
				{
					pgaAgencyRequirements = new PGARequirementCollection(new PGARequirementProvider(this));
					pgaAgencyRequirements.Populate();
					RegisterEditableChildObject(pgaAgencyRequirements);
				}
				return pgaAgencyRequirements;
			}
		}

		PGARequirementCollection pgaAgencyRequirements;

		internal void RefreshBindingForDeclaredPGAHeaders()
		{
			if (IsIIDDeclaration)
			{
				PGARequirements.RefreshBindingForDeclaredPGAHeaders();
			}
		}

		#endregion

		#region AVSQueryResults

		public AVSQueryResultCollection AVSQueryResults
		{
			get
			{
				if (fAVSQueryResultCollection == null)
				{
					fAVSQueryResultCollection = new AVSQueryResultCollection(this);
					fAVSQueryResultCollection.Load();
				}
				return fAVSQueryResultCollection;
			}
		}

		AVSQueryResultCollection fAVSQueryResultCollection;

		#endregion

		#region SIMA Measures
		public SIMADumpingNumberCollection SIMAMeasures
		{
			get { return fSIMAMeasures ?? (fSIMAMeasures = new SIMADumpingNumberCollection(Factory)); }
		}
		SIMADumpingNumberCollection fSIMAMeasures;
		#endregion

		#region RulingConfigurations

		public new CACusRulingConfigCollection RulingConfigurations => (CACusRulingConfigCollection)base.RulingConfigurations;

		protected override CusRulingConfigCombinedCollection GetRulingConfigurations()
		{
			var result = new CACusRulingConfigCollection(this);
			result.HasChangesChanged += JobComInvoiceLine_OnMarkApportionmentDirty;

			return result;
		}

		protected override bool SupportRulingConfigurations => true;

		protected override ZBool ConfigurationsReadOnly => CA_CalculationMethod != RefCusRulingTypeList.Codes.T;

		#endregion

		#endregion

		#region Implementation

		Integration.Customs.CA.IDutyAndTaxCollection Integration.Customs.CA.IJobComInvoiceLine.DutiesAndTaxes => DutiesAndTaxes;

		protected override ZString CustomsCountryCodeCore
		{
			get { return Core.Constants.CountryCodes.Canada; }
		}

		protected override ZString GetPartPivotTypeCore()
		{
			return !IsExport
				? ClassificationTypeList.Codes.HTI
				: (IsDataLoadingModule ? ClassificationTypeList.Codes.SHB : ClassificationTypeList.Codes.HTE);
		}

		internal ITariffData Tariff => new TariffWrapper(this);

		[ReadOnlyMember(nameof(JI_LineNo_ReadOnly))]
		public override ZShort JI_LineNo
		{
			get => base.JI_LineNo;
			set => base.JI_LineNo = value;
		}

		bool JI_LineNo_ReadOnly
		{
			get { return IsRemissionRepairLineIncludingParent; }
		}

		public ZString RulingDescription => Ruling != null ? Ruling.ZZX_Description : ZString.Empty;

		public ZPropertyInfo RulingDescriptionInfo => GetZPropertyInfo(nameof(RulingDescription));

		internal ZZRefCusRulingCombined Ruling
		{
			get
			{
				if (ruling == null || ruling.IsDeleted || needReloadRuling)
				{
					using (new DisposableAction(() => needReloadRuling = false))
					{
						ruling = null;
						if (!CA_AuthorityNumber.IsEmpty)
						{
							var declaration = Declaration;
							var org = declaration != null ? declaration.ImporterOfRecord ?? declaration.Importer : InvoiceHeader?.Buyer;
							var query = GetZZRefCusRulingCombinedQuery();
							var rulings = Factory.Load<ZZRefCusRulingCombined>(query);

							if (rulings.Length > 0)
							{
								if (org != null)
								{
									ruling = rulings.FirstOrDefault(x => (x.AppliesToAddress?.OA_OH ?? ZGuid.Empty) == org.PK)
										?? rulings.FirstOrDefault(x => x.ZZX_OA_AppliesTo.IsEmpty);
								}
								else
								{
									ruling = rulings.FirstOrDefault(x => x.ZZX_OA_AppliesTo.IsEmpty) ?? rulings.FirstOrDefault();
								}
							}
						}
					}
				}

				return ruling;
			}
		}

		ZZRefCusRulingCombined ruling;
		bool needReloadRuling;

		ZQuery GetZZRefCusRulingCombinedQuery()
		{
			var result = new ZQuery(ZZRefCusRulingCombinedSchema.ZZX_RulingNumber, CA_AuthorityNumber);
			result.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
			result.OrderBy = ZZRefCusRulingCombinedSchema.ZZX_StartDate.Name + " DESC";

			return result;
		}

		#region ReadOnly

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			bool result;
			switch (property.Name)
			{
				case Schema.JI_LinePrice:
				case Schema.JI_Description:
				case Schema.CA_ADJCode:
				case Schema.CA_ADJValue:
				case Schema.CA_CVforCurrConv:
				case Schema.CA_CustomsValue:
				case Schema.CA_CVforCurrConvOvr:
				case Schema.CA_CustomsValueOvr:
				case Schema.JI_CountryOfOrigin:
				case Schema.JI_StateOrRegionOfOrigin:
				case Schema.JI_Tariff:
				case Schema.JI_FormattedTariff:
					result = false;
					break;
				default:
					result = IsRemissionRepairLine;
					break;
			}
			return result || base.GetShouldPropertiesBeReadOnly(property);
		}

		#endregion

		public override bool ReadOnly
		{
			get
			{
				return base.ReadOnly || CA_IsAutoDummyHSCodeCasualImportLine || IsLuxuryTaxInvoiceLine;
			}

			set => base.ReadOnly = value;
		}

		public override void Delete()
		{
			if (IsB2OrB3XAdjustments)
			{
				if (CA_IsAccountForLine)
				{
					ReadOnlyAsClaimForFilteredInvoiceLines.RemoveAndDeleteAll();
				}
			}

			if (IsLuxuryTaxInvoiceLine)
			{
				if (luxuryTaxInvoiceLineSynchroniser != null)
				{
					luxuryTaxInvoiceLineSynchroniser.Dispose();
					luxuryTaxInvoiceLineSynchroniser = null;
				}
			}

			DeleteLuxuryTaxInvoiceLine();

			RefreshInvoiceLinesWithPGAs();
			base.Delete();
		}

		#region ICanDelete

		protected override bool CanDeleteCore
		{
			get { return base.CanDeleteCore && !IsRemissionRepairLineIncludingParent && IsCommoditySequenceEmpty && !IsLuxuryTaxInvoiceLine && !(Declaration?.HasAB3AcceptedOrWaiting ?? false); }
		}

		protected override MultilingualString ReasonForNotAbleToDeleteCore
		{
			get
			{
				if (IsRemissionRepairLineIncludingParent)
				{
					return ResString.GetMultilingualString("B9E29DB2-E879-4A36-89D4-8623BA0CD737", "This line may not be deleted because it has an associated Remission Line. To delete this line change or remove the Remission Type of the parent line.");
				}
				else if (!IsCommoditySequenceEmpty)
				{
					return ResString.GetMultilingualString("CADInvoiceLineNotAbleToDelete", "Invoice line not allowed to be deleted because there is CAD response for it.");
				}
				else if (Declaration?.HasAB3AcceptedOrWaiting ?? false)
				{
					return ResString.GetMultilingualString("E7F5C047-A245-4A60-BDCC-1DE4FD80DA61", "Invoice line may not be deleted because this CAD has already been reported, or is waiting for a response.");
				}
				else if (IsLuxuryTaxInvoiceLine)
				{
					return ResString.GetMultilingualString("2443A85A-5C6B-4B82-9C33-B964F79B509E", "The luxury tax invoice line can not be deleted because it has an associated line. To delete this line, uncheck the Luxury Tax Applies checkbox of the parent line.");
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			if (IsB2OrB3XAdjustments && !this.CA_IsAccountForLine && this.CorrespondingAsAccountedForInvoiceLine != null)
			{
				return ResString.GetMultilingualString("63563c29-a938-4602-84c9-e1654686f825", "Deleting a line in 'As Claimed' will delete the corresponding line in 'As Accounted'.");
			}
			else
			{
				return base.GetWarningBeforeBeingDeleted();
			}
		}

		#endregion

		#region OnMarkApportionmentDirty

		event EventHandler OnMarkApportionmentDirty
		{
			add
			{
				JI_TariffInfo.ValueChanged += value;
				CA_99TariffCodeInfo.ValueChanged += value;
				CA_ADJCodeInfo.ValueChanged += value;
				CA_CVforCurrConvInfo.ValueChanged += value;
				CA_CVforCurrConvOvrInfo.ValueChanged += value;
				CA_CustomsValueInfo.ValueChanged += value;
				CA_CustomsValueOvrInfo.ValueChanged += value;
				JI_CustomsQuantityInfo.ValueChanged += value;
				JI_CustomsUnitQtyInfo.ValueChanged += value;
				JI_CustomsSecondQuantityInfo.ValueChanged += value;
				JI_CustomsSecondUnitQtyInfo.ValueChanged += value;
				JI_CustomsThirdQuantityInfo.ValueChanged += value;
				JI_CustomsThirdUnitQtyInfo.ValueChanged += value;
				CA_CalculationMethodInfo.ValueChanged += value;
				CA_TreatmentCodeInfo.ValueChanged += value;
				CA_ValueForDutyCodeInfo.ValueChanged += value;
				CA_AuthorityNumberInfo.ValueChanged += value;
				CA_TRSNumberInfo.ValueChanged += value;
				JI_CountryOfOriginInfo.ValueChanged += value;
				JI_StateOrRegionOfOriginInfo.ValueChanged += value;
				CA_RN_NKExportInfo.ValueChanged += value;
				CA_USStateOfExportInfo.ValueChanged += value;
				CA_CasualImportDestinationProvinceInfo.ValueChanged += value;
				CA_CasualImportCommodityInfo.ValueChanged += value;
				CA_IsCasualImportInfo.ValueChanged += value;
				CA_IsExemptInfo.ValueChanged += value;
				Declaration.JE_CustomsOfficeInfo.ValueChanged += value;
			}
			remove
			{
				JI_TariffInfo.ValueChanged -= value;
				CA_99TariffCodeInfo.ValueChanged -= value;
				CA_ADJCodeInfo.ValueChanged -= value;
				CA_CVforCurrConvInfo.ValueChanged -= value;
				CA_CVforCurrConvOvrInfo.ValueChanged -= value;
				CA_CustomsValueInfo.ValueChanged -= value;
				CA_CustomsValueOvrInfo.ValueChanged -= value;
				JI_CustomsQuantityInfo.ValueChanged -= value;
				JI_CustomsUnitQtyInfo.ValueChanged -= value;
				JI_CustomsSecondQuantityInfo.ValueChanged -= value;
				JI_CustomsSecondUnitQtyInfo.ValueChanged -= value;
				JI_CustomsThirdQuantityInfo.ValueChanged -= value;
				JI_CustomsThirdUnitQtyInfo.ValueChanged -= value;
				CA_CalculationMethodInfo.ValueChanged -= value;
				CA_TreatmentCodeInfo.ValueChanged -= value;
				CA_ValueForDutyCodeInfo.ValueChanged -= value;
				CA_AuthorityNumberInfo.ValueChanged -= value;
				CA_TRSNumberInfo.ValueChanged -= value;
				JI_CountryOfOriginInfo.ValueChanged -= value;
				JI_StateOrRegionOfOriginInfo.ValueChanged -= value;
				CA_RN_NKExportInfo.ValueChanged -= value;
				CA_USStateOfExportInfo.ValueChanged -= value;
				CA_CasualImportDestinationProvinceInfo.ValueChanged -= value;
				CA_CasualImportCommodityInfo.ValueChanged -= value;
				CA_IsCasualImportInfo.ValueChanged -= value;
				CA_IsExemptInfo.ValueChanged -= value;
				Declaration.JE_CustomsOfficeInfo.ValueChanged -= value;
			}
		}

		void JobComInvoiceLine_OnMarkApportionmentDirty(object sender, EventArgs e)
		{
			MarkApportionmentDirty(true);
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : JobComInvoiceLineFetchStrategy
		{
			public Strategy(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
			}

			JobComInvoiceLine InvoiceLine
			{
				get { return BusinessObject as JobComInvoiceLine; }
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				if (columns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.JI_B3LineNumber) != null)
				{
					Factory.AddFetchHint(CusUnderbondDecSchema.BU_JI, BusinessObject.PK);
				}
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				if (InvoiceLine.IsImport)
				{
					Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
				}
				if (InvoiceLine.IsDataLoadingModule)
				{
					Factory.AddFetchHint(typeof(CACExportTariff), CACExportTariffSchema.CE_Code, InvoiceLine.JI_Tariff);
				}
				else
				{
					Factory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem, InvoiceLine.JI_Tariff, InvoiceLine.UniversalTariffValuationDate));
				}
				InvoiceLine.pgaHeaders.FetchForValidate();
			}
		}

		#endregion

		#region IUltimateDistributee Members

		ZString IUltimateDistributee.SupplierName
		{
			get { return InvoiceHeader != null ? InvoiceHeader.SupplierDocumentaryAddress.AddressAsASingleLine : ZString.Empty; }
		}

		#endregion

		#region ISecondCustomsQuantity Members

		ZPropertyInfo ISecondCustomsQuantity.SecondCustomsQtyInfo
		{
			get { return JI_CustomsSecondQuantityInfo; }
		}

		ZPropertyInfo ISecondCustomsQuantity.SecondCustomsUQInfo
		{
			get { return JI_CustomsSecondUnitQtyInfo; }
		}

		#endregion

		#region B2 Documents

		public B2AsClaimForLineViewCollection ReadOnlyAsClaimForFilteredInvoiceLines => readOnlyAsClaimForFilteredInvoiceLines ??= new B2AsClaimForLineViewCollection(this, Declaration.InvoiceLines);
		B2AsClaimForLineViewCollection readOnlyAsClaimForFilteredInvoiceLines;

		public bool IsB2OrB3XAdjustments
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && (declaration.IsB2Adjustments || declaration.IsB3X);
			}
		}

		public bool IsB2AsAccountForSeededLine
		{
			get
			{
				return IsB2OrB3XAdjustments && CA_IsAccountForLine && CA_IsSeeded;
			}
		}

		public JobComInvoiceLine LineAccountFor
		{
			get { return IsB2OrB3XAdjustments ? (JobComInvoiceLine)ParentTariffLine : null; }
			set { this.JI_ParentID = value.PK; }
		}

		public void OverwriteDutiesAndTaxesFromAsAccountedLine(JobComInvoiceLine asAccountedForLine)
		{
			if (IsB2OrB3XAdjustments && !this.CA_IsAccountForLine && asAccountedForLine.CA_IsAccountForLine)
			{
				DutiesAndTaxes.DeleteAll();
				foreach (var dutyAndTaxToCopy in asAccountedForLine.DutiesAndTaxes)
				{
					var clonedDutyAndTax = (DutyAndTax)dutyAndTaxToCopy.Clone();
					using (clonedDutyAndTax.GetValidationSuspender())
					using (clonedDutyAndTax.SuspendSettingHasChanges())
					{
						clonedDutyAndTax.Parent = this;
						this.DutiesAndTaxes.Add(clonedDutyAndTax);
					}
					clonedDutyAndTax.C1_TaxType = dutyAndTaxToCopy.C1_TaxType;
					clonedDutyAndTax.C1_Amount = dutyAndTaxToCopy.C1_Amount;
					clonedDutyAndTax.C1_Code = dutyAndTaxToCopy.C1_Code;
					clonedDutyAndTax.C1_ExemptCode = dutyAndTaxToCopy.C1_ExemptCode;
					clonedDutyAndTax.C1_ForeignCurrency = dutyAndTaxToCopy.C1_ForeignCurrency;
					clonedDutyAndTax.C1_ForeignRate = dutyAndTaxToCopy.C1_ForeignRate;
					clonedDutyAndTax.C1_NormalValueCurrency = dutyAndTaxToCopy.C1_NormalValueCurrency;
					clonedDutyAndTax.C1_NormalValuePerUnit = dutyAndTaxToCopy.C1_NormalValuePerUnit;
					clonedDutyAndTax.C1_OriginalAmount = dutyAndTaxToCopy.C1_OriginalAmount;
					clonedDutyAndTax.C1_Override = dutyAndTaxToCopy.C1_Override;
					clonedDutyAndTax.C1_PreviousTranLine = dutyAndTaxToCopy.C1_PreviousTranLine;
					clonedDutyAndTax.C1_PreviousTranNumber = dutyAndTaxToCopy.C1_PreviousTranNumber;
					clonedDutyAndTax.C1_Rate = dutyAndTaxToCopy.C1_Rate;
					clonedDutyAndTax.C1_RateType = dutyAndTaxToCopy.C1_RateType;
					clonedDutyAndTax.C1_UnitOfMeasure = dutyAndTaxToCopy.C1_UnitOfMeasure;
					clonedDutyAndTax.C1_ValueForCalculation = dutyAndTaxToCopy.C1_ValueForCalculation;
				}
				asAccountedForLine.Declaration.MarkApportionmentDirty();
			}
		}

		public void GetDutyAndTaxFrom(IClassificationLine1 classificationLine)
		{
			this.Override(DutyAndTaxTypes.Codes.SIMADuty, classificationLine.SIMACode, classificationLine.SIMAAssessment, null, null);
			this.Override(DutyAndTaxTypes.Codes.GST, classificationLine.GSTExemptionCode, classificationLine.GSTAmount, classificationLine.RateOfGST,
				classificationLine.GSTRateType);
			if (!classificationLine.ExciseTaxRateType.IsEmpty)
			{
				this.Override(DutyAndTaxTypes.Codes.ExciseTax, classificationLine.ExciseExemptionCode, classificationLine.ExciseTaxAmount,
					classificationLine.ExciseTaxRate, classificationLine.ExciseTaxRateType);
			}
			foreach (var classification2 in classificationLine.ClassificationLines)
			{
				if (!classification2.CustomsDutyRateType.IsEmpty)
				{
					this.OverrideCustomsDutyAmount(classification2.CustomsDutyRate, classification2.CustomsDutyRateType,
						classification2.CustomsDutyAmount);
				}
			}
		}

		DutyAndTax Override(ZString taxType, ZString exemptCode, ZDecimal amount, ZDecimal? rate, ZString? rateType)
		{
			var duty = DutiesAndTaxes.FirstOrDefault(x => taxType == DutyAndTaxTypes.Codes.SIMADuty ? (bool)DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType) : x.C1_TaxType == taxType);
			if (duty == null)
			{
				duty = DutiesAndTaxes.AddNew();
				duty.C1_TaxType = taxType;
				duty.C1_Override = false;
				if (rate.HasValue)
				{
					duty.C1_Rate = rate.Value;
				}
				if (rateType.HasValue)
				{
					duty.C1_RateType = rateType.Value;
				}
				duty.C1_ExemptCode = exemptCode;
				duty.C1_Amount = amount;
			}
			else
			{
				if (duty.C1_ExemptCode == exemptCode && duty.C1_Amount == amount && (!rateType.HasValue || duty.C1_RateType == rateType.Value) && (!rate.HasValue || duty.C1_Rate == rate.Value))
				{
					duty.C1_Override = false;
				}
				else
				{
					if (rate.HasValue)
					{
						duty.C1_Rate = rate.Value;
					}
					if (rateType.HasValue)
					{
						duty.C1_RateType = rateType.Value;
					}
					duty.C1_ExemptCode = exemptCode;
					duty.C1_Amount = amount;
					duty.C1_Override = true;
				}
			}
			return duty;
		}

		DutyAndTax OverrideCustomsDutyAmount(ZDecimal rate, ZString rateType, ZDecimal amount)
		{
			var duty = DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && x.C1_Rate == rate && x.C1_RateType == rateType);
			if (duty != null)
			{
				if (amount != duty.C1_Amount)
				{
					duty.C1_Override = true;
					duty.C1_Amount = amount;
				}
				else
				{
					duty.C1_Override = false;
				}
			}
			return duty;
		}

		public JobComInvoiceLine CreateAsClaimedForLine()
		{
			JobComInvoiceLine asClaimedForLine = null;
			if (this.CA_IsAccountForLine && InvoiceHeader is JobComInvoiceHeader header && header.CorrespondingAsClaimedForInvoice is JobComInvoiceHeader b2AsClaimedForInvoice)
			{
				asClaimedForLine = b2AsClaimedForInvoice.AsClaimForFilteredInvoiceLines.AddNew(this);
			}
			return asClaimedForLine;
		}

		public JobComInvoiceLine CorrespondingAsClaimedForInvoiceLine => Factory.GetValue(ref correspondingAsClaimedForInvoiceLineCached, () =>
		{
			var originalLineNo = CA_OriginalLineNo;
			return ReadOnlyAsClaimForFilteredInvoiceLines
				.Cast<JobComInvoiceLine>()
				.Where(x => x.CA_IsSeeded && x.CA_OriginalLineNo == originalLineNo)
				.Select(s => (s.CA_OriginalLineNo, Line: s))
				.OrderBy(x => x.CA_OriginalLineNo)
				.FirstOrDefault().Line;
		});
		CachedProperty<JobComInvoiceLine> correspondingAsClaimedForInvoiceLineCached;

		public JobComInvoiceLine CorrespondingAsAccountedForInvoiceLine => Factory.GetValue(ref correspondingAsAccountedForInvoiceLineCached, () =>
		{
			var originalLineNo = CA_OriginalLineNo;
			return InvoiceHeader?
				.CorrespondingAsAccountedForInvoice?
				.AsAccountForFilteredInvoiceLines?
				.Cast<JobComInvoiceLine>()
				.Where(x => x.PK == JI_ParentID && x.CA_OriginalLineNo == CA_OriginalLineNo)
				.Select(s => (s.CA_OriginalLineNo, Line: s))
				.OrderBy(o => o.CA_OriginalLineNo)
				.FirstOrDefault().Line;
		});
		CachedProperty<JobComInvoiceLine> correspondingAsAccountedForInvoiceLineCached;

		public void EnableAndSynchronise(bool forceSync = false)
		{
			if (ShouldSynchronise && !B2AsClaimedInvoiceLineSynchroniser.IsEnabled)
			{
				using (GetValidationSuspender())
				{
					B2AsClaimedInvoiceLineSynchroniser.SetEnabled(ShouldSynchronise, B2AsClaimedInvoiceLineSynchroniser.DetectEnabled);
					if (forceSync)
					{
						B2AsClaimedInvoiceLineSynchroniser.Synchronise(forceSync);
					}
					else
					{
						B2AsClaimedInvoiceLineSynchroniser.Synchronise();
					}
				}
			}
		}

		public void EnableSynchroniser()
		{
			if (ShouldSynchronise)
			{
				B2AsClaimedInvoiceLineSynchroniser.SetEnabled(ShouldSynchronise, B2AsClaimedInvoiceLineSynchroniser.DetectEnabled);
			}
		}

		internal B2JobComInvoiceLineSynchroniser B2AsClaimedInvoiceLineSynchroniser
		{
			get
			{
				if (fB2AsClaimedInvoiceLineSynchroniser == null)
				{
					var destination = this.CorrespondingAsClaimedForInvoiceLine ?? this.CreateAsClaimedForLine();
					fB2AsClaimedInvoiceLineSynchroniser = new B2JobComInvoiceLineSynchroniser(destination, this);
				}
				return fB2AsClaimedInvoiceLineSynchroniser;
			}
		}
		B2JobComInvoiceLineSynchroniser fB2AsClaimedInvoiceLineSynchroniser;

		public void ReEnableAndSynchronise()
		{
			if (fB2AsClaimedInvoiceLineSynchroniser != null)
			{
				fB2AsClaimedInvoiceLineSynchroniser.Dispose();
				fB2AsClaimedInvoiceLineSynchroniser = null;
			}

			EnableAndSynchronise(true);

			this.CorrespondingAsClaimedForInvoiceLine?.OverwriteDutiesAndTaxesFromAsAccountedLine(this);
		}

		internal bool ShouldSynchronise
		{
			get
			{
				return IsB2OrB3XAdjustments &&
					InvoiceHeader != null &&
					InvoiceHeader.CorrespondingAsClaimedForInvoice != null &&
					!this.CA_OriginalLineNo.IsEmpty &&
					this.CA_IsAccountForLine;
			}
		}

		#endregion

		#region For CADCurrentDataDocument

		public Decimal AlcoholPercentByVolume
		{
			get
			{
				if (alcholPercent == null)
				{
					alcholPercent = new CachedProperty<ZDecimal>(Factory, () =>
					JI_CustomsUnitQty == CustomsUnitOfMeasureList.Codes.AlcoholByVolume ? JI_CustomsQuantity : JI_CustomsSecondUnitQty == CustomsUnitOfMeasureList.Codes.AlcoholByVolume ? JI_CustomsSecondQuantity : JI_CustomsThirdUnitQty == CustomsUnitOfMeasureList.Codes.AlcoholByVolume ? JI_CustomsThirdQuantity : ZDecimal.Zero
					);
				}

				return alcholPercent.Value;
			}
		}
		CachedProperty<ZDecimal> alcholPercent;

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CADutyAndTax, typeof(DutyAndTax));
			result.Add(CusAddInfoTypeAttribute.Codes.CAHCPGAHeader, typeof(HCPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CAPHACPGAHeader, typeof(PHACPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader, typeof(NRCanPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CADFOPGAHeader, typeof(DFOPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CAGACPGAHeader, typeof(GACPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader, typeof(CFIAPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CACNSCPGAHeader, typeof(CNSCPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader, typeof(ECCCPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CATCPGAHeader, typeof(TCPGAHeader));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.SITTNumber, typeof(SITTCertificationNumber));
			result.Add(CusCodeDataTypeList.Codes.Permit, typeof(InvoiceLineExportPermit));
			result.Add(CusCodeDataTypeList.Codes.CFIANumber, typeof(CFIARegistrationNumber));
			return result;
		}

		#endregion

		#region IHaveAdditionalDataForBorderWise Members

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return Lookups.Tariffs.TypeOfElements; }
		}

		public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise(IsDataLoadingModule ? "E" : "I", EffectiveDateForDutyRate);
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			if (!IsInDatabase || HasChanges)
			{
				((IAddInfoManager)this).AddInfo.UpdateRelatedPropertyInfo();
			}
			var newInvoiceLine = (JobComInvoiceLine)new CAJobComInvoiceLineDeepCloneStrategy(this, CloneType.DeepTemplateCopy, InvoiceHeader, null).Clone();
			InvoiceHeader?.InvoiceLineLineNumberGenerator.RecalculateWhenAdded(newInvoiceLine);
			return newInvoiceLine;
		}

		#endregion

		public override ZBool ShouldClone
		{
			get { return !Declaration.IsIM2 || !JI_B3LineNumber.EndsWith(SplitLine, StringComparison.OrdinalIgnoreCase); }
		}

		#region Bonded Warehouse

		protected override bool IsGoingIntoBondedWarehouseCore
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsInwardBondedWarehousingEnabled && UseBondedWarehouseAutomation;
			}
		}

		protected override bool UseBondedWarehouseAutomationCore
		{
			get { return !IsRemissionRepairLine && !CA_IsAutoDummyHSCodeCasualImportLine; }
		}

		#endregion

		#region IHasPGARequirements Members

		ZGuid IHasPGARequirements.OA_Manufacturer
		{
			get => JI_OA_ManufacturerAddress;
			set => JI_OA_ManufacturerAddress = value;
		}

		ZPropertyInfo IHasPGARequirements.OA_ManufacturerInfo => JI_OA_ManufacturerAddressInfo;

		ZString IHasPGARequirements.RN_NKCountryOfOrigin
		{
			get => JI_CountryOfOrigin;
			set => JI_CountryOfOrigin = value;
		}

		ZPropertyInfo IHasPGARequirements.RN_NKCountryOfOriginInfo => JI_CountryOfOriginInfo;

		ZString IHasPGARequirements.RW_NKOriginState
		{
			get => JI_StateOrRegionOfOrigin;
			set => JI_StateOrRegionOfOrigin = value;
		}

		ZPropertyInfo IHasPGARequirements.RW_NKOriginStateInfo => JI_StateOrRegionOfOriginInfo;

		ZString IHasPGARequirements.RN_NKCountryOfSource
		{
			get => CA_RN_NKSource;
			set => CA_RN_NKSource = value;
		}

		ZPropertyInfo IHasPGARequirements.RN_NKCountryOfSourceInfo => CA_RN_NKSourceInfo;

		ZString IHasPGARequirements.RW_NKCountryOfSourceState
		{
			get => CA_StateOfSource;
			set => CA_StateOfSource = value;
		}

		ZPropertyInfo IHasPGARequirements.RW_NKCountryOfSourceStateInfo => CA_StateOfSourceInfo;

		ZAddress IHasPGARequirements.OA_ManufacturerAddress_ZAddress => JI_OA_ManufacturerAddress_ZAddress;

		RefCountryCollection IHasPGARequirements.CountryOfOriginsLookup => Lookups.CountryOfOrigins as RefCountryCollection;

		CodeDescriptionPairList IHasPGARequirements.StateCodeListLookup => Lookups.StateCodesList;

		OrgHeaderCollection IHasPGARequirements.ManufacturersLookup => Lookups.SupplierList;

		RefCountryCollection IHasPGARequirements.CountryOfSourceLookup => AddInfoLookups.DefaultOrigins;

		CodeDescriptionPairList IHasPGARequirements.CountryOfSourceStateLookup => AddInfoLookups.StatesOfExport;

		ZString IHasPGARequirements.JI_BrandName
		{
			get
			{
				return JI_BrandName;
			}
			set
			{
				JI_BrandName = value;
			}
		}

		ZPropertyInfo IHasPGARequirements.JI_BrandNameInfo => JI_BrandNameInfo;

		ZString IHasPGARequirements.JI_Model
		{
			get
			{
				return JI_Model;
			}
			set
			{
				JI_Model = value;
			}
		}

		ZPropertyInfo IHasPGARequirements.JI_ModelInfo => JI_ModelInfo;
		ZString IHasPGARequirements.Tariff => JI_Tariff;
		ZPropertyInfo IHasPGARequirements.TariffInfo => JI_TariffInfo;

		#endregion

		#region IPGARequirementSupporter

		ZBool IPGARequirementSupporter.IsDeleted => IsDeleted;
		ZString IPGARequirementSupporter.Tariff => JI_Tariff;
		ZDateTime IPGARequirementSupporter.EffectiveDate => ZDateTime.Today;
		ZBool IPGARequirementSupporter.IsPGARequirementEffective => Declaration?.IsIID ?? false;
		ZPropertyInfo IPGARequirementSupporter.HCIndInfo => CA_HCIndInfo;
		ZPropertyInfo IPGARequirementSupporter.PHACIndInfo => CA_PHACIndInfo;
		ZPropertyInfo IPGARequirementSupporter.NRCanIndInfo => CA_NRCanIndInfo;
		ZPropertyInfo IPGARequirementSupporter.DFOIndInfo => CA_DFOIndInfo;
		ZPropertyInfo IPGARequirementSupporter.GACIndInfo => CA_GACIndInfo;
		ZPropertyInfo IPGARequirementSupporter.ECCCIndInfo => CA_ECCCIndInfo;
		ZPropertyInfo IPGARequirementSupporter.CNSCIndInfo => CA_CNSCIndInfo;
		ZPropertyInfo IPGARequirementSupporter.TCIndInfo => CA_TCIndInfo;
		ZPropertyInfo IPGARequirementSupporter.CFIAIndInfo => CA_CFIAIndInfo;
		IPGAProgramRequirementProvider IPGARequirementSupporter.CFIARequirementProvider => CFIAPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.CNSCRequirementProvider => CNSCPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.DFORequirementProvider => DFOPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.ECCCRequirementProvider => ECCCPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.GACRequirementProvider => GACPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.HCRequirementProvider => HCPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.NRCanRequirementProvider => NRCanPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.PHACRequirementProvider => PHACPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.TCRequirementProvider => TCPGAHeader;
		SetterSuspender IPGARequirementSupporter.SetterSuspender => null;

		#endregion

		#region PGA

		public HCPGAHeader HCPGAHeader => YesNoList.IsYesOrNo(CA_HCInd) ? pgaHeaders.Load<HCPGAHeader>() : Factory.GetNull<HCPGAHeader>();
		public PHACPGAHeader PHACPGAHeader => YesNoList.IsYesOrNo(CA_PHACInd) ? pgaHeaders.Load<PHACPGAHeader>() : Factory.GetNull<PHACPGAHeader>();
		public NRCanPGAHeader NRCanPGAHeader => YesNoList.IsYesOrNo(CA_NRCanInd) ? pgaHeaders.Load<NRCanPGAHeader>() : Factory.GetNull<NRCanPGAHeader>();
		public DFOPGAHeader DFOPGAHeader => YesNoList.IsYesOrNo(CA_DFOInd) ? pgaHeaders.Load<DFOPGAHeader>() : Factory.GetNull<DFOPGAHeader>();
		public GACPGAHeader GACPGAHeader => YesNoList.IsYesOrNo(CA_GACInd) ? pgaHeaders.Load<GACPGAHeader>() : Factory.GetNull<GACPGAHeader>();
		public CFIAPGAHeader CFIAPGAHeader => YesNoList.IsYesOrNo(CA_CFIAInd) ? pgaHeaders.Load<CFIAPGAHeader>() : Factory.GetNull<CFIAPGAHeader>();
		public CNSCPGAHeader CNSCPGAHeader => YesNoList.IsYesOrNo(CA_CNSCInd) ? pgaHeaders.Load<CNSCPGAHeader>() : Factory.GetNull<CNSCPGAHeader>();
		public ECCCPGAHeader ECCCPGAHeader => YesNoList.IsYesOrNo(CA_ECCCInd) ? pgaHeaders.Load<ECCCPGAHeader>() : Factory.GetNull<ECCCPGAHeader>();
		public TCPGAHeader TCPGAHeader => YesNoList.IsYesOrNo(CA_TCInd) ? pgaHeaders.Load<TCPGAHeader>() : Factory.GetNull<TCPGAHeader>();

		internal readonly CusAddInfoChildrenCollection pgaHeaders;

		internal void RefreshInvoiceLinesWithPGAs()
		{
			Declaration?.RefreshInvoiceLinesWithPGAs();
			InvoiceHeader?.RefreshInvoiceLinesWithPGAs();
		}

		public void AddDefaultCFIARegistrationNumber()
		{
			var cusCode = Declaration?.EffectiveImporter?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense, Core.Constants.CountryCodes.Canada);
			AddDefaultCFIARegistrationNumber(cusCode);
		}

		public void AddDefaultCFIARegistrationNumber(OrgCusCode cusCode)
		{
			if (cusCode != null)
			{
				var dataAutoAdded = CFIARegistrationNumbers.Cast<CFIARegistrationNumber>().FirstOrDefault(x => x.CY_Code == RegistrationNumberHelper.SafeFoodForCanadiansLicence);
				if (dataAutoAdded == null)
				{
					CFIARegistrationNumbers.AddNew(RegistrationNumberHelper.SafeFoodForCanadiansLicence, cusCode.OK_CustomsRegNo);
				}
			}
		}

		public void ClearPGAIndicators()
		{
			CA_CFIAInd = ZString.Empty;
			CA_CNSCInd = ZString.Empty;
			CA_DFOInd = ZString.Empty;
			CA_ECCCInd = ZString.Empty;
			CA_GACInd = ZString.Empty;
			CA_HCInd = ZString.Empty;
			CA_NRCanInd = ZString.Empty;
			CA_PHACInd = ZString.Empty;
			CA_TCInd = ZString.Empty;
		}

		public PGAContactDetails GetContactDetails(ZString type)
		{
			if (type == LPCOHolderPartyTypeCodes.Codes.Broker)
			{
				var staff = Declaration?.CusAgent;
				var orgProxy = Declaration?.EffectiveBranch?.OrgProxy;

				return new PGAContactDetails(orgProxy, staff);
			}

			var orgHeader = this.GetLPCOHolderParty(type);
			return new PGAContactDetails(orgHeader);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fB2AsClaimedInvoiceLineSynchroniser != null)
				{
					fB2AsClaimedInvoiceLineSynchroniser.SetEnabled(false, fB2AsClaimedInvoiceLineSynchroniser.DetectEnabled);
					fB2AsClaimedInvoiceLineSynchroniser.Dispose();
					fB2AsClaimedInvoiceLineSynchroniser = null;
				}

				if (repairLineSynchroniser != null)
				{
					repairLineSynchroniser.SetEnabled(false, repairLineSynchroniser.DetectEnabled);
					repairLineSynchroniser.Dispose();
					repairLineSynchroniser = null;
				}

				if (luxuryTaxInvoiceLineSynchroniser != null)
				{
					luxuryTaxInvoiceLineSynchroniser.SetEnabled(false, luxuryTaxInvoiceLineSynchroniser.DetectEnabled);
					luxuryTaxInvoiceLineSynchroniser.Dispose();
					luxuryTaxInvoiceLineSynchroniser = null;
				}

				if (dutyAndTaxManager != null)
				{
					((IDisposable)dutyAndTaxManager).Dispose();
					dutyAndTaxManager = null;
				}
			}
		}

		#endregion

		event EventHandler IDutyAndTaxData.QuantityChanged
		{
			add
			{
				JI_CustomsQuantityInfo.ValueChanged += value;
				JI_CustomsUnitQtyInfo.ValueChanged += value;
				JI_CustomsSecondQuantityInfo.ValueChanged += value;
				JI_CustomsSecondUnitQtyInfo.ValueChanged += value;
				JI_CustomsThirdQuantityInfo.ValueChanged += value;
				JI_CustomsThirdUnitQtyInfo.ValueChanged += value;
			}
			remove
			{
				JI_CustomsQuantityInfo.ValueChanged -= value;
				JI_CustomsUnitQtyInfo.ValueChanged -= value;
				JI_CustomsSecondQuantityInfo.ValueChanged -= value;
				JI_CustomsSecondUnitQtyInfo.ValueChanged -= value;
				JI_CustomsThirdQuantityInfo.ValueChanged -= value;
				JI_CustomsThirdUnitQtyInfo.ValueChanged -= value;
			}
		}

		protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage)
		{
			return new InvoiceLinePackageValidation(linkPackage, this);
		}

		protected override ZBool IsSupportEmptyPackType(BasePackage package)
		{
			var result = base.IsSupportEmptyPackType(package);

			if (!result && Declaration != null && Declaration.IsImport && Declaration.IsIID)
			{
				if (package.CW_PackQty == package.TotalUsageCount)
				{
					result = true;
				}
			}

			return result;
		}

		public override void CopyHazMatCodeFromUNDGs(Customs.Business.OrgSupplierPart product)
		{
			if (product != null && IsIIDDeclaration)
			{
				var uNDGs = product.UNDGs;
				JI_HazMatCode = uNDGs.Count > 0 && uNDGs.FirstOrDefault().UNDGSubstance != null ? uNDGs.FirstOrDefault().UNDGSubstance.DG_Code : ZString.Empty;
			}
		}

		void CalculateAMMVCharge()
		{
			ApportionedCharges.OfType<InvoiceLineApportionCharge>().Where(ch => ch.IsAMMV()).DeleteAll();
			if ((!CA_AMMVPerUnit.IsEmpty && !JI_InvoiceQuantity.IsEmpty) || (!CA_AMMVPercentage.IsEmpty && !JI_LinePrice.IsEmpty))
			{
				var ammvCharge = ApportionedCharges.AddNewAMMVCharge();
				if (!CA_AMMVPerUnit.IsEmpty && !JI_InvoiceQuantity.IsEmpty)
				{
					ammvCharge.J7_Amount = CA_AMMVPerUnit * JI_InvoiceQuantity;
					var ammvPerUnitCurrency = Pivot?.CCA_AMMVPerUnitCurrency;
					ammvCharge.J7_RX_NKCurrency = string.IsNullOrEmpty(ammvPerUnitCurrency) ? (ZString)Core.Constants.CurrencyCodes.Canada : (ZString)ammvPerUnitCurrency;
				}
				else
				{
					ammvCharge.J7_Amount = CA_AMMVPercentage * JI_LinePrice / 100;
					ammvCharge.J7_RX_NKCurrency = JI_RX_NKLinePriceCurr;
					ammvCharge.J7_Percentage = CA_AMMVPercentage;
				}
			}
		}
	}
}
