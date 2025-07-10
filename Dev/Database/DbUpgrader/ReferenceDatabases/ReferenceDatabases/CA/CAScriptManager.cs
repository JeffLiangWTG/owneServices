using System.Collections.Generic;
using System.Data;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Scripts
{
	#region CACPortOfExit

	public class CACPortOfExit : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACPortOfExit"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACPortOfExit(
	CP_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACPortOfExit_PE_PK DEFAULT (newid()),
	CP_Code nvarchar(50) NOT NULL,
	CP_OfficialCode int NOT NULL,
	CONSTRAINT PK_CACPortOfExit PRIMARY KEY CLUSTERED
	(
		CP_PK ASC
	),
	CONSTRAINT CK_CACPortOfExit_CP_Code CHECK ((CP_Code<>'')),
	CONSTRAINT CK_CACPortOfExit_CP_OfficialCode CHECK ((CP_OfficialCode>(0)))
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACPortOfExit_CP_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACPortOfExit_CP_Code ON CACPortOfExit (CP_Code ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACPortOfExit.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "CP_Code, CP_OfficialCode"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.NVarChar, SqlDbType.Int }; }
		}

		#endregion

		#region Constants

		public const string ChangeCP_OfficialCodeColumnTypeScript =
			@"
IF  EXISTS (SELECT null FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'dbo.CK_CACPortOfExit_CP_OfficialCode') AND parent_object_id = OBJECT_ID(N'CACPortOfExit'))
ALTER TABLE CACPortOfExit DROP CONSTRAINT CK_CACPortOfExit_CP_OfficialCode

IF  EXISTS (SELECT null FROM sys.objects WHERE object_id = OBJECT_ID(N'DF_CACPortOfExit_CP_OfficialCode') AND type = 'D')
ALTER TABLE CACPortOfExit DROP CONSTRAINT DF_CACPortOfExit_CP_OfficialCode

ALTER TABLE CACPortOfExit ALTER COLUMN CP_OfficialCode varchar(3) NOT NULL
ALTER TABLE CACPortOfExit WITH CHECK ADD CONSTRAINT CK_CACPortOfExit_CP_OfficialCode CHECK((CP_OfficialCode<>''))
ALTER TABLE CACPortOfExit CHECK CONSTRAINT CK_CACPortOfExit_CP_OfficialCode
";

		#endregion
	}

	#endregion

	#region CACPlaceOfReport

	public class CACPlaceOfReport : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACPlaceOfReport"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACPlaceOfReport(
	CR_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACPlaceOfReport_PR_PK DEFAULT (newid()),
	CR_Code nvarchar(50) NOT NULL,
	CR_OfficialCode int NOT NULL,
	CONSTRAINT PK_CACPlaceOfReport PRIMARY KEY CLUSTERED
	(
		CR_PK ASC
	),
	CONSTRAINT CK_CACPlaceOfReport_CR_Code CHECK ((CR_Code<>'')),
	CONSTRAINT CK_CACPlaceOfReport_CR_OfficialCode CHECK ((CR_OfficialCode>(0)))
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACPlaceOfReport_CR_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACPlaceOfReport_CR_Code ON CACPlaceOfReport (CR_Code ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACPlaceOfReport.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "CR_Code, CR_OfficialCode"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.NVarChar, SqlDbType.Int }; }
		}

		#endregion

		#region Constants

		public const string ChangeCR_OfficialCodeColumnTypeScript =
			@"
IF EXISTS (SELECT null FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'dbo.CK_CACPlaceOfReport_CR_OfficialCode') AND parent_object_id = OBJECT_ID(N'CACPlaceOfReport'))
ALTER TABLE CACPlaceOfReport DROP CONSTRAINT CK_CACPlaceOfReport_CR_OfficialCode

IF EXISTS (SELECT null FROM sys.objects WHERE object_id = OBJECT_ID(N'DF_CACPlaceOfReport_CR_OfficialCode') AND type = 'D')
ALTER TABLE CACPlaceOfReport DROP CONSTRAINT DF_CACPlaceOfReport_CR_OfficialCode

