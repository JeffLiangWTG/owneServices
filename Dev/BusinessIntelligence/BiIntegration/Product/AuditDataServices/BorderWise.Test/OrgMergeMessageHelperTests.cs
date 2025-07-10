using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	class OrgMergeMessageHelperTests : TestCaseWithFactory
	{
		public void TestEnsureOrganisationMergerAddsExpectedLog()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(Factory, oldOrg, newOrg)) { ActionOnSave = OrganisationMergerActionOnSave.MergeOnly };
			testMerger.Save();

			var newFactory = new BusinessObjectFactory();
			var oldOrgReloaded = newFactory.Load<OrgHeader>(oldOrg.PK);

			var mergeLog = oldOrgReloaded.Logs.MostRecentLogByEventTime(OrganisationMerger.MergedLogEvent);
			AssertNotNull("Should have merge log on old org", mergeLog);
			AssertEndsWith("Should have new org pk in reference", "|" + newOrg.PK, mergeLog.SL_Reference);
		}

		public void TestIsMergeTransaction()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			oldOrg.Logs.AddNew(OrganisationMerger.MergedLogEvent, "Something|" + newOrg.PK);
			Factory.Save();

			var mergeMessageHelper = new OrgMergeMessageHelperForTest();
			mergeMessageHelper.HasPotentialOrgHeaderChangeOverride = true;
			mergeMessageHelper.TransactionEndTimeOverride = ZDateTime.UtcNow.AddDays(1);
			var isMerge = mergeMessageHelper.IsMergeTransaction(Array.Empty<byte>(), oldOrg.PK.ToGuid(), true, out var foundOrgPk, out var foundOrg);
			Assert(isMerge);
			AssertEquals(newOrg.PK, foundOrgPk);
			AssertNotNull(foundOrg);
			AssertEquals(newOrg.PK, foundOrg.PK);

			isMerge = mergeMessageHelper.IsMergeTransaction(Array.Empty<byte>(), oldOrg.PK.ToGuid(), false, out foundOrgPk, out foundOrg);
			Assert(isMerge);
			AssertEquals(newOrg.PK, foundOrgPk);
			AssertNull(foundOrg);

			mergeMessageHelper.HasPotentialOrgHeaderChangeOverride = false;
			isMerge = mergeMessageHelper.IsMergeTransaction(Array.Empty<byte>(), oldOrg.PK.ToGuid(), true, out foundOrgPk, out foundOrg);
			Assert(!isMerge);

			mergeMessageHelper.HasPotentialOrgHeaderChangeOverride = true;
			mergeMessageHelper.TransactionEndTimeOverride = ZDateTime.UtcNow.AddDays(-1);
			isMerge = mergeMessageHelper.IsMergeTransaction(Array.Empty<byte>(), oldOrg.PK.ToGuid(), true, out foundOrgPk, out foundOrg);
			Assert("Should not use logs after transaction end time", !isMerge);
		}

		public void TestIsMergeTransactionForContactChange()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			oldOrg.Logs.AddNew(OrganisationMerger.MergedLogEvent, "Something|" + Guid.NewGuid());
			Factory.Save();

			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_OH, typeof(Guid));
			changeTable.Columns.Add(OrgContactSchema.Constants.OC_IsActive, typeof(bool));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));

			var changeRow = changeTable.NewRow();
			changeRow[OrgContactSchema.Constants.OC_OH] = oldOrg.PK.ToGuid();
			changeRow[OrgContactSchema.Constants.OC_IsActive] = true;
			changeRow[AuditFieldNames.StartLsnFieldName] = Array.Empty<byte>();

			changeTable.Rows.Add(changeRow);

			var mergeMessageHelper = new OrgMergeMessageHelperForTest();
			mergeMessageHelper.HasPotentialOrgHeaderChangeOverride = true;
			mergeMessageHelper.TransactionEndTimeOverride = ZDateTime.UtcNow.AddDays(1);
			var isMerge = mergeMessageHelper.IsMergeTransactionForContactChange(changeRow);
			Assert("Should not check inserted rows", !isMerge);

			changeRow.AcceptChanges();
			changeRow.Delete(); // Mark row as deleted

			isMerge = mergeMessageHelper.IsMergeTransactionForContactChange(changeRow);
			Assert(isMerge);
		}

		public void TestIsMergeTransactionForOrgChange()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			oldOrg.Logs.AddNew(OrganisationMerger.MergedLogEvent, "Something|" + newOrg.PK);
			Factory.Save();

			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgHeaderSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(OrgHeaderSchema.Constants.OH_IsActive, typeof(bool));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));

			var changeRow = changeTable.NewRow();
			changeRow[OrgHeaderSchema.Constants.PK] = oldOrg.PK.ToGuid();
			changeRow[OrgHeaderSchema.Constants.OH_IsActive] = true;
			changeRow[AuditFieldNames.StartLsnFieldName] = Array.Empty<byte>();

			changeTable.Rows.Add(changeRow);

			var mergeMessageHelper = new OrgMergeMessageHelperForTest();
			mergeMessageHelper.HasPotentialOrgHeaderChangeOverride = true;
			mergeMessageHelper.TransactionEndTimeOverride = ZDateTime.UtcNow.AddDays(1);
			var isMerge = mergeMessageHelper.IsMergeTransactionForOrgChange(changeRow, out var foundOrgPk, out var foundOrg);
			Assert("Should not check inserted rows", !isMerge);

			changeRow.AcceptChanges();
			changeRow.Delete(); // Mark row as deleted

			isMerge = mergeMessageHelper.IsMergeTransactionForOrgChange(changeRow, out foundOrgPk, out foundOrg);
			Assert(isMerge);

			AssertEquals(newOrg.PK, foundOrgPk);
			AssertNotNull(foundOrg);
			AssertEquals(newOrg.PK, foundOrg.PK);
		}
	}

	class OrgMergeMessageHelperForTest : OrgMergeMessageHelper
	{
		public bool HasPotentialOrgHeaderChangeOverride { get; set; }
		public ZDateTime TransactionEndTimeOverride { get; set; }

		protected override bool HasPotentialOrgHeaderChange(byte[] transactionLsn, Guid orgPk) => HasPotentialOrgHeaderChangeOverride;

		protected override ZDateTime GetTransactionEndTime(DbConnection connection, byte[] transactionLsn) => TransactionEndTimeOverride;
	}
}
