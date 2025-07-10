using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(StmNoteSubscriber))]
	class StmNoteSubscriberTest : DataScienceAuditSubscriberTestBase<StmNoteSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new StmNoteSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
			@"
The schema of the table StmNote required by StmNoteSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
			() =>
			{
				AssertEquals(15, subscriber.ColumnInfos.Count);

				AssertEquals("ST_PK", subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

				AssertEquals("ST_Description", subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("nvarchar(50)", subscriber.ColumnInfos[1].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

				AssertEquals("ST_ForceRead", subscriber.ColumnInfos[2].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[2].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

				AssertEquals("ST_GC_RelatedCompany", subscriber.ColumnInfos[3].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

				AssertEquals("ST_IsCustomDescription", subscriber.ColumnInfos[4].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[4].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

				AssertEquals("ST_NoteContext", subscriber.ColumnInfos[5].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[5].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

				AssertEquals("ST_NoteData", subscriber.ColumnInfos[6].ColumnName);
				AssertEquals("varbinary(max)", subscriber.ColumnInfos[6].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

				AssertEquals("ST_NoteText", subscriber.ColumnInfos[7].ColumnName);
				AssertEquals("nvarchar(max)", subscriber.ColumnInfos[7].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

				AssertEquals("ST_NoteType", subscriber.ColumnInfos[8].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

				AssertEquals("ST_ParentID", subscriber.ColumnInfos[9].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[9].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

				AssertEquals("ST_SystemCreateTimeUtc", subscriber.ColumnInfos[10].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[10].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

				AssertEquals("ST_SystemCreateUser", subscriber.ColumnInfos[11].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

				AssertEquals("ST_SystemLastEditTimeUtc", subscriber.ColumnInfos[12].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[12].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

				AssertEquals("ST_SystemLastEditUser", subscriber.ColumnInfos[13].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

				AssertEquals("ST_Table", subscriber.ColumnInfos[14].ColumnName);
				AssertEquals("varchar(35)", subscriber.ColumnInfos[14].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);
			});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
