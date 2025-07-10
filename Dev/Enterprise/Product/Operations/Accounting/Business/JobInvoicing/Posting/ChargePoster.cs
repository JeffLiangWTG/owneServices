using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	/// <summary>
	/// Provides an object that posts charges to an invoice.
	/// Only supports the posting of IReceivablesPostingCharges - ie AR charges.
	/// </summary>
	public class ChargePoster : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ChargePoster(BusinessObjectFactory factory)
				: base(factory)
		{
		}

		public ChargePoster(BusinessObjectFactory factory, BasePostManager postManager)
				: base(factory)
		{
			this.PostManager = postManager;
		}

		readonly BasePostManager PostManager;

		/// <summary>
		/// Post the given receivables charges.
		/// </summary>
		/// <param name="charges">Collection of receivables charges.</param>
		/// <returns>The Invoice or Credit Note containing the posted charges.</returns>
		public InvoicingBase Post(IReceivablesPostingChargeCollection charges)
		{
			Factory.SetContext(BusinessContext.PostingReceivableChargesForFactoryLevel);
			using (new DisposableAction(
				() => charges.Cast<BusinessObject>().ForEach(x => x.SetContext(BusinessContext.PostingReceivableCharges)),
				() => charges.Cast<BusinessObject>().ForEach(x => x.RemoveContext(BusinessContext.PostingReceivableCharges))))
			{
				InvoicingBase invoice = GetInvoice(charges);

				ZString postingCurrency = charges.IsBillInLocalCurrency ? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency : charges.PostingCurrency;
				ZDecimal postingCurrencyExchangeRate = GetPostingExchangeRate(charges);

				var exchangeRate = Env.CurrentCompany.ExchangeRate;
				ZDecimal supposedInvoiceLocalAmount = 0;
				foreach (IReceivablesPostingCharge charge in charges)
				{
					UnsignedChargeAmounts amounts = CalculateAmountsFromCharge(charge, postingCurrency, postingCurrencyExchangeRate);
					supposedInvoiceLocalAmount += exchangeRate.ForeignToLocal(amounts.UnsignedOSExTaxAmount + amounts.UnsignedOSTaxAmount, postingCurrencyExchangeRate);
				}

				OrgHeader debtor = Factory.Load<OrgHeader>(charges.Debtor);

				if (Math.Abs(supposedInvoiceLocalAmount) < debtor.MiscServ.OM_ARTreatDisbursementsAsStandardValue)
				{
					SetInvoiceTypeOnCharges(charges);
				}

				ZDateTime postTime = ZDateTime.Now;
				SetInvoiceHeaderDetails(charges, invoice, postingCurrency, postingCurrencyExchangeRate);
				invoice.AH_TransactionCategory = charges.Key.InvoiceType;
				invoice.AH_ChequeOrReference = charges.Key.SellReference;
				using (invoice.SetExchangeRateSuspender.GetSuspender())
				{
					SetInvoiceAndDueDates(charges, invoice, ZDateTime.Empty);
				}
				var invoiceDate = GetInvoiceDateFromInvoice(invoice);
				var ledger = invoice.AH_Ledger;
				Factory.SetContext(BusinessContext.PostingReceivableChargesForTaxCalculation);
				using (invoice.Lines.SuspendListChanged())
				{
					Factory.AddFetchHint(JobChargeSchema.Instance, new ZQuery(JobChargeSchema.JR_JH, charges.Select(c => c.Job?.PK).Where(pk => pk.HasValue).Distinct()));

					for (int i = 0; i < charges.Count; i++)
					{
						var charge = charges[i];
						var line = (InvoicingLineBase)invoice.Lines.AddNew();

						using (line.GetValidationSuspender())
						{
							SetInvoiceLineDetails(line, charge, postTime, invoiceDate, ledger, postingCurrency, postingCurrencyExchangeRate, charges);
						}

						charge.SetRevenueTransactionLine(line);
					}

					if (invoice.HasZeroLocalButNonZeroOSTaxAmount)
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(invoice.PK,
							CriticalValidationInfoCollectorServiceKeyType.OSTaxAmountIsNotZeroWhileLocalTaxAmoutIsZero,
							() => { return invoice.GetTransactionHeaderTaxAmountWithLinesTaxAmountInfo(); },
							CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
					}
				}

				if (IsConsolRelated(charges.Key))
				{
					SetInvoiceHeaderDetailsForConsol(invoice);
					if (charges.Job != null)
					{
						SetConsolidatedInvoiceRefAndDepartment(charges.Job, invoice, charges.Key.JobNumber);
					}
				}

				charges.PostedInvoice = invoice;
				PostedInvoices.Add(invoice);

				new CFXTransactionPoster(charges, Factory).CreateCFXTransactions();
				return invoice;
			}
		}

		internal static bool IsConsolRelated(PostingChargeKey key)
		{
			return key.JobNumber.StartsWith("C", StringComparison.Ordinal);
		}

		protected virtual ZDate GetInvoiceDateFromInvoice(InvoicingBase invoice) => invoice?.AH_InvoiceDate.Date ?? ZDate.Today;

		static void SetInvoiceTypeOnCharges(IReceivablesPostingChargeCollection charges)
		{
			ZString newInvoiceType = ZString.Empty;

			if (charges.Key.InvoiceType == InvoiceTypesList.Codes.DisbursementInForeignCurrency)
			{
				newInvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			}
			else if (charges.Key.InvoiceType == InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching)
			{
				newInvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			}
			else if (charges.Key.InvoiceType == InvoiceTypesList.Codes.DisbursementInvoice)
			{
				newInvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			}
			else if (charges.Key.InvoiceType == InvoiceTypesList.Codes.DisbursementInvoice_Batching)
			{
				newInvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			}

			if (!newInvoiceType.IsEmpty)
			{
				charges.Key.InvoiceType = newInvoiceType;
				foreach (IReceivablesPostingCharge charge in charges)
				{
					charge.InvoiceType = newInvoiceType;
				}
			}
		}

		protected virtual ZDecimal GetPostingExchangeRate(IReceivablesPostingChargeCollection charges)
		{
			ZDecimal postingCurrencyExchangeRate = charges.IsBillInLocalCurrency ? (ZDecimal)1m : charges.PostingCurrencyExchangeRate;
			return postingCurrencyExchangeRate;
		}

		/// <summary>
		/// Posts a single charge to a new invoice.
		/// </summary>
		/// <param name="charge">Single charge to post.</param>
		/// <returns>The invoice to which this charge was posted.</returns>
		internal InvoicingBase Post(IReceivablesPostingCharge charge)
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(charge.Debtor.PK, charge.InvoiceType, charge.Job.JobNumber, charge.DebtorAddressPK, charge.DebtorContactPK, charge.TaxRatePostingGroupId, charge.Branch, taxBranch: charge.SellTaxBranch);
			charges.Add(charge);

			return Post(charges);
		}

		public ZDecimal GetSupposedInvoiceAmount(IReceivablesPostingChargeCollection charges)
		{
			ZDecimal supposedInvoiceLocalAmount = 0;

			ZString postingCurrency = charges.IsBillInLocalCurrency ? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency : charges.PostingCurrency;

			ZDecimal postingCurrencyExchangeRate = charges.IsBillInLocalCurrency ? (ZDecimal)1m : charges.PostingCurrencyExchangeRate;

			foreach (IReceivablesPostingCharge charge in charges)
			{
				UnsignedChargeAmounts amounts = CalculateAmountsFromCharge(charge, postingCurrency, postingCurrencyExchangeRate);
				supposedInvoiceLocalAmount += Env.CurrentCompany.ExchangeRate.ForeignToLocal(amounts.UnsignedOSExTaxAmount + amounts.UnsignedOSTaxAmount, postingCurrencyExchangeRate);
			}
			return supposedInvoiceLocalAmount;
		}

		public void ChangeTransactionDescriptionOnAllARInvoicesAndCFXLines(ZString defaultDescription)
		{
			foreach (InvoicingBase invoice in PostedInvoices)
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					invoice.AH_Desc = defaultDescription.Left(AutoAccTransactionHeader.Schema.AH_DescMaxLength);

					foreach (InvoicingLineBase line in invoice.Lines)
					{
						Charge charge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, line.PK));
						if (charge != null)
						{
							JCJournalLine journalLine = Factory.Load<JCJournalLine>(charge.JR_AL_CFXLine);
							if (journalLine != null)
							{
								JCJournalHeader journal = Factory.Load<JCJournalHeader>(journalLine.AL_AH);
								if (journal != null)
								{
									journal.AH_Desc = defaultDescription.Left(AutoAccTransactionHeader.Schema.AH_DescMaxLength);
									break;
								}
							}
						}
					}
				}
			}
		}

		public void ChangeTransactionDateOnAllARInvoicesAndCFXLines(ZDateTime invoiceDate, ZDateTime postDate)
		{
			foreach (InvoicingBase invoice in PostedInvoices)
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					var oldInvoiceDate = invoice.AH_InvoiceDate;
					SetInvoiceAndDueDates(null, invoice, invoiceDate);
					if (oldInvoiceDate != invoice.AH_InvoiceDate)
					{
						UpdateTaxDateAndAmountOnApplicableLines(invoice);
					}

					if (!postDate.IsEmpty)
					{
						invoice.AH_PostDate = postDate;

						ObjectFactory.Get<ITaxProcessor>().UpdatePostDate(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice));

						foreach (InvoicingLineBase line in invoice.Lines)
						{
							Charge charge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, line.PK));
							if (charge != null)
							{
								JCJournalLine journalLine = Factory.Load<JCJournalLine>(charge.JR_AL_CFXLine);
								if (journalLine != null)
								{
									JCJournalHeader journal = Factory.Load<JCJournalHeader>(journalLine.AL_AH);
									if (journal != null)
									{
										journal.AH_PostDate = invoice.AH_PostDate;
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		internal InvoicingBase[] GetInvoices(RefCurrency currency, OrgHeader org)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, currency.RX_Code);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);
			return (InvoicingBase[])PostedInvoices.Find(filter);
		}

		internal InvoicingBase[] GetInvoices(RefCurrency currency, OrgHeader org, ZString jobNumber)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, currency.RX_Code);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_JobNumber, jobNumber);
			return (InvoicingBase[])PostedInvoices.Find(filter);
		}

		internal InvoicingBase[] GetInvoices(OrgHeader org)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_OH, org.PK);
			return (InvoicingBase[])PostedInvoices.Find(filter);
		}

		internal bool ContainsInvoice(RefCurrency currency, OrgHeader org)
		{
			return GetInvoices(currency, org).Length > 0;
		}

		#region Posted Invoices

		/// <summary>
		/// List of all invoices posted by this Charge Poster.
		/// </summary>
		public InvoicingBaseCollection PostedInvoices
		{
			get
			{
				if (fPostedInvoices == null)
				{
					fPostedInvoices = new InvoicingBaseCollection(Factory);
				}
				return fPostedInvoices;
			}
		}

		InvoicingBaseCollection fPostedInvoices;

		#endregion

		HashSet<ZGuid> LinesNeedTaxDateCalculationBasedOnInvoiceDate
		{
			get
			{
				if (fLinesNeedTaxDateCalculationBasedOnInvoiceDate == null)
				{
					fLinesNeedTaxDateCalculationBasedOnInvoiceDate = new HashSet<ZGuid>();
				}
				return fLinesNeedTaxDateCalculationBasedOnInvoiceDate;
			}
		}
		HashSet<ZGuid> fLinesNeedTaxDateCalculationBasedOnInvoiceDate;

		#region Consol Agent Invoice Posting

		void SetInvoiceHeaderDetailsForConsol(InvoicingBase invoice)
		{
			invoice.AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(
							AccountingConstants.VoucherItemRegistryCode.ConsolARInvoice,
							Res.GetString("34da305f-f49f-45a4-8959-e4f32b4179a1", "Freight Consol Invoice"));
			invoice.AH_JH = ZGuid.Empty;
		}

		void SetConsolidatedInvoiceRefAndDepartment(IPostingJob job, InvoicingBase invoice, string consolNumber)
		{
			IJobCostingPlugIn consol = GetConsol(consolNumber, Factory);
			invoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, invoice.PK);
			job.DecrementUniqueJobInvoiceNumber();
			ZGuid defaultDepartment = ConsolInvoiceBranchDepartmentCalculator.GetDepartment(consol, Factory);
			invoice.AH_GE = (defaultDepartment.IsEmpty) ? GlbDepartment.CurrentDepartment.PK : defaultDepartment;

			var branch = ObjectFactory.Get<IAccountingDependencyFactory>().GetJobCostingPlugInHelpers().FindBranchFromConsolAgentsAndJobHeaders(consol, invoice.AH_GC, Factory) ?? GlbBranch.CurrentBranch;
			invoice.AH_GB = branch.PK;
			ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation);
		}

		static internal IJobCostingPlugIn GetConsol(string consolNumber, BusinessObjectFactory factory)
		{
			return GenericConsol.GenericConsol.GetIJobCostingPlugInByPrimaryCode(factory, consolNumber);
		}

		#endregion

		#region Implementation

		protected virtual void SetInvoiceHeaderDetails(IReceivablesPostingChargeCollection charges, InvoicingBase invoice, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate)
		{
			invoice.AH_JH = charges.JobPK;

			if (ShouldSetTransactionHeaderBranch)
			{
				invoice.AH_GB = charges.InvoicePostingBranch;
				ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, charges);
			}

			invoice.AH_GE = charges.InvoicePostingDepartment;
			ZString desc = (invoice is ARInvoice ?
					AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.JobARInvoice, "") :
					AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.JobARCreditNote, "")) +
			" " + charges.JobNumber;
			invoice.AH_Desc = desc.TrimStart();

			invoice.AH_OH = charges.Debtor;
			invoice.AH_OA_InvoiceAddressOverride = charges.DebtorAddress;
			invoice.AH_OC_InvoiceContactOverride = charges.DebtorContact;

			if (ShouldSetConsolidatedInvoiceRef)
			{
				invoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(invoice, charges.Job);
			}

			invoice.AH_RX_NKTransactionCurrency = postingCurrency;
			invoice.AH_ExchangeRate = postingCurrencyExchangeRate;

			if (!charges.Key.PlaceOfSupply.IsEmpty
				&& AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.Value)
			{
				invoice.AH_PlaceOfSupply = charges.Key.PlaceOfSupply;
			}

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				invoice.AH_GB_TaxBranch = charges.Key.TaxBranch;
			}
		}

		protected internal virtual bool ShouldSetTransactionHeaderBranch => true;

		protected internal virtual bool ShouldSetConsolidatedInvoiceRef
		{
			get { return true; }
		}

		protected virtual void SetInvoiceLineDetails(InvoicingLineBase line, IReceivablesPostingCharge charge, ZDateTime postTime, ZDate invoiceDate, ZString ledger, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate, IReceivablesPostingChargeCollection charges)
		{
			using (line.InvoiceBase.GetSetGSTOnLinesSuspender())
			{
				line.AL_AC = charge.ChargeCode;
			}

			ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetChargeComplianceDescriptionPostingHelper().UpdateInvoiceLineAndChargeDescriptions(line, charge);

			line.AL_UnitQty = 0;
			line.AL_UnitPrice = 0M;
			line.AL_OSUnitPrice = 0M;
			line.AL_PostPeriod = 0;
			line.AL_PostToGL = "N";
			line.AL_ReversePeriod = 0;
			line.AL_ReverseToGL = "N";
			line.AL_ReverseDate = ZDateTime.Empty;
			line.AL_PreventInvoicePrintGrouping = charge.PreventInvoicePrintGrouping;
			line.AL_JH = charge.Job != null ? charge.Job.PK : ZGuid.Empty;
			line.AL_GB = charge.Branch;
			line.AL_GE = charge.Department;
			line.AL_AG_PercentOf = ZGuid.Empty;
			line.AL_PercentageOfPeriod = 0;

			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_Sequence = charge.DisplaySequence;

			line.AL_OH = charge.Debtor.PK;
			line.AL_PlaceOfSupply = charge.SellPlaceOfSupply;
			line.AL_SupplyType = charge.SellSupplyType;
			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				using (line.TaxBranchCalculationSuspender.GetSuspender())
				{
					line.AL_GB_TaxBranch = charge.SellTaxBranch;
				}
			}
			line.AL_AT = charge.SellGSTRate;
			if (charge.SellGSTRate.IsValid && charge.SellTaxDate.IsEmpty)
			{
				var taxDate = CalculateTaxDateBasedOnRegistry(charge, line, invoiceDate, ledger);
				charge.SellTaxDate = taxDate;
			}
			line.SetTaxDateSafe(charge.SellTaxDate);

			line.AL_A9_VATClass = charge.SellTaxMessage;
			line.AL_AW = charge.SellWHTRate;

			line.AL_RX_NKTransactionCurrency = postingCurrency;
			CopyExchangeRateAndAmount(line, charge, postingCurrency, postingCurrencyExchangeRate);

			line.AL_PostDate = postTime;
			line.AL_GovtChargeCode = charge.GovtChargeCode;
		}

		ZDate CalculateTaxDateBasedOnRegistry(IReceivablesPostingCharge charge, InvoicingLineBase line, ZDate invoiceDate, ZString ledger)
		{
			var result = new Tuple<ZDate, ZString>(ZDate.Today, ZString.Empty);

			var job = charge.Job;
			if (job != null)
			{
				var plugIn = job.Consumer?.InvoicingSupporter;
				var taxDateOption = job.GetTaxDateDefaultingOptionForJob(Factory, ledger);
				if (taxDateOption != null)
				{
					result = job.GetTaxDateBasedOnRegistryDefaultingOption(plugIn, taxDateOption.TaxDateOption, invoiceDate);
					if (taxDateOption.TaxDateOption == TaxDateDefaultingOption.Code.InvoiceDate)
					{
						LinesNeedTaxDateCalculationBasedOnInvoiceDate.Add(line.PK);
					}
				}
			}
			return result.Item1;
		}

		public void CopyExchangeRateAndAmount(InvoicingLineBase line, IReceivablesPostingCharge charge, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate, bool updateOnlyTax = false)
		{
			var chargeBizo = (BusinessObject)charge;
			using (new DisposableAction(() => chargeBizo.SetContext(Charge.Contexts.PosterCopiesExchangeRateAndAmount),
				() => chargeBizo.RemoveContext(Charge.Contexts.PosterCopiesExchangeRateAndAmount)))
			{
				var amounts = CalculateAmountsFromCharge(charge, postingCurrency, postingCurrencyExchangeRate, updateOnlyTax);
				ZInt multiplier = line.InvoiceBase is ARCreditNote ? -1 : 1;

				if (!updateOnlyTax)
				{
					line.AL_ExchangeRate = postingCurrencyExchangeRate;
					line.AL_OSExTaxAmount = multiplier * amounts.UnsignedOSExTaxAmount;
					if (!charge.BillInLocalCurrency)
					{
						using (line.GetOSAmountCalculationSuspender())
						{
							line.AL_LocalExTaxAmount = multiplier * amounts.UnsignedLocalExTaxAmount;
						}
					}
					line.AL_OSWHTAmount = multiplier * amounts.UnsignedOSWHTAmount;
				}

				line.AL_OSTaxAmount = multiplier * amounts.UnsignedOSTaxAmount;
			}
		}

		UnsignedChargeAmounts CalculateAmountsFromCharge(IReceivablesPostingCharge charge, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate, bool updateOnlyTax = false)
		{
			if (charge.IsRevenuePosted)
			{
				var message = (NoResString)"Function CalculateAmountsFromCharge cannot be used if charge is revenue Posted, otherwise it is possible to return an incorrect Line Tax Amount.";

				var key = "ChargePoster.CalculateAmountsFromCharge";
				ErrorReporter.ReportOnce(key, message);
			}

			var amounts = new UnsignedChargeAmounts();
			if (charge.BillInLocalCurrency)
			{
				amounts.UnsignedOSExTaxAmount = charge.LocalSellAmount;
				var exRate = charge.InvoiceSellExchangeRate;
				if (charge.SellGSTRate.IsValid)
				{
					var unsignedLocalisedForeignAmount = charge.LocalSellAmount;
					var sellRate = Factory.Load<AccTaxRate>(charge.SellGSTRate);

					if (sellRate != null)
					{
						if (charge.SellCurrency.RX_Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							amounts.UnsignedOSTaxAmount = charge.OSSellTaxAmount;
						}
						else
						{
							amounts.UnsignedOSTaxAmount = charge.LocalSellTaxAmount;
						}
					}
				}

				amounts.UnsignedOSWHTAmount = (ZDecimal)Env.CurrentCompany.ExchangeRate.ForeignToLocal(charge.OSSellWHTAmount, exRate);
			}
			else if (charge.SellCurrency.RX_Code != postingCurrency)
			{
				amounts.UnsignedOSExTaxAmount = GetCrossRateAmountFromCharge(charge.OSSellAmount, charge.SellCurrency.RX_Code, charge.SellExchangeRate, postingCurrency, postingCurrencyExchangeRate);
				amounts.UnsignedOSTaxAmount = GetCrossRateAmountFromCharge(charge.OSSellTaxAmount, charge.SellCurrency.RX_Code, charge.SellExchangeRate, postingCurrency, postingCurrencyExchangeRate);
				amounts.UnsignedOSWHTAmount = GetCrossRateAmountFromCharge(charge.OSSellWHTAmount, charge.SellCurrency.RX_Code, charge.SellExchangeRate, postingCurrency, postingCurrencyExchangeRate);
			}
			else
			{
				if (!updateOnlyTax && charge.SellExchangeRate != postingCurrencyExchangeRate && OverrideChargeExchangeRatesOnPosting)
				{
					if (charge.SellCurrency == null || !postingCurrency.EqualsIgnoringCase(charge.SellCurrency.RX_Code))
					{
						var chargeForErrorReporting = charge as Charge;
						var errorMessage = @$"Attempting to update currency on posting however the charge currency ({charge.SellCurrency?.RX_Code ?? "NULL"}) does not match {postingCurrency}

{chargeForErrorReporting.GetJobChargeInfo()}";
						ErrorReporter.ReportOnce("ChargePoster.AttemptedToUpdateExchangeRateOfDifferentCurrency", errorMessage);
					}

					using (charge.SuspendAutoCalculations())
					{
						charge.SellExchangeRate = postingCurrencyExchangeRate;
						charge.LocalSellAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(charge.OSSellAmount, postingCurrencyExchangeRate);
					}
				}

				amounts.UnsignedOSExTaxAmount = charge.OSSellAmount;
				amounts.UnsignedOSTaxAmount = charge.OSSellTaxAmount;
				amounts.UnsignedOSWHTAmount = charge.OSSellWHTAmount;
			}

			amounts.UnsignedLocalExTaxAmount = charge.LocalSellAmount;

			return amounts;
		}

		protected internal virtual bool OverrideChargeExchangeRatesOnPosting
		{
			get { return true; }
		}

		struct UnsignedChargeAmounts
		{
			public ZDecimal UnsignedOSExTaxAmount
			{
				get { return fUnsignedOSExTaxAmount; }
				set { fUnsignedOSExTaxAmount = value; }
			}

			ZDecimal fUnsignedOSExTaxAmount;

			public ZDecimal UnsignedOSTaxAmount
			{
				get { return fUnsignedOSTaxAmount; }
				set { fUnsignedOSTaxAmount = value; }
			}

			ZDecimal fUnsignedOSTaxAmount;

			public ZDecimal UnsignedOSWHTAmount
			{
				get { return fUnsignedOSWHTAmount; }
				set { fUnsignedOSWHTAmount = value; }
			}

			ZDecimal fUnsignedOSWHTAmount;

			public ZDecimal UnsignedLocalExTaxAmount
			{
				get { return unsignedLocalExTaxAmount; }
				set { unsignedLocalExTaxAmount = value; }
			}
			ZDecimal unsignedLocalExTaxAmount;
		}

		ZDecimal GetCrossRateAmountFromCharge(ZDecimal amount, ZString chargeCurrency, ZDecimal chargeExchangeRate, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate)
		{
			ZDecimal result = 0m;
			if (postingCurrency != chargeCurrency)
			{
				ZDecimal unRoundedLocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(amount, chargeExchangeRate);
				result = Env.CurrentCompany.ExchangeRate.LocalToForeign(unRoundedLocalAmount, postingCurrencyExchangeRate, postingCurrency);
			}
			else
			{
				result = amount;
			}
			return result;
		}

		internal InvoicingBase GetInvoice(IReceivablesPostingChargeCollection charges)
		{
			InvoicingBase invoice = null;
			var costsLinkedToCharge = ZDecimal.Zero;
			var profitShareChargesTotal = ZDecimal.Zero;

			if (charges.Count > 0)
			{
				var jobCharge = (JobCharge)charges.FirstOrDefault();
				var consolInvoicingPostManager = PostManager as ConsolInvoicingPostManager;
				if (consolInvoicingPostManager != null)
				{
					costsLinkedToCharge = consolInvoicingPostManager.GetCostsTotalLinkedToCharges(jobCharge);
					if (!AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value)
					{
						profitShareChargesTotal = consolInvoicingPostManager.GetProfitShareChargesTotal();
					}
				}
				else
				{
					costsLinkedToCharge = GetCostsTotalLinkedToCharges(jobCharge);
				}
			}

			if (charges.IsPositiveCost.HasValue && !charges.IsPositiveCost.Value
				|| !charges.IsPositiveCost.HasValue && ResultingInvoiceTotalInLocalCurrency(charges) >= (costsLinkedToCharge + profitShareChargesTotal))
			{
				invoice = Factory.New<ARInvoice>();
			}
			else
			{
				invoice = Factory.New<ARCreditNote>();
			}

			return invoice;
		}

		ZDecimal GetCostsTotalLinkedToCharges(JobCharge jobCharge)
		{
			var costsLinkedToCharge = ZDecimal.Zero;
			var consolCost = (JobConsolCost)jobCharge.ParentConsolCost;
			var consol = consolCost?.Consol;
			if (consol != null)
			{
				var agentToInvoice = consol.IsLoadPortLocal() ? consol.ReceivingAgentAPInvoicingParty : consol.SendingAgentAPInvoicingParty;

				if (consolCost.Creditor != null &&
						!consolCost.E6_InvoiceNum.IsEmpty &&
						consolCost.E6_InvoiceDate.IsValid &&
						consolCost.E6_PaymentDate.IsValid &&
						agentToInvoice != null &&
						agentToInvoice.PK == consolCost.Creditor.PK &&
						consolCost.E6_Calc_IncludeOnAgentInvoice &&
						!consolCost.IsPosted &&
						jobCharge.JR_OH_SellAccount == jobCharge.JR_OH_CostAccount)
				{
					costsLinkedToCharge += consolCost.E6_LocalCostAmount;
				}
			}
			return costsLinkedToCharge;
		}

		protected virtual ZDecimal ResultingInvoiceTotalInLocalCurrency(IReceivablesPostingChargeCollection charges)
		{
			return charges.TotalValueInLocalCurrency;
		}

		#region Shipment Due Date Setter

		void SetInvoiceAndDueDates(IReceivablesPostingChargeCollection charges, InvoicingBase invoice, ZDateTime invoiceDate)
		{
			IJobInvoicingPlugIn plugIn = null;
			if (charges != null && charges.Count > 0)
			{
				if (!charges.Any(charge => charge.Job.PK != charges.Job.PK))
				{
					plugIn = charges.Job.Consumer;
				}
				else if (charges.JobInvoicingPlugIn != null)
				{
					plugIn = charges.JobInvoicingPlugIn;
				}
			}
			else if (invoice.Lines.Count > 0)
			{
				var invoiceLine = invoice.Lines[0];
				if (invoiceLine.InvoicingJob != null && !invoice.Lines.Where(line => line.AL_JH != invoiceLine.AL_JH).Any())
				{
					plugIn = invoiceLine.InvoicingJob.PlugInData;
				}
			}

			string invoiceType = charges == null ? invoice.AH_TransactionCategory : charges.Key.InvoiceType;

			var dueDateSetter = new InvoiceAndDueDateCalculator(plugIn,
				invoiceDate.IsEmpty ? invoice.AH_InvoiceDate : invoiceDate,
				invoice.Header,
				invoice.JobType,
				invoice.Direction,
				invoice.TransportMode,
				invoice.AH_GB,
				invoice.AH_GE,
				LedgerTypes.AccountsReceivable,
				invoiceType,
				invoice.Job);
			invoice.AH_InvoiceDate = invoiceDate.IsEmpty ? dueDateSetter.InvoiceDate : invoiceDate;
			invoice.AH_InvoiceTerm = dueDateSetter.InvoiceTerm;
			invoice.AH_InvoiceTermDays = dueDateSetter.InvoiceTermDays;
			invoice.AH_DueDate = dueDateSetter.DueDate;

			if (dueDateSetter.IsInvoiceTermOverridden)
			{
				invoice.MessageForInvoiceTermOverriding = dueDateSetter.MessageForInvoiceTermOverriding;
			}
		}

		void UpdateTaxDateAndAmountOnApplicableLines(InvoicingBase invoice)
		{
			if (!LinesNeedTaxDateCalculationBasedOnInvoiceDate.Any())
			{
				return;
			}

			var invoiceDate = invoice.AH_InvoiceDate.Date;
			var lines = invoice.Lines.Where(x => LinesNeedTaxDateCalculationBasedOnInvoiceDate.Contains(x.PK)).Cast<InvoicingLineBase>();
			foreach (var x in lines)
			{
				var charge = ((ILineMatching)x).Charge;
				charge.SetSellTaxDateSafe(invoiceDate);
				x.SetTaxDateSafe(charge.JR_SellTaxDate);

				using (charge.ClearRevenueLinkOnlyTemporary())
				using (new DisposableAction(
					() => charge.SetContext(BusinessContext.PostingReceivableCharges),
					() => charge.RemoveContext(BusinessContext.PostingReceivableCharges)))
				{
					CopyExchangeRateAndAmount(x, charge, x.AL_RX_NKTransactionCurrency, x.AL_ExchangeRate, true);
				}
			}
		}

		#endregion

		#endregion
	}
}
