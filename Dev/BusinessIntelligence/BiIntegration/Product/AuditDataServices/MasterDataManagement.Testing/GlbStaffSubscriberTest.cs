using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	abstract class GlbStaffSubscriberTest<TSubscriber> : PatternMatchingBaseSubscriberTest<GlbStaff, TSubscriber> where TSubscriber : ActualDataChangesAuditSubscriber
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberDoesNotProcessWithNullMaster()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.NewWithValidTestData<GlbStaff>();

			bizO.GS_Code = "QAA";
			bizO.GS_IsSystemAccount = true;

			factory.Save();

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbStaff SET GS_PER = NULL, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GETUTCDATE() WHERE GS_PK = '{bizO.PK}'");

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				AssertPatternMatchingNewRecordsPrecondition(factory, bizO);

				var query = @"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_UserAddress1, GS_ResourceType) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}');";
				var insertIntoCDCText = string.Format(CultureInfo.InvariantCulture, query, "2", bizO.PK, bizO.GS_Code, bizO.GS_UserAddress1, bizO.GS_ResourceType);

				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: null
				);

				AssertPatternMatchingNewRecordsPrecondition(factory, bizO);
			}
		}
	}
}
