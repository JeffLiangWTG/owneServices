using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.AU
{
	[TestedType(typeof(AUCmrReferenceDbUpgrader))]
	class AUCmrReferenceDbUpgraderTest : ReferenceDbUpgraderTest<AUCmrReferenceDbUpgrader>
	{
		protected override string[] ExpectedReferenceTables
		{
			get
			{
				return new string[]
				{
					"CMRAHECCCode",
					"CMRAqisCommodity",
					"CMRAqisCommodityStatisticalClassification",
					"CMRAqisConcern",
					"CMRAqisDocumentType",
					"CMRAqisEntity",
					"CMRAqisPremises",
					"CMRAqisProcessingType",
					"CMRAqisProducer",
					"CMRBerthCode",
					"CMRCharacteristic",
					"CMRCommunityProtectionProfile",
					"CMRCommunityProtectionRisk",
					"CMRCommunityProtectionRiskMessageAdvice",
					"CMRCustomsShipRegister",
					"CMREstablishmentCodes",
					"CMRInstrument",
					"CMRInstrumentCategory",
					"CMRInstrumentCategoryCharacteristic",
					"CMRInstrumentCategoryCountry",
					"CMRInstrumentCategoryMessageAdvice",
					"CMRInstrumentCategoryPreferenceScheme",
					"CMRInstrumentCategoryTariffGroup",
					"CMRInstrumentCharacteristic",
					"CMRInstrumentCountry",
					"CMRInstrumentMessageAdvice",
					"CMRInstrumentPreferenceScheme",
					"CMRInstrumentTariffGroup",
					"CMRLodgementQuestion",
					"CMRMessageAdvice",
					"CMRPermitRequirement",
					"CMRPermitRequirementExclusions",
					"CMRPreferenceRulePeriodCharacteristic",
					"CMRPreferenceRulePeriodCountry",
					"CMRPreferenceRulePeriodSnapshot",
					"CMRPreferenceRulePeriodTariffGroup",
					"CMRPreferenceSchemePeriodCountry",
					"CMRPreferenceSchemePeriodSnapshot",
					"CMRPreferenceSchemeRule",
					"CMRPreferenceSchemeRuleMessageAdvice",
					"CMRRefundReason",
					"CMRStatisticalClassificationPeriodCharacteristic",
					"CMRStatisticalClassificationPeriodMessageAdvice",
					"CMRStatisticalClassificationPeriodSnapshot",
					"CMRTariffClassificationCharacteristic",
					"CMRTariffClassificationConcordance",
					"CMRTariffClassificationMessageAdvice",
					"CMRTariffClassificationSnapshot",
					"CMRTariffRatePeriodAdditionalDutyCalculation",
					"CMRTariffRatePeriodCharacteristic",
					"CMRTariffRatePeriodMessageAdvice",
					"CMRTariffRatePeriodSnapshot",
					"CMRTreatmentRatePeriodAdditionalDutyCalculation",
					"CMRTreatmentRatePeriodCharacteristic",
					"CMRTreatmentRatePeriodMessageAdvice",
					"CMRTreatmentRatePeriodSnapshot",
					"CMRTreatmentSnapshot",
					"CMRTreatmentSnapshotCountry",
					"CMRTreatmentSnapshotTariffGroup",
					"CMRCodeLists",
					"CMRSeaImpendingArrivals",
					"CMRSACThesaurus",
					"CMRAqisPostCodes",
				};
			}
		}

		protected override void PerformExtraAssertsAfterUpgrade(DbConnection refDbConn)
		{
			base.PerformExtraAssertsAfterUpgrade(refDbConn);

			AssertTableRowCount(refDbConn, "CMRSACThesaurus", 135);
			AssertTableRowCount(refDbConn, "CMRAqisPostCodes", 941);

			AssertEquals(true, DbObjectCreator.IndexExists(refDbConn, "CMRCommunityProtectionProfile", "NR_UX__CMRCommunityProtectionProfile1"));

			AssertEquals(false, DbObjectCreator.IndexExists(refDbConn, "CMRRefundReason", "NR_UX__CMRRefundReason1"));
			AssertEquals(true, DbObjectCreator.IndexExists(refDbConn, "CMRRefundReason", "NR_UX__CMRRefundReason"));
			DbObjectCreator.IndexColumnExists(refDbConn, "CMRRefundReason", "NR_UX__CMRRefundReason", 1, "CR_RefundReasonTimeLimitDayCount");
		}
	}

	[TestedType(typeof(AUCmrReferenceDbUpgrader))]
	sealed class AUCmrReferenceDbUpgraderWithExistingStmExtendedPropertyTest : AUCmrReferenceDbUpgraderTest
	{
		protected override string[] ExpectedReferenceTables
		{
			get
			{
				var result = new List<string>();
				result.AddRange(base.ExpectedReferenceTables);
				result.Add("StmExtendedProperty");
				return result.ToArray();
			}
		}

		protected override bool ExistingDataRequired => true;

		protected override void PrepareExistingDataBeforeUpgrade(DbConnection refDbConn)
		{
			var sql = @"
CREATE TABLE [StmExtendedProperty] (
   [SEP_Class] VARCHAR(60) NOT NULL DEFAULT '',
   [SEP_DatabaseNameSuffix] VARCHAR(128) NOT NULL DEFAULT '',
   [SEP_SchemaName] NVARCHAR(128) NOT NULL DEFAULT '',
   [SEP_MajorObjectName] NVARCHAR(128) NOT NULL DEFAULT '',
   [SEP_MinorObjectName] NVARCHAR(128) NOT NULL DEFAULT '',
   [SEP_Name] VARCHAR(200) NOT NULL DEFAULT '',
   [SEP_Value] VARCHAR(200) NOT NULL DEFAULT '',
);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__SEP_Class_SEP_DatabaseNameSuffix_SEP_SchemaName_SEP_MajorObjectName_SEP_MinorObjectName_SEP_Name] ON [StmExtendedProperty] ([SEP_Class] ASC,[SEP_DatabaseNameSuffix] ASC,[SEP_SchemaName] ASC,[SEP_MajorObjectName] ASC,[SEP_MinorObjectName] ASC,[SEP_Name] ASC)

ALTER TABLE [StmExtendedProperty]
    SET (LOCK_ESCALATION = DISABLE);
";
			refDbConn.ExecuteNonQuery(sql);
		}

		protected override void PerformExtraAssertsAfterUpgrade(DbConnection refDbConn)
		{
			base.PerformExtraAssertsAfterUpgrade(refDbConn);

			AssertEquals("Column SEP_SystemCreateTimeUtc exists in StmExtendedProperty", true, DbObjectCreator.ColumnExists(refDbConn, "StmExtendedProperty", "SEP_SystemCreateTimeUtc"));
			AssertEquals("Column SEP_SystemCreateUser exists in StmExtendedProperty", true, DbObjectCreator.ColumnExists(refDbConn, "StmExtendedProperty", "SEP_SystemCreateUser"));
			AssertEquals("Column SEP_SystemLastEditTimeUtc exists in StmExtendedProperty", true, DbObjectCreator.ColumnExists(refDbConn, "StmExtendedProperty", "SEP_SystemLastEditTimeUtc"));
			AssertEquals("Column SEP_SystemLastEditUser exists in StmExtendedProperty", true, DbObjectCreator.ColumnExists(refDbConn, "StmExtendedProperty", "SEP_SystemLastEditUser"));
			AssertEquals("Index NR_RX__SEP_SystemCreateTimeUtc exists in StmExtendedProperty", true, DbObjectCreator.IndexExists(refDbConn, "StmExtendedProperty", "NR_RX__SEP_SystemCreateTimeUtc"));
			AssertEquals("Index NR_RX__SEP_SystemLastEditTimeUtc exists in StmExtendedProperty", true, DbObjectCreator.IndexExists(refDbConn, "StmExtendedProperty", "NR_RX__SEP_SystemLastEditTimeUtc"));
		}
	}
}
