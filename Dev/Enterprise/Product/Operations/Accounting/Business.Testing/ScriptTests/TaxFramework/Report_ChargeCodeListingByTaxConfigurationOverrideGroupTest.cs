using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ChargeCodeListingByTaxConfigurationOverrideGroupTest : ScriptTest
	{
		public void TestChargeCodeListingByTaxConfigurationOverrideGroupDetails()
		{
			var companyABC = TestObjectCreator.CreateNewCompany("ABC");
			var companyXYZ = TestObjectCreator.CreateNewCompany("XYZ");

			var chargeCode1 = TestObjectCreator.CC1;
			var chargeCode2 = TestObjectCreator.CC2;
			var chargeCode3 = TestObjectCreator.CC3;

			chargeCode1.AC_LocalLanguageDescription = "Local Description 1 CC1";
			chargeCode2.AC_LocalLanguageDescription = "Local Description 2 CC2";

			chargeCode1.AC_ChargeGroup = "NJR";

			var taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(companyABC);
			var taxConfiguration2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(companyXYZ);

			taxConfiguration1.ETC_Code = "TAXCONFIG1";
			taxConfiguration2.ETC_Code = "TAXCONFIG2";
			taxConfiguration1.ETC_Description = "Test configuration A";
			taxConfiguration2.ETC_Description = "Test configuration B";

			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(companyABC, taxConfiguration1);
			var taxOverrideGroup2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(companyXYZ, taxConfiguration2);
			var taxOverrideGroup3 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(companyXYZ, null);
			taxOverrideGroup1.AX_Code = "TAXGRP1";
			taxOverrideGroup2.AX_Code = "TAXGRP2";
			taxOverrideGroup3.AX_Code = "TAXGRP3";
			taxOverrideGroup1.AX_Description = "TAXGRP1 Description";
			taxOverrideGroup2.AX_Description = "TAXGRP2 Description";
			taxOverrideGroup3.AX_Description = "TAXGRP3 Description";

			taxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_RateNumerator = 15;
			taxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_RateDenominator = 100;
			taxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_RateNumerator = 25;
			taxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_RateDenominator = 10;

			taxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "STESTA";
			taxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "STESTB";
			taxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCodeDescription = "Service Code Test Description AAAA";
			taxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCodeDescription = "Service Code Test Description BBBB";

			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxOverrideGroup1, chargeCode1);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxOverrideGroup2, chargeCode2);

			Factory.Save();

			var dataTable = RunScript(companyABC.PK, chargeCode1.AC_ChargeGroup);
			var expectedResult = @"
