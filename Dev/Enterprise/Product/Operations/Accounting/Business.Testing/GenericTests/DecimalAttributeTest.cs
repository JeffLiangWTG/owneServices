using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class DecimalAttributeTest : TestCaseWithFactory
	{
		#region Public ZDecimals have DecimalPlaceAttribute

		[ExpectNoExceptions]
		public void TestDecimalsHaveAttribute()
		{
			var alreadyChecked = new List<PropertyTypeHighestTrio>();
			foreach (var currentType in typesToTest)
			{
				if (ShouldTestForDecimals(currentType))
				{
					var publicPropertiesToTest = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(info =>
						info.PropertyType == typeof(ZDecimal)
						&& !info.Name.EndsWith("_ForTestOnly")
						&& !Attribute.IsDefined(info, typeof(DecimalPlacesAttribute))
						&& PropertyExclusions.All(classProperty => classProperty.className != GetHighestType(info, currentType).Name && classProperty.propertyName != info.Name)
						&& alreadyChecked.All(propertyClass => propertyClass.PropertyName != info.Name && propertyClass.HighestType != GetHighestType(info, currentType))).ToList();

					publicPropertiesToTest.ForEach(info => alreadyChecked.Add(new PropertyTypeHighestTrio(info.Name, currentType, GetHighestType(info, currentType))));

					var type = currentType;
					while (type != null && !type.Name.StartsWith("Auto"))
					{
						var interfaceProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Where(info =>
							info.PropertyType == typeof(ZDecimal)
							&& !info.Name.EndsWith("_ForTestOnly")
							&& info.Name.Contains(".")
							&& !Attribute.IsDefined(info, typeof(DecimalPlacesAttribute))
							&& PropertyExclusions.All(classProperty => classProperty.className != type.Name && classProperty.propertyName != info.Name)
							&& alreadyChecked.All(propertyClass => propertyClass.PropertyName != info.Name && propertyClass.HighestType != GetHighestType(info, currentType))).ToList();

						interfaceProperties.ForEach(info => alreadyChecked.Add(new PropertyTypeHighestTrio(info.Name, currentType, GetHighestType(info, currentType))));

						type = type.BaseType;
					}
				}
			}

			if (alreadyChecked.Any())
			{
				var result = string.Join("\r\n", alreadyChecked.Select(propertyClass => string.Format(@"{0}({1}) - {2}", propertyClass.DeclaringType, propertyClass.HighestType.Name, propertyClass.PropertyName)).ToList());
				throw new Exception("\r\nThese Properties need a DecimalPlaces attribute:\r\n\r\nClass owning the property(Highest Non-Auto Class in Hierarchy) - Property Name\r\n\r\n" + result);
			}
		}

		PropertyTypeHighestTrio cachedHighest;

		readonly List<Type> typesToTest = AssembliesToUse.Select(asm => Assembly.LoadFrom(asm).GetTypes()).SelectMany(x => x).ToList();

		#region Assemblies

		//Add to here to test a new library
		static readonly List<string> AssembliesToUse = new List<string> {
			"Enterprise.Accounting.Business.dll",
			"Enterprise.Accounting.Business.XmlSerializers.dll",
			"Enterprise.Accounting.DataTransfer.dll",
			"Enterprise.Accounting.DataTransfer.XmlSerializers.dll",
			"Enterprise.Accounting.Export.dll",
			"Enterprise.Accounting.GUI.dll",
			"Enterprise.Accounting.Integration.dll",
			"Enterprise.Accounting.Module.dll",
			"Enterprise.Accounting.ReportTableProviders.dll",
			"Enterprise.Accounting.ServiceTasks.dll",
			"Enterprise.Accounting.Web.dll"
		};

		#endregion

		#region Class Exclusions

		//Add class name here to exclude a class from the test
		static readonly List<string> ClassExclusions = new List<string> {
			"NettingCalculation",
			"NettingFXOffer",
			"NettingPayableTransaction",
			"NettingPayableTransactionLine",
			"NettingReceivableTransaction",
			"NettingReceivableTransactionLine",
			"NettingSystemExchangeRate"
		};

		#endregion

		#region Property Exclusions

		//Add new ClassPropertyPair(class name, property name) here to exclude a property from the test
		static readonly List<ClassPropertyPair> PropertyExclusions = new List<ClassPropertyPair> {
			//These are to be excluded from the test
			new ClassPropertyPair("InvoicingLineBase", "AL_ExchangeRate_Decimals"),
			new ClassPropertyPair("TransactionLineSummary", "AL_ExchangeRate_Decimals"),
			new ClassPropertyPair("JobCharge", "JobChargeAttrib_UnroundedItemsToRate"),
			new ClassPropertyPair("JobCharge", "JobChargeAttrib_ItemsToRate"),
			new ClassPropertyPair("NoteJournalCurrency", "CurrentSellRate"),
			new ClassPropertyPair("NoteJournalCurrency", "CurrentBuyRate"),
			new ClassPropertyPair("NoteJournalCurrency", "CurrentCustomsRate"),

			//These are to be fixed in a future work item
			new ClassPropertyPair("AmountBasedMultiLevelAuthorisationRequirement", "Amount"),
			new ClassPropertyPair("CostVarianceApprovalAuthorisationRequirement", "Percentage"),
			new ClassPropertyPair("CostVarianceApprovalAuthorisationRequirement", "LocalCostAmount"),
			new ClassPropertyPair("CostVarianceApprovalAuthorisationRequirement", "TotalInvoiceVarianceAmount"),
			new ClassPropertyPair("JobProfitLossRequiringReasonParameters", "LossThreshold"),
			new ClassPropertyPair("JobProfitLossRequiringReasonParameters", "ProfitThreshold"),
			new ClassPropertyPair("AccountingJournalLine", "AL_ExchangeRate"),
			new ClassPropertyPair("AccountingJournalLine", "AL_LineAmount"),
			new ClassPropertyPair("AccountingJournalLine", "AL_OSAmount"),
			new ClassPropertyPair("AccountingJournalLine", "AL_GSTVAT"),
			new ClassPropertyPair("AccountingJournalLine", "AL_OSExTaxAmount"),
			new ClassPropertyPair("AccountingJournalLine", "AL_OSTaxAmount"),
			new ClassPropertyPair("SelectableCommissionLineGrouping`2", "EntitySelectedAmount"),
			new ClassPropertyPair("JobPaymentBasisView", "RateValue"),
			new ClassPropertyPair("ChargeWithAppendedDescription", "JR_OSSellAmt"),
			new ClassPropertyPair("ChargeWithAppendedDescription", "JR_LocalSellAmt"),
			new ClassPropertyPair("ChargeWithAppendedDescription", "JR_OSSellGSTAmt"),
			new ClassPropertyPair("ChargeWithAppendedDescription", "JR_OSCostAmt"),
			new ClassPropertyPair("ChargeWithAppendedDescription", "JR_LocalCostAmt"),
			new ClassPropertyPair("ChargeWithAppendedDescription", "JR_OSCostGSTAmt"),
			new ClassPropertyPair("OrganisationBalance", "TotalOutstanding"),
			new ClassPropertyPair("ConsolRevenue", "SellAmount"),
			new ClassPropertyPair("StorageFeeInvoicePayment", "Enterprise.Accounting.Business.JobInvoicing.IApportionedChargesHeader.FreeSpace"),
			new ClassPropertyPair("ChinaJournal", "OriginalAmount"),
			new ClassPropertyPair("Voucher", "DebitQuantity"),
			new ClassPropertyPair("Voucher", "DebitCurrencyAmount"),
			new ClassPropertyPair("Voucher", "DebitAmountLocalCurrency"),
			new ClassPropertyPair("Voucher", "CreditQuantity"),
			new ClassPropertyPair("Voucher", "CreditCurrencyAmount"),
			new ClassPropertyPair("Voucher", "CreditAmountLocalCurrency"),
			new ClassPropertyPair("Voucher", "ExRate"),
			new ClassPropertyPair("Voucher", "UnitPrice"),
			new ClassPropertyPair("GLBalance", "BalanceAmount"),
			new ClassPropertyPair("GLBalance", "BalanceAmountInternal"),
			new ClassPropertyPair("VoucherDataSource", "OSAmount"),
			new ClassPropertyPair("VoucherLine", "CreditAmount"),
			new ClassPropertyPair("VoucherLine", "DebitAmount"),
			new ClassPropertyPair("VoucherLine", "OSCreditAmount"),
			new ClassPropertyPair("VoucherLine", "OSDebitAmount"),
			new ClassPropertyPair("VoucherLine", "ForeignCurrencyAmount"),
			new ClassPropertyPair("VoucherLine", "ExchangeRate"),
			new ClassPropertyPair("VoucherLine", "OutstandingAmount"),
			new ClassPropertyPair("ARAPTransactions", "LocalBalance"),
			new ClassPropertyPair("ARAPTransactions", "OSBalance"),
			new ClassPropertyPair("ARAPTransactions", "LocalTransactionAmount"),
			new ClassPropertyPair("ARAPTransactions", "OSTransactionAmount"),
			new ClassPropertyPair("GLAccountBalancesAndMovements", "OpenQuantity"),
			new ClassPropertyPair("GLAccountBalancesAndMovements", "OpenBalanceCurrency"),
			new ClassPropertyPair("GLAccountBalancesAndMovements", "OpenBalanceLocalCurrency"),
			new ClassPropertyPair("GLAccountBalancesAndMovements", "EndQuantity"),
			new ClassPropertyPair("GLAccountBalancesAndMovements", "EndBalanceCurrency"),
			new ClassPropertyPair("GLAccountBalancesAndMovements", "EndBalanceLocalCurrency"),
			new ClassPropertyPair("ReportsData", "ReportItemValue"),
			new ClassPropertyPair("BankReconDirectReceipt", "Enterprise.Accounting.Business.Base.Interfaces.ITransaction.OverseasTotalAmount"),
			new ClassPropertyPair("BankReconDirectReceipt", "Enterprise.Accounting.Integration.IeNettTransaction.ExchangeRate"),
			new ClassPropertyPair("BankReconciliation", "Difference"),
			new ClassPropertyPair("BankTransfer", "BuyAmount"),
			new ClassPropertyPair("BankTransfer", "BuyExchangeRate"),
			new ClassPropertyPair("BankTransfer", "LocalAmount"),
			new ClassPropertyPair("BankTransfer", "LocalExchangeRate"),
			new ClassPropertyPair("BankTransfer", "SellExchangeRate"),
			new ClassPropertyPair("BankTransfer", "FinanceChargeOSAmount"),
			new ClassPropertyPair("BankTransfer", "FinanceChargeExchangeRate"),
			new ClassPropertyPair("BankTransfer", "FinanceChargeLocalAmount"),
			new ClassPropertyPair("BankTransfer", "FinanceChargeOSTaxAmount"),
			new ClassPropertyPair("BankTransfer", "FinanceChargeLocalTaxAmount"),
			new ClassPropertyPair("BankTransfer", "FinanceChargeOSTotal"),
			new ClassPropertyPair("BankTransfer", "FinanceChargeTotalLocalAmount"),
			new ClassPropertyPair("BankTransfer", "OverseasTotalAmount"),
			new ClassPropertyPair("CreditStatusBusinessObject", "CurrentDisbursementOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "FirstAgeingDisbursementOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "SecondAgeingDisbursementOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "ThirdAgeingDisbursementOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "TotalDisbursementOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "OnePeriodTotal"),
			new ClassPropertyPair("CreditStatusBusinessObject", "TwoPeriodTotal"),
			new ClassPropertyPair("CreditStatusBusinessObject", "ThreePeriodTotal"),
			new ClassPropertyPair("CreditStatusBusinessObject", "TotalOutstandingAmount"),
			new ClassPropertyPair("CreditStatusBusinessObject", "CurrentTotal"),
			new ClassPropertyPair("CreditStatusBusinessObject", "OM_ARTreatDisbursementsAsStandardValue"),
			new ClassPropertyPair("CreditStatusBusinessObject", "LastPurchaseSale"),
			new ClassPropertyPair("CreditStatusBusinessObject", "LastPaymentReceipt"),
			new ClassPropertyPair("CreditStatusBusinessObject", "MTD_PTDSales"),
			new ClassPropertyPair("CreditStatusBusinessObject", "YTDPurchaseSales"),
			new ClassPropertyPair("CreditStatusBusinessObject", "LYRPurchaseSales"),
			new ClassPropertyPair("CreditStatusBusinessObject", "CreditBalance"),
			new ClassPropertyPair("CreditStatusBusinessObject", "CreditLimit"),
			new ClassPropertyPair("CreditStatusBusinessObject", "CurrentStandardOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "FirstAgeingStandardOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "SecondAgeingStandardOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "ThirdAgeingStandardOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "TotalStandardOutstanding"),
			new ClassPropertyPair("CreditStatusBusinessObject", "PostedRevenue"),
			new ClassPropertyPair("CreditStatusBusinessObject", "UnrecognisedWIP"),
			new ClassPropertyPair("CreditStatusBusinessObject", "RecognisedWIP"),
			new ClassPropertyPair("CreditStatusBusinessObject", "SumOfTotalWIPAndRevenue"),
			new ClassPropertyPair("NettingClearingJournal", "TransactionAmount"),
			new ClassPropertyPair("NettingTransaction", "NettingSystemExchangeRate"),
			new ClassPropertyPair("NettingTransaction", "ParticipantExchangeRate"),
			new ClassPropertyPair("NettingTransaction", "NettingSystemAmount"),
			new ClassPropertyPair("NettingTransaction", "ParticipantAmount"),
			new ClassPropertyPair("NettingTransaction", "ReceivableAmount"),
			new ClassPropertyPair("NettingTransaction", "PayableAmount"),
			new ClassPropertyPair("NettingCurrency", "CrossExchangeRate"),
			new ClassPropertyPair("NettingCurrency", "ExchangeValue"),
			new ClassPropertyPair("NettingMovement", "MovementAmount"),
			new ClassPropertyPair("NettingMovement", "SignedMovementAmount"),
			new ClassPropertyPair("NettingMovement", "ReportingRate"),
			new ClassPropertyPair("NettingMovement", "NettingCurrencyReportingAmount"),
			new ClassPropertyPair("NettingMovement", "Dealtrate"),
			new ClassPropertyPair("NettingMovement", "NettingCurrencyDealtAmount"),
			new ClassPropertyPair("InvoicingLineTaxSummary", "LocalExTaxAmount"),
			new ClassPropertyPair("InvoicingLineTaxSummary", "LocalTaxAmount"),
			new ClassPropertyPair("InvoicingLineTaxSummary", "LocalTotalAmount"),
			new ClassPropertyPair("InvoicingLineTaxSummary", "TaxRate"),
			new ClassPropertyPair("APInvoiceCharges", "AH_OSTotalAmount"),
			new ClassPropertyPair("APInvoiceCharges", "AH_LocalExTaxAmount"),
			new ClassPropertyPair("APInvoiceCharges", "AH_LocalTaxAmount"),
			new ClassPropertyPair("ApportionSplitCharge", "GrossVolume"),
			new ClassPropertyPair("BaseCharge", "JR_ActualVolume"),
			new ClassPropertyPair("Charge", "Enterprise.MasterFiles.Business.IDefaultLandedCostInput.ExchangeRate"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.OSSellAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.LocalSellAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.OSSellTaxAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.LocalSellTaxAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.OSSellWHTAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.SellExchangeRate"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.InvoiceSellExchangeRate"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesPostingCharge.CFXAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesTaxAmountCalculation.OsExTaxAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesTaxAmountCalculation.LocalExTaxAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesTaxAmountCalculation.OsTaxAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.Posting.IReceivablesTaxAmountCalculation.LocalTaxAmount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.IApportionedCharge.TEUCount"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.IApportionedCharge.ExcessActualVolumeWeight"),
			new ClassPropertyPair("Charge", "Enterprise.Accounting.Business.JobInvoicing.IApportionedCharge.ExcessChargeableVolumeWeight"),
			new ClassPropertyPair("InvoiceDependentJob", "JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency"),
			new ClassPropertyPair("InvoiceDependentJob", "JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency"),
			new ClassPropertyPair("InvoiceDependentJob", "JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency"),
			new ClassPropertyPair("InvoiceDependentJob", "JH_RelatedInvoiceLinesTotalCost"),
			new ClassPropertyPair("InvoiceDependentJob", "JH_RelatedInvoiceLinesCostAmount"),
			new ClassPropertyPair("InvoiceDependentJob", "JH_RelatedInvoiceLinesCostTaxAmount"),
			new ClassPropertyPair("JobProfitLoss", "TotalAccrual"),
			new ClassPropertyPair("JobProfitLoss", "TotalAccrualExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalWIP"),
			new ClassPropertyPair("JobProfitLoss", "TotalWIPExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalCost"),
			new ClassPropertyPair("JobProfitLoss", "TotalCostExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalRevenue"),
			new ClassPropertyPair("JobProfitLoss", "TotalRevenueExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalLineAmount"),
			new ClassPropertyPair("JobProfitLoss", "TotalLineAmountExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalAccrualRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalAccrualRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalWIPRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalWIPRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalCostRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalCostRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalRevenueRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalRevenueRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalLineAmountRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalLineAmountRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalAccrualNotRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalAccrualNotRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalWIPNotRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalWIPNotRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalCostNotRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalCostNotRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalRevenueNotRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalRevenueNotRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalLineAmountNotRecognized"),
			new ClassPropertyPair("JobProfitLoss", "TotalLineAmountNotRecognizedExcludingDSB"),
			new ClassPropertyPair("JobProfitLoss", "TotalTaxExpenseRevenue"),
			new ClassPropertyPair("JobProfitLoss", "TotalTaxExpenseCost"),
			new ClassPropertyPair("ProfitShareShipmentDetail", "ProfitSharePerecent"),
			new ClassPropertyPair("DummyBaseBusinessObject", "Z0_AnotherDecimal"),
			new ClassPropertyPair("DummyBaseBusinessObject", "Z0_Decimal"),
			new ClassPropertyPair("DummyBaseBusinessObject", "Z0_Money"),
			new ClassPropertyPair("ApprovalRequestDetails", "MaxAmountToApprove"),
			new ClassPropertyPair("GLJournalLine", "Enterprise.Accounting.Business.Base.Interfaces.IDebitCreditAmounts.OSUnsignedLineAmount"),
			new ClassPropertyPair("GLJournalLine", "Enterprise.Accounting.Business.Base.Interfaces.IDebitCreditAmounts.LocalUnsignedLineAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "GoodsExTaxAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "GoodsTaxAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "ServiceExTaxAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "ServiceTaxAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "TotalExTaxAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "TotalTaxAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "GeneralLedgerAmountDR"),
			new ClassPropertyPair("AccComplianceReportLineBase", "GeneralLedgerAmountCR"),
			new ClassPropertyPair("AccComplianceReportLineBase", "TaxRecoverableAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "TaxNotRecoverableAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "TaxReverseChargeAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "TaxReverseChargeInputAmount"),
			new ClassPropertyPair("AccComplianceReportLineBase", "TaxReverseChargeOutputAmount"),
			new ClassPropertyPair("ContraRow", "Enterprise.Accounting.Business.Base.Transaction.IMatching.OSOutstandingAmount"),
			new ClassPropertyPair("ContraRow", "Enterprise.Accounting.Business.Base.Transaction.IMatching.OutstandingAmount"),
			new ClassPropertyPair("ContraRow", "Enterprise.Accounting.Business.Base.Transaction.IMatching.OriginalOutstandingAmount"),
			new ClassPropertyPair("ContraRow", "Enterprise.Accounting.Business.Base.Transaction.IMatching.OSPartialPaymentAmount"),
			new ClassPropertyPair("ContraRow", "Enterprise.Accounting.Business.Base.Transaction.IMatching.LocalPartialPaymentAmount"),
			new ClassPropertyPair("ContraRow", "Enterprise.Accounting.Business.Base.Transaction.IMatching.ExchangeRateAmount"),
			new ClassPropertyPair("Contra", "AH_ExchangeRateAmount"),
			new ClassPropertyPair("Contra", "AH_InvoiceAmount"),
			new ClassPropertyPair("Contra", "AH_OSTotal"),
			new ClassPropertyPair("Contra", "AH_Calc_RecBeforeContra"),
			new ClassPropertyPair("Contra", "AH_Calc_RecAfterContra"),
			new ClassPropertyPair("Contra", "AH_Calc_PayBeforeContra"),
			new ClassPropertyPair("Contra", "AH_Calc_PayAfterContra"),
			new ClassPropertyPair("Transfer", "AH_Calc_FromBeforeTransfer"),
			new ClassPropertyPair("Transfer", "AH_Calc_FromAfterTransfer"),
			new ClassPropertyPair("Transfer", "AH_Calc_ToBeforeTransfer"),
			new ClassPropertyPair("Transfer", "AH_Calc_ToAfterTransfer"),
			new ClassPropertyPair("ARReceiptBatchPoster", "SellExRate"),
			new ClassPropertyPair("ARReceiptBatchPoster", "ForeignCurrencyTotal"),
			new ClassPropertyPair("ARReceiptBatchPoster", "LocalCurrencyTotal"),
			new ClassPropertyPair("TransactionWithOverriddenBranchAndDepartment", "AH_OSTaxAmount"),
			new ClassPropertyPair("TransactionWithOverriddenBranchAndDepartment", "AH_OSExTaxAmount"),
			new ClassPropertyPair("CASSAdjustmentLine", "WeightChargePP"),
			new ClassPropertyPair("CASSAdjustmentLine", "ValuationChargePP"),
			new ClassPropertyPair("CASSAdjustmentLine", "ChargesDueCarrierPP"),
			new ClassPropertyPair("CASSAdjustmentLine", "ChargesDueAgentCC"),
			new ClassPropertyPair("CASSAdjustmentLine", "Commission"),
			new ClassPropertyPair("CASSAdjustmentLine", "Incentive"),
			new ClassPropertyPair("CASSAdjustmentLine", "Weight"),
			new ClassPropertyPair("CASSCostExportLine", "WeightChargePP_Original"),
			new ClassPropertyPair("CASSCostExportLine", "ValuationChargePP_Original"),
			new ClassPropertyPair("CASSCostExportLine", "ChargesDueCarrierPP_Original"),
			new ClassPropertyPair("CASSCostExportLine", "ChargesDueAgentCC_Original"),
			new ClassPropertyPair("CASSCostExportLine", "Commission_Original"),
			new ClassPropertyPair("CASSCostExportLine", "Discount_Original"),
			new ClassPropertyPair("CASSCostImportLine", "WeightCharges_Original"),
			new ClassPropertyPair("CASSCostImportLine", "ChargesDueCarrierCC_Original"),
			new ClassPropertyPair("CASSCostImportLine", "FeeAmount_Original"),
			new ClassPropertyPair("CASSCostImportLine", "HandlingCharges_Original"),
			new ClassPropertyPair("CASSCostImportLine", "StorageCharges_Original"),
			new ClassPropertyPair("CASSCostImportLine", "OtherCharge1Amount_Original"),
			new ClassPropertyPair("CASSCostImportLine", "OtherCharge2Amount_Original"),
			new ClassPropertyPair("CASSCostImportLine", "MiscellaneousChargesAmount_Original"),
			new ClassPropertyPair("PrintStatementForAccountMovement", "OpeningBalance"),
			new ClassPropertyPair("PrintStatementForAccountMovement", "ClosingBalance"),
			new ClassPropertyPair("PrintStatement", "OutStandingAmountGreaterThan"),
			new ClassPropertyPair("UniversalTransactionLineWrapper", "RecoverableGSTVATPercentage"),
			new ClassPropertyPair("UniversalTransactionLineWrapper", "OSGSTVATAmount"),
			new ClassPropertyPair("UniversalTransactionLineWrapper", "OSTotalAmount"),
			new ClassPropertyPair("UniversalTransactionLineWrapper", "OSWHTAmount"),
			new ClassPropertyPair("UniversalTransactionLineWrapper", "LocalGSTVATAmount"),
			new ClassPropertyPair("UniversalTransactionLineWrapper", "LocalWHTAmount"),
			new ClassPropertyPair("UniversalTransactionWrapper", "OSExGSTVATAmount"),
			new ClassPropertyPair("UniversalTransactionWrapper", "OSTotal"),
			new ClassPropertyPair("UniversalTransactionWrapper", "LocalExVATAmount"),
			new ClassPropertyPair("UniversalTransactionWrapper", "LocalVATAmount"),
			new ClassPropertyPair("UniversalTransactionWrapper", "LocalTotal"),
			new ClassPropertyPair("APInvoiceChargesApprovalRequestChargeDetails", "OSCostAmount"),
			new ClassPropertyPair("ARCreditNoteApprovalRequestChargeDetails", "OSSellAmount"),
			new ClassPropertyPair("ARCreditNoteApprovalRequestChargeDetails", "LocalSellAmount"),
			new ClassPropertyPair("InvoiceLineOverrideForEditingDescription", "AL_OverseasTotal"),
			new ClassPropertyPair("InvoiceLineOverrideForEditingDescription", "AL_LocalExTaxAmount"),
			new ClassPropertyPair("InvoiceLineOverrideForEditingDescription", "AL_LocalTaxAmount"),
			new ClassPropertyPair("InvoiceLineOverrideForEditingDescription", "AL_LocalTotalAmount"),
			new ClassPropertyPair("InvoiceLineOverrideForEditingDescription", "AL_LocalGSTAmount"),
			new ClassPropertyPair("InvoiceLineOverrideForEditingDescription", "AL_OSGSTAmount"),
			new ClassPropertyPair("PeriodicInvoiceBase", "OSExTaxAmount"),
			new ClassPropertyPair("PeriodicInvoiceBase", "OSTaxAmount"),
			new ClassPropertyPair("PeriodicInvoiceBase", "OSExtraTaxAmount"),
			new ClassPropertyPair("PeriodicInvoiceBase", "LocalExtraTaxAmount"),
			new ClassPropertyPair("IntercompanyCostsApportionment", "ForeignApportionedAmount"),
			new ClassPropertyPair("IntercompanyCostsApportionment", "ForeignGST"),
			new ClassPropertyPair("IntercompanyCostsApportionment", "CompanyLocalAmount"),
			new ClassPropertyPair("IntercompanyCostsApportionment", "CompanyLocalExchangeRate"),
			new ClassPropertyPair("IntercompanyCostsApportionment", "ApportionmentFactor"),
			new ClassPropertyPair("IntercompanyCostsApportionment", "LocalGST"),
			new ClassPropertyPair("IntercompanyCostsApportionmentInvoice", "ExpectedInvoiceTotal"),
			new ClassPropertyPair("IntercompanyCostsApportionmentInvoice", "InvoiceAmount"),
			new ClassPropertyPair("IntercompanyCostsApportionmentInvoice", "GSTAmount"),
			new ClassPropertyPair("IntercompanyCostsApportionmentInvoiceLine", "Tax"),
			new ClassPropertyPair("IntercompanyCostsApportionmentInvoiceLine", "GSTInclusiveAmount"),
			new ClassPropertyPair("IntercompanyCostsApportionmentInvoiceLine", "Total"),
			new ClassPropertyPair("IntercompanyCostsApportionmentInvoiceLine", "LocalTax"),
			new ClassPropertyPair("InvoicingBasePayLineMediator", "LineTotalPaidAmount"),
			new ClassPropertyPair("InvoicingBasePayLineMediator", "LineTotalLocalPaidAmount"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period1"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period2"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period3"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period4"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period5"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period6"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period7"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period8"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period9"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period10"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period11"),
			new ClassPropertyPair("AggregationDiscrepanciesCalculatorLine", "Period12"),
			new ClassPropertyPair("APBulkInvoicePoster", "ExpectedBatchTotal"),
			new ClassPropertyPair("APBulkInvoicePoster", "TotalLocalExTaxAmount"),
			new ClassPropertyPair("APBulkInvoicePoster", "TotalOSExTaxAmount"),
			new ClassPropertyPair("APBulkInvoicePoster", "TotalOSTaxAmount"),
			new ClassPropertyPair("APBulkInvoicePoster", "TotalOSAmount"),
			new ClassPropertyPair("APBulkInvoicePoster", "RetrievedAccrualTotal"),
			new ClassPropertyPair("InvoicingBaseBulkChargeImporter", "SelectedLocalTotal"),
			new ClassPropertyPair("InvoicingBaseBulkChargeImporter", "SelectedInvoiceCurrencyTotal"),
			new ClassPropertyPair("CASSBillingLine", "SystemWeight"),
			new ClassPropertyPair("CASSBillingLine", "CASSWeight"),
			new ClassPropertyPair("CASSBillingLine", "WeightDifference"),
			new ClassPropertyPair("CASSBillingLine", "WeightDifferenceMargin"),
			new ClassPropertyPair("CASSBillingLine", "SystemCostAccrualValue"),
			new ClassPropertyPair("CASSBillingLine", "SystemCostPostedValue"),
			new ClassPropertyPair("CASSBillingLine", "CASSCostValue"),
			new ClassPropertyPair("CASSBillingLine", "CASSCostValueInLocalCurrency"),
			new ClassPropertyPair("CASSBillingLine", "CASSCostValueInLocalCurrencyForDisplay"),
			new ClassPropertyPair("CASSBillingLine", "CASSRejectedClaimValueInLocalCurrencyForDisplay"),
			new ClassPropertyPair("CASSBillingLine", "CASSCostTaxValue"),
			new ClassPropertyPair("CASSBillingLine", "CASSCostAdjustedValue"),
			new ClassPropertyPair("CASSBillingLine", "CASSCostAdjustedValueInLocalCurrency"),
			new ClassPropertyPair("CASSBillingLine", "CASSCostTaxAdjustedValue"),
			new ClassPropertyPair("CASSBillingLine", "NetCASSCost"),
			new ClassPropertyPair("CASSBillingLine", "CostDifference"),
			new ClassPropertyPair("CASSBillingLine", "CostDifferenceMargin"),
			new ClassPropertyPair("InvoicingBaseBulkConsolCostImporter", "SelectedTotal"),
			new ClassPropertyPair("APInvoiceForBulkPoster", "Enterprise.Accounting.Business.Base.Transaction.ISupportMatchingOfMyLines.LineTotalPaidAmount"),
			new ClassPropertyPair("APInvoiceForBulkPoster", "Enterprise.Accounting.Business.Base.Transaction.ISupportMatchingOfMyLines.LineTotalLocalPaidAmount"),
			new ClassPropertyPair("APInvoiceForBulkPoster", "Enterprise.Accounting.Business.Base.Transaction.ISupportMatchingOfMyLines.LineTotalPaidAmountPosted"),
			new ClassPropertyPair("TransactionsPendingAllocation", "BatchLocalTotal"),
			new ClassPropertyPair("TransactionsPendingAllocation", "BatchLocalTaxTotal"),
			new ClassPropertyPair("APInvoiceLine", "Enterprise.Accounting.Business.JobInvoicing.IApportionedCharge.ChargeableUnits"),
			new ClassPropertyPair("APInvoiceLine", "Enterprise.Accounting.Business.JobInvoicing.IApportionedCharge.GrossWeight"),
			new ClassPropertyPair("APInvoiceLine", "Enterprise.Accounting.Business.JobInvoicing.IApportionedCharge.GrossVolume"),
			new ClassPropertyPair("CASSBilling", "TotalCASSCostValue"),
			new ClassPropertyPair("CASSBilling", "TotalCASSRejectedClaimValue"),
			new ClassPropertyPair("CASSBilling", "TotalSystemCostAccrualValue"),
			new ClassPropertyPair("CASSBilling", "TotalCostDifferenceValue"),
			new ClassPropertyPair("CASSBilling", "TotalNetCASSCostValue"),
			new ClassPropertyPair("CASSBilling", "TotalCASSCostAdjustedValue"),
			new ClassPropertyPair("CASSBilling", "TotalHiddenCASSCostValue"),
			new ClassPropertyPair("CASSBilling", "TotalHiddenCASSRejectedClaimValue"),
			new ClassPropertyPair("CASSBilling", "TotalHiddenSystemCostAccrualValue"),
			new ClassPropertyPair("CASSBilling", "TotalHiddenCostDifferenceValue"),
			new ClassPropertyPair("CASSBilling", "TotalHiddenNetCASSCostValue"),
			new ClassPropertyPair("CASSBilling", "TotalHiddenCASSCostAdjustedValue"),
			new ClassPropertyPair("CASSBilling", "TotalAllCASSCostValue"),
			new ClassPropertyPair("CASSBilling", "TotalAllCASSRejectedClaimValue"),
			new ClassPropertyPair("CASSBilling", "TotalAllSystemCostAccrualValue"),
			new ClassPropertyPair("CASSBilling", "TotalAllCostDifferenceValue"),
			new ClassPropertyPair("CASSBilling", "TotalAllNetCASSCostValue"),
			new ClassPropertyPair("CASSBilling", "TotalAllCASSCostAdjustedValue"),
			new ClassPropertyPair("Statement", "OutstandingAmountGreaterThan"),
			new ClassPropertyPair("PeriodicInvoiceSelectableJob", "JH_OSAmountForPeriodicBilling"),
			new ClassPropertyPair("PeriodicInvoiceSelectableJob", "JH_OSTaxAmountForPeriodicBilling"),
			new ClassPropertyPair("PeriodicInvoiceSelectableJob", "JH_LocalAmountForPeriodicBilling"),
			new ClassPropertyPair("PeriodicInvoiceSelectableJob", "JH_LocalTaxAmountForPeriodicBilling"),
			new ClassPropertyPair("PeriodicInvoiceSelectableJob", "JH_OSExtraTaxAmount"),
			new ClassPropertyPair("PeriodicInvoiceSelectableJob", "JH_LocalExtraTaxAmount"),
			new ClassPropertyPair("PrimaryOrgSelector", "TotalLocalOutstandingAmount"),
			new ClassPropertyPair("VoucherKingDeeK3", "CalculatedAmount"),
			new ClassPropertyPair("VoucherKingDeeK3", "CalculatedExchangeRate"),
			new ClassPropertyPair("JobChargeQuickCalculateBusinessObject", "SellRate"),
			new ClassPropertyPair("JobChargeQuickCalculateBusinessObject", "CostRate"),
			new ClassPropertyPair("JobChargeQuickCalculateBusinessObject", "Quantity"),
			new ClassPropertyPair("JobChargeQuickCalculateBusinessObject", "SellTotal"),
			new ClassPropertyPair("JobChargeQuickCalculateBusinessObject", "CostTotal"),
			new ClassPropertyPair("JobChargeQuickCalculateBusinessObject", "SellMinimum"),
			new ClassPropertyPair("JobChargeQuickCalculateBusinessObject", "CostMinimum"),
			new ClassPropertyPair("ContainerCalculationData", "Cost"),
			new ClassPropertyPair("ContainerCalculationData", "Sell"),
			new ClassPropertyPair("ProfitLossDetail", "ZY_Calc_LineAmount"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Revenue"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_WIP"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Cost"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Accrual"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_LineAmount"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_MarginPercentage"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Revenue_Recognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_WIP_Recognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Cost_Recognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Accrual_Recognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_LineAmount_Recognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_MarginPercentage_Recognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Revenue_NotRecognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_WIP_NotRecognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Cost_NotRecognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_Accrual_NotRecognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_LineAmount_NotRecognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_MarginPercentage_NotRecognized"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_TaxExpenseRevenue"),
			new ClassPropertyPair("ProfitLossSummaryDetail", "ZZ_Calc_TaxExpenseCost"),
			new ClassPropertyPair("JobManagementAmountFilter", "DefaultProperty"),
			new ClassPropertyPair("JobManagementAmountFilter", "Property"),
			new ClassPropertyPair("ProcessTask", "RelevantEstimateHours"),
			new ClassPropertyPair("ProcessTask", "StandardEstimateHours"),
			new ClassPropertyPair("ProcessTask", "ActualDurationHours"),
			new ClassPropertyPair("ProcessTask", "EstimatedTimeToCompleteHours"),
			new ClassPropertyPair("ProcessTask", "LowEstimatedDurationHours"),
			new ClassPropertyPair("ProcessTask", "HighEstimatedDurationHours"),
			new ClassPropertyPair("ProcessTask", "SuspendedDurationHours"),
			new ClassPropertyPair("ProcessTaskWithUTCAdapters", "P9_EstimateVariationFactor"),
			new ClassPropertyPair("DaysAndAmountOverdueModuleFilter", "AmountOverdue"),
			new ClassPropertyPair("AccComplianceReportLine", "AH_GSTAmount"),
			new ClassPropertyPair("AccComplianceReportLine", "SPVTaxAmount")
		};

		#endregion

		Type GetHighestType(PropertyInfo info, Type originalType)
		{
			if (cachedHighest.PropertyName == info.Name && cachedHighest.DeclaringType == originalType)
			{
				return cachedHighest.HighestType;
			}

			var tempType = originalType;
			var type = tempType.BaseType;
			while (type != null && !type.Name.StartsWith("Auto"))
			{
				var property = type.GetProperty(info.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property == null || Attribute.IsDefined(property, typeof(DecimalPlacesAttribute)))
				{
					break;
				}
				tempType = type;
				type = type.BaseType;
			}

			cachedHighest = new PropertyTypeHighestTrio(info.Name, originalType, tempType);
			return tempType;
		}

		bool ShouldTestForDecimals(Type type) => type.IsPublic
													 && ClassExclusions.All(className => className != type.Name)
													 && type.IsSubclassOf(typeof(BusinessObject))
													 && !type.IsAbstract;

		#region Structs

		struct PropertyTypeHighestTrio
		{
			public PropertyTypeHighestTrio(string propertyName, Type declaringType, Type highestType)
			{
				PropertyName = propertyName;
				DeclaringType = declaringType;
				HighestType = highestType;
			}

			public string PropertyName;

			public Type DeclaringType;

			public Type HighestType;
		}

		struct ClassPropertyPair
		{
			public ClassPropertyPair(string classType, string propertyType)
			{
				this.className = classType;
				this.propertyName = propertyType;
			}
			public string className;
			public string propertyName;
		}

		#endregion

		#endregion
	}
}
