using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class ShipmentCargoReportUserControl : ZUserControl
	{
		public ShipmentCargoReportUserControl()
		{
			InitializeComponent();

			packingUserControl.GroupBoxContainers.CaptionResourceString = Res.GetData("ShipmentCargoReportUserControl|4ce7483a-3b6b-4715-9f14-4fc532c744ea", "Packing");
			packingUserControl.PackingGrid.GetColumnStyle(CusSCAPivot.Schema.CV_PackageType).CaptionResourceString = Res.GetData("ShipmentCargoReportUserControl|2235803b-3177-4976-be21-ac0b7475e8da", "UQ");
			packingUserControl.PackingGrid.GetColumnStyle(CusSCAPivot.Schema.CV_WeightUQ).CaptionResourceString = Res.GetData("ShipmentCargoReportUserControl|30a0f408-009b-4752-8578-722a66416e1b", "UQ");
			packingUserControl.PackingDetailsGroupBox.CaptionResourceString = Res.GetData("ShipmentCargoReportUserControl|5e5c7c37-5b39-4369-9a72-a580f6f965dd", "Packing Details");
			messagesPlugInUserControl.HistoryGroupBox.CaptionResourceString = Res.GetData("ShipmentCargoReportUserControl|44a7e067-4bb9-4557-b134-d2bdd67b44bf", "History");
		}

		CusSCAHouse HouseBill
		{
			get { return DataSource as CusSCAHouse; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var houseBill = HouseBill;
			if (houseBill != null)
			{
				houseBill.CA_OverrideFreightDefaultsInfo.ValueChanged -= OnCA_OverrideFreightDefaultsInfo_Changed;
				houseBill.CA_OverrideFreightDefaultsInfo.ValueChanged += OnCA_OverrideFreightDefaultsInfo_Changed;
				OnCA_OverrideFreightDefaultsInfo_Changed(null, null);
				containersGroupBox.Visible = houseBill.OceanBill != null;
			}
		}

		void OnCA_OverrideFreightDefaultsInfo_Changed(object sender, EventArgs e)
		{
			var houseBill = HouseBill;
			if (houseBill != null)
			{
				var oceanBill = houseBill.OceanBill;
				if (oceanBill != null)
				{
					containersGrid.SetReadOnly(oceanBill.ShouldSyncroniseWithConsol);
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			var cusSCAHouse = dataSource == null ? null : dataSource as CusSCAHouse;
			this.OceanBillDetailsTabPage.Text = cusSCAHouse != null && cusSCAHouse.OceanBill != null && cusSCAHouse.OceanBill.IsSea ? Res.GetString("cc8ca0e9-f2e8-4c04-910c-8a54337db731", "Ocean Bill") : Res.GetString("ed1b34cf-0a9f-4ccd-8a1a-4738d87d780f", "Master Bill");
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
			var houseBill = HouseBill;
			if (houseBill != null)
			{
				houseBill.CA_OverrideFreightDefaultsInfo.ValueChanged -= OnCA_OverrideFreightDefaultsInfo_Changed;
			}
			base.Dispose(disposing);
		}
	}
}
