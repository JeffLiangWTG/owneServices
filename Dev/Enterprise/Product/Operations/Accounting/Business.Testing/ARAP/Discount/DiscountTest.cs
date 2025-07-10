using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(Discount))]
	public abstract class DiscountTest : TransactionHeaderTest
	{
		protected virtual Discount Discount
		{
			get { return Header as Discount; }
		}

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedAmount) => 0;

		public void TestNumberFountain()
		{
			AssertNotNull("number fountain should not be null", ((Discount)GetNewBusinessObject()).NumberFountainForTransactionNumber_ForTestOnly);
		}

		public void TestSettingInvoiceAmountSetsOutstandingAmount()
		{
			Discount.AH_OSExTaxAmount = 30M;
			Discount.BindableInvoiceAmount = 30M;
			AssertEquals("OSOutstanding should be 30", 30M, ((IMatching)Discount).OSOutstandingAmount);
			AssertEquals("OSPartialPaymentAmount should be 30 since partial payment is not allowed", 30M, ((IMatching)Discount).OSPartialPaymentAmount);
			AssertEquals("LocalPartialPaymentAmount should be 30 since partial payment is not allowed", 30M, ((IMatching)Discount).LocalPartialPaymentAmount);
		}

		public void TestFullyPay()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestFullyPay_EnableNewOSOutstandingAmountFeature()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertFullyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			Discount.AH_LocalExTaxAmount = -30M;
			Discount.AH_OutstandingAmount = -30M;
			Discount.AH_OSTotalAmount = -30M;
			Discount.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Discount.MakeOSOutstandingAmountApplicable(-30m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Discount.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", -30m, Discount.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now;

			((IMatching)Discount).FullyPay(expectedFullyPaidDate);

			Assert("Fully paid date should not be empty", !Discount.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Outstanding amount should be zero", 0M, Discount.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", 0m, Discount.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, Discount.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Current amount should be set", -30M, Discount.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
		}

		public void TestGenerateMatchLinks()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestGenerateMatchLinks_EnableNewOSOutstandingAmountFeature()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertGenerateMatchLinks(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			Discount.AH_LocalExTaxAmount = -30M;
			Discount.AH_OutstandingAmount = -30M;
			Discount.AH_OSTotalAmount = -30M;
			Discount.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Discount.MakeOSOutstandingAmountApplicable(-30m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Discount.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", -30m, Discount.AH_OSOutstandingAmount);
			}

			((IMatching)Discount).FullyPay(ZDateTime.Now);
			Discount.GenerateMatchLinks();

			TransactionMatchLinkCollection matchLinks = ((IMatching)Discount).CurrentMatchGroup;
			TransactionMatchLink matchLink = matchLinks[0];

			AssertEquals("Matchlink should be for Discount", Discount.PK, matchLink.AP_AH);
			AssertEquals("Matchlink amount should be -30", -30M, matchLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? -30m : 0m, matchLink.AP_OSAmount);
		}

		public void TestUnmatch()
		{
			AssertUnmatch(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatch_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatch(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatch(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestCaseHelper.ClearTable(TransactionMatchLink.Schema.TableName);
			Discount.AH_OSExTaxAmount = 10M;
			Discount.AH_LocalExTaxAmount = 10M; // actually 10 in DB
			Discount.AH_LocalOutstandingAmount = 0M;
			Discount.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink dSCMatchLink = ((IMatching)Discount).CurrentMatchGroup.AddNew();
			dSCMatchLink.AP_Amount = 10M;
			dSCMatchLink.AP_MatchGroupNum = "M00001002";
			dSCMatchLink.AP_AH = Discount.PK;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -10M;

			TransactionMatchLink link = ((IMatching)Discount).CurrentMatchGroup.AddNew();
			link.AP_Amount = -10M;
			link.AP_MatchGroupNum = "M00001002";
			link.AP_AH = headerToMatch.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(Discount);

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Discount.MakeOSOutstandingAmountApplicable(0m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Discount.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, Discount.AH_OSOutstandingAmount);

				dSCMatchLink.AP_OSAmount = 10m;

				AssertEquals("PreCondition - AP_OSAmount", 10m, dSCMatchLink.AP_OSAmount);
			}

			Factory.Save();
			((IMatching)Discount).CurrentMatchGroup.RemoveAll();

			dSCMatchLink.Unmatch();
			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			((IMatching)Discount).ChangeUnmatchDate(expectedPostDate);

			dSCMatchLink.Delete();
			link.Delete();

			Factory.Save();

			TransactionHeader loadedDSC = Factory.Load(typeof(TransactionHeader), Discount.PK) as Discount;
			Assert("The loadedDSC should be a discount", loadedDSC is Discount);
			Assert("Current DSC should be cancelled", loadedDSC.AH_IsCancelled);
			AssertEquals("Current DSC should have OutstandingAmount = 0", 0M, loadedDSC.AH_OutstandingAmount);
			AssertEquals("Current DSC AH_OSOutstandingAmount", 0M, loadedDSC.AH_OSOutstandingAmount);
			AssertEquals("Current DSC AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, loadedDSC.AH_IsOSOutstandingAmountApplicable);
			Assert("Current DSC should be FullyPaid", !loadedDSC.AH_FullyPaidDate.IsEmpty);
			Assert("TransactionBelongsToGroup should be empty", loadedDSC.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("PostDate should be changed", ZDateTime.Today, loadedDSC.AH_PostDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, loadedDSC.AH_FullyPaidDate.Date);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("Should be 2 matchlinks", 2, matchLinks.Count);

			IReversing iRevDSC = ((IReversing)Discount).ReverseTransaction;
			Assert("Should be Discount", iRevDSC is Discount);
			Discount revDSC = (Discount)iRevDSC;
			AssertEquals("Reversing DSC Should have InvoiceAmount = -10", -10M, revDSC.AH_InvoiceAmount);
			AssertEquals("Reversing DSC should have OutstandingAmount = 0", 0M, revDSC.AH_OutstandingAmount);
			AssertEquals("Reversing DSC AH_OSOutstandingAmount", 0M, revDSC.AH_OSOutstandingAmount);
			AssertEquals("Reversing DSC AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, revDSC.AH_IsOSOutstandingAmountApplicable);
			Assert("Reversing DSC should be cancelled", revDSC.AH_IsCancelled);
			Assert("Reversing DSC should be fullypaid", !revDSC.AH_FullyPaidDate.IsEmpty);
			Assert("TransactionBelongsToGroup should not be empty", !revDSC.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("ReversingDSC should have TransactionBelongsToGroup as PK of LoadedDSC",
				loadedDSC.PK, revDSC.AH_TransactionBelongsToGroup);
			AssertEquals("PostDate should be changed", expectedPostDate, revDSC.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revDSC.AH_FullyPaidDate.Date);

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revDSC.PK);
			TransactionMatchLink revDSCLink = Factory.LoadTop1<TransactionMatchLink>(filter);
			AssertNotNull("There should be a matchlink for RevDSC", revDSCLink);
			AssertEquals("Match Group should be M00001000", "M00001000", revDSCLink.AP_MatchGroupNum);
			AssertEquals("Amount should be -10", -10M, revDSCLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? -10M : 0m, revDSCLink.AP_OSAmount);
			AssertEquals("MatchDate should be as changed post date", expectedPostDate, revDSCLink.AP_MatchDate);
		}

		public void TestLocalForeignDataEntry()
		{
			Discount.AH_ExchangeRate = 10M;
			Discount.BindableOSAmount = 900m;
			AssertEquals("OSTotal should recalculate to 300", 90M, Discount.BindableInvoiceAmount);

			Discount.BindableInvoiceAmount = 100M;
			AssertEquals("OSTotal should recalculate to 100", 9M, Discount.AH_ExchangeRate);

			Discount.AH_ExchangeRate = 2M;
			AssertEquals("OSTotal should recalculate to 300", 450M, Discount.BindableInvoiceAmount);
		}

		public void TestAmountsTogether()
		{
			Discount.BindableOSAmount = 0;
			Discount.AH_ExchangeRate = 1;

			AssertEquals("BindableInvoiceAmount should be 0", 0m, Discount.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 0", 0m, Discount.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, Discount.AH_ExchangeRate);

			Discount.BindableOSAmount = 1.5;
			AssertEquals("BindableInvoiceAmount should be 1.5", 1.5m, Discount.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 1.5", 1.5m, Discount.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, Discount.AH_ExchangeRate);

			Discount.BindableOSAmount = 2;
			AssertEquals("BindableInvoiceAmount should be 2", 2m, Discount.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, Discount.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, Discount.AH_ExchangeRate);

			Discount.BindableInvoiceAmount = 4;
			AssertEquals("BindableInvoiceAmount should be 4", 4m, Discount.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, Discount.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 0.5", 0.5m, Discount.AH_ExchangeRate);

			Discount.AH_ExchangeRate = 3;
			AssertEquals("BindableInvoiceAmount should be 0.67", 0.67m, Discount.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, Discount.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 3", 3m, Discount.AH_ExchangeRate);

			Discount.BindableInvoiceAmount = 0;
			Discount.AH_ExchangeRate = 0;
			Discount.BindableOSAmount = 0;

			Discount.AH_ExchangeRate = 2;
			Discount.BindableOSAmount = 10;
			AssertEquals("BindableInvoiceAmount should be 5", 5m, Discount.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 10", 10m, Discount.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 2", 2m, Discount.AH_ExchangeRate);

			Discount.BindableOSAmount = 5;
			AssertEquals("BindableInvoiceAmount should be 2.5", 2.5m, Discount.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 5", 5m, Discount.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 2", 2m, Discount.AH_ExchangeRate);
		}

		// required because of LocalForeignDataEntry
		public override void TestAH_OSOutstandingAmount()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 0m); //there is no point of setting non zero tax amount here as discount does not have VAT taxes.
			Header.AH_LocalOutstandingAmount = 378.02m;
			AssertEquals("OS Outstanding Amount", 215.473m, Header.AH_Calc_OSOutstandingAmount);

			Header.AH_LocalOutstandingAmount = 110.540m;
			AssertEquals("OS Outstanding Amount with decimal not matching local total (i.e. invoiceamount+gstamount)",
				63.008m, Header.AH_Calc_OSOutstandingAmount);

			Header.AH_ExchangeRate = 0.123456m;
			AssertEquals("OS Outstanding Amount with decimal and very small Exchange Rate", 200.453m, Header.AH_Calc_OSOutstandingAmount);
		}

		public void TestGeneratePaymentApprovalItems()
		{
			// This will only be called for manually created contras
			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			IMatching discountAsIMatching = (IMatching)Header;
			discountAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);

			AssertEquals("Payment Approval Items", 0, discountAsIMatching.PaymentApprovalItems.Count);
		}

		public void TestAH_AGReadOnly()
		{
			Assert("AH_AG must be readonly.", Discount.AH_AGInfo.ReadOnly);
		}

		public void TestCheckpointToUnmatch()
		{
			AssertNull("No checkpoint is required to unmatch discount", ((IMiscellaneousTransaction)GetNewBusinessObject()).CheckpointForUnmatch);
		}
	}
}
