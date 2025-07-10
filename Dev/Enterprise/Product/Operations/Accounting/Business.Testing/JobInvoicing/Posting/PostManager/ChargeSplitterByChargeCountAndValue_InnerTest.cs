using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Accounting.Business.JobInvoicing.Posting.ChargeSplitterByChargeCountAndValue;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	class ChargeSplitterByChargeCountAndValue_InnerTest : TestCaseWithFactory
	{
		public void TestShouldChargesBeSplit_MaxLineAndDeboreAddress()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				IReceivablesPostingChargeCollection charges = Helper.GetChargeCollection(3, ChinaClient);
				PostingChargeCollection totalCharges = new PostingChargeCollection();
				totalCharges.SetCharges(new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0), charges);

				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				Assert("Only 3 lines, max lines is 3", !ShouldChargesBeSplit_ForTestOnly(charges));

				Charge chg4 = Factory.New<Charge>();
				chg4.JR_OH_SellAccount = ChinaClient.PK;
				chg4.JR_OA_SellInvoiceAddress = ChinaClient.Addresses[0].PK;
				charges.Add(chg4);
				Assert("4 lines - 1 too many and Debtor address is in China.", ShouldChargesBeSplit_ForTestOnly(charges));

				charges = Helper.GetChargeCollection(4, AusClient);
				Assert("4 lines - 1 too many But Debtor address not in China.", !ShouldChargesBeSplit_ForTestOnly(charges));

				AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DIF");
				Assert("4 lines - 1 too many But Debtor address not in China.", ShouldChargesBeSplit_ForTestOnly(charges));

				AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SAM");
				Assert("4 lines - 1 too many But Debtor address not in China.", !ShouldChargesBeSplit_ForTestOnly(charges));

				AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL");
				Assert("4 lines - 1 too many But Debtor address not in China.", ShouldChargesBeSplit_ForTestOnly(charges));
			}
		}

		public void TestShouldChargesBeSplit_MaxValue()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL");

				IReceivablesPostingChargeCollection charges = Helper.GetChargeCollection(4, ChinaClient);
				PostingChargeCollection totalCharges = new PostingChargeCollection();
				totalCharges.SetCharges(new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0), charges);
				Assert("Max value not set in registry. No Splitting", !ShouldChargesBeSplit_ForTestOnly(charges));

				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);
				Assert("Total Charge Amount excceeds MaxValue", ShouldChargesBeSplit_ForTestOnly(charges));

				charges = Helper.GetChargeCollection(2, AusClient);
				Assert("Total Charge Amount doesn't excceed MaxValue", !ShouldChargesBeSplit_ForTestOnly(charges));
			}
		}

		public void TestSplitChargesCount()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				IReceivablesPostingChargeCollection chinaCharges = Helper.GetChargeCollection(7, ChinaClient);
				IReceivablesPostingChargeCollection ausCharges = Helper.GetChargeCollection(5, AusClient);
				IReceivablesPostingChargeCollection chinaCharges2 = Helper.GetChargeCollection(8, ChinaClient);

				PostingChargeCollection totalCharges = new PostingChargeCollection();
				PostingChargeKey key1 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				PostingChargeKey key2 = new PostingChargeKey(AusClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				PostingChargeKey key3 = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				totalCharges.SetCharges(key1, chinaCharges);
				totalCharges.SetCharges(key2, ausCharges);
				totalCharges.SetCharges(key3, chinaCharges2);

				AssertEquals(3, totalCharges.Count);

				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
				totalCharges = GetSplitCharges(totalCharges);

				AssertEquals("7 China charges split into 3 collections, then the other 8 split into another 3, plus the original Aus charges collection NOT split", 7, totalCharges.Count);

				PostingChargeKey retrievedChinaCharges1Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				PostingChargeKey retrievedChinaCharges2Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				retrievedChinaCharges2Key.SplitInvoiceCount = 1;
				PostingChargeKey retrievedChinaCharges3Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				retrievedChinaCharges3Key.SplitInvoiceCount = 2;

				PostingChargeKey retrievedChinaCharges1KeyABC = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				PostingChargeKey retrievedChinaCharges2KeyABC = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				retrievedChinaCharges2KeyABC.SplitInvoiceCount = 1;
				PostingChargeKey retrievedChinaCharges3KeyABC = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
				retrievedChinaCharges3KeyABC.SplitInvoiceCount = 2;

				PostingChargeKey retrievedAusChargesKey = new PostingChargeKey(AusClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);

				AssertEquals("3 charges in 1st china collection", 3, totalCharges.GetCharges(retrievedChinaCharges1Key).Count);
				AssertEquals("3 charges in 2nd china collection", 3, totalCharges.GetCharges(retrievedChinaCharges2Key).Count);
				AssertEquals("1 charge in 3rd china collection", 1, totalCharges.GetCharges(retrievedChinaCharges3Key).Count);

				AssertEquals("3 charges in 1st china collection", 3, totalCharges.GetCharges(retrievedChinaCharges1KeyABC).Count);
				AssertEquals("3 charges in 2nd china collection", 3, totalCharges.GetCharges(retrievedChinaCharges2KeyABC).Count);
				AssertEquals("2 charge in 3rd china collection", 2, totalCharges.GetCharges(retrievedChinaCharges3KeyABC).Count);

				AssertEquals("5 charges in only Aus collection", 5, totalCharges.GetCharges(retrievedAusChargesKey).Count);
			}
		}

		public void TestSplitChargesCount_BranchLevelPostingEnabled()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
				AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
				var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);

				IReceivablesPostingChargeCollection chinaCharges_branch1 = Helper.GetChargeCollection(5, ChinaClient, branch1);
				IReceivablesPostingChargeCollection chinaCharges_branch2 = Helper.GetChargeCollection(2, ChinaClient, branch2);
				IReceivablesPostingChargeCollection ausCharges = Helper.GetChargeCollection(5, AusClient);
				IReceivablesPostingChargeCollection chinaCharges_ABC_branch1 = Helper.GetChargeCollection(5, ChinaClient, branch1);
				IReceivablesPostingChargeCollection chinaCharges_ABC_branch2 = Helper.GetChargeCollection(2, ChinaClient, branch2);

				PostingChargeCollection totalCharges = new PostingChargeCollection();

				PostingChargeKey chinaClient_KeyBranch1 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, branch1.PK);

				PostingChargeKey chinaClient_KeyBranch2 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, branch2.PK);

				PostingChargeKey auClient_Key2 = new PostingChargeKey(AusClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);

				PostingChargeKey chinaClient_Key_ABC_Branch_1 = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0, branch1.PK);

				PostingChargeKey chinaClient_Key_ABC_Branch_2 = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0, branch2.PK);

				totalCharges.SetCharges(chinaClient_KeyBranch1, chinaCharges_branch1);
				totalCharges.SetCharges(chinaClient_KeyBranch2, chinaCharges_branch2);
				totalCharges.SetCharges(auClient_Key2, ausCharges);
				totalCharges.SetCharges(chinaClient_Key_ABC_Branch_1, chinaCharges_ABC_branch1);
				totalCharges.SetCharges(chinaClient_Key_ABC_Branch_2, chinaCharges_ABC_branch2);

				AssertEquals(5, totalCharges.Count);

				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
				totalCharges = GetSplitCharges(totalCharges);

				AssertEquals("Both branch1 charges will be split in to 2, increasing the total number of charges from 5 to 7", 7, totalCharges.Count);

				PostingChargeKey retrievedChinaChargesKey_Branch1 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, branch1.PK);
				PostingChargeKey retrievedChinaChargesKey_Branch1_1 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, branch1.PK);
				retrievedChinaChargesKey_Branch1_1.SplitInvoiceCount = 1;

				PostingChargeKey retrievedChinaChargesKey_Branch2 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, branch2.PK);

				PostingChargeKey retrievedChinaChargesKey_ABC_Branch1 = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0, branch1.PK);
				PostingChargeKey retrievedChinaChargesKey_ABC_Branch1_1 = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0, branch1.PK);
				retrievedChinaChargesKey_ABC_Branch1_1.SplitInvoiceCount = 1;

				PostingChargeKey retrievedChinaChargesKey_ABC_Branch2 = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0, branch2.PK);

				PostingChargeKey retrievedAusChargesKey = new PostingChargeKey(AusClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);

				AssertEquals("3 charges in 1st china collection for branch1", 3, totalCharges.GetCharges(retrievedChinaChargesKey_Branch1).Count);
				AssertEquals("2 charges in 2nd china collection for branch1", 2, totalCharges.GetCharges(retrievedChinaChargesKey_Branch1_1).Count);

				AssertEquals("2 charges in 1st china collection for branch2", 2, totalCharges.GetCharges(retrievedChinaChargesKey_Branch2).Count);

				AssertEquals("3 charges in 1st china collection for branch1", 3, totalCharges.GetCharges(retrievedChinaChargesKey_ABC_Branch1).Count);
				AssertEquals("2 charges in 2nd china collection for branch1", 2, totalCharges.GetCharges(retrievedChinaChargesKey_ABC_Branch1_1).Count);

				AssertEquals("2 charges in 1st china collection for branch2", 2, totalCharges.GetCharges(retrievedChinaChargesKey_ABC_Branch2).Count);

				AssertEquals("5 charges in only Aus collection", 5, totalCharges.GetCharges(retrievedAusChargesKey).Count);
			}
		}

		public void TestSplitChargesCount_PlaceOfSupplyLevelPostingEnabled()
		{
			var chargesPlace1 = Helper.GetChargeCollection(4, ChinaClient, placeOfSupply: "NSW");
			var chargesPlace2 = Helper.GetChargeCollection(5, ChinaClient, placeOfSupply: "WA");

			var chargesPlace1Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");
			var chargesPlace2Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "WA");
			AssertEquals("Keys are equal", 0, chargesPlace1Key.CompareTo(chargesPlace2Key));

			var allCharges = new PostingChargeCollection();

			allCharges.SetCharges(chargesPlace1Key, chargesPlace1);
			allCharges.SetCharges(chargesPlace2Key, chargesPlace2);

			AssertEquals(1, allCharges.Count);
			AssertEquals("5 charges in total overriden by 2nd SetCharges", 5, allCharges.GetCharges(chargesPlace1Key).Count);
			AssertEquals("5 charges in total overriden by 2nd SetCharges", 5, allCharges.GetCharges(chargesPlace2Key).Count);

			using (AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				allCharges = GetSplitCharges(allCharges);
			}

			AssertEquals("Charges will be split into 2", 2, allCharges.Count);

			AssertChargesSlitForKey(allCharges, chargesPlace1Key, 3, 2);
			AssertChargesSlitForKey(allCharges, chargesPlace2Key, 3, 2);

			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				chargesPlace1 = Helper.GetChargeCollection(4, ChinaClient, placeOfSupply: "NSW");
				chargesPlace2 = Helper.GetChargeCollection(5, ChinaClient, placeOfSupply: "WA");

				chargesPlace1Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");
				chargesPlace2Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "WA");
				AssertNotEquals("Keys are not equal", 0, chargesPlace1Key.CompareTo(chargesPlace2Key));

				allCharges = new PostingChargeCollection();

				allCharges.SetCharges(chargesPlace1Key, chargesPlace1);
				allCharges.SetCharges(chargesPlace2Key, chargesPlace2);

				AssertEquals(2, allCharges.Count);

				using (AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
				{
					allCharges = GetSplitCharges(allCharges);
				}

				AssertEquals("Both charge collections will be split making total count 4", 4, allCharges.Count);

				AssertChargesSlitForKey(allCharges, chargesPlace1Key, 3, 1);
				AssertChargesSlitForKey(allCharges, chargesPlace2Key, 3, 2);
			}
		}

		void AssertChargesSlitForKey(PostingChargeCollection charges, PostingChargeKey key, params int[] splitCounts)
		{
			var currentKey = new PostingChargeKey(key.Org, key.InvoiceType, key.JobNumber, key.OrgAddress, key.OrgContact, key.TaxRatePostingGroupId, key.Branch, key.PlaceOfSupply, key.TaxBranch);
			AssertEquals("Copied key must be identical", 0, key.CompareTo(currentKey));

			for (var i = 0; i < splitCounts.Length; i++)
			{
				currentKey.SplitInvoiceCount = i;
				AssertEquals($"Expect {splitCounts[i]} charges in the split #{i}", splitCounts[i], charges.GetCharges(currentKey).Count);
			}
		}

		public void TestSplitChargesCount_TaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var chargesPlace1 = Helper.GetChargeCollection(3, ChinaClient, taxBranch: TestObjectCreator.NonCurrentBranch);
				var chargesPlace2 = Helper.GetChargeCollection(3, ChinaClient, taxBranch: GlbBranch.CurrentBranch);

				var chargesPlace1Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, taxBranch: TestObjectCreator.NonCurrentBranch.PK);
				var chargesPlace2Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0, taxBranch: GlbBranch.CurrentBranch.PK);

				AssertEquals("Keys are not equal", -1, chargesPlace1Key.CompareTo(chargesPlace2Key));

				var allCharges = new PostingChargeCollection();
				allCharges.SetCharges(chargesPlace1Key, chargesPlace1);
				allCharges.SetCharges(chargesPlace2Key, chargesPlace2);

				using (AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
				{
					allCharges = GetSplitCharges(allCharges);
					AssertEquals("Both charge collections will be split making total count 4", 4, allCharges.Count);
					AssertChargesSlitForKey(allCharges, chargesPlace1Key, 2, 1);
					AssertChargesSlitForKey(allCharges, chargesPlace2Key, 2, 1);
				}
			}
		}

		public void TestSplitChargesCount_WithAddressContact()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				var chinaCharges = Helper.GetChargeCollection(7, ChinaClient);
				var chinaCharges2 = Helper.GetChargeCollection(8, Helper.ChinaClient2);

				var expectedAddress1 = ChinaClient.Addresses[0].PK;
				var expectedAddress2 = Helper.ChinaClient2.Addresses[0].PK;

				var totalCharges = new PostingChargeCollection();
				var key1 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress1, ZGuid.Empty, 0);
				var key2 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress2, ZGuid.Empty, 0);
				totalCharges.SetCharges(key1, chinaCharges);
				totalCharges.SetCharges(key2, chinaCharges2);

				AssertEquals(2, totalCharges.Count);

				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

				totalCharges = GetSplitCharges(totalCharges);

				AssertEquals("7 China charges split into 3 collections, then the other 8 split into another 3", 6, totalCharges.Count);

				var retrievedChinaChargesKey1 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress1, ZGuid.Empty, 0);
				var retrievedChinaChargesKey2 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress1, ZGuid.Empty, 0);
				retrievedChinaChargesKey2.SplitInvoiceCount = 1;
				var retrievedChinaChargesKey3 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress1, ZGuid.Empty, 0);
				retrievedChinaChargesKey3.SplitInvoiceCount = 2;

				var retrievedChinaCharges2Key1 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress2, ZGuid.Empty, 0);
				var retrievedChinaCharges2Key2 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress2, ZGuid.Empty, 0);
				retrievedChinaCharges2Key2.SplitInvoiceCount = 1;
				var retrievedChinaCharges2Key3 = new PostingChargeKey(ChinaClient.PK, "", expectedAddress2, ZGuid.Empty, 0);
				retrievedChinaCharges2Key3.SplitInvoiceCount = 2;

				Action<string, int, IReceivablesPostingChargeCollection, ZGuid, ZGuid> assertAddressContact = (message, count, charges, addressPK, contactPK) =>
				{
					AssertEquals(message, count, charges.Count);
					var invalidCharge = charges.FirstOrDefault(charge => charge.DebtorAddressPK != addressPK || charge.DebtorContactPK != contactPK);
					AssertNull("All charges should have correct address and contact", invalidCharge);
				};

				assertAddressContact("3 charges in 1st china collection", 3, totalCharges.GetCharges(retrievedChinaChargesKey1), expectedAddress1, ZGuid.Empty);
				assertAddressContact("3 charges in 2nd china collection", 3, totalCharges.GetCharges(retrievedChinaChargesKey2), expectedAddress1, ZGuid.Empty);
				assertAddressContact("1 charge in 3rd china collection", 1, totalCharges.GetCharges(retrievedChinaChargesKey3), expectedAddress1, ZGuid.Empty);

				assertAddressContact("3 charges in 1st china collection", 3, totalCharges.GetCharges(retrievedChinaCharges2Key1), expectedAddress2, ZGuid.Empty);
				assertAddressContact("3 charges in 2nd china collection", 3, totalCharges.GetCharges(retrievedChinaCharges2Key2), expectedAddress2, ZGuid.Empty);
				assertAddressContact("2 charge in 3rd china collection", 2, totalCharges.GetCharges(retrievedChinaCharges2Key3), expectedAddress2, ZGuid.Empty);
			}
		}

		public void TestSplitChargesWithPostingGroupsUnchanged()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				IReceivablesPostingChargeCollection chinaCharges = Helper.GetChargeCollection(7, ChinaClient);
				IReceivablesPostingChargeCollection ausCharges = Helper.GetChargeCollection(5, AusClient);
				IReceivablesPostingChargeCollection chinaCharges2 = Helper.GetChargeCollection(8, ChinaClient);

				PostingChargeCollection totalCharges = new PostingChargeCollection();
				PostingChargeKey key1 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				PostingChargeKey key2 = new PostingChargeKey(AusClient.PK, "", ZGuid.Empty, ZGuid.Empty, 1);
				PostingChargeKey key3 = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 2);
				totalCharges.SetCharges(key1, chinaCharges);
				totalCharges.SetCharges(key2, ausCharges);
				totalCharges.SetCharges(key3, chinaCharges2);

				AssertEquals(3, totalCharges.Count);

				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

				totalCharges = GetSplitCharges(totalCharges);

				AssertEquals("7 China charges split into 3 collections, then the other 8 split into another 3, plus the original Aus charges collection NOT split", 7, totalCharges.Count);

				PostingChargeKey retrievedChinaCharges1Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				PostingChargeKey retrievedChinaCharges2Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				retrievedChinaCharges2Key.SplitInvoiceCount = 1;
				PostingChargeKey retrievedChinaCharges3Key = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				retrievedChinaCharges3Key.SplitInvoiceCount = 2;

				PostingChargeKey retrievedChinaCharges1KeyABC = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 2);
				PostingChargeKey retrievedChinaCharges2KeyABC = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 2);
				retrievedChinaCharges2KeyABC.SplitInvoiceCount = 1;
				PostingChargeKey retrievedChinaCharges3KeyABC = new PostingChargeKey(ChinaClient.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 2);
				retrievedChinaCharges3KeyABC.SplitInvoiceCount = 2;

				PostingChargeKey retrievedAusChargesKey = new PostingChargeKey(AusClient.PK, "", ZGuid.Empty, ZGuid.Empty, 1);

				AssertEquals("3 charges in 1st china collection", 3, totalCharges.GetCharges(retrievedChinaCharges1Key).Count);
				AssertEquals("3 charges in 2nd china collection", 3, totalCharges.GetCharges(retrievedChinaCharges2Key).Count);
				AssertEquals("1 charge in 3rd china collection", 1, totalCharges.GetCharges(retrievedChinaCharges3Key).Count);

				AssertEquals("3 charges in 1st china collection", 3, totalCharges.GetCharges(retrievedChinaCharges1KeyABC).Count);
				AssertEquals("3 charges in 2nd china collection", 3, totalCharges.GetCharges(retrievedChinaCharges2KeyABC).Count);
				AssertEquals("2 charge in 3rd china collection", 2, totalCharges.GetCharges(retrievedChinaCharges3KeyABC).Count);

				AssertEquals("5 charges in only Aus collection", 5, totalCharges.GetCharges(retrievedAusChargesKey).Count);
			}
		}

		public void TestSplitInvoiceCountNoChange()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				IReceivablesPostingChargeCollection chinaCharges = Helper.GetChargeCollection(3, ChinaClient);
				IReceivablesPostingChargeCollection ausCharges = Helper.GetChargeCollection(5, AusClient);

				PostingChargeCollection totalCharges = new PostingChargeCollection();
				PostingChargeKey key1 = new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				PostingChargeKey key2 = new PostingChargeKey(AusClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
				totalCharges.SetCharges(key1, chinaCharges);
				totalCharges.SetCharges(key2, ausCharges);

				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

				AssertEquals(2, totalCharges.Count);
				totalCharges = GetSplitCharges(totalCharges);
				AssertEquals("No change - as the china collection had the same amount as the maximim", 2, totalCharges.Count);
			}
		}

		public void TestExceptionIsThrownWhenASingleChargeAmountExceedsMaxValue()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Helper.ChinaBranch.PK.ToGuid(), Helper.Department.PK.ToGuid()))
			{
				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 25);
				AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL");

				IReceivablesPostingChargeCollection charges = Helper.GetChargeCollection(1, ChinaClient);
				PostingChargeCollection totalCharges = new PostingChargeCollection();
				totalCharges.SetCharges(new PostingChargeKey(ChinaClient.PK, "", ZGuid.Empty, ZGuid.Empty, 0), charges);

				AssertExceptionThrown(typeof(CriticalPostingErrorException),
									  "The Charge has exceeded the maximum value (25) as defined in the following registry Accounting -> Job Invoicing -> Invoice Splitting Rules -> Maximum value. Please split the charge to multiple lines with value less than the maximum value.",
									  () => GetSplitCharges(totalCharges));
			}
		}

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;

		#region Implementation

		ChargeSplitterByChargeCountTestHelper Helper;

		OrgHeader ChinaClient;
		OrgHeader AusClient;

		protected override void SetUp()
		{
			base.SetUp();

			Helper = new ChargeSplitterByChargeCountTestHelper(Factory);

			ChinaClient = Helper.ChinaClient;
			AusClient = Helper.AusClient;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
		}

		#endregion
	}
}
