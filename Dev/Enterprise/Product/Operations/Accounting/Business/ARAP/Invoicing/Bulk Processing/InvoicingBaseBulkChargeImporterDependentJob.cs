using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseBulkChargeImporterDependentJob : DependentJob<InvoicingBaseBulkChargeImporter>
	{
		public InvoicingBaseBulkChargeImporterDependentJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		#region Charges

		internal void ClearCharges()
		{
			if (fCharges != null)
			{
				UnHookChargesEventHandlers();
				fCharges.RemoveAll();
				UnRegisterEditableChildObject(fCharges);
				fCharges = null;
			}
		}

		ChargeForBulkChargeImportCollection fCharges;

		void HookChargesEventHandlers()
		{
			if (fCharges != null)
			{
				fCharges.IsSelectedChanged += fCharges_IsSelectedChanged;
			}
		}

		void UnHookChargesEventHandlers()
		{
			if (fCharges != null)
			{
				fCharges.IsSelectedChanged -= fCharges_IsSelectedChanged;
			}
		}

		InvoicingBaseBulkChargeImporter fMaster;

		internal new InvoicingBaseBulkChargeImporter Master
		{
			get { return fMaster ?? (fMaster = base.Master); }
			set { fMaster = value; }
		}

		[ChildEditable(true)]
		public new ChargeForBulkChargeImportCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ChargeForBulkChargeImportCollection(this);
					HookChargesEventHandlers();
					if (Master != null && Master.ChargesFilteredByViewingPermission != null)
					{
						fCharges.AddRange((from Charge charge in Master.ChargesFilteredByViewingPermission where charge.JR_JH == this.PK select charge));
						if (Master.ParentChildrenJobPKDictionary != null && Master.ParentChildrenJobPKDictionary.ContainsKey(this.PK))
						{
							fCharges.AddRange((from Charge charge in Master.ChargesFilteredByViewingPermission where Master.ParentChildrenJobPKDictionary[this.PK].Contains(charge.JR_JH) select charge));
						}
						if (AdditionalJobsToShowChargesFor.Any())
						{
							fCharges.AddRange((from Charge charge in Master.ChargesFilteredByViewingPermission where AdditionalJobsToShowChargesFor.Contains(charge.JR_JH) select charge));
						}
					}
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}

		void fCharges_IsSelectedChanged(object sender, EventArgs e)
		{
			SelectedChargesLocalCostAmountInfo.RefreshBinding();
			if (IsSelectedChanged != null)
			{
				IsSelectedChanged(sender, e);
			}
		}

		public IEnumerable<Charge> GetChargesSelectedForImport()
		{
			var result = new List<Charge>();
			foreach (Charge charge in Charges)
			{
				if (charge.IsSelectedForImport)
				{
					result.Add(charge);
				}
			}
			return result;
		}

		ZQuery fChargesAdditionalFilter;
		public ZQuery ChargesAdditionalFilter
		{
			get { return fChargesAdditionalFilter ?? (fChargesAdditionalFilter = new ZQuery()); }
			set
			{
				fChargesAdditionalFilter = value;
				Charges.Load(value);
			}
		}

		internal new List<ZGuid> AdditionalJobsToShowChargesFor
		{
			get { return base.AdditionalJobsToShowChargesFor; }
		}

		#endregion

		#endregion

		protected override bool GetPropertiesReadOnlyStateCore(System.ComponentModel.PropertyDescriptor property)
		{
			return property.Name != IsSelectedForImportInfo.Name && base.GetPropertiesReadOnlyStateCore(property);
		}

		#region Properties

		#region IsSelectedForImport

		public event EventHandler IsSelectedChanged;

		ZBool fIsSelectedForImport;
		public ZBool IsSelectedForImport
		{
			get { return fIsSelectedForImport; }
			set
			{
				SetNonPersistentPropertyValue(IsSelectedForImportInfo, ref fIsSelectedForImport, value);
				using (Master != null ? Master.GetUpdateSelectedLocalTotalSuspender() : null)
				{
					foreach (Charge charge in Charges)
					{
						charge.IsSelectedForImport = value;
					}
				}
				Charges.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSelectedForImportInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelectedForImport)); }
		}

		internal void ClearIsSelectedChangedEventHandlers()
		{
			IsSelectedChanged = null;
		}
		#endregion

		#region SelectedChargesLocalCostAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal SelectedChargesLocalCostAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (Charge charge in GetChargesSelectedForImport())
				{
					result += charge.JR_LocalCostAmt;
				}

				return result;
			}
		}

		public ZPropertyInfo SelectedChargesLocalCostAmountInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedChargesLocalCostAmount)); }
		}

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal SelectedChargesInvoiceCurrencyAmount
		{
			get
			{
				ZDecimal result = 0m;

				foreach (Charge charge in GetChargesSelectedForImport())
				{
					if (ParentAPInvoice != null && !ParentAPInvoice.AH_RX_NKTransactionCurrency.IsEmpty)
					{
						ZDecimal amountToIncrement;

						if (charge.JR_RX_NKCostCurrency == ParentAPInvoice.AH_RX_NKTransactionCurrency)
						{
							amountToIncrement = charge.JR_OSCostAmt;
						}
						else if (ParentAPInvoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							amountToIncrement = charge.JR_LocalCostAmt;
						}
						else
						{
							ZDecimal localAmountUnRounded = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(charge.JR_OSCostAmt, charge.JR_OSCostExRate);
							// The SelectedChargesInvoiceCurrencyAmount property is used only when AH_PostedToEFT is true. Falling back to Invoice Ex Rate if called without AH_PostedToEFT
							var exRate = ParentAPInvoice.AH_PostedToEFT ? InvoiceCurrencyJobExchangeRate : ParentAPInvoice.AH_ExchangeRate;
							amountToIncrement = Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmountUnRounded, exRate, ParentAPInvoice.AH_RX_NKTransactionCurrency);
						}

						result += amountToIncrement;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo SelectedChargesInvoiceCurrencyAmountInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedChargesInvoiceCurrencyAmount)); }
		}

		#endregion

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal InvoiceCurrencyJobExchangeRate
		{
			get
			{
				ZDecimal result = 0m;
				if (ParentAPInvoice != null)
				{
					result = ParentAPInvoice.GetExchangeRateFromJobExRateConfig(this);
				}
				return result;
			}
		}

		public ZPropertyInfo InvoiceCurrencyJobExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceCurrencyJobExchangeRate)); }
		}

		InvoicingBase ParentAPInvoice
		{
			get { return Master.ParentInvoice; }
		}

		public override int LocalDecimals => ParentAPInvoice != null ? ParentAPInvoice.Company.GetLocalDecimals() : base.LocalDecimals;

		public int ExchangeRateDecimals => ParentAPInvoice != null ? ParentAPInvoice.Company.ExchangeRateDecimalPlaces : GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		public int OSDecimals => ParentAPInvoice?.TransactionCurrency != null ? ParentAPInvoice.TransactionCurrency.Decimals : LocalDecimals;

		#endregion

		#region Overrides

		public override void RefreshCachedValues()
		{
			Charges.Load(ChargesAdditionalFilter);
		}

		#endregion
	}
}
