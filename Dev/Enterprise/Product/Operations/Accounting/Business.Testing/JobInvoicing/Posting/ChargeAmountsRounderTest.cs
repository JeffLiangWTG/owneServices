using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class ChargeAmountsRounderTest : TestCaseWithFactory
	{
		public void TestRoundChargeAmountsJPY()
		{
			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYen);

			AssertEquals(61m, job.Charges[0].JR_LocalSellAmt);
			AssertEquals(106m, job.Charges[0].JR_OSSellAmt);
			AssertEquals(13m, job.Charges[1].JR_LocalSellAmt);
			AssertEquals(26m, job.Charges[1].JR_OSSellAmt);
			AssertEquals(53m, job.Charges[2].JR_LocalSellAmt);
			AssertEquals(53m, job.Charges[2].JR_OSSellAmt);
			AssertEquals(13m, job.Charges[3].JR_LocalSellAmt);
			AssertEquals(13m, job.Charges[3].JR_OSSellAmt);
			AssertEquals(53m, job.Charges[4].JR_LocalSellAmt);
			AssertEquals(106m, job.Charges[4].JR_OSSellAmt);
			AssertEquals(13m, job.Charges[5].JR_LocalSellAmt);
			AssertEquals(26m, job.Charges[5].JR_OSSellAmt);
		}

		public void TestRoundChargeAmountsJPY_SellInvoiceCurrency()
		{
			SetUpTestData(true);
			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYen);

			AssertEquals(61m, job.Charges[0].JR_LocalSellAmt);
			AssertEquals(106m, job.Charges[0].JR_OSSellAmt);
			AssertEquals(13m, job.Charges[1].JR_LocalSellAmt);
			AssertEquals(26m, job.Charges[1].JR_OSSellAmt);
			AssertEquals(53m, job.Charges[2].JR_LocalSellAmt);
			AssertEquals(53m, job.Charges[2].JR_OSSellAmt);
			AssertEquals(13m, job.Charges[3].JR_LocalSellAmt);
			AssertEquals(13m, job.Charges[3].JR_OSSellAmt);
			AssertEquals(53m, job.Charges[4].JR_LocalSellAmt);
			AssertEquals(106m, job.Charges[4].JR_OSSellAmt);
			AssertEquals(13m, job.Charges[5].JR_LocalSellAmt);
			AssertEquals(26m, job.Charges[5].JR_OSSellAmt);
		}

		public void TestRoundChargeAmountsJPX()
		{
			AccountingConfigurationRegistry.Instance.RoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.CC2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.GSTFREE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.IncludeInRoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.CC3.PK.ToGuid());

			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYenWithCharge);
			AssertChargesForJPX();

			//run second time to check that nothing changed
			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYenWithCharge);
			AssertChargesForJPX();
		}

		public void TestRoundChargeAmountsJPX_SellInvoiceCurrency()
		{
			// Testing border case when user set a Local Currency as  SellInvoice Currency.
			SetUpTestData(true);
			AccountingConfigurationRegistry.Instance.RoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.CC2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.GSTFREE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.IncludeInRoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.CC3.PK.ToGuid());

			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYenWithCharge);
			AssertChargesForJPX();

			//run second time to check that nothing changed
			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYenWithCharge);
			AssertChargesForJPX();
		}

		void AssertChargesForJPX()
		{
			AssertEquals(7, job.Charges.Count);
			AssertEquals(4, job2.Charges.Count);
			AssertEquals(2, job3.Charges.Count);

			AssertEquals(53m, job.Charges[0].JR_LocalSellAmt);
			Assert("job Charge 0 HasContext(AddChargeWhenPostingCancelled)", !job.Charges[0].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(13m, job.Charges[1].JR_LocalSellAmt);
			Assert("job Charge 1 HasContext(AddChargeWhenPostingCancelled)", !job.Charges[1].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(53m, job.Charges[2].JR_LocalSellAmt);
			Assert("job Charge 2 HasContext(AddChargeWhenPostingCancelled)", !job.Charges[2].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(13m, job.Charges[3].JR_LocalSellAmt);
			Assert("job Charge 3 HasContext(AddChargeWhenPostingCancelled)", !job.Charges[3].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(53m, job.Charges[4].JR_LocalSellAmt);
			Assert("job Charge 4 HasContext(AddChargeWhenPostingCancelled)", !job.Charges[4].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(13m, job.Charges[5].JR_LocalSellAmt);
			Assert("job Charge 5 HasContext(AddChargeWhenPostingCancelled)", !job.Charges[5].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(1m, job.Charges[6].JR_LocalSellAmt);
			Assert("job Charge 6 HasContext(AddChargeWhenPostingCancelled)", job.Charges[6].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(creator.CC2.PK, job.Charges[6].JR_AC);
			AssertEquals(creator.GSTFREE1.PK, job.Charges[6].JR_AT_SellGSTRate);
			AssertEquals(InvoiceTypesList.Codes.DisbursementInvoice, job.Charges[6].JR_InvoiceType);

			AssertEquals(11m, job2.Charges[0].JR_LocalSellAmt);
			Assert("job 2 Charge 0 HasContext(AddChargeWhenPostingCancelled)", !job2.Charges[0].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(11m, job2.Charges[1].JR_LocalSellAmt);
			Assert("job 2 Charge 1 HasContext(AddChargeWhenPostingCancelled)", !job2.Charges[1].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(11m, job2.Charges[2].JR_LocalSellAmt);
			Assert("job 2 Charge 2 HasContext(AddChargeWhenPostingCancelled)", !job2.Charges[2].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(7m, job2.Charges[3].JR_LocalSellAmt);
			Assert("job 2 Charge 3 HasContext(AddChargeWhenPostingCancelled)", job2.Charges[3].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(creator.CC2.PK, job2.Charges[3].JR_AC);
			AssertEquals(creator.GSTFREE1.PK, job2.Charges[3].JR_AT_SellGSTRate);

			AssertEquals(11m, job3.Charges[0].JR_LocalSellAmt);
			Assert("job 3 Charge 0 HasContext(AddChargeWhenPostingCancelled)", !job3.Charges[0].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(11m, job3.Charges[1].JR_LocalSellAmt);
			Assert("job 3 Charge 1 HasContext(AddChargeWhenPostingCancelled)", !job3.Charges[1].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));

			AssertEquals(-44m, job4.Charges[0].JR_LocalSellAmt);
			Assert("job 4 Charge 0 HasContext(AddChargeWhenPostingCancelled)", !job4.Charges[0].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals(-44m, job4.Charges[1].JR_LocalSellAmt);
			Assert("job 4 Charge 1 HasContext(AddChargeWhenPostingCancelled)", !job4.Charges[1].HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled));
			AssertEquals("rounding should handle negative amounts", 8m, job4.Charges[2].JR_LocalSellAmt);
			AssertEquals(creator.CC2.PK, job4.Charges[2].JR_AC);
			AssertEquals(creator.GSTFREE1.PK, job4.Charges[2].JR_AT_SellGSTRate);
		}

		public void TestDebtorIsSetForRoundingChargeWithJPX()
		{
			// Testing border case when user set a Local Currency as a SellInvoice Currency.
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "JPAMM";
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "JPAAM";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_INCO = "CIP";

			Job newJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			newJob.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;
			newJob.Parent = shipment;

			newJob.Charges.AddNew();
			newJob.Charges[0].JR_AC = creator.CC2.PK;
			newJob.Charges[0].JR_OH_SellAccount = creator.AALSHI.PK;
			newJob.Charges[0].JR_RX_NKSellCurrency = "USD";
			newJob.Charges[0].JR_OSSellExRate = 1;
			newJob.Charges[0].JR_LocalSellAmt = 21797;

			newJob.Charges.AddNew();
			newJob.Charges[1].JR_AC = creator.CC3.PK;
			newJob.Charges[1].JR_OH_SellAccount = creator.AALSHI.PK;
			newJob.Charges[1].JR_RX_NKSellCurrency = "USD";
			newJob.Charges[1].JR_OSSellExRate = 1;
			newJob.Charges[1].JR_LocalSellAmt = 9948;

			Factory.Save();

			IReceivablesPostingChargeCollection chargesPostingCollection = new IReceivablesPostingChargeCollection();
			chargesPostingCollection.AddRange((IEnumerable<IReceivablesPostingCharge>)newJob.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(chargesPostingCollection);

			AssertEquals("Precondition - total 2 charges", 2, newJob.Charges.Count);
			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYenWithCharge);
			AssertEquals("Postcondition - total 3 charges as new charge will be created for JPX rounding amount", 3, newJob.Charges.Count);
			foreach (Charge charge in newJob.Charges)
			{
				AssertEquals("charge's debtor should be set", creator.AALSHI.PK, charge.JR_OH_SellAccount);
				AssertEquals("charge's address should be empty as it not set on original charge", ZGuid.Empty, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("charge's contact should be empty as it not set on original charge", ZGuid.Empty, charge.JR_OC_SellInvoiceContact);
			}
		}

		public void TestDebtorAddressContactIsSetForNewCharges()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "JPAMM";
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "JPAAM";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_INCO = "CIP";

			Job newJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			newJob.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;
			newJob.Parent = shipment;

			newJob.Charges.AddNew();
			newJob.Charges[0].JR_AC = creator.CC2.PK;
			var expectedDebtor = creator.AALSHI;
			var expectedAddress = creator.CreateAddress(expectedDebtor);
			var expectedContact = creator.CreateContact(expectedDebtor);
			newJob.Charges[0].JR_OH_SellAccount = expectedDebtor.PK;
			newJob.Charges[0].JR_OA_SellInvoiceAddress = expectedAddress.PK;
			newJob.Charges[0].JR_OC_SellInvoiceContact = expectedContact.PK;
			newJob.Charges[0].JR_RX_NKSellCurrency = "USD";
			newJob.Charges[0].JR_OSSellExRate = 1;
			newJob.Charges[0].JR_LocalSellAmt = 21797;

			Factory.Save();

			IReceivablesPostingChargeCollection chargesPostingCollection = new IReceivablesPostingChargeCollection();
			chargesPostingCollection.AddRange((IEnumerable<IReceivablesPostingCharge>)newJob.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(chargesPostingCollection);

			AssertEquals("Precondition - charge count", 1, newJob.Charges.Count);
			new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYenWithCharge);
			AssertEquals("Postcondition - charges count must be one more as new charge will be created for JPX rounding amount", 2, newJob.Charges.Count);
			foreach (Charge charge in newJob.Charges)
			{
				AssertEquals("charge's debtor", expectedDebtor.PK, charge.JR_OH_SellAccount);
				AssertEquals("charge's address", expectedAddress.PK, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("charge's contact", expectedContact.PK, charge.JR_OC_SellInvoiceContact);
			}
		}

		public void TestValidationIsRunAlwaysForJPXRoundingCharge()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "JPAMM";
			var shipment = creator.CreateShipment("S001", "USLAX", "JPAAM", transportMode: Constants.TransportModes.Air, incoTerm: Constants.IncoTerms.CarriageAndInsurancePaidTo);
			var newJob = creator.CreateJob(shipment, false, localClientOrg: creator.LocalClient);
			creator.SetExchangeRate(newJob, creator.USD, 1);
			var charge = creator.CreateCharge(newJob, creator.CC1, sellCurrency: creator.USD, osSellAmt: 21797, debtor: newJob.LocalCharges);
			Factory.Save();

			var chargesPostingCollection = new IReceivablesPostingChargeCollection();
			chargesPostingCollection.AddRange(newJob.Charges.Cast<IReceivablesPostingCharge>());
			var distributedCharges = new PostingChargeDistributor().DistributeCharges(chargesPostingCollection);

			var previousChargeCount = newJob.Charges.Count;
			Factory.SuspendValidation();
			try
			{
				new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, Constants.RoundingRules.Codes.JapanYenWithCharge);
			}
			finally
			{
				Factory.ResumeValidation();
			}
			AssertEquals("Postcondition: charge Count difference - new charge must be created for JPX rounding amount.", 1, newJob.Charges.Count - previousChargeCount);
			var newCharge = newJob.Charges.Find(x => x.PK != charge.PK).First();
			Assert("New charge must be validated even when factory validation is suspended.", newCharge.HasErrors);
		}

		#region Implementation

		Job job;
		Job job2;
		Job job3;
		Job job4;
		PostingChargeCollection distributedCharges;
		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			creator = new TestObjectCreator(Factory);
			SetUpTestData();
		}

		/// <summary>
		/// Re-usable test data set up
		/// </summary>
		/// <param name="useSellInvoiceCurrency">Used for testing border case when user set a Local Currency as  SellInvoice Currency</param>
		void SetUpTestData(bool useSellInvoiceCurrency = false)
		{
			creator.CC2.AC_ChargeGroup = "FRT";
			creator.CC3.AC_ChargeGroup = "FRT";

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "USLAX";

			job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;

			creator.CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			for (int i = 0; i < 6; i++)
			{
				job.Charges.AddNew();
				job.Charges[i].JR_AC = creator.CC1.PK;
				job.Charges[i].JR_OH_SellAccount = creator.AALSHI.PK;
			}

			job.Charges[2].JR_AC = creator.CC3.PK;
			job.Charges[0].JR_RX_NKSellCurrency = job.Charges[1].JR_RX_NKSellCurrency = "USD";
			job.Charges[4].JR_RX_NKSellCurrency = job.Charges[5].JR_RX_NKSellCurrency = "USD";

			if (useSellInvoiceCurrency)
			{
				job.Charges[0].JR_RX_NKSellInvoiceCurrency = job.Charges[1].JR_RX_NKSellInvoiceCurrency = "AUD";
			}

			job.Charges[0].JR_InvoiceType = job.Charges[1].JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			job.Charges[2].JR_InvoiceType = job.Charges[3].JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			job.Charges[4].JR_InvoiceType = job.Charges[5].JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;

			job.ExchangeRates[0].JF_BaseRate = 2m;
			job.Charges[0].JR_LocalSellAmt = job.Charges[2].JR_LocalSellAmt = job.Charges[4].JR_LocalSellAmt = 53m;
			job.Charges[1].JR_LocalSellAmt = job.Charges[3].JR_LocalSellAmt = job.Charges[5].JR_LocalSellAmt = 13m;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKDestination = "AUSYD";
			shipment2.JS_RL_NKOrigin = "USLAX";

			job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.Parent = shipment2;
			job2.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;

			job2.Charges.AddNew();
			job2.Charges[0].JR_AC = creator.CC1.PK;
			job2.Charges[0].JR_OH_SellAccount = creator.AALSHI.PK;
			job2.Charges[0].JR_RX_NKSellCurrency = "USD";
			job2.Charges[0].RevenueExchangeRate.SetBuyRate_ForTestOnly(1m);

			if (useSellInvoiceCurrency)
			{
				job2.Charges[0].JR_RX_NKSellInvoiceCurrency = "AUD";
			}
			job2.Charges[0].JR_LocalSellAmt = 11m;

			job2.Charges.AddNew();
			job2.Charges[1].JR_AC = creator.CC2.PK;
			job2.Charges[1].JR_OH_SellAccount = creator.AALSHI.PK;
			job2.Charges[1].JR_RX_NKSellCurrency = "USD";
			job2.Charges[1].RevenueExchangeRate.SetBuyRate_ForTestOnly(1m);

			if (useSellInvoiceCurrency)
			{
				job2.Charges[1].JR_RX_NKSellInvoiceCurrency = "AUD";
			}
			job2.Charges[1].JR_LocalSellAmt = 11m;

			job2.Charges.AddNew();
			job2.Charges[2].JR_AC = creator.CC3.PK;
			job2.Charges[2].JR_OH_SellAccount = creator.AALSHI.PK;
			job2.Charges[2].JR_RX_NKSellCurrency = "USD";
			job2.Charges[2].RevenueExchangeRate.SetBuyRate_ForTestOnly(1m);

			if (useSellInvoiceCurrency)
			{
				job2.Charges[2].JR_RX_NKSellInvoiceCurrency = "AUD";
			}
			job2.Charges[2].JR_LocalSellAmt = 11m;

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment3.JS_RL_NKDestination = "AUSYD";
			shipment3.JS_RL_NKOrigin = "USLAX";

			job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.Parent = shipment3;
			job3.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;

			job3.Charges.AddNew();
			job3.Charges[0].JR_AC = creator.CC2.PK;
			job3.Charges[0].JR_OH_SellAccount = creator.AALSHI.PK;
			job3.Charges[0].JR_RX_NKSellCurrency = "USD";
			job3.Charges[0].RevenueExchangeRate.SetBuyRate_ForTestOnly(1m);

			if (useSellInvoiceCurrency)
			{
				job3.Charges[0].JR_RX_NKSellInvoiceCurrency = "AUD";
			}
			job3.Charges[0].JR_LocalSellAmt = 11m;

			job3.Charges.AddNew();
			job3.Charges[1].JR_AC = creator.CC3.PK;
			job3.Charges[1].JR_OH_SellAccount = creator.AALSHI.PK;
			job3.Charges[1].JR_RX_NKSellCurrency = "USD";
			job3.Charges[1].RevenueExchangeRate.SetBuyRate_ForTestOnly(1m);

			if (useSellInvoiceCurrency)
			{
				job3.Charges[1].JR_RX_NKSellInvoiceCurrency = "AUD";
			}
			job3.Charges[1].JR_LocalSellAmt = 11m;

			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment4.JS_RL_NKDestination = "AUSYD";
			shipment4.JS_RL_NKOrigin = "USLAX";

			job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.Parent = shipment4;
			job4.JH_JobNum = "000000001";
			job4.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;

			job4.Charges.AddNew();
			job4.Charges[0].JR_AC = creator.CC1.PK;
			job4.Charges[0].JR_OH_SellAccount = creator.AALSHI.PK;
			job4.Charges[0].JR_RX_NKSellCurrency = "USD";
			job4.Charges[0].RevenueExchangeRate.SetBuyRate_ForTestOnly(4m);

			if (useSellInvoiceCurrency)
			{
				job4.Charges[0].JR_RX_NKSellInvoiceCurrency = "AUD";
			}
			job4.Charges[0].JR_LocalSellAmt = -44m;

			job4.Charges.AddNew();
			job4.Charges[1].JR_AC = creator.CC1.PK;
			job4.Charges[1].JR_OH_SellAccount = creator.AALSHI.PK;
			job4.Charges[1].JR_RX_NKSellCurrency = "USD";
			job4.Charges[1].RevenueExchangeRate.SetBuyRate_ForTestOnly(4m);

			if (useSellInvoiceCurrency)
			{
				job4.Charges[1].JR_RX_NKSellInvoiceCurrency = "AUD";
			}
			job4.Charges[1].JR_LocalSellAmt = -44m;

			Factory.Save();

			IReceivablesPostingChargeCollection chargesPostingCollection = new IReceivablesPostingChargeCollection();

			chargesPostingCollection.AddRange((IEnumerable<IReceivablesPostingCharge>)job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			chargesPostingCollection.AddRange((IEnumerable<IReceivablesPostingCharge>)job2.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			chargesPostingCollection.AddRange((IEnumerable<IReceivablesPostingCharge>)job3.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			chargesPostingCollection.AddRange((IEnumerable<IReceivablesPostingCharge>)job4.Charges.ToArray(typeof(IReceivablesPostingCharge)));

			distributedCharges = new PostingChargeDistributor().DistributeCharges(chargesPostingCollection);

			AssertEquals(3, distributedCharges.Count);
		}

		#endregion
	}
}