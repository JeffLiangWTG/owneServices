using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(EDIMessageDetails))]
	class EDIMessageDetailsTest : DbCreateScriptTest
	{
		public void TestDiscardedMessages()
		{
			var discardedInterchange = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", new DateTime(2020, 2, 1, 1, 1, 1), "~BP", "XXXXXX", "DEC", "1", "DCD");
			var discardedMessage = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, discardedInterchange, Guid.Empty, new DateTime(2020, 2, 1, 1, 1, 1), "DCD", createUser: "~BP");
			var errorInterchange = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", new DateTime(2020, 2, 1, 1, 1, 1), "~BP", "XXXXXX", "DEC", "2", "ERR");
			var errorMessage = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, errorInterchange, Guid.Empty, new DateTime(2020, 2, 1, 1, 1, 1), "ERR", createUser: "~BP");

			var sql = @"select EM_PK from dbo.EDIMessageDetails(@companyPK, @dateFrom, @dateTo, @EI_ApplicationCode, @EM_ApplicationCode)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1, 1, 1, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2, 1, 1, 1));
				command.AddParameter("@EI_ApplicationCode", SqlDbType.VarChar, EDIInterchangeSchema.EI_ApplicationCode.MaxLength, string.Empty);
				command.AddParameter("@EM_ApplicationCode", SqlDbType.VarChar, EDIMessageSchema.EM_ApplicationCode.MaxLength, string.Empty);
				using (var reader = command.ExecuteReader())
				{
					var data = new List<Guid>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"]);
					}
					AssertCollectionContains(errorMessage, data);
					AssertCollectionNotContains(discardedMessage, data);
				}
			}
		}

		public void TestSelectBasedOnEM_GB()
		{
			var companyPK2 = TestDataCreator.CreateCompany("TC2", "DE", "DD2");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, "TB2", "DEHAM");
			var interchange1 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", new DateTime(2020, 2, 1, 1, 1, 1), "~BP", "XXXXXX", "DEC", "1", "PRS");
			var message1 = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange1, Guid.Empty, new DateTime(2020, 2, 1, 1, 1, 1), createUser: "~BP");
			var interchange2 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", new DateTime(2020, 2, 1, 1, 1, 1), "~BP", "XXXXXX", "DEC", "2", "PRS");
			var message2 = TestDataCreator.CreateEDIMessage(branchPK2, departmentPK, interchange2, Guid.Empty, new DateTime(2020, 2, 1, 1, 1, 1), createUser: "~BP");

			var sql = @"select EM_PK from dbo.EDIMessageDetails(@companyPK, @dateFrom, @dateTo, @EI_ApplicationCode, @EM_ApplicationCode)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1, 1, 1, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2, 1, 1, 1));
				command.AddParameter("@EI_ApplicationCode", SqlDbType.VarChar, EDIInterchangeSchema.EI_ApplicationCode.MaxLength, string.Empty);
				command.AddParameter("@EM_ApplicationCode", SqlDbType.VarChar, EDIMessageSchema.EM_ApplicationCode.MaxLength, string.Empty);
				using (var reader = command.ExecuteReader())
				{
					var data = new List<Guid>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"]);
					}
					AssertCollectionContains(message1, data);
					AssertCollectionNotContains(message2, data);
				}
			}
		}

		public void TestSelectBasedOnCreateTime()
		{
			var interchange1 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", new DateTime(2020, 2, 1, 1, 1, 1), "~BP", "XXXXXX", "DEC", "1", "PRS");
			var message1 = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange1, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", createUser: "~BP");
			var interchange2 = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", new DateTime(2020, 2, 2, 1, 2, 1), "~BP", "XXXXXX", "DEC", "2", "PRS");
			var message2 = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange2, Guid.Empty, new DateTime(2020, 2, 2, 1, 2, 1), "PRS", createUser: "~BP");

			var sql = @"select EM_PK from dbo.EDIMessageDetails(@companyPK, @dateFrom, @dateTo, @EI_ApplicationCode, @EM_ApplicationCode)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1, 1, 1, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2, 1, 1, 1));
				command.AddParameter("@EI_ApplicationCode", SqlDbType.VarChar, EDIInterchangeSchema.EI_ApplicationCode.MaxLength, string.Empty);
				command.AddParameter("@EM_ApplicationCode", SqlDbType.VarChar, EDIMessageSchema.EM_ApplicationCode.MaxLength, string.Empty);
				using (var reader = command.ExecuteReader())
				{
					var data = new List<Guid>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"]);
					}
					AssertCollectionContains(message1, data);
					AssertCollectionNotContains(message2, data);
				}
			}
		}

		public void TestDeliveredTime()
		{
			var interchange = TestDataCreator.CreateEDIInterchange(branchPK, "GMD", new DateTime(2020, 2, 1, 1, 1, 1), "~BP", "XXXXXX", "DEC", "1", "PRS", new DateTimeOffset(new DateTime(2020, 2, 1, 1, 1, 1)));
			var message = TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchange, Guid.Empty, new DateTime(2020, 2, 1, 23, 59, 59), "PRS", createUser: "~BP");

			var sql = @"select EM_PK, DeliveredTime from dbo.EDIMessageDetails(@companyPK, @dateFrom, @dateTo, @EI_ApplicationCode, @EM_ApplicationCode)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.DateTime, new DateTime(2020, 2, 1, 1, 1, 1));
				command.AddParameter("@dateTo", SqlDbType.DateTime, new DateTime(2020, 2, 2, 1, 1, 1));
				command.AddParameter("@EI_ApplicationCode", SqlDbType.VarChar, EDIInterchangeSchema.EI_ApplicationCode.MaxLength, string.Empty);
				command.AddParameter("@EM_ApplicationCode", SqlDbType.VarChar, EDIMessageSchema.EM_ApplicationCode.MaxLength, string.Empty);
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, DateTimeOffset>();
					while (reader.Read())
					{
						data.Add((Guid)reader["EM_PK"], (DateTimeOffset)reader["DeliveredTime"]);
					}
					AssertEquals(new DateTimeOffset(new DateTime(2020, 2, 1, 1, 1, 1)), data[message]);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany("TC1", "DE", "DDE");
			branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "DEBER");
			departmentPK = TestDataCreator.CreateDepartment("TD1");
		}

		Guid companyPK, branchPK, departmentPK;
	}
}
