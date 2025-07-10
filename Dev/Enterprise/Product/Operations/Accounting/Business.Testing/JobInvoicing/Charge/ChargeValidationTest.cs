using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ChargeValidationTest : ChargeWithCostValidationTest
	{
		public void TestException_WhenClosedJobReopenerIsNull()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			var testCharge = testJob.Charges.AddNew();

			AssertNoExceptionThrown(() => new ChargeValidation(testCharge, null));
		}

		#region Charge Group Validations

		public void TestMultipleGatewayChargesUsingSameChargeCode_AddingNewChargeInvalidatesCachedValidationResult()
		{
			var expectedError = "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", sendingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
			});

			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		public void TestMultipleGatewayChargesUsingSameChargeCode_RemovingChargeInvalidatesCachedValidationResult()
		{
			var expectedError = "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", sendingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			charge2.Delete();
			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
			});
		}

		public void TestMultipleGatewayChargesUsingSameChargeCode_ChangingChargeCodeInvalidatesCachedValidationResult()
		{
			var expectedError = "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", sendingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			charge2.JR_AC = TestObjectCreator.CC2.PK;
			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
				AssertNoWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		public void TestMultipleGatewayChargesUsingSameChargeCode_ChangingDebtorInvalidatesCachedValidationResult()
		{
			var expectedError = "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", sendingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
				AssertNoWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		public void TestMultipleGatewayChargesUsingSameChargeCode_ChangingLocalSellAmountInvalidatesCachedValidationResult()
		{
			var expectedError = "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", sendingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_Calc_RelatedJobNumber = "S00001";
			charge2.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			charge2.JR_LocalSellAmt = 0m;
			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
				AssertNoWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		#endregion

		public void TestDecreaseChargeAmtWithoutARCashAdvanceRequest()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111109";
			var testCharge = testJob.Charges.AddNew();
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				testCharge.JR_OSSellAmt = 100m;
				testCharge.JR_IsARCashAdvance = true;
				AssertNoErrors("Should not have errors", testCharge.JR_IsARCashAdvanceInfo);
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
				testCharge.JR_OSSellAmt = 90m;
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
				testCharge.JR_OSSellAmt = 110m;
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
			}
		}
		public void TestChangeChargeAmtWithRequestedARCashAdvanceRequest()
		{
			AssertChangeChargeAmtWithARCashAdvanceRequest(CashAdvanceStatusCodes.RequestHeader.Requested);
		}

		public void TestChangeChargeAmtWithPaidARCashAdvanceRequest()
		{
			AssertChangeChargeAmtWithARCashAdvanceRequest(CashAdvanceStatusCodes.RequestHeader.Paid);
		}

		public void TestDecreaseChargeAmtWithCancelledARCashAdvanceRequest()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111110";
			var testCharge = testJob.Charges.AddNew();
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				testCharge.JR_OSSellAmt = 100m;
				testCharge.JR_IsARCashAdvance = true;
				AssertNoErrors("Should not have errors", testCharge.JR_IsARCashAdvanceInfo);
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
				var cashAdvanceHeader = TestObjectCreator.CreateCashAdvanceRequestHeader(testJob, TestObjectCreator.Debtor1, LedgerTypes.AccountsReceivable, 90m, 90m, "AUD");
				var cashAdvanceLine = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader, 90m, 90m);
				testCharge.JR_CAL_ARLine = cashAdvanceLine.PK;
				cashAdvanceLine.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
				AssertNotNull(testCharge.ARCashAdvanceRequestHeader);
				AssertEquals(cashAdvanceHeader.PK, testCharge.ARCashAdvanceRequestHeader.PK);
				testCharge.ARCashAdvanceRequestHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
				testCharge.ARCashAdvanceRequestLine.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
				testCharge.JR_OSSellAmt = 90m;
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
				testCharge.JR_OSSellAmt = 85m;
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
				testCharge.JR_OSSellAmt = 110m;
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
			}
		}
		public void TestSetNonEmptySellInvoicCurrencyWithARCashAdvanceRequest()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111110";
			var testCharge = testJob.Charges.AddNew();
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				testCharge.JR_OSSellAmt = 100m;
				testCharge.JR_IsARCashAdvance = true;
				AssertNoErrors("Should not have errors", testCharge.JR_IsARCashAdvanceInfo);
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
				testCharge.JR_RX_NKSellInvoiceCurrency = "AUD";
				Assert(testCharge.JR_RX_NKSellInvoiceCurrencyInfo.HasError($"Advance Payments are not currently supported on charges with non-empty Sell Invoice Currency. Please either clear the Sell Invoice Currency, or remove the \"Advance Payment Required\" flag"));
				testCharge.JR_RX_NKSellInvoiceCurrency = "";
				AssertNoErrors("Should not have errors", testCharge.JR_RX_NKSellInvoiceCurrencyInfo);
			}
		}
		public void TestSetNonEmptySellInvoicCurrencyWithAPCashAdvanceRequest()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111000";
			var testCharge = testJob.Charges.AddNew();
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_OH_CostAccount = TestObjectCreator.Debtor1.PK;
				testCharge.JR_OSCostAmt = 100m;
				testCharge.JR_IsAPCashAdvance = true;
				AssertNoErrors("Should not have errors", testCharge.JR_IsAPCashAdvanceInfo);
				AssertNoErrors("Should not have errors", testCharge.JR_OSCostAmtInfo);
				testCharge.JR_RX_NKSellInvoiceCurrency = "AUD";
				AssertNoErrors("Should not have errors", testCharge.JR_RX_NKSellInvoiceCurrencyInfo);
			}
		}
		public void TestSetNonEmptySellInvoicCurrencyWithARCashAdvanceRequestAndPostedRevenue()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111115";
			var testCharge = testJob.Charges.AddNew();
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var arLine = TestObjectCreator.CreateInvoiceLine(arInvoice, 50);
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				testCharge.JR_OSSellAmt = 100m;
				testCharge.JR_IsARCashAdvance = true;
				testCharge.JR_AL_ARLine = arLine.PK;
				testCharge.JR_RX_NKSellInvoiceCurrency = "AUD";
				AssertEquals("Precondition: Revenue is posted", true, testCharge.IsRevenuePosted);
				AssertEquals("Sell invoice currency", "AUD", testCharge.JR_RX_NKSellInvoiceCurrency);
				AssertNoErrors("Should not have errors", testCharge.JR_RX_NKSellInvoiceCurrencyInfo);
			}
		}
		public void TestChangeInvoiceTypeWithoutARCashAdvanceRequest()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111001";
			var testCharge = testJob.Charges.AddNew();
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				testCharge.JR_OSSellAmt = 100m;
				testCharge.JR_RX_NKSellCurrency = "USD";
				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				testCharge.JR_IsARCashAdvance = true;
				AssertNoErrors("Should not have errors", testCharge.JR_IsARCashAdvanceInfo);
				AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);
				AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
				AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
			}
		}

		public void TestChangeInvoiceTypeWithRequestedARCashAdvanceRequestForeignToLocal()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", Guid.Empty);

			// Change from Foriegn to Local
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert(testCharge.JR_InvoiceTypeInfo.HasError($"This charge has an active Advance Payment. Changing the Invoice Type will result in an invoice currency different to the Advance Payment currency. Please cancel the Advance Payment if you need to change the Invoice Type of this charge."));
		}

		public void TestChangeInvoiceTypeWithRequestedARCashAdvanceRequestForeignToForeign()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", Guid.Empty);

			//Change from Foriegn to Foriegn
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
		}

		public void TestChangeInvoiceTypeWithRequestedARCashAdvanceRequestLocalToForeign()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.FinalInvoice, "AUD", Guid.Empty);

			// Change from local to Foriegn
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			Assert(testCharge.JR_InvoiceTypeInfo.HasError($"This charge has an active Advance Payment. Changing the Invoice Type will result in an invoice currency different to the Advance Payment currency. Please cancel the Advance Payment if you need to change the Invoice Type of this charge."));
		}

		public void TestChangeInvoiceTypeWithRequestedARCashAdvanceRequestLocalToLocal()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.FinalInvoice, "AUD", Guid.Empty);

			//Change from local to local
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
		}

		public void TestChangeInvoiceTypeWithRequestedARCashAdvanceRequestToEmpty()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", Guid.Empty);

			// Change to Empty
			testCharge.JR_InvoiceType = "";
			Assert(testCharge.JR_InvoiceTypeInfo.HasError("Please enter an Invoice Type."));
		}

		public void TestChangeInvoiceTypeWithPaidARCashAdvanceRequestForeignToLocal()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", Guid.Empty, CashAdvanceStatusCodes.RequestHeader.Paid);

			// Change from Foriegn to Local
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert(testCharge.JR_InvoiceTypeInfo.HasError($"This charge has an active Advance Payment. Changing the Invoice Type will result in an invoice currency different to the Advance Payment currency. Please cancel the Advance Payment if you need to change the Invoice Type of this charge."));
		}

		public void TestChangeInvoiceTypeWithPaidARCashAdvanceRequestForeignToForeign()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", Guid.Empty, CashAdvanceStatusCodes.RequestHeader.Paid);

			//Change from Foriegn to Foriegn
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
		}

		public void TestChangeInvoiceTypeWithPaidARCashAdvanceRequestLocalToForeign()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.FinalInvoice, "AUD", Guid.Empty, CashAdvanceStatusCodes.RequestHeader.Paid);

			// Change from local to Foriegn
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			Assert(testCharge.JR_InvoiceTypeInfo.HasError($"This charge has an active Advance Payment. Changing the Invoice Type will result in an invoice currency different to the Advance Payment currency. Please cancel the Advance Payment if you need to change the Invoice Type of this charge."));
		}

		public void TestChangeInvoiceTypeWithPaidARCashAdvanceRequestLocalToLocal()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.FinalInvoice, "AUD", Guid.Empty, CashAdvanceStatusCodes.RequestHeader.Paid);

			//Change from local to local
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
		}

		public void TestChangeInvoiceTypeWithCancelledARCashAdvanceRequestForeignToLocal()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", Guid.Empty, null, true);

			// Change from Foriegn to Local
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
			Factory.Save();
		}

		public void TestChangeInvoiceTypeWithCancelledARCashAdvanceRequestForeignToForeign()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", Guid.Empty, null, true);

			//Change from Foriegn to Foriegn
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
			Factory.Save();
		}

		public void TestChangeInvoiceTypeWithCancelledARCashAdvanceRequestLocalToLocal()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.FinalInvoice, "AUD", Guid.Empty, null, true);

			//Change from local to local
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
			Factory.Save();
		}

		public void TestChangeInvoiceTypeWithCancelledARCashAdvanceRequestLocalToForeign()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.FinalInvoice, "AUD", Guid.Empty,  null, true);
			testCharge.JR_RX_NKSellCurrency = "USD";
			testCharge.ARCashAdvanceRequestHeader.CAH_RX_NKTransactionCurrency = "USD";

			// Change from local to Foriegn
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
			Factory.Save();
		}

		public void TestCheckJR_IsARCashAdvance()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111111";
			var testCharge = testJob.Charges.AddNew();

			var expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt = "Advance Payment cannot be requested until Debtor and OS Amount are specified. Please enter charge amount and debtor code.";
			var expectedErrorForCashAdvanceFlag_NegativeSellAmount = "Advance Payment cannot be requested for negative charge amounts. Please enter a positive charge amount.";
			var expectedErrorForEmptyDebtor = "Sell amount and Debtor cannot be blank when charge line is flagged 'Advance Payment Required'";
			var expectedErrorForZeroSellAmount = expectedErrorForEmptyDebtor;
			var expectedErrorForNegativeSellAmount = "Sell amount cannot be negative when charge line is flagged 'Advance Payment Required'";

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_OH_SellAccount = ZGuid.Empty;
				testCharge.JR_OSSellAmt = 0.0;
				testCharge.JR_IsARCashAdvance = true;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertHasError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertHasError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertHasError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);

				testCharge.JR_IsARCashAdvance = false;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertNoError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);

				testCharge.JR_OSSellAmt = -100m;
				testCharge.JR_IsARCashAdvance = true;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertHasError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertHasError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertHasError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertHasError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);

				testCharge.JR_IsARCashAdvance = false;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertNoError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);

				testCharge.JR_OSSellAmt = 100m;
				testCharge.JR_IsARCashAdvance = true;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertHasError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertHasError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);

				testCharge.JR_IsARCashAdvance = false;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertNoError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);

				testCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				testCharge.JR_IsARCashAdvance = true;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertNoError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);

				testCharge.JR_IsARCashAdvance = false;

				testCharge.Validation.ValidateJR_IsARCashAdvance();
				testCharge.Validation.ValidateJR_OH_SellAccount();
				testCharge.Validation.ValidateJR_OSSellAmt();

				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_EmptyDebtorAndZeroSellAmt);
				AssertNoError(testCharge.JR_IsARCashAdvanceInfo, expectedErrorForCashAdvanceFlag_NegativeSellAmount);
				AssertNoError(testCharge.JR_OH_SellAccountInfo, expectedErrorForEmptyDebtor);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForZeroSellAmount);
				AssertNoError(testCharge.JR_OSSellAmtInfo, expectedErrorForNegativeSellAmount);
			}
		}

		public void TestCheckJR_OSSellAmt()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S1";
			ForwardingShipment shipment2 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S2";

			var creator = new TestObjectCreator(Factory);
			var master = new ConsolRevenueMaster(consol, Factory);
			var rev = new ConsolRevenue.ConsolRevenue(master);
			rev.ChargeCode = creator.CC1.PK;
			rev.SellAmount = 100m;
			rev.SplitCharges[0].JR_OSSellAmt = 1m;

			Assert(rev.SplitCharges[0].IsUsedForApportionment);
			Assert(rev.SplitCharges[0].JR_OSSellAmtInfo.HasError("The sum of the apportioned OS amounts must be equal to the consol sell OS amount."));

			Assert(rev.SplitCharges[1].IsUsedForApportionment);
			Assert(rev.SplitCharges[1].JR_OSSellAmtInfo.HasError("The sum of the apportioned OS amounts must be equal to the consol sell OS amount."));

			rev.SplitCharges[1].IsUsedForApportionment = false;
			Assert(!rev.SplitCharges[1].JR_OSSellAmtInfo.HasError("The sum of the apportioned OS amounts must be equal to the consol sell OS amount."));

			master.ReleaseMutexes();
		}

		public void TestValidateJR_OSSellInvoiceExRate_ForDisplay()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			job.ExchangeRates.AddRate(TestObjectCreator.USD, 0m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			Assert("Not Revenue Posted", !charge.IsRevenuePosted);
			AssertEquals(0m, charge.JR_OSSellInvoiceExRate);
			charge.Validation.ValidateJR_OSSellInvoiceExRate_ForDisplay();
			AssertHasError("should have error", charge.JR_OSSellInvoiceExRate_ForDisplayInfo, $"Sell Invoice Exchange Rate for Currency {TestObjectCreator.USD.RX_Code} must be greater than 0.");

			var transactionLine = Factory.New<AccTransactionLines>();
			charge.JR_AL_ARLine = transactionLine.PK;
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			Assert("Revenue Posted", charge.IsRevenuePosted);
			AssertEquals(0m, charge.JR_OSSellInvoiceExRate);
			charge.Validation.ValidateJR_OSSellInvoiceExRate_ForDisplay();
			AssertNoErrors("should have no error", charge.JR_OSSellInvoiceExRate_ForDisplayInfo);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			Assert("Not BillInInvoiceCurrency", !charge.BillInInvoiceCurrency);
			Assert("Not Revenue Posted", !charge.IsRevenuePosted);
			AssertEquals(0m, charge.JR_OSSellInvoiceExRate);
			charge.Validation.ValidateJR_OSSellInvoiceExRate_ForDisplay();
			AssertNoErrors("should have no error", charge.JR_OSSellInvoiceExRate_ForDisplayInfo);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			Assert("Not Revenue Posted", !charge.IsRevenuePosted);
			AssertEquals(1.0m, charge.JR_OSSellInvoiceExRate);
			charge.Validation.ValidateJR_OSSellInvoiceExRate_ForDisplay();
			AssertNoErrors("should have no error", charge.JR_OSSellInvoiceExRate_ForDisplayInfo);
		}

		public void TestValidateJR_OSSellInvoiceExRate_ForDisplay_DoesNotFailWithDeletedExchangeRate()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var chargeCC1 = job.Charges.AddNew();
			chargeCC1.JR_AC = TestObjectCreator.CC1.PK;
			chargeCC1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			chargeCC1.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			chargeCC1.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			chargeCC1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			chargeCC1.JR_OSSellAmt = 150m;
			chargeCC1.JR_OSCostAmt = 250m;

			Factory.Save();

			var newExchangeRateInFactory1 = job.ExchangeRates.AddNew();
			newExchangeRateInFactory1.JF_BaseRate = 1.3m;
			newExchangeRateInFactory1.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			newExchangeRateInFactory1.JF_OH_Org = TestObjectCreator.Debtor1.PK;
			newExchangeRateInFactory1.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;

			chargeCC1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			AssertNotNull("Precondition: after the sell invoice currency is added the exchange rate for CC1 line should be available", chargeCC1.SellInvoiceExchangeRate);

			newExchangeRateInFactory1.Delete();
			Assert(newExchangeRateInFactory1.IsDeleted);

			CombineAssertions("Precondition: conditions required to access SellInvoiceExchangeRate.Rate", () =>
			{
				Assert(chargeCC1.BillInInvoiceCurrency);
				Assert(!chargeCC1.IsRevenuePosted);
				Assert(!chargeCC1.BIllInInvoiceCurrencySameAsSellCurrency);
				AssertNotNull(chargeCC1.SellInvoiceExchangeRate);
			});

			AssertNoExceptionThrown("After exchange rate has been deleted, validation should succeed. Deleted exchange rate must not be accessed.", () => chargeCC1.Validation.ValidateJR_OSSellInvoiceExRate_ForDisplay());
		}

		public void TestCheckJR_EstimatedCost_AutomaticChangeWarning()
		{
			var charge = GetNewParentBusinessObject(Factory) as Charge;
			charge.FillWithValidTestData();

			charge.JR_OSCostAmt = 10;
			AssertNoWarnings(charge.JR_EstimatedCostInfo);

			charge.JR_EstimatedCost = 20;
			AssertNoWarnings(charge.JR_EstimatedCostInfo);

			Factory.Save();
			Assert("Precondition: IsInDatabase", charge.IsInDatabase);

			var expectedMessage = "The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.";
			AssertNotEquals("Precondition: JR_OSCostAmt and JR_EstimatedCost", charge.JR_OSCostAmt, charge.JR_EstimatedCost);
			charge.JR_OSCostAmt = charge.JR_EstimatedCost;
			AssertHasWarning(charge.JR_EstimatedCostInfo, expectedMessage);

			charge.JR_EstimatedCost = 0;
			AssertNoWarnings("No warning is shown when estimate amount is 0", charge.JR_EstimatedCostInfo);

			charge.JR_EstimatedCost = (ZDecimal)charge.JR_OSCostAmtInfo.OriginalValue;
			AssertHasWarning(charge.JR_EstimatedCostInfo, expectedMessage);

			charge.JR_OSCostAmt = (ZDecimal)charge.JR_OSCostAmtInfo.OriginalValue;
			AssertNoWarnings("Cost is not changed from db value.", charge.JR_EstimatedCostInfo);

			charge.JR_EstimatedCost = 30;
			AssertNoWarnings("Cost is not changed from db value.", charge.JR_EstimatedCostInfo);

			charge.JR_OSCostAmt = 40;
			AssertHasWarning(charge.JR_EstimatedCostInfo, expectedMessage);

			charge.ReverseAccrual(ZDateTime.Now);
			charge.JR_AL_APLine = Factory.New<APInvoiceLine>().PK;
			Assert("Precondition: IsCostPosted", charge.IsCostPosted);
			charge.RunPreSaveValidation();
			AssertNoWarnings("Don't show warning for posted cost", charge.JR_EstimatedCostInfo);
		}

		public void TestCheckJR_EstimatedRevenue_AutomaticChangeWarning()
		{
			var charge = GetNewParentBusinessObject(Factory) as Charge;
			charge.FillWithValidTestData();

			charge.JR_OSSellAmt = 10;
			AssertNoWarnings(charge.JR_EstimatedRevenueInfo);

			charge.JR_EstimatedRevenue = 20;
			AssertNoWarnings(charge.JR_EstimatedRevenueInfo);

			Factory.Save();
			Assert("Precondition: IsInDatabase", charge.IsInDatabase);

			var expectedMessage = "The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount.";
			AssertNotEquals("Precondition: JR_OSSellAmt and JR_EstimatedRevenue", charge.JR_OSSellAmt, charge.JR_EstimatedRevenue);
			charge.JR_OSSellAmt = charge.JR_EstimatedRevenue;
			AssertHasWarning(charge.JR_EstimatedRevenueInfo, expectedMessage);

			charge.JR_EstimatedRevenue = 0;
			AssertNoWarnings("No warning is shown when estimate amount is 0", charge.JR_EstimatedRevenueInfo);

			charge.JR_EstimatedRevenue = (ZDecimal)charge.JR_OSSellAmtInfo.OriginalValue;
			AssertHasWarning(charge.JR_EstimatedRevenueInfo, expectedMessage);

			charge.JR_OSSellAmt = (ZDecimal)charge.JR_OSSellAmtInfo.OriginalValue;
			AssertNoWarnings("Cost is not changed from db value.", charge.JR_EstimatedRevenueInfo);

			charge.JR_EstimatedRevenue = 30;
			AssertNoWarnings("Cost is not changed from db value.", charge.JR_EstimatedRevenueInfo);

			charge.JR_OSSellAmt = 40;
			AssertHasWarning(charge.JR_EstimatedRevenueInfo, expectedMessage);

			charge.ReverseWIP(ZDateTime.Now);
			charge.JR_AL_ARLine = Factory.New<ARInvoiceLine>().PK;
			Assert("Precondition: IsRevenuePosted", charge.IsRevenuePosted);
			charge.RunPreSaveValidation();
			AssertNoWarnings("Don't show warning for posted revenue", charge.JR_EstimatedRevenueInfo);
		}

		public void TestAPInvoiceNumForCustomsDisbursementCharges()
		{
			MockCustomsJobProvider parent = Factory.New<MockCustomsJobProvider>();
			parent.ValidAPInvoiceNumsToMatch = new ZString[] { "ABC001", "ABC002" };

			Job job = new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OH_CostAccount = creditor.PK;

			charge.JR_APInvoiceNum = "ABC003";
			AssertHasWarning(charge.JR_APInvoiceNumInfo, "This AP invoice number is incorrect. For Customs Disbursement Charges, the AP invoice number should match one of the entry numbers of a declaration. You can leave this number blank and it will be filled in automatically when the system does auto-billing.");

			charge.JR_APInvoiceNum = "ABC002/1";
			AssertNoWarning(charge.JR_APInvoiceNumInfo, "This AP invoice number is incorrect. For Customs Disbursement Charges, the AP invoice number should match one of the entry numbers of a declaration. You can leave this number blank and it will be filled in automatically when the system does auto-billing.");

			charge.JR_APInvoiceNum = "ABC003";
			AssertHasWarning(charge.JR_APInvoiceNumInfo, "This AP invoice number is incorrect. For Customs Disbursement Charges, the AP invoice number should match one of the entry numbers of a declaration. You can leave this number blank and it will be filled in automatically when the system does auto-billing.");

			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_APInvoiceNum = "ABC004";
			AssertNoWarning(charge.JR_APInvoiceNumInfo, "This AP invoice number is incorrect. For Customs Disbursement Charges, the AP invoice number should match one of the entry numbers of a declaration. You can leave this number blank and it will be filled in automatically when the system does auto-billing.");
		}

		public void TestRunAPInvoiceNumberExistsValidation()
		{
			OrgHeader creditor = Factory.New<OrgHeader>();
			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_OH_CostAccount = creditor.PK;
			testCharge.JR_APInvoiceNum = "222";
			AssertHasErrors("Invoice num & creditor entered but no cost amounts, should have errors", testCharge.JR_APInvoiceNumInfo);

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_APInvoiceNum = "221";
			AssertNoErrors("For Comment charges, it does not matter whether it has a cost amount or not.", testCharge.JR_APInvoiceNumInfo);

			chargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			testCharge.JR_APInvoiceNum = "222";
			AssertHasErrors("Invoice num & creditor entered but no cost amounts, should have errors", testCharge.JR_APInvoiceNumInfo);

			testCharge.JR_OSCostAmt = 444;
			testCharge.JR_APInvoiceNum = "333";
			AssertNoErrors("Invoice num, Cost amount entered, should not have errors", testCharge.JR_APInvoiceNumInfo);

			testCharge.JR_OSCostAmt = ZDecimal.Zero;
			testCharge.JR_LocalCostAmt = 333;
			testCharge.JR_APInvoiceNum = "444";
			AssertNoErrors("Invoice num, Cost amount entered, should not have errors", testCharge.JR_APInvoiceNumInfo);
		}

		public void TestRunAPInvoiceNumberExistsValidationWithApportionSplitCharge()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111111";

			var testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.CC1.PK;

			Factory.Save();

			testCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			testCharge.JR_OSCostAmt = 0m;
			testCharge.JR_LocalCostAmt = 0m;
			testCharge.JR_APInvoiceNum = "222";
			AssertHasError("Invoice num & creditor entered but no cost amounts, should have errors", testCharge.JR_APInvoiceNumInfo, "Please add cost amounts to this charge");

			var apportionSplitCharge = Factory.Load<ApportionSplitCharge>(testCharge.PK);
			apportionSplitCharge.JR_IsUsedForApportionment = true;
			testCharge.JR_APInvoiceNum = "333";
			AssertHasError("ApportionSplitCharge loaded, but it's in used", testCharge.JR_APInvoiceNumInfo, "Please add cost amounts to this charge");

			apportionSplitCharge.JR_IsUsedForApportionment = false;
			testCharge.JR_APInvoiceNum = "444";
			AssertNoErrors("ApportionSplitCharge loaded, but it isn't in used", testCharge.JR_APInvoiceNumInfo);
		}

		public void TestCheckJR_AK()
		{
			string warningSamePrinterMessage = AccChequeBook.WarningChequeBookWithSamePrinterMessageStart + "'Book1'" + AccChequeBook.WarningSamePrinterMessageEnd;

			BusinessObject printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;

			OrgHeader creditor = Factory.New<OrgHeader>();
			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_OH_CostAccount = creditor.PK;
			testCharge.JR_OSCostAmt = 444;
			testCharge.JR_APInvoiceNum = "333";
			testCharge.JR_AB = book2.AK_AB;
			testCharge.JR_AK = book2.PK;
			Assert("Should have warning about another Cheque Book with the same printer", testCharge.JR_AKInfo.HasWarning(warningSamePrinterMessage));
		}

		public void TestCheckJR_DisplaySequence()
		{
			OrgHeader creditor = Factory.New<OrgHeader>();
			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_OH_CostAccount = creditor.PK;
			testCharge.JR_OSCostAmt = 444;
			testCharge.JR_APInvoiceNum = "333";

			testCharge.JR_DisplaySequence = 0;
			AssertNoErrors("Display Sequence between 0 and 32767, should not have errors", testCharge.JR_DisplaySequenceInfo);

			testCharge.JR_DisplaySequence = -1;
			AssertHasErrors("Display Sequence should be between 0 and 32767", testCharge.JR_DisplaySequenceInfo);

			testCharge.JR_DisplaySequence = 32767;
			AssertNoErrors("Display Sequence between 0 and 32767, should not have errors", testCharge.JR_DisplaySequenceInfo);

			ARInvoiceLine revenueLine = Factory.New<ARInvoiceLine>();
			testCharge.JR_AL_ARLine = revenueLine.PK;
			testCharge.JR_DisplaySequence = -1;
			Assert("Postcondition: IsRevenuePosted must be true.", testCharge.IsRevenuePosted);
			AssertNoErrors("Display Sequence between 0 and 32767, should not have errors when revenue is posted.", testCharge.JR_DisplaySequenceInfo);
		}

		public void TestDepartmentValidation()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "11111111";

			TestObjectCreator creator = new TestObjectCreator(Factory);
			GlbDepartment inactiveDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			testJob.JH_GE = inactiveDepartment.PK;
			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = creator.CC1.PK;
			testCharge.JR_GE = inactiveDepartment.PK;
			AssertNoErrors("Should not have errors", testCharge.JR_GEInfo);

			inactiveDepartment.GE_IsActive = false;
			testCharge.Validation.ValidateJR_GE();
			AssertHasErrors(testCharge.JR_GEInfo);

			testJob.Factory.Save();

			inactiveDepartment.GE_IsActive = false;
			GlbDepartment secondInactiveDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIA");
			secondInactiveDepartment.GE_IsActive = false;

			testCharge.Validation.ValidateJR_GE();
			AssertNoErrors("Should not have errors", testCharge.JR_GEInfo);

			testCharge.JR_GE = secondInactiveDepartment.PK;
			testCharge.Validation.ValidateJR_GE();
			AssertHasErrors("Should have errors", testCharge.JR_GEInfo);
		}

		public void TestDepartmentValidationOnChargeCode()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);

			AccTaxRate gST = objectCreator.CreateTaxRate("GST", "GSTRate", 10);
			AccWithholding wHT = objectCreator.CreateOrLoadWithholdingTax("WHT", "WHTRate", 5);

			AccChargeCode currentDepartmentChargeCode = objectCreator.CreateChargeCode("MRG100_1", "Margin100%", Constants.ChargeType.Margin, 100, gST, wHT, GlbDepartment.CurrentDepartment.GE_Code);
			AccChargeCode nonCurrentDepartmentChargeCode = objectCreator.CreateChargeCode("MRG100_2", "Margin100%", Constants.ChargeType.Margin, 100, gST, wHT, "TE, OSEC");
			AccChargeCode allDepartmentsChargeCode = objectCreator.CreateChargeCode("MRG100_3", "Margin100%", Constants.ChargeType.Margin, 100, gST, wHT, "ALL");

			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			testCharge.JR_LocalSellAmt = 50.00m;

			testCharge.JR_AC = currentDepartmentChargeCode.PK;
			Assert("Department should not be in error", !testCharge.JR_GEInfo.HasErrors());

			testCharge.JR_AC = nonCurrentDepartmentChargeCode.PK;
			Assert("Department should be in error", testCharge.JR_GEInfo.HasErrors());

			testCharge.JR_AC = allDepartmentsChargeCode.PK;
			Assert("Department should not be in error", !testCharge.JR_GEInfo.HasErrors());
		}

		public override void TestCheckJR_GE()
		{
			base.TestCheckJR_GE();

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);

			AccTaxRate gST = objectCreator.CreateTaxRate("GST", "GSTRate", 10);
			AccWithholding wHT = objectCreator.CreateOrLoadWithholdingTax("WHT", "WHTRate", 5);

			AccChargeCode currentDepartmentChargeCode = objectCreator.CreateChargeCode("Chg1", "Charge code of Current Department", Constants.ChargeType.Margin, 100, gST, wHT, NonMiscDepartment.GE_Code);
			AccChargeCode nonCurrentDepartmentChargeCode = objectCreator.CreateChargeCode("Chg2", "Charge code of Non Current Department", Constants.ChargeType.Margin, 100, gST, wHT, NonMiscDepartment1.GE_Code + ", OSEC");

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge testCharge = job.Charges.AddNew();
			testCharge.JR_LocalSellAmt = 50.00m;
			testCharge.JR_GE = NonMiscDepartment.PK;

			testCharge.JR_AC = currentDepartmentChargeCode.PK;
			Assert("Department should not be in error", !testCharge.JR_GEInfo.HasErrors());

			Factory.Save();

			AssertEquals("IsInDatabase should be true", true, testCharge.IsInDatabase);
			AssertEquals("JR_GEInfo.HasChanges should be false", false, testCharge.JR_GEInfo.HasChanges);
			AssertEquals("JR_ACInfo.HasChanges should be false", false, testCharge.JR_ACInfo.HasChanges);

			testCharge.JR_AC = nonCurrentDepartmentChargeCode.PK;

			AssertEquals("IsInDatabase should be true", true, testCharge.IsInDatabase);
			AssertEquals("TestCharge.JR_GEInfo.HasChanges should be false", false, testCharge.JR_GEInfo.HasChanges);
			AssertEquals("TestCharge.JR_ACInfo.HasChanges should be true", true, testCharge.JR_ACInfo.HasChanges);

			AssertHasError("Department should be in error", testCharge.JR_GEInfo, "This department is not valid for the charge code specified on this Job.");
		}

		public void TestDepartmentValidation_MiscellaneousDepartment()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_GE = MiscDepartment.PK;
			charge.Validation.ValidateJR_GE();
			Assert("Department should have error", charge.JR_GEInfo.HasErrors());
			Assert("Department Error", charge.JR_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));

			charge.JR_JH = ZGuid.Empty;
			charge.Validation.ValidateJR_GE();
			Assert("Department should not have error", !charge.JR_GEInfo.HasErrors());

			charge.JR_JH = job.PK;
			charge.JR_GE = NonMiscDepartment.PK;
			charge.Validation.ValidateJR_GE();
			Assert("Department should not have error", !charge.JR_GEInfo.HasErrors());

			charge.JR_GE = MiscDepartment.PK;
			charge.Factory.Save();
			charge.Validation.ValidateJR_GE();
			Assert("Department should not have error", !charge.JR_GEInfo.HasErrors());

			charge.JR_GE = NonMiscDepartment.PK;
			charge.Factory.Save();
			charge.JR_GE = MiscDepartment.PK;
			charge.Validation.ValidateJR_GE();
			Assert("Department should have error", charge.JR_GEInfo.HasErrors());
			Assert("Department Error", charge.JR_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));

			job.PlugInData = new JobValidationTest.MockJobInvoicingPlugIn(false);
			charge.Validation.ValidateJR_GE();
			Assert("Department should not have error", !charge.JR_GEInfo.HasErrors());

			job.PlugInData = new JobValidationTest.MockJobInvoicingPlugIn(true);
			charge.Validation.ValidateJR_GE();
			Assert("Department should have error", charge.JR_GEInfo.HasErrors());
			Assert("Department Error", charge.JR_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));
		}

		[TestDate(2021, 03, 07)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCheckFieldsThatMustBeEqualOverChargesForSameTransaction_DifferentLoginCompany()
		{
			var creator = new TestObjectCreator(Factory);

			var company = creator.CreateNewCompany("XXX");
			var branch = creator.CreateNewBranch(company, "BRN");

			Factory.Save();

			var invoiceNumber = "123";
			var invoiceDate = ZDateTime.Today.AddDays(1);
			var creditor = creator.Creditor1;
			var currency = creator.USD;

			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var charge1 = creator.CreateCharge(job1, creator.CC1, "Charge 1", creator.AUD, 1m, creditor, creator.AUD, 1m, creator.Agent);
			charge1.JR_RX_NKCostCurrency = currency.RX_Code;
			charge1.JR_APInvoiceNum = invoiceNumber;
			charge1.JR_APInvoiceDate = invoiceDate;

			charge1.Validation.ValidateAll();

			AssertNoErrors(charge1);

			var charge2 = creator.CreateCharge(job1, creator.CC1, "Charge 2", creator.AUD, 1m, creditor, creator.AUD, 1m, creator.Agent);
			charge2.JR_RX_NKCostCurrency = currency.RX_Code;
			charge2.JR_APInvoiceNum = invoiceNumber;
			charge2.JR_APInvoiceDate = invoiceDate.AddDays(1);

			charge2.Validation.ValidateAll();

			var expectedError = "Different to Invoice Date entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_APInvoiceDate.ToShortDateString();
			AssertHasError(charge2.JR_APInvoiceDateInfo, expectedError);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				creator.Agent.CompanyData.OB_IsDebtor = true;
				creditor.CompanyData.OB_IsCreditor = true;

				Factory.Save();

				var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
				var charge3 = creator.CreateCharge(job2, creator.CC1, "Charge 3", creator.AUD, 1m, creditor, creator.AUD, 1m, creator.Agent);
				charge3.JR_RX_NKCostCurrency = currency.RX_Code;
				charge3.JR_APInvoiceNum = invoiceNumber;
				charge3.JR_APInvoiceDate = invoiceDate.AddDays(1);

				charge3.Validation.ValidateAll();

				AssertNoErrors(charge3);
			}
		}

		public override void TestCheckFieldsThatMustBeEqualOverChargesForSameTransaction()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Charge charge1 = creator.CreateCharge(testJob, creator.CC1, "Charge 1", creator.AUD, 1m, creator.Creditor1, creator.AUD, 1m, creator.Agent);
			charge1.JR_RX_NKCostCurrency = "GBP";
			charge1.JR_OSCostExRate = 1.6m;
			charge1.JR_APInvoiceNum = "1";
			charge1.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge1.JR_APDocumentReceivedDate = ZDateTime.Now.AddDays(3);
			charge1.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge1.JR_AB = creator.GBPBankAccount.PK;
			var gBPChequeBook = creator.CreateChequeBook("Test AU chequebook", 100, creator.GBPBankAccount);
			charge1.BankAccount.AB_ChequeNumDigits = 5;
			charge1.JR_AK = gBPChequeBook.PK;
			charge1.JR_ChequeNo = "1";
			charge1.JR_CostReference = "ABC1";

			Charge charge2 = creator.CreateCharge(testJob, creator.CC8, "Charge 2", creator.USD, 2m, creator.Creditor1, creator.AUD, 2m, creator.Agent);
			charge2.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge2.JR_OSCostExRate = 2 * charge1.JR_OSCostExRate;
			charge2.JR_APInvoiceNum = "1";
			charge2.JR_APInvoiceDate = charge1.JR_APInvoiceDate.AddDays(1);
			charge2.JR_APDocumentReceivedDate = charge1.JR_APDocumentReceivedDate.AddDays(1);
			charge2.JR_PaymentDate = charge1.JR_PaymentDate.AddDays(1);
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			charge2.JR_AB = creator.USDBankAccount.PK;
			charge2.BankAccount.AB_ChequeNumDigits = 5;
			charge2.JR_AK = creator.USDChequeBook.PK;
			charge2.JR_ChequeNo = "2";
			charge2.JR_CostReference = "ABC2";

			charge2.Validation.ValidateAll();

			string message = "Invoice/payment details must be the same over all charges for same AP transaction.";
			string expectedCostCurrencyWarning = @"This cost will be posted on a local currency Payables Invoice.
