using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(RecentCommissionsMatrix))]
	public class RecentCommissionsMatrixTest : NonPersistentBusinessObjectCollectionTestCase<RecentCommissionsMatrix>
	{
		#region Allowed Actions

		public void TestAllowNew()
		{
			var matrix = GetNewRecentCommissionMatrix();
			AssertEquals(false, matrix.AllowNew);
		}

		public void TestAllowRemove()
		{
			var matrix = GetNewRecentCommissionMatrix();
			AssertEquals(false, matrix.AllowRemove);
		}

		#endregion Allowed Actions

		#region Refresh

		public void TestRefresh()
		{
			CreateNewCommissionLine(new ZDate(2016, 1, 1), AccCommissionLineCommissionStatusList.Codes.Approved, 1);
			CreateNewCommissionLine(new ZDate(2016, 1, 31), AccCommissionLineCommissionStatusList.Codes.Approved, 2);
			CreateNewCommissionLine(new ZDate(2016, 2, 1), AccCommissionLineCommissionStatusList.Codes.Approved, 3);
			CreateNewCommissionLine(new ZDate(2016, 2, 28), AccCommissionLineCommissionStatusList.Codes.Approved, 4);
			CreateNewCommissionLine(new ZDate(2016, 3, 1), AccCommissionLineCommissionStatusList.Codes.Approved, 5);
			CreateNewCommissionLine(new ZDate(2016, 3, 31), AccCommissionLineCommissionStatusList.Codes.Approved, 6);
			CreateNewCommissionLine(new ZDate(2016, 4, 1), AccCommissionLineCommissionStatusList.Codes.Approved, 7);
			CreateNewCommissionLine(new ZDate(2016, 4, 30), AccCommissionLineCommissionStatusList.Codes.Approved, 8);
			CreateNewCommissionLine(new ZDate(2016, 5, 1), AccCommissionLineCommissionStatusList.Codes.Approved, 9);
			CreateNewCommissionLine(new ZDate(2016, 5, 31), AccCommissionLineCommissionStatusList.Codes.Approved, 10);
			CreateNewCommissionLine(new ZDate(2016, 6, 1), AccCommissionLineCommissionStatusList.Codes.Approved, 11);
			CreateNewCommissionLine(new ZDate(2016, 6, 30), AccCommissionLineCommissionStatusList.Codes.Approved, 12);

			CreateNewCommissionLine(new ZDate(2016, 1, 1), AccCommissionLineCommissionStatusList.Codes.Paid, 13);
			CreateNewCommissionLine(new ZDate(2016, 1, 31), AccCommissionLineCommissionStatusList.Codes.Paid, 14);
			CreateNewCommissionLine(new ZDate(2016, 2, 1), AccCommissionLineCommissionStatusList.Codes.Paid, 15);
			CreateNewCommissionLine(new ZDate(2016, 2, 28), AccCommissionLineCommissionStatusList.Codes.Paid, 16);
			CreateNewCommissionLine(new ZDate(2016, 3, 1), AccCommissionLineCommissionStatusList.Codes.Paid, 17);
			CreateNewCommissionLine(new ZDate(2016, 3, 31), AccCommissionLineCommissionStatusList.Codes.Paid, 18);
			CreateNewCommissionLine(new ZDate(2016, 4, 1), AccCommissionLineCommissionStatusList.Codes.Paid, 19);
			CreateNewCommissionLine(new ZDate(2016, 4, 30), AccCommissionLineCommissionStatusList.Codes.Paid, 20);
			CreateNewCommissionLine(new ZDate(2016, 5, 1), AccCommissionLineCommissionStatusList.Codes.Paid, 21);
			CreateNewCommissionLine(new ZDate(2016, 5, 31), AccCommissionLineCommissionStatusList.Codes.Paid, 22);
			CreateNewCommissionLine(new ZDate(2016, 6, 1), AccCommissionLineCommissionStatusList.Codes.Paid, 23);
			CreateNewCommissionLine(new ZDate(2016, 6, 30), AccCommissionLineCommissionStatusList.Codes.Paid, 24);

			CreateNewCommissionLine(new ZDate(2016, 1, 1), AccCommissionLineCommissionStatusList.Codes.Pending, 25);
			CreateNewCommissionLine(new ZDate(2016, 1, 31), AccCommissionLineCommissionStatusList.Codes.Pending, 26);
			CreateNewCommissionLine(new ZDate(2016, 2, 1), AccCommissionLineCommissionStatusList.Codes.Pending, 27);
			CreateNewCommissionLine(new ZDate(2016, 2, 28), AccCommissionLineCommissionStatusList.Codes.Pending, 28);
			CreateNewCommissionLine(new ZDate(2016, 3, 1), AccCommissionLineCommissionStatusList.Codes.Pending, 29);
			CreateNewCommissionLine(new ZDate(2016, 3, 31), AccCommissionLineCommissionStatusList.Codes.Pending, 30);
			CreateNewCommissionLine(new ZDate(2016, 4, 1), AccCommissionLineCommissionStatusList.Codes.Pending, 31);
			CreateNewCommissionLine(new ZDate(2016, 4, 30), AccCommissionLineCommissionStatusList.Codes.Pending, 32);
			CreateNewCommissionLine(new ZDate(2016, 5, 1), AccCommissionLineCommissionStatusList.Codes.Pending, 33);
			CreateNewCommissionLine(new ZDate(2016, 5, 31), AccCommissionLineCommissionStatusList.Codes.Pending, 34);
			CreateNewCommissionLine(new ZDate(2016, 6, 1), AccCommissionLineCommissionStatusList.Codes.Pending, 35);
			CreateNewCommissionLine(new ZDate(2016, 6, 30), AccCommissionLineCommissionStatusList.Codes.Pending, 36);

			CreateNewCommissionLine(new ZDate(2016, 1, 1), "", 37);
			CreateNewCommissionLine(new ZDate(2016, 1, 31), "", 38);
			CreateNewCommissionLine(new ZDate(2016, 2, 1), "", 39);
			CreateNewCommissionLine(new ZDate(2016, 2, 28), "", 40);
			CreateNewCommissionLine(new ZDate(2016, 3, 1), "", 41);
			CreateNewCommissionLine(new ZDate(2016, 3, 31), "", 42);
			CreateNewCommissionLine(new ZDate(2016, 4, 1), "", 43);
			CreateNewCommissionLine(new ZDate(2016, 4, 30), "", 44);
			CreateNewCommissionLine(new ZDate(2016, 5, 1), "", 45);
			CreateNewCommissionLine(new ZDate(2016, 5, 31), "", 46);
			CreateNewCommissionLine(new ZDate(2016, 6, 1), "", 47);
			CreateNewCommissionLine(new ZDate(2016, 6, 30), "", 48);

			Factory.Save();

			var viewCommissionLineCollection = new ViewCommissionLineCollection(Factory);
			viewCommissionLineCollection.AdditionalFilter = ZQuery.NoResultQuery;
			var matrix = new RecentCommissionsMatrix(new ZDateTime(2016, 6, 15), viewCommissionLineCollection);
			AssertEquals(0, matrix.Count);

			viewCommissionLineCollection.AdditionalFilter = new ZQuery();
			AssertEquals(4, matrix.Count);
			var row = matrix.GetRow("APP");
			AssertEquals(78m, row.Total);
			AssertEquals(23m, row.TotalCurrentMonth);
			AssertEquals(19m, row.TotalPreviousMonth);
			AssertEquals(15m, row.Total2MonthsAgo);
			AssertEquals(11m, row.Total3MonthsAgo);
			AssertEquals(10m, row.TotalOver3MonthsAgo);

			row = matrix.GetRow("PAI");
			AssertEquals(222m, row.Total);
			AssertEquals(47m, row.TotalCurrentMonth);
			AssertEquals(43m, row.TotalPreviousMonth);
			AssertEquals(39m, row.Total2MonthsAgo);
			AssertEquals(35m, row.Total3MonthsAgo);
			AssertEquals(58m, row.TotalOver3MonthsAgo);

			row = matrix.GetRow("PEN");
			AssertEquals(876m, row.Total);
			AssertEquals(166m, row.TotalCurrentMonth);
			AssertEquals(158m, row.TotalPreviousMonth);
			AssertEquals(150m, row.Total2MonthsAgo);
			AssertEquals(142m, row.Total3MonthsAgo);
			AssertEquals(260m, row.TotalOver3MonthsAgo);

			row = matrix.GetRow("");
			AssertEquals(1176m, row.Total);
			AssertEquals(236m, row.TotalCurrentMonth);
			AssertEquals(220m, row.TotalPreviousMonth);
			AssertEquals(204m, row.Total2MonthsAgo);
			AssertEquals(188m, row.Total3MonthsAgo);
			AssertEquals(328m, row.TotalOver3MonthsAgo);
		}

		#endregion Refresh

		#region Overrides

		RecentCommissionsMatrix GetNewRecentCommissionMatrix()
		{
			return new RecentCommissionsMatrix(new ZDateTime(2002, 2, 2), new ViewCommissionLineCollection(Factory));
		}

		protected override RecentCommissionsMatrix GetCollectionToTest()
		{
			return GetNewRecentCommissionMatrix();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RecentCommissionsRow();
		}

		#endregion Overrides

		#region Implementation

		AccCommissionLine CreateNewCommissionLine(ZDate recognitionDate, ZString commissionStatusCode, ZDecimal preferredCommissionAmount)
		{
			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader.CH0_CommissionDate = recognitionDate;

			var commissionLine = commissionHeader.Lines.AddNew();
			commissionLine.CL0_GS_NKStaff = Staff.GS_Code;

			commissionLine.CL0_EntityCommissionAmount = preferredCommissionAmount;

			if (commissionStatusCode == AccCommissionLineCommissionStatusList.Codes.Approved)
			{
				commissionLine.CL0_ApprovedDateTimeUtc = new ZDateTime(2001, 1, 1);
			}
			else if (commissionStatusCode == AccCommissionLineCommissionStatusList.Codes.Paid)
			{
				commissionLine.CL0_PaidDateTimeUtc = new ZDateTime(2001, 1, 1);
			}

			commissionLine.CL0_RX_NKCommissionCurrency = commissionLine.CL0_RX_NKTransactionCurrency = "AUD";
			return commissionLine;
		}

		GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.NewWithValidTestData<GlbStaff>();
				}

				return staff;
			}
		}

		GlbStaff staff;

		#endregion Implementation
	}

	public static class Extensions
	{
		public static RecentCommissionsRow GetRow(this RecentCommissionsMatrix matrix, string statusCode)
		{
			if (matrix.Count != 4)
			{
				return null;
			}

			switch (statusCode)
			{
				case AccCommissionLineCommissionStatusList.Codes.Pending:
					{
						return matrix[0];
					}
				case AccCommissionLineCommissionStatusList.Codes.Approved:
					{
						return matrix[1];
					}
				case AccCommissionLineCommissionStatusList.Codes.Paid:
					{
						return matrix[2];
					}
				case "":
					{
						return matrix[3];
					}
				default:
					{
						return null;
					}
			}
		}
	}
}
