using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public partial class ReportsTabUserControl : ZUserControl, IReportsGridUserControlProvider
	{
		public ReportsTabUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var countryCode = ((CusExitHeader)DataSource).CountryCode;
			layoutProvider = ExitControlLayoutProvider.GetLayoutProvider(countryCode);
			menuProvider = ExitControlMenuProviderManager.GetMenuProvider(countryCode);

			InitializeAdditionalTabs();
			InitializeRemovableTabs();
			ReportTabControl.Controls.Add(MessagesTabPage);

			SetReportsGridUserControl();
			SetReportItemsGridUserControl();
			SetReportsMessagesUserControl();
		}
		IExitControlLayoutProvider layoutProvider;
		IExitControlMenuProvider menuProvider;
		IReportItemsUserControl reportItemsUserControl;
		BaseMessagesTabUserControl reportsMessagesTabUserControl;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookCurrentExitReportEvents();
				if (ReportsGrid != null)
				{
					var listManager = ReportsGrid.ListManager;
					if (listManager != null)
					{
						listManager.CurrentChanged -= ListManager_CurrentChanged;
					}

					ReportsGrid.AfterBind -= ReportsGrid_AfterBind;
				}
				if (components != null)
				{
					components.Dispose();
				}
				if (MessagesTabPage != null)
				{
					MessagesTabPage.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		void InitializeAdditionalTabs()
		{
			var additionalTabPageIndex = 1;
			foreach (var tabPageDetail in layoutProvider.AdditionalReportTabPages)
			{
				var tabPage = new ZTabPage();
				var userControl = tabPageDetail.CreateUserControl();

				ReportTabControl.Controls.Add(tabPage);

				tabPage.CaptionResourceString = tabPageDetail.Caption;
				tabPage.Controls.Add(userControl);
				tabPage.Name = $"AdditionalReportTabPage{additionalTabPageIndex}";

				BindingSource.SetBindingMember(userControl, tabPageDetail.UserControlBindingMember);
				userControl.Dock = DockStyle.Fill;
				userControl.Name = $"AdditionalReportTabUserControl{additionalTabPageIndex}";

				additionalTabPageIndex++;

				if (tabPageDetail.IsVisible != null)
				{
					tabPageVisibilityInfos.Value.Add(new TabPageVisibilityInfo<CusExitReport>(tabPage.Name, tabPageDetail.IsVisible));
				}
			}
		}

		void InitializeRemovableTabs()
		{
			foreach (var tabPageName in layoutProvider.RemovableReportTabPageNames)
			{
				var tab = ReportTabControl.GetTabPage(tabPageName);
				if (tab != null)
				{
					ReportTabControl.Controls.Remove(tab);
				}
			}
		}

		void SetReportsGridUserControl()
		{
			reportsGridUserControl = layoutProvider.CreateReportsGridUserControl();
			if (reportsGridUserControl != null)
			{
				if (menuProvider?.GetReportsGridUserControlMenuProvider(this) is IReportsGridUserControlMenuProvider reportsGridUserControlMenuProvider)
				{
					AddAdditionalReportsGridMenuItems(reportsGridUserControlMenuProvider);
					AddAdditionalReportsMainMenuItems(reportsGridUserControlMenuProvider);
				}
				ReportsGrid = reportsGridUserControl.ReportsGrid;
				ReportsGrid.AfterBind += ReportsGrid_AfterBind;
				var gridUserControl = (Control)reportsGridUserControl;
				ReportsSplitContainer.Panel1.Controls.Add(gridUserControl);
				BindingSource.SetBindingMember(gridUserControl, ".");
				gridUserControl.Dock = DockStyle.Fill;
			}
		}
		IReportsGridUserControl reportsGridUserControl;

		void AddAdditionalReportsGridMenuItems(IReportsGridUserControlMenuProvider reportsGridUserControlMenuProvider)
		{
			if (reportsGridUserControlMenuProvider.AdditionalReportsGridMenuItems is ZMenuItem[] additionalReportsGridMenuItems)
			{
				var menuItems = new List<ZMenuItem>();
				menuItems.Add(new ZMenuItem("-"));
				menuItems.AddRange(additionalReportsGridMenuItems);
				menuItems.Add(new ZMenuItem("-"));
				var contextMenuItems = reportsGridUserControl.ReportsGrid.ContextMenu.MenuItems;
				contextMenuItems.InsertRange(contextMenuItems.IndexOfKey(ZGrid.AvailableLayoutsContextMenuName), menuItems);
			}
		}

		void AddAdditionalReportsMainMenuItems(IReportsGridUserControlMenuProvider reportsGridUserControlMenuProvider)
		{
			if (this.GetExitControlMainMenuSupporter() is IExitControlMainMenuSupporter supporter && reportsGridUserControlMenuProvider.AdditionalMenuItemsForMainForm is ZMenuItem[] additionalMenuItemsForMainForm)
			{
				supporter.AddReportsGridMenuItems(additionalMenuItemsForMainForm);
			}
		}

		void SetReportItemsGridUserControl()
		{
			reportItemsUserControl = layoutProvider.CreateReportItemsUserControl();
			if (reportItemsUserControl is Control userControl)
			{
				ReportItemsTabPage.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = DockStyle.Fill;
			}
		}

		void SetReportsMessagesUserControl()
		{
			reportsMessagesTabUserControl = layoutProvider.CreateReportsMessagesUserControl();
			if (reportsMessagesTabUserControl is Control userControl)
			{
				MessagesTabPage.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, "Messages");
				userControl.Dock = DockStyle.Fill;
			}
		}

		void ReportsGrid_AfterBind(object sender, EventArgs e)
		{
			var listManager = ReportsGrid?.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged += ListManager_CurrentChanged;
				ListManager_CurrentChanged(null, null);
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			UnHookCurrentExitReportEvents();
			currentExitReport = (CusExitReport)ReportsGrid?.ListManager?.GetCurrent();
			HookCurrentExitReportEvents();
			CurrentExitReportChanged(null, null);
		}
		CusExitReport currentExitReport;

		void UnHookCurrentExitReportEvents()
		{
			if (currentExitReport != null)
			{
				currentExitReport.CER_TypeInfo.ValueChanged -= CurrentExitReportChanged;
			}
		}

		void HookCurrentExitReportEvents()
		{
			if (currentExitReport != null)
			{
				currentExitReport.CER_TypeInfo.ValueChanged += CurrentExitReportChanged;
			}
		}

		void CurrentExitReportChanged(object sender, EventArgs eventArgs)
		{
			var allTabPages = ReportTabControl.AllTabPages;
			foreach (var tabPageVisibilityInfo in tabPageVisibilityInfos.Value)
			{
				if (allTabPages.SingleOrDefault(t => t.Name == tabPageVisibilityInfo.TabPageName) is ZTabPage tabPage)
				{
					tabPage.TabVisible = currentExitReport != null && tabPageVisibilityInfo.IsVisible(currentExitReport);
				}
			}

			if (reportItemsUserControl != null)
			{
				reportItemsUserControl.OnExitReportChanged(currentExitReport);
			}
		}

		internal ZGrid ReportsGrid { get; set; }

		readonly Lazy<List<TabPageVisibilityInfo<CusExitReport>>> tabPageVisibilityInfos = new Lazy<List<TabPageVisibilityInfo<CusExitReport>>>();

		IReportsGridUserControl IReportsGridUserControlProvider.UserControl => reportsGridUserControl ?? (reportsGridUserControl = new ReportsGridUserControl());
	}
}
