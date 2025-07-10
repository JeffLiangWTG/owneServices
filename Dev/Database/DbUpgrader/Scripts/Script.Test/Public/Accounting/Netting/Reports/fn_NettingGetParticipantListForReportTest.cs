using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_NettingGetParticipantListForReport))]
	class fn_NettingGetParticipantListForReportTest : DbCreateScriptTest
	{
		public void Testfn_NettingGetParticipantListForReport()
		{
			var company_pk = "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC";
			var ns_pk = "257913DA-C48F-45B5-B7BC-4FA69A1594EE";
			var ns_sql = $@"INSERT INTO dbo.NettingSystem([NS_PK], [NS_Code], [NS_Description], [NS_GC]) VALUES ('{ns_pk}', 'TESTNS', 'TEST NETTING SYSTEM', '{company_pk}')";
			TestConnection.ExecuteNonQuery(ns_sql);

			var oh_abigas = "0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1";
			var au1_company_sql = $"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES(newid(), 'AU1', 'AU test company1', 'AU', 'AUD', '{oh_abigas}')";
			var nso_abigas_pk = "54761E37-28B3-4B41-9AB3-BC887EC4B3FD";
			var nso_abigas_sql = $@"INSERT INTO dbo.NettingOrganisation([NSO_PK],[NSO_NS_NettingSystem],[NSO_NettingType],[NSO_OH_Organisation],[NSO_RX_NKReportingCurrency],[NSO_RX_NKARSettlementCurrency],[NSO_RX_NKAPSettlementCurrency])
									VALUES('{nso_abigas_pk}', '{ns_pk}', 'FUL', '{oh_abigas}', 'EUR', 'EUR', 'EUR')";

			var oh_aalshi = "E8BD88D2-A5C1-43FE-A788-A53ADBB86403";
			var au2_company_sql = $"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES(newid(), 'AU2', 'AU test company2', 'AU', 'AUD', '{oh_aalshi}')";
			var nso_aalshi_pk = "C65396AE-8126-4838-8C8B-BBB198AD32E9";
			var nso_aalshi_sql = $@"INSERT INTO dbo.NettingOrganisation([NSO_PK],[NSO_NS_NettingSystem],[NSO_NettingType],[NSO_OH_Organisation],[NSO_RX_NKReportingCurrency],[NSO_RX_NKARSettlementCurrency],[NSO_RX_NKAPSettlementCurrency])
									VALUES('{nso_aalshi_pk}', '{ns_pk}', 'CUR', '{oh_aalshi}', 'USD', 'USD', 'USD')";

			var oh_toyo = "07D36865-9637-4643-BB0E-DB9B40EC37FA";
			var jap_company_sql = $"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES(newid(), 'JAP', 'JP test company', 'JP', 'JPY', '{oh_toyo}')";
			var nso_toyo_pk = "4575B17E-8CDD-496D-A0E7-744DBF564BB0";
			var nso_toyo_sql = $@"INSERT INTO dbo.NettingOrganisation([NSO_PK],[NSO_NS_NettingSystem],[NSO_NettingType],[NSO_OH_Organisation],[NSO_RX_NKReportingCurrency],[NSO_RX_NKARSettlementCurrency],[NSO_RX_NKAPSettlementCurrency])
									VALUES('{nso_toyo_pk}', '{ns_pk}', 'HOM', '{oh_toyo}', 'JPY', 'JPY', 'JPY')";

			var oh_jasSingapore = "D01B62F9-E8E0-4A4E-9DE0-C32842ED355A";
			var sin_company_sql = $"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES(newid(), 'SN1', 'SG test company', 'SG', 'SGD', '{oh_jasSingapore}')";
			var nso_jasSingapore_pk = "3FCA47D1-7E01-4BEB-90F3-0FEEF293DEB0";
			var nso_jasSingapore_sql = $@"INSERT INTO dbo.NettingOrganisation([NSO_PK],[NSO_NS_NettingSystem],[NSO_NettingType],[NSO_OH_Organisation],[NSO_RX_NKReportingCurrency],[NSO_RX_NKARSettlementCurrency],[NSO_RX_NKAPSettlementCurrency])
									VALUES('{nso_jasSingapore_pk}', '{ns_pk}', 'GRS', '{oh_jasSingapore}', 'SGD', 'SGD', 'SGD')";

			var oh_PacficNetwork = "fb3511ed-abc9-468f-b39c-3f8fe4784cfb";
			var nso_PacficNetwork = "159653F7-A5CE-4F47-B01E-E7C98124E3B3";
			var nso_PacficNetwork_sql = $@"INSERT INTO dbo.NettingOrganisation([NSO_PK],[NSO_NS_NettingSystem],[NSO_NettingType],[NSO_OH_Organisation],[NSO_RX_NKReportingCurrency],[NSO_RX_NKARSettlementCurrency],[NSO_RX_NKAPSettlementCurrency])
									VALUES('{nso_PacficNetwork}', '{ns_pk}', 'GRS', '{oh_PacficNetwork}', 'NZD', 'NZD', 'NZD')";

			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'NettingSystemOrg', NULL, NULL, 'GID', NULL, '{oh_jasSingapore}')");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgHeader SET OH_IsActive = 0 WHERE OH_PK='{oh_toyo}'");

			TestConnection.ExecuteNonQuery(au1_company_sql);
			TestConnection.ExecuteNonQuery(au2_company_sql);
			TestConnection.ExecuteNonQuery(jap_company_sql);
			TestConnection.ExecuteNonQuery(sin_company_sql);

			TestConnection.ExecuteNonQuery(nso_abigas_sql);
			TestConnection.ExecuteNonQuery(nso_aalshi_sql);
			TestConnection.ExecuteNonQuery(nso_toyo_sql);
			TestConnection.ExecuteNonQuery(nso_jasSingapore_sql);
			TestConnection.ExecuteNonQuery(nso_PacficNetwork_sql);

			InsertIntoOrgCusCode("HID", "123456", new Guid(oh_abigas), "AU");
			InsertIntoOrgCusCode("HID", "234567", new Guid(oh_abigas), "NZ");
			InsertIntoOrgCusCode("HID", "ABCD1234", new Guid(oh_aalshi));
			InsertIntoOrgCusCode("HID", "XYZ123", new Guid(oh_jasSingapore));

			var testHelper = new TestDbHelperBase(TestConnection);

			var result = RunScript(new Guid(company_pk), "All", "All", string.Empty, string.Empty, string.Empty);
			var expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
