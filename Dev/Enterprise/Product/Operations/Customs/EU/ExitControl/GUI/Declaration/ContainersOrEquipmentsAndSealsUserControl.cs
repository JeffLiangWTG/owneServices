using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class ContainersOrEquipmentsAndSealsUserControl : ZUserControl
	{
		public ContainersOrEquipmentsAndSealsUserControl()
		{
			InitializeComponent();
		}

		CusExitHeader ExitHeader => (CusExitHeader)DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = ExitControlLayoutProvider.GetLayoutProvider(ExitHeader?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			SetUpContainersOrEquipmentsGridColumns(provider);
			SetUpSealsGridColumns(provider);
		}

		void SetUpContainersOrEquipmentsGridColumns(IExitControlLayoutProvider provider)
		{
			var layout = provider.ContainersOrEquipmentsGridLayout;
			if (layout != null)
			{
				ContainersOrEquipmentsGrid.ApplyGridColumnLayout(layout);
			}
		}

		void SetUpSealsGridColumns(IExitControlLayoutProvider provider)
		{
			var layout = provider.SealsGridLayout;
			if (layout != null)
			{
				SealsGrid.ApplyGridColumnLayout(layout);
			}
		}
	}
}