AC_Code    AC_Desc                                                                          AC_LocalLanguageDescription                                                      AC_ChargeGroup AX_Code    AX_Description                                                                                                                                                                                                                                             ETC_Code                       ETC_Description                                                                  AX_RateNumerator AX_RateDenominator AX_TaxAuthorityServiceCode AX_TaxAuthorityServiceCodeDescription                                            TaxRate
---------- -------------------------------------------------------------------------------- -------------------------------------------------------------------------------- -------------- ---------- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------------------ -------------------------------------------------------------------------------- ---------------- ------------------ -------------------------- -------------------------------------------------------------------------------- ---------------------------------------
ZZCC1      Charge Code 1                                                                    Local Description 1 CC1                                                          NJR            TAXGRP1    TAXGRP1 Description                                                                                                                                                                                                                                        TAXCONFIG1                     Test configuration A                                                             15               100                STESTA                     Service Code Test Description AAAA                                               0.15
";
			AssertTableAsTextFromSQLServerManagenentStudio("", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(companyXYZ.PK, chargeCode2.AC_ChargeGroup);
			expectedResult = @"
AC_Code    AC_Desc                                                                          AC_LocalLanguageDescription                                                      AC_ChargeGroup AX_Code    AX_Description                                                                                                                                                                                                                                             ETC_Code                       ETC_Description                                                                  AX_RateNumerator AX_RateDenominator AX_TaxAuthorityServiceCode AX_TaxAuthorityServiceCodeDescription                                            TaxRate
---------- -------------------------------------------------------------------------------- -------------------------------------------------------------------------------- -------------- ---------- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------------------ -------------------------------------------------------------------------------- ---------------- ------------------ -------------------------- -------------------------------------------------------------------------------- ---------------------------------------
ZZCC2      Charge Code 2                                                                    Local Description 2 CC2                                                          NGC            TAXGRP2    TAXGRP2 Description                                                                                                                                                                                                                                        TAXCONFIG2                     Test configuration B                                                             25               10                 STESTB                     Service Code Test Description BBBB                                               2.50
";
			AssertTableAsTextFromSQLServerManagenentStudio("", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(companyABC.PK, chargeCode1.AC_ChargeGroup, new[] { chargeCode1.PK });
			expectedResult = @"
AC_Code    AC_Desc                                                                          AC_LocalLanguageDescription                                                      AC_ChargeGroup AX_Code    AX_Description                                                                                                                                                                                                                                             ETC_Code                       ETC_Description                                                                  AX_RateNumerator AX_RateDenominator AX_TaxAuthorityServiceCode AX_TaxAuthorityServiceCodeDescription                                            TaxRate
---------- -------------------------------------------------------------------------------- -------------------------------------------------------------------------------- -------------- ---------- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------------------ -------------------------------------------------------------------------------- ---------------- ------------------ -------------------------- -------------------------------------------------------------------------------- ---------------------------------------
ZZCC1      Charge Code 1                                                                    Local Description 1 CC1                                                          NJR            TAXGRP1    TAXGRP1 Description                                                                                                                                                                                                                                        TAXCONFIG1                     Test configuration A                                                             15               100                STESTA                     Service Code Test Description AAAA                                               0.15
";
			AssertTableAsTextFromSQLServerManagenentStudio("", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(companyXYZ.PK, chargeCode2.AC_ChargeGroup, new[] { chargeCode2.PK });
			expectedResult = @"
AC_Code    AC_Desc                                                                          AC_LocalLanguageDescription                                                      AC_ChargeGroup AX_Code    AX_Description                                                                                                                                                                                                                                             ETC_Code                       ETC_Description                                                                  AX_RateNumerator AX_RateDenominator AX_TaxAuthorityServiceCode AX_TaxAuthorityServiceCodeDescription                                            TaxRate
---------- -------------------------------------------------------------------------------- -------------------------------------------------------------------------------- -------------- ---------- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------------------ -------------------------------------------------------------------------------- ---------------- ------------------ -------------------------- -------------------------------------------------------------------------------- ---------------------------------------
ZZCC2      Charge Code 2                                                                    Local Description 2 CC2                                                          NGC            TAXGRP2    TAXGRP2 Description                                                                                                                                                                                                                                        TAXCONFIG2                     Test configuration B                                                             25               10                 STESTB                     Service Code Test Description BBBB                                               2.50
";
			AssertTableAsTextFromSQLServerManagenentStudio("", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(companyXYZ.PK, chargeCode3.AC_ChargeGroup, new[] { chargeCode3.PK });
			expectedResult = @"
AC_Code    AC_Desc                                                                          AC_LocalLanguageDescription                                                      AC_ChargeGroup AX_Code    AX_Description                                                                                                                                                                                                                                             ETC_Code                       ETC_Description                                                                  AX_RateNumerator AX_RateDenominator AX_TaxAuthorityServiceCode AX_TaxAuthorityServiceCodeDescription                                            TaxRate
---------- -------------------------------------------------------------------------------- -------------------------------------------------------------------------------- -------------- ---------- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------------------ -------------------------------------------------------------------------------- ---------------- ------------------ -------------------------- -------------------------------------------------------------------------------- ---------------------------------------
";
			AssertTableAsTextFromSQLServerManagenentStudio("", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		DataTable RunScript(ZGuid companyPK, ZString chargeGroup, ZGuid[] chargeCodePKs = null)
		{
			var sqlQuery = string.Format(@"SELECT * FROM Report_ChargeCodeListingByTaxConfigurationOverrideGroup('{0}', '{1}', @ChargeCodePKs, @ChargeCodePKsIsEmpty)", companyPK, chargeGroup);
			var command = Db.Connection.Command(sqlQuery);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@ChargeCodePKs", chargeCodePKs);
			return DataUtils.GetDataTableFromCommand(command);
		}

		void AddTVP_uniqueidentifierAndIsEmptyParameters(DbCommand command, string paramName, ZGuid[] values)
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(Guid));

			if (values != null)
			{
				foreach (var value in values)
				{
					table.Rows.Add(value.ToGuid());
				}
			}

			command.AddTableValuedParameter(paramName, "dbo.TVP_uniqueidentifier", table);
			command.AddParameter(paramName + "IsEmpty", SqlDbType.Bit, table.Rows.Count == 0);
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
