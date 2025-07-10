using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USEntrySummary))]
	class USEntrySummaryTest : DbCreateScriptTest
	{
		public void TestAdditionalTariff()
		{
			var clusterKey = 1;
			var entryLinePK = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, "DutyRateDesc=1.4%*HasMPF=Y", 34.64m, 140m, clusterKey);
			var cusEntryLinePK1 = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, "DutyRateDesc=15%*SupAdditionalLine=Y", 34.64m, 200m, clusterKey);
			var cusEntryLinePK2 = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, "DutyRateDesc=25%*SupAdditionalLine2=Y", 34.64m, 150m, clusterKey);
			var cusEntryLinePK3 = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, "DutyRateDesc=35%*SupAdditionalLine3=Y", 34.64m, 0m, clusterKey);
			var cusEntryLinePK4 = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, "DutyRateDesc=45%", 34.64m, 0m, clusterKey);
			var cusEntryLinePK5 = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, "DutyRateDesc=55%*SupAdditionalLine5=Y", 34.64m, 0m, clusterKey);

			var invoiceLinePK = Guid.NewGuid();
			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_CL, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'SupTariff=99038815', '', @entryLinePK, 1);
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038823', 'AT1', 'SupDuty=200', 10, 110, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038805', 'AT2', 'SupDuty=150', 20, 120, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030121', 'AT3', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030122', 'AT4', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030123', 'AT5', '', 0, 0, 'US');
";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.ExecuteNonQuery();
			}

			var cusUnderBondDecSql = @"
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK1, @invoiceLinePK, @clPK1, @clusterKey);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK2, @invoiceLinePK, @clPK2, @clusterKey);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK3, @invoiceLinePK, @clPK3, @clusterKey)
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK4, @invoiceLinePK, @clPK4, @clusterKey);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK5, @invoiceLinePK, @clPK5, @clusterKey)";
			using (var command = Db.Connection.Command(cusUnderBondDecSql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@clPK1", SqlDbType.UniqueIdentifier, cusEntryLinePK1);
				command.AddParameter("@clPK2", SqlDbType.UniqueIdentifier, cusEntryLinePK2);
				command.AddParameter("@clPK3", SqlDbType.UniqueIdentifier, cusEntryLinePK3);
				command.AddParameter("@clPK4", SqlDbType.UniqueIdentifier, cusEntryLinePK4);
				command.AddParameter("@clPK5", SqlDbType.UniqueIdentifier, cusEntryLinePK5);
				command.AddParameter("@bondDecPK1", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK2", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK3", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK4", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK5", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			var sql = @"SELECT * FROM dbo.USEntrySummary(@companyPK, null, null, null, null, null, null, null)";

			using (var command = Db.Connection.Command(sql))
			{
				var reportListForTariff = new List<Tuple<string, string, string, string, string>>();
				var reportListForDuty = new List<Tuple<decimal, decimal, decimal, decimal, decimal, decimal>>();
				var reportListForQty = new List<Tuple<decimal, decimal, decimal, decimal, decimal>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var addtionalTariff1 = (string)reader["ProvProgAddtionalTariff1"];
						var addtionalTariff2 = (string)reader["ProvProgAddtionalTariff2"];
						var addtionalTariff3 = (string)reader["ProvProgAddtionalTariff3"];
						var addtionalTariff4 = (string)reader["ProvProgAddtionalTariff4"];
						var addtionalTariff5 = (string)reader["ProvProgAddtionalTariff5"];
						reportListForTariff.Add(new Tuple<string, string, string, string, string>(addtionalTariff1, addtionalTariff2, addtionalTariff3, addtionalTariff4, addtionalTariff5));

						var addtionalDuty1 = (decimal)reader["ProvProgAddtionalDuty1"];
						var addtionalDuty2 = (decimal)reader["ProvProgAddtionalDuty2"];
						var addtionalDuty3 = (decimal)reader["ProvProgAddtionalDuty3"];
						var addtionalDuty4 = (decimal)reader["ProvProgAddtionalDuty4"];
						var addtionalDuty5 = (decimal)reader["ProvProgAddtionalDuty5"];
						var totalDuty = (decimal)reader["TotalDuty"];
						reportListForDuty.Add(new Tuple<decimal, decimal, decimal, decimal, decimal, decimal>(addtionalDuty1, addtionalDuty2, addtionalDuty3, addtionalDuty4, addtionalDuty5, totalDuty));

						var addtionalQty1 = (decimal)reader["ProvProgAddtionalQty1"];
						var addtionalQty2 = (decimal)reader["ProvProgAddtionalQty2"];
						var addtionalQty3 = (decimal)reader["ProvProgAddtionalQty3"];
						var addtionalQty4 = (decimal)reader["ProvProgAddtionalQty4"];
						var addtionalQty5 = (decimal)reader["ProvProgAddtionalQty5"];
						reportListForQty.Add(new Tuple<decimal, decimal, decimal, decimal, decimal>(addtionalQty1, addtionalQty2, addtionalQty3, addtionalQty4, addtionalQty5));
					}
				}

				AssertEquals(1, reportListForTariff.Count);
				AssertEquals(1, reportListForDuty.Count);
				AssertEquals(1, reportListForQty.Count);

				CombineAssertions(() =>
				{
					var tariffResult1 = reportListForTariff[0];
					AssertEquals("99038823", tariffResult1.Item1);
					AssertEquals("99038805", tariffResult1.Item2);
					AssertEquals("99030121", tariffResult1.Item3);
					AssertEquals("99030122", tariffResult1.Item4);
					AssertEquals("99030123", tariffResult1.Item5);

					var dutyResult1 = reportListForDuty[0];
					AssertEquals(200m, dutyResult1.Item1);
					AssertEquals(150m, dutyResult1.Item2);
					AssertEquals(0m, dutyResult1.Item3);
					AssertEquals(0m, dutyResult1.Item4);
					AssertEquals(0m, dutyResult1.Item5);
					AssertEquals(490m, dutyResult1.Item6);

					var qtyResult1 = reportListForQty[0];
					AssertEquals(10m, qtyResult1.Item1);
					AssertEquals(20m, qtyResult1.Item2);
					AssertEquals(0m, qtyResult1.Item3);
					AssertEquals(0m, qtyResult1.Item4);
					AssertEquals(0m, qtyResult1.Item5);
				});
			}
		}

		public void TestOneInvoiceLineWithOneEntryLine()
		{
			var clusterKey = 1;
			var entryLinePK = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, "DutyRateDesc=1.4%*HasMPF=Y", 34.64m, 140m, clusterKey);
			var invoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, 1, "CustomsValue=10000*Duty=140*FTADuty=140*FTAPayableMPF=34.64*NonFTADuty=140*NonFTAPayableMPF=34.64*PayableMPF=34.64*SPI=N/A", 100m, "KG", 10000m, "TESTPRD", Guid.Empty, Guid.Empty, entryLinePK, 12.5m, "", 0m, clusterKey);

			var sql = @"SELECT * from dbo.USEntrySummary(@companyPK, null, null, null, null, null, null, null) ORDER BY JE_DeclarationReference, EntryLineNo, InvoiceLineNo";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					AssertResults(reader, "BT0000001", "SV973000001", 1, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "HK", "HONG KONG", "N/A", "7202195000", "", 100m, "KG", 10000m, 0m, 140m, 34.64m, 12.5m, "", 0m, "", 0m, 10000m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "", "N", 140m, 0m, "TESTPRD", "TESTSUP", "Supplier Company Name", "MB2019100901");
				}
			}
		}

		public void TestTwoInvoiceLinesMergedIntoOneEntryLine()
		{
			var clusterKey = 1;
			var entryLinePK = CreateCusEntryLine(entrySummaryPK, "7202111000", 1, 10000m, "DutyRateDesc=1.4%*HasMPF=Y", 34.64m, 140m, clusterKey);
			var invoiceLine1PK = CreateJobComInvoiceLine(invoiceHeaderPK, 1, "CustomsValue=1000*Duty=14*FTADuty=14*FTAPayableMPF=3.46*NonFTADuty=14*NonFTAPayableMPF=3.46*PayableMPF=3.46*SPI=N/A", 100m, "KG", 1000m, "TESTPRD", Guid.Empty, Guid.Empty, entryLinePK, 1.25m, "107", 10m, clusterKey);
			CreateInvoiceLineCharge(invoiceLine1PK, "056", 20m);
			var invoiceLine2PK = CreateJobComInvoiceLine(invoiceHeaderPK, 2, "CustomsValue=9000*Duty=126*FTADuty=126*FTAPayableMPF=31.18*NonFTADuty=126*NonFTAPayableMPF=31.18*PayableMPF=31.18*SPI=N/A", 900m, "KG", 9000m, "TESTPRD", Guid.Empty, Guid.Empty, entryLinePK, 11.25m, "107", 20m, clusterKey);
			CreateInvoiceLineCharge(invoiceLine2PK, "056", 45m);

			var sql = @"SELECT * from dbo.USEntrySummary(@companyPK, null, null, null, null, null, null, null) ORDER BY JE_DeclarationReference, EntryLineNo, InvoiceLineNo";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					AssertResults(reader, "BT0000001", "SV973000001", 1, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "HK", "HONG KONG", "N/A", "7202111000", "", 1000m, "KG", 10000m, 0m, 140m, 34.64m, 12.5m, "056", 65m, "", 0m, 10000m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "", "N", 140m, 0m, "TESTPRD", "TESTSUP", "Supplier Company Name", "MB2019100901");
				}
			}
		}

		public void TestXVVLines()
		{
			var clusterKey = 1;
			var supXEntryLinePK = CreateCusEntryLine(entrySummaryPK, "9802008068", 1, 75000m, "SupLine=Y", 0m, 0m, clusterKey);
			var normalXEntryLinePK = CreateCusEntryLine(entrySummaryPK, "6211111010", 1, 8758m, $"ChildLineNum=1*CL_ParentLine={supXEntryLinePK}", 0m, 2434.72m, clusterKey);
			var xInvoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, 1, "CustomsValue=8758*Duty=2434.72*IsParent=Y*SetInd=X*SupTariff=9802008068*UC_NKCountryOfExport=MG*UC_NKCountryOfOrigin=MG", 100m, "DOZ", 0m, "", Guid.Empty, Guid.Empty, normalXEntryLinePK, 104.7m, "", 0m, clusterKey);
			CreateCusUnderbondDec(supXEntryLinePK, xInvoiceLinePK, clusterKey);

			var supV1EntryLinePK = CreateCusEntryLine(entrySummaryPK, "9802008068", 2, 50000m, $"ChildLineNum=1*CL_ParentLine={supXEntryLinePK}*SupLine=Y", 0m, 0m, clusterKey);
			var normalV1EntryLinePK = CreateCusEntryLine(entrySummaryPK, "6211111010", 2, 7410m, $"ChildLineNum=1*CL_ParentLine={supV1EntryLinePK}", 0m, 0m, clusterKey);
			var v1InvoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, 2, "98GoodsValue=50000*CustomsValue=7410*SetInd=V*SupTariff=9802008068*UC_NKCountryOfExport=CR*UC_NKCountryOfOrigin=MG", 0m, "DOZ", 7409.52m, "", Guid.Empty, xInvoiceLinePK, normalV1EntryLinePK, 0m, "", 0m, clusterKey);
			CreateCusUnderbondDec(supV1EntryLinePK, v1InvoiceLinePK, clusterKey);

			var supV2EntryLinePK = CreateCusEntryLine(entrySummaryPK, "9802008068", 3, 25000m, $"ChildLineNum=2*CL_ParentLine={supXEntryLinePK}*SupLine=Y", 0m, 0m, clusterKey);
			var normalV2EntryLinePK = CreateCusEntryLine(entrySummaryPK, "6505000100", 3, 1348m, $"ChildLineNum=1*CL_ParentLine={supV2EntryLinePK}*CWOs=27D", 0m, 0m, clusterKey);
			var v2InvoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, 3, "98GoodsValue=25000*CustomsValue=1348*SetInd=V*SupTariff=9802008068*UC_NKCountryOfExport=CR*UC_NKCountryOfOrigin=CR", 0m, "KG", 1348.08m, "", Guid.Empty, xInvoiceLinePK, normalV2EntryLinePK, 0m, "", 0m, clusterKey);
			CreateCusUnderbondDec(supV2EntryLinePK, v2InvoiceLinePK, clusterKey);

			var sql = @"SELECT * from dbo.USEntrySummary(@companyPK, null, null, null, null, null, null, null) ORDER BY JE_DeclarationReference, EntryLineNo, InvoiceLineNo";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					AssertResults(reader, "BT0000001", "SV973000001", 1, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "MG", "MADAGASCAR (MALAGASY)", "", "6211111010", "9802008068", 100m, "DOZ", 8758m, 0m, 2434.72m, 0m, 104.7m, "", 0m, "", 0m, 0m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "X", "N", 2434.72m, 0m, "", "TESTSUP", "Supplier Company Name", "MB2019100901");
					AssertResults(reader, "BT0000001", "SV973000001", 2, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "MG", "MADAGASCAR (MALAGASY)", "", "6211111010", "9802008068", 0m, "DOZ", 7410m, 50000m, 0m, 0m, 0m, "", 0m, "", 0m, 7409.52m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "V", "Y", 0m, 0m, "", "TESTSUP", "Supplier Company Name", "MB2019100901");
					AssertResults(reader, "BT0000001", "SV973000001", 3, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "CR", "COSTA RICA", "", "6505000100", "9802008068", 0m, "KG", 1348m, 25000m, 0m, 0m, 0m, "", 0m, "", 0m, 1348.08m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "V", "Y", 0m, 0m, "", "TESTSUP", "Supplier Company Name", "MB2019100901");
				}
			}
		}

		public void TestInvoiceLineWithSupTariff()
		{
			var clusterKey = 1;
			var supEntryLinePK = CreateCusEntryLine(entrySummaryPK, "99038501", 1, 10000m, "DutyRateDesc=10%*HasMPF=Y*SupLine=Y", 34.64m, 1000m, clusterKey);
			var normalEntryLinePK = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 0m, $"ChildLineNum=1*CL_ParentLine={supEntryLinePK}*DutyRateDesc=1.4%*HasMPF=Y", 0m, 140m, clusterKey);

			var lineManufacturerAddressPK = TestDataCreator.CreateAddress(supplierPK, "LINEMAN", "Line Manufacturer Address");
			var invoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, 1, "CustomsValue=10000*Duty=140*FTADuty=140*FTAPayableMPF=34.64*NonFTADuty=140*NonFTAPayableMPF=34.64*PayableMPF=34.64*SPI=N/A*SupDuty=1000*SupTariff=99038501", 100m, "KG", 10000, "TESTPRD", lineManufacturerAddressPK, Guid.Empty, normalEntryLinePK, 12.5m, "056", 10m, clusterKey);
			CreateCusUnderbondDec(supEntryLinePK, invoiceLinePK, clusterKey);

			var sql = @"SELECT * from dbo.USEntrySummary(@companyPK, null, null, null, null, null, null, null) ORDER BY JE_DeclarationReference, EntryLineNo, InvoiceLineNo";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					AssertResults(reader, "BT0000001", "SV973000001", 1, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "HK", "HONG KONG", "N/A", "7202195000", "99038501", 100m, "KG", 10000m, 0m, 1140m, 34.64m, 12.5m, "056", 10m, "", 0m, 10000m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "", "N", 140m, 1000m, "TESTPRD", "TESTSUP", "Supplier Company Name", "MB2019100901");
				}
			}
		}

		public void TestReconFlaggedWithOtherReconIndicatorOnly()
		{
			var comPK = TestDataCreator.CreateCompany("TAT", "US", "USD");
			var impPK = TestDataCreator.CreateOrganisation("IMRR8", "TEST Importer");
			var mfrAddressPK = TestDataCreator.CreateAddress(supplierPK, "MANTEST", "TEST Manufacturer Address");
			var branchPK = TestDataCreator.CreateBranch(comPK, "TA1", "USPHL");
			var clusterKey = 2;
			var decPK = CreateJobDeclaration(branchPK, comPK, impPK, supplierPK, mfrAddressPK, "EntryFilerCode=SV9", "TEST00008", clusterKey);
			TestDataCreator.CreateCusEntryNum(decPK, "JobDeclaration", "78000008", "ENS", "CUS", "US");
			var invoicePK = CreateJobComInvoiceHeader(decPK, "INV202005", "UC_NKCountryOfOrigin=US*UC_NKCountryOfExport=AU", Guid.Empty, clusterKey);
			var entryPK = CreateCusEntryHeader(decPK, "ENS", clusterKey);
			var entryLinePK = CreateCusEntryLine(entryPK, "7202195000", 1, 10000m, "DutyRateDesc=1.4%*HasMPF=Y", 34.64m, 140m, clusterKey);
			CreateJobComInvoiceLine(invoicePK, 1, "CustomsValue=10000*Duty=140*FTADuty=140*FTAPayableMPF=34.64*NonFTADuty=140*NonFTAPayableMPF=34.64*PayableMPF=34.64*SPI=N/A", 100m, "KG", 10000m, "TESTPRD", Guid.Empty, Guid.Empty, entryLinePK, 12.5m, "", 0m, clusterKey);

			CheckReconFlaggedValueWithOtherReconIndicator(comPK, decPK, "EntryFilerCode=SV9*OtherReconIndicator=VL");
			CheckReconFlaggedValueWithOtherReconIndicator(comPK, decPK, "EntryFilerCode=SV9*OtherReconIndicator=NA");
			CheckReconFlaggedValueWithOtherReconIndicator(comPK, decPK, "EntryFilerCode=SV9");
		}

		void CheckReconFlaggedValueWithOtherReconIndicator(Guid comPK, Guid decPK, string addInfo)
		{
			UpdateJobDeclarationAddInfo(decPK, addInfo);

			var sql = @"SELECT * from dbo.USEntrySummary(@companyPK, null, null, null, null, null, null, null) ORDER BY JE_DeclarationReference, EntryLineNo, InvoiceLineNo";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, comPK);
				using (var reader = command.ExecuteReader())
				{
					Assert(reader.Read());
					AssertEquals("JE_DeclarationReference", "TEST00008", reader["JE_DeclarationReference"]);
					AssertEquals("ENSEntryNumber", "SV978000008", reader["ENSEntryNumber"]);
					AssertEquals("EntryLineNo", (short)1, reader["EntryLineNo"]);
					AssertEquals("ReconFlagged", addInfo.Contains("OtherReconIndicator=NA") || !addInfo.Contains("OtherReconIndicator") ? "" : "Y", reader["ReconFlagged"]);
					AssertEquals("CountryOfOrigin", "US", reader["CountryOfOrigin"]);
				}
			}
		}

		public void TestInvoiceLinesWithCombinedTariff()
		{
			var clusterKey = 1;
			var parentSupEntryLinePK = CreateCusEntryLine(entrySummaryPK, "9802005060", 1, 100m, "SupLine=Y", 0m, 0m, clusterKey);
			var parentNormalEntryLinePK = CreateCusEntryLine(entrySummaryPK, "", 1, 0m, $"ChildLineNum=1*CL_ParentLine={parentSupEntryLinePK}", 0m, 0m, clusterKey);

			var childSupEntryLinePK = CreateCusEntryLine(entrySummaryPK, "99038001", 1, 0m, $"ChildLineNum=2*CL_ParentLine={parentSupEntryLinePK}*DutyRateDesc=25%*SupLine=Y", 0m, 2500m, clusterKey);
			var childNormalEntryLinePK = CreateCusEntryLine(entrySummaryPK, "7202195000", 1, 10000m, $"ChildLineNum=3*CL_ParentLine={parentSupEntryLinePK}*DutyRateDesc=1.4%", 0m, 140m, clusterKey);

			var parentInvoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, 1, "98GoodsValue=100*FTASPI=C*IsParent=Y*SPI=N/A*SupTariff=9802005060*UC_NKCountryOfExport=CN*UC_NKCountryOfOrigin=CN", 0m, "", 0m, "", Guid.Empty, Guid.Empty, parentNormalEntryLinePK, 0m, "", 0m, clusterKey);
			CreateCusUnderbondDec(parentSupEntryLinePK, parentInvoiceLinePK, clusterKey);

			var childInvoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, 2, "CustomsValue=10000*Duty=140*FTADuty=140*FTASPI=C*NonFTADuty=140*SupDuty=2500*SupTariff=99038001", 100m, "KG", 10000m, "", Guid.Empty, parentInvoiceLinePK, childNormalEntryLinePK, 12.5m, "", 0m, clusterKey);
			CreateCusUnderbondDec(childSupEntryLinePK, childInvoiceLinePK, clusterKey);

			var sql = @"SELECT * from dbo.USEntrySummary(@companyPK, null, null, null, null, null, null, null) ORDER BY JE_DeclarationReference, EntryLineNo, InvoiceLineNo";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					AssertResults(reader, "BT0000001", "SV973000001", 1, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "CN", "CHINA(MAINLAND)", "N/A", "", "9802005060", 0m, "", 0m, 100m, 0m, 0m, 0m, "", 0m, "", 0m, 0m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "", "N", 0m, 0m, "", "TESTSUP", "Supplier Company Name", "MB2019100901");
					AssertResults(reader, "BT0000001", "SV973000001", 1, new DateTime(2019, 10, 01), "FTA", "Y", "1101", "PHILADELPHIA, PA", "CN", "CHINA(MAINLAND)", "N/A", "7202195000", "99038001", 100m, "KG", 10000m, 0m, 2640m, 0m, 12.5m, "", 0m, "", 0m, 10000m, "", "", null, "", "891", "TITANIC", "TEST OWNER", "INV20191009", "", "Y", 140m, 2500m, "", "TESTSUP", "Supplier Company Name", "MB2019100901");
				}
			}
		}

		public void TestSystemCreateTimeUtc()
		{
			var utcNow = DateTime.UtcNow;

			var clusterKey1 = 2;
			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", "SEA", "Calypso", "0308", DateTime.Now, clusterKey1, createTime: utcNow, dataModel: "US");
			CreateEntry(declarationPk1, clusterKey1);

			var clusterKey2 = 3;
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 3, createTime: utcNow.AddMinutes(2), dataModel: "US");
			CreateEntry(declarationPk2, clusterKey2);

			var clusterKey3 = 4;
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", "SEA", "Calypso", "0308", DateTime.Now, 4, createTime: utcNow.AddMinutes(-2), dataModel: "US");
			CreateEntry(declarationPk3, clusterKey3);

			var reportSql = @"SELECT JE_PK FROM USEntrySummary(@companyPK, null, null, @dateFrom, @dateTo, null, null, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<Guid>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.SmallDateTime, utcNow);
				command.AddParameter("@dateTo", SqlDbType.SmallDateTime, utcNow.AddMinutes(1));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((Guid)reader["JE_PK"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals(declarationPk1, retList[0]);
			}
		}

		public void TestEntryDate()
		{
			var now = DateTime.Now;

			var clusterKey1 = 2;
			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "IMP", clusterKey1, addInfo: $"EntryDate={now.AddMinutes(1):yyyy-MM-dd HH:mm:ss}", dataModel: "US");
			CreateEntry(declarationPk1, clusterKey1);

			var clusterKey2 = 3;
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "IMP", clusterKey2, addInfo: $"EntryDate={now.AddMinutes(-3):yyyy-MM-dd HH:mm:ss}", dataModel: "US");
			CreateEntry(declarationPk2, clusterKey2);

			var clusterKey3 = 4;
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "IMP", clusterKey3, addInfo: $"EntryDate={now.AddMinutes(3):yyyy-MM-dd HH:mm:ss}", dataModel: "US");
			CreateEntry(declarationPk3, clusterKey3);

			var reportSql = @"SELECT JE_PK FROM USEntrySummary(@companyPK, null, null, null, null, @entryDateFrom, @entryDateTo, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<Guid>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryDateFrom", SqlDbType.SmallDateTime, now);
				command.AddParameter("@entryDateTo", SqlDbType.SmallDateTime, now.AddMinutes(2));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((Guid)reader["JE_PK"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals(declarationPk1, retList[0]);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var referenceDate = new DateTime(2017, 12, 1);
			companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI", referenceDate);
			supplierPK = TestDataCreator.CreateOrganisation("TESTSUP", "Supplier Company Name");
			var importerPK = TestDataCreator.CreateOrganisation("TESTIMP", "Importer Company Name");
			var manfucaturerAddressPK = TestDataCreator.CreateAddress(supplierPK, "MANUADD", "Manufacturer Address");
			var clusterKey = 1;
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, importerPK, supplierPK, manfucaturerAddressPK, "EntryFilerCode=SV9*EntryDate=2019-10-01*OtherReconIndicator=NA*NAFTAReconIndicator=Y*SuretyCode=891*SchDEntry=1101", "BT0000001", clusterKey);
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "73000001", "ENS", "CUS", "US");

			invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, "INV20191009", "UC_NKCountryOfOrigin=HK*UC_NKCountryOfExport=CN", Guid.Empty, clusterKey);

			entrySummaryPK = CreateCusEntryHeader(declarationPK, "ENS", clusterKey);
		}
		Guid supplierPK;
		Guid companyPK;
		Guid branchPK;
		Guid invoiceHeaderPK;
		Guid entrySummaryPK;

		void CreateEntry(Guid declarationPk1, int clusterKey1)
		{
			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPk1, "INV20191009", "UC_NKCountryOfOrigin=HK*UC_NKCountryOfExport=CN", Guid.Empty, clusterKey1);
			var entrySummaryPK1 = CreateCusEntryHeader(declarationPk1, "ENS", clusterKey1);
			var entryLinePK1 = CreateCusEntryLine(entrySummaryPK1, "7202195000", 1, 10000m, "DutyRateDesc=1.4%*HasMPF=Y", 34.64m, 140m, clusterKey1);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 1, "CustomsValue=10000*Duty=140*FTADuty=140*FTAPayableMPF=34.64*NonFTADuty=140*NonFTAPayableMPF=34.64*PayableMPF=34.64*SPI=N/A", 100m, "KG", 10000m, "TESTPRD", Guid.Empty, Guid.Empty, entryLinePK1, 12.5m, "", 0m, clusterKey1);
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid comPK, Guid importerPK, Guid supplierPK, Guid manufacturerAddressPK, string addInfo, string referenceNumber, int clusterKey, bool withAddOn = true)
		{
			var declarationPK = Guid.NewGuid();
			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_OA_ManufacturerAddress, JE_AddInfo, JE_MasterBill, JE_VesselName, JE_OwnerRef, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, @referenceNumber, 'ACE', 0, @manufacturerAddressPK, @addInfo, 'MB2019100901', 'TITANIC', 'TEST OWNER', @clusterKey)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, comPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplierPK);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@referenceNumber", SqlDbType.VarChar, referenceNumber);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				if (manufacturerAddressPK == Guid.Empty)
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, manufacturerAddressPK);
				}

				command.ExecuteNonQuery();
			}

			return declarationPK;
		}

		void UpdateJobDeclarationAddInfo(Guid declarationPK, string addInfo)
		{
			var addInfoSql = @"
UPDATE JobDeclaration
SET
	JE_AddInfo = @addInfo,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";

			using (var command = Db.Connection.Command(addInfoSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateCusEntryHeader(Guid declarationPK, string messageType, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var entryHeaderSql = @"INSERT INTO dbo.CusEntryHeader(CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES(@entryHeaderPK, 'US', @declarationPK, @messageType, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(entryHeaderSql))
			{
				command.AddParameter("entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		Guid CreateCusEntryLine(Guid entryHeaderPK, string tariffNumber, int entryLineNo, decimal customsValue, string entryLineAddInfo, decimal mpfAmount, decimal dutyAmount, int clusterKey)
		{
			var entryLinePK = Guid.NewGuid();
			var entryLineSql = @"INSERT INTO dbo.CusEntryLine(CL_PK, CL_DataModel, CL_CH, CL_AdValoremTariff, CL_LineNumber, CL_CustomsValue, CL_AddInfo, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser) VALUES
(@entryLinePK, 'US', @entryPK, @tariffNumber, @entryLineNo, @customsValue, @entryLineAddInfo, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(entryLineSql))
			{
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@entryPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@tariffNumber", SqlDbType.VarChar, tariffNumber);
				command.AddParameter("@entryLineNo", SqlDbType.SmallInt, entryLineNo);
				command.AddParameter("@customsValue", SqlDbType.Money, customsValue);
				command.AddParameter("@entryLineAddInfo", SqlDbType.VarChar, entryLineAddInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();

				if (mpfAmount > 0)
				{
					CreateEntryLineChargeFee(entryLinePK, "499", mpfAmount, clusterKey);
				}

				if (dutyAmount > 0)
				{
					CreateEntryLineChargeFee(entryLinePK, "DTY", dutyAmount, clusterKey);
				}
			}
			return entryLinePK;
		}

		Guid CreateJobComInvoiceHeader(Guid declarationPK, string invoiceNumber, string invoiceAddInfo, Guid manufacturerAddressPK, int clusterKey)
		{
			var invoicePK = Guid.NewGuid();
			var invoiceSql = @"INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_InvoiceNumber, JZ_AddInfo, JZ_OA_ManufacturerAddress, JZ_ClusterKey) VALUES (@invoicePK, 'US', @declarationPK, @invoiceNumber, @invoiceAddInfo, @manufacturerAddress, @clusterKey)";

			using (var command = Db.Connection.Command(invoiceSql))
			{
				command.AddParameter("@invoicePK", SqlDbType.UniqueIdentifier, invoicePK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@invoiceNumber", SqlDbType.VarChar, invoiceNumber);
				command.AddParameter("@invoiceAddInfo", SqlDbType.VarChar, invoiceAddInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				if (manufacturerAddressPK == Guid.Empty)
				{
					command.AddParameter("@manufacturerAddress", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@manufacturerAddress", SqlDbType.UniqueIdentifier, manufacturerAddressPK);
				}

				command.ExecuteNonQuery();
			}
			return invoicePK;
		}

		Guid CreateJobComInvoiceLine(Guid invoicePK, int lineNo, string invoiceLineAddInfo, decimal customsQty, string customsUQ, decimal linePrice, string productCode, Guid manufacturerAddressPK, Guid parentLinePK, Guid entryLinePK, decimal hmfAmount, string otherFeeCode, decimal otherFeeAmount, int clusterKey)
		{
			var invoiceLinePK = Guid.NewGuid();
			var invoiceLineSql = @"INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_AddInfo, JI_CustomsQuantity, JI_CustomsUnitQty, JI_LinePrice, JI_PartNo, JI_OA_ManufacturerAddress, JI_ParentID, JI_CL, JI_ClusterKey) VALUES
(@invoiceLinePK, 'US', @invoicePK, @lineNo, @invoiceLineAddInfo, @customsQty, @customsUQ, @linePrice, @partNo, @manufacturerAddress, @parentLinePK, @entryLinePK, @clusterKey)";

			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@invoicePK", SqlDbType.UniqueIdentifier, invoicePK);
				command.AddParameter("@lineNo", SqlDbType.SmallInt, lineNo);
				command.AddParameter("@invoiceLineAddInfo", SqlDbType.VarChar, invoiceLineAddInfo);
				command.AddParameter("@customsQty", SqlDbType.Decimal, customsQty);
				command.AddParameter("@customsUQ", SqlDbType.VarChar, customsUQ);
				command.AddParameter("@linePrice", SqlDbType.Money, linePrice);
				command.AddParameter("@partNo", SqlDbType.VarChar, productCode);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				if (manufacturerAddressPK == Guid.Empty)
				{
					command.AddParameter("@manufacturerAddress", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@manufacturerAddress", SqlDbType.UniqueIdentifier, manufacturerAddressPK);
				}

				if (parentLinePK == Guid.Empty)
				{
					command.AddParameter("@parentLinePK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@parentLinePK", SqlDbType.UniqueIdentifier, parentLinePK);
				}

				command.ExecuteNonQuery();

				if (hmfAmount > 0)
				{
					CreateInvoiceLineCharge(invoiceLinePK, "501", hmfAmount);
				}

				if (!string.IsNullOrEmpty(otherFeeCode) && otherFeeAmount > 0m)
				{
					CreateInvoiceLineCharge(invoiceLinePK, otherFeeCode, otherFeeAmount);
				}
			}
			return invoiceLinePK;
		}

		void CreateEntryLineChargeFee(Guid entryLinePK, string chargeType, decimal chargeAmount, int clusterKey)
		{
			var chargeFeeSql = @"INSERT INTO dbo.CusEntryLineFee(CF_PK, CF_CL, CF_ChargeType, CF_ChargeAmount, CF_ClusterKey, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser) VALUES(@chargeFeePK, @entryLinePK, @chargeType, @chargeAmount, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(chargeFeeSql))
			{
				command.AddParameter("@chargeFeePK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmount", SqlDbType.Decimal, chargeAmount);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
		}

		void CreateInvoiceLineCharge(Guid invoiceLinePK, string chargeType, decimal chargeAmount)
		{
			var invoiceLineChargeSql = @"INSERT INTO dbo.CusCodeData(CY_PK, CY_ParentID, CY_ParentTableCode, CY_Type, CY_Code, CY_Data) VALUES
(@invoiceLineChargePK, @invoiceLinePK, 'JI', 'FEE', @chargeType, @chargeAmount)";

			using (var command = Db.Connection.Command(invoiceLineChargeSql))
			{
				command.AddParameter("@invoiceLineChargePK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmount", SqlDbType.VarChar, " " + chargeAmount.ToString());

				command.ExecuteNonQuery();
			}
		}

		void CreateCusUnderbondDec(Guid entryLinePK, Guid invoiceLinePK, int clusterKey)
		{
			var cusUnderbondDecSql = @"INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_CL, BU_JI, BU_ClusterKey) VALUES(@underbondPK, @entryLinePK, @invoiceLinePK, @clusterKey)";

			using (var command = Db.Connection.Command(cusUnderbondDecSql))
			{
				command.AddParameter("@underbondPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
		}

		void AssertResults(IDataReader reader, string declarationReference, string ensEntryNumber, int entryLineNumber, DateTime? entryDate, string reconIssue
			, string reconFlagged, string entryPort, string entryPortDescription, string countryOfOrigin, string countryOfOriginName, string spi, string tariff,
			string provTariff, decimal customsQty1, string customsUQ1, decimal lineEnteredValue, decimal usOriginValue, decimal totalDuty, decimal payableMPF,
			decimal payableHMF, string otherFeeCode, decimal otherFeeAmount, string cottonFeeExempt, decimal irTaxAmount, decimal linePrice, string supplierCode,
			string supplierName, DateTime? dutyPaidDate, string secondarySPI, string suretyCode, string vesselName, string ownerReference, string invoiceNumber,
			string setIndicator, string isChild, decimal standardDuty, decimal provDuty, string productCode, string manufacturerCode, string manufacturerName, string masterBill)
		{
			CombineAssertions(() =>
			{
				Assert(reader.Read());
				AssertEquals("JE_DeclarationReference", declarationReference, reader["JE_DeclarationReference"]);
				AssertEquals("ENSEntryNumber", ensEntryNumber, reader["ENSEntryNumber"]);
				AssertEquals("EntryLineNo", (short)entryLineNumber, reader["EntryLineNo"]);
				AssertEquals("ReconIssue", reconIssue, reader["ReconIssue"]);
				AssertEquals("ReconFlagged", reconFlagged, reader["ReconFlagged"]);
				AssertEquals("EntryPort", entryPort, reader["EntryPort"]);
				AssertEquals("EntryPortDescription", entryPortDescription, reader["EntryPortDescription"]);
				AssertEquals("CountryOfOrigin", countryOfOrigin, reader["CountryOfOrigin"]);
				AssertEquals("CountryOfOriginName", countryOfOriginName, reader["CountryOfOriginName"]);
				AssertEquals("SPI", spi, reader["SPI"]);
				AssertEquals("Tariff", tariff, reader["Tariff"]);
				AssertEquals("ProvTariff", provTariff, reader["ProvTariff"]);
				AssertEquals("CustomsQty1", customsQty1, reader["CustomsQty1"]);
				AssertEquals("CustomsUQ1", customsUQ1, reader["CustomsUQ1"]);
				AssertEquals("CustomsQty2", 0m, reader["CustomsQty2"]);
				AssertEquals("CustomsUQ2", string.Empty, reader["CustomsUQ2"]);
				AssertEquals("ProvQty1", 0m, reader["ProvQty1"]);
				AssertEquals("ProvUQ1", string.Empty, reader["ProvUQ1"]);
				AssertEquals("LineEnteredValue", lineEnteredValue, reader["LineEnteredValue"]);
				AssertEquals("USOrigValue", usOriginValue, reader["USOrigValue"]);
				AssertEquals("TotalDuty", totalDuty, reader["TotalDuty"]);
				AssertEquals("PayableMPF", payableMPF, reader["PayableMPF"]);
				AssertEquals("PayableHMF", payableHMF, reader["PayableHMF"]);
				AssertEquals("OtherFeeCode", otherFeeCode, reader["OtherFeeCode"]);
				AssertEquals("OtherFeeAmount", otherFeeAmount, reader["OtherFeeAmount"]);
				AssertEquals("CottonFeeExempt", cottonFeeExempt, reader["CottonFeeExempt"]);
				AssertEquals("IRTaxAmount", irTaxAmount, reader["IRTaxAmount"]);
				AssertEquals("LinePrice", linePrice, reader["LinePrice"]);
				AssertEquals("SupplierCode", supplierCode, (string)reader["SupplierCode"]);
				AssertEquals("SupplierName", supplierName, (string)reader["SupplierName"]);
				AssertEquals("SecondarySPI", secondarySPI, reader["SecondarySPI"]);
				AssertEquals("SuretyCode", suretyCode, reader["SuretyCode"]);
				AssertEquals("JE_VesselName", vesselName, reader["JE_VesselName"]);
				AssertEquals("JE_OwnerRef", ownerReference, reader["JE_OwnerRef"]);
				AssertEquals("JZ_InvoiceNumber", invoiceNumber, reader["JZ_InvoiceNumber"]);
				AssertEquals("SetIndicator", setIndicator, reader["SetIndicator"]);
				AssertEquals("IsChild", isChild, reader["IsChild"]);
				AssertEquals("StandardDuty", standardDuty, reader["StandardDuty"]);
				AssertEquals("ProvProgDuty", provDuty, reader["ProvProgDuty"]);
				AssertEquals("ProductCode", productCode, reader["ProductCode"]);
				AssertEquals("ManufacturerCode", manufacturerCode, reader["ManufacturerCode"]);
				AssertEquals("ManufacturerName", manufacturerName, reader["ManufacturerName"]);
				AssertEquals("JE_MasterBill", masterBill, reader["JE_MasterBill"]);

				if (entryDate != null)
				{
					AssertEquals("US_EntryDate", entryDate, (DateTime)reader["US_EntryDate"]);
				}
				else
				{
					AssertEquals("US_EntryDate", DBNull.Value, reader["US_EntryDate"]);
				}

				if (dutyPaidDate != null)
				{
					AssertEquals("DutyPaidDate", dutyPaidDate, reader["DutyPaidDate"]);
				}
				else
				{
					AssertEquals("DutyPaidDate", DBNull.Value, reader["DutyPaidDate"]);
				}
			});
		}
		#endregion
	}
}
