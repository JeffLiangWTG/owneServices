using System.Collections.Generic;
using System.Data;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US
{
	#region USCAESResponseCode

	public class USCAESResponseCode : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCAESResponseCode"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCAESResponseCode
(
	UY_PK uniqueidentifier CONSTRAINT PK_USCAESResponseCode PRIMARY KEY NONCLUSTERED,
	UY_Code char (3) NOT NULL,
	UY_Severity char (13) NOT NULL CONSTRAINT DF_USCAESResponseCode_UY_Severity DEFAULT (''),
	UY_NarrativeText char (50) NOT NULL CONSTRAINT DF_USCAESResponseCode_UY_NarrativeText DEFAULT (''),
	CONSTRAINT UY_Code CHECK ([UY_Code] <> '')
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "NR_IX__UY_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_IX__UY_Code ON USCAESResponseCode (UY_Code)" } }; }
		}

		#endregion
	}

	#endregion

	#region USCAffirmationOfCompliance

	public class USCAffirmationOfCompliance : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCAffirmationOfCompliance"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCAffirmationOfCompliance
(
	UL_PK uniqueidentifier NOT NULL,
	UL_Code char(3) NOT NULL CONSTRAINT DF_USCAffirmationOfCompliance_UL_Code DEFAULT (''),
	UL_Description varchar(45) NOT NULL CONSTRAINT DF_USCAffirmationOfCompliance_UL_Description DEFAULT (''),
	UL_QualifierIndicator char(1) NOT NULL CONSTRAINT DF_USCAffirmationOfCompliance_UL_QualifierIndicator DEFAULT ('N'),
	UL_IsExpired bit NOT NULL CONSTRAINT DF_USCAffirmationOfCompliance_UL_IsExpired DEFAULT (0),
	CONSTRAINT PK_USCAffirmationOfCompliance PRIMARY KEY NONCLUSTERED (UL_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "NR_UX__UL_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_UX__UL_Code ON USCAffirmationOfCompliance (UL_Code)" } }; }
		}

		#endregion
	}

	#endregion

	#region USCAntiDumpingBondCashIndicator

	public class USCAntiDumpingBondCashIndicator : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCAntiDumpingBondCashIndicator"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCAntiDumpingBondCashIndicator
(
	UV_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCAntiDumpingBondCashIndicator_UV_PK DEFAULT (newid()),
	UV_UY uniqueidentifier NOT NULL,
	UV_BondCashIndicator varchar(1) NOT NULL,
	UV_BondCashDate datetime NOT NULL,
	CONSTRAINT PK_USCAntiDumpingBondCashIndicator PRIMARY KEY NONCLUSTERED (UV_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "FK_RX__UV_UY", CreateIndexScript = "CREATE NONCLUSTERED INDEX FK_RX__UV_UY ON USCAntiDumpingBondCashIndicator (UV_UY ASC)" } }; }
		}

		#endregion
	}

	#endregion

	#region USCAntiDumpingCase

	public class USCAntiDumpingCase : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCAntiDumpingCase"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCAntiDumpingCase
(
	UY_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_PK DEFAULT (newid()),
	UY_ISOCountryCode char(2) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_ISOCountryCode DEFAULT (''),
	UY_CaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_CaseNumber DEFAULT (''),
	UY_RelatedCaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_RelatedCaseNumber DEFAULT (''),
	UY_ManufactureIDCode varchar(15) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_ManufactureIDCode DEFAULT (''),
	UY_ShipperID varchar(15) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_ShipperID DEFAULT (''),
	UY_CaseStatus varchar(1) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_CaseStatus DEFAULT (''),
	UY_CaseStatusDate datetime NOT NULL,
	UY_BondCashIndicator varchar(1) NOT NULL,
	UY_Contact varchar(20) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_Contact DEFAULT (''),
	UY_Phone varchar(10) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_Phone DEFAULT (''),
	UY_LiquidationSuspensionDate datetime NULL,
	UY_ShortDescription varchar(30) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_ShortDescription DEFAULT (''),
	UY_ManufacturerName varchar(50) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_ManufacturerName DEFAULT (''),
	UY_Shipper varchar(50) NOT NULL CONSTRAINT DF_USCAntiDumpingCase_UY_Shipper DEFAULT (''),
	CONSTRAINT PK_USCAntiDumpingCase PRIMARY KEY NONCLUSTERED (UY_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "NR_UX__UY_CaseNumber", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX NR_UX__UY_CaseNumber ON USCAntiDumpingCase (UY_CaseNumber ASC)" } }; }
		}

		#endregion
	}

	#endregion

	#region USCAntiDumpingRate

	public class USCAntiDumpingRate : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCAntiDumpingRate"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCAntiDumpingRate
(
	UW_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCAntiDumpingRate_UW_PK DEFAULT (newid()),
	UW_UY uniqueidentifier NOT NULL,
	UW_RateIndicator char(1) NOT NULL CONSTRAINT DF_USCAntiDumpingRate_UW_RateIndicator DEFAULT ('Y'),
	UW_DepositRate1 decimal(5, 4) NOT NULL CONSTRAINT DF_USCAntiDumpingRate_UW_DepositRate1 DEFAULT (0),
	UW_DepositRate2 decimal(5, 4) NOT NULL CONSTRAINT DF_USCAntiDumpingRate_UW_DepositRate2 DEFAULT (0),
	UW_DepositRate3 decimal(5, 4) NOT NULL CONSTRAINT DF_USCAntiDumpingRate_UW_DepositRate3 DEFAULT (0),
	UW_EffectiveEntryDate datetime NULL,
	UW_EffectiveExportDate datetime NULL,
	CONSTRAINT PK_USCAntiDumpingRate PRIMARY KEY NONCLUSTERED (UW_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "FK_RX__UW_UY", CreateIndexScript = "CREATE NONCLUSTERED INDEX FK_RX__UW_UY ON USCAntiDumpingRate (UW_UY ASC)" } }; }
		}

		#endregion
	}

	#endregion

	#region USCAntiDumpingTariff

	public class USCAntiDumpingTariff : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCAntiDumpingTariff"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCAntiDumpingTariff
