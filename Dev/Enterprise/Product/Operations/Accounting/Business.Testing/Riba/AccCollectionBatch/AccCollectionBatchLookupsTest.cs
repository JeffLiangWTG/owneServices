using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	internal class AccCollectionBatchLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestACB_CollectionFileFormat_List()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
				AssertEquals(4, batch.Lookups.ACB_CollectionFileFormat_List.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SEP", "DEF", "RIB", "UNK" }, batch.Lookups.ACB_CollectionFileFormat_List.GetAllCodes());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
				AssertEquals(3, batch.Lookups.ACB_CollectionFileFormat_List.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SEP", "DEF", "UNK" }, batch.Lookups.ACB_CollectionFileFormat_List.GetAllCodes());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
				AssertEquals(2, batch.Lookups.ACB_CollectionFileFormat_List.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "DEF", "UNK" }, batch.Lookups.ACB_CollectionFileFormat_List.GetAllCodes());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
				AssertEquals(1, batch.Lookups.ACB_CollectionFileFormat_List.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "ITA" }, batch.Lookups.ACB_CollectionFileFormat_List.GetAllCodes());
			}
		}

		public void TestBankAccounts()
		{
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			bank1.AB_GB = GlbBranch.CurrentBranch.PK;
			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();
			bank2.AB_GB = ZGuid.Empty;
			AccBankAccount bank3 = Factory.NewWithValidTestData<AccBankAccount>();
			bank3.AB_GB = newBranch.PK;
			AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
			AssertNotNull("BankAccounts should not be null", batch.Lookups.BankAccounts);
			AssertEquals("BankAccounts Type", typeof(AccBankAccountCollection), batch.Lookups.BankAccounts.GetType());
			batch.Lookups.BankAccounts.Load();
			AssertEquals("BankAccounts Count", 2, batch.Lookups.BankAccounts.Count);
			AssertEquals("BankAccounts contains Bank 1", true, batch.Lookups.BankAccounts.Contains(bank1.PK));
			AssertEquals("BankAccounts contains Bank 2", true, batch.Lookups.BankAccounts.Contains(bank2.PK));
			AssertEquals("BankAccounts contains Bank 3", false, batch.Lookups.BankAccounts.Contains(bank3.PK));
		}

		public void TestCheckACB_Type()
		{
			var registry = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			registry.RemoveAll();
			registry.Add("STD", (NoResString)"Standard Batch", false);
			registry.Add("ST1", (NoResString)"STD Desc", false);
			registry.Add("ST2", (NoResString)"STD Desc", true);
			registry.Add("ST3", (NoResString)"STD Desc", false);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			AssertEquals("Collection batch type is a CodeDescriptionBoolCollection", typeof(CodeDescriptionBoolCollection), registry.GetType());
			AssertEquals("STD Standard batch type code is present", true, registry.ContainsCode("STD"));
			AssertEquals("STD Standard batch not ticked", false, registry.GetBoolFromCode("STD"));
			AssertEquals("ST1 code is present", true, registry.ContainsCode("ST1"));
			AssertEquals("ST1 type not ticked", false, registry.GetBoolFromCode("ST1"));
			AssertEquals("ST2 code is present", true, registry.ContainsCode("ST2"));
			AssertEquals("ST2 type ticked", true, registry.GetBoolFromCode("ST2"));
			AssertEquals("ST3 code is present", true, registry.ContainsCode("ST3"));
			AssertEquals("ST3 type not ticked", false, registry.GetBoolFromCode("ST3"));
		}
	}
}
