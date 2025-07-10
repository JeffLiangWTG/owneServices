using System;
using System.Drawing;
using Enterprise.BarcodeParsing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BarcodeParsing.GUI
{
	public partial class BarcodeRuleComponentsUserControl : ZUserControl
	{
		public BarcodeRuleComponentsUserControl()
		{
			InitializeComponent();
			RuleComponentsGrid.AllowSorting = false;
		}

		#region Binding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();
			base.OnCurrentDataItemChanged(e);
			HookEvents();
		}

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			if (Rule != null)
			{
				Rule.BRU_IsDelimiterMultiComponentInfo.ValueChanged += BRU_IsDelimiterMultiComponentInfo_ValueChanged;
			}

			if (RuleComponentsGrid != null)
			{
				RuleComponentsGrid.ColourDeciding += RuleComponentsGrid_ColourDeciding;
			}
		}

		void UnhookEvents()
		{
			if (Rule != null)
			{
				Rule.BRU_IsDelimiterMultiComponentInfo.ValueChanged -= BRU_IsDelimiterMultiComponentInfo_ValueChanged;
			}

			if (RuleComponentsGrid != null)
			{
				RuleComponentsGrid.ColourDeciding -= RuleComponentsGrid_ColourDeciding;
			}
		}

		#endregion

		#region Actions

		#region BRU_IsDelimiterMultiComponentInfo_ValueChanged

		void BRU_IsDelimiterMultiComponentInfo_ValueChanged(object sender, EventArgs e)
		{
			RuleComponentsGrid.RefreshTableStyles();
		}

		#endregion

		#region RuleComponentsGrid_ColourDeciding

		void RuleComponentsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var ruleComponent = e.ObjectAtRow as BarcodeRuleComponent;
			if (ruleComponent != null && ruleComponent.IsGroupedWithMultiComponentDelimiter)
			{
				e.Colour = Color.LightBlue;
			}
		}

		#endregion

		#endregion

		#region Rule

		BarcodeRule Rule
		{
			get { return (BarcodeRule)CurrentDataItem; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