A mix of Cost Currencies have recorded for this Creditor and Invoice Number. Because of this, they will be posted as a local currency payables transaction.";

			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.CostAccount.CompanyData.OB_APCostsSelfBilled = true;
			charge2.Validation.ValidateAll();
			AssertNoErrors(message, charge2.JR_RX_NKCostCurrencyInfo);
			AssertNoWarnings(message, charge2.JR_RX_NKCostCurrencyInfo);
			AssertNoErrors(message, charge2.JR_OSCostExRateInfo);
			AssertNoErrors(message, charge2.JR_APInvoiceDateInfo);
			AssertNoErrors(message, charge2.JR_APDocumentReceivedDateInfo);
			AssertNoErrors(message, charge2.JR_PaymentDateInfo);
			AssertNoErrors(message, charge2.JR_PaymentTypeInfo);
			AssertNoErrors(message, charge2.JR_ABInfo);
			AssertNoErrors(message, charge2.JR_AKInfo);
			AssertNoErrors(message, charge2.JR_ChequeNoInfo);
			AssertNoErrors(message, charge2.JR_CostReferenceInfo);

			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			charge2.CostAccount.CompanyData.OB_APCostsSelfBilled = false;
			charge2.Validation.ValidateAll();
			AssertNoErrors(message, charge2.JR_RX_NKCostCurrencyInfo);
			AssertHasWarning(message, charge2.JR_RX_NKCostCurrencyInfo, expectedCostCurrencyWarning);
			AssertNoErrors(message, charge2.JR_OSCostExRateInfo);
			AssertHasError(message, charge2.JR_APInvoiceDateInfo, "Different to Invoice Date entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_APInvoiceDate.ToShortDateString());
			AssertHasError(message, charge2.JR_APDocumentReceivedDateInfo, "Different to AP Document Received Date entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_APDocumentReceivedDate.ToShortDateString());
			AssertHasError(message, charge2.JR_PaymentDateInfo, "Different to Payment Date entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_PaymentDate.ToShortDateString());
			AssertHasError(message, charge2.JR_PaymentTypeInfo, "Different to Payment Type entered on other charge for the same creditor and AP invoice number. Expected CHQ");
			AssertHasError(message, charge2.JR_ABInfo, "Different to Bank Account entered on other charge for the same creditor and AP invoice number. Expected " + charge1.BankAccount.AB_Code);
			AssertHasError(message, charge2.JR_CostReferenceInfo, "Different to Supplier Cost Reference entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_CostReference);

			charge2.JR_RX_NKCostCurrency = ZString.Empty;
			charge2.JR_APInvoiceDate = ZDateTime.Empty;
			charge2.JR_APDocumentReceivedDate = ZDateTime.Empty;
			charge2.JR_PaymentDate = ZDateTime.Empty;
			charge2.JR_PaymentType = "";
			charge2.JR_CostReference = ZString.Empty;

			charge2.Validation.ValidateAll();

			AssertHasWarning(message, charge2.JR_RX_NKCostCurrencyInfo, expectedCostCurrencyWarning);
			AssertHasError(message, charge2.JR_APInvoiceDateInfo, "Different to Invoice Date entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_APInvoiceDate.ToShortDateString());
			AssertHasError(message, charge2.JR_APDocumentReceivedDateInfo, "Different to AP Document Received Date entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_APDocumentReceivedDate.ToShortDateString());
			AssertHasError(message, charge2.JR_PaymentDateInfo, "Different to Payment Date entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_PaymentDate.ToShortDateString());
			AssertHasError(message, charge2.JR_PaymentTypeInfo, "Different to Payment Type entered on other charge for the same creditor and AP invoice number. Expected CHQ");
			AssertHasError(message, charge2.JR_CostReferenceInfo, "Different to Supplier Cost Reference entered on other charge for the same creditor and AP invoice number. Expected " + charge1.JR_CostReference);

			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_AK = creator.USDChequeBook.PK;
			charge2.JR_ChequeNo = "2";
			charge2.Validation.ValidateAll();
			AssertHasError(message, charge2.JR_ABInfo, "Different to Bank Account entered on other charge for the same creditor and AP invoice number. Expected " + charge1.BankAccount.AB_Code);
			AssertHasError(message, charge2.JR_AKInfo, "Different to Check Book entered on other charge for the same creditor and AP invoice number. Expected " + charge1.ChequeBook.AK_Code);
			AssertHasError(message, charge2.JR_ChequeNoInfo, "Different to Check Number entered on other charge for the same creditor and AP invoice number. Expected 00001");

			charge2.JR_PaymentType = "";
			charge2.JR_AB = ZGuid.Empty;
			charge2.JR_AK = ZGuid.Empty;
			charge2.JR_ChequeNo = "";
			charge1.Validation.ValidateAll();

			AssertHasWarning(message, charge1.JR_RX_NKCostCurrencyInfo, expectedCostCurrencyWarning);
			AssertHasError(message, charge1.JR_APInvoiceDateInfo, "Different to Invoice Date entered on other charge for the same creditor and AP invoice number. Expected empty");
			AssertHasError(message, charge1.JR_APDocumentReceivedDateInfo, "Different to AP Document Received Date entered on other charge for the same creditor and AP invoice number. Expected empty");
			AssertHasError(message, charge1.JR_PaymentDateInfo, "Different to Payment Date entered on other charge for the same creditor and AP invoice number. Expected empty");
			AssertHasError(message, charge1.JR_PaymentTypeInfo, "Different to Payment Type entered on other charge for the same creditor and AP invoice number. Expected empty");
			AssertHasError(message, charge1.JR_ABInfo, "Different to Bank Account entered on other charge for the same creditor and AP invoice number. Expected empty");
			AssertHasError(message, charge1.JR_AKInfo, "Different to Check Book entered on other charge for the same creditor and AP invoice number. Expected empty");
			AssertHasError(message, charge1.JR_ChequeNoInfo, "Different to Check Number entered on other charge for the same creditor and AP invoice number. Expected empty");
			AssertHasError(message, charge1.JR_CostReferenceInfo, "Different to Supplier Cost Reference entered on other charge for the same creditor and AP invoice number. Expected empty");

			charge2.JR_RX_NKCostCurrency = creator.GBP.RX_Code;
			charge2.JR_OSCostExRate = 3.2m;
			charge2.JR_APInvoiceNum = "1";
			charge2.JR_APInvoiceDate = charge1.JR_APInvoiceDate;
			charge2.JR_APDocumentReceivedDate = charge1.JR_APDocumentReceivedDate;
			charge2.JR_PaymentDate = charge1.JR_PaymentDate;
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_AB = creator.GBPBankAccount.PK;
			charge2.JR_AK = gBPChequeBook.PK;
			charge2.JR_ChequeNo = "1";
			charge2.JR_CostReference = charge1.JR_CostReference;

			charge2.Validation.ValidateAll();

			message = "Fields now match, so there should not be errors.";

			AssertNoErrors(message, charge2.JR_RX_NKCostCurrencyInfo);
			AssertNoWarnings(message, charge2.JR_RX_NKCostCurrencyInfo);
			AssertNoErrors(charge2.JR_OSCostExRateInfo);
			AssertNoErrors(message, charge2.JR_APInvoiceDateInfo);
			AssertNoErrors(message, charge2.JR_APDocumentReceivedDateInfo);
			AssertNoErrors(message, charge2.JR_PaymentDateInfo);
			AssertNoErrors(message, charge2.JR_PaymentTypeInfo);
			AssertNoErrors(message, charge2.JR_ABInfo);
			AssertNoErrors(message, charge2.JR_AKInfo);
			AssertNoErrors(message, charge2.JR_ChequeNoInfo);
			AssertNoErrors(message, charge2.JR_CostReferenceInfo);

			charge2.JR_OSCostExRate = 1.6m;
			charge2.Validation.ValidateAll();
			AssertNoErrors(message, charge2.JR_RX_NKCostCurrencyInfo);
			AssertNoWarnings(message, charge2.JR_RX_NKCostCurrencyInfo);
			AssertNoErrors(message, charge2.JR_OSCostExRateInfo);
			AssertNoErrors(message, charge2.JR_APInvoiceDateInfo);
			AssertNoErrors(message, charge2.JR_APDocumentReceivedDateInfo);
			AssertNoErrors(message, charge2.JR_PaymentDateInfo);
			AssertNoErrors(message, charge2.JR_PaymentTypeInfo);
			AssertNoErrors(message, charge2.JR_ABInfo);
			AssertNoErrors(message, charge2.JR_AKInfo);
			AssertNoErrors(message, charge2.JR_ChequeNoInfo);
			AssertNoErrors(message, charge2.JR_CostReferenceInfo);
		}

		public void TestCheckJR_PaymentType_WithMatchedJournal()
		{
			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal.AH_OH = TestObjectCreator.AALSHI.PK;
			journal.AH_ChequeOrReference = "T001";
			journal.AH_InvoiceAmount = 200;
			journal.AH_OutstandingAmount = 200;
			journal.AH_OSTotal = 200;
			Factory.Save();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 250m;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_APInvoiceNum = "T001";
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;

			var expectedErrorMessage = string.Format("Payment details cannot be entered as this AP Invoice Number matches an unpaid ‘Carried Forward’ journal’s payment reference. When this invoice is posted, it will automatically be matched against the journal, up to the value of the invoice. The unpaid journal transaction number is [{0}].", charge.MatchedWithTNFJournalNum);
			AssertHasError("should found the matched journal", charge.JR_PaymentTypeInfo, expectedErrorMessage);

			charge.JR_APInvoiceNum = "INV100";
			AssertNoErrors("should not found the matched journal", charge.JR_PaymentTypeInfo);
		}

		public void TestChequeNumberInUse()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			PrepareForTestChequeNumberInUse(creator);

			Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			Charge charge1 = creator.CreateCharge(testJob, creator.CC1, "Charge 1", creator.AUD, 1m, creator.Creditor1, creator.AUD, 1m, creator.Agent);
			charge1.JR_RX_NKCostCurrency = creator.AUD.RX_Code;
			charge1.JR_OSCostExRate = 1m;
			charge1.JR_APInvoiceNum = "1";
			charge1.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge1.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge1.JR_AB = fTestBank.PK;
			charge1.BankAccount.AB_ChequeNumDigits = 5;
			charge1.JR_AK = fTestChequeBook.PK;

			charge1.JR_ChequeNo = "10001";
			AssertHasErrors("JobCharge with this cheque number", charge1.JR_ChequeNoInfo);
			charge1.JR_ChequeNo = "10002";
			AssertNoErrors("Payment Approval with this cheque number", charge1.JR_ChequeNoInfo);
			charge1.JR_ChequeNo = "10003";
			AssertNoErrors("Hot Cheque with this cheque number", charge1.JR_ChequeNoInfo);
			charge1.JR_ChequeNo = "10004";
			AssertHasErrors("AP Payment with this cheque number", charge1.JR_ChequeNoInfo);
			charge1.JR_ChequeNo = "10005";
			AssertHasErrors("Direct Payment with this cheque number", charge1.JR_ChequeNoInfo);
			charge1.JR_ChequeNo = "10006";
			AssertNoErrors("Reversed Payment with this cheque number", charge1.JR_ChequeNoInfo);
			charge1.JR_ChequeNo = "10007";
			AssertNoErrors("Nothing using this cheque number", charge1.JR_ChequeNoInfo);
		}

		public void TestValidateJR_InvoiceType()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "ORG";
			RefCurrency uSCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			RefCurrency foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "CAD"));
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Env.CurrentCompany.LocalCurrency.Code));

			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_OH_SellAccount = org.PK;

			testCharge.JR_InvoiceType = "---";
			AssertHasErrors("JR_InvoiceType should be invalid", testCharge.JR_InvoiceTypeInfo);
			testCharge.JR_InvoiceType = ZString.Empty;
			AssertHasErrors("JR InvoiceType should be mandatory", testCharge.JR_InvoiceTypeInfo);
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertNoErrors("JR_InvoiceType should be valid type", testCharge.JR_InvoiceTypeInfo);

			testCharge.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			testCharge.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals("JR_InvoiceType should be readonly", true, testCharge.JR_InvoiceTypeInfo.ReadOnly);

			testCharge.JR_InvoiceType = "---";
			AssertNoErrors("Invoice type is readonly, should not have errors", testCharge.JR_InvoiceTypeInfo);

			testCharge.ARLine.AL_LineType = "";
			AssertEquals("JR_InvoiceType should not be readonly", false, testCharge.JR_InvoiceTypeInfo.ReadOnly);

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();

			testCharge.JR_AC = chargeCode.PK;

			//Check Charge Code group is Freight for "FRT" and "FRD" invoice type
			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only Freight Charges may appear on the Freight Invoice in Foreign Currency.");

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only Freight Charges may appear on the Deferred - Freight Foreign Currency Periodic Invoice.");

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			AssertNoMessageError("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo, "Only Freight Charges may appear on the Deferred - Freight Foreign Currency Periodic Invoice.");

			//Check Sell Currency is Foreign Currency  for "FRT" and "FRD" invoice type
			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.JR_RX_NKSellCurrency = uSCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			testCharge.JR_RX_NKSellCurrency = currency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Freight Invoice in Foreign Currency.");

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			AssertNoMessageError("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Freight Invoice in Foreign Currency.");

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			testCharge.JR_RX_NKSellCurrency = currency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only Freight Charges may appear on the Deferred - Freight Foreign Currency Periodic Invoice.");

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			AssertNoMessageError("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo, "Only Freight Charges may appear on the Deferred - Freight Foreign Currency Periodic Invoice.");

			//Other
			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			testCharge.JR_RX_NKSellCurrency = currency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Foreign Currency Invoice.");

			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Deferred - Foreign Currency Periodic Invoice.");

			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, string.Format("There are deferred charges on this job that cannot post.  Please amend the Debtor's Periodic Invoicing setup and/or the Invoice Types used on charges on this job.  The {0} on this job has a Deferred Invoice Type however the Debtor's Periodic Invoicing configuration does not allow this charge code to be deferred.", testCharge.ChargeCode.AC_Code));

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Loading;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			testCharge.JR_RX_NKSellCurrency = currency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Disbursement Invoice in Foreign Currency.");

			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Deferred - Disbursement Foreign Currency Periodic Invoice.");

			testCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			AssertHasError(testCharge.JR_InvoiceTypeInfo, string.Format("There are deferred charges on this job that cannot post.  Please amend the Debtor's Periodic Invoicing setup and/or the Invoice Types used on charges on this job.  The {0} on this job has a Deferred Invoice Type however the Debtor's Periodic Invoicing configuration does not allow this charge code to be deferred.", testCharge.ChargeCode.AC_Code));

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Loading;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertNoErrors("Invoice type should be valid", testCharge.JR_InvoiceTypeInfo);

			testCharge.JR_OH_SellAccount = ZGuid.Empty;
			testCharge.JR_InvoiceType = ZString.Empty;
			AssertNoErrors("Invoice type is not readonly, and is blank - but no debtor so it's not mandatory", testCharge.JR_InvoiceTypeInfo);

			testCharge.JR_InvoiceType = "---";
			AssertHasError("List validation should run even if debtor is empty", testCharge.JR_InvoiceTypeInfo, "Enter a valid Invoice Type.");

			var agencyBookingCharge = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.AgencyBooking), false).Charges.AddNew();
			agencyBookingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			AssertHasError(agencyBookingCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Collect - Foreign Currency.");

			agencyBookingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
			AssertHasError(agencyBookingCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Pre-Paid - Foreign Currency.");

			agencyBookingCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			agencyBookingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			AssertNoErrors("Invoice type should be valid", agencyBookingCharge.JR_InvoiceTypeInfo);

			agencyBookingCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			agencyBookingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
			AssertNoErrors("Invoice type should be valid", agencyBookingCharge.JR_InvoiceTypeInfo);

			var agencyBillOfLadingCharge = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.AgencyBillOfLading), false).Charges.AddNew();
			agencyBillOfLadingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			AssertHasError(agencyBillOfLadingCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Collect - Foreign Currency.");

			agencyBillOfLadingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
			AssertHasError(agencyBillOfLadingCharge.JR_InvoiceTypeInfo, "Only charges with a foreign Sell currency may appear on the Pre-Paid - Foreign Currency.");

			agencyBillOfLadingCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			agencyBillOfLadingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			AssertNoErrors("Invoice type should be valid", agencyBillOfLadingCharge.JR_InvoiceTypeInfo);

			agencyBillOfLadingCharge.JR_RX_NKSellCurrency = foreignCurrency.RX_Code;
			agencyBillOfLadingCharge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
			AssertNoErrors("Invoice type should be valid", agencyBillOfLadingCharge.JR_InvoiceTypeInfo);
		}

		public void TestValidateJR_NONInvoiceType()
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			AccChargeCode chargeCode = TestObjectCreator.CC1;

			using (Job job = TestObjectCreator.CreateJob(shipment as IJobInvoicingPlugIn))
			{
				Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI,
					TestObjectCreator.USD, 10M, TestObjectCreator.ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
				Charge charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc2", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI,
					TestObjectCreator.USD, 10M, TestObjectCreator.ABIGAS);
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertNoErrors("Charge not in database - DoNotPost", charge.JR_InvoiceTypeInfo);
				AssertNoErrors("Charge not in database - FinalInvoice", charge2.JR_InvoiceTypeInfo);
				job.Factory.Save();

				ISecurityCheckpoint rootSecurity = job.PlugInData.InvoicingSupporter.JobInvoicingSecurity;
				ISecurityCheckpoint invoicingSecurity = rootSecurity != null ? rootSecurity.FindChild(rootSecurity.Code + SecurityCore.Invoicing) : null;
				ISecurityCheckpoint security = invoicingSecurity != null ? invoicingSecurity.FindChild(rootSecurity.Code + SecurityCore.AllowUseOfNONInvoiceType) : null;
				AssertNotNull("Security Checkpoint", security);

				bool originalValue = security.IsAllowed;
				try
				{
					security.IsAllowed = false;
					AssertEquals("Security IsAllowed is false", false, security.IsAllowed);

					Charge charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc3", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI,
						TestObjectCreator.USD, 10M, TestObjectCreator.ABIGAS);
					charge3.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
					string expectedError = @"You do not have sufficient security rights to change the invoice type to 'NON - Do Not Post.
