using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USInvoiceLines))]
	class USInvoiceLinesTest : DbCreateScriptTest
	{
		public void TestJI_SupTariffAndAdditionalTariff()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var cusEntryENSHeaderPK = CreateCusEntryHeader(declarationPK, "DestinationState=TN", "ENS", 2032.47f, 1);
			var cusEntrySEHeaderPK = CreateCusEntryHeader(declarationPK, "DestinationState=TN", "SE", 0f, 1);
			var cusEntryLinePK1 = CreateCusEntryLine(cusEntryENSHeaderPK, "DutyRateDesc=15%*SupAdditionalLine=Y", "99038815", 15.0f, 1);
			var cusEntryLinePK2 = CreateCusEntryLine(cusEntryENSHeaderPK, "DutyRateDesc=25%*SupAdditionalLine2=Y", "99038816", 15.0f, 1);
			var cusEntryLinePK3 = CreateCusEntryLine(cusEntryENSHeaderPK, "DutyRateDesc=35%*SupAdditionalLine3=Y", "99038817", 15.0f, 1);
			var cusEntryLinePK4 = CreateCusEntryLine(cusEntryENSHeaderPK, "DutyRateDesc=45%", "99038818", 15.0f, 1);
			var cusEntryLinePK5 = CreateCusEntryLine(cusEntrySEHeaderPK, "DutyRateDesc=55%*SupAdditionalLine5=Y", "99038819", 15.0f, 1);

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'SupTariff=99038815*Duty=10*SupDuty=20', '', 1);
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038823', 'AT1', 'SupDuty=200', 10, 'GK', 110, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038805', 'AT2', 'SupDuty=150', 20, 'TNE', 120, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030121', 'AT3', '', 0, 'ME', 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030122', 'AT4', '', 0, '', 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030123', 'AT5', '', 0, 0, 'US');

				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'SupTariff=99038001', '', 1);
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK2, 'JI', '99038826', 'AT1', 'SupDuty=300', 30, 130, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK2, 'JI', '99038806', 'AT2', '', 0, 0, 'US')
";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}

			var cusUnderBondDecSql = @"
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK1, @invoiceLinePK1, @clPK1, 1);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK2, @invoiceLinePK1, @clPK2, 1);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK3, @invoiceLinePK1, @clPK3, 1)
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK4, @invoiceLinePK1, @clPK4, 1);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK5, @invoiceLinePK1, @clPK5, 1)";
			using (var command = Db.Connection.Command(cusUnderBondDecSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
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
				command.ExecuteNonQuery();
			}

			var provProgTariffStartsWith1 = "99030123";
			var provProgTariffStartsWith2 = "99031234";
			var provProgTariffStartsWith3 = "99038001";
			var provProgTariffStartsWith4 = "";
			var provProgTariffStartsWith5 = "";

			var reportSql = @"SELECT JI_PK, SupTariff, ProvProgAddtionalTariff1, ProvProgAddtionalTariff2, ProvProgAddtionalTariff3,ProvProgAddtionalTariff4, ProvProgAddtionalTariff5,
ProvProgAddtionalDuty1, ProvProgAddtionalDuty2, ProvProgAddtionalDuty3, ProvProgAddtionalDuty4, ProvProgAddtionalDuty5, TotalLineDuty,
ProvProgAddtionalDutyRate1, ProvProgAddtionalDutyRate2, ProvProgAddtionalDutyRate3, ProvProgAddtionalDutyRate4, ProvProgAddtionalDutyRate5,
ProvProgAddtionalQty1, ProvProgAddtionalQty2, ProvProgAddtionalQty3, ProvProgAddtionalQty4, ProvProgAddtionalQty5,
ProvProgAddtionalUQ1, ProvProgAddtionalUQ2, ProvProgAddtionalUQ3, ProvProgAddtionalUQ4, ProvProgAddtionalUQ5,
ProvProgAddtionalGoodsValue1, ProvProgAddtionalGoodsValue2, ProvProgAddtionalGoodsValue3, ProvProgAddtionalGoodsValue4, ProvProgAddtionalGoodsValue5
FROM dbo.USInvoiceLines(@companyPK,'','','','','','','','','','','','','',@provProgTariffStartsWith1,@provProgTariffStartsWith2,@provProgTariffStartsWith3,@provProgTariffStartsWith4,@provProgTariffStartsWith5,'','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportListForTariff = new List<Tuple<Guid, string, string, string, string, string, string>>();
				var reportListForDuty = new List<Tuple<Guid, decimal, decimal, decimal, decimal, decimal>>();
				var reportListForTatalLineRate = new List<Tuple<Guid, decimal>>();
				var reportListForDutyRate = new List<Tuple<Guid, string, string, string, string, string>>();
				var reportListForQty = new List<Tuple<Guid, decimal, decimal, decimal, decimal, decimal>>();
				var reportListForGoodsValue = new List<Tuple<Guid, decimal, decimal, decimal, decimal, decimal>>();
				var reportListForUQ = new List<Tuple<Guid, string, string, string, string, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@provProgTariffStartsWith1", SqlDbType.VarChar, provProgTariffStartsWith1);
				command.AddParameter("@provProgTariffStartsWith2", SqlDbType.VarChar, provProgTariffStartsWith2);
				command.AddParameter("@provProgTariffStartsWith3", SqlDbType.VarChar, provProgTariffStartsWith3);
				command.AddParameter("@provProgTariffStartsWith4", SqlDbType.VarChar, provProgTariffStartsWith4);
				command.AddParameter("@provProgTariffStartsWith5", SqlDbType.VarChar, provProgTariffStartsWith5);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var supTariff = (string)reader["SupTariff"];
						var addtionalTariff1 = (string)reader["ProvProgAddtionalTariff1"];
						var addtionalTariff2 = (string)reader["ProvProgAddtionalTariff2"];
						var addtionalTariff3 = (string)reader["ProvProgAddtionalTariff3"];
						var addtionalTariff4 = (string)reader["ProvProgAddtionalTariff4"];
						var addtionalTariff5 = (string)reader["ProvProgAddtionalTariff5"];
						reportListForTariff.Add(new Tuple<Guid, string, string, string, string, string, string>(pk, supTariff, addtionalTariff1, addtionalTariff2, addtionalTariff3, addtionalTariff4, addtionalTariff5));

						var addtionalDuty1 = (decimal)reader["ProvProgAddtionalDuty1"];
						var addtionalDuty2 = (decimal)reader["ProvProgAddtionalDuty2"];
						var addtionalDuty3 = (decimal)reader["ProvProgAddtionalDuty3"];
						var addtionalDuty4 = (decimal)reader["ProvProgAddtionalDuty4"];
						var addtionalDuty5 = (decimal)reader["ProvProgAddtionalDuty5"];
						reportListForDuty.Add(new Tuple<Guid, decimal, decimal, decimal, decimal, decimal>(pk, addtionalDuty1, addtionalDuty2, addtionalDuty3, addtionalDuty4, addtionalDuty5));

						var tatalLineRate = (decimal)reader["TotalLineDuty"];
						reportListForTatalLineRate.Add(new Tuple<Guid, decimal>(pk, tatalLineRate));

						var addtionalDutyRate1 = (string)reader["ProvProgAddtionalDutyRate1"];
						var addtionalDutyRate2 = (string)reader["ProvProgAddtionalDutyRate2"];
						var addtionalDutyRate3 = (string)reader["ProvProgAddtionalDutyRate3"];
						var addtionalDutyRate4 = (string)reader["ProvProgAddtionalDutyRate4"];
						var addtionalDutyRate5 = (string)reader["ProvProgAddtionalDutyRate5"];
						reportListForDutyRate.Add(new Tuple<Guid, string, string, string, string, string>(pk, addtionalDutyRate1, addtionalDutyRate2, addtionalDutyRate3, addtionalDutyRate4, addtionalDutyRate5));

						var addtionalQty1 = (decimal)reader["ProvProgAddtionalQty1"];
						var addtionalQty2 = (decimal)reader["ProvProgAddtionalQty2"];
						var addtionalQty3 = (decimal)reader["ProvProgAddtionalQty3"];
						var addtionalQty4 = (decimal)reader["ProvProgAddtionalQty4"];
						var addtionalQty5 = (decimal)reader["ProvProgAddtionalQty5"];
						reportListForQty.Add(new Tuple<Guid, decimal, decimal, decimal, decimal, decimal>(pk, addtionalQty1, addtionalQty2, addtionalQty3, addtionalQty4, addtionalQty5));

						var addtionalUQ1 = (string)reader["ProvProgAddtionalUQ1"];
						var addtionalUQ2 = (string)reader["ProvProgAddtionalUQ2"];
						var addtionalUQ3 = (string)reader["ProvProgAddtionalUQ3"];
						var addtionalUQ4 = (string)reader["ProvProgAddtionalUQ4"];
						var addtionalUQ5 = (string)reader["ProvProgAddtionalUQ5"];
						reportListForUQ.Add(new Tuple<Guid, string, string, string, string, string>(pk, addtionalUQ1, addtionalUQ2, addtionalUQ3, addtionalUQ4, addtionalUQ5));

						var addtionalGoodsValue1 = (decimal)reader["ProvProgAddtionalGoodsValue1"];
						var addtionalGoodsValue2 = (decimal)reader["ProvProgAddtionalGoodsValue2"];
						var addtionalGoodsValue3 = (decimal)reader["ProvProgAddtionalGoodsValue3"];
						var addtionalGoodsValue4 = (decimal)reader["ProvProgAddtionalGoodsValue4"];
						var addtionalGoodsValue5 = (decimal)reader["ProvProgAddtionalGoodsValue5"];
						reportListForGoodsValue.Add(new Tuple<Guid, decimal, decimal, decimal, decimal, decimal>(pk, addtionalGoodsValue1, addtionalGoodsValue2, addtionalGoodsValue3, addtionalGoodsValue4, addtionalGoodsValue5));
					}
				}
				AssertEquals(2, reportListForTariff.Count);
				AssertEquals(2, reportListForDuty.Count);
				AssertEquals(2, reportListForQty.Count);
				AssertEquals(2, reportListForGoodsValue.Count);
				CombineAssertions(() =>
				{
					var tariffResult1 = reportListForTariff.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("99038815", tariffResult1.Item2);
					AssertEquals("99038823", tariffResult1.Item3);
					AssertEquals("99038805", tariffResult1.Item4);
					AssertEquals("99030121", tariffResult1.Item5);
					AssertEquals("99030122", tariffResult1.Item6);
					AssertEquals("99030123", tariffResult1.Item7);

					var dutyResult1 = reportListForDuty.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(200m, dutyResult1.Item2);
					AssertEquals(150m, dutyResult1.Item3);
					AssertEquals(0m, dutyResult1.Item4);
					AssertEquals(0m, dutyResult1.Item5);
					AssertEquals(0m, dutyResult1.Item6);

					var reportListForTatalLineRateResult1 = reportListForTatalLineRate.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(380m, reportListForTatalLineRateResult1.Item2);

					var dutyRateResult1 = reportListForDutyRate.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("15%", dutyRateResult1.Item2);
					AssertEquals("25%", dutyRateResult1.Item3);
					AssertEquals("35%", dutyRateResult1.Item4);
					AssertEquals("", dutyRateResult1.Item5);
					AssertEquals("", dutyRateResult1.Item6);

					var qtyResult1 = reportListForQty.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(10m, qtyResult1.Item2);
					AssertEquals(20m, qtyResult1.Item3);
					AssertEquals(0m, qtyResult1.Item4);
					AssertEquals(0m, qtyResult1.Item5);
					AssertEquals(0m, qtyResult1.Item6);

					var uqResult1 = reportListForUQ.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("GK", uqResult1.Item2);
					AssertEquals("TNE", uqResult1.Item3);
					AssertEquals("ME", uqResult1.Item4);
					AssertEquals("", uqResult1.Item5);
					AssertEquals("", uqResult1.Item6);

					var goodsValueResult1 = reportListForGoodsValue.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(110m, goodsValueResult1.Item2);
					AssertEquals(120m, goodsValueResult1.Item3);
					AssertEquals(0m, goodsValueResult1.Item4);
					AssertEquals(0m, goodsValueResult1.Item5);
					AssertEquals(0m, goodsValueResult1.Item6);

					var tariffResult2 = reportListForTariff.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals("99038001", tariffResult2.Item2);
					AssertEquals("99038826", tariffResult2.Item3);
					AssertEquals("99038806", tariffResult2.Item4);
					AssertEquals(ZString.Empty, tariffResult2.Item5);
					AssertEquals(ZString.Empty, tariffResult2.Item6);
					AssertEquals(ZString.Empty, tariffResult2.Item7);

					var dutyResult2 = reportListForDuty.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals(300m, dutyResult2.Item2);
					AssertEquals(0m, dutyResult2.Item3);
					AssertEquals(0m, dutyResult2.Item4);
					AssertEquals(0m, dutyResult2.Item5);
					AssertEquals(0m, dutyResult2.Item6);

					var reportListForTatalLineRateResult2 = reportListForTatalLineRate.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals(300m, reportListForTatalLineRateResult2.Item2);

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

		public void TestAluminumAndSteelDataFields()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 2);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 2, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'Prim_NA=Y*RN_NKPrimCtry=AU*Sec_NA=Y*RN_NKSecCtry=CA*RN_NKCastCtry=CH*RN_NKCertOrigin=IN*RN_NKMeltCtry=IL', '', 2);

				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'Prim_NA=N*RN_NKPrimCtry=US*Sec_NA=N*RN_NKSecCtry=UK*RN_NKCastCtry=AU*RN_NKCertOrigin=NL*RN_NKMeltCtry=CH', '', 2);
