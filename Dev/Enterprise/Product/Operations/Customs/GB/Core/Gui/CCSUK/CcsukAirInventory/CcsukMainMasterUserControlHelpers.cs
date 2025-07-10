using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukMainMasterUserControlHelpers : ZUserControl
	{
		public CcsukMainMasterUserControlHelpers()
		{
			InitializeComponent();
			helper = new DependentObjectControllerHelper(new DependentFormPresenter());
		}

		protected override void OnLoad(System.EventArgs e)
		{
			// Is there a better way?
			base.OnLoad(e);
			if (BindingSource.DataSource != null)
			{
				var basicOrMawbBindingSource = BindingSource.DataSource as CusMAWB;
				if (basicOrMawbBindingSource != null)
				{
					SetCusMawb(basicOrMawbBindingSource);
				}
			}
			else
			{
				BindingSource.DataSourceChanged -= new System.EventHandler(BindingSource_DataSourceChanged);
				BindingSource.DataSourceChanged += new System.EventHandler(BindingSource_DataSourceChanged);
			}
		}

		void BindingSource_DataSourceChanged(object sender, System.EventArgs e)
		{
			if (BindingSource.DataSource as CusMAWB != null)
			{
				SetCusMawb((CusMAWB)BindingSource.DataSource);
			}
		}

		internal void SetCusMawb(CusMAWB basicOrMawbBindingSource)
		{
			if (basicOrMawbBindingSource.IsBasic)
			{
				RemovalsAndFallbackTabPage.TabVisible = basicOrMawbBindingSource.IsInDatabase;
				DeliveryTabPage.TabVisible = basicOrMawbBindingSource.IsProfileAShed && basicOrMawbBindingSource.IsInDatabase;
				UpdateSplitsAndHousesTabsVisibility(basicOrMawbBindingSource);
			}
			else
			{
				RemovalsAndFallbackTabPage.TabVisible = false;
				SplitsTabPage.TabVisible = false;
				DeliveryTabPage.TabVisible = false;
			}

			if (Env.Security.AirCcsukHouse.IsAllowed) // TODO - change to hawb right
			{
				var manager = new CusAwbDelegateProvider(delegate
				{ return ChildrenGrid.SelectedElements.Length == 1 ? (CusHAWB)ChildrenGrid.SelectedElements[0] : null; });
				var ccsukMessagingMenu = new CcsukMenu(manager, (ZForm)ParentForm, false, true);
				ChildrenGrid.ContextMenu.MenuItems.Add(0, ccsukMessagingMenu);
			}

			basicOrMawbBindingSource.OnPimaChanged += HandleOnPimaChanged;
			basicOrMawbBindingSource.OnSplitsCountChanged += UpdateSplitsAndHousesTabsVisibility;
			underbondUserControl1.ToggleRemovalTabVisibility(basicOrMawbBindingSource);
		}

		void UpdateSplitsAndHousesTabsVisibility(ICcsukCusAwb awb)
		{
			var basicOrMawb = awb as CusMAWB;
			if (basicOrMawb != null)
			{
				SplitsTabPage.TabVisible = basicOrMawb.HasSplits;
				HousesTabPage.TabVisible = !(!basicOrMawb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_AddChildAwb) || basicOrMawb.MasterLevelHouseHelper.HasDeclaration);
			}
		}

		void HandleOnPimaChanged(ICcsukCusAwb awb)
		{
			DeliveryTabPage.TabVisible = awb.IsInDatabase && (LicenceAndPimaHelper.IsFullShed(awb) || LicenceAndPimaHelper.IsFallbackShed(awb));
			underbondUserControl1.ToggleRemovalTabVisibility(awb);
		}

		void ChildrenGrid_DoubleClick(object sender, System.EventArgs e)
		{
			DoEdit();
		}

		void DoEdit()
		{
			if (ChildrenGrid.SelectedElements.Length == 1)
			{
				if (Env.Security.AirCcsukHouse.IsAllowed)
				{
					helper.EditExisting(ChildrenGrid.SelectedElements[0]);
				}
				else
				{
					Globals.Message.ShowError(Env.Security.AirCcsukHouse.ErrorMessageForNotAllowed);
				}
			}
			else
			{
				Globals.Message.ShowInformation("Select a single bill first");
			}
		}

		readonly DependentObjectControllerHelper helper;
	}
}
