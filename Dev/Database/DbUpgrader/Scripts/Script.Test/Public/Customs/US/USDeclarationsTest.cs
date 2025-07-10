using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USDeclarations))]
	class USDeclarationsTest : DbCreateScriptTest
	{
		public void TestInvoiceCount()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, 1);
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, true, 1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, 2);
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, 2);
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, true, 2);
			const string sql = @"select JE_PK, InvoiceCount from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, int>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetInt32(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 1, result[declarationPK1]);
						AssertEquals("declarationPK2", 2, result[declarationPK2]);
					});
				}
			}
		}

		public void TestInvoiceLineCount()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, dataModel: "US");
			var dec2Invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, dataModel: "US");
			const string sql = @"select JE_PK, InvoiceLineCount from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, int>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetInt32(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 1, result[declaration1PK]);
						AssertEquals("declarationPK2", 5, result[declaration2PK]);
					});
				}
			}
		}

		public void TestSupplementaryTariffCount()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "SupTariff=9802008068", dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "SupTariff=9802008068", dataModel: "US");
			var dec2Invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, addInfo: "SupTariff=9802008068", dataModel: "US");
			const string sql = @"select JE_PK, SupplementaryTariffCount from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, int>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetInt32(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 1, result[declaration1PK]);
						AssertEquals("declarationPK2", 2, result[declaration2PK]);
					});
				}
			}
		}

		public void TestSupplementaryTariffCountWithAdditionalTariffs()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1, dataModel: "US");
			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "SupTariff=9802008068", dataModel: "US");
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "SupTariff=9802008068", dataModel: "US");
			var invoiceLinePK3 = TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "SupTariff=9802008068", dataModel: "US");
			var invoiceLinePK4 = TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "SupTariff=9802008068", dataModel: "US");
			var invoiceLinePK5 = TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "SupTariff=N/A", dataModel: "US");

			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '9802008068', 'AT1', 'SupDuty=200', 10, 'GK', 110, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038805', 'AT2', 'SupDuty=150', 20, 'TNE', 120, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK2, 'JI', '99030121', 'AT1', '', 0, 'ME', 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK3, 'JI', '99030122', 'AT1', '', 0, '', 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK4, 'JI', '99030123', 'AT1', '', 0, 'KG', 0, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK4, 'JI', 'N/A', 'AT2', '', 0, 'KG', 0, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_UQ1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK5, 'JI', 'N/A', 'AT1', '', 0, 'KG', 0, 'US');
