using System;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public sealed partial class TransactionLinesGridUserControl : ZUserControl
	{
		public TransactionLinesGridUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			UpdateGridColumnLayout();
		}

		new CusIntrastatLine DataSource => base.DataSource as CusIntrastatLine;

		void UpdateGridColumnLayout()
		{
			TransactionLinesGrid.ApplyGridColumnLayout(LayoutProvider.TransactionLinesGridColumnLayout);
		}

		IIntrastatLayoutProvider LayoutProvider => layoutProvider ??= IntrastatLayoutProvider.GetLayoutProvider(DataSource?.Header.CountryCode);
		IIntrastatLayoutProvider layoutProvider;
	}
}
