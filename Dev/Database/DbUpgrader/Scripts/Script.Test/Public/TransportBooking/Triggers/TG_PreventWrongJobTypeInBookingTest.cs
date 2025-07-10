using CargoWise.DbUpgrader.Scripts.Definitions.TransportBooking.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransportBooking.Triggers.Testing
{
	[TestedType(typeof(TG_PreventWrongJobTypeInBooking))]
	class TG_PreventWrongJobTypeInBookingTest : DbCreateScriptTest
	{
		#region TestCanAddCompatibleBookingToConsolidation

		public void TestCanAddConsignmentToConsignmentConsolidation()
		{
			AssertCanAddCompatibleBookingToConsolidation("CSN", "CSN");
		}

		public void TestCanAddBookingToBookingConsolidation()
		{
			AssertCanAddCompatibleBookingToConsolidation("BKG", "BKG");
		}

		public void TestCanAddBookingToQteBookingConsolidation()
		{
			AssertCanAddCompatibleBookingToConsolidation("BKG", "HLS");
		}

		public void TestCanAddBookingToHlsBookingConsolidation()
		{
			AssertCanAddCompatibleBookingToConsolidation("BKG", "QTE");
		}

		void AssertCanAddCompatibleBookingToConsolidation(string bookingJobType, string consolidationJobType)
		{
			AssertNoExceptionThrown($@"Should be able to add a {bookingJobType} Booking to a {consolidationJobType} Booking Consolidation",
				() => CreateBookingWithConsolidation(bookingJobType, consolidationJobType));
		}

		#endregion

		#region TestCannotAddIncompatibleBookingToConsolidation

		public void TestCannotAddBookingToConsignmentConsolidation()
		{
			AssertCannotAddIncompatibleBookingToConsolidation("BKG", "CSN");
		}

		public void TestCannotAddConsignmentToBookingConsolidation()
		{
			AssertCannotAddIncompatibleBookingToConsolidation("CSN", "BKG");
		}

		public void TestCannotAddConsignmentToQteBookingConsolidation()
		{
			AssertCannotAddIncompatibleBookingToConsolidation("CSN", "QTE");
		}

		public void TestCannotAddConsignmentToHlsBookingConsolidation()
		{
			AssertCannotAddIncompatibleBookingToConsolidation("CSN", "HLS");
		}

		void AssertCannotAddIncompatibleBookingToConsolidation(string bookingJobType, string consolidationJobType)
		{
			var exception = AssertExceptionThrown<SqlException>("Should not be able to add a Booking to a Consignment Consolidation",
				() => CreateBookingWithConsolidation(bookingJobType, consolidationJobType));

			AssertEquals(@"Attempt to set JobType to other than that of the associated Consolidation.
The transaction ended in the trigger. The batch has been aborted.", exception.Message);
		}

		#endregion

		#region TestCanAssignCompatibleBookingToConsolidation

		public void TestCanAssignConsignmentToConsignmentConsolidation()
		{
			AssertCanAssignCompatibleBookingToConsolidation("CSN", "CSN");
		}

		public void TestCanAssignBookingToBookingConsolidation()
		{
			AssertCanAssignCompatibleBookingToConsolidation("BKG", "BKG");
		}

		public void TestCanAssignBookingToQteBookingConsolidation()
		{
			AssertCanAssignCompatibleBookingToConsolidation("BKG", "QTE");
		}

		public void TestCanAssignBookingToHlsBookingConsolidation()
		{
			AssertCanAssignCompatibleBookingToConsolidation("BKG", "HLS");
		}

		void AssertCanAssignCompatibleBookingToConsolidation(string bookingJobType, string consolidationJobType)
		{
			AssertNoExceptionThrown($@"Should be able to reasign a {bookingJobType} Booking to a {consolidationJobType} Booking Consolidation",
				() => CreateBookingAndAssignToNewConsolidation(bookingJobType, consolidationJobType));
		}

		#endregion

		#region TestCannotAssignIncompatibleBookingToConsolidation

		public void TestCannotAssignBookingToConsignmentConsolidation()
		{
			AssertCannotAssignIncompatibleBookingToConsolidation("BKG", "CSN");
		}

		public void TestCannotAssignConsignmentToBookingConsolidation()
		{
			AssertCannotAssignIncompatibleBookingToConsolidation("CSN", "BKG");
		}

		public void TestCannotAssignConsignmentToQteBookingConsolidation()
		{
			AssertCannotAssignIncompatibleBookingToConsolidation("CSN", "QTE");
		}

		public void TestCannotAssignConsignmentToHlsBookingConsolidation()
		{
			AssertCannotAssignIncompatibleBookingToConsolidation("CSN", "HLS");
		}

		void AssertCannotAssignIncompatibleBookingToConsolidation(string bookingJobType, string consolidationJobType)
		{
			var exception = AssertExceptionThrown<SqlException>($@"Should not be able to reassign a {bookingJobType} Booking to a {consolidationJobType} Booking Consolidation",
				() => CreateBookingAndAssignToNewConsolidation(bookingJobType, consolidationJobType));

			AssertEquals(@"Attempt to attach a booking to a Consolidation of a different Job Type.
The transaction ended in the trigger. The batch has been aborted.", exception.Message);
		}

		#endregion

		#region Implementation

		void CreateBookingWithConsolidation(string bookingJobType, string consolidationJobType)
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var bookingConsolPk = TestDataCreator.CreateBookingConsolidation("CB0001", consolidationJobType);

			TestDataCreator.CreateBooking(branchPK, bookingConsolPk, "TB0001", bookingJobType);
		}

		void CreateBookingAndAssignToNewConsolidation(string bookingJobType, string newConsolidationJobType)
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var originalConsolidationJobType = bookingJobType == "CSN" ? "CSN" : "BKG";
			var originalConsolPk = TestDataCreator.CreateBookingConsolidation("CB0001", originalConsolidationJobType);
			var bookingPK = TestDataCreator.CreateBooking(branchPK, originalConsolPk, "TB0001", bookingJobType);

			var newConsolPk = TestDataCreator.CreateBookingConsolidation("CB0002", newConsolidationJobType);

			var updateSQL = $@"
				Update dbo.DtbBooking
				Set
					KM_KB_Booking = '{newConsolPk}'
				Where
					KM_PK ='{bookingPK}'
				";

			TestConnection.ExecuteNonQuery(updateSQL);
		}
		#endregion
	}
}