(
	UX_PK uniqueidentifier NOT NULL,
	UX_UY uniqueidentifier NOT NULL,
	UX_Tariff varchar(10) NOT NULL CONSTRAINT DF_USCAntiDumpingTariff_UX_Tariff DEFAULT (''),
	CONSTRAINT PK_USCAntiDumpingTariff PRIMARY KEY NONCLUSTERED (UX_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "FK_RX__UX_UY", CreateIndexScript = "CREATE NONCLUSTERED INDEX FK_RX__UX_UY ON USCAntiDumpingTariff (UX_UY ASC)" },
					new IndexScript { IndexName = "FK_RX__UX_Tariff", CreateIndexScript = "CREATE NONCLUSTERED INDEX FK_RX__UX_Tariff ON USCAntiDumpingTariff (UX_Tariff ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCCarrier

	public class USCCarrier : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCCarrier"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCCarrier
(
	UI_PK uniqueidentifier NOT NULL,
	UI_Code varchar(4) NOT NULL,
	UI_Name varchar(35) NOT NULL,
	UI_ModeOfTransportation varchar(2) NOT NULL,
	UI_Address varchar(105) NOT NULL,
	UI_AirwayBillPrefix varchar(3) NOT NULL,
	CONSTRAINT PK_USCCarrier PRIMARY KEY NONCLUSTERED (UI_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] {
				new IndexScript { IndexName = "NR_IX__USCCarrier_UI_Code", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_IX__USCCarrier_UI_Code ON USCCarrier (UI_Code)" }
			};
			}
		}

		#endregion
	}

	#endregion

	#region USCChapter

	public class USCChapter : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCChapter"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCChapter
(
	UK_PK uniqueidentifier NOT NULL,
	UK_Chapter char(2) NOT NULL,
	UK_Description varchar(max) NOT NULL,
	UK_Notes varchar(max) NOT NULL,
	UK_UP uniqueidentifier NOT NULL
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return System.Array.Empty<IndexScript>(); }
		}

		#endregion
	}

	#endregion

	#region USCCountry

	public class USCCountry : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCCountry"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCCountry
(
	UC_PK uniqueidentifier CONSTRAINT PK_USCCountry PRIMARY KEY NONCLUSTERED,UC_ISOCountryCode char (2) NOT NULL,
	UC_RateColumnIndicator varchar (1) NOT NULL,
	UC_RateColumnBeginDate datetime NULL,
	UC_RateColumnEndDate datetime NULL,
	UC_RestrictionIndicator char (1) NOT NULL CONSTRAINT DF_USCCountry_UC_RestrictionIndicator DEFAULT ('N'),
	UC_RestrictionIndicatorBeginDate datetime NULL,
	UC_RestrictionIndicatorEndDate datetime NULL,
	UC_GSPIndicator char (1) NOT NULL CONSTRAINT DF_USCCountry_UC_GSPIndicator DEFAULT ('N'),
	UC_GSPBeginDate datetime NULL,
	UC_GSPEndDate datetime NULL,
	UC_SPICode varchar (1) NOT NULL,
	UC_SPIBeginDate datetime NULL,
	UC_SPIEndDate datetime NULL,
	UC_Name varchar (30) NOT NULL,
	UC_CurrencyName varchar (15) NOT NULL,
	UC_CurrencyCode char (3) NOT NULL,
	UC_DrawbackEligibility char (1) NOT NULL CONSTRAINT DF_USCCountry_UC_DrawbackEligibility DEFAULT ('N'),
	UC_SheduleCCountryCode char (4) NOT NULL,
	UC_SpecialTradeProgramsIndicator varchar (1) NOT NULL,
	UC_SpecialTradeProgramsBeginDate datetime NULL,
	UC_SpecialTradeProgramsEndDate datetime NULL,
	UC_LesserDevelopedCountry char (1) NOT NULL CONSTRAINT DF_USCCountry_UC_LesserDevelopedCountry DEFAULT ('N'),
	UC_MiscellaneousSPIIndicator varchar (2) NOT NULL,
	UC_MiscellaneousSPIBeginDate datetime NULL,
	UC_MiscellaneousSPIEndDate datetime NULL,
	UC_Code char (2) NOT NULL DEFAULT('')
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "NR_IX__UC_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_IX__UC_Code ON USCCountry (UC_ISOCountryCode )" } }; }
		}

		#endregion
	}

	#endregion

	#region USCFDAProductNumber

	public class USCFDAProductNumber : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCFDAProductNumber"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCFDAProductNumber 
(
	UP_PK uniqueidentifier NOT NULL CONSTRAINT DF_UP_PK DEFAULT (newid()),
	UP_Code varchar(7) NOT NULL CONSTRAINT DF_UP_Code DEFAULT (''),
	UP_Desc varchar(60) NOT NULL CONSTRAINT DF_UP_Desc DEFAULT (''),
	CONSTRAINT PK_UX__UP_PK PRIMARY KEY NONCLUSTERED (UP_PK)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_UX__UP_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_UX__UP_Code ON USCFDAProductNumber (UP_Code)" },
					new IndexScript { IndexName = "NR_RX__UP_Desc", CreateIndexScript = "CREATE INDEX NR_RX__UP_Desc ON USCFDAProductNumber (UP_Desc)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCFIRMS

	public class USCFIRMS : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCFIRMS"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCFIRMS 
(
	US_PK uniqueidentifier NOT NULL ,
	US_Code char(4) NOT NULL ,
	US_DistrictPortCode char(4) NOT NULL ,
	US_IsActive char(1) NOT NULL ,
	US_FacilityType char(2) NOT NULL ,
	US_Name varchar(35) NOT NULL ,
	US_LastUpdate datetime NULL ,
	US_Address varchar(35) NOT NULL ,
	US_City varchar(35) NOT NULL ,
	US_State char(2) NOT NULL ,
	US_ZipCode varchar(9) NOT NULL ,
	US_Country char(2) NOT NULL ,
	CONSTRAINT PK_USCFIRMS PRIMARY KEY NONCLUSTERED (US_PK)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "NR_UX__US_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_UX__US_Code ON USCFIRMS (US_Code)" } }; }
		}

		#endregion
	}

	#endregion

	#region USCForeignPort

	public class USCForeignPort : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCForeignPort"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCForeignPort
