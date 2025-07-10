using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.DE;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.DE.Testing
{
	[TestedType(typeof(DEMessageLinkedObjectDetails))]
	class DEMessageLinkedObjectDetailsTest : DbCreateScriptTest
	{
		public void TestDEMessageType()
		{
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", createUser: "~BP");

			var sql = @"select EM_PK, DEMessageType from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1, 1, 1, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2, 1, 1, 1));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], (string)reader["DEMessageType"]);
					}

					AssertEquals("The first six characters", "SCHOFF", data[message]);
				}
			}
		}
		public void TestNumbersWhenLinkTableIsCusEntryHeader()
		{
			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001000", "IMP", 1, null, null, "X002140");
			var entryHeader = TestDataCreator.CreateCusEntryHeader(isValid: true,
				messageType: "IMP",
				status: "",
				entryStatus: "",
				bgmReference: "00505655JSA20170214004008",

				totalPaid: 470.33f,
				linenum: 1,
				addInfo: "Test=test*Packages=5*test1=test1*RelPrintInd=Y",
				jePk: declaration,
				entrySubmittedDate: new DateTime(2017, 1, 2, 0, 11, 0),

				entryReleaseDate: new DateTime(2017, 1, 3, 0, 12, 0),
				instruction: Guid.Empty,
				warehouseTransactionStatus: "IUP",
				warehouseReleaseDate: new DateTime(2017, 1, 4, 0, 14, 0),
				bondAcquittedDate: new DateTime(2017, 1, 5),
				bondValidToDate: new DateTime(2017, 1, 6),
				clusterKey: 1);
			TestDataCreator.CreateCusEntryNum(entryHeader, "CusEntryHeader", "MRN001", "MRN", "CUS", "DE");
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, entryHeader, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", "TRX", "CusEntryHeader", createUser: "~BP");
			TestDataCreator.CreateEdiMessageLocalReferenceNumber(message, "X002140_Note€");
			TestDataCreator.CreateEdiMessageRegistrationNumber(message, "MRN001_Note, Benutzerteilerledigung㐿㪳çË");

			var sql = @"select EM_PK, LocalReferenceNo, RegistrationNumber from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, NumbersObject>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], new NumbersObject() { LocalReferenceNo = (string)reader["LocalReferenceNo"], RegistrationNumber = (string)reader["RegistrationNumber"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, data.Count);
						AssertEquals("LocalReferenceNumber", "X002140_Note€", data[message].LocalReferenceNo);
						AssertEquals("RegistrationNumber", "MRN001_Note, Benutzerteilerledigung㐿㪳çË", data[message].RegistrationNumber);
					});
				}
			}
		}

		public void TestNumbersWhenLinkTableIsCusTempStorageDec()
		{
			var customer = TestDataCreator.CreateOrganisation("XXXXXX", "SumA test account");
			var jobHeaderPK = TestDataCreator.CreateCusTempStorageJobHeader(branchPK, "JOBNUM0001", "REF001", customer, new DateTime(2020, 2, 1, 23, 59, 59));
			var storageDecPK = TestDataCreator.CreateCusTempStorageDec(jobHeaderPK, new DateTime(2020, 2, 1, 23, 59, 59));
			TestDataCreator.CreateCusEntryNum(storageDecPK, "CusTempStorageDec", "ATB0001", "SUM", "CUS", "DE");
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, storageDecPK, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", "TRX", "CusTempStorageDec", createUser: "~BP");
			TestDataCreator.CreateEdiMessageLocalReferenceNumber(message, "REF001_Note");
			TestDataCreator.CreateEdiMessageRegistrationNumber(message, "ATB0001_Note");

			var jobHeaderPK2 = TestDataCreator.CreateCusTempStorageJobHeader(branchPK, "JOBNUM0002", "REF002", customer, new DateTime(2020, 2, 1, 23, 59, 59));
			var storageDecPK2 = TestDataCreator.CreateCusTempStorageDec(jobHeaderPK2, new DateTime(2020, 2, 1, 23, 59, 59));
			TestDataCreator.CreateCusEntryNum(storageDecPK2, "CusTempStorageDec", "ATB0002", "REX", "CUS", "DE");

			var date2 = new DateTime(2020, 2, 1, 23, 59, 59);
			var interchange2 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", date2, "~BP", "XXXXXX", "DEC", "2", "PRS", date2);
			var message2 = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange2, storageDecPK2, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", "TRX", "CusTempStorageDec", createUser: "~BP");
			TestDataCreator.CreateEdiMessageLocalReferenceNumber(message2, "REF002_Note");
			TestDataCreator.CreateEdiMessageRegistrationNumber(message2, "ATB0002_Note");

			var sql = @"select EM_PK, LocalReferenceNo, RegistrationNumber from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, NumbersObject>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], new NumbersObject() { LocalReferenceNo = (string)reader["LocalReferenceNo"], RegistrationNumber = (string)reader["RegistrationNumber"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 2, data.Count);
						AssertEquals("LocalReferenceNumber", "REF001_Note", data[message].LocalReferenceNo);
						AssertEquals("RegistrationNumber of SUM", "ATB0001_Note", data[message].RegistrationNumber);

						AssertEquals("LocalReferenceNumber 2", "REF002_Note", data[message2].LocalReferenceNo);
						AssertEquals("RegistrationNumber of REX", "ATB0002_Note", data[message2].RegistrationNumber);
					});
				}
			}
		}

		public void TestNumbersWhenLinkTableIsCusTempStorageRegHeader_NoDeclaration()
		{
			var regHeaderPK = TestDataCreator.CreateCusTempStorageRegHeader("REF001", "SUM", new DateTime(2020, 2, 1, 23, 59, 59));
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, regHeaderPK, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", "RCV", "CusTempStorageRegHeader", createUser: "~BP");
			TestDataCreator.CreateEdiMessageRegistrationNumber(message, "REF001_Note");

			var sql = @"select EM_PK, LocalReferenceNo, RegistrationNumber from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, NumbersObject>();
					while (reader.Read())
					{
						NumbersObject newNumbersObject = new NumbersObject()
						{
							LocalReferenceNo = reader.IsDBNull(reader.GetOrdinal("LocalReferenceNo")) ? string.Empty : (string)reader["LocalReferenceNo"],
							RegistrationNumber = (string)reader["RegistrationNumber"]
						};
						data.Add((Guid)reader["EM_PK"], newNumbersObject);
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, data.Count);
						AssertEquals("LocalReferenceNumber", "", data[message].LocalReferenceNo);
						AssertEquals("RegistrationNumber", "REF001_Note", data[message].RegistrationNumber);
					});
				}
			}
		}

		public void TestNumbersWhenLinkTableIsCusTempStorageRegHeader_WithDeclaration()
		{
			var customer = TestDataCreator.CreateOrganisation("XXXXXX", "SumA test account");
			var jobHeaderPK = TestDataCreator.CreateCusTempStorageJobHeader(branchPK, "JOBNUM0001", "REF001", customer, new DateTime(2020, 2, 1, 23, 59, 59));
			var storageDecPK = TestDataCreator.CreateCusTempStorageDec(jobHeaderPK, new DateTime(2020, 2, 1, 23, 59, 59));
			TestDataCreator.CreateCusEntryNum(storageDecPK, "CusTempStorageDec", "ATB0001", "SUM", "CUS", "DE");

			var regHeaderPK = TestDataCreator.CreateCusTempStorageRegHeader("ATB0001", "SUM", new DateTime(2020, 2, 1, 23, 59, 59));
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, regHeaderPK, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", "RCV", "CusTempStorageRegHeader", createUser: "~BP");
			TestDataCreator.CreateEdiMessageLocalReferenceNumber(message, "REF001_Note");
			TestDataCreator.CreateEdiMessageRegistrationNumber(message, "ATB0001_Note");

			var sql = @"select EM_PK, LocalReferenceNo, RegistrationNumber from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, NumbersObject>();
					while (reader.Read())
					{
						NumbersObject newNumbersObject = new NumbersObject();
						newNumbersObject.LocalReferenceNo = (string)reader["LocalReferenceNo"];
						newNumbersObject.RegistrationNumber = (string)reader["RegistrationNumber"];
						data.Add((Guid)reader["EM_PK"], newNumbersObject);
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, data.Count);
						AssertEquals("LocalReferenceNumber", "REF001_Note", data[message].LocalReferenceNo);
						AssertEquals("RegistrationNumber", "ATB0001_Note", data[message].RegistrationNumber);
					});
				}
			}
		}

		public void TestNumbersWhenLinkTableIsCusInbondHeader()
		{
			var inbondHeaderPK = TestDataCreator.CreateCusInbondHeader("NCT0000001", branchPK);
			TestDataCreator.CreateCusEntryNum(inbondHeaderPK, "CusInbondHeader", "MRN001", "MRN", "CUS", "DE");
			TestDataCreator.CreateGenAddOnColumn(inbondHeaderPK, "BH", "ReferenceNumber", "STR", "LocalREF001");
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, inbondHeaderPK, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", "RCV", "CusInbondHeader", createUser: "~BP");
			TestDataCreator.CreateEdiMessageLocalReferenceNumber(message, "LocalREF001_Note");
			TestDataCreator.CreateEdiMessageRegistrationNumber(message, "MRN001_Note");

			var sql = @"select EM_PK, LocalReferenceNo, RegistrationNumber from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, NumbersObject>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], new NumbersObject() { LocalReferenceNo = (string)reader["LocalReferenceNo"], RegistrationNumber = (string)reader["RegistrationNumber"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, data.Count);
						AssertEquals("LocalReferenceNumber", "LocalREF001_Note", data[message].LocalReferenceNo);
						AssertEquals("RegistrationNumber", "MRN001_Note", data[message].RegistrationNumber);
					});
				}
			}
		}

		public void TestNumbersWhenLinkTableIsEmpty()
		{
			var customer = TestDataCreator.CreateOrganisation("XXXXXX", "SumA test account");
			var jobHeaderPK = TestDataCreator.CreateCusTempStorageJobHeader(branchPK, "JOBNUM0001", "REF001", customer, new DateTime(2020, 2, 1, 23, 59, 59));
			var storageDecPK = TestDataCreator.CreateCusTempStorageDec(jobHeaderPK, new DateTime(2020, 2, 1, 23, 59, 59));
			TestDataCreator.CreateCusEntryNum(storageDecPK, "CusTempStorageDec", "ATB0001", "SUM", "CUS", "DE");

			var storageDecPK2 = TestDataCreator.CreateCusTempStorageDec(jobHeaderPK, new DateTime(2020, 2, 1, 23, 59, 59));
			TestDataCreator.CreateCusEntryNum(storageDecPK2, "CusTempStorageDec", "ATB0002", "SUM", "CUS", "DE");

			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", "RCV", "", "~BP", "CUSFIN58660000003133130061119150845", "TST");
			TestDataCreator.CreateEdiMessageLocalReferenceNumber(message, "REF001_Note");
			TestDataCreator.CreateEdiMessageRegistrationNumber(message, "ATB0001_Note, ATB0002_Note");

			var regHeader1 = TestDataCreator.CreateCusTempStorageRegHeader("ATB0001", "SUM", new DateTime(2020, 2, 1, 23, 59, 59));
			var regLine1 = TestDataCreator.CreateCusTempStorageRegLine(regHeader1, 1);
			TestDataCreator.CreateCusTempStorageRegLineTransaction(regLine1, "CUSFIN58660000003133130061119150845", new DateTime(2020, 2, 1, 23, 59, 59), "CON");

			var regHeader2 = TestDataCreator.CreateCusTempStorageRegHeader("ATB0002", "SUM", new DateTime(2020, 2, 1, 23, 59, 59));
			var regLine2 = TestDataCreator.CreateCusTempStorageRegLine(regHeader2, 1);
			TestDataCreator.CreateCusTempStorageRegLineTransaction(regLine2, "CUSFIN58660000003133130061119150845", new DateTime(2020, 2, 1, 23, 59, 59), "CON");

			var sql = @"select EM_PK, LocalReferenceNo, RegistrationNumber from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, NumbersObject>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], new NumbersObject()
						{
							LocalReferenceNo = reader.IsDBNull(reader.GetOrdinal("LocalReferenceNo")) ? string.Empty : (string)reader["LocalReferenceNo"],
							RegistrationNumber = reader.IsDBNull(reader.GetOrdinal("RegistrationNumber")) ? string.Empty : (string)reader["RegistrationNumber"],
						});
					}
					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, data.Count);
						AssertEquals("LocalReferenceNumber", "REF001_Note", data[message].LocalReferenceNo);
						AssertEquals("RegistrationNumber", "ATB0001_Note, ATB0002_Note", data[message].RegistrationNumber);
					});
				}
			}
		}

		public void TestBranchCode()
		{
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", createUser: "~BP");
			TestDataCreator.CreateGenAddOnColumn(message, "EM", "LogbookEORIBranchSuffix", "STR", "0001");

			var sql = @"select EM_PK, BranchCode from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1, 1, 1, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2, 1, 1, 1));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], (string)reader["BranchCode"]);
					}

					AssertEquals("0001", data[message]);
				}
			}
		}

		public void TestDeliveredTime()
		{
			var date2 = new DateTime(2020, 1, 31, 23, 59, 59, DateTimeKind.Utc);
			var date3 = new DateTime(2020, 2, 2, 03, 05, 05, DateTimeKind.Utc);

			var interchange2 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", date2, "NLK", "XXXXXX", "DEC", "0002", "PRS", date2.AddHours(5));
			var interchange3 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", date3, "NLK", "XXXXXX", "DEC", "0003", "PRS", date3.AddHours(-5));

			TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", createUser: "~BP");
			TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange2, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", createUser: "~BP");
			TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange3, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", "DEA", "SCHOFF-8.9", createUser: "~BP");

			var sql = @"select EM_PK, DeliveredTime from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1, 1, 1, 1, DateTimeKind.Utc));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2, 1, 1, 1, DateTimeKind.Utc));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, DateTime>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], (DateTime)reader["DeliveredTime"]);
					}

					AssertEquals(3, data.Count);
				}
			}
		}

		public void TestDeliveredTime_CorrectTimeZone()
		{
			var createTimeUTC = new DateTime(2020, 5, 1, 13, 12, 11);
			var deliveredTimeOffset = new DateTimeOffset(2020, 5, 1, 15, 05, 05, TimeSpan.FromHours(2));

			var interchange2 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", createTimeUTC, "NLK", "XXXXXX", "DEC", "0002", "PRS", deliveredTimeOffset);
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange2, Guid.Empty, createTimeUTC, "PRS", "DEA", "SCHOFF-8.9", createUser: "~BP");

			var sql = @"select EM_PK, DeliveredTime from dbo.DEMessageLinkedObjectDetails(@companyPK, @module, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@module", SqlDbType.VarChar, "ATLAS/AES");
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 5, 1, 0, 0, 0));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 5, 1, 23, 59, 59));
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, DateTime>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], (DateTime)reader["DeliveredTime"]);
					}

					CombineAssertions(() =>
					{
						AssertEquals("Count", 1, data.Count);
						AssertEquals("Delivered Time", new DateTime(2020, 5, 1, 15, 05, 05), data[message]);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var messageDate = new DateTime(2020, 2, 1, 23, 59, 59, DateTimeKind.Utc);

			companyPK = TestDataCreator.CreateCompany("TC1", "DE", "DDE");
			branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "DEBER");
			departmentPK = TestDataCreator.CreateDepartment("TD1");
			interchange = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", messageDate, "~BP", "XXXXXX", "DEC", "1", "PRS", messageDate);
		}
		Guid companyPK, branchPK, departmentPK, interchange;

		class NumbersObject
		{
			public string LocalReferenceNo;
			public string RegistrationNumber;
		}
	}
}