ALTER TABLE CACPlaceOfReport ALTER COLUMN CR_OfficialCode varchar(3) NOT NULL
ALTER TABLE CACPlaceOfReport WITH CHECK ADD CONSTRAINT CK_CACPlaceOfReport_CR_OfficialCode CHECK((CR_OfficialCode<>''))
ALTER TABLE CACPlaceOfReport CHECK CONSTRAINT CK_CACPlaceOfReport_CR_OfficialCode
";

		#endregion
	}

	#endregion

	#region CACExportTariff

	public class CACExportTariff : ITableScript
	{
		#region ITableScript Members

		string ITableScript.TableName
		{
			get { return TableName; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACExportTariff(
	CE_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACExportTariff_ET_PK DEFAULT (newid()),
	CE_Code nvarchar(10) NOT NULL,
	CE_Description nvarchar(255) NOT NULL,
	CE_Unit nvarchar(3) NOT NULL CONSTRAINT DF_CACExportTariff_ET_Unit DEFAULT (''),
	CONSTRAINT PK_CACExportTariff PRIMARY KEY CLUSTERED
	(
		CE_PK ASC
	),
	CONSTRAINT CK_CACExportTariff_CE_Code CHECK ((CE_Code<>'')),
	CONSTRAINT CK_CACExportTariff_CE_Description CHECK ((CE_Description<>''))
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
						   {
							   new IndexScript
								   {
									   IndexName = "IX_CACExportTariff_CE_Code_CE_Unit",
									   CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACExportTariff_CE_Code_CE_Unit ON CACExportTariff (CE_Code ASC, CE_Unit ASC)"
								   }
						   };
			}
		}

		#endregion

		#region Constants

		public const string TableName = "CACExportTariff";

		public const string CE_IsConveyanceIDRequiredColumnName = "CE_IsConveyanceIDRequired";

		public const string AddCE_IsConveyanceIDRequiredScript = @"
ALTER TABLE CACExportTariff
	WITH CHECK ADD CE_IsConveyanceIDRequired CHAR(1) NOT NULL CONSTRAINT DF_CACExportTariff_CE_IsConveyanceIDRequired DEFAULT 'N'
	CONSTRAINT CK_CACExportTariff_CE_IsConveyanceIDRequired CHECK (CE_IsConveyanceIDRequired='N' OR CE_IsConveyanceIDRequired='Y')";

		#endregion
	}

	#endregion

	#region CACTradeZone

	class CACTradeZone : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACTradeZone"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACTradeZone(
	CT_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACTradeZone_CT_PK DEFAULT (newid()),
	CT_Code nvarchar(4) NOT NULL,
	CT_Description nvarchar(255) NOT NULL,
	CT_USState nvarchar(50) NOT NULL
	CONSTRAINT PK_CACTradeZone PRIMARY KEY CLUSTERED (CT_PK ASC),
	CONSTRAINT CK_CACTradeZone_CT_Code CHECK (CT_Code<>''),
	CONSTRAINT CK_CACTradeZone_CT_Description CHECK (CT_Description<>''),
	CONSTRAINT CK_CACTradeZone_CT_USState CHECK (CT_USState<>'')
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACTradeZone_CT_Code_CT_USState", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACTradeZone_CT_Code_CT_USState ON CACTradeZone (CT_Code ASC, CT_USState ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACTradeZone.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "CT_Code, CT_Description, CT_USState"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar }; }
		}

		#endregion
	}

	#endregion

	#region CACUSPortOfExit

	class CACUSPortOfExit : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACUSPortOfExit"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACUSPortOfExit(
	CU_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACUSPortOfExit_CU_PK DEFAULT (newid()),
	CU_Code nvarchar(4) NOT NULL,
	CU_Description nvarchar(255) NOT NULL,
	CU_USState nvarchar(50) NOT NULL
	CONSTRAINT PK_CACUSPortOfExit PRIMARY KEY CLUSTERED (CU_PK ASC),
	CONSTRAINT CK_CACUSPortOfExit_CU_Code CHECK (LEN(CU_Code) = 4),
	CONSTRAINT CK_CACUSPortOfExit_CU_Description CHECK (CU_Description<>''),
	CONSTRAINT CK_CACUSPortOfExit_CU_USState CHECK (CU_USState<>'')
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACUSPortOfExit_CU_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACUSPortOfExit_CU_Code ON CACUSPortOfExit (CU_Code ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACUSPortOfExit.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "CU_Code, CU_Description, CU_USState"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar }; }
		}

		#endregion
	}

	#endregion

	#region CACClassHeader

	public class CACClassHeader : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACClassHeader"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACClassHeader(
	ZA_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACClassHeader_ZA_PK  DEFAULT (newid()),
	ZA_ClassificationNumber varchar(10) NOT NULL,
	ZA_EffectiveDate smalldatetime NULL,
	ZA_ExpiryDate smalldatetime NULL,
	ZA_AreaCode char(3) NOT NULL,
	ZA_ClassAuthorityNumber varchar(13) NOT NULL,
	ZA_StatisticalUOMCode char(3) NOT NULL,
	ZA_ExchangeDateDeterminationFlag char(1) NOT NULL,
	ZA_InactiveInd char(1) NOT NULL,
	ZA_TariffAuthorityNumber varchar(13) NOT NULL,
	ZA_TariffEffectiveDate smalldatetime NULL,
	ZA_TariffExpiryDate smalldatetime NULL,
	ZA_PermitInd char(1) NOT NULL,
	ZA_QuotaInd char(1) NOT NULL,
 CONSTRAINT PK_CACClassHeader PRIMARY KEY CLUSTERED (ZA_PK ASC),
 CONSTRAINT CK_ZA_AreaCode CHECK  ((Len(ZA_AreaCode)=(3))),
 CONSTRAINT CK_ZA_PermitInd CHECK  ((ZA_PermitInd='' OR ZA_PermitInd='N' OR ZA_PermitInd='Y')),
 CONSTRAINT CK_ZA_QuotaInd CHECK  ((ZA_QuotaInd='' OR ZA_QuotaInd='N' OR ZA_QuotaInd='Y'))
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
						{
							new IndexScript
								{
									IndexName = "IX_CACClassHeader_ZA_ClassificationNumber_ZA_EffectiveDate",
									CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACClassHeader_ZA_ClassificationNumber_ZA_EffectiveDate ON CACClassHeader (ZA_ClassificationNumber ASC, ZA_EffectiveDate ASC)"
								},
							new IndexScript
								{
									IndexName = "IX_CACClassHeader_ZA_ClassificationNumber_ZA_EffectiveDate_ZA_ExpiryDate",
									CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACClassHeader_ZA_ClassificationNumber_ZA_EffectiveDate_ZA_ExpiryDate ON CACClassHeader (ZA_ClassificationNumber ASC, ZA_EffectiveDate ASC, ZA_ExpiryDate ASC)"
								}
				};
			}
		}

		#endregion
	}

	#endregion

	#region CACRateHeader

	class CACRateHeader : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACRateHeader"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACRateHeader(
	ZB_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACRateHeader_ZB_PK  DEFAULT (newid()),
	ZB_ZA_ClassHeader uniqueidentifier NOT NULL,
	ZB_EffectiveDate smalldatetime NULL,
	ZB_ExpiryDate smalldatetime NULL,
	ZB_FreeInd char(1) NOT NULL,
	ZB_UnitOfMeasure char(3) NOT NULL,
	ZB_Inactive char(1) NOT NULL,
	ZB_DutyRateAuthorityNumber varchar(13) NOT NULL,
	ZB_RateType char(3) NOT NULL,
 CONSTRAINT PK_CACRateHeader PRIMARY KEY CLUSTERED (ZB_PK ASC),
 CONSTRAINT FK_CACRateHeader_CACClassHeader FOREIGN KEY(ZB_ZA_ClassHeader) REFERENCES CACClassHeader (ZA_PK) ON DELETE CASCADE,
 CONSTRAINT CK_ZB_FreeInd CHECK  ((ZB_FreeInd='' OR ZB_FreeInd='N' OR ZB_FreeInd='Y')),
 CONSTRAINT CK_ZB_RateType CHECK  ((ZB_RateType='EXS' OR ZB_RateType='CLS')),
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
						   {
							   new IndexScript
								   {
									   IndexName = "IX_CACRateHeader_ZB_ZA_ClassHeader_ZB_EffectiveDate_ZB_UnitOfMeasure",
									   CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACRateHeader_ZB_ZA_ClassHeader_ZB_EffectiveDate_ZB_UnitOfMeasure ON CACRateHeader (ZB_ZA_ClassHeader ASC, ZB_EffectiveDate ASC, ZB_UnitOfMeasure ASC) WHERE ZB_RateType = 'EXS'"
								   },
							   new IndexScript
								   {
									   IndexName = "IX_CACRateHeader_ZB_ZA_ClassHeader_ZB_EffectiveDate",
									   CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACRateHeader_ZB_ZA_ClassHeader_ZB_EffectiveDate ON CACRateHeader (ZB_ZA_ClassHeader ASC, ZB_EffectiveDate ASC) WHERE ZB_RateType = 'CLS'"
								   }
						   };
			}
		}

		#endregion
	}

	#endregion

	#region CACRate

	class CACRate : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACRate"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACRate(
	ZC_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACRate_ZC_PK  DEFAULT (newid()),
	ZC_ParentID uniqueidentifier NOT NULL,
	ZC_ParentTableCode varchar(3) NOT NULL,
	ZC_TreatmentCode char(2) NOT NULL,
	ZC_FreeInd char(1) NOT NULL,
	ZC_Inactive char(1) NOT NULL,
 CONSTRAINT PK_CACRate PRIMARY KEY CLUSTERED (ZC_PK ASC),
 CONSTRAINT CK_ZC_FreeInd CHECK  ((ZC_FreeInd='' OR ZC_FreeInd='N' OR ZC_FreeInd='Y'))
 )";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACRate_ZC_ParentID_ZC_TreatmentCode", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACRate_ZC_ParentID_ZC_TreatmentCode ON CACRate (ZC_ParentID ASC, ZC_TreatmentCode ASC)" } }; }
		}

		#endregion
	}

	#endregion

	#region CACRateLine

	class CACRateLine : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACRateLine"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACRateLine(
	ZR_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACRateLine_ZR_PK  DEFAULT (newid()),
	ZR_ZC_Rate uniqueidentifier NOT NULL,
	ZR_DutyRateType varchar(1) NOT NULL,
	ZR_DutyRateMin decimal(9, 5) NOT NULL,
	ZR_DutyRateMax decimal(9, 5) NOT NULL,
	ZR_DutyRateRegular decimal(9, 5) NOT NULL,
 CONSTRAINT PK_CACRateLine PRIMARY KEY CLUSTERED (ZR_PK ASC),
 CONSTRAINT FK_CACRateLine_CACRate FOREIGN KEY(ZR_ZC_Rate) REFERENCES CACRate (ZC_PK) ON DELETE CASCADE
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts => new[] { new IndexScript { IndexName = "NR_IX__CACRateLine_ZR_ZC_Rate", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_IX__CACRateLine_ZR_ZC_Rate ON CACRateLine (ZR_ZC_Rate)" } };

		#endregion
	}

	#endregion

	#region CACTaxRefNumHeader

	class CACTaxRefNumHeader : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACTaxRefNumHeader"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACTaxRefNumHeader(
	ZD_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACTaxRefNumHeader_ZD_PK  DEFAULT (newid()),
	ZD_ZA_ClassNumber uniqueidentifier NOT NULL,
	ZD_EffectiveDate smalldatetime NULL,
	ZD_ExpiryDate smalldatetime NULL,
	ZD_Inactive char(1) NOT NULL,
 CONSTRAINT PK_CACTaxRefNumHeader PRIMARY KEY CLUSTERED (ZD_PK ASC),
 CONSTRAINT FK_CACTaxRefNumHeader_CACClassHeader FOREIGN KEY(ZD_ZA_ClassNumber) REFERENCES CACClassHeader (ZA_PK) ON DELETE CASCADE
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACTaxRefNumHeader_ZD_ZA_ClassNumber_ZD_EffectiveDate", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACTaxRefNumHeader_ZD_ZA_ClassNumber_ZD_EffectiveDate ON CACTaxRefNumHeader (ZD_ZA_ClassNumber ASC, ZD_EffectiveDate ASC)" } }; }
		}

		#endregion
	}

	#endregion

	#region CACTaxRefNumber

	class CACTaxRefNumber : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACTaxRefNumber"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACTaxRefNumber(
	ZE_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACTaxRefNumber_ZE_PK  DEFAULT (newid()),
	ZE_ZD_TaxRefNumHeader uniqueidentifier NOT NULL,
	ZE_GSTRefNumber char(3) NOT NULL,
	ZE_ExciseTaxRefNumber char(3) NOT NULL,
 CONSTRAINT PK_CACTaxRefNumber PRIMARY KEY CLUSTERED (ZE_PK ASC),
 CONSTRAINT FK_CACTaxRefNumber_CACTaxRefNumHeader FOREIGN KEY(ZE_ZD_TaxRefNumHeader) REFERENCES CACTaxRefNumHeader (ZD_PK) ON DELETE CASCADE
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts => new[] { new IndexScript { IndexName = "NR_IX__CACTaxRefNumber_ZE_ZD_TaxRefNumHeader", CreateIndexScript = "CREATE NONCLUSTERED INDEX NR_IX__CACTaxRefNumber_ZE_ZD_TaxRefNumHeader ON CACTaxRefNumber (ZE_ZD_TaxRefNumHeader)" } };

		#endregion
	}

	#endregion

	#region CACTariffHeader

	class CACTariffHeader : ITableScript
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACTariffHeader"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACTariffHeader(
	ZF_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACTariffHeader_ZF_PK  DEFAULT (newid()),
	ZF_TariffCode varchar(4) NOT NULL,
	ZF_AuthEffectiveDate smalldatetime NULL,
	ZF_AuthExpiryDate smalldatetime NULL,
	ZF_RateEffectiveDate smalldatetime NULL,
	ZF_RateExpiryDate smalldatetime NULL,
	ZF_TariffCodeAuthorityNumber varchar(13) NOT NULL,
	ZF_Inactive char(1) NOT NULL,
	ZF_FreeInd char(1) NOT NULL,
	ZF_GST0RateInd char(1) NOT NULL,
 CONSTRAINT PK_CACTariffHeader PRIMARY KEY CLUSTERED (ZF_PK ASC),
 CONSTRAINT CK_ZF_FreeInd CHECK  ((ZF_FreeInd='' OR ZF_FreeInd='N' OR ZF_FreeInd='Y')),
 CONSTRAINT CK_ZF_GST0RateInd CHECK  ((ZF_GST0RateInd='' OR ZF_GST0RateInd='N' OR ZF_GST0RateInd='Y'))
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACTariffHeader_ZF_TariffCode_ZF_AuthEffectiveDate", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACTariffHeader_ZF_TariffCode_ZF_AuthEffectiveDate ON CACTariffHeader (ZF_TariffCode ASC, ZF_AuthEffectiveDate ASC)" } }; }
		}

		#endregion
	}

	#endregion

	#region CACTaxRate

	class CACTaxRate : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACTaxRate"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACTaxRate(
	ZH_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACTaxRate_ZH_PK  DEFAULT (newid()),
	ZH_TaxRefNumber char(3) NOT NULL,
	ZH_EffectiveDate smalldatetime NULL,
	ZH_ExpiryDate smalldatetime NULL,
	ZH_CheckInd char(1) NOT NULL,
	ZH_CheckGroup varchar(1) NOT NULL,
	ZH_RateType varchar(1) NOT NULL,
	ZH_Rate decimal(9, 5) NOT NULL,
	ZH_UnitOfMeasure char(3) NOT NULL,
	ZH_Title nvarchar(60) NOT NULL,
	ZH_Inactive char(1) NOT NULL,
	ZH_TaxType char(3) NOT NULL,
 CONSTRAINT PK_CACTaxRate PRIMARY KEY CLUSTERED (ZH_PK ASC),
 CONSTRAINT CK_ZH_TaxType CHECK  ((ZH_TaxType='GST' OR ZH_TaxType='EXS'))
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACTaxRate_ZH_TaxRefNumber_ZH_EffectiveDate", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACTaxRate_ZH_TaxRefNumber_ZH_EffectiveDate ON CACTaxRate (ZH_TaxRefNumber ASC, ZH_EffectiveDate ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACTaxRateCodes.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "ZH_TaxRefNumber, ZH_EffectiveDate, ZH_ExpiryDate, ZH_CheckInd, ZH_CheckGroup, ZH_RateType, ZH_Rate, ZH_UnitOfMeasure, ZH_Title, ZH_Inactive, ZH_TaxType"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get
			{
				return new[] { SqlDbType.Char, SqlDbType.SmallDateTime, SqlDbType.SmallDateTime, SqlDbType.Char, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.Decimal,
														SqlDbType.Char, SqlDbType.NVarChar, SqlDbType.Char, SqlDbType.Char };
			}
		}

		#endregion
	}

	#endregion

	#region CACFIARegTypes

	class CACFIARegTypes : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACFIARegTypes"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACFIARegTypes(
	FR_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACFIARegTypes_FR_PK  DEFAULT (newid()),
	FR_Code char(3) NOT NULL,
	FR_Desc nvarchar(255) NOT NULL,
	FR_DescFrench nvarchar(255) NOT NULL,
	FR_RegType char(1) NOT NULL,
	FR_RegSubType char(1) NOT NULL,
	CONSTRAINT PK_CACFIARegTypes PRIMARY KEY CLUSTERED (FR_PK ASC),
	CONSTRAINT CK_CACFIARegTypes_FR_Code CHECK  (FR_Code <> '')
 )";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACFIARegTypes_FR_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACFIARegTypes_FR_Code ON CACFIARegTypes (FR_Code ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACFIARegTypes.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "FR_Code, FR_Desc, FR_DescFrench, FR_RegType, FR_RegSubType"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.Char, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.Char, SqlDbType.Char }; }
		}

		#endregion
	}

	#endregion

	#region CACFIAEndUseCodes

	class CACFIAEndUseCodes : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACFIAEndUseCodes"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACFIAEndUseCodes(
	FE_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACFIAEndUseCodes_FE_PK  DEFAULT (newid()),
	FE_Code char(3) NOT NULL,
	FE_Desc nvarchar(255) NOT NULL,
	FE_DescFrench nvarchar(255) NOT NULL,
	CONSTRAINT PK_CACFIAEndUseCodes PRIMARY KEY CLUSTERED (FE_PK ASC),
	CONSTRAINT CK_CACFIAEndUseCodes_FE_Code CHECK  (FE_Code <> '')
 )";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACFIAEndUseCodes_FE_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACFIAEndUseCodes_FE_Code ON CACFIAEndUseCodes (FE_Code ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACFIAEndUseCodes.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "FE_Code, FE_Desc, FE_DescFrench"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.Char, SqlDbType.NVarChar, SqlDbType.NVarChar }; }
		}

		#endregion
	}

	#endregion

	#region CACAcrossErrorCodes

	public class CACAcrossErrorCodes : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACAcrossErrorCodes"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACAcrossErrorCodes(
	CO_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACAcrossErrorCodes_CO_PK DEFAULT (newid()),
	CO_Code nvarchar(10) NOT NULL,
	CO_ElementID nvarchar(10) NOT NULL,
	CO_MsgNo nvarchar(10) NOT NULL,
	CO_Group nvarchar(10) NOT NULL,
	CO_Description nvarchar(4000) NOT NULL,
	CO_MessageText nvarchar(255) NOT NULL,
	CO_FrenchDescription nvarchar(255) NOT NULL,
	CO_FrenchMessageText nvarchar(255) NOT NULL,
	CONSTRAINT PK_CACAcrossErrorCodes PRIMARY KEY CLUSTERED
	(
		CO_PK ASC
	),
	CONSTRAINT CK_CACAcrossErrorCodes_CO_Code CHECK ((CO_Code<>'')),
	CONSTRAINT CK_CACAcrossErrorCodes_CO_Description CHECK ((CO_Description<>''))
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get
			{
				return new[]
						   {
							   new IndexScript
								   {
									   IndexName = "IX_CACAcrossErrorCodes_CO_Code_CO_ElementID",
									   CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACAcrossErrorCodes_CO_Code_CO_ElementID ON CACAcrossErrorCodes (CO_Code ASC, CO_ElementID ASC)"
								   },
							   new IndexScript
								   {
									   IndexName = "IX_CACAcrossErrorCodes_CO_MsgNo",
									   CreateIndexScript = "CREATE NONCLUSTERED INDEX IX_CACAcrossErrorCodes_CO_MsgNo ON CACAcrossErrorCodes (CO_MsgNo ASC)"
								   }
						   };
			}
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACAcrossErrorCodes.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "CO_Code, CO_ElementID, CO_MsgNo, CO_Group, CO_Description, CO_MessageText, CO_FrenchDescription, CO_FrenchMessageText"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar }; }
		}

		#endregion
	}

	#endregion

	#region CACCBSAOfficeCodes

	class CACCBSAOfficeCodes : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACCBSAOfficeCodes"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACCBSAOfficeCodes(
	CQ_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACCBSAOfficeCodes_CQ_PK DEFAULT (newid()),
	CQ_Code nvarchar(4) NOT NULL,
	CQ_Description nvarchar(255) NOT NULL,
	CQ_Province char(2) NOT NULL,
	CQ_RelatedUSPortOfExit nvarchar(4) NULL
	CONSTRAINT PK_CACCBSAOfficeCodes PRIMARY KEY CLUSTERED (CQ_PK ASC),
	CONSTRAINT CK_CACCBSAOfficeCodes_CQ_Code CHECK (LEN(CQ_Code) = 4),
	CONSTRAINT CK_CACCBSAOfficeCodes_CQ_Description CHECK (CQ_Description<>'')
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACCBSAOfficeCodes_CQ_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACCBSAOfficeCodes_CQ_Code ON CACCBSAOfficeCodes (CQ_Code ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACCBSAOfficeCodes.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "CQ_Code, CQ_Description, CQ_Province, CQ_RelatedUSPortOfExit"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.Char, SqlDbType.NVarChar }; }
		}

		#endregion
	}

	#endregion

	#region CACountryPreference

	class CACountryPreference : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACountryPreference"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACountryPreference(
	CA_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACountryPreference_CA_PK DEFAULT (newid()),
	CA_CountryCode char(2) NOT NULL,
	CA_ValidTariffTreatments nvarchar(255) NOT NULL,
	CONSTRAINT PK_CACountryPreference PRIMARY KEY CLUSTERED
	(
		CA_PK ASC
	),
	CONSTRAINT CK_CACountryPreference_CA_CountryCode CHECK ((CA_CountryCode<>'')),
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACountryPreference_CA_CountryCode", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACountryPreference_CA_CountryCode ON CACountryPreference (CA_CountryCode ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACountryPreferences.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "CA_CountryCode, CA_ValidTariffTreatments"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.Char, SqlDbType.NVarChar }; }
		}

		#endregion
	}

	#endregion

	#region CACCasualImpCommodities

	class CACCasualImpCommodities : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACCasualImpCommodities"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACCasualImpCommodities(
	IC_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACCasualImpCommodities_IC_PK DEFAULT (newid()),
	IC_Code nvarchar(25) NOT NULL,
	IC_Type varchar(1) NOT NULL,
	IC_Description nvarchar(255) NOT NULL
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return System.Array.Empty<IndexScript>(); }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACCasualImpCommodities.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "IC_Code, IC_Type, IC_Description"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.NVarChar, SqlDbType.VarChar, SqlDbType.NVarChar }; }
		}

		#endregion
	}

	#endregion

	#region CACCasualImpRates

	class CACCasualImpRates : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACCasualImpRates"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACCasualImpRates(
	IR_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACCasualImpRates_IR_PK DEFAULT (newid()),
	IR_Province char(2) NOT NULL,
	IR_ProcessingType varchar(1) NOT NULL,
	IR_Commodity nvarchar(25) NOT NULL,
	IR_AdValoremBasis char(10),
	IR_RateType1 varchar(1) NOT NULL,
	IR_Units1 char(3) NOT NULL,
	IR_RegularRate1 decimal(9, 5) NOT NULL,
	IR_MinimumRate1 decimal(9, 5) NOT NULL,
	IR_MaximumRate1 decimal(9, 5) NOT NULL,
	IR_RateType2 varchar(1) NOT NULL,
	IR_Units2 char(3) NOT NULL,
	IR_RegularRate2 decimal(9, 5) NOT NULL,
	IR_MinimumRate2 decimal(9, 5) NOT NULL,
	IR_MaximumRate2 decimal(9, 5) NOT NULL,
	IR_RateType3 varchar(1) NOT NULL,
	IR_Units3 char(3) NOT NULL,
	IR_RegularRate3 decimal(9, 5) NOT NULL,
	IR_MinimumRate3 decimal(9, 5) NOT NULL,
	IR_MaximumRate3 decimal(9, 5) NOT NULL,
	IR_EffectiveDateFrom smalldatetime NULL,
	IR_EffectiveDateTo smalldatetime NULL
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return System.Array.Empty<IndexScript>(); }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACCasualImpRates.csv"; }
		}

		public string ColumnSqlList
		{
			get
			{
				return @"IR_Province, IR_ProcessingType, IR_Commodity, IR_AdValoremBasis,
				IR_RateType1, IR_Units1, IR_RegularRate1, IR_MinimumRate1, IR_MaximumRate1,
				IR_RateType2, IR_Units2, IR_RegularRate2, IR_MinimumRate2, IR_MaximumRate2,
				IR_RateType3, IR_Units3, IR_RegularRate3, IR_MinimumRate3, IR_MaximumRate3,
				IR_EffectiveDateFrom, IR_EffectiveDateTo";
			}
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get
			{
				return new[] { SqlDbType.Char, SqlDbType.VarChar, SqlDbType.NVarChar, SqlDbType.Char,
				SqlDbType.VarChar, SqlDbType.Char, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Decimal,
				SqlDbType.VarChar, SqlDbType.Char, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Decimal,
				SqlDbType.VarChar, SqlDbType.Char, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Decimal,
				SqlDbType.SmallDateTime, SqlDbType.SmallDateTime };
			}
		}

		#endregion
	}

	#endregion

	#region CACCasualImpDummyHS

	class CACCasualImpDummyHS : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACCasualImpDummyHS"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACCasualImpDummyHS(
	ID_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACCasualImpDummyHS_ID_PK DEFAULT (newid()),
	ID_Province char(2) NOT NULL,
	ID_Type varchar(1) NOT NULL,
	ID_Code char(10) NOT NULL
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return System.Array.Empty<IndexScript>(); }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACCasualImpDummyHS.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "ID_Province, ID_Type, ID_Code"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.Char, SqlDbType.VarChar, SqlDbType.Char }; }
		}

		#endregion
	}

	#endregion

	#region IID reference files

	#region CACDocumentTypes

	class CACDocumentTypes : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "CACDocumentTypes"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACDocumentTypes(
	FR_PK uniqueidentifier NOT NULL CONSTRAINT DF_CACDocumentTypes_FR_PK DEFAULT (newid()),
	FR_GovAgencyIDCode varchar(5) NOT NULL,
	FR_Code char(4) NOT NULL,
	FR_Desc nvarchar(255) NOT NULL,
	FR_IsDefaultRefNum CHAR(1) NOT NULL
	CONSTRAINT PK_CACDocumentTypes PRIMARY KEY CLUSTERED (FR_PK ASC),
	CONSTRAINT CK_CACDocumentTypes_FR_GovAgencyIDCode CHECK (FR_GovAgencyIDCode <> ''),
	CONSTRAINT CK_CACDocumentTypes_FR_Code CHECK (FR_Code <> ''),
	CONSTRAINT CK_CACDocumentTypes_FR_IsDefaultRefNum CHECK ((FR_IsDefaultRefNum='' OR FR_IsDefaultRefNum='N' OR FR_IsDefaultRefNum='Y'))
 )";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_CACDocumentTypes_FR_GovAgencyIDCode_FR_Code", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_CACDocumentTypes_FR_GovAgencyIDCode_FR_Code ON CACDocumentTypes (FR_GovAgencyIDCode ASC, FR_Code ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACDocumentTypes.csv"; }
		}

		public string ColumnSqlList
		{
			get { return "FR_GovAgencyIDCode, FR_Code, FR_Desc, FR_IsDefaultRefNum"; }
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get { return new[] { SqlDbType.VarChar, SqlDbType.Char, SqlDbType.NVarChar, SqlDbType.Char }; }
		}

		#endregion
	}

	#endregion

	#region CACPGAHSCode

	class CACPGAHSCode : ITableScript, IPopulateData
	{
		#region ITableScript
		public string TableName => "CACPGAHSCode";

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE CACPGAHSCode(
	HC_PK uniqueidentifier not null Constraint DF_CACPGAHSCode_HC_PK default(newid()),
	HC_Type varchar(5) NOT NULL,
	HC_FromHS varchar(10) NOT NULL,
	HC_ToHS varchar(10) NOT NULL,
	HC_EffectiveDateFrom smalldatetime NOT NULL,
	HC_EffectiveDateTo smalldatetime NOT NULL,
	HC_ProgramInd varchar(3)
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts => System.Array.Empty<IndexScript>();

		#endregion

		#region IPopulateData

		public string CsvFileName => "CACPGAHSCode.csv";

		public string ColumnSqlList => "HC_Type, HC_FromHS, HC_ToHS, HC_EffectiveDateFrom, HC_EffectiveDateTo, HC_ProgramInd";

		public IReadOnlyList<SqlDbType> ColumnTypes => new[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.SmallDateTime, SqlDbType.SmallDateTime, SqlDbType.VarChar };

		#endregion
	}

	#endregion

	#endregion

	#region Temp Tables

	class TempCACClass : ITableScript, IPopulateData
	{
		#region ITableScript Members

		public string TableName
		{
			get { return "TempCACClass"; }
		}

		public string CreateTableScript
		{
			get
			{
				return @"
CREATE TABLE TempCACClass(
	TARIFF nvarchar(10)
)";
			}
		}

		public IReadOnlyList<IndexScript> CreateIndexScripts
		{
			get { return new[] { new IndexScript { IndexName = "IX_TempCACClass_Tariff", CreateIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_TempCACClass_Tariff ON TempCACClass (Tariff ASC)" } }; }
		}

		#endregion

		#region IPopulateData Members

		public string CsvFileName
		{
			get { return "CACClass.csv"; }
		}

		public string ColumnSqlList
		{
			get
			{
				return @"Tariff";
			}
		}

		public IReadOnlyList<SqlDbType> ColumnTypes
		{
			get
			{
				return new[] { SqlDbType.NVarChar };
			}
		}

		#endregion
	}

	#endregion

}
