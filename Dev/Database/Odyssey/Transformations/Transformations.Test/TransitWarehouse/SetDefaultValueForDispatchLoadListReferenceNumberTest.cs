using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(SetDefaultValueForDispatchLoadListReferenceNumber))]
	public class SetDefaultValueForDispatchLoadListReferenceNumberTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var dll = new WhsItemDispatchLoadList("DLL001", whs, "test").AppendInsertAndReturnObject(sql);
			dllPK = dll.PK;

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			WhsItemDispatchLoadList.AssertFromDB(TestConnection, dllPK)
				.ExpectEquals("Should be set to jobID", p => new { p.WDL_ReferenceNumber }, new { WDL_ReferenceNumber = "DLL001" })
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new SetDefaultValueForDispatchLoadListReferenceNumber();

		Guid dllPK;
	}
}
