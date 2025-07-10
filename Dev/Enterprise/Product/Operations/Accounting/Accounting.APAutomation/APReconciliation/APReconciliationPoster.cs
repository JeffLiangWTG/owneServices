using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAPReconciliationPoster
	{
		T PostFromDraftInvoice<T>(AccDraftInvoiceHeader draftInvoice
			, PosterConfigurationDTO posterConfiguration
			, out (string Msg, string Caption) validationError
			, out (string Msg, string Caption) reconciliationError
		) where T : InvoicingBase;

		T AutoReconcileAndPost<T>(AccDraftInvoiceHeader draftInvoice
			, out string errorMessage
		) where T : InvoicingBase;

		APReconciliationProcessingResult ReconcileFromDraftInvoice<T>(AccDraftInvoiceHeader draftInvoice
			, out (string Msg, string Caption) validationError
		) where T : InvoicingBase;
	}

	public class PosterConfigurationDTO
	{
		public Accruals[] SelectedAccruals { get; set; }

		public class Accruals : IReconciliationLineIdentifier
		{
			public Guid LineIdentifier { get; set; } //This can be JobCharge or ConsolCost PK
			public APReconciliationLineTypes LineType { get; set; } //This can be the tabel code (JR or E6)
		}
	}

	public class APReconciliationPoster : IAPReconciliationPoster
	{
		T IAPReconciliationPoster.PostFromDraftInvoice<T>(AccDraftInvoiceHeader draftInvoice
			, PosterConfigurationDTO posterConfiguration
			, out (string Msg, string Caption) validationError
			, out (string Msg, string Caption) reconciliationError
		)
		{
			reconciliationError = default;

			if (!PostFromDraftInvoiceValidationCore(draftInvoice, typeof(T), out validationError))
			{
				return null;
			}

			var invoicingBase = CreateInvoiceFromDraftInvoice<T>(draftInvoice);

			draftInvoice.AIH_AH_PostedTransactionHeader = invoicingBase.PK;

			if (posterConfiguration?.SelectedAccruals != null && posterConfiguration.SelectedAccruals.Any())
			{
				ObjectFactory.Get<IReconciliationLineConverter>()
					.ImportReconciliationLinesToInvoice(invoicingBase, posterConfiguration.SelectedAccruals);
			}
			else
			{
				AutoReconcileInvoice(draftInvoice, invoicingBase, out reconciliationError);
			}

			validationError = ProcessTaxesIfNecessary(invoicingBase);

			return invoicingBase;
		}

		T IAPReconciliationPoster.AutoReconcileAndPost<T>(AccDraftInvoiceHeader draftInvoice
			, out string errorMessage)
		{
			errorMessage = default;

			if (!PostFromDraftInvoiceValidationCore(draftInvoice, typeof(T), out var validationError))
			{
				errorMessage = validationError.Msg;
				return null;
			}

			var reconciliationResult = Reconcile(draftInvoice);
			if (reconciliationResult.Result != APReconciliationResultTypes.Success)
			{
				errorMessage = reconciliationResult.FailureReason;
				return null;
			}

			var invoicingBase = CreateInvoiceFromDraftInvoice<T>(draftInvoice);

			ObjectFactory.Get<IReconciliationLineConverter>()
				.ImportReconciliationLinesToInvoice(invoicingBase, reconciliationResult.ReconciliableAccruals);

			var taxCalculationError = ProcessTaxesIfNecessary(invoicingBase);
			if (taxCalculationError != default)
			{
				errorMessage = taxCalculationError.Msg;
				return null;
			}

			invoicingBase.RunPreSaveValidation();
			if (invoicingBase.HasErrors)
			{
				errorMessage = invoicingBase.GetErrors().ToMessageListString();
				return null;
			}

			draftInvoice.AIH_AH_PostedTransactionHeader = invoicingBase.PK;

			return invoicingBase;
		}

		(string Msg, string Caption) ProcessTaxesIfNecessary(InvoicingBase invoicingBase)
		{
			var processFailureCaption = Res.GetString("4c950d62-dbb3-4abe-adca-e1e50619953f", "Fail to calculate tax records");
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoicingBase);

			if (taxRecordParent.ShouldCalculateTaxTransactions)
			{
				using (invoicingBase.SuspendExpectedInvoiceTotalValidation)
				{
					invoicingBase.MarkAsNeedingValidationIncludingChildren();
					invoicingBase.RunPreSaveValidation();
				}

				if (invoicingBase.HasErrors)
				{
					return (Res.GetString("b0da6c60-3566-4c2e-9a1f-ca4360897bed", "There are Accounting transaction errors to be corrected before calculating tax records."), processFailureCaption);
				}

				var errorMessage = ObjectFactory.Get<ITaxProcessor>().ProcessTaxesOnPosting(taxRecordParent);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					return (errorMessage, processFailureCaption);
				}
			}

			return default;
		}

		T CreateInvoiceFromDraftInvoice<T>(AccDraftInvoiceHeader draftInvoice) where T : InvoicingBase
		{
			var invoicingBase = draftInvoice.Factory.New<T>();

			invoicingBase.Factory.ServiceContainer.AddService(new CreditorInvoicedExchangeRateProvider(draftInvoice));

			invoicingBase.SubmittedFromInvoicingForm = true;
			invoicingBase.AH_TransactionNum = draftInvoice.AIH_TransactionNumber;

			if (invoicingBase.ShouldShowOriginalInvoiceReferenceFields)
			{
				AssignOriginalTransactionInfo();
			}

			if (invoicingBase.AH_OH != draftInvoice.AIH_OH_Creditor)
			{
				invoicingBase.AH_OH = draftInvoice.AIH_OH_Creditor;
			}

			if (draftInvoice.AIH_OA_CreditorAddress.IsValid)
			{
				invoicingBase.AH_OA_InvoiceAddressOverride = draftInvoice.AIH_OA_CreditorAddress;
			}

			if (draftInvoice.AIH_OC_CreditorContact.IsValid)
			{
				invoicingBase.AH_OC_InvoiceContactOverride = draftInvoice.AIH_OC_CreditorContact;
			}

			if (!draftInvoice.AIH_RX_NKTransactionCurrency.IsEmpty)
			{
				invoicingBase.ExchangeRate.Currency = draftInvoice.AIH_RX_NKTransactionCurrency;
			}

			if (!draftInvoice.AIH_PostDate.IsEmpty)
			{
				invoicingBase.AH_PostDate = draftInvoice.AIH_PostDate;
			}

			if (!draftInvoice.AIH_TransactionDate.IsEmpty)
			{
				invoicingBase.AH_InvoiceDate = draftInvoice.AIH_TransactionDate;
			}

			if (!draftInvoice.AIH_DocumentReceivedDate.IsEmpty)
			{
				invoicingBase.AH_DocumentReceivedDate = draftInvoice.AIH_DocumentReceivedDate;
			}

			if (!draftInvoice.AIH_DueDate.IsEmpty)
			{
				invoicingBase.AH_DueDate = draftInvoice.AIH_DueDate;
			}

			invoicingBase.AH_Desc = draftInvoice.AIH_Description;
			invoicingBase.SetExpectedOSAmountFromAccDraftInvoice(draftInvoice);
			invoicingBase.DraftInvoiceHeaderPK = draftInvoice.PK;

			return invoicingBase;

			void AssignOriginalTransactionInfo()
			{
				if (draftInvoice.AIH_AH_OriginalTransaction.IsValid)
				{
					invoicingBase.OriginalTransactionReference = draftInvoice.AIH_AH_OriginalTransaction;
				}
				else
				{
					invoicingBase.AH_OriginalTransactionNum = draftInvoice.AIH_OriginalTransactionNum;
					invoicingBase.AH_OriginalInvoiceDate = draftInvoice.AIH_OriginalInvoiceDate;
				}
			}
		}

		APReconciliationProcessingResult IAPReconciliationPoster.ReconcileFromDraftInvoice<T>(AccDraftInvoiceHeader draftInvoice
			, out (string Msg, string Caption) validationError
		)
		{
			if (!PostFromDraftInvoiceValidationCore(draftInvoice, typeof(T), out validationError))
			{
				return null;
			}

			return Reconcile(draftInvoice);
		}

		static bool PostFromDraftInvoiceValidationCore(AccDraftInvoiceHeader draftInvoiceHeader
			, Type targetType
			, out (string Msg, string Caption) error)
		{
			if (draftInvoiceHeader.Company == null || draftInvoiceHeader.Company.PK != GlbCompany.CurrentCompany.PK)
			{
				var humanReadableTransactionType = GetTargetHumanReadableTransactionType(targetType);
				var message = Res.GetString("A3D7B9E2-BC89-4D79-ACB7-4F984AB5D3EA"
					, @"You are trying to post a draft {0} to company {1} - {2}.
You are either not logged into CargoWise, or logged in to a different company.
Please login to company {1} - {2} before attempting to post the draft {0}."
					, humanReadableTransactionType
					, draftInvoiceHeader.Company?.GC_Code
					, draftInvoiceHeader.Company?.GC_Name);
				var caption = Res.GetString("E4E80329-750F-4519-B3D2-C9269ECEB305"
					, "Draft {0} cannot be posted"
					, humanReadableTransactionType);

				error = (message, caption);
				return false;
			}

			if (!draftInvoiceHeader.AIH_AH_PostedTransactionHeader.IsEmpty)
			{
				var humanReadableTransactionType = GetTargetHumanReadableTransactionType(targetType);
				var message = Res.GetString("F7B2D8C1-5E6F-4A8D-9E7F-1D3B2C4A5E6F"
					, @"This draft {0} is already either posted as an AP {0}, an AP {0} awaiting approval, or AP {0} has been saved as incomplete."
					, humanReadableTransactionType);
				var caption = Res.GetString("D8F4C6A5-7E2B-4F8D-9D1B-FC6E3B2A9D4C"
					, "Draft {0} already posted"
					, humanReadableTransactionType);

				error = (message, caption);
				return false;
			}

			if (draftInvoiceHeader.AIH_Status == AccDraftInvoiceHeaderStatus.AwaitingApproval)
			{
				var message = Res.GetString("8708DF01-0F73-4501-9D94-220AAF238951", "This invoice has been marked as Awaiting Approval. Please approve the invoice prior to posting.");
				var caption = Res.GetString("BFBEC9DA-C6FD-45F5-86E1-5F1AABFF8A6D", "Transactions Awaiting Approved cannot be posted.");

				error = (message, caption);
				return false;
			}

			error = default;
			return true;
		}

		void AutoReconcileInvoice(AccDraftInvoiceHeader draftInvoice, InvoicingBase targetInvoice
			, out (string Msg, string Caption) error
		)
		{
			error = default;

			var reconcileResult = Reconcile(draftInvoice);

			if (reconcileResult.Result == APReconciliationResultTypes.Success)
			{
				ObjectFactory.Get<IReconciliationLineConverter>()
					.ImportReconciliationLinesToInvoice(targetInvoice, reconcileResult.ReconciliableAccruals);
			}
			else
			{
				var humanReadableTransactionType = GetTargetHumanReadableTransactionType(targetInvoice.GetType());
				var caption = Res.GetString("5EBE5D6A-8DFC-4057-A4FE-13C51CEBBB90"
					, "Unable to reconcile the {0}"
					, humanReadableTransactionType);

				error = (reconcileResult.FailureReason, caption);
			}
		}

		APReconciliationProcessingResult Reconcile(AccDraftInvoiceHeader draftInvoice)
		{
			return ObjectFactory.Get<IAPReconciliationProcessor>().Reconcile(draftInvoice);
		}

		static string GetTargetHumanReadableTransactionType(Type targetType)
		{
			return TransactionTypeMapping.TryGetValue(targetType, out var result)
				? result
				: throw new ArgumentException("targetType out of scope");
		}

		[ThreadSafe]
		static IReadOnlyDictionary<Type, string> TransactionTypeMapping { get; } = new Dictionary<Type, string>()
		{
			{ typeof(APInvoice), Res.GetString("B1F64A6E-86FA-4B6E-B3A9-C6D2F48E80B2", "invoice") },
			{ typeof(APCreditNote), Res.GetString("0B1D2E6A-4A5C-4D8E-98A9-3B4E7A1F6C2D", "credit note") },
		};
	}
}
