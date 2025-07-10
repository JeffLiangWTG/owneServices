using CargoWise.DbUpgrader.Scripts.Definitions.TransportBooking.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransportBooking.Triggers.Testing
{
	[TestedType(typeof(TG_PreventWrongJobTypeInBookingConsolidation))]
	class TG_PreventWrongJobTypeInBookingConsolidationTest : DbCreateScriptTest
	{
		public void TestCanChangeBookingConsolidationWithBookingsJobType()
		{
			var sql = new SqlQueryBuilder();

			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var bookingConsolPk = TestDataCreator.CreateBookingConsolidation("CB0001", "BKG");
			var bookingPk = TestDataCreator.CreateBooking(branchPK, bookingConsolPk, "TB0001", "BKG");

			var updateSQL = $@"
				Update dbo.DtbBookingConsolidation
				Set
					KB_JobType = 'HLS'
				Where
					KB_PK ='{bookingConsolPk}'
				";

			AssertNoExceptionThrown($@"Should be able to change a Booking Consolidation from BKG to HLS.",
				() => TestConnection.ExecuteNonQuery(updateSQL));
		}

		public void TestCannotChangeBookingConsolidationToConsignmentJobType()
		{
			var sql = new SqlQueryBuilder();

			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var bookingConsolPk = TestDataCreator.CreateBookingConsolidation("CB0001", "BKG");
			var bookingPk = TestDataCreator.CreateBooking(branchPK, bookingConsolPk, "TB0001", "BKG");

			var updateSQL = $@"
				Update dbo.DtbBookingConsolidation
				Set
					KB_JobType = 'CSN'
				Where
					KB_PK ='{bookingConsolPk}'
				";

			var exception = AssertExceptionThrown<SqlException>("Should not be able to change a Consolidation from BKG' to 'CSN'",
				() => TestConnection.ExecuteNonQuery(updateSQL));
			AssertEquals(@"Attempt to set JobType to other than that of an associated Booking.
The transaction ended in the trigger. The batch has been aborted.", exception.Message);
		}

		public void TestCannotChangeConsignmentConsolidationToBookingJobType()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var bookingConsolPk = TestDataCreator.CreateBookingConsolidation("CB0001", "CSN");
			var bookingPk = TestDataCreator.CreateBooking(branchPK, bookingConsolPk, "TB0001", "CSN");

			var updateSQL = $@"
				Update dbo.DtbBookingConsolidation
				Set
					KB_JobType = 'BKG'
				Where
					KB_PK ='{bookingConsolPk}'
				";

			var exception = AssertExceptionThrown<SqlException>("Should not be able to change a Consolidation from BKG' to 'CSN'", () => TestConnection.ExecuteNonQuery(updateSQL));
			AssertEquals(@"Attempt to set JobType to other than that of an associated Booking.
The transaction ended in the trigger. The batch has been aborted.", exception.Message);
		}

		public void TestCanChangeConsolidationWithoutBookingsJobType()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var bookingConsolPk = TestDataCreator.CreateBookingConsolidation("CB0001", "BKG");

			var changeBookingTypeSql = $@"
				Update dbo.DtbBookingConsolidation
				Set
					KB_JobType = 'HLS'
				Where
					KB_PK ='{bookingConsolPk}'
				";

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(changeBookingTypeSql));

			var changeBookingToConsignmentSql = $@"
				Update dbo.DtbBookingConsolidation
				Set
					KB_JobType = 'CSN'
				Where
					KB_PK ='{bookingConsolPk}'
				";

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(changeBookingToConsignmentSql));
		}
	}
}

