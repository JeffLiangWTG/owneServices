using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing.Tests
{
	[TestedType(typeof(AccChargeCodeSubscriber))]
	class AccChargeCodeSubscriberTest : DataScienceAuditSubscriberTestBase<AccChargeCodeSubscriber>
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
The schema of the table AccChargeCode required by AccChargeCodeSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(47, subscriber.ColumnInfos.Count);

					AssertEquals("AC_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("AC_AC_RevenueChargeCode", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("AC_AG_AccrualAccount", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("AC_AG_CostAccount", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("AC_AG_CostClearingAccount", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("AC_AG_DisbursementShortfallAccount", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("AC_AG_DisbursementSurplusAccount", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("AC_AG_RevenueAccount", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("AC_AG_RevenueClearingAccount", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("AC_AG_WIPAccount", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("AC_AllowDescriptionOvertype", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("AC_AR_ExpenseGroup", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("AC_AR_SalesGroup", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("AC_AT_GSTRate", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("AC_AW_WithholdingTaxRate", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("AC_AX_TaxOverrideGroup", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("AC_ChargeGroup", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("AC_ChargeOtherGroups", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("AC_ChargeSubGroup", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("AC_ChargeType", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("AC_Code", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("varchar(10)", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("AC_DefaultCommissionProduct", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("varchar(8)", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("AC_DefaultCommissionService", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("varchar(8)", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("AC_DefaultCommissionSubModule", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("varchar(8)", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);

					AssertEquals("AC_DepartmentFilterList", subscriber.ColumnInfos[24].ColumnName);
					AssertEquals("varchar(200)", subscriber.ColumnInfos[24].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[24].IsNullable);

					AssertEquals("AC_Desc", subscriber.ColumnInfos[25].ColumnName);
					AssertEquals("varchar(80)", subscriber.ColumnInfos[25].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[25].IsNullable);

					AssertEquals("AC_EnergySourceGroup", subscriber.ColumnInfos[26].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[26].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[26].IsNullable);

					AssertEquals("AC_ENettChargeCodeMap", subscriber.ColumnInfos[27].ColumnName);
					AssertEquals("varchar(10)", subscriber.ColumnInfos[27].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[27].IsNullable);

					AssertEquals("AC_GC", subscriber.ColumnInfos[28].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[28].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[28].IsNullable);

					AssertEquals("AC_GoodsServiceType", subscriber.ColumnInfos[29].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[29].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[29].IsNullable);

					AssertEquals("AC_GovtChargeCode", subscriber.ColumnInfos[30].ColumnName);
					AssertEquals("nvarchar(23)", subscriber.ColumnInfos[30].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[30].IsNullable);

					AssertEquals("AC_IATA_ChargeCodeMap", subscriber.ColumnInfos[31].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[31].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[31].IsNullable);

					AssertEquals("AC_InputGSTVATRecoverable", subscriber.ColumnInfos[32].ColumnName);
					AssertEquals("decimal(5,4)", subscriber.ColumnInfos[32].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[32].IsNullable);

					AssertEquals("AC_IsActive", subscriber.ColumnInfos[33].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[33].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[33].IsNullable);

					AssertEquals("AC_IsAdhocServiceCharge", subscriber.ColumnInfos[34].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[34].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[34].IsNullable);

					AssertEquals("AC_IsCommissionable", subscriber.ColumnInfos[35].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[35].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[35].IsNullable);

					AssertEquals("AC_IsGroupageCharge", subscriber.ColumnInfos[36].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[36].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[36].IsNullable);

					AssertEquals("AC_LocalLanguageDescription", subscriber.ColumnInfos[37].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[37].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[37].IsNullable);

					AssertEquals("AC_MarginPercentage", subscriber.ColumnInfos[38].ColumnName);
					AssertEquals("decimal(5,2)", subscriber.ColumnInfos[38].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[38].IsNullable);

					AssertEquals("AC_PrintSequence", subscriber.ColumnInfos[39].ColumnName);
					AssertEquals("smallint", subscriber.ColumnInfos[39].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[39].IsNullable);

					AssertEquals("AC_RateCalculator", subscriber.ColumnInfos[40].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[40].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[40].IsNullable);

					AssertEquals("AC_ShowOnQuotation", subscriber.ColumnInfos[41].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[41].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[41].IsNullable);

					AssertEquals("AC_SuppressOnQuoteIfZero", subscriber.ColumnInfos[42].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[42].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[42].IsNullable);

					AssertEquals("AC_SystemCreateTimeUtc", subscriber.ColumnInfos[43].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[43].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[43].IsNullable);

					AssertEquals("AC_SystemCreateUser", subscriber.ColumnInfos[44].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[44].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[44].IsNullable);

					AssertEquals("AC_SystemLastEditTimeUtc", subscriber.ColumnInfos[45].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[45].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[45].IsNullable);

					AssertEquals("AC_SystemLastEditUser", subscriber.ColumnInfos[46].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[46].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[46].IsNullable);
				}
			);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new AccChargeCodeSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
