using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public sealed partial class TransactionLineDetailsTabUserControl : ZUserControl
	{
		public TransactionLineDetailsTabUserControl()
		{
			InitializeComponent();
		}

		new CusIntrastatHeader DataSource => (CusIntrastatHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			SetDetailsWithGridLayout();
		}

		void SetDetailsWithGridLayout()
		{
			var layout = LayoutProvider.TransactionLineDetailsWithGridLayout;
			DynamicTransactionLineDetailsPanel.UpdateLayout(layout);

			var grid = (ZUserControl)Activator.CreateInstance(layout.GridUserControlType);
			TransactionLinesSplitContainer.Panel1.Controls.Add(grid);
			BindingSource.SetBindingMember(grid, ".");
			grid.Dock = DockStyle.Fill;
		}

		IIntrastatLayoutProvider LayoutProvider => layoutProvider ??= IntrastatLayoutProvider.GetLayoutProvider(DataSource?.CountryCode);
		IIntrastatLayoutProvider layoutProvider;
	}
}
