using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USImportInvoiceLinePGADOT))]
	class USImportInvoiceLinePGADOTTest : DbCreateScriptTest
	{
		public void TestGetCorrectData()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declarationPK1 = Guid.NewGuid();
			var declarationPK2 = Guid.NewGuid();
			var importerPK = TestDataCreator.CreateOrganisation("oh1", "OrgH1");
			var declarationSql = @"
				INSERT INTO dbo.JobDeclaration (JE_OH_Importer, JE_DataModel, JE_PK, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_ClusterKey) VALUES (@importerPK, 'US', @declarationPK1, 'IMP', @branchPK, @companyPK, 'JOB1', 'ACS', 0, 'PGAExpeditedRelease=Y', 1);
				INSERT INTO dbo.JobDeclaration (JE_OH_Importer, JE_DataModel, JE_PK, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey) VALUES (@importerPK, 'US', @declarationPK2, 'IMP', @branchPK, @companyPK, 'JOB2', 'ACS', 0, 2);";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.ExecuteNonQuery();
			}

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK1, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK2, 2, dataModel: "US");
			var dotID1 = Guid.NewGuid();
			var detailID1 = Guid.NewGuid();
			var detailID2 = Guid.NewGuid();

			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038823', 'AT1', 'SupDuty=200', 10, 110, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038805', 'AT2', 'SupDuty=150', 20, 120, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030121', 'AT3', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030122', 'AT4', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030123', 'AT5', '', 0, 0, 'US')";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.ExecuteNonQuery();
			}

			var cusAddInfoSql = @"
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(@dotID1, 'NTH', 'NHTProgramCode=123456789*NHTBoxNumber=22334455*IntendedUseCode=nameBlah*IntendedUseDesc=PC1*NHTDOTSuretyCode=ProC1', 'JI', @invoiceLinePK1, 1)
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(@detailID1, 'NTD', 'NHTBrandName=123*NHTCategoryCode=456', 'B7', @dotID1, 1)
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(@detailID2, 'NTD', 'NHTBrandName=123*NHTCategoryCode=456', 'B7', @dotID1, 1)
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'NTA', 'NHTAdditionalIdentityNumber=222*NHTAdditionalIdentityNumQualifier=AKG', 'B7', @detailID1, 1)
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'NTA', 'NHTAdditionalIdentityNumber=222*NHTAdditionalIdentityNumQualifier=AKG', 'B7', @detailID1, 1)";
			using (var command = Db.Connection.Command(cusAddInfoSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@dotID1", SqlDbType.UniqueIdentifier, dotID1);
				command.AddParameter("@detailID1", SqlDbType.UniqueIdentifier, detailID1);
				command.AddParameter("@detailID2", SqlDbType.UniqueIdentifier, detailID2);
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT * FROM USImportInvoiceLinePGADOT(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,NULL,NULL,NULL,NULL)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("99038823", reader["ProvProgAddtionalTariff1"].ToString());
						AssertEquals("99038805", reader["ProvProgAddtionalTariff2"].ToString());
						AssertEquals("99030121", reader["ProvProgAddtionalTariff3"].ToString());
						AssertEquals("99030122", reader["ProvProgAddtionalTariff4"].ToString());
						AssertEquals("99030123", reader["ProvProgAddtionalTariff5"].ToString());
						AssertEquals(200m, (decimal)reader["ProvProgAddtionalDuty1"]);
						AssertEquals(150m, (decimal)reader["ProvProgAddtionalDuty2"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty3"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty4"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty5"]);
						AssertEquals(10m, (decimal)reader["ProvProgAddtionalQty1"]);
						AssertEquals(20m, (decimal)reader["ProvProgAddtionalQty2"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty3"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty4"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty5"]);
						AssertEquals(110m, (decimal)reader["ProvProgAddtionalGoodsValue1"]);
						AssertEquals(120m, (decimal)reader["ProvProgAddtionalGoodsValue2"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue3"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue4"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue5"]);

						AssertEquals("ROWROLE", "HEADER", reader["ROWROLE"].ToString());
						AssertEquals("DOTProgramCode", "123456789", reader["DOTProgramCode"].ToString());
						AssertEquals("DOTBoxNumber", "22334455", reader["DOTBoxNumber"].ToString());
						AssertEquals("DOTIntendedUseCode", "nameBlah", reader["DOTIntendedUseCode"].ToString());
						AssertEquals("DOTIntendedUseDesc", "PC1", reader["DOTIntendedUseDesc"].ToString());
						AssertEquals("DOTSuretyCode", "ProC1", reader["DOTSuretyCode"].ToString());
						AssertEquals("DOTBrandName", "123", reader["DOTBrandName"].ToString());
						AssertEquals("DOTCategoryCode", "456", reader["DOTCategoryCode"].ToString());
					});

					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("99038823", reader["ProvProgAddtionalTariff1"].ToString());
						AssertEquals("99038805", reader["ProvProgAddtionalTariff2"].ToString());
						AssertEquals("99030121", reader["ProvProgAddtionalTariff3"].ToString());
						AssertEquals("99030122", reader["ProvProgAddtionalTariff4"].ToString());
						AssertEquals("99030123", reader["ProvProgAddtionalTariff5"].ToString());
						AssertEquals(200m, (decimal)reader["ProvProgAddtionalDuty1"]);
						AssertEquals(150m, (decimal)reader["ProvProgAddtionalDuty2"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty3"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty4"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty5"]);
						AssertEquals(10m, (decimal)reader["ProvProgAddtionalQty1"]);
						AssertEquals(20m, (decimal)reader["ProvProgAddtionalQty2"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty3"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty4"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty5"]);
						AssertEquals(110m, (decimal)reader["ProvProgAddtionalGoodsValue1"]);
						AssertEquals(120m, (decimal)reader["ProvProgAddtionalGoodsValue2"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue3"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue4"]);
						AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue5"]);

						AssertEquals("ROWROLE", "SUBLINE", reader["ROWROLE"].ToString());
						AssertEquals("DOTProgramCode", "123456789", reader["DOTProgramCode"].ToString());
						AssertEquals("DOTBoxNumber", "22334455", reader["DOTBoxNumber"].ToString());
						AssertEquals("DOTIntendedUseCode", "nameBlah", reader["DOTIntendedUseCode"].ToString());
						AssertEquals("DOTIntendedUseDesc", "PC1", reader["DOTIntendedUseDesc"].ToString());
						AssertEquals("DOTSuretyCode", "ProC1", reader["DOTSuretyCode"].ToString());
						AssertEquals("DOTBrandName", "123", reader["DOTBrandName"].ToString());
						AssertEquals("DOTCategoryCode", "456", reader["DOTCategoryCode"].ToString());
					});
				}
			}
		}
	}
}
