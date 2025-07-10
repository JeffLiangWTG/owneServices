using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.TW
{
	[TestedType(typeof(Report_TWImportTaxAndFeesPaymentList))]
	class Report_TWImportTaxAndFeesPaymentListTest : DbCreateScriptTest
	{
		public void TestJE_PK()
		{
			var reportSql = @"SELECT JE_PK FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("Branch not match", !reader.Read());
				}
			}
		}

		public void TestColumnsFromCusEntryInstruction()
		{
			var reportSql = @"SELECT JE_PK, DeclarationDate FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());

					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("DeclarationDate", new DateTime(2019, 12, 12), reader.GetDateTime(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromJobDeclaration()
		{
			Db.Connection.Command($@"
UPDATE
	dbo.JobDeclaration
SET
	JE_DeclarationReference = 'B00001001',
	JE_DateAtFinalDestination = DATEFROMPARTS(2021, 1, 1),
	JE_PaymentMethod = 'BRK',
	JE_TransportMode = 'AIR',
	JE_DefermentAccountNumber = '56789',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = 'ABC',
	JE_MasterBill = 'Master123',
	JE_HouseBill = 'House456'
WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, JobNumber, ImportDate, PaymentMethod, CreateUser, LastEditUser, TransportMode, GuaranteeNumber, MasterBill, HouseBill FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("JobNumber", "B00001001", reader.GetString(1));
					AssertEquals("ImportDate", new DateTime(2021, 1, 1), reader.GetDateTime(2));
					AssertEquals("PaymentMethod", "BRK", reader.GetString(3));
					AssertEquals("CreateUser", "ABC", reader.GetString(4));
					AssertEquals("LastEditUser", "ABC", reader.GetString(5));
					AssertEquals("TransportMode", "AIR", reader.GetString(6));
					AssertEquals("GuaranteeNumber", "56789", reader.GetString(7));
					AssertEquals("MasterBill", "Master123", reader.GetString(8));
					AssertEquals("HouseBill", "House456", reader.GetString(9));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromCusEntryHeader()
		{
			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryHeader
SET
	CH_EntryReleaseDate = DATEFROMPARTS(2021, 1, 1),
	CH_SystemLastEditTimeUtc = GETUTCDATE(),
	CH_SystemLastEditUser = '~BP'
WHERE CH_PK = '{entryHeaderPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, ReleaseDate FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("ReleaseDate", new DateTime(2021, 1, 1), reader.GetDateTime(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromCusEntryPayInfo()
		{
			var reportSql = @"SELECT JE_PK, ReceiptDate, DueDate, MemoID, ReferenceID, ReasonOfIssue, CustomsBankAccount FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("ReceiptDate", new DateTime(2021, 1, 1), reader.GetDateTime(1));
					AssertEquals("DueDate", new DateTime(2021, 2, 2), reader.GetDateTime(2));
					AssertEquals("MemoID", "Memo123", reader.GetString(3));
					AssertEquals("ReferenceId", "ReferenceId123", reader.GetString(4));
					AssertEquals("ReasonOfIssue", "1", reader.GetString(5));
					AssertEquals("CustomsBankAccount", "298560000", reader.GetString(6));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromJobDeclarationCusEntryNum()
		{
			var cusEntryNumPK = TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "ABAM10123BBBB2", "IMP", "CUS", "TW");

			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryStatus = 'C1',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, EntryNumber FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "AB/AM/10/123/BBBB2", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryNum = '',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			reportSql = @"SELECT JE_PK, EntryNumber FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromCusEntryHeaderCusEntryNum()
		{
			var cusEntryNumPKFromCusEntryHeader = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "BCBN10123BBBB2", "IMP", "CUS", "TW");

			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryStatus = 'C1',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPKFromCusEntryHeader}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, EntryNumber, ClearanceStatus FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "BC/BN/10/123/BBBB2", reader.GetString(1));
					AssertEquals("ClearanceStatus", "C1", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			var cusEntryNumPKFromJobDeclaration = TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "ABAM10123BBBB2", "IMP", "CUS", "TW");
			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryStatus = 'C2',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPKFromJobDeclaration}'").ExecuteNonQuery();

			reportSql = @"SELECT JE_PK, EntryNumber, ClearanceStatus FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "AB/AM/10/123/BBBB2", reader.GetString(1));
					AssertEquals("ClearanceStatus", "C1", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTotalTaxAmount()
		{
			CreateCusEntryPayInfo(entryHeaderPK, "D10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo123", "ReferenceId123", 200m, "1", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "A20", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo456", "ReferenceId123", 300m, "1", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "D10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo456", "ReferenceId123", 400m, "1", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "D10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo456", "ReferenceId123", 401m, "2", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "D10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo456", "ReferenceId456", 402m, "1", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "D10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo456", "ReferenceId123", 403m, "1", "298560001", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "D10", new DateTime(2021, 1, 2), new DateTime(2021, 2, 3), "Memo456", "ReferenceId123", 404m, "1", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "D10", new DateTime(2021, 1, 3), new DateTime(2021, 2, 4), "Memo456", "ReferenceId123", 405m, "1", "298560000", 1);

			var reportSql = @"SELECT JE_PK,TotalCashTaxAmount, TotalNonCashTaxAmount FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType) ORDER BY MemoID, TotalNonCashTaxAmount";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", 100m, reader.GetDecimal(1));
					AssertEquals("TotalNonCashTaxAmount", 200m, reader.GetDecimal(2));

					Assert("record 2", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", 300m, reader.GetDecimal(1));
					AssertEquals("TotalNonCashTaxAmount", 400m, reader.GetDecimal(2));

					Assert("record 3", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", 0m, reader.GetDecimal(1));
					AssertEquals("TotalNonCashTaxAmount", 401m, reader.GetDecimal(2));

					Assert("record 4", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", 0m, reader.GetDecimal(1));
					AssertEquals("TotalNonCashTaxAmount", 402m, reader.GetDecimal(2));

					Assert("record 5", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", 0m, reader.GetDecimal(1));
					AssertEquals("TotalNonCashTaxAmount", 403m, reader.GetDecimal(2));

					Assert("record 6", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", 0m, reader.GetDecimal(1));
					AssertEquals("TotalNonCashTaxAmount", 404m, reader.GetDecimal(2));

					Assert("record 7", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", 0m, reader.GetDecimal(1));
					AssertEquals("TotalNonCashTaxAmount", 405m, reader.GetDecimal(2));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestReasonOfIssueDescription()
		{
			CreateCusEntryPayInfo(entryHeaderPK, "A10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo123", "ReferenceId123", 100m, "2", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "A10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo123", "ReferenceId123", 100m, "3", "298560000", 1);
			CreateCusEntryPayInfo(entryHeaderPK, "A10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo123", "ReferenceId123", 100m, "4", "298560000", 1);

			var reportSql = @"SELECT JE_PK,ReasonOfIssueDescription FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType) ORDER BY ReasonOfIssue";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", "扣繳未成", reader.GetString(1));

					Assert("record 2", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", "申請繳現", reader.GetString(1));

					Assert("record 3", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", "先放後稅擔保額度不足", reader.GetString(1));

					Assert("record 4", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalCashTaxAmount", "申請EDI線上扣繳", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestSupplierName()
		{
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override SUD", declarationPK, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);

			var reportSql = @"SELECT JE_PK, SupplierName FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("SupplierName", "Org Override SUD", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override STA", declarationPK, "JE", "STA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("SupplierName", "Org Override STA", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestImporterName()
		{
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override IMD", declarationPK, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);

			var reportSql = @"SELECT JE_PK, ImporterName FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("ImporterName", "Org Override IMD", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override ITA", declarationPK, "JE", "ITA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("ImporterName", "Org Override ITA", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestDeclarant()
		{
			var organisation1PK = TestDataCreator.CreateOrganisation("ORG_1", "Org One");
			var organisation1MainAddressPK = TestDataCreator.CreateAddress(organisation1PK, "Head Office", "Somewhere", "TW");
			Db.Connection.Command($@"
UPDATE
	dbo.JobDeclaration
SET
	JE_OA_DeclarantAddress = '{organisation1MainAddressPK}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{declarationPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK,Declarant FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType) ORDER BY ReasonOfIssue";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "All");
				command.AddParameter("@PaymentType", SqlDbType.VarChar, "All");

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("Declarant", organisation1PK, reader.GetGuid(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTransportModeCondition()
		{
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "SEA", "All");
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "All", "All");
			AssertImportTaxAndFeesPaymentListDoNotHaveData(companyPK, "AIR", "All");

			Db.Connection.Command($@"
UPDATE
	dbo.JobDeclaration
SET
	JE_TransportMode = 'AIR',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{declarationPK}'").ExecuteNonQuery();

			AssertImportTaxAndFeesPaymentListDoNotHaveData(companyPK, "SEA", "All");
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "All", "All");
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "AIR", "All");
		}

		#region PaymentTypeCondition

		public void TestPaymentTypeCondition()
		{
			foreach (var transactionTypes in N5110TransactionTypes)
			{
				AssertN5110TransactionType(entryPayInfoPK, transactionTypes);
			}
			foreach (var transactionTypes in N5111TransactionTypes)
			{
				AssertN5111TransactionType(entryPayInfoPK, transactionTypes);
			}
		}

		readonly string[] N5110TransactionTypes = new string[] { "A10", "A20", "A30", "A40", "A50", "B10", "B31", "B32", "B40", "B51", "B52", "B60", "C10", "C20" };

		readonly string[] N5111TransactionTypes = new string[] { "C21", "C22", "C23", "C24", "C25", "C31", "C32", "C33", "C34", "D10", "F10", "F11", "F12", "F13", "F14", "F15", "F16", "F20", "F21", "F22", "F23", "F30", "F31", "F40", "F50", "F51", "F52", "F55", "F56", "F57", "F58", "F88", "F99", "X00" };

		void AssertN5110TransactionType(Guid entryPayInfoPK, string transactionType)
		{
			UpdateTransactionType(entryPayInfoPK, transactionType);
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "All", "N5110");
			AssertImportTaxAndFeesPaymentListDoNotHaveData(companyPK, "All", "N5111");
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "All", "All");
		}

		void AssertN5111TransactionType(Guid entryPayInfoPK, string transactionType)
		{
			UpdateTransactionType(entryPayInfoPK, transactionType);
			AssertImportTaxAndFeesPaymentListDoNotHaveData(companyPK, "All", "N5110");
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "All", "N5111");
			AssertImportTaxAndFeesPaymentListHasSingleDataOnly(companyPK, "All", "All");
		}

		void UpdateTransactionType(Guid entryPayInfoPK, string transactionType)
		{
			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryPayInfo
SET
	C9_TransactionType = '{transactionType}',
	C9_SystemLastEditTimeUtc = GETUTCDATE(),
	C9_SystemLastEditUser = '~BP'
WHERE C9_PK = '{entryPayInfoPK}'").ExecuteNonQuery();
		}

		#endregion

		const string reportSql = @"select JE_PK FROM Report_TWImportTaxAndFeesPaymentList(@companyPK, @TransportMode, @PaymentType)";

		void AssertImportTaxAndFeesPaymentListHasSingleDataOnly(Guid companyPK, string transportMode, string paymentType)
		{
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@PaymentType", SqlDbType.VarChar, paymentType);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		void AssertImportTaxAndFeesPaymentListDoNotHaveData(Guid companyPK, string transportMode, string paymentType)
		{
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@TransportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@PaymentType", SqlDbType.VarChar, paymentType);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		Guid companyPK;
		Guid branchPK;
		Guid declarationPK;
		Guid entryHeaderPK;
		Guid entryPayInfoPK;

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("TC1", "TW", "NTD");
			branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'TAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			entryPayInfoPK = CreateCusEntryPayInfo(entryHeaderPK, "A10", new DateTime(2021, 1, 1), new DateTime(2021, 2, 2), "Memo123", "ReferenceId123", 100m, "1", "298560000", 1);
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string messageType, string transportMode, string createUser, string lastEditUser, int clusterKey)
		{
			var declarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_TransportMode, JE_SystemCreateUser, JE_SystemLastEditUser, JE_ClusterKey)
VALUES (@declarationPK, 'TW', @messageType, @branchPK, @companyPK, @transportMode, @createUser, @lastEditUser, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobDeclarationSchema.JE_TransportMode.MaxLength, transportMode);
				command.AddParameter("@createUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusEntryInstruction(Guid declarationPK, DateTime dateForDuty, int clusterKey)
		{
			var entryInstructionPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@entryInstructionPK, 'TW', @declarationPK, @dateForDuty, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dateForDuty", SqlDbType.SmallDateTime, CusEntryInstructionSchema.CEI_DateForDuty.MaxLength, dateForDuty);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryInstructionPK;
		}

		Guid CreateCusEntryHeader(Guid declarationPK, string entryStatus, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_EntryStatus, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@entryHeaderPK, 'TW', @declarationPK, @entryStatus, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, CusEntryHeaderSchema.CH_EntryStatus.MaxLength, entryStatus);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		Guid CreateCusEntryPayInfo(Guid entryHeaderPK, string transactionType, DateTime receiptDate, DateTime paymentDate, string memoID, string referenceID, decimal paymentAmount, string reasonOfIssue, string customsBankAccount, int clusterKey)
		{
			var entryPayInfoPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryPayInfo (C9_PK, C9_CH, C9_TransactionType , C9_ReceiptDate, C9_PaymentDate, C9_IncomingPayResponseNo, C9_PaymentReference, C9_PaymentAmount, C9_PaymentReasonCode, C9_BankAccount, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
VALUES (@entryPayInfoPK, @entryHeaderPK, @transactionType, @receiptDate, @paymentDate, @memoID, @referenceID, @paymentAmount, @reasonOfIssue, @customsBankAccount, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryPayInfoPK", SqlDbType.UniqueIdentifier, entryPayInfoPK);
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@transactionType", SqlDbType.VarChar, transactionType);
				command.AddParameter("@receiptDate", SqlDbType.SmallDateTime, receiptDate);
				command.AddParameter("@paymentDate", SqlDbType.SmallDateTime, paymentDate);
				command.AddParameter("@memoID", SqlDbType.VarChar, memoID);
				command.AddParameter("@referenceID", SqlDbType.VarChar, referenceID);
				command.AddParameter("@paymentAmount", SqlDbType.Money, paymentAmount);
				command.AddParameter("@reasonOfIssue", SqlDbType.VarChar, reasonOfIssue);
				command.AddParameter("@customsBankAccount", SqlDbType.VarChar, customsBankAccount);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryPayInfoPK;
		}
	}
}