";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceLinePK2", SqlDbType.UniqueIdentifier, invoiceLinePK2);
				command.AddParameter("@invoiceLinePK3", SqlDbType.UniqueIdentifier, invoiceLinePK3);
				command.AddParameter("@invoiceLinePK4", SqlDbType.UniqueIdentifier, invoiceLinePK4);
				command.AddParameter("@invoiceLinePK5", SqlDbType.UniqueIdentifier, invoiceLinePK5);
				command.ExecuteNonQuery();
			}

			const string sql = @"select JE_PK, SupplementaryTariffCount from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, int>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetInt32(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 9, result[declaration1PK]);
				});
				}
			}
		}

		public void TestEntryCharges()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, "ENS", dataModel: "US");
			TestDataCreator.CreateCusEntryHeaderCharges(dec1Entry1PK, "017", 1.7m, 1);
			TestDataCreator.CreateCusEntryHeaderCharges(dec1Entry1PK, "018", 1.8m, 1);
			TestDataCreator.CreateCusEntryHeaderCharges(dec1Entry1PK, "016", 1.6m, 1);
			TestDataCreator.CreateCusEntryHeaderCharges(dec1Entry1PK, "022", 2.2m, 1);
			TestDataCreator.CreateCusEntryHeaderCharges(dec1Entry1PK, "501", 5.01m, 1);
			TestDataCreator.CreateCusEntryHeaderCharges(dec1Entry1PK, "499", 4.99m, 1);
			TestDataCreator.CreateCusEntryHeaderCharges(dec1Entry1PK, "ARS", 3.19m, 1);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "FTZ", dataModel: "US");
			TestDataCreator.CreateCusEntryHeaderCharges(dec2Entry1PK, "017", 1.7m, 2);
			TestDataCreator.CreateCusEntryHeaderCharges(dec2Entry1PK, "018", 1.8m, 2);
			TestDataCreator.CreateCusEntryHeaderCharges(dec2Entry1PK, "016", 1.6m, 2);
			TestDataCreator.CreateCusEntryHeaderCharges(dec2Entry1PK, "022", 2.2m, 2);
			TestDataCreator.CreateCusEntryHeaderCharges(dec2Entry1PK, "501", 5.01m, 2);
			TestDataCreator.CreateCusEntryHeaderCharges(dec2Entry1PK, "499", 4.99m, 2);
			TestDataCreator.CreateCusEntryHeaderCharges(dec2Entry1PK, "ARS", 3.19m, 2);

			const string sql = @"select JE_PK, IRTaxAmount, MPF, HMF, OtherFees, TotalFees from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (decimal IRTaxAmount, decimal MPF, decimal HMF, decimal OtherFees, decimal TotalFees)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], (reader.GetDecimal(1), reader.GetDecimal(2), reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", (7.3m, 4.99m, 5.01m, 3.19m, 13.19m), result[declaration1PK]);
						AssertEquals("declarationPK2", (7.3m, 4.99m, 5.01m, 3.19m, 13.19m), result[declaration2PK]);
					});
				}
			}
		}

		public void TestTotalEnteredValue_JE_AddInfoHasNoTotalEnteredValue()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, dataModel: "US");
			var dec1Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec1Entry1PK, 1);
			var dec1Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "CustomsValue=1", entryLinePK: dec1Ent1Line1PK, dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "ENS", dataModel: "US");
			var dec2Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec2Entry1PK, 2);
			var dec2Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "CustomsValue=2*SetInd=X", entryLinePK: dec2Ent1Line1PK, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "CustomsValue=3*SecondarySPI=X", entryLinePK: dec2Ent1Line1PK, dataModel: "US");
			var dec2Invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, addInfo: "CustomsValue=4.1*SetInd=V", entryLinePK: dec2Ent1Line1PK, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, addInfo: "CustomsValue=5.2*SecondarySPI=V", entryLinePK: dec2Ent1Line1PK, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, addInfo: "CustomsValue=6.3", entryLinePK: dec2Ent1Line1PK, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice2PK, 2, entryLinePK: dec2Ent1Line1PK, dataModel: "US");
			const string sql = @"select JE_PK, TotalEnteredValue from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetDecimal(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 0m, result[declaration1PK]);
						AssertEquals("declarationPK2", 15.0m, result[declaration2PK]);
					});
				}
			}
		}

		public void TestCustomsDisbursements_TotalDuty()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, dataModel: "US");
			var dec1Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec1Entry1PK, 1);
			TestDataCreator.CreateCusEntryLineFee(dec1Ent1Line1PK, "DTY", 1.1f, 1);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "ENS", dataModel: "US");
			var dec2Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec2Entry1PK, 2);
			TestDataCreator.CreateCusEntryLineFee(dec2Ent1Line1PK, "DTY", 2.2f, 2);
			const string sql = @"select JE_PK, TotalDuty from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetDecimal(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 0m, result[declaration1PK]);
						AssertEquals("declarationPK2", 2.2m, result[declaration2PK]);
					});
				}
			}
		}

		public void TestCustomsDisbursements_AntiDumping()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, dataModel: "US");
			var dec1Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec1Entry1PK, 1);
			TestDataCreator.CreateCusEntryLineFee(dec1Ent1Line1PK, "ADD", 1.1f, 1);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "ENS", dataModel: "US");
			var dec2Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec2Entry1PK, 2);
			TestDataCreator.CreateCusEntryLineFee(dec2Ent1Line1PK, "ADD", 2.2f, 2);
			const string sql = @"select JE_PK, AntiDumping from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetDecimal(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 0m, result[declaration1PK]);
						AssertEquals("declarationPK2", 2.2m, result[declaration2PK]);
					});
				}
			}
		}

		public void TestCustomsDisbursements_CounterValing()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var dec1Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration1PK, 1, dataModel: "US");
			var dec1Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec1Entry1PK, 1);
			TestDataCreator.CreateCusEntryLineFee(dec1Ent1Line1PK, "CVD", 1.1f, 1);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var dec2Entry1PK = TestDataCreator.CreateCusEntryHeader(declaration2PK, 2, "ENS", dataModel: "US");
			var dec2Ent1Line1PK = TestDataCreator.CreateCusEntryLine(dec2Entry1PK, 2);
			TestDataCreator.CreateCusEntryLineFee(dec2Ent1Line1PK, "CVD", 2.2f, 2);
			const string sql = @"select JE_PK, CounterValing from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetDecimal(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 0m, result[declaration1PK]);
						AssertEquals("declarationPK2", 2.2m, result[declaration2PK]);
					});
				}
			}
		}

		public void TestBilledAmount()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var departmentPK = TestDataCreator.CreateOrExistingDepartment("TC1");

			var accTaxRate = TestDataCreator.CreateAccTaxRate("GST", "Standard Rated", "RAT", "US");

			var disbursementCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges-GST", "DSB");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "B002", "00112233", "WRK");
			var accTransHeaderPK = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, departmentPK, jobHeaderPK, "AR", "INV", "B055", 20.0m, 3.25m, 28.25m, 1.0m, 28.25m);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 1, "Customs Disbursement Charges", "REV", 4.0m, chargePK: disbursementCharge);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 2, "Customs Disbursement Charges", "REV", 5.0m, chargePK: disbursementCharge);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 3, "Customs Disbursement Charges", "REV", 5.0m);

			const string sql = @"select JE_PK, BilledAmount from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetDecimal(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", 0m, result[declaration1PK]);
						AssertEquals("declarationPK2", 9m, result[declaration2PK]);
					});
				}
			}
		}

		public void TestBillDispositionDates()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration1PK, 1, "51/52/53");
			var declaration1Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration1PK, 1, "51/52/53");
			var declaration1Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration1PK, 1, "51/52/53");
			TestDataCreator.CreateCusAddInfo("UDP", "Code=51*DispositionDate=2018-08-29 13:51:00.000*Order=1*Source=SO", "CU", declaration1Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=52*DispositionDate=2018-08-29 13:52:00.000*Order=1*Source=SO", "CU", declaration1Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=53*DispositionDate=2018-08-29 13:53:00.000*Order=1*Source=SO", "CU", declaration1Bill3);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var declaration2Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration2PK, 2, "1G/1H/2P");
			var declaration2Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration2PK, 2, "1G/1H/2P");
			var declaration2Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration2PK, 2, "1G/1H/2P");
			TestDataCreator.CreateCusAddInfo("UDP", "Code=54*DispositionDate=2018-08-29 13:54:00.000*Order=1*Source=SO", "CU", declaration2Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=55*DispositionDate=2018-08-29 13:55:00.000*Order=1*Source=SO", "CU", declaration2Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=56*DispositionDate=2018-08-29 13:56:00.000*Order=1*Source=SO", "CU", declaration2Bill3);

			const string sql = @"select JE_PK, ManifestHoldCBPDate, ManifestHoldAgricultureDate, ManifestHoldOtherAgencyDate, ManifestHoldRemovedCBPDate, ManifestHoldRemovedAgricultureDate, ManifestHoldRemovedOtherAgencyDate from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, BillDispositionDates>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], new BillDispositionDates
						{
							ManifestHoldCBPDate = reader["ManifestHoldCBPDate"] == DBNull.Value ? null : reader.GetDateTime(1),
							ManifestHoldAgricultureDate = reader["ManifestHoldAgricultureDate"] == DBNull.Value ? null : reader.GetDateTime(2),
							ManifestHoldOtherAgencyDate = reader["ManifestHoldOtherAgencyDate"] == DBNull.Value ? null : reader.GetDateTime(3),
							ManifestHoldRemovedCBPDate = reader["ManifestHoldRemovedCBPDate"] == DBNull.Value ? null : reader.GetDateTime(4),
							ManifestHoldRemovedAgricultureDate = reader["ManifestHoldRemovedAgricultureDate"] == DBNull.Value ? null : reader.GetDateTime(5),
							ManifestHoldRemovedOtherAgencyDate =  reader["ManifestHoldRemovedOtherAgencyDate"] == DBNull.Value ? null : reader.GetDateTime(6),
						});
					}
					CombineAssertions(() =>
					{
						AssertBillDispositionDates("declarationPK1", new BillDispositionDates
						{
							ManifestHoldCBPDate = new DateTime(2018, 8, 29, 13, 51, 0),
							ManifestHoldAgricultureDate = new DateTime(2018, 8, 29, 13, 52, 0),
							ManifestHoldOtherAgencyDate = new DateTime(2018, 8, 29, 13, 53, 0),
							ManifestHoldRemovedCBPDate = null,
							ManifestHoldRemovedAgricultureDate = null,
							ManifestHoldRemovedOtherAgencyDate = null,
						}, result[declaration1PK]);
						AssertBillDispositionDates("declarationPK2", new BillDispositionDates
						{
							ManifestHoldCBPDate = null,
							ManifestHoldAgricultureDate = null,
							ManifestHoldOtherAgencyDate = null,
							ManifestHoldRemovedCBPDate = new DateTime(2018, 8, 29, 13, 54, 0),
							ManifestHoldRemovedAgricultureDate = new DateTime(2018, 8, 29, 13, 55, 0),
							ManifestHoldRemovedOtherAgencyDate = new DateTime(2018, 8, 29, 13, 56, 0),
						}, result[declaration2PK]);
					});
				}
			}
		}

		static void AssertBillDispositionDates(string message, BillDispositionDates current, BillDispositionDates expected)
		{
			AssertEquals($"{message}.ManifestHoldCBPDate", expected.ManifestHoldCBPDate, current.ManifestHoldCBPDate);
			AssertEquals($"{message}.ManifestHoldAgricultureDate", expected.ManifestHoldAgricultureDate, current.ManifestHoldAgricultureDate);
			AssertEquals($"{message}.ManifestHoldOtherAgencyDate", expected.ManifestHoldOtherAgencyDate, current.ManifestHoldOtherAgencyDate);
			AssertEquals($"{message}.ManifestHoldRemovedCBPDate", expected.ManifestHoldRemovedCBPDate, current.ManifestHoldRemovedCBPDate);
			AssertEquals($"{message}.ManifestHoldRemovedAgricultureDate", expected.ManifestHoldRemovedAgricultureDate, current.ManifestHoldRemovedAgricultureDate);
			AssertEquals($"{message}.ManifestHoldRemovedOtherAgencyDate", expected.ManifestHoldRemovedOtherAgencyDate, current.ManifestHoldRemovedOtherAgencyDate);
		}

		struct BillDispositionDates
		{
			public DateTime? ManifestHoldCBPDate;
			public DateTime? ManifestHoldAgricultureDate;
			public DateTime? ManifestHoldOtherAgencyDate;
			public DateTime? ManifestHoldRemovedCBPDate;
			public DateTime? ManifestHoldRemovedAgricultureDate;
			public DateTime? ManifestHoldRemovedOtherAgencyDate;
		}

		public void TestNoteData_Comments()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateStmNote("JobDeclaration", declaration1PK, "Note1", "Client Visible Job Notes");
			TestDataCreator.CreateStmNote("JobDeclaration", declaration1PK, "Note2", "def");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			TestDataCreator.CreateStmNote("JobDeclaration", declaration2PK, "Note3", "def");
			TestDataCreator.CreateStmNote("JobDeclaration", declaration2PK, "Note3", "Client Visible Job Notes");

			const string sql = @"select JE_PK, Cast(Comments as varchar(max)) from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", "Note1", result[declaration1PK]);
						AssertEquals("declarationPK2", "Note3", result[declaration2PK]);
					});
				}
			}
		}

		public void TestDecContainer()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var refContainer1PK = TestDataCreator.CreateRefContainer("RC1");
			var refContainer2PK = TestDataCreator.CreateRefContainer("RC2");
			var jobContainer1PK = TestDataCreator.CreateJobContainer("JC1", refContainer1PK);
			var jobContainer2PK = TestDataCreator.CreateJobContainer("JC2", refContainer2PK);
			var jobContainer3PK = TestDataCreator.CreateJobContainer("JC3", null);

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateCusContainer("CNT1", declaration1PK, 1, "US", jobContainer1PK);
			TestDataCreator.CreateCusContainer("CNT2", declaration1PK, 1, "US", jobContainer1PK);
			TestDataCreator.CreateCusContainer("CNT3", declaration1PK, 1, "US", jobContainer2PK);
			TestDataCreator.CreateCusContainer("CNT4", declaration1PK, 1, "US", jobContainer3PK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			TestDataCreator.CreateCusContainer("CNT5", declaration2PK, 2, "US", jobContainer1PK);
			TestDataCreator.CreateCusContainer("CNT6", declaration2PK, 2, "US", jobContainer1PK);
			TestDataCreator.CreateCusContainer("CNT7", declaration2PK, 2, "US", jobContainer2PK);
			TestDataCreator.CreateCusContainer("CNT8", declaration2PK, 2, "US", jobContainer3PK);

			const string sql = @"select JE_PK, ContainersCount, ContainerNumbers from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (int ContainersCount, string ContainerNumbers)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], (reader.GetInt32(1), reader.GetString(2)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", (3, "JC1 (RC1), JC2 (RC2), JC3"), result[declaration1PK]);
						AssertEquals("declarationPK2", (3, "JC1 (RC1), JC2 (RC2), JC3"), result[declaration2PK]);
					});
				}
			}
		}

		public void TestCountryOfOriginFromLineLevel()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, addInfo: "UC_NKCountryOfOrigin=AU", dataModel: "US");
			var dec1Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1, addInfo: "UC_NKCountryOfOrigin=TW", dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "UC_NKCountryOfOrigin=US", dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, addInfo: "UC_NKCountryOfOrigin=AU", dataModel: "US");
			var dec2Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, addInfo: "UC_NKCountryOfOrigin=TW", dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "UC_NKCountryOfOrigin=CA", dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "UC_NKCountryOfOrigin=US", dataModel: "US");
			const string sql = @"select JE_PK, CountryOfOriginFromLineLevel from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", "US", result[declaration1PK]);
						AssertEquals("declarationPK2", "CA, US", result[declaration2PK]);
					});
				}
			}
		}

		public void TestCountryOfExportFromLineLevel()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, addInfo: "UC_NKCountryOfExport=AU", dataModel: "US");
			var dec1Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration1PK, false, 1, addInfo: "UC_NKCountryOfExport=TW", dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec1Invoice1PK, 1, addInfo: "UC_NKCountryOfExport=US", dataModel: "US");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, addInfo: "UC_NKCountryOfExport=AU", dataModel: "US");
			var dec2Invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declaration2PK, false, 2, addInfo: "UC_NKCountryOfExport=TW", dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "UC_NKCountryOfExport=CA", dataModel: "US");
			TestDataCreator.CreateJobComInvoiceLine(dec2Invoice1PK, 2, addInfo: "UC_NKCountryOfExport=US", dataModel: "US");
			const string sql = @"select JE_PK, CountryOfExportFromLineLevel from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", "US", result[declaration1PK]);
						AssertEquals("declarationPK2", "CA, US", result[declaration2PK]);
					});
				}
			}
		}

		public void TestSEBillStatusWithDescription()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declaration1Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration1PK, 1, "");
			TestDataCreator.CreateCusAddInfo("UDP", "Code=91*DispositionDate=2018-08-29 13:45:00.000*Order=1*Source=SO", "CU", declaration1Bill1);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var declaration2Bill1 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration2PK, 2, "1G/1H/2P");
			var declaration2Bill2 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration2PK, 2, "1G/1H/2P");
			var declaration2Bill3 = TestDataCreator.CreateCusDecHouseBill(true, "", declaration2PK, 2, "1G/1H/2P");
			TestDataCreator.CreateCusAddInfo("UDP", "Code=91*DispositionDate=2018-08-29 13:45:00.000*Order=1*Source=SO", "CU", declaration2Bill1);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=91*DispositionDate=2018-08-29 14:32:00.000*Order=2*Source=SO", "CU", declaration2Bill2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=54*DispositionDate=2018-08-29 14:32:00.000*Order=3*Source=SO", "CU", declaration2Bill3);

			const string sql = @"select JE_PK, SEBillStatus, SEBillStatusDescription from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (string SEBillStatus, string SEBillStatusDescription)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], (reader.GetString(1), reader.GetString(2)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("declarationPK1", ("91", "91-NO BILL MATCH"), result[declaration1PK]);
						AssertEquals("declarationPK2", ("54,91", "54-CBP MANIFEST HOLD REMOVED,91-NO BILL MATCH"), result[declaration2PK]);
					});
				}
			}
		}

		class ImportDeliveryAddress
		{
			public string DeliverToName { get; set; }
			public string DeliverToOrgCode { get; set; }
		}

		public void TestImporterDeliveryAddressWithCompanyNameOverride()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000", "IMP", 1, dataModel: "US");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "SSSS", "Delivery Address", "Address 2", "Vancouver", "BC", "L8K 0A1", "Deliver To Toronto");
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum0", "ENS", "CUS", "US");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMG");
			var sql = @"select JE_PK, DeliverToName from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var addressePKs = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						addressePKs.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					AssertEquals("Deliver To Toronto", addressePKs[declarationPK]);
				}
			}
		}

		public void TestJE_OA_ShipToPartyAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "Delivery Address", "Address 1");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000", "IMP", 1, shipToPartyAddressPK: addressPK, dataModel: "US");
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum0", "ENS", "CUS", "US");

			var sql = @"select JE_PK, JE_OA_ShipToPartyAddress from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var addressePKs = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						addressePKs.Add((Guid)reader["JE_PK"], (Guid)reader["JE_OA_ShipToPartyAddress"]);
					}
					AssertEquals(addressPK, addressePKs[declarationPK]);
				}
			}
		}

		public void TestHMFOnFTZJob()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "S001", "IMP", 1, dataModel: "US");
			TestDataCreator.CreateCusEntryNum(declarationPK1, "JobDeclaration", "0260023|18|", "FTZ", "CUS", "US");

			var cusEntryHeader1PK = Guid.NewGuid();
			var cusEntryHeaderSQL =
