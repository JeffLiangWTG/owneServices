using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	public class SimilarIncidentRepositoryTest : TestCaseWithFactory
	{
		public List<ZGuid> SetupIncidents(int versionNumber, DateTime time)
		{
			var stmData = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, "IncidentSimilarity_LatestVersion"));

			if (stmData == null || stmData.Length == 0)
			{
				var sd = Factory.New<StmData>();
				sd.SD_Name = "IncidentSimilarity_LatestVersion";
				sd.SD_BinaryValue = BitConverter.GetBytes(1);
				Factory.Save();

				stmData = new[] { sd };
			}

			stmData[0].SD_BinaryValue = BitConverter.GetBytes(versionNumber);

			var incidentGuids = new List<ZGuid>();

			for (var i = 0; i < 4; i++)
			{
				var incident = Factory.New<IncidentMainBase>();
				incident.IM_IncidentType = "INC";
				if (i == 1 || i == 2)
				{
					incident.IM_Product = "PRC";
				}
				if (i == 3)
				{
					incident.IM_ProgramArea = "PRA";
				}
				incidentGuids.Add(incident.PK);
				incident.IM_SystemLastEditTimeUtc = time;
				Factory.Save();
			}
			return incidentGuids;
		}
		public List<IncidentSimilarityTfIdf> SetupIncidentSimilarityTfIdf(int versionNumber, List<ZGuid> incidentGuids, DateTime time)
		{
			var incidentSimilarityTfIdfList = new List<IncidentSimilarityTfIdf>();
			for (var i = 0; i < incidentGuids.Count; i++)
			{
				var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
				incidentSimilarityTfIdf.ISV_IM_Incident = incidentGuids[i];
				incidentSimilarityTfIdf.ISV_Status = "MCD";
				incidentSimilarityTfIdf.ISV_Version = versionNumber;
				incidentSimilarityTfIdf.ISV_IncidentLastModified = time;
				incidentSimilarityTfIdfList.Add(incidentSimilarityTfIdf);
				Factory.Save();
			}
			return incidentSimilarityTfIdfList;
		}
		public List<IncidentSimilarityMatrix> SetupIncidentSimilarityMatrix(int versionNumber, List<ZGuid> incidentGuids)
		{
			var incidentSimilarityMatrixList = new List<IncidentSimilarityMatrix>();
			for (var i = 0; i < incidentGuids.Count; i++)
			{
				for (var j = i + 1; j < incidentGuids.Count; j++)
				{
					var incidentSimilarityMatrix = Factory.New<IncidentSimilarityMatrix>();
					incidentSimilarityMatrix.ISM_IM_Incident1 = incidentGuids[i];
					incidentSimilarityMatrix.ISM_IM_Incident2 = incidentGuids[j];
					incidentSimilarityMatrix.ISM_Version = versionNumber;
					incidentSimilarityMatrix.ISM_Similarity = new decimal(0.1);
					if (i == 0 && j == 1)
					{
						incidentSimilarityMatrix.ISM_Similarity = new decimal(0.6);
					}
					incidentSimilarityMatrixList.Add(incidentSimilarityMatrix);
					Factory.Save();
				}
			}
			return incidentSimilarityMatrixList;
		}

		public void TestGetRelatedIncidentsFromMatrix()
		{
			// Arrange
			string sql = null;

			var mockIncidentAssociationQuery = new Mock<IIncidentAssociationQuery>();
			mockIncidentAssociationQuery
				.Setup(m => m.QueryObjects(It.IsAny<string>(), It.IsAny<ZSqlParameterCollection>()))
				.Returns((string query, ZSqlParameterCollection parameterCollection) =>
				{
					sql = query;
					var returnVal = new List<Dictionary<string, object>>();

					for (var i = 0; i < 10; i++)
					{
						if (i % 2 == 0)
						{
							returnVal.Add(new Dictionary<string, object> { { "ISM_IM_Incident1", Guid.NewGuid() }, { "ISM_IM_Incident2", ZGuid.BrettsGuid.ToGuid() } });
						}
						else
						{
							returnVal.Add(new Dictionary<string, object> { { "ISM_IM_Incident1", ZGuid.BrettsGuid.ToGuid() }, { "ISM_IM_Incident2", Guid.NewGuid() } });
						}
					}

					return returnVal;
				});

			var similarIncidentRepository = new SimilarIncidentRepository(mockIncidentAssociationQuery.Object);

			// Act
			var result = similarIncidentRepository.GetRelatedIncidentGuidsFromMatrix(ZGuid.BrettsGuid).ToList();

			// Assert
			AssertEquals(sql, "SELECT ISM_IM_Incident1, ISM_IM_Incident2 FROM dbo.IncidentSimilarityMatrix WHERE ISM_Version = @ver AND (ISM_IM_Incident1 = @guid1 OR ISM_IM_Incident2 = @guid2)");
			AssertEquals(10, result.Count);
		}

		public void GetIncidentsWithSimilarity()
		{
			var stmData = Factory.New<StmData>();
			var versionNumber = 234;
			stmData.SD_Name = "IncidentSimilarity_LatestVersion";
			stmData.SD_BinaryValue = BitConverter.GetBytes(versionNumber);
			Factory.Save();
			var similarIncidentRepositoryOne = new SimilarIncidentRepository(new IncidentAssociationQuery(Db.Connection), Factory);
			var similarIncidentRepositoryTwo = new SimilarIncidentRepository(new IncidentAssociationQuery(Db.Connection), versionNumber - 1, Factory);

			AssertEquals(versionNumber, similarIncidentRepositoryOne.RepositoryVersion);
			AssertEquals(versionNumber - 1, similarIncidentRepositoryTwo.RepositoryVersion);
			AssertEquals(similarIncidentRepositoryOne.GetLatestVersion(), versionNumber);
		}

		public void TestSearchStoredIncidentSimilarities()
		{
			var versionNumber = 234;
			var incidentGuids = SetupIncidents(versionNumber, DateTime.UtcNow.AddDays(-1));
			SetupIncidentSimilarityTfIdf(versionNumber, incidentGuids, DateTime.UtcNow.AddDays(-1));
			var similarIncidentSearchOptions = new SimilarIncidentSearchOptions();
			similarIncidentSearchOptions.SourceIncidentPK = incidentGuids[0];
			similarIncidentSearchOptions.FromTime = DateTime.Now.AddDays(-5);
			similarIncidentSearchOptions.ToTime = DateTime.Now.AddDays(5);

			SetupIncidentSimilarityMatrix(versionNumber, incidentGuids);
			var similarIncidentRepository = new SimilarIncidentRepository(new IncidentAssociationQuery(Db.Connection), Factory);

			var incidentMatrixList = similarIncidentRepository.SearchStoredIncidentSimilarities(similarIncidentSearchOptions).ToList();
			AssertEquals(incidentMatrixList.Count, 3);
		}

		public void TestSearchStoredIncidentSimilaritiesProduct()
		{
			var versionNumber = 234;
			var incidentGuids = SetupIncidents(versionNumber, DateTime.UtcNow.AddDays(-1));
			SetupIncidentSimilarityTfIdf(versionNumber, incidentGuids, DateTime.UtcNow.AddDays(-1));
			var similarIncidentSearchOptions = new SimilarIncidentSearchOptions();
			similarIncidentSearchOptions.SourceIncidentPK = incidentGuids[0];
			similarIncidentSearchOptions.Product = "PRC";

			SetupIncidentSimilarityMatrix(versionNumber, incidentGuids);
			var similarIncidentRepository = new SimilarIncidentRepository(new IncidentAssociationQuery(Db.Connection), Factory);

			var incidentMatrixList = similarIncidentRepository.SearchStoredIncidentSimilarities(similarIncidentSearchOptions).ToList();
			AssertEquals(2,incidentMatrixList.Count);
		}

		public void TestSearchStoredIncidentSimilaritiesProductArea()
		{
			var versionNumber = 234;
			var incidentGuids = SetupIncidents(versionNumber, DateTime.UtcNow.AddDays(-1));
			SetupIncidentSimilarityTfIdf(versionNumber, incidentGuids, DateTime.UtcNow.AddDays(-1));
			var similarIncidentSearchOptions = new SimilarIncidentSearchOptions();
			similarIncidentSearchOptions.SourceIncidentPK = incidentGuids[0];
			similarIncidentSearchOptions.ProductArea = "PRA";

			SetupIncidentSimilarityMatrix(versionNumber, incidentGuids);
			var similarIncidentRepository = new SimilarIncidentRepository(new IncidentAssociationQuery(Db.Connection), Factory);

			var incidentMatrixList = similarIncidentRepository.SearchStoredIncidentSimilarities(similarIncidentSearchOptions).ToList();
			AssertEquals(1,incidentMatrixList.Count);
		}

		public void TestSearchStoredIncidentSimilaritiesMinimumSimilarity()
		{
			var versionNumber = 234;
			var incidentGuids = SetupIncidents(versionNumber, DateTime.UtcNow.AddDays(-1));
			SetupIncidentSimilarityTfIdf(versionNumber, incidentGuids, DateTime.UtcNow.AddDays(-1));
			var similarIncidentSearchOptions = new SimilarIncidentSearchOptions();
			similarIncidentSearchOptions.SourceIncidentPK = incidentGuids[0];
			similarIncidentSearchOptions.MinimumSimilarity = 0.4;
			similarIncidentSearchOptions.FromTime = DateTime.Now.AddDays(-5);
			similarIncidentSearchOptions.ToTime = DateTime.Now.AddDays(5);

			SetupIncidentSimilarityMatrix(versionNumber, incidentGuids);
			var similarIncidentRepository = new SimilarIncidentRepository(new IncidentAssociationQuery(Db.Connection), Factory);

			var incidentMatrixList = similarIncidentRepository.SearchStoredIncidentSimilarities(similarIncidentSearchOptions).ToList();
			AssertEquals(incidentMatrixList.Count, 1);
			AssertEquals(incidentMatrixList[0].ISM_IM_Incident1, incidentGuids[0]);
			AssertEquals(incidentMatrixList[0].ISM_IM_Incident2, incidentGuids[1]);
			AssertEquals(incidentMatrixList[0].ISM_Version, versionNumber);
			AssertEquals(incidentMatrixList[0].ISM_Similarity, new decimal(0.6));
		}

		public void TestSearchStoredIncidentSimilaritiesNoSource()
		{
			var versionNumber = 234;
			var incidentGuids = SetupIncidents(versionNumber, DateTime.UtcNow.AddDays(-1));
			var incidentSimilarityTfIdfList = SetupIncidentSimilarityTfIdf(versionNumber, incidentGuids, DateTime.UtcNow.AddDays(-1));
			var similarIncidentSearchOptions = new SimilarIncidentSearchOptions();
			similarIncidentSearchOptions.MinimumSimilarity = 0.4;
			similarIncidentSearchOptions.FromTime = DateTime.Now.AddDays(-5);
			similarIncidentSearchOptions.ToTime = DateTime.Now.AddDays(5);

			var incidentSimilarityMatrixList = SetupIncidentSimilarityMatrix(versionNumber, incidentGuids);
			var similarIncidentRepository = new SimilarIncidentRepository(new IncidentAssociationQuery(Db.Connection), Factory);

			AssertExceptionThrown($"'{nameof(similarIncidentSearchOptions.SourceIncidentPK)}' property must be set in '{nameof(similarIncidentSearchOptions)}' parameter.",
						 typeof(NoNullAllowedException),
						 () => similarIncidentRepository.SearchStoredIncidentSimilarities(similarIncidentSearchOptions));
		}
	}
}
