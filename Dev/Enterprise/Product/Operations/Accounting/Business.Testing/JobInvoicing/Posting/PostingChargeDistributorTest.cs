using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class PostingChargeDistributorTest : TestCaseWithFactory
	{
		public void TestDistributeChargesDifferentTaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var job = Factory.NewJobForTesting<Job>();
				var charges = new IReceivablesPostingChargeCollection();

				var charge1 = CreateNewCharge(TestObjectCreator.Debtor1, TestObjectCreator.LocalCurrency, TestObjectCreator.CC1, AgencyInvoiceTypesList.Codes.ForeignCollect, job);
				charge1.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				charges.Add(charge1);

				var charge2 = CreateNewCharge(TestObjectCreator.Debtor1, TestObjectCreator.LocalCurrency, TestObjectCreator.CC1, AgencyInvoiceTypesList.Codes.ForeignCollect, job);
				charge2.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;
				charges.Add(charge2);

				var charge3 = CreateNewCharge(TestObjectCreator.Debtor1, TestObjectCreator.LocalCurrency, TestObjectCreator.CC1, AgencyInvoiceTypesList.Codes.ForeignCollect, job);
				charge3.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				charges.Add(charge3);

				var charge4 = CreateNewCharge(TestObjectCreator.Debtor1, TestObjectCreator.LocalCurrency, TestObjectCreator.CC1, AgencyInvoiceTypesList.Codes.ForeignCollect, job);
				charge4.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;
				charges.Add(charge4);

				var distributor = GetDistributor();
				var results = distributor.DistributeCharges(charges);
				var keys = results.Keys.Cast<PostingChargeKey>();
				AssertEquals(2, results.Count);
				AssertEquals("1 collections of charges for CurrentBranch", 1, keys.Count(k => k.TaxBranch == GlbBranch.CurrentBranch.PK));
				AssertEquals("1 collections of charges for NonCurrentBranch", 1, keys.Count(k => k.TaxBranch == TestObjectCreator.NonCurrentBranch.PK));

				var key1 = results.Keys.OfType<PostingChargeKey>().First(x => x.TaxBranch == TestObjectCreator.NonCurrentBranch.PK);
				var key2 = results.Keys.OfType<PostingChargeKey>().First(x => x.TaxBranch == GlbBranch.CurrentBranch.PK);
				AssertContainsExactElementsInAnyOrder(new[] { charge1.PK, charge3.PK }, results[key1].OfType<Charge>().Select(x => x.PK));
				AssertContainsExactElementsInAnyOrder(new[] { charge2.PK, charge4.PK }, results[key2].OfType<Charge>().Select(x => x.PK));
			}
		}

		public void TestDependency()
		{
			var distributor = new PostingChargeDistributor();
			AssertType<ChargeSplitterDueToSingleTaxPerTransaction>(distributor.ChargeSplitterDueToSingleTaxPerTransaction_ExposedForTestOnly);
		}

		public void TestChargeSplitterDueToSingleTaxPerTransactionInvoked()
		{
			var chargeSplitterMock = new Mock<IChargeSplitterDueToSingleTaxPerTransaction>();

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			var job = Factory.NewJobForTesting<Job>();
			var charge1 = job.Charges.AddNew();
			var charge2 = job.Charges.AddNew();

			var key = new PostingChargeKey(debtor.PK, charge1.JR_InvoiceType, ZGuid.Empty, ZGuid.Empty, 0);
			key.TaxSystemSplitKey = 1;

			var expectedCharges = new PostingChargeCollection();
			expectedCharges.SetCharges(key, new IReceivablesPostingChargeCollection());
			expectedCharges.GetCharges(key).Add(charge1);
			var newKey = new PostingChargeKey(debtor.PK, charge1.JR_InvoiceType, ZGuid.Empty, ZGuid.Empty, 0);
			newKey.TaxSystemSplitKey = 100;
			expectedCharges.SetCharges(newKey, new IReceivablesPostingChargeCollection());
			expectedCharges.GetCharges(newKey).Add(charge2);

			var charges = new PostingChargeCollection();
			charges.SetCharges(key, new IReceivablesPostingChargeCollection());
			charges.GetCharges(key).Add(charge1);

			PostingChargeCollection passedCharges = null;
			chargeSplitterMock.Setup(x => x.GetSplitCharges(It.IsAny<PostingChargeCollection>()))
				.Callback((PostingChargeCollection c) => passedCharges = c)
				.Returns(expectedCharges);

			var distributor = new PostingChargeDistributor();
			distributor.SubstituteChargeSplitterDueToSingleTaxPerTransaction_ForTestOnly(chargeSplitterMock.Object);

			var result = distributor.DistributeCharges(charges.GetCharges(key));

			AssertEquals("2 charges should be returned", 2, result.Count);

			chargeSplitterMock.Verify(x => x.GetSplitCharges(It.IsAny<PostingChargeCollection>()), Times.Once);
		}

		public void TestJobTakenIntoDistribution()
		{
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			Job job1 = Factory.NewJobForTesting<Job>();

			Job job2 = Factory.NewJobForTesting<Job>();
			Charge charge1 = job2.Charges.AddNew();
			charge1.JR_OH_SellAccount = debtor.PK;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			Charge charge2 = job2.Charges.AddNew();
			charge2.JR_OH_SellAccount = debtor.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			PostingChargeKey key = new PostingChargeKey(debtor.PK, charge1.JR_InvoiceType, ZGuid.Empty, ZGuid.Empty, 0);

			PostingChargeDistributor distributor = new PostingChargeDistributor();
			PostingChargeCollection result = distributor.DistributeCharges(charges);
			AssertEquals(job2, result[key].Job);

			charges.Job = job1;
			result = distributor.DistributeCharges(charges);
			AssertEquals(job1, result[key].Job);
		}

		public void TestDistributeChargesBySellReferenceForSelfBilling()
		{
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_OH_SellAccount = debtor.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
			charge1.JR_SellReference = "ABC123";
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_OH_SellAccount = debtor.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
			charge2.JR_SellReference = "XYZ987";
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			PostingChargeKey key1 = new PostingChargeKey(debtor.PK, charge1.JR_InvoiceType, ZGuid.Empty, ZGuid.Empty, 0);
			key1.SellCurrency = charge1.JR_RX_NKSellCurrency;
			key1.SellReference = "ABC123";

			PostingChargeKey key2 = new PostingChargeKey(debtor.PK, charge1.JR_InvoiceType, ZGuid.Empty, ZGuid.Empty, 0);
			key2.SellCurrency = charge2.JR_RX_NKSellCurrency;
			key2.SellReference = "XYZ987";

			PostingChargeDistributor distributor = new PostingChargeDistributor();
			PostingChargeCollection result = distributor.DistributeCharges(charges);
			AssertEquals("Should be two collection of charges (i.e. split by sell reference)", 2, result.Count);
			AssertEquals("Should be one charge per collection", 1, result[key1].Count);
			AssertEquals("Should be one charge per collection", 1, result[key2].Count);
		}

		public void TestDistributeChargesDifferentOrgs()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "CR1";

			RefCurrency currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = "CR2";

			Job job = Factory.NewJobForTesting<Job>();

			IReceivablesPostingChargeCollection postingCharges = new IReceivablesPostingChargeCollection();
			var charge1 = AddNewCharge(currency1, InvoiceTypesList.Codes.FinalInvoice);
			var charge2 = AddNewCharge(currency1, InvoiceTypesList.Codes.DisbursementInvoice);
			var charge3 = AddNewCharge(currency1, InvoiceTypesList.Codes.FreightInvoice);
			var charge4 = AddNewCharge(currency2, InvoiceTypesList.Codes.FreightInvoice);
			var charge5 = AddNewCharge(currency1, InvoiceTypesList.Codes.FinalInvoice);
			var charge6 = AddNewCharge(currency1, InvoiceTypesList.Codes.DisbursementInForeignCurrency);
			var charge7 = AddNewCharge(currency2, InvoiceTypesList.Codes.DisbursementInForeignCurrency);

			PostingChargeDistributor distributor = GetDistributor();
			PostingChargeCollection results = distributor.DistributeCharges(postingCharges);

			AssertEquals("4 unique collections of charges", 6, results.Count);

			PostingChargeKey key1 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice), ZGuid.Empty, ZGuid.Empty, 0);
			PostingChargeKey key2 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FreightInvoice), ZGuid.Empty, ZGuid.Empty, 0);
			key2.SellCurrency = currency1.RX_Code;
			PostingChargeKey key3 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.DisbursementInvoice), ZGuid.Empty, ZGuid.Empty, 0);
			PostingChargeKey key4 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FreightInvoice), ZGuid.Empty, ZGuid.Empty, 0);
			key4.SellCurrency = currency2.RX_Code;
			PostingChargeKey key5 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.DisbursementInForeignCurrency), ZGuid.Empty, ZGuid.Empty, 0);
			key5.SellCurrency = currency1.RX_Code;
			PostingChargeKey key6 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.DisbursementInForeignCurrency), ZGuid.Empty, ZGuid.Empty, 0);
			key6.SellCurrency = currency2.RX_Code;

			Assert(results.ContainsKey(key1));
			IReceivablesPostingChargeCollection key1Charges = results.GetCharges(key1);
			AssertEquals(2, key1Charges.Count);
			AssertCollectionContains(charge1, key1Charges);
			AssertCollectionContains(charge5, key1Charges);

			Assert(results.ContainsKey(key2));
			IReceivablesPostingChargeCollection key2Charges = results.GetCharges(key2);
			AssertEquals(1, key2Charges.Count);
			AssertCollectionContains(charge3, key2Charges);

			Assert(results.ContainsKey(key3));
			IReceivablesPostingChargeCollection key3Charges = results.GetCharges(key3);
			AssertEquals(1, key3Charges.Count);
			AssertCollectionContains(charge2, key3Charges);

			Assert(results.ContainsKey(key4));
			IReceivablesPostingChargeCollection key4Charges = results.GetCharges(key4);
			AssertEquals(1, key4Charges.Count);
			AssertCollectionContains(charge4, key4Charges);

			Assert(results.ContainsKey(key5));
			IReceivablesPostingChargeCollection key5Charges = results.GetCharges(key5);
			AssertEquals(1, key5Charges.Count);
			AssertCollectionContains(charge6, key5Charges);

			Assert(results.ContainsKey(key6));
			IReceivablesPostingChargeCollection key6Charges = results.GetCharges(key6);
			AssertEquals(1, key6Charges.Count);
			AssertCollectionContains(charge7, key6Charges);

			Charge AddNewCharge(RefCurrency currency, string invoiceType)
			{
				var charge = job.Charges.AddNew();
				charge.JR_OH_SellAccount = org1.PK;
				charge.JR_RX_NKSellCurrency = currency.RX_Code;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_InvoiceType = GetCorrectedInvoiceType(invoiceType);
				postingCharges.Add(charge);

				return charge;
			}
		}

		public void TestDistributeChargesDifferentAddressContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var job = Factory.NewJobForTesting<Job>();

			var postingCharges = new IReceivablesPostingChargeCollection();
			var chargeAddressContacts = new Dictionary<Charge, Tuple<ZGuid, ZGuid>>();
			Action<Charge> addChargeAddressContacts = charge => chargeAddressContacts.Add(charge, new Tuple<ZGuid, ZGuid>(charge.JR_OA_SellInvoiceAddress, charge.JR_OC_SellInvoiceContact));
			Action<Charge> assertChargeAddressContacts = charge =>
			{
				AssertEquals("Address", chargeAddressContacts[charge].Item1, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("Contact", chargeAddressContacts[charge].Item2, charge.JR_OC_SellInvoiceContact);
			};
			var charge1 = AddNewCharge();
			var charge2 = AddNewCharge(address1);
			var charge3 = AddNewCharge(contact: contact1);
			var charge4 = AddNewCharge(address1, contact1);
			var charge5 = AddNewCharge(address1, contact1);
			var charge6 = AddNewCharge(address2, contact2);
			var charge7 = AddNewCharge(address1, contact2);

			var distributor = GetDistributor();
			var results = distributor.DistributeCharges(postingCharges);

			AssertEquals("Number of unique collections of charges", 6, results.Count);

			var key1 = new PostingChargeKey(org.PK, "", ZGuid.Empty, ZGuid.Empty, 0);
			var key2 = new PostingChargeKey(org.PK, "", address1.PK, ZGuid.Empty, 0);
			var key3 = new PostingChargeKey(org.PK, "", ZGuid.Empty, contact1.PK, 0);
			var key4 = new PostingChargeKey(org.PK, "", address1.PK, contact1.PK, 0);
			var key5 = new PostingChargeKey(org.PK, "", address2.PK, contact2.PK, 0);
			var key6 = new PostingChargeKey(org.PK, "", address1.PK, contact2.PK, 0);

			Assert(results.ContainsKey(key1));
			var key1Charges = results.GetCharges(key1);
			AssertEquals(1, key1Charges.Count);
			AssertCollectionContains(charge1, key1Charges);
			assertChargeAddressContacts(charge1);

			Assert(results.ContainsKey(key2));
			var key2Charges = results.GetCharges(key2);
			AssertEquals(1, key2Charges.Count);
			AssertCollectionContains(charge2, key2Charges);
			assertChargeAddressContacts(charge2);

			Assert(results.ContainsKey(key3));
			var key3Charges = results.GetCharges(key3);
			AssertEquals(1, key3Charges.Count);
			AssertCollectionContains(charge3, key3Charges);
			assertChargeAddressContacts(charge3);

			Assert(results.ContainsKey(key4));
			var key4Charges = results.GetCharges(key4);
			AssertEquals(2, key4Charges.Count);
			AssertCollectionContains(charge4, key4Charges);
			assertChargeAddressContacts(charge4);
			AssertCollectionContains(charge5, key4Charges);
			assertChargeAddressContacts(charge5);

			Assert(results.ContainsKey(key5));
			var key5Charges = results.GetCharges(key5);
			AssertEquals(1, key5Charges.Count);
			AssertCollectionContains(charge6, key5Charges);
			assertChargeAddressContacts(charge6);

			Assert(results.ContainsKey(key6));
			var key6Charges = results.GetCharges(key6);
			AssertEquals(1, key6Charges.Count);
			AssertCollectionContains(charge7, key6Charges);
			assertChargeAddressContacts(charge7);

			Charge AddNewCharge(OrgAddress address = null, OrgContact contact = null)
			{
				var charge = job.Charges.AddNew();
				charge.JR_OH_SellAccount = org.PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				if (address != null)
				{
					charge.JR_OA_SellInvoiceAddress = address.PK;
				}
				if (contact != null)
				{
					charge.JR_OC_SellInvoiceContact = contact.PK;
				}
				postingCharges.Add(charge);
				addChargeAddressContacts(charge);
				return charge;
			}
		}

		public void TestDistributeChargesFinalInvoiceAndDisbursementInvoice()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			OrgHeader org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "CR1";

			RefCurrency currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = "CR2";

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_ChargeType = Constants.ChargeType.Disbursement;
			Job job = Factory.NewJobForTesting<Job>();

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_OH_SellAccount = org1.PK;
			charge1.JR_AC = chargeCode1.PK;
			charge1.JR_RX_NKSellCurrency = currency1.RX_Code;
			charge1.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice);
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charges.Add(charge1);

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_OH_SellAccount = org1.PK;
			charge2.JR_AC = chargeCode2.PK;
			charge2.JR_RX_NKSellCurrency = currency1.RX_Code;
			charge2.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice);
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charges.Add(charge2);

			PostingChargeDistributor distributor = GetDistributor();
			PostingChargeCollection results = distributor.DistributeCharges(charges);
			AssertEquals("1 collections of charges as both are on the final invoice", 1, results.Count);

			charge2.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.DisbursementInvoice);
			results = distributor.DistributeCharges(charges);
			AssertEquals("2 collections of charges as one is final, one is disbursement", 2, results.Count);

			charge1.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.DisbursementInvoice);
			results = distributor.DistributeCharges(charges);
			AssertEquals("1 collection of charges as both are disbursement", 1, results.Count);

			charge1.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.ForeignCurrencyInvoice);
			charge2.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.ForeignCurrencyInvoice);

			results = distributor.DistributeCharges(charges);
			AssertEquals("Both same currency, so on 1 invoice", 1, results.Count);

			charge2.JR_RX_NKSellCurrency = currency2.RX_Code;
			results = distributor.DistributeCharges(charges);
			AssertEquals("Same invoice type, but different currencies - 2 invoices", 2, results.Count);

			charge1.JR_RX_NKSellCurrency = currency2.RX_Code;
			results = distributor.DistributeCharges(charges);
			AssertEquals("Same invoice type, and now both same currency 1 invoice", 1, results.Count);
		}

		public void TestDistributeChargesTaxCodeIgnoredWhenNotTaxInvoiceType()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "CUR";

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			AccTaxRate taxRate1 = Factory.New<AccTaxRate>();
			AccTaxRate taxRate2 = Factory.New<AccTaxRate>();
			Job job = Factory.NewJobForTesting<Job>();
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_OH_SellAccount = org1.PK;
			charge1.JR_AC = chargeCode1.PK;
			charge1.JR_RX_NKSellCurrency = currency1.RX_Code;
			charge1.JR_InvoiceType = "ABC";
			charge1.JR_AT_SellGSTRate = taxRate1.PK;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charges.Add(charge1);

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_OH_SellAccount = org1.PK;
			charge2.JR_AC = chargeCode1.PK;
			charge2.JR_RX_NKSellCurrency = currency1.RX_Code;
			charge2.JR_InvoiceType = "ABC";
			charge1.JR_AT_SellGSTRate = taxRate2.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charges.Add(charge2);

			PostingChargeDistributor distributor = GetDistributor();
			PostingChargeCollection results = distributor.DistributeCharges(charges);
			AssertEquals("1 collections of charges as tax code is not used yet", 1, results.Count);

			charge1.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.InvoicePerTaxCode);
			charge2.JR_InvoiceType = GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice);

			results = distributor.DistributeCharges(charges);
			AssertEquals("2 unique collection of charges as the tax codes are important now", 2, results.Count);
		}

		public void TestChargeSplitterCalled()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_RL_NKClosestPort = "AUSYD";
			orgFactory.Save();

			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "CUR";

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			Job job = Factory.NewJobForTesting<Job>();
			for (int i = 0; i < 10; i++)
			{
				charges.Add(GetTestCharge(job, org1, chargeCode1, currency1, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice)));
			}

			PostingChargeDistributor distributor = GetDistributor();
			PostingChargeCollection results = distributor.DistributeCharges(charges);
			AssertEquals("1 collections of charges as all are on the final invoice", 1, results.Count);
			PostingChargeKey key = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice), ZGuid.Empty, ZGuid.Empty, 0);
			AssertEquals("10 charges in the 1 collection", 10, results.GetCharges(key).Count);

			AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			results = distributor.DistributeCharges(charges);
			AssertEquals("1 collections of charges as all are on the final invoice, as No Max Line Number is Set", 1, results.Count);
			AssertEquals("10 charges in the 1 collection", 10, results.GetCharges(key).Count);

			AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			results = distributor.DistributeCharges(charges);
			AssertEquals("2 collections of charges as all are on the final invoice, as Maximum number of line is 5", 2, results.Count);

			PostingChargeKey key2 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice), ZGuid.Empty, ZGuid.Empty, 0);
			key2.SplitInvoiceCount = 1;
			AssertEquals("5 charges in the first collection", 5, results.GetCharges(key).Count);
			AssertEquals("5 charges in the next collection", 5, results.GetCharges(key2).Count);
		}

		public void TestChargeSplitterCalled_PlaceOfSupply()
		{
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgFactory = new BusinessObjectFactory();
				var org1 = orgFactory.NewWithValidTestData<OrgHeader>();
				org1.OH_RL_NKClosestPort = "AUSYD";
				orgFactory.Save();

				var currency1 = Factory.New<RefCurrency>();
				currency1.RX_Code = "CUR";

				var chargeCode1 = Factory.New<AccChargeCode>();

				var charges = new IReceivablesPostingChargeCollection();
				Job job = Factory.NewJobForTesting<Job>();
				for (int i = 0; i < 10; i++)
				{
					var placeOfSupply = i % 2 == 0 ? "NSW" : "WA";
					charges.Add(GetTestCharge(job, org1, chargeCode1, currency1, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice), placeOfSupply));
				}

				var distributor = GetDistributor();
				var results = distributor.DistributeCharges(charges);
				AssertEquals("2 collections of charges as there are differnt Places of Supply", 2, results.Count);
				var key1 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice), ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");
				var distributedCharges = results.GetCharges(key1);
				AssertEquals("5 charges in the 1st collection", 5, distributedCharges.Count);
				Assert("PlaceOfSupply == 'NSW'", distributedCharges.All(x => x.SellPlaceOfSupply == "NSW"));

				var key2 = new PostingChargeKey(org1.PK, GetCorrectedInvoiceType(InvoiceTypesList.Codes.FinalInvoice), ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "WA");
				distributedCharges = results.GetCharges(key2);
				AssertEquals("5 charges in the 2nd collection", 5, distributedCharges.Count);
				Assert("PlaceOfSupply == 'WA'", distributedCharges.All(x => x.SellPlaceOfSupply == "WA"));
			}
		}

		[ExpectNoExceptions]
		public void TestDistributeChargesWithoutSellCurrency()
		{
			Job job2 = Factory.NewJobForTesting<Job>();
			Charge charge1 = job2.Charges.AddNew();
			charge1.JR_RX_NKSellCurrency = "";
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			PostingChargeDistributor distributor = new PostingChargeDistributor();
			PostingChargeCollection result = distributor.DistributeCharges(charges);
		}

		public void TestDistributeChargesAgencyInvoiceTypes()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			OrgHeader org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "CR1";

			RefCurrency currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = "CR2";

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			Job job = Factory.NewJobForTesting<Job>();

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();

			var charge1 = CreateNewCharge(org1, currency1, chargeCode1, AgencyInvoiceTypesList.Codes.ForeignCollect, job);
			charges.Add(charge1);

			Charge charge2 = CreateNewCharge(org1, currency2, chargeCode2, AgencyInvoiceTypesList.Codes.ForeignCollect, job);
			charges.Add(charge2);

			Charge charge3 = CreateNewCharge(org1, currency1, chargeCode1, AgencyInvoiceTypesList.Codes.ForeignPrePaid, job);
			charges.Add(charge3);

			Charge charge4 = CreateNewCharge(org1, currency2, chargeCode2, AgencyInvoiceTypesList.Codes.ForeignPrePaid, job);
			charges.Add(charge4);

			PostingChargeDistributor distributor = GetDistributor();
			PostingChargeCollection results = distributor.DistributeCharges(charges);
			var keys = results.Keys.Cast<PostingChargeKey>();
			var firstkey = keys.First();
			AssertEquals("2 collections of charges for ForeignCollect", 2, keys.Count(k => k.InvoiceType == charge1.JR_InvoiceType));
			AssertEquals("2 collections of charges for ForeignPrePaid", 2, keys.Count(k => k.InvoiceType == charge3.JR_InvoiceType));
			AssertEquals("4 collections of charges - 2 for ForeignCollect, 2 for ForeignPrePaid", 4, results.Count);
			AssertEquals("2 keys for Currency1", 2, keys.Count(k => k.SellCurrency == currency1.RX_Code));
			AssertEquals("2 keys for Currency2", 2, keys.Count(k => k.SellCurrency == currency2.RX_Code));
		}

		Charge CreateNewCharge(OrgHeader org, RefCurrency currency, AccChargeCode chargeCode, string invoiceType, Job job)
		{
			var charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = org.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_RX_NKSellCurrency = currency.RX_Code;
			charge.JR_InvoiceType = GetCorrectedInvoiceType(invoiceType);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;

			return charge;
		}

		public void TestPostingGroupIdTakenIntoDistribution()
		{
			var tax2 = Factory.NewWithValidTestData<AccTaxRate>();
			var tax3 = Factory.NewWithValidTestData<AccTaxRate>();

			tax2.AT_PostingGroupId = 1;
			tax3.AT_PostingGroupId = 2;

			Factory.Save();

			var job1 = Factory.NewJobForTesting<Job>();
			var charge1 = job1.Charges.AddNew();
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			var charge2 = job1.Charges.AddNew();
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			var charge3 = job1.Charges.AddNew();
			charge3.JR_GB = GlbBranch.CurrentBranch.PK;
			var charge4 = job1.Charges.AddNew();
			charge4.JR_GB = GlbBranch.CurrentBranch.PK;

			charge1.JR_AT_SellGSTRate = tax2.PK;
			charge2.JR_AT_SellGSTRate = tax2.PK;
			charge3.JR_AT_SellGSTRate = tax3.PK;
			charge4.JR_AT_SellGSTRate = tax2.PK;

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);
			charges.Add(charge3);
			charges.Add(charge4);

			var distributor = new PostingChargeDistributor();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
				var result = distributor.DistributeCharges(charges);
				AssertEquals(1, result.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
				var result = distributor.DistributeCharges(charges);
				AssertEquals(2, result.Count);

				var keys = result.Keys.Cast<PostingChargeKey>();
				var collection1 = result[keys.First(x => x.TaxRatePostingGroupId == 1)];
				AssertEquals(3, collection1.Count);
				AssertEquals(true, collection1.Contains(charge1));
				AssertEquals(true, collection1.Contains(charge2));
				AssertEquals(true, collection1.Contains(charge4));

				var collection2 = result[keys.First(x => x.TaxRatePostingGroupId == 2)];
				AssertEquals(1, collection2.Count);
				AssertEquals(true, collection2.Contains(charge3));
			}
		}

		public void TestBranchTakenIntoDistributionWhenEnabled()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany);

			var job = Factory.NewJobForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			var charge1 = job.Charges.AddNew();
			var charge2 = job.Charges.AddNew();
			var charge3 = job.Charges.AddNew();
			var charge4 = job.Charges.AddNew();

			charge1.JR_GB = branch1.PK;
			charge2.JR_GB = branch2.PK;
			charge3.JR_GB = branch1.PK;
			charge4.JR_GB = branch3.PK;

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);
			charges.Add(charge3);
			charges.Add(charge4);

			var distributor = new PostingChargeDistributor();

			var result = distributor.DistributeCharges(charges);
			AssertEquals("By default branch level posting behaviour is turned off, so only one invoice will be created", 1, result.Count);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			result = distributor.DistributeCharges(charges);

			AssertEquals("Branch level registry is enabled but no branch grouping setup is configured, it invoices will be generated per branch. Hence 3 invoices should be created", 3, result.Count);

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch2.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch3.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			result = distributor.DistributeCharges(charges);
			AssertEquals(2, result.Count);

			var keys = result.Keys.Cast<PostingChargeKey>();
			var collection1 = result[keys.First(x => x.Branch == branch1.PK)];
			AssertEquals(2, collection1.Count);

			var collection2 = result[keys.First(x => x.Branch == branch3.PK)];
			AssertEquals(2, collection2.Count);
		}

		public void TestDistributorSplitsBySellInvoiceCurrencyForAllSupportingInvoiceTypes()
		{
			var job = Factory.NewJobForTesting<Job>();
			var charge1 = GetTestCharge(job, TestObjectCreator.ABIGAS, TestObjectCreator.FRT, TestObjectCreator.AUD, invoiceType: "");
			var charge2 = GetTestCharge(job, TestObjectCreator.ABIGAS, TestObjectCreator.FRT, TestObjectCreator.AUD, invoiceType: "");
			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			var distributor = new PostingChargeDistributor();

			foreach (var invoiceType in new InvoiceTypesList().ToArray().Select(x => x.Code).Union(new AgencyInvoiceTypesList().ToArray().Select(x => x.Code)))
			{
				charge1.JR_InvoiceType = invoiceType;
				charge2.JR_InvoiceType = invoiceType;

				if (invoiceType == InvoiceTypesList.Codes.DoNotPost)
				{
					// Do nothing
				}
				else if (InvoiceTypeCalculationProvider.BillInLocalCurrency(invoiceType))
				{
					charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
					Assert(invoiceType + " - BillInInvoiceCurrency expected to be true", charge2.BillInInvoiceCurrency);

					var result = distributor.DistributeCharges(charges);
					AssertEquals(invoiceType + " - Charges should be split", 2, result.Count);
				}
				else
				{
					charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;   // The filed will be read only and user could not set it but some data imports could
					Assert(invoiceType + " - BillInInvoiceCurrency expected to be false", !charge2.BillInInvoiceCurrency);

					var result = distributor.DistributeCharges(charges);
					AssertEquals(invoiceType + " - Charges should be in one group", 1, result.Count);
				}
			}
		}

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;

		#region Implementation

		protected virtual PostingChargeDistributor GetDistributor()
		{
			return new PostingChargeDistributor();
		}

		protected virtual string GetCorrectedInvoiceType(string invoiceType)
		{
			return invoiceType;
		}

		Charge GetTestCharge(Job job, OrgHeader org, AccChargeCode chargeCode, RefCurrency currency, ZString invoiceType, ZString placeOfSupply = default)
		{
			Charge newCharge = job.Charges.AddNew();
			newCharge.JR_OH_SellAccount = org.PK;
			newCharge.JR_AC = chargeCode.PK;
			newCharge.JR_RX_NKSellCurrency = currency.RX_Code;
			newCharge.JR_InvoiceType = invoiceType;
			newCharge.JR_SellPlaceOfSupply = placeOfSupply;
			newCharge.JR_GB = GlbBranch.CurrentBranch.PK;

			return newCharge;
		}

		#endregion
	}
}