Contact your system administrator for rights to set this charge to 'Not for Invoicing - Do not Post'.
This security right can be found in the following location: 'Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Allow use of 'NON - Do Not Post' invoice type'.";
					AssertHasError("Charge not in database - DoNotPost", charge3.JR_InvoiceTypeInfo, expectedError);
					charge.Validation.ValidateJR_InvoiceType();
					AssertNoErrors("Charge in database - Unchanged - DoNotPost", charge.JR_InvoiceTypeInfo);

					charge2.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
					AssertHasError("Charge in database - Changed - DoNotPost", charge2.JR_InvoiceTypeInfo, expectedError);

					Charge charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc4", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI,
						TestObjectCreator.USD, 10M, TestObjectCreator.ABIGAS);
					charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
					AssertNoErrors("Charge not in database - FinalInvoice", charge4.JR_InvoiceTypeInfo);
				}
				finally
				{
					security.IsAllowed = originalValue;
				}
			}
		}

		public void TestCheckJR_InvoiceType_SBRAndSBD()
		{
			var expectedError = "Invoice Type 'SBR' and 'SBD' cannot be selected as the Charge Debtor's Organization > A/R > Invoicing > Invoicing > Customer Self Bills check box is NOT ticked.";

			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "ORG";

			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_OH_SellAccount = org.PK;

			using (AccountingConfigurationRegistry.Instance.DisallowSelectionOfSBRandSBDInvoiceType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				org.CompanyData.OB_ARCustomerSelfBillsRevenue = false;
				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
				AssertNoError(testCharge.JR_InvoiceTypeInfo, expectedError);

				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice_Batching;
				AssertNoError(testCharge.JR_InvoiceTypeInfo, expectedError);

				org.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
				AssertNoError(testCharge.JR_InvoiceTypeInfo, expectedError);

				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice_Batching;
				AssertNoError(testCharge.JR_InvoiceTypeInfo, expectedError);
			}

			using (AccountingConfigurationRegistry.Instance.DisallowSelectionOfSBRandSBDInvoiceType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				org.CompanyData.OB_ARCustomerSelfBillsRevenue = false;
				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
				AssertHasError(testCharge.JR_InvoiceTypeInfo, expectedError);

				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice_Batching;
				AssertHasError(testCharge.JR_InvoiceTypeInfo, expectedError);

				org.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
				AssertNoError(testCharge.JR_InvoiceTypeInfo, expectedError);

				testCharge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice_Batching;
				AssertNoError(testCharge.JR_InvoiceTypeInfo, expectedError);
			}
		}

		public void TestCheckJR_InvoiceType_DeferredCharge()
		{
			TestObjectCreator.CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			OrgInvoiceType invoiceType = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = InvoiceTypeModuleList.Codes.FWD;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			OrgInvTypeDeferredCharges deferredCharges1 = invoiceType.DeferredCharges.AddNew();
			OrgInvTypeDeferredCharges deferredCharges2 = invoiceType.DeferredCharges.AddNew();
			deferredCharges1.PO_AC = TestObjectCreator.CC3.PK;
			deferredCharges2.PO_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var deferredInvoiceTypesNotForAgencyShippingJobs = InvoiceTypeCalculationProvider.DeferredInvoiceTypes.Except(new AgencyInvoiceTypesList().ToArray().Select(x => x.Code));
			AssertJR_InvoiceType_DeferredCharge(deferredInvoiceTypesNotForAgencyShippingJobs, InvoiceTypesList.Codes.FinalInvoice, shipment as IJobInvoicingPlugIn);

			var deferredInvoiceTypesForAgencyShippingJobs = InvoiceTypeCalculationProvider.DeferredInvoiceTypes.Intersect(new AgencyInvoiceTypesList().ToArray().Select(x => x.Code));
			AssertJR_InvoiceType_DeferredCharge(deferredInvoiceTypesForAgencyShippingJobs, AgencyInvoiceTypesList.Codes.ForeignCollect, TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.AgencyBillOfLading));
		}

		public void TestCheckJR_Calc_CostRatingBehavior()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 250m;

			AssertEquals("Pre-condition: should default to new charge", JobChargeLookups.CreateNewCharge, charge.JR_Calc_CostRatingBehavior);

			charge.JR_Calc_CostRatingBehavior = "XXX";

			AssertEquals("Field should revert to default value when invalid", JobChargeLookups.CreateNewCharge, charge.JR_Calc_CostRatingBehavior);
			AssertNoErrors("should be a validation error", charge.JR_Calc_CostRatingBehaviorInfo);

			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;

			AssertEquals(JobChargeLookups.ReAutorateCharge, charge.JR_Calc_CostRatingBehavior);
			AssertNoErrors("should be no validation errors when valid option", charge.JR_Calc_CostRatingBehaviorInfo);

			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;

			AssertEquals(JobChargeLookups.CreateNewCharge, charge.JR_Calc_CostRatingBehavior);
			AssertNoErrors("should be no validation errors", charge.JR_Calc_CostRatingBehaviorInfo);

			charge.JR_Calc_CostRatingBehavior = "";

			AssertEquals("Field should revert to default value when invalid", JobChargeLookups.CreateNewCharge, charge.JR_Calc_CostRatingBehavior);
			AssertNoErrors("should be a validation error", charge.JR_Calc_CostRatingBehaviorInfo);
		}

		public void TestCheckJR_Calc_SellRatingBehavior()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 250m;

			AssertEquals("Pre-condition: should default to new charge", JobChargeLookups.CreateNewCharge, charge.JR_Calc_SellRatingBehavior);

			charge.JR_Calc_SellRatingBehavior = "XXX";

			AssertEquals("Field should revert to default value when invalid", JobChargeLookups.CreateNewCharge, charge.JR_Calc_SellRatingBehavior);
			AssertNoErrors("should be a validation error", charge.JR_Calc_SellRatingBehaviorInfo);

			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			AssertEquals(JobChargeLookups.ReAutorateCharge, charge.JR_Calc_SellRatingBehavior);
			AssertNoErrors("should be no validation errors when valid option", charge.JR_Calc_SellRatingBehaviorInfo);

			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;

			AssertEquals(JobChargeLookups.CreateNewCharge, charge.JR_Calc_SellRatingBehavior);
			AssertNoErrors("should be no validation errors", charge.JR_Calc_SellRatingBehaviorInfo);

			charge.JR_Calc_SellRatingBehavior = "";

			AssertEquals("Field should revert to default value when invalid", JobChargeLookups.CreateNewCharge, charge.JR_Calc_SellRatingBehavior);
			AssertNoErrors("should be a validation error", charge.JR_Calc_SellRatingBehaviorInfo);
		}

		void AssertJR_InvoiceType_DeferredCharge(IEnumerable<string> deferredInvoiceTypes, ZString nonDeferredInvoiceType, IJobInvoicingPlugIn plugin)
		{
			using (Job job = TestObjectCreator.CreateJob(plugin))
			{
				Charge testCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI,
					TestObjectCreator.USD, 10M, TestObjectCreator.ABIGAS);

				Assert("Precondition: charge should not be deferred.", !testCharge.IsDeferredCharge);

				foreach (string deferredInvoiceType in deferredInvoiceTypes)
				{
					TestObjectCreator.CC2.AC_ChargeGroup = deferredInvoiceType == InvoiceTypesList.Codes.FreightInvoice_Batching ? ChargeCodeGroupList.Codes.Freight : ChargeCodeGroupList.Codes.Brokerage;
					testCharge.JR_InvoiceType = deferredInvoiceType;
					AssertHasError(testCharge.JR_InvoiceTypeInfo, string.Format("There are deferred charges on this job that cannot post.  Please amend the Debtor's Periodic Invoicing setup and/or the Invoice Types used on charges on this job.  The {0} on this job has a Deferred Invoice Type however the Debtor's Periodic Invoicing configuration does not allow this charge code to be deferred.", testCharge.ChargeCode.AC_Code));
				}

				AssertHasErrorContaining(testCharge.JR_InvoiceTypeInfo, "There are deferred charges on this job that cannot post.  Please amend the Debtor's Periodic Invoicing setup and/or the Invoice Types used on charges on this job.");
				testCharge.JR_InvoiceType = nonDeferredInvoiceType;
				AssertNoErrors(testCharge.JR_InvoiceTypeInfo);

				testCharge.JR_AC = ZGuid.Empty;
				AssertNull("ChargeCode should be null", testCharge.ChargeCode);
				foreach (string deferredInvoiceType in deferredInvoiceTypes)
				{
					testCharge.JR_InvoiceType = deferredInvoiceType;
					AssertNoErrorContaining(testCharge.JR_InvoiceTypeInfo, "There are deferred charges on this job that cannot post.  Please amend the Debtor's Periodic Invoicing setup and/or the Invoice Types used on charges on this job.");
				}
			}
		}

		#region Comment Charge Code Related

		public void TestCommentChargeCodeValidation()
		{
			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_AC = CommentCharge.PK;

			testCharge.JR_OSCostAmt = 0;
			testCharge.JR_LocalCostAmt = 0;
			testCharge.JR_RX_NKCostCurrency = ZString.Empty;
			testCharge.JR_OH_CostAccount = ZGuid.Empty;

			testCharge.JR_OSSellAmt = 0;
			testCharge.JR_LocalSellAmt = 0;

			testCharge.RunPreSaveValidation();

			Assert("Cost Account should not be validated for Comment Charge", !testCharge.JR_OH_CostAccountInfo.HasErrors());
			Assert("OS Cost Amount should not be validated for Comment Chargel", !testCharge.JR_OSCostAmtInfo.HasErrors());
			Assert("Local Cost Amount should not be validated for Comment Charge", !testCharge.JR_LocalCostAmtInfo.HasErrors());
			Assert("OS Sell Amount should not be validated for Comment Charge", !testCharge.JR_OSSellAmtInfo.HasErrors());
			Assert("Local Sell Amount should not be validated for Comment Charge", !testCharge.JR_LocalSellAmtInfo.HasErrors());
			Assert("Local Sell Amount should not have warning for Comment Charge", !testCharge.JR_LocalSellAmtInfo.HasWarnings());
		}

		public void TestCommentChargeCodeValidationAfterExistingCharge()
		{
			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_AC = new TestObjectCreator(Factory).CC1.PK;

			testCharge.JR_OSCostAmt = 0;
			testCharge.JR_LocalCostAmt = 0;
			testCharge.JR_RX_NKCostCurrency = ZString.Empty;
			testCharge.JR_OH_CostAccount = ZGuid.Empty;

			testCharge.JR_OSSellAmt = 0;
			testCharge.JR_LocalSellAmt = 0;

			testCharge.JR_AC = CommentCharge.PK;

			testCharge.RunPreSaveValidation();

			Assert("Cost Account should not be validated for Comment Charge", !testCharge.JR_OH_CostAccountInfo.HasErrors());
			Assert("OS Cost Amount should not be validated for Comment Chargel", !testCharge.JR_OSCostAmtInfo.HasErrors());
			Assert("Local Cost Amount should not be validated for Comment Charge", !testCharge.JR_LocalCostAmtInfo.HasErrors());
			Assert("OS Sell Amount should not be validated for Comment Charge", !testCharge.JR_OSSellAmtInfo.HasErrors());
			Assert("Local Sell Amount should not be validated for Comment Charge", !testCharge.JR_LocalSellAmtInfo.HasErrors());
			Assert("Local Sell Amount should not have warning for Comment Charge", !testCharge.JR_LocalSellAmtInfo.HasWarnings());
		}

		public void TestCommentChargeCodeGST()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			TestObjectCreator.LocalClient.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.LocalClient.CompanyData.SetAPTaxApplicable(true);

			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			testCharge.JR_OH_CostAccount = TestObjectCreator.LocalClient.PK;

			testCharge.JR_AC = CommentCharge.PK;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			testCharge.JR_AT_CostGSTRate = ZGuid.Empty;
			testCharge.JR_AT_SellGSTRate = ZGuid.Empty;

			testCharge.RunPreSaveValidation();

			Assert("Cost GST Rate should not be validated for Comment Charge", !testCharge.JR_AT_CostGSTRateInfo.HasErrors());
			Assert("Sell GST Rate should not be validated for Comment Charge", !testCharge.JR_AT_SellGSTRateInfo.HasErrors());

			Assert("Cost GST Amount should not be validated for Comment Charge", !testCharge.JR_Cost_LocalGSTAmountInfo.HasErrors());
			Assert("Cost GST Amount should not be validated for Comment Charge", !testCharge.JR_Sell_LocalGSTAmountInfo.HasErrors());
		}

		public void TestCommentChargeCodeGSTForTaxTypeInvoice()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			TestObjectCreator.LocalClient.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.LocalClient.CompanyData.SetAPTaxApplicable(true);

			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			testCharge.JR_OH_CostAccount = TestObjectCreator.LocalClient.PK;

			testCharge.JR_AC = CommentCharge.PK;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.InvoicePerTaxCode;

			testCharge.JR_AT_CostGSTRate = ZGuid.Empty;
			testCharge.JR_AT_SellGSTRate = ZGuid.Empty;

			testCharge.RunPreSaveValidation();

			Assert("Cost GST Rate should not be validated for Comment Charge", !testCharge.JR_AT_CostGSTRateInfo.HasErrors());
			Assert("Sell GST Rate should not be validated for Comment Charge", !testCharge.JR_AT_SellGSTRateInfo.HasErrors());

			Assert("Cost GST Amount should not be validated for Comment Charge", !testCharge.JR_Cost_LocalGSTAmountInfo.HasErrors());
			Assert("Sell GST Amount should not be validated for Comment Charge", !testCharge.JR_Sell_LocalGSTAmountInfo.HasErrors());
		}

		public void TestCheckJR_InvoiceType_FRT_CommentCharge()
		{
			Charge testCharge = Factory.New<Charge>();
			testCharge.JR_AC = CommentCharge.PK;

			testCharge.JR_RX_NKSellCurrency = "USD";
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;

			testCharge.RunPreSaveValidation();

			AssertEquals("JR_InvoiceType should not have errors", false, testCharge.JR_InvoiceTypeInfo.HasErrors());
			AssertEquals("JR_AC should not have errors", false, testCharge.JR_ACInfo.HasErrors());
		}

		#endregion

		public void TestValidateGatewayJRJOnlyRunOnApplicableCharges()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = consol.Shipments.AddNew();
			Factory.Save();

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (var gatewayJob = TestObjectCreator.CreateJob(consol))
			using (TestObjectCreator.CreateJob(shipment))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OH_SellAccount = consol.ReceivingForwarder.PK;
				charge.JR_LocalSellAmt = 100;
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
				Assert(GatewaySellToCostSynchroniser.IsGatewaySynchronizable(charge));
				Assert(charge.TryFindGatewaySellAndCostsFromGatewaySell(out var _));
				charge.Validation.ValidateRow();
				AssertNoRowErrors(charge);

				charge.JR_LocalSellAmt = 200;
				charge.Validation.ValidateRow();
				AssertHasRowErrorContaining(charge, "journal total line amount is not zero");

				charge.JR_GB_InternalBranch = ZGuid.Empty;
				Assert("As Internal branch no longer matches branch, charge is not valid for JRJ", !GatewaySellToCostSynchroniser.IsGatewaySynchronizable(charge));
				Assert("Pre-condition: An apportionment still exists for the charge as it has not been re-syncrhonised", charge.TryFindGatewaySellAndCostsFromGatewaySell(out var _));
				charge.Validation.ValidateRow();
				AssertNoRowErrors("When charge is not synchronisable, auto JRJ validation should not be done.", charge);
			}
		}

		public override void TestIsInvoiceNumberApplicable()
		{
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			Charge charge1 = GetNewParentBusinessObject(Factory) as Charge;
			charge1.FillWithValidTestData();
			charge1.JR_APInvoiceNum = "111";
			charge1.JR_OH_CostAccount = creditor.PK;
			charge1.JR_E6 = ZGuid.Empty;
			charge1.JR_JH = job.PK;
			AssertEquals("IsInvoiceNumberApplicable", true, charge1.Validation.IsInvoiceNumberApplicable_ForTestOnly());

			Factory.Save();
			AssertEquals("IsInvoiceNumberApplicable", true, charge1.Validation.IsInvoiceNumberApplicable_ForTestOnly());

			var newFactory = new BusinessObjectFactory();
			var job2 = newFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();

			var charge2 = GetNewParentBusinessObject(newFactory) as Charge;
			charge2.JR_GB = Env.CurrentBranchPK;
			charge2.JR_APInvoiceNum = "111";
			charge2.JR_OH_CostAccount = creditor.PK;
			charge2.JR_JH = job2.PK;
			AssertEquals("IsInvoiceNumberApplicable", false, charge2.Validation.IsInvoiceNumberApplicable_ForTestOnly());
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("IsInvoiceNumberApplicable", true, charge2.Validation.IsInvoiceNumberApplicable_ForTestOnly());
			}

			charge2.JR_GB = Env.CurrentBranchPK;
			charge2.JR_APInvoiceNum = "111";
			charge2.JR_OH_CostAccount = creditor2.PK;
			charge2.JR_JH = job2.PK;
			AssertEquals("IsInvoiceNumberApplicable", true, charge2.Validation.IsInvoiceNumberApplicable_ForTestOnly());

			charge2.JR_GB = Env.CurrentBranchPK;
			charge2.JR_APInvoiceNum = "111";
			charge2.JR_OH_CostAccount = creditor.PK;
			charge2.JR_JH = job.PK;
			AssertEquals("IsInvoiceNumberApplicable", true, charge2.Validation.IsInvoiceNumberApplicable_ForTestOnly());

			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_InvoiceNum = "111";
			cost.E6_OH_Creditor = creditor.PK;
			cost.E6_AC_ChargeCode = charge1.JR_AC;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;

			charge1.JR_E6 = cost.PK;
			Factory.Save();
			AssertEquals("IsInvoiceNumberApplicable", false, charge2.Validation.IsInvoiceNumberApplicable_ForTestOnly());
		}

		public void TestCheckJR_AC_eNettRegistration()
		{
			Charge charge = Factory.New<Charge>();
			AccChargeCode eNettRegisteredChargeCode = TestObjectCreator.CreateChargeCode("ENETT", "ENETT", Enterprise.Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHT1, "ALL");
			AccChargeCode notRegisteredChargeCode = TestObjectCreator.CreateChargeCode("NONREG", "NONREG", Enterprise.Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHT1, "ALL");
			OrgHeader eNettRegisteredOrg = Factory.NewWithValidTestData<OrgHeader>();
			eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
			OrgHeader notRegisteredOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgPatternMatchOverride ov = TestObjectCreator.AALSHI.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			ov.OO_ForeignCode = "ABC";
			ov.OO_LocalCode = eNettRegisteredChargeCode.AC_Code;
			EnettRegistrationCode regCode = new EnettRegistrationCode();
			regCode.RegistrationCode = "AAA";
			regCode.AuthenticationCode = "";
			regCode.OrganisationPK = TestObjectCreator.AALSHI.PK;
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																																	Guid.Empty,
																																	Guid.Empty,
																																	regCode);
			string warning = eNettHelper.NoEnettMappingError;

			charge.JR_OH_SellAccount = notRegisteredOrg.PK;
			charge.JR_AC = eNettRegisteredOrg.PK;
			AssertNoWarning(charge.JR_ACInfo, warning);
			charge.JR_AC = notRegisteredChargeCode.PK;
			AssertNoWarning(charge.JR_ACInfo, warning);

			charge.JR_OH_SellAccount = eNettRegisteredOrg.PK;
			charge.JR_AC = eNettRegisteredChargeCode.PK;
			AssertNoWarning(charge.JR_ACInfo, warning);
			charge.JR_AC = notRegisteredChargeCode.PK;
			AssertHasWarning(charge.JR_ACInfo, warning);
			regCode = new EnettRegistrationCode();
			regCode.RegistrationCode = "";
			regCode.AuthenticationCode = "";
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																																	Guid.Empty,
																																	Guid.Empty,
																																	regCode);
			charge.Validation.ValidateJR_AC();
			AssertNoWarning(charge.JR_ACInfo, warning);
		}

		public void TestCheckJR_AC_Gateway()
		{
			var gatewayAgent = GlbBranch.CurrentBranch.OrgProxy;
			gatewayAgent.OH_IsForwarder = true;

			var orgAppointedAgentPorts1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUSYD";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);

			Factory.Save();

			var gatewayJob = TestObjectCreator.CreateJob(consol);
			gatewayJob.JH_OA_LocalChargesAddr = gatewayAgent.MainAddress.PK;
			gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

			var charge = gatewayJob.Charges.AddNew();
			charge.JR_OH_SellAccount = gatewayAgent.PK;
			charge.JR_AC = TestObjectCreator.CC8.PK;
			charge.JR_LocalSellAmt = 11.1m;
			charge.Validation.ValidateJR_AC();

			var warning = "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.";

			AssertNoWarning("Pre-condition: only one charge should not trigger this warning", charge.JR_ACInfo, warning);

			var charge2 = gatewayJob.Charges.AddNew();
			charge2.JR_OH_SellAccount = gatewayAgent.PK;
			charge2.JR_AC = TestObjectCreator.CC8.PK;
			charge2.JR_LocalSellAmt = -44.4m;
			charge2.Validation.ValidateJR_AC();

			var unrelatedCharge = gatewayJob.Charges.AddNew();
			unrelatedCharge.JR_OH_SellAccount = gatewayAgent.PK;
			unrelatedCharge.JR_AC = TestObjectCreator.CC1.PK;
			unrelatedCharge.JR_LocalSellAmt = 33.3m;

			Assert("Pre-condition: should be a gateway consol", consol.IsGateway());
			AssertHasWarning("Expected to have the warning as we have to charges with the same code and values that aren't 0 ", charge2.JR_ACInfo, warning);

			charge.Validation.ValidateJR_AC();
			AssertHasWarning(charge.JR_ACInfo, warning);

			unrelatedCharge.Validation.ValidateJR_AC();
			AssertNoWarning("Expected no warning as this charge has another type", unrelatedCharge.JR_ACInfo, warning);
			AssertNoWarnings("Expected no warning as this charge has another type", unrelatedCharge.JR_ACInfo);

			charge2.JR_LocalSellAmt = 0m;
			charge2.Validation.ValidateJR_AC();

			AssertNoWarning("Expected no warning as charge 2 has no sell amount to apportion", charge2.JR_ACInfo, warning);

			charge.Validation.ValidateJR_AC();
			AssertNoWarning("Expected no warning as charge 2 has no sell amount to apportion", charge.JR_ACInfo, warning);

			charge2.JR_LocalSellAmt = 77.7m;
			charge2.Validation.ValidateJR_AC();

			AssertHasWarning(charge2.JR_ACInfo, warning);

			charge.Validation.ValidateJR_AC();
			AssertHasWarning(charge.JR_ACInfo, warning);

			charge2.JR_AC = TestObjectCreator.CC7.PK;
			charge2.JR_LocalSellAmt = 77.7m;
			charge2.Validation.ValidateJR_AC();

			AssertNoWarning("Expected no warning as charge codes are different", charge2.JR_ACInfo, warning);

			charge.Validation.ValidateJR_AC();
			AssertNoWarning("Expected no warning as charge codes are different", charge.JR_ACInfo, warning);

			gatewayJob.Dispose();
		}

		public void TestCheckJR_AC_Gateway_AllowTwoGatewayAgents()
		{
			var org1 = GlbCompany.CurrentCompany.Factory.NewWithValidTestData<OrgHeader>();
			var org2 = GlbCompany.CurrentCompany.Factory.NewWithValidTestData<OrgHeader>();
			var branch1 = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.GB_Code == "SYD");
			branch1.GB_OH_OrgProxy = org1.PK;
			var branch2 = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.GB_Code == "BNE");
			branch2.GB_OH_OrgProxy = org2.PK;
			GlbCompany.CurrentCompany.Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPorts1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUSYD";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);
			var orgAppointedAgentPorts2 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts2.O5_PortOrCountry = "CNSHA";
			orgAppointedAgentPorts2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.ReceivingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts2);
			Factory.Save();

			var warning = "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.";

			var gatewayJob = TestObjectCreator.CreateJob(consol);
			gatewayJob.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
			gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

			var charge = gatewayJob.Charges.AddNew();
			charge.JR_OH_SellAccount = org1.PK;
			charge.JR_AC = TestObjectCreator.CC8.PK;
			charge.JR_LocalSellAmt = 11.1m;
			charge.Validation.ValidateJR_AC();
			AssertNoWarning("Pre-condition: only one charge should not trigger this warning", charge.JR_ACInfo, warning);

			var charge2 = gatewayJob.Charges.AddNew();
			charge2.JR_OH_SellAccount = org2.PK;
			charge2.JR_AC = TestObjectCreator.CC8.PK;
			charge2.JR_LocalSellAmt = -44.4m;
			charge2.Validation.ValidateJR_AC();
			AssertHasWarning("Expected to have the warning as we have two charges, both has a gateway agent as debtor, and with the same code and values that aren't 0 ", charge2.JR_ACInfo, warning);

			charge2.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
			charge2.Validation.ValidateJR_AC();
			AssertNoWarning("Expected no warning as there is only one gateway charge line for charge code CC8 now", charge2.JR_ACInfo, warning);

			gatewayJob.Dispose();
		}

		public void TestCheckJR_SellGovtChargeCodeMessage()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
				var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m);
				Factory.Save();

				AssertEquals("Pre-condition", 1, job.Charges.Count);
				var charge1 = job.Charges[0];

				charge1.Validation.ValidateJR_SellGovtChargeCode();
				AssertEquals("charge is apportioned", true, charge1.JR_IsApportioned);
				AssertHasWarning(charge1.JR_SellGovtChargeCodeInfo, "Please enter a Sell Government Charge Code via Consolidation > Consol Costing tab.");
				AssertNoWarning(charge1.JR_SellGovtChargeCodeInfo, "Sell Government Charge Code is empty.");

				charge1.JR_E6 = ZGuid.Empty;
				charge1.Validation.ValidateJR_SellGovtChargeCode();
				AssertEquals("charge is not apportioned", false, charge1.JR_IsApportioned);
				AssertNoWarning(charge1.JR_SellGovtChargeCodeInfo, "Please enter a Sell Government Charge Code via Consolidation > Consol Costing tab.");
				AssertHasWarning(charge1.JR_SellGovtChargeCodeInfo, "Sell Government Charge Code is empty.");

				charge1.JR_SellGovtChargeCode = "AAA";
				AssertNoWarning(charge1.JR_SellGovtChargeCodeInfo, "Please enter a Sell Government Charge Code via Consolidation > Consol Costing tab.");
				AssertNoWarning(charge1.JR_SellGovtChargeCodeInfo, "Sell Government Charge Code is empty.");

				var overrideSellGovtChargeCodeSecurityRight = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellGovtCrgCode);
				overrideSellGovtChargeCodeSecurityRight.IsAllowed = false;

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;

				charge2.Validation.ValidateJR_SellGovtChargeCode();
				AssertHasError(charge2.JR_SellGovtChargeCodeInfo, "You have not been granted security rights to Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Allow Override of Invoice Sell Government Charge Code. Please enter a Sell Government Charge Code.");
				AssertNoError(charge2.JR_SellGovtChargeCodeInfo, "Please enter a Sell Government Charge Code.");

				overrideSellGovtChargeCodeSecurityRight.IsAllowed = true;
				charge2.Validation.ValidateJR_SellGovtChargeCode();
				AssertNoError(charge2.JR_SellGovtChargeCodeInfo, "You have not been granted security rights to Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Allow Override of Invoice Sell Government Charge Code. Please enter a Sell Government Charge Code.");
				AssertHasError(charge2.JR_SellGovtChargeCodeInfo, "Please enter a Sell Government Charge Code.");

				charge2.JR_SellGovtChargeCode = "CCC";
				AssertNoError(charge2.JR_SellGovtChargeCodeInfo, "You have not been granted security rights to Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Allow Override of Invoice Sell Government Charge Code. Please enter a Sell Government Charge Code.");
				AssertNoError(charge2.JR_SellGovtChargeCodeInfo, "Please enter a Sell Government Charge Code.");
			}
		}

		public void TestCheckJR_CostGovtChargeCodeMessage()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
				var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m);
				Factory.Save();

				AssertEquals("Pre-condition", 1, job.Charges.Count);
				var charge1 = job.Charges[0];

				charge1.Validation.ValidateJR_CostGovtChargeCode();
				AssertEquals("charge is apportioned", true, charge1.JR_IsApportioned);
				AssertHasWarning(charge1.JR_CostGovtChargeCodeInfo, "Please enter a Cost Government Charge Code via Consolidation > Consol Costing tab.");
				AssertNoWarning(charge1.JR_CostGovtChargeCodeInfo, "Cost Government Charge Code is empty.");

				charge1.JR_E6 = ZGuid.Empty;
				charge1.Validation.ValidateJR_CostGovtChargeCode();
				AssertEquals("charge is not apportioned", false, charge1.JR_IsApportioned);
				AssertNoWarning(charge1.JR_CostGovtChargeCodeInfo, "Please enter a Cost Government Charge Code via Consolidation > Consol Costing tab.");
				AssertHasWarning(charge1.JR_CostGovtChargeCodeInfo, "Cost Government Charge Code is empty.");

				charge1.JR_CostGovtChargeCode = "AAA";
				AssertNoWarning(charge1.JR_CostGovtChargeCodeInfo, "Please enter a Cost Government Charge Code via Consolidation > Consol Costing tab.");
				AssertNoWarning(charge1.JR_CostGovtChargeCodeInfo, "Cost Government Charge Code is empty.");

				var overrideCostGovtChargeCodeSecurityRight = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostGovtCrgCode);
				overrideCostGovtChargeCodeSecurityRight.IsAllowed = false;

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

				charge2.Validation.ValidateJR_CostGovtChargeCode();
				AssertHasError(charge2.JR_CostGovtChargeCodeInfo, "You have not been granted security rights to Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Allow Override of Invoice Cost Government Charge Code. Please enter a Cost Government Charge Code.");
				AssertNoError(charge2.JR_CostGovtChargeCodeInfo, "Please enter a Cost Government Charge Code.");

				overrideCostGovtChargeCodeSecurityRight.IsAllowed = true;
				charge2.Validation.ValidateJR_CostGovtChargeCode();
				AssertNoError(charge2.JR_CostGovtChargeCodeInfo, "You have not been granted security rights to Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Allow Override of Invoice Cost Government Charge Code. Please enter a Cost Government Charge Code.");
				AssertHasError(charge2.JR_CostGovtChargeCodeInfo, "Please enter a Cost Government Charge Code.");

				charge2.JR_CostGovtChargeCode = "CCC";
				AssertNoError(charge2.JR_CostGovtChargeCodeInfo, "You have not been granted security rights to Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Allow Override of Invoice Cost Government Charge Code. Please enter a Cost Government Charge Code.");
				AssertNoError(charge2.JR_CostGovtChargeCodeInfo, "Please enter a Cost Government Charge Code.");
			}
		}

		protected override Type GetExpectedBusinessObjectType() => typeof(Charge);

		#region Validate All

		public void TestValidateAll()
		{
			string warningText = "Any changes will require authorization from your supervisor or accountant upon saving and/or posting.";

			DummyParent parent = Factory.New<DummyParent>();
			new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();

			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = true;
			Charge charge = ((Job)parent.Job).Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			charge.Validation.ValidateAll();

			AssertEquals("Expected to have the '" + warningText + "' warning when locked.", true, HasWarning(charge, warningText));

			parent = Factory.New<DummyParent>();
			new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();

			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = false;
			charge = ((Job)parent.Job).Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			charge.Validation.ValidateAll();

			AssertEquals("NOT Expected to have the '" + warningText + "' warning when NOT locked.", false, HasWarning(charge, warningText));

			charge.Calculations.SuspendCalculations();
			charge.JR_LocalSellAmt = 50;
			charge.JR_OSSellAmt = 0;

			charge.Validation.ValidateAll();
			AssertHasRowErrorContaining(charge, "OS sell amount should be same as local sell amount when sell currency is local currency.");

			charge.JR_OSSellAmt = 50;
			charge.Validation.ValidateAll();
			AssertNoRowErrorContaining(charge, "OS sell amount should be same as local sell amount when sell currency is local currency.");

			charge.JR_LocalCostAmt = 50;
			charge.JR_OSCostAmt = 0;
			charge.Validation.ValidateAll();
			AssertHasRowErrorContaining(charge, "OS cost amount should be same as local cost amount when cost currency is local currency.");

			charge.JR_OSCostAmt = 50;
			charge.Validation.ValidateAll();
			AssertNoRowErrorContaining(charge, "OS cost amount should be same as local cost amount when cost currency is local currency.");

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var apline = TestObjectCreator.CreateInvoiceLine(apInvoice, 50);
			charge.JR_AL_APLine = apline.PK;

			AssertEquals("Precondition: Cost is posted", true, charge.IsCostPosted);
			charge.JR_OSCostAmt = 0;
			charge.Validation.ValidateAll();
			AssertNoRowErrorContaining(charge, "OS cost amount should be same as local cost amount when cost currency is local currency.");
			AssertNoRowErrorContaining(charge, "OS sell amount should be same as local sell amount when sell currency is local currency.");

			charge.JR_OSSellAmt = 0;
			charge.Validation.ValidateAll();
			AssertHasRowErrorContaining(charge, "OS sell amount should be same as local sell amount when sell currency is local currency.");

			charge.JR_OSSellAmt = 50;
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var arLine = TestObjectCreator.CreateInvoiceLine(arInvoice, 50);
			charge.JR_AL_ARLine = arLine.PK;

			AssertEquals("Precondition: Revenue is posted", true, charge.IsRevenuePosted);
			charge.JR_OSSellAmt = 0;
			charge.Validation.ValidateAll();
			AssertNoRowErrorContaining(charge, "OS sell amount should be same as local sell amount when sell currency is local currency.");
			AssertNoRowError(charge, "Should not have any error");
		}

		public void TestValidateAllWhenNonDecimalPlacesLocalAmountIsConvertedFromButNotEqualToOsAmount()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";

			DummyParent parent = Factory.New<DummyParent>();
			Job job = new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();
			job.Company.GC_RX_NKLocalCurrency = "JPY";

			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = false;
			Charge charge = ((Job)parent.Job).Charges.AddNew();
			charge.Calculations.SuspendCalculations();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostExRate = 1m;
			charge.JR_LocalCostAmt = 123m;
			charge.JR_OSCostAmt = 123.4m;

			charge.Validation.ValidateAll();
			AssertNoRowErrorContaining(charge, "OS cost amount should be same as local cost amount when cost currency is local currency.");
		}

		public void TestValidateAllWhenNonDecimalPlacesLocalSellAmountIsConvertedFromButNotEqualToOsSellAmount()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";

			DummyParent parent = Factory.New<DummyParent>();
			Job job = new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();
			job.Company.GC_RX_NKLocalCurrency = "JPY";

			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = false;
			Charge charge = ((Job)parent.Job).Charges.AddNew();
			charge.Calculations.SuspendCalculations();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSSellExRate = 1m;
			charge.JR_LocalSellAmt = 123m;
			charge.JR_OSSellAmt = 123.4m;

			charge.Validation.ValidateAll();
			AssertNoRowErrorContaining(charge, "OS sell amount should be same as local sell amount when sell currency is local currency.");
		}

		public void TestValidateAllWhenOSSellCurrencyChangedButOSSellAmountNotRecalculated()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "CNY";

			DummyParent parent = Factory.New<DummyParent>();
			Job job = new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();
			job.Company.GC_RX_NKLocalCurrency = "AUD";

			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = false;
			Charge charge = ((Job)parent.Job).Charges.AddNew();
			charge.Calculations.SuspendCalculations();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSSellExRate = 2m;
			charge.JR_LocalSellAmt = 123m;
			charge.JR_OSSellAmt = 246m;

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			charge.Validation.ValidateAll();
			AssertHasRowErrorContaining(charge, "OS sell amount should be same as local sell amount when sell currency is local currency.");
		}

		public void TestValidateAllWhenOSCostCurrencyChangedButOSCostAmountNotRecalculated()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "CNY";

			DummyParent parent = Factory.New<DummyParent>();
			Job job = new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();
			job.Company.GC_RX_NKLocalCurrency = "AUD";

			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = false;
			Charge charge = ((Job)parent.Job).Charges.AddNew();
			charge.Calculations.SuspendCalculations();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostExRate = 2m;
			charge.JR_LocalCostAmt = 123m;
			charge.JR_OSCostAmt = 246m;

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			charge.Validation.ValidateAll();
			AssertHasRowErrorContaining(charge, "OS cost amount should be same as local cost amount when cost currency is local currency.");
		}

		public void TestCheckJR_InvoiceType_NONTypePermissions()
		{
			DummyParent parent = Factory.New<DummyParent>();
			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).AllowUseOfNONInvoiceType.IsAllowed = false;
			Job job = new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			charge.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
			string expectedError = @"You do not have sufficient security rights to change the invoice type to 'NON - Do Not Post.