(
	UH_PK uniqueidentifier CONSTRAINT PK_USCForeignPort PRIMARY KEY NONCLUSTERED,
	UH_Code varchar(5) NOT NULL CONSTRAINT DF_USCForeignPort_UF_Code DEFAULT (''),
	UH_Name varchar(30) NOT NULL CONSTRAINT DF_USCForeignPort_UF_Name DEFAULT (''),
	UH_ValidForType varchar(3) NOT NULL CONSTRAINT DF_USCForeignPort_UH_ValidForType DEFAULT ('')
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_IX__UH_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX NR_IX__UH_Code ON USCForeignPort(UH_Code, UH_ValidForType)" },
					new IndexScript { IndexName = "NR_IX__UH_ValidForType", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_IX__UH_ValidForType ON USCForeignPort(UH_ValidForType)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCGoldPrice

	public class USCGoldPrice : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCGoldPrice"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCGoldPrice 
(
	UG_PK uniqueidentifier CONSTRAINT PK_USCGoldPrice PRIMARY KEY NONCLUSTERED,
	UG_Date datetime NOT NULL,
	UG_Narrative varchar (30) NOT NULL,
	UG_Price decimal(10, 2) NOT NULL
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return System.Array.Empty<IndexScript>();
			}
		}

		#endregion
	}

	#endregion

	#region USCQuota

	public class USCQuota : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCQuota"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCQuota
(
	UT_PK uniqueidentifier NOT NULL,
	UT_Code varchar(10) NOT NULL DEFAULT (''),
	UT_BeginDate datetime NOT NULL,
	UT_EndDate datetime NOT NULL,
	UT_QuotaType varchar(3) NOT NULL DEFAULT (''),
	UT_UC_NKOriginCountry char(2) NOT NULL DEFAULT (''),
	UT_FirstNamesake varchar(15) NOT NULL DEFAULT (''),
	UT_SecondNamesake varchar(9) NOT NULL DEFAULT (''),
	UT_QuotaLimit decimal(11, 0) NOT NULL DEFAULT (0),
	UT_QuotaUQ varchar(3) NOT NULL DEFAULT (''),
	UT_TextileConversionFactor decimal(10, 5) NOT NULL DEFAULT (0),
	UT_QuotaPeriod varchar(2) NOT NULL DEFAULT (''),
	UT_ThresholdQty decimal(11, 0) NOT NULL DEFAULT (0),
	UT_PeriodProcessDateIndicator varchar(3) NOT NULL DEFAULT (''),
	UT_QuotaStatus varchar(3) NOT NULL DEFAULT (''),
	UT_QuotaLimitType varchar(3) NOT NULL DEFAULT (''),
	UT_QtyToDate decimal(11, 0) NOT NULL,
	UT_IsHeld char(1) NOT NULL DEFAULT ('N'),
	UT_LastTrasactionDate datetime NULL,
	UT_LastUpdateDate datetime NOT NULL,
	UT_SecondTariffNo varchar(10) NOT NULL DEFAULT ('')
	CONSTRAINT PK_USCQuota PRIMARY KEY NONCLUSTERED (UT_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return System.Array.Empty<IndexScript>();
			}
		}

		#endregion
	}

	#endregion

	#region USCRegionDistrictPort

	public class USCRegionDistrictPort : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCRegionDistrictPort"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCRegionDistrictPort 
(
	UR_PK uniqueidentifier NOT NULL,
	UR_Code varchar (4) NOT NULL ,
	UR_Name varchar (24) NOT NULL CONSTRAINT DF__UR_Name DEFAULT (''),
	UR_Address1 varchar (32) NOT NULL CONSTRAINT DF__UR_Address1 DEFAULT (''),
	UR_Address2 varchar (32) NOT NULL CONSTRAINT DF__UR_Address2 DEFAULT (''),
	UR_Address3 varchar (32) NOT NULL CONSTRAINT DF__UR_Address3 DEFAULT (''),
	UR_PortCarrierType varchar (1) NOT NULL CONSTRAINT DF__UR_PortCarrierType DEFAULT (''),
	UR_PrimaryTeamLocation char (4) NOT NULL CONSTRAINT DF__UR_PrimaryTeamLocation DEFAULT (''),
	UR_SecondaryTeamLocation char (4) NOT NULL CONSTRAINT DF__UR_SecondaryTeamLocation DEFAULT (''),
	UR_PortOfUnlading char (1) NOT NULL CONSTRAINT DF__UR_PortOfUnlading DEFAULT (''),
	UR_City varchar (15) NOT NULL CONSTRAINT DF__UR_City DEFAULT (''),
	UR_State char (2) NOT NULL CONSTRAINT DF__UR_State DEFAULT (''),
	UR_ZipCode varchar (9) NOT NULL CONSTRAINT DF__UR_ZipCode DEFAULT (''),
	UR_SubPortCode char (5) NOT NULL CONSTRAINT DF__UR_SubPortCode DEFAULT (''),
	UR_Prefix varchar (1) NOT NULL CONSTRAINT DF__UR_Prefix DEFAULT (''),
	CONSTRAINT PK_USCRegionDistrictPort PRIMARY KEY NONCLUSTERED (UR_PK),
	CONSTRAINT CK__UR_Code CHECK ([UR_Code] <> ''),
	CONSTRAINT CK__UR_PortCarrierType CHECK ([UR_PortCarrierType] = '' or [UR_PortCarrierType] = 'A' or [UR_PortCarrierType] = 'V' or [UR_PortCarrierType] = 'B'),
	CONSTRAINT CH__UR_PortOfUnlading CHECK ([UR_PortOfUnlading] = 'N' or [UR_PortOfUnlading] = 'Y')
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] { new IndexScript { IndexName = "NR_IX__UR_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_IX__UR_Code ON USCRegionDistrictPort(UR_Code)" } };
			}
		}

		#endregion
	}

	#endregion

	#region USCRule

	public class USCRule : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCRule"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCRule
