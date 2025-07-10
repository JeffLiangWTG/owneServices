using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingOptionSelection;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ConsolInvoicingPostManager : BasePostManager
	{
		public ConsolInvoicingPostManager(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, ApportionmentListing apportionments = null, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null)
				: base(fallbackFactory, jobs, apInvoicePostGUIProvider)
		{
			this.Consol = consol;
			if (apportionments != null)
			{
				apportionments.DeleteAllCreatedJobs();
			}
			ApportionmentListing apportionmentListing = new ApportionmentListing(Factory, consol);
			apportionmentListing.IsPosting = true;
			this.ConsolCosts = apportionmentListing.CostsCollection;
		}

		List<ZGuid> postponedConsolCostPKs = new List<ZGuid>();
		bool havePostponedConsolCosts;

		readonly IJobCostingPlugIn Consol;
		readonly JobConsolCostCollection ConsolCosts;

		public static MultilingualString NoChargesToPostMessage(MultilingualString extraSuggestions = null)
		{
			var zeroBalanceWarningMsg = AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value
				? string.Empty
				: System.Environment.NewLine + Res.GetString("A01C9159-21E5-4A6D-8DD8-4327C4AE1F41", "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices.");

			return ResString.GetMultilingualString("e5f00560-de3a-4033-8591-c38e69a18334",
@"No appropriate charges were found for posting. This may be because:{0}
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.{1}{2}
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).",
				zeroBalanceWarningMsg,
				(extraSuggestions?.IsEmpty ?? true) ? string.Empty : System.Environment.NewLine,
				extraSuggestions);
		}

		public static MultilingualString JobOnHoldMessage
		{
			get
			{
				return ResString.GetMultilingualString("62e8037a-c525-4572-bb5f-68786f3dabc8",
@"The following jobs are on hold and their associated job charges will not be posted. 
Further, any consol costs with apportionment to these jobs will not be posted.

To post these jobs later, change the job status from '{0}'.{1}", JobHeaderStatus.WorkOnHold.Code, System.Environment.NewLine);
			}
		}

		#region Create Cost Only and Consol Cost Only Transactions

		protected override bool CreateCostsOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			return DoCreateCostOnlyTransactions(transactions, false);
		}

		protected override bool CreateConsolCostsOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			return DoCreateCostOnlyTransactions(transactions, true);
		}

		bool DoCreateCostOnlyTransactions(TransactionCreatorHashtable transactions, bool createConsolCostOnly)
		{
			bool result = false;

			var costTransactionCreator = new ConsolAPInvoiceCreator(fallbackFactory, Jobs, HasJobOnHold, Consol, ConsolCosts, createConsolCostOnly, apInvoicePostGUIProvider: apInvoicePostGUIProvider);
			result |= costTransactionCreator.CreateTransactions(transactions);
			result |= APPaymentCreator.CreateTransactions(transactions);
			PaymentApprovalMatcher.Match(transactions);
			result |= APCreditNoteCreator.CreateTransactions(transactions);

			return result;
		}

		#endregion

		#region Create Gateway Only Transactions

		protected override bool CreateGatewayOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			return PostReceivablesCharges(JobInvoicingPostingOption.Gateway, transactions);
		}

		#endregion

		#region Create Agent Only Transactions

		protected override bool CreateAgentOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			bool result = false;
			bool runCreatePSAsARFirst = AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value;
			if (runCreatePSAsARFirst)
			{
				result = PostProfitShareAndMasterFreightCollectCharges(transactions, runCreatePSAsARFirst, false);
				result |= PostReceivablesCharges(JobInvoicingPostingOption.Agent, transactions);
				if (havePostponedConsolCosts)
				{
					result |= PostProfitShareAndMasterFreightCollectCharges(transactions, runCreatePSAsARFirst, true);
				}
			}
			else
			{
				result = PostReceivablesCharges(JobInvoicingPostingOption.Agent, transactions);
				result |= PostProfitShareAndMasterFreightCollectCharges(transactions, runCreatePSAsARFirst, false);
			}

			return result;
		}

		#endregion

		protected override void ProcessEligibleCharges(IReceivablesPostingChargeCollection filteredCharges)
		{
			if (filteredCharges.Any()
				&& AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting
				&& !BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, filteredCharges.Select(x => x.Branch).ToHashSet()))
			{
				fCancelPosting = true;
				filteredCharges.Cast<BaseCharge>().First().AddRowError(Res.GetString("bcf3ff17-e550-4848-a1cd-2c2b758e90a9", @"Please review the charges being posted.
You may need to post some charges through the Shipment.
Posting Overseas Agent charges is being prevented because you are attempting to post a transaction containing charges for a mix of incompatible branches.
Your transaction is not permitted because there are branches from different Branch Posting Groups."));
				RaiseOnCriticalPostError(filteredCharges.Cast<BaseCharge>().First());

				throw new InterruptPostingException();
			}

			if (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value
				&& !filteredCharges.Where(x => ConsolDistributor.IsCombinedShipmentCharges(x)).AllSame(x => x.SellTaxBranch))
			{
				fCancelPosting = true;
				filteredCharges.Cast<BaseCharge>().First().AddRowError(Res.GetString("B2B95C23-0185-4B87-8438-CC846AD0784E", @"Please review the charges being posted.
You may need to post some charges through the Shipment.
Posting charges is being prevented because you are attempting to post a transaction containing charges for different tax branches.
Your transaction is not permitted because there are different tax branches."));
				RaiseOnCriticalPostError(filteredCharges.Cast<BaseCharge>().First());

				throw new InterruptPostingException();
			}

			base.ProcessEligibleCharges(filteredCharges);
		}
		#region Distribute Charges

		protected override PostingChargeCollection DistributeCharges(IReceivablesPostingChargeCollection filteredCharges)
		{
			PostingChargeCollection result = base.DistributeCharges(filteredCharges);
			// check if export, check if currencies match for agents, if not, determine currency to post in
			//OrgHeader AgentToInvoice = Consol.IsExportConsol ? Consol.ReceivingAgentAPInvoicingParty : Consol.SendingAgentAPInvoicingParty;
			if (AgentToInvoice != null)
			{
				AgentPostingOptionSelector postingSelector = null;

				foreach (IReceivablesPostingChargeCollection chargeCollection in result)
				{
					if (ConsolDistributor.IsOverseasAgentCharge(chargeCollection.Key?.Org))
					{
						if (postingSelector != null ||
								RaiseExportAgentPosting(postingSelector = new AgentPostingOptionSelector(Factory, Consol, chargeCollection)))
						{
							if (ConsolDistributor.IsFirstChargeOverseasAgentCharge(chargeCollection) && chargeCollection.DebtorBizO.MiscServ.OM_FWBillCollectFeesOnSingleInvoice)
							{
								var postingStyle = InvoiceTypesList.Codes.FinalInvoice;
								if (postingSelector.Currency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
								{
									postingStyle = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
								}
								chargeCollection.Key.InvoiceType = postingStyle;
							}

							chargeCollection.PostingCurrency = postingSelector.Currency;
							chargeCollection.PostingCurrencyExchangeRate = postingSelector.ExchangeRate;
							chargeCollection.JobInvoicingPlugIn = Consol as IJobInvoicingPlugIn;
						}
						else
						{
							fCancelPosting = true;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Create Entire Consol Transactions

		protected override bool CreateAllTransactions(TransactionCreatorHashtable transactions)
		{
			bool result = false;

			bool runCreatePSAsARFirst = AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value;
			if (runCreatePSAsARFirst)
			{
				result |= PostProfitShareAndMasterFreightCollectCharges(transactions, runCreatePSAsARFirst, false);
			}
			else
			{
				result |= PostReceivablesCharges(JobInvoicingPostingOption.All, transactions);
			}

			var costTransactionCreator = new ConsolAPInvoiceCreator(fallbackFactory, Jobs, HasJobOnHold, Consol, ConsolCosts, apInvoicePostGUIProvider: apInvoicePostGUIProvider);
			result |= costTransactionCreator.CreateTransactions(transactions);
			result |= APPaymentCreator.CreateTransactions(transactions);
			PaymentApprovalMatcher.Match(transactions);
			result |= APCreditNoteCreator.CreateTransactions(transactions);

			if (runCreatePSAsARFirst)
			{
				result |= PostReceivablesCharges(JobInvoicingPostingOption.All, transactions);
				if (havePostponedConsolCosts)
				{
					result |= PostProfitShareAndMasterFreightCollectCharges(transactions, runCreatePSAsARFirst, true);
				}
			}
			else
			{
				result |= PostProfitShareAndMasterFreightCollectCharges(transactions, runCreatePSAsARFirst, false);
			}

			return result;
		}

		#endregion

		#region Transaction Creators

		#region AP Payment Creator

		ConsolAPPaymentApprovalCreator APPaymentCreator
		{
			get
			{
				if (fAPPaymentCreator == null)
				{
					fAPPaymentCreator = new ConsolAPPaymentApprovalCreator(fallbackFactory, Jobs, Consol, HasJobOnHold);
				}
				return fAPPaymentCreator;
			}
		}

		ConsolAPPaymentApprovalCreator fAPPaymentCreator;

		#endregion

		#region Payment Matcher

		ConsolInvoicingPaymentApprovalMatcher PaymentApprovalMatcher
		{
			get
			{
				if (fPaymentApprovalMatcher == null)
				{
					fPaymentApprovalMatcher = new ConsolInvoicingPaymentApprovalMatcher(Factory, Jobs, PostingTime);
				}
				return fPaymentApprovalMatcher;
			}
		}

		ConsolInvoicingPaymentApprovalMatcher fPaymentApprovalMatcher;

		#endregion

		#region Credit Note Creator

		APCreditNoteCreator APCreditNoteCreator
		{
			get
			{
				if (fAPCreditNoteCreator == null)
				{
					fAPCreditNoteCreator = new APCreditNoteCreator(Jobs, fallbackFactory);
				}
				return fAPCreditNoteCreator;
			}
		}

		APCreditNoteCreator fAPCreditNoteCreator;

		#endregion

		#region Create ProfitShares

		internal ProfitShareDetailCollection CreatedProfitShares
		{
			get
			{
				if (fCreatedProfitShares == null)
				{
					fCreatedProfitShares = new ProfitShareCalculator(Factory, Consol, Consol.CostSupporter.ShipmentsList).CreateProfitShares();
				}
				return fCreatedProfitShares;
			}
		}

		ProfitShareDetailCollection fCreatedProfitShares;

		#endregion

		#region Collect Export Master Freight and Profit Share Credits

		JobConsolCost GetFreightCostToIncludeOnAgentInvoice(OrgHeader creditor)
		{
			JobConsolCost result = null;
			foreach (JobConsolCost cost in ConsolCosts)
			{
				if (cost.E6_AC_ChargeCode == Env.Registry.FreightChargeCode &&
				cost.E6_Calc_IncludeOnAgentInvoice &&
				cost.E6_OH_Creditor == creditor.PK &&
				!cost.IsPosted)
				{
					result = cost;
					break;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool PostProfitShareAndMasterFreightCollectCharges(TransactionCreatorHashtable transactions, bool runCreatePSAsARFirst, bool isSecondRun = false)
		{
			bool result = false;

			List<InvoicingBase> agentInvoices = new List<InvoicingBase>();
			var firstJob = Jobs.First();

			foreach (JobConsolCost cost in ConsolCosts)
			{
				if (isSecondRun)
				{
					if (!postponedConsolCostPKs.Contains(cost.PK))
					{
						continue; // skip the non-postponed ones as they are already processed in the first run.
					}
				}

				if (cost.Creditor != null &&
						!cost.E6_InvoiceNum.IsEmpty &&
						cost.E6_InvoiceDate.IsValid &&
						cost.E6_PaymentDate.IsValid &&
						AgentToInvoice != null &&
						AgentToInvoice.PK == cost.Creditor.PK)
				{
					InvoicingBase agentInvoice = null;
					if (cost.E6_Calc_IncludeOnAgentInvoice && !cost.IsPosted)
					{
						foreach (InvoicingBase invoice in Poster.PostedInvoices)
						{
							if (invoice.AH_OH == cost.E6_OH_Creditor && invoice.AH_ConsolidatedInvoiceRef.StartsWith("C", StringComparison.Ordinal))
							{
								agentInvoice = invoice;
								break;
							}
						}

						if (agentInvoice == null)
						{
							if (!isSecondRun && runCreatePSAsARFirst)
							{
								// In the first run, if "Profit share as AR" is true, postpone the "Include on Collect Invoice" cost processing until the second run,
								// because we might find a suitable agent invoice after PostReceivablesCharges() is called, so no need to create a dummy AR INV/CRD now.
								postponedConsolCostPKs.Add(cost.PK);
								havePostponedConsolCosts = true;
							}
							else
							{
								JobConsolCost freightCost = GetFreightCostToIncludeOnAgentInvoice(cost.Creditor);
								PostingChargeKey key = new PostingChargeKey(cost.Creditor.PK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, cost.E6_Calc_PostingGroupId, placeOfSupply: cost.E6_PlaceOfSupply, taxBranch: cost.E6_GB_CostTaxBranch);
								AgentInvoiceCreationPostingChargeCollection agentInvoiceCreation = new AgentInvoiceCreationPostingChargeCollection();
								agentInvoiceCreation.Key = key;
								agentInvoiceCreation.SetDebtor(cost.Creditor.PK);
								agentInvoiceCreation.SetInvoicePostingBranch(firstJob.JH_GB);
								agentInvoiceCreation.SetInvoicePostingDepartment(firstJob.JH_GE);
								agentInvoiceCreation.SetIsBillInLocalCurrency(false);
								agentInvoiceCreation.SetJobNumber(Consol.JK_UniqueConsignRef);
								agentInvoiceCreation.SetJobPK(firstJob.PK);
								agentInvoiceCreation.SetPostingJob(firstJob);
								if (freightCost != null)
								{
									agentInvoiceCreation.PostingCurrency = freightCost.Currency.RX_Code;
									agentInvoiceCreation.PostingCurrencyExchangeRate = freightCost.E6_ExchangeRate;
								}
								else
								{
									agentInvoiceCreation.PostingCurrency = cost.Currency.RX_Code;
									agentInvoiceCreation.PostingCurrencyExchangeRate = cost.E6_ExchangeRate;
								}
								agentInvoiceCreation.SetIsPositiveCost(cost.InvoiceLocalTotal > 0);
								agentInvoice = Poster.Post(agentInvoiceCreation);
							}
						}
					}
					if (agentInvoice != null)
					{
						agentInvoices.Add(agentInvoice);
					}
				}
			}

			foreach (ProfitShareDetail profitShare in CreatedProfitShares)
			{
				for (int index = profitShare.ProfitShareShipmentDetails.Count - 1; index >= 0; index--)
				{
					ProfitShareShipmentDetail detail = profitShare.ProfitShareShipmentDetails[index];
					Job[] jobs = Jobs.Where(x => x.JH_ParentID == detail.Parent.PK).ToArray();
					if (jobs.Length > 0 && jobs[0].JH_IsProfitSharePosted)
					{
						profitShare.ProfitShareShipmentDetails.Remove(detail);
					}
				}
			}

			RefCurrency currency = agentInvoices.Count > 0 ? agentInvoices[0].TransactionCurrency : GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal exchangeRate = agentInvoices.Count > 0 ? agentInvoices[0].AH_ExchangeRate : (ZDecimal)1;

			if (currency.RX_Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				exchangeRate = 1m;
			}

			bool profitShareChargeHasError = false;
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(Consol, CreatedProfitShares, currency, exchangeRate, Poster, ConsolCosts);
			if (IsProfitShareToApportion(CreatedProfitShares))
			{
				bool @continue = true;

				if (AgentToInvoice != null)
				{
					if (!AgentToInvoice.OH_IsDebtor || !AgentToInvoice.OH_IsCreditor ||
							(GlbCompany.CurrentCompany.GC_IsGSTRegistered && AgentToInvoice.CompanyData.IsAPTaxApplicable != AgentToInvoice.CompanyData.IsARTaxApplicable))
					{
						fCancelPosting = true;
						@continue = false;

						string errorMessage = Res.GetString("E839765A-6021-4161-B46E-858061926EF5", "The Agent organization {0} used for posting Profit Share has an invalid configuration.\r\nIt should be both Receivables and Payables. Make sure the {1} is Applicable flag is either checked or unchecked for both roles.\r\nPlease fix organization data and try again",
								AgentToInvoice.OH_Code, Country.GetConsumptionTaxDescription(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

						TransactionHeader[] invoices = transactions.GetAllARTransactions();
						if (invoices.Length == 0)
						{
							invoices = transactions.GetAllAPTransactions();
						}

						if (invoices.Length == 0)
						{
							invoices = new TransactionHeader[] { Factory.New<ARInvoice>() };
						}

						invoices[0].AddRowError(errorMessage);
						RaiseOnCriticalPostError(invoices[0]);
					}
				}

				@continue = @continue && RaiseProfitShareConfirmation(CreatedProfitShares);
				if (@continue)
				{
					if (AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value == Guid.Empty)
					{
						RaiseIncorrectRegistrySetup(((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.ProfitShareChargeCode).Location);
					}
					else
					{
						new ProfitShareConsolChargeCreator(postingDetails, Factory).CreateCharges();

						var chargesWithError = new List<BaseCharge>();

						foreach (JobConsolCost consolCost in postingDetails.AllConsolCosts)
						{
							foreach (ApportionSplitCharge apportionmentCharge in consolCost.ApportionmentCharges)
							{
								if (apportionmentCharge.ChargeCode != null && apportionmentCharge.ChargeCode.PK == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value)
								{
									using (new DisposableAction(() => apportionmentCharge.IgnoreValidationSuspended = true, () => apportionmentCharge.IgnoreValidationSuspended = false))
									{
										apportionmentCharge.Validation.ValidateAll();
									}

									if (apportionmentCharge.HasErrors)
									{
										chargesWithError.Add(apportionmentCharge);
									}
								}
							}
						}

						if (chargesWithError.Any())
						{
							result = false;
							fCancelPosting = true;
							profitShareChargeHasError = true;
						}
						RaiseOnCriticalPostError(chargesWithError);
					}
				}
			}

			if (!profitShareChargeHasError)
			{
				if (isSecondRun)  // For second run, no need to postpone any costs since it's the final run.
				{
					postponedConsolCostPKs = null;
				}
				result |= new MasterFreightConsolAPInvoiceCreator(postingDetails, fallbackFactory, Jobs, HasJobOnHold, Consol, ConsolCosts, postponedConsolCostPKs).CreateTransactions(transactions);

				result |= new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails).NegateInvoices(transactions);
			}

			return result;
		}

		public ZDecimal GetCostsTotalLinkedToCharges(JobCharge jobCharge)
		{
			var costsLinkedToCharge = ZDecimal.Zero;
			foreach (JobConsolCost consolCost in ConsolCosts)
			{
				if (consolCost.Consol != null)
				{
					if (consolCost.Creditor != null &&
							!consolCost.E6_InvoiceNum.IsEmpty &&
							consolCost.E6_InvoiceDate.IsValid &&
							consolCost.E6_PaymentDate.IsValid &&
							AgentToInvoice != null &&
							AgentToInvoice.PK == consolCost.Creditor.PK &&
							consolCost.E6_Calc_IncludeOnAgentInvoice &&
							!consolCost.IsPosted &&
							jobCharge.JR_OH_SellAccount == consolCost.E6_OH_Creditor)
					{
						costsLinkedToCharge += consolCost.E6_LocalCostAmount;
					}
				}
			}
			return costsLinkedToCharge;
		}

		public ZDecimal GetProfitShareChargesTotal()
		{
			var profitShareChargesTotal = ZDecimal.Zero;

			if (CreatedProfitShares != null)
			{
				foreach (ProfitShareDetail profitShareDetails in CreatedProfitShares)
				{
					if (profitShareDetails.HasProfitShare)
					{
						foreach (ProfitShareShipmentDetail profitShareShipmentDetail in profitShareDetails.ProfitShareShipmentDetails)
						{
							foreach (ProfitShareCharge profitShareCharge in profitShareShipmentDetail.ProfitShareCharges)
							{
								profitShareChargesTotal += profitShareCharge.ProfitShare ?? ZDecimal.Zero;
							}
						}
					}
				}
			}
			return profitShareChargesTotal;
		}

		bool IsProfitShareToApportion(ProfitShareDetailCollection profitShares)
		{
			bool result = false;
			foreach (ProfitShareDetail detail in profitShares)
			{
				if (detail.HasProfitShare)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		OrgHeader AgentToInvoice => Consol.AgentToInvoice();

		#endregion

		#region Events

		public event ProfitShareConfirmationEventHandler ProfitShareConfirmation;

		bool RaiseProfitShareConfirmation(ProfitShareDetailCollection calculatedProfitShares)
		{
			bool result = false;
			if (ProfitShareConfirmation != null)
			{
				result = ProfitShareConfirmation(this, new ProfitShareConfirmationEventArgs(calculatedProfitShares));
			}
			return result;
		}

		public event ExportAgentPostingEventHandler ExportAgentPosting;

		bool RaiseExportAgentPosting(AgentPostingOptionSelector selector)
		{
			bool result = true;
			if (ExportAgentPosting != null)
			{
				ExportAgentPostingEventArgs args = new ExportAgentPostingEventArgs(selector);
				ExportAgentPosting(this, args);
				result = !args.IsPostingCancelled;
			}
			return result;
		}

		public event IncorrectRegistrySetupEventHandler IncorrectRegistrySetup;

		void RaiseIncorrectRegistrySetup(ZString registyItemLocation)
		{
			if (IncorrectRegistrySetup != null)
			{
				IncorrectRegistrySetup(this, new IncorrectRegistrySetupEventArgs(registyItemLocation));
			}
		}

		#endregion

		#region Implementation

		ConsolPostingChargeDistributor ConsolDistributor
		{
			get { return (ConsolPostingChargeDistributor)Distributor; }
		}

		protected override PostingChargeDistributor GetDistributor()
		{
			return new ConsolPostingChargeDistributor(Consol);
		}

		protected override PostingChargeEligibilityDecider GetEligibilityDecider(Charge[] charges)
		{
			return new ConsolPostingChargeEligibilityDecider(Consol, charges);
		}

		#endregion

		#endregion

		#region Billing

		protected override void AddBillingEventCore(string billingCode, JobInvoicingPostingOption option)
		{
			if (billingCode == AccBillingCodes.GatewayBilling)
			{
				var eventCode = GetGSHBillingAuditEvent(option);
				AccBillingEventCollector.GetInstance(Consol.Factory).AddEvent(AccBillingCodes.GatewayBilling, Consol.PK, JobConsolSchema.Constants.Prefix, eventCode);
			}
			else
			{
				base.AddBillingEventCore(billingCode, option);
			}
		}

		protected override bool ShouldAddBillingEvent(string billingCode)
		{
			switch (billingCode)
			{
				case AccBillingCodes.GatewayBilling:
					return Consol is CommonConsol commonConsol &&
						commonConsol.IsGatewayConsol &&
						Consol is IBillingPlugin billingPlugin &&
						billingPlugin.IsGatewayBillingEnabled() &&
						base.ShouldAddBillingEvent(billingCode);
				default:
					return base.ShouldAddBillingEvent(billingCode);
			}
		}

		#endregion
	}

	#region Event Definitions

	public class ExportAgentPostingEventArgs : EventArgs
	{
		public ExportAgentPostingEventArgs(AgentPostingOptionSelector optionSelector)
		{
			this.OptionSelector = optionSelector;
		}

		public readonly AgentPostingOptionSelector OptionSelector;
		public bool IsPostingCancelled;
	}

	public delegate void ExportAgentPostingEventHandler(object sender, ExportAgentPostingEventArgs e);

	public class ProfitShareConfirmationEventArgs : EventArgs
	{
		public ProfitShareConfirmationEventArgs(ProfitShareDetailCollection calculatedProfitShares)
		{
			this.CalculatedProfitShares = calculatedProfitShares;
		}

		public readonly ProfitShareDetailCollection CalculatedProfitShares;
	}

	public delegate bool ProfitShareConfirmationEventHandler(object sender, ProfitShareConfirmationEventArgs e);

	public class IncorrectRegistrySetupEventArgs : EventArgs
	{
		public IncorrectRegistrySetupEventArgs(ZString registyItemLocation)
		{
			this.RegistyItemLocation = registyItemLocation;
		}

		public readonly ZString RegistyItemLocation;
	}

	public delegate void IncorrectRegistrySetupEventHandler(object sender, IncorrectRegistrySetupEventArgs e);

	#endregion
}
