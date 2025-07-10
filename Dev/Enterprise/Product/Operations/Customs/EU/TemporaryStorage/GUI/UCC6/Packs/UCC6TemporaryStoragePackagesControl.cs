using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePackagesControl : ZUserControl
	{
		public UCC6TemporaryStoragePackagesControl()
		{
			InitializeComponent();
			PackDetailsTabPage.RunWhenBindingOrFirstShown((s, args) => InitializePackGridAndPackDetailTabPage());
			SplitContainer.Panel1.AllowOutsideOfParent();
			SplitContainer.Panel2.AllowOutsideOfParent();
		}

		protected new TemporaryStorageHeader DataSource => (TemporaryStorageHeader)base.DataSource;

		ITemporaryStorageLayoutProvider fLayoutProvider;
		ITemporaryStorageLayoutProvider LayoutProvider => fLayoutProvider ?? (fLayoutProvider = TemporaryStorageLayoutProviderHelper.GetLayoutProvider(DataSource));

		void InitializePackGridAndPackDetailTabPage()
		{
			var panelLayoutWithGrid = LayoutProvider?.GetTemporaryStoragePackagesWithGridLayout();
			PackDetailsLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			var packGridControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
			SplitContainer.Panel1.Controls.Add(packGridControl);
			BindingSource.SetBindingMember(packGridControl, ".");
			packGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}
	}
}
