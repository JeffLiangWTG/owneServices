using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class HouseConsignmentDifferencesTabUserControl : ZUserControl
	{
		public HouseConsignmentDifferencesTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = provider.HouseConsignmentDifferencesLayout;
			SetupGrid();
			SetupLayout();
			HouseDetailsDynamicLayoutPanel.FindSingle<ArrivalTransportInfosUserControl>("ArrivalTransportInfosUserControl").AllowOverlap(HouseDetailsDynamicLayoutPanel.FindSingle<ZPanel>("ColumnSeparator0"));

			void SetupGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				HouseConsignmentDifferencesSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = DockStyle.Fill;
			}

			void SetupLayout()
			{
				HouseDetailsDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
				BindingSource.SetBindingMember(HouseDetailsDynamicLayoutPanel, ".");
			}
		}
	}
}
