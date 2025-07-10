using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public abstract class ApplicationGUIProvider
	{
		protected ApplicationGUIProvider()
		{
		}

		public abstract MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header);

		public virtual Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public static ApplicationGUIProvider GetApplicationGuiProvider(AsycudaManifestHeader header)
		{
			ApplicationGUIProvider provider = null;

			if (header != null)
			{
				var applicationProviderKey = header.GetApplicationProviderKey();
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "ApplicationGuiProvider_{0}_{1}_{2}", applicationProviderKey.CountryOrGrouping, applicationProviderKey.ManfestTypeCode, applicationProviderKey.ApplicationCode); // CachedValueKey

				provider = header.Factory.GetCachedValue(cacheKey, () =>
				{
					return GetApplicationGuiProvider(header.Factory, header.ApplicationBusinessProvider);
				});
			}

			return provider;
		}

		public static ApplicationGUIProvider GetApplicationGuiProvider(BusinessObjectFactory factory, ApplicationBusinessProvider businessProvider)
		{
			ApplicationGUIProvider guiProvider = null;

			if (businessProvider != null)
			{
				var businessProviderType = businessProvider.GetType();
				var allGuiProviders = GetAllApplicationGuiProviders(factory);

				guiProvider = allGuiProviders.FirstOrDefault(p => p.ApplicationBusinessProviderType == businessProviderType)
					?? allGuiProviders.FirstOrDefault(p => p.ApplicationBusinessProviderType.IsAssignableFrom(businessProviderType))
					?? ObjectFactory.Get<ApplicationGUIProvider>("ASYCUDAManifest.ApplicationGUIProvider");
			}

			return guiProvider;
		}

		public static IEnumerable<ApplicationGUIProvider> GetAllApplicationGuiProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GlobalManifestApplicationGuiProviders", () =>
			{
				return ObjectFactory.Get<IEnumerable>("GlobalManifestApplicationGuiProvider").Cast<ApplicationGUIProvider>().ToArray();
			});
		}

		public ZString GetIdentifier(AsycudaManifestHeader header) => GetIdentifierCore(header);

		protected virtual ZString GetIdentifierCore(AsycudaManifestHeader header) => header != null && !header.IsDeleted ? header.AMA_RN_NKCountry : ZString.Empty;
		public IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControl() => GetBillAdditionalTabPageUserControlCore();
		protected virtual IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore() => Enumerable.Empty<IAdditionalTabPage>();

		public void AddBillPlugins(IFormPlugInsProvider form) => AddBillPluginsCore(form);
		protected virtual void AddBillPluginsCore(IFormPlugInsProvider form) { }

		public string GetBillFormCaption(AsycudaBill bill) => GetBillFormCaptionCore(bill);
		protected virtual string GetBillFormCaptionCore(AsycudaBill bill) => null;

		public IEnumerable<IAdditionalTabPage> GetPackAdditionalTabPageUserControl() => GetPackAdditionalTabPageUserControlCore();
		protected virtual IEnumerable<IAdditionalTabPage> GetPackAdditionalTabPageUserControlCore() => Enumerable.Empty<IAdditionalTabPage>();

		public IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControls(AsycudaManifestHeader header) => GetHeaderAdditionalTabPageUserControlsCore(header);
		protected virtual IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(AsycudaManifestHeader header) => Enumerable.Empty<IAdditionalTabPage>();

		public virtual bool ShouldPositionMessagesTabAccordingToMessageLevel => false;

		public ZUserControl GetDutiesUserControl() => GetDutiesUserControlCore();
		protected virtual ZUserControl GetDutiesUserControlCore() => new AsycudaDutiesUserControl();

		public AsycudaContainerUserControl GetAsycudaContainerUserControl() => GetAsycudaContainerUserControlCore();
		protected virtual AsycudaContainerUserControl GetAsycudaContainerUserControlCore() => new AsycudaContainerUserControl();

		public ContainerCountrySpecificUserControl GetContainerCountrySpecificUserControl() => GetContainerCountrySpecificUserControlCore();

		protected virtual ContainerCountrySpecificUserControl GetContainerCountrySpecificUserControlCore() => null;

		public AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialog(MessageChooser messageChooser, string itemsType, string messageType)
		{
			return GetNewAsycudaItemSelectionDialogCore(messageChooser, itemsType, messageType);
		}

		protected virtual AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialogCore(MessageChooser messageChooser, string itemsType, string messageType) => new AsycudaItemSelectionDialog(messageChooser, itemsType);

		public IEnumerable<ZGridColumnInfo> GetPersonGridExtraColumnInfos() => GetPersonGridExtraColumnInfosCore();

		protected virtual IEnumerable<ZGridColumnInfo> GetPersonGridExtraColumnInfosCore() => Enumerable.Empty<ZGridColumnInfo>();

		public IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfos() => GetBillsGridExtraColumnInfosCore();

		protected virtual IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore() => Enumerable.Empty<ZGridColumnInfo>();

		public IEnumerable<string> GetColumnsToRemoveInBillsGrid() => GetColumnsToRemoveInBillsGridCore();

		protected virtual IEnumerable<string> GetColumnsToRemoveInBillsGridCore() => Enumerable.Empty<string>();

		public IEnumerable<string> GetBillsGridColumnsOrder() => GetBillsGridColumnsOrderCore();

		protected virtual IEnumerable<string> GetBillsGridColumnsOrderCore() => Enumerable.Empty<string>();

		public IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailability(AsycudaManifestHeader header) => GetBillsGridColumnAvailabilityCore(header);

		protected virtual IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(AsycudaManifestHeader header) => new Dictionary<bool, string[]>();

		public IEnumerable<ZMenuItem> GetBillsGridExtraMenuItems(ZGridWithDynamicColumnHandler billsGrid) => GetBillsGridExtraMenuItemsCore(billsGrid);

		protected virtual IEnumerable<ZMenuItem> GetBillsGridExtraMenuItemsCore(ZGridWithDynamicColumnHandler billsGrid) => Enumerable.Empty<ZMenuItem>();

		public void SetBillsGridExtraMenuItemsVisibility(ZMenuItem[] billsGridExtraMenuItems, AsycudaBill[] selectedBills, AsycudaBill currentBill) => SetBillsGridExtraMenuItemsVisibilityCore(billsGridExtraMenuItems, selectedBills, currentBill);

		protected virtual void SetBillsGridExtraMenuItemsVisibilityCore(ZMenuItem[] billsGridExtraMenuItems, AsycudaBill[] selectedBills, AsycudaBill currentBill)
		{
		}

		public void CustomizeBillsGrid(ZGridWithDynamicColumnHandler billsGrid) => CustomizeBillsGridCore(billsGrid);

		protected virtual void CustomizeBillsGridCore(ZGridWithDynamicColumnHandler billsGrid) { }

		public IReadOnlyDictionary<string, int> GetBillsGridColumnsWidth() => GetBillsGridColumnsWidthCore();

		protected virtual IReadOnlyDictionary<string, int> GetBillsGridColumnsWidthCore() => new Dictionary<string, int>();

		public IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChanged(AsycudaManifestHeader header) => GetBillsGridColumnVisiblilityOnValueChangedCore(header);

		protected virtual IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(AsycudaManifestHeader header) => new Dictionary<string, bool>();

		public string[] GetPackedItemColumns() => GetPackedItemColumnsCore();

		protected virtual string[] GetPackedItemColumnsCore() => null;

		public IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfos() => GetPacksGridExtraColumnInfosCore();

		protected virtual IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore() => Enumerable.Empty<ZGridColumnInfo>();

		public string[] GetPacksGridColumnsOrder() => GetPacksGridColumnsOrderCore();
		protected virtual string[] GetPacksGridColumnsOrderCore() => null;

		public IReadOnlyDictionary<bool, string[]> GetPacksGridColumnAvailability(AsycudaManifestHeader header) => GetPacksGridColumnAvailabilityCore(header);
		protected virtual IReadOnlyDictionary<bool, string[]> GetPacksGridColumnAvailabilityCore(AsycudaManifestHeader header) => new Dictionary<bool, string[]>();

		public string[] GetPacksGridMandatoryColumns() => GetPacksGridMandatoryColumnsCore();
		protected virtual string[] GetPacksGridMandatoryColumnsCore() => null;

		public string[] GetTaxesGridColumnsOrder() => GetTaxesGridColumnsOrderCore();
		protected virtual string[] GetTaxesGridColumnsOrderCore() => null;
		public IEnumerable<ZGridColumnInfo> GetTaxesGridExtraColumnInfos() => GetTaxesGridExtraColumnInfosCore();

		protected virtual IEnumerable<ZGridColumnInfo> GetTaxesGridExtraColumnInfosCore() => Enumerable.Empty<ZGridColumnInfo>();
		public ZBool ShowTaxesUserControl(AsycudaManifestHeader header) => ShowTaxesUserControlCore(header);
		protected virtual ZBool ShowTaxesUserControlCore(AsycudaManifestHeader header) => ZBool.False;

		public ZBool GetBillPartiesTabPageVisibility(AsycudaManifestHeader header) => GetBillPartiesTabPageVisibilityCore(header);
		protected virtual ZBool GetBillPartiesTabPageVisibilityCore(AsycudaManifestHeader header) => ZBool.True;

		public IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfos(AsycudaManifestHeader header) => GetContainersGridExtraColumnInfosCore(header);

		protected virtual IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(AsycudaManifestHeader header) => Enumerable.Empty<ZGridColumnInfo>();

		public IEnumerable<string> GetContainersGridColumnsOrder() => GetContainersGridColumnsOrderCore();

		protected virtual IEnumerable<string> GetContainersGridColumnsOrderCore() => Enumerable.Empty<string>();

		public IReadOnlyDictionary<bool, string[]> GetContainersGridColumnAvailability() => GetContainersGridColumnAvailabilityCore();

		protected virtual IReadOnlyDictionary<bool, string[]> GetContainersGridColumnAvailabilityCore() => new Dictionary<bool, string[]>();

		public IReadOnlyDictionary<bool, string[]> GetContainersGridColumnVisibility() => GetContainersGridColumnVisibilityCore();

		protected virtual IReadOnlyDictionary<bool, string[]> GetContainersGridColumnVisibilityCore()
			=> new Dictionary<bool, string[]>()
			{
				{
					false,
					new[]
					{
						AsycudaContainer.Schema.ACN_Seal1UnloadingState,
						AsycudaContainer.Schema.ACN_Seal2UnloadingState,
						AsycudaContainer.Schema.ACN_Seal3UnloadingState,
					}
				},
			};

		public IReadOnlyDictionary<bool, string[]> GetPacksGridColumnVisibility() => GetPacksGridColumnVisibilityCore();

		protected virtual IReadOnlyDictionary<bool, string[]> GetPacksGridColumnVisibilityCore() => new Dictionary<bool, string[]>();

		public IReadOnlyDictionary<string, int> GetContainersGridColumnsWidth() => GetContainersGridColumnsWidthCore();

		protected virtual IReadOnlyDictionary<string, int> GetContainersGridColumnsWidthCore() => new Dictionary<string, int>();

		public IEnumerable<ZMenuItem> GetMessagesGridExtraMenuItems(ZGrid messagesGrid, AsycudaManifestHeader header) => GetMessagesGridExtraMenuItemsCore(messagesGrid, header);

		protected virtual IEnumerable<ZMenuItem> GetMessagesGridExtraMenuItemsCore(ZGrid messagesGrid, AsycudaManifestHeader header) => Enumerable.Empty<ZMenuItem>();

		public IEnumerable<ZMenuItem> GetActionsExtraMenuItems() => GetActionsExtraMenuItemsCore();

		protected virtual IEnumerable<ZMenuItem> GetActionsExtraMenuItemsCore() => Enumerable.Empty<ZMenuItem>();

		public IEnumerable<ZGridColumnInfo> GetMessagesGridExtraColumnInfos() => GetMessagesGridExtraColumnInfosCore();

		protected virtual IEnumerable<ZGridColumnInfo> GetMessagesGridExtraColumnInfosCore() => Enumerable.Empty<ZGridColumnInfo>();

		public IReadOnlyDictionary<bool, string[]> GetMessagesGridColumnAvailability(AsycudaManifestHeader header) => GetMessagesGridColumnAvailabilityCore(header);

		protected virtual IReadOnlyDictionary<bool, string[]> GetMessagesGridColumnAvailabilityCore(AsycudaManifestHeader header) => new Dictionary<bool, string[]>();

		public IReadOnlyDictionary<bool, string[]> GetMessagesGridColumnVisible(AsycudaManifestHeader header) => GetMessagesGridColumnVisibleCore(header);

		protected virtual IReadOnlyDictionary<bool, string[]> GetMessagesGridColumnVisibleCore(AsycudaManifestHeader header) => new Dictionary<bool, string[]>();

		public IReadOnlyDictionary<string, int> GetMessagesGridColumnsWidth(AsycudaManifestHeader header) => GetMessagesGridColumnsWidthCore(header);

		protected virtual IReadOnlyDictionary<string, int> GetMessagesGridColumnsWidthCore(AsycudaManifestHeader header) => new Dictionary<string, int>();

		public void SetMessagesGridExtraMenuItemsVisibility(ZMenuItem[] messagesGridExtraMenuItems, EDIMessage message) => SetMessagesGridExtraMenuItemsVisibilityCore(messagesGridExtraMenuItems, message);

		protected virtual void SetMessagesGridExtraMenuItemsVisibilityCore(ZMenuItem[] messagesGridExtraMenuItems, EDIMessage message)
		{
		}

		public IEnumerable<string> GetMessagesGridOrder() => GetMessagesGridOrderCore();

		protected virtual IEnumerable<string> GetMessagesGridOrderCore() => Enumerable.Empty<string>();

		public IPanelLayoutProvider GetManifestLayout() => GetManifestLayoutCore();

		protected virtual IPanelLayoutProvider GetManifestLayoutCore() => new DefaultManifestLayouts();

		public IPanelLayoutProvider GetBillLayout() => GetBillLayoutCore();

		protected virtual IPanelLayoutProvider GetBillLayoutCore() => new DefaultBillLayouts();

		public IPanelLayoutProvider GetBillPartiesLayout() => GetBillPartiesLayoutCore();

		protected virtual IPanelLayoutProvider GetBillPartiesLayoutCore() => new DefaultBillPartiesLayouts();

		public IPanelLayoutProvider GetPackedItemDetailsLayout() => GetPackedItemDetailsLayoutCore();

		protected virtual IPanelLayoutProvider GetPackedItemDetailsLayoutCore() => new DefaultPackedItemDetailsLayouts();

		public IReadOnlyDictionary<bool, string[]> GetArrivalHeadersGridColumnAvailability() => GetArrivalHeadersGridColumnAvailabilityCore();
		protected virtual IReadOnlyDictionary<bool, string[]> GetArrivalHeadersGridColumnAvailabilityCore() => new Dictionary<bool, string[]>();

		public IReadOnlyDictionary<bool, string[]> GetArrivalLinesGridColumnAvailability() => GetArrivalLinesGridColumnAvailabilityCore();
		protected virtual IReadOnlyDictionary<bool, string[]> GetArrivalLinesGridColumnAvailabilityCore() => new Dictionary<bool, string[]>();

		public IPanelLayoutProvider GetTransferDetailsLayout() => GetTransferDetailsLayoutCore();
		protected virtual IPanelLayoutProvider GetTransferDetailsLayoutCore() => new DefaultTransferDetailsLayouts();

		public IEnumerable<ZGridColumnInfo> GetTransferBillsGridExtraColumnInfos() => GetTransferBillsGridExtraColumnInfosCore();
		protected virtual IEnumerable<ZGridColumnInfo> GetTransferBillsGridExtraColumnInfosCore() => Enumerable.Empty<ZGridColumnInfo>();
		public IEnumerable<string> GetTransferBillsGridColumnsOrder() => GetTransferBillsGridColumnsOrderCore();
		protected virtual IEnumerable<string> GetTransferBillsGridColumnsOrderCore() => Enumerable.Empty<string>();
		public ZUserControl GetTransferBillCountrySpecificUserControl() => GetTransferBillCountrySpecificUserControlCore();
		protected virtual ZUserControl GetTransferBillCountrySpecificUserControlCore() => null;

		public ResourceStringData MainTabPageGroupBoxName => MainTabPageGroupBoxNameCore;
		protected virtual ResourceStringData MainTabPageGroupBoxNameCore => Res.GetData("6da29876-ecb0-42e0-b451-1f00b491af81", "Manifest");

		public void AMA_NatureInfoValueChanged(AsycudaManifestHeader header, ZGridWithDynamicColumnHandler billsGrid, object sender, EventArgs e) => AMA_NatureInfoValueChangedCore(header, billsGrid, sender, e);

		protected virtual void AMA_NatureInfoValueChangedCore(AsycudaManifestHeader header, ZGridWithDynamicColumnHandler billsGrid, object sender, EventArgs e) { }

		public void AMA_VesselNameInfoValueChanged(AsycudaManifestHeader header, object sender, EventArgs e) => AMA_VesselNameInfoValueChangedCore(header, sender, e);

		protected virtual void AMA_VesselNameInfoValueChangedCore(AsycudaManifestHeader header, object sender, EventArgs e) { }

		public void VesselCodeFindBox_PopupSelected(AsycudaManifestHeader header, object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e) => VesselCodeFindBox_PopupSelectedCore(header, sender, e);

		protected virtual void VesselCodeFindBox_PopupSelectedCore(AsycudaManifestHeader header, object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length == 1 && e.SelectedBusinessObjects[0] is RefVessel vessel)
			{
				header.DefaultVesselValues(vessel);
			}
		}

		public ZBool ArrivalLinesReadOnly() => ArrivalLinesReadOnlyCore();
		protected virtual ZBool ArrivalLinesReadOnlyCore() => true;

		public bool IsMessageGridUserFullNameVisible() => IsMessageGridUserFullNameVisibleCore;
		protected virtual bool IsMessageGridUserFullNameVisibleCore => false;

		public bool IsInEnforceOnlyValidTransportMode() => IsInEnforceOnlyValidTransportModeCore;
		protected virtual bool IsInEnforceOnlyValidTransportModeCore => false;

		public bool IsInEnforceOnlyValidSpecificCircumstanceIndicator() => IsInEnforceOnlyValidSpecificCircumstanceIndicatorCore;
		protected virtual bool IsInEnforceOnlyValidSpecificCircumstanceIndicatorCore => false;

		public ResourceString GetAsycudaMenuCaption() => GetAsycudaMenuCaptionCore();
		protected virtual ResourceString GetAsycudaMenuCaptionCore() => null;

		public virtual bool ShouldCheckPacksGridColumnAvailability() => false;

		public ResourceStringData GetDutiesTabPageCaption() => GetDutiesTabPageCaptionCore();
		protected virtual ResourceStringData GetDutiesTabPageCaptionCore() => Res.GetData("3A8D182D-D9E8-4134-AEBC-DFB589EBE72B", "Duties");

		public IAdditionalTabPage GetHeaderMessagesUserControl() => GetHeaderMessagesUserControlCore();

		protected virtual IAdditionalTabPage GetHeaderMessagesUserControlCore() => new MessagesUserControl();

		public IAdditionalTabPage GetBillMessagesUserControl() => GetBillMessagesUserControlCore();

		protected virtual IAdditionalTabPage GetBillMessagesUserControlCore() => new MessagesUserControl();

		public void AddEDocsMenuItems(ZForm mainForm, AsycudaManifestHeader header) => AddEDocsMenuItemsCore(mainForm, header);
		protected virtual void AddEDocsMenuItemsCore(ZForm mainForm, AsycudaManifestHeader header) { }
	}
}