";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT JI_PK, PrimNA, PrimCtry, SecNA, SecCtry, CastCtry, CertOrigin, MeltCtry
FROM dbo.USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportListForAluminumAndSteel = new List<Tuple<Guid, string, string, string, string, string, string, Tuple<string>>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var primNA = (string)reader["PrimNA"];
						var primCtry = (string)reader["PrimCtry"];
						var secNA = (string)reader["SecNA"];
						var secCtry = (string)reader["SecCtry"];
						var castCtry = (string)reader["CastCtry"];
						var certOrigin = (string)reader["CertOrigin"];
						var meltCtry = (string)reader["MeltCtry"];
						reportListForAluminumAndSteel.Add(new Tuple<Guid, string, string, string, string, string, string, Tuple<string>>(pk, primNA, primCtry, secNA, secCtry, castCtry, certOrigin, new Tuple<string>(meltCtry)));
					}
				}
				AssertEquals(2, reportListForAluminumAndSteel.Count);
				CombineAssertions(() =>
				{
					var aluminumAndSteelResult1 = reportListForAluminumAndSteel.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("Y", aluminumAndSteelResult1.Item2);
					AssertEquals("AU", aluminumAndSteelResult1.Item3);
					AssertEquals("Y", aluminumAndSteelResult1.Item4);
					AssertEquals("CA", aluminumAndSteelResult1.Item5);
					AssertEquals("CH", aluminumAndSteelResult1.Item6);
					AssertEquals("IN", aluminumAndSteelResult1.Item7);
					AssertEquals("IL", aluminumAndSteelResult1.Rest.Item1);

					var aluminumAndSteelResult2 = reportListForAluminumAndSteel.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals("", aluminumAndSteelResult2.Item2);
					AssertEquals("US", aluminumAndSteelResult2.Item3);
					AssertEquals("", aluminumAndSteelResult2.Item4);
					AssertEquals("UK", aluminumAndSteelResult2.Item5);
					AssertEquals("AU", aluminumAndSteelResult2.Item6);
					AssertEquals("NL", aluminumAndSteelResult2.Item7);
					AssertEquals("CH", aluminumAndSteelResult2.Rest.Item1);
				});
			}
		}

		public void TestLineCountryOfOriginAndLineCountryOfExport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLinePK3 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'SupTariff=99038002*UC_NKCountryOfExport=CN*UC_NKCountryOfOrigin=HK', '', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'IsParent=Y*SupTariff=99038815*UC_NKCountryOfExport=HK*UC_NKCountryOfOrigin=CN', '', 1)
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ParentID, JI_ClusterKey) VALUES (@invoiceLinePK3, 'US', @invoiceHeaderPK, 3, 100, 'SupTariff=99038001', '', @invoiceLinePK2, 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@invoiceLinePK3", SqlDbType.UniqueIdentifier, invoiceLinePK3);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select JI_PK, LineCountryOfOrigin, LineCountryOfOriginName, LineCountryOfExport, LineCountryOfExportName from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string, string, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var lineCountryOfOrigin = reader["LineCountryOfOrigin"].ToString();
						var lineCountryOfOriginName = reader["LineCountryOfOriginName"].ToString();
						var lineCountryOfExport = reader["LineCountryOfExport"].ToString();
						var lineCountryOfExportName = reader["LineCountryOfExportName"].ToString();
						reportList.Add(new Tuple<Guid, string, string, string, string>(pk, lineCountryOfOrigin, lineCountryOfOriginName, lineCountryOfExport, lineCountryOfExportName));
					}
				}
				AssertEquals(3, reportList.Count);
				CombineAssertions(() =>
				{
					var result1 = reportList.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("HK", result1.Item2);
					AssertEquals("HONG KONG", result1.Item3);
					AssertEquals("CN", result1.Item4);
					AssertEquals("CHINA(MAINLAND)", result1.Item5);

					var result2 = reportList.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals("CN", result2.Item2);
					AssertEquals("CHINA(MAINLAND)", result2.Item3);
					AssertEquals("HK", result2.Item4);
					AssertEquals("HONG KONG", result2.Item5);

					var result3 = reportList.Find(x => x.Item1 == invoiceLinePK3);
					AssertEquals("CN", result3.Item2);
					AssertEquals("CHINA(MAINLAND)", result3.Item3);
					AssertEquals("HK", result3.Item4);
					AssertEquals("HONG KONG", result3.Item5);
				});
			}
		}

		public void TestGrossWeight()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_Weight, JI_Tariff, JI_AddInfo, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 2, '', 'GrossWeight=1', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_Weight, JI_Tariff, JI_AddInfo, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 0, '', 'GrossWeight=2', 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT JI_PK, JI_Weight FROM USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, decimal>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var grossWeight = (decimal)reader["JI_Weight"];
						reportList.Add(new Tuple<Guid, decimal>(pk, grossWeight));
					}
				}

				AssertEquals(2, reportList.Count);
				CombineAssertions(() =>
				{
					var result1 = reportList.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals(2m, result1.Item2);
					var result2 = reportList.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals(0m, result2.Item2);
				});
			}
		}

		public void TestContainerNumbers()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'TTBRateDesignationCode=TST1*CBMADefaultTaxAmount=1.45678912*CBMADefaultTaxRate=1.23234545', '', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'TTBRateDesignationCode=TST2*CBMADefaultTaxAmount=2.45678912*CBMADefaultTaxRate=2.23234545', '', 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}

			var refContainer01 = TestDataCreator.CreateRefContainer("REFCON0001");
			var refContainer02 = TestDataCreator.CreateRefContainer("REFCON0002");
			var refContainer03 = TestDataCreator.CreateRefContainer("REFCON0003");
			var refContainer04 = TestDataCreator.CreateRefContainer("REFCON0004");
			var jobContainer01 = TestDataCreator.CreateJobContainer("CONT0001", refContainer01);
			var jobContainer02 = TestDataCreator.CreateJobContainer("CONT0002", refContainer02);
			var jobContainer03 = TestDataCreator.CreateJobContainer("CONT0003", refContainer03);
			var jobContainer04 = TestDataCreator.CreateJobContainer("CONT0004", refContainer04);
			var container01 = TestDataCreator.CreateCusContainer("CONT0001", declarationPK, 1, "US", jobContainer01);
			var container02 = TestDataCreator.CreateCusContainer("CONT0002", declarationPK, 1, "US", jobContainer02);
			var container03 = TestDataCreator.CreateCusContainer("CONT0003", declarationPK, 1, "US", jobContainer03);
			var container04 = TestDataCreator.CreateCusContainer("CONT0004", declarationPK, 1, "US", jobContainer04);
			TestDataCreator.CreateCusContainerInvoiceLinePivot(container01, invoiceLinePK1, 1);
			TestDataCreator.CreateCusContainerInvoiceLinePivot(container03, invoiceLinePK1, 1);

			var reportSql = @"SELECT JI_PK, ContainerNumbers FROM USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var containerNumbers = (string)reader["ContainerNumbers"];
						reportList.Add(new Tuple<Guid, string>(pk, containerNumbers));
					}
				}

				AssertEquals(2, reportList.Count);
				CombineAssertions(() =>
				{
					var result1 = reportList.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("CONT0001 (REFCON0001), CONT0003 (REFCON0003)", result1.Item2);
					var result2 = reportList.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals("CONT0001 (REFCON0001), CONT0002 (REFCON0002), CONT0003 (REFCON0003), CONT0004 (REFCON0004)", result2.Item2);
				});
			}
		}

		public void TestCBMAAddInfoData()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'TTBRateDesignationCode=TST1*CBMADefaultTaxAmount=1.45678912*CBMADefaultTaxRate=1.23234545', '', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'TTBRateDesignationCode=TST2*CBMADefaultTaxAmount=2.45678912*CBMADefaultTaxRate=2.23234545', '', 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select JI_PK, TTBRateDesignationCode, CBMADefaultTaxAmount, CBMADefaultTaxRate from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, decimal, decimal>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var ttbRateDesignationCode = (string)reader["TTBRateDesignationCode"];
						var cbmaDefaultTaxAmount = (decimal)reader["CBMADefaultTaxAmount"];
						var cbmaDefaultTaxRate = (decimal)reader["CBMADefaultTaxRate"];
						reportList.Add(new Tuple<Guid, string, decimal, decimal>(pk, ttbRateDesignationCode, cbmaDefaultTaxAmount, cbmaDefaultTaxRate));
					}
				}

				AssertEquals(2, reportList.Count);
				CombineAssertions(() =>
				{
					var result1 = reportList.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("TST1", result1.Item2);
					AssertEquals(1.45679m, result1.Item3);
					AssertEquals(1.23234545m, result1.Item4);
					var result2 = reportList.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals("TST2", result2.Item2);
					AssertEquals(2.45679m, result2.Item3);
					AssertEquals(2.23234545m, result2.Item4);
				});
			}
		}

		public void TestUSInvoiceLinesValueOfExclusionInfoOnReport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_PartNo, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'ProductExclusion=01*ExclusionNumber=SDR038815', '', 'ABC123', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ParentID, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'ProductExclusion=02*ExclusionNumber=TLR000001', '', @parentID, 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select JI_PK, ProductExclusion, ExclusionNumber from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var exclusionNumber = (string)reader["ExclusionNumber"];
						var productExclusion = (string)reader["ProductExclusion"];
						reportList.Add(new Tuple<Guid, string, string>(pk, productExclusion, exclusionNumber));
					}
				}
				AssertEquals(2, reportList.Count);
				CombineAssertions(() =>
				{
					var result1 = reportList.Find(x => x.Item1 == invoiceLinePK1);
					AssertEquals("01", result1.Item2);
					AssertEquals("SDR038815", result1.Item3);
					var result2 = reportList.Find(x => x.Item1 == invoiceLinePK2);
					AssertEquals("02", result2.Item2);
					AssertEquals("TLR000001", result2.Item3);
				});
			}
		}

		public void TestOrderNumberAndJIOrderNumber()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var shipmentPK = TestDataCreator.CreateShipment("S00112233");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var org = TestDataCreator.CreateOrganisation("BUY1", "BUYER 33");
			var address = TestDataCreator.CreateAddress(org, "BUYER Address", "Address 334455");

			var orderHeaderPK1 = TestDataCreator.CreateJobOrderHeader("Z123", address, shipment: shipmentPK);
			var orderHeaderPK2 = TestDataCreator.CreateJobOrderHeader("Y123", address, orderNumberSplit: 1, shipment: shipmentPK);
			var orderHeaderPK3 = TestDataCreator.CreateJobOrderHeader("X123", address, orderNumberSplit: 2, shipment: shipmentPK);
			var orderLinePK1 = TestDataCreator.CreateJobOrderLine(orderHeaderPK1);
			var orderLinePK2 = TestDataCreator.CreateJobOrderLine(orderHeaderPK2);
			var orderLinePK3 = TestDataCreator.CreateJobOrderLine(orderHeaderPK3);
			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLinePK3 = Guid.NewGuid();
			var invoiceLinePK4 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_OrderNumber, JI_PartNo, JI_JO, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'IsParent=Y*SupTariff=99038815', '', '', 'ABC123', null, 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_OrderNumber, JI_PartNo, JI_JO, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'SupTariff=99038001', '', 'JI123', 'TEST1', @orderLinePK1, 1)
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_OrderNumber, JI_PartNo, JI_JO, JI_ClusterKey) VALUES (@invoiceLinePK3, 'US', @invoiceHeaderPK, 3, 100, 'SupTariff=99038001', '', 'JI456', 'TEST2', @orderLinePK2, 1)
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_OrderNumber, JI_PartNo, JI_JO, JI_ClusterKey) VALUES (@invoiceLinePK4, 'US', @invoiceHeaderPK, 4, 100, 'SupTariff=99038001', '', 'JI789', 'TEST3', @orderLinePK3, 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@invoiceLinePK3", SqlDbType.UniqueIdentifier, invoiceLinePK3);
				command.AddParameter("@invoiceLinePK4", SqlDbType.UniqueIdentifier, invoiceLinePK4);
				command.AddParameter("@orderLinePK1", SqlDbType.UniqueIdentifier, orderLinePK1);
				command.AddParameter("@orderLinePK2", SqlDbType.UniqueIdentifier, orderLinePK2);
				command.AddParameter("@orderLinePK3", SqlDbType.UniqueIdentifier, orderLinePK3);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select JI_LineNo, ProductCode, JI_OrderNumber from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new Dictionary<string, Tuple<string, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var lineNo = reader["JI_LineNo"].ToString();
						var productCode = reader["ProductCode"].ToString();
						var orderNumber = reader["JI_OrderNumber"].ToString();
						reportList.Add(lineNo, new Tuple<string, string>(productCode, orderNumber));
					}
				}
				AssertEquals(4, reportList.Count);
				CombineAssertions(() =>
				{
					AssertEquals("ABC123", reportList["1"].Item1);
					AssertEquals("", reportList["1"].Item2);
					AssertEquals("TEST1", reportList["2"].Item1);
					AssertEquals("Z123", reportList["2"].Item2);
					AssertEquals("TEST2", reportList["3"].Item1);
					AssertEquals("Y123-1", reportList["3"].Item2);
					AssertEquals("TEST3", reportList["4"].Item1);
					AssertEquals("X123-2", reportList["4"].Item2);
				});
			}
		}

		public void TestUSInvoiceLinePriceUnit()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_InvoiceQuantity, JI_AddInfo, JI_Tariff, JI_PartNo, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 10, 'IsParent=Y*SupTariff=99038815', '', 'ABC123', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_InvoiceQuantity, JI_AddInfo, JI_Tariff, JI_ParentID, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 0, 4, 'SupTariff=99038001', '', @parentID, 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select JI_PK, PriceUnit from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, decimal>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var productCode = (decimal)reader["PriceUnit"];
						reportList.Add(new Tuple<Guid, decimal>(pk, productCode));
					}
				}
				AssertEquals(2, reportList.Count);
				CombineAssertions(() =>
				{
					AssertEquals(10m, reportList[0].Item1 == invoiceLinePK1 ? reportList[0].Item2 : reportList[1].Item2);
					AssertEquals(0m, reportList[1].Item1 == invoiceLinePK2 ? reportList[1].Item2 : reportList[0].Item2);
				});
			}
		}

		public void TestUSInvoiceLinesValueOfProductCodeOnReport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_PartNo, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'IsParent=Y*SupTariff=99038815', '', 'ABC123', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ParentID, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'SupTariff=99038001', '', @parentID, 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select JI_PK, ProductCode from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var productCode = (string)reader["ProductCode"];
						reportList.Add(new Tuple<Guid, string>(pk, productCode));
					}
				}
				AssertEquals(2, reportList.Count);
				CombineAssertions(() =>
				{
					AssertEquals("ABC123", reportList[0].Item2);
					AssertEquals("ABC123", reportList[1].Item2);
				});
			}
		}

		public void TestUSCountryOfExportValueFromAddInfoInLineOnReport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationAddInfo = "";
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1, addInfo: declarationAddInfo);

			var billAddInfo = "UC_NKCountryOfExport=AU";
			var billPK = TestDataCreator.CreateCusDecHouseBill(true, billAddInfo, declarationPK, 1);

			var invoiceHeaderAddInfo = "";
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, addInfo: invoiceHeaderAddInfo, relatedHouseBillPK: billPK, dataModel: "US");

			var invoiceLineAddInfo = "";
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, addInfo: invoiceLineAddInfo, dataModel: "US");

			var reportSql = @"select JE_PK, CountryOfExport from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var reportList = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						reportList[reader.GetGuid(0)] = (string)reader["CountryOfExport"];
					}
					AssertEquals(1, reportList.Count);
					AssertEquals("CountryOfExport", "AU", reportList[declarationPK]);
				}
			}
		}

		public void TestUSInvoiceLinesValueOfProvProgDutyRateOnReport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var cusEntryENSHeaderPK = CreateCusEntryHeader(declarationPK, "DestinationState=TN", "ENS", 2032.47f, 1);
			var cusEntrySEHeaderPK = CreateCusEntryHeader(declarationPK, "DestinationState=TN", "SE", 0f, 1);
			var cusEntryLinePK1 = CreateCusEntryLine(cusEntryENSHeaderPK, "DutyRateDesc=15%*HasMPF=Y*SupLine=Y", "99038815", 15.0f, 1);
			var cusEntryLinePK2 = CreateCusEntryLine(cusEntrySEHeaderPK, "ChildLineNum=1*CL_ParentLine=535ce95b-286f-4148-85a7-7ddd37a4a73b*CL_SupLine=535ce95b-286f-4148-85a7-7ddd37a4a73b", "", 0f, 1);
			var cusEntryLinePK3 = CreateCusEntryLine(cusEntrySEHeaderPK, "SupLine=Y", "99038815", 0f, 1);
			var cusEntryLinePK4 = CreateCusEntryLine(cusEntrySEHeaderPK, "ChildLineNum=2*CL_ParentLine=535ce95b-286f-4148-85a7-7ddd37a4a73b*SupLine=Y", "99038801", 0f, 1);
			var cusEntryLinePK5 = CreateCusEntryLine(cusEntryENSHeaderPK, "ChildLineNum=2*CL_ParentLine=c1cb8c38-aed3-4446-95ac-7725a7fa7b57*DutyRateDesc=25%*HasMPF=Y*SupLine=Y", "99038801", 25.0f, 1);
			var cusEntryLinePK6 = CreateCusEntryLine(cusEntrySEHeaderPK, "ChildLineNum=3*CL_ParentLine=535ce95b-286f-4148-85a7-7ddd37a4a73b*CL_SupLine=cf94f640-35b4-4b22-a9a6-02cc8d1a1d4f", "8544300000", 0f, 1);
			var cusEntryLinePK7 = CreateCusEntryLine(cusEntryENSHeaderPK, "ChildLineNum=1*CL_ParentLine=c1cb8c38-aed3-4446-95ac-7725a7fa7b57*HasMPF=Y", "", 0f, 1);
			var cusEntryLinePK8 = CreateCusEntryLine(cusEntryENSHeaderPK, "ChildLineNum=3*CL_ParentLine=c1cb8c38-aed3-4446-95ac-7725a7fa7b57*DutyRateDesc=5%*HasMPF=Y", "8544300000", 5.0f, 1);

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_CL, JI_CustomsUnitQty, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 0, 'FTASPI=C*IsParent=Y*SupDuty=750*SupTariff=99038815', '', @clPK1, '', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_CL, JI_CustomsUnitQty, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 5000, 'CustomsValue=5000*Duty=250*FTAPayableMPF=26.22*FTASPI=C*NonFTADuty=250*NonFTAPayableMPF=26.22*PayableMPF=26.22*SupDuty=1250*SupTariff=99038001', '7217105030', @clPK2, '', 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@clPK1", SqlDbType.UniqueIdentifier, cusEntryLinePK7);
				command.AddParameter("@clPK2", SqlDbType.UniqueIdentifier, cusEntryLinePK8);
				command.ExecuteNonQuery();
			}

			var cusUnderBondDecSql = @"
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK1, @invoiceLinePK1, @clPK1, 1);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK2, @invoiceLinePK1, @clPK2, 1);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK3, @invoiceLinePK1, @clPK3, 1)
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK4, @invoiceLinePK2, @clPK4, 1);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK5, @invoiceLinePK2, @clPK5, 1);
				INSERT INTO dbo.CusUnderbondDec(BU_PK, BU_JI, BU_CL, BU_ClusterKey) VALUES (@bondDecPK6, @invoiceLinePK2, @clPK6, 1)";
			using (var command = Db.Connection.Command(cusUnderBondDecSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@clPK1", SqlDbType.UniqueIdentifier, cusEntryLinePK1);
				command.AddParameter("@clPK2", SqlDbType.UniqueIdentifier, cusEntryLinePK2);
				command.AddParameter("@clPK3", SqlDbType.UniqueIdentifier, cusEntryLinePK3);
				command.AddParameter("@clPK4", SqlDbType.UniqueIdentifier, cusEntryLinePK4);
				command.AddParameter("@clPK5", SqlDbType.UniqueIdentifier, cusEntryLinePK5);
				command.AddParameter("@clPK6", SqlDbType.UniqueIdentifier, cusEntryLinePK6);
				command.AddParameter("@bondDecPK1", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK2", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK3", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK4", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK5", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@bondDecPK6", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT JI_PK, ProvProgDuty, ProvProgDutyRate FROM USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '') ORDER BY JI_LineNo";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, decimal, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["JI_PK"];
						var dutyRate = (decimal)reader["ProvProgDuty"];
						var provProgRate = (string)reader["ProvProgDutyRate"];
						reportList.Add(new Tuple<Guid, decimal, string>(pk, dutyRate, provProgRate));
					}
				}
				AssertEquals(2, reportList.Count);
				CombineAssertions(() =>
				{
					AssertEquals("ProvProgDuty for Line1", 750m, reportList[0].Item2);
					AssertEquals("ProvProgDutyRate for Line1", "15%", reportList[0].Item3);
					AssertEquals("ProvProgDuty for Line2", 1250m, reportList[1].Item2);
					AssertEquals("ProvProgDutyRate for Line2", "25%", reportList[1].Item3);
				});
			}
		}

		public void TestIRCTaxRate()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var cusEntryENSHeaderPK = CreateCusEntryHeader(declarationPK, "DestinationState=TN", "ENS", 1627.40f, 1);
			var cusEntryLinePK = CreateCusEntryLine(cusEntryENSHeaderPK, "DutyRateDesc=10.0%*HasMPF=Y*SupLine=Y", "99038501", 10.0f, 1);
			var invoiceLineAddInfo1 = "IsParent=Y*TaxApply=O*TaxCode=016*TaxQty=1616*TaxRate=3.566322*TaxRateS=$3.566322/PFL*TextileCategoryNo=636";
			var invoiceLineAddInfo2 = "TaxApply=O*TaxCode=017*TaxRate=777*TaxRateS=Specify*TextileCategoryNo=636";
			var invoiceLineAddInfo3 = "SecondarySPI=C*TaxApply=O*TaxCode=022*TaxRate=22.33*TaxRateS=CBMA Eligible*TextileCategoryNo=659";
			var invoiceLineAddInfo4 = "SecondarySPI=C*TaxApply=O*TaxCode=017*TaxRate=0.07*TaxRateS=W01010";
			var invoiceLine1 = CreateInvoiceLine(invoiceHeaderPK1, cusEntryLinePK, invoiceLineAddInfo1, 1);
			var invoiceLine2 = CreateInvoiceLine(invoiceHeaderPK1, cusEntryLinePK, invoiceLineAddInfo2, 1);

			var invoiceLine3 = CreateInvoiceLine(invoiceHeaderPK1, cusEntryLinePK, invoiceLineAddInfo3, 1);
			var invoiceLine4 = CreateInvoiceLine(invoiceHeaderPK1, cusEntryLinePK, invoiceLineAddInfo4, 1);
			var reportData = GetReportData(companyPK, importerPK).ToArray();
			AssertEquals(4, reportData.Length);

			var result1 = reportData.First(x => x.invoiceLinePK == invoiceLine1);
			AssertEquals("Code = 016, TaxRate = $3.566322/PFL", "$3.566322/PFL", result1.lineIRCTaxRate);

			var result2 = reportData.First(x => x.invoiceLinePK == invoiceLine2);
			AssertEquals("Code = 017, TaxRate = $777/L", "$777/L", result2.lineIRCTaxRate);

			var result3 = reportData.First(x => x.invoiceLinePK == invoiceLine3);
			AssertEquals("Code = 022, TaxRate = $22.33/L", "$22.33/L", result3.lineIRCTaxRate);

			var result4 = reportData.First(x => x.invoiceLinePK == invoiceLine4);
			AssertEquals("Code = 017, TaxRate = $0.07", "$0.07/L", result4.lineIRCTaxRate);
		}

		public void TestFTAData()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceLineAddInfo = "CustomsValue=10000*NonFTADuty=777*FTADuty=111.56*FTASPI=AU*OA_ForeignExporter=362fb76f-eccb-4031-8682-2e3fad01ee01*NonFTAPayableMPF=34.64*SupDuty=1234*FTAPayableMPF=14.54*SupTariff=99038501";

			var cusEntryENSHeaderPK = CreateCusEntryHeader(declarationPK, "DestinationState=TN", "ENS", 1627.40f, 1);

			var cusEntryLinePK = CreateCusEntryLine(cusEntryENSHeaderPK, "DutyRateDesc=10.0%*HasMPF=Y*SupLine=Y", "99038501", 10.0f, 1);
			var parentAddInfo = string.Format("ChildLineNum=1*CL_ParentLine={0}*DutyRateDesc=5.8%*HasMPF=Y", cusEntryLinePK);
			var cusentryLinePK2 = CreateCusEntryLine(cusEntryENSHeaderPK, parentAddInfo, "7607113000", 5.8f, 1);
			var invoiceLine = CreateInvoiceLine(invoiceHeaderPK1, cusentryLinePK2, invoiceLineAddInfo, 1);

			var reportData = GetReportData(companyPK, importerPK).ToArray();
			AssertEquals(1, reportData.Length);
			CombineAssertions(() =>
			{
				AssertEquals("invoiceLine", invoiceLine, reportData[0].invoiceLinePK);
				AssertEquals("FTADuty", 111.56m, reportData[0].ftaDuty);
				AssertEquals("FTAPayableMPF", 14.54m, reportData[0].ftaPayableMPF);
				AssertEquals("NonFTADuty", 777m, reportData[0].nonFtaDuty);
				AssertEquals("NonFTAPayableMPF", 34.64m, reportData[0].nonFtaPayableMPF);
			});
		}

		IEnumerable<(Guid invoiceLinePK, decimal ftaDuty, decimal ftaPayableMPF, string ftaSPI, decimal nonFtaDuty, decimal nonFtaPayableMPF, string lineIRCTaxRate)> GetReportData(Guid companyPK, Guid importerPK)
		{
			var reportSql = @"select JI_PK, FTADuty, FTAPayableMPF, FTASPI, NonFTADuty, NonFTAPayableMPF, LineIRCTaxRate from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<(Guid invoiceLinePK, decimal ftaDuty, decimal ftaPayableMPF, decimal nonFtaDuty, decimal nonFtaPayableMPF, string lineIRCTaxRate)>();

				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return GetData(reader);
					}
				}
			}
		}

		(Guid invoiceLinePK, decimal ftaDuty, decimal ftaPayableMPF, string ftaSPI, decimal nonFtaDuty, decimal nonFtaPayableMPF, string lineIRCTaxRate) GetData(IDataReader reader)
		{
			return ((Guid)reader["JI_PK"], (decimal)reader["FTADuty"], (decimal)reader["FTAPayableMPF"], (string)reader["FTASPI"], (decimal)reader["NonFTADuty"], (decimal)reader["NonFTAPayableMPF"], (string)reader["LineIRCTaxRate"]);
		}

		public Guid CreateJobDeclaration(Guid importerPK, Guid branchPK, Guid companyPK, int clusterKey, Guid? shipToPartyAddressPK = null, Guid? manufacturerAddressPK = null, Guid? exporterPK = null, string addInfo = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_OA_ShipToPartyAddress, JE_OA_ManufacturerAddress, JE_OH_Exporter, JE_ClusterKey, JE_AddInfo, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'B01TEST', 'ACE', 0, @shipToPartyAddressPK, @manufacturerAddressPK, @exporterPK, @clusterKey, @addInfo, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				if (shipToPartyAddressPK != null)
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, shipToPartyAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				if (manufacturerAddressPK != null)
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, manufacturerAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				if (exporterPK != null)
				{
					command.AddParameter("@exporterPK", SqlDbType.UniqueIdentifier, exporterPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@exporterPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		public Guid CreateInvoiceLine(Guid invoiceHeaderPK1, Guid cusEntryLinePK, string addInfo, int clusterKey)
		{
			var invoiceLinePK11 = Guid.NewGuid();

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_CL, JI_CustomsUnitQty, JI_ClusterKey)
	VALUES (@invoiceLinePK11, 'US', @invoiceHeaderPK1, 1, 1.00, @addInfo, '7607113000', @clPK, 'L', @clusterKey)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceLinePK11", SqlDbType.UniqueIdentifier, invoiceLinePK11);
				command.AddParameter("@clPK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
			return invoiceLinePK11;
		}

		public Guid CreateCusEntryHeader(Guid declarationPK, string addInfo, string messageType, float totalPaid, int clusterKey)
		{
			var cusEntryPK = Guid.NewGuid();
			var sqlCusEntry = @"
	INSERT INTO dbo.CusEntryHeader ([CH_PK], [CH_DataModel], [CH_IsValid], [CH_MessageType], [CH_TotalPaid], [CH_AddInfo], [CH_JE], [CH_ClusterKey], CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES (
		@ch_pk, 'US', 1, @messageType, @totalPaid, @addInfo, @jePk, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(sqlCusEntry))
			{
				command.AddParameter("@ch_pk", SqlDbType.UniqueIdentifier, cusEntryPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@totalPaid", SqlDbType.VarChar, totalPaid);
				command.AddParameter("@jePk", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
			return cusEntryPK;
		}

		public Guid CreateCusEntryLine(Guid clCH, string clAddInfo, string clAdValoremTariff, float clDutyPercent, int clusterKey)
		{
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.CusEntryLine([CL_PK], [CL_DataModel], [CL_IsValid], [CL_CH], [CL_AdValoremTariff], [CL_DutyPercent], [CL_AddInfo], [CL_ClusterKey], CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@clPk, 'US', 1, @clCh, @clAdValoremTariff, @clDutyPercent, @clAddInfo, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@clCh", SqlDbType.UniqueIdentifier, clCH);
				command.AddParameter("@clAdValoremTariff", SqlDbType.VarChar, clAdValoremTariff);
				command.AddParameter("@clDutyPercent", SqlDbType.Float, clDutyPercent);
				command.AddParameter("@clAddInfo", SqlDbType.VarChar, clAddInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			return result;
		}

		public void TestPGAExpeditedRelease()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = Guid.NewGuid();
			var declarationPK2 = Guid.NewGuid();

			var declarationSql = @"
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_ClusterKey) VALUES (@declarationPK1, 'US', 'IMP', @branchPK, @companyPK, 'JOB1', 'ACS', 0, 'PGAExpeditedRelease=Y', 1);
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey) VALUES (@declarationPK2, 'US', 'IMP', @branchPK, @companyPK, 'JOB2', 'ACS', 0, 2);";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK1, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK2, 2, dataModel: "US");

			var reportSql = @"select JE_DeclarationReference, PGAExpeditedRelease from USInvoiceLines(@companyPK,'','','','','','','', '','', '', '','','','','','','','','','','','','','',null,'','','Y')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var pgaExpeditedRelease = "";
					var job = "";
					while (reader.Read())
					{
						job = reader.GetString(0);
						pgaExpeditedRelease = reader.GetString(1);
					}
					CombineAssertions(() =>
					{
						AssertEquals("PGA Expedited Release", "JOB1", job);
						AssertEquals("PGA Expedited Release", "Y", pgaExpeditedRelease);
					});
				}
			}
		}

		public void TestReleaseDateAndFTZDate()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = Guid.NewGuid();
			var declarationPK2 = Guid.NewGuid();

			var declarationSql = @"
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_EntryAuthorisationDate, JE_ClusterKey) VALUES (@declarationPK1, 'US', 'IMP', @branchPK, @companyPK, 'JOB1', 'ACS', 0, 'PGAExpeditedRelease=Y', '2019-01-01', 1);
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_EntryAuthorisationDate, JE_ClusterKey) VALUES (@declarationPK2, 'US', 'FTZ', @branchPK, @companyPK, 'JOB2', 'ACS', 0, 'PGAExpeditedRelease=Y', '2019-02-02', 2);";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK1, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK2, 2, dataModel: "US");

			var reportSql = @"select ReleaseDate, FTZAdmissionDate from USInvoiceLines(@companyPK,'','','','','','','', '','', '', '','','','','','','','','','','','','','',null,'','','Y') WHERE JE_MessageType = @jE_MessageType";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@jE_MessageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var releaseDate = reader.GetValue(0);
						var fTZAdmissionDate = reader.GetValue(1);
						CombineAssertions(() =>
						{
							AssertEquals("ReleaseDate should has value when JE_MessageType is not FTZ", new DateTime(2019, 1, 1), releaseDate);
							AssertEquals("ReleaseDate should not has value when JE_MessageType is not FTZ", DBNull.Value, fTZAdmissionDate);
						});
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@jE_MessageType", SqlDbType.VarChar, "FTZ");

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var releaseDate = reader.GetValue(0);
						var fTZAdmissionDate = reader.GetValue(1);
						CombineAssertions(() =>
						{
							AssertEquals("ReleaseDate should not has value when JE_MessageType is FTZ", DBNull.Value, releaseDate);
							AssertEquals("ReleaseDate should has value when JE_MessageType is FTZ", new DateTime(2019, 2, 2), fTZAdmissionDate);
						});
					}
				}
			}
		}

		public void TestInvoiceLineEffectiveShipToPartyAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var org = TestDataCreator.CreateOrganisation("STP1", "SHIP TO PARTY");
			var address = TestDataCreator.CreateAddress(org, "ShipToParty Address", "Address 11111");

			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1, shipToPartyAddressPK: address);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var org2 = TestDataCreator.CreateOrganisation("STP2", "SHIP TO PARTY2");
			var address2 = TestDataCreator.CreateAddress(org2, "ShipToParty Address2", "Address 22222");

			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, shipToPartyAddressPK: address2, dataModel: "US");

			var reportSql = @"select JI_PK, InvoiceLineEffectiveShipToPartyAddress from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);

				using (var reader = command.ExecuteReader())
				{
					var invoiceLines = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						invoiceLines[reader.GetGuid(0)] = (Guid)reader["InvoiceLineEffectiveShipToPartyAddress"];
					}
					CombineAssertions(() =>
					{
						AssertEquals("ShipToPartyAddress fall back", address, invoiceLines[invoiceLinePK1]);
						AssertEquals("ShipToPartyAddress on Invoice Line", address2, invoiceLines[invoiceLinePK2]);
					});
				}
			}
		}

		public void TestInvoiceLineEffectiveManufacturerAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var org = TestDataCreator.CreateOrganisation("MAN1", "MANUFACTURER1");
			var address = TestDataCreator.CreateAddress(org, "Manufacturer Address", "Address 11111");

			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1, manufacturerAddressPK: address);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var org2 = TestDataCreator.CreateOrganisation("MAN2", "MANUFACTURER2");
			var address2 = TestDataCreator.CreateAddress(org2, "Manufacturer Address2", "Address 22222");

			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, manufacturerAddressPK: address2, dataModel: "US");

			var reportSql = @"select JI_PK, InvoiceLineEffectiveManufacturerAddress from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);

				using (var reader = command.ExecuteReader())
				{
					var invoiceLines = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						invoiceLines[reader.GetGuid(0)] = (Guid)reader["InvoiceLineEffectiveManufacturerAddress"];
					}
					CombineAssertions(() =>
					{
						AssertEquals("ManufacturerAddress fall back", address, invoiceLines[invoiceLinePK1]);
						AssertEquals("ManufacturerAddress on Invoice Line", address2, invoiceLines[invoiceLinePK2]);
					});
				}
			}
		}

		public void TestInvoiceLineEffectiveExporterAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var org = TestDataCreator.CreateOrganisation("FEXP", "EXPORTER");
			var address = TestDataCreator.CreateAddress(org, "Exporter Address", "Address 11111");
			TestDataCreator.CreateOrgAddressCapability(address, "OFC", true);
			var address2 = TestDataCreator.CreateAddress(org, "Exporter Address2", "Address 22222");
			TestDataCreator.CreateOrgAddressCapability(address2, "OFC", false);
			var orgCusCode = TestDataCreator.CreateOrgCusCode(org, "MID", "USMIDTEST", "US", address2);

			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1, exporterPK: org);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, dataModel: "US");

			var reportSql = @"select JI_PK, InvoiceLineExporterPK, InvoiceLineExporterMID from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);

				using (var reader = command.ExecuteReader())
				{
					var invoiceLines = new Dictionary<Guid, Guid>();
					var invoiceLineExporterMID = string.Empty;
					while (reader.Read())
					{
						invoiceLines[reader.GetGuid(0)] = (Guid)reader["InvoiceLineExporterPK"];
						invoiceLineExporterMID = (string)reader["InvoiceLineExporterMID"];
					}
					AssertEquals("InvoiceLineExporterPK", org, invoiceLines[invoiceLinePK1]);
					AssertEquals("InvoiceLineExporterMID", "USMIDTEST", invoiceLineExporterMID);
				}
			}
		}

		public void TestFTZNo()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, addInfo: "FTZNo=123456", dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK1, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK2, 2, dataModel: "US");

			var sql = @"select JI_PK, FTZNo from USInvoiceLines(@companyPK,'','','','','','','', '','', '', '','','','','','','','','','','','','','',null,'','','')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, object>();
					while (reader.Read())
					{
						data.Add(reader.GetGuid(0), reader.GetValue(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("invoiceLinePK1", "123456", data[invoiceLinePK1].ToString());
						AssertEquals("invoiceLinePK2", string.Empty, data[invoiceLinePK2]);
					});
				}
			}
		}

		public void TestLineEnteredValueOnXInvoiceLine()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK1 = Guid.NewGuid();
			var invoiceLinePK2 = Guid.NewGuid();
			var invoiceLinePK3 = Guid.NewGuid();
			var invoiceLineSql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ClusterKey) VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK, 1, 100, 'CustomsValue=5000*SetInd=X*IsParent=Y*SupTariff=99038815', '4818900080', 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ParentID, JI_ClusterKey) VALUES (@invoiceLinePK2, 'US', @invoiceHeaderPK, 2, 100, 'CustomsValue=2000*SetInd=V*SupTariff=99038815', '4818900080', @parentID, 1);
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_Tariff, JI_ParentID, JI_ClusterKey) VALUES (@invoiceLinePK3, 'US', @invoiceHeaderPK, 3, 100, 'CustomsValue=3000*SetInd=V*SupTariff=99030124', '2853909090', @parentID, 1);";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@invoiceLinePK3", SqlDbType.UniqueIdentifier, invoiceLinePK3);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select JI_PK, EnteredValue from USInvoiceLines(@companyPK,'','','','','','','','','','','','','','','','','','','','','','','','', @importerPK,'', '', '') order by JI_LineNo";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<(Guid InvoiceLinePK, decimal LineEnteredValue)>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						reportList.Add(((Guid)reader["JI_PK"], (decimal)reader["EnteredValue"]));
					}
				}
				AssertEquals(3, reportList.Count);
				CombineAssertions(() =>
				{
					AssertEquals("Line Entered Value on X line", 0m, reportList[0].LineEnteredValue);
					AssertEquals("Line Entered Value on V parent line", 2000m, reportList[1].LineEnteredValue);
					AssertEquals("Line Entered Value on V child line", 3000m, reportList[2].LineEnteredValue);
				});
			}
		}
	}
}
