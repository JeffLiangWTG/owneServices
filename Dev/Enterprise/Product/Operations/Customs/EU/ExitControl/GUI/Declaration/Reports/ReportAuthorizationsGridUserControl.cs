using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public sealed partial class ReportAuthorizationsGridUserControl : ZUserControl
{
	public ReportAuthorizationsGridUserControl()
	{
		InitializeComponent();
	}

	new CusExitHeader DataSource => (CusExitHeader)base.DataSource;

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		if (ExitControlLayoutProvider.GetLayoutProvider(DataSource?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode) is { } provider
			&& provider.AuthorizationGridColumnLayout is { } layout)
		{
			AuthorizationsGrid.ApplyGridColumnLayout(layout);
		}
	}
}
