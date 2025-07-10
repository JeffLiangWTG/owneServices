using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class IReceivablesPostingChargeCollectionTest : TestCaseWithFactory
	{
		public void TestJob()
		{
			Job job1 = Factory.NewJobForTesting<Job>();

			ForwardingShipment ship = Factory.New<ForwardingShipment>();
			Job job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = ship.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Charge charge1 = job2.Charges.AddNew();
			Charge charge2 = job2.Charges.AddNew();

			job1.Charges.IncludeChargesFromJobs(job2.PK);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			AssertEquals(job2, charges.Job);

			charges.Job = job1;
			AssertEquals(job1, charges.Job);
		}

		public void TestIsDisbursementChargeCollection()
		{
			IReceivablesPostingChargeCollection coll = new IReceivablesPostingChargeCollection();
			PostingChargeKey key = new PostingChargeKey(ZGuid.NewZGuid(), InvoiceTypesList.Codes.FreightInvoice, ZGuid.Empty, ZGuid.Empty, 0);
			coll.Key = key;
			AssertEquals("Not a disbursement collection", false, coll.IsDisbursementChargeCollection);

			key = new PostingChargeKey(ZGuid.NewZGuid(), InvoiceTypesList.Codes.DisbursementInvoice, ZGuid.Empty, ZGuid.Empty, 0);
			coll.Key = key;
			AssertEquals("A disbursement collection", true, coll.IsDisbursementChargeCollection);

			key = new PostingChargeKey(ZGuid.NewZGuid(), InvoiceTypesList.Codes.DisbursementInvoice_Batching, ZGuid.Empty, ZGuid.Empty, 0);
			coll.Key = key;
			AssertEquals("A disbursement collection", true, coll.IsDisbursementChargeCollection);

			key = new PostingChargeKey(ZGuid.NewZGuid(), InvoiceTypesList.Codes.DisbursementInForeignCurrency, ZGuid.Empty, ZGuid.Empty, 0);
			coll.Key = key;
			AssertEquals("A disbursement collection", true, coll.IsDisbursementChargeCollection);

			key = new PostingChargeKey(ZGuid.NewZGuid(), InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching, ZGuid.Empty, ZGuid.Empty, 0);
			coll.Key = key;
			AssertEquals("A disbursement collection", true, coll.IsDisbursementChargeCollection);
		}

		public void TestFieldsProxiedFromFirstCharge()
		{
			IReceivablesPostingChargeCollection coll = new IReceivablesPostingChargeCollection();
			AssertEquals("No charges so CFX Should not be created", false, coll.ShouldCreateCFXJournal);
			TestObjectCreator helper = new TestObjectCreator(new BusinessObjectFactory());

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_JobNum = "00001001";
			testJob.LocalChargesPK = helper.LocalClient.PK;
			testJob.JH_LocalChargesCFX = 5m;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			ExchangeRate rate = testJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "USD";
			rate.JF_BaseRate = 0.73m;

			Charge charge1 = testJob.Charges.AddNew();
			charge1.JR_LocalSellAmt = 500m;
			charge1.JR_OH_SellAccount = helper.LocalClient.PK;
			charge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			coll.Add(charge1);

			AssertEquals("Posting Currency should be same as firstcharge", charge1.SellCurrency.RX_Code, coll.PostingCurrency);
			AssertEquals("Posting Currency Exchange Rate should be same as first charge", charge1.JR_OSSellExRate, coll.PostingCurrencyExchangeRate);

			AssertEquals("Invoice posting department", testJob.JH_GE, coll.InvoicePostingDepartment);
			AssertEquals("Invoice posting branch", testJob.JH_GB, coll.InvoicePostingBranch);

			AssertEquals("Job PK", testJob.PK, coll.JobPK);
			AssertEquals("Job Number", testJob.JH_JobNum, coll.JobNumber);
			AssertEquals("Bill In local", ((IReceivablesPostingCharge)charge1).BillInLocalCurrency, coll.IsBillInLocalCurrency);
			AssertEquals("Debtor", charge1.JR_OH_SellAccount, coll.Debtor);
			AssertEquals("DebtorAddress", ZGuid.Empty, coll.DebtorAddress);
			AssertEquals("DebtorContact", ZGuid.Empty, coll.DebtorContact);

			coll.PostingCurrency = helper.USD.RX_Code;
			coll.PostingCurrencyExchangeRate = 0.65m;

			AssertEquals("Should bring through correct posting currency even though charge is in different currency", helper.USD.RX_Code, coll.PostingCurrency);
			AssertEquals("Should bring through correct posting currency exrate even though charge has a different exrate", 0.65m, coll.PostingCurrencyExchangeRate);
		}

		public void TestFieldsProxiedFromFirstCharge_DebtorAddressContact()
		{
			var collection = new IReceivablesPostingChargeCollection();
			var helper = new TestObjectCreator(new BusinessObjectFactory());

			var job = Factory.NewJobForTesting<Job>();
			var charge1 = job.Charges.AddNew();
			var expectedDebtor = helper.LocalClient;
			var expectedAddress = helper.CreateAddress(expectedDebtor);
			var expectedContact = helper.CreateContact(expectedDebtor);
			charge1.JR_OH_SellAccount = expectedDebtor.PK;
			charge1.JR_OA_SellInvoiceAddress = expectedAddress.PK;
			charge1.JR_OC_SellInvoiceContact = expectedContact.PK;
			collection.Add(charge1);

			collection.Add(job.Charges.AddNew()); //to have more charges in collaction than only the first one.

			AssertEquals("Debtor", expectedDebtor.PK, collection.Debtor);
			AssertEquals("DebtorAddress", expectedAddress.PK, collection.DebtorAddress);
			AssertEquals("DebtorContact", expectedContact.PK, collection.DebtorContact);
		}

		public void TestShouldCreateCFXJournal()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestObjectCreator helper = new TestObjectCreator(new BusinessObjectFactory());

			IReceivablesPostingChargeCollection coll = new IReceivablesPostingChargeCollection();
			AssertEquals("No charges so CFX Should not be created", false, coll.ShouldCreateCFXJournal);

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.LocalChargesPK = helper.LocalClient.PK;
			testJob.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5m);

			ExchangeRate rate = testJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "USD";
			rate.JF_BaseRate = 0.73m;

			Charge charge1 = testJob.Charges.AddNew();
			charge1.JR_LocalSellAmt = 500m;
			charge1.JR_OH_SellAccount = helper.LocalClient.PK;
			coll.Add(charge1);

			AssertEquals("Precondition: CFX Amount is zero", 0m, charge1.JR_CFXAmt);
			AssertEquals("1 charge, but since CFX Amount is zero, no CFX Should be created", false, coll.ShouldCreateCFXJournal);

			charge1.JR_RX_NKSellCurrency = helper.USD.RX_Code;
			charge1.JR_OSSellAmt = 500m;
			Assert("Precondition: CFX Amount is not zero", charge1.JR_CFXAmt > 0m);
			AssertEquals("1 charge, and since CFX Amount is not zero, CFX Should be created", true, coll.ShouldCreateCFXJournal);
		}

		public void TestTotalValueInLocalCurrency()
		{
			IReceivablesPostingChargeCollection coll = new IReceivablesPostingChargeCollection();
			AssertEquals(0m, coll.TotalValueInLocalCurrency);

			Charge charge1 = Factory.New<Charge>();
			charge1.JR_LocalSellAmt = 500m;
			coll.Add(charge1);

			Charge charge2 = Factory.New<Charge>();
			charge2.JR_LocalSellAmt = 90m;
			AccTaxRate rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.SetRateNumerator_ForTestOnly(10);
			charge2.JR_AT_SellGSTRate = rate.PK;
			coll.Add(charge2);

			AssertEquals(599m, coll.TotalValueInLocalCurrency);
		}

		public void TestTotalValueInForeignCurrency()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());

			IReceivablesPostingChargeCollection coll = new IReceivablesPostingChargeCollection();
			AssertEquals(0m, coll.TotalValueInForeignCurrency);

			Charge charge1 = Factory.New<Charge>();
			charge1.JR_OSSellExRate = 0.8m;
			charge1.JR_RX_NKSellCurrency = testObjectCreator.USD.RX_Code;
			charge1.JR_OSSellAmt = 400m;
			coll.Add(charge1);

			Charge charge2 = Factory.New<Charge>();
			charge2.JR_OSSellExRate = 0.8m;
			charge2.JR_RX_NKSellCurrency = testObjectCreator.USD.RX_Code;
			charge2.JR_OSSellAmt = 200m;
			AccTaxRate rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.SetRateNumerator_ForTestOnly(10);
			charge2.JR_AT_SellGSTRate = rate.PK;
			coll.Add(charge2);

			AssertEquals(620m, coll.TotalValueInForeignCurrency);
			AssertEquals(775m, coll.TotalValueInLocalCurrency);
		}

		public void TestITransactionBranchCalculationDataProviderFromJobCharge()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var shipment = testObjectCreator.CreateShipment("S001");
			var job = testObjectCreator.CreateJob(shipment, false);

			var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "", null, 100, testObjectCreator.Creditor1, "INV1", null, 0, null);
			var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "", null, 100, testObjectCreator.Creditor1, "INV1", null, 0, null);
			var receivablesPostingCharges = new IReceivablesPostingChargeCollection();
			receivablesPostingCharges.Add(charge1);
			receivablesPostingCharges.Add(charge2);
			var charges = receivablesPostingCharges as ITransactionBranchCalculationDataProviderFromJobCharge;

			var expectedJobBranchPK = job.JH_GB;
			var expectedLineBranchPKs = receivablesPostingCharges.Select(x => x.Branch).ToHashSet();

			CombineAssertions(() =>
			{
				AssertEquals("JobBranchPK", expectedJobBranchPK, charges.JobBranchPK);
				AssertContainsExactElementsInAnyOrder("LineBranchPKs", expectedLineBranchPKs, charges.LineBranchPKs);
				AssertEquals("charges.AnyCharges", true, charges.AnyCharges);
			});
		}
	}
}
