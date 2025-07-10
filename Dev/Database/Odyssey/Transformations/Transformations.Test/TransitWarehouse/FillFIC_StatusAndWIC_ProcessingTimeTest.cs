using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(FillFIC_StatusAndWIC_ProcessingTime))]
	public class FillFIC_StatusAndWIC_ProcessingTimeTest : DataTransformationTestCase
	{
		Guid NotStartedLocationPK;
		Guid InProcessLocationPK;
		Guid CompletedLocationPK;
		readonly DateTime now = DateTime.Now;

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouseOld_V01("WH1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var notStartedLocation = new WhsItemCycleCountLocation(location1.PK, jobID: "CC001") { WIC_GS_NKAssignedTo = "AAA", WIC_Status = "" }.AppendInsertAndReturnObject(sql);
			NotStartedLocationPK = notStartedLocation.PK;

			var inProcessCountLocation = new WhsItemCycleCountLocation(location1.PK, jobID: "CC002") { WIC_GS_NKAssignedTo = "AAA", WIC_Status = "", WIC_StartTime = now.AddMinutes(-10) }.AppendInsertAndReturnObject(sql);
			InProcessLocationPK = inProcessCountLocation.PK;

			var completedCountLocation = new WhsItemCycleCountLocation(location2.PK, jobID: "CC003") { WIC_GS_NKAssignedTo = "AAA", WIC_Status = "", WIC_StartTime = now.AddMinutes(-10), WIC_EndTime = now }.AppendInsertAndReturnObject(sql);
			CompletedLocationPK = completedCountLocation.PK;

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsItemCycleCountLocationSchema.Constants.SqlSchemaName, WhsItemCycleCountLocationSchema.Constants.TableName, "Constraint_WIC_Status"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsItemCycleCountLocationSchema.Constants.SqlSchemaName, WhsItemCycleCountLocationSchema.Constants.TableName, "Constraint_WIC_ProcessingTimeMustNotBeLaterThanStartTimeAndEarlierThanEndTime"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsItemCycleCountLocationSchema.Constants.SqlSchemaName, WhsItemCycleCountLocationSchema.Constants.TableName, "Constraint_WIC_Status_StartTimeAndEndTime"))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		protected override void AssertTransformationResults()
		{
			WhsItemCycleCountLocation.AssertFromDB(TestConnection, NotStartedLocationPK)
				.ExpectEquals("NotStartedLocation's Status should be NST.", cycleCountLocation => cycleCountLocation.WIC_Status, "NST")
				.ExpectEquals("NotStartedLocation's ProcessingTime should be null.", cycleCountLocation => cycleCountLocation.WIC_ProcessingTime, null)
				.VerifyAll();

			WhsItemCycleCountLocation.AssertFromDB(TestConnection, InProcessLocationPK)
				.ExpectEquals("InProcessLocation's Status should be INP.", cycleCountLocation => cycleCountLocation.WIC_Status, "INP")
				.ExpectEquals("InProcessLocation's ProcessingTime should be null.", cycleCountLocation => cycleCountLocation.WIC_ProcessingTime, null)
				.VerifyAll();

			WhsItemCycleCountLocation.AssertFromDB(TestConnection, CompletedLocationPK)
				.ExpectEquals("CompletedLocation's Status should be CMP.", cycleCountLocation => cycleCountLocation.WIC_Status, "CMP")
				.ExpectEquals("CompletedLocation's ProcessingTime should be not null.", cycleCountLocation => cycleCountLocation.WIC_ProcessingTime, now)
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new FillFIC_StatusAndWIC_ProcessingTime();
	}
}
