using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public class ConsolCostCalculationStrategyWithCalculationsTest : TestCaseWithFactory
	{
		public void TestDefaultChargeIsNotCreatedWhenConsolCostHasConsolCostCalculationStrategyWithCalculationsDuringPostingContext()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_ActualVolume = 10m;
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol);
			shipment2.JS_ActualWeight = 0m;
			shipment2.JS_ActualVolume = 0m;
			TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			Factory.Save();

			consolCost.RemoveNonApplicableCharges();
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			AssertEquals(100m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);

			Assert(!consolCost.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting));
			consolCost.UpdateApportionmentChargesListing();
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 0));
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 100m));

			consolCost.RemoveNonApplicableCharges();
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			AssertEquals(100m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);

			consolCost.SetContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting);
			Assert(consolCost.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting));

			consolCost.UpdateApportionmentChargesListing();
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			AssertEquals(100m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
		}

		public void TestHandleDeleteRemovesApportionmentChargesFromCollection()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001010", consol);
			Factory.Save();

			AssertHandleDeleteForNewConsolCost(consolCost =>
			{
				Factory.Save();
				var charge = consolCost.ApportionmentCharges[0];
				charge.JR_OSSellAmt = 300;
				charge.JR_LocalSellAmt = 300;
				Factory.Save();
				Assert("Precondition: charge.IsInDatabase", charge.IsInDatabase);
				Assert("Precondition: charge.IsChargeRevenueEdited", charge.IsChargeRevenueEdited);
				Assert("Precondition: charge should not have changes to avoid canceling then before delete", !charge.HasChanges);
			});

			AssertHandleDeleteForNewConsolCost(consolCost =>
			{
				var charge = consolCost.ApportionmentCharges[0];
				charge.JR_AL_APLine = Factory.New<APInvoiceLine>().PK;
				Assert("Precondition: charge.IsCostPosted", charge.IsCostPosted);
			});

			AssertHandleDeleteForNewConsolCost(consolCost =>
			{
				var charge = consolCost.ApportionmentCharges[0];
				charge.JR_AL_ARLine = Factory.New<ARInvoiceLine>().PK;
				Assert("Precondition: charge.IsRevenuePosted", charge.IsRevenuePosted);
			});

			AssertHandleDeleteForNewConsolCost(consolCost =>
			{
				var charge = consolCost.ApportionmentCharges[0];
				Assert("Precondition: charge.IsInDatabase", !charge.IsInDatabase);
				Assert("Precondition: charge.IsChargeRevenueEdited", !charge.IsChargeRevenueEdited);
				Assert("Precondition: charge.IsCostPosted", !charge.IsCostPosted);
				Assert("Precondition: charge.IsRevenuePosted", !charge.IsRevenuePosted);
			});

			void AssertHandleDeleteForNewConsolCost(Action<JobConsolCost> setupConsolCost)
			{
				var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
				setupConsolCost(consolCost);

				AssertEquals("Precondition: ApportionmentCharges.Count", 1, consolCost.ApportionmentCharges.Count);
				var calculationStrategy = new JobConsolCost.ConsolCostCalculationStrategy(consolCost);
				calculationStrategy.HandleDelete();
				AssertEquals("ApportionmentCharges.Count", 0, consolCost.ApportionmentCharges.Count);
			}
		}

		public void TestClearingCostLineAfterInvalidDataInConsolCostDoesNotAffectSellPart()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUMEL", "NZAKL", "C001001");
			Factory.Save();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			var job = new Job.Loader(shipment).Load(true, false);
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, arInvoice, job, TestObjectCreator.CC12, TestObjectCreator.AUD, 1M, "Rev line", 100M);
			line.AL_AT = TestObjectCreator.GSTFREE1.PK;
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC12.PK;
				cost.E6_OSCostAmount = 110M;
				AssertEquals("Should have 1 charge lines", 1, cost.ApportionmentCharges.Count);
				var jobCharge = TestObjectCreator.CreateJobCharge(line, shipment.Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
				jobCharge.JR_OSCostAmt = 110m;
				AssertNoErrors("Precondition", cost);
				Factory.Save();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
				cost.Delete();
				try
				{
					Factory.Save();
				}
				catch (OnSavingCriticalCheckException)
				{
					Fail("Saving does not fail with following critical validation exception: Related REV line tax code and class are not the same as charge tax code and class.");
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}

			if (ExceptionReporterTestListener.Instance.Count == 1 && ErrorReporter.LastKeyReported.Equals("JobCharge.ModifyingTaxRateOnPostedCharge") && ErrorReporter.LastMessageReported.StartsWith("Modifying Tax Rate on Posted Charge."))
			{
				ErrorReporter.Clear();
			}
		}

		public void TestOSCostAmountWhenChangingFromForeignCurrencyToNoDecimalLocalCurrnecy()
		{
			string originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.KoreaSouth);
			try
			{
				var consol = TestObjectCreator.CreateConsol("KRSEL", "AUSYD", "C00001000");
				consol.JK_TransportMode = "AIR";
				var shipment = TestObjectCreator.CreateShipment("S00001000", "KRSEL", "AUSYD", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);
				Factory.Save();
				var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
				consolCost.E6_RX_NKCurrency = "USD";
				consolCost.E6_ExchangeRate = 1.2m;
				consolCost.E6_LocalCostAmount = 1234m;
				Factory.Save();
				AssertEquals("OSCostAmount", 1480.8m, consolCost.E6_OSCostAmount);
				AssertEquals("LocalCostAmount", 1234m, consolCost.E6_LocalCostAmount);
				consolCost.E6_RX_NKCurrency = "KRW";
				AssertEquals("[OSCostAmount] When setting a currency to a local currency, LocalOSAmount should set the previous LocalCostAmount", 1234m, consolCost.E6_OSCostAmount);
				AssertEquals("[LocalCostAmount] When setting a currency to a local currency, LocalCostAmount should remain the same.", 1234m, consolCost.E6_LocalCostAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestOSCostAmountWhenChangingFromForeignCurrencyToAnotherNoDecimalForeignCurrnecy()
		{
			RefCurrency kRW = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");
			RefExchangeRate kRWSellRate = CreateExchangeRate(kRW, 0.8m, Core.Constants.ExchangeRateTypes.Code.SellRate);
			RefExchangeRate kRWBuyRate = CreateExchangeRate(kRW, 0.8m, Core.Constants.ExchangeRateTypes.Code.BuyRate);
			var consol = TestObjectCreator.CreateConsol("KRSEL", "AUSYD", "C00001000");
			consol.JK_TransportMode = "AIR";
			var shipment = TestObjectCreator.CreateShipment("S00001000", "KRSEL", "AUSYD", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_ExchangeRate = 1.2m;
			consolCost.E6_LocalCostAmount = 1234.45m;
			Factory.Save();
			AssertEquals("OSCostAmount", 1481.34m, consolCost.E6_OSCostAmount);
			AssertEquals("LocalCostAmount", 1234.45m, consolCost.E6_LocalCostAmount);
			var x = GlbCompany.CurrentCompany.LocalCurrency;
			consolCost.E6_RX_NKCurrency = "KRW";
			AssertEquals("[OSCostAmount] When setting a currency to another no deciaml foreign currency, LocalOSAmount should set no decimal foreign currency", 1481m, consolCost.E6_OSCostAmount);
			AssertEquals("[LocalCostAmount] When setting a currency to another no deciaml foreign currency, LocalOSAmount should recalculate", 1851.25m, consolCost.E6_LocalCostAmount);
		}

		public void TestOSCostAmountReapportionedOnChangingToNoDecimalForeignCurrnecyAndSettingExchangeRate()
		{
			var consol = TestObjectCreator.CreateConsol("IDJKT", "AUSYD", "C00001000");
			var shipment1 = TestObjectCreator.CreateShipment("S00001001", "IDJKT", "AUSYD", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var shipment2 = TestObjectCreator.CreateShipment("S00001002", "IDJKT", "AUSYD", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var shipment3 = TestObjectCreator.CreateShipment("S00001003", "IDJKT", "AUSYD", consol);
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			Factory.Save();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_OSCostAmount = 500m;
			consolCost.E6_ExchangeRate = 0.625m;
			consolCost.E6_ApportionmentMethod = "SHP";
			Factory.Save();
			AssertEquals("OSCostAmount", 500m, consolCost.E6_OSCostAmount);
			Assert("All OS Amounts should have anything after decimal point", consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_OSCostAmt.DecimalPlaces != 0));
			RefCurrency iDR = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "IDR");
			AssertEquals("No Decimals for IDR", 0, iDR.Decimals);
			consolCost.E6_RX_NKCurrency = "IDR";
			consolCost.E6_ExchangeRate = 10000m;
			AssertEquals("OSCostAmount", 500m, consolCost.E6_OSCostAmount);
			AssertEquals("Sum of OSCostAmounts on Charges", 500m, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Sum(x => x.JR_OSCostAmt));
			Assert("All OS Amounts should not have anything after decimal point", consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_OSCostAmt.DecimalPlaces == 0));
		}

		RefExchangeRate CreateExchangeRate(RefCurrency currency, ZDecimal rate, ZString type)
		{
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = type;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			return exchangeRate;
		}

		[TestDate(2012, 03, 15)]
		public void TestReversalOfConsolAPInvoiceAllowsDeletionOfConsolCost_WithNoCriticalValidationError()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			var consol = TestObjectCreator.CreateConsol("NZAKL", "AUSYD", "C00001000");
			consol.JK_TransportMode = "AIR";
			var shipment = TestObjectCreator.CreateShipment("S00001000", "NZAKL", "AUSYD", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var shipment2 = TestObjectCreator.CreateShipment("S00001001", "NZAKL", "AUSYD", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI);
			ZDateTime postDate = new ZDateTime(2012, 03, 03);
			ZDateTime invoiceDate = new ZDateTime(2012, 03, 04);
			invoice.AH_PostDate = postDate;
			invoice.AH_InvoiceDate = invoiceDate;
			var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			consolCost.E6_OSCostAmount = 1000m;
			AssertEquals("consolCost.ApportionmentCharges.Count", 2, consolCost.ApportionmentCharges.Count);
			invoice.AH_JH = ZGuid.Empty;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.ImportAllApportionmentsFromCosting();
			AssertEquals("invoice.Lines.Count", 2, invoice.Lines.Count);
			Factory.Save();
			Charge[] charges = Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, invoice.Lines[0].PK));
			AssertEquals("charges.Length", 1, charges.Length);
			Charge charge = charges[0];
			AssertNotNull("charge", charge);
			charge.JR_OSSellAmt = 250m;
			AssertEquals("JR_OSCostAmt", 500m, charge.JR_OSCostAmt);
			AssertEquals("JR_OSSellAmt", 250m, charge.JR_OSSellAmt);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			invoice = newFactory.Load<APInvoice>(invoice.PK);
			ReversingFactory reversingFactory = new ReversingFactory();
			var reversingBase = reversingFactory.NewReversing(invoice);
			reversingBase.Reverse();
			AssertNotNull("ReverseInvoice", invoice.ReverseInvoice);
			invoice.ReverseInvoice.AH_TransactionNum = "TEST_TRANSACTIONNUM";
			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			JobConsolCostCollection consolCosts = new JobConsolCostCollection(newFactory, consol);
			consolCosts.Load();
			AssertEquals("consolCosts.Count", 1, consolCosts.Count);
			consolCost = consolCosts[0];
			consolCost.Delete();
			try
			{
				newFactory.Save();
				Assert(true);
			}
			catch (Exception ex)
			{
				Assert(string.Format("Expected no exception but caught: {0}", ex.Message), false);
			}
		}

		public void TestDeleteMJA_JobConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment = TestObjectCreator.CreateShipment("S001010", consol);
			var jobCharge = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.ManualJobAccrualChargeCode, TestObjectCreator.AALSHI, 99M, true);
			Factory.Save();
			AssertEquals("Shipment should have 1 charge", 1, ((Job)shipment.Job).Charges.Count);
			AssertEquals("JR_OSSellAmt is zero for MJA job charge", 0M, ((Job)shipment.Job).Charges[0].JR_OSSellAmt);
			AssertEquals("JR_OSCostAmt is as was set for MJA job charge", 99m, ((Job)shipment.Job).Charges[0].JR_OSCostAmt);
			var calculationStrategy = new JobConsolCost.ConsolCostCalculationStrategy(jobCharge);
			calculationStrategy.HandleDelete();
			AssertEquals("Shipment should have no charge", 0, ((Job)shipment.Job).Charges.Count);
			jobCharge = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.ManualJobAccrualChargeCode, TestObjectCreator.AALSHI, 99M, true);
			Factory.Save();
			AssertEquals("Shipment should have 1 charge", 1, ((Job)shipment.Job).Charges.Count);
			((Job)shipment.Job).Charges[0].JR_OSSellAmt = 100m;
			Factory.Save();
			AssertEquals("JR_OSSellAmt is as was set for MJA job charge", 100M, ((Job)shipment.Job).Charges[0].JR_OSSellAmt);
			AssertEquals("JR_OSCostAmt is as was set for MJA job charge", 99m, ((Job)shipment.Job).Charges[0].JR_OSCostAmt);
			calculationStrategy = new JobConsolCost.ConsolCostCalculationStrategy(jobCharge);
			calculationStrategy.HandleDelete();
			AssertEquals("Shipment should have 1 charge", 1, ((Job)shipment.Job).Charges.Count);
			AssertEquals("JR_OSSellAmt is as was set for MJA job charge", 100m, ((Job)shipment.Job).Charges[0].JR_OSSellAmt);
			AssertEquals("JR_OSCostAmt is zero for MJA job charge", 0m, ((Job)shipment.Job).Charges[0].JR_OSCostAmt);
		}

		public void TestDeleteWhenCostPostedIncorrectly_JobConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment1 = TestObjectCreator.CreateShipment("S001010", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S001011", consol);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 99M, true);
			Factory.Save();
			AssertEquals("Shipment 1 should have 1 charge", 1, ((Job)shipment1.Job).Charges.Count);
			AssertEquals("Shipment 2 should have 1 charge", 1, ((Job)shipment2.Job).Charges.Count);
			JobConsolCostCollection consolCosts = new JobConsolCostCollection(Factory, consol);
			consolCosts.Load();
			AssertEquals("consolCosts.Count", 1, consolCosts.Count);
			cost = consolCosts[0];
			//Partial Posting
			//One ApportionmentCharge is not posted.
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI) as APInvoice;
			invoice.AH_TransactionNum = "INV0001";
			var apline1 = TestObjectCreator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 1", 100m);
			cost.E6_AH_APInvoice = invoice.PK;

			((Job)shipment1.Job).Charges[0].ReverseAccrual(DateTime.Today);
			((Job)shipment1.Job).Charges[0].JR_AL_APLine = apline1.PK;
			Assert(cost.IsPosted);
			Assert(!cost.IsPostedCorrectly);
			AssertEquals("Cost Shouldn't be deleted", false, cost.IsDeleted);
			AssertEquals("Shipment1 should have 1 charge", 1, ((Job)shipment1.Job).Charges.Count);
			AssertEquals("Shipment 1 Charge should have a valid APInvoice", invoice.PK, ((Job)shipment1.Job).Charges[0].APLine.AL_AH);
			AssertEquals("Shipment 2 Charge shouldn't have a valid JR_AL_APLine", ZGuid.Empty, ((Job)shipment2.Job).Charges[0].APLine.AL_AH);
			cost.Delete();
			AssertEquals("Cost Should be deleted", true, cost.IsDeleted);
			//Before deleting a cost all changes made to the apportioned charges will be canceled 
			//Hence value of ((Job)shipment1.Job).Charges[0].IsCostPosted will be rollbacked to false
			//which will trigger the deletion of the charge inside ConsolCostCalculationStrategyWithCalculations.HandleDelete() function.
			AssertEquals("Shipment1 should have no charge", 0, ((Job)shipment1.Job).Charges.Count);
			AssertEquals("Shipment2 shouldn't have any charge", 0, ((Job)shipment2.Job).Charges.Count);
		}

		public void TestDeleteAndChargesWhenCostPostedIncorrectly_JobConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment1 = TestObjectCreator.CreateShipment("S001010", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S001011", consol);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 99M, true);
			Factory.Save();
			AssertEquals("Shipment 1 should have 1 charge", 1, ((Job)shipment1.Job).Charges.Count);
			AssertEquals("Shipment 2 should have 1 charge", 1, ((Job)shipment2.Job).Charges.Count);
			JobConsolCostCollection consolCosts = new JobConsolCostCollection(Factory, consol);
			consolCosts.Load();
			AssertEquals("consolCosts.Count", 1, consolCosts.Count);
			cost = consolCosts[0];
			//Partial Posting
			//One ApportionmentCharge is not posted.
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI) as APInvoice;
			invoice.AH_TransactionNum = "INV0001";
			var apline1 = TestObjectCreator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 1", 100m);
			cost.E6_AH_APInvoice = invoice.PK;

			((Job)shipment1.Job).Charges[0].ReverseAccrual(DateTime.Today);
			((Job)shipment1.Job).Charges[0].JR_AL_APLine = apline1.PK;
			Assert(cost.IsPosted);
			Assert(!cost.IsPostedCorrectly);
			AssertEquals("Cost Shouldn't be deleted", false, cost.IsDeleted);
			AssertEquals("Shipment1 should have 1 charge", 1, ((Job)shipment1.Job).Charges.Count);
			AssertEquals("Shipment 1 Charge should have a valid APInvoice", invoice.PK, ((Job)shipment1.Job).Charges[0].APLine.AL_AH);
			AssertEquals("Shipment 2 Charge shouldn't have a valid JR_AL_APLine", ZGuid.Empty, ((Job)shipment2.Job).Charges[0].APLine.AL_AH);
			AssertNoExceptionThrown(() => cost.DeleteCostAndCharges());
			AssertEquals("Cost Should be deleted", true, cost.IsDeleted);
			AssertEquals("Shipment1 shouldn't have any charge", 0, ((Job)shipment1.Job).Charges.Count);
			AssertEquals("Shipment2 shouldn't have any charge", 0, ((Job)shipment2.Job).Charges.Count);
		}

		public void TestDeleteUnusedJobHeader_JobConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment = TestObjectCreator.CreateShipment("S001010", consol);
			Factory.Save();
			AssertNull("job header is not created", shipment.Job);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			AssertNotNull("job header is created", shipment.Job);

			var calculationStrategy = new JobConsolCost.InvoicingBaseConsolCostCalculationStrategy(consolCost);
			calculationStrategy.UpdateApportionmentChargesListing();
			calculationStrategy.HandleDelete();
			AssertNull("job header is deleted", shipment.Job);
		}

		public void TestSaveUnusedJobInCorrectlyWhenImportJobConsolCost()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment1 = TestObjectCreator.CreateShipment("S001010", consol1);
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000102");
			var shipment2 = TestObjectCreator.CreateShipment("S001011", consol2);
			Factory.Save();
			AssertNull("shipment1 job header is not created", shipment1.Job);
			AssertNull("shipment2 job header is not created", shipment2.Job);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI, ZDateTime.Today);
			var consolCost1 = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol1.PK, JobConsolSchema.Constants.Prefix);
			consolCost1.Delete();

			var consolCost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost2.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost2.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol2.PK, JobConsolSchema.Constants.Prefix);
			consolCost2.E6_OSCostAmount = 100m;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.ImportAllApportionmentsFromCosting();
			Factory.Save();

			AssertNull("shipment1 job header should not be created", shipment1.Job);
			AssertNotNull("shipment2 job header is created", shipment2.Job);
		}

		[TestDate(2020, 2, 10)]
		public void TestSetParentJobUpdatesTaxDateOnConsolCostBasedOnRegistry_UseInvoiceDate()
		{
			AssertSetParentJobUpdatesTaxDateOnConsolCostBasedOnRegistry(TaxDateDefaultingOption.Code.InvoiceDate);
		}

		[TestDate(2020, 2, 10)]
		public void TestSetParentJobUpdatesTaxDateOnConsolCostBasedOnRegistry_UseTodayDate()
		{
			AssertSetParentJobUpdatesTaxDateOnConsolCostBasedOnRegistry(TaxDateDefaultingOption.Code.Today);
		}

		void AssertSetParentJobUpdatesTaxDateOnConsolCostBasedOnRegistry(ZString dateOption)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			Factory.Save();
			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = dateOption;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI, new ZDateTime(2020, 02, 05));
				var consolCost1 = invoice.ConsolCosting.ConsolCosts.AddNew();

				Assert(consolCost1.E6_TaxDate.IsEmpty);
				consolCost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
				if (dateOption == TaxDateDefaultingOption.Code.InvoiceDate)
				{
					AssertEquals("should default to parent invoice's invoice date based on registry setting", new ZDate(2020, 02, 05), consolCost1.E6_TaxDate);
				}
				else if (dateOption == TaxDateDefaultingOption.Code.Today)
				{
					AssertEquals("should default to today's date based on registry setting", ZDate.Today, consolCost1.E6_TaxDate);
				}
				else
				{
					Fail("Invalid tax date option");
				}
			}
		}

		public void TestSaveUsedJobCorrectlyWhenImportJobConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment = TestObjectCreator.CreateShipment("S001010", consol);
			Factory.Save();
			AssertNull("shipment job header is not created", shipment.Job);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI, ZDateTime.Today);
			var consolCost1 = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);

			var consolCost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost2.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost2.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			consolCost2.E6_OSCostAmount = 100m;

			consolCost1.Delete();
			invoice.SubmittedFromInvoicingForm = true;
			invoice.ImportAllApportionmentsFromCosting();
			Factory.Save();

			AssertNotNull("shipment job header should be created", shipment.Job);
		}

		public void TestUpdateApportionmentChargesListingRemovesNonrelevantJobChargesSafely()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S001002");
			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			var shipment3 = TestObjectCreator.CreateShipment("S001003");
			shipment3.JS_JS_ColoadMasterShipment = shipment1.PK;
			new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			AssertNotNull(shipment2.Job);
			Factory.Save();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, 150m, true);
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.UpdateApportionmentChargesListing();
			AssertEquals(3, consolCost.ApportionmentCharges.Count);
			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			consolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			Factory.Save();
			AssertEquals("Shipment should have a charge", 1, ((Job)shipment2.Job).Charges.Count);
			var charge = ((Job)shipment2.Job).Charges[0];
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("10001", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			var arLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, (Job)shipment2.Job, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, "Revenue", 50m);
			charge.ReverseWIP(ZDateTime.Now);
			charge.JR_AL_ARLine = arLine.PK;
			var accrual = charge.APLine;
			AssertNotNull("has Accruaal", accrual);
			AssertEquals("accrual.AL_LineType", TransactionLineTypes.Accrual, accrual.AL_LineType);
			Assert("Accrual should not be reversed", accrual.AL_ReverseDate.IsEmpty);
			// Make second and third charges non-relevant for apportions by excluding their Shipments, so AddShipmentPKsToList() method will remove it 
			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;
			shipment2.JS_IsCancelled = true;
			consolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = false;
			shipment3.JS_IsCancelled = true;
			var calculationStrategy = new JobConsolCost.ConsolCostCalculationStrategy(consolCost);
			int apportionmentListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler((sender, e) =>
			{
				apportionmentListChangedHitCount++;
			}

			);
			((IBindingList)consolCost.ApportionmentCharges).ListChanged += listChangedHandler;
			calculationStrategy.UpdateApportionmentChargesListing();
			AssertEquals("ListChanged on ConsolCost.ApportionmentCharges should be called once", 1, apportionmentListChangedHitCount);
			AssertEquals("Shipment should still have a charge", 1, ((Job)shipment2.Job).Charges.Count);
			charge = ((Job)shipment2.Job).Charges[0];
			AssertEquals("JR_OSCostAmt must be reset", 0m, charge.JR_OSCostAmt);
			AssertEquals("JR_OSSellAmt must be preserved", 50m, charge.JR_OSSellAmt);
			AssertEquals("JR_AL_APLine", ZGuid.Empty, charge.JR_AL_APLine);
			AssertEquals("Accrual must be reversed", false, accrual.AL_ReverseDate.IsEmpty);
			AssertEquals("JR_AL_ARLine should AR Invoice line", arLine.PK, charge.JR_AL_ARLine);
		}

		public void TestUpdateApportionmentChargesListingDoesNotAddDefaultChargesForInactiveJobHeader()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment1 = TestObjectCreator.CreateShipment("S001010", consol);
			var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			var shipment2 = TestObjectCreator.CreateShipment("S001011", consol);
			var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			var shipment3 = TestObjectCreator.CreateShipment("S001012", consol);
			Factory.Save();
			AssertEquals("job header is active", true, shipment1.Job.JH_IsActive);
			AssertEquals("job header is active", true, shipment2.Job.JH_IsActive);
			AssertEquals("job header does not exist", null, shipment3.Job);

			job2.MarkAsInactive();
			Factory.Save();
			AssertEquals("job header is active", true, job1.JH_IsActive);
			AssertEquals("job header is inactive", false, job2.JH_IsActive);
			AssertEquals("job header does not exist", null, shipment3.Job);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			AssertEquals(3, consolCost.ApportionmentCharges.Count);
			AssertEquals("job header is active", true, job1.JH_IsActive);
			AssertEquals("job header is active", true, job2.JH_IsActive);
			AssertEquals("job header is active", true, shipment3.Job.JH_IsActive);

			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;
			consolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = false;
			Factory.Save();
			var calculationStrategy = new JobConsolCost.ConsolCostCalculationStrategy(consolCost);
			calculationStrategy.UpdateApportionmentChargesListing();
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			AssertEquals("Shipment1 has a charge", 1, job1.Charges.Count);
			AssertEquals("Shipment2 has no charge", 0, job2.Charges.Count);
			AssertEquals("job header does not exist", null, shipment3.Job);
			AssertEquals("job header is active", true, job1.JH_IsActive);
			AssertEquals("job header is active", false, job2.JH_IsActive);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestChangingExchangeRateFromZeroDoesNotLeaveErrorOnLocalAmount()
		{
			var consol = TestObjectCreator.CreateConsol("KRSEL", "AUSYD", "C00001000");
			consol.JK_TransportMode = "AIR";
			var shipment = TestObjectCreator.CreateShipment("S00001000", "KRSEL", "AUSYD", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			consolCost.E6_OSCostAmount = 1480.8m;
			consolCost.E6_RX_NKCurrency = "USD";
			AssertEquals("E6_ExchangeRate reset to 0 after changing from Local to Foreighn currency", 0m, consolCost.E6_ExchangeRate);
			AssertHasError(consolCost.E6_ExchangeRateInfo, "Please enter an Exchange Rate.");
			AssertHasError(consolCost.E6_LocalCostAmountInfo, "Please enter a Local Cost Amount.");
			consolCost.E6_ExchangeRate = 1.2m;
			AssertEquals("E6_LocalCostAmount is set", 1234m, consolCost.E6_LocalCostAmount);
			AssertNoErrors("No errors on E6_ExchangeRate", consolCost.E6_ExchangeRateInfo);
			AssertNoErrors("No errors on LocalCostAmount", consolCost.E6_LocalCostAmountInfo);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestChangingCurrencyFromDecimalPlacesToNonDecimalPlacesCurrencyWithNoExchangeRateDoesNotErrorReport()
		{
			ExceptionReporterTestListener.Instance.Clear();
			var consol = TestObjectCreator.CreateConsol("KRSEL", "AUSYD", "C00001000");
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			consolCost.E6_OSCostAmount = 1.88m;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.Code;

			AssertNotEquals("Precondition", TestObjectCreator.TWD.Decimals, TestObjectCreator.AUD.Decimals);
			AssertNotEquals("Precondition", TestObjectCreator.TWD.Decimals, consolCost.E6_OSCostAmount.DecimalPlaces);
			AssertEquals("Precondition", 0m, consolCost.CalculationStrategy.CalculateExchangeRateBasedOnToJobBillingExchangeRateConfig(TestObjectCreator.TWD));

			consolCost.E6_RX_NKCurrency = TestObjectCreator.TWD.Code;

			AssertEquals(TestObjectCreator.TWD.Decimals, consolCost.E6_OSCostAmount.DecimalPlaces);
			AssertEquals(2m, consolCost.E6_OSCostAmount);
			AssertEquals(0m, consolCost.E6_ExchangeRate);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestHandleDelete_InactiveJob()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment = TestObjectCreator.CreateShipment("S001010", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			Factory.Save();
			Assert("job header is active", shipment.Job.JH_IsActive);

			var job = shipment.Job;
			job.MarkAsInactive();
			Factory.Save();
			Assert("job header is inactive", !job.JH_IsActive);

			var calculationStrategy = new JobConsolCost.InvoicingBaseConsolCostCalculationStrategy(consolCost);
			calculationStrategy.UpdateApportionmentChargesListing();
			Assert("Job should has changes", job.HasChanges);
			Assert("job header is active", job.JH_IsActive);

			calculationStrategy.HandleDelete();
			Factory.Save();

			Assert("job header is inactive", !job.JH_IsActive);
		}

		public void TestHandleDelete_InactiveJobHasTransaction()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment = TestObjectCreator.CreateShipment("S001010", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			arInvoice.AH_JH = shipment.Job.PK;
			Factory.Save();
			Assert("job header is active", shipment.Job.JH_IsActive);

			var job = shipment.Job;
			job.MarkAsInactive();
			Factory.Save();
			Assert("job header is inactive", !job.JH_IsActive);

			var calculationStrategy = new JobConsolCost.InvoicingBaseConsolCostCalculationStrategy(consolCost);
			calculationStrategy.UpdateApportionmentChargesListing();
			Assert("Job should has changes", job.HasChanges);
			Assert("job header is active", job.JH_IsActive);

			calculationStrategy.HandleDelete();

			Assert("job header is active", job.JH_IsActive);
			job.Dispose();
		}

		TestObjectCreator TestObjectCreator;
		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		protected override void TearDown()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			base.TearDown();
		}
	}
}
