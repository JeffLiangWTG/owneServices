using System.Threading;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(UpdateEQ_DateReceivedColumnType))]
	class UpdateEQ_DateReceivedColumnTypeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateEQ_DateReceivedColumnType();

		protected override void PrepareTestData()
		{
		}

		protected override void AssertTransformationResults()
		{
			Assert(true);
		}

		public void TestDroppingTemporaryColumnIfExisting()
		{
			var tempColumnName = "_DTO_EQ_DateReceived";
			TestConnection.ExecuteNonQuery(@$"
ALTER TABLE dbo.JobRequiredDocument
ADD {tempColumnName} datetimeoffset(0) NULL");
			var triggerName = "TG_JobRequiredDocument_KeepEQ_DateReceivedInSync";
			var triggerDefinition = @$"
CREATE TRIGGER {triggerName}
ON dbo.JobRequiredDocument
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
END".Trim();
			TestConnection.ExecuteNonQuery(triggerDefinition);

			var fromTimeLog = "ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.FromTime";
			var toTimeLog = "ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.ToTime";
			var statusLog = "ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.Status";

			ExtProperty.Database.Update(TestConnection, fromTimeLog, "2025-01-01");
			ExtProperty.Database.Update(TestConnection, toTimeLog, "2025-12-01");
			ExtProperty.Database.Update(TestConnection, statusLog, "Finished");

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Should have dropped the tempt column", false, DbObjectCreator.ColumnExists(TestConnection, JobRequiredDocumentSchema.Constants.TableName, tempColumnName));
			AssertEquals("Should have dropped the tempt trigger.", false, DbObjectCreator.TriggerExists(TestConnection, JobRequiredDocumentSchema.Constants.TableName, triggerName));
			AssertNull("No ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.FromTime", ExtProperty.Database.Select(TestConnection, fromTimeLog));
			AssertNull("No ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.ToTime", ExtProperty.Database.Select(TestConnection, toTimeLog));
			AssertNull("No ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.Status", ExtProperty.Database.Select(TestConnection, statusLog));
		}
	}
}
