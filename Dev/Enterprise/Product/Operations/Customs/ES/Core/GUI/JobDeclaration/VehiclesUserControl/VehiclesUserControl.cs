using System;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class VehiclesUserControl : ZUserControl
{
	public VehiclesUserControl()
	{
		InitializeComponent();

		if (!DesignModeFinder.IsDesigning)
		{
			AddDynamicLayoutUserControl();
		}
	}

	protected new JobComInvoiceLine DataSource => (JobComInvoiceLine)base.DataSource;

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		AddVehiclesGridUserControl();
	}

	void AddDynamicLayoutUserControl()
	{
		DynamicVehicleDetailsPanel.UpdateLayout(VehicleDetailsPanelLayout);
	}

	void AddVehiclesGridUserControl()
	{
		var gridControl = (ZUserControl)Activator.CreateInstance(VehicleDetailsPanelLayout.GridUserControlType);
		VehiclesSplitContainer.Panel1.Controls.Add(gridControl);
		BindingSource.SetBindingMember(gridControl, ".");
		gridControl.Dock = DockStyle.Fill;
	}

	IPanelLayoutWithGridProvider VehicleDetailsPanelLayout => vehicleDetailsPanelLayout ??= new VehicleDetailsLayoutWithGrid();
	IPanelLayoutWithGridProvider vehicleDetailsPanelLayout;
}