(
	U0_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCRule_U0_PK DEFAULT (newid()),
	U0_Code varchar(3) NOT NULL DEFAULT ('')
	CONSTRAINT PK_USCRule PRIMARY KEY NONCLUSTERED (U0_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] { new IndexScript { IndexName = "NR_UX__U0_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_UX__U0_Code ON USCRule (U0_Code)" } };
			}
		}

		public string CsvFileName
		{
			get { return "USCRule.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "U0_Code"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.VarChar }; }
		}

		#endregion
	}

	#endregion

	#region USCScheduleB

	public class USCScheduleB : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCScheduleB"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCScheduleB 
(
	UB_PK uniqueidentifier CONSTRAINT PK_USCScheduleB PRIMARY KEY NONCLUSTERED,
	UB_Code varchar (10) NOT NULL CONSTRAINT CK_USCScheduleB_UB_Code CHECK ([UB_Code] <> ''),
	UB_Unit1 varchar (3) NULL CONSTRAINT DF_USCScheduleB_UB_StatisticalUnit1 DEFAULT (''),
	UB_Unit2 varchar (3) NULL CONSTRAINT DF_UB_StatisticalUnit2 DEFAULT (''),
	UB_ShortDescription varchar (50) NULL CONSTRAINT DF_USCScheduleB_UB_ShortDescription DEFAULT ('')
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] { new IndexScript { IndexName = "NR_IX__UB_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_IX__UB_Code ON USCScheduleB(UB_Code)" } };
			}
		}

		#endregion
	}

	#endregion

	#region USCSection

	public class USCSection : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCSection"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCSection
(
	UP_PK uniqueidentifier NOT NULL,
	UP_Section tinyint NOT NULL,
	UP_Description varchar(max) NOT NULL,
	UP_Notes varchar(max) NOT NULL
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return System.Array.Empty<IndexScript>();
			}
		}

		#endregion
	}

	#endregion

	#region USCTariff

	public class USCTariff : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTariff"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTariff 
(
	UE_PK uniqueidentifier CONSTRAINT PK_USCTariff PRIMARY KEY NONCLUSTERED,
	UE_Tariff varchar (10) NOT NULL CONSTRAINT DF_USCTariff_UE_Tariff DEFAULT (''),
	UE_DateFrom datetime NOT NULL,
	UE_DateTo datetime NOT NULL,
	UE_NumberOfReportingUnits tinyint NOT NULL CONSTRAINT DF_USCTariff_UE_NumberOfReportingUnits DEFAULT (0),
	UE_Unit1 varchar (3) NOT NULL CONSTRAINT DF_USCTariff_UE_StatisticalUnit1 DEFAULT (''),
	UE_Unit2 varchar (3) NOT NULL CONSTRAINT DF_USCTariff_UE_StatisticalUnit2 DEFAULT (''),
	UE_Unit3 varchar (3) NOT NULL CONSTRAINT DF_USCTariff_UE_Unit3 DEFAULT (''),
	UE_DutyComputationCode varchar (1) NOT NULL CONSTRAINT DF_USCTariff_UE_DutyComputationCode DEFAULT (''),
	UE_ShortDescription varchar (30) NOT NULL CONSTRAINT DF_USCTariff_UE_ShortDescription DEFAULT (''),
	UE_IsBaseRate char (1) NOT NULL CONSTRAINT DF_USCTariff_US_IsBaseRate DEFAULT ('N'),
	UE_Column1RateSpecific decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariff_UE_Column1RateSpecific DEFAULT (0),
	UE_Column1RateAdValorem decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariff_UE_ColumnRate1AdValorem DEFAULT (0),
	UE_Column1RateOther decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariff_UE_ColumnRate1Other DEFAULT (0),
	UE_Column2RateSpecific decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariff_UE_ColumnRate2Specific DEFAULT (0),
	UE_Column2RateAdValorem decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariff_UE_ColumnRate2AdValorem DEFAULT (0),
	UE_Column2RateOther decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariff_UE_ColumnRate2Other DEFAULT (0),
	UE_CountervailingDutyFlag char (1) NOT NULL CONSTRAINT DF_USCTariff_UE_CountervailingDutyFlag DEFAULT ('N'),
	UE_AdditionalTariffNumberIndicator char (1) NOT NULL CONSTRAINT DF_USCTariff_UE_AdditionalTariffNumberIndicator DEFAULT ('N'),
	UE_PermitLicenseIndicator char (2) NOT NULL CONSTRAINT DF_USCTariff_UE_PermitLicenseIndicator DEFAULT (''),
	UE_GSPExcludedCountries varchar (20) NOT NULL CONSTRAINT DF_USCTariff_UE_GSPExcludedCountries DEFAULT (''),
	UE_OGACodes varchar (75) NOT NULL CONSTRAINT DF_USCTariff_UE_OGACodes DEFAULT (''),
	UE_AntiDumping char (1) NOT NULL CONSTRAINT DF_USCTariff_UE_AntiDumping DEFAULT ('N'),
	UE_QuotaIndicator char (1) NOT NULL CONSTRAINT DF_USCTariff_UE_QuotaIndicator DEFAULT ('N'),
	UE_TextileCategoryNumber char (3) NOT NULL CONSTRAINT DF_USCTariff_UE_TextileCategoryNumber DEFAULT (''),
	UE_SPICode varchar (60) NOT NULL CONSTRAINT DF_USCTariff_UE_SPICode DEFAULT (''),
	UE_ISOCountryofOriginEditCode char (2) NOT NULL CONSTRAINT DF_USCTariff_UE_ISOCountryofOriginEditCode DEFAULT ('')
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] { new IndexScript { IndexName = "NR_IX__USCTariff", CreateIndexScript = "CREATE UNIQUE INDEX NR_IX__USCTariff ON USCTariff(UE_Tariff, UE_DateFrom, UE_DateTo)" } };
			}
		}

		#endregion
	}

	#endregion

	#region USCTariffDateRestriction

	public class USCTariffDateRestriction : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTariffDateRestriction"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTariffDateRestriction 
