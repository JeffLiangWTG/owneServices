using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class APInvoiceCreator : BaseTransactionCreator
	{
		public APInvoiceCreator(Job job, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null)
			: this(job, null, apInvoicePostGUIProvider)
		{
		}

		public APInvoiceCreator(Job job, Predicate<Charge> isEligibleForPosting, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null)
			: this(job, null, false, null, apInvoicePostGUIProvider: apInvoicePostGUIProvider)
		{
			this.isEligibleForPosting = isEligibleForPosting;
		}

		public APInvoiceCreator(Job job, IJobCostingPlugIn consol, bool consolHasJobOnHold, JobConsolCostCollection consolCosts, bool createConsolCostOnly = false, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null)
			: base(job, consol, consolHasJobOnHold)
		{
			this.ConsolCosts = consolCosts;
			this.CreateConsolCostOnly = createConsolCostOnly;
			this.apInvoicePostGUIProvider = apInvoicePostGUIProvider;
		}

		readonly JobConsolCostCollection ConsolCosts;
		readonly Predicate<Charge> isEligibleForPosting;
		readonly bool CreateConsolCostOnly;
		readonly IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider;

		protected override bool IsChargeApplicable(Charge charge) =>
			base.IsChargeApplicable(charge) &&
				charge.HasValidDataForCostPosting &&
				ShouldPostAgentRelatedCharge(charge) &&
				(isEligibleForPosting == null || isEligibleForPosting(charge));

		protected virtual bool ShouldPostAgentRelatedCharge(Charge charge) => !charge.IsAgentRelatedCharge;

		public bool IsInvoiceCreatedAndWontBeRepalcedByCreditNote { get; private set; } //APCreditNoteCreator will later delete negative invoices and create Credit Notes instead. Such invoices should not be reported here as created, APCreditNoteCreator will report created Credit Notes instead.

		#region Batch Operation

		public void CreateTransactionsForMultiJobOperation()
		{
			CreateTransactionsCore(transactionsMultiJobOperation, FinalCalculationsDuringMultiJobOperationSuspender.IsSuspended);
		}

		public IDisposable InitializeMultiJobOperation(TransactionCreatorHashtable transactions)
		{
			if (FinalCalculationsDuringMultiJobOperationSuspender.IsSuspended)
			{
				throw new InvalidOperationException("InitializeMultiJobOperation can't be called while previous operation is not finished.");
			}

			transactionsMultiJobOperation = transactions;
			chargesForInvoicesMultiJobOperation = new Dictionary<APInvoiceChargesKey, APInvoiceCharges>();
			return FinalCalculationsDuringMultiJobOperationSuspender.GetSuspender();
		}

		void FinalizeMultiJobOperation()
		{
			List<APInvoice> createdInvoices = null;
			if (chargesForInvoicesMultiJobOperation != null)
			{
				createdInvoices = CreateInvoices(chargesForInvoicesMultiJobOperation.Values.ToArray(), transactionsMultiJobOperation);
			}
			if (createdInvoices != null && transactionsMultiJobOperation != null)
			{
				CreateUnapprovedInvoiceAccordingToSecurity(createdInvoices, transactionsMultiJobOperation);
			}
			transactionsMultiJobOperation = null;
			chargesForInvoicesMultiJobOperation = null;
		}

		TransactionCreatorHashtable transactionsMultiJobOperation;
		Dictionary<APInvoiceChargesKey, APInvoiceCharges> chargesForInvoicesMultiJobOperation;

		FunctionalitySuspender FinalCalculationsDuringMultiJobOperationSuspender
		{
			get { return finalCalculationsDuringMultiJobOperationSuspender ?? (finalCalculationsDuringMultiJobOperationSuspender = new FunctionalitySuspender(() => FinalizeMultiJobOperation())); }
		}
		FunctionalitySuspender finalCalculationsDuringMultiJobOperationSuspender;

		#endregion

		protected override bool CreateTransactionsCore(TransactionCreatorHashtable transactions, bool isMultiJobOperationInProgress)
		{
			var chargesForInvoices = isMultiJobOperationInProgress ? chargesForInvoicesMultiJobOperation : new Dictionary<APInvoiceChargesKey, APInvoiceCharges>();

			var postInvoiceByRequest = false;
			var creditorOnlyAccepted = ZString.Empty;
			var invoiceNumberOnlyAccepted = ZString.Empty;
			if (apInvoicePostGUIProvider != null && apInvoicePostGUIProvider.RequestToCompare != null)
			{
				creditorOnlyAccepted = apInvoicePostGUIProvider.RequestToCompare.PostingDetails.Creditor;
				invoiceNumberOnlyAccepted = apInvoicePostGUIProvider.RequestToCompare.PostingDetails.TransactionNumber;
				postInvoiceByRequest = true;
			}

			foreach (Charge charge in Charges)
			{
				if (!CreateConsolCostOnly || charge.JR_IsApportioned)
				{
					var key = new APInvoiceChargesKey(charge);

					if (postInvoiceByRequest && (key.Creditor != creditorOnlyAccepted || key.InvoiceNumber != invoiceNumberOnlyAccepted))
					{
						continue;
					}

					APInvoiceCharges invoiceCharges;
					if (!chargesForInvoices.TryGetValue(key, out invoiceCharges))
					{
						invoiceCharges = new APInvoiceCharges(key.Creditor, key.InvoiceNumber, key.JobPK, key.JobParentTableCode, apInvoicePostGUIProvider, key.PlaceOfSupply);
						invoiceCharges.PostDate = PostingTime;
						chargesForInvoices.Add(key, invoiceCharges);
					}

					invoiceCharges.Charges.Add(charge);
				}
			}

			if (!isMultiJobOperationInProgress)
			{
				var createdInvoices = CreateInvoices(chargesForInvoices.Values.ToArray(), transactions);
				CreateUnapprovedInvoiceAccordingToSecurity(createdInvoices, transactions);
			}

			return IsInvoiceCreatedAndWontBeRepalcedByCreditNote;
		}

		static string GetInvoiceNumber(Charge charge, string placeOfSupply)
		{
			return charge.JR_IsApportioned
				? charge.JR_APInvoiceNum
				: (charge.JR_APInvoiceNum.IsEmpty
					? new ZString("###" + charge.Job.JH_JobNum + (!string.IsNullOrEmpty(placeOfSupply) ? "/" + placeOfSupply : string.Empty))
					: charge.JR_APInvoiceNum);
		}

		List<APInvoice> CreateInvoices(APInvoiceCharges[] chargesForInvoices, TransactionCreatorHashtable transactions)
		{
			var apInvoiceBackDatingHelper = new BackDatingAndExRateOptionHelper(IsConsol ? Consol as IJobInvoicingPlugIn : Job.Parent as IJobInvoicingPlugIn, chargesForInvoices);
			apInvoiceBackDatingHelper.PerformTransactionBackDating();
			apInvoiceBackDatingHelper.ApplyExRateOption();

			var chargesForInvoicesToProcessFurther = chargesForInvoices;
			if (apInvoicePostGUIProvider != null && AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
			{
				var processOnlyInvoicesForSelectedRequest = apInvoicePostGUIProvider.RequestToCompare != null;
				var chargesForInvoicesToApprove = new List<APInvoiceCharges>();
				var chargesForInvoicesToNotApprove = new List<APInvoiceCharges>();
				foreach (var chargesForInvoice in chargesForInvoices)
				{
					if (IsItInvoice(chargesForInvoice))
					{
						chargesForInvoicesToApprove.Add(chargesForInvoice);
					}
					else if (!processOnlyInvoicesForSelectedRequest)
					{
						chargesForInvoicesToNotApprove.Add(chargesForInvoice);
					}
				}
				var chargesForInvoicesAfterApproval = AuthorizeAPInvoiceChargesForPostingAndAddNotPostedHere(chargesForInvoicesToApprove.ToArray());
				chargesForInvoicesToProcessFurther = chargesForInvoicesAfterApproval.Concat(chargesForInvoicesToNotApprove).ToArray();
			}

			IsInvoiceCreatedAndWontBeRepalcedByCreditNote = chargesForInvoicesToProcessFurther.Any(x => IsItInvoice(x));

			var createdInvoices = new List<APInvoice>();
			var unpostedCharges = new List<APInvoiceCharges>(chargesForInvoices);
			foreach (var chargesForInvoice in chargesForInvoicesToProcessFurther)
			{
				if (chargesForInvoice.IsExcludedFromPosting)
				{
					continue;
				}

				Factory.ChildFactories.Add(chargesForInvoice.FactoryForAPInvoiceApprovalRequests);
				transactions.AddAPInvoiceApprovalRequest(chargesForInvoice.ApprovingRequest);

				if (!chargesForInvoice.IsInvoiceAllowedToBeCreated)
				{
					continue;
				}

				var invoice = Factory.New<APInvoice>();
				PopulateInvoiceValues(chargesForInvoice, invoice);
				transactions.AddAPInvoice(invoice, chargesForInvoice.Creditor, chargesForInvoice.InvoiceNumber);
				createdInvoices.Add(invoice);
				unpostedCharges.Remove(chargesForInvoice);

				AdjustLocalRoundedValuesForImportedConsolCosts(invoice, chargesForInvoice);
			}

			apInvoiceBackDatingHelper.RestoreUnpostedChargesExRate(unpostedCharges.SelectMany(x => x.Charges).ToArray());
			return createdInvoices;
		}

		void AdjustLocalRoundedValuesForImportedConsolCosts(APInvoice invoice, APInvoiceCharges chargesForInvoice)
		{
			if (ConsolCosts == null)
			{
				return;
			}

			var consolCostApportionmentCharges = ConsolCosts
				.Cast<JobConsolCost>()
				.SelectMany(consol => consol.ApportionmentCharges.Cast<ApportionSplitCharge>())
				.ToDictionary(x => x.PK, x => x);

			var mapperLineToApportionmentCharge = chargesForInvoice.Charges
				.Where(charge => !charge.JR_AL_APLine.IsEmpty && consolCostApportionmentCharges.ContainsKey(charge.PK))
				.GroupBy(charge => charge.JR_AL_APLine)
				.ToDictionary(
					x => x.Key,
					x => consolCostApportionmentCharges[x.First().PK]
				);

			invoice.AdjustLocalRoundedValuesForImportedConsolCosts(ConsolCosts, mapperLineToApportionmentCharge);
		}

		static bool IsItInvoice(ITransactionForApproval transaction) => transaction.AH_LocalTotalAmount >= 0;

		APInvoiceCharges[] AuthorizeAPInvoiceChargesForPostingAndAddNotPostedHere(APInvoiceCharges[] chargesForInvoices)
		{
			var chargesForInvoicesToProcessFurther = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(chargesForInvoices);

			var isUserConfirmationRequired = chargesForInvoicesToProcessFurther.Any(x => x.IsUserConfirmationRequired)
				&& chargesForInvoicesToProcessFurther.Any(x => x.ContinueProcessing);
			if (isUserConfirmationRequired && !apInvoicePostGUIProvider.IsForPreviewOnly)
			{
				var userAnswer = apInvoicePostGUIProvider.ShowPostingConfirmationForm(chargesForInvoicesToProcessFurther);
				if (userAnswer != ZDialogResult.OK)
				{
					throw new InterruptPostingException();
				}
			}

			return chargesForInvoicesToProcessFurther;
		}

		void CreateUnapprovedInvoiceAccordingToSecurity(List<APInvoice> createdInvoices, TransactionCreatorHashtable transactions)
		{
			if (apInvoicePostGUIProvider != null && !AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
			{
				foreach (APInvoice invoice in createdInvoices)
				{
					var validator = new UnapprovedTransactionValidationHelper();
					var requiredCheckpoint = validator.RequiredSecurityCheckPoint(invoice);
					if (!requiredCheckpoint.IsAllowed)
					{
						var converter = new UnapprovedTransactionConverter(Factory);
						string clientCode = invoice.Header.OH_Code;
						string apInvoiceNumber = invoice.AH_TransactionNum;
						transactions.RemoveAPInvoice(clientCode, apInvoiceNumber);
						transactions.AddAPInvoice((APInvoice)converter.ConvertToUA(invoice), clientCode, apInvoiceNumber);
					}
				}
			}
		}

		void PopulateInvoiceValues(APInvoiceCharges chargesForInvoice, APInvoice invoice)
		{
			if (invoice.AH_PostDate.Date != chargesForInvoice.PostDate.Date)
			{
				invoice.AH_PostDate = chargesForInvoice.PostDate;
			}

			SetInvoiceHeadeValues(invoice, chargesForInvoice);

			foreach (var charge in chargesForInvoice.Charges)
			{
				if (charge.JR_E6.IsValid)
				{
					var cost = (JobConsolCost)ConsolCosts.FindByPK(charge.JR_E6);
					if (cost != null)
					{
						cost.E6_AH_APInvoice = invoice.PK;
					}
				}

				SetJobAdditionalReferences(charge, invoice, chargesForInvoice.PostDate);
			}
			if (AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
			{
				invoice.ApprovingRequestForJobPosting = chargesForInvoice.PostedRequest;
				invoice.ApprovingUserPK = chargesForInvoice.ApprovingUserPK;
			}
		}

		internal static void SetInvoiceHeadeValues(InvoicingBase invoice, APInvoiceCharges chargesForInvoice)
		{
			if (chargesForInvoice.Charges.Count == 0)
			{
				return;
			}

			var charge = chargesForInvoice.Charges[0];
			var job = charge.InvoicingJob;

			invoice.AH_JH = job.PK;

			invoice.AH_GB = AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.Value ? GlbBranch.CurrentBranch.PK : job.JH_GB;
			ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, chargesForInvoice);

			invoice.AH_GE = job.JH_GE;
			invoice.AH_Desc = (AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(
								AccountingConstants.VoucherItemRegistryCode.JobAPInvoice, "") + " " + job.JH_JobNum).Trim();

			invoice.AH_OH = charge.JR_OH_CostAccount;
			if (invoice.IsSelfBillingInvoice)
			{
				invoice.AH_InvoiceDate = ZDateTime.Now;
			}
			else
			{
				invoice.AH_TransactionNum = charge.JR_APInvoiceNum;
				invoice.AH_InvoiceDate = charge.JR_APInvoiceDate;
				invoice.AH_DocumentReceivedDate = charge.JR_APDocumentReceivedDate;
				invoice.AH_DueDate = charge.JR_PaymentDate;
			}

			using (invoice.SetExchangeRateSuspender.GetSuspender())
			{
				invoice.AH_RX_NKTransactionCurrency = charge.JR_RX_NKCostCurrency;
				if (charge.JR_RX_NKCostCurrency != Env.CurrentCompany.LocalCurrency.Code && !invoice.UseJobExchangeRate)
				{
					invoice.UseJobExchangeRate = true;
				}
			}
			invoice.AH_ExchangeRate = charge.JR_OSCostExRate;
			if (invoice.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
				chargesForInvoice.Charges.Any(x => x.JR_RX_NKCostCurrency != invoice.AH_RX_NKTransactionCurrency))
			{
				invoice.ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
			invoice.AH_ChequeOrReference = charge.JR_CostReference;

			if (chargesForInvoice.PostedRequest != null && !chargesForInvoice.PostedRequest.RequisitionStatus.IsEmpty && !chargesForInvoice.PostedRequest.RequisitionDate.IsEmpty)
			{
				invoice.AH_RequisitionStatus = chargesForInvoice.PostedRequest.RequisitionStatus;
				invoice.AH_RequisitionDate = chargesForInvoice.PostedRequest.RequisitionDate;
			}

			if (!chargesForInvoice.PlaceOfSupply.IsDefault)
			{
				invoice.AH_PlaceOfSupply = chargesForInvoice.PlaceOfSupply;
			}

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				invoice.AH_GB_TaxBranch = charge.JR_GB_CostTaxBranch;
			}
		}

		protected virtual void SetJobAdditionalReferences(Charge charge, APInvoice invoice, ZDateTime postingTime)
		{
			charge.CreateCostTransactionLine(invoice, postingTime);
		}

		struct APInvoiceChargesKey
		{
			public APInvoiceChargesKey(Charge charge)
			{
				Argument.NotNull(charge, nameof(charge));

				Creditor = charge.CostAccount?.OH_Code;
				JobPK = charge.JR_IsApportioned ? ZGuid.Empty : charge.Job.PK;
				JobParentTableCode = JobPK.IsEmpty ? string.Empty : charge.Job.TablePrefix;
				PlaceOfSupply = PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(charge.Company ?? GlbCompany.CurrentCompany)
					&& AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.Value
					? charge.JR_CostPlaceOfSupply.ToString() : null;
				InvoiceNumber = GetInvoiceNumber(charge, PlaceOfSupply);
			}

			public readonly string Creditor;
			public readonly string InvoiceNumber;
			public readonly ZGuid JobPK;
			public readonly string JobParentTableCode;
			public readonly string PlaceOfSupply;
		}

		class BackDatingAndExRateOptionHelper
		{
			readonly IJobInvoicingPlugIn JobInvoicingPlugIn;
			readonly APInvoiceCharges[] APInvoiceCharges;
			readonly Dictionary<ZGuid, Tuple<ZDecimal, ZDecimal, ZDecimal?, ZDecimal>> ChargesExRateUpdated;

			public BackDatingAndExRateOptionHelper(IJobInvoicingPlugIn jobInvoicingPlugIn, APInvoiceCharges[] apInvoiceCharges)
			{
				JobInvoicingPlugIn = jobInvoicingPlugIn;
				APInvoiceCharges = apInvoiceCharges;
				ChargesExRateUpdated = new Dictionary<ZGuid, Tuple<ZDecimal, ZDecimal, ZDecimal?, ZDecimal>>();
			}

			public void PerformTransactionBackDating()
			{
				var postDateConfigurationHelper = new PostDateConfigurationHelper(JobInvoicingPlugIn);
				var postDateConfiguration = postDateConfigurationHelper.FindPostDateConfiguration();
				if (postDateConfiguration != null)
				{
					foreach (var apInvoiceCharge in APInvoiceCharges)
					{
						var postDate = postDateConfigurationHelper.GetPostDate(apInvoiceCharge.PostDate, apInvoiceCharge.Charges[0].JR_APInvoiceDate);
						if (!postDate.IsEmpty && apInvoiceCharge.PostDate.Date != postDate.Date)
						{
							apInvoiceCharge.PostDate = postDate;
						}
					}
				}
			}

			public void ApplyExRateOption()
			{
				var invoices = APInvoiceCharges.OrderBy(x => x.PostDate).ToArray();

				foreach (var apInvoice in invoices)
				{
					var chargesToBeUpdated = apInvoice.Charges.Cast<Charge>()
							.Where(x => ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, x.JR_RX_NKCostCurrency == x.Company.GC_RX_NKLocalCurrency, x.JR_GC)
							&& x.JR_RX_NKCostCurrency != x.Company.GC_RX_NKLocalCurrency).ToArray();
					if (chargesToBeUpdated.Any())
					{
						AddToChargesExRateUpdated(chargesToBeUpdated);
						ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, chargesToBeUpdated, apInvoice.PostDate);
					}
				}
			}

			public void RestoreUnpostedChargesExRate(Charge[] unpostedCharges)
			{
				if (ChargesExRateUpdated.Any() && unpostedCharges.Any())
				{
					foreach (var charge in unpostedCharges.Where(x => ChargesExRateUpdated.ContainsKey(x.PK)))
					{
						if (ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, charge.JR_CostCurrency == charge.Company.GC_RX_NKLocalCurrency, charge.JR_GC))
						{
							var originalValues = ChargesExRateUpdated[charge.PK];
							var originalOSCostExRate = originalValues.Item1;
							var originalOSSellExRate = originalValues.Item2;
							var originalSellExRateBaseRate = originalValues.Item3;
							var originalParentConsolCostExRate = originalValues.Item4;

							if (charge.JR_OSCostExRate != originalOSCostExRate)
							{
								if (charge.CostExchangeRate != null)
								{
									charge.CostExchangeRate.SetBaseRate(originalOSCostExRate);
								}
								charge.JR_OSCostExRate = originalOSCostExRate;
							}

							if (charge.JR_OSSellExRate != originalOSSellExRate)
							{
								if (charge.RevenueExchangeRate != null && originalSellExRateBaseRate != null)
								{
									charge.RevenueExchangeRate.SetBaseRate(originalSellExRateBaseRate.Value);
								}
								charge.JR_OSSellExRate = originalOSSellExRate;
							}

							var consolCost = charge.ParentConsolCost;
							if (consolCost != null && consolCost.E6_ExchangeRate != originalParentConsolCostExRate)
							{
								using (new DisposableAction(() => consolCost.SetContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting), () => consolCost.RemoveContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting)))
								{
									consolCost.E6_ExchangeRate = originalParentConsolCostExRate;
								}
							}
						}
					}
				}
			}

			void AddToChargesExRateUpdated(Charge[] charges)
			{
				foreach (var charge in charges)
				{
					var originalValues = Tuple.Create(charge.JR_OSCostExRate, charge.JR_OSSellExRate, charge.RevenueExchangeRate?.Rate,
						charge.ParentConsolCost != null ? charge.ParentConsolCost.E6_ExchangeRate : ZDecimal.Zero);
					ChargesExRateUpdated.Add(charge.PK, originalValues);
				}
			}
		}
	}
}
