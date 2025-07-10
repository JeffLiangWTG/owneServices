using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USImportInvoiceLinePGAFDA))]
	class USImportInvoiceLinePGAFDATest : DbCreateScriptTest
	{
		public void TestProdCountryValue()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declarationPK = Guid.NewGuid();
			var importerPK = TestDataCreator.CreateOrganisation("oh1", "OrgH1");
			var declarationSql = @"
				INSERT INTO dbo.JobDeclaration (JE_OH_Importer, JE_DataModel, JE_PK, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_ClusterKey) VALUES (@importerPK, 'US', @declarationPK, 'IMP', @branchPK, @companyPK, 'JOB1', 'ACS', 0, 'PGAExpeditedRelease=Y', 1);";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.ExecuteNonQuery();
			}

			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, addInfo: "FDAIndicator=D*UC_NKCountryOfOrigin=XB", dataModel: "US");
			var invoiceLinePK3 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, addInfo: "FDAIndicator=D*UC_NKCountryOfOrigin=US", dataModel: "US");

			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038823', 'AT1', 'SupDuty=200', 10, 110, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038805', 'AT2', 'SupDuty=150', 20, 120, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030121', 'AT3', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030122', 'AT4', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030123', 'AT5', '', 0, 0, 'US')

				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK2, 'JI', '99038826', 'AT1', 'SupDuty=300', 30, 130, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK2, 'JI', '99038806', 'AT2', '', 0, 0, 'US');";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}

			var cusAddInfoSql = @"
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'FDA', 'ProgramCode=PC1*ProcessingCode=ProC1*ProdCountry=DE', 'JI', @invoiceLinePK1, 1);
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'FDA', '', 'JI', @invoiceLinePK2, 1);
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'FDA', '', 'JI', @invoiceLinePK3, 1)";
			using (var command = Db.Connection.Command(cusAddInfoSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@invoiceLinePK3", SqlDbType.UniqueIdentifier, invoiceLinePK3);
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT * FROM USImportInvoiceLinePGAFDA(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
				,NULL, NUll, NULL, NUll, NUll, NUll)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string>>();
				var reportListForTariff = new List<Tuple<Guid, string, string, string, string, string>>();
				var reportListForDuty = new List<Tuple<Guid, decimal, decimal, decimal, decimal, decimal>>();
				var reportListForQty = new List<Tuple<Guid, decimal, decimal, decimal, decimal, decimal>>();
				var reportListForGoodsValue = new List<Tuple<Guid, decimal, decimal, decimal, decimal, decimal>>();

				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["InvoiceLinePK"];
						var effectiveCountry = (string)reader["FDAGrowthProductionCountry"];
						reportList.Add(new Tuple<Guid, string>(pk, effectiveCountry));

						if (pk == invoiceLinePK1 || pk == invoiceLinePK2)
						{
							var addtionalTariff1 = (string)reader["ProvProgAddtionalTariff1"];
							var addtionalTariff2 = (string)reader["ProvProgAddtionalTariff2"];
							var addtionalTariff3 = (string)reader["ProvProgAddtionalTariff3"];
							var addtionalTariff4 = (string)reader["ProvProgAddtionalTariff4"];
							var addtionalTariff5 = (string)reader["ProvProgAddtionalTariff5"];
							reportListForTariff.Add(new Tuple<Guid, string, string, string, string, string>(pk, addtionalTariff1, addtionalTariff2, addtionalTariff3, addtionalTariff4, addtionalTariff5));

							var addtionalDuty1 = (decimal)reader["ProvProgAddtionalDuty1"];
							var addtionalDuty2 = (decimal)reader["ProvProgAddtionalDuty2"];
							var addtionalDuty3 = (decimal)reader["ProvProgAddtionalDuty3"];
							var addtionalDuty4 = (decimal)reader["ProvProgAddtionalDuty4"];
							var addtionalDuty5 = (decimal)reader["ProvProgAddtionalDuty5"];
							reportListForDuty.Add(new Tuple<Guid, decimal, decimal, decimal, decimal, decimal>(pk, addtionalDuty1, addtionalDuty2, addtionalDuty3, addtionalDuty4, addtionalDuty5));

							var addtionalQty1 = (decimal)reader["ProvProgAddtionalQty1"];
							var addtionalQty2 = (decimal)reader["ProvProgAddtionalQty2"];
							var addtionalQty3 = (decimal)reader["ProvProgAddtionalQty3"];
							var addtionalQty4 = (decimal)reader["ProvProgAddtionalQty4"];
							var addtionalQty5 = (decimal)reader["ProvProgAddtionalQty5"];
							reportListForQty.Add(new Tuple<Guid, decimal, decimal, decimal, decimal, decimal>(pk, addtionalQty1, addtionalQty2, addtionalQty3, addtionalQty4, addtionalQty5));

							var addtionalGoodsValue1 = (decimal)reader["ProvProgAddtionalGoodsValue1"];
							var addtionalGoodsValue2 = (decimal)reader["ProvProgAddtionalGoodsValue2"];
							var addtionalGoodsValue3 = (decimal)reader["ProvProgAddtionalGoodsValue3"];
							var addtionalGoodsValue4 = (decimal)reader["ProvProgAddtionalGoodsValue4"];
							var addtionalGoodsValue5 = (decimal)reader["ProvProgAddtionalGoodsValue5"];
							reportListForGoodsValue.Add(new Tuple<Guid, decimal, decimal, decimal, decimal, decimal>(pk, addtionalGoodsValue1, addtionalGoodsValue2, addtionalGoodsValue3, addtionalGoodsValue4, addtionalGoodsValue5));
						}
					}
				}
				AssertEquals(3, reportList.Count);
				AssertEquals(2, reportListForTariff.Count);
				AssertEquals(2, reportListForDuty.Count);
				AssertEquals(2, reportListForQty.Count);
				AssertEquals(2, reportListForGoodsValue.Count);

				CombineAssertions(() =>
				{
					AssertEquals("DE", reportList.Find(x => x.Item1 == invoiceLinePK1).Item2);
					AssertEquals("CA", reportList.Find(x => x.Item1 == invoiceLinePK2).Item2);
					AssertEquals("US", reportList.Find(x => x.Item1 == invoiceLinePK3).Item2);

					var tariffResult1 = reportListForTariff.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("99038823", tariffResult1.Item2);
					AssertEquals("99038805", tariffResult1.Item3);
					AssertEquals("99030121", tariffResult1.Item4);
					AssertEquals("99030122", tariffResult1.Item5);
					AssertEquals("99030123", tariffResult1.Item6);

					var dutyResult1 = reportListForDuty.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(200m, dutyResult1.Item2);
					AssertEquals(150m, dutyResult1.Item3);
					AssertEquals(0m, dutyResult1.Item4);
					AssertEquals(0m, dutyResult1.Item5);
					AssertEquals(0m, dutyResult1.Item6);

					var qtyResult1 = reportListForQty.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(10m, qtyResult1.Item2);
					AssertEquals(20m, qtyResult1.Item3);
					AssertEquals(0m, qtyResult1.Item4);
					AssertEquals(0m, qtyResult1.Item5);
					AssertEquals(0m, qtyResult1.Item6);

					var goodsValueResult1 = reportListForGoodsValue.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(110m, goodsValueResult1.Item2);
					AssertEquals(120m, goodsValueResult1.Item3);
					AssertEquals(0m, goodsValueResult1.Item4);
					AssertEquals(0m, goodsValueResult1.Item5);
					AssertEquals(0m, goodsValueResult1.Item6);

					var tariffResult2 = reportListForTariff.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals("99038826", tariffResult2.Item2);
					AssertEquals("99038806", tariffResult2.Item3);
					AssertEquals(ZString.Empty, tariffResult2.Item4);
					AssertEquals(ZString.Empty, tariffResult2.Item5);
					AssertEquals(ZString.Empty, tariffResult2.Item6);

					var dutyResult2 = reportListForDuty.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals(300m, dutyResult2.Item2);
					AssertEquals(0m, dutyResult2.Item3);
					AssertEquals(0m, dutyResult2.Item4);
					AssertEquals(0m, dutyResult2.Item5);
					AssertEquals(0m, dutyResult2.Item6);

					var qtyResult2 = reportListForQty.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals(30m, qtyResult2.Item2);
					AssertEquals(0m, qtyResult2.Item3);
					AssertEquals(0m, qtyResult2.Item4);
					AssertEquals(0m, qtyResult2.Item5);
					AssertEquals(0m, qtyResult2.Item6);

					var goodsValueResult2 = reportListForGoodsValue.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals(130m, goodsValueResult2.Item2);
					AssertEquals(0m, goodsValueResult2.Item3);
					AssertEquals(0m, goodsValueResult2.Item4);
					AssertEquals(0m, goodsValueResult2.Item5);
					AssertEquals(0m, goodsValueResult2.Item6);
				});
			}
		}

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
				UPDATE dbo.JobComInvoiceLine SET
					JI_AddInfo = 'NMFS370Ind=D*TTBInd=D*TSCAInd=D*FDAIndicator=D*ODSLineNumber=101010101*TSCALineNumber=010101010',
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
				UPDATE dbo.JobComInvoiceLine SET
					JI_AddInfo = 'NMFS370Ind=C*TTBInd=C*TSCAInd=C*FDAIndicator=C*NMFS370DisclaimReason=A*TTBDisclaimReason=B*TSCADisclaimReason=C*FDADisclaimReason=A*ODSLineNumber=2020202*TSCALineNumber=0202020',
					JI_LineNo = '2',
					JI_PartNo = 'PenBlahLah2',
					JI_Tariff='111111',
					JI_PartAttrib1 = 'DDD',
					JI_PartAttrib2 = 'EEE',
					JI_PartAttrib3 = 'FFF',
					JI_OA_ShipToPartyAddress=@OA_ShipToPartyAddress2,
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

			var reportSql = @"SELECT * FROM USImportInvoiceLinePGAFDA(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
				,NULL, NUll, NULL, NUll, NUll, NUll)";
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
						AssertEquals("FDAAgencyProgram", "PC1", reader["FDAAgencyProgram"].ToString());
						AssertEquals("FDAProcessingCode", "ProC1", reader["FDAProcessingCode"].ToString());
						AssertEquals("FDAProductCode", "BLAH", reader["FDAProductCode"].ToString());
						AssertEquals("FDARefusedCountry", "CN", reader["FDARefusedCountry"].ToString());
						AssertEquals("FDAQty", "11A,22B,33C,44D,55E,0F", reader["FDAQty"].ToString());
					});
				}
			}
		}
	}
}
