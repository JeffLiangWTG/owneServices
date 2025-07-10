using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class AsycudaManifestMainControl : ZUserControl
	{
		public AsycudaManifestMainControl()
		{
			InitializeComponent();
		}

		ManifestHeadersWrapper HeadersWrapper => CurrentDataItem as ManifestHeadersWrapper;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var consol = HeadersWrapper?.Consol;
			if (consol != null)
			{
				consol.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}

			base.SetDataBinding(dataSource, dataMember);
			var headersWrapper = this.HeadersWrapper;
			consol = headersWrapper?.Consol;
			if (consol != null)
			{
				var factory = consol.Factory;
				factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}

			headersWrapper?.SynchroniseHeaders();
			LoadCountryTabs();
		}

		public void SelectAndShowBill(ZGuid billPk)
		{
			foreach (var header in HeadersWrapper.Headers.Cast<AsycudaManifestHeader>())
			{
				var bill = header.Bills.FindByPK(billPk);
				if (bill != null)
				{
					var countryCode = header.AMA_RN_NKCountry;
					var manifestType = header.AMA_ManifestType;
					var tabPageName = GetTabPageName(countryCode, manifestType);
					var childHeaderTab = MainTabControl.GetTabPage(tabPageName);
					if (childHeaderTab != null)
					{
						MainTabControl.SelectedTab = childHeaderTab;
						childHeaderTab.Controls.OfType<AsycudaManifestUserControl>().FirstOrDefault()?.SelectAndShowBill(billPk);
					}
					break;
				}
			}
		}

		public void SelectTabByCountry(ZString countryCode, ZString manifestType)
		{
			if (HeadersWrapper?.Headers.GetHeader(countryCode, manifestType) != null)
			{
				var tabPageName = GetTabPageName(countryCode, manifestType);
				MainTabControl.SelectedTab = MainTabControl.GetTabPage(tabPageName);
			}
		}

		protected override void Dispose(bool disposing)
		{
			ForwardingConsol consolProcessingAfterAllEventsUnhooked = null;
			if (disposing)
			{
				if (HeadersWrapper is ManifestHeadersWrapper headerWrapper)
				{
					foreach (AsycudaManifestHeader header in headerWrapper.Headers)
					{
						header.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
						header.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
						header.AMA_RN_NKCountryInfo.ValueChanged -= AMA_RN_NKCountryInfo_ValueChanged;
					}

					consolProcessingAfterAllEventsUnhooked = headerWrapper.Consol;
					if (consolProcessingAfterAllEventsUnhooked != null)
					{
						consolProcessingAfterAllEventsUnhooked.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
					}

					headerWrapper.UnloadHeaders();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);

			if (consolProcessingAfterAllEventsUnhooked != null)
			{
				consolProcessingAfterAllEventsUnhooked.Factory.UnlockAllChildrenOnParent(MutexIDs.AsycudaManJobBeingCreated, consolProcessingAfterAllEventsUnhooked.PK);
			}
		}

		public void LoadCountryTabs()
		{
			if (HeadersWrapper is ManifestHeadersWrapper headersWrapper)
			{
				const string manifestManagementTabPageName = "ManifestManagementTabPage";
				var tabIndex = 0;

				if (!headersWrapper.IsManifestConsolDecouplingEnabled)
				{
					var headers = headersWrapper.Headers.Cast<AsycudaManifestHeader>().OrderBy(c => c.CountryName).ThenBy(c => c.AMA_ManifestTypeDescription);
					foreach (var header in headers)
					{
						var countryCode = header.AMA_RN_NKCountry;
						var manifestType = header.AMA_ManifestType;
						var tabPageName = GetTabPageName(countryCode, manifestType);
						var childHeaderTab = MainTabControl.GetTabPage(tabPageName);

						if (!header.IsDeleted)
						{
							if (childHeaderTab == null)
							{
								childHeaderTab = new ZTabPage
								{
									Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true),
									Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true),
									Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1103, 138, true),
									UseVisualStyleBackColor = true,
									Name = tabPageName,
									Text = GetTabPageText(header.CountryName, header.AMA_ManifestTypeDescription)
								};

								header.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
								header.AMA_ManifestTypeInfo.ValueChanged += AMA_ManifestTypeInfo_ValueChanged;
								header.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
								header.AMA_TransportModeInfo.ValueChanged += AMA_TransportModeInfo_ValueChanged;
								header.AMA_RN_NKCountryInfo.ValueChanged -= AMA_RN_NKCountryInfo_ValueChanged;
								header.AMA_RN_NKCountryInfo.ValueChanged += AMA_RN_NKCountryInfo_ValueChanged;

								var manifestUserControl = new AsycudaManifestUserControl();
								manifestUserControl.SetDataBinding(header, string.Empty);
								manifestUserControl.BindingContext = MainTabControl.BindingContext;

								childHeaderTab.Controls.Add(manifestUserControl);
								manifestUserControl.Dock = DockStyle.Fill;
								MainTabControl.TabPages.Insert(childHeaderTab, tabIndex);
							}

							tabIndex++;
						}
					}
				}

				var newHeaderTab = MainTabControl.GetTabPage(manifestManagementTabPageName);

				if (newHeaderTab == null)
				{
					newHeaderTab = new ZTabPage
					{
						Name = manifestManagementTabPageName,
						Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true),
						Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true),
						Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1103, 138, true),
						Text = Res.GetString("6D907E3D-2393-4A28-844B-A7C05D1CEC79", "Manifest Management"),
						UseVisualStyleBackColor = true
					};

					var headerManagementUserControl = new HeaderManagementUserControl();
					newHeaderTab.Controls.Add(headerManagementUserControl);

					headerManagementUserControl.BindingSource.SetDataBinding(headersWrapper, string.Empty);
					headerManagementUserControl.Dock = DockStyle.Fill;

					headerManagementUserControl.OnCreateNewHeader -= OnCreateNewHeader;
					headerManagementUserControl.OnCreateNewHeader += OnCreateNewHeader;

					headerManagementUserControl.OnManifestDeleting -= OnManifestDeleting;
					headerManagementUserControl.OnManifestDeleting += OnManifestDeleting;

					MainTabControl.TabPages.Insert(newHeaderTab, tabIndex);
				}
				else
				{
					newHeaderTab.TabIndex = tabIndex;
				}

				newHeaderTab.ResumeLayout(false);
				newHeaderTab.PerformLayout();
			}

			MainTabControl.Refresh();
		}

		void AMA_ManifestTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is InfoEventArgs args && args.Info.BizObj is AsycudaManifestHeader header && e is ValueChangedEventArgs valueChangedEventArgs)
			{
				var oldManifestType = (ZString)valueChangedEventArgs.OldValue;
				var newManifestType = (ZString)valueChangedEventArgs.NewValue;

				UpdateManifestTab(header, oldManifestType, newManifestType, header.AMA_RN_NKCountry, (ZString)header.AMA_RN_NKCountryInfo.Value);
			}
		}

		void AMA_RN_NKCountryInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is InfoEventArgs args && args.Info.BizObj is AsycudaManifestHeader header && e is ValueChangedEventArgs valueChangedEventArgs)
			{
				var oldCountry = (ZString)valueChangedEventArgs.OldValue;
				var newCountry = (ZString)valueChangedEventArgs.NewValue;

				UpdateManifestTab(header, header.AMA_ManifestType, (ZString)header.AMA_ManifestTypeInfo.Value, oldCountry, newCountry);
			}
		}

		void AMA_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is InfoEventArgs args && args.Info.BizObj is AsycudaManifestHeader header)
			{
				UpdateManifestTab(header, header.AMA_ManifestType, (ZString)header.AMA_ManifestTypeInfo.Value, header.AMA_RN_NKCountry, (ZString)header.AMA_RN_NKCountryInfo.Value);
			}
		}

		void UpdateManifestTab(AsycudaManifestHeader header, ZString oldManifestType, ZString newManifestType, ZString oldCountryCode, ZString newCountryCode)
		{
			if (isInUpdateManifestTab)
			{
				return;
			}

			isInUpdateManifestTab = true;
			try
			{
				var childHeaderTab = GetTabPage(oldCountryCode, oldManifestType);
				if (childHeaderTab != null)
				{
					var text = GetTabPageText(header.CountryName, header.AMA_ManifestTypeDescription);
					var existingTabPage = GetTabPage(newCountryCode, newManifestType);
					if (existingTabPage != null && existingTabPage != childHeaderTab)
					{
						Globals.Message.ShowError(Res.GetString("FB60D226-85E3-4AA3-B344-91513F1B874C",
							"The following manifest already exists on this consol, only one is allowed: {0}",
							text));
						header.AMA_ManifestType = oldManifestType;
						header.AMA_RN_NKCountry = oldCountryCode;
					}
					else
					{
						childHeaderTab.Name = GetTabPageName(newCountryCode, newManifestType);
						childHeaderTab.Text = text;
					}
				}
			}
			finally
			{
				isInUpdateManifestTab = false;
			}
		}
		bool isInUpdateManifestTab;

		void OnManifestDeleting(object sender, EventArgs e)
		{
			if (sender is AsycudaManifestHeader manifestToDelete)
			{
				var countryCode = manifestToDelete.AMA_RN_NKCountry;
				var manifestType = manifestToDelete.AMA_ManifestType;
				var tabPage = GetTabPage(countryCode, manifestType);
				if (tabPage != null)
				{
					MainTabControl.SelectTab(tabPage);
					var manifestInTabPage = tabPage.FindSingleOrDefault<AsycudaManifestUserControl>()?.ManifestHeader;
					if (manifestInTabPage != null && manifestInTabPage == manifestToDelete)
					{
						UnlockHeaderIfLocked(manifestInTabPage);
						MainTabControl.TabPages.Remove(tabPage);
						foreach (Control control in tabPage.Controls)
						{
							control.Dispose();
						}

						tabPage.Dispose();
					}

					MainTabControl.SelectTab(MainTabControl.TabPages.Cast<ZTabPage>().Last());
				}
				manifestToDelete.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
				manifestToDelete.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
				manifestToDelete.AMA_RN_NKCountryInfo.ValueChanged -= AMA_RN_NKCountryInfo_ValueChanged;
			}
		}

		ZString GetTabPageName(ZString countryCode, ZString manifestType)
		{
			return ZString.Format("{0}{1}TabPage", countryCode, manifestType);
		}

		ZString GetTabPageText(ZString countryDescription, ZString manifestType)
		{
			var tabPageText = Res.GetString("0491F515-F683-4282-AFF7-5B34293B7168", "{0} - {1}", countryDescription, manifestType);
			return ZTabPage.GetEscapedTabPageText(tabPageText);
		}

		ZTabPage GetTabPage(ZString countryCode, ZString manifestType)
		{
			var tabPageName = GetTabPageName(countryCode, manifestType);
			return MainTabControl.GetTabPage(tabPageName);
		}

		void OnCreateNewHeader(object sender, EventArgs e)
		{
			var countryCode = HeadersWrapper.WR_CountryCode;
			var manifestType = HeadersWrapper.WR_ManifestType;
			if (!countryCode.IsEmpty && !manifestType.IsEmpty)
			{
				var consol = HeadersWrapper.Consol;
				var childHeaderTab = GetTabPage(countryCode, manifestType);
				if (childHeaderTab == null)
				{
					if (HeadersWrapper.Headers.GetHeader(countryCode, manifestType) == null && !consol.CheckManifestHeaderHasBeenCreated(countryCode, manifestType))
					{
						CreateCountry(consol, countryCode, manifestType);
						childHeaderTab = GetTabPage(countryCode, manifestType);
						if (childHeaderTab != null)
						{
							MainTabControl.SelectTab(childHeaderTab);
						}
					}
					else
					{
						Globals.Message.ShowError(
							Res.GetString("59E8772C-3857-4466-85EE-960BD2DF22DD", "Another user has already created a Manifest for {0}. Please close and re-open the Consol to see any newly added Manifest Countries", HeadersWrapper.CountryCodes.GetDescriptionFromCode(countryCode)),
							Res.GetString("0FE56549-959D-4413-AD84-2CC6A720F893", "Create New Manifest"));
					}
				}
				else
				{
					MainTabControl.SelectTab(childHeaderTab);
				}
			}
		}

		void CreateCountry(ForwardingConsol consol, ZString countryCode, ZString manifestType)
		{
			var factory = consol.Factory;
			var consolPK = consol.PK;
			if (factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, consolPK, countryCode))
			{
				if (!consol.CheckManifestHeaderHasBeenCreated(countryCode, manifestType))
				{
					HeadersWrapper.CreateCountry(countryCode, manifestType);
				}

				LoadCountryTabs();
				HeadersWrapper.WR_CountryCode = ZString.Empty;
				HeadersWrapper.WR_ManifestType = ZString.Empty;
			}
			else
			{
				Globals.Message.ShowError(
					Res.GetString("66BA942D-C241-4BDB-841C-6AD6CF29BB33", "{0} is already in the process of creating a Manifest for {1}.\r\nYou should be able to access the Manifest when the person has saved the record. Please try later.\r\n", factory.GetChildLockByInfo(MutexIDs.AsycudaManJobBeingCreated, consolPK, countryCode), HeadersWrapper.CountryCodes.GetDescriptionFromCode(countryCode)),
					Res.GetString("997C5826-DBAF-43BC-AA49-678B967D90AA", "Create New Manifest"));
			}
		}

		void UnlockHeaderIfLocked(AsycudaManifestHeader header)
		{
			var consol = header.Consol;
			var factory = consol.Factory;
			var consolPK = consol.PK;
			var countryCode = header.AMA_RN_NKCountry;
			factory.UnlockChild(MutexIDs.AsycudaManJobBeingCreated, consolPK, countryCode);
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.UnlockAllChildrenOnParent(MutexIDs.AsycudaManJobBeingCreated, HeadersWrapper.Consol.PK);
			}
		}
	}
}
