using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AE.Manifest.GUI;

public sealed class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
{
	public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

	public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

	protected override IPanelLayoutProvider GetManifestLayoutCore() => new ManifestLayouts();

	protected override IPanelLayoutProvider GetBillLayoutCore() => new BillDetailsLayout();

	protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
	{
		yield return new AsycudaPackUserControl();
	}

	protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
	{
		foreach (var columnInfo in base.GetBillsGridExtraColumnInfosCore())
		{
			yield return columnInfo;
		}

		yield return new ZDropEditColumnStyleInfo
		{
			ColumnName = AE.Manifest.Business.AsycudaBill.Schema.Negotiable,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
		};

		yield return new ZTextBoxColumnStyleInfo
		{
			ColumnName = AE.Manifest.Business.AsycudaBill.Schema.Payer,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
		};

		var billStatusGroup = Enterprise.Customs.AE.Manifest.GUI.Res.GetData("49c3122c-5eb3-476f-85b5-0b482933ec5d", "Bill Status");

		var messageStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		messageStatusTextBoxColumnStyleInfo.IsVisible = false;
		messageStatusTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		messageStatusTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_MessageStatus;
		messageStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		messageStatusTextBoxColumnStyleInfo.IsUnavailable = false;
		yield return messageStatusTextBoxColumnStyleInfo;

		var billStatusDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
		billStatusDropEditColumnStyleInfo.IsVisible = false;
		billStatusDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		billStatusDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_BillStatus;
		billStatusDropEditColumnStyleInfo.GroupName = billStatusGroup;
		billStatusDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
		billStatusDropEditColumnStyleInfo.IsUnavailable = false;
		yield return billStatusDropEditColumnStyleInfo;

		var billStatusDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		billStatusDescriptionTextBoxColumnStyleInfo.IsVisible = false;
		billStatusDescriptionTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_BillStatusDescription;
		billStatusDescriptionTextBoxColumnStyleInfo.GroupName = billStatusGroup;
		billStatusDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		yield return billStatusDescriptionTextBoxColumnStyleInfo;
	}

	protected override IEnumerable<string> GetColumnsToRemoveInBillsGridCore()
	{
		yield return AE.Manifest.Business.AsycudaBill.Schema.ABL_MarksAndNumbers;
		yield return AE.Manifest.Business.AsycudaBill.Schema.ABL_Remarks;
		yield return AE.Manifest.Business.AsycudaBill.Schema.ABL_GoodsDescription;
	}

	protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(AsycudaManifestHeader header)
	{
		var temperatureGroup = Res.GetData("7B705E9D-D592-41D9-8100-721683333906", "Temperature");

		var containerTemperatureColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		containerTemperatureColumnStyleInfo.ColumnName = Customs.AE.Manifest.Business.AsycudaContainer.Schema.ACN_SetPointTemperature;
		containerTemperatureColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		containerTemperatureColumnStyleInfo.GroupName = temperatureGroup;
		yield return containerTemperatureColumnStyleInfo;

		var containerTemperatureUQColumnStyleInfo = new ZDropEditColumnStyleInfo();
		containerTemperatureUQColumnStyleInfo.ColumnName = Customs.AE.Manifest.Business.AsycudaContainer.Schema.ACN_SetPointTemperatureUnit;
		containerTemperatureUQColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		containerTemperatureUQColumnStyleInfo.GroupName = temperatureGroup;
		yield return containerTemperatureUQColumnStyleInfo;
	}

	protected override AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialogCore(MessageChooser messageChooser, string itemsType, string messageType) => new BillsSelectionDialog((Business.MessageChooser)messageChooser, itemsType);

	protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
	{
		var tariffColumnStyleInfo = new Universal.GUI.TariffColumnStyleInfo();
		tariffColumnStyleInfo.ColumnName = TariffBindingColumnName;
		tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return tariffColumnStyleInfo;
	}

	protected override IReadOnlyDictionary<bool, string[]> GetPacksGridColumnVisibilityCore() => new Dictionary<bool, string[]>
	{
		{ false, new[] { AsycudaPack.Schema.APA_CommodityCode } }
	};

	protected override string[] GetPacksGridColumnsOrderCore() => new[]
	{
		AsycudaPack.Schema.ContainerPK,
		AsycudaPack.Schema.APA_PackQty,
		AsycudaPack.Schema.APA_PackUQ,
		TariffBindingColumnName,
		AsycudaPack.Schema.APA_GoodsDescription,
		AsycudaPack.Schema.APA_MarksAndNumbers,
		AsycudaPack.Schema.APA_Weight,
		AsycudaPack.Schema.APA_WeightUQ,
		AsycudaPack.Schema.APA_Volume,
		AsycudaPack.Schema.APA_VolumeUQ,
	};

	const string TariffBindingColumnName = nameof(AsycudaPack.PackedItem) + "+" + AsycudaPackedItem.Schema.API_FormattedTariff;

	protected override void AddEDocsMenuItemsCore(ZForm mainForm, AsycudaManifestHeader header)
	{
		var plugIn = (eDocsPlugIn)mainForm.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
		var userControl = (eDocsUserControl)plugIn.UserControl;
		var grid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");

		userControl.OnContextMenuBuilt += (s, e) =>
		{
			grid.ContextMenu.MenuItems.Add(new SendDocumentToNAICMenuItem((s, e) => SendSupportingDocuments((Business.AsycudaManifestHeader)header, mainForm, grid)));
		};
	}

	void SendSupportingDocuments(Business.AsycudaManifestHeader header, ZForm mainForm, DocumentsZGrid grid)
	{
		if (SaveDataFirst.Confirm(header, mainForm))
		{
			var manifestWrapper = new Business.ManifestSupportingDocSendingObjectParent(header);
			var doc = grid.CurrentElement;
			using (var form = new SupportingDocSendingForm(manifestWrapper))
			{
				var defaultObject = manifestWrapper.SendingObjectsCollection.AddNew();
				defaultObject.EDoc = doc.PK;
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					var countOfMessages = new Business.DocMessageSender(manifestWrapper).SendMessages();
					if (countOfMessages > 0)
					{
						Globals.Message.Show(Res.GetString("2FE188DA-C849-493F-A774-455490AE0557", "{0} message(s) have been sent.", countOfMessages));
					}
					else
					{
						Globals.Message.Show(Res.GetString("0B7992A7-AFF0-4784-949A-833D5645B588", "Some problems occurred during message creation."));
					}
				}
			}
		}
	}
}
