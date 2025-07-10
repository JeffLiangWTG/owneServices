using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(OrgHeaderSubscriber))]
	class OrgHeaderSubscriberTest : DataScienceAuditSubscriberTestBase<OrgHeaderSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new OrgHeaderSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new OrgHeaderSubscriber();

			// Assert
			AssertEquals("DOH", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
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
The schema of the table OrgHeader required by OrgHeaderSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(86, subscriber.ColumnInfos.Count);

					AssertEquals("OH_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("OH_Category", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("OH_Code", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("nvarchar(12)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("OH_FullName", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("nvarchar(100)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("OH_IsActive", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("OH_IsAirCTO", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("OH_IsAirLine", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("OH_IsAirWholesaler", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("OH_IsBroker", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("OH_IsCompetitor", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("OH_IsConsignee", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("OH_IsConsignor", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("OH_IsContainerYard", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("OH_IsControllingAgent", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("OH_IsControllingCustomer", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("OH_IsDistributionCentre", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("OH_IsForwarder", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("OH_IsFumigationContractor", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("OH_IsGlobalAccount", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("OH_IsLineHaulProvider", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("OH_IsLocalTransport", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("OH_IsMiscFreightServices", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("OH_IsNationalAccount", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("OH_IsPackDepot", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);

					AssertEquals("OH_IsPersonalEffectsAccount", subscriber.ColumnInfos[24].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[24].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[24].IsNullable);

					AssertEquals("OH_IsRailHead", subscriber.ColumnInfos[25].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[25].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[25].IsNullable);

					AssertEquals("OH_IsRailProvider", subscriber.ColumnInfos[26].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[26].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[26].IsNullable);

					AssertEquals("OH_IsRoadFreightDepot", subscriber.ColumnInfos[27].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[27].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[27].IsNullable);

					AssertEquals("OH_IsSalesLead", subscriber.ColumnInfos[28].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[28].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[28].IsNullable);

					AssertEquals("OH_IsSeaCTO", subscriber.ColumnInfos[29].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[29].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[29].IsNullable);

					AssertEquals("OH_IsSeaWholesaler", subscriber.ColumnInfos[30].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[30].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[30].IsNullable);

					AssertEquals("OH_IsShippingConsortium", subscriber.ColumnInfos[31].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[31].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[31].IsNullable);

					AssertEquals("OH_IsShippingLine", subscriber.ColumnInfos[32].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[32].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[32].IsNullable);

					AssertEquals("OH_IsShippingProvider", subscriber.ColumnInfos[33].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[33].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[33].IsNullable);

					AssertEquals("OH_IsTempAccount", subscriber.ColumnInfos[34].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[34].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[34].IsNullable);

					AssertEquals("OH_IsTransportClient", subscriber.ColumnInfos[35].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[35].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[35].IsNullable);

					AssertEquals("OH_IsUnpackDepot", subscriber.ColumnInfos[36].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[36].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[36].IsNullable);

					AssertEquals("OH_IsUserFlag1", subscriber.ColumnInfos[37].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[37].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[37].IsNullable);

					AssertEquals("OH_IsUserFlag10", subscriber.ColumnInfos[38].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[38].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[38].IsNullable);

					AssertEquals("OH_IsUserFlag11", subscriber.ColumnInfos[39].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[39].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[39].IsNullable);

					AssertEquals("OH_IsUserFlag12", subscriber.ColumnInfos[40].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[40].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[40].IsNullable);

					AssertEquals("OH_IsUserFlag13", subscriber.ColumnInfos[41].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[41].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[41].IsNullable);

					AssertEquals("OH_IsUserFlag14", subscriber.ColumnInfos[42].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[42].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[42].IsNullable);

					AssertEquals("OH_IsUserFlag15", subscriber.ColumnInfos[43].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[43].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[43].IsNullable);

					AssertEquals("OH_IsUserFlag16", subscriber.ColumnInfos[44].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[44].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[44].IsNullable);

					AssertEquals("OH_IsUserFlag17", subscriber.ColumnInfos[45].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[45].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[45].IsNullable);

					AssertEquals("OH_IsUserFlag18", subscriber.ColumnInfos[46].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[46].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[46].IsNullable);

					AssertEquals("OH_IsUserFlag19", subscriber.ColumnInfos[47].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[47].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[47].IsNullable);

					AssertEquals("OH_IsUserFlag2", subscriber.ColumnInfos[48].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[48].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[48].IsNullable);

					AssertEquals("OH_IsUserFlag20", subscriber.ColumnInfos[49].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[49].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[49].IsNullable);

					AssertEquals("OH_IsUserFlag21", subscriber.ColumnInfos[50].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[50].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[50].IsNullable);

					AssertEquals("OH_IsUserFlag22", subscriber.ColumnInfos[51].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[51].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[51].IsNullable);

					AssertEquals("OH_IsUserFlag23", subscriber.ColumnInfos[52].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[52].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[52].IsNullable);

					AssertEquals("OH_IsUserFlag24", subscriber.ColumnInfos[53].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[53].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[53].IsNullable);

					AssertEquals("OH_IsUserFlag3", subscriber.ColumnInfos[54].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[54].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[54].IsNullable);

					AssertEquals("OH_IsUserFlag4", subscriber.ColumnInfos[55].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[55].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[55].IsNullable);

					AssertEquals("OH_IsUserFlag5", subscriber.ColumnInfos[56].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[56].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[56].IsNullable);

					AssertEquals("OH_IsUserFlag6", subscriber.ColumnInfos[57].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[57].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[57].IsNullable);

					AssertEquals("OH_IsUserFlag7", subscriber.ColumnInfos[58].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[58].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[58].IsNullable);

					AssertEquals("OH_IsUserFlag8", subscriber.ColumnInfos[59].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[59].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[59].IsNullable);

					AssertEquals("OH_IsUserFlag9", subscriber.ColumnInfos[60].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[60].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[60].IsNullable);

					AssertEquals("OH_IsValid", subscriber.ColumnInfos[61].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[61].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[61].IsNullable);

					AssertEquals("OH_IsWarehouseClient", subscriber.ColumnInfos[62].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[62].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[62].IsNullable);

					AssertEquals("OH_Language", subscriber.ColumnInfos[63].ColumnName);
					AssertEquals("varchar(7)", subscriber.ColumnInfos[63].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[63].IsNullable);

					AssertEquals("OH_RL_NKClosestPort", subscriber.ColumnInfos[64].ColumnName);
					AssertEquals("varchar(5)", subscriber.ColumnInfos[64].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[64].IsNullable);

					AssertEquals("OH_SystemCreateTimeUtc", subscriber.ColumnInfos[65].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[65].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[65].IsNullable);

					AssertEquals("OH_SystemCreateUser", subscriber.ColumnInfos[66].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[66].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[66].IsNullable);

					AssertEquals("OH_SystemCreateBranch", subscriber.ColumnInfos[67].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[67].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[67].IsNullable);

					AssertEquals("OH_SystemCreateDepartment", subscriber.ColumnInfos[68].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[68].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[68].IsNullable);

					AssertEquals("OH_SystemLastEditUser", subscriber.ColumnInfos[69].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[69].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[69].IsNullable);

					AssertEquals("OH_ScreeningStatus", subscriber.ColumnInfos[70].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[70].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[70].IsNullable);

					AssertEquals("OH_RSL_ShippingLine", subscriber.ColumnInfos[71].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[71].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[71].IsNullable);

					AssertEquals("OH_IsUserFlag25", subscriber.ColumnInfos[72].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[72].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[72].IsNullable);

					AssertEquals("OH_IsUserFlag26", subscriber.ColumnInfos[73].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[73].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[73].IsNullable);

					AssertEquals("OH_IsUserFlag27", subscriber.ColumnInfos[74].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[74].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[74].IsNullable);

					AssertEquals("OH_IsUserFlag28", subscriber.ColumnInfos[75].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[75].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[75].IsNullable);

					AssertEquals("OH_IsUserFlag29", subscriber.ColumnInfos[76].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[76].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[76].IsNullable);

					AssertEquals("OH_IsUserFlag30", subscriber.ColumnInfos[77].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[77].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[77].IsNullable);

					AssertEquals("OH_IsUserFlag31", subscriber.ColumnInfos[78].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[78].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[78].IsNullable);

					AssertEquals("OH_IsUserFlag32", subscriber.ColumnInfos[79].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[79].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[79].IsNullable);

					AssertEquals("OH_IsFerryWaterTerminal", subscriber.ColumnInfos[80].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[80].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[80].IsNullable);

					AssertEquals("OH_IsContainerLeasingCompany", subscriber.ColumnInfos[81].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[81].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[81].IsNullable);

					AssertEquals("OH_OverrideAdditionalAddressInformation", subscriber.ColumnInfos[82].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[82].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[82].IsNullable);

					AssertEquals("OH_IsInlandWaterwayProvider", subscriber.ColumnInfos[83].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[83].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[83].IsNullable);

					AssertEquals("OH_IsVGMContractor", subscriber.ColumnInfos[84].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[84].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[84].IsNullable);

					AssertEquals("OH_SystemLastEditTimeUtc", subscriber.ColumnInfos[85].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[85].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[85].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
