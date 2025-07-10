using System.Globalization;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.ReferenceDatabases.CA.Scripts;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	[TestedType(typeof(CAReferenceDbUpgrader))]
	sealed class CAReferenceDbUpgraderTest : ReferenceDbUpgraderTest<CAReferenceDbUpgrader>
	{
		protected override CAReferenceDbUpgrader GetNewReferenceDbUpgrader()
		{
			return new CAReferenceDbUpgraderForTesting(upgradeContext, testConnection, logger);
		}

		protected override string[] ExpectedReferenceTables
		{
			get
			{
				return new[]
				{
					"CACExportTariff",
					"CACPlaceOfReport",
					"CACPortOfExit",
					"CACTradeZone",
					"CACUSPortOfExit",
					"CACClassHeader",
					"CACRateHeader",
					"CACTaxRefNumHeader",
					"CACTaxRefNumber",
					"CACRate",
					"CACRateLine",
					"CACTariffHeader",
					"CACTaxRate",
					"CACFIARegTypes",
					"CACFIAEndUseCodes",
					"CACAcrossErrorCodes",
					"CACCBSAOfficeCodes",
					"CACountryPreference",
					"CACCasualImpCommodities",
					"CACCasualImpRates",
					"CACCasualImpDummyHS",
					"CACDocumentTypes",
					"CACPGAHSCode"
				};
			}
		}

		protected override bool ExistingDataRequired
		{
			get { return true; }
		}

		protected override void PerformExtraAssertsAfterUpgrade(DbConnection refDbConn)
		{
			base.PerformExtraAssertsAfterUpgrade(refDbConn);

			AssertEquals("CACExportTariff should have new column CE_IsConveyanceIDRequired", true, DbObjectCreator.ColumnExists(refDbConn, "CACExportTariff", "CE_IsConveyanceIDRequired"));
			AssertEquals("CACPlaceOfReport data populated", 161, GetRowCount(refDbConn, "CACPlaceOfReport"));
			AssertEquals("CACPortOfExit data populated", 254, GetRowCount(refDbConn, "CACPortOfExit"));
			AssertEquals("CACTradeZone data populated", 703, GetRowCount(refDbConn, "CACTradeZone"));
			AssertEquals("CACUSPortOfExit data populated", 532, GetRowCount(refDbConn, "CACUSPortOfExit"));
			AssertOfficialCodeColumn(refDbConn, "CP_OfficialCode", "CK_CACPortOfExit_CP_OfficialCode");
			AssertOfficialCodeColumn(refDbConn, "CR_OfficialCode", "CK_CACPlaceOfReport_CR_OfficialCode");
			AssertEquals("CACFIARegTypes data populated", 174, GetRowCount(refDbConn, "CACFIARegTypes"));
			AssertEquals("CACFIAEndUseCodes data populated", 103, GetRowCount(refDbConn, "CACFIAEndUseCodes"));
			AssertEquals("CACAcrossErrorCodes populated", 3936, GetRowCount(refDbConn, "CACAcrossErrorCodes"));
			AssertEquals("CACCBSAOfficeCodes populated", 272, GetRowCount(refDbConn, "CACCBSAOfficeCodes"));
			AssertOfficeCodesLinkToPortOfExit(refDbConn, "Boissevain MB  from Dunseith ND.", "0507", "3422");
			AssertOfficeCodesLinkToPortOfExit(refDbConn, "Huntington BC from Sumas Wash.", "0817", "3009");
			AssertEquals("CACountryPreference populated", 224, GetRowCount(refDbConn, "CACountryPreference"));
			AssertCountryPreference(refDbConn, "UA", "09,02,32");
			AssertCountryPreference(refDbConn, "US", "10,12,02");
			foreach (var countryCode in new[] { "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "ES", "FI", "FR", "GR", "HR",
				"HU", "IE", "IT", "LT", "LU", "LV", "MT", "NL", "PL", "PT", "RO", "SE", "SI", "SK" })
			{
				AssertCountryPreference(refDbConn, countryCode, "02,31");
			}
			AssertCountryPreference(refDbConn, "AU", "04,02,33");
			AssertCountryPreference(refDbConn, "GB", "02,34");
			AssertCountryPreference(refDbConn, "GI", "02,34");
			AssertCountryPreference(refDbConn, "IM", "02,34");
			AssertCountryPreference(refDbConn, "BN", "02");
			AssertCountryPreference(refDbConn, "CL", "14,02");
			AssertCountryPreference(refDbConn, "JP", "02,33");
			AssertCountryPreference(refDbConn, "MM", "08,09,02");
			AssertCountryPreference(refDbConn, "MY", "02,33");
			AssertCountryPreference(refDbConn, "MX", "11,12,02,33");
			AssertCountryPreference(refDbConn, "NZ", "05,02,33");
			AssertCountryPreference(refDbConn, "PE", "25,02,33");
			AssertCountryPreference(refDbConn, "SG", "02,33");
			AssertCountryPreference(refDbConn, "SS", "08,09,02");
			AssertCountryPreference(refDbConn, "VN", "09,02,33");
			AssertEquals("CACTaxRate populated", 104, GetRowCount(refDbConn, "CACTaxRate"));
			AssertEquals("CACRate.ZC_ParentTableCode created?", true, DbObjectCreator.ColumnExists(refDbConn, "CACRate", "ZC_ParentTableCode"));
			AssertEquals("CACRate.ZC_ParentTable exists?", false, DbObjectCreator.ColumnExists(refDbConn, "CACRate", "ZC_ParentTable"));
			AssertEquals("CACCasualImpCommodities populated", 13, GetRowCount(refDbConn, "CACCasualImpCommodities"));
			AssertEquals("CACCasualImpRates populated", 132, GetRowCount(refDbConn, "CACCasualImpRates"));
			AssertEquals("CACCasualImpDummyHS populated", 39, GetRowCount(refDbConn, "CACCasualImpDummyHS"));
			AssertEquals("CACDocumentTypes populated", 104, GetRowCount(refDbConn, "CACDocumentTypes"));
			AssertEquals("CACPGAHSCode populated", 8405, GetRowCount(refDbConn, "CACPGAHSCode"));

			AssertEquals("CACRateLine.NR_IX__CACRateLine_ZR_ZC_Rate exists", true, DbObjectCreator.IndexExists(refDbConn, "CACRateLine", "NR_IX__CACRateLine_ZR_ZC_Rate"));
			AssertEquals("CACTaxRefNumber.NR_IX__CACTaxRefNumber_ZE_ZD_TaxRefNumHeader exists", true, DbObjectCreator.IndexExists(refDbConn, "CACTaxRefNumber", "NR_IX__CACTaxRefNumber_ZE_ZD_TaxRefNumHeader"));

			AssertOfficeCodesLinkToPortOfExit(refDbConn, "Emerson MB", "0502", "3401");

			AssertDocumentTypes(refDbConn, "5037", "Consumer Products Product Label", "HC", "Y");
			AssertDocumentTypes(refDbConn, "5039", "Veterinary Drugs - Drug Identification (DI)", "HC", "N");
			AssertDocumentTypes(refDbConn, "80", "Carbon steel", "GAC", "N");
			AssertDocumentTypes(refDbConn, "81", "Specialty Steel Products", "GAC", "N");
			AssertDocumentTypes(refDbConn, "83", "Aluminum Products", "GAC", "N");
			AssertDocumentTypes(refDbConn, "5016", "Office of Controlled Substances - Class B Precursor Registration", "HC", "N");

			Assert("CACSubLocation droped", !DbObjectCreator.TableExists(refDbConn, "CACSubLocation"));
			AssertRegSubTypeColumn(refDbConn, "CACFIARegTypes", "FR_RegSubType");

			foreach (var cfiaCode in new[] { "105", "115", "117" })
			{
				AssertCFIARegSubTypeIsWeight(refDbConn, cfiaCode);
			}

			var lastCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
			try
			{
				var script = new CACTaxRate();
				((CAReferenceDbUpgraderForTesting)refDbUpgrader).PopulateTableFromCsvFile_Exposed(refDbConn, script.CsvFileName, script.TableName, script.ColumnSqlList, script.ColumnTypes);
			}
			catch
			{
				Fail("No exception should be thrown due to culture");
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = lastCulture;
			}

			AssertEquals("Y", DataUtils.LoadDbExtendedProperty(refDbConn, CAReferenceDbUpgrader.RefreshRequiredPropertyName));
		}

		void AssertOfficeCodesLinkToPortOfExit(DbConnection refDbConn, string message, string officecode, string portofexit)
		{
			string findQantas = string.Format("select * from CACCBSAOfficeCodes where CQ_Code = '{0}'", officecode);

			using (var reader = refDbConn.Command(findQantas).ExecuteReader())
			{
				AssertEquals(string.Format("Should find an CACCBSAOfficeCodes with CQ_Code {0}", officecode), true, reader.Read());
				AssertEquals(message, portofexit, reader["CQ_RelatedUSPortOfExit"].ToString());
			}
		}

		void AssertDocumentTypes(DbConnection refDbConn, string code, string des, string agencyID, string isDefaultRefNum)
		{
			string findQantas = string.Format("select * from CACDocumentTypes where FR_Code = '{0}'", code);

			using (var reader = refDbConn.Command(findQantas).ExecuteReader())
			{
				AssertEquals(string.Format("Should find an CACDocumentTypes with FR_Code {0}", code), true, reader.Read());
				AssertEquals("FR_Desc", des, reader["FR_Desc"].ToString());
				AssertEquals("FR_GovAgencyIDCode", agencyID, reader["FR_GovAgencyIDCode"].ToString());
				AssertEquals("FR_IsDefaultRefNum", isDefaultRefNum, reader["FR_IsDefaultRefNum"].ToString());
			}
		}

		void AssertCFIARegSubTypeIsWeight(DbConnection refDbConn, string code)
		{
			string findQantas = string.Format("select * from CACFIARegTypes where FR_Code = '{0}'", code);

			using (var reader = refDbConn.Command(findQantas).ExecuteReader())
			{
				AssertEquals(string.Format("Should find a CACFIARegTypes with FR_Code {0}", code), true, reader.Read());
				AssertEquals("FR_RegSubType", "W", reader["FR_RegSubType"].ToString());
			}
		}

		void AssertCountryPreference(DbConnection refDbConn, string countryCode, string validTariffTreatments)
		{
			var sqlText = $"select CA_ValidTariffTreatments from CACountryPreference where CA_CountryCode = '{countryCode}'";
			AssertEquals("CA_ValidTariffTreatments", validTariffTreatments, refDbConn.ExecuteScalar(sqlText).ToString());
		}

		int GetRowCount(DbConnection refDbConn, string tableName)
		{
			return (int)refDbConn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", tableName));
		}

		void AssertOfficialCodeColumn(DbConnection refDbConn, string columnName, string checkConstraintName)
		{
			var sqlText = string.Format(@"
				SELECT COUNT(*)
				FROM
					sys.columns col
					INNER JOIN sys.types tp
					ON col.system_type_id = tp.system_type_id
					INNER JOIN sys.check_constraints ck
					ON col.object_id = ck.parent_object_id
				WHERE
					col.name = '{0}'
					AND tp.name = 'varchar'
					AND col.max_length = 3
					AND ck.name = '{1}'", columnName, checkConstraintName);

			AssertEquals(columnName, true, (int)refDbConn.ExecuteScalar(sqlText) == 1);
		}

		void AssertRegSubTypeColumn(DbConnection refDbConn, string tableName, string columnName)
		{
			var sqlText = $@"
				SELECT COUNT(*)
				FROM
					sys.objects obj 
					INNER JOIN sys.columns col 
					ON obj.object_id = col.object_id
				WHERE 
					obj.name = '{tableName}'
					AND col.name='{columnName}'";

			AssertEquals(columnName, true, (int)refDbConn.ExecuteScalar(sqlText) == 1);
		}

		protected override void SetUp()
		{
			upgradeContext = new Mock<IUpgradeContext>().Object;
			base.SetUp();
		}

		IUpgradeContext upgradeContext;
	}
}
