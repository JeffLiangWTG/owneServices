using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public partial class ExitControlUserControl : ZUserControl, IExitControlMainMenuSupporter
	{
		public ExitControlUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			InitializeAdditionalTabs();
			InitializeRemovableTabs();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			layoutProvider = null;
			AddOrUpdateContainersOrEquipmentsAndSealsUserControl();
		}
		CusExitHeader ExitHeader => (CusExitHeader)CurrentDataItem;

		void InitializeAdditionalTabs()
		{
			var additionalTabPageIndex = 1;
			foreach (var tabPageDetail in LayoutProvider.AdditionalDeclarationTabPages)
			{
				var tabPage = new ZTabPage();
				var userControl = tabPageDetail.CreateUserControl();

				ExitControlTabControl.Controls.Add(tabPage);

				tabPage.CaptionResourceString = tabPageDetail.Caption;
				tabPage.Controls.Add(userControl);
				tabPage.Name = $"AdditionalDecalrationTabPage{additionalTabPageIndex}";

				BindingSource.SetBindingMember(userControl, tabPageDetail.UserControlBindingMember);
				userControl.Dock = DockStyle.Fill;
				userControl.Name = $"AdditionalDecalrationTabUserControl{additionalTabPageIndex}";

				additionalTabPageIndex++;
			}
		}

		void InitializeRemovableTabs()
		{
			foreach (var tabPageName in LayoutProvider.RemovableDeclarationTabPageNames)
			{
				var tab = ExitControlTabControl.FindSingleOrDefault<ZTabPage>(tabPageName);
				if (tab != null)
				{
					ExitControlTabControl.Controls.Remove(tab);
				}
			}
		}

		void AddOrUpdateContainersOrEquipmentsAndSealsUserControl()
		{
			if (containersOrEquipmentsAndSealsUserControl != null)
			{
				ContainersOrEquipmentsTabPage.Controls.Add(containersOrEquipmentsAndSealsUserControl);
				containersOrEquipmentsAndSealsUserControl.Dispose();
				containersOrEquipmentsAndSealsUserControl = null;
			}
			containersOrEquipmentsAndSealsUserControl = (ZUserControl)Activator.CreateInstance(LayoutProvider.ContainersOrEquipmentsAndSealsUserControlType);
			ContainersOrEquipmentsTabPage.Controls.Add(containersOrEquipmentsAndSealsUserControl);
			BindingSource.SetBindingMember(containersOrEquipmentsAndSealsUserControl, ".");
			containersOrEquipmentsAndSealsUserControl.Dock = DockStyle.Fill;
		}
		ZUserControl containersOrEquipmentsAndSealsUserControl;

		IExitControlLayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = ExitControlLayoutProvider.GetLayoutProvider(ExitHeader?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		IExitControlLayoutProvider layoutProvider;

		void IExitControlMainMenuSupporter.AddConsignmentsGridMenuItems(ZMenuItem[] menuItems)
		{
			exitControlMenuItem?.InsertBeforeSendToCustomsMenuItem(menuItems);
		}

		void IExitControlMainMenuSupporter.AddReportsGridMenuItems(ZMenuItem[] menuItems)
		{
			exitControlMenuItem?.InsertBeforeSendToCustomsMenuItem(menuItems);
		}

		internal void SetExitControlMenuItem(ExitControlMenuItem exitControlMenuItem)
		{
			this.exitControlMenuItem = exitControlMenuItem;
		}
		ExitControlMenuItem exitControlMenuItem;
	}
}
