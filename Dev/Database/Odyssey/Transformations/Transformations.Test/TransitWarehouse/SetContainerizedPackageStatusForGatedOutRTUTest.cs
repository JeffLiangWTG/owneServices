using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(SetContainerizedPackageStatusForGatedOutRTU))]
	public class SetContainerizedPackageStatusForGatedOutRTUTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var rtu = new WhsItemReceiveTransportationUnit(whs, "RTU0001", location, "CNT0001", "CNT");
			rtu.WRH_GateInTime = new DateTimeOffset(2023, 11, 16, 10, 39, 0, 0, System.TimeSpan.Zero);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = new DateTimeOffset(2023, 11, 17, 10, 39, 0, 0, System.TimeSpan.Zero);
			rtu.WRH_UnloadCompleteTime = new DateTimeOffset(2023, 11, 18, 10, 39, 0, 0, System.TimeSpan.Zero);
			rtu.WRH_GateOutTime = new DateTimeOffset(2023, 11, 19, 10, 39, 0, 0, System.TimeSpan.Zero);
			rtu.AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rtu.PK, "PJ00001", "WRH").AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(packageJob, "UNT", 1).AppendInsertAndReturnObject(sql);
			var packageExtension = new PkgPackageExtension(rtu.PK, "WRH", package.PK).AppendInsertAndReturnObject(sql);
			containerizedPackageState = new WhsItemPackageState(package.PK, whs, null, "GIN", "SEC") { WPS_IsSecure = true, WPS_IsHandlingUnit = true }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			WhsItemPackageState.AssertFromDB(TestConnection, containerizedPackageState.PK)
				.ExpectEquals("Containerized Package State should become DEP status when RTU is gated out", p => p.WPS_Status, "DEP")
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new SetContainerizedPackageStatusForGatedOutRTU();

		public override string[] expectedIndex => new string[] {
			"NONCLUSTERED INDEX [_WTG__Set WPS_Status of containerized package to DEP if its RTU is gated out_1] ON [dbo].[WhsItemReceiveTransportationUnit] ([WRH_GateOutTime]) INCLUDE ([WRH_PK]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set WPS_Status of containerized package to DEP if its RTU is gated out_2] ON [dbo].[PkgPackageExtension] ([KPN_ParentTableCode]) INCLUDE ([KPN_KP_Package], [KPN_ParentID]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		WhsItemPackageState containerizedPackageState;
	}
}
