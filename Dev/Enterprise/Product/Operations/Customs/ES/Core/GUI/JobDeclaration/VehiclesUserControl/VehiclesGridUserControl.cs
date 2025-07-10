using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class VehiclesGridUserControl : ZUserControl
{
	public VehiclesGridUserControl()
	{
		InitializeComponent();
	}

	protected new JobComInvoiceLine DataSource => (JobComInvoiceLine)base.DataSource;

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		UpdateGridColumnLayout();
	}

	void UpdateGridColumnLayout()
	{
		VehiclesGrid.ApplyGridColumnLayout(GetGridColumnLayoutProvider());
	}

	IGridColumnLayoutProvider GetGridColumnLayoutProvider() => gridColumnLayoutProvider ??= new VehiclesGridColumnLayout();
	IGridColumnLayoutProvider gridColumnLayoutProvider;
}
