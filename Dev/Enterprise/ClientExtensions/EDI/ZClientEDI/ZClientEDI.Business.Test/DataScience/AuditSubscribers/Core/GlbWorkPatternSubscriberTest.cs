using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbWorkPatternSubscriber))]
	class GlbWorkPatternSubscriberTest : DataScienceAuditSubscriberTestBase<GlbWorkPatternSubscriber>
	{
		[DatCapabilityRequirement("SQLCDCREADY")]
		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table GlbWorkPattern required by GlbWorkPatternSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(13, subscriber.ColumnInfos.Count);

					AssertEquals("GWP_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GWP_AutoEffectiveEndDate", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("datetimeoffset(0)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GWP_Comment", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(300)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GWP_EffectiveDate", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("datetimeoffset(0)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GWP_GCR_ChangeRequest", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GWP_GS_Staff", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GWP_IsApproved", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GWP_Name", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GWP_StandardDuration", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GWP_SystemCreateTimeUtc", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GWP_SystemCreateUser", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("GWP_SystemLastEditTimeUtc", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("GWP_SystemLastEditUser", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);
			});
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbWorkPatternSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
		//{
		//	get
		//	{
		//		yield return "GWP_Comment";
		//		yield return "GWP_GCR_ChangeRequest";
		//		yield return "GWP_Name";
		//	}
		//}
	}
}
