using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(UpdateSupplierBookingSums))]
	internal sealed class UpdateSupplierBookingSumsTest : DbCreateScriptTest
	{
		SupplierBookingGeneratorForTests generator;

		struct SumValues
		{
			public decimal GrossWeightInKg { get; set; }
			public decimal CubicInM3 { get; set; }
			public int PiecesManifested { get; set; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			generator = new SupplierBookingGeneratorForTests(TestConnection);
		}

		public void TestResultIsCorrectForNoLines()
		{
			var consignorPk = generator.GenerateAddress();

			var headerPk = generator.NewSupplierBookingHeader("Test", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");

			UpdateSums(headerPk);
			var sumValues = GetSumValues(headerPk);

			AssertEquals(0, sumValues.PiecesManifested);
			AssertEquals(0m, sumValues.GrossWeightInKg);
			AssertEquals(0m, sumValues.CubicInM3);
		}

		public void TestResultIsCorrectForOneLine()
		{
			var consignorPk = generator.GenerateAddress();

			var headerPk = generator.NewSupplierBookingHeader("Test", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");

			generator.NewSupplierBookingLine(headerPk, "Test", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");

			UpdateSums(headerPk);
			var sumValues = GetSumValues(headerPk);

			AssertEquals(1, sumValues.PiecesManifested);
			AssertEquals(123.45m, sumValues.GrossWeightInKg);
			AssertEquals(99.9m, sumValues.CubicInM3);
		}

		public void TestResultIsCorrectWithSufficientPrecisionForMultipleLines()
		{
			var consignorPk = generator.GenerateAddress();

			var headerPk = generator.NewSupplierBookingHeader("Test", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");

			for (int i = 0; i < 20; i++)
			{
				generator.NewSupplierBookingLine(headerPk, "Test", 100.0m, "AUD", 1, "LB", 1, "CY", "TestDescription", "TestMarks",
					1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");
			}

			UpdateSums(headerPk);
			var sumValues = GetSumValues(headerPk);

			AssertEquals(20, sumValues.PiecesManifested);

			decimal expectedWeighSum = 20 * 0.45359237m;
			decimal expectedVolumeSum = 20 * 0.764554858m;

			AssertEquals(Math.Round(expectedWeighSum, 3), sumValues.GrossWeightInKg);
			AssertEquals(Math.Round(expectedVolumeSum, 3), sumValues.CubicInM3);
		}

		public void TestResultIsCorrectForMultipleLinesWithDifferentHeaders()
		{
			var consignorPk = generator.GenerateAddress();

			var headerPk1 = generator.NewSupplierBookingHeader("Test1", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var headerPk2 = generator.NewSupplierBookingHeader("Test2", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");

			generator.NewSupplierBookingLine(headerPk1, "Test", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");
			generator.NewSupplierBookingLine(headerPk1, "Test", 100.0m, "AUD", 100.0, "KG", 200.0, "M3", "TestDescription", "TestMarks",
				2, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");
			generator.NewSupplierBookingLine(headerPk2, "Test", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");

			UpdateSums(headerPk1);
			UpdateSums(headerPk2);

			var sumValues = GetSumValues(headerPk1);
			AssertEquals(3, sumValues.PiecesManifested);
			AssertEquals(223.45m, sumValues.GrossWeightInKg);
			AssertEquals(299.9m, sumValues.CubicInM3);

			sumValues = GetSumValues(headerPk2);
			AssertEquals(1, sumValues.PiecesManifested);
			AssertEquals(123.45m, sumValues.GrossWeightInKg);
			AssertEquals(99.9m, sumValues.CubicInM3);
		}

		public void TestResultIsCorrectIfNoHeaderPk()
		{
			var consignorPk = generator.GenerateAddress();

			var headerPk1 = generator.NewSupplierBookingHeader("Test1", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var headerPk2 = generator.NewSupplierBookingHeader("Test2", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");

			generator.NewSupplierBookingLine(headerPk1, "Test", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");
			generator.NewSupplierBookingLine(headerPk1, "Test", 100.0m, "AUD", 100.0, "KG", 200.0, "M3", "TestDescription", "TestMarks",
				2, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");
			generator.NewSupplierBookingLine(headerPk2, "Test", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");

			UpdateSums(null);

			var sumValues = GetSumValues(headerPk1);
			AssertEquals(3, sumValues.PiecesManifested);
			AssertEquals(223.45m, sumValues.GrossWeightInKg);
			AssertEquals(299.9m, sumValues.CubicInM3);

			sumValues = GetSumValues(headerPk2);
			AssertEquals(1, sumValues.PiecesManifested);
			AssertEquals(123.45m, sumValues.GrossWeightInKg);
			AssertEquals(99.9m, sumValues.CubicInM3);
		}

		public void TestResultIsCorrectIfLinesAreDeleted()
		{
			var consignorPk = generator.GenerateAddress();

			var headerPk = generator.NewSupplierBookingHeader("Test", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var linePk = generator.NewSupplierBookingLine(headerPk, "Test", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");

			UpdateSums(headerPk);
			var sumValues = GetSumValues(headerPk);
			AssertEquals(1, sumValues.PiecesManifested);

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.SupplierBookingLine WHERE DL_PK = '" + linePk + "'");

			UpdateSums(headerPk);
			sumValues = GetSumValues(headerPk);
			AssertEquals(0, sumValues.PiecesManifested);
		}

		#region Implementation

		static SumValues GetSumValues(Guid headerPK)
		{
			var sumValues = new SumValues();

			using (var command = Db.Connection.Command("SELECT DH_GrossWeightInKg, DH_CubicInM3, DH_PiecesManifested FROM dbo.SupplierBookingHeader WHERE DH_PK = '" + headerPK + "'"))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						sumValues.GrossWeightInKg = reader.GetDecimal(0);
						sumValues.CubicInM3 = reader.GetDecimal(1);
						sumValues.PiecesManifested = reader.GetInt32(2);
					}
				}
			}

			return sumValues;
		}

		static void UpdateSums(Guid? headerPK)
		{
			using (var command = Db.Connection.Command("UpdateSupplierBookingSums"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@HeaderPk", SqlDbType.UniqueIdentifier, headerPK);

				command.ExecuteNonQuery();
			}
		}
		#endregion
	}
}
