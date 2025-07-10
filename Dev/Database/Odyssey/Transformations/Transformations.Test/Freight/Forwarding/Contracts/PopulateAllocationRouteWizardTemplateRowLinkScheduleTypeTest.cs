using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(PopulateAllocationRouteWizardTemplateRowLinkScheduleType))]
	class PopulateAllocationRouteWizardTemplateRowLinkScheduleTypeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateAllocationRouteWizardTemplateRowLinkScheduleType();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "AllocationRouteWizardTemplateRow", "ARR_HasLinkedSchedule", "BIT", "0");
			var testDataCreator = new TransformationTestDataCreator();
			var headerPK = testDataCreator.CreateAllocationRouteWizardTemplateHeader("MyHeader");
			rowWithNoLinkedSchedule = testDataCreator.CreateAllocationRouteWizardTemplateRow(headerPK, 0, hasLinkedSchedule: false);
			rowWithLinkedSchedule = testDataCreator.CreateAllocationRouteWizardTemplateRow(headerPK, 1, hasLinkedSchedule: true);
		}

		Guid rowWithNoLinkedSchedule;
		Guid rowWithLinkedSchedule;

		protected override void AssertTransformationResults()
		{
			AssertEquals("DNL", GetLinkScheduleType(rowWithNoLinkedSchedule));
			AssertEquals("ANY", GetLinkScheduleType(rowWithLinkedSchedule));
		}

		string GetLinkScheduleType(Guid rowPK)
		{
			var selectTransformationResultCommand = "SELECT ARR_LinkScheduleType FROM dbo.AllocationRouteWizardTemplateRow WHERE ARR_PK = @ARR_PK";

			using (var command = Db.Connection.Command(selectTransformationResultCommand))
			{
				command.AddParameter("@ARR_PK", System.Data.SqlDbType.UniqueIdentifier, rowPK);
				return command.ExecuteScalar() as string;
			}
		}
	}
}
