using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public partial class ConsignmentsTabUserControl : ZUserControl, IConsignmentsGridUserControlProvider
	{
		public ConsignmentsTabUserControl()
		{
			InitializeComponent();
		}

		new CusExitHeader DataSource => (CusExitHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var countryCode = DataSource.CountryCode;
			layoutProvider = ExitControlLayoutProvider.GetLayoutProvider(countryCode);
			menuProvider = ExitControlMenuProviderManager.GetMenuProvider(countryCode);
			InitializeAdditionalTabs();
			InitializeRemovableTabs();
			SetConsignmentsGridPanel();
		}
		IExitControlLayoutProvider layoutProvider;
		IExitControlMenuProvider menuProvider;

		void InitializeAdditionalTabs()
		{
			var additionalTabPageIndex = 1;
			foreach (var tabPageDetail in layoutProvider.AdditionalConsignmentTabPages)
			{
				var tabPage = new ZTabPage();
				var userControl = tabPageDetail.CreateUserControl();

				ConsignmentTabControl.Controls.Add(tabPage);

				tabPage.CaptionResourceString = tabPageDetail.Caption;
				tabPage.Controls.Add(userControl);
				tabPage.Name = $"AdditionalConsignmentTabPage{additionalTabPageIndex}";

				BindingSource.SetBindingMember(userControl, tabPageDetail.UserControlBindingMember);
				userControl.Dock = DockStyle.Fill;
				userControl.Name = $"AdditionalConsignmentTabUserControl{additionalTabPageIndex}";

				additionalTabPageIndex++;
			}
		}

		void InitializeRemovableTabs()
		{
			foreach (var tabPageName in layoutProvider.RemovableConsignmentTabPageNames)
			{
				var tab = ConsignmentTabControl.FindSingleOrDefault<ZTabPage>(tabPageName);
				if (tab != null)
				{
					ConsignmentTabControl.Controls.Remove(tab);
				}
			}
		}

		void SetConsignmentsGridPanel()
		{
			consignmentsGridUserControl = layoutProvider.ConsignmentsGridUserControl;
			if (consignmentsGridUserControl != null)
			{
				if (menuProvider?.GetConsignmentsGridUserControlMenuProvider(this) is IConsignmentsGridUserControlMenuProvider consignmentsGridUserControlMenuProvider)
				{
					AddAdditionalConsignmentsGridMenuItems(consignmentsGridUserControlMenuProvider);
					AddAdditionalConsignmentsMainMenuItems(consignmentsGridUserControlMenuProvider);
				}
				var gridUserControl = (Control)consignmentsGridUserControl;
				ConsignmentsSplitContainer.Panel1.Controls.Add(gridUserControl);
				BindingSource.SetBindingMember(gridUserControl, ".");
				gridUserControl.Dock = DockStyle.Fill;
			}
		}
		IConsignmentsGridUserControl consignmentsGridUserControl;

		void AddAdditionalConsignmentsGridMenuItems(IConsignmentsGridUserControlMenuProvider consignmentsGridUserControlMenuProvider)
		{
			if (consignmentsGridUserControlMenuProvider.AdditionalConsignmentsGridMenuItems is ZMenuItem[] additionalConsignmentsGridMenuItems)
			{
				var menuItems = new List<ZMenuItem>();
				menuItems.Add(new ZMenuItem("-"));
				menuItems.AddRange(additionalConsignmentsGridMenuItems);
				menuItems.Add(new ZMenuItem("-"));
				var contextMenuItems = consignmentsGridUserControl.ConsignmentsGrid.ContextMenu.MenuItems;
				contextMenuItems.InsertRange(contextMenuItems.IndexOfKey(ZGrid.AvailableLayoutsContextMenuName), menuItems);
			}
		}

		void AddAdditionalConsignmentsMainMenuItems(IConsignmentsGridUserControlMenuProvider consignmentsGridUserControlMenuProvider)
		{
			if (this.GetExitControlMainMenuSupporter() is IExitControlMainMenuSupporter supporter && consignmentsGridUserControlMenuProvider.AdditionalMenuItemsForMainForm is ZMenuItem[] additionalMenuItemsForMainForm)
			{
				supporter.AddConsignmentsGridMenuItems(additionalMenuItemsForMainForm);
			}
		}

		IConsignmentsGridUserControl IConsignmentsGridUserControlProvider.UserControl => consignmentsGridUserControl ?? (consignmentsGridUserControl = new ConsignmentsGridUserControl());
		CusExitHeader IConsignmentsGridUserControlProvider.ExitHeader => DataSource;
	}
}
