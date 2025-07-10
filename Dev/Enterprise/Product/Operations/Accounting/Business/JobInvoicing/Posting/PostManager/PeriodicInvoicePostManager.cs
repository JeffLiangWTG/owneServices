using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class PeriodicInvoicePostManager : BasePostManager
	{
		public PeriodicInvoicePostManager(PeriodicInvoice periodicInvoice)
			: base(periodicInvoice.Factory, periodicInvoice.SelectedJobs)
		{
			PeriodicInvoice = periodicInvoice;
		}

		readonly PeriodicInvoice PeriodicInvoice;

		bool IsMiscInvoicesPosted;

		protected override Charge[] Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = PeriodicInvoice.Charges.ToArray<Charge>();
				}
				return fCharges;
			}
		}

		protected override void RollbackPostingCore()
		{
			throw new NotSupportedException();
		}

		protected override bool CreateAllTransactions(TransactionCreatorHashtable transactions)
		{
			return CreateRevenueOnlyTransactions(transactions);
		}

		protected override void ProcessEligibleCharges(IReceivablesPostingChargeCollection filteredCharges)
		{
			foreach (IPostingCharge charge in filteredCharges)
			{
				var chargeWithCost = charge as ChargeWithCost;
				var sellReference = PeriodicInvoice.SellReference;
				if (!sellReference.IsEmpty && chargeWithCost.JR_SellReference.IsEmpty)
				{
					chargeWithCost.JR_SellReference = sellReference;
				}
			}
			base.ProcessEligibleCharges(filteredCharges);
		}

		protected override DisposableAction GetProcessEligibleChargesValidationDisposable(IBusiness charge)
			=> charge?.Factory?.HasContext(BusinessContext.PeriodicInvoiceHasAlreadyBeenValidatedInAnotherFactory) ?? false
				? DisposableAction.NoAction  // Validation is suspended when already run in another factory.
				: base.GetProcessEligibleChargesValidationDisposable(charge);

		protected override bool CreateRevenueOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			var reversingFactory = new ReversingFactory();
			var miscInvoices = PeriodicInvoice.SelectedMiscInvoices;
			foreach (InvoicingBase invoice in miscInvoices)
			{
				ReversingBase reverser = reversingFactory.NewReversing(invoice);
				if (!reverser.CanReverseTransaction)
				{
					fCancelPosting = true;
					break;
				}
			}

			bool result = PostReceivablesCharges(JobInvoicingPostingOption.Revenue, transactions);

			if (!CancelPosting && !IsMiscInvoicesPosted && miscInvoices.Any())
			{
				var uniqueBranches = new HashSet<ZGuid>();
				if (AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting)
				{
					foreach (var distinctBranchGroups in miscInvoices.GroupBy(x => x.AH_GB))
					{
						var parentBranch = BranchLevelPostingHelper.GetParentBranchPK(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, distinctBranchGroups.Key);
						if (!uniqueBranches.Contains(parentBranch))
						{
							uniqueBranches.Add(parentBranch);
						}
					}
				}
				else
				{
					uniqueBranches.Add(GlbBranch.CurrentBranch.PK);
				}

				foreach (var branchPK in uniqueBranches)
				{
					InvoicingBase invoice1 = Poster.GetInvoice(new IReceivablesPostingChargeCollection());
					invoice1.AH_GB = branchPK;

					invoice1.AH_OH = PeriodicInvoice.DebtorPK;
					invoice1.ExchangeRate.Currency = PeriodicInvoice.CurrencyNK;
					invoice1.AH_TransactionCategory = PeriodicInvoice.InvoiceType;

					AddPeriodicInvoiceDataToPostedInvoice(invoice1);

					Poster.PostedInvoices.Add(invoice1);
					transactions.AddARInvoice(invoice1);
				}

				result = true;
			}
			return result;
		}

		protected override PostingChargeEligibilityDecider GetEligibilityDecider(Charge[] charges)
		{
			var predicate = new Predicate<Charge>(x => x.InvoicingJob != null && x.InvoicingJob.ConsumerTypeShouldCreateWIP(x.JR_InvoiceType));
			return new PeriodicInvoicePostingChargeEligibilityDecider(charges, predicate);
		}

		protected override ChargePoster GetPoster()
		{
			return new PeriodicInvoiceChargePoster(PeriodicInvoice);
		}

		protected override void OnInvoicePosted(JobInvoicingPostingOption postingOption, IReceivablesPostingChargeCollection distributedChargeCollection)
		{
			base.OnInvoicePosted(postingOption, distributedChargeCollection);

			AddPeriodicInvoiceDataToPostedInvoice(distributedChargeCollection.PostedInvoice);
		}

		void AddPeriodicInvoiceDataToPostedInvoice(InvoicingBase postedInvoice)
		{
			postedInvoice.AH_InvoiceDate = PeriodicInvoice.InvoiceDate;
			postedInvoice.AH_InvoiceTerm = PeriodicInvoice.InvoiceTerm;
			postedInvoice.AH_InvoiceTermDays = PeriodicInvoice.InvoiceTermDays;
			postedInvoice.AH_DueDate = PeriodicInvoice.DueDate;
			postedInvoice.AH_PostDate = PeriodicInvoice.PostDate;
			foreach (InvoicingLineBase invoiceLine in postedInvoice.Lines)
			{
				invoiceLine.AL_PostDate = PeriodicInvoice.PostDate;
			}

			if (ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, postedInvoice.IsLocalCurrencyTransaction, postedInvoice.AH_GC))
			{
				ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(postedInvoice, postedInvoice.Factory);
				ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(postedInvoice);
			}

			var jobs = (from InvoicingLineBase invoiceLine in postedInvoice.Lines where invoiceLine.Job != null select invoiceLine.Job).Distinct().ToArray();
			foreach (JobHeader header in jobs)
			{
				header.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, header.PK);
				header.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, header.JH_ParentID);
			}

			IEnumerable<InvoicingBase> miscInvoices;

			if (AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting)
			{
				miscInvoices = PeriodicInvoice.SelectedMiscInvoices.Where(x => BranchLevelPostingHelper.GetAssociatedBranches(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, postedInvoice.AH_GB).Contains(x.AH_GB));
			}
			else
			{
				miscInvoices = PeriodicInvoice.SelectedMiscInvoices;
			}

			if (miscInvoices.Any())
			{
				ReversingFactory reversingFactory = new ReversingFactory();
				using (postedInvoice.Lines.SuspendListChanged())
				{
					foreach (InvoicingBase invoice in miscInvoices)
					{
						foreach (InvoicingLineBase invoiceLine in invoice.Lines)
						{
							InvoicingLineBase newPostedInvoiceLine = (InvoicingLineBase)postedInvoice.Lines.AddNew();
							newPostedInvoiceLine.SetValues(invoiceLine, convertAmoutSignsBetweenTransactionTypes: true);
							newPostedInvoiceLine.AL_PostDate = PeriodicInvoice.PostDate;
							newPostedInvoiceLine.AL_OH = PeriodicInvoice.DebtorPK;
						}
						reversingFactory.NewReversing(invoice).Reverse();
					}
				}

				IsMiscInvoicesPosted = true;
			}
		}

		protected override void CheckForCriticalErrors(TransactionCreatorHashtable transactions)
		{
			base.CheckForCriticalErrors(transactions);

			InvoicingBase[] aRInvoicesAndCredits = transactions.GetAllARInvoicesAndCreditNotes();
			foreach (InvoicingBase aRInv in aRInvoicesAndCredits)
			{
				if (aRInv.ShouldCheckForCompliance && aRInv.ShouldAllocateComplianceNumberOnPostingForAR())
				{
					ZString complianceError = aRInv.AssignComplianceSubTypeAndCheckComplianceErrors();
					if (!complianceError.IsEmpty)
					{
						aRInv.AddRowError(complianceError);
						fCancelPosting = true;
						RaiseOnCriticalPostError(aRInv);
					}
				}

				var (_, errors) = ExporterExemptionValidationHelper.CheckExporterExemption(aRInv.Header, aRInv.AH_Ledger, aRInv.AH_PostDate, aRInv.Lines);
				if (!errors.IsEmpty)
				{
					aRInv.AddRowError(errors);
					fCancelPosting = true;
					RaiseOnCriticalPostError(aRInv);
				}
			}

			if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled)
			{
				foreach (InvoicingBase aRInv in aRInvoicesAndCredits)
				{
					if (aRInv is IInvoiceAssociatedToCashAdvanceRequest cahUpdater)
					{
						InvoiceValidation.ClearAllCashAdvanceRelatedRowErrors(aRInv as Invoice);
						var errorMessages = new Dictionary<string, List<string>>();
						var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages);
						cahUpdater.Accept(validator);

						if (errorMessages.Any())
						{
							var fullErrorMessageBuilder = new ZStringBuilder();
							foreach (var errorMessage in errorMessages)
							{
								fullErrorMessageBuilder.Append(errorMessage.Key);
								fullErrorMessageBuilder.Append(string.Join(System.Environment.NewLine, errorMessage.Value));
								fullErrorMessageBuilder.Append(string.Empty);
							}

							aRInv.AddRowError(fullErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
							fCancelPosting = true;
							RaiseOnCriticalPostError(aRInv);
						}
					}
				}
			}
		}
	}
}
