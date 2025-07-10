using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.ContainerYard
{
	[TestedType(typeof(UpdatePickupAndReadyDateForROILine))]
	public class UpdatePickupAndReadyDateForROILineTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var orgHeader = new OrgHeader("ORG").AppendInsertAndReturnObject(sql);
			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);

			fromDate = DateTime.Now.Date.AddDays(10);
			toDate = DateTime.Now.Date.AddDays(20);
			var releaseAdvice = new CYDReleaseAdvice(warehouse, "JOB001", fromDate, toDate).AppendInsertAndReturnObject(sql);
			var unitLine = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			new CYDReleaseAdviceLine(releaseAdvice, refContainer) { YEL_YLI_UnitLineItem = unitLine }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			var releaseAdviceLines = CYDReleaseAdviceLine.ShallowLoadFromDB(TestConnection);

			var releaseAdviceLinesWithNullDateCount = releaseAdviceLines.Count(r => r.YEL_PickupDate == null || r.YEL_ReadyDate == null);
			AssertEquals("These two date fields should not be null", 0, releaseAdviceLinesWithNullDateCount);

			var pickupDate = releaseAdviceLines.Single(r => r.YEL_PickupDate != null).YEL_PickupDate;
			AssertEquals("Pick up date must be updated to release advice's To Date", toDate, pickupDate);

			var readyDate = releaseAdviceLines.Single(r => r.YEL_ReadyDate != null).YEL_ReadyDate;
			AssertEquals("Ready date must be updated to release advice's To Date", toDate, readyDate);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePickupAndReadyDateForROILine();

		DateTime toDate;

		DateTime fromDate;
	}
}
