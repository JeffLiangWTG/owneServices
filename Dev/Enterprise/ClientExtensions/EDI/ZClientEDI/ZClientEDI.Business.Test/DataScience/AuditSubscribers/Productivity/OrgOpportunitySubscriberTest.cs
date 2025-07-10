using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(OrgOpportunitySubscriber))]
	class OrgOpportunitySubscriberTest : DataScienceAuditSubscriberTestBase<OrgOpportunitySubscriber>
	{
		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new OrgOpportunitySubscriber();

			// Assert
			AssertEquals("DOO", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgOpportunitySubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(2, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table OrgOpportunity required by OrgOpportunitySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(38, subscriber.ColumnInfos.Count);

					AssertEquals("P8_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("P8_CloseCertainty", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("tinyint", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("P8_ClosedDate", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("P8_DateForExchangeRate", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("P8_DiscountAmount", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("money", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("P8_EstimatedCloseDate", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("P8_EstimatedValue", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("money", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("P8_GS_NKPrimarySalesPerson", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("P8_LostReason", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("P8_OpportunityDescription", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("P8_OpportunityNotes", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varbinary(max)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("P8_OpportunityType", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("P8_Outcome", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("P8_PackageType", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("P8_RecallDate", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("P8_RentalMultiplier", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("money", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("P8_RX_NKEstimatedValueCurrency", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("P8_Source", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("P8_SourceDetails", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("nvarchar(200)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("P8_Stage", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("P8_G0", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("P8_GC", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("P8_O1_Enquiry", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("P8_OA", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[23].IsNullable);

					AssertEquals("P8_OA_AssignedOffice", subscriber.ColumnInfos[24].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[24].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[24].IsNullable);

					AssertEquals("P8_OC", subscriber.ColumnInfos[25].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[25].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[25].IsNullable);

					AssertEquals("P8_OC_AssignedOfficeContact", subscriber.ColumnInfos[26].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[26].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[26].IsNullable);

					AssertEquals("P8_OC_ReferringContact", subscriber.ColumnInfos[27].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[27].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[27].IsNullable);

					AssertEquals("P8_OH", subscriber.ColumnInfos[28].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[28].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[28].IsNullable);

					AssertEquals("P8_OH_ReferringOrganisation", subscriber.ColumnInfos[29].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[29].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[29].IsNullable);

					AssertEquals("P8_OpportunityID", subscriber.ColumnInfos[30].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[30].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[30].IsNullable);

					AssertEquals("P8_Status", subscriber.ColumnInfos[31].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[31].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[31].IsNullable);

					AssertEquals("P8_SystemCreateTimeUtc", subscriber.ColumnInfos[32].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[32].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[32].IsNullable);

					AssertEquals("P8_SystemCreateUser", subscriber.ColumnInfos[33].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[33].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[33].IsNullable);

					AssertEquals("P8_SystemCreateBranch", subscriber.ColumnInfos[34].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[34].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[34].IsNullable);

					AssertEquals("P8_SystemCreateDepartment", subscriber.ColumnInfos[35].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[35].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[35].IsNullable);

					AssertEquals("P8_SystemLastEditTimeUtc", subscriber.ColumnInfos[36].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[36].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[36].IsNullable);

					AssertEquals("P8_SystemLastEditUser", subscriber.ColumnInfos[37].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[37].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[37].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
