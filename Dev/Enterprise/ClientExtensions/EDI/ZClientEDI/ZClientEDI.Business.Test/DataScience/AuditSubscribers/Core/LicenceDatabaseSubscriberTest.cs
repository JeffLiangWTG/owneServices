using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(LicenceDatabaseSubscriber))]
	class LicenceDatabaseSubscriberTest : DataScienceAuditSubscriberTestBase<LicenceDatabaseSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new LicenceDatabaseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(5, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table LicenceDatabase required by LicenceDatabaseSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(21, subscriber.ColumnInfos.Count);

					AssertEquals("LD_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("LD_ServerCode", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("LD_LicenceType", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("LD_LicenceExpiry", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("LD_LastHeartbeat", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("LD_ReleaseRing", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("LD_LE", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("LD_OH_BillingParty", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("LD_IsActive", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("LD_DatabaseNumber", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("LD_Status", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("LD_Product", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("LD_HostedLocation", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("LD_LD_ParentDatabase", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("LD_GS_NKOwner", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("LD_Billable", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(1)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("LD_TenantID", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("LD_SystemCreateTimeUtc", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("LD_SystemCreateUser", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("LD_SystemLastEditTimeUtc", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("LD_SystemLastEditUser", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "LD_NextRunTimeUtcMUG";
				yield return "LD_NextRunTimeUtcUPG";
				yield return "LD_Password";
				yield return "LD_ScheduleStateMUG";
				yield return "LD_ScheduleStateUPG";
				yield return "LD_TokenAuthenticationEnabled";
				yield return LicenceDatabaseSchema.Constants.LD_FeatureControlRuleLastSyncUtc;
				yield return LicenceDatabaseSchema.Constants.LD_EnablePackageDownloadOptimization;
				yield return LicenceDatabaseSchema.Constants.LD_AllowAutoLogin;
				yield return LicenceDatabaseSchema.Constants.LD_CanReregisterToSameServer;
				yield return LicenceDatabaseSchema.Constants.LD_DBServerSecurityMode;
				yield return LicenceDatabaseSchema.Constants.LD_ETS_TrustedSystem;
				yield return LicenceDatabaseSchema.Constants.LD_HL_CurrentRunningVersion;
				yield return LicenceDatabaseSchema.Constants.LD_HL_CurrentSentVersion;
				yield return LicenceDatabaseSchema.Constants.LD_ManualLicenceExpiry;
				yield return LicenceDatabaseSchema.Constants.LD_OC_ContractInstallerOrInternalTechContact;
				yield return LicenceDatabaseSchema.Constants.LD_OC_LicenseeAdminContact;
				yield return LicenceDatabaseSchema.Constants.LD_OH_WebAccessOrg;
				yield return "LD_FCS_FeatureSet";
				yield return "LD_FeatureSetConfigDateUtc";
			}
		}
	}
}
