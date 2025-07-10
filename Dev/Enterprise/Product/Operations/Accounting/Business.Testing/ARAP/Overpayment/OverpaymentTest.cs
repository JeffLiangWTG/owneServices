using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	[TestedType(typeof(Overpayment))]
	public abstract class OverpaymentTest : TransactionHeaderTest
	{
		protected virtual Overpayment Overpayment
		{
			get { return Header as Overpayment; }
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(OverpaymentValidation); }
		}

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedAmount) => 0;

		public void TestNumberFountain()
		{
			AssertNotNull("number fountain should not be null", ((Overpayment)GetNewBusinessObject()).NumberFountainForTransactionNumber_ForTestOnly);
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

			Overpayment.AH_LocalExTaxAmount = 60M;
			Overpayment.AH_OSTotalAmount = 60M;
			Overpayment.AH_OutstandingAmount = 60M;
			Overpayment.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Overpayment.MakeOSOutstandingAmountApplicable(60m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Overpayment.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 60m, Overpayment.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now;

			((IMatching)Overpayment).FullyPay(expectedFullyPaidDate);

			Assert("Fully paid date should not be empty", !Overpayment.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Outstanding amount should be zero", 0M, Overpayment.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", 0m, Overpayment.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, Overpayment.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Current amount should be set", 60M, Overpayment.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
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

			Overpayment.AH_LocalExTaxAmount = 70M;
			Overpayment.AH_OSTotalAmount = 70M;
			Overpayment.AH_OutstandingAmount = 70M;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Overpayment.MakeOSOutstandingAmountApplicable(70m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Overpayment.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 70m, Overpayment.AH_OSOutstandingAmount);
			}

			((IMatching)Overpayment).FullyPay(ZDateTime.Now);
			Overpayment.GenerateMatchLinks();

			TransactionMatchLinkCollection matchLinks = ((IMatching)Overpayment).CurrentMatchGroup;

			AssertEquals("There should be 1 matchlink created", 1, matchLinks.Count);
			TransactionMatchLink matchLink = matchLinks[0];
			AssertEquals("Matchlink should be for Overpayment", Overpayment.PK, matchLink.AP_AH);
			AssertEquals("Matchlink amount should be 70", 70M, matchLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? 70m : 0m, matchLink.AP_OSAmount);
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

			Overpayment.AH_OSExTaxAmount = 19M;
			Overpayment.AH_LocalExTaxAmount = 19M; // actually 19 in DB
			Overpayment.AH_LocalOutstandingAmount = 0M;
			Overpayment.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink oVPMatchLink = ((IMatching)Overpayment).CurrentMatchGroup.AddNew();
			oVPMatchLink.AP_Amount = 19M;
			oVPMatchLink.AP_MatchGroupNum = "M00001022";
			oVPMatchLink.AP_AH = Overpayment.PK;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -19M;

			TransactionMatchLink link = ((IMatching)Overpayment).CurrentMatchGroup.AddNew();
			link.AP_Amount = -19M;
			link.AP_MatchGroupNum = "M00001002";
			link.AP_AH = headerToMatch.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(Overpayment);

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Overpayment.MakeOSOutstandingAmountApplicable(0m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Overpayment.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, Overpayment.AH_OSOutstandingAmount);

				oVPMatchLink.AP_OSAmount = 19m;

				AssertEquals("PreCondition - AP_OSAmount", 19m, oVPMatchLink.AP_OSAmount);
			}

			Factory.Save();

			((IMatching)Overpayment).CurrentMatchGroup.RemoveAll();

			oVPMatchLink.Unmatch();
			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			((IMatching)Overpayment).ChangeUnmatchDate(expectedPostDate);

			oVPMatchLink.Delete();
			link.Delete();

			Factory.Save();

			TransactionHeader loadedOVP = Factory.Load<TransactionHeader>(Overpayment.PK);
			Assert("LoadedOVP should be an Overpayment", loadedOVP is Overpayment);
			Assert("Current OVP should be cancelled", loadedOVP.AH_IsCancelled);
			AssertEquals("Current OVP should have OutstandingAmount = 0", 0M, loadedOVP.AH_OutstandingAmount);
			AssertEquals("Current OVP AH_OSOutstandingAmount", 0M, loadedOVP.AH_OSOutstandingAmount);
			AssertEquals("Current OVP AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, loadedOVP.AH_IsOSOutstandingAmountApplicable);
			Assert("Current OVP should be FullyPaid", !loadedOVP.AH_FullyPaidDate.IsEmpty);
			Assert("TransactionBelongsToGroup should be empty on LoadedOVP", loadedOVP.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("PostDate should be changed", ZDateTime.Today, loadedOVP.AH_PostDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, loadedOVP.AH_FullyPaidDate.Date);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("Should be 2 matchlinks", 2, matchLinks.Count);

			IReversing iRevOVP = ((IReversing)Overpayment).ReverseTransaction;
			Assert("Should be Overpayment", iRevOVP is Overpayment);
			Overpayment revOVP = (Overpayment)iRevOVP;
			AssertEquals("Reversing OVP Should have InvoiceAmount = -19", -19M, revOVP.AH_InvoiceAmount);
			AssertEquals("Reversing OVP should have OutstandingAmount = 0", 0M, revOVP.AH_OutstandingAmount);
			AssertEquals("Reversing OVP AH_OSOutstandingAmount", 0M, revOVP.AH_OSOutstandingAmount);
			AssertEquals("Reversing OVP AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, revOVP.AH_IsOSOutstandingAmountApplicable);
			Assert("Reversing OVP should be cancelled", revOVP.AH_IsCancelled);
			Assert("Reversing OVP should be fullypaid", !revOVP.AH_FullyPaidDate.IsEmpty);
			AssertEquals("TransactionBelongsToGroup on RevOVP should = PK of LoadedOVP",
				loadedOVP.PK, revOVP.AH_TransactionBelongsToGroup);
			AssertEquals("PostDate should be changed", expectedPostDate, revOVP.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revOVP.AH_FullyPaidDate.Date);

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revOVP.PK);
			TransactionMatchLink revOVPLink = Factory.LoadTop1<TransactionMatchLink>(filter);
			AssertNotNull("There should be a matchlink for RevOVP", revOVPLink);
			AssertEquals("Match Group should be M00001000", "M00001000", revOVPLink.AP_MatchGroupNum);
			AssertEquals("Amount should be -19", -19M, revOVPLink.AP_Amount);
			Assert("MatchDate should not be null", !revOVPLink.AP_MatchDate.IsEmpty);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? -19M : 0m, revOVPLink.AP_OSAmount);
			AssertEquals("MatchDate should be as changed post date", expectedPostDate, revOVPLink.AP_MatchDate);
		}

		public void TestLocalForeignDataEntry()
		{
			Overpayment.AH_ExchangeRate = 10M;
			Overpayment.BindableOSAmount = 900m;
			AssertEquals("OSTotal should recalculate to 300", 90M, Overpayment.BindableInvoiceAmount);

			Overpayment.BindableInvoiceAmount = 100M;
			AssertEquals("OSTotal should recalculate to 100", 9M, Overpayment.AH_ExchangeRate);

			Overpayment.AH_ExchangeRate = 2M;
			AssertEquals("OSTotal should recalculate to 300", 450M, Overpayment.BindableInvoiceAmount);
		}

		public void TestAmountsTogether()
		{
			Overpayment.BindableOSAmount = 0;
			Overpayment.AH_ExchangeRate = 1;

			AssertEquals("BindableInvoiceAmount should be 0", 0m, Overpayment.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 0", 0m, Overpayment.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, Overpayment.AH_ExchangeRate);

			Overpayment.BindableOSAmount = 1.5;
			AssertEquals("BindableInvoiceAmount should be 1.5", 1.5m, Overpayment.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 1.5", 1.5m, Overpayment.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, Overpayment.AH_ExchangeRate);

			Overpayment.BindableOSAmount = 2;
			AssertEquals("BindableInvoiceAmount should be 2", 2m, Overpayment.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, Overpayment.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, Overpayment.AH_ExchangeRate);

			Overpayment.BindableInvoiceAmount = 4;
			AssertEquals("BindableInvoiceAmount should be 4", 4m, Overpayment.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, Overpayment.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 0.5", 0.5m, Overpayment.AH_ExchangeRate);

			Overpayment.AH_ExchangeRate = 3;
			AssertEquals("BindableInvoiceAmount should be 0.67", 0.67m, Overpayment.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, Overpayment.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 3", 3m, Overpayment.AH_ExchangeRate);

			Overpayment.BindableInvoiceAmount = 0;
			Overpayment.AH_ExchangeRate = 0;
			Overpayment.BindableOSAmount = 0;

			Overpayment.AH_ExchangeRate = 2;
			Overpayment.BindableOSAmount = 10;
			AssertEquals("BindableInvoiceAmount should be 5", 5m, Overpayment.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 10", 10m, Overpayment.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 2", 2m, Overpayment.AH_ExchangeRate);

			Overpayment.BindableOSAmount = 5;
			AssertEquals("BindableInvoiceAmount should be 2.5", 2.5m, Overpayment.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 5", 5m, Overpayment.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 2", 2m, Overpayment.AH_ExchangeRate);
		}

		// required because of LocalForeignDataEntry
		public override void TestAH_OSOutstandingAmount()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 0); //there is no point of setting non zero tax amount here as overpayment does not have VAT taxes.
			Header.AH_LocalOutstandingAmount = 378.02m;
			AssertEquals("OS Outstanding Amount", 215.473m, Header.AH_Calc_OSOutstandingAmount);

			Header.AH_LocalOutstandingAmount = 110.540m;
			AssertEquals("OS Outstanding Amount with decimal not matching local total (i.e. invoiceamount+gstamount)",
				63.008m, Header.AH_Calc_OSOutstandingAmount);

			Header.AH_ExchangeRate = 0.123456m;
			// Setting ExchangeRate causes LocalForeignDataEntry 
			// to recalculate Foreign which resets outstanding amt to
			// InvoiceAmount + GSTAmount, so OSOutstandingAmount is 
			// just OSTotalAmount
			AssertEquals("OS Outstanding Amount with decimal and very small Exchange Rate", 200.453m, Header.AH_Calc_OSOutstandingAmount);
		}

		public void TestBindableInvoiceAmountValidation()
		{
			Overpayment.BindableInvoiceAmount = -90M;
			Assert("BindableInvoiceAmount has errors", Overpayment.BindableInvoiceAmountInfo.HasErrors());
			Overpayment.BindableInvoiceAmount = 90M;
			Assert("BindableInvoiceAmount should not have errors", !Overpayment.BindableInvoiceAmountInfo.HasErrors());
		}

		public void TestGeneratePaymentApprovalItems()
		{
			IMatching overpaymentAsIMatching = Overpayment;

			Overpayment.AH_InvoiceAmount = 548.72M;
			Overpayment.AH_ExchangeRate = 0.6873M;
			Overpayment.AH_InvoiceAmount = 548.72M; // set here because ExchangeRate recalculates OSExTaxAmount
			Overpayment.AH_OutstandingAmount = 548.72M;
			Overpayment.AH_OSTotal = 377.13M;

			overpaymentAsIMatching.OSPartialPaymentAmount = 150M;

			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			overpaymentAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);

			AssertEquals("Payment Approval Items", 0, overpaymentAsIMatching.PaymentApprovalItems.Count);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			AssertEquals("GL Account must be set by default.", AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value, Overpayment.AH_AG);
		}

		public void TestAH_AGReadOnly()
		{
			Assert("AH_AG must be readonly.", Overpayment.AH_AGInfo.ReadOnly);
		}
	}
}
