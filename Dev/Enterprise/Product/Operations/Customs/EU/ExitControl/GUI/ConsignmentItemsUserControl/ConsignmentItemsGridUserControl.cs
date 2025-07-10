using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class ConsignmentItemsGridUserControl : ZUserControl
	{
		public ConsignmentItemsGridUserControl()
		{
			InitializeComponent();
		}

		CusExitHeader ExitHeader => base.DataSource as CusExitHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = ExitControlLayoutProvider.GetLayoutProvider(ExitHeader?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			SetUpItemsGridColumns(provider);
		}

		void SetUpItemsGridColumns(IExitControlLayoutProvider provider)
		{
			var layout = provider.ConsignmentItemsGridLayout;
			if (layout != null)
			{
				ItemsGrid.ApplyGridColumnLayout(layout);
			}
		}
	}
}
