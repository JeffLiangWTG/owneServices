using System;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class EMCSInvoiceLineControl : ZUserControl
	{
		public EMCSInvoiceLineControl()
		{
			InitializeComponent();
			AddTabPages();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var dataGroupingCode = ((EMCSJobDeclaration)DataSource).GetDefaultDataGroupingCode();
			var provider = EMCSLayoutProvider.GetLayoutProvider(dataGroupingCode);
			var panelLayoutWithGrid = provider.GetInvoiceLineDetailsPanelLayoutWithGrid(dataGroupingCode);
			SetInvoiceLineDetailsGrid();
			SetInvoiceLineDetailsLayout();

			void SetInvoiceLineDetailsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				SplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetInvoiceLineDetailsLayout()
			{
				ClassificationDetailsDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}

		void AddTabPages()
		{
			var emcsInvoiceLineArrivalInformationUserControl = new EMCSInvoiceLineArrivalInformationUserControl();
			BindingSource.SetBindingMember(emcsInvoiceLineArrivalInformationUserControl, ".");
			ArrivalInformationTabPage.Controls.Add(emcsInvoiceLineArrivalInformationUserControl);
		}
	}
}