Contact your system administrator for rights to set this charge to 'Not for Invoicing - Do not Post'.
This security right can be found in the following location: 'Root -> Inv -> InvAllowNONInvT'.";
			AssertHasError(charge.JR_InvoiceTypeInfo, expectedError);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertNoErrors(charge.JR_InvoiceTypeInfo);

			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).AllowUseOfNONInvoiceType.IsAllowed = true;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
			AssertNoErrors(charge.JR_InvoiceTypeInfo);
		}

		bool HasWarning(Charge charge, string text)
		{
			foreach (INotification notification in charge.RowWarnings)
			{
				if (text == notification.Message)
				{
					return true;
				}
			}
			return false;
		}

		class DummyParent : CommonShipment, IJobInvoicingPlugIn
		{
			public DummyParent(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
				Job header = factory.NewJobWithValidTestDataForTesting<Job>();
				header.Parent = this;
			}

			protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
			{
				return new DummyParentJobInvoicingSupporter(this);
			}
		}

		class DummyParentJobInvoicingSupporter : CommonShipmentInvoicingSupporter
		{
			public DummyParentJobInvoicingSupporter(DummyParent parent)
				: base(parent)
			{
			}

			public bool fEditSecurityLock;
			public override bool EditSecurityLock
			{
				get
				{
					return fEditSecurityLock;
				}
			}

			protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
			{
				if (jobInvoivingSecurity == null)
				{
					var security = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
					jobInvoivingSecurity = new SecurityCheckpoint("Root", (NoResString)"Root", null, security, false);
					var invoicing = new SecurityCheckpoint(jobInvoivingSecurity.Code + SecurityCore.Invoicing, (NoResString)SecurityCore.Invoicing, jobInvoivingSecurity, null, false);
					allowUseOfNONInvoiceType = new SecurityCheckpoint(jobInvoivingSecurity.Code + SecurityCore.AllowUseOfNONInvoiceType, (NoResString)SecurityCore.AllowUseOfNONInvoiceType, invoicing, null, false);
				}

				return jobInvoivingSecurity;
			}

			SecurityCheckpoint jobInvoivingSecurity;
			SecurityCheckpoint allowUseOfNONInvoiceType;

			public SecurityCheckpoint AllowUseOfNONInvoiceType
			{
				get { return JobInvoicingSecurity != null ? allowUseOfNONInvoiceType : null; }
			}
		}

		#endregion

		#region Implementation

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			fTestBank = Factory.NewWithValidTestData<AccBankAccount>();
			fTestBank.AB_ChequeNumDigits = (ZByte)5;
			fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_AB = fTestBank.PK;
			fTestChequeBook.AK_StartNo = 10000;
			fTestChequeBook.AK_LastNo = 99999;
		}

		AccBankAccount fTestBank;
		AccChequeBook fTestChequeBook;

		#endregion

		#region Comment Charge

		protected AccChargeCode fCommentCharge;
		protected AccChargeCode CommentCharge
		{
			get
			{
				if (fCommentCharge == null)
				{
					fCommentCharge = CreateChargeCode("CMT1", "Comment Charge Code 1", Constants.ChargeType.Comment, 0, null, null);
				}
				return fCommentCharge;
			}
		}

		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "ZZ" + code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			if (gST != null)
			{
				chargeCode.AC_AT_GSTRate = gST.PK;
			}

			if (wHT != null)
			{
				chargeCode.AC_AW_WithholdingTaxRate = wHT.PK;
			}

			return chargeCode;
		}

		#endregion

		#region PrepareForTestChequeNumberInUse

		void PrepareForTestChequeNumberInUse(TestObjectCreator creator)
		{
			Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			Charge charge = creator.CreateCharge(testJob, creator.CC1, "Charge 1", creator.AUD, 1m, creator.Creditor1, creator.AUD, 1m, creator.Agent);
			charge.JR_RX_NKCostCurrency = creator.AUD.RX_Code;
			charge.JR_OSCostExRate = 1m;
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_AB = fTestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 5;
			charge.JR_AK = fTestChequeBook.PK;
			charge.JR_ChequeNo = "10001";
			Assert("Cheque number is not used yet", !charge.JR_ChequeNoInfo.HasErrors());

			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = creator.ABIGAS.PK;
			paymentApproval.AV_AB = fTestBank.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_AK = fTestChequeBook.PK;
			paymentApproval.AV_ChequeOrReference = "10002";
			Assert("Cheque number is not used yet", !paymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_AK = fTestChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "10003";
			Assert("Cheque number is not used yet", !hotCheque.AQ_ChequeNumberInfo.HasErrors());

			Payment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_AB = fTestBank.PK;
			payment.ChequeBook = fTestChequeBook.PK;
			payment.AH_ChequeOrReference = "10004";
			Assert("Cheque number is not used yet", !payment.AH_ChequeOrReferenceInfo.HasErrors());

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			directPayment.AH_AB = fTestBank.PK;
			directPayment.ChequeBookPK = fTestChequeBook.PK;
			directPayment.AH_ChequeOrReference = "10005";
			Assert("Cheque number is not used yet", !directPayment.AH_ChequeOrReferenceInfo.HasErrors());

			Payment reversedPayment = Factory.NewWithValidTestData<APPayment>();
			reversedPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			reversedPayment.AH_AB = fTestBank.PK;
			reversedPayment.ChequeBook = fTestChequeBook.PK;
			reversedPayment.AH_ChequeOrReference = "10006";
			reversedPayment.AH_IsCancelled = true;
			((IMatching)reversedPayment).CurrentMatchGroup.AddNew().AP_AH = reversedPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(reversedPayment);
			Factory.Save();
		}

		#endregion

		protected GlbDepartment NonMiscDepartment
		{
			get { return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)); }
		}

		protected GlbDepartment NonMiscDepartment1
		{
			get
			{
				return Factory.LoadTop1<GlbDepartment>(new ZQuery(new ZQuery(GlbDepartmentSchema.GE_Misc, false),
					  new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, NonMiscDepartment.PK)));
			}
		}

		protected GlbDepartment MiscDepartment
		{
			get { return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, true)); }
		}

		protected override BaseCharge GetNewParentBusinessObject(BusinessObjectFactory factory)
		{
			return factory.New<Charge>();
		}

		#endregion

		Charge SetupJobWithCharge(ZString invoiceType, ZString currency, ZGuid taxId, string cashAdvanceStatus = null, bool isCancelled = false)
		{
			var testJob = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var testCharge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, 100M, 100M);
			testCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			testCharge.JR_OSSellAmt = 100m;
			testCharge.JR_RX_NKSellCurrency = currency;
			testCharge.JR_InvoiceType = invoiceType;
			testCharge.JR_IsARCashAdvance = true;
			testCharge.JR_AT_SellGSTRate = taxId;
			AssertNoErrors("Should not have errors", testCharge.JR_IsARCashAdvanceInfo);
			AssertNoErrors("Should not have errors", testCharge.JR_OSSellAmtInfo);

			var cashAdvanceHeader = TestObjectCreator.CreateCashAdvanceRequestHeader(testJob, TestObjectCreator.Debtor1, LedgerTypes.AccountsReceivable, testCharge.TotalLocalRevenueAmount, testCharge.JR_Calc_OSSellAmtWithGST, "AUD");
			var cashAdvanceLine = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader, testCharge.TotalLocalRevenueAmount, testCharge.JR_Calc_OSSellAmtWithGST);
			if (cashAdvanceStatus != null)
			{
				cashAdvanceHeader.CAH_Status = cashAdvanceStatus;
				cashAdvanceLine.CAL_Status = cashAdvanceStatus;

				if (cashAdvanceStatus == CashAdvanceStatusCodes.RequestLine.Paid)
				{
					cashAdvanceLine.CAL_LocalPaidAmount = 90m;
					cashAdvanceLine.CAL_OSPaidAmount = 90m;
				}
			}

			testCharge.JR_CAL_ARLine = cashAdvanceLine.PK;
			AssertNotNull(testCharge.ARCashAdvanceRequestHeader);
			AssertEquals(cashAdvanceHeader.PK, testCharge.ARCashAdvanceRequestHeader.PK);
			AssertNotNull(testCharge.ARCashAdvanceRequestLine);
			AssertEquals(cashAdvanceLine.PK, testCharge.ARCashAdvanceRequestLine.PK);
			AssertNoErrors("Should not have errors", testCharge.JR_InvoiceTypeInfo);
			Factory.Save();
			if (isCancelled)
			{
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
				cashAdvanceLine.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
				Factory.Save();
			}

			return testCharge;
		}

		void AssertChangeChargeAmtWithARCashAdvanceRequest(ZString cashAdvanceStatus)
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.ForeignCurrencyInvoice, "USD", TestObjectCreator.GST1.PK, cashAdvanceStatus);

				testCharge.JR_OSSellAmt = 110m;
				AssertNoWarnings("Should not have a warning", testCharge.JR_OSSellAmtInfo);
				testCharge.JR_OSSellAmt = 85m;
				Assert(testCharge.JR_OSSellAmtInfo.HasWarning($"This charge has an active Advance Payment Request for {testCharge.ARCashAdvanceRequestHeader.Lines[0].CAL_OSAmount} {testCharge.ARCashAdvanceRequestHeader.CAH_RX_NKTransactionCurrency}. Please review that the charge amount is correct."));
				AssertNoWarnings("Should not have a warning", testCharge.JR_LocalSellAmtInfo);
				testCharge.JR_OSSellAmt = 100m;
				AssertNoWarnings("Should not have a warning", testCharge.JR_OSSellAmtInfo);

				using (AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					testCharge = SetupJobWithCharge(InvoiceTypesList.Codes.FinalInvoice, "AUD", TestObjectCreator.GST1.PK, cashAdvanceStatus);

					testCharge.JR_LocalSellAmt = 110m;
					AssertNoWarnings("Should not have a warning", testCharge.JR_LocalSellAmtInfo);
					testCharge.JR_LocalSellAmt = 85m;
					Assert(testCharge.JR_LocalSellAmtInfo.HasWarning($"This charge has an active Advance Payment Request for {testCharge.ARCashAdvanceRequestHeader.Lines[0].CAL_LocalAmount} {testCharge.ARCashAdvanceRequestHeader.CAH_RX_NKTransactionCurrency}. Please review that the charge amount is correct."));
					AssertNoWarnings("Should not have a warning", testCharge.JR_OSSellAmtInfo);
					testCharge.JR_LocalSellAmt = 100;
					AssertNoWarnings("Should not have a warning", testCharge.JR_LocalSellAmtInfo);
				}
			}
		}
	}
}
