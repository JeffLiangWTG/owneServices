using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APBulkInvoicePoster : InvoiceBulkOperation
	{
		public APBulkInvoicePoster(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public APBulkInvoicePoster()
		{
		}

		#region Public Members

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			ApportionAccruals();

			bool hasComplianceErrors = false;
			foreach (APInvoiceForBulkPoster invoice in Invoices)
			{
				var complianceError = invoice.AssignComplianceSubTypeAndCheckComplianceErrors();
				if (!complianceError.IsEmpty)
				{
					invoice.AddRowError(complianceError);
					hasComplianceErrors = true;
				}
			}
			if (hasComplianceErrors)
			{
				throw new APBulkInvoiceComplianceSequenceRelatedException();
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

		public void ApportionAccruals()
		{
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.GetSuspender(Factory))
			{
				UpdateRetrievedAccrualsTotal();
				UpdateTotalOSExTaxAmount();
				UpdateTotalOSTaxAmount();

				ZDecimal totalLocalExTaxAmount = TotalLocalExTaxAmount;
				Dictionary<APInvoiceForBulkPoster, ZDecimal> amounts = new Dictionary<APInvoiceForBulkPoster, ZDecimal>();
				Dictionary<APInvoiceForBulkPoster, ZDecimal> taxAmounts = new Dictionary<APInvoiceForBulkPoster, ZDecimal>();
				var selectedAccruals = new List<Accrual>();
				foreach (Accrual accrual in Accruals)
				{
					if (accrual.ShouldReverese)
					{
						selectedAccruals.Add(accrual);
					}
				}

				foreach (APInvoiceForBulkPoster invoice in Invoices)
				{
					ZDecimal initialLocalExTaxAmount = invoice.AH_LocalExTaxAmount;
					ZDecimal initialLocalTaxAmount = invoice.AH_LocalTaxAmount;
					amounts.Add(invoice, invoice.AH_OSExTaxAmount);
					taxAmounts.Add(invoice, invoice.AH_OSTaxAmount);

					using (invoice.Lines.SuspendListChanged())
					{
						invoice.Lines.RemoveAndDeleteAll();
						APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
						invoice.ImportAccrualsIntoInvoice(selectedAccruals, line1);

						foreach (APInvoiceLine line in invoice.Lines)
						{
							line.AL_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(line.AL_LocalExTaxAmount * (initialLocalExTaxAmount / totalLocalExTaxAmount), invoice.AH_ExchangeRate, invoice.AH_RX_NKTransactionCurrency);
							line.AL_OSTaxAmount = line.AL_OSExTaxAmount / initialLocalExTaxAmount * initialLocalTaxAmount;
							line.AL_IsFinalCharge = true;
						}
					}
				}

				// Ensuring that all accruals are precisely apportioned
				for (int i = 0; i < selectedAccruals.Count; i++)
				{
					Accrual accrual = selectedAccruals[i];
					APInvoiceLine lineWithLargestExTaxAmount = null;
					ZDecimal sum = 0;
					foreach (APInvoiceForBulkPoster invoice in Invoices)
					{
						APInvoiceLine currentLine = (APInvoiceLine)invoice.Lines[i];
						sum += currentLine.AL_LocalExTaxAmount;
						if (lineWithLargestExTaxAmount == null || lineWithLargestExTaxAmount.AL_LocalExTaxAmount < currentLine.AL_LocalExTaxAmount)
						{
							lineWithLargestExTaxAmount = currentLine;
						}
					}
					if (sum != accrual.AL_LocalExTaxAmount)
					{
						lineWithLargestExTaxAmount.AL_LocalExTaxAmount += accrual.AL_LocalExTaxAmount - sum;
						lineWithLargestExTaxAmount.AL_OSExTaxAmount = (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(lineWithLargestExTaxAmount.AL_LocalExTaxAmount, lineWithLargestExTaxAmount.APInvoice.AH_ExchangeRate, lineWithLargestExTaxAmount.APInvoice.AH_RX_NKTransactionCurrency);
					}
				}

				foreach (APInvoiceForBulkPoster invoice in Invoices)
				{
					if (invoice.AH_OSExTaxAmount != amounts[invoice])
					{
						using (invoice.Lines.SuspendListChanged())
						{
							var line = (APInvoiceLine)(invoice.Lines.AddNew());

							line.GenericCharge = DiscrepancyGLAccountPK;
							line.AL_GB = invoice.Lines[0].AL_GB;
							line.AL_GE = invoice.Lines[0].AL_GE;
							line.AL_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
							line.AL_ExchangeRate = invoice.AH_ExchangeRate;
							line.AL_JH = ZGuid.Empty;
							line.AL_AT = invoice.TaxRate;
							line.SetTaxDateSafe(invoice.TaxDate);

							line.AL_OSExTaxAmount = amounts[invoice] - invoice.AH_OSExTaxAmount;
							line.AL_OSTaxAmount = line.AL_OSExTaxAmount / amounts[invoice] * taxAmounts[invoice];
						}
					}

					if (invoice.AH_OSTaxAmount != taxAmounts[invoice])
					{
						APInvoiceLine lineWithLargestTaxAmount = null;
						foreach (APInvoiceLine line in invoice.Lines)
						{
							if (lineWithLargestTaxAmount == null || lineWithLargestTaxAmount.AL_OSTaxAmount < line.AL_OSTaxAmount)
							{
								lineWithLargestTaxAmount = line;
							}
						}
						lineWithLargestTaxAmount.AL_OSTaxAmount += taxAmounts[invoice] - invoice.AH_OSTaxAmount;
					}
				}
			}
		}

		public void LoadAccrualCollection()
		{
			Accruals.Load(Filters.GetQuery());
			UpdateRetrievedAccrualsTotal();
		}

		#endregion

		#region GUIBindable Members

		#region Filters

		public new APBulkInvoicePosterFilters Filters
		{
			get { return (APBulkInvoicePosterFilters)FiltersCore; }
		}

		protected override InvoiceBulkOperationFilters GetNewFilters()
		{
			return new APBulkInvoicePosterFilters(Factory);
		}

		#endregion

		#region Accruals

		public AccrualCollection Accruals
		{
			get
			{
				if (fAccruals == null)
				{
					fAccruals = new AccrualCollection(Factory);
					fAccruals.SetAllowNew(false);
					fAccruals.SetAllowRemove(false);
					fAccruals.RecalculateAmounts += new EventHandler(Accruals_RecalculateAmounts);
				}
				return fAccruals;
			}
		}
		AccrualCollection fAccruals;

		void Accruals_RecalculateAmounts(object sender, EventArgs e)
		{
			UpdateRetrievedAccrualsTotal();
		}

		#endregion

		#region Invoices

		public APInvoiceForBulkPosterCollection Invoices
		{
			get
			{
				if (fInvoices == null)
				{
					fInvoices = new APInvoiceForBulkPosterCollection(Factory, this);
					RegisterEditableChildObject(fInvoices);
				}
				return fInvoices;
			}
		}
		APInvoiceForBulkPosterCollection fInvoices;

		#endregion

		#region ExpectedBatchTotal

		public ZDecimal ExpectedBatchTotal
		{
			get { return fExpectedBatchTotal; }
			set
			{
				SetNonPersistentPropertyValue(ExpectedBatchTotalInfo, ref fExpectedBatchTotal, value);
				if (!IsValidationSuspended)
				{
					ValidateExpectedBatchTotal();
				}
			}
		}
		ZDecimal fExpectedBatchTotal;

		public ZPropertyInfo ExpectedBatchTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ExpectedBatchTotal)); }
		}

		#endregion

		#region TotalLocalExTaxAmount

		[ReadOnly(true)]
		public ZDecimal TotalLocalExTaxAmount
		{
			get { return fTotalLocalExTaxAmount; }
			set
			{
				SetNonPersistentPropertyValue(TotalLocalExTaxAmountInfo, ref fTotalLocalExTaxAmount, value);
				if (!IsValidationSuspended)
				{
					ValidateDifference();
				}
			}
		}
		ZDecimal fTotalLocalExTaxAmount;

		public ZPropertyInfo TotalLocalExTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLocalExTaxAmount)); }
		}

		#endregion

		#region TotalOSExTaxAmount

		[ReadOnly(true)]
		public ZDecimal TotalOSExTaxAmount
		{
			get { return fTotalOSExTaxAmount; }
			set
			{
				SetNonPersistentPropertyValue(TotalOSExTaxAmountInfo, ref fTotalOSExTaxAmount, value);
			}
		}
		ZDecimal fTotalOSExTaxAmount;

		public ZPropertyInfo TotalOSExTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOSExTaxAmount)); }
		}

		#endregion

		#region TotalOSTaxAmount

		[ReadOnly(true)]
		public ZDecimal TotalOSTaxAmount
		{
			get { return fTotalOSTaxAmount; }
			set
			{
				SetNonPersistentPropertyValue(TotalOSTaxAmountInfo, ref fTotalOSTaxAmount, value);
			}
		}
		ZDecimal fTotalOSTaxAmount;

		public ZPropertyInfo TotalOSTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOSTaxAmount)); }
		}

		#endregion

		#region TotalOSAmount
		[ReadOnly(true)]
		public ZDecimal TotalOSAmount
		{
			get { return fTotalOSAmount; }
			set
			{
				SetNonPersistentPropertyValue(TotalOSAmountInfo, ref fTotalOSAmount, value);
				if (!IsValidationSuspended)
				{
					ValidateExpectedBatchTotal();
				}
			}
		}
		ZDecimal fTotalOSAmount;

		public ZPropertyInfo TotalOSAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOSAmount)); }
		}

		#endregion

		#region RetrievedAccrualTotal

		[ReadOnly(true)]
		public ZDecimal RetrievedAccrualTotal
		{
			get { return fRetrievedAccrualTotal; }
			set
			{
				SetNonPersistentPropertyValue(RetrievedAccrualTotalInfo, ref fRetrievedAccrualTotal, value);
				if (!IsValidationSuspended)
				{
					ValidateRetrievedAccrualTotal();
					ValidateDifference();
				}
			}
		}
		ZDecimal fRetrievedAccrualTotal;

		public ZPropertyInfo RetrievedAccrualTotalInfo
		{
			get { return GetZPropertyInfo(nameof(RetrievedAccrualTotal)); }
		}

		#endregion

		#region Difference

		public ZDecimal Difference
		{
			get { return RetrievedAccrualTotal - TotalLocalExTaxAmount; }
		}

		public ZPropertyInfo DifferenceInfo
		{
			get { return GetZPropertyInfo(nameof(Difference)); }
		}

		#endregion

		public void UpdateTotalOSExTaxAmount()
		{
			ZDecimal result = 0;
			foreach (APInvoiceForBulkPoster invoice in Invoices)
			{
				result += invoice.AH_OSExTaxAmount;
			}
			TotalOSExTaxAmount = result;
			UpdateTotalLocalExTaxAmount();
			UpdateTotalOSAmount();
		}

		void UpdateTotalLocalExTaxAmount()
		{
			ZDecimal result = 0;
			foreach (APInvoiceForBulkPoster invoice in Invoices)
			{
				result += invoice.AH_LocalExTaxAmount;
			}
			TotalLocalExTaxAmount = result;
		}

		public void UpdateTotalOSTaxAmount()
		{
			ZDecimal result = 0;
			foreach (APInvoiceForBulkPoster invoice in Invoices)
			{
				result += invoice.AH_OSTaxAmount;
			}
			TotalOSTaxAmount = result;
			UpdateTotalOSAmount();
		}

		protected void UpdateTotalOSAmount()
		{
			ZDecimal result = 0;
			foreach (APInvoiceForBulkPoster invoice in Invoices)
			{
				result += invoice.AH_OSTotalAmount;
			}
			TotalOSAmount = result;
		}

		public void UpdateRetrievedAccrualsTotal()
		{
			ZDecimal result = 0;
			foreach (Accrual accrual in Accruals)
			{
				if (accrual.ShouldReverese)
				{
					result += accrual.AL_LocalExTaxAmount;
				}
			}
			RetrievedAccrualTotal = result;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateDiscrepancyGLAccount();
			ValidateRetrievedAccrualTotal();
			ValidateTotalLocalExTaxAmount();
			ValidateExpectedBatchTotal();
			ValidateDifference();
		}

		public void ValidateRetrievedAccrualTotal()
		{
			RetrievedAccrualTotalInfo.ClearAllNotifications();
			if (RetrievedAccrualTotal == 0)
			{
				RetrievedAccrualTotalInfo.AddError(Res.GetString("b561bfb3-85e2-4227-83bd-be623e3c1aaf", "Amount can not equal 0."));
			}
		}

		public void ValidateTotalLocalExTaxAmount()
		{
			TotalLocalExTaxAmountInfo.ClearAllNotifications();
			if (TotalLocalExTaxAmount == 0)
			{
				TotalLocalExTaxAmountInfo.AddError(Res.GetString("b561bfb3-85e2-4227-83bd-be623e3c1aaf", "Amount can not equal 0."));
			}
		}

		public void ValidateExpectedBatchTotal()
		{
			ExpectedBatchTotalInfo.ClearAllNotifications();
			if (ExpectedBatchTotal != TotalOSAmount)
			{
				ExpectedBatchTotalInfo.AddError(Res.GetString("886b626b-22d5-4116-aeb9-fcf82458f58f", "Expected Batch Total should equal the AP Invoice Total."));
			}
		}

		public void ValidateDifference()
		{
			DifferenceInfo.ClearAllNotifications();
			int maxAccrualVsActualDiscrepancies = AccountingConfigurationRegistry.Instance.MaxAccrualVsActualDiscrepancies.Value;
			if (maxAccrualVsActualDiscrepancies != 0 && Math.Abs(Difference) > maxAccrualVsActualDiscrepancies)
			{
				DifferenceInfo.AddError(Res.GetString("70cf1c1f-0df6-4131-85d2-4242d56749c0", "Difference between Invoices and Accruals totals exceeds maximum value set."));
			}
			else if (Difference != 0.0m)
			{
				DifferenceInfo.AddWarning(Res.GetString("e976cea6-0651-403d-b21d-ec744fc162cf", "Invoices and Accruals totals do not match."));
			}
		}

		#endregion

		#region Implementation

		ZGuid DiscrepancyGLAccountPK
		{
			get
			{
				if (!fDiscrepancyGLAccountPK.IsValid)
				{
					fDiscrepancyGLAccountPK = (Guid)AccountingConfigurationRegistry.Instance.DiscrepancyGLAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				}
				return fDiscrepancyGLAccountPK;
			}
		}
		ZGuid fDiscrepancyGLAccountPK;

		void ValidateDiscrepancyGLAccount()
		{
			if (!DiscrepancyGLAccountPK.IsValid)
			{
				AddRowError(Res.GetString("3726d122-bba4-4469-acf9-484d10fdcde8", "Discrepancy GL Account is not set. Please specify the Discrepancy GL Account in System Registry."));
			}
		}

		#endregion
	}
}
