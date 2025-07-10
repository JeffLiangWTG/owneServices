using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(OrgSupplierPartUnitsPerPallet))]
	class OrgSupplierPartUnitsPerPalletTest : DbCreateScriptTest
	{
		#region TestNoArithmeticOverflowIsThrown_WhenPalletSizeIsHuge

		public void TestNoArithmeticOverflowIsThrown_WhenPalletSizeIsHuge()
		{
			// setup a Pallet size of 270 Quintillion (270000000000000000000), which will overflow our UnitsPerPallet Function
			// this was an actual setup on UAT GP1
			var part = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(part, 30000000, "UNT", "BOX").Insert(TestConnection);
			new OrgPartUnit(part, 1000000, "BOX", "CTN").Insert(TestConnection);
			new OrgPartUnit(part, 9000000, "CTN", "PLT").Insert(TestConnection);

			AssertEquals("Overflow result should return null.", DBNull.Value, TestConnection.ExecuteScalar($"SELECT UnitsPerPallet FROM OrgSupplierPartUnitsPerPallet({part.PK.AsSQL()})"));
		}

		#endregion

		#region TestNoDivisionByZeroIsThrown_WhenQuantityIs0

		public void TestNoDivisionByZeroIsThrown_WhenQuantityIs0()
		{
			var part = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(part, 10, "UNT", "CAS").Insert(TestConnection);
			new OrgPartUnit(part, 0, "CAS", "PLT").Insert(TestConnection);

			AssertEquals("Division by 0 result should return 0.", 0.0m, TestConnection.ExecuteScalar($"SELECT UnitsPerPallet FROM OrgSupplierPartUnitsPerPallet({part.PK.AsSQL()})"));
		}

		#endregion

		#region TestZeroResult_WhenQuantityIsNegative

		public void TestZeroResult_WhenQuantityIsNegative()
		{
			var part = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(part, 10, "UNT", "CAS").Insert(TestConnection);
			new OrgPartUnit(part, -5, "CAS", "PLT").Insert(TestConnection);

			AssertEquals("Negative quantity in parent should result in 0.", 0.0m, TestConnection.ExecuteScalar($"SELECT UnitsPerPallet FROM OrgSupplierPartUnitsPerPallet({part.PK.AsSQL()})"));
		}
		#endregion
	}
}

