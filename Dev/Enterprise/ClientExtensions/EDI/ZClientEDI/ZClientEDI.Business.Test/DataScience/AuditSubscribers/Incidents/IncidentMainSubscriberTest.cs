using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents.Tests
{
	[TestedType(typeof(IncidentMainSubscriber))]
	class IncidentMainSubscriberTest : DataScienceAuditSubscriberTestBase<IncidentMainSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new IncidentMainSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);
			//AssertNullOrEmpty(generatedCode);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(8, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
			@"
The schema of the table IncidentMain required by IncidentMainSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
			() =>
			{
				AssertEquals(49, subscriber.ColumnInfos.Count);

				AssertEquals("IM_PK", subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

				AssertEquals("IM_BugFixDeployed", subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[1].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

				AssertEquals("IM_Category", subscriber.ColumnInfos[2].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

				AssertEquals("IM_ChargableWork", subscriber.ColumnInfos[3].ColumnName);
				AssertEquals("char(1)", subscriber.ColumnInfos[3].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

				AssertEquals("IM_ClientBugSeverity", subscriber.ColumnInfos[4].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

				AssertEquals("IM_CloseTimeUtc", subscriber.ColumnInfos[5].ColumnName);
				AssertEquals("datetime", subscriber.ColumnInfos[5].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

				AssertEquals("IM_ClosureResolution", subscriber.ColumnInfos[6].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

				AssertEquals("IM_DefectDisposition", subscriber.ColumnInfos[7].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

				AssertEquals("IM_DefectStatus", subscriber.ColumnInfos[8].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

				AssertEquals("IM_DepositAmountRequired", subscriber.ColumnInfos[9].ColumnName);
				AssertEquals("money", subscriber.ColumnInfos[9].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

				AssertEquals("IM_Description", subscriber.ColumnInfos[10].ColumnName);
				AssertEquals("nvarchar(80)", subscriber.ColumnInfos[10].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

				AssertEquals("IM_FeatureRequestCost", subscriber.ColumnInfos[11].ColumnName);
				AssertEquals("money", subscriber.ColumnInfos[11].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

				AssertEquals("IM_FeatureRequestDisposition", subscriber.ColumnInfos[12].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

				AssertEquals("IM_FeatureRequestIndustryValue", subscriber.ColumnInfos[13].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

				AssertEquals("IM_FeatureRequestStatus", subscriber.ColumnInfos[14].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[14].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

				AssertEquals("IM_FeatureRequestType", subscriber.ColumnInfos[15].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[15].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

				AssertEquals("IM_GC", subscriber.ColumnInfos[16].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[16].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[16].IsNullable);

				AssertEquals("IM_GG_Team", subscriber.ColumnInfos[17].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[17].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[17].IsNullable);

				AssertEquals("IM_IMT_Triage", subscriber.ColumnInfos[18].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[18].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[18].IsNullable);

				AssertEquals("IM_INC_Request", subscriber.ColumnInfos[19].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[19].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[19].IsNullable);

				AssertEquals("IM_IncidentNumber", subscriber.ColumnInfos[20].ColumnName);
				AssertEquals("varchar(20)", subscriber.ColumnInfos[20].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

				AssertEquals("IM_IncidentType", subscriber.ColumnInfos[21].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[21].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

				AssertEquals("IM_Language", subscriber.ColumnInfos[22].ColumnName);
				AssertEquals("varchar(7)", subscriber.ColumnInfos[22].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

				AssertEquals("IM_Module", subscriber.ColumnInfos[23].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[23].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);

				AssertEquals("IM_OA_BranchAddress", subscriber.ColumnInfos[24].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[24].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[24].IsNullable);

				AssertEquals("IM_OC_Contact", subscriber.ColumnInfos[25].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[25].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[25].IsNullable);

				AssertEquals("IM_OH_Client", subscriber.ColumnInfos[26].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[26].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[26].IsNullable);

				AssertEquals("IM_PatchTo", subscriber.ColumnInfos[27].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[27].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[27].IsNullable);

				AssertEquals("IM_Priority", subscriber.ColumnInfos[28].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[28].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[28].IsNullable);

				AssertEquals("IM_Product", subscriber.ColumnInfos[29].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[29].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[29].IsNullable);

				AssertEquals("IM_ProgramArea", subscriber.ColumnInfos[30].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[30].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[30].IsNullable);

				AssertEquals("IM_QuoteAmount", subscriber.ColumnInfos[31].ColumnName);
				AssertEquals("money", subscriber.ColumnInfos[31].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[31].IsNullable);

				AssertEquals("IM_ResolutionCode", subscriber.ColumnInfos[32].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[32].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[32].IsNullable);

				AssertEquals("IM_ResolveTimeUtc", subscriber.ColumnInfos[33].ColumnName);
				AssertEquals("datetime", subscriber.ColumnInfos[33].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[33].IsNullable);

				AssertEquals("IM_RN_NKCountry", subscriber.ColumnInfos[34].ColumnName);
				AssertEquals("varchar(2)", subscriber.ColumnInfos[34].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[34].IsNullable);

				AssertEquals("IM_RX_NKQuoteCurrency", subscriber.ColumnInfos[35].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[35].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[35].IsNullable);

				AssertEquals("IM_ServiceType", subscriber.ColumnInfos[36].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[36].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[36].IsNullable);

				AssertEquals("IM_Source", subscriber.ColumnInfos[37].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[37].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[37].IsNullable);

				AssertEquals("IM_SourceModuleId", subscriber.ColumnInfos[38].ColumnName);
				AssertEquals("varchar(50)", subscriber.ColumnInfos[38].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[38].IsNullable);

				AssertEquals("IM_Status", subscriber.ColumnInfos[39].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[39].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[39].IsNullable);

				AssertEquals("IM_SubCategory", subscriber.ColumnInfos[40].ColumnName);
				AssertEquals("varchar(20)", subscriber.ColumnInfos[40].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[40].IsNullable);

				AssertEquals("IM_SystemCreateTimeUtc", subscriber.ColumnInfos[41].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[41].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[41].IsNullable);

				AssertEquals("IM_SystemCreateUser", subscriber.ColumnInfos[42].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[42].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[42].IsNullable);

				AssertEquals("IM_SystemCreateBranch", subscriber.ColumnInfos[43].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[43].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[43].IsNullable);

				AssertEquals("IM_SystemCreateDepartment", subscriber.ColumnInfos[44].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[44].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[44].IsNullable);

				AssertEquals("IM_SystemLastEditTimeUtc", subscriber.ColumnInfos[45].ColumnName);
				AssertEquals("datetime", subscriber.ColumnInfos[45].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[45].IsNullable);

				AssertEquals("IM_SystemLastEditUser", subscriber.ColumnInfos[46].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[46].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[46].IsNullable);

				AssertEquals("IM_UpgradeAssuranceAccepted", subscriber.ColumnInfos[47].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[47].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[47].IsNullable);

				AssertEquals("IM_WorkItemType", subscriber.ColumnInfos[48].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[48].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[48].IsNullable);
			});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "IM_ServiceStatus";
				yield return "IM_HL_ClientReportedOnVersion";
			}
		}
	}
}
