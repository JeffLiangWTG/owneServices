using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostCollection : BusinessObjectCollection<JobConsolCost>, IBindingList
	{
		public JobConsolCostCollection(BusinessObjectFactory factory, IGenericJobCostPlugIn jobCostPlugIn, bool isUsedForGateway = false)
			: base(factory)
		{
			GenericJobCostPlugIn = jobCostPlugIn;
			IsUsedForGateway = isUsedForGateway;
			((IBindingList)this).ListChanged += JobConsolCostCollection_ListChanged;
		}

		void JobConsolCostCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			var forwardingConsol = GenericJobCostPlugIn as ForwardingConsol;
			if (forwardingConsol != null)
			{
				forwardingConsol.JK_Calc_ConsolidatedFreightCostChargeableInfo.RefreshBinding();
				forwardingConsol.JK_Calc_ShipmentFreightCostChargeableInfo.RefreshBinding();
			}
		}

		public readonly IGenericJobCostPlugIn GenericJobCostPlugIn;

		// in phrase 2, this will become readonly
		public /*readonly*/ bool IsUsedForGateway;

		bool isPosting;

		public bool IsPosting
		{
			get
			{
				return isPosting;
			}
			set
			{
				isPosting = value;
				foreach (JobConsolCost cost in this)
				{
					cost.IsPosting = IsPosting;
				}
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			JobConsolCost cost = (JobConsolCost)child;
			using (cost.GetValidationSuspender())
			using (cost.SuspendSplittingApportionAmount())
			{
				cost.IsPosting = IsPosting;
				if (BypassReportSettingParentSuspender.IsSuspended)
				{
					cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(GenericJobCostPlugIn.CostSupporter.PK, GenericJobCostPlugIn.CostSupporter.Type);
				}
				else
				{
					using (cost.ReportSettingParentSuspender.GetSuspender())
					{
						cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(GenericJobCostPlugIn.CostSupporter.PK, GenericJobCostPlugIn.CostSupporter.Type);
					}
				}
				cost.E6_PPDCLT = PrepaidCollectList.Codes.All;
				cost.E6_GC = GlbCompany.CurrentCompany.PK;
				cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				cost.E6_ExchangeRate = 1m;
				cost.E6_GS_NKConsolCostOwner = GlbStaff.CurrentUser.GS_Code;

				cost.E6_ApportionmentMethod = cost.Consol.GetApportionmentMethod(cost.ChargeCode);
			}
		}

		public override bool ReadOnly
		{
			get { return IsUsedForGateway || base.ReadOnly; }
		}

		public SecurityCheckpoint ConsolJobInvoicingEnterOrModifyCheckPoint => IsUsedForGateway ? Env.Security.GatewayConsolJobInvoicingEnterOrModify : Env.Security.MaintainConsolJobInvoicingEnterOrModify;
		protected override bool AllowRemoveCore
		{
			get { return Env.Security.MaintainConsolJobInvoicingDelete.IsAllowed; }
		}

#if DEBUG
		public bool AllowRemoveCoreForTest => AllowRemoveCore;
#endif

		public void UpdateAllConsolCostApportionmentCharges()
		{
			foreach (JobConsolCost cost in this)
			{
				cost.UpdateApportionmentChargesListing();
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();

			result.AddToFilter(JobConsolCostSchema.E6_ParentID, GenericJobCostPlugIn.CostSupporter?.PK ?? ZGuid.Empty);
			result.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, GenericJobCostPlugIn.CostSupporter?.Type ?? ZString.Empty);
			result.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
			var op = IsUsedForGateway ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal;
			result.AddToFilter(JobConsolCostSchema.E6_GatewaySellChargeID, op, DBNull.Value);
			return result;
		}

		public void RaiseOnReplaceShipmentExchangeRate(object sender, ReplaceShipmentExchangeRateEventArgs args)
		{
			if (OnReplaceShipmentExchangeRate != null)
			{
				OnReplaceShipmentExchangeRate(sender, args);
			}
		}

		#region OnReplaceShipmentExchangeRate

		public bool IsReplaceShipmentExchangeRateHooked
		{
			get { return OnReplaceShipmentExchangeRate != null; }
		}

		public event EventHandler<ReplaceShipmentExchangeRateEventArgs> OnReplaceShipmentExchangeRate;

		public ZBool DoesReplaceShipmentExchangeRate()
		{
			ReplaceShipmentExchangeRateEventArgs eventArgs = new ReplaceShipmentExchangeRateEventArgs();
			RaiseOnReplaceShipmentExchangeRate(this, eventArgs);
			return eventArgs.DoesReplaceShipmentExchangeRate;
		}

		#endregion

		/// <summary>
		/// Use this method to create and add a new Consol Cost instead of generic AddNew() which will report a Developer Exception.
		/// This method checks if Consol Costs could be added for the Consol and returns null when it is not allowed.
		/// </summary>
		/// <returns>New JobConsolCost. Could return null if adding Consol Costs is not allowed (e.g. Gateway Consol).</returns>
		public JobConsolCost TryAddNew()
		{
			JobConsolCost result = null;
			var parent = GenericJobCostPlugIn as BusinessObject;

			if (parent != null &&
				(EnforceCreatingConsolCosts || !IsUsedForGateway))
			{
				parent.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					result = AddNew();
				}
				finally
				{
					parent.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
			}
			return result;
		}

		bool EnforceCreatingConsolCosts
		{
			get
			{
				var parent = GenericJobCostPlugIn as BusinessObject;
				return parent != null && parent.HasContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		FunctionalitySuspender BypassReportSettingParentSuspender
		{
			get { return bypassReportSettingParentSuspender ?? (bypassReportSettingParentSuspender = new FunctionalitySuspender());  }
		}
		FunctionalitySuspender bypassReportSettingParentSuspender;

		protected override BusinessObject AddNewCore()
		{
			if (EnforceCreatingConsolCosts)
			{
				return base.AddNewCore();
			}
			else
			{
				using (BypassReportSettingParentSuspender.GetSuspender())
				{
					return base.AddNewCore();
				}
			}
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			if (EnforceCreatingConsolCosts)
			{
				return base.AddNewCore(bizOType);
			}
			else
			{
				using (BypassReportSettingParentSuspender.GetSuspender())
				{
					return base.AddNewCore(bizOType);
				}
			}
		}
	}

	public class ReplaceShipmentExchangeRateEventArgs : EventArgs
	{
		ZBool fDoesReplaceShipmentExchangeRate;

		public ZBool DoesReplaceShipmentExchangeRate
		{
			get { return fDoesReplaceShipmentExchangeRate; }
			set { fDoesReplaceShipmentExchangeRate = value; }
		}
	}
}
