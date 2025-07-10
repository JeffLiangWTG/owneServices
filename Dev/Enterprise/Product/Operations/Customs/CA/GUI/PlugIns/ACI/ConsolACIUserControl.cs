using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ConsolACIUserControl : ZUserControl
	{
		public ConsolACIUserControl()
		{
			InitializeComponent();
		}

		CusSCAOceanBill OceanBill
		{
			get { return DataSource as CusSCAOceanBill; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var oceanBill = OceanBill;
			if (oceanBill != null)
			{
				foreach (var houseBill in oceanBill.HouseBills)
				{
					houseBill.CA_OverrideFreightDefaultsInfo.ValueChanged -= OnCA_OverrideFreightDefaultsInfo_Changed;
					houseBill.CA_OverrideFreightDefaultsInfo.ValueChanged += OnCA_OverrideFreightDefaultsInfo_Changed;
					OnCA_OverrideFreightDefaultsInfo_Changed(null, null);
				}
			}
		}

		void OnCA_OverrideFreightDefaultsInfo_Changed(object sender, EventArgs e)
		{
			var oceanBill = OceanBill;
			if (oceanBill != null)
			{
				this.containersGrid.SetReadOnly(oceanBill.ShouldSyncroniseWithConsol);
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			var oceanBill = OceanBill;
			if (oceanBill != null)
			{
				foreach (var houseBill in oceanBill.HouseBills)
				{
					houseBill.CA_OverrideFreightDefaultsInfo.ValueChanged -= OnCA_OverrideFreightDefaultsInfo_Changed;
				}
			}
			base.Dispose(disposing);
		}
	}
}
