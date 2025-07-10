using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetIntermUltimateConsigneeInfoInline))]
	class csfn_GetIntermUltimateConsigneeInfoInlineTest : DbCreateScriptTest
	{
		Guid DeclarationPK;
		Guid CompanyPK;
		Guid BranchPK;

		Guid InvHeader1PK;
		Guid OrgHeader1PK;
		Guid OrgAddress1PK;
		Guid OrgCusCode1PK;

		Guid InvHeader2PK;
		Guid OrgHeader2PK;
		Guid OrgAddress2PK;
		Guid OrgCusCode2PK;

		void PrepareTestData()
		{
			DeclarationPK = Guid.NewGuid();
			InvHeader1PK = Guid.NewGuid();
			OrgHeader1PK = Guid.NewGuid();
			OrgAddress1PK = Guid.NewGuid();
			OrgCusCode1PK = Guid.NewGuid();
			InvHeader2PK = Guid.NewGuid();
			OrgHeader2PK = Guid.NewGuid();
			OrgAddress2PK = Guid.NewGuid();
			OrgCusCode2PK = Guid.NewGuid();
			CompanyPK = Guid.NewGuid();
			BranchPK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES(@CompanyPK, 'US', 'USD', 'USC', 'US company')
				INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES(@BranchPK, @CompanyPK, 'USB', 'US')
				
				--Data to be Prepared
				INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_AddInfo, JE_RN_NKTransportNationality, JE_RL_NKFinalDestination, JE_RL_NKOrigin, JE_ClusterKey)
									values(@DeclarationPK, 'US', @BranchPK, @CompanyPK, '', 'US', 'CNHK', 'USLON', 1)

				INSERT INTO dbo.OrgHeader(OH_PK, OH_IsValid, OH_Code, OH_FullName)
								VALUES(@OrgHeader1PK, 1, 'TSOH1', 'Test OH 1')
				INSERT INTO dbo.OrgAddress(OA_OH, OA_PK, OA_IsValid, OA_Code, OA_Address1, OA_Address2, OA_City, OA_State, OA_PostCode)
								VALUES(@OrgHeader1PK, @OrgAddress1PK, 1, 'TSOA1', 'TEST OA 1 ADD1', 'TEST OA 1 ADD2', 'CT1', 'ST1', '1111')
				INSERT INTO dbo.OrgCusCode(OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_OH, OK_RN_NKCodeCountry)
								VALUES(@OrgCusCode1PK, 1, 'TSCRN1', 'EIN', @OrgHeader1PK, 'US')
				INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Phone)
								VALUES (NEWID(), @OrgHeader1PK, 'JAKE', '1234567890')

				INSERT INTO dbo.OrgHeader(OH_PK, OH_IsValid, OH_Code, OH_FullName)
								VALUES(@OrgHeader2PK, 1, 'TSOH2', 'Test OH 2')
				INSERT INTO dbo.OrgAddress(OA_OH, OA_PK, OA_IsValid, OA_Code, OA_Address1, OA_Address2, OA_City, OA_State, OA_PostCode)
								VALUES(@OrgHeader2PK, @OrgAddress2PK, 1, 'TSOA2', 'TEST OA 2 ADD1', 'TEST OA 2 ADD2', 'CT2', 'ST2', '2222')
				INSERT INTO dbo.OrgCusCode(OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_OH, OK_RN_NKCodeCountry)
								VALUES(@OrgCusCode2PK, 1, 'TSCRN2', 'EIN', @OrgHeader2PK, 'US')
				INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Phone)
								VALUES (NEWID(), @OrgHeader2PK, 'JAKE', '1234567890')
				INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Phone)
								VALUES (NEWID(), @OrgHeader2PK, 'Dong', '0987654321')