@"insert into dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) values (@cusEntryHeader1PK, 'US', @declarationPK, 'ENS', 1, getutcdate(), '~BP', getutcdate(), '~BP')
insert into dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeAmount, C1_ChargeType, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser) values (newid(), @cusEntryHeader1PK, 125.00, '501', 1, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(cusEntryHeaderSQL))
			{
				command.AddParameter("@cusEntryHeader1PK", SqlDbType.UniqueIdentifier, cusEntryHeader1PK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK1);

				command.ExecuteNonQuery();
			}

			var sql = @"select JE_PK, HMF from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						data.Add((Guid)reader["JE_PK"], (decimal)reader["HMF"]);
					}
					AssertEquals(((decimal)(125.00)), data[declarationPK1]);
				}
			}
		}

		public void TestImportDeliveryAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var shipmentPK1 = TestDataCreator.CreateShipment("S001");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "S001", "IMP", 1, shipmentPK1, dataModel: "US");
			TestDataCreator.CreateCusEntryNum(declarationPK1, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");
			TestDataCreator.CreateDocAddress(Guid.Empty, "Company Name 1", shipmentPK1, "JS", "CEG");

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			TestDataCreator.CreateCusEntryNum(declarationPK2, "JobDeclaration", "EntryNum2", "ENS", "CUS", "US");
			TestDataCreator.CreateDocAddress(Guid.Empty, "Company Name 2", declarationPK2, "JE", "IMG");

			var organisationPK3 = TestDataCreator.CreateOrganisation("OrgCode3", "Company Name 3");
			var addressPK3 = TestDataCreator.CreateAddress(organisationPK3, "Delivery Address", "Address 1");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "US");
			TestDataCreator.CreateCusEntryNum(declarationPK3, "JobDeclaration", "EntryNum3", "ENS", "CUS", "US");
			TestDataCreator.CreateDocAddress(addressPK3, "", declarationPK3, "JE", "IMG");

			var sql = @"select JE_PK, DeliverToName, DeliverToOrgCode from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var addresses = new Dictionary<Guid, ImportDeliveryAddress>();
					while (reader.Read())
					{
						addresses[reader.GetGuid(0)] = new ImportDeliveryAddress()
						{
							DeliverToName = reader.GetString(1),
							DeliverToOrgCode = reader.GetString(2),
						};
					}
					AssertContainsExactElementsInAnyOrder(new[] { declarationPK1, declarationPK2, declarationPK3 }, addresses.Keys);
					AssertEquals("Company Name 1", addresses[declarationPK1].DeliverToName);
					AssertEquals("", addresses[declarationPK1].DeliverToOrgCode);
					AssertEquals("Company Name 2", addresses[declarationPK2].DeliverToName);
					AssertEquals("", addresses[declarationPK2].DeliverToOrgCode);
					AssertEquals("Company Name 3", addresses[declarationPK3].DeliverToName);
					AssertEquals("OrgCode3", addresses[declarationPK3].DeliverToOrgCode);
				}
			}
		}

		public void TestTotalEnteredValue()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationPK = Guid.NewGuid();
			var declarationPK2 = Guid.NewGuid();
			var declarationRef1 = Guid.NewGuid().ToString("n");
			var declarationRef2 = Guid.NewGuid().ToString("n");

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, @declarationReference1, 'ACS', 0, 1)
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_ClusterKey)
VALUES (@declarationPK2, 'US', 'IMP', @branchPK, @companyPK, @importerPK, @declarationReference2, 'ACS', 0, 'TotalEnteredValue=12580', 2)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@declarationReference1", SqlDbType.VarChar, declarationRef1);
				command.AddParameter("@declarationReference2", SqlDbType.VarChar, declarationRef2);
				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK11 = Guid.NewGuid();
			var invoiceLinePK12 = Guid.NewGuid();
			var invoiceLinePK13 = Guid.NewGuid();

			var invoiceLinePK21 = Guid.NewGuid();
			var invoiceLinePK22 = Guid.NewGuid();
			var invoiceLinePK23 = Guid.NewGuid();
			var invoiceLinePK24 = Guid.NewGuid();

			var cusEntryLinePK1 = Guid.NewGuid();
			var cusEntryLinePK2 = Guid.NewGuid();
			var cusEntryLinePK3 = Guid.NewGuid();

			var cusEntryLinePK4 = Guid.NewGuid();
			var cusEntryLinePK5 = Guid.NewGuid();
			var cusEntryLinePK6 = Guid.NewGuid();
			var cusEntryLinePK7 = Guid.NewGuid();

			var cusEntryHeader1PK = Guid.NewGuid();

			var cusEntryHeaderSQL = @"insert into dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) values (@cusEntryHeader1PK, 'US', @declarationPK, 'ENS', 1, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(cusEntryHeaderSQL))
			{
				command.AddParameter("@cusEntryHeader1PK", SqlDbType.UniqueIdentifier, cusEntryHeader1PK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);

				command.ExecuteNonQuery();
			}

			var cusEntryLineSql = string.Format(@"
INSERT INTO dbo.CusEntryLine(CL_PK, CL_DataModel, CL_CH, CL_CustomsValue, CL_AddInfo, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES
(@cusEntryLinePK1, 'US', @cusEntryHeaderPK1, 1.00, '', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@cusEntryLinePK2, 'US', @cusEntryHeaderPK1, 2.00, '', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@cusEntryLinePK3, 'US', @cusEntryHeaderPK1, 3.00, '', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@cusEntryLinePK4, 'US', @cusEntryHeaderPK1, 4.00, '', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@cusEntryLinePK5, 'US', @cusEntryHeaderPK1, 13.00, '', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@cusEntryLinePK6, 'US', @cusEntryHeaderPK1, 6.00, 'ChildLineNum=1*CL_ParentLine={0}', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@cusEntryLinePK7, 'US', @cusEntryHeaderPK1, 7.00, 'ChildLineNum=2*CL_ParentLine={0}', 1, getutcdate(), '~BP', getutcdate(), '~BP');
", cusEntryLinePK5.ToString());

			using (var command = Db.Connection.Command(cusEntryLineSql))
			{
				command.AddParameter("@cusEntryHeaderPK1", SqlDbType.UniqueIdentifier, cusEntryHeader1PK);
				command.AddParameter("@cusEntryLinePK1", SqlDbType.UniqueIdentifier, cusEntryLinePK1);
				command.AddParameter("@cusEntryLinePK2", SqlDbType.UniqueIdentifier, cusEntryLinePK2);
				command.AddParameter("@cusEntryLinePK3", SqlDbType.UniqueIdentifier, cusEntryLinePK3);
				command.AddParameter("@cusEntryLinePK4", SqlDbType.UniqueIdentifier, cusEntryLinePK4);
				command.AddParameter("@cusEntryLinePK5", SqlDbType.UniqueIdentifier, cusEntryLinePK5);
				command.AddParameter("@cusEntryLinePK6", SqlDbType.UniqueIdentifier, cusEntryLinePK6);
				command.AddParameter("@cusEntryLinePK7", SqlDbType.UniqueIdentifier, cusEntryLinePK7);

				command.ExecuteNonQuery();
			}

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_CL, JI_ClusterKey)
VALUES (@invoiceLinePK11, 'US', @invoiceHeaderPK1, 1, 1.00, 'CustomsValue=1', @cusEntryLinePK1, 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_CL, JI_ClusterKey)
VALUES (@invoiceLinePK12, 'US', @invoiceHeaderPK1, 2, 2.00, 'CustomsValue=2', @cusEntryLinePK2, 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_CL, JI_ClusterKey)
VALUES( @invoiceLinePK13, 'US', @invoiceHeaderPK1, 3, 3.00, 'CustomsValue=3', @cusEntryLinePK3, 1)

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_CL, JI_ClusterKey)
VALUES (@invoiceLinePK21, 'US', @invoiceHeaderPK2, 1, 4.00, 'CustomsValue=4', @cusEntryLinePK4, 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_CL, JI_ClusterKey)
VALUES (@invoiceLinePK22, 'US', @invoiceHeaderPK2, 2, 5.00, 'CustomsValue=13*IsParent=Y*SecondarySPI=X', @cusEntryLinePK5, 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ParentID, JI_CL, JI_ClusterKey)
VALUES (@invoiceLinePK23, 'US', @invoiceHeaderPK2, 3, 6.00, 'CustomsValue=6*SecondarySPI=V', @invoiceLinePK22, @cusEntryLinePK6, 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ParentID, JI_CL, JI_ClusterKey)
VALUES (@invoiceLinePK24, 'US', @invoiceHeaderPK2, 4, 7.33, 'CustomsValue=7.33*SecondarySPI=V', @invoiceLinePK22, @cusEntryLinePK7, 1)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceHeaderPK2", SqlDbType.UniqueIdentifier, invoiceHeaderPK2);

				command.AddParameter("@invoiceLinePK11", SqlDbType.UniqueIdentifier, invoiceLinePK11);
				command.AddParameter("@invoiceLinePK12", SqlDbType.UniqueIdentifier, invoiceLinePK12);
				command.AddParameter("@invoiceLinePK13", SqlDbType.UniqueIdentifier, invoiceLinePK13);

				command.AddParameter("@invoiceLinePK21", SqlDbType.UniqueIdentifier, invoiceLinePK21);
				command.AddParameter("@invoiceLinePK22", SqlDbType.UniqueIdentifier, invoiceLinePK22);
				command.AddParameter("@invoiceLinePK23", SqlDbType.UniqueIdentifier, invoiceLinePK23);
				command.AddParameter("@invoiceLinePK24", SqlDbType.UniqueIdentifier, invoiceLinePK24);

				command.AddParameter("@cusEntryLinePK1", SqlDbType.UniqueIdentifier, cusEntryLinePK1);
				command.AddParameter("@cusEntryLinePK2", SqlDbType.UniqueIdentifier, cusEntryLinePK2);
				command.AddParameter("@cusEntryLinePK3", SqlDbType.UniqueIdentifier, cusEntryLinePK3);
				command.AddParameter("@cusEntryLinePK4", SqlDbType.UniqueIdentifier, cusEntryLinePK4);
				command.AddParameter("@cusEntryLinePK5", SqlDbType.UniqueIdentifier, cusEntryLinePK5);
				command.AddParameter("@cusEntryLinePK6", SqlDbType.UniqueIdentifier, cusEntryLinePK6);
				command.AddParameter("@cusEntryLinePK7", SqlDbType.UniqueIdentifier, cusEntryLinePK7);

				command.ExecuteNonQuery();
			}

			var reportSql = @"select JE_PK, TotalEnteredValue from USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null,@importerPK, null, null, null, null, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = reader["JE_PK"].ToString();
						var totalEnteredValue = reader["TotalEnteredValue"].ToString();
						if (pk == declarationPK.ToString())
						{
							AssertEquals("Total entered invoice value.", "23.0000", totalEnteredValue);
						}
						else
						{
							AssertEquals("Total entered invoice value.", "12580.0000", totalEnteredValue);
						}
					}
				}
			}
		}

		class ReleaseStatus
		{
			public string Code { get; set; }
			public string CodeDescription { get; set; }
		}

		public void TestReleaseStatusDescription()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "US");
			var declarationPK4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B004", "IMP", 4, dataModel: "US");
			var declarationPK5 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B005", "IMP", 5, dataModel: "US");
			var declarationPK6 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B006", "IMP", 6, dataModel: "US");
			var declarationPK7 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B007", "IMP", 7, dataModel: "US");
			var declarationPK8 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B008", "IMP", 8, dataModel: "US");
			var declarationPK9 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B009", "IMP", 9, dataModel: "US");
			var declarationPK10 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0010", "IMP", 10, dataModel: "US");
			var declarationPK11 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0011", "IMP", 11, dataModel: "US");

			var sql = @"
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'CAN', 'JE', @declarationPK1);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'DEL', 'JE', @declarationPK2);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'EXM', 'JE', @declarationPK3);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'HLD', 'JE', @declarationPK4);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'NRL', 'JE', @declarationPK5);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'REL', 'JE', @declarationPK6);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'ADM', 'JE', @declarationPK7);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'DOC', 'JE', @declarationPK8);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'NRC', 'JE', @declarationPK9);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'NRT', 'JE', @declarationPK10);
				INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES (NEWID(), 'US_ReleaseStatus', 'STR', 'RVW', 'JE', @declarationPK11);
			";
			TestDataCreator.CreateCusEntryNum(declarationPK1, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK2, "JobDeclaration", "EntryNum2", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK3, "JobDeclaration", "EntryNum3", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK4, "JobDeclaration", "EntryNum4", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK5, "JobDeclaration", "EntryNum5", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK6, "JobDeclaration", "EntryNum6", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK7, "JobDeclaration", "EntryNum7", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK8, "JobDeclaration", "EntryNum8", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK9, "JobDeclaration", "EntryNum9", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK10, "JobDeclaration", "EntryNum10", "ENS", "CUS", "US");
			TestDataCreator.CreateCusEntryNum(declarationPK11, "JobDeclaration", "EntryNum11", "ENS", "CUS", "US");

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@declarationPK3", SqlDbType.UniqueIdentifier, declarationPK3);
				command.AddParameter("@declarationPK4", SqlDbType.UniqueIdentifier, declarationPK4);
				command.AddParameter("@declarationPK5", SqlDbType.UniqueIdentifier, declarationPK5);
				command.AddParameter("@declarationPK6", SqlDbType.UniqueIdentifier, declarationPK6);
				command.AddParameter("@declarationPK7", SqlDbType.UniqueIdentifier, declarationPK7);
				command.AddParameter("@declarationPK8", SqlDbType.UniqueIdentifier, declarationPK8);
				command.AddParameter("@declarationPK9", SqlDbType.UniqueIdentifier, declarationPK9);
				command.AddParameter("@declarationPK10", SqlDbType.UniqueIdentifier, declarationPK10);
				command.AddParameter("@declarationPK11", SqlDbType.UniqueIdentifier, declarationPK11);
				command.ExecuteNonQuery();
			}

			sql = @"select JE_PK, ReleaseStatus, ReleaseStatusDescription from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, ReleaseStatus>();

					while (reader.Read())
					{
						data[reader.GetGuid(0)] = new ReleaseStatus()
						{
							Code = reader.GetString(1),
							CodeDescription = reader.GetString(2),
						};
					}
					AssertContainsExactElementsInAnyOrder(new[] {
						declarationPK1, declarationPK2, declarationPK3, declarationPK4, declarationPK5, declarationPK6,
						declarationPK7, declarationPK8, declarationPK9, declarationPK10, declarationPK11 }, data.Keys);
					AssertEquals("CAN", data[declarationPK1].Code);
					AssertEquals("CANCELLED", data[declarationPK1].CodeDescription);

					AssertEquals("DEL", data[declarationPK2].Code);
					AssertEquals("DELETED", data[declarationPK2].CodeDescription);

					AssertEquals("EXM", data[declarationPK3].Code);
					AssertEquals("EXAM", data[declarationPK3].CodeDescription);

					AssertEquals("HLD", data[declarationPK4].Code);
					AssertEquals("HOLD", data[declarationPK4].CodeDescription);

					AssertEquals("NRL", data[declarationPK5].Code);
					AssertEquals("NOT RELEASED", data[declarationPK5].CodeDescription);

					AssertEquals("REL", data[declarationPK6].Code);
					AssertEquals("RELEASED", data[declarationPK6].CodeDescription);

					AssertEquals("ADM", data[declarationPK7].Code);
					AssertEquals("ADMISSIBLE", data[declarationPK7].CodeDescription);

					AssertEquals("DOC", data[declarationPK8].Code);
					AssertEquals("DOCUMENTS REQUESTED", data[declarationPK8].CodeDescription);

					AssertEquals("NRC", data[declarationPK9].Code);
					AssertEquals("NOT RELEASED, CANCELLATION PENDING", data[declarationPK9].CodeDescription);

					AssertEquals("NRT", data[declarationPK10].Code);
					AssertEquals("NOT RELEVANT", data[declarationPK10].CodeDescription);

					AssertEquals("RVW", data[declarationPK11].Code);
					AssertEquals("CBP REVIEW", data[declarationPK11].CodeDescription);
				}
			}
		}

		public void TestFieldsInJE_AddInfo()
		{
			var sql = @"
--Declare variables
DECLARE @CompanyPk UNIQUEIDENTIFIER
DECLARE @BranchPk UNIQUEIDENTIFIER
DECLARE @CusEntryNumPk UNIQUEIDENTIFIER
DECLARE @declarationRef1 varchar(35)
DECLARE @declarationRef2 varchar(35)

--Assign values
SET @CompanyPk = NEWID()
SET @BranchPk = NEWID()
SET @declarationRef1 = LEFT(NEWID(), 35)
SET @declarationRef2 = LEFT(NEWID(), 35)

--Arrange for tests
INSERT INTO dbo.GlbCompany (GC_Pk, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)  VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_Pk, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.JobDeclaration (JE_Pk, JE_DataModel, JE_GB, JE_GC, JE_MessageType, JE_AddInfo, JE_ClusterKey, JE_DeclarationReference) VALUES (NEWID(), 'US', @BranchPk, @CompanyPK, 'IMP', 'NAFTAClaimStat=Y*ProtestStat=Y', 1, @declarationRef1)
INSERT INTO dbo.JobDeclaration (JE_Pk, JE_DataModel, JE_GB, JE_GC, JE_MessageType, JE_AddInfo, JE_ClusterKey, JE_DeclarationReference) VALUES (NEWID(), 'US', @BranchPk, @CompanyPK, 'IMP', 'PriorDisclosure=Y', 2, @declarationRef2)
SELECT JE_ClusterKey, NAFTAClaimStat, PriorDisclosure, ProtestStat FROM dbo.USDeclarations(@CompanyPk, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)
ORDER BY JE_ClusterKey
";
			using (var command = Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new List<(object, object, object)>();
					while (reader.Read())
					{
						result.Add((reader["NAFTAClaimStat"], reader["PriorDisclosure"], reader["ProtestStat"]));
					}

					CombineAssertions(() =>
					{
						AssertEquals("NAFTAClaimStat", "Y", result[0].Item1);
						AssertEquals("PriorDisclosure", string.Empty, result[0].Item2);
						AssertEquals("ProtestStat", "Y", result[0].Item3);

						AssertEquals("NAFTAClaimStat", string.Empty, result[1].Item1);
						AssertEquals("PriorDisclosure", "Y", result[1].Item2);
						AssertEquals("ProtestStat", string.Empty, result[1].Item3);
					});
				}
			}
		}

		public void TestFTZNo()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, addInfo: "FTZNo=123456", dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");

			var sql = @"select JE_PK, FTZNo from dbo.USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";
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
						AssertEquals("declarationPK1: ", "123456", data[declarationPK1].ToString());
						AssertEquals("declarationPK2: ", string.Empty, data[declarationPK2]);
					});
				}
			}
		}

		public void TestAuditData()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			TestDataCreator.CreateGlbStaff("HXU", "HXU FULLNAME");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "US");

			var updateAuditQql = @"
