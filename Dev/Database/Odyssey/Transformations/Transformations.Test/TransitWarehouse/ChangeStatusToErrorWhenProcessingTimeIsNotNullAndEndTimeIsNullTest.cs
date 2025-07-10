using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(ChangeStatusToErrorWhenProcessingTimeIsNotNullAndEndTimeIsNull))]
	public class ChangeStatusToErrorWhenProcessingTimeIsNotNullAndEndTimeIsNullTest : DataTransformationTestCase
	{
		Guid ProcessingLocationPK;
		Guid CompletedLocationPK;
		Guid InProcessLocationPK;
		Guid ErrorLocation_OPNVariancePK;
		Guid ErrorLocation_APPVariancePK;
		Guid ErrorLocation_REJVariancePK;
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
			var location3 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 3 }.AppendInsertAndReturnObject(sql);

			var inProcessCycleCountLocation = new WhsItemCycleCountLocation(location1.PK, jobID: "CC001") { WIC_GS_NKAssignedTo = "AAA", WIC_Status = "INP", WIC_StartTime = now.AddMinutes(-10) }.AppendInsertAndReturnObject(sql);
			InProcessLocationPK = inProcessCycleCountLocation.PK;

			var processingCycleCountLocation = new WhsItemCycleCountLocation(location2.PK, jobID: "CC002") { WIC_GS_NKAssignedTo = "AAA", WIC_Status = "PCV", WIC_StartTime = now.AddMinutes(-10), WIC_ProcessingTime = now.AddMinutes(-5) }.AppendInsertAndReturnObject(sql);
			ProcessingLocationPK = processingCycleCountLocation.PK;

			var completedCountLocation = new WhsItemCycleCountLocation(location3.PK, jobID: "CC003") { WIC_GS_NKAssignedTo = "AAA", WIC_Status = "CMP", WIC_StartTime = now.AddMinutes(-10), WIC_ProcessingTime = now.AddMinutes(-5), WIC_EndTime = now }.AppendInsertAndReturnObject(sql);
			CompletedLocationPK = completedCountLocation.PK;

			var errorCountLocation = new WhsItemCycleCountLocation(location3.PK, jobID: "CC004") { WIC_GS_NKAssignedTo = "AAA", WIC_Status = "ERR", WIC_StartTime = now.AddMinutes(-10), WIC_ProcessingTime = now.AddMinutes(-5) }.AppendInsertAndReturnObject(sql);

			ErrorLocation_OPNVariancePK = new WhsItemCycleCountLocationVariance(errorCountLocation, "OPN").PK;
			ErrorLocation_APPVariancePK = new WhsItemCycleCountLocationVariance(errorCountLocation, "APP").PK;
			ErrorLocation_REJVariancePK = new WhsItemCycleCountLocationVariance(errorCountLocation, "REJ").PK;

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			WhsItemCycleCountLocation.AssertFromDB(TestConnection, InProcessLocationPK)
				.ExpectEquals("InProcessLocation's Status should be INP.", cycleCountLocation => cycleCountLocation.WIC_Status, "INP")
				.VerifyAll();

			WhsItemCycleCountLocation.AssertFromDB(TestConnection, ProcessingLocationPK)
				.ExpectEquals("ProcessingLocationPK's Status should be ERR.", cycleCountLocation => cycleCountLocation.WIC_Status, "ERR")
				.VerifyAll();

			WhsItemCycleCountLocation.AssertFromDB(TestConnection, CompletedLocationPK)
				.ExpectEquals("CompletedLocation's Status should be CMP.", cycleCountLocation => cycleCountLocation.WIC_Status, "CMP")
				.VerifyAll();

			AssertEquals("All errorLocation's variances should be remove.", false, WhsItemCycleCountLocationVariance.ExistsInDB(TestConnection, ErrorLocation_OPNVariancePK));
			AssertEquals("All errorLocation's variances should be remove.", false, WhsItemCycleCountLocationVariance.ExistsInDB(TestConnection, ErrorLocation_APPVariancePK));
			AssertEquals("All errorLocation's variances should be remove.", false, WhsItemCycleCountLocationVariance.ExistsInDB(TestConnection, ErrorLocation_REJVariancePK));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ChangeStatusToErrorWhenProcessingTimeIsNotNullAndEndTimeIsNull();
	}
}
