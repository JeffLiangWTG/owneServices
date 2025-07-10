using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing.Tests
{
	[TestedType(typeof(AccGLHeaderSubscriber))]
	class AccGLHeaderSubscriberTest : DataScienceAuditSubscriberTestBase<AccGLHeaderSubscriber>
	{
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
The schema of the table AccGLHeader required by AccGLHeaderSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(24, subscriber.ColumnInfos.Count);

					AssertEquals("AG_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("AG_AccountGroup", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(1)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("AG_AccountNum", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(10)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("AG_AccountType", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("AG_AG_AlternateNum", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("AG_AG_ConsolidationNum", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("AG_AG_HeaderDependsOnTotal", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("AG_AG_PercentNum", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("AG_CashFlowType", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("AG_Column", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("AG_ControlAccount", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("AG_DebitCredit", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("AG_Description", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(35)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("AG_DisallowDirectPosting", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("AG_IsActive", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("AG_IsGlobal", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("AG_Notes", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("nvarchar(max)", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("AG_PrintSequence", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("AG_StatisticalUnits", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("AG_SystemCreateTimeUtc", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("AG_SystemCreateUser", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("AG_SystemLastEditTimeUtc", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("AG_SystemLastEditUser", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("AG_TotalLevel", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);
				}
			);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new AccGLHeaderSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
