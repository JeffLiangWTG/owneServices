using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class ConsignmentItemPackingDetailsUserControl : ZUserControl
	{
		public ConsignmentItemPackingDetailsUserControl()
		{
			InitializeComponent();
		}

		CusExitHeader ExitHeader => base.DataSource as CusExitHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = ExitControlLayoutProvider.GetLayoutProvider(ExitHeader?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			SetUpPackingDetailsGridColumns(provider);
		}

		void SetUpPackingDetailsGridColumns(IExitControlLayoutProvider provider)
		{
			var layout = provider.ConsignmentItemPackingDetailsGridLayout;
			if (layout != null)
			{
				PackingDetailsGrid.ApplyGridColumnLayout(layout);
			}
		}
	}
}
