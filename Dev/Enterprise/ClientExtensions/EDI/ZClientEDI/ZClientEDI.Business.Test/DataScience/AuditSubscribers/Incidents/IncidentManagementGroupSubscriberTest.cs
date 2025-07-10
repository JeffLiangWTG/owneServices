using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents.Tests
{
	[TestedType(typeof(IncidentManagementGroupSubscriber))]
	class IncidentManagementGroupSubscriberTest : DataScienceAuditSubscriberTestBase<IncidentManagementGroupSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new IncidentManagementGroupSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(3, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
			The schema of the table IncidentMain required by IncidentManagementGroupSubscriber has changed.
			The Data Science team will need to adjust their data pipelines before this change can be committed.
			Please contact the Data Science team.",
				() =>
				{
					AssertEquals(30, subscriber.ColumnInfos.Count);

					AssertEquals("ING_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("ING_IncidentGroupNumber", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("ING_Type", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("ING_Description", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("ING_ServiceOutage", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("ING_BusinessImpact", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("ING_Urgency", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("ING_GS_NKGroupOwner", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("ING_Status", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("ING_Product", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("ING_ProductArea", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("ING_Priority", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("ING_SourceModuleId", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("ING_Category", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("ING_Module", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("ING_RN_NKCountry", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("ING_SystemCreateTimeUtc", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("ING_SystemCreateUser", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("ING_SystemCreateBranch", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("ING_SystemCreateDepartment", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("ING_SystemLastEditTimeUtc", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("ING_SystemLastEditUser", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("ING_GS_NKDefaultResponder", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("ING_IsAutoReply", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);

					AssertEquals("ING_IsBroadcastToControlledOnly", subscriber.ColumnInfos[24].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[24].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[24].IsNullable);

					AssertEquals("ING_IsKnownIssue", subscriber.ColumnInfos[25].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[25].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[25].IsNullable);

					AssertEquals("ING_IMT_Triage", subscriber.ColumnInfos[26].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[26].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[26].IsNullable);

					AssertEquals("ING_ServiceType", subscriber.ColumnInfos[27].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[27].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[27].IsNullable);

					AssertEquals("ING_IsAutoReplyUnflagsCommunication", subscriber.ColumnInfos[28].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[28].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[28].IsNullable);

					AssertEquals("ING_IsInterimBroadcastUnflagsCommunication", subscriber.ColumnInfos[29].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[29].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[29].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
