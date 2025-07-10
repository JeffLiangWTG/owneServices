using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US
{
	class USRefDbScriptRunner
	{
		public USRefDbScriptRunner(DbConnection conn, USReferenceDbUpgrader upgrader)
		{
			this.conn = conn;
			this.upgrader = upgrader;
		}

		readonly DbConnection conn;
		readonly USReferenceDbUpgrader upgrader;

		#region Data Resources

		const string CSVResources = "Enterprise.DbUpgrader.ReferenceDatabases.US.";
		const string CarriersDataCSVResource = "USCarriersData.csv";
		const string USCAESResponseCodeDataResource = "USCAESResponseCodeData.csv";
		const string USScheduleBResource = "USScheduleBData.txt";
		const string USCTariffRuleDataCSVResource = "USCTariffRule.csv";
		const string USCRuleSecondaryTariffDataCSVResource = "USCRuleSecondaryTariff.csv";
		const string USCRuleSecondaryTariffExceptionDataCSVResource = "USCRuleSecondaryTariffException.csv";
		const string USCTariffRuleDataTTBCSVResource = "USCTariffRuleTTB.csv";
		const string USCAMSProductResource = "USCAMSProductResource.csv";
		const string USCZipCodeResource = "USCZipCode.csv";
		const string USCTariffRuleAMSDataCSVResource = "USCTariffRuleAMS.csv";
		const string USCTariffRuleFDADataCSVResource = "USCTariffRuleFDA.csv";
		const string USCTariffRuleTTBPriceCSVResource = "USCTariffRuleTTBPrice.csv";
		const string USTariffeRuleAESExcludedCSVResource = "USCTariffRuleAESExcluded.csv";
		const string USCTariffRuleCFEResource = "USCTariffRuleCFE.csv";
		const string USCSTN9902TariffRuleResource = "USCSTN9902TariffRule.csv";
		const string USCSTN9902RuleSecondaryTariffResource = "USCSTN9902RuleSecondaryTariff.csv";

		StreamReader USCTariffRuleCFEDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCTariffRuleCFEResource)); }
		}

		StreamReader USCTariffRuleDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCTariffRuleDataCSVResource)); }
		}

		StreamReader USCRuleSecondaryTariffDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCRuleSecondaryTariffDataCSVResource)); }
		}

		StreamReader USCTariffRuleTTBDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCTariffRuleDataTTBCSVResource)); }
		}

		StreamReader USCRuleSecondaryTariffExceptionDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCRuleSecondaryTariffExceptionDataCSVResource)); }
		}

		StreamReader CarriersDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + CarriersDataCSVResource)); }
		}

		StreamReader USCAESResponseCodeDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCAESResponseCodeDataResource)); }
		}

		StreamReader ScheduleBDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USScheduleBResource)); }
		}

		StreamReader USAMSProductDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCAMSProductResource)); }
		}

		StreamReader USZipDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCZipCodeResource)); }
		}

		StreamReader USCTariffRuleAMSDataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCTariffRuleAMSDataCSVResource)); }
		}

		StreamReader USCTariffRuleFDADataCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCTariffRuleFDADataCSVResource)); }
		}

		StreamReader USCTariffRuleTTBPriceCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCTariffRuleTTBPriceCSVResource)); }
		}

		StreamReader USTariffeRuleAESExcludedCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USTariffeRuleAESExcludedCSVResource)); }
		}

		StreamReader USCSTN9902TariffRuleCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCSTN9902TariffRuleResource)); }
		}

		StreamReader USCSTN9902RuleSecondaryTariffCSV
		{
			get { return new StreamReader(GetType().Assembly.GetManifestResourceStream(CSVResources + USCSTN9902RuleSecondaryTariffResource)); }
		}

		#endregion

		#region Version number step upgrade methods

		public void InsertRecordsFor_ATPDEA_CAFTAClaims()
		{
			string insertTariffRecords = @"
if not exists (select 1 from USCTariff where UE_Tariff = '2517100015' and UE_DateFrom = '1990-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_Unit1, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_Column2RateAdValorem, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '2517100015', '1990-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 1, 'T', '7', 'PEBBLES AND GRAVEL,BROKEN/', 'N', 0.30000000, 'N', 'N', 'N', 'N')
end

if not exists (select 1 from USCTariff where UE_Tariff = '9802009000' and UE_DateFrom = '1994-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator, UE_ISOCountryofOriginEditCode)
	values
	(newid(), '9802009000', '1994-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 0, '0', 'TXTL&APRL,ASB IN MX OF US', 'N', 'N', 'Y', 'N', 'N', 'MX')
end

if not exists (select 1 from USCTariff where UE_Tariff = '98030050' and UE_DateFrom = '1989-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '98030050', '1989-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 0, '0', 'SUBSTANTIAL CONTAINERS US&', 'N', 'N', 'N', 'N', 'N')
end

if not exists (select 1 from USCTariff where UE_Tariff = '98040005' and UE_DateFrom = '1989-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '98040005', '1989-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 0, '0', 'HOUSEHOLD EFFECTS USED 1 Y', 'N', 'N', 'N', 'N', 'N')
end

if not exists (select 1 from USCTariff where UE_Tariff = '98060005' and UE_DateFrom = '1989-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '98060005', '1989-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 0, '0', 'EFFCTS;AMB.MIN.&DIPS ETC', 'N', 'N', 'N', 'N', 'N')
end

if not exists (select 1 from USCTariff where UE_Tariff = '98070040' and UE_DateFrom = '1989-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '98070040', '1989-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 0, '0', 'HONORARY METAL ARTCL F/BES', 'N', 'N', 'N', 'N', 'N')
end

if not exists (select 1 from USCTariff where UE_Tariff = '9808001000' and UE_DateFrom = '1989-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_Unit1, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '9808001000', '1989-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 1, 'X', '0', 'ENGRAV ETCHS PHOTOS ETC:US', 'N', 'N', 'N', 'N', 'N')
end

if not exists (select 1 from USCTariff where UE_Tariff = '98090010' and UE_DateFrom = '1989-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '98090010', '1989-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 0, '0', 'PUBLIC DOCS;MICROFILM,MICR', 'N', 'N', 'N', 'N', 'N')
end

if not exists (select 1 from USCTariff where UE_Tariff = '9810006000' and UE_DateFrom = '1989-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_Unit1, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator, UE_OGACodes)
	values
	(newid(), '9810006000', '1989-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 1, 'X', '0', 'INSTRMNTS N/MFG IN US;N/PR', 'N', 'N', 'N', 'N', 'N', 'FD1')
end

if not exists (select 1 from USCTariff where UE_Tariff = '9817009800' and UE_DateFrom = '1995-01-01 00:00:00.000' and UE_DateTo = '2099-12-31 00:00:00.000')
begin
	insert into USCTariff
	(UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_NumberOfReportingUnits, UE_Unit1, UE_DutyComputationCode, UE_ShortDescription, UE_IsBaseRate, UE_CountervailingDutyFlag, UE_AdditionalTariffNumberIndicator, UE_AntiDumping, UE_QuotaIndicator)
	values
	(newid(), '9817009800', '1995-01-01 00:00:00.000', '2099-12-31 00:00:00.000', 1, 'X', '0', 'THEATRICAL SCENERY F/TEMP', 'N', 'N', 'N', 'N', 'N')
end
";
			conn.ExecuteNonQuery(insertTariffRecords);
		}

		public void RegionDistrictPortUR_CodeDecrease()
		{
			string sqlText = @"
				exec ('
				IF NOT EXISTS (SELECT null from information_Schema.columns where column_Name = ''UR_Prefix'')
					alter table uscregiondistrictport
						add UR_Prefix varchar(1) not null default ''''')
				";
			conn.ExecuteNonQuery(sqlText);

			sqlText = @"
				SELECT Count(*) FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'USCRegionDistrictPort'
				AND COLUMN_NAME = 'UR_Code'
				AND DATA_TYPE = 'varchar'
				AND CHARACTER_MAXIMUM_LENGTH > '4'";

			int columnCount = (int)conn.ExecuteScalar(sqlText);

			if (columnCount > 0)
			{
				sqlText = "update USCRegionDistrictPort set UR_CODE = substring(UR_Code, 2, 4)";
				conn.ExecuteNonQuery(sqlText);

				conn.ExecuteNonQuery(@"
exec ('
IF EXISTS (SELECT null from dbo.sysindexes where name = N''IX_USCRegionDistrictPort'' and id = object_id(N''[dbo].[USCRegionDistrictPort]''))
	drop index uscregiondistrictport.IX_USCRegionDistrictPort')");

				conn.ExecuteNonQuery(@"
exec ('
IF EXISTS (SELECT null from dbo.sysindexes where name = N''NR_IX__UR_Code'' and id = object_id(N''[dbo].[USCRegionDistrictPort]''))
	drop index uscregiondistrictport.NR_IX__UR_Code')");

				conn.ExecuteNonQuery(@"
exec ('
		alter table uscregiondistrictport
		alter column UR_Code varchar(4)')");

				conn.ExecuteNonQuery(@"
exec ('
	CREATE UNIQUE INDEX NR_IX__UR_Code ON USCRegionDistrictPort(UR_Code)
')");
			}
		}

		public void USCCountryConstraintRemoval()
		{
			string dropConstraints = @"
IF  EXISTS (SELECT null FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND parent_object_id = OBJECT_ID(N'[dbo].[USCCountry]'))
ALTER TABLE [dbo].[USCCountry] DROP CONSTRAINT [{0}]";

			conn.ExecuteNonQuery(string.Format(dropConstraints, "UC_DrawbackEligibility"));
			conn.ExecuteNonQuery(string.Format(dropConstraints, "UC_GSPIndicator"));
			conn.ExecuteNonQuery(string.Format(dropConstraints, "UC_LesserDevelopedCountry"));
			conn.ExecuteNonQuery(string.Format(dropConstraints, "UC_RateColumnIndicator"));
			conn.ExecuteNonQuery(string.Format(dropConstraints, "UC_RestrictionIndicator"));
			conn.ExecuteNonQuery(string.Format(dropConstraints, "UC_SpecialTradeProgramsIndicator"));
			conn.ExecuteNonQuery(string.Format(dropConstraints, "UC_SPICode"));
		}

		public void USCCarrierAddressLengthChange()
		{
			string changeAddressSizeSQLStatment = @"ALTER TABLE USCCarrier ALTER COLUMN UI_Address varchar(105) NOT NULL";
			conn.ExecuteNonQuery(changeAddressSizeSQLStatment);
		}

		public void UpdateUSCCarrierDataAndCreateUSCDataVersion()
		{
			AddUSCDataVersionIfNecessary();
			// These code have been submitted to Customs and came back negative:
			var deleteNotExistDataSql = @"DELETE FROM USCCarrier WHERE UI_CODE IN (
					'09', '13', '14', '16', '17', '20', '21', '23', '25', '26', '27', '29', '2J', '2L', '30', '3G', '3P', '3Q', '4A', '4I', '4V', '5B', '5M', '6E', '6G', '6O', '6P', '7Z', '8I', '8V', '9H', '9R', 'A4',
'AAHD', 'AG', 'AJ', 'ALFA', 'AMQM', 'ANGJ', 'AOAE', 'AOSQ', 'AOXR', 'AQ', 'AXSJ', 'B0', 'BAG', 'BDPL', 'BFTD', 'BH', 'BN', 'BNSO', 'BU', 'BZ', 'CAML', 'CB', 'CHVW', 'CMGU', 'CMUG', 'CN', 'CPHE', 'CRSA',
'CSWA', 'CTPU', 'DARS', 'DD', 'DELM', 'DGFN', 'DI', 'DICL', 'DLHQ', 'DWEC', 'DY', 'DYFL', 'EATG', 'EBMP', 'ECLV', 'EEFI', 'ELON', 'F7', 'FA', 'FALO', 'FZ', 'GBKP', 'GEC', 'GK', 'HDYN', 'HTGM', 'HW',
'IF', 'IN','ISTN', 'JC', 'JOTO', 'KG', 'KI', 'KRKI', 'KW', 'L8', 'LATN', 'LE', 'LIDB', 'LJ', 'LKDT', 'LKTE', 'LOES', 'M2', 'MAPE', 'MEDS', 'MICL', 'MJWI', 'MKEO', 'MNJC', 'MRJJ', 'MSCY', 'MSQL', 'MT',
'N3', 'N6', 'N9', 'NJKG', 'NMCO', 'NPSC', 'O3', 'OCGT', 'OCLJ', 'OOLL', 'P9', 'PBCR', 'PGLM', 'PLKD','PLKQ', 'PPIL', 'PQ', 'PSEH', 'PTXN', 'Q7', 'QL', 'QPTK', 'R0', 'R4', 'RMZF', 'RN', 'RV', 'RYPQ',
'SGDE', 'SHSF', 'SHTL', 'SHZS', 'SI', 'SKIF', 'SSUS', 'T2', 'T6', 'T7', 'TB', 'TBJL', 'TE', 'TEVL', 'TF', 'TGDD', 'TKUS', 'TKYI', 'TMCL', 'TST', 'TXDN', 'URIC', 'UWTS', 'V3', 'V5', 'VASS', 'W9', 'WV',
'WVYT', 'XC', 'XX', 'XXXQ', 'YE', 'YH', 'YP', 'YQ', 'YU', 'Z0', 'Z4', 'Z7', 'Z8', 'ZF'
				);";
			conn.ExecuteNonQuery(deleteNotExistDataSql);

			string uSCCarrierSystemScript = @"

IF object_id('tempdb..#TempUSCCarrierSystem') IS NOT NULL
BEGIN
DROP TABLE #TempUSCCarrierSystem
END;

CREATE TABLE #TempUSCCarrierSystem (
	[US_PK] [UNIQUEIDENTIFIER] NOT NULL PRIMARY KEY NONCLUSTERED
)
";
			conn.ExecuteNonQuery(uSCCarrierSystemScript);
			using (StreamReader reader = CarriersDataCSV)
			{
				string carrierData;

				while ((carrierData = reader.ReadLine()) != null)
				{
					string[] splitCarrierData = new OCsvLine(carrierData, '').FieldValues;

					string carrierCode = splitCarrierData[0];
					string carrierName = splitCarrierData[1];
					string carrierModeOfTransportation = splitCarrierData[2];
					string carrierAddress = splitCarrierData[3];
					string carrierAirwayBillPrefix = splitCarrierData[4];

					string carrierCommandText = @"
IF EXISTS (SELECT UI_Code FROM USCCarrier WHERE UI_Code = @Code OR (UI_Code = '' AND UI_NAME = @Name))
BEGIN
	UPDATE USCCarrier
	SET UI_CODE = @Code,
		UI_NAME = @Name,
		UI_ModeOfTransportation = @ModeOfTransportation,
		UI_Address = @Address,
		UI_AirwayBillPrefix = @AirwayBillPrefix
	WHERE UI_Code = @Code OR (UI_Code = '' AND UI_NAME = @Name)
END ELSE
BEGIN
	INSERT INTO USCCarrier (UI_PK, UI_Code, UI_Name, UI_ModeOfTransportation, UI_Address, UI_AirwayBillPrefix)
	VALUES (newid(), @Code, @Name, @ModeOfTransportation, @Address, @AirwayBillPrefix)
END

INSERT #TempUSCCarrierSystem SELECT UI_PK FROM USCCarrier WHERE UI_Code = @Code";
					using (var cmd = conn.Command(carrierCommandText))
					{
						cmd.AddParameter("@Code", SqlDbType.VarChar, USCCarrierSchema.UI_Code.MaxLength, carrierCode);
						cmd.AddParameter("@Name", SqlDbType.VarChar, USCCarrierSchema.UI_Name.MaxLength, carrierName);
						cmd.AddParameter("@ModeOfTransportation", SqlDbType.VarChar, USCCarrierSchema.UI_ModeOfTransportation.MaxLength, carrierModeOfTransportation);
						cmd.AddParameter("@Address", SqlDbType.VarChar, USCCarrierSchema.UI_Address.MaxLength, carrierAddress);
						cmd.AddParameter("@AirwayBillPrefix", SqlDbType.VarChar, USCCarrierSchema.UI_AirwayBillPrefix.MaxLength, carrierAirwayBillPrefix);
						cmd.ExecuteNonQuery();
					}
				}
			}

			// Shared DB use different connection so we should do delete non-client specific data from shared db and the transformation can get them from the old ref db.
			if (RefDbTableNameResolver.IsSharedDatabase(conn.CurrentDatabase))
			{
				conn.ExecuteNonQuery(@"
DELETE USCCarrier WHERE UI_PK NOT IN (SELECT US_PK FROM #TempUSCCarrierSystem);
DROP TABLE #TempUSCCarrierSystem;
");
			}
		}

		#region USCImportEstablishment

		public void AddUSCImportEstablishment()
		{
			upgrader.CreateTables(conn, new ITableScript[] { new USCImportEstablishment(), new USCImportEstablishmentAlternateName() });
		}

		#endregion

		public void AddUSCDataVersionIfNecessary()
		{
			upgrader.CreateTables(conn, new ITableScript[] { new USCDataVersion() });
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCDataVersion", "UZ_UpdateTime", "DATETIME NULL"));
		}

		public void AddExtraUSCForeignPort()
		{
			upgrader.CreateTables(conn, new ITableScript[] { new USCForeignPort() });
			if (DbObjectCreator.IndexExists(conn, "USCForeignPort", "NR_UX__UH_Code")) // new index is NR_IX__UH_Code (non-unique)
			{
				conn.ExecuteNonQuery("DROP INDEX NR_UX__UH_Code ON USCForeignPort");
			}

			if (DbObjectCreator.IndexExists(conn, "USCForeignPort", "IX_USCForeignPort")) // new index is NR_IX__UH_Code (non-unique)
			{
				conn.ExecuteNonQuery("DROP INDEX IX_USCForeignPort ON USCForeignPort");
			}

			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCForeignPort", "UH_ValidForType", "varchar(3) NOT NULL CONSTRAINT DF_USCForeignPort_UH_ValidForType DEFAULT ('')"));
			conn.ExecuteNonQuery("DELETE USCForeignPort WHERE UH_ValidForType <> ''");

			string addExtraForeignPorts = AESForeignPortsInsertString + @"

-- Foreign Port for InBond

	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80101', 'Alberta', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80102', 'Manitoba', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80103', 'Saskatchewan', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80104', 'Northwest Territories', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80105', 'Yukon', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80106', 'British Columbia', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80108', 'Quebec', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80109', 'Nova Scotia', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80110', 'New Brunswick', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80111', 'Prince Edward Island', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80112', 'Newfoundland', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80113', 'Nunavut ', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '80107', 'Ontario', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97101', 'Aguascalientes', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97102', 'Baja California Norte', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97103', 'Baja California Sur', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97104', 'Chihuahua', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97105', 'Colima', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97106', 'Campeche', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97107', 'Coahuila', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97108', 'Chiapas', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97109', 'Distrito Federal', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97110', 'Durango', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97111', 'Guerrero', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97112', 'Guanajuato', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97113', 'Hidalgo', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97114', 'Jalisco', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97115', 'Michoacan', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97116', 'Morelos', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97117', 'Mexico', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97118', 'Navarit', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97119', 'Nuevo Leon', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97120', 'Oaxaca', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97121', 'Puebla', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97122', 'Quintana Roo', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97123', 'Queretaro', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97124', 'Sinaloa', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97125', 'San Luis Potosi', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97126', 'Sonora', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97127', 'Tabasco', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97128', 'Tlaxcala', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97129', 'Tamaulipas', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97130', 'Veracruz', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97131', 'Yucatan', 'INB')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '97132', 'Zacatecas', 'INB')";
			conn.ExecuteNonQuery(addExtraForeignPorts);
		}

		public void AddExtraUSCForeignPortIfNeeded()
		{
			AddExtraUSCForeignPort();
		}

		public void USCQuotaTableChanges()
		{
			string sqlText = @"
				if exists (select 1
										from INFORMATION_SCHEMA.COLUMNS
										where TABLE_NAME = 'USCQuota'
										and COLUMN_NAME = 'UT_LastTrasactionDate'
										and IS_NULLABLE = 'NO')
				begin
					alter table USCQuota
						alter
							column UT_LastTrasactionDate datetime NULL

					alter table USCQuota
						add
								constraint DF__USCQuota__UT_Code_1
								default '' FOR UT_Code,

								constraint DF__USCQuota__UT_UC_NKOriginCountry_1
								default '' FOR UT_UC_NKOriginCountry,

								constraint DF__USCQuota__UT_QuotaLimit_1
								default 0 FOR UT_QuotaLimit,

								constraint DF__USCQuota__UT_TextileConversionFactor_1
								default 0 FOR UT_TextileConversionFactor,

								constraint DF__USCQuota__UT_ThresholdQty_1
								default 0 FOR UT_ThresholdQty
				end";

			conn.ExecuteNonQuery(sqlText);
		}

		public void UpdateUSCForeignPortIndex()
		{
			if (DbObjectCreator.IndexExists(conn, "USCForeignPort", "NR_IX__UH_Code")) // new index is NR_IX__UH_Code (non-unique)
			{
				conn.ExecuteNonQuery("DROP INDEX NR_IX__UH_Code ON USCForeignPort");
			}
		}

		public void RenameDF_USCScheduleB_UB_CodeCheckConstraint()
		{
			conn.ExecuteNonQuery(@"
IF EXISTS (SELECT null FROM sys.objects WHERE name = 'DF_USCScheduleB_UB_Code')
EXEC sp_rename 'DF_USCScheduleB_UB_Code', 'CK_USCScheduleB_UB_Code', 'OBJECT'");
		}

		public void RemoveUSCCarierWithEmptyCodeAndAddConstraint()
		{
			string deleteAndAdd = @"
DELETE [dbo].[USCCarrier] WHERE [UI_Code]=''
IF NOT EXISTS (SELECT null FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_USCCarrier_UI_CodeNotEmpty]') AND parent_object_id = OBJECT_ID(N'[dbo].[USCCarrier]'))
ALTER TABLE [dbo].[USCCarrier] WITH CHECK ADD CONSTRAINT [CK_USCCarrier_UI_CodeNotEmpty] CHECK ([UI_Code]<>'')
";
			conn.ExecuteNonQuery(deleteAndAdd);
		}

		public void UpdateAESPortNames()
		{
			conn.ExecuteNonQuery("DELETE USCForeignPort WHERE UH_ValidForType = 'AES'");
			conn.ExecuteNonQuery(AESForeignPortsInsertString);
		}

		string AESForeignPortsInsertString
		{
			get
			{
				return @"
-- Foreign Ports for AES

	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4901', 'AGUADILLA, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4904', 'FAJARDO, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4907', 'MAYAGUEZ, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4908', 'PONCE, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4909', 'SAN JUAN, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4913', 'INTL AIRPORT, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '57042', 'QINZHOU, CHINA', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5101', 'CHARLOTTE AMALIE, USVI', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5102', 'CRUZ BAY, VI', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5104', 'CHRISTIANSTED, VI', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5105', 'FREDERIKSTED, VI', 'AES')
	";
			}
		}

		#endregion

		#region Replace Schedule B Data

		internal void ReplaceScheduleBData()
		{
			if (!DbObjectCreator.TableExists(conn, "USCScheduleB"))
			{
				const string USCScheduleBCreationScript = @"CREATE TABLE USCScheduleB (UB_PK uniqueidentifier CONSTRAINT PK_USCScheduleB PRIMARY KEY NONCLUSTERED,UB_Code varchar (10) NOT NULL CONSTRAINT CK_USCScheduleB_UB_Code CHECK ([UB_Code] <> ''),UB_Unit1 varchar (3) NULL CONSTRAINT DF_USCScheduleB_UB_StatisticalUnit1 DEFAULT (''),UB_Unit2 varchar (3) NULL CONSTRAINT DF_UB_StatisticalUnit2 DEFAULT (''),UB_ShortDescription varchar (50) NULL CONSTRAINT DF_USCScheduleB_UB_ShortDescription DEFAULT (''))
CREATE UNIQUE INDEX NR_IX__UB_Code ON USCScheduleB(UB_Code)";

				conn.ExecuteNonQuery(USCScheduleBCreationScript);
			}

			TruncateTable(conn, "USCScheduleB");

			string tariffCode, description, uq1, uq2;

			using (StreamReader reader = ScheduleBDataCSV)
			{
				string refData = string.Empty;
				while ((refData = reader.ReadLine()) != null)
				{
					tariffCode = refData.Substring(0, 10);
					description = DataUtils.EscapeSingleQuotes(refData.Substring(15, 50));
					uq1 = refData.Substring(170, 3);
					uq2 = refData.Substring(178, 3);

					string strSql = string.Format(@"INSERT INTO USCScheduleB (UB_PK, UB_Code, UB_Unit1, UB_Unit2, UB_ShortDescription)
			VALUES (newid(), '{0}', '{1}', '{2}', '{3}')", tariffCode, uq1.Trim(), uq2.Trim(), description.Trim());
					try
					{
						conn.ExecuteNonQuery(strSql);
					}
					catch (Exception ex)
					{
						throw new Exception(ex.Message + "\n" + strSql, ex);
					}
				}
			}
			UpdateScheduleBDataVersion();
		}

		void TruncateTable(DbConnection conn, string tableName)
		{
			conn.ExecuteNonQuery("TRUNCATE TABLE " + tableName);
		}

		void UpdateScheduleBDataVersion()
		{
			const string script = @"
UPDATE USCDataVersion SET UZ_Version = '{0}', UZ_UpdateTime = CURRENT_TIMESTAMP WHERE UZ_Name = 'ScheduleBDataVersion';
IF @@ROWCOUNT = 0
BEGIN
  INSERT INTO USCDataVersion(UZ_PK, UZ_Name, UZ_Version, UZ_Note, UZ_UpdateTime) VALUES (newid(), 'ScheduleBDataVersion', '{0}', 'ScheduleBDataVersion', CURRENT_TIMESTAMP)
END
";

			AddUSCDataVersionIfNecessary();
			conn.ExecuteNonQuery(string.Format(script, USReferenceDbUpgrader.ScheduleBDataVersionYear));
		}

		#endregion

		#region Version 42 - Add ACE tables

		internal void AddACETablesForAntidumpingCountervailingDuty()
		{
			upgrader.CreateTables(conn, new ITableScript[] {
				new USCACCase(),
				new USCACCaseRate(),
				new USCACCaseEvent(),
				new USCACCaseBondCash(),
				new USCACCaseTariff(),
				new USCACCaseLiqSuspension()
			});
		}

		#endregion

		#region Version 57 - AddMissingSchDPorts

		public void AddMissingSchDPorts()
		{
			string addText = @"
	IF NOT EXISTS (SELECT null FROM dbo.USCRegionDistrictPort WHERE UR_Code = '1113')
	BEGIN
		INSERT INTO USCRegionDistrictPort (UR_PK, UR_Code, UR_Name, UR_State, UR_PortOfUnlading)
			VALUES (NEWID(), '1113', 'GLOUCESTER CITY', 'NJ', 'N')
	END

	IF NOT EXISTS (SELECT null FROM dbo.USCRegionDistrictPort WHERE UR_Code = '4117')
	BEGIN
		INSERT INTO USCRegionDistrictPort (UR_PK, UR_Code, UR_Name, UR_State, UR_PortOfUnlading)
			VALUES (NEWID(), '4117', 'HURON', 'OH', 'N')
	END

	IF NOT EXISTS (SELECT null FROM dbo.USCRegionDistrictPort WHERE UR_Code = '4121')
	BEGIN
		INSERT INTO USCRegionDistrictPort (UR_PK, UR_Code, UR_Name, UR_State, UR_PortOfUnlading)
			VALUES (NEWID(), '4121', 'LORAIN', 'OH', 'N')
	END";

			conn.ExecuteNonQuery(addText);
		}

		#endregion

		#region Version 58 - Update AES Response Codes

		public void UpdateUSCAESResponseCode()
		{
			conn.ExecuteNonQuery("DELETE FROM USCAESResponseCode");

			using (StreamReader reader = USCAESResponseCodeDataCSV)
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					string[] elements = new OCsvLine(line, '').FieldValues;
					string code = elements[0];
					string severity = elements[1];
					string narrativeText = elements[2];

					string sqlText = string.Format(@"INSERT INTO USCAESResponseCode (UY_PK, UY_CODE, UY_Severity, UY_NarrativeText) VALUES (newid(), '{0}', '{1}', '{2}'){3}", code, severity, narrativeText, System.Environment.NewLine);
					conn.ExecuteNonQuery(sqlText);
				}
			}
		}

		#endregion

		#region Version 62 - Adjust USCTariff Schema For UE_OGACodes column

		public void AdjustUSCTariffSchemaForUE_OGACodes()
		{
			string script = @"IF EXISTS(
					SELECT * FROM INFORMATION_SCHEMA.COLUMNS
					WHERE TABLE_NAME = 'USCTariff'
					AND COLUMN_NAME = 'UE_OGACodes'
					AND DATA_TYPE = 'varchar'
					AND CHARACTER_MAXIMUM_LENGTH != 30
				)
					ALTER TABLE USCTariff ALTER COLUMN UE_OGACodes varchar(30) NOT NULL";

			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 63 - Add US Country Codes

		internal void USCCountry_AddUcCode()
		{
			upgrader.CreateTables(conn, new ITableScript[] { new USCCountry() });
			var scriptToAddColumn = DbSchemaChange.GetAddColumnIfNotExistsScript("USCCountry", "UC_Code", "char(2) NOT NULL DEFAULT('')");
			conn.ExecuteNonQuery(scriptToAddColumn);

			string scriptToCleanUpIndexes = @"
--This index was added on UC_ISOCountryCode when the table is created
if exists(select null from sys.indexes where name = 'NR_IX__UC_Code')
	begin
		drop index usccountry.NR_IX__UC_Code
	end

IF EXISTS (SELECT null FROM sys.indexes WHERE name = 'NR_IX__UC_ISOCountryCode')
	begin
		DROP INDEX USCCountry.NR_IX__UC_ISOCountryCode
	end

--This index appeared on the backup somehow.
IF EXISTS (SELECT null FROM sys.indexes WHERE name = 'UC_Code')
	begin
		DROP INDEX USCCountry.UC_Code
	end
";
			conn.ExecuteNonQuery(scriptToCleanUpIndexes);

			//UC_IsoCountryCode is planned to be removed in the future. This extra check won't cause a problem.
			if (DbObjectCreator.ColumnExists(conn, "usccountry", "UC_IsoCountryCode"))
			{
				string scriptToAlter = @"update usccountry set UC_Code = UC_IsoCountryCode where UC_Code = ''";
				conn.ExecuteNonQuery(scriptToAlter);
			}

			string scriptToCreateIndex = @"
delete from USCCountry where UC_Code = '' or UC_Code in (select UC_Code from USCCountry group by UC_Code having count(*) > 1)
CREATE UNIQUE INDEX NR_IX__UC_Code ON USCCountry(UC_Code)
";
			conn.ExecuteNonQuery(scriptToCreateIndex);
		}

		#endregion

		#region Version 64 - Change index on USCACCase and Add Add Clustered indexes on USCACCase prefix

		public void ChangeIndexOnUSCACCasePrefixTables()
		{
			ChangeIndexOnTable("USCACCase", "U5_CaseNumber", "NR_UC__U5_CaseNumber");
			ChangeIndexOnTable("USCACCaseBondCash", "U8_CaseNumber", "NR_RC__U8_CaseNumber");
			ChangeIndexOnTable("USCACCaseEvent", "U7_CaseNumber", "NR_RC__U7_CaseNumber");
			ChangeIndexOnTable("USCACCaseLiqSuspension", "UN_CaseNumber", "NR_RC__UN_CaseNumber");
			ChangeIndexOnTable("USCACCaseRate", "U6_CaseNumber", "NR_RC__U6_CaseNumber");
			ChangeIndexOnTable("USCACCaseTariff", "U9_CaseNumber", "NR_RC__U9_CaseNumber");
		}

		void ChangeIndexOnTable(string tableName, string cloumnName, string indexName)
		{
			if (DbObjectCreator.IndexExists(conn, tableName, indexName))
			{
				conn.ExecuteNonQuery(string.Format("Drop index [{0}] ON [{1}]", indexName, tableName));
			}
			string script =
				@"IF EXISTS(select name from sys.indexes where object_name(object_id)='{0}' and (type_desc = 'CLUSTERED'))
					begin
						declare @indexname varchar(128),@sql varchar(256)
						set @indexname = (select name from sys.indexes where object_name(object_id)='{0}' and (type_desc = 'CLUSTERED'))
						set @sql =  ('Drop index [' + @indexname + '] on [{0}]')
						exec (@sql)
					end";
			script = string.Format(script, tableName);
			conn.ExecuteNonQuery(script);
			if (DbObjectCreator.TableExists(conn, tableName) && DbObjectCreator.ColumnExists(conn, tableName, cloumnName))
			{
				if (tableName == "USCACCase")
				{
					if (DbObjectCreator.IndexExists(conn, "USCACCase", "NR_U5__U5_CaseNumber"))
					{
						conn.ExecuteNonQuery("Drop index [NR_U5__U5_CaseNumber] ON [USCACCase]");
					}
				}
			}
		}

		#endregion

		#region Version 65 - Adjust USCTariff Schema For UE_OGACodes column Part 2

		public void AdjustUSCTariffSchemaForUE_OGACodes_2()
		{
			string script = @"IF EXISTS(
					SELECT * FROM INFORMATION_SCHEMA.COLUMNS
					WHERE TABLE_NAME = 'USCTariff'
					AND COLUMN_NAME = 'UE_OGACodes'
					AND DATA_TYPE = 'varchar'
					AND CHARACTER_MAXIMUM_LENGTH != 75
				)
					ALTER TABLE USCTariff ALTER COLUMN UE_OGACodes varchar(75) NOT NULL";

			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 66 - Move Error Tables into Code from RefDB_Ent_US uscaceerror and uscabierror

		public void ReMoveUSErrorTables()
		{
			string script =
				@"IF EXISTS (SELECT null FROM sys.objects where name = 'USCACEError' and type_desc = 'USER_TABLE')
					begin
						drop table USCACEError
					end

					IF EXISTS (SELECT null FROM sys.objects where name = 'USCABIError' and type_desc = 'USER_TABLE')
					begin
						drop table USCABIError
					end";
			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 68 - Add new Column UE_PGACodes to USCTariff

		public void AddNewColumnPGACodesToUSCTariff()
		{
			string script = @"
IF NOT EXISTS(SELECT null from information_Schema.columns where COLUMN_NAME = 'UE_PGACodes' and TABLE_NAME = 'USCTariff')
	begin
		alter table USCTariff add UE_PGACodes varchar(60) NOT NULL CONSTRAINT [DF_USCTariff_UE_PGACodes]  DEFAULT ('')
	end";
			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 69 - Rebuild USCRule related data

		public void ReBuildUSCTariffRule()
		{
			ClearExistingTariffRule();
			PopulateUSCRule();
			PopulateUSCTariffRule(USCTariffRuleDataCSV);
			PopulateUSCRuleSecondaryTariff(USCRuleSecondaryTariffDataCSV);
			PopulateUSCRuleSecondaryTariffException(USCRuleSecondaryTariffExceptionDataCSV);
		}

		void ClearExistingTariffRule()
		{
			upgrader.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCRuleSecondaryTariff(),
				new USCRuleSecondaryTariffException(),
			});

			string deleteRuleScript = @"
DELETE USCRuleSecondaryTariffException
DELETE USCRuleSecondaryTariff
DELETE USCTariffRuleException
DELETE USCTariffRule
DELETE USCRule";
			conn.ExecuteNonQuery(deleteRuleScript);

			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRule", "U1_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRuleException", "U2_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCRuleSecondaryTariff", "U3_Tariff2", "varchar(10) NOT NULL DEFAULT('')"));
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCRuleSecondaryTariff", "U3_Tariff3", "varchar(10) NOT NULL DEFAULT('')"));
		}

		void PopulateUSCRule()
		{
			upgrader.ClearAndRePopulateTables(conn, new IPopulateData[] { new USCRule() });
		}

		void PopulateUSCTariffRule(StreamReader streamReader)
		{
			//The data in the CSV were generated from source data as follow:
			//SELECT U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo, U1_TariffTo, U1_PK
			//FROM USCTariffRule
			//ORDER BY U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo
			using (var reader = streamReader)
			{
				string tariffRuleData;
				var scriptBuilder = new StringBuilder();
				var count = 0;
				while ((tariffRuleData = reader.ReadLine()) != null)
				{
					count++;
					string[] splitTariffRuleData = new OCsvLine(tariffRuleData, '').FieldValues;

					var ruleCode = splitTariffRuleData[0];
					var tariff = splitTariffRuleData[1];
					var dateFrom = splitTariffRuleData[2];
					var dateTo = splitTariffRuleData[3];
					if (string.IsNullOrEmpty(dateTo))
					{
						dateTo = "NULL";
					}
					else
					{
						dateTo = "'" + dateTo + "'";
					}
					var tariffTo = splitTariffRuleData[4];
					var tariffRulePK = splitTariffRuleData[5];

					scriptBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, @"INSERT USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo, U1_TariffTo)
VALUES ('{0}', '{1}', '{2}', '{3}', {4}, '{5}')",
						tariffRulePK, ruleCode, tariff, dateFrom, dateTo, tariffTo));

					if (count % 100 == 0)
					{
						conn.ExecuteNonQuery(scriptBuilder.ToString());
						count = 0;
						scriptBuilder.Clear();
					}
				}
				if (scriptBuilder.Length > 0)
				{
					conn.ExecuteNonQuery(scriptBuilder.ToString());
				}
			}
		}

		void PopulateUSCRuleSecondaryTariff(StreamReader streamReader)
		{
			//The data in the CSV were generated from source data as follow:
			//SELECT U3_U1, U3_TariffFrom, U3_TariffTo, U3_DateFrom, U3_DateTo, U3_Tariff2, U3_Tariff3, U3_PK
			//FROM USCRuleSecondaryTariff
			//INNER JOIN USCTariffRule ON U3_U1 = U1_PK
			//ORDER BY U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo, U3_TariffFrom, U3_TariffTo, U3_DateFrom

			using (var reader = streamReader)
			{
				string ruleSecondaryTariffData;
				var scriptBuilder = new StringBuilder();
				var count = 0;
				while ((ruleSecondaryTariffData = reader.ReadLine()) != null)
				{
					count++;
					string[] splitRuleSecondaryTariffData = new OCsvLine(ruleSecondaryTariffData, '').FieldValues;

					var tariffRulePK = splitRuleSecondaryTariffData[0];
					var tariffFrom = splitRuleSecondaryTariffData[1];
					var tariffTo = splitRuleSecondaryTariffData[2];
					var dateFrom = splitRuleSecondaryTariffData[3];
					var dateTo = splitRuleSecondaryTariffData[4];
					if (string.IsNullOrEmpty(dateTo))
					{
						dateTo = "NULL";
					}
					else
					{
						dateTo = "'" + dateTo + "'";
					}
					var tariff2 = splitRuleSecondaryTariffData[5];
					var tariff3 = splitRuleSecondaryTariffData[6];
					var ruleSecondaryTariffPK = splitRuleSecondaryTariffData[7];

					scriptBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, @"INSERT USCRuleSecondaryTariff (U3_PK, U3_U1, U3_TariffFrom, U3_TariffTo, U3_DateFrom, U3_DateTo, U3_Tariff2, U3_Tariff3)
VALUES ('{7}', '{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}')",
						tariffRulePK, tariffFrom, tariffTo, dateFrom, dateTo, tariff2, tariff3, ruleSecondaryTariffPK));

					if (count % 100 == 0)
					{
						conn.ExecuteNonQuery(scriptBuilder.ToString());
						count = 0;
						scriptBuilder.Clear();
					}
				}
				if (scriptBuilder.Length > 0)
				{
					conn.ExecuteNonQuery(scriptBuilder.ToString());
				}
			}
		}

		void PopulateUSCRuleSecondaryTariffException(StreamReader streamReader)
		{
			//The data in the CSV were generated from source data as follow:
			//SELECT U4_U3, U4_Tariff, U4_DateFrom, U4_DateTo
			//FROM USCRuleSecondaryTariffException
			//INNER JOIN USCRuleSecondaryTariff ON U4_U3 = U3_PK
			//INNER JOIN USCTariffRule ON U3_U1 = U1_PK
			//ORDER BY U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo, U3_TariffFrom, U3_TariffTo, U3_DateFrom, U4_Tariff, U4_DateFrom
			using (var reader = streamReader)
			{
				string ruleSecondaryTariffExceptionData;
				var scriptBuilder = new StringBuilder();
				var count = 0;
				while ((ruleSecondaryTariffExceptionData = reader.ReadLine()) != null)
				{
					count++;
					string[] splitRuleSecondaryTariffExceptionData = new OCsvLine(ruleSecondaryTariffExceptionData, '').FieldValues;

					var ruleSecondaryTariffPK = splitRuleSecondaryTariffExceptionData[0];
					var tariff = splitRuleSecondaryTariffExceptionData[1];
					var dateFrom = splitRuleSecondaryTariffExceptionData[2];
					var dateTo = splitRuleSecondaryTariffExceptionData[3];
					if (string.IsNullOrEmpty(dateTo))
					{
						dateTo = "NULL";
					}
					else
					{
						dateTo = "'" + dateTo + "'";
					}

					scriptBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, @"INSERT USCRuleSecondaryTariffException (U4_PK, U4_U3, U4_Tariff, U4_DateFrom, U4_DateTo)
VALUES (NEWID(), '{0}', '{1}', '{2}', {3})",
						ruleSecondaryTariffPK, tariff, dateFrom, dateTo));

					if (count % 100 == 0)
					{
						conn.ExecuteNonQuery(scriptBuilder.ToString());
						count = 0;
						scriptBuilder.Clear();
					}
				}
				if (scriptBuilder.Length > 0)
				{
					conn.ExecuteNonQuery(scriptBuilder.ToString());
				}
			}
		}

		#endregion

		#region Version 71 - Add new bit column UL_IsExpired in USCAffirmationOfCompliance with a default value of 0

		public void AddNewColumnIsExpiredToUSCAffirmationOfCompliance()
		{
			string script =
				@"if not exists (select 1
										from INFORMATION_SCHEMA.COLUMNS
										where TABLE_NAME = 'USCAffirmationOfCompliance'
										and COLUMN_NAME = 'UL_IsExpired'
										and IS_NULLABLE = 'NO')
				begin
					alter table USCAffirmationOfCompliance add UL_IsExpired bit NOT NULL CONSTRAINT [DF_USCAffirmationOfCompliance_UL_IsExpired] default 0 with values
				end";
			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 73 - ModifySTNRule, remove Tariff 8206000000 From STN Rule and modify Tariff 9822.05.15

		// delete tariff '17011110' for STN rule tariff 9822.05.15
		public void ModifySTNRule()
		{
			//delete U1_RuleCode = 'STN' AND U1_Tariff = '8206000000'
			string deleteSql = @"
DELETE USCRuleSecondaryTariff WHERE U3_U1 = 'A60AB456-80B0-4257-9AB8-9386800B4FFC'

DELETE USCTariffRule WHERE U1_pk = 'A60AB456-80B0-4257-9AB8-9386800B4FFC'

UPDATE USCRuleSecondaryTariff SET U3_DateTo = '2013-12-31 00:00:00.000' WHERE U3_PK = 'E386B9AD-6CAD-4DA2-889B-9CC4F76A0231'

IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '23063361-F8CD-4D74-B588-48253F110BE2')
BEGIN
	INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom)
		VALUES ('23063361-F8CD-4D74-B588-48253F110BE2', '799B28B2-0D35-4D65-A036-06D484E3E295', '17011310', '2000-01-01 00:00:00.000')
END

IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'BBF487C2-F10D-4BC6-B5C8-14796ED57748')
BEGIN
	INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom)
		VALUES ('BBF487C2-F10D-4BC6-B5C8-14796ED57748', '799B28B2-0D35-4D65-A036-06D484E3E295', '17011410', '2000-01-01 00:00:00.000')
END

";
			conn.ExecuteNonQuery(deleteSql);
		}

		#endregion

		#region Version 76 - Modify STN Tariff Rule For 9911.97.00

		public void ModifySTNTariffRuleFor99119700()
		{
			string modifySTNSQL = @"
IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '02A85728-FA17-4818-B44F-16361D09C461')
BEGIN
	INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom)
		VALUES ('02A85728-FA17-4818-B44F-16361D09C461', '2EA18AB8-A362-43B0-8745-7F99129BE432', '2008979030', '2000-01-01 00:00:00.000')
END
";
			conn.ExecuteNonQuery(modifySTNSQL);
		}

		#endregion

		#region Version 79 - Add TBP and TBC and TBF Tariff Rule

		public void AddTBPAndTBCAndTBFTariffRule()
		{
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRule", "U1_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));

			using (StreamReader reader = USCTariffRuleTTBDataCSV)
			{
				string data;
				var script = new SqlQueryBuilder(@"
IF EXISTS (SELECT NULL FROM USCRule where U0_Code = 'TB1')
BEGIN
	DELETE USCTariffRule WHERE U1_RuleCode = 'TB1'
	DELETE USCRule WHERE U0_Code = 'TB1'
END
IF EXISTS (SELECT NULL FROM USCRule where U0_Code = 'TB2')
BEGIN
	DELETE USCTariffRule WHERE U1_RuleCode = 'TB2'
	DELETE USCRule WHERE U0_Code = 'TB2'
END

IF NOT EXISTS (SELECT NULL FROM USCRule where U0_Code = 'TBP') INSERT INTO USCRule(U0_Code) VALUES ('TBP')
IF NOT EXISTS (SELECT NULL FROM USCRule where U0_Code = 'TBC') INSERT INTO USCRule(U0_Code) VALUES ('TBC')
IF NOT EXISTS (SELECT NULL FROM USCRule where U0_Code = 'TBF') INSERT INTO USCRule(U0_Code) VALUES ('TBF')");

				while ((data = reader.ReadLine()) != null)
				{
					var splitData = new OCsvLine(data, ',').FieldValues;
					var ruleCode = splitData[0];
					var tariff = splitData[1];
					var dateFrom = splitData[2];
					var tariffTo = splitData.Length == 4 ? splitData[3] : "";

					script.Append(string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS (SELECT * FROM USCTariffRule WHERE U1_RuleCode = '{0}' and U1_Tariff = '{1}')
INSERT INTO USCTariffRule(U1_RuleCode, U1_Tariff, U1_DateFrom, U1_TariffTo) VALUES('{0}', '{1}', '{2}', '{3}')", ruleCode, tariff, dateFrom, tariffTo));
				}
				conn.ExecuteNonQuery(script.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region Version 80 - Add Tariff number 96083000 for 960850

		public void ModifyHTSTariffRuleFor96083000()
		{
			string modifySTNSQL = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS (SELECT * FROM USCRuleSecondaryTariff WHERE U3_PK = 'FBADD9B6-CD31-42CD-BE21-6A17D71E8479')
BEGIN
	INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom)
		VALUES ('FBADD9B6-CD31-42CD-BE21-6A17D71E8479', '006DA522-211D-4BA1-A8A2-27C80FEF0257', '960830', '2000-01-01 00:00:00.000')
END
");
			conn.ExecuteNonQuery(modifySTNSQL);
		}

		#endregion

		#region Version 81 - Add US AMS Products

		public void AddUSAMSProducts()
		{
			upgrader.CreateTables(conn, new ITableScript[] { new USCAMSProductNumber() });

			if (DbObjectCreator.TableExists(conn, "USCAMSProductNumber"))
			{
				using (StreamReader reader = USAMSProductDataCSV)
				{
					string data;
					while ((data = reader.ReadLine()) != null)
					{
						var splitData = new OCsvLine(data, ',').FieldValues;
						var title = splitData[0];
						var code = splitData[1];
						var desc = splitData[2];
						var type = splitData[3];

						var commandText = @"IF NOT EXISTS (SELECT NULL FROM USCAMSProductNumber WHERE UA_Code = @code)
BEGIN
INSERT INTO USCAMSProductNumber(UA_PK, UA_Code, UA_Desc, UA_ClassTitle, UA_ProductType) VALUES(newid(), @code, @desc, @classTitle, @productType)
END
";
						using (var cmd = conn.Command(commandText))
						{
							cmd.AddParameter("@code", SqlDbType.VarChar, 8, code.TrimEnd());
							cmd.AddParameter("@desc", SqlDbType.VarChar, 120, desc.TrimEnd());
							cmd.AddParameter("@classTitle", SqlDbType.VarChar, 60, title.TrimEnd());
							cmd.AddParameter("@productType", SqlDbType.VarChar, 3, type.TrimEnd());
							cmd.ExecuteNonQuery();
						}
					}
				}
			}
		}

		#endregion

		#region Version 83 - Update US Zip Code && Add FD0 Tariff Rule

		public void UpdateUSZipCodeAndAddPGAGDAFD0TariffsRule()
		{
			UpdateUSZipCode();
			AddPGAGDAFD0TariffsRule();
		}

		void UpdateUSZipCode()
		{
			if (DbObjectCreator.TableExists(conn, "USCZipCode"))
			{
				var alterTableScript = @"IF EXISTS (SELECT * FROM sys.tables tab  INNER JOIN sys.indexes ind ON tab.object_id = ind.object_id WHERE tab.name = 'USCZipCode' AND ind.name = 'NR_IX__USCZipCode')
DROP INDEX USCZipCode.NR_IX__USCZipCode

truncate table USCZipCode
alter table USCZipCode ALTER COLUMN UZ_BeginZipCodeRange char(5)
alter table USCZipCode ALTER COLUMN UZ_EndZipCodeRange char(5)
CREATE UNIQUE INDEX NR_IX__USCZipCode ON USCZipCode(UZ_State, UZ_BeginZipCodeRange, UZ_EndZipCodeRange)

";
				conn.ExecuteNonQuery(alterTableScript);

				using (StreamReader reader = USZipDataCSV)
				{
					var script = new SqlQueryBuilder();
					string data;
					while ((data = reader.ReadLine()) != null)
					{
						var splitData = new OCsvLine(data, ',').FieldValues;
						if (splitData.Length == 3)
						{
							var beginZipCode = splitData[0];
							var endZipCode = splitData[1];
							var state = splitData[2];
							script.Append(string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO USCZipCode(UZ_PK, UZ_BeginZipCodeRange, UZ_EndZipCodeRange, UZ_State) VALUES(newid(), '{0}', '{1}', '{2}')", beginZipCode.TrimEnd(), endZipCode.TrimEnd(), state.TrimEnd()));
						}
					}
					conn.ExecuteNonQuery(script.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		void AddPGAGDAFD0TariffsRule()
		{
			using (StreamReader reader = USCTariffRuleFDADataCSV)
			{
				string data;
				var script = new SqlQueryBuilder();
				var scriptProgram = new SqlQueryBuilder();
				scriptProgram.Append(" IF NOT EXISTS (SELECT * FROM USCRule where U0_Code = 'FD0') INSERT INTO USCRule(U0_Code) VALUES ('FD0')");
				while ((data = reader.ReadLine()) != null)
				{
					string[] splitData = new OCsvLine(data, ',').FieldValues;
					string ruleCode = splitData[0];
					if (ruleCode == "FD0" && splitData.Length == 2)
					{
						string tariff = splitData[1];
						string dateFrom = "1999-01-01";

						script.Append(string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS (SELECT * FROM USCTariffRule WHERE U1_RuleCode = '{0}' and U1_Tariff = '{1}')
INSERT INTO USCTariffRule(U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES('{0}', '{1}', '{2}')", ruleCode, tariff, dateFrom));
					}
				}
				if (!script.IsEmpty)
				{
					scriptProgram.Append(script.ToString());
				}
				conn.ExecuteNonQuery(scriptProgram.ToString());
			}
		}

		#endregion

		#region Version 84 - Add TTB Price Rule

		public void AddTTBPriceRule()
		{
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRule", "U1_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));

			using (var reader = USCTariffRuleTTBPriceCSV)
			{
				string data;
				var script = new SqlQueryBuilder(@"IF NOT EXISTS (SELECT NULL FROM USCRule WHERE U0_Code = 'TPR') INSERT INTO USCRule(U0_Code) VALUES ('TPR')");

				while ((data = reader.ReadLine()) != null)
				{
					var splitData = new OCsvLine(data, ',').FieldValues;
					var ruleCode = splitData[0];
					var tariff = splitData[1];
					var dateFrom = splitData[2];

					script.Append(string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS (SELECT * FROM USCTariffRule WHERE U1_RuleCode = '{0}' and U1_Tariff = '{1}')
INSERT INTO USCTariffRule(U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES('{0}', '{1}', '{2}')", ruleCode, tariff, dateFrom));
				}
				conn.ExecuteNonQuery(script.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region Version 89 - Update Tariff Rule Exceptions For 9903

		public void UpdateTariffRuleExceptionFor9903()
		{
			upgrader.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException()
			});
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRuleException", "U2_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));

			var script = @"
IF NOT EXISTS (SELECT * FROM USCTariffRuleException WHERE U2_Tariff = '990317' AND U2_TariffTo = '990318')
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '990317', '2000-01-01', '2099-12-31', '990318' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END;

IF NOT EXISTS (SELECT * FROM USCTariffRuleException WHERE U2_Tariff = '990353' AND U2_TariffTo = '')
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '990353', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END;";

			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 92 - Update CFE rule

		public void UpdateCFERule()
		{
			using (var reader = USCTariffRuleCFEDataCSV)
			{
				string tariffRuleData;
				var scriptBuilder = new StringBuilder();
				scriptBuilder.AppendLine(@"DELETE FROM dbo.USCTariffRule WHERE U1_RuleCode = 'CFE';");

				var count = 0;
				while ((tariffRuleData = reader.ReadLine()) != null)
				{
					count++;
					string[] splitTariffRuleData = new OCsvLine(tariffRuleData, '').FieldValues;

					var ruleCode = splitTariffRuleData[0];
					var tariff = splitTariffRuleData[1];
					var dateFrom = splitTariffRuleData[2];
					var dateTo = splitTariffRuleData[3];
					if (string.IsNullOrEmpty(dateTo))
					{
						dateTo = "NULL";
					}
					else
					{
						dateTo = "'" + dateTo + "'";
					}
					var tariffTo = splitTariffRuleData[4];
					var tariffRulePK = splitTariffRuleData[5];

					scriptBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, @"INSERT USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo, U1_TariffTo)
VALUES ('{0}', '{1}', '{2}', '{3}', {4}, '{5}')",
						tariffRulePK, ruleCode, tariff, dateFrom, dateTo, tariffTo));

					if (count % 100 == 0)
					{
						conn.ExecuteNonQuery(scriptBuilder.ToString());
						count = 0;
						scriptBuilder.Clear();
					}
				}
				if (scriptBuilder.Length > 0)
				{
					conn.ExecuteNonQuery(scriptBuilder.ToString());
				}
			}
		}

		#endregion

		#region Version 93 - Update AES Tariff rule (Exclude the tariff numbers in the file)

		public void UpdateAESTariffRuleExcluded()
		{
			using (StreamReader reader = USTariffeRuleAESExcludedCSV)
			{
				string data;
				var script = new SqlQueryBuilder();
				script.Append(@"DELETE FROM dbo.USCTariffRule WHERE U1_RuleCode = 'AES';");

				while ((data = reader.ReadLine()) != null)
				{
					script.Append(string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO dbo.USCTariffRule(U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES('{0}', '{1}', '2000-01-01')", "AES", data));
				}
				if (!script.IsEmpty)
				{
					conn.ExecuteNonQuery(script.ToString());
				}
			}
		}

		#endregion

		#region Version 94 - Modify STN Tariff Rule For 9817.84.01

		public void ModifySTNTariffRuleFor98178401()
		{
			string modifySTNSQL = @"
IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'C1DFDA81-4BB0-4ACA-8CDC-38071237ACF8')
BEGIN
	INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom)
		VALUES ('C1DFDA81-4BB0-4ACA-8CDC-38071237ACF8', 'E265613A-FED1-47D7-9298-F0825C9E3A21', '84798994', '2000-01-01 00:00:00.000')
END
";
			conn.ExecuteNonQuery(modifySTNSQL);

			modifySTNSQL = @"
IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '51C857AE-F1ED-4F21-B02E-45C75C260B2B')
BEGIN
	INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom)
		VALUES ('51C857AE-F1ED-4F21-B02E-45C75C260B2B', 'E265613A-FED1-47D7-9298-F0825C9E3A21', '903180', '2000-01-01 00:00:00.000')
END
";
			conn.ExecuteNonQuery(modifySTNSQL);

			modifySTNSQL = @"
	DELETE USCRuleSecondaryTariff WHERE U3_PK = '991D913E-F319-4187-B4FE-746B710A0B88' AND U3_TariffFrom = '84798998' 
";
			conn.ExecuteNonQuery(modifySTNSQL);
		}

		#endregion

		#region Version 95 - Remove USCCountry With Empty Code And Add Constraint

		public void RemoveUSCCountryWithEmptyCodeAndAddConstraint()
		{
			string deleteAndAdd = @"
IF EXISTS  (SELECT null FROM Sys.Objects WHERE object_ID = object_id(N'[dbo].[USCCountry]') AND OBJECTPROPERTY(object_ID, 'IsTable') = 1) 
DELETE [dbo].[USCCountry] WHERE [UC_Code]=''
IF NOT EXISTS (SELECT null FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_USCCountry_UC_CodeNotEmpty]') AND parent_object_id = OBJECT_ID(N'[dbo].[USCCountry]'))
ALTER TABLE [dbo].[USCCountry] WITH CHECK ADD CONSTRAINT [CK_USCCountry_UC_CodeNotEmpty] CHECK ([UC_Code]<>'')
";
			conn.ExecuteNonQuery(deleteAndAdd);
		}

		#endregion

		#region Version 96 - Adjust USCQuota Schema For USCQuota column UT_FirstNamesake

		public void AdjustUSCQuotaSchemaForUT_FirstNamesake()
		{
			string script = @"IF EXISTS(
					SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
					WHERE TABLE_NAME = 'USCQuota'
					AND COLUMN_NAME = 'UT_FirstNamesake'
					AND DATA_TYPE = 'varchar'
					AND CHARACTER_MAXIMUM_LENGTH != 15
				)
					ALTER TABLE USCQuota ALTER COLUMN UT_FirstNamesake varchar(15) NOT NULL";

			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 98 - Update Tariff Rule Exceptions For 9903

		public void UpdateTariffRuleExceptionFor9903_1()
		{
			var script = @"
IF NOT EXISTS (SELECT * FROM USCTariffRuleException WHERE U2_Tariff = '99038501' AND U2_TariffTo = '')
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '99038501', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END;

IF NOT EXISTS (SELECT * FROM USCTariffRuleException WHERE U2_Tariff = '99034005' AND U2_TariffTo = '')
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '99034005', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END;

IF NOT EXISTS (SELECT * FROM USCTariffRuleException WHERE U2_Tariff = '99034105' AND U2_TariffTo = '')
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '99034105', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END;

IF NOT EXISTS (SELECT * FROM USCTariffRuleException WHERE U2_Tariff = '99034110' AND U2_TariffTo = '')
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '99034110', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END;

IF NOT EXISTS (SELECT * FROM USCTariffRuleException WHERE U2_Tariff = '99038001' AND U2_TariffTo = '')
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '99038001', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END;
";

			conn.ExecuteNonQuery(script);
		}

		#endregion

		#region Version 99 - Update A99 Tariff Rule

		public void UpdateA99TariffRule()
		{
			string modifyA99TariffRuleSQL = @"
IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'C0CF62CC-A1D3-4BB0-A149-791C15642AAB')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('C0CF62CC-A1D3-4BB0-A149-791C15642AAB', 'A99', '990380', '2018-03-23 00:00:00.000')
END

IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'FB7D2E0A-D8AE-4D3B-BE53-C4ACFCD64512')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('FB7D2E0A-D8AE-4D3B-BE53-C4ACFCD64512', 'A99', '99038501', '2018-03-23 00:00:00.000')
END

IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'FFB0B1E0-3A98-49FE-84E9-65322FE80F05')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom, U1_DateTo)
	VALUES ('FFB0B1E0-3A98-49FE-84E9-65322FE80F05', 'A99', '990345', '2018-02-07 00:00:00.000', '2021-02-07 00:00:00.000')
END

IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'FDA9E954-6F64-4507-B55A-E2BA5C38762D')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('FDA9E954-6F64-4507-B55A-E2BA5C38762D', 'A99', '99038505', '2018-03-23 00:00:00.000')
END

IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '3C658E9F-BFDA-4F86-A3FA-3D7F84CCD0E4')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('3C658E9F-BFDA-4F86-A3FA-3D7F84CCD0E4', 'A99', '99038801', '2018-07-01 00:00:00.000')
END
";
			conn.ExecuteNonQuery(modifyA99TariffRuleSQL);
		}

		#endregion

		#region Version 101 - UpdateA99TariffRuleFor990380

		public void UpdateA99TariffRuleFor990380()
		{
			string modifyA99TariffRuleSQL = @"UPDATE USCTariffRule SET U1_Tariff = '990380' WHERE U1_PK = 'C0CF62CC-A1D3-4BB0-A149-791C15642AAB' AND U1_Tariff <> '990380'";
			conn.ExecuteNonQuery(modifyA99TariffRuleSQL);
		}

		#endregion

		#region Version 102 - A99 => 9903.85, 9903.88 (6 digits)
		public void UpdateTariffRuleFor99038()
		{
			string modifyA99TariffRuleSQL = @"
	DELETE USCTariffRule WHERE U1_PK = 'FDA9E954-6F64-4507-B55A-E2BA5C38762D' AND U1_Tariff = '99038505' 
	UPDATE USCTariffRule 
	SET U1_Tariff = '990388' WHERE U1_PK = '3C658E9F-BFDA-4F86-A3FA-3D7F84CCD0E4' AND U1_Tariff <> '990388'
	UPDATE USCTariffRule 
	SET U1_Tariff = '990385' WHERE U1_PK = 'FB7D2E0A-D8AE-4D3B-BE53-C4ACFCD64512' AND U1_Tariff <> '990385'

	UPDATE USCTariffRuleException
	SET U2_Tariff = '990385'
	FROM USCTariffRuleException
	JOIN USCTariffRule ON U2_U1 = U1_PK
	WHERE U2_Tariff = '99038501' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903'

	UPDATE USCTariffRuleException
	SET U2_Tariff = '990380'
	FROM USCTariffRuleException
	JOIN USCTariffRule ON U2_U1 = U1_PK
	WHERE U2_Tariff = '99038001' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903'

IF NOT EXISTS (SELECT * FROM USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '990388' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903' )
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '990388', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END

IF NOT EXISTS (SELECT * FROM USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '990345' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903' )
BEGIN
	INSERT INTO USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
	SELECT NEWID(), U1_PK, '990345', '2000-01-01', '2099-12-31', '' FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END
";
			conn.ExecuteNonQuery(modifyA99TariffRuleSQL);
		}
		#endregion

		#region Version 103 - Rebuild STN 9902 tariff rule

		public void RebuildSTN9902TariffRule()
		{
			string removeExistingSTN9902TariffRule = @"
DELETE USCRuleSecondaryTariffException
FROM USCRuleSecondaryTariffException
JOIN USCRuleSecondaryTariff ON U4_U3 = U3_PK
JOIN USCTariffRule ON U3_U1 = U1_PK
WHERE U1_RuleCode = 'STN' AND U1_Tariff LIKE '9902%'

DELETE USCRuleSecondaryTariff
FROM USCRuleSecondaryTariff
JOIN USCTariffRule ON U3_U1 = U1_PK
WHERE U1_RuleCode = 'STN' AND U1_Tariff LIKE '9902%'

DELETE USCTariffRule
WHERE U1_RuleCode = 'STN' AND U1_Tariff LIKE '9902%'
";

			conn.ExecuteNonQuery(removeExistingSTN9902TariffRule);
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRule", "U1_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRuleException", "U2_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCRuleSecondaryTariff", "U3_Tariff2", "varchar(10) NOT NULL DEFAULT('')"));
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCRuleSecondaryTariff", "U3_Tariff3", "varchar(10) NOT NULL DEFAULT('')"));

			PopulateUSCTariffRule(USCSTN9902TariffRuleCSV);
			PopulateUSCRuleSecondaryTariff(USCSTN9902RuleSecondaryTariffCSV);
		}

		#endregion

		#region Version 104 - Remove empty code data

		public void RemoveEmptyCodeOnUSCFIRMS()
		{
			if (DbObjectCreator.TableExists(conn, "USCFIRMS"))
			{
				conn.ExecuteNonQuery("DELETE FROM USCFIRMS WHERE US_Code = ''");
			}
		}

		#endregion

		#region Version 106 - Update STN 9902 Rule Secondary Tariff For 9902.01.16

		public void UpdateSTN9902RuleSecondaryTariffFor99020116()
		{
			string updateSql = @"
			UPDATE USCRuleSecondaryTariff SET U3_DateTo = '2018-10-31 00:00:00.000' WHERE U3_PK = 'E2A6F3FE-BAE3-4D45-BAE3-07D73346EF09'

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '28CFA4B7-6488-43EC-8137-601DB74DBA64')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('28CFA4B7-6488-43EC-8137-601DB74DBA64', 'E621D0FE-B399-454B-92A5-DE2C78DADAE1', '20098970', '2018-11-01 00:00:00.000', '2020-12-31 00:00:00.000')
			END
			";
			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 107 - Update EGFV in USCCarrier

		public void UpdateEGFVUSCCarrier()
		{
			string updateSql = @"
IF EXISTS (SELECT NULL FROM USCCarrier WHERE UI_Code = 'EGFV')
BEGIN
	UPDATE USCCarrier
		SET UI_NAME = 'EVEREST GLOBAL FREIGHT SERVICES INC',
			UI_ModeOfTransportation = '10',
			UI_Address = '1918 STATE ROUTE 27 EDISON NJ 08 817-3213 US'
		WHERE UI_Code = 'EGFV'
END
ELSE
BEGIN
	INSERT INTO USCCarrier (UI_PK, UI_Code, UI_Name, UI_ModeOfTransportation, UI_Address, UI_AirwayBillPrefix)
	VALUES (newid(), 'EGFV', 'EVEREST GLOBAL FREIGHT SERVICES INC', '10', '1918 STATE ROUTE 27 EDISON NJ 08 817-3213 US', '')
END";

			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 108 - Rebuild PGA AMS Tariff Rule

		public void RebuildPGAAMSTariffsRule()
		{
			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCTariffRule", "U1_TariffTo", "varchar(10) NOT NULL DEFAULT('')"));

			using (StreamReader reader = USCTariffRuleAMSDataCSV)
			{
				string data;
				var script = new SqlQueryBuilder();
				var scriptProgram = new SqlQueryBuilder();
				var ruleCodes = new List<string>(new[] { "EG1", "EG2", "MO1", "MO2", "MO4", "MO5", "MO6", "MO7", "MO8", "PN1" });
				ruleCodes.ForEach(ruleCode =>
				{
					scriptProgram.Append($" IF NOT EXISTS (SELECT NULL FROM USCRule where U0_Code = '{ruleCode}') INSERT INTO USCRule(U0_Code) VALUES ('{ruleCode}')");
				});
				scriptProgram.Append($" DELETE FROM USCTariffRule WHERE U1_RuleCode IN ({string.Join(", ", ruleCodes.Select(ruleCode => $"'{ruleCode}'"))})");

				while ((data = reader.ReadLine()) != null)
				{
					string[] splitData = new OCsvLine(data, ',').FieldValues;
					string ruleCode = splitData[0];
					if (ruleCodes.Contains(ruleCode))
					{
						string tariff = splitData[1];
						string dateFrom = splitData[2];

						script.Append(string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS (SELECT * FROM USCTariffRule WHERE U1_RuleCode = '{0}' and U1_Tariff = '{1}')
INSERT INTO USCTariffRule(U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES('{0}', '{1}', '{2}')", ruleCode, tariff, dateFrom));
					}
				}
				if (!script.IsEmpty)
				{
					scriptProgram.Append(script.ToString());
				}
				conn.ExecuteNonQuery(scriptProgram.ToString());
			}
		}

		#endregion

		#region Version 116 - Update STN Rule For 9817.00.60

		public void UpdateSTNRuleFor98170060()
		{
			string updateSql = @"
DELETE USCRuleSecondaryTariff
WHERE U3_U1 = '7E8717A6-245F-4AF0-B016-CF1473FD72C9'

DELETE USCTariffRule
WHERE U1_Tariff = '9817006000' AND U1_RuleCode = 'STN' AND U1_PK = '7E8717A6-245F-4AF0-B016-CF1473FD72C9'";
			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 117 - Update STN Rule For 9817.95.05 and 9817.95.01

		public void AdjustSTNRuleFor98179501And98179505()
		{
			string updateSql = @"
			UPDATE USCRuleSecondaryTariff SET U3_DateTo = '2021-12-31 00:00:00.000' WHERE U3_PK = '38E95538-F44F-4F2A-91E7-E1BFAEA9D480'

			UPDATE USCRuleSecondaryTariff SET U3_DateTo = '2021-12-31 00:00:00.000' WHERE U3_PK = 'D188CEB6-638A-491A-8A51-C4602EFDBACA'

			UPDATE USCRuleSecondaryTariff SET U3_DateTo = '2021-12-31 00:00:00.000' WHERE U3_PK = 'AB2AB343-42F8-4F6B-985C-CF0A4198FC60'

			UPDATE USCRuleSecondaryTariff SET U3_DateTo = '2021-12-31 00:00:00.000' WHERE U3_PK = '45C07832-91B3-4BA5-B3A3-0CA964C3E747'

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'F735BF60-3543-446D-8D34-91E1283D6116')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('F735BF60-3543-446D-8D34-91E1283D6116', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '853951', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '2EB94A82-5A77-4B9B-9744-FDCA255908DB')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('2EB94A82-5A77-4B9B-9744-FDCA255908DB', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '940521', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '57ECAC0A-F325-44CE-8CDE-1E2D6D2BC800')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('57ECAC0A-F325-44CE-8CDE-1E2D6D2BC800', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '940529', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '8CE0FA58-D8F1-4E70-B002-79970ABA2779')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('8CE0FA58-D8F1-4E70-B002-79970ABA2779', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '940541', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '00039F10-D9B2-4DEB-A329-3FE8ACA58C13')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('00039F10-D9B2-4DEB-A329-3FE8ACA58C13', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '940542', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'EAB9A18E-B790-491D-85D3-1D6AF4E170B3')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('EAB9A18E-B790-491D-85D3-1D6AF4E170B3', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '940549', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'B9ED0A46-146A-4232-8A9C-9409C4825370')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('B9ED0A46-146A-4232-8A9C-9409C4825370', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '853951', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '89DE82EF-5862-40CA-8719-F8A0C09D5B74')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('89DE82EF-5862-40CA-8719-F8A0C09D5B74', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '940521', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '3FC2D32E-806D-4876-9CB8-021DCDC9ED2F')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('3FC2D32E-806D-4876-9CB8-021DCDC9ED2F', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '940529', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'CCE51609-0C1A-428F-82F0-03706F21826E')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('CCE51609-0C1A-428F-82F0-03706F21826E', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '940541', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '72313832-E6A2-4FEE-90EE-7F4F4460E524')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('72313832-E6A2-4FEE-90EE-7F4F4460E524', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '940542', '2022-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '21435E75-15A0-4444-BF21-0C32F4FAB167')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('21435E75-15A0-4444-BF21-0C32F4FAB167', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '940549', '2022-01-01 00:00:00.000', NULL)
			END";
			conn.ExecuteNonQuery(updateSql);
		}
		#endregion

		#region Version 121 - Update STN Rule For USCTariffRule and USCRuleSecondaryTariff

		public void AdjustSTNRuleForUSCTariffRuleAndUSCRuleSecondaryTariff()
		{
			string updateSql = @"
			UPDATE USCTariffRule SET U1_DateTo = '2012-02-02 00:00:00.000' WHERE U1_PK = '8D48D4AA-0233-40FD-8F24-59C3418DFC74'

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '63A0491E-D9E2-4326-8E2E-784617534761')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('63A0491E-D9E2-4326-8E2E-784617534761', '12EA0C89-B3DB-4934-A59E-4E0F377BA90B', '7013322090', '2000-01-01 00:00:00.000', NULL)
			END

			IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'A284107D-AB67-4A51-BA3A-7D2FDCCE7A85')
			BEGIN
				INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
				VALUES ('A284107D-AB67-4A51-BA3A-7D2FDCCE7A85', 'STN', '8205901000', '2012-02-03 00:00:00.000')
			END

			IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '2C85F7E6-D444-447A-B6D8-49A82A70DA1D')
			BEGIN
				INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
				VALUES ('2C85F7E6-D444-447A-B6D8-49A82A70DA1D', 'STN', '8205906000', '2012-02-03 00:00:00.000')
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '0D7EA039-0330-48EA-8736-9AC8548556FC')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('0D7EA039-0330-48EA-8736-9AC8548556FC', 'A284107D-AB67-4A51-BA3A-7D2FDCCE7A85', '8205', '2012-02-03 00:00:00.000', NULL)
			END

			IF NOT EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '386960EE-43FF-46DF-8B49-6B6351FB24E2')
			BEGIN
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
				VALUES('386960EE-43FF-46DF-8B49-6B6351FB24E2', '2C85F7E6-D444-447A-B6D8-49A82A70DA1D', '8205', '2012-02-03 00:00:00.000', NULL)
			END";
			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 122 - Update STN Rule For USCRuleSecondaryTariff

		public void AdjustSTNRuleForUSCRuleSecondaryTariff()
		{
			string updateSql = @"
IF EXISTS(SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '63A0491E-D9E2-4326-8E2E-784617534761')
BEGIN
	UPDATE USCRuleSecondaryTariff
	SET U3_TariffFrom = '7013372090'
	WHERE U3_PK = '63A0491E-D9E2-4326-8E2E-784617534761'
END ELSE
BEGIN
	INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo)
	VALUES('63A0491E-D9E2-4326-8E2E-784617534761', '12EA0C89-B3DB-4934-A59E-4E0F377BA90B', '7013372090', '2000-01-01 00:00:00.000', NULL)
END";
			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 123 - Add 1701.14.50 as Associated Secondary tariff in STN rule for 9822.05.20

		public void AddAssociatedSecondaryTariffFor98220520()
		{
			string updateSql = @"
IF NOT EXISTS(SELECT 1 FROM USCRuleSecondaryTariff WHERE U3_PK = '6B928F4A-B0BE-4D88-92E4-AF1513B2273A')
INSERT INTO dbo.USCRuleSecondaryTariff
(
	U3_PK,
	U3_U1,
	U3_TariffFrom,
	U3_TariffTo,
	U3_DateFrom,
	U3_DateTo
)
VALUES
(
	'6B928F4A-B0BE-4D88-92E4-AF1513B2273A',
	'F2E4CD86-C0E9-4A6B-96D8-A0640C618AC3',
	'17011450',
	'',
	'2000-01-01',
	NULL
)";
			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 124

		public void USCDataVersionWhen25xx()
		{
			string updateSql = @"
UPDATE USCDataVersion
SET UZ_Version = '2422', UZ_UpdateTime = CURRENT_TIMESTAMP
WHERE UZ_Version like '25__' AND UZ_Name = 'LastHTSAttempt'
";
			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 125 - Add 9903.01, 9903.81, 9903.89 for A99 Tariff Rule

		public void AddA99TariffRuleFor9903()
		{
			string updateSql = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.USCTariffRule WHERE U1_PK = 'CD836551-1989-4CC4-8AC4-E131768398E6')
			BEGIN
				INSERT INTO dbo.USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
				VALUES ('CD836551-1989-4CC4-8AC4-E131768398E6', 'A99', '990301', '2000-01-01 00:00:00.000')
			END

			IF NOT EXISTS (SELECT 1 FROM dbo.USCTariffRule WHERE U1_PK = '0D88E7F3-63B5-446D-8E31-50F4DF59200B')
			BEGIN
				INSERT INTO dbo.USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
				VALUES ('0D88E7F3-63B5-446D-8E31-50F4DF59200B', 'A99', '990381', '2000-01-01 00:00:00.000')
			END

			IF NOT EXISTS (SELECT 1 FROM dbo.USCTariffRule WHERE U1_PK = 'BFECB802-731C-45CD-A3E4-9677F4CC3061')
			BEGIN
				INSERT INTO dbo.USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
				VALUES ('BFECB802-731C-45CD-A3E4-9677F4CC3061', 'A99', '990389', '2000-01-01 00:00:00.000')
			END

			IF NOT EXISTS (SELECT 1 FROM dbo.USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '990301' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903')
			BEGIN
				INSERT INTO dbo.USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
				SELECT NEWID(), U1_PK, '990301', '2000-01-01', '2099-12-31', '' FROM dbo.USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
			END

			IF NOT EXISTS (SELECT 1 FROM dbo.USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '990381' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903')
			BEGIN
				INSERT INTO dbo.USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
				SELECT NEWID(), U1_PK, '990381', '2000-01-01', '2099-12-31', '' FROM dbo.USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
			END

			IF NOT EXISTS (SELECT 1 FROM dbo.USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '990389' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903')
			BEGIN
				INSERT INTO dbo.USCTariffRuleException (U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo, U2_TariffTo)
				SELECT NEWID(), U1_PK, '990389', '2000-01-01', '2099-12-31', '' FROM dbo.USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
			END";
			conn.ExecuteNonQuery(updateSql);
		}

		#endregion

		#region Version 126 - DeleteFMETariffRuleFor9903

		public void DeleteFMETariffRuleFor9903()
		{
			string deleteFME9903 = @"DELETE dbo.USCTariffRule WHERE U1_RuleCode = 'FME' and U1_Tariff = '9903' and U1_TariffTo = '9904'";
			conn.ExecuteNonQuery(deleteFME9903);
		}

		#endregion
	}
}
