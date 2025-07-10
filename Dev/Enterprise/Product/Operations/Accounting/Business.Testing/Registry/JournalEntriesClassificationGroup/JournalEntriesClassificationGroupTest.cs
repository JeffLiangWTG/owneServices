using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesClassificationGroup))]
	public class JournalEntriesClassificationGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Properties

		#region GroupCode

		public void TestGroupCode()
		{
			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection();
			var aPCTRJournalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();
			aPCTRJournalEntriesClassificationGroup.Ledger = LedgerTypes.AccountsPayable;
			aPCTRJournalEntriesClassificationGroup.TransactionType = TransactionTypes.Contra;

			AssertEquals(string.Empty, aPCTRJournalEntriesClassificationGroup.GroupCode);

			var journalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();
			journalEntriesClassificationGroup.Ledger = LedgerTypes.AccountsReceivable;
			journalEntriesClassificationGroup.TransactionType = TransactionTypes.Invoice;
			journalEntriesClassificationGroup.GroupCode = "TST";
			AssertEquals("TST", journalEntriesClassificationGroup.GroupCode);
			AssertEquals(string.Empty, aPCTRJournalEntriesClassificationGroup.GroupCode);

			journalEntriesClassificationGroup.TransactionType = TransactionTypes.Contra;
			journalEntriesClassificationGroup.GroupCode = "TS2";
			AssertEquals("TS2", journalEntriesClassificationGroup.GroupCode);
			AssertEquals("TS2", aPCTRJournalEntriesClassificationGroup.GroupCode);
		}

		#endregion

		#region GroupCodeDescription

		public void TestGroupCodeDescription()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Test Description";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);

			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			var journalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();
			journalEntriesClassificationGroup.GroupCode = "TST";

			AssertEquals("Test Description", journalEntriesClassificationGroup.GroupCodeDescription);
		}

		#endregion

		#endregion

		#region ReadOnly

		public void TestLedger_ReadOnly()
		{
			var journalEntriesClassificationGroup = new JournalEntriesClassificationGroup();
			Assert(journalEntriesClassificationGroup.LedgerInfo.ReadOnly);
		}

		public void TestTransactionType_ReadOnly()
		{
			var journalEntriesClassificationGroup = new JournalEntriesClassificationGroup();
			Assert(journalEntriesClassificationGroup.TransactionTypeInfo.ReadOnly);
		}

		public void TestGroupCode_ReadOnly()
		{
			var journalEntriesClassificationGroup = new JournalEntriesClassificationGroup();
			AssertEquals(false, journalEntriesClassificationGroup.GroupCodeInfo.ReadOnly);

			journalEntriesClassificationGroup.Ledger = LedgerTypes.AccountsPayable;
			journalEntriesClassificationGroup.TransactionType = TransactionTypes.Contra;
			AssertEquals(true, journalEntriesClassificationGroup.GroupCodeInfo.ReadOnly);
		}

		#endregion

		#region Validation

		public void TestValidateGroupCode()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Description";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);

			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			var journalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();
			journalEntriesClassificationGroup.GroupCode = "TS2";
			journalEntriesClassificationGroupCodeCollection.RunPreSaveValidation();
			AssertHasErrors("Please enter a valid Group Code.", journalEntriesClassificationGroup.GroupCodeInfo);

			journalEntriesClassificationGroup.GroupCode = "TST";
			journalEntriesClassificationGroupCodeCollection.RunPreSaveValidation();
			AssertEquals(false, journalEntriesClassificationGroup.GroupCodeInfo.HasErrors());

			var journalEntriesClassificationGroup2 = journalEntriesClassificationGroupCollection.AddNew();
			journalEntriesClassificationGroup.RunPreSaveValidation();
			AssertHasErrors("Group Code must be all filled or all not filled.", journalEntriesClassificationGroup.GroupCodeInfo);

			journalEntriesClassificationGroup2.GroupCode = "TST";
			journalEntriesClassificationGroup.RunPreSaveValidation();
			AssertEquals(false, journalEntriesClassificationGroup.GroupCodeInfo.HasErrors());

			journalEntriesClassificationGroup.GroupCode = "";
			journalEntriesClassificationGroup2.GroupCode = "";
			journalEntriesClassificationGroup.RunPreSaveValidation();
			AssertEquals(false, journalEntriesClassificationGroup.GroupCodeInfo.HasErrors());
		}

		#endregion

		#region List

		public void TestGroupCodeList()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Description";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);

			var journalEntriesClassificationGroupCodeCollectionInOtherCompany = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCodeInOtherCompany = journalEntriesClassificationGroupCodeCollectionInOtherCompany.AddNew();
			journalEntriesClassificationGroupCodeInOtherCompany.Code = "TS2";
			journalEntriesClassificationGroupCodeInOtherCompany.Description = "Description2";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(GlbCompany.GetDemoCompany(Factory).PK.ToGuid(), Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollectionInOtherCompany);

			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			var journalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();

			AssertEquals(1, journalEntriesClassificationGroup.GroupCodeList.Count);

			var groupCodeInList = journalEntriesClassificationGroup.GroupCodeList.Cast<JournalEntriesClassificationGroupCode>().First();
			AssertEquals("TST", groupCodeInList.Code);
			AssertEquals("Description", groupCodeInList.Description);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new JournalEntriesClassificationGroup(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
