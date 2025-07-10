using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public delegate void SetSecurityProviderDelegate(params InvoicingBase[] transactions);
	public delegate bool? ShowReversalDatesForm(TransactionHeaderCollection reversedInvoices);

	public class JobInvoicingReverser
	{
		public JobInvoicingReverser(Job jobToReverse)
			: this(jobToReverse, null, null)
		{
		}

		public JobInvoicingReverser(Job jobToReverse, SetSecurityProviderDelegate securityProviderSetter, IPostingJobTransactionsApprovalGUIProvider approvalGUIProvider)
		{
			this.Factory = jobToReverse.CreateNewFactory();
			this.JobToReverse = (Job)ReversingFactory.ImportFromAnotherFactory(jobToReverse);
			ApprovalGUIProvider = approvalGUIProvider;
			InvoicesAndCreditNotesToReverse = GetInvoicesAndCreditNotes(securityProviderSetter);
		}

		public BusinessObjectFactory ReversingFactory
		{
			get { return Factory; }
		}

		public string IsValidToReverseAllInvoices()
		{
			if (JobToReverse.HasChanges)
			{
				return Res.GetString("28af6a78-ec9e-4dc2-84bd-9cb429f57236", "Please save job {0} before posting costs and/or charges.", JobToReverse.JH_JobNum);
			}

			if (JobToReverse.IsWorkOnHold || JobToReverse.IsInvoiceOnHold || JobToReverse.IsReadyForFinancialClosureWithoutPostSecurity)
			{
				return Res.GetString("518fc319-76b0-40a8-94d9-11a3c587ffc7", "Cannot reverse Invoices because the job has status '{0}'.", JobToReverse.JobStatusList.GetDescriptionFromCode(JobToReverse.JH_Status));
			}

			foreach (InvoicingBase invoiceOrCreditNote in InvoicesAndCreditNotesToReverse)
			{
				if (invoiceOrCreditNote.IsPartiallyOrFullyPaid && (!((invoiceOrCreditNote as IInvoiceAssociatedToCashAdvanceRequest)?.IsOutstandingAmountPaidOnlyViaCashAdvance() ?? false)))
				{
					return Res.GetString("9559efac-1c7e-4af9-a598-229b2f43e082", "Cannot reverse Invoices because Invoice {0} is partially or fully paid.", invoiceOrCreditNote.AH_TransactionNum);
				}

				if (invoiceOrCreditNote.IsBelongToMultipleJobs)
				{
					return Res.GetString("a17ffe44-5e73-4878-bb94-372a63370f85", "Cannot reverse Invoices because Invoice {0} contains charges that belong to multiple jobs.\r\n\r\nHowever, you can reverse this invoice through the reverse function under Manage > Receivables > Receivables Transactions by selecting the transaction and click Reverse", invoiceOrCreditNote.AH_TransactionNum);
				}
			}

			return string.Empty;
		}

		public void ReverseAllInvoices(string reversingReason, string reversingCode)
		{
			ReverseAllInvoices(reversingReason, reversingCode, null);
		}

		public void ReverseAllInvoices(string reversingReason, string reversingCode, ShowReversalDatesForm showReversalDatesForm)
		{
			if (string.IsNullOrEmpty(IsValidToReverseAllInvoices()))
			{
				JobToReverse.IsReversingInProcess = true;

				using (JobToReverse.GetValidationSuspender())
				using (JobToReverse.CheckForChargesDisplaySequenceDuplicatesSuspender.GetSuspender())
				{
					try
					{
						bool canContinue = true;
						var invoicesToBeReversed = InvoicesAndCreditNotesToReverse.Where(x => x.AH_TransactionType == TransactionTypes.Invoice && x.AH_Ledger == LedgerTypes.AccountsReceivable);
						if (ApprovalGUIProvider != null && invoicesToBeReversed.Any())
						{
							var result = PerformReversalAuthorization(reversingCode, invoicesToBeReversed);
							canContinue = result.Item1;
							ContinueWithApprovalFactorySave = result.Item2;
						}

						if (canContinue)
						{
							var reversedInvoices = new JobTransactionReverser(InvoicesAndCreditNotesToReverse).ReverseAllInvoices(reversingReason, reversingCode);
							if (reversedInvoices.Count() == InvoicesAndCreditNotesToReverse.Count())
							{
								var reversedCollection = new TransactionHeaderCollection(Factory);
								reversedCollection.AddRange(reversedInvoices.Select(x => x.ReverseInvoice).Where(x => x != null));

								bool? reversalDateFormResult = true;

								if (showReversalDatesForm != null && InvoicesAndCreditNotesToReverse.Any())
								{
									reversalDateFormResult = showReversalDatesForm(reversedCollection);
								}

								if (reversalDateFormResult.HasValue)  //null means cancel (don't save)
								{
									if (!reversalDateFormResult.Value)  //revert to today's date
									{
										foreach (InvoicingBase reversedInvoice in reversedCollection)
										{
											reversedInvoice.AH_InvoiceDate = ZDateTime.Today;
											reversedInvoice.AH_PostDate = ZDateTime.Today;
										}
									}

									new WIPAccrual.WIPAccrualReverser(JobToReverse, Factory).ReverseAll(false);
									if (AreReversalsValid())
									{
										ContinueWithSave = true;
										JobToReverse.ReOpenJobStatus();
										JobToReverse.ClearARLinks(InvoicesAndCreditNotesToReverse);
										JobToReverse.ClearUnpostedChargesAmounts();
										JobToReverse.CreateWIPsForPostedCost();
									}
								}
							}

							if (!ContinueWithSave)
							{
								ContinueWithApprovalFactorySave = false;
							}
						}
					}
					finally
					{
						JobToReverse.IsReversingInProcess = false;
						JobToReverse.UpdateTotals();
					}
				}
			}
		}

		protected virtual Tuple<bool, bool> PerformReversalAuthorization(string reversingCode, IEnumerable<InvoicingBase> invoicesToBeReversed)
		{
			return new ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest(ApprovalGUIProvider, reversingCode: reversingCode).PerformLevelAuthorizationForReversing(invoicesToBeReversed.ToArray());
		}

		public bool ContinueWithSave { get; private set; }
		public bool ContinueWithApprovalFactorySave { get; private set; }

		internal bool AreReversalsValid()
		{
			bool result = true;
			bool supportComplianceSubType = GlbCompany.CurrentCompany.Country.SupportComplianceSubType;

			ZString complianceError;
			foreach (var reverseInvoice in InvoicesAndCreditNotesToReverse.Select(x => x.ReverseInvoice).Where(x => x != null))
			{
				if (reverseInvoice.IsDeleted)
				{
					result = false;
					Errors.Add(Res.GetString("EAF497F9-2590-451A-ABF5-9F6C1CC76950", "Should not delete a reverse invoice."));
				}
				else if (supportComplianceSubType
					&& reverseInvoice.IsComplianceNumberAllocationMandatory
					&& reverseInvoice.ShouldAllocateComplianceNumberOnPosting()
					&& !(complianceError = reverseInvoice.AssignComplianceSubTypeAndCheckComplianceErrors()).IsEmpty)
				{
					result = false;
					Errors.Add(reverseInvoice.OriginalTransaction.AH_TransactionNum + ":\t" + complianceError);
				}
				else
				{
					reverseInvoice.RunPreSaveValidation();
					result = result && !reverseInvoice.HasRowErrors && !JobInvoicingReverserValidationHelper.HasErrors(reverseInvoice);

					if (!result)
					{
						foreach (INotification error in reverseInvoice.Notifications.GetErrors())
						{
							Errors.Add(error.Message);
						}
					}
				}
			}
			return result;
		}

		public StringCollectionX Errors = new StringCollectionX();

		#region Implementation

		protected internal Job JobToReverse;
		protected BusinessObjectFactory Factory;
		protected IEnumerable<InvoicingBase> InvoicesAndCreditNotesToReverse;
		protected IPostingJobTransactionsApprovalGUIProvider ApprovalGUIProvider;

		/// <summary>
		/// Finds all AR invoices and Credit notes that have references for the Job
		/// </summary>
		/// <returns>Collection of AR invoices and Credit notes suitable for reversing</returns>
		protected IEnumerable<InvoicingBase> GetInvoicesAndCreditNotes(SetSecurityProviderDelegate securityProviderSetter)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(InvoicingBase));
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.Equal, JobToReverse.JH_GC);
			query.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(InvoicingLineBase), AccTransactionLinesSchema.AL_AH);
			subQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, JobToReverse.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var invoicesAndCreditNotesToReverse = Factory.Load<InvoicingBase>(new InvoicingBaseCollection(Factory, query).CompleteFilter);

			if (securityProviderSetter != null)
			{
				securityProviderSetter(invoicesAndCreditNotesToReverse);
			}

			return invoicesAndCreditNotesToReverse;
		}

		#endregion
	}
}
