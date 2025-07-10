using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransportBooking;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransportBooking.Testing
{
	[TestedType(typeof(ViewDtbBookingConfirmationWithQuantity))]
	internal sealed class ViewDtbBookingConfirmationWithQuantityTest : DbCreateScriptTest
	{
		public void TestQueryHasNoScans()
		{
			var sql = new SqlQueryBuilder();
			var consolidation = CreateBookingConsolidation(sql, "CB0001", "BKG");
			var booking = CreateBooking(sql, consolidation, "TB0001");
			var instructionType = "PIC";
			var pkgPackageJob = CreatePkgPackageJob(sql);
			var package = CreatePkgPackage(sql, pkgPackageJob);
			var quantity = 42;
			var unusedQuantity = 0;
			for (int i = 0; i < 1000; i++)
			{
				var instruction = CreateInstruction(sql, booking, instructionType);
				var instructionPkgDivot = CreateInstructionPkgDivot(sql, unusedQuantity, package, instruction);
				var requiredFrom = new DateTime(2030 + (i % 40), 1, 1 + i / 40);
				CreateConfirmation(sql, instruction, instructionType, quantity, instructionPkgDivot, requiredFrom);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS DtbBookingInstruction WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS DtbBookingInstructionPkgDivot WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS DtbBookingConfirmation WITH FULLSCAN");

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var totalRows = 0;
				TestConnection.ExecuteReader($"SELECT * FROM dbo.ViewDtbBookingConfirmationWithQuantity JOIN dbo.DtbBookingConfirmation on DtbBookingConfirmation.KK_PK = ViewDtbBookingConfirmationWithQuantity.KK_PK WHERE DtbBookingConfirmation.KK_RequiredFrom = '2030-1-1 0:00:00'",
					_ => totalRows++);
				AssertEquals("Precondition: Haven't created the correct number of rows in the view table", 1, totalRows);
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("ViewDtbBookingConfirmationWithQuantity"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
				Assert("There should be no table scans.", !queryPlanAnalyzer.TableScans.Any());
				Assert("There should be no index scans.", !queryPlanAnalyzer.IndexScans.Any());
			}
		}

		public void TestView_CanJoinOnKK_KD_BookingInstructionPkgDivot()
		{
			var sql = new SqlQueryBuilder();
			var consolidation = CreateBookingConsolidation(sql, "CB0001", "BKG");
			var booking = CreateBooking(sql, consolidation, "TB0001");
			var instructionType = "PIC";
			var instruction = CreateInstruction(sql, booking, instructionType);
			var pkgPackageJob = CreatePkgPackageJob(sql);
			var package = CreatePkgPackage(sql, pkgPackageJob);
			var quantity = 42;
			var unusedQuantity = 0;
			var instructionPkgDivot = CreateInstructionPkgDivot(sql, unusedQuantity, package, instruction);
			var requiredFrom = new DateTime(2030, 1, 1);
			var confirmation = CreateConfirmation(sql, instruction, instructionType, quantity, instructionPkgDivot, requiredFrom);

			// Unrelated entry, to ensure we aren't doing joins where conditions are missing
			var unrelatedInstruction = CreateInstruction(sql, booking, instructionType);
			var unrelatedInstructionPkgDivot = CreateInstructionPkgDivot(sql, unusedQuantity, package, unrelatedInstruction);
			CreateConfirmation(sql, unrelatedInstruction, instructionType, quantity, unrelatedInstructionPkgDivot, requiredFrom);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
					string.Format(@"SELECT * FROM dbo.ViewDtbBookingConfirmationWithQuantity where KK_PK = '{0}'", confirmation));
			AssertEquals("Should be able to join using DtbBookingConfirmation.KK_KD_BookingInstructionPkgDivot and DtbBookingInstructionPkgDivot.KD_PK", 1, result.Rows.Count);
			AssertEquals("Quantity should use KK_Quantity not KD_Quantity", quantity, result.Rows[0]["Quantity"]);
		}

		public void TestView_CanJoinOnKD_KN_BookingInstruction()
		{
			var sql = new SqlQueryBuilder();
			var consolidation = CreateBookingConsolidation(sql, "CB0001", "BKG");
			var booking = CreateBooking(sql, consolidation, "TB0001");
			var instructionType = "PIC";
			var instruction = CreateInstruction(sql, booking, instructionType);
			var pkgPackageJob = CreatePkgPackageJob(sql);
			var package = CreatePkgPackage(sql, pkgPackageJob);
			var quantity = 42;
			var unusedQuantity = 0;
			var instructionPkgDivot = CreateInstructionPkgDivot(sql, quantity, package, instruction);
			var requiredFrom = new DateTime(2030, 1, 1);
			var confirmation = CreateConfirmation(sql, instruction, instructionType, unusedQuantity, null, requiredFrom);

			// Unrelated entry, to ensure we aren't doing joins where conditions are missing
			var unrelatedInstruction = CreateInstruction(sql, booking, instructionType);
			var unrelatedInstructionPkgDivot = CreateInstructionPkgDivot(sql, quantity, package, unrelatedInstruction);
			CreateConfirmation(sql, instruction, instructionType, unusedQuantity, null, requiredFrom);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
					string.Format(@"SELECT * FROM dbo.ViewDtbBookingConfirmationWithQuantity where KK_PK = '{0}'", confirmation));
			AssertEquals("Should be able to join between DtbBookingInstructionPkgDivot.KD_KN_BookingInstruction and DtbBookingInstruction.KN_PK", 1, result.Rows.Count);
			AssertEquals("Quantity should use KD_Quantity not KK_Quantity", quantity, result.Rows[0]["Quantity"]);
		}

		public void TestView_KK_KD_BookingInstructionPkgDivotAvailable_DoesNotUseKD_KN_BookingInstruction()
		{
			var sql = new SqlQueryBuilder();
			var consolidation = CreateBookingConsolidation(sql, "CB0001", "BKG");
			var booking = CreateBooking(sql, consolidation, "TB0001");
			var instructionType = "PIC";
			var instruction = CreateInstruction(sql, booking, instructionType);
			var pkgPackageJob = CreatePkgPackageJob(sql);
			var package = CreatePkgPackage(sql, pkgPackageJob);
			var quantity = 42;
			var unusedQuantity = 0;
			var instructionPkgDivot = CreateInstructionPkgDivot(sql, unusedQuantity, package, instruction);
			var requiredFrom = new DateTime(2030, 1, 1);
			// Create a confirmation where KK_KD_BookingInstructionPkgDivot is not null
			var confirmation = CreateConfirmation(sql, instruction, instructionType, quantity, instructionPkgDivot, requiredFrom);

			var unrelatedPackage = CreatePkgPackage(sql, pkgPackageJob);
			// Create an instructionPkgDivot with a valid KD_KN_BookingInstruction
			var unrelatedInstructionPkgDivot = CreateInstructionPkgDivot(sql, quantity, unrelatedPackage, instruction);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
					"SELECT * FROM dbo.ViewDtbBookingConfirmationWithQuantity");
			AssertEquals("Since DtbBookingConfirmation.KK_KD_BookingInstructionPkgDivot is not null, should not find the additional instruction package divot", 1, result.Rows.Count);
		}

		Guid CreatePkgPackageJob(SqlQueryBuilder sql)
		{
			var pkgPackageJob = new PkgPackageJob(Guid.NewGuid(), parentTableCode: "KM").AppendInsertAndReturnObject(sql);
			return pkgPackageJob.PK;
		}

		Guid CreatePkgPackage(SqlQueryBuilder sql, Guid pkgPackageId)
		{
			var pk = Guid.NewGuid();
			sql.Append($"insert into dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_F3_NKPackType, KP_Sequence, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) values ('{pk}', '{pkgPackageId}', 'CNT', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			return pk;
		}

		Guid CreateBookingConsolidation(SqlQueryBuilder sql, string jobID, string jobType)
		{
			var dtbBookingConsolidation = new DtbBookingConsolidation()
			{
				KB_JobID = jobID,
				KB_JobType = jobType
			};
			sql.Append(dtbBookingConsolidation.GetInsertStatement());
			return dtbBookingConsolidation.PK;
		}

		Guid CreateBooking(SqlQueryBuilder sql, Guid consolPk, string jobId)
		{
			var branchPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var dtbBooking = new DtbBooking(consolPk)
			{
				KM_JobID = jobId,
				KM_KB_Booking = consolPk,
				KM_GB_Branch = branchPK
			};
			sql.Append(dtbBooking.GetInsertStatement());
			return dtbBooking.PK;
		}

		Guid CreateInstruction(SqlQueryBuilder sql, Guid bookingPK, string instructionType)
		{
			var dtbBookingInstruction = new DtbBookingInstruction(bookingPK)
			{
				KN_InstructionType = instructionType
			};
			sql.Append(dtbBookingInstruction.GetInsertStatement());
			return dtbBookingInstruction.PK;
		}

		Guid CreateConfirmation(SqlQueryBuilder sql, Guid instructionPK, string confirmationType, int quantity, Guid? pkgDivotPK, DateTime requiredFrom)
		{
			var dtbBookingConfirmation = new DtbBookingConfirmation(instructionPK, pkgDivotPK)
			{
				KK_ConfirmationType = confirmationType,
				KK_Quantity = quantity,
				KK_RequiredFrom = requiredFrom
			};
			sql.Append(dtbBookingConfirmation.GetInsertStatement());
			return dtbBookingConfirmation.PK;
		}

		Guid CreateInstructionPkgDivot(SqlQueryBuilder sql, int quantity, Guid packagePK, Guid instructionPK)
		{
			var dtbBookingInstructionPkgDivot = new DtbBookingInstructionPkgDivot(instructionPK, packagePK)
			{
				KD_Quantity = quantity
			};
			sql.Append(dtbBookingInstructionPkgDivot.GetInsertStatement());
			return dtbBookingInstructionPkgDivot.PK;
		}
	}
}
