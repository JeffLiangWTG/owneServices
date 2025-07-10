using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(UpdateRegistryKeyItemsFromB3ToEntry))]
	public class UpdateRegistryKeyItemsFromB3ToEntryTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("B3StatementDateThreshold"));
			AssertEquals(0, Helper.GetStmDataRowCount("B3AutomaticSendingDelayThresholds"));
			AssertEquals(0, Helper.GetStmDataRowCount("B3LateSendingFailsafeWarningThresholds"));
			AssertEquals(0, Helper.GetStmDataRowCount("DefaultDeferredLowValueB3SendAction"));
			AssertEquals(0, Helper.GetStmDataRowCount("DefaultDeferredNormalB3SendAction"));
			AssertEquals(0, Helper.GetStmDataRowCount("ACROSSHighValueProductAudit"));
			AssertEquals(0, Helper.GetStmDataRowCount("ACROSSLowValueProductAudit"));
			AssertEquals(0, Helper.GetStmDataRowCount("B3HighValueProductAudit"));
			AssertEquals(0, Helper.GetStmDataRowCount("B3LowValueProductAudit"));

			AssertEquals(1, Helper.GetStmDataRowCount("EntryStatementDateThreshold"));
			AssertEquals(1, Helper.GetStmDataRowCount("EntryAutomaticSendingDelayThresholds"));
			AssertEquals(1, Helper.GetStmDataRowCount("EntryLateSendingFailsafeWarningThresholds"));
			AssertEquals(1, Helper.GetStmDataRowCount("DefaultDeferredLowValueEntrySendAction"));
			AssertEquals(1, Helper.GetStmDataRowCount("DefaultDeferredNormalEntrySendAction"));
			AssertEquals(1, Helper.GetStmDataRowCount("ReleaseHighValueProductAudit"));
			AssertEquals(1, Helper.GetStmDataRowCount("ReleaseLowValueProductAudit"));
			AssertEquals(1, Helper.GetStmDataRowCount("EntryLowValueProductAudit"));
			AssertEquals(1, Helper.GetStmDataRowCount("EntryLowValueProductAudit"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateRegistryKeyItemsFromB3ToEntry();

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("B3StatementDateThreshold", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("B3AutomaticSendingDelayThresholds", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("B3LateSendingFailsafeWarningThresholds", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("DefaultDeferredLowValueB3SendAction", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("DefaultDeferredNormalB3SendAction", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("ACROSSHighValueProductAudit", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("ACROSSLowValueProductAudit", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("B3HighValueProductAudit", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("B3LowValueProductAudit", Guid.Empty, Guid.Empty);
		}
	}
}

