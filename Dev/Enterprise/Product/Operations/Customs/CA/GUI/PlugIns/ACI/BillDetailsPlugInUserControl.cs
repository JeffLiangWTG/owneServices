using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class BillDetailsPlugInUserControl : ZUserControl
	{
		public BillDetailsPlugInUserControl()
		{
			InitializeComponent();
		}
		CusSCAHouse cusSCAHouse;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			cusSCAHouse = dataSource == null ? null : dataSource as CusSCAHouse;
			if (cusSCAHouse != null)
			{
				cusSCAHouse.OnOverrideFreightDefaultsChanging += new CancelEventHandler(CusSCAHouse_OnOverrideFreightDefaultsChanging);
			}
			SetStatusColor();
		}

		void CusSCAHouse_OnOverrideFreightDefaultsChanging(object sender, CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("5B674192-6996-41E2-9201-4879E8EFD81B", "Removing the override will reset ACI data.\r\nYou may lose changes that you have made.\r\n\r\nDo you wish to proceed?"), Res.GetString("F416AC3A-228B-4457-A373-A7554685B687", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		void SetStatusColor()
		{
			var jobStatus = cusSCAHouse == null ? ZString.Empty : ((IEDIFACTMessageAttachee)cusSCAHouse).JobStatus;
			var statusCalculator = new SupplementaryCargoReportStatusCalculator();
			if (statusCalculator.IsClear(jobStatus))
			{
				ShipmentStatusDescriptionZTextBox.ColorChanger.ForceBackColor(Color.LightGreen);
			}
			else if (statusCalculator.IsRAOutstanding(jobStatus))
			{
				ShipmentStatusDescriptionZTextBox.ColorChanger.ForceBackColor(Color.LightSalmon);
			}
			else if (statusCalculator.IsInError(jobStatus))
			{
				ShipmentStatusDescriptionZTextBox.ColorChanger.ForceBackColor(Color.Red);
			}
			else
			{
				ShipmentStatusDescriptionZTextBox.ColorChanger.ForceBackColor(SystemColors.Control);
			}
		}

		void ShipmentStatusDescriptionZTextBox_TextChanged(object sender, EventArgs e)
		{
			SetStatusColor();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (cusSCAHouse != null)
				{
					cusSCAHouse.OnOverrideFreightDefaultsChanging -= new CancelEventHandler(CusSCAHouse_OnOverrideFreightDefaultsChanging);
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
