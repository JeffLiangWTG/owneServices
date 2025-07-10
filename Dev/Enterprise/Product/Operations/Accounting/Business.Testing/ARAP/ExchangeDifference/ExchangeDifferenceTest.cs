using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ExchangeDifference))]
	public abstract class ExchangeDifferenceTest : TransactionHeaderTest
	{
		protected virtual ExchangeDifference ExchangeDifference
		{
			get { return Header as ExchangeDifference; }
		}

		protected override void SetupHeaderForReversing(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			base.SetupHeaderForReversing(aH_OSExTaxAmount, aH_OSTaxAmount);
			Header.AH_LocalOutstandingAmount = Header.AH_LocalExTaxAmount + Header.AH_LocalTaxAmount;
		}

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedAmount) => 0;

		public void TestNumberFountain()
		{
			AssertNotNull("number fountain should not be null", ((ExchangeDifference)GetNewBusinessObject()).NumberFountainForTransactionNumber_ForTestOnly);
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

			ExchangeDifference.AH_LocalExTaxAmount = -45M;
			ExchangeDifference.AH_OSTotalAmount = 0M;
			ExchangeDifference.AH_OutstandingAmount = -45M;
			ExchangeDifference.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				ExchangeDifference.MakeOSOutstandingAmountApplicable(-45);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ExchangeDifference.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", -45m, ExchangeDifference.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now;

			((IMatching)ExchangeDifference).FullyPay(expectedFullyPaidDate);

			Assert("Fully Paid date should not be null", !ExchangeDifference.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Outstanding amount should be 0", 0M, ExchangeDifference.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", 0m, ExchangeDifference.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, ExchangeDifference.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Current paid amount should be -45M", -45M, ExchangeDifference.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
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

			ExchangeDifference.AH_LocalExTaxAmount = 55M;
			ExchangeDifference.AH_OutstandingAmount = 55M;
			ExchangeDifference.AH_OSTotalAmount = 55M;
			ExchangeDifference.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				ExchangeDifference.MakeOSOutstandingAmountApplicable(55m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ExchangeDifference.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 55m, ExchangeDifference.AH_OSOutstandingAmount);
			}

			((IMatching)ExchangeDifference).FullyPay(ZDateTime.Now);
			ExchangeDifference.GenerateMatchLinks();

			TransactionMatchLinkCollection matchLinks = ((IMatching)ExchangeDifference).CurrentMatchGroup;
			AssertEquals("There should be one matchlink for Exchangediff", 1, matchLinks.Count);
			TransactionMatchLink matchLink = matchLinks[0];

			AssertEquals("Matchlink should be for this exchangediff", ExchangeDifference.PK, matchLink.AP_AH);
			AssertEquals("Matchlink amount should be 55", 55M, matchLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? 55m : 0m, matchLink.AP_OSAmount);
		}

		public void TestDescriptionOfUnmatch()
		{
			TestCaseHelper.ClearTable(TransactionMatchLink.Schema.TableName);

			ExchangeDifference.AH_OSExTaxAmount = -10M;
			ExchangeDifference.AH_LocalExTaxAmount = -10M;
			ExchangeDifference.AH_LocalOutstandingAmount = 0M;
			ExchangeDifference.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink exxMatchLink = ((IMatching)ExchangeDifference).CurrentMatchGroup.AddNew();
			exxMatchLink.AP_Amount = 10M;
			exxMatchLink.AP_MatchGroupNum = "M00001002";
			exxMatchLink.AP_AH = ExchangeDifference.PK;

			Receipt receipt;

			if (ExchangeDifference is ARExchangeDifference)
			{
				receipt = Factory.NewWithValidTestData<ARReceipt>();
				receipt.AH_LocalExTaxAmount = 10M;
				receipt.AH_OSTotalAmount = 10M;
				receipt.AH_LocalOutstandingAmount = 0M;
				receipt.AH_FullyPaidDate = ZDateTime.Today;
			}
			else
			{
				receipt = Factory.NewWithValidTestData<APReceipt>();
				receipt.AH_LocalExTaxAmount = 10M;
				receipt.AH_OSTotalAmount = 10M;
				receipt.AH_LocalOutstandingAmount = 0M;
				receipt.AH_FullyPaidDate = ZDateTime.Today;
			}

			TransactionMatchLink recMatchLink = ((IMatching)ExchangeDifference).CurrentMatchGroup.AddNew();
			recMatchLink.AP_Amount = -10M;
			recMatchLink.AP_MatchGroupNum = "M00001002";
			recMatchLink.AP_AH = receipt.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(ExchangeDifference);
			Factory.Save();

			exxMatchLink.Unmatch();

			var iRevEXX = (ExchangeDifference)ExchangeDifference.ReverseTransaction;
			AssertEquals($"EXCHANGE DIFFERENCE RELATING TO UN-MATCH NO. {exxMatchLink.AP_MatchGroupNum} [{receipt.AH_TransactionType}:{receipt.AH_TransactionNum}]", iRevEXX.AH_Desc);
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

			ExchangeDifference.AH_OSExTaxAmount = 10M;
			ExchangeDifference.AH_LocalExTaxAmount = 10M; // actually -10 in DB
			ExchangeDifference.AH_LocalOutstandingAmount = 0M;
			ExchangeDifference.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink eXXMatchLink = ((IMatching)ExchangeDifference).CurrentMatchGroup.AddNew();
			eXXMatchLink.AP_Amount = -10M;
			eXXMatchLink.AP_MatchGroupNum = "M00001002";
			eXXMatchLink.AP_AH = ExchangeDifference.PK;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = 10M;

			TransactionMatchLink link = ((IMatching)ExchangeDifference).CurrentMatchGroup.AddNew();
			link.AP_Amount = 10M;
			link.AP_MatchGroupNum = "M00001002";
			link.AP_AH = headerToMatch.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(ExchangeDifference);

			if (isEnableNewOSOutstandingAmountFeature)
			{
				ExchangeDifference.MakeOSOutstandingAmountApplicable(0m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ExchangeDifference.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, ExchangeDifference.AH_OSOutstandingAmount);

				eXXMatchLink.AP_OSAmount = -10m;

				AssertEquals("PreCondition - AP_OSAmount", -10m, eXXMatchLink.AP_OSAmount);
			}

			Factory.Save();

			((IMatching)ExchangeDifference).CurrentMatchGroup.RemoveAll();

			eXXMatchLink.Unmatch();
			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			((IMatching)ExchangeDifference).ChangeUnmatchDate(expectedPostDate);

			eXXMatchLink.Delete();
			link.Delete();

			Factory.Save();

			TransactionHeader loadedEXX = Factory.Load<TransactionHeader>(ExchangeDifference.PK);

			Assert("LoadedEXX should be an ExchangeDifference", loadedEXX is ExchangeDifference);
			Assert("Current EXX should be cancelled", loadedEXX.AH_IsCancelled);
			AssertEquals("Current EXX should have OutstandingAmount = 0", 0M, loadedEXX.AH_OutstandingAmount);
			AssertEquals("Current EXX AH_OSOutstandingAmount", 0M, loadedEXX.AH_OSOutstandingAmount);
			AssertEquals("Current EXX AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, loadedEXX.AH_IsOSOutstandingAmountApplicable);
			Assert("Current EXX should be FullyPaid", !loadedEXX.AH_FullyPaidDate.IsEmpty);
			Assert("LoadedEXX should have empty TransactionBelongsToGroup", loadedEXX.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("PostDate should be changed", ZDateTime.Today, loadedEXX.AH_PostDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, loadedEXX.AH_FullyPaidDate.Date);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("Should be 2 matchlinks", 2, matchLinks.Count);

			IReversing iRevEXX = ((IReversing)ExchangeDifference).ReverseTransaction;
			Assert("Should be ExchangeDifference", iRevEXX is ExchangeDifference);
			ExchangeDifference revEXX = (ExchangeDifference)iRevEXX;
			AssertEquals("Reversing EXX Should have InvoiceAmount = 10", 10M, revEXX.AH_InvoiceAmount);
			AssertEquals("Reversing EXX should have OutstandingAmount = 0", 0M, revEXX.AH_OutstandingAmount);
			AssertEquals("Reversing EXX AH_OSOutstandingAmount", 0M, revEXX.AH_OSOutstandingAmount);
			AssertEquals("Reversing EXX AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, revEXX.AH_IsOSOutstandingAmountApplicable);
			Assert("Reversing EXX should be cancelled", revEXX.AH_IsCancelled);
			Assert("Reversing EXX should be fullypaid", !revEXX.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Reversing EXX should have TransactionBelongsToGroup = PK of LoadedEXX",
				loadedEXX.PK, revEXX.AH_TransactionBelongsToGroup);
			AssertContains("Description", "EXCHANGE DIFFERENCE RELATING TO UN-MATCH NO", revEXX.AH_Desc);
			AssertEquals("PostDate should be changed", expectedPostDate, revEXX.AH_PostDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revEXX.AH_FullyPaidDate.Date);

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revEXX.PK);
			TransactionMatchLink revEXXLink = Factory.LoadTop1<TransactionMatchLink>(filter);
			AssertNotNull("There should be a matchlink for RevEXX", revEXXLink);
			AssertEquals("Match Group should be M00001000", "M00001000", revEXXLink.AP_MatchGroupNum);
			AssertEquals("Amount should be 10", 10M, revEXXLink.AP_Amount);
			Assert("MatchDate should not be null", !revEXXLink.AP_MatchDate.IsEmpty);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? 10M : 0m, revEXXLink.AP_OSAmount);
			AssertEquals("MatchDate should be as changed post date", expectedPostDate, revEXXLink.AP_MatchDate);
		}

		public void TestLocalForeignDataEntry()
		{
			ExchangeDifference.AH_ExchangeRate = 10M;
			ExchangeDifference.BindableOSAmount = 900m;
			AssertEquals("OSTotal should recalculate to 300", 90M, ExchangeDifference.BindableInvoiceAmount);

			ExchangeDifference.BindableInvoiceAmount = 100M;
			AssertEquals("OSTotal should recalculate to 100", 9M, ExchangeDifference.AH_ExchangeRate);

			ExchangeDifference.AH_ExchangeRate = 2M;
			AssertEquals("OSTotal should recalculate to 300", 450M, ExchangeDifference.BindableInvoiceAmount);
		}

		public void TestAmountsTogether()
		{
			ExchangeDifference.BindableOSAmount = 0;
			ExchangeDifference.AH_ExchangeRate = 1;

			AssertEquals("BindableInvoiceAmount should be 0", 0m, ExchangeDifference.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 0", 0m, ExchangeDifference.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, ExchangeDifference.AH_ExchangeRate);

			ExchangeDifference.BindableOSAmount = 1.5;
			AssertEquals("BindableInvoiceAmount should be 1.5", 1.5m, ExchangeDifference.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 1.5", 1.5m, ExchangeDifference.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, ExchangeDifference.AH_ExchangeRate);

			ExchangeDifference.BindableOSAmount = 2;
			AssertEquals("BindableInvoiceAmount should be 2", 2m, ExchangeDifference.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, ExchangeDifference.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1m, ExchangeDifference.AH_ExchangeRate);

			ExchangeDifference.BindableInvoiceAmount = 4;
			AssertEquals("BindableInvoiceAmount should be 4", 4m, ExchangeDifference.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, ExchangeDifference.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 0.5", 0.5m, ExchangeDifference.AH_ExchangeRate);

			ExchangeDifference.AH_ExchangeRate = 3;
			AssertEquals("BindableInvoiceAmount should be 0.67", 0.67m, ExchangeDifference.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 2", 2m, ExchangeDifference.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 3", 3m, ExchangeDifference.AH_ExchangeRate);

			ExchangeDifference.BindableInvoiceAmount = 0;
			ExchangeDifference.AH_ExchangeRate = 0;
			ExchangeDifference.BindableOSAmount = 0;

			ExchangeDifference.AH_ExchangeRate = 2;
			ExchangeDifference.BindableOSAmount = 10;
			AssertEquals("BindableInvoiceAmount should be 5", 5m, ExchangeDifference.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 10", 10m, ExchangeDifference.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 2", 2m, ExchangeDifference.AH_ExchangeRate);

			ExchangeDifference.BindableOSAmount = 5;
			AssertEquals("BindableInvoiceAmount should be 2.5", 2.5m, ExchangeDifference.BindableInvoiceAmount);
			AssertEquals("BindableOSAmount should be 5", 5m, ExchangeDifference.BindableOSAmount);
			AssertEquals("AH_ExchangeRate should be 2", 2m, ExchangeDifference.AH_ExchangeRate);
		}

		// required because of LocalForeignDataEntry
		public override void TestAH_OSOutstandingAmount()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 0); //there is no point of setting non zero tax amount here as exchange rate does not have VAT taxes.
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
			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			IMatching exchangeDifferenceAsIMatching = (IMatching)Header;
			exchangeDifferenceAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);

			AssertEquals("Payment Approval Items", 0, exchangeDifferenceAsIMatching.PaymentApprovalItems.Count);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			AssertEquals("GL Account must be set by default.", AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value, ExchangeDifference.AH_AG);
		}

		public void TestAH_AGReadOnly()
		{
			Assert("AH_AG must be readonly.", ExchangeDifference.AH_AGInfo.ReadOnly);
		}

		public void TestAH_AG()
		{
			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());

			ExchangeDifference.BindableOSAmount = -100;
			AssertEquals(AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount.Value, ExchangeDifference.AH_AG);

			ExchangeDifference.BindableOSAmount = 100;
			AssertEquals(AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value, ExchangeDifference.AH_AG);

			ExchangeDifference.BindableOSAmount = 0;
			AssertEquals(AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value, ExchangeDifference.AH_AG);
		}

		public void TestCheckpointToUnmatch()
		{
			AssertNull("No checkpoint is required to unmatch exchange difference", ((IMiscellaneousTransaction)GetNewBusinessObject()).CheckpointForUnmatch);
		}
	}
}
