using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class SimilarityMatrixBootstrapperQueryExecutorTest : TestCaseWithFactory
	{
		int currentVersion;
		int newVersion;

		protected override void SetUp()
		{
			base.SetUp();
			currentVersion = 2;
			newVersion = currentVersion + 1;
		}

		Guid MakeIncidentMain()
		{
			var incidentMain = Factory.New<IncidentMainBase>();
			incidentMain.FillWithValidTestData();
			Factory.Save();
			return new Guid(incidentMain.PK.ToString());
		}

		Guid MakeIncidentSimilarityTfIdf(Guid incidentMainPk, int version)
		{
			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			incidentSimilarityTfIdf.FillWithValidTestData();
			incidentSimilarityTfIdf.ISV_IM_Incident = incidentMainPk;
			incidentSimilarityTfIdf.ISV_Version = version;
			Factory.Save();
			return new Guid(incidentSimilarityTfIdf.PK.ToString());
		}

		Guid MakeIncidentSimilarityMatrix(Guid incidentMainPk1, Guid incidentMainPk2, int version)
		{
			var incidentSimilarityMatrix = Factory.New<IncidentSimilarityMatrix>();
			incidentSimilarityMatrix.FillWithValidTestData();
			incidentSimilarityMatrix.ISM_IM_Incident1 = incidentMainPk1;
			incidentSimilarityMatrix.ISM_IM_Incident2 = incidentMainPk2;
			incidentSimilarityMatrix.ISM_Similarity = 0.9;
			incidentSimilarityMatrix.ISM_Version = version;
			Factory.Save();
			return new Guid(incidentSimilarityMatrix.PK.ToString());
		}

		public void TestSimilarityMatrixBootstrapperQueryExecutor_nullConnection()
		{
			//Arange
			var dataTransferObjects = Enumerable.Range(1, 3).Select(i => new SimilarityMatrixDataTransferObject(ZGuid.NewZGuid().ToGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid(), newVersion, i / 10d)).ToList();
			//Act
			//Assert
			AssertExceptionThrown<ArgumentNullException>(() => new SimilarityMatrixBootstrapperQueryExecutor(null));
		}

		[StressTest]
		public void TestSimilarityMatrixBootstrapperQueryExecutor_commit()
		{
			//Arange
			var incidentGuidList = Enumerable.Range(0, 50).Select(_ => MakeIncidentMain()).ToList();
			var incidentGuidMap = incidentGuidList.Select((pk, i) => (pk, i)).ToDictionary(pki => pki.pk, pki => pki.i);

			var tfidfGuidList = new List<Guid>() {
				MakeIncidentSimilarityTfIdf(incidentGuidList[0], currentVersion),
				MakeIncidentSimilarityTfIdf(incidentGuidList[1], currentVersion),
				MakeIncidentSimilarityTfIdf(incidentGuidList[2], currentVersion)
			};

			var dataTransferObjects = Enumerable
				.Range(0, incidentGuidList.Count)
				.SelectMany(i => Enumerable.Range(0, incidentGuidList.Count).Select(j => (i, j)))
				.Select(indices => new SimilarityMatrixDataTransferObject(
					ZGuid.NewZGuid().ToGuid(),
					incidentGuidList[indices.i],
					incidentGuidList[indices.j],
					newVersion,
					(1.0 + Math.Min(indices.i, indices.j)) / (1.0 + Math.Max(indices.i, indices.j))))
				.ToList();

			var similarityMatrixBootstrapperQueryExecutor = new SimilarityMatrixBootstrapperQueryExecutor();

			//Act
			similarityMatrixBootstrapperQueryExecutor.Commit(dataTransferObjects, tfidfGuidList[0]);

			//Assert
			Factory.ReloadAll<IncidentSimilarityMatrix>();
			var query = new ZQuery();
			query.OrderBy = AutoIncidentSimilarityMatrix.Schema.ISM_Similarity;

			var newIncidentSimilarityMatrices = Factory.Load<IncidentSimilarityMatrix>(query);
			AssertEquals(50 * 50, newIncidentSimilarityMatrices.Length);
			foreach (var ism in newIncidentSimilarityMatrices)
			{
				AssertEquals(newVersion, ism.ISM_Version);

				Assert(incidentGuidMap.TryGetValue(ism.ISM_IM_Incident1.ToGuid(), out var i));
				Assert(incidentGuidMap.TryGetValue(ism.ISM_IM_Incident2.ToGuid(), out var j));
				var expectedSimilarity = (1.0 + Math.Min(i, j)) / (1.0 + Math.Max(i, j));
				var actualSimilarity = (double)ism.ISM_Similarity;
				Assert((Math.Abs(expectedSimilarity - actualSimilarity)) <= 1E-5);
			}
		}

		public void TestSimilarityMatrixBootstrapperQueryExecutor_DeleteExistingRows()
		{
			//Arange
			var incidentGuidList = new List<Guid>() {
				MakeIncidentMain(),
				MakeIncidentMain(),
				MakeIncidentMain()
			};

			var tfidfGuidList = new List<Guid>() {
				MakeIncidentSimilarityTfIdf(incidentGuidList[0], currentVersion),
				MakeIncidentSimilarityTfIdf(incidentGuidList[1], currentVersion),
				MakeIncidentSimilarityTfIdf(incidentGuidList[2], currentVersion)
			};

			var matrixGuidList = new List<Guid>() {
				MakeIncidentSimilarityMatrix(incidentGuidList[0], incidentGuidList[1], currentVersion),
				MakeIncidentSimilarityMatrix(incidentGuidList[0], incidentGuidList[2], currentVersion),
				MakeIncidentSimilarityMatrix(incidentGuidList[1], incidentGuidList[2], currentVersion),
				MakeIncidentSimilarityMatrix(incidentGuidList[0], incidentGuidList[2], newVersion),
			};

			var similarityMatrixBootstrapperQueryExecutor = new SimilarityMatrixBootstrapperQueryExecutor();

			//Act
			var numberDeleted = similarityMatrixBootstrapperQueryExecutor.DeleteExistingRows(incidentGuidList[0], currentVersion);

			AssertEquals(3, numberDeleted);

			var sqlText = string.Format(
				@"SELECT COUNT(*) FROM {0} WHERE {1} IN {2}",
				AutoIncidentSimilarityMatrix.Schema.TableName,
				AutoIncidentSimilarityMatrix.Schema.PK,
				string.Format(
					"({0}, {1})",
					matrixGuidList[2].ToSqlGuid(),
					matrixGuidList[3].ToSqlGuid()
				)
			);
			AssertEquals(2, (int)Db.Connection.ExecuteScalar(sqlText));

			sqlText = string.Format(
				@"SELECT COUNT(*) FROM {0}",
				AutoIncidentSimilarityMatrix.Schema.TableName
			);
			AssertEquals(2, (int)Db.Connection.ExecuteScalar(sqlText));

			sqlText = string.Format(
				@"SELECT COUNT(*) FROM {0} WHERE {1} IN {2}",
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.PK,
				string.Format(
					"({0}, {1})",
					tfidfGuidList[1].ToSqlGuid(),
					tfidfGuidList[2].ToSqlGuid()
				)
			);
			AssertEquals(2, (int)Db.Connection.ExecuteScalar(sqlText));
			sqlText = string.Format(
				@"SELECT COUNT(*) FROM {0}",
				AutoIncidentSimilarityTfIdf.Schema.TableName
			);
			AssertEquals(2, (int)Db.Connection.ExecuteScalar(sqlText));
		}
	}
}