(
	UF_PK uniqueidentifier CONSTRAINT PK_USCTariffDateRestriction PRIMARY KEY NONCLUSTERED,
	UF_UE uniqueidentifier NOT NULL,
	UF_EntryDateRestrictionCode varchar (1) NOT NULL,
	UF_EntryDateRestrictionFrom smallint NOT NULL,
	UF_EntryDateRestrictionTo smallint NOT NULL 
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return System.Array.Empty<IndexScript>();
			}
		}

		#endregion
	}

	#endregion

	#region USCTariffDutyRate

	public class USCTariffDutyRate : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTariffDutyRate"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTariffDutyRate 
(
	UD_PK uniqueidentifier CONSTRAINT PK_USCTariffDutyRate PRIMARY KEY NONCLUSTERED,
	UD_UE uniqueidentifier NOT NULL,
	UD_DutyElement varchar (1) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_DutyElement DEFAULT (''),
	UD_ISOCountryCode char (2) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_ISOCountryCode DEFAULT (''),
	UD_SpecificSpecialRate decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_SpecificSpecialRate DEFAULT (0),
	UD_AdValoremSpecialRate decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_AdValoremSpecialRate DEFAULT (0),
	UD_OtherSpecialRate decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_OtherSpecialRate DEFAULT (0),
	UD_TaxFeeClassCode char (3) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_TaxFeeClassCode DEFAULT (''),
	UD_TaxFeeComputationCode varchar (1) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_TaxFeeComputationCode DEFAULT (''),
	UD_TaxFeeFlag varchar (1) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_TaxFeeFlag DEFAULT (''),
	UD_TaxFeeSpecificRate decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_TaxFeeSpecificRate DEFAULT (0),
	UD_TaxFeeAdvalorem decimal(12, 8) NOT NULL CONSTRAINT DF_USCTariffDutyRate_UD_TaxFeeAdvalorem DEFAULT (0)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] { new IndexScript { IndexName = "IX__UD_UE", CreateIndexScript = "CREATE INDEX IX__UD_UE ON USCTariffDutyRate(UD_UE)" } };
			}
		}

		#endregion
	}

	#endregion

	#region USCTariffQuantity

	public class USCTariffQuantity : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTariffQuantity"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTariffQuantity 