AALSHI       A.A.L. SHIPPING AGENCIES P/L                                                                         1           ABCD1234                            CUR             USD                        USD                           USD                           AU2     AU test company2
ABIGAS       ABI GAS & TOOLS                                                                                      1           123456                              FUL             EUR                        EUR                           EUR                           AU1     AU test company1
PACAKL       PACIFIC NETWORK                                                                                      1           NULL                                GRS             NZD                        NZD                           NZD                           NULL    NULL
TOYUMP       TOYO UMPANKI CO LTD                                                                                  0           NULL                                HOM             JPY                        JPY                           JPY                           JAP     JP test company
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("All participants", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "Active", "All", string.Empty, string.Empty, string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
AALSHI       A.A.L. SHIPPING AGENCIES P/L                                                                         1           ABCD1234                            CUR             USD                        USD                           USD                           AU2     AU test company2
ABIGAS       ABI GAS & TOOLS                                                                                      1           123456                              FUL             EUR                        EUR                           EUR                           AU1     AU test company1
PACAKL       PACIFIC NETWORK                                                                                      1           NULL                                GRS             NZD                        NZD                           NZD                           NULL    NULL
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Active participants", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "Inactive", "All", string.Empty, string.Empty, string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
TOYUMP       TOYO UMPANKI CO LTD                                                                                  0           NULL                                HOM             JPY                        JPY                           JPY                           JAP     JP test company
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Inactive participants", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "All", "CUR", string.Empty, string.Empty, string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
AALSHI       A.A.L. SHIPPING AGENCIES P/L                                                                         1           ABCD1234                            CUR             USD                        USD                           USD                           AU2     AU test company2
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("CUR netting type", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "All", "GRS", string.Empty, string.Empty, string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
PACAKL       PACIFIC NETWORK                                                                                      1           NULL                                GRS             NZD                        NZD                           NZD                           NULL    NULL
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("GRS netting type", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "All", "FUL", string.Empty, string.Empty, string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
ABIGAS       ABI GAS & TOOLS                                                                                      1           123456                              FUL             EUR                        EUR                           EUR                           AU1     AU test company1
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("FUL netting type", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "All", "HOM", string.Empty, string.Empty, string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
TOYUMP       TOYO UMPANKI CO LTD                                                                                  0           NULL                                HOM             JPY                        JPY                           JPY                           JAP     JP test company
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("FUL netting type", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "All", "All", "USD", string.Empty, string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
AALSHI       A.A.L. SHIPPING AGENCIES P/L                                                                         1           ABCD1234                            CUR             USD                        USD                           USD                           AU2     AU test company2
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("FUL netting type", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "All", "All", string.Empty, "EUR", string.Empty);
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
ABIGAS       ABI GAS & TOOLS                                                                                      1           123456                              FUL             EUR                        EUR                           EUR                           AU1     AU test company1
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("FUL netting type", result, expectedResult, new List<string>() { });

			result = RunScript(new Guid(company_pk), "All", "All", string.Empty, string.Empty, "JPY");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_IsActive OK_CustomsRegNo                     NSO_NettingType NSO_RX_NKReportingCurrency NSO_RX_NKAPSettlementCurrency NSO_RX_NKARSettlementCurrency GC_Code GC_Name
------------ ---------------------------------------------------------------------------------------------------- ----------- ----------------------------------- --------------- -------------------------- ----------------------------- ----------------------------- ------- ----------------------------------------------------------------------------------------------------
TOYUMP       TOYO UMPANKI CO LTD                                                                                  0           NULL                                HOM             JPY                        JPY                           JPY                           JAP     JP test company
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("FUL netting type", result, expectedResult, new List<string>() { });
		}

		DataTable RunScript(Guid company_pk, string activeStatusValue, string nettingType, string reportingCurrency, string payableCurrency, string receivableCurrency)
		{
			var sql = $@"select * from fn_NettingGetParticipantListForReport('{company_pk}', '{activeStatusValue}', '{nettingType}', '{reportingCurrency}', '{payableCurrency}', '{receivableCurrency}') order by OH_Code, OH_FullName";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		void InsertIntoOrgCusCode(string codeType, string customsCode, Guid orgPK, string countryCode = "AU")
		{
			string query = @"INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (newid(), @OK_CustomsRegNo, @OK_CodeType, @OK_RN_NKCodeCountry, @OK_OH)";

			using (var command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@OK_CustomsRegNo", customsCode, OrgCusCodeSchema.OK_CustomsRegNo);
				command.AddParameterBasedOnDbColumn("@OK_CodeType", codeType, OrgCusCodeSchema.OK_CodeType);
				command.AddParameterBasedOnDbColumn("@OK_OH", orgPK, OrgCusCodeSchema.OK_OH);
				command.AddParameterBasedOnDbColumn("@OK_RN_NKCodeCountry", countryCode, OrgCusCodeSchema.OK_RN_NKCodeCountry);
				command.ExecuteNonQuery();
			}
		}
	}
}

