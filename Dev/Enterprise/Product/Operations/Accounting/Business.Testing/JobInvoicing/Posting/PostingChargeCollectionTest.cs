using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class PostingChargeCollectionTest : TestCaseWithFactory
	{
		public void TestGetSetContainsKeyBehaviours()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			RefCurrency currency1 = Factory.New<RefCurrency>();
			RefCurrency currency2 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));

			PostingChargeKey key1 = new PostingChargeKey(org1.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
			PostingChargeKey key2 = new PostingChargeKey(org2.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
			PostingChargeKey key3 = new PostingChargeKey(org2.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);

			IReceivablesPostingChargeCollection charges1 = new IReceivablesPostingChargeCollection();
			IReceivablesPostingChargeCollection charges2 = new IReceivablesPostingChargeCollection();
			IReceivablesPostingChargeCollection charges3 = new IReceivablesPostingChargeCollection();

			PostingChargeCollection coll = new PostingChargeCollection();
			AssertEquals("Precondition: No collections", 0, coll.Count);
			AssertEquals("Precondition: Keys do not exist", false, coll.ContainsKey(key1));
			AssertEquals("Precondition: Keys do not exist", false, coll.ContainsKey(key2));
			AssertNull("Correct item returned", coll.GetCharges(key1));
			AssertNull("Correct item returned", coll.GetCharges(key2));

			coll.SetCharges(key1, charges1);
			AssertEquals("1 collection", 1, coll.Count);
			AssertEquals("Key 1 exists", true, coll.ContainsKey(key1));
			AssertEquals("Key 2 does not exist", false, coll.ContainsKey(key2));
			AssertEquals("Correct item returned", charges1, coll.GetCharges(key1));
			AssertNull("Correct item returned", coll.GetCharges(key2));

			coll.SetCharges(key1, charges2);
			AssertEquals("Still 1 collection", 1, coll.Count);
			AssertEquals("Key 1 exists", true, coll.ContainsKey(key1));
			AssertEquals("Key 2 does not exist", false, coll.ContainsKey(key2));
			AssertEquals("Correct item returned", charges2, coll.GetCharges(key1));
			AssertNull("Correct item returned", coll.GetCharges(key2));

			coll.SetCharges(key2, charges3);
			AssertEquals("2 collections", 2, coll.Count);
			AssertEquals("Key 1 exists", true, coll.ContainsKey(key1));
			AssertEquals("Key 2 exists", true, coll.ContainsKey(key2));
			AssertEquals("Correct item returned", charges2, coll.GetCharges(key1));
			AssertEquals("Correct item returned", charges3, coll.GetCharges(key2));
			AssertEquals("Correct item returned - same key values", charges3, coll.GetCharges(key3));

			coll.SetCharges(key3, charges1);
			AssertEquals("Still 2 collections as Key 3 and Key 2 are identical in value", 2, coll.Count);
		}

		public void TestCompareKey()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			RefCurrency currency1 = Factory.New<RefCurrency>();
			RefCurrency currency2 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			RefCurrency currency2DiffFactory = new BusinessObjectFactory().LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));

			PostingChargeKey key1 = new PostingChargeKey(org1.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
			PostingChargeKey key2 = new PostingChargeKey(org1.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);

			AssertEquals("Keys are equal", 0, key1.CompareTo(key2));
			AssertEquals("Keys are equal", 0, key2.CompareTo(key1));

			PostingChargeKey key3 = new PostingChargeKey(org2.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
			key3.SellCurrency = currency2.RX_Code;

			AssertEquals("Keys are NOT equal", -1, key1.CompareTo(key3));
			AssertEquals("Keys are NOT equal", -1, key3.CompareTo(key1));
			AssertEquals("Keys are NOT equal", -1, key2.CompareTo(key3));
			AssertEquals("Keys are NOT equal", -1, key3.CompareTo(key2));

			PostingChargeKey key4 = new PostingChargeKey(org2.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
			key4.SellCurrency = currency2DiffFactory.RX_Code;

			AssertEquals("Keys are equal", 0, key3.CompareTo(key4));
			AssertEquals("Keys are equal", 0, key4.CompareTo(key3));

			key4.SellReference = "ABC123";
			AssertEquals("Keys are not equal", -1, key3.CompareTo(key4));
			AssertEquals("Keys are equal", -1, key4.CompareTo(key3));

			key3.SellReference = "ABC123";
			AssertEquals("Keys are equal", 0, key3.CompareTo(key4));
			AssertEquals("Keys are equal", 0, key4.CompareTo(key3));

			key3.SellReference = "XYZ987";
			AssertEquals("Keys are not equal", -1, key3.CompareTo(key4));
			AssertEquals("Keys are equal", -1, key4.CompareTo(key3));
		}

		public void TestCompareKeyWithAddressContact()
		{
			var org1 = TestObjectCreator.LocalClient;
			var address1 = TestObjectCreator.CreateAddress(org1);
			var contact1 = TestObjectCreator.CreateContact(org1);
			var address2 = TestObjectCreator.CreateAddress(org1, "123 Street");
			var contact2 = TestObjectCreator.CreateContact(org1, "Ben");
			var org2 = TestObjectCreator.LocalClient2;
			var currency1 = TestObjectCreator.USD;
			var currency2 = TestObjectCreator.AUD;

			var key1 = new PostingChargeKey(org1.PK, "", address1.PK, contact1.PK, 0);
			var key2 = new PostingChargeKey(org1.PK, "", address2.PK, contact2.PK, 0);

			AssertEquals(0, key1.CompareTo(key1));
			AssertEquals(0, key2.CompareTo(key2));
			AssertEquals(-1, key1.CompareTo(key2));
			AssertEquals(-1, key2.CompareTo(key1));

			var key3 = new PostingChargeKey(org1.PK, "", ZGuid.Empty, contact1.PK, 0);
			var key4 = new PostingChargeKey(org1.PK, "", address1.PK, ZGuid.Empty, 0);

			AssertEquals(0, key3.CompareTo(key3));
			AssertEquals(0, key4.CompareTo(key4));
			AssertEquals(-1, key1.CompareTo(key3));
			AssertEquals(-1, key3.CompareTo(key1));
			AssertEquals(-1, key1.CompareTo(key4));
			AssertEquals(-1, key4.CompareTo(key1));
			AssertEquals(-1, key3.CompareTo(key4));
			AssertEquals(-1, key4.CompareTo(key3));

			var key5 = new PostingChargeKey(org1.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
			AssertEquals(0, key5.CompareTo(key5));
			AssertEquals(-1, key1.CompareTo(key5));
			AssertEquals(-1, key2.CompareTo(key5));
			AssertEquals(-1, key3.CompareTo(key5));
			AssertEquals(-1, key4.CompareTo(key5));
			AssertEquals(-1, key5.CompareTo(key1));
			AssertEquals(-1, key5.CompareTo(key2));
			AssertEquals(-1, key5.CompareTo(key3));
			AssertEquals(-1, key5.CompareTo(key4));

			var key6 = new PostingChargeKey(org1.PK, "", address2.PK, contact2.PK, 0);
			AssertEquals(0, key2.CompareTo(key6));

			var key7 = new PostingChargeKey(org2.PK, "", address2.PK, contact2.PK, 0);
			AssertEquals(0, key7.CompareTo(key7));
			AssertEquals(-1, key6.CompareTo(key7));
			AssertEquals(-1, key7.CompareTo(key6));
		}

		public void TestKeys()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);

			PostingChargeCollection postingChargeCollection = new PostingChargeCollection();
			PostingChargeKey key1 = new PostingChargeKey(org1.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);
			PostingChargeKey key2 = new PostingChargeKey(org2.PK, "ABC", ZGuid.Empty, ZGuid.Empty, 0);

			IReceivablesPostingChargeCollection charges1 = new IReceivablesPostingChargeCollection();
			IReceivablesPostingChargeCollection charges2 = new IReceivablesPostingChargeCollection();

			AssertEquals("Keys.Count", 0, postingChargeCollection.Keys.Count);
			AssertCollectionNotContains(key1, postingChargeCollection.Keys);
			AssertCollectionNotContains(key2, postingChargeCollection.Keys);

			postingChargeCollection.SetCharges(key1, charges1);
			AssertCollectionContains(key1, postingChargeCollection.Keys);
			AssertCollectionNotContains(key2, postingChargeCollection.Keys);

			postingChargeCollection.SetCharges(key2, charges2);
			AssertCollectionContains(key1, postingChargeCollection.Keys);
			AssertCollectionContains(key2, postingChargeCollection.Keys);
		}

		public void TestConstructorWithAddressContact()
		{
			var expectedOrg = TestObjectCreator.LocalClient;
			var expectedOrgAddress = TestObjectCreator.CreateAddress(expectedOrg);
			var expectedOrgContact = TestObjectCreator.CreateContact(expectedOrg);
			var key = new PostingChargeKey(expectedOrg.PK, "", expectedOrgAddress.PK, expectedOrgContact.PK, 0);

			AssertEquals("key.Org", expectedOrg.PK, key.Org);
			AssertEquals("key.OrgAddress", expectedOrgAddress.PK, key.OrgAddress);
			AssertEquals("key.OrgContact", expectedOrgContact.PK, key.OrgContact);
		}

		public void TestConstructorWithPostingGroup()
		{
			var key = new PostingChargeKey(ZGuid.Empty, "", ZGuid.Empty, ZGuid.Empty, 99);
			AssertEquals(key.TaxRatePostingGroupId, new ZShort(99));

			key = new PostingChargeKey(ZGuid.Empty, "", "", ZGuid.Empty, ZGuid.Empty, 88);
			AssertEquals(key.TaxRatePostingGroupId, new ZShort(88));
		}

		public void TestConstructorWithExistingPostingChargeKey()
		{
			var orgPK = ZGuid.NewZGuid();
			var invoiceType = "ABC";
			var orgAddressPK = ZGuid.NewZGuid();
			var orgContactPK = ZGuid.NewZGuid();
			ZShort taxRatePostingGroupID = 99;
			var branchPK = ZGuid.NewZGuid();
			var placeOfSupply = "XY";
			var taxRatePK = ZGuid.NewZGuid();
			var sellCurrency = "XXX";
			var splitInvoiceCount = 10;
			var sellReference = "YYY";
			var taxSystemSplitKey = 11;
			var taxBranch = ZGuid.NewZGuid();

			var key = new PostingChargeKey(orgPK, invoiceType, orgAddressPK, orgContactPK, taxRatePostingGroupID, branchPK, placeOfSupply, taxBranch);
			key.TaxRate = taxRatePK;
			key.SellCurrency = sellCurrency;
			key.SplitInvoiceCount = splitInvoiceCount;
			key.SellReference = sellReference;
			key.IsCommentChargeKey = false;
			key.TaxSystemSplitKey = taxSystemSplitKey;

			var newKey = new PostingChargeKey(key);

			AssertEquals("The keys will be same", 0, key.CompareTo(newKey));
		}

		public void TestCompareKeysWithTaxBranch()
		{
			AssertCompareKeysWithTaxBranch(true);
			AssertCompareKeysWithTaxBranch(false);

			void AssertCompareKeysWithTaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var key1 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, taxBranch: TestObjectCreator.NonCurrentBranch.PK);
					var key2 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, taxBranch: GlbBranch.CurrentBranch.PK);
					var key3 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, taxBranch: TestObjectCreator.NonCurrentBranch.PK);

					AssertEquals(!enableTaxBranchReporting, key1.TaxBranch.IsEmpty);
					AssertEquals(!enableTaxBranchReporting, key2.TaxBranch.IsEmpty);
					AssertEquals(!enableTaxBranchReporting, key3.TaxBranch.IsEmpty);

					AssertEquals(enableTaxBranchReporting ? -1 : 0, key1.CompareTo(key2));
					AssertEquals(enableTaxBranchReporting ? -1 : 0, key2.CompareTo(key3));
					AssertEquals(0, key1.CompareTo(key3));
				}
			}
		}

		public void TestCompareToWithPostingGroup()
		{
			var key1 = new PostingChargeKey(ZGuid.Empty, "", ZGuid.Empty, ZGuid.Empty, 99);
			var key2 = new PostingChargeKey(ZGuid.Empty, "", "", ZGuid.Empty, ZGuid.Empty, 88);
			var key3 = new PostingChargeKey(ZGuid.Empty, "", "", ZGuid.Empty, ZGuid.Empty, 99);

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
				AssertEquals(0, key1.CompareTo(key2));
				AssertEquals(0, key1.CompareTo(key3));
				AssertEquals(0, key2.CompareTo(key3));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
				AssertEquals(0, key1.CompareTo(key2));
				AssertEquals(0, key1.CompareTo(key3));
				AssertEquals(0, key2.CompareTo(key3));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
				AssertEquals(-1, key1.CompareTo(key2));
				AssertEquals(0, key1.CompareTo(key3));
				AssertEquals(-1, key2.CompareTo(key3));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
				AssertEquals(0, key1.CompareTo(key2));
				AssertEquals(0, key1.CompareTo(key3));
				AssertEquals(0, key2.CompareTo(key3));
			}
		}

		public void TestCompareKeysWithBranch()
		{
			var key1 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, GlbBranch.CurrentBranch.PK);
			var key2 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, TestObjectCreator.NonCurrentBranch.PK);
			var key3 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, GlbBranch.CurrentBranch.PK);

			AssertEquals(0, key1.CompareTo(key2));
			AssertEquals(0, key2.CompareTo(key3));
			AssertEquals(0, key1.CompareTo(key3));

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			key1 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, GlbBranch.CurrentBranch.PK);
			key2 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, TestObjectCreator.NonCurrentBranch.PK);
			key3 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, GlbBranch.CurrentBranch.PK);

			AssertEquals(-1, key1.CompareTo(key2));
			AssertEquals(-1, key2.CompareTo(key3));
			AssertEquals(0, key1.CompareTo(key3));
		}

		public void TestCompareKeysWithPlaceOfSupply()
		{
			Assert("Pre-condition: Registry Item is not set", !AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.Value);

			var key1 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");
			var key2 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "WA");
			var key3 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");

			AssertEquals(0, key1.CompareTo(key2));
			AssertEquals(0, key2.CompareTo(key3));
			AssertEquals(0, key1.CompareTo(key3));

			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				key1 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");
				key2 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "WA");
				key3 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");

				AssertEquals(-1, key1.CompareTo(key2));
				AssertEquals(-1, key2.CompareTo(key3));
				AssertEquals(0, key1.CompareTo(key3));
			}
		}

		public void TestCompareKeysWithTaxSystemSplitKey()
		{
			var key1 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);
			key1.TaxSystemSplitKey = 1;

			var key2 = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);
			key2.TaxSystemSplitKey = 1;

			AssertEquals(0, key1.CompareTo(key2));

			key2.TaxSystemSplitKey = 2;

			AssertEquals(-1, key1.CompareTo(key2));
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
