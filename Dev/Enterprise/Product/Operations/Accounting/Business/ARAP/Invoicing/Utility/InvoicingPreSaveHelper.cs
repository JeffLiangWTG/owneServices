using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IInvoicingPreSaveHelper
	{
		void PreSaveActionsForNonJobBillingPosting(InvoicingBase invoicingBase);
	}

	public class InvoicingPreSaveHelper : IInvoicingPreSaveHelper
	{
		public InvoicingPreSaveHelper(ILogger logger = null)
		{
			invoiceRoundingLineCreator_constructorInitializedOnly = new InvoiceRoundingLineCreator();
			serviceLogger = logger;
		}

		IInvoiceRoundingLineCreator InvoiceRoundingLineCreator => invoiceRoundingLineCreator_constructorInitializedOnly;
		IInvoiceRoundingLineCreator invoiceRoundingLineCreator_constructorInitializedOnly;
		readonly ILogger serviceLogger;

		public bool PreSaveActionsForPeriodicInvoice(InvoicingBase invoice, ISecurityOverrideProvider provider, Action<InvoicingBase> updateSecurityProviderMode)
		{
			PrepareTransactionsForAuthorisationCalculation(invoice);
			Dictionary<Guid, Guid> creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
			SecurityOverrideProviderSource.Get(invoice).Provider = provider;
			updateSecurityProviderMode?.Invoke(invoice);
			return invoice.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping, true);
		}

		void IInvoicingPreSaveHelper.PreSaveActionsForNonJobBillingPosting(InvoicingBase invoice)
		{
			var surchargeLineCreator = ObjectFactory.Get<ISurchargeLineCreator>();

			surchargeLineCreator.AddSurchargeLine(invoice);
			InvoiceRoundingLineCreator.AddRoundingLine(invoice);
		}

		public PreSaveActionsResult PreSaveActions(InvoicingBase invoice, IPostingJobAndTransactionApprovalGUIProvider approvalGUIProvider, bool isBulkPosting, IPostingTransactionApprovalGUIProvider aRCreditNoteForAmendingApprovalGUIProvider = null, bool isTransactionWithApprovalRequest = false, bool alwaysCreateApprovalRequest = false, bool shouldDisplayMessagesToGUI = false, Action<InvoicingBase> updateSecurityProviderMode = null)
		{
			((IInvoicingPreSaveHelper)this).PreSaveActionsForNonJobBillingPosting(invoice);
			PrepareTransactionsForAuthorisationCalculation(invoice);

			if (invoice.HasContext(BusinessContext.AmendingInvoice))
			{
				Argument.NotNull(aRCreditNoteForAmendingApprovalGUIProvider, nameof(aRCreditNoteForAmendingApprovalGUIProvider));
			}

			if (invoice == null)
			{
				return PreSaveActionsResult.Failure(Res.GetString("97949230-81F7-4751-B2E7-8D715C383DD0", "The invoice must not be null."));
			}

			PreSaveActionsResult result;
			invoice.ResetRelatedJobsForReversing();
			if (!invoice.HasContext(BusinessContext.ValidateTransactionForApproveAndPost) && invoice.HasClosedJob && !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(invoice, invoice.RelatedJobsForReversing) && invoice.ShouldReopenJob)
			{
				invoice.ResetRelatedJobsForReversing();
				result = PreSaveActionsResult.Failure(Res.GetString("6D353000-9E8B-4171-81D5-CF41F985F419", "Job requires reopening."));
				if (!shouldDisplayMessagesToGUI)
				{
					invoice.AddRowError(result.ErrorMessage);
				}
			}
			else if (invoice.HasContext(BusinessContext.AmendingInvoice))
			{
				return ProcessAmendingTransactions(invoice, aRCreditNoteForAmendingApprovalGUIProvider, serviceLogger);
			}
			else
			{
				result = CheckLevelSecurityRight(invoice, aRCreditNoteForAmendingApprovalGUIProvider, serviceLogger, updateSecurityProviderMode);
				if (result.HasError && !shouldDisplayMessagesToGUI)
				{
					invoice.AddRowError(Res.GetString("01A66065-197F-41E0-8EAF-1625FD72C05E", "You don't have security right to post this transaction."));
				}

				if (!invoice.HasContext(BusinessContext.ValidateTransactionForApproveAndPost) && result.CanProceed)
				{
					if (!AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
					{
						invoice.AddRowError(Res.GetString("36B6C9B2-322D-4978-B48D-3EAA5D959E25", "This transaction is linked to approval request but ‘{0}’ registry has been set to ‘No’. Please raise an eRequest to request assistance to set this registry to ‘Yes’ before posting the transaction.", AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Caption));
					}
					else
					{
						InvoicingBase transactionToApprove = invoice as APInvoice;
						if (transactionToApprove == null && isTransactionWithApprovalRequest)
						{
							transactionToApprove = invoice as APCreditNote;
						}

						if (transactionToApprove != null && transactionToApprove.IsBeingCreatedAllocatedOrCompletedAndNotConvertedFromAR)
						{
							var provider = transactionToApprove.SecurityOverrideProvider;
							try
							{
								var securityRightsMessage = Res.GetString("01F38062-625F-41E0-8EAF-8973AA72B57D", "You don't have security right to post this transaction. Please go to {0}", AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.Location());
								result = new APInvoiceLevelAuthorizationWithApprovalRequest(approvalGUIProvider, transactionToApprove, alwaysCreateApprovalRequest).PerformLevelAuthorization() ? PreSaveActionsResult.Success() : PreSaveActionsResult.Failure(securityRightsMessage, true);
								if (result.HasError && !shouldDisplayMessagesToGUI)
								{
									invoice.AddRowError(securityRightsMessage);
								}
							}
							finally
							{
								transactionToApprove.SecurityOverrideProvider = provider;
							}
						}
					}
				}

				if (result.CanProceed && invoice.HasInvalidPostingGroups)
				{
					result = CheckPostingGroups(invoice, isBulkPosting, shouldDisplayMessagesToGUI, approvalGUIProvider);
				}

				if (result.CanProceed && invoice.HasClosedJob)
				{
					result = ProcessInvoiceWithClosedJob(invoice, isBulkPosting);
				}
			}

			if (CountrySpecificValidationHelper.NeedToCheckCompanyAndOrgsRegistrationNumber() && GlbCompany.CurrentCompany.GC_BusinessRegNo.IsEmpty)
			{
				result.WarningMessage = Res.GetString("8D38583C-9E90-467C-910B-EFBB2C46D836", "Tax Registration Number of the Login Company is missing, this is mandatory for your country/region reporting.");
			}

			return result;
		}

		internal static void PrepareTransactionsForAuthorisationCalculation(InvoicingBase invoice)
		{
			if (invoice is ARCreditNote || invoice is ARAdjustmentNote)
			{
				invoice.TransactionsForAuthorisationCalculation = new List<InvoicingBase>();
				invoice.TransactionsForAuthorisationCalculation.Add(invoice);

				if (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.Value)
				{
					invoice.TransactionsForAuthorisationCalculation.AddRange(invoice.GetLineLevelTransactionsGroupedByBranchAndDept());
				}
			}
		}

		static PreSaveActionsResult CheckPostingGroups(InvoicingBase invoice, bool isBulkPosting, bool isGUI, IPostingJobAndTransactionApprovalGUIProvider guiProvider)
		{
			var result = PreSaveActionsResult.Success();
			var invalidPostingGroupsMessage = Res.GetString("0B371D9E-9E49-4F8A-97A2-CEA86EA89433", "You have prepared charges using a mix of Tax ID Posting Groups.");

			if (invoice.HasContext(BusinessContext.ValidateTransactionForApproveAndPost))
			{
				if (isBulkPosting || (guiProvider.IsPostingWithInvalidPostingGroupsAllowed(invoice) == ContinueWithSave.No))
				{
					if (!isGUI)
					{
						invoice.AddRowError(invalidPostingGroupsMessage);
					}
				}
			}
			else
			{
				result = guiProvider.IsPostingWithInvalidPostingGroupsAllowed(invoice) == ContinueWithSave.Yes ? PreSaveActionsResult.Success() : PreSaveActionsResult.Failure(invalidPostingGroupsMessage);
			}

			return result;
		}

		static PreSaveActionsResult ProcessInvoiceWithClosedJob(InvoicingBase invoice, bool isBulkPosting)
		{
			var result = PreSaveActionsResult.Success();

			if (invoice.ShouldReopenJob || invoice.HasContext(BusinessContext.ValidateTransactionForApproveAndPost))
			{
				var postingRequiresJobReopeningMessage = Res.GetString("C86CAB24-95C4-450E-BE3E-9911A64266F7", "Posting requires job reopening.");

				if (invoice.HasContext(BusinessContext.ValidateTransactionForApproveAndPost) &&
							(isBulkPosting ||
							!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(invoice, invoice.RelatedJobsForReversing) ||
							!invoice.ReOpenClosedJob()))
				{
					invoice.AddRowError(postingRequiresJobReopeningMessage);
				}
				else
				{
					result = invoice.ReOpenClosedJob() ? PreSaveActionsResult.Success() : PreSaveActionsResult.Failure(postingRequiresJobReopeningMessage);
				}
			}

			return result;
		}

		static PreSaveActionsResult ProcessAmendingTransactions(InvoicingBase invoice, IPostingTransactionApprovalGUIProvider aRCreditNoteForAmendingApprovalGUIProvider, ILogger serviceLogger)
		{
			var result = PreSaveActionsResult.Success();

			IAmending amending = invoice as IAmending;
			if (amending != null)
			{
				if (amending.OriginalTransaction is IReversing reversing && reversing.IsReversed)
				{
					result = PreSaveActionsResult.Failure(Res.GetString("067182D6-DD55-4A65-8FCF-84ECBCE5AC50", "Cannot amend selected transaction as this has been reversed."));
				}
			}

			if (result.CanProceed)
			{
				result = CheckLevelSecurityRight(invoice, aRCreditNoteForAmendingApprovalGUIProvider, serviceLogger);
			}

			if (result.CanProceed && amending != null && invoice.ShouldReopenJob)
			{
				invoice.ReOpenClosedJob();
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		static PreSaveActionsResult CheckLevelSecurityRight(InvoicingBase invoice, IPostingTransactionApprovalGUIProvider gUIProvider, ILogger serviceLogger, Action<InvoicingBase> updateSecurityProviderMode = null)
		{
			var result = PreSaveActionsResult.Success();

			if (invoice.IsAmendingWithARCreditNote
				&& (invoice as IAmending)?.OriginalTransaction != null)
			{
				result = new ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest(gUIProvider, (ARCreditNote)invoice, serviceLogger).PerformLevelAuthorization()
					? PreSaveActionsResult.Success() : PreSaveActionsResult.Failure(Res.GetString("266F1553-B247-4985-BEF9-C6EB2A88E93B", "Level authorization check failed with Approval Request."));
			}
			else if (invoice.IsBeingCreatedPostedAllocatedApprovedOrIncomplete)
			{
				if (invoice is ARCreditNote || invoice is ARAdjustmentNote)
				{
					var creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
					updateSecurityProviderMode?.Invoke(invoice);
					result = invoice.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping, true)
						? PreSaveActionsResult.Success() : PreSaveActionsResult.Failure(Res.GetString("40108F2F-562E-4A30-9E84-9E26E8481879", "Level security check failed for AR Credit/Adjustment Note."), true);
				}
				else
				{
					result = invoice.CheckLevelSecurityRights()
						? PreSaveActionsResult.Success() : PreSaveActionsResult.Failure(Res.GetString("A4F956B2-29E6-4218-BB8E-DCBED215F4CC", "Level security check failed for Invoice."));
				}
			}

			return result;
		}

		public static bool ShouldAddErrorIfInvoiceDateIsInTheFuture(ZGuid companyPK, ZPropertyInfo datePropertyInfo, Type invoiceType)
		{
			if (AccountingMasterFilesRegistry.Instance.DisallowPostingInvoicesWithAFutureInvoiceDate.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				var supportType = new Type[]
				{
					typeof(ARInvoice),
					typeof(ARCreditNote),
					typeof(ARAdjustmentNote),
					typeof(PeriodicInvoiceBase),
					typeof(ChangeTransactionDatesBusinessObjectBase),
				};

				var supportedPropertyNames = new string[] {
					AutoAccTransactionHeader.Schema.AH_InvoiceDate,
					AutoJobCharge.Schema.JR_APInvoiceDate,
					PeriodicInvoiceBase.Schema.InvoiceDate,
					ChangeTransactionDatesBusinessObjectBase.Schema.InvoiceDate
				};
				if (supportedPropertyNames.Contains(datePropertyInfo.Name) && supportType.Contains(invoiceType) && datePropertyInfo.Value is ZDateTime && ((ZDateTime)datePropertyInfo.Value).Date > ZDateTime.Today)
				{
					return true;
				}
			}

			return false;
		}

#if DEBUG
		public void SubstituteInvoiceRoundingLineCreator_ForTestOnly(IInvoiceRoundingLineCreator replacement) => invoiceRoundingLineCreator_constructorInitializedOnly = replacement;
		public IInvoiceRoundingLineCreator InvoiceRoundingLineCreator_ExposedForTestOnly => InvoiceRoundingLineCreator;
#endif
	}
}