(
	UQ_PK uniqueidentifier CONSTRAINT PK_USCTariffQuantity PRIMARY KEY NONCLUSTERED,
	UQ_UE uniqueidentifier NOT NULL,
	UQ_QuantityEditCode varchar (3) NOT NULL,
	UQ_LowerBound decimal(10, 5) NOT NULL,
	UQ_UpperBound decimal(10, 5) NOT NULL 
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "FK_RX__UQ_UE", CreateIndexScript = "CREATE NONCLUSTERED INDEX FK_RX__UQ_UE ON USCTariffQuantity (UQ_UE ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCTariffRule

	public class USCTariffRule : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTariffRule"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTariffRule
(
	U1_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCTariffRule_U1_PK DEFAULT (newid()),
	U1_RuleCode varchar(3) NOT NULL DEFAULT (''),
	U1_Tariff varchar(10) NOT NULL DEFAULT (''),
	U1_DateFrom datetime NOT NULL,
	U1_DateTo datetime NULL,
	U1_TariffTo varchar(10) NOT NULL DEFAULT (''),
	CONSTRAINT PK_USCTariffRule PRIMARY KEY NONCLUSTERED (U1_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] {
					new IndexScript { IndexName = "NR_IX__USCTariffRule", CreateIndexScript = "CREATE UNIQUE INDEX NR_IX__USCTariffRule ON USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo)" },
					new IndexScript { IndexName = "NR_RX__U1_RuleCode", CreateIndexScript = "CREATE INDEX NR_RX__U1_RuleCode ON USCTariffRule (U1_RuleCode)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCTariffRuleException

	public class USCTariffRuleException : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTariffRuleException"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTariffRuleException
(
	U2_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCTariffRuleException_U2_PK DEFAULT (newid()),
	U2_U1 uniqueidentifier NOT NULL CONSTRAINT USCTariffRuleException_U2_U1_FK2_USCTariffRule_RRR_120N FOREIGN KEY(U2_U1) REFERENCES USCTariffRule (U1_PK),
	U2_Tariff varchar(10) NOT NULL DEFAULT (''),
	U2_DateFrom datetime NULL,
	U2_DateTo datetime NULL,
	U2_TariffTo varchar(10) NOT NULL DEFAULT(''),
	CONSTRAINT PK_USCTariffRuleException PRIMARY KEY NONCLUSTERED (U2_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] {
					new IndexScript { IndexName = "NR_IX__USCTariffRuleException_U2_U1", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_IX__USCTariffRuleException_U2_U1 ON USCTariffRuleException (U2_U1)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCTariffValue

	public class USCTariffValue : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTariffValue"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTariffValue 
(
	UA_PK uniqueidentifier CONSTRAINT PK_USCTariffValue PRIMARY KEY NONCLUSTERED,
	UA_UE uniqueidentifier NOT NULL,
	UA_ValueEditCode char (3) NOT NULL,
	UA_ValueLowBounds decimal(10, 5) NOT NULL,
	UA_ValueHighBounds decimal(10, 5) NOT NULL
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "FK_RX__UA_UE", CreateIndexScript = "CREATE NONCLUSTERED INDEX FK_RX__UA_UE ON USCTariffValue (UA_UE ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCTeamSpecialist

	public class USCTeamSpecialist : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCTeamSpecialist"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCTeamSpecialist
(
	UJ_PK uniqueidentifier NOT NULL,
	UJ_DistrictPortCode char(4) NOT NULL,
	UJ_FieldImportSpecialistTeamNumber char(3) NOT NULL,
	UJ_TariffNumberFrom varchar(10) NOT NULL,
	UJ_TariffNumberTo varchar(10) NOT NULL,
	UJ_CountryFrom varchar(4) NOT NULL,
	UJ_CountryTo varchar(4) NOT NULL,
	UJ_ImporterOfRecordName varchar(32) NOT NULL,
	CONSTRAINT PK_USCTeamSpecialist PRIMARY KEY NONCLUSTERED (UJ_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return System.Array.Empty<IndexScript>();
			}
		}

		#endregion
	}

	#endregion

	#region USCVisa

	public class USCVisa : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCVisa"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCVisa
(
	UO_PK uniqueidentifier NOT NULL,
	UO_TextileCategoryNo varchar(5) NOT NULL,
	UO_BeginDate datetime NOT NULL,
	UO_EndDate datetime NOT NULL,
	UO_UC_NKOriginCountry char(2) NOT NULL DEFAULT (''),
	UO_IsStardardVisaFormat char(1) NOT NULL DEFAULT ('N'),
	UO_IsELVIS char(1) NOT NULL DEFAULT ('N'),
	UO_IsVisaExemptForSample char(1) NOT NULL DEFAULT ('N'),
	UO_IsVisaExemptForForklore char(1) NOT NULL DEFAULT ('N'),
	UO_IsVisaQuantityIndicated char(1) NOT NULL DEFAULT ('N'),
	UO_IsExceptionToVisaReq char(1) NOT NULL DEFAULT ('N'),
	UO_IsPartCategory char(1) NOT NULL DEFAULT ('N'),
	UO_IsSpecialProgram char(1) NOT NULL DEFAULT ('N'),
	CONSTRAINT PK_USCVisa PRIMARY KEY NONCLUSTERED (UO_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return System.Array.Empty<IndexScript>();
			}
		}

		#endregion
	}

	#endregion

	#region USCVisaTariff

	public class USCVisaTariff : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCVisaTariff"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCVisaTariff
(
	UK_PK uniqueidentifier NOT NULL,
	UK_Tariff varchar(10) NOT NULL,
	UK_UO uniqueidentifier NOT NULL CONSTRAINT USCVisaTariff_UK_UO_FK2_USCVisa_RRR_120N FOREIGN KEY(UK_UO) REFERENCES USCVisa (UO_PK),
	CONSTRAINT PK_USCVisaTariff PRIMARY KEY NONCLUSTERED (UK_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] {
					new IndexScript { IndexName = "NR_IX__USCVisaTariff_UK_UO", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_IX__USCVisaTariff_UK_UO ON USCVisaTariff (UK_UO)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCZipCode

	public class USCZipCode : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCZipCode"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCZipCode
(
	UZ_PK uniqueidentifier CONSTRAINT PK_USCZipCode PRIMARY KEY NONCLUSTERED,
	UZ_BeginZipCodeRange char (5) NOT NULL CONSTRAINT DF_USCZipCode_UZ_BeginZipCodeRange DEFAULT (''),
	UZ_EndZipCodeRange char (5) NOT NULL CONSTRAINT DF_USCZipCode_UZ_EndZipCodeRange DEFAULT (''),
	UZ_State char (2) NOT NULL CONSTRAINT DF_USCZipCode_UZ_State DEFAULT ('')
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[] { new IndexScript { IndexName = "NR_IX__USCZipCode", CreateIndexScript = "CREATE UNIQUE INDEX NR_IX__USCZipCode ON USCZipCode(UZ_State, UZ_BeginZipCodeRange, UZ_EndZipCodeRange)" } };
			}
		}

		#endregion
	}

	#endregion

	#region USCAMSProductNumber

	public class USCAMSProductNumber : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCAMSProductNumber"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCAMSProductNumber
(
	UA_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCAMSProductNumber_UA_PK DEFAULT (newid()),
	UA_Code varchar(8) NOT NULL CONSTRAINT DF_USCAMSProductNumber_UA_Code DEFAULT (''),
	UA_Desc varchar(120) NOT NULL CONSTRAINT DF_USCAMSProductNumber_UA_Desc DEFAULT (''),
	UA_ClassTitle varchar(60) NOT NULL CONSTRAINT DF_USCAMSProductNumber_UA_ClassTitle DEFAULT (''),
	UA_ProductType varchar(3) NOT NULL CONSTRAINT DF_USCAMSProductNumber_UA_ProductType DEFAULT (''),
	UA_IsActive bit NOT NULL CONSTRAINT DF_USCAMSProductNumber_UA_IsActive DEFAULT ((1)),
	CONSTRAINT PK_USCAMSProductNumber PRIMARY KEY NONCLUSTERED (UA_PK)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_UX__UA_Code", CreateIndexScript = "CREATE UNIQUE INDEX NR_UX__UA_Code ON USCAMSProductNumber (UA_Code)" },
					new IndexScript { IndexName = "NR_UX__UA_Desc", CreateIndexScript = "CREATE INDEX NR_UX__UA_Desc ON USCAMSProductNumber (UA_Desc)" },
					new IndexScript { IndexName = "NR_UX__UA_ClassTitle", CreateIndexScript = "CREATE INDEX NR_UX__UA_ClassTitle ON USCAMSProductNumber (UA_ClassTitle)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCDataVersion

	public class USCDataVersion : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCDataVersion"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCDataVersion
(
	UZ_PK uniqueidentifier NOT NULL,
	UZ_Name varchar(300) NOT NULL CONSTRAINT DF_USCDataVersion_UZ_Name DEFAULT (''),
	UZ_Version int NOT NULL CONSTRAINT DF_USCDataVersion_UZ_Version DEFAULT (0),
	UZ_Note nvarchar(300) NOT NULL CONSTRAINT DF_USCDataVersion_UZ_Note DEFAULT (''),
	UZ_UpdateTime DATETIME NULL,
	CONSTRAINT PK_UX_USCDataVersion_UZ_PK PRIMARY KEY CLUSTERED (UZ_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "IX_USCDataVersion_UZ_Name", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_USCDataVersion_UZ_Name ON USCDataVersion (UZ_Name ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCRuleSecondaryTariff

	public class USCRuleSecondaryTariff : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCRuleSecondaryTariff"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCRuleSecondaryTariff
(
	U3_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCRuleSecondaryTariff_U3_PK DEFAULT (newid()),
	U3_U1 uniqueidentifier NOT NULL CONSTRAINT USCRuleSecondaryTariff_U3_U1_FK2_USCTariffRule_RRR_120N FOREIGN KEY(U3_U1) REFERENCES USCTariffRule (U1_PK),
	U3_TariffFrom varchar(10) NOT NULL DEFAULT (''),
	U3_TariffTo varchar(10) NOT NULL DEFAULT (''),
	U3_DateFrom datetime NOT NULL,
	U3_DateTo datetime NULL
	CONSTRAINT PK_USCRuleSecondaryTariff PRIMARY KEY NONCLUSTERED (U3_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "IX_USCRuleSecondaryTariff_U3_U1", CreateIndexScript = "CREATE NONCLUSTERED INDEX IX_USCRuleSecondaryTariff_U3_U1 ON USCRuleSecondaryTariff (U3_U1 ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCImportEstablishment

	public class USCImportEstablishment : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCImportEstablishment"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCImportEstablishment
(
	IE_PK uniqueidentifier NOT NULL,
	IE_EstablishmentNumber varchar(20) NOT NULL CONSTRAINT DF_USCImportEstablishment_IE_EstablishmentNumber DEFAULT (''),
	IE_CompanyName varchar(50) NOT NULL CONSTRAINT DF_USCImportEstablishment_IE_CompanyName DEFAULT (''),
	IE_Address1 varchar(50) NOT NULL CONSTRAINT DF_USCImportEstablishment_IE_Address1 DEFAULT (''),
	IE_Phone varchar(50) NOT NULL CONSTRAINT DF_USCImportEstablishment_IE_Phone DEFAULT (''),
	IE_GrantDate datetime NULL,
	IE_Address2 varchar(50) NOT NULL CONSTRAINT DF_USCImportEstablishment_IE_Address2 DEFAULT (''),
	IE_EstablishmentDescription varchar(50) NOT NULL CONSTRAINT DF_USCImportEstablishment_IE_EstablishmentDescription DEFAULT (''),
	IE_IsSystemGenerated char(1) NOT NULL CONSTRAINT DF_USCImportEstablishment_IE_IsSystemGenerated DEFAULT ('N'),
	CONSTRAINT PK_USCImportEstablishment PRIMARY KEY CLUSTERED (IE_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return System.Array.Empty<IndexScript>();
			}
		}

		#endregion
	}

	#endregion

	#region USCImportEstablishmentAlternateName

	public class USCImportEstablishmentAlternateName : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCImportEstablishmentAlternateName"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCImportEstablishmentAlternateName
(
	IA_PK uniqueidentifier NOT NULL,
	IA_IE uniqueidentifier NOT NULL CONSTRAINT FK_USCImportEstablishment_USCImportEstablishmentAlternateName FOREIGN KEY(IA_IE) REFERENCES USCImportEstablishment (IE_PK),
	IA_OperationType varchar(50) NOT NULL CONSTRAINT DF_USCImportEstablishmentAlternateName_IA_OperationType DEFAULT (''),
	IA_DoingBusinessAs varchar(50) NOT NULL CONSTRAINT DF_USCImportEstablishmentAlternateName_IA_DoingBusinessAs DEFAULT (''),
	CONSTRAINT PK_USCImportEstablishmentAlternateName PRIMARY KEY CLUSTERED (IA_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "IX_USCImportEstablishmentAlternateName_IA_IE", CreateIndexScript = "CREATE NONCLUSTERED INDEX IX_USCImportEstablishmentAlternateName_IA_IE ON USCImportEstablishmentAlternateName (IA_IE ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCRuleSecondaryTariffException

	public class USCRuleSecondaryTariffException : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCRuleSecondaryTariffException"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCRuleSecondaryTariffException
(
	U4_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCTariffRuleException_U4_PK DEFAULT (newid()),
	U4_U3 uniqueidentifier NOT NULL CONSTRAINT USCRuleSecondaryTariffException_U4_U3_FK2_USCRuleSecondaryTariff_RRR_120N FOREIGN KEY (U4_U3) REFERENCES USCRuleSecondaryTariff (U3_PK),
	U4_Tariff varchar(10) NOT NULL DEFAULT (''),
	U4_DateFrom datetime NULL,
	U4_DateTo datetime NULL,
	CONSTRAINT PK_USCRuleSecondaryTariffException PRIMARY KEY NONCLUSTERED (U4_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_IX__USCRuleSecondaryTariffException_U4_U3", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_IX__USCRuleSecondaryTariffException_U4_U3 ON USCRuleSecondaryTariffException (U4_U3)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCACCase

	public class USCACCase : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCACCase"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCACCase(
	U5_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCACCase_U5_PK DEFAULT (newid()),
	U5_CaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCase_U5_CaseNumber DEFAULT (''),
	U5_RelatedCaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCase_U5_RelatedCaseNumber DEFAULT (''),
	U5_ShortDescription varchar(30) NOT NULL CONSTRAINT DF_USCACCase_U5_ShortDescription DEFAULT (''),
	U5_ISOCountryCode varchar(2) NOT NULL CONSTRAINT DF_USCACCase_U5_ISOCountryCode DEFAULT (''),
	U5_CaseStatus varchar(2) NOT NULL CONSTRAINT DF_USCACCase_U5_CaseStatus DEFAULT (''),
	U5_CaseStatusDate datetime NOT NULL,
	U5_OfficialName varchar(320) NOT NULL CONSTRAINT DF_USCACCase_U5_OfficialName DEFAULT (''),
	U5_ManufacturerMID varchar(15) NOT NULL CONSTRAINT DF_USCACCase_U5_ManufacturerMID DEFAULT (''),
	U5_ManufacturerName varchar(100) NOT NULL CONSTRAINT DF_USCACCase_U5_ManufacturerName DEFAULT (''),
	U5_ForeignExporterMID varchar(15) NOT NULL CONSTRAINT DF_USCACCase_U5_ForeignExporterMID DEFAULT (''),
	U5_ForeignExporterName varchar(100) NOT NULL CONSTRAINT DF_USCACCase_U5_ForeignExporterName DEFAULT (''),
	U5_ContactOffice varchar(20) NOT NULL CONSTRAINT DF_USCACCase_U5_ContactOffice DEFAULT (''),
	U5_ContactName varchar(20) NOT NULL CONSTRAINT DF_USCACCase_U5_ContactName DEFAULT (''),
	U5_Phone1 varchar(16) NOT NULL CONSTRAINT DF_USCACCase_U5_Phone1 DEFAULT (''),
	U5_Phone2 varchar(16) NOT NULL CONSTRAINT DF_USCACCase_U5_Phone2 DEFAULT (''),
	CONSTRAINT PK_USCACCase PRIMARY KEY NONCLUSTERED (U5_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_UC__U5_CaseNumber", CreateIndexScript = "CREATE UNIQUE CLUSTERED INDEX NR_UC__U5_CaseNumber ON USCACCase (U5_CaseNumber ASC)" },
					new IndexScript { IndexName = "NR_RX__U5_CaseStatus_U5_CaseNumber_U5_ISOCountryCode", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_RX__U5_CaseStatus_U5_CaseNumber_U5_ISOCountryCode ON USCACCase (U5_CaseStatus ASC, U5_CaseNumber ASC, U5_ISOCountryCode ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCACCaseRate

	public class USCACCaseRate : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCACCaseRate"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCACCaseRate(
	U6_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCACCaseRate_U6_PK DEFAULT (newid()),
	U6_CaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCaseRate_U6_CaseNumber DEFAULT (''),
	U6_AdValoremRate decimal(6, 4) NOT NULL CONSTRAINT DF_USCACCaseRate_U6_AdValoremRate DEFAULT (0),
	U6_SpecificRate decimal(8, 2) NOT NULL CONSTRAINT DF_USCACCaseRate_U6_SpecificRate DEFAULT (0),
	U6_Unit varchar(3) NOT NULL CONSTRAINT DF_USCACCaseRate_U6_Unit DEFAULT (''),
	U6_UnitDesc varchar(25) NOT NULL CONSTRAINT DF_USCACCaseRate_U6_UnitDesc DEFAULT (''),
	U6_EffectiveDate datetime NULL,
	U6_AddedDate datetime NULL,
	U6_InactivatedDate datetime NULL,
	CONSTRAINT PK_USCACCaseRate PRIMARY KEY NONCLUSTERED (U6_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_RC__U6_CaseNumber", CreateIndexScript = "CREATE CLUSTERED INDEX NR_RC__U6_CaseNumber ON USCACCaseRate (U6_CaseNumber ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCACCaseEvent

	public class USCACCaseEvent : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCACCaseEvent"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCACCaseEvent(
	U7_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCACCaseEvent_U7_PK DEFAULT (newid()),
	U7_CaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCaseEvent_U7_CaseNumber DEFAULT (''),
	U7_Event varchar(30) NOT NULL CONSTRAINT DF_USCACCaseEvent_U7_Event DEFAULT (''),
	U7_Determination varchar(10) NOT NULL CONSTRAINT DF_USCACCaseEvent_U7_Determination DEFAULT (''),
	U7_FedRegCitation varchar(9) NOT NULL CONSTRAINT DF_USCACCaseEvent_U7_FedRegCitation DEFAULT (''),
	U7_EffectiveDate datetime NULL,
	U7_AddedDate datetime NULL,
	U7_InactivatedDate datetime NULL,
	CONSTRAINT PK_USCACCaseEvent PRIMARY KEY NONCLUSTERED (U7_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_RC__U7_CaseNumber", CreateIndexScript = "CREATE CLUSTERED INDEX NR_RC__U7_CaseNumber ON USCACCaseEvent (U7_CaseNumber ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCACCaseBondCash

	public class USCACCaseBondCash : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCACCaseBondCash"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCACCaseBondCash(
	U8_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCACCaseBondCash_U8_PK DEFAULT (newid()),
	U8_CaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCaseBondCash_U8_CaseNumber DEFAULT (''),
	U8_Indicator varchar(3) NOT NULL CONSTRAINT DF_USCACCaseBondCash_U8_Indicator DEFAULT (''),
	U8_EffectiveDate datetime NULL,
	U8_AddedDate datetime NULL,
	U8_InactivatedDate datetime NULL,
	CONSTRAINT PK_USCACCaseBondCash PRIMARY KEY NONCLUSTERED (U8_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_RC__U8_CaseNumber", CreateIndexScript = "CREATE CLUSTERED INDEX NR_RC__U8_CaseNumber ON USCACCaseBondCash (U8_CaseNumber ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCACCaseTariff

	public class USCACCaseTariff : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCACCaseTariff"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCACCaseTariff(
	U9_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCACCaseTariff_U9_PK DEFAULT (newid()),
	U9_CaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCaseTariff_U9_CaseNumber DEFAULT (''),
	U9_TariffNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCaseTariff_U9_TariffNumber DEFAULT (''),
	U9_AddedDate datetime NULL,
	U9_InactivatedDate datetime NULL,
	CONSTRAINT PK_USCACCaseTariff PRIMARY KEY NONCLUSTERED (U9_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_RC__U9_CaseNumber", CreateIndexScript = "CREATE CLUSTERED INDEX NR_RC__U9_CaseNumber ON USCACCaseTariff (U9_CaseNumber ASC)" },
					new IndexScript { IndexName = "NR_RX__U9_TariffNumber_U9_CaseNumber", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_RX__U9_TariffNumber_U9_CaseNumber ON USCACCaseTariff (U9_TariffNumber ASC, U9_CaseNumber ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion

	#region USCACCaseLiqSuspension

	public class USCACCaseLiqSuspension : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "USCACCaseLiqSuspension"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE USCACCaseLiqSuspension(
	UN_PK uniqueidentifier NOT NULL CONSTRAINT DF_USCACCaseLiqSuspension_UN_PK DEFAULT (newid()),
	UN_CaseNumber varchar(10) NOT NULL CONSTRAINT DF_USCACCaseLiqSuspension_UN_CaseNumber DEFAULT (''),
	UN_Action varchar(5) NOT NULL CONSTRAINT DF_USCACCaseLiqSuspension_UN_Action DEFAULT (''),
	UN_EffectiveDate datetime NULL,
	UN_AddedDate datetime NULL,
	UN_InactivatedDate datetime NULL,
	CONSTRAINT PK_USCACCaseLiqSuspension PRIMARY KEY NONCLUSTERED (UN_PK ASC)
)
";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
				{
					new IndexScript { IndexName = "NR_RC__UN_CaseNumber", CreateIndexScript = "CREATE CLUSTERED INDEX NR_RC__UN_CaseNumber ON USCACCaseLiqSuspension (UN_CaseNumber ASC)" }
				};
			}
		}

		#endregion
	}

	#endregion
}