";
			RunSqlForTest(sql);
		}

		void RunSqlForTest(string sql)
		{
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@DeclarationPK", SqlDbType.UniqueIdentifier, DeclarationPK);
				command.AddParameter("@InvHeader1PK", SqlDbType.UniqueIdentifier, InvHeader1PK);
				command.AddParameter("@OrgHeader1PK", SqlDbType.UniqueIdentifier, OrgHeader1PK);
				command.AddParameter("@OrgAddress1PK", SqlDbType.UniqueIdentifier, OrgAddress1PK);
				command.AddParameter("@OrgCusCode1PK", SqlDbType.UniqueIdentifier, OrgCusCode1PK);
				command.AddParameter("@InvHeader2PK", SqlDbType.UniqueIdentifier, InvHeader2PK);
				command.AddParameter("@OrgHeader2PK", SqlDbType.UniqueIdentifier, OrgHeader2PK);
				command.AddParameter("@OrgAddress2PK", SqlDbType.UniqueIdentifier, OrgAddress2PK);
				command.AddParameter("@OrgCusCode2PK", SqlDbType.UniqueIdentifier, OrgCusCode2PK);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, CompanyPK);
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, BranchPK);
				command.ExecuteNonQuery();
			}
		}

		public void TestGetIntermUltimateConsigneeInfoInline()
		{
			PrepareTestData();
			AssertQueryResult(DeclarationPK, OrgHeader1PK, OrgHeader1PK, 0, new ExpectedValues());
			var oneHeaderOnly = $@"
				--Only Have One Header
				INSERT INTO dbo.JobComInvoiceHeader(JZ_JE, JZ_DataModel, JZ_PK, JZ_IsValid, JZ_AddInfo, JZ_OH_Consignee, JZ_ClusterKey)
								VALUES(@DeclarationPK, 'US', @InvHeader1PK, 1, 'UltimateConsigneeType=C', '{OrgHeader1PK.ToString()}', 1)";
			RunSqlForTest(oneHeaderOnly);
			TestDataCreator.CreateDocAddress(OrgAddress1PK, "", InvHeader1PK, "JZ", "UCE", "", "", "", "", "", "JAKE", "", "", "", false);
			TestDataCreator.CreateDocAddress(OrgAddress1PK, "", InvHeader1PK, "JZ", "ICE", "", "", "", "", "", "", "", "", "", false);
			AssertQueryResult(DeclarationPK, OrgHeader1PK, OrgHeader1PK, 1, new ExpectedValues()
			{
				ExpectedIntermConsigneeCode = "TSOH1",
				ExpectedIntermConsigneeFullName = "Test OH 1",
				ExpectedIntermConsigneeIDNumber = "EIN: TSCRN1",
				ExpectedUltimateConsigneeType = "C",
				ExpectedUltimateConsigneeAddress1 = "TEST OA 1 ADD1",
				ExpectedUltimateConsigneeAddress2 = "TEST OA 1 ADD2",
				ExpectedUltimateConsigneeCity = "CT1",
				ExpectedUltimateConsigneeState = "ST1",
				ExpectedUltimateConsigneePostCode = "1111",
				ExpectedUltimateConsigneeContact = "JAKE",
				ExpectedUltimateConsigneePhone = "1234567890",
				ExpectedUltimateConsigneeCode = "TSOH1",
				ExpectedUltimateConsigneeFullName = "Test OH 1",
				ExpectedUltimateConsigneeIDNumber = "EIN: TSCRN1",
			});

			var twoHeadersWithSameIntermUltimateTypeContactPhone = $@"
				Delete From dbo.JobComInvoiceHeader Where JZ_PK = @InvHeader2PK
				--Now Have Two Headers with Same Interm, Same Ultimate Also same UltimateConsigneeType BuyerContact BuyerPhone
				INSERT INTO dbo.JobComInvoiceHeader(JZ_JE, JZ_DataModel, JZ_PK, JZ_IsValid, JZ_AddInfo, JZ_OH_Consignee, JZ_ClusterKey)
								VALUES(@DeclarationPK, 'US', @InvHeader2PK, 1, 'UltimateConsigneeType=C', '{OrgHeader1PK.ToString()}', 1)";
			RunSqlForTest(twoHeadersWithSameIntermUltimateTypeContactPhone);
			TestDataCreator.CreateDocAddress(OrgAddress1PK, "", InvHeader2PK, "JZ", "UCE", "", "", "", "", "", "JAKE", "", "", "", false);
			TestDataCreator.CreateDocAddress(OrgAddress1PK, "", InvHeader2PK, "JZ", "ICE", "", "", "", "", "", "", "", "", "", false);
			AssertQueryResult(DeclarationPK, OrgHeader1PK, OrgHeader1PK, 1, new ExpectedValues()
			{
				ExpectedIntermConsigneeCode = "TSOH1",
				ExpectedIntermConsigneeFullName = "Test OH 1",
				ExpectedIntermConsigneeIDNumber = "EIN: TSCRN1",
				ExpectedUltimateConsigneeType = "C",
				ExpectedUltimateConsigneeAddress1 = "TEST OA 1 ADD1",
				ExpectedUltimateConsigneeAddress2 = "TEST OA 1 ADD2",
				ExpectedUltimateConsigneeCity = "CT1",
				ExpectedUltimateConsigneeState = "ST1",
				ExpectedUltimateConsigneePostCode = "1111",
				ExpectedUltimateConsigneeContact = "JAKE",
				ExpectedUltimateConsigneePhone = "1234567890",
				ExpectedUltimateConsigneeCode = "TSOH1",
				ExpectedUltimateConsigneeFullName = "Test OH 1",
				ExpectedUltimateConsigneeIDNumber = "EIN: TSCRN1",
			});

			var twoHeadersWithDifferentIntermUltimateSameTypeContactPhone = $@"
				Delete From dbo.JobComInvoiceHeader Where JZ_PK = @InvHeader2PK
				Delete From dbo.JobDocAddress Where E2_ParentID = @InvHeader2PK
				--Now Have Two Headers with Different Interm Also Different Ultimate but same UltimateConsigneeType BuyerContact BuyerPhone
				INSERT INTO dbo.JobComInvoiceHeader(JZ_JE, JZ_DataModel, JZ_PK, JZ_IsValid, JZ_AddInfo, JZ_OH_Consignee, JZ_ClusterKey)
								VALUES(@DeclarationPK, 'US', @InvHeader2PK, 1, 'UltimateConsigneeType=C', '{OrgHeader2PK.ToString()}', 1)";
			RunSqlForTest(twoHeadersWithDifferentIntermUltimateSameTypeContactPhone);
			TestDataCreator.CreateDocAddress(OrgAddress2PK, "", InvHeader2PK, "JZ", "UCE", "", "", "", "", "", "JAKE", "", "", "", false);
			TestDataCreator.CreateDocAddress(OrgAddress2PK, "", InvHeader2PK, "JZ", "ICE", "", "", "", "", "", "", "", "", "", false);
			AssertQueryResult(DeclarationPK, OrgHeader1PK, OrgHeader1PK, 1, new ExpectedValues()
			{
				ExpectedIntermConsigneeCode = "MULTI",
				ExpectedIntermConsigneeFullName = "MULTI",
				ExpectedIntermConsigneeIDNumber = "MULTI",
				ExpectedUltimateConsigneeType = "C",
				ExpectedUltimateConsigneeAddress1 = "MULTI",
				ExpectedUltimateConsigneeAddress2 = "MULTI",
				ExpectedUltimateConsigneeCity = "MULTI",
				ExpectedUltimateConsigneeState = "MULTI",
				ExpectedUltimateConsigneePostCode = "MULTI",
				ExpectedUltimateConsigneeContact = "JAKE",
				ExpectedUltimateConsigneePhone = "1234567890",
				ExpectedUltimateConsigneeCode = "MULTI",
				ExpectedUltimateConsigneeFullName = "MULTI",
				ExpectedUltimateConsigneeIDNumber = "MULTI",
			});
			var twoHeadersWithDifferentIntermUltimateTypeContactPhone = $@"
				Delete From dbo.JobComInvoiceHeader Where JZ_PK = @InvHeader2PK
				Delete From dbo.JobDocAddress Where E2_ParentID = @InvHeader2PK
				--Now Have Two Headers with Different Interm Also Different Ultimate ALSO Different UltimateConsigneeType BuyerContact BuyerPhone
				INSERT INTO dbo.JobComInvoiceHeader(JZ_JE, JZ_DataModel, JZ_PK, JZ_IsValid,JZ_AddInfo, JZ_OH_Consignee, JZ_ClusterKey)
								VALUES(@DeclarationPK, 'US', @InvHeader2PK, 1,'UltimateConsigneeType=D', '{OrgHeader2PK.ToString()}', 1)";
			RunSqlForTest(twoHeadersWithDifferentIntermUltimateTypeContactPhone);
			TestDataCreator.CreateDocAddress(OrgAddress2PK, "", InvHeader2PK, "JZ", "UCE", "", "", "", "", "", "Dong", "", "", "", false);
			TestDataCreator.CreateDocAddress(OrgAddress2PK, "", InvHeader2PK, "JZ", "ICE", "", "", "", "", "", "", "", "", "", false);
			AssertQueryResult(DeclarationPK, OrgHeader1PK, OrgHeader1PK, 1, new ExpectedValues()
			{
				ExpectedIntermConsigneeCode = "MULTI",
				ExpectedIntermConsigneeFullName = "MULTI",
				ExpectedIntermConsigneeIDNumber = "MULTI",
				ExpectedUltimateConsigneeType = "MULTI",
				ExpectedUltimateConsigneeAddress1 = "MULTI",
				ExpectedUltimateConsigneeAddress2 = "MULTI",
				ExpectedUltimateConsigneeCity = "MULTI",
				ExpectedUltimateConsigneeState = "MULTI",
				ExpectedUltimateConsigneePostCode = "MULTI",
				ExpectedUltimateConsigneeContact = "MULTI",
				ExpectedUltimateConsigneePhone = "MULTI",
				ExpectedUltimateConsigneeCode = "MULTI",
				ExpectedUltimateConsigneeFullName = "MULTI",
				ExpectedUltimateConsigneeIDNumber = "MULTI",
			});
			var headerInExportDeclaration = $@"
				Delete From dbo.JobComInvoiceHeader WHERE JZ_PK = @InvHeader1PK
				Delete From dbo.JobDocAddress Where E2_ParentID = @InvHeader1PK
				Delete From dbo.JobComInvoiceHeader WHERE JZ_PK = @InvHeader2PK
				Delete From dbo.JobDocAddress Where E2_ParentID = @InvHeader2PK
				--Only Have One Header
				INSERT INTO dbo.JobComInvoiceHeader(JZ_JE, JZ_DataModel, JZ_PK, JZ_IsValid, JZ_AddInfo, JZ_OH_Consignee, JZ_ClusterKey)
								VALUES(@DeclarationPK, 'US', @InvHeader1PK, 1, 'UltimateConsigneeType=C', '{OrgHeader1PK.ToString()}', 1)";
			RunSqlForTest(headerInExportDeclaration);
			TestDataCreator.CreateDocAddress(Guid.Empty, "", InvHeader1PK, "JZ", "UCE", "Doc Road1", "Doc Street1", "Doc City1", "Doc State1", "Doc Post1", "Doc Cont1", "Doc Pho1", "EIN", "11-123456789", true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "", InvHeader1PK, "JZ", "ICE", "", "", "", "", "", "", "", "DUN", "123456789", true);
			AssertQueryResult(DeclarationPK, OrgHeader1PK, OrgHeader1PK, 1, new ExpectedValues()
			{
				ExpectedIntermConsigneeCode = "TSOH1",
				ExpectedIntermConsigneeFullName = "Test OH 1",
				ExpectedIntermConsigneeIDNumber = "DUN: 123456789",
				ExpectedUltimateConsigneeType = "C",
				ExpectedUltimateConsigneeAddress1 = "Doc Road1",
				ExpectedUltimateConsigneeAddress2 = "Doc Street1",
				ExpectedUltimateConsigneeCity = "Doc City1",
				ExpectedUltimateConsigneeState = "Doc State1",
				ExpectedUltimateConsigneePostCode = "Doc Post1",
				ExpectedUltimateConsigneeContact = "Doc Cont1",
				ExpectedUltimateConsigneePhone = "Doc Pho1",
				ExpectedUltimateConsigneeCode = "",
				ExpectedUltimateConsigneeFullName = "",
				ExpectedUltimateConsigneeIDNumber = "EIN: 11-123456789",
			});
		}

		void AssertQueryResult(Guid decPK, Guid intermConsigneePK, Guid ultimateConsigneePK, int expectedLines, ExpectedValues expectedValues)
		{
			var sql = string.Format(@"SELECT * FROM
							csfn_GetIntermUltimateConsigneeInfoInline('{0}', '{1}', '{2}')", decPK.ToString(), intermConsigneePK.ToString(), ultimateConsigneePK.ToString());

			using (var command = Db.Connection.Command(sql))
			{
				var loadedData = new System.Collections.Generic.List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						expectedLines--;
						CombineAssertions(() =>
						{
							AssertEquals("IntermConsigneeCode", expectedValues.ExpectedIntermConsigneeCode, reader["IntermConsigneeCode"].ToString());
							AssertEquals("IntermConsigneeFullName", expectedValues.ExpectedIntermConsigneeFullName, reader["IntermConsigneeFullName"].ToString());
							AssertEquals("IntermConsigneeIDNumber", expectedValues.ExpectedIntermConsigneeIDNumber, reader["IntermConsigneeIDNumber"].ToString());
							AssertEquals("US_UltimateConsigneeType", expectedValues.ExpectedUltimateConsigneeType, reader["US_UltimateConsigneeType"].ToString());
							AssertEquals("UltimateConsigneeAddress1", expectedValues.ExpectedUltimateConsigneeAddress1, reader["UltimateConsigneeAddress1"].ToString());
							AssertEquals("UltimateConsigneeAddress2", expectedValues.ExpectedUltimateConsigneeAddress2, reader["UltimateConsigneeAddress2"].ToString());
							AssertEquals("UltimateConsigneeCity", expectedValues.ExpectedUltimateConsigneeCity, reader["UltimateConsigneeCity"].ToString());
							AssertEquals("UltimateConsigneeState", expectedValues.ExpectedUltimateConsigneeState, reader["UltimateConsigneeState"].ToString());
							AssertEquals("UltimateConsigneePostCode", expectedValues.ExpectedUltimateConsigneePostCode, reader["UltimateConsigneePostCode"].ToString());
							AssertEquals("UltimateConsigneeContact", expectedValues.ExpectedUltimateConsigneeContact, reader["UltimateConsigneeContact"].ToString());
							AssertEquals("UltimateConsigneePhone", expectedValues.ExpectedUltimateConsigneePhone, reader["UltimateConsigneePhone"].ToString());
							AssertEquals("UltimateConsigneeCode", expectedValues.ExpectedUltimateConsigneeCode, reader["UltimateConsigneeCode"].ToString());
							AssertEquals("UltimateConsigneeFullName", expectedValues.ExpectedUltimateConsigneeFullName, reader["UltimateConsigneeFullName"].ToString());
							AssertEquals("UltimateConsigneeIDNumber", expectedValues.ExpectedUltimateConsigneeIDNumber, reader["UltimateConsigneeIDNumber"].ToString());
						});
					}
				}
			}
			Assert("Result line number should be expected:", expectedLines <= 0);
		}

		class ExpectedValues
		{
			public string ExpectedIntermConsigneeCode = string.Empty;
			public string ExpectedIntermConsigneeFullName = string.Empty;
			public string ExpectedIntermConsigneeIDNumber = string.Empty;
			public string ExpectedUltimateConsigneeType = string.Empty;
			public string ExpectedUltimateConsigneeAddress1 = string.Empty;
			public string ExpectedUltimateConsigneeAddress2 = string.Empty;
			public string ExpectedUltimateConsigneeCity = string.Empty;
			public string ExpectedUltimateConsigneeState = string.Empty;
			public string ExpectedUltimateConsigneePostCode = string.Empty;
			public string ExpectedUltimateConsigneeContact = string.Empty;
			public string ExpectedUltimateConsigneePhone = string.Empty;
			public string ExpectedUltimateConsigneeCode = string.Empty;
			public string ExpectedUltimateConsigneeFullName = string.Empty;
			public string ExpectedUltimateConsigneeIDNumber = string.Empty;
		}
	}
}
