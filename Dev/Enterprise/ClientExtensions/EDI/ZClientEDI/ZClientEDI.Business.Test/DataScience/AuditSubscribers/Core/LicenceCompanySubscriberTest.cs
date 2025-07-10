using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(LicenceCompanySubscriber))]
	class LicenceCompanySubscriberTest : DataScienceAuditSubscriberTestBase<LicenceCompanySubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new LicenceCompanySubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(3, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table LicenceCompany required by LicenceCompanySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(11, subscriber.ColumnInfos.Count);

					AssertEquals("LC_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("LC_CompanyCode", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("LC_CompanyCountry", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("LC_IsReciprocal", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("char(1)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("LC_IsGSTRegistered", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("char(1)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("LC_IsGSTCashBasis", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("char(1)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("LC_IsWHTRegistered", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("char(1)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("LC_IsWHTCashBasis", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("char(1)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("LC_OH", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("LC_LE", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("LC_CompanyNumber", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
