using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Integration;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AccountingConfigurationRegistry))]
	class AccountingConfigurationRegistryTest : RegistryItemSetTestCaseWithFactory<AccountingConfigurationRegistry>
	{
		#region Reflection Based Tests

		public void TestCountrySpecificDefaultValueRegistryItemImpl_RegistriesMustAlsoSetCacheExpensiveDefaultValueOption()
			=> AccountingMasterFilesRegistryTest.CountrySpecificDefaultValueRegistryItemImpl_RegistriesMustAlsoSetCacheExpensiveDefaultValueOption(AllItems);

		#endregion

		public void TestEnableAutoJobRevenueJournals()
		{
			AssertEquals("Caption", "Enable Auto Job Revenue Journals", ItemSet.EnableAutoJobRevenueJournals.Caption);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobCostingDefaults, ItemSet.EnableAutoJobRevenueJournals.Category);
			AssertEquals("DefaultValue", "NON", ItemSet.EnableAutoJobRevenueJournals.DefaultValue);
			AssertEquals("Hint", @"When this registry is set to YES, Job Revenue Journals (JRJ) will be created as soon as charges that are linked to organization proxies that are branches of the same log in company.

When this registry is set to TAX, the Debtor or Creditor must also have the same GST/VAT registration as the branch's organization proxy.", ItemSet.EnableAutoJobRevenueJournals.Hint);
			AssertEquals("Name", "EnableAutoJobRevenueJournals", ItemSet.EnableAutoJobRevenueJournals.Name);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableAutoJobRevenueJournals.Storage);
		}

		public void TestPayablesCashAdvanceClearingAccount()
		{
			using (ItemSet.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.PayablesCashAdvanceClearingAccount,
				"PayablesCashAdvanceClearingAccount",
				AccountingConfigurationRegistry.Categories.Accounting_CashAdvance_Payables,
				"Payables Advance Payment Clearing Account",
				 @"Enter the GL account where funds paid out in advance of receipt of an AP invoice will be recorded until the invoice is received.",
				RegistryStorageFlags.System,
				ZGuid.Empty
				);
			}
		}

		public void TestEnableReceivablesCashAdvanceFunctionality()
		{
			TestGenericRegistryItem(ItemSet.EnableReceivablesCashAdvanceFunctionality,
				"EnableReceivablesCashAdvanceFunctionality",
				"Accounting/Advance Payments/Receivables",
				"Enable Advance Payment Functionality",
				 @"Advance Payment functionality allows the request of an advance payment for nominated charges on a job prior to work commencing on the job and prior to an AR Invoice being raised.
Set to Yes to enable this functionality.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableReceivablesCashAdvanceFunctionalityWhenFeatureIsEnabled()
		{
			var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateAdvancePaymentFeatureControlMock(true);
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				TestGenericRegistryItem(ItemSet.EnableReceivablesCashAdvanceFunctionality,
				"EnableReceivablesCashAdvanceFunctionality",
				"Accounting/Advance Payments/Receivables",
				"Enable Advance Payment Functionality",
				 @"Advance Payment functionality allows the request of an advance payment for nominated charges on a job prior to work commencing on the job and prior to an AR Invoice being raised.
Set to Yes to enable this functionality.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
			}
		}

		public void TestIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation()
		{
			using (ItemSet.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation,
					"IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation",
					"Accounting/Advance Payments/Receivables",
					"Include Advance Payment Requests in Credit Controlled Document Evaluation",
					 @"When set to Yes, Advance Payment Requests will be included when evaluating if Credit Controlled Documents on a job can be produced. Credit Controlled Documents will not be produced if there is any outstanding Advance Payment amount on a job.
Set this registry to No in order to allow Credit Controlled Documents to be produced on a job when there are still unpaid Advance Payment Requests.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
			}

			foreach (bool regValue in new bool[] { true, false })
			{
				using (ItemSet.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
				{
					var systemLevelDefaultValue = ItemSet.IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					AssertEquals(regValue, systemLevelDefaultValue);

					var companyLevelDefaultValue = ItemSet.IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
					AssertEquals(regValue, companyLevelDefaultValue);
				}
			}
		}

		public void TestAllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid()
		{
			using (ItemSet.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid,
					"AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid",
					"Accounting/Advance Payments/Receivables",
					"Allow Manual Setting of Advance Payment Request Status to Paid",
					 @"Set this registry to Yes to allow the status of a Advance Payment Request to be manually set to PAI - Paid in Full.
This registry should only be set to Yes when Receivables receipts are not recorded in CargoWise. 
When receivables receipts are recorded in CargoWise for Advance Payments, receipting of the Advance Payment will set the Advance Payment status to reflect that it is paid, create AR journals and the appropriate general ledger postings.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false);
			}
		}

		public void TestCashAdvanceRequestDocumentTitle()
		{
			using (ItemSet.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.CashAdvanceRequestDocumentTitle,
				"CashAdvanceRequestDocumentTitle",
				"Accounting/Advance Payments/Receivables",
				"Advance Payment Request Document Title",
				 @"This title will display in the heading banner of the Advance Payment Request Document",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default);

				object value = ItemSet.CashAdvanceRequestDocumentTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
				Assert("Advance Payment Request", value is MultilingualString);
			}
		}

		public void TestCashAdvanceRequestDocumentMessage()
		{
			using (ItemSet.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.CashAdvanceRequestDocumentMessage,
				"CashAdvanceRequestDocumentMessage",
				"Accounting/Advance Payments/Receivables",
				"Advance Payment Request Document Message",
				 @"This message will appear in the footer of the Advance Payment Request Document",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default);

				object value = ItemSet.CashAdvanceRequestDocumentTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
				Assert("", value is MultilingualString);
			}
		}

		public void TestReceivablesCashAdvanceClearingAccount()
		{
			using (ItemSet.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.CashAdvanceClearingAccount,
				"CashAdvanceClearingAccount",
				AccountingConfigurationRegistry.Categories.Accounting_CashAdvance_Receivables,
				"Advance Payment Clearing Account",
				 @"Enter the GL account where received Advance Payments will be held until the work paid in advance is invoiced.",
				RegistryStorageFlags.System,
				ZGuid.Empty
				);
			}
		}

		public void TestReceivablesCashAdvanceRegistriesNotVisibleWhenCashAdvanceFunctionaltyDisabled()
		{
			ItemSet.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var receivablesCashAdvanceRegistries = AllItems.Where(p => p.Categories.Contains(AccountingConfigurationRegistry.Categories.Accounting_CashAdvance_Receivables)
			&& !p.Name.Equals("EnableReceivablesCashAdvanceFunctionality"));
			foreach (var registry in receivablesCashAdvanceRegistries)
			{
				AssertEquals(RegistryOptions.IsHidden, registry.Options);
			}
		}

		public void TestReceivablesCashAdvanceRegistriesVisibleWhenCashAdvanceFunctionaltyEnabled()
		{
			ItemSet.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var receivablesCashAdvanceRegistries = AllItems.Where(p => p.Categories.Contains(AccountingConfigurationRegistry.Categories.Accounting_CashAdvance_Receivables)
			&& !p.Name.Equals("EnableReceivablesCashAdvanceFunctionality"));
			foreach (var registry in receivablesCashAdvanceRegistries)
			{
				AssertEquals(RegistryOptions.Default, registry.Options);
			}
		}

		public void TestEnablePayablesCashAdvanceFunctionality()
		{
			TestGenericRegistryItem(ItemSet.EnablePayablesCashAdvanceFunctionality,
				"EnablePayablesCashAdvanceFunctionality",
				"Accounting/Advance Payments/Payables",
				"Enable Payables Advance Payment Functionality",
				 @"Payables Advance Payment functionality allows you to record and action requests for advance payment for nominated job costs prior to receiving an AP invoice for the costs. 
Set to Yes to enable this functionality.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnablePayablesCashAdvanceFunctionalityWhenFeatureIsEnabled()
		{
			var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateAdvancePaymentFeatureControlMock(true);
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				TestGenericRegistryItem(ItemSet.EnablePayablesCashAdvanceFunctionality,
				"EnablePayablesCashAdvanceFunctionality",
				"Accounting/Advance Payments/Payables",
				"Enable Payables Advance Payment Functionality",
				 @"Payables Advance Payment functionality allows you to record and action requests for advance payment for nominated job costs prior to receiving an AP invoice for the costs. 
Set to Yes to enable this functionality.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
			}
		}

		public void TestEnablePayablesInvoiceProcessingPortal()
		{
			TestGenericRegistryItem(ItemSet.EnablePayablesInvoiceProcessingPortal,
				"EnablePayablesInvoiceProcessingPortal",
				"Accounting/Payables Invoice Processing Portal",
				"Enable Payables Invoice Processing Portal (CargoWise Support Only)",
				@"This registry can be used to enable the new Payables Invoice Processing Portal for internal testing only.
Do not enable this functionality for any client.

Project reference: PRJ00042427",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestDraftTransactionStatusReasons()
		{
			var item = ItemSet.DraftTransactionStatusReasons;
			AssertEquals("DraftTransactionStatusReasons", item.Name);
			AssertEquals(Categories.Accounting_PayablesInvoiceProcessingPortal, item.Category);
			AssertEquals("Status Reasons", item.Caption);
			AssertEquals(@"This registry is used to maintain a set of default Status Reasons and determine to which of the following Statuses they can be applied.
ANL - Analyzing.
DFT - Draft.
DSC - Discarded.
DIS - In Dispute.
AFP - Approved for Posting.
AWA - Awaiting Approval.
PRS - Processed.
Tick the statuses to which the reason may be applied.

Reason Code OTH will allow users to enter a status reason description as free text. 
If it is a requirement that a pre-defined reason code and description is always used, ensure that all status checkboxes are unticked.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("DraftTransactionStatusReasons", TimeSpan.MinValue);
			using (Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.DraftTransactionStatusReasons.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("DraftTransactionStatusReasons", TimeSpan.MinValue);
			using (Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is IsOnlyForSupport", RegistryOptions.IsOnlyForSupport, ItemSet.DraftTransactionStatusReasons.Options);
			}
		}

		public void TestImportUniversalTransactionIntoPayableDraftInvoices()
		{
			TestGenericRegistryItem(ItemSet.EnableImportingUniversalTransactionIntoPayableDraftInvoices,
				"EnableImportingUniversalTransactionIntoPayableDraftInvoices",
				"Accounting/Payables Invoice Processing Portal",
				"Import Universal Transactions into Payables Draft Invoices (CargoWise Support Only)",
				@"This registry can be used to enable the incoming XUTs to be saved as draft invoices for internal testing only.
In addition to enabling this registry item, [Enable Payables Invoice Processing Portal (CargoWise Support Only)] should also be enabled to save incoming XUTs as draft invoices.
Do not enable this functionality for any client.

Project reference: PRJ00042427",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableNativeVueLandingPageForPayablesInvoiceProcessingPortal()
		{
			TestGenericRegistryItem(ItemSet.EnableNativeVueLandingPageForPayablesInvoiceProcessingPortal,
				"EnableNativeVueLandingPageForPayablesInvoiceProcessingPortal",
				"Accounting/Payables Invoice Processing Portal",
				"Enable Native Vue Landing Page For Payables Invoice Processing Portal (CargoWise Support Only)",
				@"This registry can be used to enable Native Vue Landing Page menu items of Payables Invoice Processing Portal for internal testing only.
Do not enable this functionality for any client.

Project reference: PRJ00047884",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestStatusReasonMandatory()
		{
			var item = ItemSet.StatusReasonMandatory;
			AssertEquals("StatusReasonMandatory", item.Name);
			AssertEquals(Categories.Accounting_PayablesInvoiceProcessingPortal, item.Category);
			AssertEquals("Status Reason Mandatory", item.Caption);
			AssertEquals(@"This registry determines whether it is mandatory to enter a reason or comment when changing the status of a draft transaction in the invoice processing portal.
By default, a Status Reason is not mandatory (checkbox unticked).
If it is mandatory to have a reason specified for the change to a particular Status, tick the checkbox for that status. 
A ticked checkbox will ensure that a draft transaction cannot be changed to this Status without a reason being selected or free text comment being entered.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);

			var editorInfo = (CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo;
			AssertEquals("Mandatory", editorInfo.BoolColumnCaption);
			AssertEquals("Status", editorInfo.CodeColumnCaption);
			AssertEquals(true, editorInfo.IsBoolColumnVisible);
			AssertEquals(true, editorInfo.IsCodeColumnVisible);
			AssertEquals(true, editorInfo.IsOnlyBoolColumnEditable);

			AssertEquals(4, item.DefaultValue.Count);
			AssertEquals(typeof(CodeDescriptionBoolDisallowNewCollection), item.DefaultValue.GetType());
			AccDraftInvoiceHeaderStatusList.Cast<CodeDescriptionPair>()
				.Where(item => new[] { AccDraftInvoiceHeaderStatus.AwaitingApproval, AccDraftInvoiceHeaderStatus.ApprovedForPosting, AccDraftInvoiceHeaderStatus.Discarded, AccDraftInvoiceHeaderStatus.InDispute, }.Contains(item.Code))
				.ForEach(expectedItem =>
				{
					AssertEquals(expectedItem.Description, item.DefaultValue.GetDescriptionFromCode(expectedItem.Code));
					AssertEquals(false, item.DefaultValue.GetBoolFromCode(expectedItem.Code));
				});

			ItemSet.RemoveItemFromCacheIfOlderThan("StatusReasonMandatory", TimeSpan.MinValue);
			using (Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.StatusReasonMandatory.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("StatusReasonMandatory", TimeSpan.MinValue);
			using (Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is IsOnlyForSupport", RegistryOptions.IsOnlyForSupport, ItemSet.StatusReasonMandatory.Options);
			}
		}

		public void TestRegistry_AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid()
		{
			using (ItemSet.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid,
					"AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid",
					"Accounting/Advance Payments/Payables",
					"Allow Manual Setting of Payables Advance Payment Request Status to Paid",
					@"Set this registry to Yes to allow the status of a Payables Advance Payment Request to be manually set to PAI - Paid in Full.
This registry should only be set to Yes when payables payments are not recorded in CargoWise. 
When payables payments are recorded in CargoWise for Advance Payments, payment of the Advance Payment will set the Advance Payment status to reflect that it is paid, create AP journals and the appropriate general ledger postings.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false);
			}
		}

		public void TestAmendingTransactionCopyExchangeRateFromOriginalTransaction()
		{
			AssertEquals("AmendingTransactionCopyExchangeRateFromOriginalTransaction", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransaction.Name);
			AssertEquals("Accounting/Receivable Defaults/Default Settings", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransaction.Category);
			AssertEquals("Amending Transaction Copy Exchange Rate from Original Transaction", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransaction.Caption);
			AssertEquals(@"By default, this registry is set to 'Yes' and the default exchange rate on foreign currency amending transactions is the exchange rate recorded on the original parent transaction.
When this registry is set to 'No', the default exchange rate on foreign currency amending transactions is the exchange rate configured according to the AR/AP Invoice Posting Exchange Rate Option and exchange rate source (rate type) according to the debtor's Job Billing Exchange Rates configuration.

This registry is relevant when posting Amending Receivable Transactions.

Note:  
It does NOT affect the creation of standalone Credit Notes or Reversals.
It is a company level registry configuration.", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransaction.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransaction.Storage);
			AssertEquals(true, ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransaction.DefaultValue);
		}

		public void TestAmendingTransactionCopyExchangeRateFromOriginalTransactionForAP()
		{
			AssertEquals("AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.Name);
			AssertEquals("Accounting/Payable Defaults/Default Settings", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.Category);
			AssertEquals("Amending AP Transaction Copy Exchange Rate from Original Transaction", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.Caption);
			AssertEquals(@"By default, this registry is set to 'Yes' and the default exchange rate on foreign currency amending transactions is the exchange rate recorded on the original parent transaction.
When this registry is set to 'No', the default exchange rate on foreign currency amending transactions is the exchange rate configured according to the AR/AP Invoice Posting Exchange Rate Option and exchange rate source (rate type) according to the creditor's Job Billing Exchange Rates configuration.

This registry is relevant when posting Amending AP Credit Notes.

Note:  
It does NOT affect the creation of standalone Credit Notes or Reversals.
It is a company level registry configuration.", ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.Storage);
			AssertEquals(true, ItemSet.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.DefaultValue);
		}

		public void TestInvoicePostingExchangeRateOptionAR()
		{
			AssertEquals("InvoicePostingExchangeRateOptionAR", ItemSet.InvoicePostingExchangeRateOptionAR.Name);
			AssertEquals("Accounting/Receivable Defaults/Default Settings", ItemSet.InvoicePostingExchangeRateOptionAR.Category);
			AssertEquals("AR Invoice Posting Exchange Rate Option", ItemSet.InvoicePostingExchangeRateOptionAR.Caption);
			AssertEquals(@"Use this registry to configure the exchange rate option to be used during the posting of job and non-job related Revenues.
NOTE: The configuration does not affect the application of job exchange rate during the creation of WIP's.

By default, this registry is set to DEF - Default behavior.

All Job level charges are posted as per exchange rate entered when posting in job invoicing module (including periodic invoices, etc).
All Agent Invoices are posted as per the exchange rate 'Calculation Method' selected during posting of overseas agent charges at consol level.
All Non-Job related charges posted via Receivables module are posted using the exchange rate entered during transaction entry.

To update the exchange rate during transaction posting, you can override the default behavior based on whether the the invoice is posted in LOC (Local) or FOR (Foreign) currency.
In the Exchange Rate Options, you can select;

TOD - Today's Exchange Rate.
Both Job and Non-Job related charges are posted using today's (creation date) exchange rate with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

INV - Exchange Rate based on Invoice Date.
Both Job and Non-Job related charges are posted using the exchange rate set for Invoice Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

PST - Exchange Rate based on Post Date.
Both Job and Non-Job related charges are posted using the exchange rate set for Post Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

EIT - Exchange Rate based on Earliest of Invoice Date and Tax Date.
Both Job and Non-Job related charges are posted using the exchange rate set on whichever is earliest of Invoice Date and Tax Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

You can also use the Offset column to offset exchange rates from a specific date selection (TOD, INV, PST or EIT) by the specified number of days. It can be before (-) or after (+) number of days. This field only accept integers.
When DEF option is used, exchange rate date and offset are determined based on the settings in the Job Billing Exchange Rate Configuration.

NOTE: In all cases, This registry only determines the Date of the exchange rate. Exchange rate TYPE is determined by the Job Billing Exchange Rate Configuration module or the relevant Organization.", ItemSet.InvoicePostingExchangeRateOptionAR.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.InvoicePostingExchangeRateOptionAR.Storage);
		}

		public void TestInvoicePostingExchangeRateOptionAP()
		{
			AssertEquals("InvoicePostingExchangeRateOptionAP", ItemSet.InvoicePostingExchangeRateOptionAP.Name);
			AssertEquals("Accounting/Payable Defaults/Default Settings", ItemSet.InvoicePostingExchangeRateOptionAP.Category);
			AssertEquals("AP Invoice Posting Exchange Rate Option", ItemSet.InvoicePostingExchangeRateOptionAP.Caption);
			AssertEquals(@"Use this registry to configure the exchange rate option to be used during the posting of job and non-job related Costs.
NOTE: The configuration does not affect the application of job exchange rate during the creation of Accruals.

By default, this registry is set to DEF - Default behavior.

All Job level costs are posted as per exchange rate entered when posting in job invoicing module.
All Job and Non-Job related costs posted via Payables module are posted according to the 'Use Job Exchange Rate' setting.

To update the exchange rate during transaction posting, you can override the default behavior based on whether the the invoice is posted in LOC (Local) or FOR (Foreign) currency.
In the Exchange Rate Options, you can select;

TOD - Today's Exchange Rate.
Both Job and Non-Job related costs are posted using today's (creation date) exchange rate with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

INV - Exchange Rate based on Invoice Date.
Both Job and Non-Job related costs are posted using the exchange rate set for Invoice Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

PST - Exchange Rate based on Post Date.
Both Job and Non-Job related costs are posted using the exchange rate set for Post Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

EIT - Exchange Rate based on Earliest of Invoice Date and Tax Date.
Both Job and Non-Job related costs are posted using the exchange rate set on whichever is earliest of Invoice Date and Tax Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

You can also use the Offset column to offset exchange rates from a specific date selection (TOD, INV, PST or EIT) by the specified number of days. It can be before (-) or after (+) number of days. This field only accept integers.
When DEF option is used, exchange rate date and offset are determined based on the settings in the Job Billing Exchange Rate Configuration.

NOTE: In all cases, This registry only determines the Date of the exchange rate. Exchange rate TYPE is determined by the Job Billing Exchange Rate Configuration module or the relevant Organization.", ItemSet.InvoicePostingExchangeRateOptionAP.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.InvoicePostingExchangeRateOptionAP.Storage);
		}

		public void TestInvoicePostingExchangeRateOption()
		{
			var companyPK = GlbCompany.CurrentCompany.PK;

			AssertEquals(RegistryOptions.Default, ItemSet.InvoicePostingExchangeRateOptionAR.Options);
			AssertEquals(RegistryOptions.Default, ItemSet.InvoicePostingExchangeRateOptionAP.Options);

			ItemSet.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			ItemSet.InvoicePostingExchangeRateOptionAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);

			AssertEquals(InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, true, companyPK));
			AssertEquals(InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, false, companyPK));
			AssertEquals(InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AP, true, companyPK));
			AssertEquals(InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AP, false, companyPK));

			var collectionAP = new InvoicePostingExRateOptionCollection();
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, InvoicePostingExchangeRateOption.TodayExchangeRate.Code, 0));
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, 0));

			var collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, 0));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, 0));

			ItemSet.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);
			ItemSet.InvoicePostingExchangeRateOptionAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAP);

			AssertEquals(InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, true, companyPK));
			AssertEquals(InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, false, companyPK));
			AssertEquals(InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AP, true, companyPK));
			AssertEquals(InvoicePostingExchangeRateOption.TodayExchangeRate.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AP, false, companyPK));

			AssertEquals(InvoicePostingExchangeRateOption.Default.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.None, false, companyPK));
			AssertEquals(InvoicePostingExchangeRateOption.Default.Code, ItemSet.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.None, true, companyPK));
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestEnableLineLevelApprovalRequestForARCreditNote()
		{
			AssertEquals("EnableLineLevelApprovalRequestForARCreditNote", ItemSet.EnableLineLevelApprovalRequestForARCreditNote.Name);
			AssertEquals("Accounting/Receivable Defaults/Default Settings/Credit Note And Invoice Reversal Authorization Settings", ItemSet.EnableLineLevelApprovalRequestForARCreditNote.Category);
			AssertEquals("Enable line-level credit note evaluation and approval", ItemSet.EnableLineLevelApprovalRequestForARCreditNote.Caption);
			AssertEquals(@"This registry allows you to enable evaluation and approval of credit notes at line level branch and department combination.
By default, this registry is set to NO. When posting credit notes, the system evaluates the required approval level, based on total amount of the credit note and the header branch and department.
Set this registry to YES if you want the system to additionally evaluate the required approval level, based on the total amounts for each branch and department combination at line level.
When enabled, the user will be able to post the credit note only if:
*They have sufficient approval level to header branch and department, based on the total amount of the credit.
*They have sufficient approval level to all branches and departments on the credit note, based on the total amount of each branch and department combination at line level.

For example, your system is configured to require Level 1 approval for all credit note amounts in branch B1, department D1; and Level 2 approval for credit notes in branch B2, department D2.
The user has Level 1 approval rights across all branches and departments.
This user is raising a credit note from the billing job with header branch B1 and department D1. The credit note contains a credit charge associated with branch B2 and department D2.
When this registry is set to NO, user is able to post the credit note. When this registry is set to YES, user is prevented from posting the credit note, since they don't have sufficient approval rights to post one of the line level charges.

If the posting user does not have sufficient approval level, the Security Override Login pop up screen is displayed and another user can provide on the spot authorization to post the credit note.
Alternatively, an approval request can be queued to be approved in the Credit Note Approval module. A separate approval request is created for the total amount of the credit note and the header level branch and department (parent request).
Additionally, a separate request is created for the total amount of each branch and department combination at line level (child requests).
Parent request can be approved only when all child requests are in the Approved status. Once parent request is Approved, then the credit note can be posted.
If any of the child requests gets Rejected, then the parent request is Rejected and the credit note cannot be posted.",
				ItemSet.EnableLineLevelApprovalRequestForARCreditNote.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EnableLineLevelApprovalRequestForARCreditNote.Storage);
			AssertEquals(false, ItemSet.EnableLineLevelApprovalRequestForARCreditNote.DefaultValue);
		}

		public void TestUseCurrentDateAsTransactionDateWhenAutoPosting()
		{
			TestGenericRegistryItem(ItemSet.UseCurrentDateAsTransactionDateWhenAutoPosting,
				"UseCurrentDateAsTransactionDateWhenAutoPosting",
				"Accounting/Receivable Defaults/Default Settings/Credit Note And Invoice Reversal Authorization Settings",
				"Use Current Date as Transaction Date When Auto Posting",
				 @"This registry applies when the Post Credit Note On Approval registry is set to Yes.

When this registry is set to Yes, when credit notes post on final approval, the transaction date of the credit note will be current date (the date of final approval).

When this registry is set to No, when the credit note posts (on final approval), the transaction date of the credit note will be the date that the approval request was created.

Note: This registry will be ignored when the 'Invoice and Post Dates Defaulting Behavior' registry is set to 'MTH'",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestCustomBranchDefaultingRulesEngineConfiguration()
		{
			TestGenericRegistryItem(ItemSet.CustomBranchDefaultingRulesEngineConfiguration,
				"CustomBranchDefaultingRulesEngineConfiguration",
				"Accounting/Job Invoicing",
				"Custom Branch Defaulting Rules Engine Configuration",
				 @"This registry will enable setting up Rules for Branch Defaulting in Rules Engine.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestCustomDepartmentDefaultingRuleEngineConfiguration()
		{
			TestGenericRegistryItem(ItemSet.CustomDepartmentDefaultingRuleEngineConfiguration,
				"CustomDepartmentDefaultingRuleEngineConfiguration",
				"Accounting/Job Invoicing/Default Departments",
				"Custom Department Defaulting Rules Engine Configuration",
				 @"This registry will enable setting up Rules for Department Defaulting in Rules Engine.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestGenerateARInvoiceAttachmentForEReporting()
		{
			AssertEquals("GenerateARInvoiceAttachmentForEReporting", ItemSet.GenerateARInvoiceAttachmentForEReporting.Name);
			AssertEquals("Accounting/E-Reporting and E-Invoicing Configurations", ItemSet.GenerateARInvoiceAttachmentForEReporting.Category);
			AssertEquals("Generate AR Invoice Attachment for E-Reporting", ItemSet.GenerateARInvoiceAttachmentForEReporting.Caption);
			AssertEquals(@"This registry allows you to configure the system to automatically generate AR Invoice document during the E-Reporting batching process.
When enabled, an AR Invoice document PDF will be generated and attached to eDocs of the transaction, while the invoice is being batched for E-Reporting. 

Please note that only eDocs marked as ‘Published’ are included in E-Reporting files. To ensure that the AR Invoice is included in your E-Reporting mapped files, please review the setup of the INV – Invoice document. 
To do this, navigate to Maintain > Reference Files > Document Types module, locate the invoice document (filter by Document Type = INV) and ensure the ‘Published’ check box is selected.

If you enable this registry for a login company, please ensure that this company does not have other workflow triggers that would generate additional Invoice document attachments.",
ItemSet.GenerateARInvoiceAttachmentForEReporting.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.GenerateARInvoiceAttachmentForEReporting.Storage);
			AssertEquals(false, ItemSet.GenerateARInvoiceAttachmentForEReporting.DefaultValue);
		}

		public void TestCommentChargeLineARInvoiceWarning()
		{
			AssertEquals("CommentChargeLineARInvoiceWarning", ItemSet.CommentChargeLineARInvoiceWarning.Name);
			AssertEquals("Accounting/Receivable Defaults/Default Settings", ItemSet.CommentChargeLineARInvoiceWarning.Category);
			AssertEquals("Comment Charge Line Validation", ItemSet.CommentChargeLineARInvoiceWarning.Caption);
			AssertEquals(@"Set the severity level relating to the Comment Charge on a Receivables Invoice. 
The default value for most login companies with be NON - No Action, meaning there will be no warning or error on validation of the Comment charge line.
Countries where the Comment charge line cannot be included in the transaction XML exported as part of the electronic invoicing requirements will set this to WRN (Warning Validation) or ERR (Error Validation).", ItemSet.CommentChargeLineARInvoiceWarning.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.CommentChargeLineARInvoiceWarning.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.CommentChargeLineARInvoiceWarning.Options);
		}

		public void TestCommentChargeLineARInvoiceWarningDefaultValue()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			Factory.Save();
			ResetRegistryCache();
			AssertEquals("Default Value for Turkey company should be WRN", CommentChargeLineARInvoiceWarningOptions.WarningValidation, ItemSet.CommentChargeLineARInvoiceWarning.GetValueWithoutFallback(company1.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			ResetRegistryCache();
			AssertEquals("Default Value for Australian company should be NON", CommentChargeLineARInvoiceWarningOptions.NoAction, ItemSet.CommentChargeLineARInvoiceWarning.GetValueWithoutFallback(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestPrintTaxDateInARInvoiceDocument()
		{
			var item = ItemSet.PrintTaxDateInARInvoiceDocument;
			AssertEquals(PrintTaxDateInARInvoiceDocumentOption.DoNotPrintTaxDate.Code, item.DefaultValue);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, item.Category);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Print Tax Date in AR Invoice Document", item.Caption);
			AssertEquals("Hint", @"This registry controls the ability to print the Tax Date (Date of Supply) in your AR Invoice. You can decide to print a single Tax Date in the Header or each charge line Tax Date in the body of the AR Invoice.
This is relevant when printing Receivables Tax Invoices, Tax Credit Notes and Tax Adjustment Notes.

By default, this Registry is set to 'NOT - Do Not Print Tax Date' and the receivables invoice document will not print the Date of Supply (Tax Date captured during transaction data entry) in the AR Invoice document. 

When overriding the value to 'HDR', the earliest Tax Date (Date of Supply) recorded against the charge lines of the transaction prints in the header of the AR Invoice document.

When overriding the value to 'HDT', the latest Tax Date (Date of Supply) recorded against the charge lines of the transaction prints in the header of the AR Invoice Document.

When overriding the value to 'BOD',  the Tax Date (Date of Supply) of each charge line prints at the beginning of the charge line description in the body of the AR Invoice Document.", item.Hint);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInBody.Code);
			AssertEquals(PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInBody.Code, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals(4, PrintTaxDateInARInvoiceDocumentOption.CodeList.Count);
			var expectedCodeList = new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(PrintTaxDateInARInvoiceDocumentOption.DoNotPrintTaxDate.Code, PrintTaxDateInARInvoiceDocumentOption.DoNotPrintTaxDate.Description),
				new CodeDescriptionPair(PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInBody.Code, PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInBody.Description),
				new CodeDescriptionPair(PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderEarliest.Code, PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderEarliest.Description),
				new CodeDescriptionPair(PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderLatest.Code, PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderLatest.Description)
			};
			AssertContainsExactElementsInAnyOrder(expectedCodeList.ToArray(), PrintTaxDateInARInvoiceDocumentOption.CodeList.ToArray());
		}

		public void TestDisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed()
		{
			AssertEquals("DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed", ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.Name);
			AssertEquals(Categories.Accounting, ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.Category);
			AssertEquals("Disallow reversing original transactions when amendments are not reversed", ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.Caption);
			AssertEquals(@"This registry is used to control the behavior of the system when reversing original transactions that have been amended through the 'Amend with Credit Note' or 'Amend with Invoice' action in the Billing Tab of a Job

By default, this registry is enabled for Portugal Login Companies.

When enabled, the system will validate that all the amending documents of an original invoice are canceled before allowing users to cancel the original invoice itself.", ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.Storage);
			AssertEquals(false, ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.DefaultValue);

			var portugalCompany = Factory.NewWithValidTestData<GlbCompany>();
			portugalCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Portugal;
			var australianCompany = Factory.NewWithValidTestData<GlbCompany>();
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;
			Factory.Save();

			AssertEquals("Default Value for Portugal company should be True", true, ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.GetValueWithoutFallback(portugalCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Default Value for Australian company should be False", false, ItemSet.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.GetValueWithoutFallback(australianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestTaxMessageIsMandatoryReceivables()
		{
			var item = ItemSet.TaxMessageIsMandatoryReceivables;
			AssertEquals(Constants.TaxMessageMandatoryOptionConstants.NotRequired, item.DefaultValue);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_ReceivableDefaults_DefaultSettings, item.Category);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Invoice Tax Message is Mandatory Receivables", item.Caption);
			AssertEquals("Hint", @"This registry allows you to enforce that a tax message must be recorded on receivable transactions and cash book direct receipt transactions. 

When set to NOT - Not Required, then Invoice Tax Message is not mandatory.
When set to RTZ - Required When Tax is Zero, users are prevented from posting transactions without recording a tax message when tax rate is zero.
When set to REQ - Required Always, users are prevented from posting transactions without recording a tax message, regardless of the tax rate.
When set to RET - Required When an Extra Tax element is Configured as part of the Tax ID, users are prevented from posting transactions without recording a tax message when the Tax ID includes an extra tax behavior.
When set to REZ - Required when Tax is Zero, or when Extra Tax element is Configured as part of  the Tax ID, users are prevented from posting transactions without recording a tax message when the tax rate is zero or Tax ID includes an extra tax behavior.", item.Hint);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.TaxMessageMandatoryOptionConstants.RequiredAlways);
			AssertEquals(Constants.TaxMessageMandatoryOptionConstants.RequiredAlways, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals("Count of Tax Message Mandatory Options", 5, TaxMessageMandatoryOptions.CodeList.Count);

			var codeList = new CodeDescriptionPairList();
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.NotRequired, "Not Required");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero, "Required When Tax is Zero");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredAlways, "Required Always");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero, "Required When Extra Tax Is Not Zero");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax, "Required When the Tax ID Has an Extra Tax Element or Required When Tax Is Zero");
			AssertArrayEqualsByElements(codeList.ToArray(), TaxMessageMandatoryOptions.CodeList.ToArray());

			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(ItemSet.TaxMessageIsMandatoryReceivables
				, x => x.TaxMessageIsMandatoryReceivables
				, ("REQ", "REQ"), ("REZ", "REZ")
				, "Invoice Tax Message is Mandatory Receivables"
				, startingCallCount: 0);
		}

		public void TestTaxMessageIsMandatoryPayables()
		{
			var item = ItemSet.TaxMessageIsMandatoryPayables;
			AssertEquals(Constants.TaxMessageMandatoryOptionConstants.NotRequired, item.DefaultValue);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_PayableDefaults_DefaultSettings, item.Category);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Invoice Tax Message is Mandatory Payables", item.Caption);
			AssertEquals("Hint", @"This registry allows you to enforce that a tax message must be recorded on payable transactions and cash book direct payment transactions. 

When set to NOT - Not Required, then Invoice Tax Message is not mandatory.
When set to RTZ - Required When Tax is Zero, users are prevented from posting transactions without recording a tax message when tax rate is zero.
When set to REQ - Required Always, users are prevented from posting transactions without recording a tax message, regardless of the tax rate.
When set to RET - Required When an Extra Tax element is Configured as part of the Tax ID, users are prevented from posting transactions without recording a tax message when the Tax ID includes an extra tax behavior.
When set to REZ - Required when Tax is Zero, or when Extra Tax element is Configured as part of  the Tax ID, users are prevented from posting transactions without recording a tax message when the tax rate is zero or Tax ID includes an extra tax behavior.", item.Hint);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.TaxMessageMandatoryOptionConstants.RequiredAlways);
			AssertEquals(Constants.TaxMessageMandatoryOptionConstants.RequiredAlways, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals("Count of Tax Message Mandatory Options", 5, TaxMessageMandatoryOptions.CodeList.Count);

			var codeList = new CodeDescriptionPairList();
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.NotRequired, "Not Required");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero, "Required When Tax is Zero");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredAlways, "Required Always");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero, "Required When Extra Tax Is Not Zero");
			codeList.AddPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax, "Required When the Tax ID Has an Extra Tax Element or Required When Tax Is Zero");
			AssertArrayEqualsByElements(codeList.ToArray(), TaxMessageMandatoryOptions.CodeList.ToArray());

			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(ItemSet.TaxMessageIsMandatoryPayables
				, x => x.TaxMessageIsMandatoryPayables
				, ("REQ", "REQ"), ("NOT", "NOT")
				, "Invoice Tax Message is Mandatory Payables"
				, startingCallCount: 0);
		}

		public void TestAgingOptionReceivables()
		{
			var item = ItemSet.AgingOptionReceivables;
			AssertEquals(AgingOptions.InvoiceDate, item.DefaultValue);
			AssertEquals(Categories.Accounting_ReceivableDefaults_DefaultSettings, item.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Aging Option in Receivables Enquiries", item.Caption);
			AssertEquals("Hint", @"Use this registry to set the default Aging Option in Receivables Enquiries. 

a. INV – Transactions will be aged by Invoice Date.
b. DUE – Transactions will be aged by Due Date.
c. PST - Transactions will be aged by Post Date. 

By default, this registry will be set to 'INV'.", item.Hint);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AgingOptions.PostDate);
			AssertEquals(AgingOptions.PostDate, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAgingOptionPayables()
		{
			var item = ItemSet.AgingOptionPayables;
			AssertEquals(AgingOptions.InvoiceDate, item.DefaultValue);
			AssertEquals(Categories.Accounting_PayableDefaults_DefaultSettings, item.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Aging Option in Payables Enquiries", item.Caption);
			AssertEquals("Hint", @"Use this registry to set the default Aging Option in Payables Enquiries. 

a. INV – Transactions will be aged by Invoice Date.
b. DUE – Transactions will be aged by Due Date.
c. PST - Transactions will be aged by Post Date. 

By default, this registry will be set to 'INV'.", item.Hint);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AgingOptions.PostDate);
			AssertEquals(AgingOptions.PostDate, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestEInvoicingErrorNotificationGroup()
		{
			AssertEquals("E-ReportingErrorNotificationGroup", ItemSet.EInvoicingErrorNotificationGroup.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.EInvoicingErrorNotificationGroup.Category);
			AssertEquals("E-Reporting Error Notification Group", ItemSet.EInvoicingErrorNotificationGroup.Caption);
			AssertEquals("Notify Party when any accounting transaction failed to get successfully submitted for E-Reporting.", ItemSet.EInvoicingErrorNotificationGroup.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EInvoicingErrorNotificationGroup.Storage);
			AssertEquals(RegistryOptions.IsValueMandatory, ItemSet.EInvoicingErrorNotificationGroup.Options);
			AssertEquals(Guid.Empty, ItemSet.EInvoicingErrorNotificationGroup.DefaultValue);

			var notificationGroup = Guid.NewGuid();
			ItemSet.EInvoicingErrorNotificationGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, notificationGroup);
			AssertEquals(notificationGroup, ItemSet.EInvoicingErrorNotificationGroup.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestEInvoicingCertificateExpiryNotificationGroup()
		{
			TestGenericRegistryItem(ItemSet.EInvoicingCertificateExpiryNotificationGroup,
				"E-ReportingCertificateExpiryNotificationGroup",
				Categories.Accounting_EReportingAndEInvoicingConfigurations,
				"E-Reporting Certificate/Token Expiry Notification Group",
				 @"Notify the party when the E-Invoicing Certificate/Token is approaching the expiry date.
By default, no notification will be sent. If required, you can nominate a group and configure the days to be alerted before the expiry date.
The system will send the notification as per configuration.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);

			CombineAssertions("DefaultValue", () => {
				AssertEquals("AlertDays", 0, ItemSet.EInvoicingCertificateExpiryNotificationGroup.DefaultValue.AlertDays);
				AssertEquals("NotificationGroup", ZGuid.Empty, ItemSet.EInvoicingCertificateExpiryNotificationGroup.DefaultValue.NotificationGroup);
			});
		}

		public void TestEInvoicingCertificateExpiryNotificationGroup_LogReference()
		{
			var testCreator = new TestObjectCreator(Factory);
			AssertNotNull("PreCondition", testCreator.GG1);
			Factory.Save();

			var emptyValue = new EInvoicingCertificateExpiryNotificationGroup()
			{
				NotificationGroup = ZGuid.Empty,
				AlertDays = 0
			};
			var newValue = new EInvoicingCertificateExpiryNotificationGroup()
			{
				NotificationGroup = testCreator.GG1.PK,
				AlertDays = 15
			};
			AssertCase(new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EInvoicingCertificateExpiryNotificationGroup, emptyValue, newValue)
				, expectedLogReference: "Set notification group [GG1] and [15] days.");

			AssertCase(new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EInvoicingCertificateExpiryNotificationGroup, newValue, emptyValue)
				, expectedLogReference: "Set notification group [NULL] and [0] days.");

			void AssertCase(RegistryItemWrapper.BuildLogReferenceArgs args, string expectedLogReference)
			{
				AssertEquals(expectedLogReference, ItemSet.EInvoicingCertificateExpiryNotificationGroup.OnBuildLogReference(args));
			}
		}

		public void TestPostCreditNoteOnApproval()
		{
			AssertEquals("PostCreditNoteOnApproval", ItemSet.PostCreditNoteOnApproval.Name);
			AssertEquals("Accounting/Receivable Defaults/Default Settings/Credit Note And Invoice Reversal Authorization Settings", ItemSet.PostCreditNoteOnApproval.Category);
			AssertEquals("Post Credit Note On Approval", ItemSet.PostCreditNoteOnApproval.Caption);
			AssertEquals(@"When this registry is set to Yes, credit note approval requests with the following posting options will automatically post on final approval:
Post All Charges and Costs.
Post Overseas Agent Charges.
Post All Revenue Charges.
Post Charges for All Group Companies.
Post Charges for Group Companies in My Login Country.

On final approval of the credit note request, approval requests with one of the above options recorded as the Posting Option, will post all revenue charges from the job.
The creator of the credit note approval request will be recorded as the posting user.
If charges are added after the approval has been submitted, automatic posting will be canceled and posting will need to be done from the job. 

Approval requests with the following posting options need to be manually posted from the job:
Post Local Client Charges.
Post Disbursement Charges Only.

When this registry is set to No, on final approval of the credit note request, the user is notified, and the credit note must be posted from the job.

Note: When Enforce Two Credit Note Approvers registry is set to Yes, final approval is second approval.", ItemSet.PostCreditNoteOnApproval.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.PostCreditNoteOnApproval.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.PostCreditNoteOnApproval.Options);
			AssertEquals(false, ItemSet.PostCreditNoteOnApproval.DefaultValue);
		}

		public void TestEnforceZeroBalanceDisbursements()
		{
			AssertEquals("EnforceZeroBalanceDisbursements", ItemSet.EnforceZeroBalanceDisbursements.Name);
			AssertEquals(Categories.Accounting_JobInvoicing, ItemSet.EnforceZeroBalanceDisbursements.Category);
			AssertEquals("Enforce Zero Balance Disbursements", ItemSet.EnforceZeroBalanceDisbursements.Caption);
			AssertMultilineASCIIEquals(@"This registry determines whether a zero balance between cost and sell amounts on disbursements is enforced.
When enabling this registry, staff and group security rights should be checked to ensure only authorized users are granted rights to “Save Non-zero Balance Disbursements”.
The available options are:
DEF – No Validation (this is the registry default).
CUR – Validate OS Currency and Amounts are Equal.
This option enforces both the cost and sell OS amount and currency are equal. It does not take into account the local cost and sell amounts.
LOC – Validate Local Amounts are Equal.
This option enforces the local cost is equal to the local sell less the value of any CFX Journal that applies to the transaction line.
Please note: If you use Currency Exchange Uplift (CFX) and wish to use this option, then Local Cost and Local Sell amount will only balance if the CFX Journal functionality is enabled for this company. Please refer to registry - Job Invoicing > Enable CFX.
EIT – Validate Either OS Currency and Amounts or Local Amounts are Equal (recommended method).
This option will enforce that either local cost and sell (minus CFX Journal amount) are equal or OS currency and amounts are equal.
BTH – Validate Both OS Currency and Amounts and Local Amounts are Equal.
This is the strictest validation and will ensure that both OS amount and currency are equal and Local Amount and currency (minus CFX Journal amount) are equal.", ItemSet.EnforceZeroBalanceDisbursements.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EnforceZeroBalanceDisbursements.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.EnforceZeroBalanceDisbursements.Options);
			AssertEquals(EnforceZeroBalanceDisbursementsOption.Default.Code, ItemSet.EnforceZeroBalanceDisbursements.DefaultValue.EnforceZeroBalanceDisbursementsValidationType);
		}

		public void TestCustomDefaultDepartmentConfiguration()
		{
			AssertEquals("CustomDefaultDepartmentConfiguration", ItemSet.CustomDefaultDepartmentConfiguration.Name);
			AssertEquals(Categories.Accounting_JobInvoicing_DefaultDepartments, ItemSet.CustomDefaultDepartmentConfiguration.Category);
			AssertEquals("Custom Department Defaulting Configuration", ItemSet.CustomDefaultDepartmentConfiguration.Caption);
			AssertMultilineASCIIEquals(@"This registry can no longer be edited as it has been replaced by the new GLOW Production Rules Engine. 
The new engine is accessed by enabling the “Custom Department Defaulting Rules Engine Configuration” registry and visiting the GLOW portal at the link “Set Department Defaulting Rules” below.

If the new engine is not enabled, existing registry configurations will apply.

Please contact support if you still require changes to this registry.

This registry is for developers only.

This registry allows you to configure additional rules for defaulting a department into job header. These custom rules are assessed in conjunction with the other department defaulting configurations in this registry group. For more information about using this feature, please search MyAccount for resources on Custom Department Defaulting Configuration.

To configure custom department defaulting rules, you need to supply a valid expression using IronPython programming language. Please note: Only IT personnel with a strong understanding of programming concepts and data structures should attempt this.

At run-time, your custom script will be appended to this method signature and will be executed in IronPython to determine the appropriate department:
getDefaultDepartment(obj, company, branch, department).

Job parameters:
obj - represents the Generic Freight Wrapper context.
You can access any of the properties on the operational jobs available from the GenericFreightWrapper used for DocBuilder documents. For example, to access Service Level on a shipment job, use obj.BaseShipment.ServiceLevel.RS_Code.

Environmental values:
company - This is the login company of the user.
branch - This is the login branch of the user.
department - This is the login department of the user.
You can access any properties on the login company, branch and department of the user creating the invoicing job. For example, for the country of the login company, use company.AccountingCountry.

Tip: To find the property name for any field on a form, click into that field and press CRTL+SHIFT+R and note down the Binding Member. Once you have the property name, you can find the corresponding macro using the Common Data Source Maps for GenericFreightJob wrapper. To do this, from the relevant form (e.g. a Shipment job), navigate to Documents > Customize > Common Data Source Maps > GenericFreightJob and locate the required Data Field and its Property Information/Macro. For example, Forwarding Shipment’s Destination is expressed as a macro <BaseShipment.Destination.RL_Code>. You can then construct a Python expression as obj.BaseShipment.Destination.RL_Code.

Your code must return a valid department code in order to be accepted into the job header. If the code returns an invalid or inactive department, then the department will be left blank – user will have to manually select the department. If the code returns no (or blank) value, or if the expression is incorrect, then the system will default the value as setup for each operation in other department defaulting registries.

As a guide, your expression should generally contain an “if” and “else”, as well as “return” statements. For example, you may wish to attribute all Air Export freight with Express service level to a specific department, say FEX, otherwise set the department to the user’s login department. Your expression would be:

if obj.BaseShipment.InvoicingSupporter.IsExport and obj.BaseShipment.TransportMode == 'AIR' and obj.BaseShipment.ServiceLevel.RS_Code == 'I2':
    return 'FEX'
else:
    return department.GE_Code

Note: It is important that your expression is indented appropriately with tabs just like any IronPython code block, for the system to be able to interpret it.

Once you have defined your expression, you can evaluate it against any existing billing job. To do this, enter or select a job number and click Evaluate below the expression box.", ItemSet.CustomDefaultDepartmentConfiguration.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CustomDefaultDepartmentConfiguration.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.CustomDefaultDepartmentConfiguration.Options);
			AssertEquals(ZString.Empty, ItemSet.CustomDefaultDepartmentConfiguration.DefaultValue.ConfigAsString);
		}

		public void TestCustomDefaultBranchConfiguration()
		{
			Assert("Precondition", Env.CurrentUser.IsSupportUser);

			AssertEquals("CustomDefaultBranchConfiguration", ItemSet.CustomDefaultBranchConfiguration.Name);
			AssertEquals(Categories.Accounting_JobInvoicing, ItemSet.CustomDefaultBranchConfiguration.Category);
			AssertEquals("Custom Branch Defaulting Configuration", ItemSet.CustomDefaultBranchConfiguration.Caption);
			AssertMultilineASCIIEquals(@"This registry can no longer be edited as it has been replaced by the new GLOW Production Rules Engine. 
The new engine is accessed by enabling the “Custom Branch Defaulting Rules Engine Configuration” registry and visiting the GLOW portal at the link “Set Branch Defaulting Rules” below.

If the new engine is not enabled, existing registry configurations will apply.

Please contact support if you still require changes to this registry.

This registry is for developers only.

This registry allows you to configure additional rules for defaulting a Branch into job header. These custom rules are assessed in conjunction with the other Branch defaulting configurations. For more information about using this feature, please search MyAccount for resources on Custom Branch Defaulting Configuration.

To configure custom Branch defaulting rules, you need to supply a valid expression using IronPython programming language. Please note: Only IT personnel with a strong understanding of programming concepts and data structures should attempt this.

At run-time, your custom script will be appended to this method signature and will be executed in IronPython to determine the appropriate Branch:
getDefaultBranch(obj, company, branch, department).

Job parameters:
obj – represents the Generic Freight Wrapper context.
You can access any of the properties on the operational jobs available from the GenericFreightWrapper used for DocBuilder documents. For example, to access Delivery Agent on a shipment job, use obj.BaseShipment.DeliveryAgent.OH_Code.

Environmental values:
company – This is the login company of the user.
branch – This is the login branch of the user.
department – This is the login department of the user.
You can access any properties on the login company, branch and department of the user creating the invoicing job. For example, for the country of the login company, use company.AccountingCountry.

Tip: To find the property name for any field on a form, click into that field and press CTRL+SHIFT+R and note down the Binding Member. Once you have the property name, you can find the corresponding macro using the Common Data Source Maps for GenericFreightJob expressed as a macro <BaseShipment.DeliveryAgent.OH_Code>. You can then construct a Python expression as obj.BaseShipment.DeliveryAgent.OH_Code.

Your code must return a valid Branch code in order to be accepted into the job header. If the code returns an invalid or inactive Branch, then the Branch will be left blank – user will have to manually select the Branch. If the code returns no (or blank) value, or if the expression is incorrect, then the system will default the value as setup in other Branch defaulting registries.

As a guide, your expression should generally contain an “if” and “else”, as well as “return” statements.

For example, you may wish to attribute all Air Import freight with Delivery Agent 'DELAGTB12' to a specific Branch, say 'B12', otherwise set the branch to the user’s login branch. Your expression would be:
if obj.BaseShipment.InvoicingSupporter.IsImport and obj.BaseShipment.TransportMode == 'AIR' and obj.BaseShipment.DeliveryAgent.OH_Code == 'DELAGTB12':
    return 'B12'
else:
    return branch.GB_Code

Note: It is important that your express is indented appropriately with tabs just like any IronPython code block, for the system to be able to interpret it.

Once you have defined your expression, you can evaluate it against any existing billing job. To do this, enter or select a job number and click Evaluate below the expression box.", ItemSet.CustomDefaultBranchConfiguration.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CustomDefaultBranchConfiguration.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.CustomDefaultBranchConfiguration.Options);
			AssertEquals(ZString.Empty, ItemSet.CustomDefaultBranchConfiguration.DefaultValue.ConfigAsString);
		}

		public void TestAllowSendingEInvoicingBatchWithError()
		{
			AssertEquals("AllowSendingEInvoicingBatchWithError", ItemSet.AllowSendingEInvoicingBatchWithError.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.AllowSendingEInvoicingBatchWithError.Category);
			AssertEquals("Allow sending EInvoicing batch with error (CargoWiseOne Support Only)", ItemSet.AllowSendingEInvoicingBatchWithError.Caption);
			AssertEquals(@"This registry is used to control the behaviour when sending EInvoicing batches.

By default this registry is not enabled, only transactions with 'BCH' status would be send to EHub.
When enabled, transactions with 'BER' status would also be sent.
NOTE: Please consult with the Accounting Product Team before turning on this registry.",
ItemSet.AllowSendingEInvoicingBatchWithError.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.AllowSendingEInvoicingBatchWithError.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.AllowSendingEInvoicingBatchWithError.Options);
			AssertEquals(false, ItemSet.AllowSendingEInvoicingBatchWithError.DefaultValue);
		}

		public void TestVietnamEInvoicingUserName()
		{
			var rgistryItem = ItemSet.VietnamEInvoicingUserName;
			AssertEquals("Name", "VietnamEInvoicingUserName", rgistryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, rgistryItem.Category);
			AssertEquals("Caption", "E-Invoicing Service Partner Connection User Name", rgistryItem.Caption);
			AssertEquals("Hint", "This registry defines the User Name and Password to allow access to the Vietnam e-Invoicing service partner system.\r\nBoth User Name and Password must be specified.", rgistryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, rgistryItem.Storage);
			AssertEquals("Default Value", string.Empty, rgistryItem.DefaultValue);
		}

		public void TestVietNamFPTPassword()
		{
			var rgistryItem = ItemSet.VietnamEInvoicingPassword;
			AssertEquals("Name", "VietnamEInvoicingPassword", rgistryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, rgistryItem.Category);
			AssertEquals("Caption", "E-Invoicing Service Partner Connection Password", rgistryItem.Caption);
			AssertEquals("Hint", "This registry defines the Password to allow access to the Vietnam e-Invoicing service partner system.\r\nBoth User Name and Password must be specified.", rgistryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, rgistryItem.Storage);
			Assert("EditorInfo", rgistryItem.EditorInfo is TextRegistryEditorInfo);
			AssertEquals("EdoitorInfo.EditorType", TextEditorType.Password, ((TextRegistryEditorInfo)rgistryItem.EditorInfo).EditorType);
			AssertEquals("Default Value", string.Empty, rgistryItem.DefaultValue);
		}

		public void TestVietnamEInvoicingFormNumber()
		{
			var registryItem = ItemSet.VietnamEInvoicingFormNumber;
			AssertEquals("Name", "VietnamEInvoicingFormNumber", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, registryItem.Category);
			AssertEquals("Caption", "E-Invoicing Form Number", registryItem.Caption);
			AssertEquals("Hint", "This registry defines the Form Number for Vietnam E-Invoicing.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, registryItem.Storage);
			AssertEquals("Default Value", string.Empty, registryItem.DefaultValue);
			AssertEquals("Max Length", 11, ((StringRegistryDataType)registryItem.DataType).MaxLength);
			AssertExceptionThrown(typeof(RegistryValidationException), "Length must be between 0 and 11.", () => Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "TestFormNumber"));
		}

		public void TestVietnamEInvoicingReceivingFileType()
		{
			var rgistryItem = ItemSet.VietnamEInvoicingReceivingFileType;
			AssertEquals("Name", "VietnamEInvoicingReceivingFileType", rgistryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, rgistryItem.Category);
			AssertEquals("Caption", "E-Invoicing Receiving File Type", rgistryItem.Caption);
			AssertEquals("Hint", @"This registry is relevant to Vietnam Login Company only.
On successfully submission of electronic invoice (status = SUC), a request will be sent to intermediate service provider to request for a copy of the electronic invoice.
The file received will be attached to the respective invoice record's eDoc tab.
By default, the system is configured to receive a copy of the electronic invoice in PDF format.
If required, you can override this registry value and select a different file type from the available option.", rgistryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, rgistryItem.Storage);
			AssertEquals("Default Value", AccountingConstants.VietnamEInvoicingReceivingFileTypeCodes.PDF, rgistryItem.DefaultValue);

			var oldValue = AccountingConstants.VietnamEInvoicingReceivingFileTypeCodes.PDF;
			var newValue = AccountingConstants.VietnamEInvoicingReceivingFileTypeCodes.XML;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(rgistryItem, oldValue, newValue);
			var logReference = rgistryItem.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestVietnamEInvoicingErrorMessageLanguage()
		{
			AssertEquals("Name", "VietnamEInvoicingErrorMessageLanguage", ItemSet.VietnamEInvoicingErrorMessageLanguage.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, ItemSet.VietnamEInvoicingErrorMessageLanguage.Category);
			AssertEquals("Caption", "Language of E-Invoicing Error Message Returned", ItemSet.VietnamEInvoicingErrorMessageLanguage.Caption);
			AssertEquals("Hint", @"This registry defines the language of E-Invoicing error message returned from the service provider.
By default, the language is set to Vietnamese.
If required, user can change the language to English.", ItemSet.VietnamEInvoicingErrorMessageLanguage.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.VietnamEInvoicingErrorMessageLanguage.Storage);

			AssertEquals("Default value", "vi", ItemSet.VietnamEInvoicingErrorMessageLanguage.DefaultValue);
		}

		public void TestUnitMeasurementTextOverride()
		{
			var registry = ItemSet.UnitMeasurementTextOverride;

			AssertEquals("Name", "UnitMeasurementTextOverride", registry.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, registry.Category);
			AssertEquals("Caption", "Unit Measurement Text Override", registry.Caption);
			AssertEquals("Hint", @"This registry is relevant to Vietnam Login Companies where the E-Reporting functionality is enabled only.
By default, the system will export the unit measurement as documented in the Vietnam E-Invoicing mapping guide.
If required, you can configure the list of overrides and the system will use the overrides specified during the E-Invoicing transmission.

Note: Only rate units derived from Freight Consol, Shipment, and Customs Declarations that listed in the mapping guide are supported currently.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, registry.Storage);
			AssertEquals("CountryFilterPK count", 1, registry.CountryFilterPKs.Count());
			AssertEquals("CountryFilterPK is only for Vietnam", Constants.CountryGuids.Vietnam, registry.CountryFilterPKs.First());

			var hasValueCollection = new UnitMeasurementTextOverrideCollection();
			var noValueCollection = new UnitMeasurementTextOverrideCollection();
			var value = new UnitMeasurementTextOverride();
			value.UnitMeasurement = "KG";
			value.TextOverride = "1";
			hasValueCollection.Add(value);

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, noValueCollection, hasValueCollection);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals("Override Added: KG, 1\r\n", logReference);

			var newValueCollection = new UnitMeasurementTextOverrideCollection();
			var newValue = new UnitMeasurementTextOverride();
			newValue.UnitMeasurement = "Package";
			newValue.TextOverride = "2";
			newValueCollection.Add(newValue);
			var newArgs = new RegistryItemWrapper.BuildLogReferenceArgs(registry, hasValueCollection, newValueCollection);
			var newLogReference = registry.OnBuildLogReference(newArgs);
			AssertEquals("Override Deleted: KG, 1\r\nOverride Added: Package, 2\r\n", newLogReference);
		}

		public void TestVietnamEInvoicingPreventTheVietnamElectronicInvoiceFromBeingReversed()
		{
			var registryItem = ItemSet.PreventTheVietnamElectronicInvoiceFromBeingReversed;
			AssertEquals("Name", "PreventTheVietnamElectronicInvoiceFromBeingReversed", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, registryItem.Category);
			AssertEquals("Caption", "Prevent the Vietnam Electronic Invoice from being reversed", registryItem.Caption);
			AssertEquals("Hint", @"This registry defines the effective date for restricting the reversal of Vietnamese e-Invoices.
By default, the effective date is set to June 1, 2025, aligning with Decree 70/2025/ND-CP (“Decree 70”), and the system will block invoice reversals from that date onward.
You may override the default date as needed to suit your specific requirements.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, registryItem.Options);
			AssertEquals("Default Value", new DateTime(2025, 6, 1), registryItem.DefaultValue);
			var oldValue = new DateTime(2025, 6, 1);
			var newValue = new DateTime(2025, 7, 1);
			var expectedLogMessage = $"Registry has been changed from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, oldValue, newValue);
			var logReference = registryItem.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestVietnamEInvoicingAdjustment()
		{
			AssertEquals("Name", "EnableEInvoicingAdjustment", ItemSet.VietnamEInvoicingAdjustment.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, ItemSet.VietnamEInvoicingAdjustment.Category);
			AssertEquals("Caption", "Issue Negative Adjustment via Amend with Credit Note", ItemSet.VietnamEInvoicingAdjustment.Caption);
			AssertEquals("Hint", @"This registry is relevant to Vietnam Login Companies only and controls Negative Adjustment e-Invoicing functionality.
When the registry is enabled, Amendment Credit Notes created via 'Job Billing > AR Invoice > Amend with Credit Note' function can be used to Negatively Adjust Original Invoice amount once compliance number is allocated.", ItemSet.VietnamEInvoicingAdjustment.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.VietnamEInvoicingAdjustment.Storage);

			var registry = ItemSet.VietnamEInvoicingAdjustment;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestVietnamEInvoicingAdjustmentIsVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Default value", RegistryOptions.Default, ItemSet.VietnamEInvoicingAdjustment.Options);
		}

		public void TestVietnamEInvoicingAdjustmentIsNotVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Default value", RegistryOptions.IsHidden, ItemSet.VietnamEInvoicingAdjustment.Options);
		}

		public void TestVietnamExportAmountInWordsBasedOnInvoicedCurrency()
		{
			AssertEquals("Name", "ExportAmountInWordsBasedOnInvoicedCurrency", ItemSet.VietnamExportAmountInWordsBasedOnInvoicedCurrency.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, ItemSet.VietnamExportAmountInWordsBasedOnInvoicedCurrency.Category);
			AssertEquals("Caption", "Export 'Amount in Words' based on Invoiced Currency", ItemSet.VietnamExportAmountInWordsBasedOnInvoicedCurrency.Caption);
			AssertEquals("Hint", @"This registry defines whether the Amount in Words ('word') exported in the E-Invoicing Message is based on Local or Invoiced Currency.

By default, this registry is set to 'No' and the Total Amount Including Tax in Local Currency ('totalv') describes in words will be exported.
If required, you can override this value to 'Yes' and the Total Amount Including Tax in Invoiced Currency ('total') describes in words will be exported.", ItemSet.VietnamExportAmountInWordsBasedOnInvoicedCurrency.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.VietnamExportAmountInWordsBasedOnInvoicedCurrency.Storage);
			AssertEquals("Default", false, ItemSet.VietnamExportAmountInWordsBasedOnInvoicedCurrency.DefaultValue);

			var registry = ItemSet.VietnamExportAmountInWordsBasedOnInvoicedCurrency;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry has been overridden from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestVietnamAlwaysIssueElectronicInvoicesInLocalCurrency()
		{
			AssertEquals("Name", "AlwaysIssueElectronicInvoicesInLocalCurrency", ItemSet.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, ItemSet.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.Category);
			AssertEquals("Caption", "Always Issue Electronic Invoices in Local Currency", ItemSet.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.Caption);
			AssertEquals("Hint", @"This registry is relevant to Vietnam Login Companies only.
By default, this is set to 'No' and the Electronic Invoices will be issued as in Invoiced Currency.
If required, you can override this registry value to 'Yes', and the Electronic Invoices will always be issued in Local Currency.", ItemSet.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.Storage);
			AssertEquals("Default", false, ItemSet.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.DefaultValue);

			var registry = ItemSet.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry has been overridden from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestVietnamIssuePositiveAdjustmentViaAmendWithInvoiceIsNotVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Default value", RegistryOptions.IsHidden, ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Options);
		}

		public void TestVietnamIssuePositiveAdjustmentViaAmendWithInvoiceIsVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Default value", RegistryOptions.Default, ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Options);
		}

		public void TestVietnamIssuePositiveAdjustmentViaAmendWithInvoice()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Name", "VietnamIssuePositiveAdjustmentViaAmendWithInvoice", ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Category);
			AssertEquals("Caption", "Issue Positive Adjustment via Amend with Invoice", ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Caption);
			AssertEquals("Hint", @"This registry is relevant to Vietnam Login Companies only and controls Positive Adjustment e-Invoicing functionality.
When the registry is enabled, Amendment Invoices created via 'Job Billing > AR Invoice > Amend with Invoice' function can be used to Positively Adjust Original Invoice amount once compliance number is allocated.", ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Storage);
			AssertEquals("Default", false, ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.DefaultValue);

			var registry = ItemSet.VietnamIssuePositiveAdjustmentViaAmendWithInvoice;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry has been overridden from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEInvoicingSendType()
		{
			AssertEquals("Name", "EInvoicingSendType", ItemSet.EInvoicingSendType.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam, ItemSet.EInvoicingSendType.Category);
			AssertEquals("Caption", "E-Invoicing Send Type", ItemSet.EInvoicingSendType.Caption);
			AssertEquals("Hint", @"This registry defines whether e-invoices will be sent to Vietnam General Department of Taxation (""GDT"") individually or in batches.

By default, this registry is set to 'No' and e-Invoices will be automatically sent to GDT individually.
When this registry is overridden to 'Yes', you will need to manually batch the e-Invoices on FPT.eInvoice portal and send them in batches to GDT.", ItemSet.EInvoicingSendType.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EInvoicingSendType.Storage);
			AssertEquals("Default", false, ItemSet.EInvoicingSendType.DefaultValue);

			var registry = ItemSet.EInvoicingSendType;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry has been overridden from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestIndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom()
		{
			AssertEquals("Name", "IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom", ItemSet.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_India, ItemSet.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.Category);
			AssertEquals("Caption", "Use Compliance Numbers instead of Transaction Numbers from", ItemSet.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.Caption);
			AssertEquals("Hint", "Set the date from which you want to send Compliance Numbers instead of transaction numbers for generating the E-Invoice JSON file.", ItemSet.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.Storage);
		}

		#region E-Invoicing Registries (Korea specific)

		public void TestKoreaEInvoicingAmendmentStatusCode()
		{
			var koreaEInvoicingAmendmentStatusCode_DefaultValue = new CodeDescriptionPairList();
			koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("01", (NoResString)"Mistakes or correction of entries or the tax rate is incorrectly applied");
			koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("02", "Supply amount change");
			koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("03", "The goods supplied have been returned");
			koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("04", "Disengagement of a contract");
			koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("05", "Post-opening of domestic L/C");
			koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("06", "Double issuance by mistake");

			TestGenericRegistryItem(ItemSet.KoreaEInvoicingAmendmentStatusCode,
				"KoreaEInvoicingAmendmentStatusCode",
				AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
				"Korea e-Invoicing Amendment Status Code(CargoWiseOne Support Only)",
				"This registry is used for Korea e-Invoicing amendment status code.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				koreaEInvoicingAmendmentStatusCode_DefaultValue
			);
		}

		public void TestElectronicInvoiceDocumentFallbackPassword()
		{
			var registryItem = ItemSet.ElectronicInvoiceDocumentFallbackPassword;
			AssertEquals("Name", "ElectronicInvoiceDocumentFallbackPassword", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea, registryItem.Category);
			AssertEquals("Caption", "Electronic Invoice Document Fallback Password", registryItem.Caption);
			AssertEquals("Hint", "This registry defines the fallback password to be used to secured electronic invoice document in the event the invoice recipient's organization's business tax registration number cannot be identified.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals("Default Value", string.Empty, registryItem.DefaultValue);
			AssertEquals("Max Length", 32, ((StringRegistryDataType)registryItem.DataType).MaxLength);
			AssertExceptionThrown(typeof(RegistryValidationException), "Length must be between 0 and 32.", () => registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123456789012345678901234567890123"));
			AssertEquals("CountryFilterPK", CountryFilterPKs.KoreaRepublicOf, registryItem.CountryFilterPKs);
			AssertEquals("EdoitorInfo.EditorType", TextEditorType.Password, ((TextRegistryEditorInfo)registryItem.EditorInfo).EditorType);
		}

		public void TestBackDateInvoiceDateDeadline()
		{
			var registryItem = ItemSet.BackDateInvoiceDateDeadline;
			TestRegistryItem(registryItem,
				"BackDateInvoiceDateDeadline",
				Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
				"Back Date Invoice Date Deadline (CargoWiseOne Support Only)",
				@"Please specify the deadline (number of days from the beginning of the current month) where the system should allow invoice date to be back date to previous month.

For instance, if the value specified is 10, then if today is 10 April, then user will be able to back date the invoice date to 01 March to 31 March. If today is 11 April, then a validation error should be shown when invoice date is back dated to 01 March to 31 March.

If this registry is set to 0 then no validation will be enforced.",
				RegistryStorageFlags.Company,
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
				0, 0, 26);

			AssertEquals("CountryFilterPK", CountryFilterPKs.KoreaRepublicOf, registryItem.CountryFilterPKs);
		}

		public void TestBackDateInvoiceDateDeadlineIsVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.BackDateInvoiceDateDeadline.Options);
		}

		public void TestBackDateInvoiceDateDeadlineIsNotVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.BackDateInvoiceDateDeadline.Options);
		}

		public void TestElectronicInvoiceDataElementsConfiguration()
		{
			AssertEquals("Name", "ElectronicInvoiceDataElementsConfiguration", ItemSet.ElectronicInvoiceDataElementsConfiguration.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea, ItemSet.ElectronicInvoiceDataElementsConfiguration.Category);
			AssertEquals("Caption", "Electronic Invoice Data Elements Configuration", ItemSet.ElectronicInvoiceDataElementsConfiguration.Caption);
			var expectedHint = $@"This registry is only relevant if '{AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Location()}' has been set to 'Yes'.
This registry enables you to configure the contents to be included in specific electronic invoice data elements.";
			AssertEquals("Hint", expectedHint, ItemSet.ElectronicInvoiceDataElementsConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ElectronicInvoiceDataElementsConfiguration.Storage);
			AssertEquals("Options",
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
				ItemSet.ElectronicInvoiceDataElementsConfiguration.Options
			);
			AssertEquals("CountryFilterPK", CountryFilterPKs.KoreaRepublicOf, ItemSet.ElectronicInvoiceDataElementsConfiguration.CountryFilterPKs);

			var expectedDefaultValue = new[]
			{
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, $"[외국인등록번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.ForeignerRegistrationNumber)}>][/][여권번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.PassportNumber)}>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3),

				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1,$"[당초승인번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalNumber)}>][/][당초작성일자: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalDateForCode020304)}>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3)
			};

			var defaultValue = ItemSet.ElectronicInvoiceDataElementsConfiguration.DefaultValue.OfType<KoreaSouthEInvoicingDataElementConfiguration>();

			AssertEquals("The actual and expected collection should have the same amount of elements", expectedDefaultValue.Length, defaultValue.Count());
			AssertEquals("The default value should have no duplicate 'InvoiceType' + 'DataElement' combo", 6, defaultValue.GroupBy(x => new { x.InvoiceType, x.DataElement }).Count());
			foreach (var item in defaultValue)
			{
				AssertEquals("DefaultValue", expectedDefaultValue.Single(x => x.InvoiceType == item.InvoiceType && x.DataElement == item.DataElement).Configuration, item.Configuration);
			}
		}

		public void TestElectronicInvoiceDataElementsConfigurationIsVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.Default, ItemSet.ElectronicInvoiceDataElementsConfiguration.Options);
		}

		public void TestElectronicInvoiceDataElementsConfigurationIsNotVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.ElectronicInvoiceDataElementsConfiguration.Options);
		}

		public void TestElectronicInvoiceDataElementsConfiguration_OnBuildLogReference()
		{
			var oldValue = new KoreaSouthEInvoicingDataElementConfigurationCollection
			{
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, $"<외국인등록번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.ForeignerRegistrationNumber)}> / 여권번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.PassportNumber)}>"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, $"<{nameof(KoreaSouthEInvoicingDataElementProvider.InvoiceHeaderDescription)}>"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3),

				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, $"<{nameof(KoreaSouthEInvoicingDataElementProvider.AmendStatusCodeDescriptionInKorean)}>"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, $"당초승인번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalNumber)}> / 당초작성일자: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalDateForCode020304)}>"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3, $"<{nameof(KoreaSouthEInvoicingDataElementProvider.InvoiceHeaderDescription)}>")
			};

			var newValue = oldValue.Clone(null, null) as KoreaSouthEInvoicingDataElementConfigurationCollection;
			var newValueItems = newValue.Cast<KoreaSouthEInvoicingDataElementConfiguration>();
			newValueItems.Single(x => x.InvoiceType == new ZString(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice) && x.DataElement == new ZString(EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1)).Configuration = "BBB";
			newValueItems.Single(x => x.InvoiceType == new ZString(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment) && x.DataElement == new ZString(EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3)).Configuration = "DDDD";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.OnBuildLogReference(args);
			AssertEquals(@"Original Invoice: Invoice Document Header Description Line 1 set to 'BBB'
Amendment: Invoice Document Header Description Line 3 set to 'DDDD'", logReference.TrimEnd());
		}

		public void TestEnableKoreaSouthEDIInterchangeCreator()
		{
			TestGenericRegistryItem(ItemSet.EnableKoreaSouthEDIInterchangeCreator,
				"EnableKoreaSouthEDIInterchangeCreator",
				AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
				"Enable KoreaSouth EDI Interchange Creator (CargoWiseOne Support Only)",
				@"This registry defines whether the EDI Interchange can be created when running EKR service task.
This is a temporary registry for testing purposes.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true
			);
		}

		public void TestDSBSummaryAppendingRule()
		{
			AssertEquals("Name", "DSBSummaryAppendingRule", ItemSet.DSBSummaryAppendingRule.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea, ItemSet.DSBSummaryAppendingRule.Category);
			AssertEquals("Caption", "Disbursement Fees Summary Appending Rule", ItemSet.DSBSummaryAppendingRule.Caption);
			var expectedHint = @"This registry is only relevant to South Korean companies with the Receivables E-Reporting functionality enabled. 
It defines the rule for appending the disbursement fees ('대납금') summary to the remark ('비고') section of the e-Invoice. 
By default, the disbursement fees summary will not be appended to any tax invoice. 
If necessary, you can define the rule for appending the disbursement fees summary to a tax invoice based on business needs.

Note: 
1. Disbursement fees should be recorded with EXCLUDE tax ID.
2. A value of zero(0) means that the disbursement summary will not be appended to tax invoice(s) created for the said Tax ID. 
3. The system will append the disbursement summary to one of the tax invoice posted within the posting session based on the order specified. E.g. Order 1 will take priority over order 2.";
			AssertEquals("Hint", expectedHint, ItemSet.DSBSummaryAppendingRule.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.DSBSummaryAppendingRule.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("DSBSummaryAppendingRule", TimeSpan.Zero);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.DSBSummaryAppendingRule.Options);

			AssertEquals("CountryFilterPK", CountryFilterPKs.KoreaRepublicOf, ItemSet.DSBSummaryAppendingRule.CountryFilterPKs);

			var defaultValue = ItemSet.DSBSummaryAppendingRule.DefaultValue.OfType<KoreaSouthEInvoicingDSBSummaryAppendingRule>();

			AssertEquals(0, defaultValue.Count());
		}

		#endregion

		public void TestJobRevenueJournalGLAccountDefaultingRules()
		{
			AssertEquals("DefaultValue", AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code, ItemSet.JobRevenueJournalGLAccountDefaultingRules.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing, ItemSet.JobRevenueJournalGLAccountDefaultingRules.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.JobRevenueJournalGLAccountDefaultingRules.Storage);
			AssertEquals("Caption", "Job Revenue Journal GL Account Defaulting Rules", ItemSet.JobRevenueJournalGLAccountDefaultingRules.Caption);
			AssertEquals("Hint", @"By default, this registry value will be set to 'REV' and the system will use Charge Code's Revenue GL Account.

To use the Charge Code's Cost GL Account, set to 'CST' option.
To use the Charge Code's Cost GL Account for Debit entry and Revenue GL Account for Credit entry, set to 'BTH' option.", ItemSet.JobRevenueJournalGLAccountDefaultingRules.Hint);

			ItemSet.JobRevenueJournalGLAccountDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code);
			AssertEquals("System level Value", AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code, ItemSet.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Company level Value", AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code, ItemSet.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			ItemSet.JobRevenueJournalGLAccountDefaultingRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code);
			AssertEquals("System level Value", AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code, ItemSet.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Company level Value", AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code, ItemSet.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			//event log
			var registry = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules;
			var oldValue = AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code;
			var newValue = AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestJobDeactivationConfiguration()
		{
			var item = ItemSet.JobDeactivationConfiguration;
			AssertEquals("DefaultValue", JobDeactivationConfigurations.Default.Code, item.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing, item.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Caption", "Job Deactivation Configuration", item.Caption);
			AssertEquals("Hint", @"By default, this registry value will be set to ""DEF - No accounting transaction has been posted"" and  billing job can only be deactivated if no accounting transaction has been posted.

If this registry value is overridden and set to ""NPL - No active WIPs and ACRs"", billing jobs with reversed WIPs and ACRs can be deactivated.", item.Hint);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JobDeactivationConfigurations.NoActiveWIPACR.Code);
			AssertEquals("System level Value", JobDeactivationConfigurations.NoActiveWIPACR.Code, item.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Company level Value", JobDeactivationConfigurations.NoActiveWIPACR.Code, item.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			item.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, JobDeactivationConfigurations.Default.Code);
			AssertEquals("System level Value", JobDeactivationConfigurations.NoActiveWIPACR.Code, item.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Company level Value", JobDeactivationConfigurations.Default.Code, item.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var registry = AccountingConfigurationRegistry.Instance.JobDeactivationConfiguration;
			var oldValue = JobDeactivationConfigurations.Default.Code;
			var newValue = JobDeactivationConfigurations.NoActiveWIPACR.Code;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.JobDeactivationConfiguration.OnBuildLogReference(args);

			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestPayableEnforceUniqueTransactionNumber()
		{
			AssertEquals("DefaultValue", false, ItemSet.PayableEnforceUniqueTransactionNumber.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.PayableEnforceUniqueTransactionNumber.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.PayableEnforceUniqueTransactionNumber.Storage);
			AssertEquals("Caption", "Enforce Unique Transaction Number", ItemSet.PayableEnforceUniqueTransactionNumber.Caption);
			AssertEquals("Hint", @"By default, the system will allow you to post AP Invoice, AP Credit Note and AP Adjustment Note with the same Transaction Number.
For example:
	AP INV 00001001
	AP CRD 00001001
	AP ADJ 00001001

If required, this registry can be used to enforce unique transaction number across different transaction type.
For example:
	AP INV 00001001
	AP CRD 00001002
	AP ADJ 00001003

This is useful if you need to align the validation rule in CargoWiseOne and your external ERP system.", ItemSet.PayableEnforceUniqueTransactionNumber.Hint);

			ItemSet.PayableEnforceUniqueTransactionNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("System level Value", true, ItemSet.PayableEnforceUniqueTransactionNumber.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Company level Value", true, ItemSet.PayableEnforceUniqueTransactionNumber.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var registry = Instance.PayableEnforceUniqueTransactionNumber;
			var expectedLogMessage = $"Registry value changed from [False] to [True].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, false, true);
			var logReference = Instance.PayableEnforceUniqueTransactionNumber.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestPeriodicBillingChargePostingPerformanceImprovementConfiguration()
		{
			AssertEquals("DefaultValue", 500, ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing, ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.Options);
			AssertEquals("Caption", "Periodic billing charge posting performance improvement configuration", ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.Caption);
			AssertEquals("Hint", @"This registry is used to configure the minimum number of charges which will trigger the special performance improvement functionality of periodic billing charge posting.

By default this registry is set to 500. When the total number of charges is greater than 500 the performance improvement will be applied.", ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.Hint);

			ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 600);
			AssertEquals("Value", 600, ItemSet.PeriodicBillingChargePostingPerformanceImprovementConfiguration.Value);
		}

		public void TestPayableOrderApprovalRestriction()
		{
			var codeDescriptionCollection = ItemSet.PayableOrderApprovalRestriction.DefaultValue;
			AssertRegistryValues(codeDescriptionCollection, false);

			codeDescriptionCollection
				.ToList<CodeDescriptionBool>()
				.ForEach(x => x.Bool = ((x.Code == PayableOrderRestrictionList.Codes.SupplierIsTempOrg)));   // Boolean condition is evaluated at runtime
			ItemSet.PayableOrderApprovalRestriction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, codeDescriptionCollection);

			AssertRegistryValues(ItemSet.PayableOrderApprovalRestriction.Value, true);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_PurchaseOrders, ItemSet.PayableOrderApprovalRestriction.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.PayableOrderApprovalRestriction.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.PayableOrderApprovalRestriction.Options);
			AssertEquals("Caption", "Enable Order Approval Restrictions", ItemSet.PayableOrderApprovalRestriction.Caption);

			var expectedHint = @"This registry controls when a Purchase Order can be approved.

Creator(PO initiator) Approval restriction: When this restriction is enabled, a Purchase Order may not be approved by the same user who initiated (created) it. When this restriction is not enabled, the initiator may approve their own Purchase Order as long as it is within their level of authority.

Blank Suppliers restriction: When this restriction is enabled, the Purchase Order may not be approved if the Supplier is blank.

Overridden Supplier Details restriction: When this restriction is enabled, the Purchase Order may not be approved if the Supplier details are manually overridden. A valid Supplier Organization must be selected.

Temporary Organizations restriction: When this restriction is enabled, the Purchase Order may not be approved if it is associated with a Supplier Organization flagged as 'Temporary Account'.";
			AssertEquals("Hint", expectedHint, ItemSet.PayableOrderApprovalRestriction.Hint);
		}

		void AssertRegistryValues(CodeDescriptionBoolCollection codeDescriptionCollection, bool value)
		{
			codeDescriptionCollection
				.ToList<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (x.Code == PayableOrderRestrictionList.Codes.SupplierIsTempOrg)
					{
						AssertEquals("Value for PO restriction: " + x.Code, value, x.Bool);
					}
					else
					{
						AssertEquals("Value for PO restriction: " + x.Code, !value, x.Bool);
					}
				});
		}

		public void TestIndividualPayableOrderRegistry()
		{
			var codeDescriptionList = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value;
			foreach (CodeDescriptionBool item in codeDescriptionList)
			{
				item.Bool = !item.Bool;
				SaveRegistryAndAssertValues(codeDescriptionList);

				item.Bool = !item.Bool;
				SaveRegistryAndAssertValues(codeDescriptionList);
			}
		}

		void SaveRegistryAndAssertValues(CodeDescriptionBoolCollection codeDescriptionList)
		{
			AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionList);
			var codeDescriptionList2 = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value;
			foreach (CodeDescriptionBool item in codeDescriptionList)
			{
				AssertEquals(codeDescriptionList.GetBoolFromCode(item.Code), codeDescriptionList2.GetBoolFromCode(item.Code));
			}
		}

		public void TestPaymentReceiptTypeReferenceNumberRegistryDefaults()
		{
			AssertEquals("Default Payment / Receipt Reference Number", ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting, ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.Storage);
			AssertEquals(@"The Registry setting allows you to configure a default reference number for all receipt and payment types. You can nominate the reference number for all new transactions in the below grid.

These values will default into the Reference Number field when creating a new transaction.
As the reference number for ‘CHQ’ type will always be unique, the system will not support a default reference for this receipt and payment type.

Note: Where the same transactions type can be used in Receipts and Payment the same reference number value will be used.", ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.Hint);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, true))
			{
				var systemLevelDefaultValue = ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("Default List Count", 19, systemLevelDefaultValue.Count);

				var companyLevelDefaultValue = ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals("Default List Count", 20, companyLevelDefaultValue.Count);
			}

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, false))
			{
				var systemLevelDefaultValue = ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("Default List Count", 19, systemLevelDefaultValue.Count);

				var companyLevelDefaultValue = ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals("Default List Count", 19, companyLevelDefaultValue.Count);

				var list = ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
				AssertEquals("Default List Count", 19, list.Count);
				var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
				AssertEquals(ReceiptTypes.CreditCard, element.Type);
				AssertEquals(ZString.Empty, element.ReferenceNumber);

				element.ReferenceNumber = "Test1";
				ItemSet.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
				element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
				AssertEquals(ReceiptTypes.CreditCard, element.Type);
				AssertEquals("Test1", element.ReferenceNumber);
			}
		}

		public void TestDefaultPaymentType()
		{
			AssertEquals("DefaultValue", ReceiptTypes.Cheque, ItemSet.DefaultPaymentType.DefaultValue);
			ItemSet.DefaultPaymentType.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			AssertEquals("Value", ReceiptTypes.Cash, ItemSet.DefaultPaymentType.Value);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, true))
			{
				((IRegistryItemInternals)ItemSet.DefaultPaymentType).GetRegistryItemPK(Guid.Empty, Guid.Empty, Guid.Empty);
				var lookupListForSystemLevel = ((CodePairRegistryDataType)ItemSet.DefaultPaymentType.DataType).LookUpList;
				AssertEquals("Default List Count", 18, lookupListForSystemLevel.Count);

				((IRegistryItemInternals)ItemSet.DefaultPaymentType).GetRegistryItemPK(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var lookupListForCompanyLevel = ((CodePairRegistryDataType)ItemSet.DefaultPaymentType.DataType).LookUpList;
				AssertEquals("Default List Count", 19, lookupListForCompanyLevel.Count);
			}

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, false))
			{
				((IRegistryItemInternals)ItemSet.DefaultPaymentType).GetRegistryItemPK(Guid.Empty, Guid.Empty, Guid.Empty);
				var lookupListForSystemLevel = ((CodePairRegistryDataType)ItemSet.DefaultPaymentType.DataType).LookUpList;
				AssertEquals("Default List Count", 18, lookupListForSystemLevel.Count);

				((IRegistryItemInternals)ItemSet.DefaultPaymentType).GetRegistryItemPK(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var lookupListForCompanyLevel = ((CodePairRegistryDataType)ItemSet.DefaultPaymentType.DataType).LookUpList;
				AssertEquals("Default List Count", 18, lookupListForCompanyLevel.Count);
			}
		}

		public void TestDefaultReceiptType()
		{
			AssertEquals("DefaultValue", ReceiptTypes.Cheque, ItemSet.DefaultReceiptType.DefaultValue);
			ItemSet.DefaultReceiptType.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			AssertEquals("Value", ReceiptTypes.Cash, ItemSet.DefaultReceiptType.Value);
		}

		public void TestDefaultCashBookReceiptType()
		{
			AssertEquals("DefaultValue", ReceiptTypes.Cheque, ItemSet.DefaultCashBookReceiptType.DefaultValue);
			ItemSet.DefaultCashBookReceiptType.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			AssertEquals("Value", ReceiptTypes.Cash, ItemSet.DefaultCashBookReceiptType.Value);
		}

		public void TestDefaultCashBookPaymentType()
		{
			AssertEquals("DefaultValue", ReceiptTypes.Cheque, ItemSet.DefaultCashBookPaymentType.DefaultValue);
			ItemSet.DefaultCashBookPaymentType.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			AssertEquals("Value", ReceiptTypes.Cash, ItemSet.DefaultCashBookPaymentType.Value);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, true))
			{
				var lookupList = ((CodePairRegistryDataType)ItemSet.DefaultCashBookPaymentType.DataType).LookUpList;
				AssertEquals("Default List Count", 18, lookupList.Count);
			}

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, false))
			{
				var lookupList = ((CodePairRegistryDataType)ItemSet.DefaultCashBookPaymentType.DataType).LookUpList;
				AssertEquals("Default List Count", 18, lookupList.Count);
			}
		}

		public void TestPayableAllowUserToStoreARDoc()
		{
			TestRegistryItem(ItemSet.PayableAllowUserToStoreARDoc,
				"PayableAllowUserToStoreARDoc",
				Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Categories.Accounting_PayableDefaults_DefaultSettings,
				"Store AR Invoice Documents when Importing Intercompany AP Invoices",
				"When this flag is set to 'Yes', AR Invoice documents will be stored against the Intercompany AP Invoices' eDocs tab when importing.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber()
		{
			AssertEquals("Caption", "Import Sister Company Invoice Compliance Number As Invoice Number", ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.Caption);
			AssertEquals("Category", Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.Category);
			AssertEquals("Storage Flag", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.Storage);
			AssertEquals("Hint", "When this registry is set to 'Yes', importing sister company invoice with compliance number will set receiving company 'Invoice Number’ as the sending company ‘Compliance Number’ and 'Supplier Cost Reference' as the sending company ‘Invoice Number’.", ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.Hint);
			AssertEquals("Default Value", true, ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.DefaultValue);

			ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("GetValue method does not look at system level overriden value", true, ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("Should return false as value is overriden to 'No' at system level", false, ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should return false as value is overriden to 'No' for current company", false, ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("Should return false as value is overriden to 'No' for current company", false, ItemSet.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestComplianceInvoiceBookAllocaltionFailureNotificationGroup()
		{
			TestRegistryItem(ItemSet.ComplianceInvoiceBookAllocaltionFailureNotificationGroup,
				"ComplianceInvoiceBookAllocaltionFailureNotificationGroup",
				"Accounting/Email Notification",
				"Compliance Invoice Book Allocation Failure Notification Group",
				"Email Notifications advising when a Compliance Invoice Book needs to be configured because an unsuccessful attempt was made to assign a Compliance Government Invoice number. This notification will identify when a Compliance Invoice Book a particular Sub Type and Branch needs to be added.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestEnableMiscInvoiceInPeriodicInvoice()
		{
			TestGenericRegistryItem(ItemSet.EnableMiscInvoiceInPeriodicInvoice,
				"ReceivableEnableMiscInvoiceInPeriodicInvoice",
				AccountingConfigurationRegistry.Categories.Accounting_ReceivableDefaults_DefaultSettings,
				"Enable Miscellaneous Invoices in Periodic Invoicing System (CargoWiseOne Support Only)",
				"Enable Miscellaneous Invoices in Periodic Invoicing System",
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestSecurityRightsForTransactionAllocateAndPostTrigger()
		{
			Guid testStaff = Guid.NewGuid();
			ItemSet.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testStaff);
			AssertEquals("SecurityRightsForTransactionAllocateAndPostTrigger", testStaff, ItemSet.SecurityRightsForTransactionAllocateAndPostTrigger.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.SecurityRightsForTransactionAllocateAndPostTrigger.Storage);
		}

		public void TestIsMTDProductionMode()
		{
			Assert("Pre-condition: Should be Test system", !Env.Instance.IsProductionSystem);

			TestGenericRegistryItem(ItemSet.IsMTDProductionMode,
				"IsMTDProductionMode",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"Is MTD in Production Mode (CargoWiseOne Support Only)",
				@"This registry determines if application is accessing MTD APIs in HMRC's production environment. When this registry is set to No, the application will access MTD APIs in HMRC's sandbox environment.
By default, this registry is configured to access HMRC's production environment if the current installation is using a production licence, and sandbox environment otherwise.
You can override this behaviour by overriding the registry and setting the value explicitly. If you override this registry, make sure to confirm the MTD Client ID and MTD Client secret registry items are configured to use the correct credentials.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestIsMTDProductionModeDefaultValueInProductionSystem()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			Assert("Should be Production system now", Env.Instance.IsProductionSystem);

			TestGenericRegistryItem(ItemSet.IsMTDProductionMode,
				"IsMTDProductionMode",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"Is MTD in Production Mode (CargoWiseOne Support Only)",
				@"This registry determines if application is accessing MTD APIs in HMRC's production environment. When this registry is set to No, the application will access MTD APIs in HMRC's sandbox environment.
By default, this registry is configured to access HMRC's production environment if the current installation is using a production licence, and sandbox environment otherwise.
You can override this behaviour by overriding the registry and setting the value explicitly. If you override this registry, make sure to confirm the MTD Client ID and MTD Client secret registry items are configured to use the correct credentials.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestMTDClientID()
		{
			Assert("Pre-condition: Not in Production", !ItemSet.IsMTDProductionMode.Value);

			TestGenericRegistryItem(ItemSet.MTDClientID,
				"MTDClientID",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Client ID (CargoWiseOne Support Only)",
				@"HMRC uses client ID to identify CW1 during each API call.
By default, this registry is configured to use the Production Client ID when the ""Is MTD in Production Mode"" Registry is 'Yes'. Else, its configured to use the Sandbox Client ID.
If you have overriden the ""Is MTD in Production Mode"", ensure that the correct Client ID is being used.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"dnTztKp2F1nNvEhHuR8mxyiWOYMa");
		}

		public void TestMTDClientID_Production()
		{
			ItemSet.IsMTDProductionMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestGenericRegistryItem(ItemSet.MTDClientID,
				"MTDClientID",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Client ID (CargoWiseOne Support Only)",
				@"HMRC uses client ID to identify CW1 during each API call.
By default, this registry is configured to use the Production Client ID when the ""Is MTD in Production Mode"" Registry is 'Yes'. Else, its configured to use the Sandbox Client ID.
If you have overriden the ""Is MTD in Production Mode"", ensure that the correct Client ID is being used.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"CGSgE2X6EyCrgfVHHFi5dUOVxcEa");
		}

		public void TestMTDClientSecret()
		{
			Assert("Pre-condition: Not in Production", !ItemSet.IsMTDProductionMode.Value);

			TestGenericRegistryItem(ItemSet.MTDClientSecret,
				"MTDClientSecret",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Client Secret (CargoWiseOne Support Only)",
				@"HMRC uses Client Secret to identify CW1 during each API call.
By default, this registry is configured to use the Production Client Secret when the ""Is MTD in Production Mode"" Registry is 'Yes'. Else, its configured to use the Sandbox Client Secret.
If you have overriden the ""Is MTD in Production Mode"", ensure that the correct Client Secret is being used.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"b5d69a47-ce66-45cf-b410-97a84248ebdd");

			var editorInfo = ItemSet.MTDClientSecret.EditorInfo;
			Assert("EditorInfo", editorInfo is TextRegistryEditorInfo);
			AssertEquals("EdoitorInfo.EditorType", TextEditorType.Password, ((TextRegistryEditorInfo)editorInfo).EditorType);
		}

		public void TestMTDClientSecretWithTestRegistryItem()
		{
			Assert("Pre-condition: Not in Production", !ItemSet.IsMTDProductionMode.Value);

			TestRegistryItem(
				ItemSet.MTDClientSecret,
				"MTDClientSecret",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Client Secret (CargoWiseOne Support Only)",
				@"HMRC uses Client Secret to identify CW1 during each API call.
By default, this registry is configured to use the Production Client Secret when the ""Is MTD in Production Mode"" Registry is 'Yes'. Else, its configured to use the Sandbox Client Secret.
If you have overriden the ""Is MTD in Production Mode"", ensure that the correct Client Secret is being used.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.Password,
				"b5d69a47-ce66-45cf-b410-97a84248ebdd");
		}

		public void TestMTDClientSecret_Production()
		{
			ItemSet.IsMTDProductionMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestGenericRegistryItem(ItemSet.MTDClientSecret,
				"MTDClientSecret",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Client Secret (CargoWiseOne Support Only)",
				@"HMRC uses Client Secret to identify CW1 during each API call.
By default, this registry is configured to use the Production Client Secret when the ""Is MTD in Production Mode"" Registry is 'Yes'. Else, its configured to use the Sandbox Client Secret.
If you have overriden the ""Is MTD in Production Mode"", ensure that the correct Client Secret is being used.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"e867b76e-c2e1-4879-84ad-4cb8c060c7b1");
		}

		public void TestMTDProductionWebServiceUrl()
		{
			TestGenericRegistryItem(ItemSet.MTDProductionWebServiceUrl,
				"MTDProductionWebServiceUrl",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Production Web Service Url (CargoWiseOne Support Only)",
				"Kindly enter the Url to access UK HMRC's MTD for VAT API endpoints in production mode",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"https://api.service.hmrc.gov.uk");
		}

		public void TestMTDTestWebServiceUrl()
		{
			TestGenericRegistryItem(ItemSet.MTDTestWebServiceUrl,
				"MTDTestWebServiceUrl",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Test Web Service Url (CargoWiseOne Support Only)",
				"Kindly enter the Url to access UK HMRC's MTD for VAT API endpoints in test mode",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"https://test-api.service.hmrc.gov.uk");
		}

		public void TestMTDScenarioToSimulateForObligationRequest()
		{
			TestGenericRegistryItem(ItemSet.MTDScenarioToSimulateForObligationRequest,
				"MTDScenarioToSimulateForObligationRequest",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"MTD Scenario to simulate for Obligation request(CargoWiseOne Support Only)",
				"Kindly enter the key of the scenario that you want to simulate when the 'Obligations' request is sent to UK HMRC's MTD for VAT API in test mode. List of valid keys is available here: https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0#_retrieve-vat-obligations_get_accordion",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				string.Empty);
		}

		public void TestPeriodReopenNotifyGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.PeriodReopenNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("PeriodReopenNotifyGroup", testGroup, ItemSet.PeriodReopenNotifyGroup.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGetARInvoiceMenuItemName_Overridden_OldStyle()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			ItemSet.ARInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuItem.PK.ToGuid());
			AssertEquals("When overridden, should always return the configured value, regardless of DocumentRegistry settings", menuItem.SU_MenuName, ItemSet.GetARInvoiceMenuItemName(Factory));
		}

		public void TestGetARInvoiceMenuItemName_Overridden_DocBuilder()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			ItemSet.ARInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuItem.PK.ToGuid());
			AssertEquals("When overridden, should always return the configured value, regardless of DocumentRegistry settings", menuItem.SU_MenuName, ItemSet.GetARInvoiceMenuItemName(Factory));
		}

		public void TestGetARLocalInvoiceMenuItemName()
		{
			AssertEquals("Default Value", "Class A Invoice Preprinted", ItemSet.GetARLocalInvoiceMenuItemName(Factory));
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.NotEqual, "Class A Invoice Preprinted"));
			ItemSet.ARLocalInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuItem.PK.ToGuid());
			AssertEquals("When overridden, should always return the configured value", menuItem.SU_MenuName, ItemSet.GetARLocalInvoiceMenuItemName(Factory));
		}

		public void TestCreditNoteApprovalsNotifyGroup()
		{
			AssertEquals("CreditNoteApprovalsNotifyGroup", ItemSet.CreditNoteApprovalsNotifyGroup.Name);
			AssertEquals(Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting, ItemSet.CreditNoteApprovalsNotifyGroup.Category);
			AssertEquals("Credit Note Approvals Notify Group", ItemSet.CreditNoteApprovalsNotifyGroup.Caption);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.CreditNoteApprovalsNotifyGroup.Storage);

			var expectedHint = @"This registry allows you to nominate a user group who will receive notifications about outstanding credit note approval requests.

By default, a daily email will summarize the credit note approval requests that are still in REQ - Requested status. To change the frequency of the email, please navigate to Maintain > System > Service Tasks and locate the following task: 
UCN - Credit Note Approvals Notification Email. You can then edit the Recurrence Pattern for the scheduled email.

Credit Note Approval requests can be approved or rejected in the Credit Note Approval module.";
			AssertEquals("Hint", expectedHint, ItemSet.CreditNoteApprovalsNotifyGroup.Hint);

			Guid testGroup = Guid.NewGuid();
			ItemSet.CreditNoteApprovalsNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("CreditNoteApprovalsNotifyGroup", testGroup, ItemSet.CreditNoteApprovalsNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestIsARLocalInvoiceMenuOverrideAllowed()
		{
			TestRegistryItem(ItemSet.IsARLocalInvoiceMenuOverrideAllowed, "IsARLocalInvoiceMenuOverrideAllowed", "Accounting/Receivable Defaults/Form Configurations/Local Invoice", "Allow AR Local Invoice Menu Override (CargoWiseOne Support Only)", "Setting this registry to 'Yes' will allow users to specify their own menu name to use when printing local invoices through CargoWiseOne", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestARLocalInvoiceMenuItem()
		{
			StmMenuItem item = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.ClassAInvoiceName));

			TestRegistryItem(ItemSet.ARLocalInvoiceMenuItem, "ARLocalInvoiceMenuItem", "Accounting/Receivable Defaults/Form Configurations/Local Invoice", "AR Local Invoice Menu Item Name", $"This registry item allows you to nominate a menu item that should be used when previewing and delivering AR Local Invoices throughout {BrandingFactory.Instance.ProductName}", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsValueMandatory, RegistryFindBoxCollection.ARInvoiceMenuItem, item.PK.ToGuid());
		}

		public void TestShowARCreditNoteAmountsWithOppositeSign()
		{
			TestRegistryItem(ItemSet.ShowARCreditNoteAmountsWithOppositeSign, "ShowARCreditNoteAmountsWithOppositeSign", "Accounting/Receivable Defaults/Form Configurations/Credit Note", "Show AR Credit Note Amounts With Opposite Sign", "By default , when printing an AR Credit Note, all values are printed as a “positive” figure.  The document type, it’s title and content all clearly identify the transaction as a “Credit”.  When overridden and set to “Yes” the AR Credit Note will print with amounts negated.  Note:  This registry only applies to the way amounts are printed in the AR Credit Note DocBuilder document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestDisplayTaxRateInAllLinesOfTaxSummary()
		{
			TestRegistryItem(ItemSet.DisplayTaxRateInAllLinesOfTaxSummary, "DisplayTaxRateInAllLinesOfTaxSummary", "Accounting/Receivable Defaults/Form Configurations/Invoice", "Display Tax Rate in All Lines of Tax Summary", @"This registry is relevant to Companies that use the “Tax Total Summary by Rate and Tax Message” doc strip.

By default this registry is not enabled.
This means that the “Tax Total Summary by Rate and Tax Message” doc strip prints a tax rate only on lines that contain a Tax Amount.

When set to Yes, the tax rate relevant on the tax date of the charge line will print on every line of the “Tax Total Summary by Rate and Tax” doc strip.

Charges with no tax amount will print the value “0%” in the rate column. ", RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestConsolCostDefaultChildShipmentsApportionment()
		{
			AssertEquals("DefaultValue", false, ItemSet.ConsolCostDefaultRelatedShipmentsApportionment.DefaultValue);

			ItemSet.ConsolCostDefaultRelatedShipmentsApportionment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ConsolCostDefaultRelatedShipmentsApportionment.Value);
		}

		public void TestJobRevenueJournalPrintPrompting()
		{
			AssertEquals("DefaultValue", false, ItemSet.JobRevenueJournalPrintPrompting.DefaultValue);

			ItemSet.JobRevenueJournalPrintPrompting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.JobRevenueJournalPrintPrompting.Value);
		}

		public void TestPopupARInvoiceDescriptionOverrideOnPosting()
		{
			AssertEquals("DefaultValue", false, ItemSet.PopupARInvoiceDescriptionOverrideOnPosting.DefaultValue);

			ItemSet.PopupARInvoiceDescriptionOverrideOnPosting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.PopupARInvoiceDescriptionOverrideOnPosting.Value);
		}

		public void TestEnableDeferredRevenueRecognition()
		{
			AssertEquals("DefaultValue", false, ItemSet.EnableDeferredRevenueRecognition.DefaultValue);

			ItemSet.EnableDeferredRevenueRecognition.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.EnableDeferredRevenueRecognition.Value);
		}

		public void TestAutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile()
		{
			AssertEquals("DefaultValue", false, ItemSet.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.DefaultValue);

			ItemSet.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.Value);
		}

		public void TestUseInvoiceExchangeRateWhenChangingReceiptAmount()
		{
			AssertEquals("DefaultValue", false, ItemSet.UseInvoiceExchangeRateWhenChangingReceiptAmount.DefaultValue);

			ItemSet.UseInvoiceExchangeRateWhenChangingReceiptAmount.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.UseInvoiceExchangeRateWhenChangingReceiptAmount.Value);
		}

		public void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked()
		{
			AssertEquals("DefaultValue", false, ItemSet.DataImportShouldSaveOnlyWhenThereAreNoErrors.DefaultValue);

			ItemSet.DataImportShouldSaveOnlyWhenThereAreNoErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DataImportShouldSaveOnlyWhenThereAreNoErrors.Value);
		}

		public void TestCombineInvoiceLinesByChargeCode()
		{
			AssertEquals("DefaultValue", false, ItemSet.CombineInvoiceLinesByChargeCode.DefaultValue);

			ItemSet.CombineInvoiceLinesByChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.CombineInvoiceLinesByChargeCode.Value);
		}

		public void TestGetARInvoiceMenuItemName_NotOverridden_OldStyle()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("When not overridden, should default to whatever is configured in Documents Registry", "Invoice", ItemSet.GetARInvoiceMenuItemName(Factory));
		}

		public void TestGetARInvoiceMenuItemName_NotOverridden_DocBuilder()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("When not overridden, should default to whatever is configured in Documents Registry", "DocBuilder Invoice", ItemSet.GetARInvoiceMenuItemName(Factory));
		}

		public void TestARInvoiceNumberLengthConfiguration()
		{
			AssertEquals("Default value", 8, ItemSet.ARInvoiceNumberLengthConfiguration.DefaultValue);
			AssertEquals("Value must be greater than or equal to the minimum (5)", ItemSet.ARInvoiceNumberLengthConfiguration.GetValidationErrorMessage(4, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("Value must be less than or equal to the maximum (8)", ItemSet.ARInvoiceNumberLengthConfiguration.GetValidationErrorMessage(9, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(string.Empty, ItemSet.ARInvoiceNumberLengthConfiguration.GetValidationErrorMessage(5, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(string.Empty, ItemSet.ARInvoiceNumberLengthConfiguration.GetValidationErrorMessage(8, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(string.Empty, ItemSet.ARInvoiceNumberLengthConfiguration.GetValidationErrorMessage(6, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestAllowBackPostingSubLedgerTransaction()
		{
			AssertEquals("DefaultValue", false, ItemSet.AllowBackPostingSubLedgerTransaction.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.AllowBackPostingSubLedgerTransaction.CountryFilterPKs);
			ItemSet.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AllowBackPostingSubLedgerTransaction.Value);
		}

		public void TestEnableAutomaticChargeCodeMappingForUnallocatedInvoices()
		{
			var expectedHint = @"This registry is used when importing AP Invoices using the Universal Transaction schema. 
These invoices are imported as Transactions Pending Allocation. By default, the system will automatically map the ‘Charge Code’ value in the XML to the charge code an operator selects. 
This mapping will be updated every time an invoice is posted. 
Set this registry to ‘No’ if you want to disable this automatic mapping.";

			TestGenericRegistryItem(ItemSet.EnableAutomaticChargeCodeMappingForUnallocatedInvoices,
				"EnableAutomaticChargeCodeMappingForUnallocatedInvoices",
				"Accounting/Payable Defaults/Default Settings",
				"Enable Automatic Charge Code Mapping For Unallocated Invoices",
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestEnableAutomaticOrganizationCodeMappingForUnallocatedInvoices()
		{
			var expectedHint = @"This registry is used when importing AP invoices using the Universal Transaction schema (XUT).
These invoices are imported as Transactions Pending Allocation (TPA). By default, the system will automatically map the 'Organization Code' value in the XML to the corresponding Organization Proxy found in Details > Config > EDI Code Mapping.
This mapping will be updated every time a change is made to the organization code on an invoice that is posted.";

			TestGenericRegistryItem(ItemSet.EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices,
				"EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices",
				"Accounting/Payable Defaults/Default Settings",
				"Enable Automatic Organization Code Mapping For Unallocated Invoices",
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestAllowZeroValueARInvoices()
		{
			TestGenericRegistryItem(ItemSet.AllowZeroValueARInvoices,
				"AllowZeroValueARInvoices",
				"Accounting/Receivable Defaults/Default Settings",
				"Allow Posting of Zero Value AR Invoices",
				"Set this registry to 'No' to prevent users from posting AR Invoices, Credit Notes and Adjustment Notes that have a total value of zero.",
				RegistryStorageFlags.Company,
				true);
		}

		public void TestEditPaymentAddress()
		{
			TestGenericRegistryItem(ItemSet.EditPaymentAddress,
				"EditPaymentAddress",
				"Accounting",
				"Edit AR/AP Payment Address",
				"Setting this registry to 'Yes' will allow users to edit the address and contact on AR/AP Payments.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestDisplayARStatementAgeingFields()
		{
			AssertEquals("DefaultValue", false, ItemSet.DisplayARStatementAgeingFields.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.DisplayARStatementAgeingFields.CountryFilterPKs);
			ItemSet.DisplayARStatementAgeingFields.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DisplayARStatementAgeingFields.Value);
		}

		public void TestExporterExemptionCellingLimitThreshold()
		{
			AssertEquals("DefaultValue", 0M, ItemSet.ExporterExemptionCellingLimitThreshold.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.DisplayARStatementAgeingFields.CountryFilterPKs);
			ItemSet.ExporterExemptionCellingLimitThreshold.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 20M);
			AssertEquals("Value", 20M, ItemSet.ExporterExemptionCellingLimitThreshold.Value);
		}

		public void TestThresholdValidationFCEElectronicCreditInvoice()
		{
			AssertEquals("DefaultValue", 299555.00M, ItemSet.ThresholdValidationFCEElectronicCreditInvoice.DefaultValue);
			AssertEquals("CountryFilterPK", CountryFilterPKs.Argentina, ItemSet.ThresholdValidationFCEElectronicCreditInvoice.CountryFilterPKs);
			AssertEquals(Categories.Accounting_GovernmentComplianceInvoiceDocument_Argentina, ItemSet.ThresholdValidationFCEElectronicCreditInvoice.Category);
			AssertEquals(2, ((NumericRegistryEditorInfo)ItemSet.ThresholdValidationFCEElectronicCreditInvoice.EditorInfo).DecimalPlaces);

			ItemSet.ThresholdValidationFCEElectronicCreditInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 20M);
			AssertEquals("Overridden with different value", 20M, ItemSet.ThresholdValidationFCEElectronicCreditInvoice.Value);

			try
			{
				ItemSet.ThresholdValidationFCEElectronicCreditInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, -1m);
			}
			catch (Exception ex)
			{
				AssertEquals("Value must be greater than or equal to the minimum (0)", ex.Message);
			}
		}

		public void TestAlternateChartOfAccountsForSAFTv1_10()
		{
			const string expectedName = "AlternateChartOfAccountsForSAFT";
			const string expectedCaption = "Alternate Chart of Accounts for SAF-T";
			const string expectedHint = "Please select the Alternate Chart of Accounts for the Norwegian SAF-T report.";

			var item = ItemSet.AlternateChartOfAccountsForSAFT;
			TestGenericRegistryItem(item, expectedName, Categories.Accounting_GovernmentComplianceInvoiceDocument_Norway, expectedCaption, expectedHint, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
			AssertEquals("CountryFilterPKs", CountryFilterPKs.Norway, ItemSet.AlternateChartOfAccountsForSAFT.CountryFilterPKs);
			AssertEquals("DefaultValue", Guid.Empty, ItemSet.AlternateChartOfAccountsForSAFT.DefaultValue);
		}

		public void TestAlternateChartOfAccountsForSAFTv1_30()
		{
			const string expectedName = "AlternateChartOfAccountsForSAFT";
			const string expectedCaption = "Alternate Chart of Accounts for SAF-T";
			const string expectedHint = "Please select the Alternate Chart of Accounts for the Norwegian SAF-T report.";

			var mockFeatureManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));

			using (ObjectFactory.Substitute(mockFeatureManager.Object))
			{
				var item = ItemSet.AlternateChartOfAccountsForSAFT;
				TestGenericRegistryItem(item, expectedName, Categories.Accounting_GovernmentComplianceInvoiceDocument_Norway, expectedCaption, expectedHint, RegistryStorageFlags.Company, RegistryOptions.IsValueOptional);
				AssertEquals("CountryFilterPKs", CountryFilterPKs.Norway, ItemSet.AlternateChartOfAccountsForSAFT.CountryFilterPKs);
				AssertEquals("DefaultValue", Guid.Empty, ItemSet.AlternateChartOfAccountsForSAFT.DefaultValue);
			}
		}

		public void TestAlternateChartOfAccountsForSAFTValue()
		{
			var testCreator = new TestObjectCreator(Factory);
			var chart1 = testCreator.CreateAlternateChart("SAFTGL1", "SAF-T Alternate GL Accounts 1");
			testCreator.CreateAccAlternateChartFormat(chart1, 1, "X", "tier 1");
			var glHeader1 = testCreator.CreateGLHeader("Test.aa");

			Factory.Save();

			testCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart1.PK, AlternateGLAccountAttributeCode.LFE, true);
			var alternateGLAccount1 = testCreator.CreateAccAlternateGlAccount(chart1.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount1, glHeader1.PK, 1, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

			Factory.Save();

			var item = ItemSet.AlternateChartOfAccountsForSAFT;
			item.SetValue(CompanyPK, Guid.Empty, Guid.Empty, chart1.PK.ToGuid());
			AssertEquals("Registry Item Value 1", chart1.PK.ToGuid(), item.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));

			var oldValue1 = Guid.Empty;
			var newValue1 = chart1.PK.ToGuid();
			var regItem1 = Instance.AlternateChartOfAccountsForSAFT.Inner;
			var args1 = new RegistryItemWrapper.BuildLogReferenceArgs(regItem1, oldValue1, newValue1);
			var logReference1 = item.OnBuildLogReference(args1);
			AssertEquals("Log Reference 1", "Alternate Chart of Account Code For SAF-T changed from [] to [SAFTGL1].", logReference1);

			var chart2 = testCreator.CreateAlternateChart("SAFTGL2", "SAF-T Alternate GL Accounts 2");
			testCreator.CreateAccAlternateChartFormat(chart2, 1, "X", "tier 1");
			var glHeader2 = testCreator.CreateGLHeader("Test.bb");

			Factory.Save();

			testCreator.CreateAccAlternateGLAccountDissection(glHeader2, chart2.PK, AlternateGLAccountAttributeCode.LFE, true);
			var alternateGLAccount2 = testCreator.CreateAccAlternateGlAccount(chart2.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount2, glHeader2.PK, 1, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

			Factory.Save();

			item.SetValue(CompanyPK, Guid.Empty, Guid.Empty, chart2.PK.ToGuid());
			AssertEquals("Registry Item Value 2", chart2.PK.ToGuid(), item.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));

			var oldValue2 = chart1.PK.ToGuid();
			var newValue2 = chart2.PK.ToGuid();
			var regItem2 = Instance.AlternateChartOfAccountsForSAFT.Inner;
			var args2 = new RegistryItemWrapper.BuildLogReferenceArgs(regItem2, oldValue2, newValue2);
			var logReference2 = item.OnBuildLogReference(args2);
			AssertEquals("Log Reference 2", "Alternate Chart of Account Code For SAF-T changed from [SAFTGL1] to [SAFTGL2].", logReference2);
		}

		public void TestResetAddressInChargeWhenARInvoiceReversed()
		{
			AssertEquals("DefaultValue", false, ItemSet.ResetAddressInChargeWhenARInvoiceReversed.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.ResetAddressInChargeWhenARInvoiceReversed.CountryFilterPKs);
			ItemSet.ResetAddressInChargeWhenARInvoiceReversed.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ResetAddressInChargeWhenARInvoiceReversed.Value);
		}

		public void TestEnableUsersAuthorisedToReopenClosedPeriodsRegistry()
		{
			AssertEquals("DefaultValue", false, ItemSet.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.CountryFilterPKs);
			ItemSet.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.Value);
		}

		public void TestEnableSelfAdministrationToReopenClosedPeriods()
		{
			const string expectedName = "EnableSelfAdministrationToReopenClosedPeriods";
			const string expectedCaption = "Enable self-administration to reopen closed periods";
			const string expectedHint = "When this registry is set to 'YES', Self administration, the new approval form will be displayed. Otherwise as existing behavior, request key form will be displayed.";

			var item = ItemSet.EnableSelfAdministrationToReopenClosedPeriods;
			TestGenericRegistryItem(item, expectedName, Categories.Accounting, expectedCaption, expectedHint, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
			AssertEquals("DefaultValue", false, item.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), item.CountryFilterPKs);
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, item.Value);
		}

		[TestDate(2014, 06, 06, 13, 0, 0)]
		public void TestAllowAutomaticSubLedgerTakeup()
		{
			var registry = ItemSet.AllowAutomaticSubLedgerTakeup;
			AssertEquals("DefaultValue", ZDateTime.Empty, registry.DefaultValue.NextRunDateTime);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), registry.CountryFilterPKs);

			var bizO = new AutomaticProcessRegistryBusinessObject(Factory);
			var nowTime = ZDateTime.Now;
			bizO.NextRunDateTime = nowTime;
			bizO.Interval = 15;
			bizO.IntervalType = "MINUTES";

			registry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, bizO);
			AssertEquals("Value", nowTime, registry.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).NextRunDateTime);

			var newValue = new AutomaticProcessRegistryBusinessObject(Factory);
			newValue.NextRunDateTime = nowTime.AddDays(1);
			newValue.IntervalType = "DAYS";
			newValue.Interval = 2;

			var expectedLogMessage = $"Next Run Time has been set to {newValue.NextRunDateTime}.\r\nInterval has been set to {newValue.Interval} {newValue.IntervalType}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, bizO, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestAutomaticallySaveReconciliationReportWhenSavingBankReconciliation()
		{
			TestGenericRegistryItem(ItemSet.AutomaticallySaveReconciliationReportWhenSavingBankReconciliation,
				"AutomaticallySaveReconciliationReportWhenSavingBankReconciliation",
				Categories.Accounting_CashBookDefaults,
				"Automatically Save Reconciliation Report When Saving Bank Reconciliation",
				"Set this registry to 'No' to disable saving the reconciliation history report when you save the Bank Reconciliation.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestEnableChequeManagementFunctionality()
		{
			TestGenericRegistryItem(ItemSet.EnableChequeManagementFunctionality,
				"EnableChequeManagementFunctionality",
				Categories.Accounting_CashBookDefaults,
				"Enable Cheque Management Functionality (CargoWiseOne Support Only)",
				@"It's used to control the Cheque Management functionality. By default, this is set to 'No', when enabled Cheque transactions and tracking module will be available.

**PLEASE DO NOT ENABLE THIS REGISTRY FOR ANY CUSTOMER**",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestAllowFuturePostingOfCashBookTransactions()
		{
			AssertEquals("DefaultValue", false, ItemSet.AllowFuturePostingOfCashBookTransactions.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.AllowFuturePostingOfCashBookTransactions.CountryFilterPKs);
			ItemSet.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AllowFuturePostingOfCashBookTransactions.Value);
			AssertEquals("Option", RegistryOptions.Default, ItemSet.AllowFuturePostingOfCashBookTransactions.Options);
		}

		public void TestAllowFuturePostingOfCashBookTransactionsSecurity()
		{
			var security = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			AssertNotNull(security.CashBookAllowFuturePostingOfTransactions);
			AssertEquals(false, security.CashBookAllowFuturePostingOfTransactions.Visible);

			ItemSet.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			security = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			AssertNotNull(security.CashBookAllowFuturePostingOfTransactions);
			AssertEquals(true, security.CashBookAllowFuturePostingOfTransactions.Visible);
		}

		public void TestAllowManualEntryOfStatementDateWhenEnteringBankStatement()
		{
			AssertEquals("DefaultValue", false, ItemSet.AllowManualEntryOfStatementDateWhenEnteringBankStatement.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.AllowFuturePostingOfCashBookTransactions.CountryFilterPKs);
			ItemSet.AllowManualEntryOfStatementDateWhenEnteringBankStatement.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AllowManualEntryOfStatementDateWhenEnteringBankStatement.Value);
		}

		public void TestPeriodReopenLevels()
		{
			PeriodReopenLevelsCollection newValue = new PeriodReopenLevelsCollection();
			PeriodReopenLevels copy = newValue.AddNew();
			copy.AuthorisationRequirement = "1ST";
			copy.Days = 30;
			copy.Range = "DAE";
			ItemSet.PeriodReopenLevels.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			PeriodReopenLevelsCollection value = ItemSet.PeriodReopenLevels.Value;
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].AuthorisationRequirement", "1ST", value[0].AuthorisationRequirement);
			AssertEquals("Value[0].Days", 30, value[0].Days);
			AssertEquals("Value[0].Mode", "DAE", value[0].Range);
		}

		public void TestUsersAuthorizedToReopenClosedPeriodsSetting()
		{
			TestGenericRegistryItem(ItemSet.UsersAuthorizedToReopenClosedPeriods,
				"UsersAuthorizedToReopenClosedPeriods",
				"Accounting",
				"Users Authorized to Reopen Closed Periods",
				@"CargoWise support staff can enable the Reopening of Closed Accounting Periods for a login company. 
Only the users nominated in this list will be allowed to re-open accounting periods.
After enabling this registry, Period Reopen Levels must be defined and appropriate staff security levels assigned.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				!Instance.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.Value ? RegistryOptions.IsOnlyForDevelopers : RegistryOptions.Default);
		}

		public void TestIndiaGSTReversalAllowedPeriod()
		{
			const string expectedName = "IndiaGSTReversalAllowedPeriod";
			const string expectedCaption = "India GST Reversal Allowed Period (CargoWiseOne Support Only)";
			const string expectedHint = @"By default the government allows crediting GST 8 months after Financial year end date in India (31-March). However in special circumstances, the government might change this value. Change this value after consulting with the government notification as a temporary measure till we can make permanent changes to CW.";

			TestRegistryItem(ItemSet.IndiaGSTReversalAllowedPeriod,
				expectedName,
				Categories.Accounting,
				expectedCaption,
				expectedHint,
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				8, 0, int.MaxValue
			);
		}

		public void TestUsersAuthorizedToReopenClosedPeriods()
		{
			UsersAuthorizedToReopenClosedPeriodsCollection newValue = new UsersAuthorizedToReopenClosedPeriodsCollection();
			UsersAuthorizedToReopenClosedPeriods copy = newValue.AddNew();
			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			copy.StaffPK = staff.PK;
			ItemSet.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			UsersAuthorizedToReopenClosedPeriodsCollection value = ItemSet.UsersAuthorizedToReopenClosedPeriods.Value;
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].StaffPK", staff.PK, value[0].StaffPK);
			value[0].StaffPK = new ZGuid();
			value[0].StaffPK = staff.PK;
			AssertEquals("Value[0].LoginName", staff.GS_LoginName, value[0].LoginName);
			AssertEquals("Value[0].FullName", staff.GS_FullName, value[0].FullName);
		}

		public void TestUsersAuthorizedToReopenClosedPeriodsRegistryVisibility_Enabled()
		{
			ItemSet.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should be visible only to CWSupport", RegistryOptions.Default, ItemSet.UsersAuthorizedToReopenClosedPeriods.Options);
		}

		public void TestUsersAuthorizedToReopenClosedPeriodsRegistryVisibility_Disabled()
		{
			ItemSet.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should be visible to Users", RegistryOptions.IsOnlyForDevelopers, ItemSet.UsersAuthorizedToReopenClosedPeriods.Options);
		}

		public void TestRevenueRecognitionSetup()
		{
			RevenueRecognitionCollection newValue = new RevenueRecognitionCollection();
			RevenueRecognition copy = newValue.AddNew();
			copy.JobType = "SHP";
			copy.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			copy.Mode = "AIR";
			copy.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			copy.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
			ItemSet.RevenueRecognitionSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			RevenueRecognitionCollection value = ItemSet.RevenueRecognitionSetup.Value;
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].JobType", "SHP", value[0].JobType);
			AssertEquals("Value[0].Direction", Constants.FreightShipmentDirection.Code.Import, value[0].DirectionCode);
			AssertEquals("Value[0].Mode", "AIR", value[0].Mode);
			AssertEquals("Value[0].RecognitionDateOption", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, value[0].RecognitionDateOptionCode);
			AssertEquals("Value[0].Broker", RevenueRecognitionLookups.BrokerCodes.All, value[0].BrokerCode);
		}

		public void TestRevenueRecognitionByChargeGroupSetup()
		{
			RevenueRecognitionByChargeGroupCollection newValue = new RevenueRecognitionByChargeGroupCollection();
			RevenueRecognitionByChargeGroup copy = newValue.AddNew();
			copy.ChargeGroup = "ITC";
			RevenueRecognition revRecog = copy.ChargeGroupSettings.AddNew();
			revRecog.JobType = "SHP";
			revRecog.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			revRecog.Mode = "AIR";
			revRecog.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			revRecog.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;

			ItemSet.RevenueRecognitionByChargeGroupSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			RevenueRecognitionByChargeGroupCollection value = ItemSet.RevenueRecognitionByChargeGroupSetup.Value;
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].JobType", "SHP", value[0].ChargeGroupSettings[0].JobType);
			AssertEquals("Value[0].Direction", Constants.FreightShipmentDirection.Code.Import, value[0].ChargeGroupSettings[0].DirectionCode);
			AssertEquals("Value[0].Mode", "AIR", value[0].ChargeGroupSettings[0].Mode);
			AssertEquals("Value[0].RecognitionDateOption", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, value[0].ChargeGroupSettings[0].RecognitionDateOptionCode);
			AssertEquals("Value[0].Broker", RevenueRecognitionLookups.BrokerCodes.All, value[0].ChargeGroupSettings[0].BrokerCode);
			AssertEquals("Value[0].ChargeGroup", "ITC", value[0].ChargeGroup);
		}

		public void TestRecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture()
		{
			TestGenericRegistryItem(ItemSet.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture,
				"RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture",
				"Accounting/Job Invoicing",
				"Recognize Revenue using Current Date when Revenue Recognition Date is in the future",
				 @"This registry affects the recognition of transactions posted against jobs with a Revenue Recognition Date that falls in the future. 

By default, the registry is set to ‘No’ and the system will recognize the transactions based on the respective revenue recognition setting as per current system behavior.

When the registry is overridden to ‘Yes’, the system will recognize the transactions using current date if the revenue recognition date falls in the future. 

Note: Any change to this registry value will not affect job revenue recognition dates that have already been recorded against the job. All subsequent job charges posted against the same job with the same recognition type will be recognized using the same date that has been recorded against the job except for IMM revenue recognition type.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);

			var registry = ItemSet.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestJobClosureConfigurationSetup()
		{
			var newValue = new JobClosureConfigurationHeader();
			var configLine = newValue.ConfigurationCollection.AddNew();
			configLine.JobType = "SHP";
			configLine.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			configLine.Mode = "AIR";
			configLine.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			configLine.Offset = 10;
			configLine.ReopenRestrictionOffset = 5;

			ItemSet.JobClosureConfigurationSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			var value = ItemSet.JobClosureConfigurationSetup.Value;
			AssertEquals("Line Count", 1, value.ConfigurationCollection.Count);
			AssertEquals("Line[0].JobType", "SHP", value.ConfigurationCollection[0].JobType);
			AssertEquals("Line[0].Direction", Constants.FreightShipmentDirection.Code.Import, value.ConfigurationCollection[0].DirectionCode);
			AssertEquals("Line[0].Mode", "AIR", value.ConfigurationCollection[0].Mode);
			AssertEquals("Line[0].RecognitionDateOption", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, value.ConfigurationCollection[0].JobClosureDateOptionCode);
			AssertEquals("Line[0].OffsetType", "DAY", value.ConfigurationCollection[0].OffsetType);
			AssertEquals("Line[0].Offset", 10, value.ConfigurationCollection[0].Offset);
			AssertEquals("Line[0].ReopenRestrictionOffset", 5, value.ConfigurationCollection[0].ReopenRestrictionOffset);

			var registry = ItemSet.JobClosureConfigurationSetup;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, new JobClosureConfigurationHeader(), newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(@"Configuration Added: Job Type = SHP, Direction = IMP, Mode = AIR, Dept = , Update/Close = Close, From Status = , To Status = CLS, Open Accruals = No, Open WIPs = No, Recognized Charges = ALL, Relevant Date = DEP, Offset = 10, Offset Type = DAY, Reopen Offset = 5, Reopen Offset Type = DAY
", logReference);

			JobClosureConfigurationHeader oldValue = new JobClosureConfigurationHeader();
			var configLineForOld = oldValue.ConfigurationCollection.AddNew();
			configLineForOld.JobType = "SHP";
			configLineForOld.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			configLineForOld.Mode = "AIR";
			configLineForOld.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			configLineForOld.Offset = 10;
			configLineForOld.ReopenRestrictionOffset = 5;

			args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, new JobClosureConfigurationHeader());
			logReference = registry.OnBuildLogReference(args);
			AssertEquals(@"Configuration Deleted: Job Type = SHP, Direction = IMP, Mode = AIR, Dept = , Update/Close = Close, From Status = , To Status = CLS, Open Accruals = No, Open WIPs = No, Recognized Charges = ALL, Relevant Date = DEP, Offset = 10, Offset Type = DAY, Reopen Offset = 5, Reopen Offset Type = DAY
", logReference);

			var identifier = ZGuid.NewZGuid();
			configLine.Identifier = identifier;
			configLineForOld.Identifier = identifier;
			configLine.ConfigurationType = "UPD";
			args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			logReference = registry.OnBuildLogReference(args);
			AssertEquals(@"Configuration Changed: Job Type = SHP, Direction = IMP, Mode = AIR, Dept = , Update/Close = Update, From Status = , To Status = JFC, Open Accruals = No, Open WIPs = No, Recognized Charges = ALL, Relevant Date = DEP, Offset = 10, Offset Type = DAY, Reopen Offset = 5, Reopen Offset Type = DAY
", logReference);

			configLineForOld.ConfigurationType = "UPD";
			args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			logReference = registry.OnBuildLogReference(args);
			AssertNullOrEmpty(logReference);
		}

		public void TestHidingGLJournalsApprovalItems()
		{
			Assert("NewGLJournalApprovalThresholdSetup IsHidden", !ItemSet.NewGLJournalApprovalThresholdSetup.HasOption(RegistryOptions.IsHidden));
			Assert("ExistingGLJournalApprovalThresholdSetup IsHidden", !ItemSet.ExistingGLJournalApprovalThresholdSetup.HasOption(RegistryOptions.IsHidden));
			Assert("AllowOnTheSpotApprovalsOfGLJournals IsHidden", !ItemSet.AllowOnTheSpotApprovalsOfGLJournals.HasOption(RegistryOptions.IsHidden));
			Assert("AllowUsersToApproveOwnGLJournals IsHidden", !ItemSet.AllowUsersToApproveOwnGLJournals.HasOption(RegistryOptions.IsHidden));
			Assert("GLJournalsApprovalNotifyGroup IsHidden", !ItemSet.GLJournalsApprovalNotifyGroup.HasOption(RegistryOptions.IsHidden));
		}

		public void TestGLJournalsApprovalItemsDefaultValues()
		{
			AssertEquals("NewGLJournalApprovalThresholdSetup", 0, ItemSet.NewGLJournalApprovalThresholdSetup.Value.Count);
			AssertEquals("ExistingGLJournalApprovalThresholdSetup", 0, ItemSet.ExistingGLJournalApprovalThresholdSetup.Value.Count);
			Assert("AllowOnTheSpotApprovalsOfGLJournals", ItemSet.AllowOnTheSpotApprovalsOfGLJournals.Value);
			Assert("AllowUsersToApproveOwnGLJournals", ItemSet.AllowUsersToApproveOwnGLJournals.Value);
		}

		public void TestNewGLJournalApprovalThresholdSetup()
		{
			var newValue = new GLJournalApprovalThresholdCollection();
			GLJournalApprovalThreshold copy = newValue.AddNew();
			copy.Type = "RSN";
			copy.ReportSection = "OV";

			var settings1 = new PaymentThreeLevelAuthorisationSettings();
			settings1.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.Amount = 1000.00m;
			settings1.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			copy.AuthorisationSettings.Add(settings1);

			var settings2 = new PaymentThreeLevelAuthorisationSettings();
			settings2.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			settings2.Amount = 1000.00m;
			settings2.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			copy.AuthorisationSettings.Add(settings2);

			ItemSet.NewGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			GLJournalApprovalThresholdCollection registryValue = ItemSet.NewGLJournalApprovalThresholdSetup.Value;
			AssertEquals("registryValue.Count", 1, registryValue.Count);
			AssertEquals("RSN", registryValue[0].Type);
			AssertEquals("OV", registryValue[0].ReportSection);
			AssertEquals("registryValue.AuthorisationSettings.Count", 2, registryValue[0].AuthorisationSettings.Count);

			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo, registryValue[0].AuthorisationSettings[0].Range);
			AssertEquals(1000m, registryValue[0].AuthorisationSettings[0].Amount);
			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, registryValue[0].AuthorisationSettings[0].AuthorisationRequirement);

			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above, registryValue[0].AuthorisationSettings[1].Range);
			AssertEquals(1000m, registryValue[0].AuthorisationSettings[1].Amount);
			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, registryValue[0].AuthorisationSettings[1].AuthorisationRequirement);

			AssertEquals(@"Use this registry to set the amount threshold at which an authorized user can approve new GL Journals.

The authorization levels (up to 3) restrict the amounts that can be approved by users with specific authorization levels that is set up in the security settings.
'The authorization requirement 'None' allows user with access to save and approve journals with amount up to the specific threshold level.

NOTE: When ANY type is configured, all new GL Journals (except for NJL – Note Journal) will need to be approved before they can be posted.", ItemSet.NewGLJournalApprovalThresholdSetup.Hint);
		}

		public void TestExistingGLJournalApprovalThresholdSetup()
		{
			var newValue = new GLJournalApprovalThresholdCollection();
			GLJournalApprovalThreshold copy = newValue.AddNew();
			copy.Type = "RSN";
			copy.ReportSection = "OV";

			var settings1 = new PaymentThreeLevelAuthorisationSettings();
			settings1.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.Amount = 1000.00m;
			settings1.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			copy.AuthorisationSettings.Add(settings1);

			var settings2 = new PaymentThreeLevelAuthorisationSettings();
			settings2.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			settings2.Amount = 1000.00m;
			settings2.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			copy.AuthorisationSettings.Add(settings2);

			ItemSet.ExistingGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			GLJournalApprovalThresholdCollection registryValue = ItemSet.ExistingGLJournalApprovalThresholdSetup.Value;
			AssertEquals("registryValue.Count", 1, registryValue.Count);
			AssertEquals("RSN", registryValue[0].Type);
			AssertEquals("OV", registryValue[0].ReportSection);
			AssertEquals("registryValue.AuthorisationSettings.Count", 2, registryValue[0].AuthorisationSettings.Count);

			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo, registryValue[0].AuthorisationSettings[0].Range);
			AssertEquals(1000m, registryValue[0].AuthorisationSettings[0].Amount);
			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, registryValue[0].AuthorisationSettings[0].AuthorisationRequirement);

			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above, registryValue[0].AuthorisationSettings[1].Range);
			AssertEquals(1000m, registryValue[0].AuthorisationSettings[1].Amount);
			AssertEquals(AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, registryValue[0].AuthorisationSettings[1].AuthorisationRequirement);

			AssertEquals(@"Use this registry to set the amount threshold at which an authorized user can approve existing approved GL Journals.

The authorization levels (up to 3) restrict the amounts that can be approved by users with specific authorization levels that is set up in the security settings.
'The authorization requirement 'None' allows user with access to save and approve journals with amount up to the specific threshold level.

NOTE: When ANY type is configured, all changes to existing GL Journals (except for NJL – Note Journal) will need to be approved.", ItemSet.ExistingGLJournalApprovalThresholdSetup.Hint);
		}

		public void TestJobInvoiceDescriptionConfiguration()
		{
			JobInvoiceDescriptionCollection newValue = new JobInvoiceDescriptionCollection();
			JobInvoiceDescription copy = newValue.AddNew();
			copy.JobType = "SHP";
			copy.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			copy.Mode = "AIR";
			copy.InvoiceDescription = "Description of <JobNumber>";
			ItemSet.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			JobInvoiceDescriptionCollection value = ItemSet.JobInvoiceDescriptionConfiguration.Value;
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].JobType", "SHP", value[0].JobType);
			AssertEquals("Value[0].Direction", Constants.FreightShipmentDirection.Code.Import, value[0].DirectionCode);
			AssertEquals("Value[0].Mode", "AIR", value[0].Mode);
			AssertEquals("Value[0].InvoiceDescription", "Description of <JobNumber>", value[0].InvoiceDescription);
		}

		public void TestDisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes()
		{
			const string expectedName = "DisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes";
			const string expectedCaption = "Disable Display Sequence Calculation for Warehouse Periodic Invoicing Job Types (CargoWiseOne Support Only)";
			const string expectedHint = @"By default, the system will calculate the display sequence as per current behavior. 

When this registry is set to ‘Yes’,  the display sequence of warehouse periodic invoicing jobs will not be calculated. The display sequence will always be set to 0 and all validation in relation to display sequence will be suspended.";

			TestRegistryItem(ItemSet.DisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes,
				expectedName,
				Categories.Accounting_JobInvoicing,
				expectedCaption,
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestElectronicProcessingChargeCode()
		{
			const string expectedName = "ElectronicProcessingChargeCode";
			const string expectedCaption = "Electronic Processing Charge Code";
			const string expectedHint = "This charge code will be used for the posting of accounting transactions relating to the Electronic Processing Charge.";

			var item = ItemSet.ElectronicProcessingChargeCode;
			TestGenericRegistryItem(item, expectedName, Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement, expectedCaption, expectedHint, RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.IsValueMandatory);

			AssertElectronicProcessingChargeCode(true, true);
			AssertElectronicProcessingChargeCode(true, false);
			AssertElectronicProcessingChargeCode(false, true);
			AssertElectronicProcessingChargeCode(false, false);

			var chargeCodePK = Guid.NewGuid();
			ItemSet.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCodePK);
			AssertEquals(chargeCodePK, ItemSet.ElectronicProcessingChargeCode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			var testCreator = new TestObjectCreator(Factory);
			var oldChargeCode = testCreator.CreateGlobalChargeCode("AAA");
			oldChargeCode.AC_ChargeType = "DSB";
			var newChargeCode = testCreator.CreateGlobalChargeCode("BBB");
			newChargeCode.AC_ChargeType = "DSB";

			Factory.Save();

			var oldValue = oldChargeCode.PK.ToGuid();
			var newValue = newChargeCode.PK.ToGuid();
			var regItem = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Inner;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.OnBuildLogReference(args);
			AssertEquals("Charge Code changed from [AAA] to [BBB].", logReference);
		}

		void AssertElectronicProcessingChargeCode(bool isRegistryEnabled, bool isSupportUser)
		{
			ItemSet.RemoveItemFromCacheIfOlderThan("ElectronicProcessingChargeCode", TimeSpan.MinValue);

			using (AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isRegistryEnabled))
			using (EnvProxy.Instance.SetTemporaryUserContext(isSupportUser ? User.SupportUserName : User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var expectedOptions = isRegistryEnabled ? (isSupportUser ? RegistryOptions.Default : RegistryOptions.IsReadOnly) : RegistryOptions.IsHidden;
				AssertEquals(expectedOptions | RegistryOptions.IsValueMandatory, ItemSet.ElectronicProcessingChargeCode.Options);
			}
		}

		public void TestEnableElectronicProcessingChargeFunctionality()
		{
			const string expectedName = "EnableElectronicProcessingChargeFunctionality";
			const string expectedCaption = "Enable Electronic Processing Charge Functionality (CargoWiseOne Support Only)";
			const string expectedHint = @"When this registry is set to 'Yes', the system will auto insert electronic processing charge when invoicing job is created for 'SHP - Shipment' job type. 
When this registry is set to 'No', the system will stop the insertion of electronic processing charge for new invoicing job created thereafter.";

			TestRegistryItem(ItemSet.EnableElectronicProcessingChargeFunctionality,
				expectedName,
				Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
				expectedCaption,
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false
			);

			var registry = ItemSet.EnableElectronicProcessingChargeFunctionality;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestElectronicProcessingChargeDisbursementClearingAccount()
		{
			const string expectedName = "ElectronicProcessingChargeDisbursementClearingAccount";
			const string expectedCaption = "Electronic Processing Charge Disbursement Clearing Account";
			const string expectedHint = "This GL Account will be used for the recording and clearing of the Electronic Processing Disbursement Charge.";

			var item = ItemSet.ElectronicProcessingChargeDisbursementClearingAccount;
			AsserttElectronicProcessingChargeClearingAccount(item, expectedName, expectedCaption, expectedHint, RegistryFindBoxFilter.PandLOrBSHAndNonControl_DisallowDirectPost);

			AssertElectronicProcessingChargeClearingAccountRegistryOptions(true, true, true);
			AssertElectronicProcessingChargeClearingAccountRegistryOptions(true, true, false);
			AssertElectronicProcessingChargeClearingAccountRegistryOptions(true, false, true);
			AssertElectronicProcessingChargeClearingAccountRegistryOptions(true, false, false);
		}

		public void TestElectronicProcessingChargePayableClearingAccount()
		{
			const string expectedName = "ElectronicProcessingChargePayableClearingAccount";
			const string expectedCaption = "Electronic Processing Charge Payable Clearing Account";
			const string expectedHint = "This GL Account will be used for the recording and clearing of the Electronic Processing Charge Payables.";

			var item = ItemSet.ElectronicProcessingChargePayableClearingAccount;
			AsserttElectronicProcessingChargeClearingAccount(item, expectedName, expectedCaption, expectedHint, RegistryFindBoxFilter.PandLOrBSHAndNonControl_AllowDirectPost);

			AssertElectronicProcessingChargeClearingAccountRegistryOptions(false, true, true);
			AssertElectronicProcessingChargeClearingAccountRegistryOptions(false, true, false);
			AssertElectronicProcessingChargeClearingAccountRegistryOptions(false, false, true);
			AssertElectronicProcessingChargeClearingAccountRegistryOptions(false, false, false);
		}

		void AsserttElectronicProcessingChargeClearingAccount(GuidRegistryItem item, string expectedName, string expectedCaption, string expectedHint, RegistryFindBoxFilter expectedRegistryFindBoxFilter)
		{
			TestGenericRegistryItem(item, expectedName, Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement, expectedCaption, expectedHint, RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.IsValueMandatory);

			AssertType<GuidFindBoxRegistryEditorInfo>("Editor Info", item.EditorInfo);
			var editorInfo = (GuidFindBoxRegistryEditorInfo)item.EditorInfo;
			AssertEquals("Find Box Collection", RegistryFindBoxCollection.AccGLHeader, editorInfo.FindBoxCollection);
			AssertEquals("Find Box Collection Filter", expectedRegistryFindBoxFilter, editorInfo.FindBoxFilter);

			var oldGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var newGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			oldGLHeader.AG_AccountNum = "Test.aa";
			newGLHeader.AG_AccountNum = "Test.bb";
			Factory.Save();

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(item, oldGLHeader.PK, newGLHeader.PK);
			var logReference = item.OnBuildLogReference(args);
			AssertEquals($"GL Account changed from [Test.aa] to [Test.bb].", logReference);
		}

		void AssertElectronicProcessingChargeClearingAccountRegistryOptions(bool isForRecovery, bool isEnableElectronicProcessingCharge, bool isSupportUser)
		{
			ItemSet.RemoveItemFromCacheIfOlderThan(isForRecovery ? "ElectronicProcessingChargeDisbursementClearingAccount" : "ElectronicProcessingChargePayableClearingAccount", TimeSpan.MinValue);
			using (AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isEnableElectronicProcessingCharge))
			using (EnvProxy.Instance.SetTemporaryUserContext(isSupportUser ? User.SupportUserName : User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				if (isForRecovery)
				{
					var expectedOptions = isEnableElectronicProcessingCharge ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden;
					AssertEquals(expectedOptions | RegistryOptions.IsValueMandatory, ItemSet.ElectronicProcessingChargeDisbursementClearingAccount.Options);
				}
				else
				{
					var expectedOptions = isEnableElectronicProcessingCharge ? (isSupportUser ? RegistryOptions.Default : RegistryOptions.IsReadOnly) : RegistryOptions.IsHidden;
					AssertEquals(expectedOptions | RegistryOptions.IsValueMandatory, ItemSet.ElectronicProcessingChargePayableClearingAccount.Options);
				}
			}
		}

		public void TestElectronicProcessingChargeConfiguration()
		{
			var registryItem = ItemSet.ElectronicProcessingChargeConfiguration;
			AssertEquals("Name", "ElectronicProcessingChargeConfiguration", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement, registryItem.Category);
			AssertEquals("Caption", "Electronic Processing Charge Configuration (CargoWise Support Only)", registryItem.Caption);
			AssertEquals("Hint", @"This registry defines the Job Types and relative Start Date and End Date for the posting of accounting transactions relating to the Electronic Processing Charge when the functionality has been enabled.
Note:
The job header will be always created for the corresponding Job Type during the date range specified and the Registry 'Add Job Invoicing Record at Saving/Editing of Operations Job' will be bypassed.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, registryItem.Storage);

			var mockIFeatureData = new Mock<IFeatureData>();
			var electronicProcessingChargeFeatureControlModel = new ElectronicProcessingChargeFeatureControlModel();
			electronicProcessingChargeFeatureControlModel.ElectronicProcessingChargeConfiguration = new List<ElectronicProcessingChargeConfigurationModel>();
			var startDate = ZDate.Today.AddDays(-1).ToDateTime();
			var endDate = ZDate.Today.AddDays(1).ToDateTime();
			electronicProcessingChargeFeatureControlModel.ElectronicProcessingChargeConfiguration.Add(new ElectronicProcessingChargeConfigurationModel() { JobType = "SHP", StartDate = startDate, EndDate = endDate });

			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out electronicProcessingChargeFeatureControlModel)).Returns(true);
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingElectronicProcessingChargeFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("ElectronicProcessingChargeConfiguration", TimeSpan.MinValue);

				AssertEquals(1, ItemSet.ElectronicProcessingChargeConfiguration.DefaultValue.Count);
				var defaultValueItem = ItemSet.ElectronicProcessingChargeConfiguration.DefaultValue.Cast<ElectronicProcessingChargeConfiguration>().First();
				AssertEquals("SHP", defaultValueItem.JobType);
				AssertEquals(startDate, defaultValueItem.StartDate);
				AssertEquals(endDate, defaultValueItem.EndDate);
			}

			AssertElectronicProcessingChargeConfigurationOptions(true);
			AssertElectronicProcessingChargeConfigurationOptions(false);

			AssertElectronicProcessingChargeConfigurationLog();

			void AssertElectronicProcessingChargeConfigurationOptions(bool isEnableElectronicProcessingCharge)
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("ElectronicProcessingChargeConfiguration", TimeSpan.MinValue);
				using (AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableElectronicProcessingCharge))
				{
					var expectedOptions = isEnableElectronicProcessingCharge ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden;
					AssertEquals(expectedOptions, ItemSet.ElectronicProcessingChargeConfiguration.Options);
				}
			}

			void AssertElectronicProcessingChargeConfigurationLog()
			{
				var fallbackLevel = new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				var orginalConfigurationCollection = new ElectronicProcessingChargeConfigurationCollection(fallbackLevel, Factory)
				{
					new ElectronicProcessingChargeConfiguration()
					{
						JobType = "FCN",
						StartDate = new ZDate(2025, 02, 13),
						EndDate = new ZDate(2025, 02, 13)
					},
					new ElectronicProcessingChargeConfiguration()
					{
						JobType = "GCN",
						StartDate = new ZDate(2025, 02, 12),
						EndDate = new ZDate(2025, 02, 23)
					},
					new ElectronicProcessingChargeConfiguration()
					{
						JobType = "CSH",
						StartDate = new ZDate(2025, 02, 05),
						EndDate = new ZDate(2025, 02, 25)
					}
				};

				var newConfigurationCollection = new ElectronicProcessingChargeConfigurationCollection(fallbackLevel, Factory)
				{
					new ElectronicProcessingChargeConfiguration()
					{
						JobType = "FCN",
						StartDate = new ZDate(2025, 02, 13),
						EndDate = new ZDate(2025, 02, 13)
					},
					new ElectronicProcessingChargeConfiguration()
					{
						JobType = "GCN",
						StartDate = new ZDate(2025, 02, 12),
						EndDate = new ZDate(2025, 02, 16)
					},
					new ElectronicProcessingChargeConfiguration()
					{
						JobType = "SHP",
						StartDate = new ZDate(2025, 02, 08),
						EndDate = new ZDate(2025, 02, 08)
					}
				};

				var args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, orginalConfigurationCollection, newConfigurationCollection);
				var logReference = registryItem.OnBuildLogReference(args);
				AssertEquals(@"Deleted: Job Type=GCN, Start Date=12-Feb-25, End Date=23-Feb-25
Added: Job Type=GCN, Start Date=12-Feb-25, End Date=16-Feb-25
Deleted: Job Type=CSH, Start Date=05-Feb-25, End Date=25-Feb-25
Added: Job Type=SHP, Start Date=08-Feb-25, End Date=08-Feb-25
", logReference);
			}
		}

		public void TestElectronicProcessingChargeDescriptionOverride()
		{
			var registryItem = ItemSet.ElectronicProcessingChargeDescriptionOverride;
			AssertEquals("Name", "ElectronicProcessingChargeDescriptionOverride", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement, registryItem.Category);
			AssertEquals("Caption", "Electronic Processing Charge Description Override", registryItem.Caption);
			AssertEquals("Hint", @"This registry enables you to configure default Electronic Processing Charge line description to help you differentiate Electronic Processing Charge generated from different jobs.

By default, the Electronic Processing Charge Code description is used as charge line description for sale invoices.

If you override this registry, the system will add these elements as part of the charge line description.

Note:
1. This Charge Description Override configuration only applies to SHP-Shipment Job Type.
2. For Domestic jobs, the system prioritizes applying job header branch rule SUJ/NSJ. If no configuration matches, then the company rule SCC/NSC will be used.
3. Shipment number will always be placed at the end of the charge line description.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
			AssertElectronicProcessingChargeDescriptionOverrideOptions(true);
			AssertElectronicProcessingChargeDescriptionOverrideOptions(false);
		}

		void AssertElectronicProcessingChargeDescriptionOverrideOptions(bool isEnableElectronicProcessingCharge)
		{
			ItemSet.RemoveItemFromCacheIfOlderThan("ElectronicProcessingChargeDescriptionOverride", TimeSpan.MinValue);
			using (AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableElectronicProcessingCharge))
			{
				var expectedOptions = isEnableElectronicProcessingCharge ? RegistryOptions.Default : RegistryOptions.IsHidden;
				AssertEquals(expectedOptions, ItemSet.ElectronicProcessingChargeDescriptionOverride.Options);
			}
		}

		public void TestElectronicProcessingChargeCurrency()
		{
			var registry = ItemSet.ElectronicProcessingChargeCurrency;

			AssertEquals("Name", "ElectronicProcessingChargeCurrency", registry.Name);
			AssertEquals("Category", Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement, registry.Category);
			AssertEquals("Caption", "Electronic Processing Charge Currency (CargoWiseOne Support Only)", registry.Caption);
			AssertEquals("Hint", @"The specified currency will be used to locate the applicable license fee from the reference database for the creation of the accrued transactions relating to Electronic Processing Charge.
This registry will be in a table layout with two columns. Both values must be provided
1. Currency: This refers to the charge currency.
2. Valid From Date: This refers to the start date when the charge currency should be used. Note: It can be superseded by a later start date.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, registry.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ElectronicProcessingChargeCurrency.Options);
			AssertEquals(0, registry.DefaultValue.Count);

			var mockIFeatureData = new Mock<IFeatureData>();
			var electronicProcessingChargeFeatureControlModel = new ElectronicProcessingChargeFeatureControlModel();
			electronicProcessingChargeFeatureControlModel.ElectronicProcessingChargeCurrency = new List<ElectronicProcessingChargeCurrencyModel>();
			electronicProcessingChargeFeatureControlModel.ElectronicProcessingChargeCurrency.Add(new ElectronicProcessingChargeCurrencyModel() { CurrencyCode = "CNY", ValidFromDate = DateTime.Today });
			electronicProcessingChargeFeatureControlModel.ElectronicProcessingChargeCurrency.Add(new ElectronicProcessingChargeCurrencyModel() { CurrencyCode = "XXXXX", ValidFromDate = DateTime.Today });

			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out electronicProcessingChargeFeatureControlModel)).Returns(true);
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingElectronicProcessingChargeFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("ElectronicProcessingChargeCurrency", TimeSpan.MinValue);

				AssertEquals(1, ItemSet.ElectronicProcessingChargeCurrency.DefaultValue.Count);
				var defaultValueItem = ItemSet.ElectronicProcessingChargeCurrency.DefaultValue.Cast<ElectronicProcessingChargeCurrency>().First();
				AssertEquals("CNY", defaultValueItem.Currency.RX_Code);
				AssertEquals(DateTime.Today, defaultValueItem.ValidFromDate);
			}

			var hasValueCollection = new ElectronicProcessingChargeCurrencyCollection();
			var noValueCollection = new ElectronicProcessingChargeCurrencyCollection();
			var value = new ElectronicProcessingChargeCurrency();
			var testObjectCreator = new TestObjectCreator(Factory);
			value.CurrencyPK = testObjectCreator.CNY.PK;
			value.ValidFromDate = ZDateTime.Now;
			hasValueCollection.Add(value);

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, noValueCollection, hasValueCollection);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals($"Override Added: CNY, {value.ValidFromDate}\r\n", logReference);

			var newValueCollection = new ElectronicProcessingChargeCurrencyCollection();
			var newValue = new ElectronicProcessingChargeCurrency();
			newValue.CurrencyPK = testObjectCreator.USD.PK;
			newValue.ValidFromDate = ZDateTime.Now.AddDays(1);
			newValueCollection.Add(newValue);
			var newArgs = new RegistryItemWrapper.BuildLogReferenceArgs(registry, hasValueCollection, newValueCollection);
			var newLogReference = registry.OnBuildLogReference(newArgs);
			AssertEquals($"Override Deleted: CNY, {value.ValidFromDate}\r\nOverride Added: USD, {newValue.ValidFromDate}\r\n", newLogReference);
		}

		public void TestCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestRegistryItem(ItemSet.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting,
				"CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting",
				Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
				"Carry Over Invoice Line Description During Intercompany Invoice Posting",
				@"When this registry is set to 'Yes', the system will carry over the invoice line description to the respective job charge line during Intercompany Invoice Posting.
When this registry is set to 'No', the system will work as before.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsHidden,
				true);
			ItemSet.EnableElectronicProcessingChargeFunctionality.SetValue(testObjectCreator.NonCurrentNonDemoCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			ItemSet.RemoveItemFromCacheIfOlderThan("CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting", TimeSpan.MinValue);
			AssertEquals(RegistryOptions.Default, ItemSet.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.Options);
			AssertEquals(true, ItemSet.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.IsVisible(testObjectCreator.NonCurrentNonDemoCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(false, ItemSet.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestIncludeDisbursementsPercentageMarginCalculations()
		{
			TestRegistryItem(ItemSet.IncludeDisbursementsPercentageMarginCalculations,
				"IncludeDisbursementsPercentageMarginCalculations",
				Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing,
				"Include Disbursements Percentage Margin Calculations",
				"When this registry is set to 'NO', any charge line to a charge code with a Charge Type of DSB – Disbursement will not be included in the percentage margin calculations.Charge code Type Overrides will be respected. i.e. if a charge code has a Charge Type of MRG, with a Type Override of DSB for Export Air jobs, on all jobs except Export Air, that charge code will be included in the percentage margin calculations. On Export Air jobs, charges to that charge code will be excluded.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);

			AssertEquals("DefaultValue", false, ItemSet.IncludeDisbursementsPercentageMarginCalculations.DefaultValue);
			ItemSet.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.IncludeDisbursementsPercentageMarginCalculations.Value);
			ItemSet.IncludeDisbursementsPercentageMarginCalculations.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.IncludeDisbursementsPercentageMarginCalculations.Value);
		}

		public void TestPostIntoNextOpenPeriodWhenRecognitionPeriodClosed()
		{
			AssertEquals("DefaultValue", false, ItemSet.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.DefaultValue);

			ItemSet.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.Value);
		}

		public void TestBackDateInvoicesConfiguration()
		{
			AssertEquals("DefaultValue", false, ItemSet.BackDateInvoicesConfiguration.Value.OverridePostDate);
			AssertEquals("DefaultValue", false, ItemSet.BackDateInvoicesConfiguration.Value.DefaultPostDateFromInvoiceDate);

			ItemSet.BackDateInvoicesConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				new BackDateInvoicesConfiguration()
				{
					OverridePostDate = true,
					DefaultPostDateFromInvoiceDate = true
				});
			AssertEquals("Value", true, ItemSet.BackDateInvoicesConfiguration.Value.OverridePostDate);
			AssertEquals("Value", true, ItemSet.BackDateInvoicesConfiguration.Value.DefaultPostDateFromInvoiceDate);
		}

		public void TestDefaultAllowUsersToBackDateInvoicesSetting()
		{
			AssertEquals("DefaultValue", true, ItemSet.DefaultAllowUsersToBackDateInvoicesSetting.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.DefaultAllowUsersToBackDateInvoicesSetting.CountryFilterPKs);
			ItemSet.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.DefaultAllowUsersToBackDateInvoicesSetting.Value);
		}

		#region Job Invoicing Default Departments Tests

		public void TestJobInvoicingDefaultDepartmentForwardingExportSeaFcl()
		{
			AssertEquals("DefaultValue", RegistryConstants.DepartmentPKs.ForwardingExportSea, ItemSet.JobInvoicingDefaultDepartmentForwardingExportSeaFcl.DefaultValue[0].Department);
		}

		#endregion

		#region Control Account Tests

		string GetGLHeaderAccountNum(Guid pk)
		{
			AccGLHeader obj = Factory.Load<AccGLHeader>(pk);
			return obj != null ? obj.AccountNum.ToString() : string.Empty;
		}

		// If test DB version is higher than 0, only some StmData (and Accounting) base data will get inserted and thus be correctly asserted.
		// (See $/Dev/Enterprise/Product/Core/DbUpgrader/Data/BaseData/StmData)
		bool HasDefaultValueFromBaseData(string name, string accountNum)
		{
			ZQuery query = new ZQuery(StmDataSchema.SD_Name, name);
			StmData obj = Factory.LoadTop1<StmData>(query);
			return obj != null && GetGLHeaderAccountNum(obj.SD_GuidValue.ToGuid()) == accountNum;
		}

		public void TestClearAllControlAccountRegistryItems()
		{
			AssertEquals("Should have some Control Accounts set in Registry", true, ItemSet.AreControlAccountRegistryItemsSet());
			ItemSet.ClearAllControlAccountRegistryItems();
			AssertEquals("Should have no Control Accounts set in Registry", false, ItemSet.AreControlAccountRegistryItemsSet());
		}

		public void TestJobRevenueJournalControlAccount()
		{
			AssertNotNull("JobRevenueJournalControlAccount", ItemSet.JobRevenueJournalControlAccount.Value);
			string defaultValue = "6245.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.JobRevenueJournalControlAccount.Name, defaultValue))
			{
				AssertEquals("JobRevenueJournalControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.JobRevenueJournalControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("JobRevenueJournalControlAccount.Value", newGuid, ItemSet.JobRevenueJournalControlAccount.Value);
		}

		public void TestARControlAccount()
		{
			AssertNotNull("ARControlAccount", ItemSet.ARControlAccount.Value);
			string defaultValue = "6210.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.ARControlAccount.Name, defaultValue))
			{
				AssertEquals("ARControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.ARControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("ARControlAccount.Value", newGuid, ItemSet.ARControlAccount.Value);
		}

		public void TestAPControlAccount()
		{
			AssertNotNull("APControlAccount", ItemSet.APControlAccount.Value);
			string defaultValue = "8210.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.APControlAccount.Name, defaultValue))
			{
				AssertEquals("APControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.APControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("APControlAccount.Value", newGuid, ItemSet.APControlAccount.Value);
		}

		public void TestARSuspenseControlAccount()
		{
			AssertNotNull("ARSuspenseControlAccount", ItemSet.ARSuspenseControlAccount.Value);
			string defaultValue = "6215.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.ARSuspenseControlAccount.Name, defaultValue))
			{
				AssertEquals("ARSuspenseControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.ARSuspenseControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("ARSuspenseControlAccount.Value", newGuid, ItemSet.ARSuspenseControlAccount.Value);
		}

		public void TestAPSuspenseControlAccount()
		{
			AssertNotNull("APSuspenseControlAccount", ItemSet.APSuspenseControlAccount.Value);
			string defaultValue = "8215.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.APSuspenseControlAccount.Name, defaultValue))
			{
				AssertEquals("APSuspenseControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.APSuspenseControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("APSuspenseControlAccount.Value", newGuid, ItemSet.APSuspenseControlAccount.Value);
		}

		public void TestGSTInputControlAccount()
		{
			AssertNotNull("GSTInputControlAccount", ItemSet.GSTInputControlAccount.Value);
			string defaultValue = "6310.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.GSTInputControlAccount.Name, defaultValue))
			{
				AssertEquals("GSTInputControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.GSTInputControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("GSTInputControlAccount.Value", newGuid, ItemSet.GSTInputControlAccount.Value);
			AssertEquals("Hint", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Input Tax amounts.
All VAT/GST tax amounts posted on Payables Invoice, Credit Note and Adjustment Note transactions post through the relevant Input Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Input Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Payments post to the Reportable Input Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Input Control Account on the transaction Post Date, and subsequently posts through to the Reportable Input Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Input Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.", ItemSet.GSTInputControlAccount.Hint);
		}

		public void TestGSTOutputControlAccount()
		{
			AssertNotNull("GSTOutputControlAccount", ItemSet.GSTOutputControlAccount.Value);
			string defaultValue = "8310.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.GSTOutputControlAccount.Name, defaultValue))
			{
				AssertEquals("GSTOutputControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.GSTOutputControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("GSTOutputControlAccount.Value", newGuid, ItemSet.GSTOutputControlAccount.Value);
			AssertEquals("Hint", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Output Tax amounts.
All VAT/GST tax amounts posted on Receivables Invoice, Credit Note and Adjustment Note transactions post through the relevant Output Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Output Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Receipt post to the Reportable Output Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Output Control Account on the transaction Post Date, and subsequently posts through to the Reportable Output Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Output Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.", ItemSet.GSTOutputControlAccount.Hint);
		}

		public void TestAccruedRevenueControlAccount()
		{
			AssertNotNull("AccruedRevenueControlAccount", ItemSet.AccruedRevenueControlAccount.Value);
			string defaultValue = "6240.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.AccruedRevenueControlAccount.Name, defaultValue))
			{
				AssertEquals("AccruedRevenueControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.AccruedRevenueControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("AccruedRevenueControlAccount.Value", newGuid, ItemSet.AccruedRevenueControlAccount.Value);
		}

		public void TestAccruedCostControlAccount()
		{
			AssertNotNull("AccruedCostControlAccount", ItemSet.AccruedCostControlAccount.Value);
			string defaultValue = "8410.10.00";
			if (HasDefaultValueFromBaseData(ItemSet.AccruedCostControlAccount.Name, defaultValue))
			{
				AssertEquals("AccruedCostControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.AccruedCostControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("AccruedCostControlAccount.Value", newGuid, ItemSet.AccruedCostControlAccount.Value);
		}

		public void TestWHTInputAccount()
		{
			AssertNotNull("WHTInputControlAccount", ItemSet.WHTInputControlAccount.Value);
			AssertEquals("Hint",
@"THIS REGISTRY IS OBSOLETE.
Please go to the 'Link Account > Tax Transaction' registry tree to define default general ledger accounts used when adding Tax Configurations against Login Company and Branch Records in Tax Configuration supported countries.
The WHT Input Control Account registry is not used by Tax Configuration and Tax Transaction features.

Please note: The WHT Input Control Account registry is only relevant in a small number of databases where Legacy WHT features were previously deployed.", ItemSet.WHTInputControlAccount.Hint);
			string defaultValue = "6330.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.WHTInputControlAccount.Name, defaultValue))
			{
				AssertEquals("WHTInputControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.WHTInputControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.WHTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("WHTInputControlAccount.Value", newGuid, ItemSet.WHTInputControlAccount.Value);
		}

		public void TestWHTOutputAccount()
		{
			AssertNotNull("WHTOutputControlAccount", ItemSet.WHTOutputControlAccount.Value);
			AssertEquals("Hint",
@"THIS REGISTRY IS OBSOLETE.
Please go to the 'Link Account > Tax Transaction' registry tree to define default general ledger accounts used when adding Tax Configurations against Login Company and Branch Records in Tax Configuration supported countries.
The WHT Output Control Account registry is not used by Tax Configuration and Tax Transaction features.

Please note: The WHT Output Control Account registry is only relevant in a small number of databases where Legacy WHT features were previously deployed.", ItemSet.WHTOutputControlAccount.Hint);
			string defaultValue = "8330.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.WHTOutputControlAccount.Name, defaultValue))
			{
				AssertEquals("WHTOutputControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.WHTOutputControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.WHTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("WHTOutputControlAccount.Value", newGuid, ItemSet.WHTOutputControlAccount.Value);
		}

		#region General Ledger Registry Items

		public void TestGeneralLedgerRegistryItemDataTypes()
		{
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.JobRevenueJournalControlAccount, typeof(JobRevenueJournalControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.ARControlAccount, typeof(ARControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.ARSuspenseControlAccount, typeof(ARSuspenseControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.APControlAccount, typeof(APControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.APSuspenseControlAccount, typeof(APSuspenseControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.GSTInputControlAccount, typeof(GSTInputControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.GSTOutputControlAccount, typeof(GSTOutputControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.WHTInputControlAccount, typeof(WHTInputControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.WHTOutputControlAccount, typeof(WHTOutputControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.AccruedRevenueControlAccount, typeof(AccruedRevenueControlAccountDataType));
			TestGeneralLedgerRegistryItemDataTypes(ItemSet.AccruedCostControlAccount, typeof(AccruedCostControlAccountDataType));
		}

		void TestGeneralLedgerRegistryItemDataTypes(IRegistryItem registryItem, Type expectedDataType)
		{
			AssertEquals(registryItem.Name + ".DataType", expectedDataType, registryItem.DataType.GetType());
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue, registryItem.Options);
		}

		#endregion

		#endregion

		#region Link Account Tests

		public void TestRealizedExchangeGainAccount()
		{
			AssertNotNull("ExchangeGainAccount", ItemSet.RealizedExchangeGainAccount.Value);
			AssertEquals("Realized Exchange Gain Account", ItemSet.RealizedExchangeGainAccount.Caption);
			AssertEquals("This link account is required for the posting of exchange gain when matching outstanding receivables and payables outstanding transactions.", ItemSet.RealizedExchangeGainAccount.Hint);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account", ItemSet.RealizedExchangeGainAccount.Category);
		}

		public void TestRealizedExchangeLossAccount()
		{
			AssertNotNull("ExchangeLossAccount", ItemSet.RealizedExchangeLossAccount.Value);
			AssertEquals("Realized Exchange Loss Account", ItemSet.RealizedExchangeLossAccount.Caption);
			AssertEquals("This link account is required for the posting of exchange loss when matching outstanding receivables and payables outstanding transactions.", ItemSet.RealizedExchangeLossAccount.Hint);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account", ItemSet.RealizedExchangeLossAccount.Category);
		}

		public void TestForeignCurrencyGLBalanceAdjustmentAccount()
		{
			var gl = RegistryFactory.Instance.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount);
			AssertNotNull("Default GL Account should exist in DB.", gl);

			TestGenericRegistryItem(ItemSet.ForeignCurrencyGLBalanceAdjustmentAccount,
				"GL_FOREIGN_CURRENCY_BALANCE_ADJUSTMENT_ACCOUNT",
				"Accounting/General Ledger Defaults/Link Account",
				"Foreign Currency GL Balance Adjustment Account",
				@"This link account will be defaulted during the creation of Foreign Currency GL Balance Adjustment via the General Ledger > Journals > New Foreign Currency Balances Adjustment menu.
You can override the GL Account as required.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory);
		}
		public void TestForeignCurrencyGLBalanceAdjustmentAccountNoDefaultValueInDb()
		{
			var gl = RegistryFactory.Instance.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount);
			gl.Delete();
			Factory.Save();

			TestGenericRegistryItem(ItemSet.ForeignCurrencyGLBalanceAdjustmentAccount,
				"GL_FOREIGN_CURRENCY_BALANCE_ADJUSTMENT_ACCOUNT",
				"Accounting/General Ledger Defaults/Link Account",
				"Foreign Currency GL Balance Adjustment Account",
				@"This link account will be defaulted during the creation of Foreign Currency GL Balance Adjustment via the General Ledger > Journals > New Foreign Currency Balances Adjustment menu.
You can override the GL Account as required.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				Guid.Empty);
		}

		public void TestCurrencyAdjustmentExchangeGainAccount()
		{
			TestGenericRegistryItem(ItemSet.CurrencyAdjustmentExchangeGainAccount,
				"GL_CURRENCY_ADJUSTMENT_EXCHANGE_GAIN_ACCOUNT",
				"Accounting/General Ledger Defaults/Link Account",
				"Currency Adjustment Exchange Gain Account",
				@"This link account is required for the posting of exchange gain related to the bank, receivables and payables foreign balances currency adjustments.

For bank currency adjustment, this can be done via the Cash Book > Cash Book Transaction > New Bank Currency Adjustment.
For receivables and payables foreign currency balances, this will be done by the automated process that can be enabled via the 'Auto Create A/R and A/P Outstanding Balance Currency Adjustments' system registry.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory);
		}

		public void TestCurrencyAdjustmentExchangeLossAccount()
		{
			TestGenericRegistryItem(ItemSet.CurrencyAdjustmentExchangeLossAccount,
				"GL_CURRENCY_ADJUSTMENT_EXCHANGE_LOSS_ACCOUNT",
				"Accounting/General Ledger Defaults/Link Account",
				"Currency Adjustment Exchange Loss Account",
				@"This link account is required for the posting of exchange loss related to the bank, receivables and payables foreign balances currency adjustments.

For bank currency adjustment, this can be done via the Cash Book > Cash Book Transaction > New Bank Currency Adjustment.
For receivables and payables foreign currency balances, this will be done by the automated process that can be enabled via the 'Auto Create A/R and A/P Outstanding Balance Currency Adjustments' system registry.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory);
		}

		public void TestARDiscountAccount()
		{
			AssertNotNull("DiscountARAccount", ItemSet.ARDiscountAccount.Value);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account", ItemSet.ARDiscountAccount.Category);
			AssertEquals("When DSC transaction is created in Receivables Ledger, this GL Account will be used.", "When DSC transaction is created in Receivables Ledger, this GL Account will be used.", ItemSet.ARDiscountAccount.Hint);
		}

		public void TestAPDiscountAccount()
		{
			AssertNotNull("DiscountAPAccount", ItemSet.APDiscountAccount.Value);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account", ItemSet.ARDiscountAccount.Category);
			AssertEquals("Description of registry", "When DSC transaction is created in Payables Ledger, this GL Account will be used.", ItemSet.APDiscountAccount.Hint);
		}

		public void TestOverpaymentsAccount()
		{
			AssertNotNull("OverpaymentsControlAccount", ItemSet.OverpaymentsAccount.Value);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account", ItemSet.OverpaymentsAccount.Category);
			AssertEquals(RegistryFindBoxFilter.BSH, (ItemSet.OverpaymentsAccount.EditorInfo as GuidFindBoxRegistryEditorInfo).FindBoxFilter);
		}

		public void TestAdvancedTurnoverTaxReturnAccount()
		{
			AssertNotNull("AdvancedTurnoverTaxReturnAccount", ItemSet.AdvancedTurnoverTaxReturnAccount.Value);
			TestGenericRegistryItem(ItemSet.AdvancedTurnoverTaxReturnAccount,
				"GL_ADVANCED_TURNOVER_TAX_RETURN_ACCOUNT",
				"Accounting/General Ledger Defaults/Link Account",
				"Advanced Turnover Tax Return Account",
				@"This link account indicates the account number for advance turnover tax returns. Advance turnover tax returns are reported (monthly) to fiscal authorities.
Book your VAT payment as Cashbook / Direct Payment on this account.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory | RegistryOptions.IsOnlyForSupport);
		}

		public void TestSpecialVATPrepaymentAccount()
		{
			AssertNotNull("SpecialVATPrepaymentAccount", ItemSet.SpecialVATPrepaymentAccount.Value);
			TestGenericRegistryItem(ItemSet.SpecialVATPrepaymentAccount,
				"GL_SPECIAL_VAT_PREPAYMENT_ACCOUNT",
				"Accounting/General Ledger Defaults/Link Account",
				"Special VAT Pre-payment Account",
				@"This link account indicates the account number for Special VAT Prepayments (e.g. UST 1/11 in Germany).
Usually once a year, these VAT prepayments are made when a Permanent Extension of Time has been authorized by Fiscal Authorities.
Book your special VAT prepayment as Cashbook / Direct Payment on this account.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory | RegistryOptions.IsOnlyForSupport);
		}

		#endregion

		public void TestPopupImportAccrualsScreenOnAPInvoice()
		{
			AssertEquals("DefaultValue", true, ItemSet.PopupImportAccrualsScreenOnAPInvoice.Value);

			ItemSet.PopupImportAccrualsScreenOnAPInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.PopupImportAccrualsScreenOnAPInvoice.Value);
		}

		public void TestIncludeChargesForAllOtherCreditors()
		{
			AssertEquals("DefaultValue", false, ItemSet.IncludeChargesForAllOtherCreditors.Value);

			ItemSet.IncludeChargesForAllOtherCreditors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.IncludeChargesForAllOtherCreditors.Value);
		}

		public void TestIncludeChargesForCreditorsWithTheSameAPSettlementGroup()
		{
			AssertEquals("DefaultValue", false, ItemSet.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.Value);

			ItemSet.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.Value);
		}

		public void TestAccrualAndWIPMustHaveCreditorAndDebtorCode()
		{
			AssertEquals("default", ZBool.False, AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value);
			AssertEquals("default", ZBool.False, AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value);

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZBool.True);
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZBool.True);

			AssertEquals("changed", ZBool.True, AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value);
			AssertEquals("changed", ZBool.True, AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value);
		}

		public void TestEnableNegativeAccrualBehaviours()
		{
			AssertEquals("default", ZBool.True, AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZBool.False);

			AssertEquals("changed", ZBool.False, AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value);
		}

		public void TestCheckDifferentSignsWhenApportionConsolCost()
		{
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.Options);

			AssertEquals("Default", ZBool.True, AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.Value);

			AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZBool.False);

			AssertEquals("Changed", ZBool.False, AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.Value);
		}

		public void TestInvoiceTotalRounding()
		{
			AssertEquals("Name", "InvoiceTotalRounding", ItemSet.InvoiceTotalRounding.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.InvoiceTotalRounding.Category);
			AssertEquals("Caption", "Invoice Total Rounding", ItemSet.InvoiceTotalRounding.Caption);
			AssertEquals("Hint", @"This feature is useful for companies that requires invoice total to be rounded to a specific currency units for specific currencies.
This feature is only relevant invoice currencies with 100 minor units. (e.g. 100 cents, 100 pence, 100 franc, etc.)

By default, this feature is not enabled.
When enabled, an additional non-job 'rounding' transaction line will be added to each invoice using the 'Invoice Total Rounding Charge Code', where applicable.
The Tax ID of additional line will always be set to NOTREPORT regardless of the charge code setup.

The rounding will be done on the gross up total of the invoice (i.e. VAT Inclusive charges).
You can configure the system to always round up, always round down or round based on midpoint.

Note:
The 'round based on midpoint' option can only be used if the round to currency unit is set to 1.00.
The 'round based on midpoint' option will round down if the invoice total's minor unit less than 50. E.g. 1,081.23 will be rounded to 1,081.00.
The 'round based on midpoint' option will round up if the invoice total's minor unit is 50 or more. E.g. 1,081.67 will be rounded to 1,082.00.", ItemSet.InvoiceTotalRounding.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.InvoiceTotalRounding.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.InvoiceTotalRounding.Options);

			var collection = new InvoiceTotalRoundingCollection();
			var config = collection.AddNew();
			config.Currency = Core.Constants.CurrencyCodes.China;
			config.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundDown;
			config.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.FiftyMinorUnits;

			var newCollection = new InvoiceTotalRoundingCollection();

			ItemSet.InvoiceTotalRounding.SetValue(CompanyPK, Guid.Empty, Guid.Empty, collection);

			var regItem = AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.Inner;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, newCollection, collection);
			var logReference = AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.OnBuildLogReference(args);
			AssertMultilineASCIIEquals("New Currency Added 'CNY' with Rounding Option 'ARD' and Round To Currency Unit '0.50'.", logReference);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection, newCollection);
			logReference = AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.OnBuildLogReference(args);
			AssertMultilineASCIIEquals("Existing Currency 'CNY', Rounding Option 'ARD', Round To Currency Unit '0.50' deleted.", logReference);

			var config1 = newCollection.AddNew();
			config1.Currency = Core.Constants.CurrencyCodes.China;
			config1.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundUp;
			config1.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.TenMinorUnits;

			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection, newCollection);
			logReference = AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.OnBuildLogReference(args);
			AssertMultilineASCIIEquals("Currency 'CNY', Rounding Option change from 'ARD' to 'ARU', Round To Currency Unit change from '0.50' to '0.10'.", logReference);
		}

		public void TestInvoiceTotalRoundingChargeCode()
		{
			AssertEquals("Name", "InvoiceTotalRoundingChargeCode", ItemSet.InvoiceTotalRoundingChargeCode.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.InvoiceTotalRoundingChargeCode.Category);
			AssertEquals("Caption", "Invoice Total Rounding Charge Code", ItemSet.InvoiceTotalRoundingChargeCode.Caption);
			AssertEquals("Hint", @"This charge code will be used for the creation of the rounding transaction line with reference to the 'Invoice Total Rounding' configuration.
This charge code must have a charge type of 'REV' or 'NON' only.
The Tax ID of this rounding transaction line will always be set to NOTREPORT regardless of the charge code setup.", ItemSet.InvoiceTotalRoundingChargeCode.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.InvoiceTotalRoundingChargeCode.Storage);
			AssertEquals("DefaultValue", ZGuid.Empty, ItemSet.InvoiceTotalRoundingChargeCode.DefaultValue);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.InvoiceTotalRoundingChargeCode.Options);

			var chargeCodePK = Guid.NewGuid();
			ItemSet.InvoiceTotalRoundingChargeCode.SetValue(CompanyPK, Guid.Empty, Guid.Empty, chargeCodePK);
			AssertEquals(chargeCodePK, ItemSet.InvoiceTotalRoundingChargeCode.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));

			var testCreator = new TestObjectCreator(Factory);
			var oldValue = testCreator.CC1.PK.ToGuid();
			var newValue = testCreator.CC2.PK.ToGuid();
			var regItem = AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.Inner;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.OnBuildLogReference(args);
			AssertEquals($"Charge Code changed from [ZZCC1] to [ZZCC2].", logReference);
		}

		public void TesteNettLastPaymentDateDefaultIsNow()
		{
			AssertEquals("Default Value should be today", ZDateTime.Now.Date, AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.Value.Date);
			ZDateTime newValue = ZDateTime.Now.Date.AddDays(5).ToDateTime();
			AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue.ToDateTime());
			AssertEquals("Should be 5 days from now", newValue, AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.Value);
		}

		public void TesteNettLastInvoiceDateDefaultIsNow()
		{
			AssertEquals("Default Value should be today", ZDateTime.Now.Date, AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value.Date);
			ZDateTime newValue = ZDateTime.Now.Date.AddDays(5).ToDateTime();
			AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue.ToDateTime());
			AssertEquals("Should be 5 days from now", newValue, AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value);
		}

		public void TestJobBranchDefaultOrderRule()
		{
			string expectedHint =
				"Override these values to set the order of the default branch on creation of a Job." + System.Environment.NewLine +
				"The values can be between 0 and 4." + System.Environment.NewLine +
				"A value of 0 means the rule will not be used." + System.Environment.NewLine +
				"There can be multiple rules with a value of 0. At least one rule must have a value greater than 0." + System.Environment.NewLine +
				"Any value greater than 1 must not be duplicated, i.e. 0, 1, 1, 2 is not valid." + System.Environment.NewLine +
				"There must not be gaps between the sequence of numbers for the values, i.e. 0, 1, 2, 3 is valid but 0, 1, 2, 4 is not valid.";

			TestGenericRegistryItem(ItemSet.JobBranchDefaultOrderRule, "JobBranchDefaultOrderRule", "Accounting/Job Invoicing", "Job Creation - Default Branch Rule", expectedHint, RegistryStorageFlags.Company | RegistryStorageFlags.Branch);

			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();

			rule.DefaultToBlank = 1;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 2;
			rule.DefaultToBranchOfOrganisation = 3;
			rule.DefaultToLoginUserDefault = 4;

			ItemSet.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, rule);

			IJobBranchDefaultOrderRule value = ItemSet.JobBranchDefaultOrderRule.Value;

			AssertEquals("DefaultToBlank", (short)1, value.DefaultToBlank);
			AssertEquals("DefaultToBranchRelatedToPortOrWarehouseBranch", (short)2, value.DefaultToBranchRelatedToPortOrWarehouseBranch);
			AssertEquals("DefaultToBranchOfOrganisation", (short)3, value.DefaultToBranchOfOrganisation);
			AssertEquals("DefaultToLoginUserDefault", (short)4, value.DefaultToLoginUserDefault);
		}

		public void TestRoundingChargeCode()
		{
			AssertEquals("RoundingChargeCode.DefaultChargeCode", "", ItemSet.RoundingChargeCode.DefaultChargeCode);

			Guid newGuid1 = Guid.NewGuid();
			ItemSet.RoundingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid1);
			AssertEquals("RoundingChargeCode.Value", newGuid1, ItemSet.RoundingChargeCode.Value);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.ProfitShareChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
		}

		public void TestIncludeInRoundingChargeCode()
		{
			AssertEquals("IncludeInRoundingChargeCode.DefaultChargeCode", "", ItemSet.IncludeInRoundingChargeCode.DefaultChargeCode);

			Guid newGuid1 = Guid.NewGuid();
			ItemSet.IncludeInRoundingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid1);
			AssertEquals("IncludeInRoundingChargeCode.Value", newGuid1, ItemSet.IncludeInRoundingChargeCode.Value);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.ProfitShareChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
		}

		public void TestProfitShareChargeCode()
		{
			Guid newGuid1 = Guid.NewGuid();
			Guid newGuid2 = Guid.NewGuid();

			AssertEquals("ProfitShareChargeCode.DefaultChargeCode", "PS", ItemSet.ProfitShareChargeCode.DefaultChargeCode);

			ItemSet.ProfitShareChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid1);
			AssertEquals("ProfitShareChargeCode.Value", newGuid1, ItemSet.ProfitShareChargeCode.Value);

			ItemSet.ProfitShareChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGuid2);
			AssertEquals("ProfitShareChargeCode.Value", newGuid2, ItemSet.ProfitShareChargeCode.Value);

			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.ProfitShareChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
		}

		public void TestProfitShareChargeCodesPerParty()
		{
			var defaultItemValues = ItemSet.ProfitShareChargeCodesPerParty.DefaultValue;

			ZGuid profitShareChargeCode = ItemSet.ProfitShareChargeCode.Value;
			RegistryFactory.Instance.TryGetValueFromCacheOnly("ChargeCodeWithTypeCollection.GetPartyTypes", out CodeDescriptionPairList partyTypes);
			AssertEquals("ProfitShareChargeCodesPerParty grid should have number of items:", partyTypes.Count, defaultItemValues.Count);

			foreach (CodeDescriptionPair pair in partyTypes)
			{
				AssertEquals("ProfitShareChargeCodesPerParty.Default", profitShareChargeCode, defaultItemValues.GetCode(pair.Code));
			}
		}

		public void TestHotChequeRequiredFields()
		{
			TestRegistryItem(ItemSet.AccountingRequiredChequeDate, "AccountingRequiredChequeDate", "Accounting/Payable Defaults/Default Settings/Hot Check/Hot Check Required Fields", "Required Check Date", "This registry setting controls whether the Check Date is required on creating a Hot Check.\r\nChoose 'Yes' (default) to make it mandatory to enter the Check Date.\r\nChoose 'No' to make it optional.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
			TestRegistryItem(ItemSet.AccountingRequiredJobNumber, "AccountingRequiredJobNumber", "Accounting/Payable Defaults/Default Settings/Hot Check/Hot Check Required Fields", "Required Job Number", "This registry setting controls whether Job Number is required on creating a Hot Check.\r\nChoose 'Yes' to make it mandatory to enter the Job Number.\r\nChoose 'No' (default) to make it optional.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
			TestRegistryItem(ItemSet.AccountingRequiredMasterBillNo, "AccountingRequiredMasterBillNo", "Accounting/Payable Defaults/Default Settings/Hot Check/Hot Check Required Fields", "Required Master Bill No", "This registry setting controls whether the Master Bill Number is required on creating a Hot Check.\r\nChoose 'Yes' to make it mandatory to enter the Master Bill Number.\r\nChoose 'No' (default) to make it optional.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
			TestRegistryItem(ItemSet.AccountingRequiredHouseBillNo, "AccountingRequiredHouseBillNo", "Accounting/Payable Defaults/Default Settings/Hot Check/Hot Check Required Fields", "Required House Bill No", "This registry setting controls whether the House Bill Number is required on creating a Hot Check.\r\nChoose 'Yes' to make it mandatory to enter the House Bill Number.\r\nChoose 'No' (default) to make it optional.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
			TestRegistryItem(ItemSet.AccountingRequiredDescription, "AccountingRequiredDescription", "Accounting/Payable Defaults/Default Settings/Hot Check/Hot Check Required Fields", "Required Description", "This registry setting controls whether the Description is required on creating a Hot Check.\r\nChoose 'Yes' (default) to make it mandatory to enter the Description.\r\nChoose 'No' to make it optional.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
			TestRegistryItem(ItemSet.AccountingRequiredStaff, "AccountingRequiredStaff", "Accounting/Payable Defaults/Default Settings/Hot Check/Hot Check Required Fields", "Required Staff", "This registry setting controls whether the Staff is required on creating a Hot Check.\r\nChoose 'Yes' (default) to make it mandatory to enter the Staff code.\r\nChoose 'No' to make it optional.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
			TestRegistryItem(ItemSet.AccountingAllowUserToEnterMaximumAmount, "AccountingAllowUserToEnterMaximumAmount", "Accounting/Payable Defaults/Default Settings/Hot Check", "Allow User To Enter Maximum Amount", "This registry item controls the entering of Hot Check Amount.\r\nSelect 'Yes' to allow user to enter 'Actual Amount' or 'Maximum Amount'.\r\nSelect 'No' to allow user to enter 'Actual Amount' only.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestMatchStatus()
		{
			var matchStatusItem = ItemSet.MatchStatus;
			TestRegistryItemWithDefaultCode(matchStatusItem, "MatchStatus", "Accounting", "Match Status", @"The Match Statuses listed here are used on Accounts Receivables and Account Payables Transaction.
They can be used optionally to classify the match status of each transaction.
Further, a reason code can be optionally recorded to provide the background of this classification.
The Match Status Reason Codes are configurable via the Accounting > Match Status Reason Codes system registry.", RegistryStorageFlags.System | RegistryStorageFlags.Company, 3, 1);
			Assert(((ICodeDescriptionPairListProvider)matchStatusItem).CodeDescriptionPairList.ContainsCode("UAC"));
			AssertEquals(RegistryOptions.Default, matchStatusItem.Options);
		}

		public void TestMatchStatusReason()
		{
			var matchStatusReasonItem = ItemSet.MatchStatusReason;
			TestRegistryItemWithDefaultCode(matchStatusReasonItem, "MatchStatusReason", "Accounting", "Match Status Reason", @"The Match Status Reason Codes listed here are used on Accounts Receivables and Account Payables Transaction to support a matching status.
They can be used to provide the background of the match status classification.", RegistryStorageFlags.System | RegistryStorageFlags.Company, 3, 1);
			Assert(((ICodeDescriptionPairListProvider)matchStatusReasonItem).CodeDescriptionPairList.ContainsCode("ADV"));
			AssertEquals(RegistryOptions.Default, matchStatusReasonItem.Options);
		}

		public void TestQueryClaimType()
		{
			TestRegistryItemWithDefaultCode(ItemSet.QueryClaimType, "AccountingQueryClaimType", "Accounting/Claims and Queries", "Claim Type", @"The Type Codes listed here are used on Accounts Payable and Accounts Receivable Claims and Queries.
They are used operationally to assist in the classification, evaluation and review of each claim recorded in a company.", RegistryStorageFlags.System | RegistryStorageFlags.Company, 3, 7);
			AssertEquals(Accounting.QueryClaimTypeCodeList.Codes.QCType3, ItemSet.QueryClaimType.DefaultValue.DefaultCode);
		}

		public void TestClaimStatus()
		{
			TestRegistryItemWithDefaultCode(ItemSet.ClaimStatus, "AccountingClaimStatus", "Accounting/Claims and Queries", "Claim Status", @"The Status Codes listed here are used on Accounts Payable and Accounts Receivable Claims and Queries.
They are used operationally to identify the current status of each claim recorded in a company.", RegistryStorageFlags.System | RegistryStorageFlags.Company, 3, 9);
			AssertEquals(QueryClaimStatusCodeList.Codes.QCStatus1Open, ItemSet.ClaimStatus.DefaultValue.DefaultCode);
		}

		public void TestClaimReason()
		{
			TestRegistryItemWithDefaultCode(ItemSet.ClaimReason, "AccountingClaimReason", "Accounting/Claims and Queries", "Claim Reason", @"The Reason Codes listed here are used on Accounts Payable and Accounts Receivable Claims and Queries.
They are used operationally to assist in the classification, evaluation and review of each claim recorded in a company.", RegistryStorageFlags.System | RegistryStorageFlags.Company, 3, 4);
			AssertEquals(Accounting.QueryClaimReasonCodeList.Codes.QCReason1, ItemSet.ClaimReason.DefaultValue.DefaultCode);
		}

		public void TestCollectionCallFollowUpDays()
		{
			TestRegistryItem(
				ItemSet.CollectionCallFollowUpDays,
				"CollectionCallFollowUpDays",
				"Accounting/Collection Calls",
				"Collection Call Follow Up Days",
				"The number of days between when a Collection Call is created and its 'Follow Up' Date.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				7);
		}

		public void TestCollectionCallCreateFollowUpAppointments()
		{
			TestRegistryItem(
				ItemSet.CollectionCallCreateFollowUpAppointments,
				"CollectionCallCreateFollowUpAppointments",
				"Accounting/Collection Calls",
				"Create follow up appointments",
				"This controls whether a follow up appointment will be created when you nominate a 'Follow up' date on a Collection Call",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestReceivableAuthorizationModeAndSettings()
		{
			TestGenericRegistryItem(
				ItemSet.ReceivableAuthorizationModeAndSettings,
				"ReceivableAuthorizationModeAndSettings",
				"Accounting/Receivable Defaults/Default Settings/Credit Note And Invoice Reversal Authorization Settings",
				"Credit / Adjustment Note Authorization Settings",
				@"Use this registry to control the posting of Receivables Credit Notes and Negative Adjustment Notes by setting 'Authorization' thresholds.
NOTE: When no threshold values are set, authorization of transactions at posting is not required.
Use this registry to define the amount thresholds at which AR Credit Notes and AR Negative Adjustment Notes require authorization before they can be posted. There are six levels of Authorization.
The authorization requirement 'None' allows any user with Credit Note and Adjustment Note posting rights to post transactions up to the threshold amount without need of an authorizing user.
The authorization requirements '1st Level' through to '6th Level' restricts posting or authorization of posting to users flagged as having the corresponding Approval security rights.

By default, a single authorized user needs to approve the posting of a credit note.
Set 'Authorization Mode' to TWO to enforce that two authorized users must review each credit note posting. Both users must have the specified approval level in order to authorize the posting.
Set 'Authorization Mode' to SEQ to enforce that multiple authorized users must review each credit note posting. The credit note must be reviewed by all approval levels, starting from Level 1 approver, then Level 2 and so on. Finally, the user with the specified 'Authorization Requirement' can fully approve the posting. Note that lower level authorizing users are able to reject the credit note posting. However, in all users up to the specified Approval Level must approve the credit note, before it can be posted.

NOTE: When a single approval is sufficient in order to post the credit note, if the creating user doesn't have sufficient approval level, then the 'Security Override Login' authorization is displayed. Another user with the required approval level can approve the credit note posting by supplying their username and password. Otherwise, and in cases where multiple approvers must review the credit note, the creating user can queue an Approval Request. Authorized users can review and approve the requests in the Credit Note Approval module.",
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
		}

		public void TestReceivableReversalAuthorizationModeAndSettings()
		{
			TestGenericRegistryItem(
				ItemSet.ReceivableReversalAuthorizationModeAndSettings,
				"ReceivableReversalAuthorizationModeAndSettings",
				"Accounting/Receivable Defaults/Default Settings/Credit Note And Invoice Reversal Authorization Settings",
				"Invoice / Adjustment Note Reversal Authorization Settings",
				@"Use this registry to control the reversal of Receivables Invoices and Positive Adjustment Notes by setting 'Authorization' thresholds.
NOTE: When no threshold values are set, authorization of transactions at posting is not required.
Use this registry to define the amount thresholds at which AR Credit Notes and AR Negative Adjustment Notes require authorization before they can be posted. There are six levels of Authorization.
The authorization requirement 'None' allows any user with Credit Note and Adjustment Note posting rights to post transactions up to the threshold amount without need of an authorizing user.
The authorization requirements '1st Level' through to '6th Level' restricts posting or authorization of posting to users flagged as having the corresponding Approval security rights.

By default, a single authorized user needs to approve the posting of a credit note.
Set 'Authorization Mode' to TWO to enforce that two authorized users must review each credit note posting. Both users must have the specified approval level in order to authorize the posting.
Set 'Authorization Mode' to SEQ to enforce that multiple authorized users must review each credit note posting. The credit note must be reviewed by all approval levels, starting from Level 1 approver, then Level 2 and so on. Finally, the user with the specified 'Authorization Requirement' can fully approve the posting. Note that lower level authorizing users are able to reject the credit note posting. However, in all users up to the specified Approval Level must approve the credit note, before it can be posted.

NOTE: When a single approval is sufficient in order to post the credit note, if the creating user doesn't have sufficient approval level, then the 'Security Override Login' authorization is displayed. Another user with the required approval level can approve the credit note posting by supplying their username and password. Otherwise, and in cases where multiple approvers must review the credit note, the creating user can queue an Approval Request. Authorized users can review and approve the requests in the Credit Note Approval module.",
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
		}

		public void TestPayableAuthorizationSettings()
		{
			PaymentAuthorisationSettingsCollection collection = new PaymentAuthorisationSettingsCollection();

			PaymentAuthorisationSettings upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = upToPaymentAuthorisationSettings.RangeList[0].Code;
			upToPaymentAuthorisationSettings.Amount = 100;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = upToPaymentAuthorisationSettings.AuthorisationRequirementList[0].Code;

			PaymentAuthorisationSettings abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = abovePaymentAuthorisationSettings.RangeList[1].Code;
			abovePaymentAuthorisationSettings.Amount = 100;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = abovePaymentAuthorisationSettings.AuthorisationRequirementList[1].Code;

			ItemSet.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			PaymentAuthorisationSettingsCollection value = ItemSet.PayableAuthorizationSettings.Value;

			AssertEquals("Value.Count", 2, value.Count);
			AssertEquals("Value[0].Range", upToPaymentAuthorisationSettings.Range, value[0].Range);
			AssertEquals("Value[0].Amount", upToPaymentAuthorisationSettings.Amount, value[0].Amount);
			AssertEquals("Value[0].AuthorisationRequirement", upToPaymentAuthorisationSettings.AuthorisationRequirement, value[0].AuthorisationRequirement);

			AssertEquals("Value[0].Range", abovePaymentAuthorisationSettings.Range, value[1].Range);
			AssertEquals("Value[0].Amount", abovePaymentAuthorisationSettings.Amount, value[1].Amount);
			AssertEquals("Value[0].AuthorisationRequirement", abovePaymentAuthorisationSettings.AuthorisationRequirement, value[1].AuthorisationRequirement);

			AssertEquals("Description of registry", "This registry item allows you to specify the authorization required to fully approve an unapproved payment.\r\nYou can set up the authorization required based on the local value of the payment. When no options are set, no approval is required regardless of payment value.\r\nThe system will allow you to specify required authorization for different ranges. You must specify at least one \"Up to\" line and only one \"Above\" line.", ItemSet.PayableAuthorizationSettings.Hint);
		}

		public void TestPaymentApprovalsNotifyGroup()
		{
			TestRegistryItem(
				ItemSet.PaymentApprovalsNotifyGroup,
				"PaymentApprovalsNotifyGroup",
				Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing,
				"Payment Approvals Notify Group",
				@"This registry allows you to nominate a user group who will receive notifications about outstanding AR and AP payment approval requests.

A regular email will summarize the payment approval requests that are still in AWA - Awaiting Approval status. By default, the email will be sent out daily. To change the frequency of the email, please navigate to Maintain > System > Service Tasks and locate the following task:
UPA - Payment Approvals Notification Email. You can then edit the Recurrence for the scheduled email.

Payment Approval requests can be approved or rejected in the AR and AP Payment Processing modules.",
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestExchangeRateTolerance()
		{
			AssertEquals("name", "ExchangeRateTolerance", ItemSet.ExchangeRateTolerance.Name);
			AssertEquals("category", Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing, ItemSet.ExchangeRateTolerance.Category);
			AssertEquals("name", "Exchange Rate Tolerance", ItemSet.ExchangeRateTolerance.Caption);
			AssertEquals("hint", @"This registry controls the ""Exchange Rate Tolerance"" setup of foreign currencies in Payment Approvals.

In a payment approval, when a new exchange rate is entered - it is considered to be;
i) ""Un-favorable"" when for the same overseas ""Payment Amount"" - the ""Local Amount"" to be paid increases.
ii) ""Favorable"" when for the same overseas ""Payment Amount"" - the ""Local Amount"" to be paid decreases.

When a new un-favorable exchange rate is entered, ""Exchange Rate Tolerance"" represents the maximum allowed percentage increase to the ""Local Amount"" paid (for the same foreign ""Payment Amount"") 
on the payment approval before its approval status is reset for re-approval.

Exchange Rate Tolerances can only be set as a positive value between 0-100

By default, the registry applies a ""0%"" Exchange Rate Tolerance to ALL currencies – but currency specific tolerance setups will take precedence",
				ItemSet.ExchangeRateTolerance.Hint);
			AssertEquals("storage", RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.ExchangeRateTolerance.Storage);

			AssertEquals("DefaultValue", 1, ItemSet.ExchangeRateTolerance.DefaultValue.ExchangeRateToleranceCollection.Count);
			AssertEquals("DefaultValue", ExchangeRateToleranceLookups.AllCurrencyCode, ItemSet.ExchangeRateTolerance.DefaultValue.ExchangeRateToleranceCollection[0].Currency);
			AssertEquals("DefaultValue", 0M, ItemSet.ExchangeRateTolerance.DefaultValue.ExchangeRateToleranceCollection[0].ExchangeRateTolerancePercentage);

			var newConfig = new ExchangeRateToleranceConfiguration();
			var newMethod = new ExchangeRateTolerance
			{
				Currency = "AUD",
				ExchangeRateTolerancePercentage = 10
			};
			newConfig.ExchangeRateToleranceCollection.RemoveAll();
			newConfig.ExchangeRateToleranceCollection.Add(newMethod);
			ItemSet.ExchangeRateTolerance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfig);

			AssertEquals("Value", 2, ItemSet.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection.Count);
			AssertEquals("Value", "AUD", ItemSet.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection[1].Currency);
			AssertEquals("Value", 10m, ItemSet.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection[1].ExchangeRateTolerancePercentage);
		}

		public void TestReportOrder()
		{
			ReportOrderCollection collection = new ReportOrderCollection();

			ReportOrder reportOrder = collection.AddNew();
			reportOrder.Language = reportOrder.LanguageList[0].Code;
			reportOrder.AccountsOrderBeginsWith = reportOrder.AccountOrderTypeList[0].Code;

			BusinessObject accGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			accGLAccountDescriptor1[AccGLAccountDescriptorSchema.AJ_LocalAccountNumber] = new ZString("1000.00.00");
			accGLAccountDescriptor1[AccGLAccountDescriptorSchema.Constants.AJ_ReportCategory] = new ZString(AccountTypeComboBoxConstants.Header);
			accGLAccountDescriptor1[AccGLAccountDescriptorSchema.Constants.AJ_Language] = new ZString(reportOrder.LanguageList[0].Code);
			BusinessObject accGLAccountDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			accGLAccountDescriptor2[AccGLAccountDescriptorSchema.AJ_LocalAccountNumber] = new ZString("2000.00.00");
			accGLAccountDescriptor2[AccGLAccountDescriptorSchema.Constants.AJ_ReportCategory] = new ZString(AccountTypeComboBoxConstants.Header);
			accGLAccountDescriptor2[AccGLAccountDescriptorSchema.Constants.AJ_Language] = new ZString(reportOrder.LanguageList[0].Code);

			Factory.Save();

			reportOrder.GLAccountSecondReportStartsFrom = accGLAccountDescriptor2.PK;

			ItemSet.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ReportOrderCollection value = ItemSet.ReportOrder.Value;

			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].Language", reportOrder.Language, value[0].Language);
			AssertEquals("Value[0].AccountsOrderBeginsWith", reportOrder.AccountsOrderBeginsWith, value[0].AccountsOrderBeginsWith);
			AssertEquals("Value[0].GLAccountSecondReportStartsFrom", reportOrder.GLAccountSecondReportStartsFrom, value[0].GLAccountSecondReportStartsFrom);
		}

		public void TestCalculateTaxAtHeaderLevel()
		{
			BusinessObject taiwanCompany = Factory.NewWithValidTestData<GlbCompany>();
			taiwanCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Taiwan;

			BusinessObject australianCompany = Factory.NewWithValidTestData<GlbCompany>();
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;

			Factory.Save();
			TestRegistryItem(
				ItemSet.CalculateTaxAtHeaderLevel,
				"CalculateTaxAtHeaderLevel",
				"Accounting",
				"Calculate Tax at Header Level",
				@"When this Registry is set to 'No', the VAT/GST amount of each charge line is calculated and rounded line by line. The VAT/GST amount of each charge line is calculated individually without reference to any other charge line in the same transaction.

Alternatively, when this registry is set to YES, charge lines with same VAT / GST rates in the one transaction will be ""grouped"" and if necessary, the tax amount of the largest line will be adjusted to ensure the amount recorded is correct 'at header level'.The system will make this adjustment so that the total VAT/ GST for each rate of VAT within an Invoice will be calculated based on a rounded subtotal of all taxable lines attracting the same rate of VAT / GST within an invoice.",
				RegistryStorageFlags.Company,
				false);

			AssertEquals("Default Value", false, ItemSet.CalculateTaxAtHeaderLevel.DefaultValue);

			ItemSet.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.CalculateTaxAtHeaderLevel.Value);

			ItemSet.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.CalculateTaxAtHeaderLevel.Value);
		}

		public void TestDisplayRecipientTaxIDDefaultValueForBrexit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var gbCountry = RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), Constants.CountryCodes.UnitedKingdom);
				gbCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
				gbCountry.Factory.Save();
				Assert("Before Brexit", ItemSet.DisplayRecipientTaxID.DefaultValue);

				ResetRegistryCache();

				gbCountry.RN_EconomicGrouping = "";
				gbCountry.Factory.Save();
				Assert("After Brexit", ItemSet.DisplayRecipientTaxID.DefaultValue);
			}
		}

		public void TestDisplayRecipientTaxID()
		{
			TestRegistryItem(
				ItemSet.DisplayRecipientTaxID,
				"DisplayRecipientTaxID",
				"Accounting/Receivable Defaults/Form Configurations/Invoice",
				"Display Recipient Tax ID on AR Invoice (CargoWiseOne Support Only)",
				@"This is a CargoWiseOne Support Only Registry.
It is used when the Login Company needs to print the Main Tax Registration Number of the Receivables Organization in the AR Invoice AND the CargoWiseOne Invoice is NOT yet configured to support its display.
When the Registry is set to YES, the Recipient Main Tax Registration Number will print in the AR Invoice.
NOTE: When you override this registry from NO to YES, the Recipient Tax ID Heading will use the Login Country’s name of tax. I.e. ‘CLIENT VAT’, or ‘CLIENT GST’ etc.
Please see the ‘Recipient Tax ID Heading on AR Invoices’ registry if you wish to modify the default heading displayed.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);

			AssertEquals("Default Value", false, ItemSet.DisplayRecipientTaxID.DefaultValue);
			ItemSet.DisplayRecipientTaxID.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DisplayRecipientTaxID.Value);
			ItemSet.DisplayRecipientTaxID.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.DisplayRecipientTaxID.Value);

			RefCountry[] countries = Factory.Load<RefCountry>(new ZQuery());
			foreach (RefCountry country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.RN_Code))
				{
					bool result = false;

					switch (country.RN_Code)
					{
						case Constants.CountryCodes.Netherlands:
						case Constants.CountryCodes.Norway:
						case Constants.CountryCodes.Taiwan:
						case Constants.CountryCodes.SouthAfrica:
						case Constants.CountryCodes.Philippines:
						case Constants.CountryCodes.Peru:
						case Constants.CountryCodes.Turkey:
						case Constants.CountryCodes.SriLanka:
						case Constants.CountryCodes.Ethiopia:
						case Constants.CountryCodes.Maldives:
						case Constants.CountryCodes.China:
						case Constants.CountryCodes.Indonesia:
						case Constants.CountryCodes.Tonga:
						case Constants.CountryCodes.Uganda:
						case Constants.CountryCodes.Colombia:
						case Constants.CountryCodes.Guatemala:
						case Constants.CountryCodes.Venezuela:
						case Constants.CountryCodes.Ecuador:
						case Constants.CountryCodes.ElSalvador:
						case Constants.CountryCodes.PuertoRico:
						case Constants.CountryCodes.Paraguay:
						case Constants.CountryCodes.Uruguay:
						case Constants.CountryCodes.Azerbaijan:
						case Constants.CountryCodes.Bolivia:
						case Constants.CountryCodes.FrenchPolynesia:
						case Constants.CountryCodes.Honduras:
						case Constants.CountryCodes.Kenya:
						case Constants.CountryCodes.Mauritius:
						case Constants.CountryCodes.Nicaragua:
						case Constants.CountryCodes.Mongolia:
						case Constants.CountryCodes.Botswana:
						case Constants.CountryCodes.Kazakhstan:
						case Constants.CountryCodes.Tanzania:
						case Constants.CountryCodes.Mali:
						case Constants.CountryCodes.Jordan:
						case Constants.CountryCodes.Zimbabwe:
						case Constants.CountryCodes.KoreaSouth:
						case Constants.CountryCodes.Thailand:
						case Constants.CountryCodes.VietNam:
						case Constants.CountryCodes.Senegal:
						case Constants.CountryCodes.CoteDivoire:
						case Constants.CountryCodes.Cameroon:
						case Constants.CountryCodes.Mozambique:
						case Constants.CountryCodes.EquatorialGuinea:
						case Constants.CountryCodes.DominicanRepublic:
						case Constants.CountryCodes.Yemen:
						case Constants.CountryCodes.Algeria:
						case Constants.CountryCodes.Malawi:
						case Constants.CountryCodes.Russia:
						case Constants.CountryCodes.Lebanon:
						case Constants.CountryCodes.Niger:
						case Constants.CountryCodes.CostaRica:
						case Constants.CountryCodes.Ghana:
						case Constants.CountryCodes.Belarus:
						case Constants.CountryCodes.SierraLeone:
						case Constants.CountryCodes.Spain:
						case Constants.CountryCodes.Cambodia:
						case Constants.CountryCodes.Madagascar:
						case Constants.CountryCodes.Kiribati:
						case Constants.CountryCodes.BurkinaFaso:
						case Constants.CountryCodes.Nepal:
						case Constants.CountryCodes.Curacao:
						case Constants.CountryCodes.Jamaica:
						case Constants.CountryCodes.TrinidadAndTobago:
						case Constants.CountryCodes.Togo:
						case Constants.CountryCodes.Macedonia:
						case Constants.CountryCodes.Croatia:
						case Constants.CountryCodes.Barbados:
						case Constants.CountryCodes.Rwanda:
						case Constants.CountryCodes.Kosovo:
						case Constants.CountryCodes.India:
						case Constants.CountryCodes.Iran:
						case Constants.CountryCodes.UnitedArabEmirates:
						case Constants.CountryCodes.Bahrain:
						case Constants.CountryCodes.Kuwait:
						case Constants.CountryCodes.Oman:
						case Constants.CountryCodes.Qatar:
						case Constants.CountryCodes.SaudiArabia:
						case Constants.CountryCodes.Georgia:
						case Constants.CountryCodes.NewCaledonia:
						case Constants.CountryCodes.NewZealand:
						case Constants.CountryCodes.Chad:
						case Constants.CountryCodes.LaoPeoplesDemocraticRepublic:
						case Constants.CountryCodes.Morocco:
						case Constants.CountryCodes.BosniaAndHerzegovina:
						case Constants.CountryCodes.Angola:
						case Constants.CountryCodes.Bangladesh:
						case Constants.CountryCodes.Gabon:
						case Constants.CountryCodes.PalestinianTerritory:
						case Constants.CountryCodes.Guyana:
						case Constants.CountryCodes.Mauritania:
						case Constants.CountryCodes.Sudan:
						case Constants.CountryCodes.Moldova:
						case Constants.CountryCodes.Pakistan:
						case Constants.CountryCodes.Lesotho:
						case Constants.CountryCodes.Uzbekistan:
						case Constants.CountryCodes.CookIslands:
						case Constants.CountryCodes.Vanuatu:
						case Constants.CountryCodes.Guinea:
						case Constants.CountryCodes.Albania:
						case Constants.CountryCodes.Bahamas:
						case Constants.CountryCodes.Montenegro:
						case Constants.CountryCodes.Burundi:
						case Constants.CountryCodes.Djibouti:
						case Constants.CountryCodes.SaintMartin:
						case Constants.CountryCodes.SaintKittsAndNevis:
						case Constants.CountryCodes.Serbia:
						case Constants.CountryCodes.Gambia:
						case Constants.CountryCodes.Kyrgyzstan:
						case Constants.CountryCodes.Turkmenistan:
						case Constants.CountryCodes.BonaireSintEustatiusAndSaba:
						case Constants.CountryCodes.Swaziland:
						case Constants.CountryCodes.CapeVerde:
						case Constants.CountryCodes.Suriname:
						case Constants.CountryCodes.Belize:
						case Constants.CountryCodes.Tuvalu:
						case Constants.CountryCodes.FaeroeIslands:
							result = true;
							break;
					}

					if (country.IsPartOfEuropeanUnion)
					{
						result = true;
					}

					AssertEquals($"Default value for country {country.Code}", result, ItemSet.DisplayRecipientTaxID.DefaultValue);
				}
			}
		}

		public void TestDisplayRecipientTaxID_GC_RN_NKCountryCodeIsEmpty()
		{
			var company = Factory.Load<GlbCompany>(new ZQuery()).First();
			company.GC_RN_NKCountryCode = "";
			Factory.Save();

			TestRegistryItem(
				ItemSet.DisplayRecipientTaxID,
				"DisplayRecipientTaxID",
				"Accounting/Receivable Defaults/Form Configurations/Invoice",
				"Display Recipient Tax ID on AR Invoice (CargoWiseOne Support Only)",
				@"This is a CargoWiseOne Support Only Registry.
It is used when the Login Company needs to print the Main Tax Registration Number of the Receivables Organization in the AR Invoice AND the CargoWiseOne Invoice is NOT yet configured to support its display.
When the Registry is set to YES, the Recipient Main Tax Registration Number will print in the AR Invoice.
NOTE: When you override this registry from NO to YES, the Recipient Tax ID Heading will use the Login Country’s name of tax. I.e. ‘CLIENT VAT’, or ‘CLIENT GST’ etc.
Please see the ‘Recipient Tax ID Heading on AR Invoices’ registry if you wish to modify the default heading displayed.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestDisplayRecipientTaxIDHeading()
		{
			TestRegistryItem(
				ItemSet.DisplayRecipientTaxIDHeading,
				"DisplayRecipientTaxIDHeading",
				"Accounting/Receivable Defaults/Form Configurations/Invoice",
				"Display Recipient Tax ID Heading on AR Invoice (CargoWiseOne Support Only)",
				@"This is a CargoWiseOne Support Only Registry.
Use this registry to modify the display of the Recipient Main Tax Registration heading in the AR Invoice document.
NOTE: The Recipient Tax ID Heading will only display in the AR Invoice document IF the ‘Display Recipient Tax ID on AR Invoice’ registry is set to YES.",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				string.Empty);

			AssertEquals("Default Value", string.Empty, ItemSet.DisplayRecipientTaxIDHeading.DefaultValue);
			ItemSet.DisplayRecipientTaxIDHeading.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TST");
			AssertEquals("Value", "TST", ItemSet.DisplayRecipientTaxIDHeading.Value);
		}

		public void TestFallBackToPreviousExchangeRate()
		{
			TestRegistryItem(ItemSet.FallBackToPreviousExchangeRate, "FallBackToPreviousExchangeRate", "Accounting", "Fall Back to Previous Exchange Rate", "Fall back to the previous exchange rate if today's exchange rate not found.", RegistryStorageFlags.Company, false);
		}

		public void TestPostInvoicesAndPaymentsToLoginBranch()
		{
			AssertEquals("DefaultValue", false, ItemSet.PostJobInvoicingTransactionsToLoginBranch.DefaultValue);

			ItemSet.PostJobInvoicingTransactionsToLoginBranch.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.PostJobInvoicingTransactionsToLoginBranch.Value);
		}

		public void TestIntercompanyPostingConfiguration()
		{
			IntercompanyPostingConfigurationCollection collection = new IntercompanyPostingConfigurationCollection();
			IntercompanyPostingConfiguration item = collection.AddNew();
			item.Company = "EDI";
			item.MaxCostVarianceApprovalLevel = item.MaxCostVarianceApprovalLevelList[0].Code;
			ItemSet.IntercompanyPostingConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Value", item.Company, ItemSet.IntercompanyPostingConfiguration.Value[0].Company);
			AssertEquals("Value", item.MaxCostVarianceApprovalLevel, ItemSet.IntercompanyPostingConfiguration.Value[0].MaxCostVarianceApprovalLevel);
		}

		public void TestJapanIATAImportAirLocalClientFRTChargeGroupRounding()
		{
			AssertEquals("DefaultValue", Constants.RoundingRules.Codes.None, ItemSet.JapanIATAImportAirLocalClientFRTChargeGroupRounding.DefaultValue);

			ItemSet.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYen);
			AssertEquals("Value", Constants.RoundingRules.Codes.JapanYen, ItemSet.JapanIATAImportAirLocalClientFRTChargeGroupRounding.Value);
		}

		public void TestConsolCostDefaultApportionmentMethod()
		{
			AssertEquals("DefaultValue", AllocationMethod.ChargeableUnits, ItemSet.ConsolCostDefaultApportionmentMethod.DefaultValue.ConsolCostDefaultApportionmentMethodCollection[0].Apportionment);
			AssertEquals("DefaultValue", 1, ItemSet.ConsolCostDefaultApportionmentMethod.DefaultValue.ConsolCostDefaultApportionmentMethodCollection.Count);
			var newConfig = ConsolCostDefaultApportionmentMethodConfiguration.Create_ForTestOnly(AllocationMethod.Revenue);
			ItemSet.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfig);
			AssertEquals("Value", AllocationMethod.Revenue, ItemSet.ConsolCostDefaultApportionmentMethod.Value.ConsolCostDefaultApportionmentMethodCollection[0].Apportionment);
			AssertEquals("Value", 1, ItemSet.ConsolCostDefaultApportionmentMethod.Value.ConsolCostDefaultApportionmentMethodCollection.Count);

			//event log
			var oldValue = new ConsolCostDefaultApportionmentMethodConfiguration();
			oldValue.ConsolCostDefaultApportionmentMethodCollection.RemoveAndDeleteAll();
			oldValue.ConsolCostDefaultApportionmentMethodCollection.Add(new ConsolCostDefaultApportionmentMethod()
			{
				Module = "ALL",
				Direction = "IMP",
				ContainerMode = "ALL",
				TransportMode = "ALL",
				Apportionment = "GWT",
				ConsolType = "ALL"
			});

			var newValue = new ConsolCostDefaultApportionmentMethodConfiguration();
			newValue.ConsolCostDefaultApportionmentMethodCollection.RemoveAndDeleteAll();
			newValue.ConsolCostDefaultApportionmentMethodCollection.Add(new ConsolCostDefaultApportionmentMethod()
			{
				Module = "ALL",
				Direction = "ALL",
				ContainerMode = "ALL",
				TransportMode = "ALL",
				Apportionment = "MAN",
				ConsolType = "ALL"
			});

			var expectedLogMessage = @"Deleted: Module=ALL, Consol Type=ALL, Direction=IMP, Transport Mode=ALL, Container Mode=ALL, Apportionment=GWT
Added: Module=ALL, Consol Type=ALL, Direction=ALL, Transport Mode=ALL, Container Mode=ALL, Apportionment=MAN
";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestCASSGLAccount()
		{
			AssertEquals("DefaultValue", ZGuid.Empty, ItemSet.CASSGLAccount.DefaultValue);
			Guid gLAccountPK = Guid.NewGuid();
			ItemSet.CASSGLAccount.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, gLAccountPK);
			AssertEquals("Value", gLAccountPK, ItemSet.CASSGLAccount.Value);
		}

		public void TestCASSImportAutoCreateClaims()
		{
			string expectedHint = @"This registry controls the automatic creation of claims during CASS file import. Following are the options available:
1. Create Claims for Over-billing, this option will create claims only for CASS over billing cases. (Default)
2. Create Claims for Over-billing and Under-billing, this option will create claims for CASS over billing as well as under billing cases.
3. Do NOT create claims, this option will not automatically create claims during CASS file import.";

			CodeDescriptionPairList expectedList = new CodeDescriptionPairList(OLookUpEditType.CASSAutoCreateClaim);
			TestRegistryItem(ItemSet.CASSImportAutoCreateClaims, "CASSImportAutoCreateClaims", Categories.Accounting_PayableDefaults_DefaultSettings_CASS, "CASS Import Auto Create Claims", expectedHint, RegistryStorageFlags.Company, expectedList, "OVR");
		}

		public void TestCASSCostImportAllowedDiscrepancy()
		{
			AssertEquals("DefaultValue", -1.00m, ItemSet.CASSCostImportAllowedDiscrepancy.DefaultValue);
			ItemSet.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 24);
			AssertEquals("Value", 24.00m, ItemSet.CASSCostImportAllowedDiscrepancy.Value);
		}

		public void TestCASSChargeCodes()
		{
			ResetRegistryCache();
			Env.Registry.FreightChargeCode = Guid.Empty;
			AssertEquals("DefaultValue", 0, ItemSet.CASSChargeCodes.DefaultValue.Count);

			ResetRegistryCache();
			Guid chargeCodePK = Guid.NewGuid();
			Env.Registry.FreightChargeCode = chargeCodePK;
			AssertEquals("ChargeCode", chargeCodePK, ItemSet.CASSChargeCodes.DefaultValue[0].ChargeCodePK.ToGuid());
			AssertEquals("CASSType", "ALL", ItemSet.CASSChargeCodes.DefaultValue[0].CASSType);
			AssertEquals("CASSComponentCode", "ALL", ItemSet.CASSChargeCodes.DefaultValue[0].CASSComponentCode);

			chargeCodePK = new TestObjectCreator(Factory).CC1.PK.ToGuid();
			CASSChargeCodeCollection cASSMaps = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var cASSMap = cASSMaps.AddNew();
			cASSMap.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cASSMap.CASSType = "ALL";
			cASSMap.CASSComponentCode = "ALL";
			cASSMap.ChargeCodePK = chargeCodePK;

			ItemSet.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, cASSMaps);
			AssertEquals("ChargeCode", chargeCodePK, ItemSet.CASSChargeCodes.Value[0].ChargeCodePK.ToGuid());
		}

		public void TestAutoPostMasterCollectCharge()
		{
			AssertEquals("DefaultValue", true, ItemSet.AutoPostMasterCollectCharge.DefaultValue);

			ItemSet.AutoPostMasterCollectCharge.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AutoPostMasterCollectCharge.Value);
		}

		public void TestCostConfirmationHeadingText()
		{
			AssertEquals("default value", "This document confirms that an Accounts Payable transaction has been posted with the following details:", ItemSet.CostConfirmationHeadingText.Value);
			ItemSet.CostConfirmationHeadingText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Test Caption");
			AssertEquals("set value", "Test Caption", ItemSet.CostConfirmationHeadingText.Value);
		}

		public void TestCostConfirmationDocumentTitle()
		{
			AssertEquals("default value", "Confirmation of Costs Posted", ItemSet.CostConfirmationDocumentTitle.Value);
			ItemSet.CostConfirmationDocumentTitle.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Test Caption");
			AssertEquals("set value", "Test Caption", ItemSet.CostConfirmationDocumentTitle.Value);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockGrm = Res.UseMockData())
			{
				string key = ((ResourceString)ItemSet.CostConfirmationDocumentTitle.Value).ResourceKey;
				mockGrm.Put(key, new ResourceStringData(key, "German Caption"));
				AssertEquals("set value", "German Caption", ItemSet.CostConfirmationDocumentTitle.Value);
			}
		}

		public void TestPrintOptionWhenAPInvoicePosted()
		{
			AssertEquals("default value", false, ItemSet.PrintOptionWhenAPInvoicePosted.Value);
			ItemSet.PrintOptionWhenAPInvoicePosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("set value", true, ItemSet.PrintOptionWhenAPInvoicePosted.Value);
		}

		public void TestPrintAutofatturaItalyDocument()
		{
			AssertEquals("PrintAutofatturaItalyDocument", ItemSet.PrintAutofatturaItalyDocument.Name);
			AssertEquals(Categories.Accounting_PayableDefaults_CostConfirmationDocument, ItemSet.PrintAutofatturaItalyDocument.Category);
			AssertEquals("Print Autofattura (Italy) Document", ItemSet.PrintAutofatturaItalyDocument.Caption);
			AssertEquals(@"When the registry is overridden & set to Yes, Cargo Wise Will ask user 'Do you want to print the Autofattura (IT) Document' each time an AP Invoice, Credit Note or Adjustment is posted in Payables Transactions module, or in  Unapproved Invoices module,  with Compliance subtype 'APS' and Compliance Number filled.

By default this prompting behavior is turned off.", ItemSet.PrintAutofatturaItalyDocument.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.PrintAutofatturaItalyDocument.Storage);
			AssertEquals(false, ItemSet.PrintAutofatturaItalyDocument.DefaultValue);
		}

		public void TestUnapprovedInvoiceCostConfirmationHeadingText()
		{
			AssertEquals("default value", "This document confirms that an Unapproved Accounts Payable transaction has been posted with the following details:", ItemSet.UnapprovedInvoiceCostConfirmationHeadingText.Value);
			ItemSet.UnapprovedInvoiceCostConfirmationHeadingText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Test Caption");
			AssertEquals("set value", "Test Caption", ItemSet.UnapprovedInvoiceCostConfirmationHeadingText.Value);
		}

		public void TestUnapprovedInvoiceCostConfirmationDocumentTitle()
		{
			AssertEquals("default value", "Confirmation of Costs Posted", ItemSet.UnapprovedInvoiceCostConfirmationDocumentTitle.Value);
			ItemSet.UnapprovedInvoiceCostConfirmationDocumentTitle.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Test Caption");
			AssertEquals("set value", "Test Caption", ItemSet.UnapprovedInvoiceCostConfirmationDocumentTitle.Value);
		}

		public void TestPrintOptionWhenUnapprovedAPInvoicePosted()
		{
			AssertEquals("default value", false, ItemSet.PrintOptionWhenUnapprovedAPInvoicePosted.Value);
			ItemSet.PrintOptionWhenUnapprovedAPInvoicePosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("set value", true, ItemSet.PrintOptionWhenUnapprovedAPInvoicePosted.Value);
		}

		public void TestLastAggregateStaff()
		{
			AssertEquals("default value", Guid.Empty, ItemSet.LastAggregationStaff.Value);
			Guid staffPK = Guid.NewGuid();
			ItemSet.LastAggregationStaff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, staffPK);
			AssertEquals("set value", staffPK, ItemSet.LastAggregationStaff.Value);
		}

		public void TestLastAggregateDate()
		{
			AssertEquals("default value", Guid.Empty, ItemSet.LastAggregationStaff.Value);
			DateTime date = Env.Time.CurrentLocalDateTime;
			ItemSet.LastAggregationDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, date);
			AssertEquals("set value", date, ItemSet.LastAggregationDate.Value);
		}

		public void TestShareSequentialInvoiceTransactionNumbers()
		{
			AssertEquals("DefaultValue", false, ItemSet.ShareSequentialInvoiceTransactionNumbers.DefaultValue.Value);
			ShareSequentialTransactionNumbers item = new ShareSequentialTransactionNumbers();
			item.Value = true;
			ItemSet.ShareSequentialInvoiceTransactionNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Value", true, ItemSet.ShareSequentialInvoiceTransactionNumbers.Value.Value);
		}

		public void TestShareSequentialInvoiceReferenceNumbers()
		{
			AssertEquals("DefaultValue", false, ItemSet.ShareSequentialInvoiceReferenceNumbers.DefaultValue.Value);
			ShareSequentialReferenceNumbers item = new ShareSequentialReferenceNumbers();
			item.Value = true;
			ItemSet.ShareSequentialInvoiceReferenceNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Value", true, ItemSet.ShareSequentialInvoiceReferenceNumbers.Value.Value);
		}

		public void TestSubsequentInvoiceRegistryItem()
		{
			Assert("Default Value", !ItemSet.ShouldInvoiceShowCopyWhenPrintedSubsequentTimes.Value);
			ItemSet.ShouldInvoiceShowCopyWhenPrintedSubsequentTimes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert("Default Value", ItemSet.ShouldInvoiceShowCopyWhenPrintedSubsequentTimes.Value);
		}

		public void TestCreditCardFee()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			CreditCardFeeCollection collection = new CreditCardFeeCollection();
			CreditCardFee item = collection.AddNew();
			item.ChargeCodePK = chargeCode.PK;
			item.Percentage = 10.10;

			ItemSet.CreditCardFee.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Value.Count", 1, ItemSet.CreditCardFee.Value.Count);
			AssertEquals("Value[0].ChargeCodePK", chargeCode.PK, ItemSet.CreditCardFee.Value[0].ChargeCodePK);
			AssertEquals("Value[0].Percentage", new ZDecimal(10.10), ItemSet.CreditCardFee.Value[0].Percentage);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.CreditCardFee.Storage);
		}

		public void TestZeroAmountTaxTypesDescription()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			ZeroAmountTaxTypesDescriptionsCollection collection = new ZeroAmountTaxTypesDescriptionsCollection();
			AssertEquals("Value.Count", 8, ItemSet.ZeroAmountTaxTypesDescription.Value.Count);
			AssertEquals("Value[0].TaxType", AccTaxRate.Types.Rated, ItemSet.ZeroAmountTaxTypesDescription.Value[0].TaxType);
			AssertEquals("Value[0].Description", "Zero Rated tax treatments are RAT (Rated) Tax types with a 0% tax rate.", ItemSet.ZeroAmountTaxTypesDescription.Value[0].Description);
			AssertEquals("Value[0].DefaultValue", "Zero Rated", ItemSet.ZeroAmountTaxTypesDescription.Value[0].DefaultValue);
			AssertEquals("Value[0].OverrideValue", string.Empty, ItemSet.ZeroAmountTaxTypesDescription.Value[0].OverrideValue);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.ZeroAmountTaxTypesDescription.Storage);
			AssertEquals("Hint", @"This registry defines the tax description printed in the invoice document when the VAT/GST Tax ID recorded against individual lines does not record an amount of VAT/GST tax.
If required, you can override the default values and configure a login company specific description.", ItemSet.ZeroAmountTaxTypesDescription.Hint);
		}

		public void TestDescriptionInDocumentsForTaxAmountsRule()
		{
			AssertEquals("DefaultValue", AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code, ItemSet.DescriptionInDocumentsForTaxAmountsRule.DefaultValue);
			ItemSet.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
			AssertEquals("Value", AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code, ItemSet.DescriptionInDocumentsForTaxAmountsRule.Value);
			ItemSet.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
			AssertEquals("Value", AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code, ItemSet.DescriptionInDocumentsForTaxAmountsRule.Value);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.ZeroAmountTaxTypesDescription.Storage);
			AssertEquals("Hint", @"This registry decides how charges recorded with a VAT/GST tax amount will print in the Invoice document.
Depending on your configuration of this registry, the Invoice document will print the tax rate used to calculate VAT/GST tax and/or the resulting VAT/GST tax amount.
This registry is configurable at the Login Company level only.", ItemSet.DescriptionInDocumentsForTaxAmountsRule.Hint);
		}

		public void TestInvoiceTradingTerms()
		{
			ItemSet.InvoiceTradingTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(1, 2));
			AssertEquals("Value.Size", new System.Drawing.Size(1, 2), ItemSet.InvoiceTradingTerms.Value.Size);
		}

		public void TestInvoiceCopies()
		{
			InvoiceCopyCollection defaultValue = ItemSet.InvoiceCopies.DefaultValue;
			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertEquals("DefaultValue[0].Name", "", defaultValue[0].Name);
			AssertEquals("DefaultValue[0].DeliveryMethod", nameof(PrintCopyType.ALL), defaultValue[0].DeliveryMethod);
			AssertEquals("DefaultValue[0].IncludeTradingTerms", true, defaultValue[0].IncludeTradingTerms);
			AssertEquals("DefaultValue[0].IsOriginal", true, defaultValue[0].IsOriginal);
			AssertEquals("DefaultValue[0].Order", 1, defaultValue[0].Order);

			InvoiceCopyCollection newValue = new InvoiceCopyCollection();
			InvoiceCopy copy = newValue.AddNew();
			copy.Name = (NoResString)"RHWAR!";
			copy.DeliveryMethod = nameof(PrintCopyType.EML);
			ItemSet.InvoiceCopies.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			InvoiceCopyCollection value = ItemSet.InvoiceCopies.Value;
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].Name", "RHWAR!", value[0].Name);
			AssertEquals("Value[0].DeliveryMethod", nameof(PrintCopyType.EML), value[0].DeliveryMethod);
		}

		public void TestAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob()
		{
			AssertEquals("DefaultValue", true, ItemSet.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.DefaultValue);

			ItemSet.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.Value);
		}

		public void TestInvoicePrintingOptionForChina()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			AssertEquals("DefaultValue", "ENT", ItemSet.InvoicePrintingOption.DefaultValue);
			Assert("should be hidden", ItemSet.InvoicePrintingOption.HasOption(RegistryOptions.IsHidden));
		}

		public void TestInvoicePrintingOption()
		{
			AssertEquals("DefaultValue", "TAX", ItemSet.InvoicePrintingOption.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.InvoicePrintingOption.CountryFilterPKs);
			ItemSet.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ENT");
			AssertEquals("Value", "ENT", ItemSet.InvoicePrintingOption.Value);
		}

		public void TestPortugalCertificationKey()
		{
			AssertEquals("DefaultValue", "", ItemSet.PortugalCertificationKey.DefaultValue);
			AssertEquals("Name", "PortugalCertificationKey", ItemSet.PortugalCertificationKey.Name);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.PortugalCertificationKey.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden | RegistryOptions.IsReadOnly, ItemSet.PortugalCertificationKey.Options);
			AssertEquals("DefaultValue", ZString.Empty, ItemSet.PortugalCertificationKey.DefaultValue);
		}

		public void TestElectronicPayments()
		{
			AssertEquals("DefaultValue", false, ItemSet.EnableElectronicPayments.DefaultValue);
			ItemSet.EnableElectronicPayments.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("modified", true, ItemSet.EnableElectronicPayments.Value);

			AssertEquals("DefaultValue", ZString.Empty, ItemSet.ElectronicPaymentBillerCode.DefaultValue);
			ItemSet.ElectronicPaymentBillerCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "1234");
			AssertEquals("modified", "1234", ItemSet.ElectronicPaymentBillerCode.Value);

			string defaultTerms = "Contact your bank or financial institution to make this payment from your cheque, savings, debit, credit card or transaction account.";
			AssertEquals("DefaultValue", defaultTerms, ItemSet.ElectronicPaymentTerms.DefaultValue);
			ItemSet.ElectronicPaymentTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "blah blah");
			AssertEquals("modified", "blah blah", ItemSet.ElectronicPaymentTerms.Value);
		}

		public void TestDisplayInvoiceTotalsbyTaxRate()
		{
			AssertEquals("DefaultValue", false, ItemSet.DisplayInvoiceTotalsbyTaxRate.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.DisplayInvoiceTotalsbyTaxRate.CountryFilterPKs);
			ItemSet.DisplayInvoiceTotalsbyTaxRate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DisplayInvoiceTotalsbyTaxRate.Value);
		}

		public void TestDisplayNotYetOutstandingAmountOnARStatementDocuments()
		{
			AssertEquals("DefaultValue", false, ItemSet.DisplayNotYetOutstandingAmountOnARStatementDocuments.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.DisplayNotYetOutstandingAmountOnARStatementDocuments.CountryFilterPKs);
			ItemSet.DisplayNotYetOutstandingAmountOnARStatementDocuments.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DisplayNotYetOutstandingAmountOnARStatementDocuments.Value);
		}

		public void TestShowFullListingOfContainerNumbersOnSeparatePage()
		{
			AssertEquals("DefaultValue must be false", false, ItemSet.ShowFullListingOfContainerNumbersOnSeparatePage.DefaultValue);
			ItemSet.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value must be true", true, ItemSet.ShowFullListingOfContainerNumbersOnSeparatePage.Value);
		}

		public void TestThirdPartyEInvoiceDocType()
		{
			var item = ItemSet.ThirdPartyEInvoiceDocType;
			AssertEquals("ThirdPartyEInvoiceDocType", item.Name);
			AssertEquals(Categories.Documents, item.Category);
			AssertEquals("Third Party e-Invoice Doc Type (CW1 Support Only)", item.Caption);
			AssertEquals(@"This registry is used to specify the default document type of e-invoice documents obtained from third party provider.
Changing the value of this registry will only affect e-invoice documents from a third party provider that is automatically saved in eDocs through a PDF request API.
The behavior of adding eDocs using other method will not be affected by this registry.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue, item.Options);

			var codeDescriptionpairList = ((CodePairRegistryDataType)item.DataType).LookUpList;
			var expectedDocTypes = "ACV, BOA, BOD, BRC, CAT, CLL, COM, COO, COT, CRP, DDR, DEC, EXD, FCR, GLJ, IEP, INV, JRJ, MCD, MSC, MSD, NAF, PAY, PER, PIN, PPO, PRV, PUB, QRA, SCCD, SIMG, SREP, TDM";
			AssertEquals(expectedDocTypes, codeDescriptionpairList.CodesAsString);

			var innerDataType = item.Inner.DataType as CodePairRegistryDataType;
			Assert("ValidateCode must be true", innerDataType.ValidateCode);

			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(ItemSet.ThirdPartyEInvoiceDocType
				, x => x.ThirdPartyEInvoiceDocType
				, ("INV", "INV")
				, ("MSC", "MSC")
				, "Third Party e-Invoice Doc Type (CW1 Support Only)");
		}

		public void TestShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency()
		{
			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(ItemSet.ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency
				, x => x.GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency()
				, ("True", true)
				, ("False", false)
				, "Show Local Currency Equivalent Totals on AR Invoice in OS Currency");
		}

		public void TestUseLocalExTaxAmountWhileCalculatingLocalTaxAmount()
		{
			var portugalCompany = Factory.NewWithValidTestData<GlbCompany>();
			portugalCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Portugal;

			var australianCompany = Factory.NewWithValidTestData<GlbCompany>();
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;

			var italianCompany = Factory.NewWithValidTestData<GlbCompany>();
			italianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Italy;

			Factory.Save();

			TestRegistryItem(ItemSet.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount,
				"UseLocalExTaxAmountWhileCalculatingLocalTaxAmount",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"Use Local Ex Tax Amount While Calculating Local Tax Amount (CargoWiseOne Support Only)",
				@"If the value of this registry is set to 'Yes', the Local Tax Amount is calculated by applying the Tax Rate to the Local Ex Tax Amount. Otherwise, it is calculated by converting the OS Tax Amount to the Local Tax Amount using the exchange rate.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);

			AssertEquals("Default Value for Portugal company should be False", false, ItemSet.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.GetValueWithoutFallback(portugalCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Default Value for Australian company should be False", false, ItemSet.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.GetValueWithoutFallback(australianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Default Value for Italian company should be True", true, ItemSet.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.GetValueWithoutFallback(italianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestDisallowPostingTransactionWithEmptyComplianceSubtype()
		{
			BusinessObject italyCompany = Factory.NewWithValidTestData<GlbCompany>();
			italyCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Italy;

			BusinessObject australianCompany = Factory.NewWithValidTestData<GlbCompany>();
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;

			Factory.Save();
			TestRegistryItem(ItemSet.DisallowPostingTransactionWithEmptyComplianceSubtype, "DisallowPostingTransactionWithEmptyComplianceSubtype", "Accounting/Government Compliance Invoice Document", "Disallow posting transactions with empty compliance subtype", "Tick this item as 'yes' when you want to disallow posting transactions with empty compliance subtype.", RegistryStorageFlags.Company, false);
			AssertEquals("Default Value for Italy company should be False", false, ItemSet.DisallowPostingTransactionWithEmptyComplianceSubtype.GetValueWithoutFallback(italyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Default Value for Australian company should be False", false, ItemSet.DisallowPostingTransactionWithEmptyComplianceSubtype.GetValueWithoutFallback(australianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestMaxAccrualVsActualDiscrepancies()
		{
			AssertEquals("DefaultValue must be 0", 0, ItemSet.MaxAccrualVsActualDiscrepancies.DefaultValue);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings/Bulk AP Invoice Posting", ItemSet.MaxAccrualVsActualDiscrepancies.Category);
			ItemSet.MaxAccrualVsActualDiscrepancies.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 56);
			AssertEquals("Value must be 56", 56, ItemSet.MaxAccrualVsActualDiscrepancies.Value);
		}

		public void TestEnableLocalChargeCodeDescriptionDefault()
		{
			BusinessObject chineseCompany = Factory.NewWithValidTestData<GlbCompany>();
			chineseCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.China;

			BusinessObject australianCompany = Factory.NewWithValidTestData<GlbCompany>();
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;

			Factory.Save();
			TestRegistryItem(ItemSet.EnableLocalChargeCodeDescriptionDefault, "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT", "Accounting/Job Invoicing", "Enable Local Charge Code Description Default", "In Charge Code, you can maintain translations of the Charge Code description into foreign languages.\r\n\r\nWhen this registry is set to ‘No’, Charge Code descriptions are translated into the foreign language that you used for printing a sales invoice or Rating document.\r\n\r\nWhen this registry is set to ‘Yes’, you can setup Local Language Description in Charge Code which is used as the default description for printing a sales invoice or Rating document to a local client (meaning client domicile in the same country/region as the current local in company domicile country/region).\r\n\r\nYou can always override the description of the charges before posting.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestGetGLAccountFormat()
		{
			AssertEquals("GetGLAccountFormat(\"\")", "XXXX.XXX", AccountingConfigurationRegistry.GetGLAccountFormat(""));
			AssertEquals("GetGLAccountFormat(\"AB.C\")", "XX.X", AccountingConfigurationRegistry.GetGLAccountFormat("AB.C"));
		}

		public void TestGLAccountFormatItems()
		{
			TestCaseHelper.ClearTable(AccChargeCodeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccGLHeaderSchema.Constants.TableName);
			TestRegistryItem(ItemSet.GLAccountFormat, "GL_ACCOUNT_FORMAT", "", "", "", RegistryStorageFlags.System, RegistryOptions.IsHidden, TextEditorType.TextBox, "");
			TestRegistryItem(ItemSet.GLAccountFormatLink, "Accounting/General Ledger Defaults", "GL Account Format", "The format of numbers in the General Ledger Chart of Accounts (go to Maintain -> Account -> GL Accounts) must be in the format set in this registry. Click the button below to migrate the current format to a different format. This will also update all existing General Ledger accounts.", ModuleIDs.GLAccountFormat);
		}

		public void TestIsNettingSystem()
		{
			TestGenericRegistryItem(ItemSet.IsNettingSystem,
				"IsNettingSystem",
				"Accounting/Netting",
				"Is Netting System? (CargoWiseOne Support Only)",
				"Marks a company as a Netting System. It can be set only through CargoWiseOne Support.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestNettingSystemOrg()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestGenericRegistryItem(ItemSet.NettingSystemOrg,
				"NettingSystemOrg",
				"Accounting/Netting",
				"Netting System Organization",
				"This registry allows to set the Netting System organization.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsValueMandatory,
				Guid.Empty);
		}

		public void TestNettingThresholdValue()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestGenericRegistryItem(ItemSet.NettingThresholdValue,
				"NettingThresholdValue",
				"Accounting/Netting",
				"Threshold for Netting matching",
				@"This threshold value is used as percentage value to match Receivables Netting Transactions with Payables Netting Transactions in the netting system.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				1M);
		}

		public void TestNettingStartDate()
		{
			TestGenericRegistryItem(ItemSet.NettingStartDate,
				"NettingStartDate",
				"Accounting/Netting",
				"Netting participation start date (CargoWiseOne Support Only)",
				"When this registry value is set in conjunction with 'Enable Netting', enables invoices to be sent to the netting system for processing. Those invoices that are created after this date will be sent to the netting system.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestNettingMatchingControlAccount()
		{
			AssertEquals("Name", "NettingMatchingControlAccount", ItemSet.NettingMatchingControlAccount.Name);
			AssertEquals("Category", "Accounting/Netting", ItemSet.NettingMatchingControlAccount.Category);
			AssertEquals("Caption", "Netting Clearing Account", ItemSet.NettingMatchingControlAccount.Caption);
			AssertEquals("Hint", "The system will use this general ledger account when creating balancing journals as part of the Netting Process.", ItemSet.NettingMatchingControlAccount.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.NettingMatchingControlAccount.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.NettingMatchingControlAccount.Options);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.BSHAndNonControl, ((GuidFindBoxRegistryEditorInfo)ItemSet.NettingMatchingControlAccount.EditorInfo).FindBoxFilter);
		}

		public void TestGLAccountFormatDefaultValue()
		{
			TestCaseHelper.ClearTable(AccChargeCodeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccGLHeaderSchema.Constants.TableName);
			AccGLHeader header = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();
			AssertEquals("DefaultValue", "XXXXXXXXXX", ItemSet.GLAccountFormat.DefaultValue);
		}

		public void TestDefaultDepartmentsOfLocalTransport()
		{
			AssertEquals("Caption of the Button of the registry item", "Edit Job Types", ItemSet.DefaultDepartmentsOfLocalTransport.ButtonCaption);
			AssertEquals("ModuleName of the registry item", "Port Transport", ItemSet.DefaultDepartmentsOfLocalTransport.ModuleName);
			AssertEquals("ModuleID of the registry item", ModuleIDs.CartageType, ItemSet.DefaultDepartmentsOfLocalTransport.ModuleID);
			AssertEquals("Hint of the registry item", "Default Departments for Port Transport Jobs can be set per Job Type. You can select a Job Type and edit its Default Department.", ItemSet.DefaultDepartmentsOfLocalTransport.Hint);
		}

		public void TestENettWebServiceLocation()
		{
			TestRegistryItem(ItemSet.ENettWebServiceLocation, "ENettWebServiceLocation", "Accounting/ComPay", "ComPay Web Service Location", "This registry item controls the location of the ComPay web service.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, TextEditorType.TextBox, "http://compay20-dev.1-stop.biz/IntegrationService/IntegrationService.asmx");
		}

		public void TestENettNotificationsGroup()
		{
			Guid defaultValue = ItemSet.ENettNotificationsGroup.DefaultValue;

			BusinessObject glbGroup = Factory.Load<GlbGroup>(defaultValue);
			AssertEquals("DefaultValue should be the 'All Users' group.", "ALL", glbGroup[GlbGroupSchema.Constants.GG_Code]);

			Guid newValue = Guid.NewGuid();
			ItemSet.ENettNotificationsGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value", newValue, ItemSet.ENettNotificationsGroup.Value);
		}

		public void TestRecognizeProfitOnWIPsAccrualsBeforePosting()
		{
			AssertEquals("DefaultValue", false, ItemSet.RecognizeProfitOnWIPsAccrualsBeforePosting.DefaultValue);

			ItemSet.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.RecognizeProfitOnWIPsAccrualsBeforePosting.Value);
		}

		public void TestSetJobStatusToInvoicedWhenFirstARInvoicePosted()
		{
			foreach (CodeDescriptionBool item in ItemSet.SetJobStatusToInvoicedWhenFirstARInvoicePosted.DefaultValue)
			{
				if (item.Code == JobHeaderStatus.WorkOnHold.Code
							|| item.Code == JobHeaderStatus.InvoiceOnHold.Code
							|| item.Code == JobHeaderStatus.Closed.Code)
				{
					AssertEquals("Value when Status: " + item.Code, true, item.Bool);
				}
				else
				{
					AssertEquals("Value when Status: " + item.Code, false, item.Bool);
				}
			}

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(JobHeaderStatus.Complete);
			list.Add(JobHeaderStatus.Closed);
			list.Add(JobHeaderStatus.JobInvoiced);

			new TestObjectCreator(Factory).SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), list);
			foreach (CodeDescriptionBool item in ItemSet.SetJobStatusToInvoicedWhenFirstARInvoicePosted.Value)
			{
				AssertEquals("Value when Status: " + item.Code, list.ContainsCode(item.Code), item.Bool);
			}
		}

		public void TestDoesJobStatusGetChangedToInvoicedWhenARInvoicePosted()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(JobHeaderStatus.Complete);

			new TestObjectCreator(Factory).SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), list);

			AssertEquals("Value when Status WRK ", true, ItemSet.DoesJobStatusGetChangedToInvoicedWhenARInvoicePosted(JobHeaderStatus.Working.Code));
			AssertEquals("Value when Status CMP ", false, ItemSet.DoesJobStatusGetChangedToInvoicedWhenARInvoicePosted(JobHeaderStatus.Complete.Code));
			AssertEquals("Value when Status TST (invalid value) ", false, ItemSet.DoesJobStatusGetChangedToInvoicedWhenARInvoicePosted("TST"));
		}

		public void TestCostVarianceApproval()
		{
			CostVarianceApproval item = new CostVarianceApproval();
			item.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVariance;
			item.VarianceComparisonOption = Constants.CostVarianceComparisonOption.JobAndChargeCode;
			ItemSet.CostVarianceApproval.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("VarianceCalculationStyle", Constants.CostVarianceCalculationStyle.PercentageVariance, ItemSet.CostVarianceApproval.Value.VarianceCalculationStyle);
			AssertEquals("VarianceComparisonOption", Constants.CostVarianceComparisonOption.JobAndChargeCode, ItemSet.CostVarianceApproval.Value.VarianceComparisonOption);
		}

		public void TestCostVarianceNoApprovalRequired()
		{
			AssertEquals("Caption", "Cost Variance - No approval required when charge is imported and both amount and currency matches", ItemSet.CostVarianceNoApprovalRequired.Caption);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.CostVarianceNoApprovalRequired.Category);
			AssertEquals("DefaultValue", true, ItemSet.CostVarianceNoApprovalRequired.DefaultValue);
			AssertEquals("Hint", @"When the ""Cost Variance Style And Approval Thresholds"" registry is set to 'CJB', 'CCH' or 'CJR' AND this registry is set to 'Yes', no approval will be required for charges that fulfilled the below conditions.
Further, if the ""Cost Variance Style And Approval Thresholds registry > Automatically tick cost as Final when they fall within the 'None' approval threshold."" setting is enabled, the Final flag will be ticked.

1. The charge is imported.
2. The ACR and CST amount and currency is the same.

Example 1: No approval will be required regardless of variance in local amount.
ACR is USD 800 (Local Amount 1000).
CST is USD 800 (Local Amount 1100 due to exchange rate movement).

Example 2: As the OS amount does not match, cost variance check will be enforced and may subject to approval depending on ""Cost Variance Style and Approval Threshold"" settings.
ACR is USD 800(Local Amount 1000).
CST is USD 801(Local Amount 1100).

By default, this registry will be set to 'Yes'.
If this registry is set to 'No', all charges will be subjected to cost variance analysis as per 'Cost Variance Style and Approval Threshold' registry settings.", ItemSet.CostVarianceNoApprovalRequired.Hint);
			AssertEquals("Name", "CostVarianceNoApprovalRequired", ItemSet.CostVarianceNoApprovalRequired.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CostVarianceNoApprovalRequired.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CostVarianceNoApprovalRequired.Storage);

			var registry = ItemSet.CostVarianceNoApprovalRequired;
			var oldValue = true;
			var newValue = false;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = ItemSet.CostVarianceNoApprovalRequired.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestInvoiceTransactionNumberPrefix()
		{
			AssertEquals("DefaultValue", string.Empty, ItemSet.InvoiceTransactionNumberPrefix.DefaultValue);
			ItemSet.InvoiceTransactionNumberPrefix.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABC");
			AssertEquals("Value", "ABC", ItemSet.InvoiceTransactionNumberPrefix.Value);
		}

		public void TestSelfBillingInvoiceTransactionNumberPrefix()
		{
			AssertEquals("DefaultValue", string.Empty, ItemSet.SelfBillingInvoiceTransactionNumberPrefix.DefaultValue);
			ItemSet.SelfBillingInvoiceTransactionNumberPrefix.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABC");
			AssertEquals("Value", "ABC", ItemSet.SelfBillingInvoiceTransactionNumberPrefix.Value);
		}

		public void TestSelfBillingTaxAdjustmentNoteTitle()
		{
			object value = ItemSet.TaxSelfBilledAdjustmentNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("SelfBillingTaxAdjustmentNoteTitle", value is MultilingualString);
		}

		public void TestSelfBillingNonTaxAdjustmentNoteTitle()
		{
			object value = ItemSet.NonTaxSelfBilledAdjustmentNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("SelfBillingNonTaxAdjustmentNoteTitle", value is MultilingualString);
		}

		public void TestSelfBillingAdjustmentNoteMessage()
		{
			object value = ItemSet.SelfBilledAdjustmentNoteMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("SelfBillingAdjustmentNoteMessage", value is MultilingualString);
		}

		public void TestJobProfitLossReasonCode()
		{
			JobProfitLossReasonCodeCollection newValue = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode copy = newValue.AddNew();
			copy.Code = "TST";
			copy.Description = (NoResString)"Test Code";
			ItemSet.JobProfitLossReasonCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			JobProfitLossReasonCodeCollection value = ItemSet.JobProfitLossReasonCode.Value;
			AssertEquals("Count", 1, value.Count);
			AssertEquals("Code", "TST", value[0].Code);
			AssertEquals("Description", "Test Code", value[0].Description);
		}

		public void TestJobProfitLossRequiringReasonParameters()
		{
			JobProfitLossRequiringReasonParameters newValue = new JobProfitLossRequiringReasonParameters();
			newValue.ProfitThreshold = 3M;
			newValue.JobStatusCollection.AddNew().Code = "WRK";
			ItemSet.JobProfitLossRequiringReasonParameters.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			JobProfitLossRequiringReasonParameters value = ItemSet.JobProfitLossRequiringReasonParameters.Value;
			AssertEquals("MarginThreshold", 3M, value.ProfitThreshold);
			AssertEquals("JobStatus Code", "WRK", value.JobStatusCollection[0].Code);
		}

		public void TestAccountingReceiptPrintPrompting()
		{
			AssertEquals("Caption of the registry item", "Receipt Print Prompting", ItemSet.AccountingReceiptPrintPrompting.Caption);
			AssertEquals("Hint of the registry item", "When enabled, system will prompt user to print a Receipt document upon AR Receipting.", ItemSet.AccountingReceiptPrintPrompting.Hint);

			AssertEquals("DefaultValue", true, ItemSet.AccountingReceiptPrintPrompting.DefaultValue);
			ItemSet.AccountingReceiptPrintPrompting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AccountingReceiptPrintPrompting.Value);
		}

		public void TestAlwaysGroupInvoicesPrintingForSingleDebtor()
		{
			AssertEquals("DefaultValue", true, ItemSet.AlwaysGroupInvoicesPrintingForSingleDebtor.DefaultValue);

			ItemSet.AlwaysGroupInvoicesPrintingForSingleDebtor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AlwaysGroupInvoicesPrintingForSingleDebtor.Value);
		}

		public void TestInvoicePrintingGroupByOrganization()
		{
			AssertEquals("DefaultValue", true, ItemSet.InvoicePrintingGroupByOrganization.DefaultValue);

			ItemSet.InvoicePrintingGroupByOrganization.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.InvoicePrintingGroupByOrganization.Value);
		}

		public void TestEnforceExpectedTotalTaxAndExcludingTaxAmountValidation()
		{
			AssertEquals("Name", "EnforceExpectedTotalTaxAndExcludingTaxAmountValidation", ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Name);
			AssertEquals("Category", Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Category);
			AssertEquals("Caption", "Enforce Expected Total Tax and Excluding Tax Amount Validation", ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Caption);
			AssertEquals("Hint", @"This registry affects the posting behavior of Payable Invoice and Credit Note only.

By default, this registry is set to 'No' and the system will allow posting as long as the Invoice Total (inclusive of tax) agrees with the Expected Total (inclusive of tax).

If you want the system to prevent posting when the Invoice Total Ex-Tax and/or Tax Amount does not agree with the respective Expected Totals, set this registry to 'Yes'.

When this registry is set to 'Yes', a validation error will be shown instead of a warning message when there are any discrepancies.", ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Storage);
			AssertEquals("DefaultValue", false, ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.DefaultValue);

			ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Value);

			var registry = ItemSet.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestAllowIncludingRelatedTransasctionDebtorColumnOfAP()
		{
			AssertEquals("DefaultValue", false, ItemSet.AllowIncludingRelatedTransasctionDebtorColumnOfAP.DefaultValue);

			ItemSet.AllowIncludingRelatedTransasctionDebtorColumnOfAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AllowIncludingRelatedTransasctionDebtorColumnOfAP.Value);
		}

		public void TestAllowIncludingRelatedTransasctionDebtorColumnOfAR()
		{
			AssertEquals("DefaultValue", false, ItemSet.AllowIncludingRelatedTransasctionDebtorColumnOfAR.DefaultValue);

			ItemSet.AllowIncludingRelatedTransasctionDebtorColumnOfAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AllowIncludingRelatedTransasctionDebtorColumnOfAR.Value);
		}

		public void TestAllowChargeDescriptionOverrideOnPostedARInvoice()
		{
			AssertEquals("DefaultValue", ZGuid.Empty.ToString(), ItemSet.AllowChargeDescriptionOverrideOnPostedARInvoice.DefaultValue);

			ItemSet.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "B602E07C-A17C-4F2B-B980-6FB1E3A114C0");
			AssertEquals("Value", "B602E07C-A17C-4F2B-B980-6FB1E3A114C0", ItemSet.AllowChargeDescriptionOverrideOnPostedARInvoice.Value);
		}

		public void TestDisallowSelectionOfSBRandSBDInvoiceType()
		{
			AssertEquals("Name", "DisallowSelectionOfSBRandSBDInvoiceType", ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.Category);
			AssertEquals("Caption", "Disallow selection of 'SBR' and 'SBD' invoice type if Charge Debtor is not setup as Self Bills Customer", ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.Caption);
			AssertEquals("Hint", @"By default, this registry is set to 'No' and the system will always allow users to select invoice type 'SBR' and 'SBD' when entering job billing charges, as per current system behavior.

When this registry is set to 'Yes', the system will disallow users from selecting invoice type 'SBR' and 'SBD' if the Charge Debtor's Organization > A/R > Invoicing > Invoicing > Customer Self Bills check box is NOT ticked.", ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.Options);
			AssertEquals("DefaultValue", false, ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.DefaultValue);
			ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DisallowSelectionOfSBRandSBDInvoiceType.Value);
		}

		public void TestAllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing()
		{
			AssertEquals("Name", "AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing", ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Category);
			AssertEquals("Caption", "Allow Charge To Retrieve Sell Invoice Exchange Rate From Job During Periodic Invoicing (CargoWiseOne Support Only)", ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Caption);
			AssertEquals("Hint", "This aims to fix a bug where the sell invoice currency shows as 0 when periodic invoicing.", ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Options);
			AssertEquals("DefaultValue", true, ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.DefaultValue);
			ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Value);
		}

		public void TestItalyTaxRegimeID()
		{
			var regItem = ItemSet.ItalyTaxRegimeID;
			AssertEquals("Name", "ItalyTaxRegimeID", regItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Italy, regItem.Category);
			AssertEquals("Caption", "Tax Regime ID (CargoWiseOne Support Only)", regItem.Caption);
			AssertEquals("Hint", "This registry provides Tax Regime ID for <RegimeFiscale> element in Italy e-Invoicing XML mapping.", regItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, regItem.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, regItem.Options);
			AssertEquals("Default Value", ItalyTaxRegimeIdTypes.RF01.Code, regItem.DefaultValue);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF19);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF18);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF17);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF16);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF15);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF14);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF13);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF12);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF11);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF10);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF09);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF08);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF07);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF06);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF05);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF04);
			//No RF03
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF02);
			AssertItalyTaxRegimeIDCodePair(ItalyTaxRegimeIdTypes.RF01);
		}

		void AssertItalyTaxRegimeIDCodePair(CodeDescriptionPair codePair)
		{
			ItemSet.ItalyTaxRegimeID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codePair.Code);
			AssertEquals("Value", codePair.Code, ItemSet.ItalyTaxRegimeID.Value);
		}

		#region MTD Previous Period Inclusion Rules

		public void TestNetValueOfVATErrorsThresholdLowerLimit()
		{
			AssertEquals("DefaultValue", 10000, ItemSet.NetValueOfVATErrorsThresholdLowerLimit.DefaultValue);
			AssertEquals("Category", Categories.Accounting_UKMTDPreviousPeriodInclusionRules, ItemSet.NetValueOfVATErrorsThresholdLowerLimit.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.NetValueOfVATErrorsThresholdLowerLimit.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.NetValueOfVATErrorsThresholdLowerLimit.Options);
			AssertEquals("Caption", "Net value of VAT errors threshold (lower limit)", ItemSet.NetValueOfVATErrorsThresholdLowerLimit.Caption);
			AssertEquals("Hint", "", ItemSet.NetValueOfVATErrorsThresholdLowerLimit.Hint);

			ItemSet.NetValueOfVATErrorsThresholdLowerLimit.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5000);
			AssertEquals("Value", 5000, ItemSet.NetValueOfVATErrorsThresholdLowerLimit.Value);
		}

		public void TestNetValueOfVATErrorsThresholdUpperLimit()
		{
			AssertEquals("DefaultValue", 50000, ItemSet.NetValueOfVATErrorsThresholdUpperLimit.DefaultValue);
			AssertEquals("Category", Categories.Accounting_UKMTDPreviousPeriodInclusionRules, ItemSet.NetValueOfVATErrorsThresholdUpperLimit.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.NetValueOfVATErrorsThresholdUpperLimit.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.NetValueOfVATErrorsThresholdUpperLimit.Options);
			AssertEquals("Caption", "Net value of VAT errors threshold (upper limit)", ItemSet.NetValueOfVATErrorsThresholdUpperLimit.Caption);
			AssertEquals("Hint", "", ItemSet.NetValueOfVATErrorsThresholdUpperLimit.Hint);

			ItemSet.NetValueOfVATErrorsThresholdUpperLimit.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5000);
			AssertEquals("Value", 5000, ItemSet.NetValueOfVATErrorsThresholdUpperLimit.Value);
		}

		public void TestTaxReturnAdjustmentReasonsList()
		{
			AssertEquals("Name", "TaxReturnAdjustmentReasonsList", ItemSet.TaxReturnAdjustmentReasonsList.Name);
			AssertEquals("Category", "Accounting/Tax Configurations", ItemSet.TaxReturnAdjustmentReasonsList.Category);
			AssertEquals("Caption", "Tax Return Adjustment Reason Codes", ItemSet.TaxReturnAdjustmentReasonsList.Caption);
			AssertEquals("Hint", "You can override the system defined Adjustment reason codes and descriptions for VAT return values using this Registry.", ItemSet.TaxReturnAdjustmentReasonsList.Hint);
			AssertEquals("MaxCodeLength", 3, ItemSet.TaxReturnAdjustmentReasonsList.DefaultValue.MaxCodeLength);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.TaxReturnAdjustmentReasonsList.Storage);

			var lookUpList = new CodeDescriptionPairList();
			lookUpList.AddPair("CLS", "Incorrect Classification");
			lookUpList.AddPair("DEL", "Values not known at the time of input into software");
			lookUpList.AddPair("IDE", "Incorrect Data Entry");

			AssertContainsExactElementsInAnyOrder("Default Value Codes", lookUpList.Cast<ICodeDescription>().Select(x => x.Code), ItemSet.TaxReturnAdjustmentReasonsList.DefaultValue.Cast<ICodeDescription>().Select(x => x.Code));
			AssertContainsExactElementsInAnyOrder("Value Codes", lookUpList.Cast<ICodeDescription>().Select(x => x.Code), ItemSet.TaxReturnAdjustmentReasonsList.Value.Cast<ICodeDescription>().Select(x => x.Code));

			lookUpList = new CodeDescriptionPairList();
			lookUpList.AddPair("ZZZ", "ZZZ Test");
			ItemSet.TaxReturnAdjustmentReasonsList.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, lookUpList);
			AssertContainsExactElementsInAnyOrder("Set Value Codes", lookUpList.Cast<ICodeDescription>().Select(x => x.Code), ItemSet.TaxReturnAdjustmentReasonsList.Value.Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestPercentageOfNetOutputs()
		{
			AssertEquals("DefaultValue", 1m, ItemSet.PercentageOfNetOutputs.DefaultValue);
			AssertEquals("Category", Categories.Accounting_UKMTDPreviousPeriodInclusionRules, ItemSet.PercentageOfNetOutputs.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.PercentageOfNetOutputs.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.PercentageOfNetOutputs.Options);
			AssertEquals("Caption", "Percentage of Net outputs (box 6)", ItemSet.PercentageOfNetOutputs.Caption);
			AssertEquals("Hint", "", ItemSet.PercentageOfNetOutputs.Hint);

			ItemSet.PercentageOfNetOutputs.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2m);
			AssertEquals("Value", 2m, ItemSet.PercentageOfNetOutputs.Value);
		}

		#endregion

		public void TestInvoiceDetentionDemurrageStatements()
		{
			var item = ItemSet.InvoiceDetentionDemurrageStatements;
			AssertEquals("Name", "InvoiceDetentionDemurrageStatements", item.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, item.Category);
			AssertEquals("Caption", "Detention/Demurrage Statement", item.Caption);
			AssertEquals("Hint", "The statements below are displayed in the Doc Strip \"Container Penalties\". Override the registry if you wish to change these statements.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.Company, item.Storage);

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				var expectedUsStatements = @"These charges are consistent with Federal Maritime Commission rules with respect to detention and demurrage as per ""Ocean Shipping Reform Act of 2022"", title 46 of the United States Code.
The common carrier's performance did not cause or contribute to the underlying invoiced charges.";
				AssertEquals(expectedUsStatements, ItemSet.InvoiceDetentionDemurrageStatements.GetValueWithoutFallback(company1.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals(expectedUsStatements, ItemSet.InvoiceDetentionDemurrageStatements.GetValueWithoutFallback(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals(string.Empty, ItemSet.InvoiceDetentionDemurrageStatements.GetValueWithoutFallback(company3.PK.ToGuid(), Guid.Empty, Guid.Empty));

				AssertEquals("Should query GlbCompany only once", 1, Db.Connection.ExecutedCommands.Count(x => x.Contains("GlbCompany")));
			}
		}

		#region Fiscal Tax Codes for Germany

		#region Output Net

		public void TestFiscalTaxCodeForOutputNetAmountList_DefaultValue()
		{
			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterDefaultProviderMock(Constants.CountryCodes.Germany, new FiscalOutputNetCodeList());

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				var item = ItemSet.FiscalTaxCodeForOutputNetAmountList;
				AssertEquals("Name", "FiscalTaxCodeForOutputNetAmountList", item.Name);
				AssertEquals("Category", Categories.Accounting_TaxConfigurations_Germany, item.Category);
				AssertEquals("Caption", "Fiscal Output Net Codes", item.Caption);
				AssertEquals("Hint", @"This is a list of Fiscal Tax Codes used in the Advanced Turnover Tax Return reporting for Germany. 
The Codes are equivalent to the Number Codes used for the net resp. base amounts for output tax on the paper form.", item.Hint);
				AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);

				var registryDefault = ItemSet.FiscalTaxCodeForOutputNetAmountList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("System > ONT (via CountryComplianceInfo)", new FiscalOutputNetCodeList(), registryDefault);
			}

			void CreateAndRegisterDefaultProviderMock(string countryCode, CodeDescriptionPairList defaultValueToUse)
			{
				var mock = new Mock<IFiscalTaxCodeProvider>();
				mock.Setup(x => x.GetFiscalTaxCodeForOutputNetAmount()).Returns(defaultValueToUse);

				mockComplianceFactory.Setup((x) => x.GetIFiscalTaxCodeProvider(It.Is<ZString>(c => c == countryCode))).Returns(mock.Object);
			}
		}

		#endregion

		#region Output Tax

		public void TestFiscalTaxCodeForOutputTaxAmountList_DefaultValue()
		{
			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterDefaultProviderMock(Constants.CountryCodes.Germany, new FiscalOutputTaxCodeList());

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				var item = ItemSet.FiscalTaxCodeForOutputTaxAmountList;
				AssertEquals("Name", "FiscalTaxCodeForOutputTaxAmountList", item.Name);
				AssertEquals("Category", Categories.Accounting_TaxConfigurations_Germany, item.Category);
				AssertEquals("Caption", "Fiscal Output Tax Codes", item.Caption);
				AssertEquals("Hint", @"This is a list of Fiscal Tax Codes used in the Advanced Turnover Tax Return reporting for Germany. 
The Codes are equivalent to the Number Codes used for the amounts for output tax on the paper form.", item.Hint);
				AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);

				var registryDefault = ItemSet.FiscalTaxCodeForOutputTaxAmountList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("System > OTX (via CountryComplianceInfo)", new FiscalOutputTaxCodeList(), registryDefault);
			}

			void CreateAndRegisterDefaultProviderMock(string countryCode, CodeDescriptionPairList defaultValueToUse)
			{
				var mock = new Mock<IFiscalTaxCodeProvider>();
				mock.Setup(x => x.GetFiscalTaxCodeForOutputTaxAmount()).Returns(defaultValueToUse);

				mockComplianceFactory.Setup((x) => x.GetIFiscalTaxCodeProvider(It.Is<ZString>(c => c == countryCode))).Returns(mock.Object);
			}
		}

		#endregion

		#region Input Tax

		public void TestFiscalTaxCodeForInputTaxAmountList_DefaultValue()
		{
			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterDefaultProviderMock(Constants.CountryCodes.Germany, new FiscalInputTaxCodeList());

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				var item = ItemSet.FiscalTaxCodeForInputTaxAmountList;
				AssertEquals("Name", "FiscalTaxCodeForInputTaxAmountList", item.Name);
				AssertEquals("Category", Categories.Accounting_TaxConfigurations_Germany, item.Category);
				AssertEquals("Caption", "Fiscal Input Tax Codes", item.Caption);
				AssertEquals("Hint", @"This is a list of Fiscal Tax Codes used in the Advanced Turnover Tax Return reporting for Germany. 
The Codes are equivalent to the Number Codes used for the amounts for input tax on the paper form.", item.Hint);
				AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);

				var registryDefault = ItemSet.FiscalTaxCodeForInputTaxAmountList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("System > ITX (via CountryComplianceInfo)", new FiscalInputTaxCodeList(), registryDefault);
			}

			void CreateAndRegisterDefaultProviderMock(string countryCode, CodeDescriptionPairList defaultValueToUse)
			{
				var mock = new Mock<IFiscalTaxCodeProvider>();
				mock.Setup(x => x.GetFiscalTaxCodeForInputTaxAmount()).Returns(defaultValueToUse);

				mockComplianceFactory.Setup((x) => x.GetIFiscalTaxCodeProvider(It.Is<ZString>(c => c == countryCode))).Returns(mock.Object);
			}
		}

		#endregion

		#region Valid Fiscal Tax Code Combinations

		public void TestValidFiscalTaxCodeCombinationsList()
		{
			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();

			var lookUpList = new CodeDescriptionWithThreeGroupsCollection(17);
			CreateAndRegisterDefaultProviderMock(Constants.CountryCodes.Germany, lookUpList);

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				var item = ItemSet.ValidFiscalTaxCodeCombinationsList;
				AssertEquals("Name", "ValidFiscalTaxCodeCombinationsList", item.Name);
				AssertEquals("Category", Categories.Accounting_TaxConfigurations_Germany, item.Category);
				AssertEquals("Caption", "Valid Fiscal Tax Code Combinations", item.Caption);
				AssertEquals("Hint", @"This is a list of Valid Fiscal Tax Code Combinations used to set-up the Advanced Turnover Tax Return reporting for Germany. 
The Combinations are required to identify under which Fiscal Tax Code (Number Code) a base amount for output tax, the output tax itself or the input tax has to be specified.
Sometimes both amounts, means the base amount for output tax as well as the output tax amount have to be reported.
For reverse charge taxation all three amounts are required, means the base amount for output tax, the output tax amount as well as the input tax amount which is equal to the output tax.

The Code must have a certain structure: FiscalTaxCodeOutputNet:SortOrder_FiscalTaxCodeOutputTax:SortOrder_FiscalTaxCodeInputTax:SortOrder
Each Fiscal Tax Code and the Sort Order both have 2 digits. In case the Fiscal Tax Code = NA then colon and sort order are omitted.
Example: 46:40_47:41_67:68 or 35:03_36:04_NA or NA_NA_66:65", item.Hint);
				AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);

				var defaultReg = ItemSet.ValidFiscalTaxCodeCombinationsList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

				AssertContainsExactElementsInAnyOrder("Tax Configurations > Germany > Valid Fiscal Tax Code Combinations (via CountryComplianceInfo)", lookUpList.GetCodeDescriptionPairList(), defaultReg.GetCodeDescriptionPairList());
			}

			void CreateAndRegisterDefaultProviderMock(string countryCode, CodeDescriptionWithThreeGroupsCollection defaultValueToUse)
			{
				defaultValueToUse.Add("81:01_NA_NA", FiscalOutputNetCodeList.Codes.ONT81, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("86:02_NA_NA", FiscalOutputNetCodeList.Codes.ONT86, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("35:03_36:04_NA", FiscalOutputNetCodeList.Codes.ONT35, FiscalOutputTaxCodeList.Codes.OTX36, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("77:05_NA_NA", FiscalOutputNetCodeList.Codes.ONT77, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("76:06_80:07_NA", FiscalOutputNetCodeList.Codes.ONT76, FiscalOutputTaxCodeList.Codes.OTX80, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("41:15_NA_NA", FiscalOutputNetCodeList.Codes.ONT41, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("44:16_NA_NA", FiscalOutputNetCodeList.Codes.ONT44, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("49:17_NA_NA", FiscalOutputNetCodeList.Codes.ONT49, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("43:18_NA_NA", FiscalOutputNetCodeList.Codes.ONT43, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("48:19_NA_NA", FiscalOutputNetCodeList.Codes.ONT48, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("91:25_NA_NA", FiscalOutputNetCodeList.Codes.ONT91, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("89:26_NA_NA", FiscalOutputNetCodeList.Codes.ONT89, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("93:27_NA_NA", FiscalOutputNetCodeList.Codes.ONT93, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("95:28_98:29_NA", FiscalOutputNetCodeList.Codes.ONT95, FiscalOutputTaxCodeList.Codes.OTX98, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("94:30_96:31_NA", FiscalOutputNetCodeList.Codes.ONT94, FiscalOutputTaxCodeList.Codes.OTX96, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("46:40_47:41_67:68", FiscalOutputNetCodeList.Codes.ONT46, FiscalOutputTaxCodeList.Codes.OTX47, FiscalInputTaxCodeList.Codes.ITX67);
				defaultValueToUse.Add("73:43_74:44_67:68", FiscalOutputNetCodeList.Codes.ONT73, FiscalOutputTaxCodeList.Codes.OTX74, FiscalInputTaxCodeList.Codes.ITX67);
				defaultValueToUse.Add("84:46_85:47_67:68", FiscalOutputNetCodeList.Codes.ONT84, FiscalOutputTaxCodeList.Codes.OTX85, FiscalInputTaxCodeList.Codes.ITX67);
				defaultValueToUse.Add("42:55_NA_NA", FiscalOutputNetCodeList.Codes.ONT42, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("60:56_NA_NA", FiscalOutputNetCodeList.Codes.ONT60, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("21:57_NA_NA", FiscalOutputNetCodeList.Codes.ONT21, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("45:58_NA_NA", FiscalOutputNetCodeList.Codes.ONT45, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
				defaultValueToUse.Add("NA_NA_66:65", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX66);
				defaultValueToUse.Add("NA_NA_61:66", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX61);
				defaultValueToUse.Add("NA_NA_62:67", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX62);
				defaultValueToUse.Add("NA_NA_63:69", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX63);
				defaultValueToUse.Add("NA_NA_59:70", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX59);
				defaultValueToUse.Add("NA_NA_64:71", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX64);
				defaultValueToUse.Add("NA_NA_65:72", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX65);
				defaultValueToUse.Add("NA_NA_69:73", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX69);
				defaultValueToUse.Add("NA_NA_NA", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);

				var mock = new Mock<IFiscalTaxCodeProvider>();
				mock.Setup(x => x.GetValidFiscalTaxCodeCombinations(AccountingConfigurationRegistry.Instance.FiscalTaxCodeForOutputNetAmountList.Value,
																	AccountingConfigurationRegistry.Instance.FiscalTaxCodeForOutputTaxAmountList.Value,
																	AccountingConfigurationRegistry.Instance.FiscalTaxCodeForInputTaxAmountList.Value)).Returns(defaultValueToUse);

				mockComplianceFactory.Setup((x) => x.GetIFiscalTaxCodeProvider(It.Is<ZString>(c => c == countryCode))).Returns(mock.Object);
			}
		}

		#endregion

		#region Tax Message ID Mapping

		public void TestTaxMessageIdMappingList()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTaxMsg("ABC", "ABC Tax message", "", "", countryCode: Constants.CountryCodes.Germany);
			creator.CreateTaxMsg("DEF", "DEF Tax message", "", "", countryCode: Constants.CountryCodes.Germany);
			creator.CreateTaxMsg("GHJ", "GHJ Tax message", "", "", countryCode: Constants.CountryCodes.Germany);
			Factory.Save();

			var expectedValue = new CodeDescriptionWithGroupCollection();

			expectedValue.Add("ABC", (NoResString)"ABC Tax message", "NA_NA_NA");
			expectedValue.Add("DEF", (NoResString)"DEF Tax message", "NA_NA_NA");
			expectedValue.Add("GHJ", (NoResString)"GHJ Tax message", "NA_NA_NA");

			var item = ItemSet.TaxMessageIdMappingList;
			AssertEquals("Name", "TaxMessageIdMappingList", item.Name);
			AssertEquals("Category", Categories.Accounting_TaxConfigurations_Germany, item.Category);
			AssertEquals("Caption", "Tax Message ID – Fiscal Tax Code Combinations Mapping", item.Caption);
			AssertEquals("Hint", @"This is a mapping between Tax Message IDs and Valid Fiscal Tax Code Combinations.
A Tax Message ID is set for all relevant transaction lines.
This mapping is used to identify under which Fiscal Tax Code (Number Code) a base amount for output tax, the output tax itself or the input tax has to be specified in Advanced Turnover Tax Return reporting in Germany.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			Assert("IncludeMissingDefaults", item.IncludeMissingDefaults);
			Assert("RemoveNonDefaults", item.RemoveNonDefaults);

			var defaultReg = ItemSet.TaxMessageIdMappingList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertContainsExactElementsInAnyOrder("Tax Configurations > Germany > Tax Message ID – Fiscal Tax Code Combinations Mapping (via CountryComplianceInfo)", expectedValue.GetCodeDescriptionPairList(), defaultReg.GetCodeDescriptionPairList());
			defaultReg?.ToList().ForEach(x => AssertEquals("Default Group for Tax Message ID Mapping must be equal to NA_NA_NA.", "NA_NA_NA", ((CodeDescriptionWithGroup)x).Group));
		}

		#endregion

		#endregion

		#region Stamp Duty

		public void TestStampDutyInvoiceTaxMessage()
		{
			AssertEquals("Name", "StampDutyInvoiceTaxMessage", ItemSet.StampDutyInvoiceTaxMessage.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.StampDutyInvoiceTaxMessage.Category);
			AssertEquals("Caption", "Stamp Duty Invoice Tax Message", ItemSet.StampDutyInvoiceTaxMessage.Caption);
			AssertEquals("Hint", @$"{BrandingFactory.Instance.ProductName} will record the Invoice Tax Message Code nominated in this registry against each Stamp Duty Charge Line automatically defaulted by {BrandingFactory.Instance.ProductName} into an AR Invoice at the point of posting.
Note:  Currently this behavior is only enabled for Italy Login Companies.", ItemSet.StampDutyInvoiceTaxMessage.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.StampDutyInvoiceTaxMessage.Storage);
			AssertEquals("Default Value", Guid.Empty, ItemSet.StampDutyInvoiceTaxMessage.DefaultValue);
		}

		public void TestStampDutyARDocumentMessage()
		{
			AssertEquals("Name", "StampDutyARDocumentMessage", ItemSet.StampDutyARDocumentMessage.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.StampDutyARDocumentMessage.Category);
			AssertEquals("Caption", "Stamp Duty AR Document Message", ItemSet.StampDutyARDocumentMessage.Caption);
			AssertEquals("Hint", @$"{BrandingFactory.Instance.ProductName} will print this message in all Receivables Invoice or Receivables Credit Note documents WHEN the details of the transaction itself incurs a liability of the Login Company to pay a document stamp duty.
Note: Please remember to configure your system to clearly identify AR Document copies.
Note: Currently this behavior is only enabled for Italy Login Companies.", ItemSet.StampDutyARDocumentMessage.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.StampDutyARDocumentMessage.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.StampDutyARDocumentMessage.Options);
			AssertEquals("Default Value", ZString.Empty, ItemSet.StampDutyARDocumentMessage.DefaultValue);
		}

		public void TestStampDutyChargeCode()
		{
			AssertEquals("Name", "StampDutyChargeCode", ItemSet.StampDutyChargeCode.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.StampDutyChargeCode.Category);
			AssertEquals("Caption", "Stamp Duty Charge Code", ItemSet.StampDutyChargeCode.Caption);
			AssertEquals("Hint", $"{BrandingFactory.Instance.ProductName} will use this charge code when posting invoices that attract stamp duty.  The remainder of the Stamp Duty configuration must be setup by {BrandingFactory.Instance.ProductSupportName}.", ItemSet.StampDutyChargeCode.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.StampDutyChargeCode.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, ItemSet.StampDutyChargeCode.Options);
			AssertEquals("Default Value", Guid.Empty, ItemSet.StampDutyChargeCode.DefaultValue);
		}

		public void TestTaxIDsAttractingStampDuty()
		{
			AssertEquals("Name", "TaxIDsAttractingStampDuty", ItemSet.TaxIDsAttractingStampDuty.Name);
			AssertEquals("Category", "Accounting/Tax Configurations", ItemSet.TaxIDsAttractingStampDuty.Category);
			AssertEquals("Caption", "Tax IDs Attracting Stamp Duty (CargoWiseOne Support Only)", ItemSet.TaxIDsAttractingStampDuty.Caption);
			AssertEquals("Hint", "When Tax IDs Attracting Stamp Duty are present on an AR invoice and the local ex tax amount of these lines exceeds the Stamp Duty Threshold, the Stamp Duty Fixed Amount will be applied to the invoice.", ItemSet.TaxIDsAttractingStampDuty.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.TaxIDsAttractingStampDuty.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.TaxIDsAttractingStampDuty.Options);
			AssertEquals("Default Value", Guid.Empty.ToString(), ItemSet.TaxIDsAttractingStampDuty.DefaultValue);
		}

		public void TestStampDutyFixedAmount()
		{
			AssertEquals("Name", "StampDutyFixedAmount", ItemSet.StampDutyFixedAmount.Name);
			AssertEquals("Category", "Accounting/Tax Configurations", ItemSet.StampDutyFixedAmount.Category);
			AssertEquals("Caption", "Stamp Duty Fixed Amount (CargoWiseOne Support Only)", ItemSet.StampDutyFixedAmount.Caption);
			AssertEquals("Hint", "The Stamp Duty Fixed Amount will be applied to AR Invoices, when Tax IDs Attracting Stamp Duty are present on the invoice and the local ex tax amount of these lines exceeds the Stamp Duty Threshold.", ItemSet.StampDutyFixedAmount.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.StampDutyFixedAmount.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.StampDutyFixedAmount.Options);
			AssertEquals("Default Value", 0m, ItemSet.StampDutyFixedAmount.DefaultValue);
		}

		public void TestStampDutyThreshold()
		{
			AssertEquals("Name", "StampDutyThreshold", ItemSet.StampDutyThreshold.Name);
			AssertEquals("Category", "Accounting/Tax Configurations", ItemSet.StampDutyThreshold.Category);
			AssertEquals("Caption", "Stamp Duty Threshold (CargoWiseOne Support Only)", ItemSet.StampDutyThreshold.Caption);
			AssertEquals("Hint", "If the local ex tax amount on an invoice exceeds the Stamp Duty Threshold and Tax IDs Attracting Stamp Duty are present on these lines, the Stamp Duty Fixed Amount will be applied to the AR invoice.", ItemSet.StampDutyThreshold.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.StampDutyThreshold.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.StampDutyThreshold.Options);
			AssertEquals("Default Value", 0m, ItemSet.StampDutyThreshold.DefaultValue);
		}

		#endregion

		#region Test ReferenceDisplayRegistryItems

		public void TestReferenceDisplayRegistryItems()
		{
			TestReferenceDisplayRegistryItems(
				ItemSet.ConsolsSendAndReceivingAgents,
				ItemSet.ConsolsCarrierAndTransportDetails,
				ItemSet.ConsolsBillWeightVolumeAndPackages,
				ItemSet.ConsolsLoadAndDischargePorts,
				ItemSet.ConsolsContainerDetails,

				ItemSet.CustomsDeclarationsSupplierAndImporter,
				ItemSet.CustomsDeclarationsGoodsDescriptionAndInvoiceReferences,
				ItemSet.CustomsDeclarationsOrderReferenceWeightVolumePackages,
				ItemSet.CustomsDeclarationsTransportDetailsAndBillReferences,
				ItemSet.CustomsDeclarationsOriginAndDestinationPorts,
				ItemSet.CustomsDeclarationsContainerDetails,

				ItemSet.LoadListClientReferenceAndCarrier,
				ItemSet.LoadListTransportDetails,
				ItemSet.LoadListContainerDetails,

				ItemSet.ShipmentsAndGatepassConsigneeAndConsignor,
				ItemSet.ShipmentsAndGatepassGoodsDescriptionAndInterimReceipt,
				ItemSet.ShipmentsAndGatepassTransportDetailsWeightVolumePackages,
				ItemSet.ShipmentsAndGatepassOriginAndDestinationPorts,
				ItemSet.ShipmentsAndGatepassBillAndClientReferences
			);
		}

		void TestReferenceDisplayRegistryItems(params BooleanRegistryItem[] registryItems)
		{
			foreach (BooleanRegistryItem registryItem in registryItems)
			{
				AssertEquals(registryItem.Name + ".Hint", "Shipment/Job Details to display in the AR Invoice.", registryItem.Hint);
				AssertEquals(registryItem.Name + ".Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, registryItem.Storage);
				AssertEquals(registryItem.Name + ".DefaultValue", true, registryItem.DefaultValue);

				registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals(registryItem.Name + ".Value", false, registryItem.Value);

				registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AssertEquals(registryItem.Name + ".Value", true, registryItem.Value);
			}
		}

		#endregion

		#region DirectDebitFileCreationURLs

		public void TestDirectDebitFileCreationURLs()
		{
			BusinessObject bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount[AccBankAccountSchema.AB_GC] = Env.CurrentCompany.PK;
			bankAccount[AccBankAccountSchema.AB_AllowAutoDDR] = true;
			bankAccount[AccBankAccountSchema.AB_AutoDDRFormat] = "NAB";
			bankAccount[AccBankAccountSchema.AB_IsActive] = true;

			Factory.Save();

			DirectDebitFileCreationURLCollection list = new DirectDebitFileCreationURLCollection();
			DirectDebitFileCreationURL setting = list.AddNew();
			setting.BankAccountPK = bankAccount.PK;
			setting.BankWebsite = "http://abc.com";

			ItemSet.DirectDebitFileCreationURLs.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, list);

			DirectDebitFileCreationURLCollection registryValue = ItemSet.DirectDebitFileCreationURLs.Value;
			AssertEquals("no of elements on registry", 1, registryValue.Count);
			AssertEquals("Bank account PK", bankAccount.PK, registryValue[0].BankAccountPK);
			AssertEquals("Bank 's website", "http://abc.com", registryValue[0].BankWebsite);
		}

		#endregion

		public void TestGLJournalAdjustmentCategoriesList()
		{
			AssertEquals("Name", "GLJournalAdjustmentCategoriesList", ItemSet.GLPresentationJournalCategoriesList.Name);
			AssertEquals("Category", Categories.Accounting_GeneralLedgerDefaults, ItemSet.GLPresentationJournalCategoriesList.Category);
			AssertEquals("Caption", "GL Presentation Journal Categories", ItemSet.GLPresentationJournalCategoriesList.Caption);
			AssertEquals("Hint", @"When overridden, the category codes added / listed here are available for use on General Ledger Journals.

By default, journals assigned a category are ignored by all General Ledger Reports. They are only included in the output of a report when expressly included.  These categories are used for the preparation and presentation of special purpose general ledger reports without affecting the standard financial reports.

Note:
1.	‘Elimination’ category is used in the GL Consolidations module to generate elimination journals. 
2.	‘Parent Code’ is used for grouping presentation categories. Each presentation category can only be assigned to a single parent code. During the generation of the reports, you will have the option to include presentation journals for a parent category and it’s child OR a specific category only. 
3.	‘Closing’ category is used to identify period end closing journals. At this stage, this is used in the generation of the GB-T 24589.1 Data Interface in China environment. All GL presentation journals posted against this category will be included in the accounting voucher and GL account balance sections of the XML file.", ItemSet.GLPresentationJournalCategoriesList.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.GLPresentationJournalCategoriesList.Storage);

			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "ABC";
			category.Description = (NoResString)"ABC Desc";
			category.Bool = true; // Active
			category.Bool2 = false; // Elimination
			category.Bool3 = false; // Elimination
			var category1 = list.AddNew();
			category1.Code = "DEF";
			category1.ParentCode = category.Code;
			category1.Description = (NoResString)"DEF Desc";
			category1.Bool = true; // Active
			category1.Bool2 = false; // Elimination
			category1.Bool3 = false; // Elimination
			ItemSet.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			list = ItemSet.GLPresentationJournalCategoriesList.Value;
			AssertNotNull(list);
			AssertEquals("Count", 2, list.Count);
			var categoryTest = list.ToArray<GLPresentationJournalCategory>().FirstOrDefault(x => x.Code == "ABC");
			var categoryTest1 = list.ToArray<GLPresentationJournalCategory>().FirstOrDefault(x => x.Code != "ABC");
			AssertEquals("Code", "ABC", categoryTest.Code);
			AssertEquals("Code", "", categoryTest.ParentCode);
			AssertEquals("Description", "ABC Desc", categoryTest.Description);
			AssertEquals("Bool", true, categoryTest.Bool);
			AssertEquals("Bool2", false, categoryTest.Bool2);
			AssertEquals("Bool3", false, categoryTest.Bool3);
			AssertEquals("Code", "DEF", categoryTest1.Code);
			AssertEquals("Code", "ABC", categoryTest1.ParentCode);
			AssertEquals("Description", "DEF Desc", categoryTest1.Description);
			AssertEquals("Bool", true, categoryTest1.Bool);
			AssertEquals("Bool2", false, categoryTest1.Bool2);
			AssertEquals("Bool3", false, categoryTest1.Bool3);
		}

		public void TestReasonCodesListForRejectOrders()
		{
			var item = ItemSet.OrderRejectReasonCodesList;
			AssertEquals("Name", "ReasonCodesListForRejectOrders", item.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders, item.Category);
			AssertEquals("Caption", "Collection Order Rejection Reason Codes", item.Caption);
			AssertEquals("Hint", @"Collection Order Rejection Reason Codes.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
		}

		public void TestBadDebtWriteOffAccount()
		{
			AssertNotNull("BadDebtWriteOffAccount", ItemSet.BadDebtWriteOffAccount);
			string defaultValue = "3980.00.00";
			if (HasDefaultValueFromBaseData(ItemSet.BadDebtWriteOffAccount.Name, defaultValue))
			{
				AssertEquals("BadDebtWriteOffAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum((Guid)ItemSet.BadDebtWriteOffAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("BadDebtWriteOffAccount.Value", newGuid, ItemSet.BadDebtWriteOffAccount.Value);
		}

		public void TestAPMatchingSessionControlAccount()
		{
			Guid newGuid = Guid.NewGuid();
			ItemSet.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("MatchingSessionControlAccount.Value", newGuid, ItemSet.APMatchingSessionControlAccount.Value);
		}

		public void TestARMatchingSessionControlAccount()
		{
			Guid newGuid = Guid.NewGuid();
			ItemSet.ARMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("MatchingSessionControlAccount.Value", newGuid, ItemSet.ARMatchingSessionControlAccount.Value);
		}

		public void TestAccGovtComplianceDocumentExchangeRateDate()
		{
			AssertEquals("Caption", "Compliance Document Exchange Rate Date (CargoWiseOne Support Only)", ItemSet.AccGovtComplianceDocumentExchangeRateDate.Caption);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_AccountingNextNumbers + "/Govt Tax Invoice Reference", ItemSet.AccGovtComplianceDocumentExchangeRateDate.Category);
			AssertEquals("DefaultValue", "PST", ItemSet.AccGovtComplianceDocumentExchangeRateDate.DefaultValue);
			AssertEquals("Hint", @"This registry is used when the Login Company's local currency is NOT the local currency of Login Country AND a Government Compliance Document must be printed in the country's local currency.
This registry defines which date on a transaction will be used when selecting the exchange rate when converting transaction values to the mandated Compliance Document's currency.
This registry will only be required when neither OS Currency or Local Currency on the transaction is the Country's local currency.", ItemSet.AccGovtComplianceDocumentExchangeRateDate.Hint);
			AssertEquals("Name", "AccGovtComplianceDocumentExchangeRateDate", ItemSet.AccGovtComplianceDocumentExchangeRateDate.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AccGovtComplianceDocumentExchangeRateDate.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.AccGovtComplianceDocumentExchangeRateDate.Storage);
		}

		public void TestAccGovtComplianceDocumentExchangeRateType()
		{
			AssertEquals("Caption", "Compliance Document Exchange Rate Type (CargoWiseOne Support Only)", ItemSet.AccGovtComplianceDocumentExchangeRateType.Caption);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_AccountingNextNumbers + "/Govt Tax Invoice Reference", ItemSet.AccGovtComplianceDocumentExchangeRateType.Category);
			AssertEquals("DefaultValue", "SEL", ItemSet.AccGovtComplianceDocumentExchangeRateType.DefaultValue);
			AssertEquals("Hint", @"This registry is used when the Login Company's local currency is NOT the local currency of Login Country AND a Government Compliance Document must be printed in the country's local currency.
This registry defines which exchange rate type will be used to convert transaction values to the mandated Compliance Document's currency.
This registry will only be required when neither OS Currency or Local Currency on the transaction is the Country's local currency.", ItemSet.AccGovtComplianceDocumentExchangeRateType.Hint);
			AssertEquals("Name", "AccGovtComplianceDocumentExchangeRateType", ItemSet.AccGovtComplianceDocumentExchangeRateType.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AccGovtComplianceDocumentExchangeRateType.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.AccGovtComplianceDocumentExchangeRateType.Storage);
		}

		public void TestAccNextInvoice()
		{
			ItemSet.AccNextInvoice.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextInvoice", 13, ItemSet.AccNextInvoice.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextCreditNote()
		{
			ItemSet.AccNextCreditNote.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextCreditNote", 13, ItemSet.AccNextCreditNote.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextAdjNote()
		{
			ItemSet.AccNextAdjNote.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextAdjNote", 13, ItemSet.AccNextAdjNote.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextJournal()
		{
			ItemSet.AccNextJournal.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextJournal", 13, ItemSet.AccNextJournal.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextTransfer()
		{
			ItemSet.AccNextTransfer.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextTransfer", 13, ItemSet.AccNextTransfer.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextContra()
		{
			ItemSet.AccNextContra.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextContra", 13, ItemSet.AccNextContra.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextReceipt()
		{
			ItemSet.AccNextReceipt.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextReceipt", 13, ItemSet.AccNextReceipt.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextPayment()
		{
			ItemSet.AccNextPayment.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextPayment", 13, ItemSet.AccNextPayment.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextOverpayment()
		{
			ItemSet.AccNextOverpayment.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextOverpayment", 13, ItemSet.AccNextOverpayment.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextDiscount()
		{
			ItemSet.AccNextDiscount.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextDiscount", 13, ItemSet.AccNextDiscount.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextExchangeDiff()
		{
			ItemSet.AccNextExchangeDiff.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextExchangeDiff", 13, ItemSet.AccNextExchangeDiff.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAccNextBankBatch()
		{
			ItemSet.AccNextBankBatch.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 13);
			AssertEquals("AccNextBankBatch", 13, ItemSet.AccNextBankBatch.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestDebtorCreditLimitNotifyGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.DebtorCreditLimitNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("DebtorCreditLimitNotifyGroup", testGroup, ItemSet.DebtorCreditLimitNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestCreditorCreditLimitNotifyGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.CreditorCreditLimitNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("CreditorCreditLimitNotifyGroup", testGroup, ItemSet.CreditorCreditLimitNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestJobReopenNotifyGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.JobReopenNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("JobReopenNotifyGroup", testGroup, ItemSet.JobReopenNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestTransactionReverseNotifyGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.TransactionReverseNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("TransactionReverseNotifyGroup", testGroup, ItemSet.TransactionReverseNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestTransactionUnmatchedNotifyGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.TransactionUnmatchedNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("TransactionUnmatchedNotifyGroup", testGroup, ItemSet.TransactionUnmatchedNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestIntercompanyTransactionsImportNotifyGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.IntercompanyTransactionsImportNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("IntercompanyTransactionsImportNotifyGroup", testGroup, ItemSet.IntercompanyTransactionsImportNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
			AssertEquals("IntercompanyTransactionsImportNotifyGroup Hint", "Notify Party in the event of an intercompany invoice failing to import. The email will advise transaction details and summary of the error encountered. Note: This is only applicable to an intercompany invoice import failure via the 'ISI' workflow trigger.", ItemSet.IntercompanyTransactionsImportNotifyGroup.Hint);
		}

		public void TestRevenueRecognitionNotificationGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.RevenueRecognitionNotificationGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("RevenueRecognitionNotificationGroup", testGroup, ItemSet.RevenueRecognitionNotificationGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestJobPostingNotificationGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.JobPostingNotificationGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("JobPostingNotificationGroup", testGroup, ItemSet.JobPostingNotificationGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestARCreditControlledDocumentsApprovalNotifyGroup()
		{
			var testGroup = Guid.NewGuid();
			AssertEquals("Default value", Guid.Empty, ItemSet.ARCreditControlledDocumentsApprovalNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
			ItemSet.ARCreditControlledDocumentsApprovalNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("Set value", testGroup, ItemSet.ARCreditControlledDocumentsApprovalNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestInvoiceDateIncrementingSuspensionNotifyGroup()
		{
			var testGroup = Guid.NewGuid();
			AssertEquals("Default value", Guid.Empty, ItemSet.InvoiceDateIncrementingSuspensionNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));
			ItemSet.InvoiceDateIncrementingSuspensionNotifyGroup.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("Set value", testGroup, ItemSet.InvoiceDateIncrementingSuspensionNotifyGroup.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));
		}

		[ExpectNoExceptions]
		public void TestReceivableAllowUserToModifyGSTId()
		{
			bool value = ItemSet.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
		}

		public void TestAmendingTransactionLocalTotalBehavior()
		{
			TestRegistryItem(
				ItemSet.AmendingTransactionLocalTotalBehavior,
				"AmendingTransactionLocalTotalBehavior",
				"Accounting/Receivable Defaults/Default Settings",
				"Amending Transaction Local Total Behavior",
				@"By default, this registry will be set to 'No' and users will be warned when creating an amending transaction that will, in Local Total Inclusive of Tax, credit the customer more than what was originally billed.
When this registry is set to 'Yes', users will be prevented from posting an amending transaction that will result in the Receivables Organization being credited more than what was originally invoiced.

This registry is relevant when posting Amending Receivable Transactions.
The Local Currency Equivalent Value of all amounts (including tax) recorded in the Amending Transaction, its Parent Transaction and any related Amending Transactions already posted are all taken into account.
A Warning Message or Error Message (determined by the No/YES setting of this registry) will triggered when a user attempts to post an Amending Transaction that will produce a Net result of crediting the Receivables Organization more than what was invoiced in the related transaction/s.

Note:  
This registry is relevant to the creation of Amending Invoice and Amending Credit Note transactions.
It does NOT affect the creation of standalone Credit Notes. 
It is a company level registry configuration.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);

			AssertEquals("Default Value", false, ItemSet.AmendingTransactionLocalTotalBehavior.DefaultValue);
			ItemSet.AmendingTransactionLocalTotalBehavior.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AmendingTransactionLocalTotalBehavior.Value);
			ItemSet.AmendingTransactionLocalTotalBehavior.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AmendingTransactionLocalTotalBehavior.Value);
		}

		public void TestAmendingTransactionTaxBehavior()
		{
			TestRegistryItem(
				ItemSet.AmendingTransactionTaxBehavior,
				"AmendingTransactionTaxBehavior",
				"Accounting/Receivable Defaults/Default Settings",
				"Amending Transaction Tax Behavior",
				@"When set to YES, users will be prevented from posting an amending transaction that will result in the Receivables organization being credited more tax than was originally invoiced.
When set to NO, users will be warned when creating an amending transaction that will, in total, credit the customer more tax than was originally billed.

This registry is relevant when posting Amending Receivables Transactions.
The Local Currency Equivalent Value of all Tax Amounts recorded in the Amending Transaction, its Parent Transaction and any related Amending Transactions already posted are all taken into account.
A Warning Message or Error Message (determined by the NO/YES setting of this registry) will be triggered when a user attempts to post an Amending Transaction that will produce a Net Tax result of crediting the Receivables Organization more tax than was  invoiced in the related transaction/s.

Note:  
This registry is relevant to the creation of Amending Invoice and Amending Credit Note transactions.
It does NOT affect the creation of standalone Credit Notes. 
It is a company level registry configuration.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);

			AssertEquals("Default Value", false, ItemSet.AmendingTransactionTaxBehavior.DefaultValue);
			ItemSet.AmendingTransactionTaxBehavior.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AmendingTransactionTaxBehavior.Value);
			ItemSet.AmendingTransactionTaxBehavior.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AmendingTransactionTaxBehavior.Value);

			RefCountry[] countries = Factory.Load<RefCountry>(new ZQuery());
			foreach (RefCountry country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.RN_Code))
				{
					bool result = false;

					switch (country.RN_Code)
					{
						case Core.Constants.CountryCodes.Chile:
							result = true;
							break;
					}
					AssertEquals("Default Value", result, ItemSet.AmendingTransactionTaxBehavior.DefaultValue);
				}
			}
		}

		public void TestAmendingTransactionLocalTotalBehaviorForAP()
		{
			TestRegistryItem(
				ItemSet.AmendingTransactionLocalTotalBehaviorForAP,
				"AmendingTransactionLocalTotalBehaviorForAP",
				"Accounting/Payable Defaults/Default Settings",
				"Amending AP Transaction Local Total Behavior",
				@"This registry controls the ability to post amending Payables transactions where Local Total Amount (including tax) exceed the total amount owed in the original transaction.
By default, this registry is set to “No”.
This means the system will display a warning when creating an amending Payables transaction where the total local amount (including tax) exceeds the original amount owed but can still be posted.
When overridden to “Yes”, the system will prevent posting such transactions.
NOTE:
This function applies to Amending AP Invoices and AP Credit Notes (which are linked to parent).
When an amending Payables transaction debits more than what is owed, the system will recognize the Payables Organization as in debt.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);

			AssertEquals("Default Value", false, ItemSet.AmendingTransactionLocalTotalBehaviorForAP.DefaultValue);
			ItemSet.AmendingTransactionLocalTotalBehaviorForAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AmendingTransactionLocalTotalBehaviorForAP.Value);
			ItemSet.AmendingTransactionLocalTotalBehaviorForAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AmendingTransactionLocalTotalBehaviorForAP.Value);
		}

		public void TestAmendingTransactionTaxBehaviorForAP()
		{
			TestRegistryItem(
				ItemSet.AmendingTransactionTaxBehaviorForAP,
				"AmendingTransactionTaxBehaviorForAP",
				"Accounting/Payable Defaults/Default Settings",
				"Amending AP Transaction Tax Behavior",
				@"This registry controls the ability to post amending Payables transactions where Tax amount exceed the total tax owed in the original transaction.
By default, this registry is set to “No”.
This means the system will display a warning when creating an amending Payables transaction where the tax amount exceeds the original tax owed but can still be posted.
When overridden to “Yes”, the system will prevent posting such transactions.
NOTE:
This function applies to Amending AP Invoices and AP Credit Notes (which are linked to parent).
When an amending Payables transaction debits more tax than what is owed, the system will recognize the Payables Organization as in debt.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);

			AssertEquals("Default Value", false, ItemSet.AmendingTransactionTaxBehaviorForAP.DefaultValue);
			ItemSet.AmendingTransactionTaxBehaviorForAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.AmendingTransactionTaxBehaviorForAP.Value);
			ItemSet.AmendingTransactionTaxBehaviorForAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AmendingTransactionTaxBehaviorForAP.Value);

			RefCountry[] countries = Factory.Load<RefCountry>(new ZQuery());
			foreach (RefCountry country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.RN_Code))
				{
					bool result = false;

					switch (country.RN_Code)
					{
						case Core.Constants.CountryCodes.Chile:
							result = true;
							break;
					}
					AssertEquals("Default Value", result, ItemSet.AmendingTransactionTaxBehaviorForAP.DefaultValue);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestAllowModifyInvoiceTerm()
		{
			bool value = ItemSet.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
		}

		public void TestProFormaNonTaxAdjustmentNoteTitle()
		{
			object value = ItemSet.ProFormaNonTaxAdjustmentNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaNonTaxAdjustmentNoteTitle", value is MultilingualString);
		}

		public void TestProFormaNonTaxCreditNoteTitle()
		{
			object value = ItemSet.ProFormaNonTaxCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaNonTaxCreditNoteTitle", value is MultilingualString);
		}

		public void TestProFormaNonTaxDisbursementCreditNoteTitle()
		{
			object value = ItemSet.ProFormaNonTaxDisbursementCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaNonTaxDisbursementCreditNoteTitle", value is MultilingualString);
		}

		public void TestProFormaNonTaxDisbursementInvoiceTitle()
		{
			object value = ItemSet.ProFormaNonTaxDisbursementInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaNonTaxDisbursementInvoiceTitle", value is MultilingualString);
		}

		public void TestProFormaNonTaxInvoiceTitle()
		{
			object value = ItemSet.ProFormaNonTaxInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaNonTaxInvoiceTitle", value is MultilingualString);
		}

		public void TestProFormaTaxAdjustmentNoteTitle()
		{
			object value = ItemSet.ProFormaTaxAdjustmentNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaTaxAdjustmentNoteTitle", value is MultilingualString);
		}

		public void TestProFormaTaxCreditNoteTitle()
		{
			object value = ItemSet.ProFormaTaxCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaTaxCreditNoteTitle", value is MultilingualString);
		}

		public void TestProFormaTaxDisbursementCreditNoteTitle()
		{
			object value = ItemSet.ProFormaTaxDisbursementCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaTaxDisbursementCreditNoteTitle", value is MultilingualString);
		}

		public void TestProFormaTaxDisbursementInvoiceTitle()
		{
			object value = ItemSet.ProFormaTaxDisbursementInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaTaxDisbursementInvoiceTitle", value is MultilingualString);
		}

		public void TestProFormaTaxInvoiceTitle()
		{
			var value = ItemSet.ProFormaTaxInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK) as MultilingualString;
			AssertNotNull("ProFormaTaxInvoiceTitle", value);
			using (var grmMockResourceStrings = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				grmMockResourceStrings.Put("9ecf3cc5-af05-4897-bdab-04f545a5427f", new ResourceStringData("9ecf3cc5-af05-4897-bdab-04f545a5427f", string.Empty, string.Empty, "{0} AMROF ORP", string.Empty));
				grmMockResourceStrings.Put("bafb0efb-e9db-4197-ad02-4609c9ba7447", new ResourceStringData("bafb0efb-e9db-4197-ad02-4609c9ba7447", string.Empty, string.Empty, "ECIOVNI XAT", string.Empty));

				AssertEquals("PRO FORMA TAX INVOICE", value.ToString());
				AssertEquals("PRO FORMA TAX INVOICE", value.ToString(Res.DefaultLanguage));
				AssertEquals("ECIOVNI XAT AMROF ORP", value.ToString(Core.SharedConstants.Languages.German));
			}
		}

		public void TestProFormaAdjustmentNoteMessage()
		{
			object value = ItemSet.ProFormaAdjustmentNoteMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaAdjustmentNoteMessage", value is MultilingualString);
		}

		public void TestProFormaCreditNoteMessage()
		{
			object value = ItemSet.ProFormaCreditNoteMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaCreditNoteMessage", value is MultilingualString);
		}

		public void TestProFormaDisbursementMessage()
		{
			object value = ItemSet.ProFormaDisbursementMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaDisbursementMessage", value is MultilingualString);
		}

		public void TestProFormaInvoiceMessage()
		{
			object value = ItemSet.ProFormaInvoiceMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ProFormaInvoiceMessage", value is MultilingualString);
		}

		public void TestNonTaxInvoiceTitle()
		{
			object value = ItemSet.NonTaxInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("NonTaxInvoiceTitle", value is MultilingualString);
		}

		public void TestTaxInvoiceTitle()
		{
			object value = ItemSet.TaxInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("TaxInvoiceTitle", value is MultilingualString);
		}

		public void TestNonTaxCreditNoteTitle()
		{
			object value = ItemSet.NonTaxCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("NonTaxCreditNoteTitle", value is MultilingualString);
		}

		public void TestTaxCreditNoteTitle()
		{
			object value = ItemSet.TaxCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("TaxCreditNoteTitle", value is MultilingualString);
		}

		public void TestNonTaxAdjustmentNoteTitle()
		{
			object value = ItemSet.NonTaxAdjustmentNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("NonTaxAdjustmentNoteTitle", value is MultilingualString);
		}

		public void TestTaxAdjustmentNoteTitle()
		{
			object value = ItemSet.TaxAdjustmentNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("TaxAdjustmentNoteTitle", value is MultilingualString);
		}

		public void TestNonTaxDisbursementInvoiceTitle()
		{
			object value = ItemSet.NonTaxDisbursementInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("NonTaxDisbursementInvoiceTitle", value is MultilingualString);
		}

		public void TestNonTaxDisbursementCreditNoteTitle()
		{
			object value = ItemSet.NonTaxDisbursementCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("NonTaxDisbursementCreditNoteTitle", value is MultilingualString);
		}

		public void TestTaxDisbursementCreditNoteTitle()
		{
			object value = ItemSet.TaxDisbursementCreditNoteTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("TaxDisbursementCreditNoteTitle", value is MultilingualString);
		}

		public void TestTaxDisbursementInvoiceTitle()
		{
			object value = ItemSet.TaxDisbursementInvoiceTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("TaxDisbursementInvoiceTitle", value is MultilingualString);
		}

		public void TestAmendmentAndReversalRegistryTitleDefaultValueInheritedFromParentRegistry()
		{
			var fallbackPks = Factory.Load<GlbCompany>(new ZQuery()).Select(x => x.PK.ToGuid()).ToList();
			fallbackPks.Add(Guid.Empty);

			//To test a default registry value (when registry is not overriden)
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newCompanyPkList = new List<Guid>();
			newCompanyPkList.Add(newCompany.PK.ToGuid());
			Factory.Save();

			SetUpParentRegistryAndCustomizableDataResourceString(fallbackPks, ItemSet.TaxInvoiceTitle, "Test DP Tax Invoice", "세금계산서");
			IRegistryItem[] childrenRegistryItems = new IRegistryItem[] { ItemSet.TaxInvoiceAmendmentTitle, ItemSet.TaxInvoiceReversalTitle };
			AssertRegistryValuesAndTranslableData(fallbackPks, childrenRegistryItems, "Test DP Tax Invoice", "세금계산서");
			AssertRegistryValuesAndTranslableData(newCompanyPkList, childrenRegistryItems, "Test DP Tax Invoice", "세금계산서");

			SetUpParentRegistryAndCustomizableDataResourceString(fallbackPks, ItemSet.TaxCreditNoteTitle, "Test DP Tax Credit Note", "세금계산서");
			childrenRegistryItems = new IRegistryItem[] { ItemSet.TaxCreditNoteAmendmentTitle, ItemSet.TaxCreditNoteReversalTitle };
			AssertRegistryValuesAndTranslableData(fallbackPks, childrenRegistryItems, "Test DP Tax Credit Note", "세금계산서");
			AssertRegistryValuesAndTranslableData(newCompanyPkList, childrenRegistryItems, "Test DP Tax Credit Note", "세금계산서");

			SetUpParentRegistryAndCustomizableDataResourceString(fallbackPks, ItemSet.TaxAdjustmentNoteTitle, "Test DP Tax Adjustment Note", "세금계산서");
			childrenRegistryItems = new IRegistryItem[] { ItemSet.TaxAdjustmentNoteReversalTitle };
			AssertRegistryValuesAndTranslableData(fallbackPks, childrenRegistryItems, "Test DP Tax Adjustment Note", "세금계산서");
			AssertRegistryValuesAndTranslableData(newCompanyPkList, childrenRegistryItems, "Test DP Tax Adjustment Note", "세금계산서");
			AssertNoExceptionThrownWhenGetValidationErrorMessage(fallbackPks, childrenRegistryItems, "Test DP Tax Adjustment Note");

			SetUpParentRegistryAndCustomizableDataResourceString(fallbackPks, ItemSet.TaxDisbursementCreditNoteTitle, "Test DP Tax Credit Note Disbursement Amendment", "세금계산서");
			childrenRegistryItems = new IRegistryItem[] { ItemSet.TaxDisbursementCreditNoteAmendmentTitle, ItemSet.TaxDisbursementCreditNoteReversalTitle };
			AssertRegistryValuesAndTranslableData(fallbackPks, childrenRegistryItems, "Test DP Tax Credit Note Disbursement Amendment", "세금계산서");
			AssertRegistryValuesAndTranslableData(newCompanyPkList, childrenRegistryItems, "Test DP Tax Credit Note Disbursement Amendment", "세금계산서");

			SetUpParentRegistryAndCustomizableDataResourceString(fallbackPks, ItemSet.TaxDisbursementInvoiceTitle, "Test DP Tax Invoice Disbursement Amendment", "세금계산서");
			childrenRegistryItems = new IRegistryItem[] { ItemSet.TaxDisbursementInvoiceAmendmentTitle, ItemSet.TaxDisbursementInvoiceReversalTitle };
			AssertRegistryValuesAndTranslableData(fallbackPks, childrenRegistryItems, "Test DP Tax Invoice Disbursement Amendment", "세금계산서");
			AssertRegistryValuesAndTranslableData(newCompanyPkList, childrenRegistryItems, "Test DP Tax Invoice Disbursement Amendment", "세금계산서");
		}

		void SetUpParentRegistryAndCustomizableDataResourceString(List<Guid> fallbackPks, IRegistryItem parentRegistryItem, string english, string translation)
		{
			var helper = ObjectFactory.Get<ITranslatableDataTransformHelper>();
			var targetString = new CustomizableDataResourceStrings((ICustomizableDataCaptionSource)parentRegistryItem);
			var newKey = targetString.GetMultilingualString(null, english).ResourceKey;
			helper.AddResourceString(new ResourceStringData(newKey, translation), Core.SharedConstants.Languages.Korean);
			helper.AddResourceString(new ResourceStringData(newKey, english), Res.DefaultLanguage);
			helper.SaveChanges();

			foreach (var pk in fallbackPks)
			{
				parentRegistryItem.SetValue(pk, Guid.Empty, Guid.Empty, english);
			}
		}

		void AssertRegistryValuesAndTranslableData(List<Guid> fallbackPks, IRegistryItem[] childrenRegistryItems, string expectedEnglish, string expectedTranslation)
		{
			foreach (var registryItem in childrenRegistryItems)
			{
				foreach (var pk in fallbackPks)
				{
					AssertEquals(expectedEnglish, ((ResourceString)registryItem.GetValueWithoutFallback(pk, Guid.Empty, Guid.Empty)));
					AssertEquals(expectedTranslation, ((ResourceString)registryItem.GetValueWithoutFallback(pk, Guid.Empty, Guid.Empty)).ToString(Core.SharedConstants.Languages.Korean));
				}
			}
		}

		void AssertNoExceptionThrownWhenGetValidationErrorMessage(List<Guid> fallbackPks, IEnumerable<IRegistryItem> items, object proposedValue)
		{
			foreach (var item in items)
			{
				foreach (var pk in fallbackPks)
				{
					AssertNoExceptionThrown("No Exception Thrown When GetValidationErrorMessage", () => item.GetValidationErrorMessage(proposedValue, pk, Guid.Empty, Guid.Empty));
				}
			}
		}

		public void TestDisbursementMessage()
		{
			object value = ItemSet.DisbursementMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("DisbursementMessage", value is MultilingualString);
		}

		public void TestInvoiceMessage()
		{
			object value = ItemSet.InvoiceMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("InvoiceMessage", value is MultilingualString);
		}

		[ExpectNoExceptions]
		public void TestTaxInvoiceBatchTitle()
		{
			MultilingualString value = ItemSet.TaxInvoiceBatchTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
		}

		[ExpectNoExceptions]
		public void TestNonTaxInvoiceBatchTitle()
		{
			MultilingualString value = ItemSet.NonTaxInvoiceBatchTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
		}

		[ExpectNoExceptions]
		public void TestTaxInvoiceBatchDetailsTitle()
		{
			MultilingualString value = ItemSet.TaxInvoiceBatchDetailsTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
		}

		[ExpectNoExceptions]
		public void TestNonTaxInvoiceBatchDetailsTitle()
		{
			MultilingualString value = ItemSet.NonTaxInvoiceBatchDetailsTitle.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
		}

		public void TestStatementStandardMessage()
		{
			object value = ItemSet.StatementStandardMessage.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("StatementStandardMessage", value is string);
		}

		public void TestPrintWatermarkForTransactionAwaitingApproval()
		{
			object value = ItemSet.PrintWatermarkForTransactionAwaitingApproval.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("PrintWatermarkForTransactionAwaitingApproval", value is MultilingualString);

			AssertEquals("DefaultValue", string.Empty, ItemSet.PrintWatermarkForTransactionAwaitingApproval.DefaultValue);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults, ItemSet.PrintWatermarkForTransactionAwaitingApproval.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.PrintWatermarkForTransactionAwaitingApproval.Storage);
			AssertEquals("Caption", "Print Watermark for Transactions awaiting Approval", ItemSet.PrintWatermarkForTransactionAwaitingApproval.Caption);
			AssertEquals("Hint", "This Registry setting allows users to print a Watermark text on the Transactions Awaiting Approval/Clearance from the Government. It is strongly recommended that the Watermark be prominent and unambiguous for this purpose. If this registry is empty, no watermark will be printed.", ItemSet.PrintWatermarkForTransactionAwaitingApproval.Hint);

			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(ItemSet.PrintWatermarkForTransactionAwaitingApproval
				, x => x.PrintWatermarkForTransactionAwaitingApproval
				, ("", (NoResString)"")
				, ("Invalid\r\nNo Govt. ID", (NoResString)"Invalid\r\nNo Govt. ID")
				, "Print Watermark for Transactions awaiting Approval");
		}

		#region Tax Transaction

		public void TestPrintTaxDetailPERRIISLXVAT()
		{
			AssertEquals("PrintTaxDetailPERRIISLX", ItemSet.PrintTaxDetailPERRIISLX.Name);
			AssertEquals(Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_TaxTransaction, ItemSet.PrintTaxDetailPERRIISLX.Category);
			AssertEquals("Print Tax Detail - PER RII SLX VAT", ItemSet.PrintTaxDetailPERRIISLX.Caption);
			AssertEquals(@"This registry is relevant when Perceptions (PER), Value Added Tax (VAT), Retention In Invoice (RII) or Sales Tax (SLX) Tax Transactions are recorded against Invoice or Credit Note transactions.
By default this registry is set to Yes.
When set to Yes, the details of each PER, VAT, RII or SLX tax transaction recorded will be included in the document.
When overridden and set to No, details will not be included.", ItemSet.PrintTaxDetailPERRIISLX.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.PrintTaxDetailPERRIISLX.Storage);
			AssertEquals(true, ItemSet.PrintTaxDetailPERRIISLX.DefaultValue);

			ItemSet.PrintTaxDetailPERRIISLX.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.PrintTaxDetailPERRIISLX.Value);
		}

		public void TestPrintTaxDetailTRX()
		{
			AssertEquals("PrintTaxDetailTRX", ItemSet.PrintTaxDetailTRX.Name);
			AssertEquals(Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_TaxTransaction, ItemSet.PrintTaxDetailTRX.Category);
			AssertEquals("Print Tax Detail - TRX", ItemSet.PrintTaxDetailTRX.Caption);
			AssertEquals(@"This registry is relevant when Turnover Tax (TRX) Tax Transactions are recorded against Invoice or Credit Note transactions.
By default this registry is set to Yes.
When set to Yes, the details of each TRX tax transaction recorded will be included in the document.
When overridden and set to No, details will not be included.", ItemSet.PrintTaxDetailTRX.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.PrintTaxDetailTRX.Storage);
			AssertEquals(true, ItemSet.PrintTaxDetailTRX.DefaultValue);

			ItemSet.PrintTaxDetailTRX.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.PrintTaxDetailTRX.Value);
		}

		public void TestPrintTaxDetailSPR()
		{
			AssertEquals("PrintTaxDetailSPR", ItemSet.PrintTaxDetailSPR.Name);
			AssertEquals(Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_TaxTransaction, ItemSet.PrintTaxDetailSPR.Category);
			AssertEquals("Print Tax Detail - SPR", ItemSet.PrintTaxDetailSPR.Caption);
			AssertEquals(@"This registry is relevant when Standard Payments Basis Withholding (SPR) Tax Transactions are recorded against Invoice or Credit Note transactions.
By default this registry is set to No and details of each Notional (or Realized) Withholding tax transaction recorded against a transaction are omitted from the document. 
When overridden and set to Yes, details of each SPR tax transaction will be included.", ItemSet.PrintTaxDetailSPR.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.PrintTaxDetailSPR.Storage);
			AssertEquals(false, ItemSet.PrintTaxDetailSPR.DefaultValue);

			ItemSet.PrintTaxDetailSPR.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.PrintTaxDetailSPR.Value);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestPayableFinalIndicatorDefault()
		{
			bool value = ItemSet.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
		}

		public void TestPayableAllowUserToModifyTaxMessage()
		{
			AssertEquals("DefaultValue", true, ItemSet.PayableAllowUserToModifyTaxMessage.DefaultValue);
			AssertEquals("Category", Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.PayableAllowUserToModifyTaxMessage.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.PayableAllowUserToModifyTaxMessage.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.PayableAllowUserToModifyTaxMessage.Options);
			AssertEquals("Caption", "Allow user to modify Tax Message", ItemSet.PayableAllowUserToModifyTaxMessage.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', authorized users will be able to override Invoice Cost Tax Message / AP Invoice Tax Message defaulted.", ItemSet.PayableAllowUserToModifyTaxMessage.Hint);

			ItemSet.PayableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.PayableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			ItemSet.PayableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.PayableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestTransactionReportFilters()
		{
			ItemSet.TransactionReportFilters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("TransactionReportFilters", "test", ItemSet.TransactionReportFilters.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCFXAccountRegistryFallbackCaching()
		{
			Guid value = Guid.NewGuid();
			ItemSet.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals(value, ItemSet.CFXAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals(Guid.Empty, ItemSet.CFXAccount.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals(value, ItemSet.CFXAccount.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
		}

		public void TestCFXAccountRegistryFallbackCaching_Opposite()
		{
			Guid value = Guid.NewGuid();
			ItemSet.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals(value, ItemSet.CFXAccount.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals(Guid.Empty, ItemSet.CFXAccount.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
		}

		public void TestOverrideInterOfficeBillingTaxIDToNOTREPORT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertEquals("Name", "OverrideInterOfficeBillingTaxIDToNOTREPORT", ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORT.Name);
				AssertEquals("Category", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORT.Category);
				AssertEquals("Caption", "GST Group Member Billing Default Tax ID", ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORT.Caption);
				AssertEquals("Hint", @$"This registry is relevant to Companies flagged for VAT, GST, Consumption Tax etc.
By default, {BrandingFactory.Instance.ProductName} will record the ‘Not Reportable’ Tax ID against each AR and AP Charge Line WHEN the Organization is considered part of the same VAT/GST Group as the current Login Company/Branch.
An Organization is considered part of the same VAT/GST Group as the current Login Company/Branch WHEN the organization:
1. Has the same Tax Registration details as the current Login Company, OR
2. Is an Organization proxy of the Login Company/Branch, OR
3. Has the Organization Proxy of the current Login Company/Branch listed as an ‘ACG - Accounting VAT/GST Group’ related party.
When overridden to NO, the standard tax defaulting rules (i.e. Tax ID and Tax Message) for each charge code will apply.", ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORT.Hint);
				AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORT.Storage);
				AssertEquals("Options", RegistryOptions.Default, ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORT.Options);
			}
		}

		public void TestOverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertEquals("Name", "OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy", ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.Name);
				AssertEquals("Category", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.Category);
				AssertEquals("Caption", "GST Group Member Billing For Branch Organization Proxy", ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.Caption);
				AssertEquals("Hint", $"When overridden and set to True, {BrandingFactory.Instance.ProductName} will record Not Reportable Tax ID against each AR and AP charge line when the Debtor/Creditor Organization is a Branch Organization Proxy of the Login Company and has the same GST/VAT registration number as the Charge Line Branch.", ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.Hint);
				AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.Storage);
				AssertEquals("Options", RegistryOptions.Default, ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.Options);
				Assert("Default value is False", !ItemSet.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.DefaultValue);
			}
		}

		public void TestGroupMemberBillingDefaultInvoiceTaxMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertEquals("Name", "GroupMemberBillingDefaultInvoiceTaxMessage", ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.Name);
				AssertEquals("Category", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.Category);
				AssertEquals("Caption", "GST Group Member Billing Default Invoice Tax Message", ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.Caption);
				AssertEquals("Hint", @$"This registry is relevant to companies flagged for VAT, GST, Consumption Tax etc.
{BrandingFactory.Instance.ProductName} will record the Invoice Tax Message nominated in this registry against each AR and AP Charge Line WHEN the Organization is considered part of the same VAT/GST Group as the current Login Company/Branch.
An Organization is considered part of the same VAT/GST Group as the current Login Company/Branch WHEN the organization:
1. Has the same Tax Registration details as the current Login Company, OR
2. Is an Organization proxy of the Login Company/Branch, OR
3. Has the Organization Proxy of the current Login Company/Branch listed as an ‘ACG - Accounting VAT/GST Group’ related party.
NOTE: For this registry to work, the Login Company must have the ‘GST/VAT Group Member Billing Default Tax ID' registry set to 'YES'.", ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.Hint);
				AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.Storage);
				AssertEquals("Options", RegistryOptions.IsValueMandatory, ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.Options);

				Guid newGuid = Guid.NewGuid();
				ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
				AssertEquals("MatchingGroupMemberBillingDefaultInvoiceTaxMessage.Value", newGuid, ItemSet.GroupMemberBillingDefaultInvoiceTaxMessage.Value);
			}
		}

		public void TestBankTransactionGLAccount()
		{
			AssertEquals("Name", "BankTransactionGLAccount", ItemSet.BankTransactionGLAccount.Name);
			AssertEquals("Category", "Accounting/Cash Book Defaults", ItemSet.BankTransactionGLAccount.Category);
			AssertEquals("Caption", "Bank Transaction GL Account", ItemSet.BankTransactionGLAccount.Caption);
			AssertEquals("Hint", "Bank Transaction GL Account", ItemSet.BankTransactionGLAccount.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.BankTransactionGLAccount.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, ItemSet.BankTransactionGLAccount.Options);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.PandLOrBSH, ((GuidFindBoxRegistryEditorInfo)ItemSet.BankTransactionGLAccount.EditorInfo).FindBoxFilter);
		}

		public void TestGrossProfitTotalAccount()
		{
			Guid newGuid = Guid.NewGuid();
			ItemSet.GrossProfitTotalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("GrossProfitTotalAccount", newGuid, ItemSet.GrossProfitTotalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDiscrepancyGLAccount()
		{
			Guid newGuid = Guid.NewGuid();
			ItemSet.DiscrepancyGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings/Bulk AP Invoice Posting", ItemSet.DiscrepancyGLAccount.Category);
			AssertEquals("DiscrepancyGLAccount", newGuid, ItemSet.DiscrepancyGLAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestOverheadTotalAccount()
		{
			Guid newGuid = Guid.NewGuid();
			ItemSet.OverheadTotalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("OverheadTotalAccount", newGuid, ItemSet.OverheadTotalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestNetProfitTotalAccount()
		{
			Guid newGuid = Guid.NewGuid();
			ItemSet.NetProfitTotalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("NetProfitTotalAccount", newGuid, ItemSet.NetProfitTotalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPrintLogoOnChequeIsVisibleForAll()
		{
			AssertVisible(ItemSet.PrintLogoOnCheque, false);
		}

		public void TestPayableFinalFlag()
		{
			AssertEquals("PayableFinalFlag should be false by default", false, ItemSet.PayableFinalFlag.DefaultValue);
			ItemSet.PayableFinalFlag.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PayableFinalFlag should be set to true", true, ItemSet.PayableFinalFlag.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPayableAllowUserToModifyWHTId()
		{
			TestRegistryItem(ItemSet.PayableAllowUserToModifyWHTId, "PayableAllowUserToModifyWHTId", Categories.Accounting_PayableDefaults_DefaultSettings, "Allow user to modify Withholding Tax ID", @"This registry is no longer relevant in most Login Companies. This registry refers to obsolete Withholding Tax features that recorded WHT Tax IDs on individual charge lines similar to GST/VAT features. 

Please do not use this registry.", RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
		}

		public void TestReceivableAllowUserToModifyWHTId()
		{
			TestRegistryItem(ItemSet.ReceivableAllowUserToModifyWHTId, "ReceivableAllowUserToModifyWHTId", Categories.Accounting_ReceivableDefaults_DefaultSettings, "Allow user to modify Withholding Tax ID", @"This registry is no longer relevant in most Login Companies. This registry refers to obsolete Withholding Tax features that recorded WHT Tax IDs on individual charge lines similar to GST/VAT features. 

Please do not use this registry.", RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
		}

		public void TestPayableFinalFlagForConsolCost()
		{
			AssertEquals("PayableFinalFlagForConsolCost should be true by default", true, ItemSet.PayableFinalFlagForConsolCost.DefaultValue);
			ItemSet.PayableFinalFlagForConsolCost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PayableFinalFlagForConsolCost should be set to false", false, ItemSet.PayableFinalFlagForConsolCost.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestEnableValidationWhenAutoImportIntercompanyInvoices()
		{
			TestGenericRegistryItem(ItemSet.EnableValidationWhenAutoImportIntercompanyInvoices,
				"EnableValidationWhenAutoImportIntercompanyInvoices",
				Categories.Accounting_PayableDefaults_DefaultSettings,
				"Enable Validation When Auto Import Sister Company AR Invoices as AP Invoices",
				"When this flag is set to 'Yes', the Validation will be enabled when the batch processor will automatically import sister company AR invoices as AP invoices.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestDefaultDescriptionValueForDefaultDepartmentULDRegistries()
		{
			AssertEquals("Forwarding Domestic Air ULD", ItemSet.JobInvoicingDefaultDepartmentForwardingDomesticAirULD.Caption);
			AssertEquals("Forwarding Export Air ULD", ItemSet.JobInvoicingDefaultDepartmentForwardingExportAirULD.Caption);
			AssertEquals("Forwarding Foreign Air ULD", ItemSet.JobInvoicingDefaultDepartmentForwardingForeignAirULD.Caption);
			AssertEquals("Forwarding Import Air ULD", ItemSet.JobInvoicingDefaultDepartmentForwardingImportAirULD.Caption);
		}

		public void TestNegativeCostValidationEnforced()
		{
			AssertEquals("Category", string.Format("{0}/Job Costing Defaults", AccountingConfigurationRegistry.Categories.Accounting), ItemSet.NegativeCostValidationEnforced.Category);
		}

		public void TestCollectConstructorCallStackDetailsToReportInCriticalValidationErrors()
		{
			AssertEquals("Name", "CollectConstructorCallStackDetailsToReportInCriticalValidationErrors", ItemSet.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Name);
			AssertEquals("Category", "Accounting/Critical Validation", ItemSet.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Category);
			AssertEquals("Caption", "Collect Constructor Call Stack Details", ItemSet.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Caption);
			AssertEquals("Hint", "This Registry Item enables collecting constructor Call Stack information for some accounting business objects to be reported in the critical validation errors. It is set to No by default. Set it to Yes only when investigating the critical validation reported issues.", ItemSet.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForDevelopers, ItemSet.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.DefaultValue);
		}

		public void TestBringForwardAgainstCreditor()
		{
			AssertEquals("Name", "BringForwardAgainstCreditor", ItemSet.BringForwardAgainstCreditor.Name);
			AssertEquals("Category", "Accounting/Job Costing Defaults", ItemSet.BringForwardAgainstCreditor.Category);
			AssertEquals("Caption", "Accrual Reversal Behavior When Allocated To Creditor", ItemSet.BringForwardAgainstCreditor.Caption);
			AssertEquals("Hint", @"By default this registry is set to ‘Yes’.
When overridden and set to ‘No’, the Creditor code allocated against an Accrual is always ignored when posting an AP Invoice.
Posting an AP Invoice will reverse all outstanding Accruals for the same charge code + branch + department, irrespective of the Accrual’s allocated Creditor.
Any amounts re-accrued will be re-accrued without a Creditor code.

When set to ‘Yes’, the Creditor recorded against each Accrual will be respected.
In addition:
- Accruals attributed to ANOTHER Creditor will be ignored.  Accruals for another creditor will NOT be reversed.
- Only Accruals for the relevant Creditor or Accruals with no Creditor will be considered for reversal.
- Accruals for the AP Invoice Creditor will be reversed first, then, when those accruals have been exhausted, accruals with no Creditor for the same charge code + branch + department will be used.

Note: This registry must be set to 'Yes' in order to allow multiple unposted cost lines for a given charge code on Consol Cost.
The above accrual reversal rules apply. Any re-accrue will be done on the shipment level (where applicable).", ItemSet.BringForwardAgainstCreditor.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BringForwardAgainstCreditor.Storage);
			AssertEquals("Default Value", true, ItemSet.BringForwardAgainstCreditor.DefaultValue);
		}

		public void TestRestrictPostingOfSellChargesVisibleToLoginUserOnly()
		{
			AssertEquals("Name", "RestrictPostingOfSellChargesVisibleToLoginUserOnly", ItemSet.RestrictPostingOfSellChargesVisibleToLoginUserOnly.Name);
			AssertEquals("Category", "Accounting/Job Invoicing", ItemSet.RestrictPostingOfSellChargesVisibleToLoginUserOnly.Category);
			AssertEquals("Caption", "Restrict Posting Of Sell Charges Visible To Login User Only", ItemSet.RestrictPostingOfSellChargesVisibleToLoginUserOnly.Caption);
			AssertEquals("Hint", @"When this registry is set to 'Yes', CargoWise will restrict posting of SELL charges visible to the login user only.

Note: Users can be restricted from viewing charges outside their branch/department login permission.
For more information, kindly refer to update note on 'Hide Financial outside Login Branch/Department on Billing Jobs'.", ItemSet.RestrictPostingOfSellChargesVisibleToLoginUserOnly.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.RestrictPostingOfSellChargesVisibleToLoginUserOnly.Storage);
			AssertEquals("Default Value", false, ItemSet.RestrictPostingOfSellChargesVisibleToLoginUserOnly.DefaultValue);

			var registry = Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value set to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestCarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency()
		{
			AssertEquals("Name", "CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency", ItemSet.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Name);
			AssertEquals("Category", "Accounting/Job Costing Defaults", ItemSet.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Category);
			AssertEquals("Caption", "Carry Forward Accrual Based On OS Amount Where CST Currency = ACR Currency", ItemSet.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Caption);
			AssertEquals("Hint", @"By default this registry is set to 'No'.
When this registry is set to 'Yes', the following system behavior will be applied.

The system will carry forward accrual based on (Accrual's OS Amount minus Cost's OS Amount ) x Cost's Exchange Rate on the following conditions:
1. The Accrual is imported. 
2. The Cost and Accrual Currency (Billing Tab > OS Cost Currency) is the same.
3. The OS Cost Amount < OS Accrual Amount
4. The Final flag is not ticked. 

The system will create WIP based on (Cost's OS Amount minus Accrual's OS Amount ) x Job Exchange Rate on the following conditions:
1. The Accrual is imported. 
2. The Cost and Accrual Currency (Billing Tab > OS Cost Currency) is the same.
3. The OS Cost Amount > OS Accrual Amount
4. The 'Create WIP Where CST > ACR' system registry is set to 'Yes' and Final flag is not ticked
     OR the 'Create WIP Where Cost Is Final' system registry is set to 'Yes' and Final flag is ticked.

If any one of the conditions is not fulfilled, the current logic will apply. 
Based on current logic, the accrual will be carried forward in foreign currency when the  currency and exchange rate of the cost and accrual matches.
Otherwise, the accrual will be carried forward in local currency.", ItemSet.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Storage);
			AssertEquals("Default Value", false, ItemSet.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.DefaultValue);
		}

		public void TestDisplayTaxRegistrationNumber()
		{
			AssertEquals("Name", "DisplayTaxRegistrationNumber", ItemSet.DisplayTaxRegistrationNumber.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Form Configurations/Invoice", ItemSet.DisplayTaxRegistrationNumber.Category);
			AssertEquals("Caption", "Display Tax Registration Number", ItemSet.DisplayTaxRegistrationNumber.Caption);
			AssertEquals("Hint", @"This registry is relevant to login companies enabled for GST/VAT. 
When set to ‘Yes’ the VAT/GST tax registration details of the login company will print in the Invoice document.  The GST/VAT registration number will be drawn from the tax registration number recorded against the login company’s license key. 
A login company would set this registry to ‘No’ when their VAT registration details have been included in their letterhead graphic.", ItemSet.DisplayTaxRegistrationNumber.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.DisplayTaxRegistrationNumber.Storage);
			AssertEquals("Default Value", true, ItemSet.DisplayTaxRegistrationNumber.DefaultValue);
		}

		public void TestTransactionPaymentStatusWebServiceUsesSettlementGroup()
		{
			AssertEquals("Name", "TransactionPaymentStatusWebServiceUsesSettlementGroup", ItemSet.TransactionPaymentStatusWebServiceUsesSettlementGroup.Name);
			AssertEquals("Category", "Accounting/Web", ItemSet.TransactionPaymentStatusWebServiceUsesSettlementGroup.Category);
			AssertEquals("Caption", "Transaction Payment Status and Invoice Payment Services use Settlement Group", ItemSet.TransactionPaymentStatusWebServiceUsesSettlementGroup.Caption);
			AssertEquals("Hint", "This setting defines whether Transaction Payment Status and Invoice Payment Web Services use organization Settlement Group when looking for the specified transaction number for given organization.\r\nSet 'Yes' to allow using Settlement Groups.", ItemSet.TransactionPaymentStatusWebServiceUsesSettlementGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TransactionPaymentStatusWebServiceUsesSettlementGroup.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.TransactionPaymentStatusWebServiceUsesSettlementGroup.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.TransactionPaymentStatusWebServiceUsesSettlementGroup.DefaultValue);
		}

		public void TestNonTaxSelfBilledInvoiceTitle()
		{
			AssertEquals("Name", "NonTaxSelfBilledInvoiceTitle", ItemSet.NonTaxSelfBilledInvoiceTitle.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Self Billing Invoice", ItemSet.NonTaxSelfBilledInvoiceTitle.Category);
			AssertEquals("Caption", "Non Tax Invoice Title", ItemSet.NonTaxSelfBilledInvoiceTitle.Caption);
			AssertEquals("Hint", "This registry controls the Title printed in a Non Tax Self Billed Payables Invoice Transaction Document.", ItemSet.NonTaxSelfBilledInvoiceTitle.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.NonTaxSelfBilledInvoiceTitle.Storage);
			AssertEquals("Default Value", "Recipient Created / Self Billed Invoice", ItemSet.NonTaxSelfBilledInvoiceTitle.DefaultValue);
		}

		public void TestTaxSelfBilledInvoiceTitle()
		{
			AssertEquals("Name", "TaxSelfBilledInvoiceTitle", ItemSet.TaxSelfBilledInvoiceTitle.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Self Billing Invoice", ItemSet.TaxSelfBilledInvoiceTitle.Category);
			AssertEquals("Caption", "Tax Invoice Title", ItemSet.TaxSelfBilledInvoiceTitle.Caption);
			AssertEquals("Hint", "This registry controls the Title printed in a Tax Self Billed Payables Invoice Transaction Document.", ItemSet.TaxSelfBilledInvoiceTitle.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TaxSelfBilledInvoiceTitle.Storage);
			AssertEquals("Default Value", "Recipient Created / Self Billed Tax Invoice", ItemSet.TaxSelfBilledInvoiceTitle.DefaultValue);
		}

		public void TestNonTaxSelfBilledCreditNoteTitle()
		{
			AssertEquals("Name", "NonTaxSelfBilledCreditNoteTitle", ItemSet.NonTaxSelfBilledCreditNoteTitle.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Self Billing Invoice", ItemSet.NonTaxSelfBilledCreditNoteTitle.Category);
			AssertEquals("Caption", "Non Tax Credit Note Title", ItemSet.NonTaxSelfBilledCreditNoteTitle.Caption);
			AssertEquals("Hint", "This registry controls the Title printed in a Non Tax Self Billed Payables Credit Note Transaction Document.", ItemSet.NonTaxSelfBilledCreditNoteTitle.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.NonTaxSelfBilledCreditNoteTitle.Storage);
			AssertEquals("Default Value", "Recipient Created / Self Billed Credit Note", ItemSet.NonTaxSelfBilledCreditNoteTitle.DefaultValue);
		}

		public void TestTaxSelfBilledCreditNoteTitle()
		{
			AssertEquals("Name", "TaxSelfBilledCreditNoteTitle", ItemSet.TaxSelfBilledCreditNoteTitle.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Self Billing Invoice", ItemSet.TaxSelfBilledCreditNoteTitle.Category);
			AssertEquals("Caption", "Tax Credit Note Title", ItemSet.TaxSelfBilledCreditNoteTitle.Caption);
			AssertEquals("Hint", "This registry controls the Title printed in a Tax Self Billed Payables Credit Note Transaction Document.", ItemSet.TaxSelfBilledCreditNoteTitle.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TaxSelfBilledCreditNoteTitle.Storage);
			AssertEquals("Default Value", "Recipient Created / Self Billed Tax Credit Note", ItemSet.TaxSelfBilledCreditNoteTitle.DefaultValue);
		}

		public void TestNonTaxSelfBilledAdjustmentNoteTitle()
		{
			AssertEquals("Name", "NonTaxSelfBilledAdjustmentNoteTitle", ItemSet.NonTaxSelfBilledAdjustmentNoteTitle.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Self Billing Invoice", ItemSet.NonTaxSelfBilledAdjustmentNoteTitle.Category);
			AssertEquals("Caption", "Non Tax Adjustment Note Title", ItemSet.NonTaxSelfBilledAdjustmentNoteTitle.Caption);
			AssertEquals("Hint", "This registry controls the Title printed in a Non Tax Self Billed Payables Adjustment Note Transaction Document.", ItemSet.NonTaxSelfBilledAdjustmentNoteTitle.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.NonTaxSelfBilledAdjustmentNoteTitle.Storage);
			AssertEquals("Default Value", "Recipient Created / Self Billed Adjustment Note", ItemSet.NonTaxSelfBilledAdjustmentNoteTitle.DefaultValue);
		}

		public void TestTaxSelfBilledAdjustmentNoteTitle()
		{
			AssertEquals("Name", "TaxSelfBilledAdjustmentNoteTitle", ItemSet.TaxSelfBilledAdjustmentNoteTitle.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Self Billing Invoice", ItemSet.TaxSelfBilledAdjustmentNoteTitle.Category);
			AssertEquals("Caption", "Tax Adjustment Note Title", ItemSet.TaxSelfBilledAdjustmentNoteTitle.Caption);
			AssertEquals("Hint", "This registry controls the Title printed in a Tax Self Billed Payables Adjustment Note Transaction Document.", ItemSet.TaxSelfBilledAdjustmentNoteTitle.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TaxSelfBilledAdjustmentNoteTitle.Storage);
			AssertEquals("Default Value", "Recipient Created / Self Billed Tax Adjustment Note", ItemSet.TaxSelfBilledAdjustmentNoteTitle.DefaultValue);
		}

		public void TestShowInvoicePaymentWebServiceRegistryItem()
		{
			AssertEquals("Name", "ShowInvoicePaymentWebServiceRegistryItem", ItemSet.ShowInvoicePaymentWebServiceRegistryItem.Name);
			AssertEquals("Category", "Accounting/Web", ItemSet.ShowInvoicePaymentWebServiceRegistryItem.Category);
			AssertEquals("Caption", "Show Invoice Payment Web Service Registry Item", ItemSet.ShowInvoicePaymentWebServiceRegistryItem.Caption);
			AssertEquals("Hint", "This CargoWiseOne Support Only accessible setting defines visibility of the 'Enable Invoice Payment Web Service' registry item (default - No).", ItemSet.ShowInvoicePaymentWebServiceRegistryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ShowInvoicePaymentWebServiceRegistryItem.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ShowInvoicePaymentWebServiceRegistryItem.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.ShowInvoicePaymentWebServiceRegistryItem.DefaultValue);
		}

		public void TestEnableInvoicePaymentWebService()
		{
			AssertEquals("Name", "EnableInvoicePaymentWebService", ItemSet.EnableInvoicePaymentWebService.Name);
			AssertEquals("Category", "Accounting/Web", ItemSet.EnableInvoicePaymentWebService.Category);
			AssertEquals("Caption", "Enable Invoice Payment Web Service", ItemSet.EnableInvoicePaymentWebService.Caption);
			AssertEquals("Hint", "This setting defines whether Invoice Payment Web Service is enabled (default - No). This web service implements paying the AR/AP Invoices and Credit Notes without creating and matching a Payment transaction.\r\nSet 'Yes' to enable Invoice Payment Web Service.", ItemSet.EnableInvoicePaymentWebService.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableInvoicePaymentWebService.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.EnableInvoicePaymentWebService.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.EnableInvoicePaymentWebService.DefaultValue);
		}

		public void TestEnableInvoicePaymentWebServiceVisible()
		{
			ItemSet.ShowInvoicePaymentWebServiceRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnableInvoicePaymentWebService.Options);
		}

		public void TestAccountingWebServiceUserName()
		{
			AssertEquals("Name", "AccountingWebServiceUserName", ItemSet.AccountingWebServiceUserName.Name);
			AssertEquals("Category", "Accounting/Web", ItemSet.AccountingWebServiceUserName.Category);
			AssertEquals("Caption", "Accounting Web Service User Name", ItemSet.AccountingWebServiceUserName.Caption);
			AssertEquals("Hint", "This setting defines User Name to allow access to the Accounting Web Service.\r\nBoth Accounting Web Service User Name and Password should be specified.", ItemSet.AccountingWebServiceUserName.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AccountingWebServiceUserName.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, ItemSet.AccountingWebServiceUserName.Options);
			AssertEquals("Default Value", string.Empty, ItemSet.AccountingWebServiceUserName.DefaultValue);
		}

		public void TestAccountingWebServicePassword()
		{
			AssertEquals("Name", "AccountingWebServicePassword", ItemSet.AccountingWebServicePassword.Name);
			AssertEquals("Category", "Accounting/Web", ItemSet.AccountingWebServicePassword.Category);
			AssertEquals("Caption", "Accounting Web Service Password", ItemSet.AccountingWebServicePassword.Caption);
			AssertEquals("Hint", "This setting defines password to allow access to the Accounting Web Service.\r\nBoth Accounting Web Service User Name and Password should be specified.", ItemSet.AccountingWebServicePassword.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AccountingWebServicePassword.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue | RegistryOptions.IsPasswordVisibleForControllerUser, ItemSet.AccountingWebServicePassword.Options);
			Assert("EditorInfo", ItemSet.AccountingWebServicePassword.EditorInfo is TextRegistryEditorInfo);
			AssertEquals("EdoitorInfo.EditorType", TextEditorType.Password, ((TextRegistryEditorInfo)ItemSet.AccountingWebServicePassword.EditorInfo).EditorType);
			AssertEquals("Default Value", string.Empty, ItemSet.AccountingWebServicePassword.DefaultValue);
		}

		public void TestAccountingWebServiceRemarks()
		{
			TestRegistryItem(
				ItemSet.AccountingWebServiceRemarks,
				"AccountingWebServiceRemarks",
				AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
				"China's Golden Tax Invoice Remark Configuration",
				@"You can include free text and <macro> in the invoice remark. (i.e. MBL: <ConsolMasterBill>)

Further, you can define sections using square brackets. (i.e. [section] or [MBL: <ConsolMasterBill>]) and when the macro within the section returns empty value (i.e. no <ConsolMasterBill> value is found), then the entire section [MBL: <ConsolMasterBill>] will be excluded from the export. If you want a text to be always shown, add it outside a section. (i.e. Please pay by due date. [Section])

The available data fields for inclusion are:
1.	Consol Master Bill <ConsolMasterBill>
2.	Consol Vessel <ConsolVessel>
3.	Consol Voyage / Flight <ConsolVoyFlt>
4.	Consol Load Port <ConsolLoadPort>
5.	Consol Discharge Port <ConsolDischargePort>
6.	Consol ETD <ConsolETD>
7.	Consol ETA <ConsolETA>
8.	Shipment House Bill <ShipmentHouseBill>
9.	Job Invoice Number <JobInvNumber>
10.	Transaction Number <TransNumber>
11.	Invoice Currency <InvoiceCurrency>
12.	Invoice Amount <InvoiceAmount>
13.	Invoice Exchange Rate <InvoiceExchangeRate> 
14.	Job Operator Full Name <JobOperatorFullName>
15.	Job Operator Preferred Name <JobOperatorPreferredName>
16.	Job Sales Rep Full Name <JobSalesRepFullName>
17.	Job Sales Rep Preferred Name <JobSalesRepPreferredName>
18.	Transaction Description <TransDescription>

Note: 
a.	For Forwarding and CFS Shipment Job Invoices, all data values will be included (if entered).
b.	For Forwarding Consol and Other Job Invoices, only Job Invoice Number, Transaction Number, Foreign Currency, Amount and Exchange Rate will be included (where applicable).
c.	For Periodic and Non-Job Invoices, only Transaction Number, Foreign Currency, Amount and Exchange Rate will be included (where applicable).",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				TextEditorType.Memo,
				RegistryOptions.Default,
				"[MBL: <ConsolMasterBill> ][HBL: <ShipmentHouseBill> ][Vessel: <ConsolVessel> ][Voyage/Flight: <ConsolVoyFlt> ][ETD: <ConsolETD> ][ETA: <ConsolETA> ][LoadPort: <ConsolLoadPort> ][DischargePort: <ConsolDischargePort> ][JobInvoiceNumber: <JobInvNumber>]"
			);

			ItemSet.AccountingWebServiceRemarks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals("Allow empty Remarks", string.Empty, ItemSet.AccountingWebServiceRemarks.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			Assert("Must have inner implementation of IConnectionAdjustable", ItemSet.AccountingWebServiceRemarks.Inner is IConnectionAdjustable);
		}

		public void TestCreditLimitWarningThreshold()
		{
			AssertEquals("Name", "AllowCreditLimitWarningThreshold", ItemSet.CreditLimitWarningThreshold.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration/Local Credit Limit", ItemSet.CreditLimitWarningThreshold.Category);
			AssertEquals("Caption", "Credit Limit Warning Threshold", ItemSet.CreditLimitWarningThreshold.Caption);
			AssertEquals("Hint", "Threshold expressed as a percentage of AR Credit Limit. If the threshold is exceeded a warning email will be sent to the notification group specified in the system registry at Accounting > Email Notification > AR Control Breach Notify Group. A value of zero disables the warning.", ItemSet.CreditLimitWarningThreshold.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditLimitWarningThreshold.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CreditLimitWarningThreshold.Options);
			AssertEquals("Default Value", 0, ItemSet.CreditLimitWarningThreshold.DefaultValue);

			ItemSet.CreditLimitWarningThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 80);
			AssertEquals(80, ItemSet.CreditLimitWarningThreshold.Value);
		}

		public void TestGlobalCreditLimitWarningThreshold()
		{
			AssertEquals("Name", "AllowGlobalCreditLimitWarningThreshold", ItemSet.GlobalCreditLimitWarningThreshold.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration/Global Credit Limit", ItemSet.GlobalCreditLimitWarningThreshold.Category);
			AssertEquals("Caption", "Global Credit Limit Warning Threshold", ItemSet.GlobalCreditLimitWarningThreshold.Caption);
			AssertEquals("Hint", "Threshold expressed as a percentage of AR Global Credit Limit. If the threshold is exceeded a warning email will be sent to the notification group specified in the system registry at Accounting > Email Notification > Global AR Control Breach Notify Group. A value of zero disables the warning.", ItemSet.GlobalCreditLimitWarningThreshold.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.GlobalCreditLimitWarningThreshold.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.GlobalCreditLimitWarningThreshold.Options);
			AssertEquals("Default Value", 0, ItemSet.GlobalCreditLimitWarningThreshold.DefaultValue);

			ItemSet.GlobalCreditLimitWarningThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 80);
			AssertEquals(80, ItemSet.GlobalCreditLimitWarningThreshold.Value);
		}

		public void TestUseWebServiceTimeout()
		{
			AssertEquals("Name", "UseWebServiceTimeout", ItemSet.UseWebServiceTimeout.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.UseWebServiceTimeout.Category);
			AssertEquals("Caption", "Use Web Service Timeout", ItemSet.UseWebServiceTimeout.Caption);
			AssertEquals("Hint", "Number of seconds to wait for Web Service before timing out.", ItemSet.UseWebServiceTimeout.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.UseWebServiceTimeout.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.UseWebServiceTimeout.Options);
			AssertEquals("Default Value", 5, ItemSet.UseWebServiceTimeout.DefaultValue);

			ItemSet.UseWebServiceTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10, ItemSet.UseWebServiceTimeout.Value);
		}

		public void TestMaxTimeoutCountBeforeSuspendingWebService()
		{
			AssertEquals("Name", "MaxTimeoutCountBeforeSuspendingWebService", ItemSet.MaxTimeoutCountBeforeSuspendingWebService.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.MaxTimeoutCountBeforeSuspendingWebService.Category);
			AssertEquals("Caption", "Max Web Service Timeouts before suspending", ItemSet.MaxTimeoutCountBeforeSuspendingWebService.Caption);
			AssertEquals("Hint", "Maximum number of continuous Credit Limit Web Service timeouts before suspending attempts to call it.", ItemSet.MaxTimeoutCountBeforeSuspendingWebService.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.MaxTimeoutCountBeforeSuspendingWebService.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.MaxTimeoutCountBeforeSuspendingWebService.Options);
			AssertEquals("Default Value", 5, ItemSet.MaxTimeoutCountBeforeSuspendingWebService.DefaultValue);

			ItemSet.MaxTimeoutCountBeforeSuspendingWebService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10, ItemSet.MaxTimeoutCountBeforeSuspendingWebService.Value);
		}

		public void TestWebServiceCallSuspendingPeriodInMinutes()
		{
			AssertEquals("Name", "WebServiceCallSuspendingPeriodInMinutes", ItemSet.WebServiceCallSuspendingPeriodInMinutes.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.WebServiceCallSuspendingPeriodInMinutes.Category);
			AssertEquals("Caption", "Web Service call suspending period", ItemSet.WebServiceCallSuspendingPeriodInMinutes.Caption);
			AssertEquals("Hint", "Credit Limit Web Service call suspending period in minutes. Suspending will be applied after continuous timeouts to prevent slowing down the system.", ItemSet.WebServiceCallSuspendingPeriodInMinutes.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.WebServiceCallSuspendingPeriodInMinutes.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.WebServiceCallSuspendingPeriodInMinutes.Options);
			AssertEquals("Default Value", 15, ItemSet.WebServiceCallSuspendingPeriodInMinutes.DefaultValue);

			ItemSet.WebServiceCallSuspendingPeriodInMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10, ItemSet.WebServiceCallSuspendingPeriodInMinutes.Value);
		}

		public void TestShowCreditLimitWarningOnJobPosting()
		{
			TestRegistryItem(ItemSet.ShowCreditLimitWarningOnJobPosting,
				"ShowCreditLimitWarningOnJobPosting",
				"Accounting/Credit Controlled Documents Configuration",
				"Show Credit Limit Warning on Job Posting",
				@"If this registry is set to 'Yes', a Credit Limit Warning dialog will be shown on the job related revenue posting (for example via Job Invoicing menu) when one of the Debtors exceeds its Credit Limit. 
With this dialog user can choose to continue or to cancel posting.
The default values is 'No'.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ShowCreditLimitWarningOnJobPosting.Options);
		}

		public void TestJobCostingReportRelatedDBObjectVersion()
		{
			TestRegistryItem(ItemSet.JobCostingReportRelatedDBObjectVersion,
				"JobCostingReportRelatedDBObjectVersion",
				"Accounting/Job Costing Defaults/Job Costing Reports",
				"Current version Number of Job Costing Data Queue service task related Database Objects (CargoWiseOne Support Only)",
				"Shows current version number of Job Costing Data Queue service task related Database Objects (Tables, Stored Procedure, Functions etc). These Database Objects are created/updated by Job Costing Data Queue service task and used by the Job profit Reports. Job Costing Data Queue service task updates the value of this Registry",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				-1);
		}

		public void TestJobCostingQueueDBObjectVersion()
		{
			TestRegistryItem(ItemSet.JobCostingQueueDBObjectVersion,
				"JobCostingQueueDBObjectVersion",
				"Accounting/Job Costing Defaults/Job Costing Reports",
				"Current version Number of Temporary Database Objects used to process existing transaction record (CargoWiseOne Support Only)",
				"Shows the current version number of Temporary Database Objects used by Job Costing Data Queue service task to process existing (i.e. transactions created before the initialisation of Job Costing Data Queue service task) transaction records",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				-1);
		}

		public void TestPayableEnforceBranchLevelPosting()
		{
			AssertEquals("Name", "PayableEnforceBranchLevelPosting", ItemSet.PayableEnforceBranchLevelPosting.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.PayableEnforceBranchLevelPosting.Category);
			AssertEquals("Caption", "Enforce Posting at Branch Level", ItemSet.PayableEnforceBranchLevelPosting.Caption);
			AssertEquals("Hint", @"This registry affects the posting behaviors of Payables INV, CRD, ADJ and Cash Book DPY transactions only.
By default this registry is set to No and charge lines with any mix of branches are permitted within each transaction.
This registry should be set to NO when tax collection and reporting is a Company level registration shared by all branches in the one company.
This registry should be set to YES when tax registration, collection and reporting is a Branch level registration.
When set to YES:
- Posting will restrict the mix of line branches permitted within a single transaction.
- Unless defined in the grid below, lines for separate Branches will post in separate transactions.
- Branch Posting Groups can be defined in the grid below to allow mixed transactions.
- The transaction header branch is set from the Line branch, then Job Header Branch, falling back to the Permitted Posting Group check box.
Note:  This registry does NOT affect General Ledger Journals, Job Revenue Journals or other accounting entries.", ItemSet.PayableEnforceBranchLevelPosting.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.PayableEnforceBranchLevelPosting.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.PayableEnforceBranchLevelPosting.Options);
			AssertEquals("Default value", false, ItemSet.PayableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting);
		}

		public void TestAutoImportIntercompanyEventConfiguration()
		{
			AssertEquals("Name", "AutoImportIntercompanyEventConfiguration", ItemSet.AutoImportIntercompanyEventConfiguration.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.AutoImportIntercompanyEventConfiguration.Category);
			AssertEquals("Caption", "Auto Import Intercompany Event Configuration", ItemSet.AutoImportIntercompanyEventConfiguration.Caption);
			AssertEquals("Hint", @"You can optionally restrict the auto-import of job related sister companies' invoices when specific event after a specific (start) date is present on the related operation record's Workflow & Tracking > Events tab.

Use this registry to specify one or more events and the start date for each event.

Rules:
	1.	When the related operation record contains at least one event that meets the specified start date, the sister company invoice will be auto imported.
		For all job-related invoices other than Freight Consol Invoice (i.e. Shipment, Port Transport, etc.), the system check the corresponding Operation record > Workflow & Tracking > Events.
		For job related invoices relating to Freight Consol Invoice, the system will check the corresponding Consolidation > Workflow & Tracking > Events.

	2.	This configuration only applies to auto import of intercompany invoices when the 'Auto Import Sister Company AR Invoices as AP Invoices' registry is enabled. 
		It does not apply to manual import via action menu, intercompany invoice approval module or 'ISI' workflow trigger action.

	3.	This configuration only applies to auto import of job related invoices. It does not apply to miscellaneous (non-job related) invoices and periodic invoices."
			, ItemSet.AutoImportIntercompanyEventConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AutoImportIntercompanyEventConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AutoImportIntercompanyEventConfiguration.Options);
		}

		[TestDate(2018, 6, 15)]
		public void TestBuildAutoImportIntercompanyEventConfigurationLogReference()
		{
			var registryItem = Instance.AutoImportIntercompanyEventConfiguration.Inner;

			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = true;
			var settings1 = config.IntercompanyEventSettingCollection.AddNew();
			settings1.StmEventCode = Events.CustomisableEvent00.Code;
			settings1.StartDate = ZDate.Today.AddDays(5);

			var settings2 = config.IntercompanyEventSettingCollection.AddNew();
			settings2.StmEventCode = Events.CustomisableEvent01.Code;
			settings2.StartDate = ZDate.Today.AddDays(-5);

			var config2 = new IntercompanyEventConfiguration();
			config2.EnableEventConfiguration = false;

			settings2 = config2.IntercompanyEventSettingCollection.AddNew();
			settings2.StmEventCode = Events.CustomisableEvent01.Code;
			settings2.StartDate = ZDate.Today;

			var settings3 = config2.IntercompanyEventSettingCollection.AddNew();
			settings3.StmEventCode = Events.CustomisableEvent02.Code;
			settings3.StartDate = ZDate.Today.AddDays(5);

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, config, config2);
			var result = Instance.AutoImportIntercompanyEventConfiguration.OnBuildLogReference(args);

			AssertMultilineASCIIEquals(@"Event Configuration Disabled.
New Event Added 'Z02' with Start Date '20-Jun-18'.
Existing Event 'Z00' deleted.
Event 'Z01' Start Date change from '10-Jun-18' to '15-Jun-18'.", result);
		}

		public void TestReceivableEnforceBranchLevelPosting()
		{
			AssertEquals("Name", "ReceivableEnforceBranchLevelPosting", ItemSet.ReceivableEnforceBranchLevelPosting.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.ReceivableEnforceBranchLevelPosting.Category);
			AssertEquals("Caption", "Enforce Posting at Branch Level", ItemSet.ReceivableEnforceBranchLevelPosting.Caption);
			AssertEquals("Hint", @"This registry affects the posting behaviors of Receivables INV, CRD, ADJ and Cash Book DRC transactions only.
By default this registry is set to No and charge lines with any mix of branches are permitted within each transaction.
This registry should be set to NO when tax collection and reporting is a Company level registration shared by all branches in the one company.
This registry should be set to YES when tax registration, collection and reporting is a Branch level registration.
When set to YES:
- Posting will restrict the mix of line branches permitted within a single transaction.
- Unless defined in the grid below, lines for separate Branches will post in separate transactions.
- Branch Posting Groups can be defined in the grid below to allow mixed transactions.
- The transaction header branch is set from the Line branch, then Job Header Branch, falling back to the Permitted Posting Group check box.
Note:  This registry does NOT affect General Ledger Journals, Job Revenue Journals or other accounting entries.", ItemSet.ReceivableEnforceBranchLevelPosting.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ReceivableEnforceBranchLevelPosting.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ReceivableEnforceBranchLevelPosting.Options);
			AssertEquals("Default value", false, ItemSet.ReceivableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting);
		}

		public void TestIncludeUnpostedRevenueInCreditLimitCalculation()
		{
			AssertEquals("Name", "IncludeUnpostedRevenueInCreditLimitCalculation", ItemSet.IncludeUnpostedRevenueInCreditLimitCalculation.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration/Local Credit Limit", ItemSet.IncludeUnpostedRevenueInCreditLimitCalculation.Category);
			AssertEquals("Caption", "Include Unposted Revenue in Credit Limit Calculation", ItemSet.IncludeUnpostedRevenueInCreditLimitCalculation.Caption);
			AssertEquals("Hint", "This setting determines whether Unposted Recognized and Unrecognized Revenue is included in the Organization Credit Limit checking.\r\nSet to 'Posted Revenue Only' to exclude Unposted Recognized and Unrecognized Revenue from the Credit Limit calculation.\r\nSet to 'Posted and Unposted Recognized Revenue' to include Unposted Recognized Revenue in the Credit Limit calculation.\r\nSet to 'Posted, Unposted Recognized and Unrecognized Revenue' to include Unposted Recognized and Unrecognized Revenue in the Credit Limit calculation.", ItemSet.IncludeUnpostedRevenueInCreditLimitCalculation.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.IncludeUnpostedRevenueInCreditLimitCalculation.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.IncludeUnpostedRevenueInCreditLimitCalculation.Options);
			AssertEquals("Default Value", Constants.CreditLimitChecking.Posted, ItemSet.IncludeUnpostedRevenueInCreditLimitCalculation.DefaultValue);
		}

		public void TestUseWebServiceForCreditLimit()
		{
			AssertEquals("Name", "UseWebServiceForCreditLimit", ItemSet.UseWebServiceForCreditLimit.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.UseWebServiceForCreditLimit.Category);
			AssertEquals("Caption", "Use Web Service for Credit Limit", ItemSet.UseWebServiceForCreditLimit.Caption);
			AssertEquals("Hint", $"This setting defines whether a third-party Web Service is used for Organization Credit Limit checking instead of checking on the {BrandingFactory.Instance.ProductName} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Credit Limit Check Web Service URL, User Name and Password before its enabling.", ItemSet.UseWebServiceForCreditLimit.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.UseWebServiceForCreditLimit.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.UseWebServiceForCreditLimit.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.UseWebServiceForCreditLimit.DefaultValue);
		}

		public void TestUseWebServiceForOutstandingBalance()
		{
			AssertEquals("Name", "UseWebServiceForOutstandingBalance", ItemSet.UseWebServiceForOutstandingBalance.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.UseWebServiceForOutstandingBalance.Category);
			AssertEquals("Caption", "Use Web Service for Outstanding Balance", ItemSet.UseWebServiceForOutstandingBalance.Caption);
			AssertEquals("Hint", $"This setting defines whether a third-party Web Service is used for Organization Outstanding Balance checking instead of checking on the {BrandingFactory.Instance.ProductName} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Credit Limit Check Web Service URL, User Name and Password before its enabling.", ItemSet.UseWebServiceForOutstandingBalance.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.UseWebServiceForOutstandingBalance.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.UseWebServiceForOutstandingBalance.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.UseWebServiceForOutstandingBalance.DefaultValue);
		}

		public void TestUseWebServiceForUnpostedRevenue()
		{
			AssertEquals("Name", "UseWebServiceForUnpostedRevenue", ItemSet.UseWebServiceForUnpostedRevenue.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.UseWebServiceForUnpostedRevenue.Category);
			AssertEquals("Caption", "Use Web Service for Unposted Revenue", ItemSet.UseWebServiceForUnpostedRevenue.Caption);
			AssertEquals("Hint", $"This setting defines whether a third-party Web Service is used for Organization Unposted Revenue checking instead of checking on the {BrandingFactory.Instance.ProductName} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Credit Limit Check Web Service URL, User Name and Password before its enabling.", ItemSet.UseWebServiceForUnpostedRevenue.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.UseWebServiceForUnpostedRevenue.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.UseWebServiceForUnpostedRevenue.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.UseWebServiceForUnpostedRevenue.DefaultValue);
		}

		public void TestEnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService()
		{
			AssertEquals("Name", "EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService", ItemSet.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Category);
			AssertEquals("Caption", "Enable Background Validation on Billing Tab", ItemSet.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Caption);
			AssertEquals("Hint", "This setting enables background Credit Limit Validation on Billing tab when a third-party Web Service is used for any part of Organization Credit Limit checking.\r\nSet 'No' to disable background validation.", ItemSet.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Options);
			AssertEquals("Default Value", ZBool.True, ItemSet.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.DefaultValue);
		}

		public void TestCreditLimitCheckWebServiceUrl()
		{
			AssertEquals("Name", "CreditLimitCheckWebServiceUrl", ItemSet.CreditLimitCheckWebServiceUrl.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.CreditLimitCheckWebServiceUrl.Category);
			AssertEquals("Caption", "Credit Limit Check Web Service URL", ItemSet.CreditLimitCheckWebServiceUrl.Caption);
			AssertEquals("Hint", "This setting defines the URL of a third-party Organization Credit Limit Check Web Service.", ItemSet.CreditLimitCheckWebServiceUrl.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditLimitCheckWebServiceUrl.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CreditLimitCheckWebServiceUrl.Options);
			AssertEquals("Default Value", string.Empty, ItemSet.CreditLimitCheckWebServiceUrl.DefaultValue);
		}

		public void TestCreditLimitCheckWebServiceUserName()
		{
			AssertEquals("Name", "CreditLimitCheckWebServiceUserName", ItemSet.CreditLimitCheckWebServiceUserName.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.CreditLimitCheckWebServiceUserName.Category);
			AssertEquals("Caption", "Credit Limit Check Web Service User Name", ItemSet.CreditLimitCheckWebServiceUserName.Caption);
			AssertEquals("Hint", "This setting defines User Name to access a third-party Organization Credit Limit Check Web Service.", ItemSet.CreditLimitCheckWebServiceUserName.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditLimitCheckWebServiceUserName.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CreditLimitCheckWebServiceUserName.Options);
			AssertEquals("Default Value", string.Empty, ItemSet.CreditLimitCheckWebServiceUserName.DefaultValue);
		}

		public void TestCreditLimitCheckWebServicePassword()
		{
			AssertEquals("Name", "CreditLimitCheckWebServicePassword", ItemSet.CreditLimitCheckWebServicePassword.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.CreditLimitCheckWebServicePassword.Category);
			AssertEquals("Caption", "Credit Limit Check Web Service Password", ItemSet.CreditLimitCheckWebServicePassword.Caption);
			AssertEquals("Hint", "This setting defines Password to access a third-party Organization Credit Limit Check Web Service.", ItemSet.CreditLimitCheckWebServicePassword.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditLimitCheckWebServicePassword.Storage);
			AssertEquals("Options", RegistryOptions.IsPasswordVisibleForControllerUser, ItemSet.CreditLimitCheckWebServicePassword.Options);
			Assert("EditorInfo", ItemSet.CreditLimitCheckWebServicePassword.EditorInfo is TextRegistryEditorInfo);
			AssertEquals("EdoitorInfo.EditorType", TextEditorType.Password, ((TextRegistryEditorInfo)ItemSet.CreditLimitCheckWebServicePassword.EditorInfo).EditorType);
			AssertEquals("Default Value", string.Empty, ItemSet.CreditLimitCheckWebServicePassword.DefaultValue);
		}

		public void TestResubmitCreditApprovalRequestsBasedOnBillingValues()
		{
			AssertEquals("DataType", typeof(CodePairRegistryItem), ItemSet.ResubmitCreditApprovalRequestsBasedOnBillingValues.GetType());
			AssertEquals("Name", "ResubmitCreditApprovalRequestsBasedOnBillingValues", ItemSet.ResubmitCreditApprovalRequestsBasedOnBillingValues.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.ResubmitCreditApprovalRequestsBasedOnBillingValues.Category);
			AssertEquals("Caption", "Resubmit Credit Approval Requests Based on Billing Values", ItemSet.ResubmitCreditApprovalRequestsBasedOnBillingValues.Caption);
			AssertEquals("Hint", @$"This registry allows {BrandingFactory.Instance.ProductName} to compare the current billing values to the billing values that were recorded at the point of making a credit control request.
The default behavior of this registry is to not compare current billing values when checking for credit control request.
If the registry value is overridden to a non-default value then when the sell values Increase and/or Decrease or there is a change in the organizations subjected to credit control evaluation, previous request will be canceled and a new one will be created.
The available options are:
DEF - No comparison made (this is the registry default).
INC - Create new credit control request only when sell values have increased.
BTH - Create new credit control request when sell values have both increased or decreased.", ItemSet.ResubmitCreditApprovalRequestsBasedOnBillingValues.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.ResubmitCreditApprovalRequestsBasedOnBillingValues.Storage);
			AssertEquals("Default value", ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Default.Code, ItemSet.ResubmitCreditApprovalRequestsBasedOnBillingValues.DefaultValue);
		}

		public void TestCreditLimitCheckTemporaryCreditLimitIncreaseThreshold()
		{
			AssertEquals("Name", "CreditLimitCheckTemporaryCreditLimitIncreaseThreshold", ItemSet.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Category);
			AssertEquals("Caption", "Temporary Credit Limit Increase Threshold", ItemSet.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Caption);
			AssertEquals("Hint", @"Use this registry to set a percentage or amount threshold at which an authorized user can temporary increase the credit limit for AR Organizations.
In addition, the temporary increase in credit limit can be set to expire after a specific number of days (from the day of adjustment).

The authorization levels (up to 3) restrict the setting of the temporary increase to users with specific authorization levels that is set up in the security settings.
The authorization requirement “None” allows users with access to change the credit limit amount up to the specified threshold level.", ItemSet.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Options);
		}

		public void TestUseWebServiceForTransactionPaymentStatus()
		{
			AssertEquals("Name", "UseWebServiceForTransactionPaymentStatus", ItemSet.UseWebServiceForTransactionPaymentStatus.Name);
			AssertEquals("Category", "Accounting/Transaction Payment Status", ItemSet.UseWebServiceForTransactionPaymentStatus.Category);
			AssertEquals("Caption", "Use Web Service for Transaction Payment Status", ItemSet.UseWebServiceForTransactionPaymentStatus.Caption);
			AssertEquals("Hint", $"This setting defines whether a third-party Web Service is used for Transaction Payment Status instead of using the {BrandingFactory.Instance.ProductName} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Transaction Payment Status Web Service URL, User Name and Password before its enabling.", ItemSet.UseWebServiceForTransactionPaymentStatus.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.UseWebServiceForTransactionPaymentStatus.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.UseWebServiceForTransactionPaymentStatus.Options);
			AssertEquals("Default Value", ZBool.False, ItemSet.UseWebServiceForTransactionPaymentStatus.DefaultValue);
		}

		public void TestTransactionPaymentStatusWebServiceUrl()
		{
			AssertEquals("Name", "TransactionPaymentStatusWebServiceUrl", ItemSet.TransactionPaymentStatusWebServiceUrl.Name);
			AssertEquals("Category", "Accounting/Transaction Payment Status", ItemSet.TransactionPaymentStatusWebServiceUrl.Category);
			AssertEquals("Caption", "Transaction Payment Status Web Service URL", ItemSet.TransactionPaymentStatusWebServiceUrl.Caption);
			AssertEquals("Hint", "This setting defines the URL of a third-party Transaction Payment Status Web Service.", ItemSet.TransactionPaymentStatusWebServiceUrl.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TransactionPaymentStatusWebServiceUrl.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.TransactionPaymentStatusWebServiceUrl.Options);
			AssertEquals("Default Value", string.Empty, ItemSet.TransactionPaymentStatusWebServiceUrl.DefaultValue);
		}

		public void TestTransactionPaymentStatusWebServiceUserName()
		{
			AssertEquals("Name", "TransactionPaymentStatusWebServiceUserName", ItemSet.TransactionPaymentStatusWebServiceUserName.Name);
			AssertEquals("Category", "Accounting/Transaction Payment Status", ItemSet.TransactionPaymentStatusWebServiceUserName.Category);
			AssertEquals("Caption", "Transaction Payment Status Web Service User Name", ItemSet.TransactionPaymentStatusWebServiceUserName.Caption);
			AssertEquals("Hint", "This setting defines the User Name to access a third-party Transaction Payment Status Web Service.", ItemSet.TransactionPaymentStatusWebServiceUserName.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TransactionPaymentStatusWebServiceUserName.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.TransactionPaymentStatusWebServiceUserName.Options);
			AssertEquals("Default Value", string.Empty, ItemSet.TransactionPaymentStatusWebServiceUserName.DefaultValue);
		}

		public void TestTransactionPaymentStatusWebServicePassword()
		{
			AssertEquals("Name", "TransactionPaymentStatusWebServicePassword", ItemSet.TransactionPaymentStatusWebServicePassword.Name);
			AssertEquals("Category", "Accounting/Transaction Payment Status", ItemSet.TransactionPaymentStatusWebServicePassword.Category);
			AssertEquals("Caption", "Transaction Payment Status Web Service Password", ItemSet.TransactionPaymentStatusWebServicePassword.Caption);
			AssertEquals("Hint", "This setting defines the Password to access a third-party Transaction Payment Status Web Service.", ItemSet.TransactionPaymentStatusWebServicePassword.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TransactionPaymentStatusWebServicePassword.Storage);
			AssertEquals("Options", RegistryOptions.IsPasswordVisibleForControllerUser, ItemSet.TransactionPaymentStatusWebServicePassword.Options);
			Assert("EditorInfo", ItemSet.TransactionPaymentStatusWebServicePassword.EditorInfo is TextRegistryEditorInfo);
			AssertEquals("EditorInfo.EditorType", TextEditorType.Password, ((TextRegistryEditorInfo)ItemSet.TransactionPaymentStatusWebServicePassword.EditorInfo).EditorType);
			AssertEquals("Default Value", string.Empty, ItemSet.TransactionPaymentStatusWebServicePassword.DefaultValue);
		}

		public void TestAccountingTransactionExportServiceHighWaterMark()
		{
			TestGenericRegistryItem(ItemSet.AccountingTransactionExportServiceHighWaterMark,
				"AccountingTransactionExportServiceHighWaterMark",
				"Accounting/Web",
				"Accounting Transaction Export Service High Water Mark",
				"To aid performance of the Accounting Transaction Export Service, the system will only search for un-batched transactions that were posted after this date.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				DateTime.MinValue);
			AssertType(typeof(AccountingTransactionExportHighWaterMarkDataType), ItemSet.AccountingTransactionExportServiceHighWaterMark.DataType);
		}

		public void TestExportTransactionBatchStartPostDate()
		{
			TestGenericRegistryItem(ItemSet.ExportTransactionBatchStartPostDate,
				"ExportTransactionBatchStartPostDate",
				"Accounting/Web",
				"Export transaction batch start postdate",
				"When this registry value is set, only transactions posted/reversed on this or after this date will be exported into batch.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport);
			AssertType(typeof(DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue), ItemSet.ExportTransactionBatchStartPostDate.DataType);
		}

		public void TestCostConfirmationDocumentSetting()
		{
			AssertEquals("DefaultValue", "DTL", ItemSet.CostConfirmationDocumentSettings.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.CostConfirmationDocumentSettings.CountryFilterPKs);
			ItemSet.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SMY");
			AssertEquals("Value", "SMY", ItemSet.CostConfirmationDocumentSettings.Value);
			ItemSet.CostConfirmationDocumentSettings.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "BTH");
			AssertEquals("Value", "BTH", ItemSet.CostConfirmationDocumentSettings.Value);
		}

		public void TestCostConfirmationDocumentRollupSettings()
		{
			AssertEquals("DefaultValue", "CHG", ItemSet.CostConfirmationDocumentRollupSettings.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.CostConfirmationDocumentRollupSettings.CountryFilterPKs);
			ItemSet.CostConfirmationDocumentRollupSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "JOB");
			AssertEquals("Value", "JOB", ItemSet.CostConfirmationDocumentRollupSettings.Value);
			ItemSet.CostConfirmationDocumentRollupSettings.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CHG");
			AssertEquals("Value", "CHG", ItemSet.CostConfirmationDocumentRollupSettings.Value);
		}

		public void TestClearingJournalConfiguration()
		{
			AssertEquals("DefaultValue", "STD", ItemSet.ClearingJournalConfiguration.DefaultValue);
			AssertEquals("CountryFilterPK", Enumerable.Empty<Guid>(), ItemSet.ClearingJournalConfiguration.CountryFilterPKs);
			ItemSet.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HBR");
			AssertEquals("Value", "HBR", ItemSet.ClearingJournalConfiguration.Value);
			ItemSet.ClearingJournalConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "LBR");
			AssertEquals("Value", "LBR", ItemSet.ClearingJournalConfiguration.Value);
			ItemSet.ClearingJournalConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "LBX");
			AssertEquals("Value", "LBX", ItemSet.ClearingJournalConfiguration.Value);
		}

		public void TestMaximumResultsInMatchingSearch()
		{
			AssertEquals("Caption", "Max Results in Matching Search", ItemSet.MaximumResultsInMatchingSearch.Caption);
			AssertEquals("Hint", @"This setting limits the maximum number of  Unmatched Transactions returned returned by transaction search on the Matching screen.
Increasing this number can affect performance of your matching functionality.", ItemSet.MaximumResultsInMatchingSearch.Hint);
			AssertEquals("Category", "Accounting/Matching", ItemSet.MaximumResultsInMatchingSearch.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.MaximumResultsInMatchingSearch.Storage);

			AssertEquals("DefaultValue", 1000, ItemSet.MaximumResultsInMatchingSearch.DefaultValue);
			ItemSet.MaximumResultsInMatchingSearch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1500);
			AssertEquals("Value", 1500, ItemSet.MaximumResultsInMatchingSearch.Value);
			ItemSet.MaximumResultsInMatchingSearch.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 1200);
			AssertEquals("Value", 1200, ItemSet.MaximumResultsInMatchingSearch.Value);
			ItemSet.MaximumResultsInMatchingSearch.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 1100);
			AssertEquals("Value", 1100, ItemSet.MaximumResultsInMatchingSearch.Value);
		}

		public void TestPopupUnMatchTransactionDescriptionOverrideOnUnMatching()
		{
			AssertEquals("DefaultValue", false, ItemSet.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.DefaultValue);

			ItemSet.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.Value);

			ItemSet.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.Value);
		}

		public void TestShippingDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.ShippingDefaultToCurrentLoginDept.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager, ItemSet.ShippingDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.ShippingDefaultToCurrentLoginDept.Storage);
			AssertEquals(false, ItemSet.ShippingDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.ShippingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals(true, ItemSet.ShippingDefaultToCurrentLoginDept.Value);
		}

		public void TestCustomsDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.CustomsDefaultToCurrentLoginDept.Caption);
			AssertEquals(string.Format("{0}/Default Departments/Customs", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing), ItemSet.CustomsDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.CustomsDefaultToCurrentLoginDept.Storage);
			AssertEquals(false, ItemSet.CustomsDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals(true, ItemSet.CustomsDefaultToCurrentLoginDept.Value);
		}

		public void TestCfsDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.CfsDefaultToCurrentLoginDept.Caption);
			AssertEquals(string.Format("{0}/Default Departments/CFS", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing), ItemSet.CfsDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.CfsDefaultToCurrentLoginDept.Storage);
			AssertEquals(false, ItemSet.CfsDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.CfsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals(true, ItemSet.CfsDefaultToCurrentLoginDept.Value);
		}

		public void TestWarehouseDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.WarehouseDefaultToCurrentLoginDept.Caption);
			AssertEquals(string.Format("{0}/Default Departments/Warehouse", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing), ItemSet.WarehouseDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.WarehouseDefaultToCurrentLoginDept.Storage);
			AssertEquals(false, ItemSet.WarehouseDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.WarehouseDefaultToCurrentLoginDept.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals(true, ItemSet.WarehouseDefaultToCurrentLoginDept.Value);
		}

		public void TestForwardingDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.ForwardingDefaultToCurrentLoginDept.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding, ItemSet.ForwardingDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.ForwardingDefaultToCurrentLoginDept.Storage);
			AssertEquals(false, ItemSet.ForwardingDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals(true, ItemSet.ForwardingDefaultToCurrentLoginDept.Value);
		}

		public void TestNCTSDepartureAir()
		{
			AssertEquals("NCTS Departure Air", ItemSet.NCTSDepartureAir.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDepartureAir.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSDepartureAir.Storage);
			AssertEquals(ItemSet.CustomsExportAirUld.DefaultValue, ItemSet.NCTSDepartureAir.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSDepartureAir.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureAir.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureAir.EditorInfo).FindBoxFilter);

			ItemSet.NCTSDepartureAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSDepartureAir.Value);
		}

		public void TestNCTSDeparturePost()
		{
			AssertEquals("NCTS Departure Post", ItemSet.NCTSDeparturePost.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDeparturePost.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSDeparturePost.Storage);
			AssertEquals(ItemSet.CustomsExportPost.DefaultValue, ItemSet.NCTSDeparturePost.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSDeparturePost.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDeparturePost.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDeparturePost.EditorInfo).FindBoxFilter);

			ItemSet.NCTSDeparturePost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSDeparturePost.Value);
		}

		public void TestNCTSDepartureRail()
		{
			AssertEquals("NCTS Departure Rail", ItemSet.NCTSDepartureRail.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDepartureRail.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSDepartureRail.Storage);
			AssertEquals(ItemSet.CustomsExportRail.DefaultValue, ItemSet.NCTSDepartureRail.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSDepartureRail.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureRail.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureRail.EditorInfo).FindBoxFilter);

			ItemSet.NCTSDepartureRail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSDepartureRail.Value);
		}

		public void TestNCTSDepartureRoad()
		{
			AssertEquals("NCTS Departure Road", ItemSet.NCTSDepartureRoad.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDepartureRoad.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSDepartureRoad.Storage);
			AssertEquals(ItemSet.CustomsExportRoad.DefaultValue, ItemSet.NCTSDepartureRoad.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSDepartureRoad.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureRoad.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureRoad.EditorInfo).FindBoxFilter);

			ItemSet.NCTSDepartureRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSDepartureRoad.Value);
		}

		public void TestNCTSDepartureSeaFcl()
		{
			AssertEquals("NCTS Departure Sea FCL", ItemSet.NCTSDepartureSeaFcl.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDepartureSeaFcl.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSDepartureSeaFcl.Storage);
			AssertEquals(ItemSet.CustomsExportSeaFcl.DefaultValue, ItemSet.NCTSDepartureSeaFcl.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSDepartureSeaFcl.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureSeaFcl.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureSeaFcl.EditorInfo).FindBoxFilter);

			ItemSet.NCTSDepartureSeaFcl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSDepartureSeaFcl.Value);
		}

		public void TestNCTSDepartureSeaLcl()
		{
			AssertEquals("NCTS Departure Sea LCL", ItemSet.NCTSDepartureSeaLcl.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDepartureSeaLcl.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSDepartureSeaLcl.Storage);
			AssertEquals(ItemSet.CustomsExportSeaLcl.DefaultValue, ItemSet.NCTSDepartureSeaLcl.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSDepartureSeaLcl.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureSeaLcl.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureSeaLcl.EditorInfo).FindBoxFilter);

			ItemSet.NCTSDepartureSeaLcl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSDepartureSeaLcl.Value);
		}

		public void TestNCTSDepartureOther()
		{
			AssertEquals("NCTS Departure Other", ItemSet.NCTSDepartureOther.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDepartureOther.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSDepartureOther.Storage);
			AssertEquals(ItemSet.CustomsOther.DefaultValue, ItemSet.NCTSDepartureOther.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSDepartureOther.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureOther.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSDepartureOther.EditorInfo).FindBoxFilter);

			ItemSet.NCTSDepartureOther.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSDepartureOther.Value);
		}

		public void TestNCTSArrival()
		{
			AssertEquals("NCTS Arrival", ItemSet.NCTSArrival.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSArrival.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.NCTSArrival.Storage);
			AssertEquals(ItemSet.CustomsImportOther.DefaultValue, ItemSet.NCTSArrival.DefaultValue);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.NCTSArrival.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxCollection", RegistryFindBoxCollection.GlbDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSArrival.EditorInfo).FindBoxCollection);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.NonMiscDepartment, ((GuidFindBoxRegistryEditorInfo)ItemSet.NCTSArrival.EditorInfo).FindBoxFilter);

			ItemSet.NCTSArrival.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistryConstants.DepartmentPKs.ClearanceExBond);
			AssertEquals(RegistryConstants.DepartmentPKs.ClearanceExBond, ItemSet.NCTSArrival.Value);
		}

		public void TestNCTSDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.NCTSDefaultToCurrentLoginDept.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS, ItemSet.NCTSDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.NCTSDefaultToCurrentLoginDept.Storage);
			AssertEquals(ItemSet.CustomsDefaultToCurrentLoginDept.DefaultValue, ItemSet.NCTSDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.NCTSDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.NCTSDefaultToCurrentLoginDept.Value);
		}

		public void TestTransportBookingDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.TransportBookingDefaultToCurrentLoginDept.Caption);
			AssertEquals(string.Format("{0}/Default Departments/Transport Booking", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing), ItemSet.TransportBookingDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.TransportBookingDefaultToCurrentLoginDept.Storage);
			AssertEquals(false, ItemSet.TransportBookingDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.TransportBookingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals(true, ItemSet.TransportBookingDefaultToCurrentLoginDept.Value);
		}

		public void TestLandTransportDefaultToCurrentLoginDept()
		{
			AssertEquals("Default to Current Login Dept.", ItemSet.LandTransportDefaultToCurrentLoginDept.Caption);
			AssertEquals(string.Format("{0}/Default Departments/Land Transport", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing), ItemSet.LandTransportDefaultToCurrentLoginDept.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.LandTransportDefaultToCurrentLoginDept.Storage);
			AssertEquals(true, ItemSet.LandTransportDefaultToCurrentLoginDept.DefaultValue);

			ItemSet.LandTransportDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.LandTransportDefaultToCurrentLoginDept.Value);
		}

		public void TestLandTransportJobsDefaultDepts()
		{
			var testGroup = Guid.NewGuid();

			AssertEquals("Land Transport Jobs", ItemSet.LandTransportJobsDefaultDept.Caption);
			AssertEquals(string.Format("{0}/Default Departments/Land Transport", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing), ItemSet.LandTransportJobsDefaultDept.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.LandTransportJobsDefaultDept.Storage);
			AssertEquals("Default value", RegistryConstants.DepartmentPKs.TransportBookingDefaultDepartment, ItemSet.LandTransportJobsDefaultDept.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			ItemSet.LandTransportJobsDefaultDept.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("Set value", testGroup, ItemSet.LandTransportJobsDefaultDept.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestJobLockedRetryIntervalInMinutes()
		{
			AssertEquals("Name", "JobLockedRetryIntervalInMinutes", ItemSet.JobLockedRetryIntervalInMinutes.Name);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing, ItemSet.JobLockedRetryIntervalInMinutes.Category);
			AssertEquals("Caption", "Job Locked Retry Interval In Minutes", ItemSet.JobLockedRetryIntervalInMinutes.Caption);
			AssertEquals("Hint", "Job Locked Retry Interval In Minutes.", ItemSet.JobLockedRetryIntervalInMinutes.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.JobLockedRetryIntervalInMinutes.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.JobLockedRetryIntervalInMinutes.Options);
			AssertEquals("Default Value", 5, ItemSet.JobLockedRetryIntervalInMinutes.DefaultValue);
			var dataType = (IntRegistryDataType)ItemSet.JobLockedRetryIntervalInMinutes.DataType;
			AssertEquals("Min Value", (double)1, dataType.LowerBound);
			AssertEquals("Max Value", (double)10, dataType.UpperBound);

			ItemSet.JobLockedRetryIntervalInMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			AssertEquals(2, ItemSet.JobLockedRetryIntervalInMinutes.Value);
		}

		public void TestVoucherNumberOfSupportingDocumentDefaults()
		{
			AssertEquals("Transaction Description Defaults", ItemSet.VoucherNumberOfSupportingDocumentDefaults.Caption);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Storage);
			AssertEquals(@"This registry setting allow you to configure the default values for all new transactions.
You can nominate the default value for the transaction description in the grid below.

These values will be defaulted into the 'Description' fields when creating a new transaction.
Note: Users can override these default values when posting a transaction.", ItemSet.VoucherNumberOfSupportingDocumentDefaults.Hint);

			DefaultNumberOfSupportingDocumentsCollection list = new DefaultNumberOfSupportingDocumentsCollection();
			list.AddDefaultValues("ABCD", (NoResString)"ABC", (NoResString)"ABCD Desc", 1);

			ItemSet.EnableBulkDisbursementJobsClosure.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.VoucherNumberOfSupportingDocumentDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("Count", 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value.Count);
			AssertEquals("Code", "ABCD", ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[0].Code);
			AssertEquals("Description", "ABCD Desc", ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[0].Description);

			DefaultNumberOfSupportingDocumentsCollection list1 = ItemSet.LookUpCodeDescriptionNumberPairList();

			ItemSet.VoucherNumberOfSupportingDocumentDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list1);
			AssertEquals("Count", 52, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value.Count);

			AssertEquals("Code", LedgerTypes.General + TransactionTypes.GLStandardJournal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[0].Code);
			AssertEquals("Code", LedgerTypes.CashBook + TransactionTypes.DirectReceipt, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[1].Code);
			AssertEquals("Code", LedgerTypes.CashBook + TransactionTypes.DirectPayment, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[2].Code);
			AssertEquals("Code", LedgerTypes.CashBook + TransactionTypes.Transfer, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[3].Code);
			AssertEquals("Code", LedgerTypes.CashBook + TransactionTypes.ExchangeDifference, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[4].Code);

			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.Contra, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[5].Code);
			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.Invoice, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[6].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.JobARInvoice, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[7].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.ConsolARInvoice, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[8].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.ARPeriodicInvoice, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[9].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.BadDebtWriteOff, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[10].Code);
			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.CreditNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[11].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.JobARCreditNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[12].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.ConsolARCreditNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[13].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.ARPeriodicCreditNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[14].Code);

			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.AdjustmentNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[15].Code);
			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.Journal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[16].Code);
			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.Payment, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[17].Code);
			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.Receipt, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[18].Code);
			AssertEquals("Code", LedgerTypes.AccountsReceivable + TransactionTypes.Transfer, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[19].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.ARCASH, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[20].Code);

			AssertEquals("Code", LedgerTypes.AccountsPayable + TransactionTypes.Invoice, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[21].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.JobAPInvoice, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[22].Code);
			AssertEquals("Code", LedgerTypes.AccountsPayable + TransactionTypes.CreditNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[23].Code);
			AssertEquals("Code", LedgerTypes.AccountsPayable + TransactionTypes.AdjustmentNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[24].Code);
			AssertEquals("Code", LedgerTypes.AccountsPayable + TransactionTypes.Journal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[25].Code);
			AssertEquals("Code", LedgerTypes.AccountsPayable + TransactionTypes.Payment, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[26].Code);
			AssertEquals("Code", LedgerTypes.AccountsPayable + TransactionTypes.Receipt, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[27].Code);
			AssertEquals("Code", LedgerTypes.AccountsPayable + TransactionTypes.Transfer, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[28].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.APCASH, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[29].Code);

			AssertEquals("Code", LedgerTypes.UnapprovedPayableTransactions + TransactionTypes.UACreditNote, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[30].Code);
			AssertEquals("Code", LedgerTypes.UnapprovedPayableTransactions + TransactionTypes.UAInvoice, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[31].Code);
			AssertEquals("Code", LedgerTypes.TransactionsPendingAllocation + TransactionTypes.InvoicePendingAllocation, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[32].Code);

			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.ReversalRelated, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[33].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.OverpaymentRelated, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[34].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.BankFeeJournal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[35].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.ExchangeDiff, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[36].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.DiscountRelated, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[37].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.UnOverpaymentRelated, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[38].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.UnBankFeeJournal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[39].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.UnExchangeDiff, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[40].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.UnDiscountRelated, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[41].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.CASS, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[42].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.FinanceCharge, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[43].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.JCCFX, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[44].Code);
			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.BJGDM, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[45].Code);
			AssertEquals("Code", LedgerTypes.JobCosting + TransactionTypes.JobRevenueJournal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[46].Code);
			AssertEquals("Code", Constants.TransactionCategory.Codes.AutoJobRevenueJournal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[47].Code);
			AssertEquals("Code", LedgerTypes.General + ReceiptTypes.ForeignCurrencyBalance, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[48].Code);

			AssertEquals("Code", AccountingConstants.VoucherItemRegistryCode.OutstandingBalanceCurrencyAdjustmentJournal, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[49].Code);
			AssertEquals("Code", $"{LedgerTypes.General}{TransactionTypes.GLNoteJournal}", ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[50].Code);

			AssertEquals(LedgerTypes.General + TransactionTypes.GLStandardJournal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[0].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.CashBook + TransactionTypes.DirectReceipt, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[1].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.CashBook + TransactionTypes.DirectPayment, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[2].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.CashBook + TransactionTypes.Transfer, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[3].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.CashBook + TransactionTypes.ExchangeDifference, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[4].NumberOfDefault.ToZInt());

			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Contra, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[5].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Invoice, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[6].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.JobARInvoice, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[7].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.ConsolARInvoice, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[8].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.ARPeriodicInvoice, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[9].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.BadDebtWriteOff, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[10].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.CreditNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[11].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.JobARCreditNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[12].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.ConsolARCreditNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[13].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.ARPeriodicCreditNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[14].NumberOfDefault.ToZInt());

			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.AdjustmentNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[15].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Journal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[16].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Payment, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[17].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Receipt, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[18].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Transfer, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[19].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.ARCASH, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[20].NumberOfDefault.ToZInt());

			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Invoice, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[21].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.JobAPInvoice, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[22].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.CreditNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[23].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.AdjustmentNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[24].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Journal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[25].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Payment, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[26].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Receipt, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[27].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Transfer, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[28].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.APCASH, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[29].NumberOfDefault.ToZInt());

			AssertEquals(LedgerTypes.UnapprovedPayableTransactions + TransactionTypes.UACreditNote, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[30].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.UnapprovedPayableTransactions + TransactionTypes.UAInvoice, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[31].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.TransactionsPendingAllocation + TransactionTypes.InvoicePendingAllocation, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[32].NumberOfDefault.ToZInt());

			AssertEquals(AccountingConstants.VoucherItemRegistryCode.ReversalRelated, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[33].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.OverpaymentRelated, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[34].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.BankFeeJournal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[35].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.ExchangeDiff, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[36].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.DiscountRelated, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[37].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.UnOverpaymentRelated, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[38].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.UnBankFeeJournal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[39].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.UnExchangeDiff, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[40].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.UnDiscountRelated, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[41].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.CASS, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[42].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.FinanceCharge, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[43].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.JCCFX, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[43].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.VoucherItemRegistryCode.BJGDM, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[45].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.JobCosting + TransactionTypes.JobRevenueJournal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[46].NumberOfDefault.ToZInt());
			AssertEquals(Constants.TransactionCategory.Codes.AutoJobRevenueJournal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[47].NumberOfDefault.ToZInt());
			AssertEquals(LedgerTypes.General + ReceiptTypes.ForeignCurrencyBalance, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[48].NumberOfDefault.ToZInt());

			AssertEquals(AccountingConstants.VoucherItemRegistryCode.OutstandingBalanceCurrencyAdjustmentJournal, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[49].NumberOfDefault.ToZInt());
			AssertEquals($"{LedgerTypes.General}{TransactionTypes.GLNoteJournal}", 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[50].NumberOfDefault.ToZInt());
			AssertEquals(AccountingConstants.DisbursementShortfallSurplusCode.DisbursementShortfallSurplus, 1, ItemSet.VoucherNumberOfSupportingDocumentDefaults.Value[51].NumberOfDefault.ToZInt());
		}

		public void TestTaxConfiguration()
		{
			TestRegistryItem(ItemSet.MainGSTTaxID, "MainGST", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, "Default GST Tax ID (CargoWiseOne Support Only)", "This defines the default GST Tax ID. It is used as a default value for the EU Tax ID Defaulting and CASS File Import Default Tax ID registry settings.", RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport, RegistryFindBoxCollection.AccTaxRate, Guid.Empty);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.MainGSTTaxID.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.AccTaxRateTypeRated, ((GuidFindBoxRegistryEditorInfo)ItemSet.MainGSTTaxID.EditorInfo).FindBoxFilter);

			TestRegistryItem(ItemSet.MainGSTReverseTaxID, "MainGSTRev", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, "Default GST Reverse Tax ID (CargoWiseOne Support Only)", "This defines the default GST Reverse Tax ID. It is used as a default value for the EU Tax ID Defaulting registry setting.", RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport, RegistryFindBoxCollection.AccTaxRate, Guid.Empty);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.MainGSTReverseTaxID.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.AccTaxRateTypeReverseRated, ((GuidFindBoxRegistryEditorInfo)ItemSet.MainGSTReverseTaxID.EditorInfo).FindBoxFilter);

			TestRegistryItem(ItemSet.MainNotReportableTaxID, "MainNotReport", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, "Default Not Reportable Tax ID (CargoWiseOne Support Only)", "This defines the default Not Reportable Tax ID. It is used as a default value for the EU Tax ID Defaulting registry setting.", RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport, RegistryFindBoxCollection.AccTaxRate, Guid.Empty);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.MainNotReportableTaxID.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.AccTaxRateTypeNotReportable, ((GuidFindBoxRegistryEditorInfo)ItemSet.MainNotReportableTaxID.EditorInfo).FindBoxFilter);

			TestRegistryItem(ItemSet.MainFreeGSTTaxID, "MainFreeGST", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, "Default Free GST Tax ID (CargoWiseOne Support Only)", "This defines the default Free GST Tax ID. It is used as a default value for the EU Tax ID Defaulting and CASS File Import Default Tax ID registry settings.", RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport, RegistryFindBoxCollection.AccTaxRate, Guid.Empty);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.MainFreeGSTTaxID.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.AccTaxRateTypeRated, ((GuidFindBoxRegistryEditorInfo)ItemSet.MainFreeGSTTaxID.EditorInfo).FindBoxFilter);

			TestRegistryItem(ItemSet.MainFreeGSTReverseTaxID, "MainFreeGSTRev", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, "Default Free GST Reverse Tax ID (CargoWiseOne Support Only)", "This defines the default Free GST Reverse Tax ID. It is used as a default value for the EU Tax ID Defaulting registry setting.", RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport, RegistryFindBoxCollection.AccTaxRate, Guid.Empty);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.MainFreeGSTReverseTaxID.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.AccTaxRateTypeReverseRated, ((GuidFindBoxRegistryEditorInfo)ItemSet.MainFreeGSTReverseTaxID.EditorInfo).FindBoxFilter);
		}

		public void TestAccountFeeDefaultTaxID()
		{
			var taxRate = AccTaxRate.FindExistingTaxRate(Factory, null, AccTaxRate.Types.Exempt, Env.CurrentCompany.Country.Code);
			var defaultTaxID = taxRate != null ? taxRate.PK.ToGuid() : Guid.Empty;
			TestRegistryItem(ItemSet.AccountFeeDefaultTaxID, "AccountFeeDefaultTaxID", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, "Default Tax ID for Account Fee Invoice", @"This registry is referenced when creating Receivables (AR) Account Fee invoices.
This registry defines the VAT/GST Tax ID that is used when creating Receivables Account Fee Invoices.", RegistryStorageFlags.Company, RegistryOptions.Default, RegistryFindBoxCollection.AccTaxRate, defaultTaxID);
			Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.AccountFeeDefaultTaxID.EditorInfo is GuidFindBoxRegistryEditorInfo);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.VATTaxSystem, ((GuidFindBoxRegistryEditorInfo)ItemSet.AccountFeeDefaultTaxID.EditorInfo).FindBoxFilter);

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var cnCompany = objectCreator.CreateNewCompany("CNC", Core.Constants.CountryCodes.China);
			var cnBranch = objectCreator.CreateNewBranch(cnCompany, "CNB");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, cnBranch.PK.ToGuid(), objectCreator.FESDepartment.PK.ToGuid()))
			{
				taxRate = AccTaxRate.FindExistingTaxRate(Factory, "VAT6", AccTaxRate.Types.Rated, Env.CurrentCompany.Country.Code);
				defaultTaxID = taxRate != null ? taxRate.PK.ToGuid() : Guid.Empty;
				TestRegistryItem(ItemSet.AccountFeeDefaultTaxID, "AccountFeeDefaultTaxID", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, "Default Tax ID for Account Fee Invoice", @"This registry is referenced when creating Receivables (AR) Account Fee invoices.
This registry defines the VAT/GST Tax ID that is used when creating Receivables Account Fee Invoices.", RegistryStorageFlags.Company, RegistryOptions.Default, RegistryFindBoxCollection.AccTaxRate, defaultTaxID);
				Assert("EditorInfo is GuidFindBoxRegistryEditorInfo", ItemSet.AccountFeeDefaultTaxID.EditorInfo is GuidFindBoxRegistryEditorInfo);
				AssertEquals("FindBoxFilter", RegistryFindBoxFilter.VATTaxSystem, ((GuidFindBoxRegistryEditorInfo)ItemSet.AccountFeeDefaultTaxID.EditorInfo).FindBoxFilter);
			}
		}

		public void TestConsumptionTaxGroupReportingCompany()
		{
			var registerItem = ItemSet.ConsumptionTaxGroupReportingCompany;

			TestRegistryItem(registerItem,
				"ConsumptionTaxGroupReportingCompany",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"Consumption Tax Group Reporting Company",
				@"Set the value of this registry to the Company that is responsible for reporting the VAT consumption tax returns for the entire Tax Group. For the reporting Company itself, leave the registry empty.

Note: This registry is only applicable for MTD for VAT in UK.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				RegistryFindBoxCollection.GlbCompany,
				Guid.Empty);

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var cn1 = objectCreator.CreateNewCompany("CN1");
			var cn2 = objectCreator.CreateNewCompany("CN2");
			var cn3 = objectCreator.CreateNewCompany("CN3");
			Factory.Save();

			registerItem.SetValue(cn2.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			registerItem.SetValue(cn3.PK.ToGuid(), Guid.Empty, Guid.Empty, cn2.PK.ToGuid());

			AssertEquals("Default Value", Guid.Empty, registerItem.GetValueWithoutFallback(cn1.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("cn2 should set to Guid.Empty.", Guid.Empty, registerItem.GetValueWithoutFallback(cn2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("cn3 should set to cn2.PK.Guid.", cn2.PK.ToGuid(), registerItem.GetValueWithoutFallback(cn3.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestConsumptionTaxGroupReportingCompanyFailsValidation()
		{
			var registerItem = ItemSet.ConsumptionTaxGroupReportingCompany;

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var company = objectCreator.CreateNewCompany("CN1");
			Factory.Save();

			AssertEquals("The value cannot be equal to the fallback", registerItem.GetValidationErrorMessage(company.PK.ToGuid(), company.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestInvoiceDateDefaultingBehaviour()
		{
			AssertEquals("Name", "InvAndPstDateDefaultingBehaviour", ItemSet.InvAndPstDateDefaultingBehaviour.Name);
			AssertEquals("Category", Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.InvAndPstDateDefaultingBehaviour.Category);
			AssertEquals("Caption", "Invoice and Post Dates Defaulting Behavior", ItemSet.InvAndPstDateDefaultingBehaviour.Caption);
			AssertEquals("Hint", @"This registry defines the Invoice and Post Dates defaulting behaviors of Receivables Invoice (INV), Credit Note (CRD) and Adjustment Note (ADJ) transactions.

DEF - Default Behavior.
Both Invoice and Post Dates observe the respective standard registries’ configurations, such as Back Date Invoices Configuration, Allow Back Posting Sub Ledger Transactions, etc. 
In various scenarios, users with appropriate rights can modify either or both the Invoice and Post Dates before posting. These dates recorded against transactions posted in a single day can vary.

MTH - Month End Suspension Behavior.
Single Invoice Date across the Company, incrementing each day, with Automatic Calendar Month End Suspension.
A single Invoice Date will be used for all AR INV, CRD and ADJ transactions (both Job and non-Job related) created and posted in a Login Company. The Post Date will always be the same as the Invoice Date.", ItemSet.InvAndPstDateDefaultingBehaviour.Hint);

			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.InvAndPstDateDefaultingBehaviour.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.InvAndPstDateDefaultingBehaviour.Options);
			AssertEquals("Default Value", Enterprise.Accounting.Business.AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code, ItemSet.InvAndPstDateDefaultingBehaviour.DefaultValue);
		}

		public void TestInvoiceDateDefaultingBehaviourCurrentInvoiceDate()
		{
			AssertEquals("Name", "InvAndPstDateDefaultingBehaviourInstatedDate", ItemSet.InvAndPstDateDefaultingBehaviourInstatedDate.Name);
			AssertEquals("Category", "", ItemSet.InvAndPstDateDefaultingBehaviourInstatedDate.Category);
			AssertEquals("Caption", "", ItemSet.InvAndPstDateDefaultingBehaviourInstatedDate.Caption);
			AssertEquals("Hint", "", ItemSet.InvAndPstDateDefaultingBehaviourInstatedDate.Hint);

			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.InvAndPstDateDefaultingBehaviourInstatedDate.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.InvAndPstDateDefaultingBehaviourInstatedDate.Options);
			AssertEquals("Default Value", DateTime.MinValue, ItemSet.InvAndPstDateDefaultingBehaviourInstatedDate.DefaultValue);
		}

		public void TestInvoiceRollupAndGroupDescriptionRegistryItem()
		{
			AssertEquals("Name", "InvoiceRollupAndGroupDescriptionRegistryItem", ItemSet.InvoiceRollupAndGroupDescriptionRegistryItem.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.InvoiceRollupAndGroupDescriptionRegistryItem.Category);
			AssertEquals("Caption", "Invoice Roll-up / Group Description", ItemSet.InvoiceRollupAndGroupDescriptionRegistryItem.Caption);
			AssertEquals("Hint", "This registry setting allow you to configure the default Invoice Roll-up/Group Description.", ItemSet.InvoiceRollupAndGroupDescriptionRegistryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.InvoiceRollupAndGroupDescriptionRegistryItem.Storage);

			var defaultValue = ItemSet.LookupInvoiceRollupAndGroupDescriptionDefaultValues();
			AssertEquals("same count", defaultValue.Count, ItemSet.InvoiceRollupAndGroupDescriptionRegistryItem.DefaultValue.Count);

			var itemDefault = ItemSet.InvoiceRollupAndGroupDescriptionRegistryItem.DefaultValue.Cast<InvoiceRollupAndGroupDescription>();
			foreach (InvoiceRollupAndGroupDescription description in defaultValue)
			{
				AssertNotNull("item exists in collection default", itemDefault.FirstOrDefault(x => x.Style == description.Style
																							&& x.Group == description.Group
																							&& x.EnglishDescription == description.EnglishDescription));
			}
		}

		public void TestTransactionsNumberSequenceCustomisation()
		{
			AssertEquals("TransactionNumberSequenceCustomisation", ItemSet.TransactionsNumberSequenceCustomisation.Name);
			AssertEquals("Accounting", ItemSet.TransactionsNumberSequenceCustomisation.Category);
			AssertEquals("Number Sequence Customization", ItemSet.TransactionsNumberSequenceCustomisation.Caption);
			AssertEquals(@"By default, an eight characters sequential transaction number is assigned to each transaction for all non-China login companies (E.g. 00001000). For all China login companies, a ten characters sequential transaction number comprises of two digits Accounting Year, two digits Accounting Period and six digits sequential number is assigned to each transaction (E.g. 1801001000).

Typically, all transactions of a particular ledger and type (E.g. AR Receipt, AR Invoice, AP Journal) are assigned sequential numbers from a number fountain specific to the ledger and transaction type. For example, AR Receipts are numbered sequentially from one number fountain, while AP Journals will draw their sequential transaction numbers from a separate number fountain.

When this registry is overridden, the transaction number assigned to each transaction can be a mix of Alpha Numeric characters derived from a combination of the listed elements. The ‘order’ assigned against each included element determines the order in which the elements will be combined when creating and assigning new transaction numbers.

Additionally, if the ‘Fountain’ check box of an element is ticked, this will extend the basic number fountain behavior for each transaction type and ledger combination. Each unique combination of the additionally included elements with ‘Fountain’ check box ticked will be numbered sequentially from a separate number fountain.", ItemSet.TransactionsNumberSequenceCustomisation.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.TransactionsNumberSequenceCustomisation.Storage);
		}

		public void TestTransactionTypePrefix()
		{
			AssertEquals("TransactionTypePrefix", ItemSet.TransactionTypePrefix.Name);
			AssertEquals("Accounting", ItemSet.TransactionTypePrefix.Category);
			AssertEquals("Transaction Type Prefix", ItemSet.TransactionTypePrefix.Caption);
			AssertEquals(@"A new element 'Transaction Type Prefix' has been added to the Accounting > Number Sequence Customization.

This registry enables you to specify the prefix by ledger and transaction type for inclusion to transaction number and internal reference.
When specified and the 'Transaction Type Prefix' element is included in the transaction number, then this prefix will be added to the transaction number.", ItemSet.TransactionTypePrefix.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.TransactionTypePrefix.Storage);

			var newValue = new TransactionTypePrefixCollection();
			var prefix = newValue.AddNew();
			prefix.Ledger = "AR";
			prefix.TransactionType = "DSC";
			prefix.Prefix = "XX";

			ItemSet.TransactionTypePrefix.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			var registry = ItemSet.TransactionTypePrefix;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, new TransactionTypePrefixCollection(), newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(@"Added: Ledger 'AR', Transaction Type 'DSC', Prefix 'XX'" + "\r\n", logReference);

			var oldValue1 = new TransactionTypePrefixCollection();
			var prefixForOld1 = oldValue1.AddNew();
			prefixForOld1.Ledger = "AP";
			prefixForOld1.TransactionType = "DSC";
			prefixForOld1.Prefix = "ABC";

			var oldValue2 = new TransactionTypePrefixCollection();
			var prefixForOld2 = oldValue2.AddNew();
			prefixForOld2.Ledger = "AP";
			prefixForOld2.TransactionType = "DSC";
			prefixForOld2.Prefix = "tt";

			args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue1, newValue);
			logReference = registry.OnBuildLogReference(args);
			AssertEquals(@"Deleted: Ledger 'AP', Transaction Type 'DSC', Prefix 'ABC'
Added: Ledger 'AR', Transaction Type 'DSC', Prefix 'XX'" + "\r\n", logReference);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue1, oldValue2);
			logReference = registry.OnBuildLogReference(args);
			AssertEquals(@"Edited: Ledger 'AP', Transaction Type 'DSC', Prefix 'tt'" + "\r\n", logReference);
		}

		public void TestAllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate()
		{
			AssertEquals("Name", "AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate", ItemSet.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Name);
			AssertEquals("Category", "Accounting/Reversal", ItemSet.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Category);
			AssertEquals("Caption", "Allow AR Reversal Invoice Date to Default to the Original Transaction Invoice Date", ItemSet.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Caption);
			AssertEquals("Hint", @"This registry item allows the accounts receivables invoice date for miscellaneous and periodic invoice reversals to default to the original transaction's invoice date.

Note: This registry will be ignored when the ‘Invoice and Post Dates Defaulting Behavior’ registry is set to ‘MTH’.", ItemSet.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Options);
			AssertEquals("Default Value", AccountingConstants.ReversalDefaultFromOriginalTransactionDate.TodaysDate, ItemSet.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.DefaultValue);
		}

		public void TestAllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate()
		{
			AssertEquals("Name", "AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate", ItemSet.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.Name);
			AssertEquals("Category", "Accounting/Reversal", ItemSet.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.Category);
			AssertEquals("Caption", "Allow AR Reversal Post Date to Default to the Original Transaction Invoice Date", ItemSet.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.Caption);
			AssertEquals("Hint", @"This registry item allows the accounts receivables post date for miscellaneous and periodic invoice reversals to default to the original transaction's invoice date.

Note: This registry will be ignored when the ‘Invoice and Post Dates Defaulting Behavior’ registry is set to ‘MTH’.", ItemSet.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.Options);
			AssertEquals("Default Value", AccountingConstants.ReversalDefaultFromOriginalTransactionDate.TodaysDate, ItemSet.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.DefaultValue);
		}

		public void TestAllowARReversalDueDateCalculation()
		{
			AssertEquals("Name", "AllowARReversalDueDateCalculation", ItemSet.AllowARReversalDueDateCalculation.Name);
			AssertEquals("Category", "Accounting/Reversal", ItemSet.AllowARReversalDueDateCalculation.Category);
			AssertEquals("Caption", "Allow Calculation of Due Date for AR Reversals", ItemSet.AllowARReversalDueDateCalculation.Caption);
			AssertEquals("Hint", "This registry item allows the calculation of the due date for accounts receivables invoice and credit note reversals.", ItemSet.AllowARReversalDueDateCalculation.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AllowARReversalDueDateCalculation.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AllowARReversalDueDateCalculation.Options);
			AssertEquals("Default Value", AccountingConstants.ReversalDueDateCalculation.DebtorsTerms, ItemSet.AllowARReversalDueDateCalculation.DefaultValue);
		}

		public void TestAllowAPReversalDueDateCalculation()
		{
			AssertEquals("Name", "AllowAPReversalDueDateCalculation", ItemSet.AllowAPReversalDueDateCalculation.Name);
			AssertEquals("Category", "Accounting/Reversal", ItemSet.AllowAPReversalDueDateCalculation.Category);
			AssertEquals("Caption", "Allow Calculation of Due Date for AP Reversals", ItemSet.AllowAPReversalDueDateCalculation.Caption);
			AssertEquals("Hint", "This registry item allows the calculation of the due date for accounts payables invoice and credit note reversals.", ItemSet.AllowAPReversalDueDateCalculation.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AllowAPReversalDueDateCalculation.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AllowAPReversalDueDateCalculation.Options);
			AssertEquals("Default Value", AccountingConstants.ReversalDueDateCalculation.CreditorsTerms, ItemSet.AllowAPReversalDueDateCalculation.DefaultValue);
		}

		public void TestAllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate()
		{
			AssertEquals("Name", "AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate", ItemSet.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Name);
			AssertEquals("Category", "Accounting/Reversal", ItemSet.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Category);
			AssertEquals("Caption", "Allow AP Reversal Invoice Date to Default to the Original Transaction Invoice Date", ItemSet.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Caption);
			AssertEquals("Hint", "This registry item allows the accounts payables invoice and credit note reversals to have their invoice date default to the original transaction's invoice date.", ItemSet.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Options);
			AssertEquals("Default Value", false, ItemSet.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.DefaultValue);
		}

		public void TestVoucherAppointedPartiesOptions()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Taiwan);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.Default, ItemSet.VoucherAppointedPartiesCashierDefault.Options);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.Default, ItemSet.VoucherAppointedPartiesReviewerDefault.Options);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, ItemSet.VoucherAppointedPartiesCashierDefault.Options);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, ItemSet.VoucherAppointedPartiesReviewerDefault.Options);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.Default, ItemSet.VoucherAppointedPartiesCashierDefault.Options);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.Default, ItemSet.VoucherAppointedPartiesReviewerDefault.Options);
		}

		public void TestTaxRecognitionDefaultingRules()
		{
			var defaultValue = ItemSet.TaxRecognitionDefaultingRules.DefaultValue;
			AssertEquals("defaultValue.APInputGoods", TaxRecognitionDefaultingRules.RecognitionTypesAccrualCode, defaultValue.APInputGoods);
			AssertEquals("defaultValue.APInputServices", TaxRecognitionDefaultingRules.RecognitionTypesAccrualCode, defaultValue.APInputServices);
			AssertEquals("defaultValue.AROutputGoods", TaxRecognitionDefaultingRules.RecognitionTypesAccrualCode, defaultValue.AROutputGoods);
			AssertEquals("defaultValue.AROutputServices", TaxRecognitionDefaultingRules.RecognitionTypesAccrualCode, defaultValue.AROutputServices);
			AssertEquals("defaultValue.APOrganizationOverride", TaxRecognitionDefaultingRules.OrganisationOverrideTypesNoCode, defaultValue.APOrganizationOverride);
			AssertEquals("defaultValue.AROrganizationOverride", TaxRecognitionDefaultingRules.OrganisationOverrideTypesNoCode, defaultValue.AROrganizationOverride);

			AssertEquals("Category", "Accounting/Cash Basis Tax Configuration", ItemSet.TaxRecognitionDefaultingRules.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.TaxRecognitionDefaultingRules.Storage);

			var expectedRecognitionType = TaxRecognitionDefaultingRules.RecognitionTypesCashCode;
			var expectedOrganisationOverrideType = TaxRecognitionDefaultingRules.OrganisationOverrideTypesYesCode;
			var newValue = new TaxRecognitionDefaultingRules();
			newValue.APInputGoods = expectedRecognitionType;
			newValue.APInputServices = expectedRecognitionType;
			newValue.AROutputGoods = expectedRecognitionType;
			newValue.AROutputServices = expectedRecognitionType;
			newValue.APOrganizationOverride = expectedOrganisationOverrideType;
			newValue.AROrganizationOverride = expectedOrganisationOverrideType;

			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.TaxRecognitionDefaultingRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newValue));

			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsGSTCashBasis = true;
			Factory.Save();
			ItemSet.TaxRecognitionDefaultingRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newValue);
			var value = ItemSet.TaxRecognitionDefaultingRules.Value;
			AssertEquals("APInputGoods", expectedRecognitionType, value.APInputGoods);
			AssertEquals("APInputServices", expectedRecognitionType, value.APInputServices);
			AssertEquals("AROutputGoods", expectedRecognitionType, value.AROutputGoods);
			AssertEquals("AROutputServices", expectedRecognitionType, value.AROutputServices);
			AssertEquals("APOrganizationOverride", expectedOrganisationOverrideType, value.APOrganizationOverride);
			AssertEquals("AROrganizationOverride", expectedOrganisationOverrideType, value.AROrganizationOverride);
		}

		public void TestPartPaymentTaxRealizationRule()
		{
			var defaultValue = ItemSet.PartPaymentTaxRealizationRule.DefaultValue;
			AssertEquals("DefaultValue", AccountingConstants.PartPaymentTaxRealizationRuleTypes.Proportionally.Code, defaultValue);
			AssertEquals("Category", "Accounting/Cash Basis Tax Configuration", ItemSet.TaxRecognitionDefaultingRules.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.TaxRecognitionDefaultingRules.Storage);

			var expectedValue = AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code;
			ItemSet.PartPaymentTaxRealizationRule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, expectedValue);
			AssertEquals(expectedValue, ItemSet.PartPaymentTaxRealizationRule.Value);
		}

		public void TestPendingGSTInputControlAccount()
		{
			AssertType(typeof(PendingGSTInputControlAccountDataType), ItemSet.PendingGSTInputControlAccount.DataType);

			var filter = new ZQuery(AccGLHeaderSchema.AG_ControlAccount, ZBool.True);
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "BSH");
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_IsActive, true);
			var glHeader = Factory.LoadTop1<AccGLHeader>(filter);

			ItemSet.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			AssertEquals(glHeader.PK, ItemSet.PendingGSTInputControlAccount.Value);
			AssertEquals("Hint", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Input Tax amounts.
All VAT/GST tax amounts posted on Payables Invoice, Credit Note and Adjustment Note transactions post through the relevant Input Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Input Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Payments post to the Reportable Input Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Input Control Account on the transaction Post Date, and subsequently posts through to the Reportable Input Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Input Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.", ItemSet.PendingGSTInputControlAccount.Hint);
		}

		public void TestPendingGSTInputControlAccountDefaultsCase1()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTCashBasis = false);
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory;
			AssertEquals(expectedOptions, ItemSet.PendingGSTInputControlAccount.Options);
		}

		public void TestPendingGSTInputControlAccountDefaultsCase2()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTCashBasis = false);
			companies[0].GC_IsGSTCashBasis = true;
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue;
			AssertEquals(expectedOptions, ItemSet.PendingGSTInputControlAccount.Options);
		}

		public void TestPendingGSTOutputControlAccount()
		{
			AssertType(typeof(PendingGSTOutputControlAccountDataType), ItemSet.PendingGSTOutputControlAccount.DataType);

			var filter = new ZQuery(AccGLHeaderSchema.AG_ControlAccount, ZBool.True);
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "BSH");
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_IsActive, true);
			var glHeader = Factory.LoadTop1<AccGLHeader>(filter);

			ItemSet.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			AssertEquals(glHeader.PK, ItemSet.PendingGSTOutputControlAccount.Value);
			AssertEquals("Hint", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Output Tax amounts.
All VAT/GST tax amounts posted on Receivables Invoice, Credit Note and Adjustment Note transactions post through the relevant Output Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Output Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Receipt post to the Reportable Output Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Output Control Account on the transaction Post Date, and subsequently posts through to the Reportable Output Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Output Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.", ItemSet.PendingGSTOutputControlAccount.Hint);
		}

		public void TestPendingGSTOutputControlAccountDefaultsCase1()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTCashBasis = false);
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory;
			AssertEquals(expectedOptions, ItemSet.PendingGSTOutputControlAccount.Options);
		}

		public void TestPendingGSTOutputControlAccountDefaultsCase2()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTCashBasis = false);
			companies[0].GC_IsGSTCashBasis = true;
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue;
			AssertEquals(expectedOptions, ItemSet.PendingGSTOutputControlAccount.Options);
		}

		public void TestGSTInputControlAccountDefaultsCase1()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTRegistered = false);
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory;
			AssertEquals(expectedOptions, ItemSet.GSTInputControlAccount.Options);
		}

		public void TestGSTInputControlAccountDefaultsCase2()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTRegistered = false);
			companies[0].GC_IsGSTRegistered = true;
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue;
			AssertEquals(expectedOptions, ItemSet.GSTInputControlAccount.Options);
		}

		public void TestGSTOutputControlAccountDefaultsCase1()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTRegistered = false);
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory;
			AssertEquals(expectedOptions, ItemSet.GSTOutputControlAccount.Options);
		}

		public void TestGSTOutputControlAccountDefaultsCase2()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ToList().ForEach(item => item.GC_IsGSTRegistered = false);
			companies[0].GC_IsGSTRegistered = true;
			Factory.Save();

			var expectedOptions = RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue;
			AssertEquals(expectedOptions, ItemSet.GSTOutputControlAccount.Options);
		}

		public void TestGetRegistryItemsByCategoryName()
		{
			var linkAccountPath = AccountingConfigurationRegistry.Categories.Accounting_GeneralLedgerDefaults_LinkAccount;
			var controlAccountPath = AccountingConfigurationRegistry.Categories.Accounting_GeneralLedgerDefaults_ControlAccount;
			var journalsApprovalPath = AccountingConfigurationRegistry.Categories.Accounting_GeneralLedgerDefaults_GLJournalsApproval;

			var items = ItemSet.GetRegistryItemsByCategoryName(linkAccountPath, controlAccountPath, journalsApprovalPath);

			foreach (var item in items)
			{
				Assert(item.Category == linkAccountPath || item.Category == controlAccountPath || item.Category == journalsApprovalPath);
			}
		}

		public void TestPLAppropriationAccount()
		{
			var gl = RegistryFactory.Instance.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.PLAppropriationAccount);
			AssertNotNull("Default GL Account should exist in DB.", gl);

			TestGenericRegistryItem(ItemSet.PLAppropriationAccount,
				"GL_PL_APPROPRIATION_ACCOUNT",
				"Accounting/Framework",
				"PL Appropriation Account",
				"PL Appropriation Account",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				gl.PK);
		}

		public void TestPLAppropriationAccountNoDefaultValueInDb()
		{
			var gl = RegistryFactory.Instance.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.PLAppropriationAccount);
			gl.Delete();
			Factory.Save();

			TestGenericRegistryItem(ItemSet.PLAppropriationAccount,
				"GL_PL_APPROPRIATION_ACCOUNT",
				"Accounting/Framework",
				"PL Appropriation Account",
				"PL Appropriation Account",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				Guid.Empty);
		}

		public void TestJobCostingQueueProcessBatchSize()
		{
			AssertEquals("DefaultValue", 20000, ItemSet.JobCostingQueueProcessBatchSize.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobCostingReports, ItemSet.JobCostingQueueProcessBatchSize.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.JobCostingQueueProcessBatchSize.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.JobCostingQueueProcessBatchSize.Options);
			AssertEquals("Caption", (NoResString)"Job Costing Queue process batch size (CargoWiseOne Support Only)", ItemSet.JobCostingQueueProcessBatchSize.Caption);
			AssertEquals("Hint", (NoResString)@"This registry is used to configure the number of records processed by the 'JCD' service task every time it runs.

By default this registry is set to 20000.", ItemSet.JobCostingQueueProcessBatchSize.Hint);

			IntRegistryDataType dataType = (IntRegistryDataType)ItemSet.JobCostingQueueProcessBatchSize.DataType;
			AssertEquals("Max", 99999999, (int)dataType.UpperBound);
			AssertEquals("Min", 0, (int)dataType.LowerBound);
			ItemSet.JobCostingQueueProcessBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 600);
			AssertEquals("Value", 600, ItemSet.JobCostingQueueProcessBatchSize.Value);
		}

		public void TestTransactionLineToJobCostingRecordTransformationBatchSize()
		{
			AssertEquals("DefaultValue", 1000, ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobCostingReports, ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.Options);
			AssertEquals("Caption", (NoResString)"Transaction Line To JobCosting record Transformation Batch Size (CargoWiseOne Support Only)", ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.Caption);
			AssertEquals("Hint", (NoResString)@"This registry is used to configure the number of AccTransactionLine record transferred to JobCostingQueue table in each iteration. This transformation is done by the 'ODT' service task.

By default this registry is set to 1000.", ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.Hint);

			ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 500);
			AssertEquals("Value", 500, ItemSet.TransactionLineToJobCostingRecordTransformationBatchSize.Value);
		}

		public void TestEnableLightValidationForChargeAndConsolCost()
		{
			AssertEquals("EnableLightValidationForChargeAndConsolCost", ItemSet.EnableLightValidationForChargeAndConsolCost.Name);
			AssertEquals("Accounting", ItemSet.EnableLightValidationForChargeAndConsolCost.Category);
			AssertEquals("Enable Light Validation For Charge And Consol Cost (CargoWiseOne Support Only)", ItemSet.EnableLightValidationForChargeAndConsolCost.Caption);
			AssertEquals(@"By default light validation is disabled for consol cost and charge.
In case of emergency where it is impossible for a client to continue usual operations with light validation disabled, turn on this registry to enable light validation and make the system behave as before.",
ItemSet.EnableLightValidationForChargeAndConsolCost.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnableLightValidationForChargeAndConsolCost.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableLightValidationForChargeAndConsolCost.Options);
			AssertEquals(false, ItemSet.EnableLightValidationForChargeAndConsolCost.DefaultValue);
		}

		public void TestEnablePostTransactionCalculatedPropertyCache()
		{
			AssertEquals("EnablePostTransactionCalculatedPropertyCache", ItemSet.EnablePostTransactionCalculatedPropertyCache.Name);
			AssertEquals("Accounting", ItemSet.EnablePostTransactionCalculatedPropertyCache.Category);
			AssertEquals("Enable Post Transaction Calculated Property Cache", ItemSet.EnablePostTransactionCalculatedPropertyCache.Caption);
			AssertEquals("Enable this registry to cache those heavy calculated properties of Accounting transactions. It will significantly improve the performance when posting Accounting transactions.",
ItemSet.EnablePostTransactionCalculatedPropertyCache.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnablePostTransactionCalculatedPropertyCache.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnablePostTransactionCalculatedPropertyCache.Options);
			AssertEquals(false, ItemSet.EnablePostTransactionCalculatedPropertyCache.DefaultValue);
		}

		public void TestEnableValidationForChargeWhenPostTransactions()
		{
			AssertEquals("EnableValidationForChargeWhenPostTransactions", ItemSet.EnableValidationForChargeWhenPostTransactions.Name);
			AssertEquals("Accounting", ItemSet.EnableValidationForChargeWhenPostTransactions.Category);
			AssertEquals("Enable Validation For Charge When Post Transactions (CargoWiseOne Support Only)", ItemSet.EnableValidationForChargeWhenPostTransactions.Caption);
			AssertEquals(@"By default charge validation is enabled when client post revenue or cost.
In case of emergency where it is impossible for a client to continue usual operations with validation enabled, turn off this registry to disable charge validation and make the system behave as before.",
ItemSet.EnableValidationForChargeWhenPostTransactions.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnableValidationForChargeWhenPostTransactions.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableValidationForChargeWhenPostTransactions.Options);
			AssertEquals(true, ItemSet.EnableValidationForChargeWhenPostTransactions.DefaultValue);
		}

		public void TestJCDServiceTaskController()
		{
			AssertEquals("DefaultValue", "NON", ItemSet.JCDServiceTaskController.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobCostingReports, ItemSet.JCDServiceTaskController.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.JCDServiceTaskController.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.JCDServiceTaskController.Options);
			AssertEquals("Caption", "Job Costing Data Queue Service task controller", ItemSet.JCDServiceTaskController.Caption);
			AssertEquals("Hint", "This registry is used to Initialize, Remove or Re-initialize Job Costing Data Queue Service task", ItemSet.JCDServiceTaskController.Hint);

			ItemSet.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			AssertEquals("Value", "STR", ItemSet.JCDServiceTaskController.Value);
		}

		public void TestBankCurrencyAdjustmentExchangeRateType()
			=> TestBankCurrencyAdjustmentExchangeRateType(AccountingMasterFilesConstants.GetDefaultExchangeRateTypesList());

		public void TestBankCurrencyAdjustmentExchangeRateType_ListIsUpdatedWhenCurrencyExchangeRateTypesIsChanged()
		{
			var allEnabledList = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();
			foreach (CodeDescriptionBool item in AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value)
			{
				allEnabledList.Add(item.Code, item.Description, true);
			}
			using (AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allEnabledList))
			{
				TestBankCurrencyAdjustmentExchangeRateType(AccountingMasterFilesConstants.GetExchangeRateTypesList_SystemLevel());
			}
		}

		void TestBankCurrencyAdjustmentExchangeRateType(CodeDescriptionPairList expectedList)
		{
			TestRegistryItem(ItemSet.BankCurrencyAdjustmentExchangeRateType,
				"BankCurrencyAdjustmentExchangeRateType",
				AccountingMasterFilesRegistry.Categories.Accounting_GeneralLedgerDefaults,
				"Bank Currency Adjustment Exchange Rate Type", @"This registry defines the exchange rate type that will be used when performing Bank Currency Adjustment. 

By default, this registry will be set to ‘PER’ exchange rate type. 

Note: 
For ‘PER’ exchange rate type, the system will fallback to ‘BUY’ exchange rate type if a ‘PER’ exchange rate cannot be found.
For all other exchange rate types, there will be no fallback logic.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedList,
				Constants.ExchangeRateTypes.Code.PeriodEndRate);

			var registry = AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType;
			var oldValue = Constants.ExchangeRateTypes.Code.PeriodEndRate;
			var newValue = Constants.ExchangeRateTypes.Code.BuyRate;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestAccountingRegistryItems_ShouldBePWVisible()
		{
			Assert("We should be able to see Accounting Registry Items with ProductivityWise Enabled, so this value should be set to true, and yet...", AccountingConfigurationRegistry.Instance.IsForProductivityWise);
		}

		public void TestContainerYardJobsDefaultDepts()
		{
			var testGroup = Guid.NewGuid();
			AssertEquals("Default value", new Guid("E64F8690-E35E-40B7-8FEE-D0D873970975"), ItemSet.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
			ItemSet.ContainerYardJobsDefaultDept.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("Set value", testGroup, ItemSet.ContainerYardJobsDefaultDept.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestMaximumNumberOfInvoicesAllowedOnJob()
		{
			AssertEquals("DefaultValue", 703, ItemSet.MaximumNumberOfInvoicesAllowedOnJob.DefaultValue);
			AssertEquals("Category", AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing, ItemSet.MaximumNumberOfInvoicesAllowedOnJob.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.MaximumNumberOfInvoicesAllowedOnJob.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.MaximumNumberOfInvoicesAllowedOnJob.Options);
			AssertEquals("Caption", "Maximum Number of Invoices Allowed on Job", ItemSet.MaximumNumberOfInvoicesAllowedOnJob.Caption);
			AssertEquals("Hint", "This registry is used to configure the maximum number of invoices allowed on the Job.", ItemSet.MaximumNumberOfInvoicesAllowedOnJob.Hint);

			ItemSet.MaximumNumberOfInvoicesAllowedOnJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 600);
			AssertEquals("Value", 600, ItemSet.MaximumNumberOfInvoicesAllowedOnJob.Value);
			AssertEquals("Value must be less than or equal to the maximum (703)", ItemSet.MaximumNumberOfInvoicesAllowedOnJob.GetValidationErrorMessage(7000, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestJobStatusUpdateRestrictionRule()
		{
			TestGenericRegistryItem(ItemSet.JobStatusUpdateRestrictionRule,
			"JobStatusUpdateRestrictionRule",
			Categories.Accounting_JobInvoicing,
			"Job Status Update Restriction Rule",
			@"This registry defines the Job Status Update Restriction Rules for transitioning of job status prior to job closure.

The grid below is pre-populated with the current job status update restriction rule.
If required, you can override the restriction rule and configure according to your internal control policy.

Each row represents the 'From' job status and the related security right to update the job status when restriction applies.
Each column represents the 'To' job status.
Each cell indicates if security right is required to update the job status.
If the value is set to 'No, then no restriction will be applied. All user will be able to update the job status to the specific 'To' job status.
If the value is set to ‘Yes’, then the login user will need to have the related security right to be able to update the job status to the specific 'To' job status.

Note:
The system will always enforce restriction when a job is closed (i.e. job status is changed from 'JFC' to 'CLS'). Login user must have the 'Close Single Job' or 'Close Multiple Jobs' security right depending on whether one or more jobs are closed at the same time.
The system will always enforce restriction when a job is reopened (i.e. job status is changed from 'CLS - Closed' to other job status). Login user must have the 'Reopen Jobs' or 'Allow Reopen Jobs Past Allowed Reopen Period' security right depending on whether re-open restriction is applicable.
Reopen restriction is defined under Auto Job Closure Configuration.",
			RegistryStorageFlags.System | RegistryStorageFlags.Company,
			RegistryOptions.Default);

			var registry = Instance.JobStatusUpdateRestrictionRule;
			var oldCollection = registry.Value;
			var newCollection = (JobStatusUpdateRestrictionRuleCollection)registry.Value.Clone(null, null);
			var workingRule = newCollection.Cast<JobStatusUpdateRestrictionRule>().FirstOrDefault(x => x.JobStatus == "WRK");
			workingRule.Complete = "YES";

			var completeRule = newCollection.Cast<JobStatusUpdateRestrictionRule>().FirstOrDefault(x => x.JobStatus == "CMP");
			completeRule.Working = "NO";
			completeRule.WorkOnHold = "NO";
			completeRule.InvoiceOnHold = "NO";
			completeRule.CustomsProcessActive = "NO";
			completeRule.JobReadyForRevenueAndCostPosting = "NO";
			completeRule.JobReadyForRevenuePosting = "NO";
			completeRule.JobReadyForCostPosting = "NO";
			completeRule.JobReadyForDelivery = "NO";
			completeRule.JobInvoiced = "NO";
			completeRule.JobReadyForFinancialClosure = "NO";
			completeRule.ScheduledForArchive = "NO";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldCollection, newCollection);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(@"WRK (Working): CMP changed from [NO] to [YES]
CMP (Complete): WRK changed from [YES] to [NO], WHL changed from [YES] to [NO], IHL changed from [YES] to [NO], CUS changed from [YES] to [NO], JRA changed from [YES] to [NO], JRB changed from [YES] to [NO], JRC changed from [YES] to [NO], RDD changed from [YES] to [NO], INV changed from [YES] to [NO], JFC changed from [YES] to [NO], ARC changed from [YES] to [NO]", logReference);
		}

		public void TestReceivableAllowUserToModifyTaxMessage()
		{
			AssertEquals("DefaultValue", true, ItemSet.ReceivableAllowUserToModifyTaxMessage.DefaultValue);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.ReceivableAllowUserToModifyTaxMessage.Category);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.ReceivableAllowUserToModifyTaxMessage.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ReceivableAllowUserToModifyTaxMessage.Options);
			AssertEquals("Caption", "Allow user to modify Tax Message", ItemSet.ReceivableAllowUserToModifyTaxMessage.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', authorized users will be able to override Invoice Sell Tax Message / AR Invoice Tax Message defaulted.", ItemSet.ReceivableAllowUserToModifyTaxMessage.Hint);

			ItemSet.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.ReceivableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			ItemSet.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ReceivableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestDisplayAccumulativeTotalAmountsInMultipageInvoices()
		{
			var expectedHint = @"Set this registry to 'YES', to add carry over amounts to the invoice body header and footer of invoices that are more than one page long.

A subtotal of charges will be printed on the footer of each page of the invoice, except the last page of the invoice.

The invoice carry forward subtotal from the footer on one page will be brought forward to the header of the next page.";

			TestGenericRegistryItem(ItemSet.DisplayAccumulativeTotalAmountsInMultipageInvoices,
				"DisplayAccumulativeTotalAmountsInMultipageInvoices",
				"Accounting/Receivable Defaults/Form Configurations/Invoice",
				"Include Carry Over Amounts in Invoices that have Multiple Pages",
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestPrintQuanityInInvoiceDocument()
		{
			var expectedHint = @"This registry controls the ability to include a quantity per charge line in printed 'DocBuilder' invoices and credit notes.

Set this registry to 'Yes' to include a quantity column in your printed invoices and credit notes.

Note: The quantity per charge line will only print when no summarization or roll up options are configured for printing invoices including periodic and warehouse periodic invoices.";

			TestGenericRegistryItem(ItemSet.PrintQuanityInInvoiceDocument,
				"PrintQuanityInInvoiceDocument",
				"Accounting/Receivable Defaults/Form Configurations/Invoice",
				"Print quantity in AR invoice charge lines",
				expectedHint,
				RegistryStorageFlags.Company,
				false);
		}

		public void TestOriginalInvoiceDetailsMandatoryOnARCreditNotes()
		{
			var expectedHint = @"This registry allows you to enforce that users must enter the Original Invoice details when creating new Credit Notes.
By default, this registry is set to No and the Original Invoice details are not mandatory on Credit Notes.
When you set this registry to Yes, when adding new Credit Notes in the Receivables Transactions module, the users must either select the Original Invoice Reference or manually enter the Original Invoice Number and the Original Invoice Date.";

			TestGenericRegistryItem(ItemSet.OriginalInvoiceDetailsMandatoryOnARCreditNotes,
				"OriginalInvoiceDetailsMandatoryOnARCreditNotes",
				"Accounting/Receivable Defaults/Default Settings",
				"Original Invoice Details Mandatory on AR Credit Notes",
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestOriginalInvoiceDetailsMandatoryOnAPCreditNotes()
		{
			var expectedHint = @"This registry allows you to enforce that users must enter the Original Invoice details when creating new Credit Notes.
By default, this registry is set to No and the Original Invoice details are not mandatory on Credit Notes.
When you set this registry to Yes, when adding new Credit Notes in the Payables Transactions module, the users must either select the Original Invoice Reference or manually enter the Original Invoice Number and the Original Invoice Date.";

			TestGenericRegistryItem(ItemSet.OriginalInvoiceDetailsMandatoryOnAPCreditNotes,
				"OriginalInvoiceDetailsMandatoryOnAPCreditNotes",
				"Accounting/Payable Defaults/Default Settings",
				"Original Invoice Details Mandatory on AP Credit Notes",
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestOriginalInvoiceDetailsMandatoryOnARDebitNotes()
		{
			var expectedHint = @"This registry allows you to enforce that users must enter the Original Invoice details when creating new Debit Notes.
By default, this registry is set to No and the Original Invoice details are not mandatory on Debit Notes.
When you set this registry to Yes, when adding new Debit Notes in the Receivables Transactions module, the users must either select the Original Invoice Reference or manually enter the Original Invoice Number and the Original Invoice Date.";

			TestGenericRegistryItem(ItemSet.OriginalInvoiceDetailsMandatoryOnARDebitNotes,
				"OriginalInvoiceDetailsMandatoryOnARDebitNotes",
				"Accounting/Receivable Defaults/Default Settings",
				"Original Invoice Details Mandatory on AR Debit Notes",
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestEnableTransactionLineMonitor()
		{
			TestGenericRegistryItem(ItemSet.EnableTransactionLineMonitor,
				"EnableTransactionLineMonitor",
				Categories.Accounting_Temp,
				"Enable Transaction Line Monitor (CargoWiseOne Support Only)",
				"Enable the feature to track transaction line changes.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestShareSequentialARComplianceDocumentsReferenceNumbers()
		{
			AssertEquals("ShareSequentialARComplianceDocumentsReferenceNumbers", ItemSet.ShareSequentialARComplianceDocumentsReferenceNumbers.Name);
			AssertEquals("Accounting/Government Compliance Invoice Document", ItemSet.ShareSequentialARComplianceDocumentsReferenceNumbers.Category);
			AssertEquals("Share Sequential Internal Reference For A/R Compliance Documents", ItemSet.ShareSequentialARComplianceDocumentsReferenceNumbers.Caption);
			AssertEquals(@"This registry is only relevant to system companies with the new Compliance Document Module enabled.
	Use this registry to control how Internal References are allocated to A/R Invoice and Credit Note Compliance Documents.
	By default, each transaction type (INV and CRD) will have its own separate number sequence.
	When this registry is set to 'Yes', A/R Invoice and Credit Note Compliance Documents will all be assigned a sequential reference number from a single number sequence.

	Important Note: A change in the registry value could result in two compliance document having the same Internal Reference Number. Please consult the Accounting Product team before a change is made. 

	For instance, the registry was originally set to 'Yes' and a CRD compliance document has been saved with Internal Reference 00001000.
	If the registry is changed to 'No', the next CRD compliance document will be assigned Internal Reference 00001000 and trigger the unique constraint error.", ItemSet.ShareSequentialARComplianceDocumentsReferenceNumbers.Hint);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				Assert(ItemSet.ShareSequentialARComplianceDocumentsReferenceNumbers.DefaultValue.Value);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Assert(!ItemSet.ShareSequentialARComplianceDocumentsReferenceNumbers.DefaultValue.Value);
			}
		}

		public void TestAutoJobClosureProcessUnboundedBatchSize()
		{
			AssertEquals("Category", Categories.Accounting_AutoJobClosure, ItemSet.AutoJobClosureProcessUnboundedBatchSize.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoJobClosureProcessUnboundedBatchSize.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AutoJobClosureProcessUnboundedBatchSize.Options);
			AssertEquals("DefaultValue", true, ItemSet.AutoJobClosureProcessUnboundedBatchSize.DefaultValue);
			AssertEquals("Caption", "Unbounded Auto Job Closure process batch size", ItemSet.AutoJobClosureProcessUnboundedBatchSize.Caption);

			var expectedHint = @"This registry is used to bound/unbound the number of jobs processed by the Job Closure Service Task every time it runs.";

			AssertEquals("Hint", expectedHint, ItemSet.AutoJobClosureProcessUnboundedBatchSize.Hint);

			ItemSet.AutoJobClosureProcessUnboundedBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.AutoJobClosureProcessUnboundedBatchSize.Value);
		}

		public void TestAutoJobClosureProcessBatchSize()
		{
			AssertEquals("DefaultValue", 250, ItemSet.AutoJobClosureProcessBatchSize.DefaultValue);
			AssertEquals("Category", Categories.Accounting_AutoJobClosure, ItemSet.AutoJobClosureProcessBatchSize.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoJobClosureProcessBatchSize.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AutoJobClosureProcessBatchSize.Options);
			AssertEquals("Caption", "Auto Job Closure process batch size", ItemSet.AutoJobClosureProcessBatchSize.Caption);

			var expectedHint = @"This registry is used to configure the number of jobs processed by the Job Closure Service Task every time it runs. 

By default this registry is set to 250. Maximum batch size is 100,000

Note: This batch size may impact the amount of memory taken up by Job Closure Service Task. If you need to automatically close a large number of jobs, you can set a shorter recurrence pattern for Job Closure Service Task.";
			AssertEquals("Hint", expectedHint, ItemSet.AutoJobClosureProcessBatchSize.Hint);
			AssertEquals("MaxValue", 100000D, (ItemSet.AutoJobClosureProcessBatchSize.DataType as IntRegistryDataType).UpperBound);
			AssertEquals("MinValue", 0D, (ItemSet.AutoJobClosureProcessBatchSize.DataType as IntRegistryDataType).LowerBound);

			ItemSet.AutoJobClosureProcessBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			AssertEquals("Value", 20, ItemSet.AutoJobClosureProcessBatchSize.Value);
		}

		public void TestLastUTCDateTimeOfReachingTheHighestJCSWatermark()
		{
			AssertEquals("DefaultValue", DateTime.MinValue, ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.DefaultValue);
			AssertEquals("Category", Categories.Accounting_AutoJobClosure, ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Options);
			AssertEquals("Caption", "Last time when JCS completed scanning all jobs (CargoWiseOne Support Only)", ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Caption);

			var expectedHint = @"This registry is used to check when last time Auto Job Closure service task (JCS) finished scanning all jobs for auto closure. This registry is updated by the service task once it finishes scanning all jobs available in JobHeader table.";
			AssertEquals("Hint", expectedHint, ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Hint);

			var regValue = DateTime.Today;
			ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today);
			AssertEquals("Value", regValue, ItemSet.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Value);
		}

		public void TestAutoJobClosureProcessingWatermark()
		{
			AssertEquals("DefaultValue", DateTime.MinValue, ItemSet.AutoJobClosureProcessingWatermark.DefaultValue);
			AssertEquals("Category", Categories.Accounting_AutoJobClosure, ItemSet.AutoJobClosureProcessingWatermark.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoJobClosureProcessingWatermark.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AutoJobClosureProcessingWatermark.Options);
			AssertEquals("Caption", "Auto Job Closure processing watermark (CargoWiseOne Support Only)", ItemSet.AutoJobClosureProcessingWatermark.Caption);

			var expectedHint = @"This registry is used to track up-to which date Auto Job Closure service task finished scanning jobs for auto closure.";
			AssertEquals("Hint", expectedHint, ItemSet.AutoJobClosureProcessingWatermark.Hint);

			var regValue = DateTime.Today;
			ItemSet.AutoJobClosureProcessingWatermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today);
			AssertEquals("Value", regValue, ItemSet.AutoJobClosureProcessingWatermark.Value);
		}

		public void TestAutoJobClosureQueueMaximumLength()
		{
			AssertEquals("DefaultValue", 100000, ItemSet.AutoJobClosureQueueMaximumLength.DefaultValue);
			AssertEquals("Category", Categories.Accounting_AutoJobClosure, ItemSet.AutoJobClosureQueueMaximumLength.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoJobClosureQueueMaximumLength.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AutoJobClosureQueueMaximumLength.Options);
			AssertEquals("Caption", "Maximum number of jobs allowed in the queue", ItemSet.AutoJobClosureQueueMaximumLength.Caption);
			AssertEquals("Lower Bound", 0D, (ItemSet.AutoJobClosureQueueMaximumLength.DataType as IntRegistryDataType).LowerBound);
			AssertEquals("Upper Bound", 1000000D, (ItemSet.AutoJobClosureQueueMaximumLength.DataType as IntRegistryDataType).UpperBound);

			var expectedHint = @"This registry determines maximum number of jobs that can be queued for auto closure at a time. Once Auto Job Closure service task completes processing queued jobs, a new batch of jobs will be queued.";
			AssertEquals("Hint", expectedHint, ItemSet.AutoJobClosureQueueMaximumLength.Hint);

			ItemSet.AutoJobClosureQueueMaximumLength.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10001);
			AssertEquals("Value", 10001, ItemSet.AutoJobClosureQueueMaximumLength.Value);
		}

		public void TestAutoCompactAccGLAggregateLastCompanyPeriod()
		{
			AssertEquals("DefaultValue", string.Empty, ItemSet.AutoCompactAccGLAggregateLastCompanyPeriod.DefaultValue);
			AssertEquals("Category", Categories.Accounting_AutoCompactAccGLAggregate, ItemSet.AutoCompactAccGLAggregateLastCompanyPeriod.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoCompactAccGLAggregateLastCompanyPeriod.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AutoCompactAccGLAggregateLastCompanyPeriod.Options);
			AssertEquals("Caption", "Last company/period compacted", ItemSet.AutoCompactAccGLAggregateLastCompanyPeriod.Caption);

			var expectedHint = @"This registry stores the last company PK and period that was processed by the Compact General Ledger Aggregate Service Task.

The service task processes each combination of login company and period in turn. This data records the last company/period which was processed and determines where the service task will run from next.

Clear this registry to begin processing from the first company/period. 
The data format is: '<company PK>|<period>'.";
			AssertEquals("Hint", expectedHint, ItemSet.AutoCompactAccGLAggregateLastCompanyPeriod.Hint);
		}

		public void TestAutoCompactAccGLAggregateRunDuration()
		{
			AssertEquals("DefaultValue", 4m, ItemSet.AutoCompactAccGLAggregateRunDuration.DefaultValue);
			AssertEquals("Category", Categories.Accounting_AutoCompactAccGLAggregate, ItemSet.AutoCompactAccGLAggregateRunDuration.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoCompactAccGLAggregateRunDuration.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AutoCompactAccGLAggregateRunDuration.Options);
			AssertEquals("Caption", "Maximum run time (in hours) of Compact General Ledger Aggregate Service Task", ItemSet.AutoCompactAccGLAggregateRunDuration.Caption);
			AssertEquals("Lower Bound", 0d, (ItemSet.AutoCompactAccGLAggregateRunDuration.DataType as DecimalRegistryDataType).LowerBound);
			AssertEquals("Upper Bound", 24d, (ItemSet.AutoCompactAccGLAggregateRunDuration.DataType as DecimalRegistryDataType).UpperBound);

			var editorInfo = ItemSet.AutoCompactAccGLAggregateRunDuration.EditorInfo;
			Assert("EditorInfo", editorInfo is NumericRegistryEditorInfo);
			AssertEquals("EditorInfo.DecimalPlaces", 4, ((NumericRegistryEditorInfo)editorInfo).DecimalPlaces);

			var expectedHint = @"This registry determines the maximum time, in hours, that the Compact General Ledger Aggregate Service Task will run for.

Use 1 for 1 hour, and therefore 0.5 for 30 minutes, 0.25 for 15 minutes, etc.
If 0 is entered, the service task will compact only 1 company/period per run.

You should set this registry value in conjunction with the Compact General Ledger Aggregate Service Task scheduled frequency.";
			AssertEquals("Hint", expectedHint, ItemSet.AutoCompactAccGLAggregateRunDuration.Hint);
		}

		public void TestAutoCompactAccGLAggregateThreshold()
		{
			AssertEquals("DefaultValue", 0.1m, ItemSet.AutoCompactAccGLAggregateThreshold.DefaultValue);
			AssertEquals("Category", Categories.Accounting_AutoCompactAccGLAggregate, ItemSet.AutoCompactAccGLAggregateThreshold.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoCompactAccGLAggregateThreshold.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AutoCompactAccGLAggregateThreshold.Options);
			AssertEquals("Caption", "Minimum threshold to reach for rows to be compacted", ItemSet.AutoCompactAccGLAggregateThreshold.Caption);
			AssertEquals("Lower Bound", 0d, (ItemSet.AutoCompactAccGLAggregateThreshold.DataType as DecimalRegistryDataType).LowerBound);
			AssertEquals("Upper Bound", 1d, (ItemSet.AutoCompactAccGLAggregateThreshold.DataType as DecimalRegistryDataType).UpperBound);

			var editorInfo = ItemSet.AutoCompactAccGLAggregateThreshold.EditorInfo;
			Assert("EditorInfo", editorInfo is NumericRegistryEditorInfo);
			AssertEquals("EditorInfo.DecimalPlaces", 2, ((NumericRegistryEditorInfo)editorInfo).DecimalPlaces);

			var expectedHint = @"This registry determines the minimum threshold for rows in a certain period/company that will be compacted.

E.g. If we set a value of 0.5, the service task will only compact period/companies where 50% or more rows will be eliminated from compaction.

A value of 0 will always compact even if there is no benefit.
A value of 1.0 will never compact.";
			AssertEquals("Hint", expectedHint, ItemSet.AutoCompactAccGLAggregateThreshold.Hint);
		}

		public void TestEnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions()
		{
			var expectedHint = @"This registry affects the posting behaviors of Receivables INV, CRD, ADJ and Cash Book DRC transactions only.
By default this registry is set to No and charge lines with any mix of Fixed Place of Supply are permitted within each transaction.
You should set this registry to NO when tax rules do not require you to post separate invoices for a mix of places of supply within the invoice.
You should set this registry to YES, when you are required to post separate invoices for a mix of places of supply in the invoice.
When set to YES:
- Posting will not allow mixing different places of supply within a single transaction.
- Transaction lines for separate places of supply will post in separate transactions.
- You will not be able to select a separate Place of supply for a Transaction Line for a Non Job Related invoice.";

			TestGenericRegistryItem(ItemSet.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions,
				"EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions",
				"Accounting/Fixed Place of Supply Configuration",
				"Enforce Posting at Fixed Place of Supply level (Receivable)",
				expectedHint,
				RegistryStorageFlags.Company,
				false);

			var enabledCountryList = new string[] { Core.Constants.CountryCodes.India };
			foreach (var countryCode in enabledCountryList)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					ResetRegistryCache();
					AssertEquals("Default should be TRUE for Country: " + countryCode, true, ItemSet.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.Value);
				}
			}
		}

		public void TestEnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions()
		{
			var expectedHint = @"This registry affects the posting behaviors of Payables INV, CRD, ADJ and Cash Book DPY transactions only.
By default this registry is set to No and charge lines with any mix of Fixed Place of Supply are permitted within each transaction.
You should set this registry to NO when tax rules do not require you to post separate invoices for a mix of places of supply within the invoice.
You should set this registry to YES, when you are required to post separate invoices for a mix of places of supply in the invoice.
When set to YES:
- Posting will not allow mixing different places of supply within a single transaction.
- Transaction lines for separate places of supply will post in separate transactions.
- You will not be able to select a separate Place of supply for a Transaction Line on a Non Job Related invoice.";

			TestGenericRegistryItem(ItemSet.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions,
				"EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions",
				"Accounting/Fixed Place of Supply Configuration",
				"Enforce Posting at Fixed Place of Supply level (Payable)",
				expectedHint,
				RegistryStorageFlags.Company,
				false);

			var enabledCountryList = new string[] { Core.Constants.CountryCodes.India };
			foreach (var countryCode in enabledCountryList)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					ResetRegistryCache();
					AssertEquals("Default should be TRUE for Country: " + countryCode, true, ItemSet.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.Value);
				}
			}
		}

		public void TestSplitIntercompanyInvoiceTaxAmountIntoSeparateLine()
		{
			AssertEquals("SplitIntercompanyInvoiceTaxAmountIntoSeparateLine", ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.Name);
			AssertEquals(Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.Category);
			AssertEquals("Split Intercompany Invoice Tax Amount Into Separate Line", ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.Caption);
			AssertEquals(@"This feature is useful if you are want to reconcile the AR Invoice Ex Tax Amount (in the Issuing Company) to the AP Invoice Ex Tax Amount (in the Receiving Company).

When a charge code is specified, the Invoice's Tax Amount will be posted on a separate transaction line with this charge code when intercompany invoices received from a sister company located in different country/region are imported.

Note: This charge code must be 'NON' or 'OVR' charge type.", ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.Storage);
			AssertEquals(Guid.Empty, ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.DefaultValue);

			var chargeCodePK = Guid.NewGuid();
			ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.SetValue(CompanyPK, Guid.Empty, Guid.Empty, chargeCodePK);
			AssertEquals(chargeCodePK, ItemSet.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));

			var testCreator = new TestObjectCreator(Factory);
			var oldValue = testCreator.CC1.PK.ToGuid();
			var newValue = testCreator.CC2.PK.ToGuid();
			var regItem = AccountingConfigurationRegistry.Instance.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.Inner;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.OnBuildLogReference(args);
			AssertEquals($"Registry value changed from [ZZCC1] to [ZZCC2].", logReference);
		}

		public void TestEnableMatchingValidationAll()
		{
			var registryItem = ItemSet.EnablePaymentApprovalFullMatchingValidation;
			AssertEquals("EnablePaymentApprovalFullMatchingValidation", registryItem.Name);
			AssertEquals("Accounting/Matching", registryItem.Category);
			AssertEquals("Enable Payment Approval Full Matching Validation (CargoWiseOne Support Only)", registryItem.Caption);
			AssertEquals("Enables full matching session validation before Payment Approval posting.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registryItem.Options);
			AssertEquals(true, registryItem.DefaultValue);
		}

		public void TestCreditLimitCheckOverdueInvoicesStatusCheck()
		{
			var registryItem = ItemSet.CreditLimitCheckOverdueInvoicesStatusCheck;
			AssertEquals("CreditLimitCheckOverdueInvoicesStatusCheck", registryItem.Name);
			AssertEquals(Categories.Accounting_CreditLimitCheck_Local, registryItem.Category);
			AssertEquals("Overdue Invoices Status Check", registryItem.Caption);
			AssertEquals(@"Use this registry to restrict the delivery of credit controlled documents based on the overdue statuses of Standard and Disbursement Transactions.
Users will not be allowed to generate any credit controlled documents (unless overridden by authorized credit controllers) if the total outstanding amount of the AR Transactions is above this registry setting, even if the AR Debtor had not exceeded credit limit, or put on credit hold.",
registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, registryItem.Storage);

			AssertEquals(0, registryItem.Value.Cast<CreditControlledDocumentsCheckConfiguration>().ToArray().Length);
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();
			CreditControlledDocumentsCheckConfiguration upToConfiguration = collection.AddNew();
			upToConfiguration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			upToConfiguration.NumberOfDaysOverdue = 10;
			upToConfiguration.Amount = 1000;
			upToConfiguration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToConfiguration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			CreditControlledDocumentsCheckConfiguration configuration = collection.AddNew();
			configuration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			configuration.NumberOfDaysOverdue = 10;
			configuration.Amount = 1000;
			configuration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			configuration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var creditControlledDocumentsCheckConfiguration = registryItem.Value.Cast<CreditControlledDocumentsCheckConfiguration>().ToArray();
				AssertEquals(2, creditControlledDocumentsCheckConfiguration.Length);
				AssertEquals(OrgARTermsLookups.InvoiceTypes.All.Code, creditControlledDocumentsCheckConfiguration[0].InvoiceType);
				AssertEquals(10, creditControlledDocumentsCheckConfiguration[0].NumberOfDaysOverdue);
				AssertEquals(1000m, creditControlledDocumentsCheckConfiguration[0].Amount);
			}
		}

		public void TestGlobalCreditLimitCheckOverdueInvoicesStatusCheck()
		{
			var registryItem = ItemSet.GlobalCreditLimitCheckOverdueInvoicesStatusCheck;
			AssertEquals("GlobalCreditLimitCheckOverdueInvoicesStatusCheck", registryItem.Name);
			AssertEquals(Categories.Accounting_CreditLimitCheck_Global, registryItem.Category);
			AssertEquals("Global Overdue Invoices Status Check", registryItem.Caption);
			AssertEquals(@"Use this registry to restrict the delivery of credit controlled documents based on the overdue statuses of Standard and Disbursement Invoices across all systems companies.
Users will not be allowed to generate any credit controlled documents (unless overridden by authorized credit controllers) if the total outstanding amount of the AR Invoices is above this registry setting, even if the Global Credit Control Group had not exceeded global credit limit, or put on credit hold.",
registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);

			AssertEquals(0, registryItem.Value.Cast<CreditControlledDocumentsCheckConfiguration>().ToArray().Length);
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();
			CreditControlledDocumentsCheckConfiguration upToConfiguration = collection.AddNew();
			upToConfiguration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			upToConfiguration.NumberOfDaysOverdue = 10;
			upToConfiguration.Amount = 1000;
			upToConfiguration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToConfiguration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			CreditControlledDocumentsCheckConfiguration configuration = collection.AddNew();
			configuration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			configuration.NumberOfDaysOverdue = 10;
			configuration.Amount = 1000;
			configuration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			configuration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var globalcreditControlledDocumentsCheckConfiguration = registryItem.Value.Cast<CreditControlledDocumentsCheckConfiguration>().ToArray();
				AssertEquals(2, globalcreditControlledDocumentsCheckConfiguration.Length);
				AssertEquals(OrgARTermsLookups.InvoiceTypes.All.Code, globalcreditControlledDocumentsCheckConfiguration[0].InvoiceType);
				AssertEquals(10, globalcreditControlledDocumentsCheckConfiguration[0].NumberOfDaysOverdue);
				AssertEquals(1000m, globalcreditControlledDocumentsCheckConfiguration[0].Amount);
			}
		}

		public void TestARControlAccountAdjustment()
		{
			var registerItem = ItemSet.ARControlAccountAdjustment;

			TestRegistryItem(registerItem,
				"ARControlAccountAdjustment",
				Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
				"AR Control Account Adjustment",
				string.Format("When the registry 'Auto Create A/R and A/P Outstanding Balances Currency Adjustments' is set to 'Yes', {0} will use this general ledger account when creating A/R Outstanding Balance Currency Adjustment GL Journal.", BrandingFactory.Instance.ProductName),
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory | RegistryOptions.Default,
				RegistryFindBoxCollection.AccGLHeader,
				Guid.Empty);
		}

		public void TestAPControlAccountAdjustment()
		{
			var registerItem = ItemSet.APControlAccountAdjustment;

			TestRegistryItem(registerItem,
				"APControlAccountAdjustment",
				Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
				"AP Control Account Adjustment",
				string.Format("When the registry 'Auto Create A/R and A/P Outstanding Balances Currency Adjustments' is set to 'Yes', {0} will use this general ledger account when creating A/P Outstanding Balance Currency Adjustment GL Journal.", BrandingFactory.Instance.ProductName),
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory | RegistryOptions.Default,
				RegistryFindBoxCollection.AccGLHeader,
				Guid.Empty);
		}

		public void TestAutoCreateARAndAPOutstandingBalancesCurrencyAdjustments()
		{
			var hint = @"By default this registry is set to 'No'.
When this registry is set to 'Yes' and a sub ledger is closed, the system will queue the accounting period for A/R and A/P ledger outstanding balances currency adjustment according to exchange rate type specified in the 'AR/AP Outstanding Balances Currency Adjustment Exchange Rate Type' registry.

The system will auto-calculate the currency adjustment for outstanding balance in each transacted foreign currency as at the end of the accounting period.
At the end of the process, the system will create a 'RJL' general ledger journal. (Note: The general ledger must not be closed else the automation process will fail.)

The value of currency adjustment will be determined based on the difference between: 
1. Outstanding Balance in Invoiced Currency converted to Local Currency Equivalent using the Transaction's Historical Exchange Rate. 
2. Outstanding Balance in Invoiced Currency converted to Local Currency Equivalent using the AR/AP Outstanding Balances Currency Adjustment Exchange Rate.

The 'RJL' general ledger journal will be created as follows:
1. The header description will be set with reference to the 'Accounting > Transaction Description Defaults > A/R and A/P Outstanding Balance Currency Adjustment Journal'.
2. The 'Post In' period will be set with reference to the period of which sub ledger has been closed.
3. The 'Reverse Period' will be set to 'Post In' period + 1 (Note: If the Post In period is the last period of the accounting year, it will be set to the first period of the subsequent accounting year.)
4. The general ledger journal will be automatically approved and posted as this is system generated. 
5. The accounting entries will be posted as follows:

   For A/R Outstanding Balances Currency Adjustment
   Debit/Credit  
   Credit/Debit

   For A/P Outstanding Balances Currency Adjustment
   Debit/Credit  
   Credit/Debit

A pair of journal lines will be added for each currency + ledger + unrealized exchange gain.
A pair of journal lines will be added for each currency + ledger + unrealized exchange loss.

The line description will contain information on the foreign currency balance, original local and adjusted local. 
E.g. unrealized Gain based on USD 57,550.00, Original Local AUD 70,953.03 (Ex.Rate 0.8111), Adjusted Local AUD 70,544.25 (Ex.Rate 0.8158)

Note: 
On successful creation, you will be able to view the GL Journal in Manage > General Ledger > Journals module. 
A flag will also be updated in Manage > General Ledger > Period Management module to indicate if the A/R and A/P Outstanding Balance Adjustment Journal has been created.
In the event where there is an error, the general ledger journal will not be posted with the error details provided in the service task's log file. 
You will need to correct the error and the service task will auto create the journal once all errors are cleared.";
			var registerItem = ItemSet.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments;

			TestRegistryItem(registerItem,
				"AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments",
				Categories.Accounting,
				"Auto Create A/R and A/P Outstanding Balances Currency Adjustments",
				hint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);

			var expectedLogMessage = "Registry value changed from [False] to [True]. ";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registerItem, false, true);
			var logReference = registerItem.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestDeleteQueueRecordsWhenDisabledAutoCreateARAndAPOutstandingBalancesCurrencyAdjustments()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			var testManager = new PeriodManager(Factory);
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();

			var registry = Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments;
			registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var toCloseSubLedgerPeriod = testManager.NextUnClosedSubLedgerPeriod;
			testManager.CloseSubLedgerPeriod();

			var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
			var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			Assert(registry.Value);
			AssertEquals("Should be 200702", 200702, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
			AssertEquals("Count should be 1", 1, results.Rows.Count);
			AssertEquals("ACA_ParentID", toCloseSubLedgerPeriod.PK, results.Rows[0]["ACA_ParentID"]);

			registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			registry.OnUpdateAction(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Count should be 0, because all records will be deleted", 0, results.Rows.Count);

			registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			testManager.CloseSubLedgerPeriod();
			testManager.CloseSubLedgerPeriod();

			results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Count should be 2", 2, results.Rows.Count);

			var expectedLogMessage = "Registry value changed from [True] to [False]. All queue records deleted. Period:200702, 200703";
			registry.OnUpdateAction(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, true, false);
			var logReference = registry.OnBuildLogReference(args);
			results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Count should be 0", 0, results.Rows.Count);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestNoExceptionWhenDeleteQueueRecordsWithNoneRelatedPeriod()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			var testManager = new PeriodManager(Factory);
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();

			var registry = Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments;
			registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var toCloseSubLedgerPeriod = testManager.NextUnClosedSubLedgerPeriod;
			testManager.CloseSubLedgerPeriod();
			testManager.CloseSubLedgerPeriod();

			var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
			var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			Assert(registry.Value);
			AssertEquals("Should be 200703", 200703, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
			AssertEquals("Count should be 2", 2, results.Rows.Count);

			toCloseSubLedgerPeriod.Delete();
			Factory.Save();

			registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			registry.OnUpdateAction(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, true, false);
			var logReference = registry.OnBuildLogReference(args);
			var expectedLogMessage = "Registry value changed from [True] to [False]. All queue records deleted. Period:200702";
			results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Count should be 0", 0, results.Rows.Count);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType()
		{
			var hint = @"This registry defines the exchange rate type that will be used when performing Receivables/Payables Outstanding Balances Currency Adjustments.
By default, this registry will be set to ‘PER’ exchange rate type. 

Note: 
For 'PER' exchange rate type, there will only be one exchange rate recorded for each accounting period and this will be used for the currency adjustment calculation.
For other exchange rate types (non-PER), the currency adjustment calculation will use the exchange rate valid on the End Date of the accounting period.";

			var registerItem = ItemSet.ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType;

			TestRegistryItem(registerItem,
				"ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType",
				Categories.Accounting_GeneralLedgerDefaults,
				"AR/AP Outstanding Balances Currency Adjustment Exchange Rate Type",
				hint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value.GetCodeDescriptionPairList(),
				Constants.ExchangeRateTypes.Code.PeriodEndRate);
		}

		public void TestEnablePostDateGLJournal()
		{
			TestRegistryItem(ItemSet.EnablePostDateGLJournal,
				"EnablePostDateGLJournal",
				Categories.Accounting_GeneralLedgerDefaults,
				"Enable Post Date in GL Journal Entry",
				@"This feature allows you to specify the Post Date in General Ledger Journals.
This is allowed in the journal types 'GJL - General Journal', 'NJL - Note Journal' and 'RJL - Reversing Journals'. This functionality is not available in the journal type 'AJL - Automatic Journal'.

When this registry is set to 'No', the Post Date is not visible and users can only set the Post Period.
The standard behavior in CargoWise is for the 'Post Date' to be set to the end of the selected period. Your periods are defined in Manage > General Ledger > Period Management.
The Period Management function called 'Edit Period End Date' automatically updates the Post Date of relevant Journals whenever the Period End Date is modified. This keeps the Journal Post Date within the intended period range.

When this registry is set to 'Yes', the Post Date can be explicitly set by the user (as can Reverse Date for RJL Journals).
The default values for Post Dates are retained when the user sets the Period value (i.e. End Date of that period), but the user can explicitly override the default value to a specific date within the month.
A key difference here is that CargoWise will prevent users from editing the Period End Date if a Journal's (GJL/RJL/NJL only) Post Date overlaps with the old and new dates.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);

			ItemSet.EnablePostDateGLJournal.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert(ItemSet.EnablePostDateGLJournal.Value);
			ItemSet.EnablePostDateGLJournal.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Assert(!ItemSet.EnablePostDateGLJournal.Value);
		}

		public void TestNoteGLAccountsStatisticalUnitsofMeasurement()
		{
			CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
			lookUpList.AddPair("KWH", "Kilowatt Hours");
			lookUpList.AddPair("KG", "Kilograms");
			lookUpList.AddPair("TON", "Ton");
			lookUpList.AddPair("TEU", "Twenty-foot Equivalent Unit");
			lookUpList.AddPair("HCT", "Headcount");

			TestRegistryItem(ItemSet.NoteGLAccountsStatisticalUnitsofMeasurement,
				"NoteGLAccountsStatisticalUnitsofMeasurement",
				Categories.Accounting_GeneralLedgerDefaults,
				"Note GL Account's Statistical Units of Measurement",
				@"CargoWiseOne supports the recording of non-financial 'NTE - Note' general ledger journals (i.e. statistical purpose).

This journal type enables you to record Total Carbon Emissions, Total TEU, Total M3, Total Tonnage, etc. and present them in the general ledger reports, using 'NTE - Note' GL Accounts.
Each 'NTE - Note' GL Account will require you to specify the statistical unit of measurement.

This registry enables you to define the list of statistical units that will be available for selection when creating 'NTE - Note' GL Account in Manage > Account > GL Accounts.
Below are couple of statistical units that have been defaulted to give you an idea of the statistical units that you can create.
You can re-configure the list below according to your needs.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				3,
				5,
				lookUpList.ToArray());

			lookUpList.AddPair("XXX", "XXXX");
			ItemSet.NoteGLAccountsStatisticalUnitsofMeasurement.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lookUpList);
			AssertContainsExactElementsInAnyOrder("Default value", lookUpList.Cast<ICodeDescription>().Select(x => x.Code), ItemSet.NoteGLAccountsStatisticalUnitsofMeasurement.Value.Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation()
		{
			var registryItem = ItemSet.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation;
			AssertEquals("ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation", registryItem.Name);
			AssertEquals(Categories.Accounting_CreditLimitCheck_Local, registryItem.Category);
			AssertEquals("Exclude Open Claims Amounts From Overdue Credit Checking Calculation", registryItem.Caption);
			AssertEquals(@"By default, all AR Outstanding Invoices will be taken into consideration during the local credit limit check calculation.
When this registry is set to 'Yes', claim amounts relating to AR Outstanding Invoices attach to Open Claims will be excluded from the local credit limit check calculation.

Open Claims refer to claims with 'OPN', 'WRK', 'REJ', 'CCR' and 'CCA' statuses located under Manage > Receivables > Claims and Queries.
AR Invoices refer to invoices attach the these claims.",
registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals(false, registryItem.DefaultValue);

			AssertEquals(false, registryItem.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			using (registryItem.SetTemporaryValue(CompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, registryItem.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			}
		}

		public void TestGlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation()
		{
			var registryItem = ItemSet.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation;
			AssertEquals("GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation", registryItem.Name);
			AssertEquals(Categories.Accounting_CreditLimitCheck_Global, registryItem.Category);
			AssertEquals("Global Exclude Open Claims Amounts From Overdue Credit Checking Calculation", registryItem.Caption);
			AssertEquals(@"By default, all AR Outstanding Invoices will be taken into consideration during the global credit limit check calculation.
When this registry is set to 'Yes', claim amounts relating to AR Outstanding Invoices attach to Open Claims will be excluded from the global credit limit check calculation.

Open Claims refer to claims with 'OPN', 'WRK', 'REJ', 'CCR' and 'CCA' statuses located under Manage > Receivables > Claims and Queries.
AR Invoices refer to invoices attach the these claims.",
registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(false, registryItem.DefaultValue);

			AssertEquals(false, registryItem.Value);
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, registryItem.Value);
			}
		}

		public void TestSetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused()
		{
			var registerItem = ItemSet.SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused;

			TestRegistryItem(registerItem,
				"SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused",
				Categories.Accounting_JobInvoicing,
				"Set Job Revenue Journal's Line Type based on Cost/Revenue GL Account used",
				@"By default, this registry will be set to 'No'

When this registry is set to 'Yes', the system will set the Job Revenue Journal's line type according to the GL Account used.
For instance, if the Cost GL Account is used, then the line type will be set to 'CST'. If the Revenue GL Account is used, then the line type will be set to 'REV'.

Note: You can specify the defaulting rule via the 'Job Revenue Journal GL Account Defaulting Rules' registry. You can override this default during the manual creation of Job Revenue Journal.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registerItem, oldValue, newValue);
			var logReference = AccountingConfigurationRegistry.Instance.SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEnableBulkDisbursementJobsClosure()
		{
			TestRegistryItem(ItemSet.EnableBulkDisbursementJobsClosure,
				"EnableBulkDisbursementJobsClosure",
				Categories.Accounting_JobInvoicing_DisbursementChargeManagement,
				"Enable Bulk Disbursement Jobs Closure (CargoWiseOne Support Only)",
				@"This is a temporary registry to hide the progressive feature change for bulk disbursement jobs closure feature.
This temporary registry will be removed on completion of the feature changes.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestDisbursementJobsClosureConfiguration()
		{
			var registryItem = ItemSet.DisbursementJobsClosureConfiguration;
			AssertEquals("DisbursementJobsClosureConfiguration", registryItem.Name);
			AssertEquals(Categories.Accounting_JobInvoicing_DisbursementChargeManagement, registryItem.Category);
			AssertEquals("Disbursement Jobs Closure Configuration", registryItem.Caption);
			AssertEquals(@"This registry configuration affects the closure of jobs with a disbursement surplus/shortfall balance.

You can specify a job level and an aggregated level threshold.
The Job level threshold, when specified, will be used to evaluate if disbursement jobs should be included in Disbursement Job Close Batch. Jobs that do not meet the threshold will not be batched.
The Aggregated threshold, when specified, will be used to determine if the Disbursement Job Close Batch is subjected to approval or can be automatically close.

Note:
1. Disbursement surplus/shortfall is calculated based on the values of REV and CST line types.
2. If all values are zero, then disbursement clearing balance will not be taken into consideration during job closure.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, registryItem.Storage);
			Instance.EnableBulkDisbursementJobsClosure.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, registryItem.Options);
		}

		public void TestDisbursementJobsClosureConfigurationReferenceLog()
		{
			var registryItem = ItemSet.DisbursementJobsClosureConfiguration;
			var testObjectCreator = new TestObjectCreator(Factory);
			var configOld = new DisbursementJobsClosureConfiguration() { AggregatedLevelOfSurplusUpTo = 10, AggregatedLevelOfShortfallUpTo = 20, JobLevelOfSurplusUpTo = 30, JobLevelOfShortfallUpTo = 40 };
			var configAgg = new DisbursementJobsClosureConfiguration() { AggregatedLevelOfSurplusUpTo = 110, AggregatedLevelOfShortfallUpTo = 220, JobLevelOfSurplusUpTo = 30, JobLevelOfShortfallUpTo = 40 };
			var configJob = new DisbursementJobsClosureConfiguration() { AggregatedLevelOfSurplusUpTo = 10, AggregatedLevelOfShortfallUpTo = 20, JobLevelOfSurplusUpTo = 130, JobLevelOfShortfallUpTo = 240 };
			var configAll = new DisbursementJobsClosureConfiguration() { AggregatedLevelOfSurplusUpTo = 510, AggregatedLevelOfShortfallUpTo = 620, JobLevelOfSurplusUpTo = 530, JobLevelOfShortfallUpTo = 640 };

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, configOld, configJob);
			var logReference = registryItem.OnBuildLogReference(args);
			AssertEquals($"Job Level: Shortfall 240 Surplus 130", logReference);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, configOld, configAgg);
			logReference = registryItem.OnBuildLogReference(args);
			AssertEquals($"Aggregate Level: Shortfall 220 Surplus 110", logReference);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, configOld, configAll);
			logReference = registryItem.OnBuildLogReference(args);
			AssertEquals($"Job Level: Shortfall 640 Surplus 530, Aggregate Level: Shortfall 620 Surplus 510", logReference);
		}

		public void TestAllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC()
		{
			const string expectedName = "AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC";
			const string expectedCaption = "Allow Posting of Payables Invoices Related to Existing Disbursement Charge against Jobs Ready for Financial Closure";
			const string expectedHint = @"By default, this registry will be set to 'No' in which case users who do not have the ‘Allow Posting Charges of Ready for Financial Closure Job’ security right will not be able to post costs against jobs with Ready for Financial Closure (“JFC”) status.

When this registry is set to 'Yes', all users will be able to post Payables Invoices/Credit Notes against JFC jobs when the following conditions are fulfilled:
1. The charge type is Disbursement (“DSB”). 
2. The charge is imported.";
			TestRegistryItem(ItemSet.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC,
				expectedName,
				Categories.Accounting_JobInvoicing,
				expectedCaption,
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false
			);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry has been set to [{newValue}] from [{oldValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(
				ItemSet.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC,
				oldValue,
				newValue);
			AssertEquals(expectedLogMessage, ItemSet
				.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC
				.OnBuildLogReference(args));
		}

		public void TestTaxDateDefaultingOption()
		{
			AssertEquals("Name", "TaxDateDefaultingOption", ItemSet.TaxDateDefaultingOption.Name);
			AssertEquals("Category", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ItemSet.TaxDateDefaultingOption.Category);
			AssertEquals("Caption", "Tax Date Defaulting Option", ItemSet.TaxDateDefaultingOption.Caption);
			AssertEquals("Hint", @"This registry allows you to configure the preferences for setting a Tax Date on Invoice Charges.

As you prepare charges in job billing, tax date defaults empty. If you enter a specific Tax Date on a charge, then this tax date is retained and is used to determine the applicable tax rate when posting the invoice. However, if you leave Tax Date as empty, then when posting the transaction, Tax Date is set to the preferred option as per the settings defined in this registry.

By default, Tax Date is set to Today's Date on all AR and AP Invoices. Override this registry to set the tax date to Invoice Date, Today's Date or various operational dates including Arrival Date and Departure Date.", ItemSet.TaxDateDefaultingOption.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TaxDateDefaultingOption.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.TaxDateDefaultingOption.Options);
			var collection = ItemSet.TaxDateDefaultingOption.Value;
			AssertEquals("Default entry", 1, collection.Count);
			AssertEquals("ALL", collection[0].JobType);
			AssertEquals("ALL", collection[0].Ledger);
			AssertEquals("", collection[0].DirectionCode);
			AssertEquals("", collection[0].Mode);
			AssertEquals("TDY", collection[0].TaxDateOption);
		}

		public void TestAutoPeriodClosureNotifyGroup()
		{
			AssertEquals("Name", "AutoPeriodClosureNotifyGroup", ItemSet.AutoPeriodClosureNotifyGroup.Name);
			AssertEquals("Category", Categories.Accounting_EmailNotification, ItemSet.AutoPeriodClosureNotifyGroup.Category);
			AssertEquals("Caption", "Auto Period Closure Notify Group", ItemSet.AutoPeriodClosureNotifyGroup.Caption);
			AssertEquals("Hint", @"Notify Party when a validation error prevent a Period Ledger Type from being closed.", ItemSet.AutoPeriodClosureNotifyGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AutoPeriodClosureNotifyGroup.Storage);
			AssertEquals("Default Value", Guid.Empty, ItemSet.AutoPeriodClosureNotifyGroup.Value);

			var registry = ItemSet.AutoPeriodClosureNotifyGroup;
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "TES";
			Factory.Save();
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, group1.PK, group2.PK);
			var logReference = ItemSet.AutoPeriodClosureNotifyGroup.OnBuildLogReference(args);
			AssertEquals("Notify Group set to 'TES'", logReference);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, group1.PK, Guid.NewGuid());
			logReference = ItemSet.AutoPeriodClosureNotifyGroup.OnBuildLogReference(args);
			AssertEquals("Notify Group set to ''", logReference);
		}

		public void TestAutoPeriodClosureConfiguration()
		{
			AssertEquals("Name", "AutoPeriodClosureConfiguration", ItemSet.AutoPeriodClosureConfiguration.Name);
			AssertEquals("Category", Categories.Accounting, ItemSet.AutoPeriodClosureConfiguration.Category);
			AssertEquals("Caption", "Auto Period Closure Configuration", ItemSet.AutoPeriodClosureConfiguration.Caption);
			AssertEquals("Hint", @"This registry enables you to configure the auto closure of period by ledger types after a specified time interval from the relevant period's end date. 
You can configure the system to auto close: 
· All ledger types.
· Sub ledger and general ledger only.
· Sub ledger only.
You can specify the interval in minutes, hours, or days from the relevant period's end date. 

Example:
Assume the relevant period's end date is 31-Mar-21 and the interval has been specified as follows:
· Sub ledger = 3 days.
· General ledger = 5 days.
· Adjustment ledger = 0 day.
The sub ledger will be auto closed on 3-Apr-21 and the general ledger will be auto closed on 5-Apr-21. The adjustment ledger will remain open. 

Note: 
1. The above configuration will be processed by the 'PCS - Period Closure Service Task'.
2. Period for general ledger type can only be closed after sub ledger type. Likewise, the adjustment ledger type can only be closed after the general ledger type has been closed. Thus, the interval setting for general ledger type must be the same or greater than the sub ledger, and the interval setting of the adjustment ledger type  must be the same or greater than the general ledger. An interval setting of 0 means that period will not be auto closed.
3. In the event a ledger type cannot be auto closed due to validation error, an email notification will be sent to the notify party specified in the Accounting > Email Notification > Auto Period Closure Notify Group system registry. ", ItemSet.AutoPeriodClosureConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AutoPeriodClosureConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AutoPeriodClosureConfiguration.Options);

			AssertEquals(AccountingUtils.PeriodClosureConfigurationIntervalType.Minutes, ItemSet.AutoPeriodClosureConfiguration.Value.IntervalType);
			AssertEquals(0, ItemSet.AutoPeriodClosureConfiguration.Value.SubLedgerInterval);
			AssertEquals(0, ItemSet.AutoPeriodClosureConfiguration.Value.GeneralLedgerInterval);
			AssertEquals(0, ItemSet.AutoPeriodClosureConfiguration.Value.AdjustmentLedgerInterval);

			var registry = ItemSet.AutoPeriodClosureConfiguration;
			var value1 = new PeriodClosureConfiguration(AccountingUtils.PeriodClosureConfigurationIntervalType.Minutes);
			var value2 = new PeriodClosureConfiguration(AccountingUtils.PeriodClosureConfigurationIntervalType.Minutes, 1, 2, 3);
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, value1, value2);
			var logReference = ItemSet.AutoPeriodClosureConfiguration.OnBuildLogReference(args);
			AssertEquals("Interval Type: MINS, Sub Ledger: 1, General Ledger: 2, Adjustment Ledger: 3", logReference);
		}

		public void TestSizeOfRequestedAPTransactionList_WhenTurkeyAPComplianceFeaturesEnabled() =>
			AssertSizeOfRequestedAPTransactionListRegistryItemFeatures(RegistryOptions.Default, true);

		public void TestSizeOfRequestedAPTransactionList_WhenTurkeyAPComplianceFeaturesNotEnabled() =>
			AssertSizeOfRequestedAPTransactionListRegistryItemFeatures(RegistryOptions.IsHidden, false);

		void AssertSizeOfRequestedAPTransactionListRegistryItemFeatures(RegistryOptions visibilityOption, bool isFunctionalityEnabled)
		{
			var registryDate = isFunctionalityEnabled ? ZDateTime.Now.AddDays(-2) : ZDateTime.Now.AddDays(2);

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDate.ToDateTime()))
			{
				AssertEquals("Turkey AP Functionality status", isFunctionalityEnabled, AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeatures);
				AssertEquals("SizeOfRequestedAPTransactionList", ItemSet.SizeOfRequestedAPTransactionList.Name);
				AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey, ItemSet.SizeOfRequestedAPTransactionList.Category);
				AssertEquals("Size Of Requested AP Transaction List", ItemSet.SizeOfRequestedAPTransactionList.Caption);
				AssertEquals(@"This registry is currently only referenced by Turkey login companies.
Use this registry to adjust the count of invoices included per AP list response. It can be more efficient to nominate a smaller invoice count per list request and request more regularly as opposed to very large invoice list requests.
This registry item should be set with consideration of the setting of registry item 'AP List Automated Request Schedule'.", ItemSet.SizeOfRequestedAPTransactionList.Hint);
				AssertEquals(RegistryStorageFlags.Company, ItemSet.SizeOfRequestedAPTransactionList.Storage);
				AssertEquals(visibilityOption, ItemSet.SizeOfRequestedAPTransactionList.Options);
				AssertEquals(20, ItemSet.SizeOfRequestedAPTransactionList.DefaultValue);

				ItemSet.SizeOfRequestedAPTransactionList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.SizeOfRequestedAPTransactionList.DefaultValue);
				AssertEquals("Default value, not overridden", 20, ItemSet.SizeOfRequestedAPTransactionList.Value);

				ItemSet.SizeOfRequestedAPTransactionList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
				AssertEquals("Overridden with different value", 100, ItemSet.SizeOfRequestedAPTransactionList.Value);

				try
				{
					ItemSet.SizeOfRequestedAPTransactionList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				}
				catch (Exception ex)
				{
					AssertEquals("Value must be greater than or equal to the minimum (1)", ex.Message);
				}

				try
				{
					ItemSet.SizeOfRequestedAPTransactionList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1001);
				}
				catch (Exception ex)
				{
					AssertEquals("Value must be less than or equal to the maximum (1000)", ex.Message);
				}
			}
		}

		public void TestApplyLocalChargeCodeDescriptionDefaultToForeignDebtors()
		{
			AssertEquals("Name", "ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors", ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Name);
			AssertEquals("Category", AccChargeCodeRegistry.Categories.Accounting_JobInvoicing, ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Category);
			AssertEquals("Caption", "Apply Local Charge Code Description Default to Foreign Debtors", ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Caption);
			AssertEquals("Hint", @"This registry can only be enabled when the  ‘Enable Local Charge Code Description Default’ registry is set to ‘Yes’.

By default, this registry is set to 'No'.
When this registry is set to ‘Yes’, local language description setup in Charge Code will be defaulted when job charges are entered against foreign debtors.
Likewise, local language description will be used when printing rating document to foreign clients.", ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Options);
			AssertEquals("EnableLocalChargeCodeDescriptionDefault", false, ItemSet.EnableLocalChargeCodeDescriptionDefault.Value);
			AssertEquals("IsVisible", false, ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			using (ItemSet.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("IsVisible", true, ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}

			var registry = ItemSet.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestSuppressWarningWhenThereIsNoComplianceBookSetups()
		{
			AssertEquals("Name", "SuppressWarningWhenThereIsNoComplianceBookSetups", ItemSet.SuppressWarningWhenThereIsNoComplianceBookSetups.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, ItemSet.SuppressWarningWhenThereIsNoComplianceBookSetups.Category);
			AssertEquals("Caption", "Suppress Warning when there is no Compliance Book Setups", ItemSet.SuppressWarningWhenThereIsNoComplianceBookSetups.Caption);
			AssertEquals("Hint", "By default CargoWise will warn user when they attempt to use Compliance Numbers but have not configured an applicable Compliance Book. You can suppress this warning message by setting this registry value to 'Yes'.", ItemSet.SuppressWarningWhenThereIsNoComplianceBookSetups.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.SuppressWarningWhenThereIsNoComplianceBookSetups.Storage);
			AssertEquals("Default Value", false, ItemSet.SuppressWarningWhenThereIsNoComplianceBookSetups.Value);
		}

		public void TestCustomJobTaxBranchDefaultingRulesEngineConfiguration()
		{
			TestGenericRegistryItem(ItemSet.CustomJobTaxBranchDefaultingRulesEngineConfiguration,
				"CustomJobTaxBranchDefaultingRulesEngineConfiguration",
				"Accounting/Job Invoicing",
				"Custom Job Tax Branch Defaulting Rules Engine Configuration",
				 @"This registry will enable setting up Rules for Job Tax Branch Defaulting in Rules Engine.",
				RegistryStorageFlags.System,
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
				false);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.CustomJobTaxBranchDefaultingRulesEngineConfiguration, oldValue, newValue);
			var logReference = ItemSet.CustomJobTaxBranchDefaultingRulesEngineConfiguration.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestJCDReportDataCollectionStartingPeriod()
		{
			TestGenericRegistryItem(ItemSet.JCDReportDataCollectionStartingPeriod,
				"JCDReportDataCollectionStartingPeriod",
				string.Empty,
				string.Empty,
				String.Empty,
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				0);

			var newValue = 201901;
			ItemSet.JCDReportDataCollectionStartingPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals(201901, ItemSet.JCDReportDataCollectionStartingPeriod.Value);
		}

		#region Supply Type Configuration by Charge Group

		public void TestSupplyTypeConfigurationByChargeGroup()
		{
			TestGenericRegistryItem(ItemSet.SupplyTypeConfigurationByChargeGroup,
				"SupplyTypeConfigurationByChargeGroup",
				"Accounting/Tax Configurations/Supply Type",
				"Supply Type Configuration By Charge Group",
				 @"This registry is used to configure Supply Type Defaulting Behavior by Charge Code Rating Group.",
				RegistryStorageFlags.Company,
				AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);
		}

		public void TestSetSupplyTypeConfigurationByChargeGroup()
		{
			SupplyTypeConfigurationByChargeGroupCollection newValue = new SupplyTypeConfigurationByChargeGroupCollection();
			SupplyTypeConfigurationByChargeGroup copy = newValue.AddNew();
			copy.ChargeGroup = "ITC";
			SupplyTypeConfiguration supplyTypeConf = copy.ChargeGroupSettings.AddNew();
			supplyTypeConf.JobType = "SHP";
			supplyTypeConf.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			supplyTypeConf.Mode = "AIR";
			supplyTypeConf.Incoterm = "ALL";
			supplyTypeConf.LineDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			supplyTypeConf.SupplyType = "LOC";

			ItemSet.SupplyTypeConfigurationByChargeGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			SupplyTypeConfigurationByChargeGroupCollection value = ItemSet.SupplyTypeConfigurationByChargeGroup.Value;
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].JobType", "SHP", value[0].ChargeGroupSettings[0].JobType);
			AssertEquals("Value[0].Direction", Constants.FreightShipmentDirection.Code.Import, value[0].ChargeGroupSettings[0].DirectionCode);
			AssertEquals("Value[0].Mode", "AIR", value[0].ChargeGroupSettings[0].Mode);
			AssertEquals("Value[0].Incoterm", "ALL", value[0].ChargeGroupSettings[0].Incoterm);
			AssertEquals("Value[0].LineDepartmentPK", GlbDepartment.CurrentDepartment.PK, value[0].ChargeGroupSettings[0].LineDepartmentPK);
			AssertEquals("Value[0].SupplyType", "LOC", value[0].ChargeGroupSettings[0].SupplyType);
			AssertEquals("Value[0].ChargeGroup", "ITC", value[0].ChargeGroup);
		}

		public void TestSupplyTypeConfigurationByChargeGroup_Log()
		{
			var oldValue = ItemSet.SupplyTypeConfigurationByChargeGroup.Value;

			var newValue = new SupplyTypeConfigurationByChargeGroupCollection();
			var copy = newValue.AddNew();
			copy.ChargeGroup = "BRK";
			var supplyTypeConf = copy.ChargeGroupSettings.AddNew();
			supplyTypeConf.JobType = "SHP";
			supplyTypeConf.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			supplyTypeConf.Mode = "AIR";
			supplyTypeConf.Incoterm = "ALL";
			supplyTypeConf.LineDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			supplyTypeConf.SupplyType = "LOC";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.SupplyTypeConfigurationByChargeGroup, oldValue, newValue);
			var logReference = ItemSet.SupplyTypeConfigurationByChargeGroup.OnBuildLogReference(args);
			AssertEquals(@"Added: Charge Group: BRK, Job Type:SHP, Direction:IMP, Transport Mode:AIR, Incoterm:ALL, Line Department:BRN, Supply Type: LOC
", logReference);

			ItemSet.SupplyTypeConfigurationByChargeGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			oldValue = ItemSet.SupplyTypeConfigurationByChargeGroup.Value;
			supplyTypeConf.SupplyType = "LOX";
			args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.SupplyTypeConfigurationByChargeGroup, oldValue, newValue);
			logReference = ItemSet.SupplyTypeConfigurationByChargeGroup.OnBuildLogReference(args);
			AssertEquals(@"Deleted: Charge Group: BRK, Job Type:SHP, Direction:IMP, Transport Mode:AIR, Incoterm:ALL, Line Department:BRN, Supply Type: LOC
Added: Charge Group: BRK, Job Type:SHP, Direction:IMP, Transport Mode:AIR, Incoterm:ALL, Line Department:BRN, Supply Type: LOX
", logReference);

			ItemSet.SupplyTypeConfigurationByChargeGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			oldValue = ItemSet.SupplyTypeConfigurationByChargeGroup.Value;
			copy.ChargeGroupSettings.RemoveAndDeleteAll();
			args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.SupplyTypeConfigurationByChargeGroup, oldValue, newValue);
			logReference = ItemSet.SupplyTypeConfigurationByChargeGroup.OnBuildLogReference(args);
			AssertEquals(@"Deleted: Charge Group: BRK, Job Type:SHP, Direction:IMP, Transport Mode:AIR, Incoterm:ALL, Line Department:BRN, Supply Type: LOX
", logReference);
		}

		public void TestAlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB()
		{
			TestGenericRegistryItem(ItemSet.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB,
				"AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB",
				"Accounting/Tax Configurations/Supply Type",
				"Always set Supply Type to 'DSB' if Charge Type is 'DSB'",
				 @"When this registry is set to 'Yes', the system will always set the charge's supply type to 'DSB' where the charge type of job charge is 'DSB'.",
				RegistryStorageFlags.Company,
				AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
				false);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value change from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB, oldValue, newValue);
			var logReference = ItemSet.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		#endregion

		public void TestMexicoEInvoicingReversalStatusCodeRegistry()
		{
			var reversalStatusCodeList = new CodeDescriptionPairList();
			reversalStatusCodeList.AddPair("02", "Transactions issued with unrelated errors");
			reversalStatusCodeList.AddPair("03", "The operation was not carried out");

			TestGenericRegistryItem(ItemSet.MexicoEInvoicingReversalStatusCode,
				"MexicoEInvoicingReversalStatusCode",
				AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations_Mexico,
					"Mexico Cancelations Reversal Status Code (CargoWiseOne Support Only)",
					"This registry is used to configure valid reversal reason codes for Mexico Cancelations within the E-Invoicing module.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				reversalStatusCodeList
			);
		}

		public void TestMexicoNotificationRemainingFolioConfigurationRegistryItem()
		{
			var result = ItemSet.MexicoRemainingFolioNotification;

			TestGenericRegistryItem(
				item: result,
				expectedName: "MexicoRemainingFolioNotification",
				expectedCategory: Categories.Accounting_EReportingAndEInvoicingConfigurations_Mexico,
				expectedCaption: "Mexico Remaining Quantity of 'Timbres/Folios' Notification",
				expectedHint: @"This registry lets you define when you want to receive notification e-mails and reminders to alert you that the number of 'Folios/Timbres' to be used in E-Invoicing transactions is running low for your Mexico Login Company.

An e-mail notification will be sent to the group configured against the 'Accounting -> E-Reporting and E-Invoicing Configurations -> E-Reporting Error Notification Group' registry when the number of available 'Folios/Timbres' reaches the value set in the Quantity of remaining 'Folios/Timbres' field of this registry.

After the first e-mail notification is triggered, reminder e-mails will be sent after a certain amount of timbres has been consumed.
Use the 'Interval' field to set how often you want to receive reminders after the first e-mail notification is triggered.

Example of use of this registry:
'Quantity of remaining Folios/Timbres' value: 100.
'Interval' value: 10.
The first notification email will be sent when 100 'timbres' are left to be used in the Electronic Invoicing process.
After that, a reminder e-mail will be sent each time 10 'timbres' are consumed.
Each reminder will contain the updated number of available 'timbres'.

The e-mail notifications will stop when a new pack of 'timbres' is assigned to the Login Company.

Note: The number of available 'timbres' is shown in the 'E-Reporting Error/ Warning/ Status' column of the Receivables Transactions and Consolidation modules and all operational jobs.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default
				);

			AssertEquals("CountryFilterPKs", CountryFilterPKs.Mexico, result.CountryFilterPKs);
		}

		public void TestMexicoNotificationRemainingFolioConfigurationRegistryItemDefaultValue()
		{
			var result = ItemSet.MexicoRemainingFolioNotification;

			AssertEquals("Default Value Should be 100", 100, result.DefaultValue.FoliosQuantity);
			AssertEquals("Default Value Should be 10", 10, result.DefaultValue.Interval);
		}

		public void TestChinaEInvoicingReceivingFileType()
		{
			var registryItem = ItemSet.ChinaEInvoicingReceivingFileType;
			AssertEquals("Name", "ChinaEInvoicingReceivingFileType", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_China, registryItem.Category);
			AssertEquals("Caption", "E-Invoicing Receiving File Type", registryItem.Caption);
			AssertEquals("Hint", @"This registry is relevant to China Login Company Only.
By default, the system only attaches a copy of the e-invoice in PDF format to the invoice's eDocs tab.
If required, you can specify debtors and/or debtor groups to also download copies of the e-invoice in OFD and/or XML format to the invoice's eDocs tab at the same time.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals("CountryFilterPK", CountryFilterPKs.China, registryItem.CountryFilterPKs);
			AssertEquals("Option", RegistryOptions.Default, registryItem.Options);
		}

		public void TestARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting()
		{
			AssertEquals("Name", "ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting", ItemSet.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Category);
			AssertEquals("Caption", "AR invoice store and use invoice issuer and recepient information during post when printing", ItemSet.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Caption);
			AssertEquals("Hint", "If the registry is set to 'Yes' store issuer and recepient information during invoice post and then use that information when printing the invoice.", ItemSet.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Storage);
			AssertEquals("Option", RegistryOptions.IsOnlyForSupport, ItemSet.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Options);
			AssertEquals("Default Value", false, ItemSet.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Value);
		}

		public void TestARExceedCreditLimitGrantedEmailNotificationNote()
		{
			AssertEquals("Name", "ARExceedCreditLimitGrantedEmailNotificationNote", ItemSet.ARExceedCreditLimitGrantedEmailNotificationNote.Name);
			AssertEquals("Category", Categories.Accounting_EmailNotification, ItemSet.ARExceedCreditLimitGrantedEmailNotificationNote.Category);
			AssertEquals("Caption", "AR Exceed Credit Limit Granted Email Notification Note", ItemSet.ARExceedCreditLimitGrantedEmailNotificationNote.Caption);
			AssertEquals("Hint", "When specified, the note will be included in the Exceeded Credit Limit Granted Notification Email. \r\n\r\nThis note can be used to provide instruction to AR Control Breach Notify Group in relation to exceeding of credit limit granted.", ItemSet.ARExceedCreditLimitGrantedEmailNotificationNote.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch, ItemSet.ARExceedCreditLimitGrantedEmailNotificationNote.Storage);
			AssertEquals("Option", RegistryOptions.Default, ItemSet.ARExceedCreditLimitGrantedEmailNotificationNote.Options);

			object value = ItemSet.ARExceedCreditLimitGrantedEmailNotificationNote.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("ARExceedCreditLimitGrantedEmailNotificationNote", value is MultilingualString);
		}

		public void TestCrossTradeDebtorDefaultingConfiguration()
		{
			AssertEquals("Name", "CrossTradeDebtorDefaultingConfiguration", ItemSet.CrossTradeDebtorDefaultingConfiguration.Name);
			AssertEquals("Category", Categories.Accounting_JobInvoicing, ItemSet.CrossTradeDebtorDefaultingConfiguration.Category);
			AssertEquals("Caption", "Cross Trade Debtor Defaulting Configuration", ItemSet.CrossTradeDebtorDefaultingConfiguration.Caption);
			AssertEquals("Hint", "This registry enables you to configure the charge line debtor to default on a Cross Trade job based on whether the charge is Prepaid or Collect.\r\nThese rules only apply to Cross Trade Forwarding Shipments, Quick Bookings and Bookings with Quotes.\r\nIf Default Debtor is Job's Controlling Customer, and the controlling customer has an IFT Party, then the PIC IFT will default for prepaid charges and DLV IFT will default for collect charges. \r\nIf there is no IFT, the Controlling Customer will default.\r\n\r\n \r\n\r\nNote: When determining the Debtor to default, the Charge Agent Always Charge Code and Charge Local Client Always Charge Code registries will be taken into account and respected above the defaults set in this registry.", ItemSet.CrossTradeDebtorDefaultingConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CrossTradeDebtorDefaultingConfiguration.Storage);
			AssertEquals("Option", RegistryOptions.Default, ItemSet.CrossTradeDebtorDefaultingConfiguration.Options);
			AssertEquals("Default Value", true, ItemSet.CrossTradeDebtorDefaultingConfiguration.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK) is CrossTradeDebtorConfigurationHeader);

			var value = ItemSet.CrossTradeDebtorDefaultingConfiguration.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			AssertEquals("Line Count", 2, value.Configurations.Count);

			var collection = value.Configurations;

			AssertEquals("ALL", collection[0].JobType);
			AssertEquals("ALL", collection[0].Mode);
			AssertEquals("OTH", collection[0].DirectionCode);
			AssertEquals("CCX", collection[0].ChargePaymentType);
			AssertEquals("CBP", collection[0].Debtor);

			AssertEquals("ALL", collection[1].JobType);
			AssertEquals("ALL", collection[1].Mode);
			AssertEquals("OTH", collection[1].DirectionCode);
			AssertEquals("PPD", collection[1].ChargePaymentType);
			AssertEquals("PBP", collection[1].Debtor);
		}

		public void TestCrossTradeDebtorDefaultingConfigurationWhenEnableCrossTradeDebtorDefaultingFunctionalityDisabled()
		{
			AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var jobInvoicingRegistries = AllItems.Where(p => p.Categories.Contains(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing)
			&& p.Name.Equals("CrossTradeDebtorDefaultingConfiguration"));
			foreach (var registry in jobInvoicingRegistries)
			{
				AssertEquals(RegistryOptions.IsHidden, registry.Options);
			}
		}

		public void TestCrossTradeDebtorDefaultingConfigurationWhenEnableCrossTradeDebtorDefaultingFunctionalityEnabled()
		{
			AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var jobInvoicingRegistries = AllItems.Where(p => p.Categories.Contains(AccountingConfigurationRegistry.Categories.Accounting_JobInvoicing)
			&& p.Name.Equals("CrossTradeDebtorDefaultingConfiguration"));
			foreach (var registry in jobInvoicingRegistries)
			{
				AssertEquals(RegistryOptions.Default, registry.Options);
			}
		}

		public void TestReceivableDefaultsAccountGroupDefaultingConfiguration()
		{
			AssertEquals("Name", "ARAccountGroup", ItemSet.ARAccountGroup.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.ARAccountGroup.Category);
			AssertEquals("Caption", "Account Group", ItemSet.ARAccountGroup.Caption);
			AssertEquals("Hint", "Use this registry to define the A/R Account Group defaulted against each new Receivable Organization as it is created. When overridden, the Debtor Group defined against this registry is used to set an organization’s A/R Account Group when creating new Receivable Organizations.", ItemSet.ARAccountGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ARAccountGroup.Storage);
			AssertEquals("Option", RegistryOptions.IsValueMandatory, ItemSet.ARAccountGroup.Options);
			var value = ItemSet.ARAccountGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			AssertEquals("Default Value", Guid.Empty, value);
		}

		public void TestPayableDefaultsAccountGroupDefaultingConfiguration()
		{
			AssertEquals("Name", "APAccountGroup", ItemSet.APAccountGroup.Name);
			AssertEquals("Category", Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.APAccountGroup.Category);
			AssertEquals("Caption", "Account Group", ItemSet.APAccountGroup.Caption);
			AssertEquals("Hint", "Use this registry to define the A/P Account Group defaulted against each new Payable Organization as it is created. When overridden, the Creditor Group defined against this registry is used to set an organization’s A/P Account Group when creating new Payable Organizations.", ItemSet.APAccountGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.APAccountGroup.Storage);
			AssertEquals("Option", RegistryOptions.IsValueMandatory, ItemSet.APAccountGroup.Options);
			var value = ItemSet.APAccountGroup.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			AssertEquals("Default Value", Guid.Empty, value);
		}

		public void TestTransactionNumberOptionForCollectionBatchExportFile()
		{
			AssertEquals(AccountingMasterFilesConstants.TransactionNumberCodes.ComplianceOrInvoiceNr, ItemSet.TransactionNumberOptionForCollectionBatchExportFile.DefaultValue);
			AssertEquals(AccountingConfigurationRegistry.Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders, ItemSet.TransactionNumberOptionForCollectionBatchExportFile.Category);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.TransactionNumberOptionForCollectionBatchExportFile.Storage);
			AssertEquals("Transaction Number Option for Collection Batch Export File", ItemSet.TransactionNumberOptionForCollectionBatchExportFile.Caption);
			AssertEquals("Hint", @"This registry is used to define which Reference Number should be mapped as Invoice Number when RIBA and SEPA files are generated for Collection Order Batches.
When setting the value to ""INV"", the system uses the Transaction Number;
when setting the value to ""CIN"", the system uses the Compliance Number when it is filled, otherwise it uses the Transaction Number.", ItemSet.TransactionNumberOptionForCollectionBatchExportFile.Hint);

			ItemSet.TransactionNumberOptionForCollectionBatchExportFile.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.TransactionNumberCodes.InvoiceNr);
			AssertEquals(AccountingMasterFilesConstants.TransactionNumberCodes.InvoiceNr, ItemSet.TransactionNumberOptionForCollectionBatchExportFile.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestEInvoicingAmendmentCodes()
		{
			var item = ItemSet.EInvoicingAmendmentCodes;
			AssertEquals("EInvoicingAmendmentCodes", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("E-Invoicing Amendment Codes (CargoWise Support Only)", item.Caption);
			AssertEquals("This registry is used to configure the Amendment Codes according to the tax authorities of each country within the E-Invoicing module", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue, item.Options);

			var emptyCodeDescriptionPairList = new ReadOnlyCodeDescriptionPairList();
			AssertEquals(emptyCodeDescriptionPairList, item.Value);
			AssertEquals(emptyCodeDescriptionPairList, item.DefaultValue);

			var mockOne = new CodeDescriptionPairList();
			mockOne.AddPair((NoResString)"INV", (NoResString)"Some temp string");

			var displayValOne = "INV - Some temp string";

			var mockTwo = new CodeDescriptionPairList();
			mockTwo.AddPair((NoResString)"MSC", (NoResString)"Some other temp string");
			mockTwo.AddPair((NoResString)"02", (NoResString)"Some third temp string");

			var displayValTwo = "MSC - Some other temp string; 02 - Some third temp string";

			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(
				item
				, x => x.EInvoicingAmendmentCodes
				, (displayValOne, mockOne)
				, (displayValTwo, mockTwo)
				, "E-Invoicing Amendment Codes (CargoWise Support Only)"
				, startingCallCount: 0);
		}

		public void TestMalaysiaEInvoicingPortalBaseUrl_ProductionSystem() => TestMalaysiaEInvoicingPortalBaseUrl(true);

		public void TestMalaysiaEInvoicingPortalBaseUrl_NonProductionSystem() => TestMalaysiaEInvoicingPortalBaseUrl(false);

		void TestMalaysiaEInvoicingPortalBaseUrl(bool isProductionSystem)
		{
			var itemSet = ItemSet;

			if (isProductionSystem)
			{
				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
				itemSet = GetNewItemSet();
			}

			AssertEquals("Is Production", isProductionSystem, Env.Instance.IsProductionSystem);

			AssertEquals("Name", "MalaysiaEInvoicingPortalBaseUrl", ItemSet.MalaysiaEInvoicingPortalBaseUrl.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia, ItemSet.MalaysiaEInvoicingPortalBaseUrl.Category);
			AssertEquals("Caption", "E-Invoicing Portal Base URL", ItemSet.MalaysiaEInvoicingPortalBaseUrl.Caption);
			AssertEquals("Hint", "This registry defines the e-Invoice portal Base URL used by Malaysia e-Invoicing.", ItemSet.MalaysiaEInvoicingPortalBaseUrl.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.MalaysiaEInvoicingPortalBaseUrl.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.MalaysiaEInvoicingPortalBaseUrl.Options);

			var expectedDefaultValueUrl = isProductionSystem ? "https://myinvois.hasil.gov.my" : "https://preprod.myinvois.hasil.gov.my";
			AssertEquals("DefaultValue", expectedDefaultValueUrl, ItemSet.MalaysiaEInvoicingPortalBaseUrl.DefaultValue);

			var oldValue = "old.url";
			var newValue = "new.url";
			var expectLogMessage = $"The value changed from [{oldValue}] to [{newValue}]";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.MalaysiaEInvoicingPortalBaseUrl, oldValue, newValue);
			var logReference = Instance.MalaysiaEInvoicingPortalBaseUrl.OnBuildLogReference(args);
			AssertEquals(expectLogMessage, logReference);
		}

		#region GenerateJournalEntriesStartDate

		[TestDate(2023, 4, 28, 12, 12, 12)]
		public void TestGenerateJournalEntriesStartDate()
		{
			TestGenericRegistryItem(ItemSet.GenerateJournalEntriesStartDate, "GenerateJournalEntriesStartDate", Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, "Generate Journal Entries - Start Date", @"By default, journal entries are not stored in the database. They are calculated as needed when generating certain types of reports, such as the GL Transactions report.
When a start date is specified, the system will generate journal entries for all accounting transactions posted from that date onwards and store them in the database.

Note:
1. The date specified must be the start date of the first accounting period.
2. This date must not be later than today's date.
3. This date must be earlier or the same as previous date, if already specified.
4. If 'Journal Entries Last Processed Date' registry does not have a value, then the 'Start Date' must fall in the current accounting year.
5. If 'Journal Entries Last Processed Date' registry has a value, then the 'Start Date' must fall in the prior accounting year based on the last processed date.", RegistryStorageFlags.Company, RegistryOptions.IsHidden);

			AssertEquals("Default value is Empty", DateTime.MinValue, ItemSet.GenerateJournalEntriesStartDate.DefaultValue);

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateJournalEntriesStartDate", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.GenerateJournalEntriesStartDate.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateJournalEntriesStartDate", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is RegistryOptions.IsReadOnly | RegistryOptions.IsOnlyForSupport",
					RegistryOptions.IsReadOnly | RegistryOptions.IsOnlyForSupport,
					ItemSet.GenerateJournalEntriesStartDate.Options);
			}

			var oldValue = ItemSet.GenerateJournalEntriesStartDate.DefaultValue;
			var newValue = ZDateTime.Today.ToDateTime();
			var expectedLogMessage = $"Start Date set to '28/04/2023 12:00:00 AM'.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.GenerateJournalEntriesStartDate, oldValue, newValue);
			AssertEquals(expectedLogMessage, ItemSet.GenerateJournalEntriesStartDate.OnBuildLogReference(args));
		}

		[TestDate(2023, 3, 28, 0, 0, 0)]
		public void TestGenerateJournalEntriesStartDateValue()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var today = ZDateTime.Today.ToDateTime();
			var year = today.Year;
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(year - 1, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.SetupSinglePeriod(202105, new DateTime(2021, 2, 1), new DateTime(2021, 5, 31), companyPK);
			periodTestHelper.SetupSinglePeriod(202103, new DateTime(2020, 2, 1), new DateTime(2020, 5, 31), companyPK);

			AssertExceptionThrown<RegistryValidationException>(
				"RegistryValidationException", "Please select a valid date.",
				() => ItemSet.GenerateJournalEntriesStartDate.DataType.Validate(ItemSet.GenerateJournalEntriesStartDate, DateTime.MinValue, companyPK, Guid.Empty, Guid.Empty));

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(companyPK, Guid.Empty, Guid.Empty, DateTime.MinValue);

			AssertNoExceptionThrown(() => ItemSet.GenerateJournalEntriesStartDate.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)));
			AssertEquals("Date is set to today", today, AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty));
		}

		#endregion

		#region GenerateAndStoreJournalEntriesForPostedAccountingTransactions

		public void TestGenerateAndStoreJournalEntriesForPostedAccountingTransactions()
		{
			TestGenericRegistryItem(ItemSet.GenerateAndStoreJournalEntriesForPostedAccountingTransactions, "GenerateAndStoreJournalEntriesForPostedAccountingTransactions", Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, "Generate and Store Journal Entries for Posted Accounting Transactions", @"By default, this feature is disabled. 

When enabled, the system will generate and store the journal entries for all newly posted accounting transactions. Additionally, the journal entries of all previously posted accounting transactions will be generated in batches from the most recent accounting year to the first accounting year. 

You can track the status of the backlog transactions processing via the ""Journal Entries Last Processed Date"" registry.  

Important Note: You cannot disable this registry once it has been enabled.", RegistryStorageFlags.Company, RegistryOptions.IsHidden);

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateAndStoreJournalEntriesForPostedAccountingTransactions", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateAndStoreJournalEntriesForPostedAccountingTransactions", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is MustOverrideDefaultValue",
					RegistryOptions.MustOverrideDefaultValue,
					ItemSet.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Options);
			}
		}

		public void TestGenerateAndStoreJournalEntriesForPostedAccountingTransactions_SetValue()
		{
			var oldValue = ItemSet.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.DefaultValue;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [No] to [Yes].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.GenerateAndStoreJournalEntriesForPostedAccountingTransactions, oldValue, newValue);
			AssertEquals(expectedLogMessage, ItemSet.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.OnBuildLogReference(args));
		}

		#endregion

		#region E-Invoicing Registries (China specific)

		public void TestDoNotQueueInvoicesContainingSpecificChargesForTransmission()
		{
			TestGenericRegistryItem(ItemSet.DoNotQueueInvoicesContainingSpecificChargesForTransmission,
				"DoNotQueueInvoicesContainingSpecificChargesForTransmission",
				AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
					"Do Not Queue Invoices Containing Specific Charges For Transmission",
					@"By default, all eligible receivables invoices will be queued and transmitted to RongJin's Tax Easy portal for fapiao issuance.

With this registry configuration, we will allow clients to optionally choose not to queue and transmit invoices containing specific charges to Tax Easy portal due to confidentiality reason.
Instead, they will issue the relative fapiao directly through the E-Tax China portal and manually update the compliance number and date back into CargoWise.

Note:
1.	Please consult the Accounting Asia Product Team before sharing or enabling this feature.
2.	This registry is configurable at branch level for China Login Companies Only and will only show the Current Login Company's Branches only. Please login to the required China Login Company to adjust the configuration.",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport
			);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.China))
			{
				AssertEquals("IsVisible", true, ItemSet.DoNotQueueInvoicesContainingSpecificChargesForTransmission.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				var creator = new TestObjectCreator(Factory);
				AssertEquals("IsVisible", false, ItemSet.DoNotQueueInvoicesContainingSpecificChargesForTransmission.IsVisible(creator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public void TestDoNotQueueInvoicesContainingSpecificChargesForTransmissionLog()
		{
			var oldCharge = Factory.NewWithValidTestData<AccChargeCode>();
			oldCharge.AC_Code = "NAB";
			var newCharge = Factory.NewWithValidTestData<AccChargeCode>();
			newCharge.AC_Code = "NA2";
			Factory.Save();
			var oldSettings = new GenericChargeConfigurationCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var oldSetting = oldSettings.AddNew();
			oldSetting.ChargePK = oldCharge.PK;

			var newSettings = new GenericChargeConfigurationCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var newSetting = newSettings.AddNew();
			newSetting.ChargePK = newCharge.PK;

			var expectedLogMessage = $"NAB is deleted.NA2 is added.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.DoNotQueueInvoicesContainingSpecificChargesForTransmission, oldSettings, newSettings);
			var logReference = ItemSet.DoNotQueueInvoicesContainingSpecificChargesForTransmission.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestDoNotSendTheseInfomationForFullyDigitalizedEInvoice()
		{
			TestGenericRegistryItem(ItemSet.DoNotSendTheseInfomationForFullyDigitalizedEInvoice,
				"DoNotSendTheseInfomationForFullyDigitalizedEInvoice",
				AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
					"Do Not Send These Information For Fully Digitalized E-Invoice",
					@"This registry enables you to exclude certain optional data from being transmitted in relation to Fully Digitalized Electronic Invoices.

By default, the 'Buyer's Address' will be excluded.
You can adjust the configuration as required.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default
			);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.China))
			{
				AssertEquals("IsVisible", true, ItemSet.DoNotSendTheseInfomationForFullyDigitalizedEInvoice.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				var creator = new TestObjectCreator(Factory);
				AssertEquals("IsVisible", false, ItemSet.DoNotSendTheseInfomationForFullyDigitalizedEInvoice.IsVisible(creator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public void TestDoNotSendTheseInfomationForFullyDigitalizedEInvoiceLog()
		{
			var oldSettings = new ExcludedFullyDigitalizedElectronicInvoiceData() { BuyerAddress = true };
			var newSettings = new ExcludedFullyDigitalizedElectronicInvoiceData() { BuyerPhoneNumber = true, BuyerBankAccount = true };

			var expectedLogMessage = "Buyer's Address: Un-ticked, Buyer's Phone Number: Ticked, Buyer's Bank Account: Ticked";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.DoNotSendTheseInfomationForFullyDigitalizedEInvoice, oldSettings, newSettings);
			var logReference = ItemSet.DoNotSendTheseInfomationForFullyDigitalizedEInvoice.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		#endregion

		#region JournalEntriesNumberCustomisation

		public void TestJournalEntriesNumberCustomisation()
		{
			var item = ItemSet.JournalEntriesNumberCustomisation;
			AssertEquals("JournalEntriesNumberCustomisation", item.Name);
			AssertEquals(Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, item.Category);
			AssertEquals("Journal Entries Number Customization", item.Caption);
			AssertEquals(@"This registry controls the auto allocation of unique reference number to each set of Journal Entries generated.

Before configuring the unique reference number format, please ensure that 'Generate and Store Journal Entries for Posted Accounting Transactions' has been enabled. 

By default, no reference number will be assigned to journal entries.

Note:
1. For 'GEN' allocation option, the system will start allocating number from the first journal entries in the first accounting period order by Post Date, Ledger + (Transaction Type + Transaction Number) / (Job Number). Once the number allocation is up-to-date, the system will allocate number as journal entries are generated. 
2. GL Journal can no longer be edited once unique reference number has been allocated. GL Journal can be reversed and re-entered if required.
3. IMPORTANT: Please configure Number Rule and Allocation Option as desired. Both Number Rule and Allocation Option cannot be edited once Allocation Option has been set to GEN.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesNumberCustomisation", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.JournalEntriesNumberCustomisation.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesNumberCustomisation", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is Default", RegistryOptions.Default, ItemSet.JournalEntriesNumberCustomisation.Options);
			}
		}

		public void TestJournalEntriesClassificationGroup()
		{
			var item = ItemSet.JournalEntriesClassificationGroup;
			AssertEquals("JournalEntriesClassificationGroup", item.Name);
			AssertEquals(Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, item.Category);
			AssertEquals("Journal Entries Classification Group", item.Caption);
			AssertEquals(@"By default, all journal entries will share one sequence number and there will be no classification group. 

If required, you can group transaction types and assign a 'Journal Entries Classification Code to each group, and include this code when configuring the 'Journal Entries Number Customization' with the 'Fountain' check box ticked.

Note:
1. AP CTR will have the same 'Group Code' as AR CTR.
2. If a 'Group Code' is assigned to one ledger and transaction type, a Group Code must be assigned to all ledgers and transaction types. 
3. You can create a separate number fountain for each Journal Entries Classification Group by ticking the 'Fountain' check box in the 'Journal Entries Number Customization' registry. Then all transaction types within each group will share the same number sequence.
4. The Journal Entries Classification Group cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesClassificationGroup", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.JournalEntriesClassificationGroup.Options);
			}
			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesClassificationGroup", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is Default", RegistryOptions.Default, ItemSet.JournalEntriesClassificationGroup.Options);
			}
		}

		public void TestJournalEntriesClassificationGroupCode()
		{
			var item = ItemSet.JournalEntriesClassificationGroupCode;
			AssertEquals("JournalEntriesClassificationGroupCode", item.Name);
			AssertEquals(Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, item.Category);
			AssertEquals("Journal Entries Classification Group Code", item.Caption);
			AssertEquals(@"The Journal Entries Classification Group Code is used to group multiple ledgers and transaction types together for the classification of journal entries and number sequencing.

Note: The Journal Entries Classification Group Code cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesClassificationGroupCode", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.JournalEntriesClassificationGroupCode.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesClassificationGroupCode", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is Default", RegistryOptions.Default, ItemSet.JournalEntriesClassificationGroupCode.Options);
			}
		}

		#endregion

		#region EnableAJLandRJLForChinaCompany

		public void TestEnableAJLandRJLForChinaCompany()
		{
			var item = ItemSet.EnableAJLandRJLForChinaCompany;
			AssertEquals("EnableAJLandRJLForChinaCompany", item.Name);
			AssertEquals(Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, item.Category);
			AssertEquals("Enable AJL and RJL for China Company (CWSupport Only)", item.Caption);
			AssertEquals(@"By default, the registry is set to No.
When this registry is overridden to Yes, the system will display the registry 'Enable AJL and RJL Creation'.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);

			var data = new GeneralLedgerDataFeatureControlModel()
			{
				EnableAJLRJLForCN = true
			};
			var mockIFeatureData = new Mock<IFeatureData>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out data)).Returns(true);
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			AssertEquals("If feature control data is not set up or set to false, EnableAJLandRJLForChinaCompany registry default value is false", false, ItemSet.EnableAJLandRJLForChinaCompany.DefaultValue);
			AssertEquals("EnableAJLandRJLForChinaCompany registry is not overrided", false, ItemSet.EnableAJLandRJLForChinaCompany.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("EnableAJLandRJLForChinaCompany", TimeSpan.FromMilliseconds(-1));
				AssertEquals("If feature control data is set to true, EnableAJLandRJLForChinaCompany registry default value is true", true, ItemSet.EnableAJLandRJLForChinaCompany.DefaultValue);
				AssertEquals("EnableAJLandRJLForChinaCompany registry is not overrided", false, ItemSet.EnableAJLandRJLForChinaCompany.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		public void TestEnableAJLandRJL()
		{
			var item = ItemSet.EnableAJLandRJL;
			AssertEquals("EnableAJLandRJL", item.Name);
			AssertEquals(Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, item.Category);
			AssertEquals("Enable AJL and RJL Creation", item.Caption);
			AssertEquals(@"By default, the registry is set to No.
You can change it to Yes to enable the creation of AJL and RJL for your China company.
Note: Please ensure you have set 'Allocation Option' to 'GEN' in the registry 'Journal Entries Number Customization'.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(false, item.DefaultValue);
			Assert(item.CountryFilterPKs.Contains(Constants.CountryGuids.China));
			AssertEquals(RegistryOptions.IsHidden, item.Options);
			AccountingConfigurationRegistry.Instance.EnableAJLandRJLForChinaCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals(RegistryOptions.Default, ItemSet.EnableAJLandRJL.Options);
		}

		#endregion

		#region AllocateJournalEntriesNumberStartDate

		public void TestAllocateJournalEntriesNumberStartDate()
		{
			var item = ItemSet.AllocateJournalEntriesNumberStartDate;
			AssertEquals("AllocateJournalEntriesNumberStartDate", item.Name);
			AssertEquals(Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, item.Category);
			AssertEquals("Allocate Journal Entries Number - Start Date (CWSupport Only)", item.Caption);
			AssertEquals(@"By default, this registry value will be empty.
When a value is set, the system will commence to allocate unique reference number to journal entries posted from this date when the allocation option of the ""Journal Entries Number Customization"" registry is set to 'GEN'.

Note: The system will set this date to be the 'Start Date' of period 1 of the current fiscal year when the allocation option is set to 'GEN'.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("AllocateJournalEntriesNumberStartDate", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.AllocateJournalEntriesNumberStartDate.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("AllocateJournalEntriesNumberStartDate", TimeSpan.MinValue);
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is IsOnlyForSupport and IsReadOnly", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly, ItemSet.AllocateJournalEntriesNumberStartDate.Options);
				AssertEquals("AllocateJournalEntriesNumberStartDate should be empty.", DateTime.MinValue, item.Value);
			}
		}

		#endregion

		#region Implementation

		void ResetRegistryCache()
		{
			RegistryItemDictionary.Instance.PurgeAll();
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "VoucherAppointedPartiesCashierDefault";
				yield return "VoucherAppointedPartiesReviewerDefault";
				yield return "ARInvoiceMenuItem";
				yield return "ARLocalInvoiceMenuItem";
				yield return "GatewayBillingEnabled";
				yield return "StampDutyARDocumentMessage";
				yield return "NettingSystemOrg";
				yield return "NettingThresholdValue";
				yield return "AllowDescriptionChargeLineFilterInPeriodicInvoice";
				yield return "PeriodicBillingChargePostingPerformanceImprovementConfiguration";
				yield return "EnableLineLevelApprovalRequestForARCreditNote";
				yield return "MaximumNumberOfInvoicesAllowedOnJob";
				yield return "MatchStatus";
				yield return "MatchStatusReason";
				yield return "InvoiceTotalRounding";
				yield return "InvoiceTotalRoundingChargeCode";
				yield return "AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC";
				yield return "JobStatusUpdateRestrictionRule";
				yield return "DisbursementJobsClosureConfiguration";
				yield return "EnableEInvoicingAdjustment";
				yield return "SizeOfRequestedAPTransactionList";
				yield return "SupplyTypeClassificationCodesIsMandatory";
				yield return "SupplyTypeClassificationCodesList";
				yield return "CustomJobTaxBranchDefaultingRulesEngineConfiguration";
				yield return "CashAdvanceRequestDocumentTitle";
				yield return "CashAdvanceRequestDocumentMessage";
				yield return "CashAdvanceClearingAccount";
				yield return "IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation";
				yield return "AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid";
				yield return "AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid";
				yield return "SupplyTypeConfigurationByChargeGroup";
				yield return "AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB";
				yield return "PayablesCashAdvanceClearingAccount";
				yield return "InvoicePostingExchangeRateOptionAR";
				yield return "InvoicePostingExchangeRateOptionAP";
				yield return "BackDateInvoiceDateDeadline";
				yield return "ElectronicInvoiceDataElementsConfiguration";
				yield return "DSBSummaryAppendingRule";
				yield return "GenerateJournalEntriesStartDate";
				yield return "GL_ADVANCED_TURNOVER_TAX_RETURN_ACCOUNT";
				yield return "GL_SPECIAL_VAT_PREPAYMENT_ACCOUNT";
				yield return "GenerateAndStoreJournalEntriesForPostedAccountingTransactions";
				yield return "JournalEntriesLastProcessedDate";
				yield return "VietnamIssuePositiveAdjustmentViaAmendWithInvoice";
				yield return "ThirdPartyEInvoiceDocType";
				yield return "JournalEntriesNumberCustomisation";
				yield return "JournalEntriesClassificationGroup";
				yield return "JournalEntriesClassificationGroupCode";
				yield return "ElectronicProcessingChargeDisbursementClearingAccount";
				yield return "ElectronicProcessingChargePayableClearingAccount";
				yield return "ElectronicProcessingChargeCode";
				yield return "ElectronicProcessingChargeDescriptionOverride";
				yield return "CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting";
				yield return "AlternateChartOfAccountsForSAFT";
				yield return "EnableAJLandRJL";
				yield return "DraftTransactionStatusReasons";
				yield return "StatusReasonMandatory";
			}
		}

		Guid CompanyPK;
		Guid BranchPK;
		Guid DepartmentPK;
		IDisposable mock;
		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = Guid.NewGuid();
			BranchPK = Guid.NewGuid();
			DepartmentPK = Guid.NewGuid();

			mock = ObjectFactory.New<ITranslatableDataTransformHelper>().MockResourceStringSources();
		}

		protected override void TearDown()
		{
			mock.Dispose();
		}

		#endregion
	}

	[TestedType(typeof(ConsumptionTaxGroupReportingCompanyDataType))]
	class ConsumptionTaxGroupReportingCompanyDataTypeTest : RegistryDataTypeTestCase<ConsumptionTaxGroupReportingCompanyDataType>
	{
		protected override ConsumptionTaxGroupReportingCompanyDataType GetNewDataType()
		{
			return new ConsumptionTaxGroupReportingCompanyDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Guid guid = Guid.NewGuid();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(guid, new ConsumptionTaxGroupReportingCompanyDataType().Serialise(guid)),
				new ValidSampleAndBinaryValueInDB(Guid.Empty, new ConsumptionTaxGroupReportingCompanyDataType().Serialise(Guid.Empty))
			};
		}
	}
}
