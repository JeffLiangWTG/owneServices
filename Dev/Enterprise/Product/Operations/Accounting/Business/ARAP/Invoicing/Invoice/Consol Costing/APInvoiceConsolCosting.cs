using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceConsolCosting : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Consol Summary

		public class APInvoiceConsolSummary : NonPersistentBusinessObject, IObsoleteValidation
		{
			public static class Schema
			{
				public const string ConsolNo = "ConsolNo";
				public const string MasterBillNo = "MasterBillNo";
				public const string LocalTotalAmount = "LocalTotalAmount";
				public const string LocalTotalTaxAmount = "LocalTotalTaxAmount";
				public const string LocalTotalAmountWithTax = "LocalTotalAmountWithTax";
				public const string LocalCurrencyDecimals = "LocalCurrencyDecimals";
				public const string InvoiceCurrencyTotalAmount = "InvoiceCurrencyTotalAmount";
				public const string InvoiceCurrencyTotalTaxAmount = "InvoiceCurrencyTotalTaxAmount";
				public const string InvoiceCurrencyTotalAmountWithTax = "InvoiceCurrencyTotalAmountWithTax";
				public const string InvoiceCurrency = "InvoiceCurrency";
				public const string InvoiceCurrencyDecimals = "InvoiceCurrencyDecimals";
			}

			public APInvoiceConsolSummary(IJobCostingPlugIn consol, InvoicingBase parentAPInvoice) : base(parentAPInvoice.Factory)
			{
				this.Consol = consol;
				this.ParentAPInvoice = parentAPInvoice;
			}

			readonly InvoicingBase ParentAPInvoice;
			public IJobCostingPlugIn Consol { get; private set; }

			public ZString ConsolNo { get { return Consol == null ? ZString.Empty : Consol.JK_UniqueConsignRef; } }
			public ZPropertyInfo ConsolNoInfo { get { return GetZPropertyInfo(Schema.ConsolNo); } }

			public ZString MasterBillNo { get { return Consol == null ? ZString.Empty : Consol.CostSupporter.MasterBillNum; } }
			public ZPropertyInfo MasterBillNoInfo { get { return GetZPropertyInfo(Schema.MasterBillNo); } }

			ZDecimal fLocalTotalAmount;
			public ZDecimal LocalTotalAmount
			{
				get { return fLocalTotalAmount; }
				set { SetNonPersistentPropertyValue(LocalTotalAmountInfo, ref fLocalTotalAmount, value); }
			}
			public ZPropertyInfo LocalTotalAmountInfo { get { return GetZPropertyInfo(Schema.LocalTotalAmount); } }

			ZDecimal fLocalTotalTaxAmount;
			public ZDecimal LocalTotalTaxAmount
			{
				get { return fLocalTotalTaxAmount; }
				set { SetNonPersistentPropertyValue(LocalTotalTaxAmountInfo, ref fLocalTotalTaxAmount, value); }
			}
			public ZPropertyInfo LocalTotalTaxAmountInfo { get { return GetZPropertyInfo(Schema.LocalTotalTaxAmount); } }

			ZDecimal fLocalTotalAmountWithTax;
			public ZDecimal LocalTotalAmountWithTax
			{
				get { return fLocalTotalAmountWithTax; }
				set { SetNonPersistentPropertyValue(LocalTotalAmountWithTaxInfo, ref fLocalTotalAmountWithTax, value); }
			}
			public ZPropertyInfo LocalTotalAmountWithTaxInfo { get { return GetZPropertyInfo(Schema.LocalTotalAmountWithTax); } }

			public ZInt LocalCurrencyDecimals { get { return ParentAPInvoice.AH_Calc_LocalRXDecimals; } }
			public ZPropertyInfo LocalCurrencyDecimalsInfo { get { return GetZPropertyInfo(Schema.LocalCurrencyDecimals); } }

			ZDecimal fInvoiceCurrencyTotalAmount;
			public ZDecimal InvoiceCurrencyTotalAmount
			{
				get { return fInvoiceCurrencyTotalAmount; }
				set { SetNonPersistentPropertyValue(InvoiceCurrencyTotalAmountInfo, ref fInvoiceCurrencyTotalAmount, value); }
			}
			public ZPropertyInfo InvoiceCurrencyTotalAmountInfo { get { return GetZPropertyInfo(Schema.InvoiceCurrencyTotalAmount); } }

			ZDecimal fInvoiceCurrencyTotalTaxAmount;
			public ZDecimal InvoiceCurrencyTotalTaxAmount
			{
				get { return fInvoiceCurrencyTotalTaxAmount; }
				set { SetNonPersistentPropertyValue(InvoiceCurrencyTotalTaxAmountInfo, ref fInvoiceCurrencyTotalTaxAmount, value); }
			}
			public ZPropertyInfo InvoiceCurrencyTotalTaxAmountInfo { get { return GetZPropertyInfo(Schema.InvoiceCurrencyTotalTaxAmount); } }

			ZDecimal fInvoiceCurrencyTotalAmountWithTax;
			public ZDecimal InvoiceCurrencyTotalAmountWithTax
			{
				get { return fInvoiceCurrencyTotalAmountWithTax; }
				set { SetNonPersistentPropertyValue(InvoiceCurrencyTotalAmountWithTaxInfo, ref fInvoiceCurrencyTotalAmountWithTax, value); }
			}
			public ZPropertyInfo InvoiceCurrencyTotalAmountWithTaxInfo { get { return GetZPropertyInfo(Schema.InvoiceCurrencyTotalAmountWithTax); } }

			ZString fInvoiceCurrency;

			[MaxLength(3)]
			public ZString InvoiceCurrency
			{
				get { return fInvoiceCurrency; }
				set { SetNonPersistentPropertyValue(InvoiceCurrencyInfo, ref fInvoiceCurrency, value); }
			}

			public ZPropertyInfo InvoiceCurrencyInfo { get { return GetZPropertyInfo(Schema.InvoiceCurrency); } }

			public ZInt InvoiceCurrencyDecimals { get { return ParentAPInvoice.AH_Calc_RXDecimals; } }
			public ZPropertyInfo InvoiceCurrencyDecimalsInfo { get { return GetZPropertyInfo(Schema.InvoiceCurrencyDecimals); } }
		}

		public class APInvoiceConsolSummaryCollection : NonPersistentBusinessObjectCollection<APInvoiceConsolSummary>
		{
			public APInvoiceConsolSummaryCollection(APInvoiceConsolCosting parentCosting) : base(parentCosting.Factory)
			{
				this.ParentCosting = parentCosting;
			}

			readonly APInvoiceConsolCosting ParentCosting;

			InvoicingBase ParentAPInvoice
			{
				get { return ParentCosting != null ? ParentCosting.ParentAPInvoice : null; }
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new APInvoiceConsolSummary(null, null);
			}

#if DEBUG
			public int UpdateCount_ForTestOnly;
#endif

			public void Update()
			{
				if (!UpdateSuspender.IsSuspended && ParentAPInvoice != null && !ParentAPInvoice.IsDeleted)
				{
#if DEBUG
					if (Globals.IsTest)
					{
						UpdateCount_ForTestOnly++;
					}
#endif
					RemoveAll();

					string postingCurrency = ParentAPInvoice.AH_RX_NKTransactionCurrency;
					foreach (JobConsolCost consolCost in ParentCosting.ConsolCosts)
					{
						string consolCostCurrency = consolCost.E6_RX_NKCurrency;
						decimal postingExchangeRate = GetPostingExchangeRate(consolCost, postingCurrency, consolCostCurrency);

						decimal localExTaxAmount = consolCost.E6_Calc_LocalExTaxAmount;
						decimal localTaxAmount = consolCost.E6_Calc_LocalGSTAmount;
						decimal osExTaxAmount, osTaxAmount;

						if (postingCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							osExTaxAmount = localExTaxAmount;
							osTaxAmount = localTaxAmount;
						}
						else
						{
							osExTaxAmount = GetCrossRateAmount(consolCost.E6_OSCostAmount, consolCostCurrency, consolCost.E6_ExchangeRate, postingCurrency, postingExchangeRate);
							osTaxAmount = GetCrossRateAmount(consolCost.E6_OSGSTAmount_Calc, consolCostCurrency, consolCost.E6_ExchangeRate, postingCurrency, postingExchangeRate);

							if (postingExchangeRate != consolCost.E6_ExchangeRate)
							{
								localExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(osExTaxAmount, postingExchangeRate);
								localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, consolCost.E6_GC, localExTaxAmount,
									consolCost.TaxRate, consolCost.TaxRate?.GetRate(consolCost.E6_TaxDate), consolCost.TaxRate?.GetEffectiveExtraRate(consolCost.E6_TaxDate),
									osTaxAmount, postingExchangeRate);
							}
						}

						APInvoiceConsolSummary summary = RetrieveOrAddSummary(consolCost.Consol);
						summary.InvoiceCurrency = postingCurrency;
						summary.LocalTotalAmount += localExTaxAmount;
						summary.LocalTotalTaxAmount += localTaxAmount;
						summary.LocalTotalAmountWithTax += localExTaxAmount + localTaxAmount;
						summary.InvoiceCurrencyTotalAmount += osExTaxAmount;
						summary.InvoiceCurrencyTotalTaxAmount += osTaxAmount;
						summary.InvoiceCurrencyTotalAmountWithTax += osExTaxAmount + osTaxAmount;
					}
				}
			}

			decimal GetPostingExchangeRate(JobConsolCost consolCost, string postingCurrency, string consolCostCurrency)
			{
				decimal result;

				if (ParentAPInvoice.AH_PostedToEFT)
				{
					result = (postingCurrency == consolCostCurrency) ? consolCost.E6_ExchangeRate : consolCost.GetExchangeRateBasedOnJobBillingExchangeRateConfiguration(ParentAPInvoice.TransactionCurrency);
				}
				else
				{
					result = ParentAPInvoice.AH_ExchangeRate;
				}

				return result;
			}

			decimal GetCrossRateAmount(decimal sourceAmount, string sourceCurrency, decimal sourceExchangeRate, string postingCurrency, decimal postingExchangeRate)
			{
				decimal result = 0m;

				if (postingCurrency == sourceCurrency && sourceExchangeRate == postingExchangeRate)
				{
					result = sourceAmount;
				}
				else
				{
					ZDecimal unRoundedLocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(sourceAmount, sourceExchangeRate);
					result = Env.CurrentCompany.ExchangeRate.LocalToForeign(unRoundedLocalAmount, postingExchangeRate, postingCurrency);
				}

				return result;
			}

			APInvoiceConsolSummary RetrieveOrAddSummary(IJobCostingPlugIn consol)
			{
				APInvoiceConsolSummary result = null;
				foreach (APInvoiceConsolSummary summary in this)
				{
					if (summary.Consol == consol)
					{
						result = summary;
					}
				}
				if (result == null)
				{
					result = new APInvoiceConsolSummary(consol, ParentAPInvoice);
					Add(result);
				}

				return result;
			}

			public override bool ReadOnly
			{
				get { return true; }
			}

			public FunctionalitySuspender UpdateSuspender => updateSuspender ?? (updateSuspender = new FunctionalitySuspender(() => Update(), true));
			FunctionalitySuspender updateSuspender;
		}
		#endregion

		public APInvoiceConsolCosting(BusinessObjectFactory factory, InvoicingBase parentAPInvoice)
			: base(factory)
		{
			this.ParentAPInvoice = parentAPInvoice;
		}

		public InvoicingBase ParentAPInvoice { get; private set; }

		internal CostVarianceApprovalHelper CostVarianceApprovalHelper
		{
			get
			{
				return costVarianceApprovalHelper ?? (costVarianceApprovalHelper = new CostVarianceApprovalHelper(this));
			}
		}
		CostVarianceApprovalHelper costVarianceApprovalHelper;

		APInvoiceConsolCostCollection fConsolCosts;
		public APInvoiceConsolCostCollection ConsolCosts
		{
			get
			{
				if (fConsolCosts == null)
				{
					fConsolCosts = new APInvoiceConsolCostCollection(Factory, ParentAPInvoice);
					if (ParentAPInvoice.IsInvoiceApproving)
					{
						fConsolCosts.Load();
						foreach (JobConsolCost consolCost in fConsolCosts)
						{
							consolCost.ParentAPInvoice = ParentAPInvoice;
							using (consolCost.SuspendSplittingApportionAmount())
							{
								if (ParentAPInvoice.AH_TransactionType == Enterprise.ZArchitecture.Core.TransactionTypes.CreditNote)
								{
									consolCost.E6_OSCostAmount = -consolCost.E6_OSCostAmount;
								}
							}
						}
					}
					fConsolCosts.ConsolCostAmountChanged += ConsolCosts_ConsolCostAmountChanged;
					fConsolCosts.CountChanged += ConsolCosts_ConsolCostAmountChanged;
					RegisterEditableChildObject(fConsolCosts);
				}
				return fConsolCosts;
			}
		}

		void ConsolCosts_ConsolCostAmountChanged(object sender, EventArgs e)
		{
			ConsolSummary.Update();
			RefreshBindingIncludingChildren();
		}

		#region ConsolSummary

		APInvoiceConsolSummaryCollection fConsolSummary;
		[ChildEditable]
		public APInvoiceConsolSummaryCollection ConsolSummary
		{
			get
			{
				if (fConsolSummary == null)
				{
					fConsolSummary = new APInvoiceConsolSummaryCollection(this);
					fConsolSummary.Update();
					RegisterEditableChildObject(fConsolSummary);
				}
				return fConsolSummary;
			}
		}

		#endregion

		#region CostVarianceApproval
#if DEBUG

		public void ClearCachedCostVarianceApproval_ForTestOnly()
		{
			CostVarianceApprovalHelper.ClearCachedCostVarianceApproval_ForTestOnly();
		}

#endif
		#endregion
	}
}
