using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.TestFramework.FreightSailingTestHelper;

namespace Enterprise.Build.Database.Script.Public.Freight.Sailing
{
	class TG_JobSailing_UpdateIntegrationTest : TransactionedTestCase
	{
		public void TestJobSailingTrigger()
		{
			var jobSailingPK = new FreightSailingTestHelper(TestConnection).SetupBasicLinkedTransportAndReturnPKOf(JobTypes.Sailing);

			var depotReceivalCommences = ZDateTime.Today;
			var depotCutOff = depotReceivalCommences.AddDays(1);
			var depotAvailabilityDate = depotReceivalCommences.AddDays(2);
			var depotStorageDate = depotReceivalCommences.AddDays(3);
			var onlineScheduleStatus = Enterprise.Core.Constants.FlightScheduleStatus.Matched;
			var serviceString = "myServiceString";
			var arrivalPortRouteId = "arrivalId";
			var departurePortRouteId = "departureId";

			var updateSql = @"
				UPDATE
					dbo.JobSailing
				SET
					{0} = '{1}',
					JX_SystemLastEditTimeUtc = GETUTCDATE(),
					JX_SystemLastEditUser = '~BP'
				WHERE
					JX_PK = '{2}'";

			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_DepotReceivalCommences", depotReceivalCommences, jobSailingPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_DepotCutOff", depotCutOff, jobSailingPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_DepotAvailabilityDate", depotAvailabilityDate, jobSailingPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_DepotStorageDate", depotStorageDate, jobSailingPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_OnlineScheduleStatus", onlineScheduleStatus, jobSailingPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_ServiceString", serviceString, jobSailingPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_ArrivalPortRouteId", arrivalPortRouteId, jobSailingPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JX_DeparturePortRouteId", departurePortRouteId, jobSailingPK));

			var selectSql = @"
				SELECT
					{0}
				FROM
					dbo.JobConsolTransport
					JOIN dbo.JobSailing ON JX_PK = JW_JX";

			AssertEquals(depotReceivalCommences, new ZDateTime(TestConnection.ExecuteScalar(string.Format(selectSql, "JW_DepotReceivalCommences"))));
			AssertEquals(depotCutOff, new ZDateTime(TestConnection.ExecuteScalar(string.Format(selectSql, "JW_DepotCutOff"))));
			AssertEquals(depotAvailabilityDate, new ZDateTime(TestConnection.ExecuteScalar(string.Format(selectSql, "JW_DepotAvailabilityDate"))));
			AssertEquals(depotStorageDate, new ZDateTime(TestConnection.ExecuteScalar(string.Format(selectSql, "JW_DepotStorageDate"))));
			AssertEquals(onlineScheduleStatus, (string)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_OnlineScheduleStatus", jobSailingPK)));
			AssertEquals(serviceString, (string)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_ServiceString", jobSailingPK)));
			AssertEquals(arrivalPortRouteId, (string)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_ArrivalPortRouteId", jobSailingPK)));
			AssertEquals(departurePortRouteId, (string)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_DeparturePortRouteId", jobSailingPK)));
		}
	}
}

