using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(OrgContactSubscriber))]
	class OrgContactSubscriberTest : DataScienceAuditSubscriberTestBase<OrgContactSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);
			//AssertNull(generatedCode);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
			@"
The schema of the table OrgContact required by OrgContactSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
			() =>
			{
				AssertEquals(11, subscriber.ColumnInfos.Count);

				AssertEquals("OC_PK", subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

				AssertEquals("OC_ContactName", subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("nvarchar(256)", subscriber.ColumnInfos[1].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

				AssertEquals("OC_Email", subscriber.ColumnInfos[2].ColumnName);
				AssertEquals("nvarchar(254)", subscriber.ColumnInfos[2].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

				AssertEquals("OC_OH", subscriber.ColumnInfos[3].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

				AssertEquals("OC_OH_AddressOverride", subscriber.ColumnInfos[4].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

				AssertEquals("OC_SystemCreateBranch", subscriber.ColumnInfos[5].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[5].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

				AssertEquals("OC_SystemCreateDepartment", subscriber.ColumnInfos[6].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

				AssertEquals("OC_SystemCreateTimeUtc", subscriber.ColumnInfos[7].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

				AssertEquals("OC_SystemCreateUser", subscriber.ColumnInfos[8].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

				AssertEquals("OC_SystemLastEditTimeUtc", subscriber.ColumnInfos[9].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[9].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

				AssertEquals("OC_SystemLastEditUser", subscriber.ColumnInfos[10].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);
			});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "OC_AttachmentType";
				yield return "OC_Birthday";
				yield return "OC_ContactSource";
				yield return "OC_DetailsVerified";
				yield return "OC_Fax";
				yield return "OC_Gender";
				yield return "OC_HomePhone";
				yield return "OC_IsActive";
				yield return "OC_IsValid";
				yield return "OC_JobCategory";
				yield return "OC_Language";
				yield return "OC_Mobile";
				yield return "OC_NotifyMode";
				yield return "OC_OA_OrgAddress";
				yield return "OC_OtherPhone";
				yield return "OC_Pager";
				yield return "OC_PasswordHash";
				yield return "OC_PasswordHashIterations";
				yield return "OC_PasswordSalt";
				yield return "OC_PER";
				yield return "OC_Phone";
				yield return "OC_PhoneExtension";
				yield return "OC_RN_NKNationality";
				yield return "OC_Salutation";
				yield return "OC_Title";
				yield return "OC_WebAccessEnabled";
				yield return "OC_WebContractSignedDate";
				yield return "OC_YearJoinedCompany";
				yield return "OC_YearJoinedIndustry";
				yield return "OC_ProfilePhoto";
				yield return "OC_PersonalInfo";
			}
		}
	}
}
