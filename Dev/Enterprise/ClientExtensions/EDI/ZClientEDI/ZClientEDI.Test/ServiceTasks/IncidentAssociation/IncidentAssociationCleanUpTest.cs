using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.Test
{
	public class IncidentAssociationCleanUpTest : TestCaseWithFactory
	{
		readonly int currentVersion = 2;

		public void TestCleanupOldAssociations()
		{
			// Arrange
			var metaISVProvider = new MetaIncidentSimilarityTfIdfProvider();
			double[] doubleArray = { 1, 0, 1 };
			metaISVProvider.Save(new List<MetaIncidentSimilarityTfIdf>
			{
				new MetaIncidentSimilarityTfIdf
				{
					IncidentGuid = new Guid("00000000-0000-0000-0001-000000000000"),
					Version = currentVersion,
					Status = IncidentSimilarityTfIdf.Status.TermFrequencyComputed,
					IncidentLastModified = DateTime.UtcNow.AddMonths(-EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value - 1),
					TermFrequency = doubleArray.ToList(),
				},
				new MetaIncidentSimilarityTfIdf
				{
					IncidentGuid = new Guid("00000000-0000-0000-0002-000000000000"),
					Version = currentVersion,
					Status = IncidentSimilarityTfIdf.Status.TermFrequencyComputed,
					IncidentLastModified = DateTime.UtcNow,
					TermFrequency = doubleArray.ToList(),
				},
				new MetaIncidentSimilarityTfIdf
				{
					IncidentGuid = new Guid("00000000-0000-0000-0003-000000000000"),
					Version = currentVersion,
					Status = IncidentSimilarityTfIdf.Status.TermFrequencyComputed,
					IncidentLastModified = DateTime.UtcNow,
					TermFrequency = doubleArray.ToList(),
				},
			});

			var similarityMatrixBootstrapperQueryExecutors = new SimilarityMatrixBootstrapperQueryExecutor();
			similarityMatrixBootstrapperQueryExecutors.Commit(
				new List<SimilarityMatrixDataTransferObject>
				{
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0001-0000-000000000000"),
						new Guid("00000000-0000-0000-0001-000000000000"),
						new Guid("00000000-0000-0000-0001-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0002-0000-000000000000"),
						new Guid("00000000-0000-0000-0001-000000000000"),
						new Guid("00000000-0000-0000-0002-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0003-0000-000000000000"),
						new Guid("00000000-0000-0000-0001-000000000000"),
						new Guid("00000000-0000-0000-0003-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0004-0000-000000000000"),
						new Guid("00000000-0000-0000-0002-000000000000"),
						new Guid("00000000-0000-0000-0001-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0005-0000-000000000000"),
						new Guid("00000000-0000-0000-0002-000000000000"),
						new Guid("00000000-0000-0000-0002-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0006-0000-000000000000"),
						new Guid("00000000-0000-0000-0002-000000000000"),
						new Guid("00000000-0000-0000-0003-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0007-0000-000000000000"),
						new Guid("00000000-0000-0000-0003-000000000000"),
						new Guid("00000000-0000-0000-0001-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0009-0000-000000000000"),
						new Guid("00000000-0000-0000-0003-000000000000"),
						new Guid("00000000-0000-0000-0002-000000000000"),
						currentVersion,
						0.5),
					new SimilarityMatrixDataTransferObject(
						new Guid("00000000-0000-0010-0000-000000000000"),
						new Guid("00000000-0000-0000-0003-000000000000"),
						new Guid("00000000-0000-0000-0003-000000000000"),
						currentVersion,
						0.5),
				},
				new Guid("00000000-0000-0000-0001-000000000000"));

			// Act
			new IncidentAssociationNewIncidentsRunner(null).CleanUpOldIncidentAssociations();

			// Assert
			Factory.ReloadAll<IncidentSimilarityTfIdf>();
			var query = new ZQuery();
			query.OrderBy = AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident;
			var tfIdfs = Factory.Load<IncidentSimilarityTfIdf>(query);
			AssertEquals(2, tfIdfs.Length);
			AssertEquals(new Guid("00000000-0000-0000-0002-000000000000"), tfIdfs[0].ISV_IM_Incident);
			AssertEquals(new Guid("00000000-0000-0000-0003-000000000000"), tfIdfs[1].ISV_IM_Incident);

			Factory.ReloadAll<IncidentSimilarityMatrix>();
			query = new ZQuery();
			query.OrderBy = AutoIncidentSimilarityMatrix.Schema.PK;
			var similarities = Factory.Load<IncidentSimilarityMatrix>(query);
			AssertEquals(4, similarities.Length);
			AssertEquals(new Guid("00000000-0000-0005-0000-000000000000"), similarities[0].PK);
			AssertEquals(new Guid("00000000-0000-0006-0000-000000000000"), similarities[1].PK);
			AssertEquals(new Guid("00000000-0000-0009-0000-000000000000"), similarities[2].PK);
			AssertEquals(new Guid("00000000-0000-0010-0000-000000000000"), similarities[3].PK);
		}
	}
}
