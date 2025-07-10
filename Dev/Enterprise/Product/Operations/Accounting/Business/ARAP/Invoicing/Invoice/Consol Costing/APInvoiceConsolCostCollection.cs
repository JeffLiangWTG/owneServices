using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class APInvoiceConsolCostCollection : BusinessObjectCollection<JobConsolCost>
	{
		public APInvoiceConsolCostCollection(BusinessObjectFactory factory, InvoicingBase parentAPInvoice)
			: base(factory)
		{
			this.ParentAPInvoice = parentAPInvoice;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			IDisposable childValidationSuspender = ParentAPInvoice.IsValidationSuspended ? child.GetValidationSuspender() : null;
			try
			{
				base.SetDefaultsForNewChild(child);
				var newCost = (JobConsolCost)child;
				newCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);   // Allow later setting of the E6_ParentID and E6_ParentTableCode onthis new Consol Cost as we do not have them at the moment of adding
				HookEvents(newCost); //should be here as well as in OnCountChanged because otherwise all settings in this method will be without event hadlers attached

				newCost.ParentAPInvoice = ParentAPInvoice;
				newCost.E6_RX_NKCurrency = ParentAPInvoice.AH_RX_NKTransactionCurrency;
				if (newCost.E6_ExchangeRate.IsEmpty || !ParentAPInvoice.AH_PostedToEFT)
				{
					newCost.E6_ExchangeRate = ParentAPInvoice.AH_ExchangeRate;
				}

				newCost.E6_OH_Creditor = ParentAPInvoice.AH_OH;
				newCost.E6_InvoiceNum = ParentAPInvoice.AH_TransactionNum;
				newCost.E6_InvoiceDate = ParentAPInvoice.AH_InvoiceDate;
				newCost.E6_DocumentReceivedDate = ParentAPInvoice.AH_DocumentReceivedDate;
				newCost.E6_CostReference = ParentAPInvoice.AH_ChequeOrReference;
				newCost.E6_PaymentDate = ParentAPInvoice.AH_DueDate;
				newCost.E6_GC = GlbCompany.CurrentCompany.PK;
				newCost.E6_ApportionmentMethod = newCost.Consol.GetApportionmentMethod(newCost.ChargeCode);
				newCost.E6_PPDCLT = PrepaidCollectList.Codes.All;
				newCost.IsFinal = AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				newCost.E6_IsTaxAmountOverridden = true;
				newCost.E6_GB_CostTaxBranch = ParentAPInvoice.AH_GB_TaxBranch;

				JobConsolCost previousCost = null;
				if (Count >= 1)
				{
					previousCost = this[Count - 1];
				}

				if (!IsSettingDefaultConsolSuspended && previousCost != null && previousCost.E6_ParentID.IsValid && !previousCost.E6_ParentTableCode.IsEmpty)
				{
					using (newCost.GetConsolChangedSuspender())
					{
						if (EnforceCreatingConsolCosts)
						{
							using (newCost.ReportSettingParentSuspender.GetSuspender())
							{
								newCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(previousCost.E6_ParentID, previousCost.E6_ParentTableCode);
							}
						}
						else
						{
							newCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(previousCost.E6_ParentID, previousCost.E6_ParentTableCode);
						}
					}
				}
			}
			finally
			{
				if (childValidationSuspender != null)
				{
					childValidationSuspender.Dispose();
				}
			}
		}

#if DEBUG
		public JobConsolCost TryAddNewForConsol_ForTestOnly(IJobCostingPlugIn consol)
		{
			Argument.NotNull(consol, nameof(consol));
			JobConsolCost result = null;

			if (CheckEnforceCreatingConsolCostsForConsol(consol))
			{
				using (GetSuspenderForSettingDefaultConsol())
				{
					result = AddNew();
				}

				using (result.ReportSettingParentSuspender.GetSuspender())
				{
					result.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.CostSupporter.PK, consol.CostSupporter.Type);
				}
			}
			return result;
		}