UPDATE dbo.JobDeclaration
SET
	JE_AuditDateUtc = '2021-05-18',
	JE_AuditReference = 'Test Reference',
	JE_GS_NKAuditUser = 'HXU',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_DeclarationReference = 'B0001'";
			using (var command = Db.Connection.Command(updateAuditQql))
			{
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT AuditDate,AuditReference,AuditUser,AuditUserName FROM USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(new DateTime(2021, 5, 18), (DateTime)reader["AuditDate"]);
						AssertEquals("Test Reference", reader["AuditReference"].ToString());
						AssertEquals("HXU", reader["AuditUser"].ToString());
						AssertEquals("HXU FULLNAME", reader["AuditUserName"].ToString());
					}

					AssertEquals(1, count);
				}
			}
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

			var reportSql = @"select JE_DeclarationReference, PGAExpeditedRelease from USDeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 'Y')";

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

					AssertEquals("PGA Expedited Release", "JOB1", job);
					AssertEquals("PGA Expedited Release", "Y", pgaExpeditedRelease);
				}
			}
		}

		public void TestSearchForENSActionAndCRLAction()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US", addInfo: "ENSAction=Incomplete*CRLAction=Complete");
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US", addInfo: "ENSAction=Complete*CRLAction=Incomplete");
			var declaration3PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "US", addInfo: "ENSAction=Incomplete");
			var declaration4PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B004", "IMP", 4, dataModel: "US");

			var sql = "SELECT JE_PK FROM dbo.USDeclarations(@CompanyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @ENSAction, NULL, NULL, @CRLAction, NULL)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				cmd.AddParameter("@ENSAction", SqlDbType.VarChar, "ALL");
				cmd.AddParameter("@CRLAction", SqlDbType.VarChar, "");
				using (var reader = cmd.ExecuteReader())
				{
					var dt = new DataTable("USDeclarations");
					dt.Load(reader);
					AssertEquals(3, dt.Rows.Count);
					AssertEquals(3, dt.Select($"JE_PK = '{declaration1PK}' OR JE_PK = '{declaration2PK}' OR JE_PK = '{declaration3PK}'").Length);
				}

				cmd.SetParameterValue("@ENSAction", "Incomplete");
				cmd.SetParameterValue("@CRLAction", "");
				using (var reader = cmd.ExecuteReader())
				{
					var dt = new DataTable("USDeclarations");
					dt.Load(reader);
					AssertEquals(2, dt.Rows.Count);
					AssertEquals(2, dt.Select($"JE_PK = '{declaration1PK}' OR JE_PK = '{declaration3PK}'").Length);
				}

				cmd.SetParameterValue("@ENSAction", "");
				cmd.SetParameterValue("@CRLAction", "ALL");
				using (var reader = cmd.ExecuteReader())
				{
					var dt = new DataTable("USDeclarations");
					dt.Load(reader);
					AssertEquals(2, dt.Rows.Count);
					AssertEquals(2, dt.Select($"JE_PK = '{declaration1PK}' OR JE_PK = '{declaration2PK}'").Length);
				}

				cmd.SetParameterValue("@ENSAction", "");
				cmd.SetParameterValue("@CRLAction", "Incomplete");
				using (var reader = cmd.ExecuteReader())
				{
					var dt = new DataTable("USDeclarations");
					dt.Load(reader);
					AssertEquals(1, dt.Rows.Count);
					AssertEquals(1, dt.Select($"JE_PK = '{declaration2PK}'").Length);
				}

				cmd.SetParameterValue("@ENSAction", "Complete");
				cmd.SetParameterValue("@CRLAction", "Incomplete");
				using (var reader = cmd.ExecuteReader())
				{
					var dt = new DataTable("USDeclarations");
					dt.Load(reader);
					AssertEquals(1, dt.Rows.Count);
					AssertEquals(1, dt.Select($"JE_PK = '{declaration2PK}'").Length);
				}
			}
		}
	}
}
