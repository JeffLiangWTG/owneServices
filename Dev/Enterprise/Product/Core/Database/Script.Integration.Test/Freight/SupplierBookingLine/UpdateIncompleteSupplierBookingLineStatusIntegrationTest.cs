using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.SupplierBookingLine.Testing
{
	internal sealed class UpdateIncompleteSupplierBookingLineStatusIntegrationTest : TransactionedTestCase
	{
		SupplierBookingGeneratorForTests generator;

		protected override void SetUp()
		{
			base.SetUp();
			generator = new SupplierBookingGeneratorForTests(TestConnection);
		}

		public void TestResultIsCorrectForOneLine()
		{
			var consignorPk = generator.GenerateAddress();

			var headerPk = generator.NewSupplierBookingHeader("Test", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");

			var linePk = generator.NewSupplierBookingLine(headerPk, "Test", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");

			UpdateLineStatus(headerPk, Constants.SupplierBookingLineStatus.Codes.Confirmed);
			var statusList = GetStatusOfLines(headerPk);

			AssertEquals(linePk, statusList.Single().Item1);
			AssertEquals(Constants.SupplierBookingLineStatus.Codes.Confirmed, statusList.Single().Item2);
		}

		public void TestResultIsCorrectForMultipleLines()
		{
			const string OtherStatus = "BBB";
			var consignorPk = generator.GenerateAddress();

			var headerPk1 = generator.NewSupplierBookingHeader("Test1", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var headerPk2 = generator.NewSupplierBookingHeader("Test2", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var headerPk3 = generator.NewSupplierBookingHeader("Test3", false, consignorPk, DateTime.Now, "ABC", DateTime.Now, "ABC");

			var linePk11 = generator.NewSupplierBookingLine(headerPk1, "Test11", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");
			var linePk12 = generator.NewSupplierBookingLine(headerPk1, "Test12", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", status: Constants.SupplierBookingLineStatus.Codes.Incomplete);
			var linePk13 = generator.NewSupplierBookingLine(headerPk1, "Test13", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", status: "FOO");
			var linePk21 = generator.NewSupplierBookingLine(headerPk2, "Test21", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");
			var linePk22 = generator.NewSupplierBookingLine(headerPk2, "Test22", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", status: Constants.SupplierBookingLineStatus.Codes.Incomplete);
			var linePk31 = generator.NewSupplierBookingLine(headerPk3, "Test31", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU");

			UpdateLineStatus(headerPk1, Constants.SupplierBookingLineStatus.Codes.Confirmed);
			UpdateLineStatus(headerPk2, OtherStatus);

			var statusList1 = GetStatusOfLines(headerPk1).ToDictionary(x => x.Item1, x => x.Item2);
			var statusList2 = GetStatusOfLines(headerPk2).ToDictionary(x => x.Item1, x => x.Item2);
			var statusList3 = GetStatusOfLines(headerPk3).ToDictionary(x => x.Item1, x => x.Item2);

			AssertEquals("statusList1.Count", 3, statusList1.Count);
			Assert("statusList1 contains line11", statusList1.ContainsKey(linePk11));
			Assert("statusList1 contains line12", statusList1.ContainsKey(linePk12));
			Assert("statusList1 contains line13", statusList1.ContainsKey(linePk13));
			AssertEquals("line11 status", Constants.SupplierBookingLineStatus.Codes.Confirmed, statusList1[linePk11]);
			AssertEquals("line12 status", Constants.SupplierBookingLineStatus.Codes.Confirmed, statusList1[linePk12]);
			AssertEquals("line13 status", "FOO", statusList1[linePk13]);

			AssertEquals("statusList2.Count", 2, statusList2.Count);
			Assert("statusList2 contains line21", statusList2.ContainsKey(linePk21));
			Assert("statusList2 contains line22", statusList2.ContainsKey(linePk22));
			AssertEquals("line21 status", OtherStatus, statusList2[linePk21]);
			AssertEquals("line22 status", OtherStatus, statusList2[linePk22]);

			AssertEquals("statusList3.Count", 1, statusList3.Count);
			Assert("statusList3 contains line31", statusList3.ContainsKey(linePk31));
			AssertNotEquals("line31 status", Constants.SupplierBookingLineStatus.Codes.Confirmed, statusList3[linePk31]);
			AssertNotEquals("line31 status", OtherStatus, statusList3[linePk31]);
		}

		#region Implementation

		static IEnumerable<Tuple<Guid, string>> GetStatusOfLines(Guid headerPK)
		{
			var result = new List<Tuple<Guid, string>>();

			using (var command = Db.Connection.Command("SELECT DL_PK, DL_Status FROM dbo.SupplierBookingLine WHERE DL_DH_BookingHeader = '" + headerPK + "'"))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(Tuple.Create(reader.GetGuid(0), reader.GetString(1)));
					}
				}
			}

			return result;
		}

		static void UpdateLineStatus(Guid? headerPK, string status)
		{
			using (var command = Db.Connection.Command("UpdateIncompleteSupplierBookingLineStatus"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@HeaderPk", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@IncompleteStatus", SqlDbType.VarChar, Constants.SupplierBookingLineStatus.Codes.Incomplete);

				if (!string.IsNullOrEmpty(status))
				{
					command.AddParameter("@Status", SqlDbType.VarChar, status);
				}

				command.ExecuteNonQuery();
			}
		}
		#endregion
	}
}
