using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class H7ApplicationGUIProvider : ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(H7ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override ResourceString GetAsycudaMenuCaptionCore() => ResString.GetMultilingualString("40a11275-3e26-483f-9083-e11ab06308ca", "&Messaging");

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new EUH7ManifestLayouts();

		protected override IEnumerable<ZMenuItem> GetActionsExtraMenuItemsCore()
		{
			var caption = ResString.GetMultilingualString("550b14fd-20ab-4244-9df4-cf618308d13f", "Convert to Stand Alone Declaration");
			yield return new ZMenuItem(caption, H7BillsModuleView);
		}

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new EUH7BillPartiesLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new EUH7BillLayouts();

		public IPanelLayoutProvider GetH7ItemDetailsLayout() => GetItemDetailsLayoutCore();

		protected virtual IPanelLayoutProvider GetItemDetailsLayoutCore() => new EUH7ItemDetailsLayouts();

		public IPanelLayoutProvider GetH7PackDetailsLayout() => GetPackDetailsLayoutCore();

		protected virtual IPanelLayoutProvider GetPackDetailsLayoutCore() => new EUH7PackDetailsLayouts();

		protected override void CustomizeBillsGridCore(ZGridWithDynamicColumnHandler billsGrid)
		{
			base.CustomizeBillsGridCore(billsGrid);

			var transportValueResourceStringData = Res.GetData("24593769-c7ec-42e5-9449-b450cf537ae3", "Transport Value");
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_TransportValue, transportValueResourceStringData);
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency, transportValueResourceStringData);

			billsGrid.MouseDoubleClick -= BillsGrid_DoubleClick;
			billsGrid.MouseDoubleClick += BillsGrid_DoubleClick;
			billsGrid.ContextMenu.Popup -= onPopUp;
			billsGrid.ContextMenu.Popup += onPopUp;
			void onPopUp(object s, EventArgs e) => BillsGridContextMenu_Popup(billsGrid);
		}

		void H7BillsModuleView(object sender, EventArgs e)
		{
			var menu = sender as ZMenuItem;
			var mainForm = menu.GetMainMenu()?.GetForm() as ZForm;
			var header = mainForm.BusinessEntity as AsycudaManifestHeader;

			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.EUH7Bill);
			var filters = new FilterBusinessObjectDefaults
			{
				new FilterBusinessObjectDefault("Job Reference", "Property", header.AMA_JobReference, false)
			};
			module.FilterBusinessObject.SetExternalDefaults(filters);

			using (var popup = new EmbeddedModulePopup(module, null, true))
			{
				popup.RequireAtLeastOneItemToBeSelected = false;
				ZFormModaliser.ShowDialogWithoutDispose(popup);
			}
		}

		void BillsGrid_DoubleClick(object sender, MouseEventArgs e)
		{
			var billsGrid = sender as ZGridWithDynamicColumnHandler;
			var bill = billsGrid.GetCurrent() as AsycudaBill;
			var result = GridEntityFormOpener.OpenForm(billsGrid, e, () => billsGrid.GetCurrent(), ControllerIDs.Customs.EU.EUH7Bill);

			billFormClosed = (sender, e) =>
			{
				if (bill != null)
				{
					bill.Reload();
					bill.LockBillIfConverted();
				}

				result.Form.Closed -= billFormClosed;
			};

			if (result.Form != null)
			{
				result.Form.Closed += billFormClosed;
			}
		}

		EventHandler billFormClosed;

		void BillsGridContextMenu_Popup(ZGridWithDynamicColumnHandler billGrid)
		{
			if (menuItemConvertToFormalDeclaration == null)
			{
				menuItemConvertToFormalDeclaration = new ConvertToStandAloneDeclarationMenuItem(() => billGrid.SelectedElements);
				billGrid.ContextMenu.MenuItems.Add(menuItemConvertToFormalDeclaration);
			}
		}

		MenuItem menuItemConvertToFormalDeclaration;

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new AdditionalDocumentsUserControl();
			yield return new SupportingDocumentsUserControl();
			yield return new PreviousDocumentsUserControl();
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new EUH7PackUserControl();
			yield return new EUH7ItemUserControl();
			yield return new AdditionalDocumentsUserControl();
			yield return new SupportingDocumentsUserControl();
			yield return new PreviousDocumentsUserControl();
		}

		protected override void AddBillPluginsCore(IFormPlugInsProvider form)
		{
			var logsTabPage = form.TopLevelTabControl?.GetTabPage("logsTabPage");
			var requestedIndex = logsTabPage != null ? form.TopLevelTabControl.TabPages.IndexOf(logsTabPage) : -1;
			form.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.eDocsPlugIn, requestedIndex);
		}

		protected override string GetBillFormCaptionCore(ASYCUDA.Business.AsycudaBill bill)
		{
			string caption = null;
			if (bill != null)
			{
				caption = Res.GetString("20824c7a-0d7a-4b08-ac0c-018759f03c88", "Bill {0}", bill.ABL_BillNumber);
			}

			return caption;
		}

		public IEnumerable<IAdditionalTabPage> GetItemAdditionalTabPageUserControl() => GetItemAdditionalTabPageUserControlCore();

		protected virtual IEnumerable<IAdditionalTabPage> GetItemAdditionalTabPageUserControlCore()
		{
			yield return new EUH7ItemPacksUserControl();
			yield return new AdditionalDocumentsUserControl();
			yield return new SupportingDocumentsUserControl();
			yield return new PreviousDocumentsUserControl();
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					new[]
					{
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
					}
				}
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var mrnTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			mrnTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.MovementReferenceNumber;
			mrnTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return mrnTextBoxColumnStyleInfo;

			var goodsValueResourceStringData = Res.GetData("f50de7b8-9a6e-4bd0-9976-d08c920d5742", "Goods Value (Total)");
			var goodsValueColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			goodsValueColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_GoodsValue;
			goodsValueColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			goodsValueColumnStyleInfo.GroupName = goodsValueResourceStringData;
			yield return goodsValueColumnStyleInfo;

			var goodsValueCurrencyColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			goodsValueCurrencyColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency;
			goodsValueCurrencyColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			goodsValueCurrencyColumnStyleInfo.GroupName = goodsValueResourceStringData;
			yield return goodsValueCurrencyColumnStyleInfo;

			var incotermColumnStyleInfo = new ZDropEditColumnStyleInfo();
			incotermColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_Incoterm;
			incotermColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			incotermColumnStyleInfo.IsVisible = false;
			yield return incotermColumnStyleInfo;

			var additionalProcedureColumnStyleInfo = new ZDropEditColumnStyleInfo();
			additionalProcedureColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_Procedure;
			additionalProcedureColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return additionalProcedureColumnStyleInfo;

			var standAloneDeclarationReferenceColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			standAloneDeclarationReferenceColumnStyleInfo.ColumnName = "EntrySummaryReferenceNumber";
			standAloneDeclarationReferenceColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			standAloneDeclarationReferenceColumnStyleInfo.IsReadOnly = true;
			yield return standAloneDeclarationReferenceColumnStyleInfo;

			var containerNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			containerNumberColumnStyleInfo.ColumnName = "ContainerNumber";
			containerNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			containerNumberColumnStyleInfo.IsVisible = false;
			yield return containerNumberColumnStyleInfo;

			var containerModeColumnStyleInfo = new ZDropEditColumnStyleInfo();
			containerModeColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_ContainerMode;
			containerModeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			containerModeColumnStyleInfo.IsVisible = false;
			yield return containerModeColumnStyleInfo;

			var messageStatusColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageStatusColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_MessageStatus;
			messageStatusColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			messageStatusColumnStyleInfo.IsReadOnly = true;
			yield return messageStatusColumnStyleInfo;

			var customStatusColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			customStatusColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_BillStatus;
			customStatusColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			customStatusColumnStyleInfo.IsReadOnly = true;
			yield return customStatusColumnStyleInfo;
		}

		public IReadOnlyDictionary<bool, string[]> GetItemsGridColumnAvailability(AsycudaManifestHeader header) => GetItemsGridColumnAvailabilityCore(header);

		protected virtual IReadOnlyDictionary<bool, string[]> GetItemsGridColumnAvailabilityCore(AsycudaManifestHeader header) => new Dictionary<bool, string[]>();

		public void CustomizeItemsGrid(ZGrid itemsGrid, AsycudaManifestHeader header) => CustomizeItemsGridCore(itemsGrid, header);

		protected virtual void CustomizeItemsGridCore(ZGrid itemsGrid, AsycudaManifestHeader header)
		{
			var tariffColumnStyle = itemsGrid.ColumnStyles.ToArray().FirstOrDefault(x => x is TariffColumnStyleInfo) as TariffColumnStyleInfo;
			if (tariffColumnStyle != null)
			{
				tariffColumnStyle.GetDataGrouping = () => header.ApplicationBusinessProvider?.PackedItemTariffDataGrouping ?? ZString.Empty;
				tariffColumnStyle.GetEffectiveDate = () => header.ApplicationBusinessProvider?.GetEffectiveDateForDutyRate(header) ?? ZDateTime.Empty;
				tariffColumnStyle.GetTariffType = () => header.ApplicationBusinessProvider?.PackedItemTariffType ?? ZString.Empty;
			}
		}

		public void CustomizeTariffFindBox(TariffFindBox tariffFindBox, AsycudaManifestHeader header) => CustomizeTariffFindBoxCore(tariffFindBox, header);

		protected virtual void CustomizeTariffFindBoxCore(TariffFindBox tariffFindBox, AsycudaManifestHeader header)
		{
			tariffFindBox.GetDataGrouping = () => header.ApplicationBusinessProvider?.PackedItemTariffDataGrouping ?? ZString.Empty;
			tariffFindBox.GetEffectiveDate = () => header.ApplicationBusinessProvider?.GetEffectiveDateForDutyRate(header) ?? ZDateTime.Empty;
			tariffFindBox.GetTariffType = () => header.ApplicationBusinessProvider?.PackedItemTariffType ?? ZString.Empty;
		}

		public override bool ShouldPositionMessagesTabAccordingToMessageLevel => true;

		protected override IAdditionalTabPage GetBillMessagesUserControlCore()
		{
			return new EUH7MessagesUserControl();
		}
	}
}