#endif

		bool EnforceCreatingConsolCosts
		{
			get
			{
				return ParentAPInvoice != null && ParentAPInvoice.HasContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		bool CheckEnforceCreatingConsolCostsForConsol(IJobCostingPlugIn consol)
		{
			return consol is BusinessObject consolBizO && (consolBizO.HasContext(BusinessContext.EnableDirectSettingConsolCostParent) || !consol.IsGatewayBillingEnabled());
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			var consolCost = (JobConsolCost)e.BizObject;
			UnHookEvents(consolCost);
			if (e.ItemAdded)
			{
				HookEvents(consolCost);
			}
		}

		void HookEvents(JobConsolCost consolCost)
		{
			consolCost.APInvoiceConsolCostingJobCreationError += APInvoiceConsolCostCollection_APInvoiceConsolCostingJobCreationError;
			if (ConsolCostAmountChanged != null)
			{
				consolCost.E6_LocalCostAmountInfo.ValueChanged += ConsolCostAmountChanged;
				consolCost.E6_ParentIDInfo.ValueChanged += ConsolCostAmountChanged;
				consolCost.E6_OSGSTAmount_CalcInfo.ValueChanged += ConsolCostAmountChanged;
			}
		}

		void UnHookEvents(JobConsolCost consolCost)
		{
			consolCost.APInvoiceConsolCostingJobCreationError -= APInvoiceConsolCostCollection_APInvoiceConsolCostingJobCreationError;
			if (ConsolCostAmountChanged != null)
			{
				consolCost.E6_LocalCostAmountInfo.ValueChanged -= ConsolCostAmountChanged;
				consolCost.E6_ParentIDInfo.ValueChanged -= ConsolCostAmountChanged;
				consolCost.E6_OSGSTAmount_CalcInfo.ValueChanged -= ConsolCostAmountChanged;
			}
		}

		void APInvoiceConsolCostCollection_APInvoiceConsolCostingJobCreationError(object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e)
		{
			if (OnJobCreationError != null)
			{
				OnJobCreationError(sender, e);
			}
			else
			{
				throw new JobCreationException(e.ErrorMessage);
			}
		}

		public void RaiseOnConsolChanged(JobConsolCost sender, JobConsolCost.ConsolChangedEventArgs args)
		{
			if (OnConsolChanged != null)
			{
				OnConsolChanged(sender, args);
			}
		}

		public bool IsConsolCostChangedHooked
		{
			get { return OnConsolChanged != null; }
		}

		public event EventHandler<JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs> OnJobCreationError;
		public event EventHandler<JobConsolCost.ConsolChangedEventArgs> OnConsolChanged;
		public event EventHandler ConsolCostAmountChanged;

		public readonly InvoicingBase ParentAPInvoice;

		protected override bool AllowNewCore
		{
			get { return !(ParentAPInvoice?.IsInvoiceApproving ?? false) && base.AllowNewCore; }
		}

		protected override bool AllowRemoveCore
		{
			get { return !(ParentAPInvoice?.IsInvoiceApproving ?? false) && base.AllowRemoveCore; }
		}

		public override void Load()
		{
			base.Load(new ZQuery(JobConsolCostSchema.E6_AH_APInvoice, ParentAPInvoice.PK));
		}

		class SettingDefaultConsolSuspender : IDisposable
		{
			internal SettingDefaultConsolSuspender(APInvoiceConsolCostCollection parent)
			{
				this.parent = parent;
				this.parent.settingDefaultConsolSuspendCount++;
			}

			void IDisposable.Dispose()
			{
				parent.settingDefaultConsolSuspendCount--;
			}

			readonly APInvoiceConsolCostCollection parent;
		}

		public IDisposable GetSuspenderForSettingDefaultConsol()
		{
			return new SettingDefaultConsolSuspender(this);
		}

		protected bool IsSettingDefaultConsolSuspended
		{
			get { return settingDefaultConsolSuspendCount > 0; }
		}
		int settingDefaultConsolSuspendCount;

		internal bool IsConsolCostSelectionOnConsolChangedSuspended
		{
			get;
			private set;
		}

		public ConsolCostSelectionOnConsolChangedSuspender GetConsolCostSelectionOnConsolChangedSuspender()
		{
			return new ConsolCostSelectionOnConsolChangedSuspender(this);
		}

		public class ConsolCostSelectionOnConsolChangedSuspender : IDisposable
		{
			public ConsolCostSelectionOnConsolChangedSuspender(APInvoiceConsolCostCollection parent)
			{
				parent.IsConsolCostSelectionOnConsolChangedSuspended = true;
				this.Parent = parent;
			}

			readonly APInvoiceConsolCostCollection Parent;

			void IDisposable.Dispose()
			{
				Parent.IsConsolCostSelectionOnConsolChangedSuspended = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		protected override DisposableList GetAdditionalListChangedSuspenders()
		{
			DisposableList list = null;
			try
			{
				list = base.GetAdditionalListChangedSuspenders();
				if (list == null)
				{
					list = new DisposableList(1);
				}

				list.Add(GetSuspenderForSettingDefaultConsol());

				return list;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				list?.Dispose();
				throw;
			}
		}

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new DisposableList(new[] { base.SuspendAdditionallyForImport(), ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateSuspender.GetSuspender(), SuspendListChanged(), GetConsolCostSelectionOnConsolChangedSuspender() });
		}

#if DEBUG
		public int FireListChangedCount_ForTestOnly;

#endif
	}
}
