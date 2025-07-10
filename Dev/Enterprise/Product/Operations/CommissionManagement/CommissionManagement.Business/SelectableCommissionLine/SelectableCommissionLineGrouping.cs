using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.CommissionManagement.Business
{
	public abstract class SelectableCommissionLineGrouping<TGrouping, TLine> : CommissionLineGrouping<TGrouping, TLine>
		where TGrouping : SelectableCommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, ISelectableViewCommissionLineProvider
	{
		#region Schema

		public new class Schema : AutoCommissionLineGrouping.Schema
		{
			public const string IsSelected = "IsSelected";
			public const string EntitySelectedAmount = "EntitySelectedAmount";
		}

		#endregion

		#region Constructors

		protected SelectableCommissionLineGrouping(BusinessObjectFactory factory, ViewCommissionLineGrouper<TLine>[] subGroupers)
			: base(factory, subGroupers)
		{
		}

		#endregion

		#region Property

		#region IsSelected

		[ResourceStringData("CommissionFinalizerLineItemGrouping|IsSelected", Caption = "Selected")]
		public ZBool IsSelected
		{
			get { return isSelected; }
			set
			{
				if (isSelected != value)
				{
					SetNonPersistentPropertyValue(IsSelectedInfo, ref isSelected, value);
					Validation.ValidateIsSelected();

					if (!refreshingIsSelected)
					{
						if (IsLeaf)
						{
							foreach (var lineItem in CommissionLineProviders)
							{
								lineItem.IsSelected = value;
							}
						}
						else
						{
							foreach (var subGrouping in SubGroupings)
							{
								subGrouping.IsSelected = value;
							}
						}
					}
				}
			}
		}
		ZBool isSelected;

		public ZPropertyInfo IsSelectedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSelected); }
		}

		#endregion

		#region EntitySelectedAmount

		[ResourceStringData("SelectableCommissionLineGrouping|EntitySelectedAmount", Caption = "Selected Amount")]
		public ZDecimal EntitySelectedAmount
		{
			get
			{
				return IsLeaf ?
					CommissionLineProviders.Where(x => x.IsSelected).Sum(x => x.ViewCommissionLine.VCL_EntityCommissionAmountInPreferredCurrency) :
					SubGroupings.Sum(x => x.EntitySelectedAmount);
			}
		}

		public ZPropertyInfo EntitySelectedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.EntitySelectedAmount); }
		}

		#endregion

		#endregion

		#region Event Handlers

		protected override void HookSubGroupingEventHandlers()
		{
			base.HookSubGroupingEventHandlers();

			RefreshIsSelected();

			foreach (var subGrouping in SubGroupings)
			{
				subGrouping.IsSelectedInfo.ValueChanged += IsSelectedInfo_ValueChanged;
				subGrouping.EntitySelectedAmountInfo.ValueChanged += EntitySelectedAmountInfo_ValueChanged;
			}
		}

		protected override void HookCommissionLineProviderEventHandlers()
		{
			base.HookCommissionLineProviderEventHandlers();

			RefreshIsSelected();

			foreach (var commissionLineProvider in CommissionLineProviders)
			{
				commissionLineProvider.IsSelectedInfo.ValueChanged += IsSelectedInfo_ValueChanged;
			}
		}

		void IsSelectedInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshIsSelected();
			EntitySelectedAmountInfo.RefreshBinding();
		}

		void EntitySelectedAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			EntitySelectedAmountInfo.RefreshBinding();
		}

		void RefreshIsSelected()
		{
			refreshingIsSelected = true;
			try
			{
				if (IsLeaf)
				{
					IsSelected = CommissionLineProviders.Any(x => x.IsSelected);
				}
				else
				{
					IsSelected = SubGroupings.Any(x => x.IsSelected);
				}
			}
			finally
			{
				refreshingIsSelected = false;
			}
		}
		bool refreshingIsSelected;

		#endregion

		public new SelectableCommissionLineGroupingValidation<TGrouping, TLine> Validation
		{
			get { return (SelectableCommissionLineGroupingValidation<TGrouping, TLine>)base.Validation; }
		}

		protected override CommissionLineGroupingValidation GetNewValidation()
		{
			return new SelectableCommissionLineGroupingValidation<TGrouping, TLine>(this);
		}
	}
}
