using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class ReportAdditionalDocumentsGridUserControl : ZUserControl
	{
		public ReportAdditionalDocumentsGridUserControl()
		{
			InitializeComponent();
		}

		new CusExitHeader DataSource => (CusExitHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var provider = ExitControlLayoutProvider.GetLayoutProvider(DataSource?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var layout = provider?.ReportAdditionalDocumentsGridLayout;
			if (layout != null)
			{
				AdditionalDocumentsGrid.ApplyGridColumnLayout(provider.ReportAdditionalDocumentsGridLayout);
			}
		}
	}
}
