using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(DeleteOrphanedVoyages))]
	public class DeleteOrphanedVoyagesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteOrphanedVoyages();
		}

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var containerPk = testDataCreator.CreateRefContainer("TANK", "TNK");
			var containerStockPk = testDataCreator.CreateRefContainerStock(containerPk);
			var orgPk = testDataCreator.CreateOrg("TESTORG");
			var companyPk = testDataCreator.CreateCompany("TST", "AU");
			var today = DateTime.Today;

			var voyagePk1 = testDataCreator.CreateJobVoyage("ABC123", "ABC123", false, string.Empty);
			var containerMove1 = testDataCreator.CreateJobContainerMove(voyagePk1, containerStockPk);
			var tradeLaneVoyagePk1 = testDataCreator.CreateJobTradeLaneVoyage(voyagePk1, orgPk);
			var accountPk1 = testDataCreator.CreateJobVoyAccount(voyagePk1, orgPk, companyPk);
			var voyageExRatePk1 = testDataCreator.CreateJobVoyageExRate(voyagePk1, companyPk);
			var country1 = testDataCreator.CreateJobVoyCountry(voyagePk1);
			var originPk11 = testDataCreator.CreateJobVoyOrigin(voyagePk1, "GBLON", null, string.Empty, today, today, today);
			var originPk12 = testDataCreator.CreateJobVoyOrigin(voyagePk1, "TWKHH", null, string.Empty, today, today, today);
			var destinationPk11 = testDataCreator.CreateJobVoyDestination(voyagePk1, "GGSTS", null, string.Empty, today, today, today);
			var destinationPk12 = testDataCreator.CreateJobVoyDestination(voyagePk1, "AUSYD", null, string.Empty, today, today, today);
			var sailingPk11 = testDataCreator.CreateSailing(originPk11, destinationPk11, string.Empty, string.Empty);

			recordsThatShouldNotBeDeleted = new List<(ITableSchema, Guid)>();
			recordsThatShouldNotBeDeleted.Add((JobVoyageSchema.Instance, voyagePk1));
			recordsThatShouldNotBeDeleted.Add((JobContainerMoveSchema.Instance, containerMove1));
			recordsThatShouldNotBeDeleted.Add((JobTradeLaneVoyageSchema.Instance, tradeLaneVoyagePk1));
			recordsThatShouldNotBeDeleted.Add((JobVoyAccountSchema.Instance, accountPk1));
			recordsThatShouldNotBeDeleted.Add((JobVoyageExRateSchema.Instance, voyageExRatePk1));
			recordsThatShouldNotBeDeleted.Add((JobVoyCountrySchema.Instance, country1));
			recordsThatShouldNotBeDeleted.Add((JobVoyOriginSchema.Instance, originPk11));
			recordsThatShouldNotBeDeleted.Add((JobVoyOriginSchema.Instance, originPk12));
			recordsThatShouldNotBeDeleted.Add((JobVoyDestinationSchema.Instance, destinationPk11));
			recordsThatShouldNotBeDeleted.Add((JobVoyDestinationSchema.Instance, destinationPk12));
			recordsThatShouldNotBeDeleted.Add((JobSailingSchema.Instance, sailingPk11));

			var voyagePk2 = testDataCreator.CreateJobVoyage("DEF123", "DEF123", false, string.Empty);
			var containerMove2 = testDataCreator.CreateJobContainerMove(voyagePk2, containerStockPk);
			var tradeLaneVoyagePk2 = testDataCreator.CreateJobTradeLaneVoyage(voyagePk2, orgPk);
			var accountPk2 = testDataCreator.CreateJobVoyAccount(voyagePk2, orgPk, companyPk);
			var voyageExRatePk2 = testDataCreator.CreateJobVoyageExRate(voyagePk2, companyPk);
			var country2 = testDataCreator.CreateJobVoyCountry(voyagePk2);
			var originPk2 = testDataCreator.CreateJobVoyOrigin(voyagePk2, "ZAJNB", null, string.Empty, today, today, today);
			var destinationPk2 = testDataCreator.CreateJobVoyDestination(voyagePk2, "USNYC", null, string.Empty, today, today, today);

			recordsThatShouldNotBeDeleted.Add((JobVoyageSchema.Instance, voyagePk2));
			recordsThatShouldNotBeDeleted.Add((JobContainerMoveSchema.Instance, containerMove2));
			recordsThatShouldNotBeDeleted.Add((JobTradeLaneVoyageSchema.Instance, tradeLaneVoyagePk2));
			recordsThatShouldNotBeDeleted.Add((JobVoyAccountSchema.Instance, accountPk2));
			recordsThatShouldNotBeDeleted.Add((JobVoyageExRateSchema.Instance, voyageExRatePk2));
			recordsThatShouldNotBeDeleted.Add((JobVoyCountrySchema.Instance, country2));
			recordsThatShouldNotBeDeleted.Add((JobVoyOriginSchema.Instance, originPk2));
			recordsThatShouldNotBeDeleted.Add((JobVoyDestinationSchema.Instance, destinationPk2));

			var voyagePk3 = testDataCreator.CreateJobVoyage("GHI123", "GHI123", false, string.Empty);
			var tradeLaneVoyagePk3 = testDataCreator.CreateJobTradeLaneVoyage(voyagePk3, orgPk);
			var voyageExRatePk3 = testDataCreator.CreateJobVoyageExRate(voyagePk3, companyPk);
			var country3 = testDataCreator.CreateJobVoyCountry(voyagePk3);
			var originPk3 = testDataCreator.CreateJobVoyOrigin(voyagePk3, "GBMNC", null, string.Empty, today, today, today);
			var destinationPk3 = testDataCreator.CreateJobVoyDestination(voyagePk3, "USLAX", null, string.Empty, today, today, today);

			recordsThatShouldBeDeleted = new List<(ITableSchema, Guid)>();
			recordsThatShouldBeDeleted.Add((JobVoyageSchema.Instance, voyagePk3));
			recordsThatShouldBeDeleted.Add((JobTradeLaneVoyageSchema.Instance, tradeLaneVoyagePk3));
			recordsThatShouldBeDeleted.Add((JobVoyageExRateSchema.Instance, voyageExRatePk3));
			recordsThatShouldBeDeleted.Add((JobVoyCountrySchema.Instance, country3));
			recordsThatShouldBeDeleted.Add((JobVoyOriginSchema.Instance, originPk3));
			recordsThatShouldBeDeleted.Add((JobVoyDestinationSchema.Instance, destinationPk3));
		}

		protected override void AssertTransformationResults()
		{
			foreach (var record in recordsThatShouldNotBeDeleted)
			{
				AssertEquals(true, DoesRecordExist(record.schema, record.pk));
			}

			foreach (var record in recordsThatShouldBeDeleted)
			{
				AssertEquals(false, DoesRecordExist(record.schema, record.pk));
			}
		}

		bool DoesRecordExist(ITableSchema schema, Guid pk)
		{
			var sql = $@"
SELECT CASE
	WHEN EXISTS (SELECT * FROM {schema.TableName} WHERE {schema.PK.Name} = @pk) THEN CAST (1 AS BIT)
	ELSE CAST (0 AS BIT)
	END";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				return (bool)cmd.ExecuteScalar();
			}
		}

		List<(ITableSchema schema, Guid pk)> recordsThatShouldNotBeDeleted;
		List<(ITableSchema schema, Guid pk)> recordsThatShouldBeDeleted;
	}
}
