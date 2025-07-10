using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;
using ResString = Enterprise.Customs.ASYCUDA.Gui.ResString;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class AsycudaManifestUserControl : ManifestSpecificProviderUserControl, IResetMessageStatusSupporter, ISupportMultipleResourceStringDataSupporter
	{
		const string IsVisibleForBindingString = "IsVisibleForBinding";

		public AsycudaManifestUserControl()
		{
			InitializeComponent();
			createDeclarationMenuItemIndex = BillsGrid.DeleteMenuItem.Index + 1;
			AddCreateCustomsDeclarationMenuItem();
			BillsGrid.AfterBind -= BillsGrid_AfterBind;
			BillsGrid.AfterBind += BillsGrid_AfterBind;
			BillCustomFieldsControl.Load -= BillCustomFieldsControl_Load;
			BillCustomFieldsControl.Load += BillCustomFieldsControl_Load;
			asycudaBillUserControl.Load -= asycudaBillUserControl_Load;
			asycudaBillUserControl.Load += asycudaBillUserControl_Load;
			asycudaBillPartiesUserControl.Load -= asycudaBillPartiesUserControl_Load;
			asycudaBillPartiesUserControl.Load += asycudaBillPartiesUserControl_Load;
		}

		void asycudaBillUserControl_Load(object sender, EventArgs e)
		{
			var columnSeparator1 = asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("ColumnSeparator1", false).FirstOrDefault();
			var columnSeparator0 = dynamicManifestDetailsPanel.Controls.Find("ColumnSeparator0", false).FirstOrDefault();

			if (columnSeparator0 != null)
			{
				dynamicManifestDetailsPanel.Controls.Find("DeparturePortUNLOCOCodeFindBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("ScheduleDCodeFindBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("ManifestTypeDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("PortOfFirstArrivalCodeFindBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("NatureDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("ConveyanceCountryCodeFindBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("TransportModeDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("PortOfDischargeCodeFindBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("ContainerModeDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("BuyersConsolidationCheckBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				dynamicManifestDetailsPanel.Controls.Find("LloydsNumberTextBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
			}
			if (columnSeparator1 != null)
			{
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find(nameof(CommonBillUserControl.NotifyPartyAddressControl), false).FirstOrDefault()?.AllowOverlap(columnSeparator1);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find(nameof(CommonBillUserControl.ConsigneeAddressControl), false).FirstOrDefault()?.AllowOverlap(columnSeparator1);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find(nameof(CommonBillUserControl.ShipperAddressControl), false).FirstOrDefault()?.AllowOverlap(columnSeparator1);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find(nameof(CommonBillUserControl.AgentAddressControl), false).FirstOrDefault()?.AllowOverlap(columnSeparator1);
			}
			columnSeparator0 = asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("ColumnSeparator0", false).FirstOrDefault();
			if (columnSeparator0 != null)
			{
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("PriorTransportationModeDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("FinalDestinationPortUserControl", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("ArrivalPortUserControl", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("DeparturePortUserControl", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("LadingPortUserControl", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("UnladingPortUserControl", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("OriginPortUserControl", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("PlaceOfReceiptTextBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("AESITNNumberTextBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("VolumeCalcDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("GrossWeightCalcDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("ManifestQtyCalcDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("BillIssuerCodeFindBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("BillNumberTextBox", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
				asycudaBillUserControl.dynamicBilllDetailsPanel.Controls.Find("SpecialCargoCodesDropEdit", false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
			}
		}

		void asycudaBillPartiesUserControl_Load(object sender, EventArgs e)
		{
			var columnSeparator0 = asycudaBillPartiesUserControl.DynamicBillPartiesPanel.Controls.Find("ColumnSeparator0", false).FirstOrDefault();
			var columnSeparator1 = asycudaBillPartiesUserControl.DynamicBillPartiesPanel.Controls.Find("ColumnSeparator1", false).FirstOrDefault();

			if (columnSeparator0 != null)
			{
				asycudaBillPartiesUserControl.DynamicBillPartiesPanel.Controls.Find(nameof(CommonBillUserControl.ShipperAddressControl), false).FirstOrDefault()?.AllowOverlap(columnSeparator0);
			}

			if (columnSeparator1 != null)
			{
				asycudaBillPartiesUserControl.DynamicBillPartiesPanel.Controls.Find(nameof(CommonBillUserControl.ConsigneeAddressControl), false).FirstOrDefault()?.AllowOverlap(columnSeparator1);
			}
		}

		void SetMainTabPageGroupBoxCaption()
		{
			if (Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				this.ManifestGroupBox.CaptionResourceString = provider.MainTabPageGroupBoxName;
			}
		}

		void SetTabPageVisible()
		{
			if (Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				this.dutiesTabPage.TabVisible = provider.ShowTaxesUserControl(header);
				var dutiesTabPageName = provider.GetDutiesTabPageCaption();
				this.dutiesTabPage.CaptionResourceString = dutiesTabPageName;
				if (this.dutiesTabPage.TabVisible)
				{
					this.asycudaDutiesUserControl.SetControlVisibility(header);
				}

				this.arrivalsTabPage.TabVisible = header.FeatureProvider?.SupportArrivalInformation(header) ?? false;
				SetBillPartiesTabPageVisibility(provider);
			}
		}

		public new AsycudaManifestHeader CurrentDataItem => (AsycudaManifestHeader)base.CurrentDataItem;

		void BillCustomFieldsControl_Load(object sender, EventArgs e)
		{
			BindBillCustomFieldsControlToCurrentSelectedBill();
		}

		void BillsGrid_AfterBind(object sender, EventArgs e)
		{
			var billsGridListManager = BillsGrid.ListManager;
			if (billsGridListManager != null)
			{
				billsGridListManager.CurrentItemChanged -= ListManager_CurrentItemChanged;
				billsGridListManager.CurrentItemChanged += ListManager_CurrentItemChanged;
			}
			ListManager_CurrentItemChanged(null, EventArgs.Empty);
		}

		void ListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			BindBillCustomFieldsControlToCurrentSelectedBill();
		}

		void BindBillCustomFieldsControlToCurrentSelectedBill()
		{
			BillCustomFieldsControl.SetDataBinding(CurrentSelectedBill, string.Empty);
		}

		readonly int createDeclarationMenuItemIndex;

		void AddCreateCustomsDeclarationMenuItem()
		{
			createDeclarationMenuItem = new ZMenuItem(ResString.GetMultilingualString("58b716ff-12aa-47f2-81b4-f7beab0f32bc", "Create Customs Declaration"));
			createDeclarationMenuItem.Click += (s, e) =>
			{
				var bill = CurrentSelectedBill;
				var header = bill?.Header;
				if (header != null)
				{
					if (header.HasChanges)
					{
						Globals.Message.ShowWarning(ResString.GetMultilingualString("{F75E9FA8-E4F3-4781-A44A-0DF316596E36}", "Cannot create Declaration as there are unsaved changes."));
					}
					else
					{
						if (bill.IsDeclarationCreationEnabled)
						{
							var declaration = bill.CreateCustomsDeclaration();
							if (declaration != null)
							{
								var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
								controller.ShowEditForm(declaration);
#if DEBUG
								lastControllerForTesting = controller;
#endif
							}
						}
					}
				}
			};
			BillsGrid.ContextMenu.MenuItems.InsertRange(createDeclarationMenuItemIndex, new[] { createDeclarationMenuItem });
			BillsGrid.ContextMenu.Popup += (s, e) =>
			{
				var bill = CurrentSelectedBill;
				createDeclarationMenuItem.Visible = bill?.IsDeclarationCreationEnabled ?? false;
				SetBillsGridExtraContextMenuItemsVisibility(bill);
			};
		}

		void RemoveColumnsInBillGrid()
		{
			var header = Header;
			if (header != null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				var columnsToRemove = provider?.GetColumnsToRemoveInBillsGrid().ToArray() ?? [];
				BillsGrid.RemoveFromAvailableColumns(columnsToRemove);
			}
		}

#if DEBUG
		public ZController LastControllerForTesting
		{
			get { return lastControllerForTesting; }
		}
		ZController lastControllerForTesting;
#endif

		AsycudaBill CurrentSelectedBill => BillsGrid.GetCurrent() as AsycudaBill;

		public AsycudaManifestHeader ManifestHeader => Header;

		public void SelectAndShowBill(ZGuid bilPK)
		{
			if (Header is AsycudaManifestHeader header)
			{
				var bill = (AsycudaBill)header.Bills.FindByPK(bilPK);
				if (bill != null)
				{
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var manager = (CurrencyManager)asycudaBillUserControl.BindingContext[header, "Bills"];
					var index = manager.List.IndexOf(bill);
					if (index >= 0)
					{
						manager.Position = index;
					}
				}
			}
		}

		#region Location columns for second country and minimum size

		protected override void HookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
			header.SynchroniseWithSourceIfNeeded();
			header.EnableBillsLock(true);
			header.OnTransportModeChanged += ManifestHeader_OnTransportModeChanged;
			header.OnNatureOrCountryChanged += ManifestHeader_OnNatureOrCountryChanged;
			header.NeedPersonsTabInfo.ValueChanged += ManifestHeader_OnNeedPersonsTabChanged;
			header.OnManifestTypeChanged += ManifestHeader_OnManifestTypeChanged;
			header.OnSpecificCircumstanceIndicatorChanged += ManifestHeader_OnSpecificCircumstanceIndicatorChanged;

			OverrideFreightDefaults.CaptionResourceString = header.IsStandAlone
				? Res.GetData("0015746C-2F48-4CD9-9A3A-12697928A2B9", "Override Default Values from Sailing", "Override Default Values from Sailing?  Tick this box to ignore the values brought through from the sailing, replacing them with your own.")
				: Res.GetData("311BCD4B-C819-4FDD-A497-D9BE2B4ED402", "Override Default Values from Consol", "Override Default Values from Consol/Shipment?  Tick this box to ignore the values brought through from the consol and shipment, replacing them with your own.");
		}

		protected override void UnHookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
			header.OnTransportModeChanged -= ManifestHeader_OnTransportModeChanged;
			header.OnNatureOrCountryChanged -= ManifestHeader_OnNatureOrCountryChanged;
			header.NeedPersonsTabInfo.ValueChanged -= ManifestHeader_OnNeedPersonsTabChanged;
			header.OnManifestTypeChanged -= ManifestHeader_OnManifestTypeChanged;
			header.OnSpecificCircumstanceIndicatorChanged -= ManifestHeader_OnSpecificCircumstanceIndicatorChanged;
			header.EnableBillsLock(false);
		}

		#endregion

		void ManifestHeader_OnTransportModeChanged(object sender, EventArgs e)
		{
			var header = Header;
			if (header != null)
			{
				SetContainersTabPageVisibility(header);
				SailingUserControl.Visible = header.CanBeLinkedWithSailing;
				SetArrivalsTabPageVisibility(header);

				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				SetupBillsGridExtraColumnInfos(provider);
				SetBillsGridExtraColumnInfosVisibilityOnValueChanged(provider);
				SetTransportTabPageVisibility(header);
				SetAsycudaContainerUserControl();
				SetBillPartiesTabPageVisibility(provider);
			}
		}

		void ManifestHeader_OnSpecificCircumstanceIndicatorChanged(object sender, EventArgs e)
		{
			var header = Header;
			if (header != null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				SetBillsGridExtraColumnInfosVisibilityOnValueChanged(provider);
			}
		}

		void ManifestHeader_OnNatureOrCountryChanged(object sender, EventArgs e)
		{
			var header = Header;
			if (header != null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				SetupBillsGridExtraColumnInfos(provider);
				SetBillsGridExtraColumnInfosVisibilityOnValueChanged(provider);
				SetBillPartiesTabPageVisibility(provider);
			}
		}

		void ManifestHeader_OnNeedPersonsTabChanged(object sender, EventArgs e)
		{
			var header = Header;
			if (header != null)
			{
				personsTabPage.TabVisible = header.NeedPersonsTab;
			}
		}

		void ManifestHeader_OnManifestTypeChanged(object sender, EventArgs e)
		{
			var header = Header;
			if (header != null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				dutiesTabPage.TabVisible = provider.ShowTaxesUserControl(header);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				OverrideFreightDefaults.DataBindings.RemoveBinding(IsVisibleForBindingString);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				OverrideFreightDefaults.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsOverrideFreightDefaultsVisible"));
			}

			ManifestHeader_OnTransportModeChanged(this, EventArgs.Empty);
			ManifestHeader_OnNatureOrCountryChanged(this, EventArgs.Empty);
			ManifestHeader_OnNeedPersonsTabChanged(this, EventArgs.Empty);
			ManifestHeader_OnSpecificCircumstanceIndicatorChanged(this, EventArgs.Empty);
			RemoveColumnsInBillGrid();

			AddAdditionalTabsOfManifestHeaderTab();
			AddBillAdditionalTabPage();
			AddBillMessagesTabPage();
			SetMainTabPageGroupBoxCaption();
			SetTabPageVisible();
		}

		void SetAsycudaContainerUserControl()
		{
			if (containersTabPage.Controls.Count == 0 && containersTabPage.TabVisible && Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				this.asycudaContainerUserControl = provider.GetAsycudaContainerUserControl();
				this.asycudaContainerUserControl.AllowDrop = true;
				this.asycudaContainerUserControl.AutoScroll = true;
				this.BindingSource.SetBindingMember(this.asycudaContainerUserControl, ".");
				this.asycudaContainerUserControl.Dock = DockStyle.Fill;
				this.asycudaContainerUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
				this.asycudaContainerUserControl.Name = "asycudaContainerUserControl";
				this.containersTabPage.Controls.Add(this.asycudaContainerUserControl);
			}
		}

		void SetContainersTabPageVisibility(AsycudaManifestHeader header)
		{
			var isAir = header.IsAir;
			containersTabPage.TabVisible = !isAir && header.HasContainers;
		}

		protected override Control ControlToAddManifestSpecificUserControl => CountrySpecificDetailsPanel;
		protected override string ManifestSpecificUserControlDataMember => "";

		void SetArrivalsTabPageVisibility(AsycudaManifestHeader header)
		{
			arrivalsTabPage.TabVisible = header.FeatureProvider.SupportArrivalInformation(header);
		}

		void CustomiseMenuItem()
		{
			var messageStatusTextBox = dynamicManifestDetailsPanel.Controls.Find(nameof(CommonManifestControlBag.MessageStatusTextBox), true).FirstOrDefault();
			resetMessageStatus = messageStatusTextBox == null ? null : ResetMessageStatusHelper.CreateResetMessageStatusMenuItem(messageStatusTextBox, ResetMessageStatus_Click, ResetMessageStatus_Popup);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			ManifestHeader_OnTransportModeChanged(null, EventArgs.Empty);
			ManifestHeader_OnNatureOrCountryChanged(null, EventArgs.Empty);
			ManifestHeader_OnNeedPersonsTabChanged(null, EventArgs.Empty);
			ManifestHeader_OnSpecificCircumstanceIndicatorChanged(null, EventArgs.Empty);
		}

		#region AsycudaManifestHeaderAdditionalTab

		UserControlGenerateHelper additionalHeaderLevelUserControlsHelper;

		void AddAdditionalTabsOfManifestHeaderTab()
		{
			if (additionalHeaderLevelUserControlsHelper == null && Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				var headerAdditionalTabPageUserControls = provider.GetHeaderAdditionalTabPageUserControls(header);
				if (!provider.ShouldPositionMessagesTabAccordingToMessageLevel || header.ManifestType.IsManifestMessageLevel())
				{
					headerAdditionalTabPageUserControls = headerAdditionalTabPageUserControls.Append(provider.GetHeaderMessagesUserControl());
				}
				additionalHeaderLevelUserControlsHelper = new UserControlGenerateHelper(header, headerAdditionalTabPageUserControls);
				additionalHeaderLevelUserControlsHelper.AddAdditionalTabPages(mainTabControl, null, "", () => new AsycudaManifestHeaderAdditionalTabPageUserControl(), 0);
			}
		}

		#endregion

		#region AsycudaManifestBillAdditionalTab

		UserControlGenerateHelper additionalBillLevelUserControlsHelper;
		void AddBillAdditionalTabPage()
		{
			if (additionalBillLevelUserControlsHelper == null && Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				var billAdditionalTabPageUserControls = provider.GetBillAdditionalTabPageUserControl();
				additionalBillLevelUserControlsHelper = new UserControlGenerateHelper(header, billAdditionalTabPageUserControls);
				additionalBillLevelUserControlsHelper.AddAdditionalTabPages(billsAndPacksTabControl, BindingSource, "Bills", () => new BillAdditionalTabPageUserControl());
			}
		}

		#endregion

		#region AsycudaManifestBillMessagesTab

		UserControlGenerateHelper additionalBillMessagesTabControlsHelper;
		void AddBillMessagesTabPage()
		{
			if (additionalBillMessagesTabControlsHelper == null && Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				if (provider.ShouldPositionMessagesTabAccordingToMessageLevel && header.ManifestType.IsBillMessageLevel())
				{
					var messageTab = new List<IAdditionalTabPage>() { provider.GetBillMessagesUserControl() };
					additionalBillMessagesTabControlsHelper = new UserControlGenerateHelper(header, messageTab);
					additionalBillMessagesTabControlsHelper.AddAdditionalTabPages(billsAndPacksTabControl, BindingSource, "Bills", () => new BillAdditionalTabPageUserControl(), 0);
				}
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (GetVesselCodeFindBox() is ZCodeFindBox vesselCodeFindBox)
				{
					vesselCodeFindBox.PopupSelected -= VesselCodeFindBox_PopupSelected;
				}
				if (Header is AsycudaManifestHeader header)
				{
					header.AMA_VesselNameInfo.ValueChanged -= AMA_VesselNameInfoValueChanged;
					header.AMA_NatureInfo.ValueChanged -= AMA_NatureInfoValueChanged;
				}
				additionalHeaderLevelUserControlsHelper?.DisposeTabPages();
				additionalBillLevelUserControlsHelper?.DisposeTabPages();
				additionalBillMessagesTabControlsHelper?.DisposeTabPages();
			}
			base.Dispose(disposing);
		}

		void IResetMessageStatusSupporter.ResetMessageStatus_Popup() => ResetMessageStatus_Popup();
		void ResetMessageStatus_Popup() => ResetMessageStatusHelper.ResetMessageStatus_PopupManifestLevel(Header, resetMessageStatus, false);

		void IResetMessageStatusSupporter.ResetMessageStatus_Click() => ResetMessageStatus_Click();
		void ResetMessageStatus_Click() => ResetMessageStatusHelper.ResetMessageStatus_ClickManifestLevel(Header);
		ZMenuItem resetMessageStatus;

		protected override void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			var manifestLayout = provider?.GetManifestLayout();
			dynamicManifestDetailsPanel.UpdateLayout(manifestLayout);

			base.OnProviderIdentifierChanged(provider);
			SetupBillsGridExtraColumnInfos(provider);
			SetupBillsGridExtraContextMenuItems(provider);
			SetPersonGridExtraColumnInfosVisibility(provider);
			CustomiseMenuItem();
			SetProviderInvolvedEvents();
		}

		void SetProviderInvolvedEvents()
		{
			if (GetVesselCodeFindBox() is ZCodeFindBox vesselCodeFindBox)
			{
				vesselCodeFindBox.PopupSelected -= VesselCodeFindBox_PopupSelected;
				vesselCodeFindBox.PopupSelected += VesselCodeFindBox_PopupSelected;
			}

			if (Header is AsycudaManifestHeader header)
			{
				header.AMA_VesselNameInfo.ValueChanged -= AMA_VesselNameInfoValueChanged;
				header.AMA_VesselNameInfo.ValueChanged += AMA_VesselNameInfoValueChanged;
				header.AMA_NatureInfo.ValueChanged -= AMA_NatureInfoValueChanged;
				header.AMA_NatureInfo.ValueChanged += AMA_NatureInfoValueChanged;
				AMA_NatureInfoValueChanged(null, null);
			}
		}

		void AMA_NatureInfoValueChanged(object sender, EventArgs e)
		{
			if (Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				provider.AMA_NatureInfoValueChanged(header, BillsGrid, sender, e);
			}
		}

		Control GetVesselCodeFindBox()
		{
			return dynamicManifestDetailsPanel.Controls.Find(nameof(CommonManifestControlBag.VesselCodeFindBox), true).FirstOrDefault();
		}

		void AMA_VesselNameInfoValueChanged(object sender, EventArgs e)
		{
			if (Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				provider.AMA_VesselNameInfoValueChanged(header, sender, e);
			}
		}

		void VesselCodeFindBox_PopupSelected(object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (Header is AsycudaManifestHeader header)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				provider.VesselCodeFindBox_PopupSelected(header, sender, e);
			}
		}

		protected override void AMA_ManifestTypeInfo_ValueChangedCore(object sender, EventArgs e)
		{
			base.AMA_ManifestTypeInfo_ValueChangedCore(sender, e);
			var header = Header;
			var isHeaderNull = (header == null);
			if (isHeaderNull)
			{
				containersTabPage.TabVisible = false;
				billsAndPacksTabPage.TabVisible = false;
				transportTabPage.TabVisible = false;
			}
			else
			{
				billsAndPacksTabPage.TabVisible = header.HasBillsAndPacks;
				SetContainersTabPageVisibility(header);

				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				SetupBillsGridExtraColumnInfos(provider);
				SetBillsGridExtraColumnInfosVisibilityOnValueChanged(provider);
				SetArrivalsTabPageVisibility(header);
			}
		}

		protected override void EnforceOnlyValidManifestTypes(object sender, EventArgs args)
		{
			if (sender is AsycudaManifestHeader header && args is ValueChangedEventArgs valueChangedEventArgs)
			{
				if (isInEnforceOnlyValidManifestTypes)
				{
					return;
				}

				isInEnforceOnlyValidManifestTypes = true;
				try
				{
					var oldManifestType = (ZString)valueChangedEventArgs.OldValue;
					var newManifestType = (ZString)valueChangedEventArgs.NewValue;
					var manifestTypes = ApplicationBusinessProvider.GetApplicationBusinessProvider(header.Factory,
							(header.AMA_RN_NKCountry, oldManifestType, header.AMA_ApplicationCode)).ManifestTypes
						.Where(x => x.ApplicableManifestStyles.Contains((string)header.AMA_ApplicationCode));
					if (manifestTypes.Any() && manifestTypes.All(m => m.Code != newManifestType))
					{
						var revertValue = manifestTypes.Any(m => m.Code == oldManifestType)
							? oldManifestType
							: new ZString(manifestTypes.First().Code);
						Globals.Message.ShowError(Res.GetString("3B08D1C9-CB3F-4ABE-9385-57DE63AF5B2C",
							"Manifest Type cannot be set to an invalid value: {0}.", newManifestType));
						header.AMA_ManifestType = revertValue;
					}
				}
				finally
				{
					isInEnforceOnlyValidManifestTypes = false;
				}
			}
		}
		bool isInEnforceOnlyValidManifestTypes;

		protected override void EnforceOnlyValidTransportMode(object sender, EventArgs args)
		{
			if (sender is AsycudaManifestHeader header && args is ValueChangedEventArgs valueChangedEventArgs)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
				if (provider.IsInEnforceOnlyValidTransportMode())
				{
					var oldTransportMode = (ZString)valueChangedEventArgs.OldValue;
					var newTransportMode = (ZString)valueChangedEventArgs.NewValue;
					var transportModes = header.Lookups.TransportModeList.ToArray();
					if (transportModes.Any() && transportModes.All(m => m.Code != newTransportMode))
					{
						var revertValue = transportModes.Any(m => m.Code == oldTransportMode)
							? oldTransportMode
							: new ZString(transportModes.First().Code);
						Globals.Message.ShowError(Res.GetString("EE542F14-94E3-44DF-BA33-C3360CD3A898",
							"Transport Mode cannot be set to an invalid value: {0}.", newTransportMode));
						header.AMA_TransportMode = revertValue;
					}
				}
			}
		}

		protected override void EnforceOnlyValidSpecificCircumstanceIndicator(object sender, EventArgs args)
		{
			if (sender is AsycudaManifestHeader header && args is ValueChangedEventArgs valueChangedEventArgs)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
				if (provider.IsInEnforceOnlyValidSpecificCircumstanceIndicator())
				{
					var oldSpecificCircumstanceIndicator = (ZString)valueChangedEventArgs.OldValue;
					var newSpecificCircumstanceIndicator = (ZString)valueChangedEventArgs.NewValue;
					var specificCircumstanceIndicators = header.Lookups.SpecificCircumstanceList.ToArray();
					if (!newSpecificCircumstanceIndicator.IsEmpty && specificCircumstanceIndicators.Any() && specificCircumstanceIndicators.All(m => m.Code != newSpecificCircumstanceIndicator))
					{
						var revertValue = specificCircumstanceIndicators.Any(m => m.Code == oldSpecificCircumstanceIndicator)
							? oldSpecificCircumstanceIndicator
							: new ZString(specificCircumstanceIndicators.First().Code);
						Globals.Message.ShowError(Res.GetString("F4297D34-C48D-48F7-916D-3E53980D3D5E",
							"Specific Circumstance Indicator cannot be set to an invalid value: {0}.", newSpecificCircumstanceIndicator));
						header.SpecificCircumstanceIndicator = revertValue;
					}
				}
			}
		}

		void SetupBillsGridExtraContextMenuItems(ApplicationGUIProvider provider)
		{
			var contextMenu = BillsGrid.ContextMenu;

			if (contextMenu != null)
			{
				if (billsGridExtraMenuItems != null)
				{
					var billsGridMenuItems = contextMenu.MenuItems;
					foreach (var menuItem in billsGridExtraMenuItems)
					{
						billsGridMenuItems.Remove(menuItem);
					}
				}
				billsGridExtraMenuItems = provider?.GetBillsGridExtraMenuItems(BillsGrid).ToArray();
				if (billsGridExtraMenuItems != null && billsGridExtraMenuItems.Length > 0)
				{
					contextMenu.MenuItems.InsertRange(createDeclarationMenuItemIndex + 1, billsGridExtraMenuItems);
				}
			}
		}
		ZMenuItem[] billsGridExtraMenuItems;

		void SetBillsGridExtraContextMenuItemsVisibility(AsycudaBill bill)
		{
			if (billsGridExtraMenuItems != null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
				if (provider != null)
				{
					provider.SetBillsGridExtraMenuItemsVisibility(billsGridExtraMenuItems, BillsGrid.GetSelectedElements<AsycudaBill>(), bill);
				}
			}
		}

		void SetBillsGridExtraColumnInfosVisibilityOnValueChanged(ApplicationGUIProvider provider)
		{
			var billsGridColumnVisiblility = provider?.GetBillsGridColumnVisiblilityOnValueChanged(Header);
			if (billsGridColumnVisiblility != null && billsGridColumnVisiblility.Any())
			{
				using (BillsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					foreach (var billsGridColumnVisiblilityData in billsGridColumnVisiblility)
					{
						BillsGrid.SetAvailability(billsGridColumnVisiblilityData.Value, billsGridColumnVisiblilityData.Key);
						BillsGrid.SetColumnVisible(billsGridColumnVisiblilityData.Value, billsGridColumnVisiblilityData.Key);
					}
				}
			}
		}

		void SetupBillsGridExtraColumnInfos(ApplicationGUIProvider provider)
		{
			using (BillsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (billsGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in billsGridExtraColumnInfos)
					{
						BillsGrid.ColumnStyles.Remove(columnInfo);
					}
				}
				billsGridExtraColumnInfos = provider?.GetBillsGridExtraColumnInfos().ToArray();
				if (billsGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in billsGridExtraColumnInfos)
					{
						BillsGrid.ColumnStyles.Add(columnInfo);
					}
				}
				var billsGridColumnAvailability = provider?.GetBillsGridColumnAvailability(Header);
				if (billsGridColumnAvailability != null)
				{
					foreach (var billsGridColumnAvailabilityData in billsGridColumnAvailability)
					{
						BillsGrid.SetAvailability(billsGridColumnAvailabilityData.Key, billsGridColumnAvailabilityData.Value);
					}
				}
				var billsGridColumnsOrder = provider?.GetBillsGridColumnsOrder().ToArray();
				if (billsGridColumnsOrder != null)
				{
					BillsGrid.ReOrderColumns(billsGridColumnsOrder);
				}

				var billsGridColumnsWidth = provider?.GetBillsGridColumnsWidth();
				if (billsGridColumnsWidth != null)
				{
					foreach (var billsGridColumnsWidthData in billsGridColumnsWidth)
					{
						var columnStyle = BillsGrid.GetColumnStyle(billsGridColumnsWidthData.Key);
						columnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(billsGridColumnsWidthData.Value);
					}
				}

				provider?.CustomizeBillsGrid(BillsGrid);

				BillsGrid.RefreshLabels();
			}
		}
		ZGridColumnInfo[] billsGridExtraColumnInfos;

		void SetPersonGridExtraColumnInfosVisibility(ApplicationGUIProvider provider)
		{
			using (personsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (personGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in personGridExtraColumnInfos)
					{
						personsGrid.ColumnStyles.Remove(columnInfo);
					}
				}
				personGridExtraColumnInfos = provider?.GetPersonGridExtraColumnInfos().ToArray();
				if (personGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in personGridExtraColumnInfos)
					{
						personsGrid.ColumnStyles.Add(columnInfo);
					}
				}
			}
		}
		ZGridColumnInfo[] personGridExtraColumnInfos;

		void SetTransportTabPageVisibility(AsycudaManifestHeader header)
		{
			transportTabPage.TabVisible = header.ShowTransportMeansTab;
		}

		void SetBillPartiesTabPageVisibility(ApplicationGUIProvider provider)
		{
			if (provider != null)
			{
				billPartiesTabPage.TabVisible = provider.GetBillPartiesTabPageVisibility(Header);
			}
		}

		public ISupportMultipleResourceStringData SupportMultipleResourceStringData => DataSource as ISupportMultipleResourceStringData;
	}
}
