using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class IncidentAssociationQueryTest : TestCaseWithFactory
	{
		int currentVersion;
		DateTime now;

		protected override void SetUp()
		{
			base.SetUp();
			now = DateTime.UtcNow;
			currentVersion = 2;
		}

		void MakeStmData(int version, string name = "IncidentSimilarity_LatestVersion")
		{
			var stmData = Factory.New<StmData>();
			stmData.FillWithValidTestData();
			stmData.SD_Name = name;
			stmData.SD_BinaryValue = BitConverter.GetBytes(version);
			Factory.Save();
		}

		Guid MakeIncidentMain(DateTime date)
		{
			var incidentMain = Factory.New<IncidentMainBase>();
			incidentMain.FillWithValidTestData();
			incidentMain.IM_SystemLastEditTimeUtc = date;
			Factory.Save();

			var dateParam = "@date";
			var pkParam = "@pk";

			var parameters = new ZSqlParameterCollection
				{
					ZSqlParameter.New(dateParam, date, IncidentMainSchema.IM_SystemLastEditTimeUtc),
					ZSqlParameter.New(pkParam, incidentMain.PK, IncidentMainSchema.PK),
				};
			var cmd = Db.Connection.Command(string.Format(
					"UPDATE {0} SET {1} = {2} WHERE {3} = {4};",
					AutoIncidentMain.Schema.TableName,
					AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc,
					dateParam,
					AutoIncidentMain.Schema.PK,
					pkParam
				));
			cmd.AddParameters(parameters);
			cmd.ExecuteNonQuery();
			return new Guid(incidentMain.PK.ToString());
		}

		Guid MakeIncidentSimilarityTfIdf(Guid incidentGuid, DateTime lastEdited)
		{
			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			incidentSimilarityTfIdf.FillWithValidTestData();
			incidentSimilarityTfIdf.ISV_Version = currentVersion;
			incidentSimilarityTfIdf.ISV_IM_Incident = incidentGuid;
			incidentSimilarityTfIdf.ISV_IncidentLastModified = new CargoWise.Types.ZDateTime(lastEdited);

			Factory.Save();

			var dateParam = "@date";
			var pkParam = "@pk";

			var parameters = new ZSqlParameterCollection
				{
					ZSqlParameter.New(dateParam, lastEdited, IncidentMainSchema.IM_SystemLastEditTimeUtc),
					ZSqlParameter.New(pkParam, incidentSimilarityTfIdf.PK, IncidentMainSchema.PK),
				};
			var cmd = Db.Connection.Command(string.Format(
					"UPDATE {0} SET {1} = {2} WHERE {3} = {4};",
					IncidentSimilarityTfIdf.Schema.TableName,
					IncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
					dateParam,
					IncidentSimilarityTfIdf.Schema.PK,
					pkParam
				));
			cmd.AddParameters(parameters);
			cmd.ExecuteNonQuery();

			return new Guid(incidentSimilarityTfIdf.PK.ToString());
		}

		public void TestIncidentAssociationQuery_GetLatestVersion()
		{
			//Arrange
			MakeStmData(currentVersion);
			var incidentAssociationQuery = new IncidentAssociationQuery();
			//Act 
			var version = incidentAssociationQuery.GetLatestVersion();
			//Assert
			AssertEquals("version should be 2", currentVersion, version);
		}

		public void TestIncidentAssociationQuery_QueryObjects()
		{
			MakeStmData(1, "SD_Name_one");
			MakeStmData(2, "SD_Name_Two");

			//Arrange
			var incidentAssociationQuery = new IncidentAssociationQuery();
			//Act 
			var returnValue = incidentAssociationQuery.QueryObjects(
				string.Format(
					"SELECT {0}, {1} FROM {2} WHERE {0} IN ('SD_Name_one','SD_Name_Two')",
					StmData.Schema.SD_Name,
					StmData.Schema.SD_BinaryValue,
					StmData.Schema.TableName)
				).ToList();
			//Assert
			AssertEquals(1, BitConverter.ToInt32((byte[])returnValue[0][StmData.Schema.SD_BinaryValue], 0));
			AssertEquals(2, BitConverter.ToInt32((byte[])returnValue[1][StmData.Schema.SD_BinaryValue], 0));
		}

		public void TestIncidentAssociationQuery_GetIncidentGuidsBetween()
		{
			//Arrange
			var fromDate = now.AddDays(-20);
			var toDate = now.AddDays(1);

			var guid1 = MakeIncidentMain(now.AddDays(1));
			var guid2 = MakeIncidentMain(now.AddDays(-20));
			var guid3 = MakeIncidentMain(now.AddDays(-3));

			MakeIncidentMain(now.AddDays(-21));
			MakeIncidentMain(now.AddDays(2));
			var incidentAssociationQuery = new IncidentAssociationQuery();

			//Act 
			var returnValue = incidentAssociationQuery.GetIncidentGuidsBetween(fromDate, toDate);
			//Assert
			AssertEquals("Expect only 3 incidents in range", 3, returnValue.Count());
			AssertCollectionContains(guid1, returnValue);
			AssertCollectionContains(guid2, returnValue);
			AssertCollectionContains(guid3, returnValue);
		}

		public void TestIncidentAssociationQuery_GetNewAndUpdatedIncidentGuids()
		{
			//Arrange
			var fromDate = now.AddDays(-20);
			var toDate = now.AddDays(1);

			var guid1 = MakeIncidentMain(now.AddDays(-1));
			var guid2 = MakeIncidentMain(now.AddDays(-2));
			MakeIncidentSimilarityTfIdf(guid2, now.AddDays(-3));
			var guid3 = MakeIncidentMain(now.AddDays(-3));
			MakeIncidentSimilarityTfIdf(guid3, now.AddDays(-4));

			MakeIncidentMain(now.AddDays(-22));
			MakeIncidentMain(now.AddDays(2));

			var incidentAssociationQuery = new IncidentAssociationQuery();
			//Act 
			var returnValue = incidentAssociationQuery.GetNewAndUpdatedIncidentGuids(currentVersion, fromDate, toDate);
			//Assert
			AssertEquals("Expect only 3 incidents in range", 3, returnValue.Count());
			AssertCollectionContains(guid1, returnValue);
			AssertCollectionContains(guid2, returnValue);
			AssertCollectionContains(guid3, returnValue);
		}

		public void TestIncidentAssociationQuery_UpdateTfIdfTimestamps()
		{
			//Arrange
			var incidentMainGuids = new List<Guid>()
			{
				MakeIncidentMain(now.AddDays(-10)),
				MakeIncidentMain(now.AddDays(-8)),
				MakeIncidentMain(now.AddDays(-6)),
				MakeIncidentMain(now.AddDays(-4)),
			};
			var incidentSimilarityTfIdfGuids = new List<Guid>()
			{
				MakeIncidentSimilarityTfIdf(incidentMainGuids[0], now.AddDays(-10)),
				MakeIncidentSimilarityTfIdf(incidentMainGuids[1], now.AddDays(-8)),
				MakeIncidentSimilarityTfIdf(incidentMainGuids[2], now.AddDays(-6)),
				MakeIncidentSimilarityTfIdf(incidentMainGuids[3], now.AddDays(-4)),
			};

			var incidentAssociationQuery = new IncidentAssociationQuery();

			//Act
			var timestamps = new Dictionary<Guid, DateTime>()
			{
				{ incidentMainGuids[0], now.AddDays(-5) },
				{ incidentMainGuids[1], now.AddDays(-4) },
				{ incidentMainGuids[2], now.AddDays(-3) },
				{ incidentMainGuids[3], now.AddDays(-2) },
			};

			incidentAssociationQuery.UpdateTfIdfTimestamps(currentVersion, timestamps);

			//Assert
			var query = new ZQuery();
			query.OrderBy = AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified;

			Factory.ReloadAll<IncidentSimilarityTfIdf>();
			var newIncidentSimilarityTfIdfs = Factory.Load<IncidentSimilarityTfIdf>(query);
			AssertEquals(4, newIncidentSimilarityTfIdfs.Length);

			for (int i = 0; i < 4; i++)
			{
				AssertEquals(incidentSimilarityTfIdfGuids[i], newIncidentSimilarityTfIdfs[i].PK);
				AssertEquals(incidentMainGuids[i], newIncidentSimilarityTfIdfs[i].ISV_IM_Incident);
				AssertZDatesWithin5Minutes("ISV_IncidentLastModified should be updated.", now.AddDays(-5 + i), newIncidentSimilarityTfIdfs[i].ISV_IncidentLastModified);
			}
		}
	}
}
