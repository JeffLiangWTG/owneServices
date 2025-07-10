using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USImportInvoiceLineWithPGAData))]
	class USImportInvoiceLineWithPGADataTest : DbCreateScriptTest
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

			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038823', 'AT1', 'SupDuty=200', 10, 110, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038805', 'AT2', 'SupDuty=150', 20, 120, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030121', 'AT3', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030122', 'AT4', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030123', 'AT5', '', 0, 0, 'US');

				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK2, 'JI', '99038826', 'AT1', 'SupDuty=300', 30, 130, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK2, 'JI', '99038806', 'AT2', '', 0, 0, 'US')";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}

			var cusAddInfoSql = @"
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'FDA', 'ProgramCode=PC1*ProcessingCode=ProC1*ProductCode=BLAH*RefusedCountry=CN*Qty1=11*Qty2=22*Qty3=33*Qty4=44*Qty5=55*UQ1=A*UQ2=B*UQ3=C*UQ4=D*UQ5=E*UQ6=F', 'JI', @invoiceLinePK1, 1)
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'ATF', '', 'JI', @invoiceLinePK2, 1)";
			using (var command = Db.Connection.Command(cusAddInfoSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}
			var productPK1 = Guid.NewGuid();
			var productPK2 = Guid.NewGuid();
			var oA_SupplierAddress1 = Guid.NewGuid();
			var oA_SupplierAddress2 = Guid.NewGuid();
			var oA_ShipToPartyAddress1 = Guid.NewGuid();
			var oA_ShipToPartyAddress2 = Guid.NewGuid();
			var oH_IOR1 = Guid.NewGuid();
			var oH_IOR2 = Guid.NewGuid();
			var oA_ManufacturerAddress1 = Guid.NewGuid();
			var oA_ManufacturerAddress2 = Guid.NewGuid();
			var productSql = @"
				INSERT INTO dbo.OrgSupplierPart(OP_PK, OP_PartNum) VALUES (@productPK1, 'MaskBlahLah');
				INSERT INTO dbo.OrgSupplierPart(OP_PK, OP_PartNum) VALUES (@productPK2, 'PenBlahLah2');
				INSERT INTO dbo.OrgPartRelation(OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES(NEWID(), @productPK1, @importerPK, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT INTO dbo.OrgPartRelation(OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES(NEWID(), @productPK2, @importerPK, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES(@OH_IOR1, 'IOR1');
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Code) VALUES(@OA_SupplierAddress1, @OH_IOR1, 'Address 1', 'ADD1');
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Code) VALUES(@OA_SupplierAddress2, @OH_IOR1, 'Address 2', 'ADD2');
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Code) VALUES(@OA_ShipToPartyAddress1, @OH_IOR1, 'Address 3', 'ADD3');
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Code) VALUES(@OA_ShipToPartyAddress2, @OH_IOR1, 'Address 4', 'ADD4');
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Code) VALUES(@OA_ManufacturerAddress1, @OH_IOR1, 'Address 5', 'ADD5');
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Code) VALUES(@OA_ManufacturerAddress2, @OH_IOR1, 'Address 6', 'ADD6');
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_AddInfo = 'UC_NKCountryOfOrigin=US*NMFS370Ind=D*TTBInd=D*TSCAInd=D*FDAIndicator=D*ODSLineNumber=101010101*TSCALineNumber=010101010',
					JI_LineNo = '1',
					JI_PartNo = 'MaskBlahLah',
					JI_Tariff='000000',
					JI_PartAttrib1 = 'AAA',
					JI_PartAttrib2 = 'BBB',
					JI_PartAttrib3 = 'CCC',
					JI_OA_ShipToPartyAddress=@OA_ShipToPartyAddress1,
					JI_OA_ManufacturerAddress=@OA_ManufacturerAddress1,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = @invoiceLinePK1;
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_AddInfo = 'UC_NKCountryOfOrigin=CA*NMFS370Ind=C*TTBInd=C*TSCAInd=C*FDAIndicator=C*NMFS370DisclaimReason=A*TTBDisclaimReason=B*TSCADisclaimReason=C*FDADisclaimReason=A*ODSLineNumber=2020202*TSCALineNumber=0202020',
					JI_LineNo = '2',
					JI_PartNo = 'PenBlahLah2',
					JI_Tariff='111111',
					JI_PartAttrib1 = 'DDD',
					JI_PartAttrib2 = 'EEE',
					JI_PartAttrib3 = 'FFF',
					JI_OA_ShipToPartyAddress=@OA_ShipToPartyAddress2 ,
					JI_OA_ManufacturerAddress=@OA_ManufacturerAddress2,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = @invoiceLinePK2;
				UPDATE dbo.JobDeclaration
				SET
					JE_AddInfo = 'PGAExpeditedRelease=Y*OH_IOR='+@OH_IOR1,
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE
					JE_PK = @declarationPK1;
				UPDATE dbo.JobDeclaration
				SET
					JE_AddInfo = 'OH_IOR=' + @OH_IOR2,
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE
					JE_PK = @declarationPK2;
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_OH_Supplier = @OH_IOR1,
					JZ_OA_SupplierAddress = @OA_SupplierAddress1,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = @invoiceHeaderPK1;
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_OH_Supplier = @OH_IOR1,
					JZ_OA_SupplierAddress = @OA_SupplierAddress2,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = @invoiceHeaderPK2;";
			using (var command = Db.Connection.Command(productSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@productPK1", SqlDbType.UniqueIdentifier, productPK1);
				command.AddParameter("@productPK2", SqlDbType.UniqueIdentifier, productPK2);
				command.AddParameter("@OA_SupplierAddress1", SqlDbType.NVarChar, oA_SupplierAddress1.ToString());
				command.AddParameter("@OA_SupplierAddress2", SqlDbType.NVarChar, oA_SupplierAddress2.ToString());
				command.AddParameter("@OA_ShipToPartyAddress1", SqlDbType.UniqueIdentifier, oA_ShipToPartyAddress1);
				command.AddParameter("@OA_ShipToPartyAddress2", SqlDbType.UniqueIdentifier, oA_ShipToPartyAddress2);
				command.AddParameter("@OH_IOR1", SqlDbType.NVarChar, oH_IOR1.ToString());
				command.AddParameter("@OH_IOR2", SqlDbType.NVarChar, oH_IOR2.ToString());
				command.AddParameter("@OA_ManufacturerAddress1", SqlDbType.UniqueIdentifier, oA_ManufacturerAddress1);
				command.AddParameter("@OA_ManufacturerAddress2", SqlDbType.UniqueIdentifier, oA_ManufacturerAddress2);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceHeaderPK2", SqlDbType.UniqueIdentifier, invoiceHeaderPK2);
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT * FROM USImportInvoiceLineWithPGAData(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL) ORDER BY INVLnNo";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("INVLnNo", "1", reader["INVLnNo"].ToString());
						AssertEquals("Tariff", "000000", reader["Tariff"].ToString());
						AssertEquals("ProductCode", "MaskBlahLah", reader["ProductCode"].ToString());
						AssertEquals("PGAIndicators", "EPA(TSCA) - D,FDA(PGA) - D,NMFS(370) - D,TTB(PGA) - D", reader["PGAIndicators"].ToString());
						AssertEquals("ProductAttribute1", "AAA", reader["ProductAttribute1"].ToString());
						AssertEquals("ProductAttribute2", "BBB", reader["ProductAttribute2"].ToString());
						AssertEquals("ProductAttribute3", "CCC", reader["ProductAttribute3"].ToString());
						AssertEquals("ODSLineNumberAddInfoValue", "101010101", reader["ODSLineNumberAddInfoValue"].ToString());
						AssertEquals("TSCALineNumberAddInfoValue", "010101010", reader["TSCALineNumberAddInfoValue"].ToString());
						AssertEquals("InvoiceSupplierAddressPK", oA_SupplierAddress1.ToString(), reader["InvoiceSupplierAddressPK"].ToString());
						AssertEquals("InvoiceLineEffectiveShipToPartyAddress", oA_ShipToPartyAddress1.ToString(), reader["InvoiceLineEffectiveShipToPartyAddress"].ToString());
						AssertEquals("InvoiceLineEffectiveManufacturerAddress", oA_ManufacturerAddress1.ToString(), reader["InvoiceLineEffectiveManufacturerAddress"].ToString());
						AssertEquals("CountryOfOriginAddInfoValue", "US", reader["CountryOfOriginAddInfoValue"].ToString());
						AssertEquals("ProvProgAddtionalTariff1", "99038823", reader["ProvProgAddtionalTariff1"].ToString());
						AssertEquals("ProvProgAddtionalTariff2", "99038805", reader["ProvProgAddtionalTariff2"].ToString());
						AssertEquals("ProvProgAddtionalTariff3", "99030121", reader["ProvProgAddtionalTariff3"].ToString());
						AssertEquals("ProvProgAddtionalTariff4", "99030122", reader["ProvProgAddtionalTariff4"].ToString());
						AssertEquals("ProvProgAddtionalTariff5", "99030123", reader["ProvProgAddtionalTariff5"].ToString());
						AssertEquals("ProvProgAddtionalDuty1", 200m, (decimal)reader["ProvProgAddtionalDuty1"]);
						AssertEquals("ProvProgAddtionalDuty2", 150m, (decimal)reader["ProvProgAddtionalDuty2"]);
						AssertEquals("ProvProgAddtionalDuty3", 0m, (decimal)reader["ProvProgAddtionalDuty3"]);
						AssertEquals("ProvProgAddtionalDuty4", 0m, (decimal)reader["ProvProgAddtionalDuty4"]);
						AssertEquals("ProvProgAddtionalDuty5", 0m, (decimal)reader["ProvProgAddtionalDuty5"]);
						AssertEquals("ProvProgAddtionalQty1", 10m, (decimal)reader["ProvProgAddtionalQty1"]);
						AssertEquals("ProvProgAddtionalQty2", 20m, (decimal)reader["ProvProgAddtionalQty2"]);
						AssertEquals("ProvProgAddtionalQty3", 0m, (decimal)reader["ProvProgAddtionalQty3"]);
						AssertEquals("ProvProgAddtionalQty4", 0m, (decimal)reader["ProvProgAddtionalQty4"]);
						AssertEquals("ProvProgAddtionalQty5", 0m, (decimal)reader["ProvProgAddtionalQty5"]);
						AssertEquals("ProvProgAddtionalGoodsValue1", 110m, (decimal)reader["ProvProgAddtionalGoodsValue1"]);
						AssertEquals("ProvProgAddtionalGoodsValue2", 120m, (decimal)reader["ProvProgAddtionalGoodsValue2"]);
						AssertEquals("ProvProgAddtionalGoodsValue3", 0m, (decimal)reader["ProvProgAddtionalGoodsValue3"]);
						AssertEquals("ProvProgAddtionalGoodsValue4", 0m, (decimal)reader["ProvProgAddtionalGoodsValue4"]);
						AssertEquals("ProvProgAddtionalGoodsValue5", 0m, (decimal)reader["ProvProgAddtionalGoodsValue5"]);
					});

					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("INVLnNo", "2", reader["INVLnNo"].ToString());
						AssertEquals("Tariff", "111111", reader["Tariff"].ToString());
						AssertEquals("ProductCode", "PenBlahLah2", reader["ProductCode"].ToString());
						AssertEquals("PGAIndicators", "EPA(TSCA) - C/C,FDA(PGA) - C/A,NMFS(370) - C/A,TTB(PGA) - C/B", reader["PGAIndicators"].ToString());
						AssertEquals("ProductAttribute1", "DDD", reader["ProductAttribute1"].ToString());
						AssertEquals("ProductAttribute2", "EEE", reader["ProductAttribute2"].ToString());
						AssertEquals("ProductAttribute3", "FFF", reader["ProductAttribute3"].ToString());
						AssertEquals("ODSLineNumberAddInfoValue", "2020202", reader["ODSLineNumberAddInfoValue"].ToString());
						AssertEquals("TSCALineNumberAddInfoValue", "0202020", reader["TSCALineNumberAddInfoValue"].ToString());
						AssertEquals("InvoiceSupplierAddressPK", oA_SupplierAddress2.ToString(), reader["InvoiceSupplierAddressPK"].ToString());
						AssertEquals("InvoiceLineEffectiveShipToPartyAddress", oA_ShipToPartyAddress2.ToString(), reader["InvoiceLineEffectiveShipToPartyAddress"].ToString());
						AssertEquals("InvoiceLineEffectiveManufacturerAddress", oA_ManufacturerAddress2.ToString(), reader["InvoiceLineEffectiveManufacturerAddress"].ToString());
						AssertEquals("CountryOfOriginAddInfoValue", "CA", reader["CountryOfOriginAddInfoValue"].ToString());
						AssertEquals("ProvProgAddtionalTariff1", "99038826", reader["ProvProgAddtionalTariff1"].ToString());
						AssertEquals("ProvProgAddtionalTariff2", "99038806", reader["ProvProgAddtionalTariff2"].ToString());
						AssertEquals("ProvProgAddtionalTariff3", ZString.Empty, reader["ProvProgAddtionalTariff3"].ToString());
						AssertEquals("ProvProgAddtionalTariff4", ZString.Empty, reader["ProvProgAddtionalTariff4"].ToString());
						AssertEquals("ProvProgAddtionalTariff5", ZString.Empty, reader["ProvProgAddtionalTariff5"].ToString());
						AssertEquals("ProvProgAddtionalDuty1", 300m, (decimal)reader["ProvProgAddtionalDuty1"]);
						AssertEquals("ProvProgAddtionalDuty2", 0m, (decimal)reader["ProvProgAddtionalDuty2"]);
						AssertEquals("ProvProgAddtionalDuty3", 0m, (decimal)reader["ProvProgAddtionalDuty3"]);
						AssertEquals("ProvProgAddtionalDuty4", 0m, (decimal)reader["ProvProgAddtionalDuty4"]);
						AssertEquals("ProvProgAddtionalDuty5", 0m, (decimal)reader["ProvProgAddtionalDuty5"]);
						AssertEquals("ProvProgAddtionalQty1", 30m, (decimal)reader["ProvProgAddtionalQty1"]);
						AssertEquals("ProvProgAddtionalQty2", 00m, (decimal)reader["ProvProgAddtionalQty2"]);
						AssertEquals("ProvProgAddtionalQty3", 0m, (decimal)reader["ProvProgAddtionalQty3"]);
						AssertEquals("ProvProgAddtionalQty4", 0m, (decimal)reader["ProvProgAddtionalQty4"]);
						AssertEquals("ProvProgAddtionalQty5", 0m, (decimal)reader["ProvProgAddtionalQty5"]);
						AssertEquals("ProvProgAddtionalGoodsValue1", 130m, (decimal)reader["ProvProgAddtionalGoodsValue1"]);
						AssertEquals("ProvProgAddtionalGoodsValue2", 0m, (decimal)reader["ProvProgAddtionalGoodsValue2"]);
						AssertEquals("ProvProgAddtionalGoodsValue3", 0m, (decimal)reader["ProvProgAddtionalGoodsValue3"]);
						AssertEquals("ProvProgAddtionalGoodsValue4", 0m, (decimal)reader["ProvProgAddtionalGoodsValue4"]);
						AssertEquals("ProvProgAddtionalGoodsValue5", 0m, (decimal)reader["ProvProgAddtionalGoodsValue5"]);
					});
				}
			}
		}
	}
}
