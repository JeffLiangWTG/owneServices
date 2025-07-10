using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class JobDeclarationUserControl : EUJobDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			ReorderTabPages();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var borderTransportMeansDropEdit = ShipmentTypeLayoutPanel.FindSingle<Control>("BorderTransportMeansDropEdit");
			ShipmentTypeLayoutPanel.FindSingle<Control>("TransportModeDropEdit").AllowOverlap(borderTransportMeansDropEdit);

			var containerModeDropEdit = ShipmentTypeLayoutPanel.FindSingle<Control>("ContainerModeDropEdit");
			borderTransportMeansDropEdit.AllowOverlap(containerModeDropEdit);
		}

		protected override Type GetCustomsOfficesUserControlType() => typeof(CustomsOfficesUserControl);

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			var isExport = JobDeclaration.IsExport;
			var isImport = JobDeclaration.IsImport;
			var isUCC6AndIsImport = JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6AndIsImport;
			var isImportV1orV2 = isImport && (JobDeclaration is JobDeclaration jobDeclaration1 && jobDeclaration1.IsV1OrV2ApplicationCode);

			PresentationUserControl.Visible = isExport;
			JE_SubLocationOfGoodsTextBox.Visible = !isExport && !isImport;
			JE_Calc_LocationOfGoodsCtryTextBox.Visible = !isExport && !isImport;
			JE_UCRTextBox.Visible = isImport;
			RegionOfDestinationDropEdit.Visible = isUCC6AndIsImport;
			JE_LocationOfGoodsCodeFindBox.Visible = !isImport;
			JE_LocationQualifierDropEdit.Visible = !isImport;
			JE_LocationOtherInformationDropEdit.Visible = !isImport;

			if (isExport)
			{
				GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 88, true);
				GoodsDescriptionTextBox.TabIndex = 11;

				JE_LocationOtherInformationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 113, true);
				JE_LocationOtherInformationDropEdit.TabIndex = 11;

				JE_LocationQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 113, true);
				JE_LocationQualifierDropEdit.TabIndex = 12;

				JE_LocationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 137, true);
				JE_LocationOfGoodsCodeFindBox.TabIndex = 13;

				OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 161, true);
				OwnersReferenceTextBox.TabIndex = 14;
				TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 161, true);
				TotalNoOfPacksCalcDropEdit.TabIndex = 15;

				JE_ExportDateBoundDateEdit2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long;

				IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 184, true);
				IncoTermDropEdit.TabIndex = 16;

				IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 183, true);
				IncoTermExplainButton.TabIndex = 17;

				JE_ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 184, true);
				JE_ShipmentIncoTermPlaceTextBox.TabIndex = 18;

				WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 207, true);
				WeightzCalcDropEdit.TabIndex = 20;

				VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 207, true);
				VolumeCalcDropEdit.TabIndex = 19;

				ImporterDocAddress.CaptionResourceString = Res.GetData("5c8b8f33-e47d-40d2-b879-0da86afd99b7", "Importer/Consignee", "[13 03 000 000] Importer/Consignee");
				SupplierDocAddress.CaptionResourceString = Res.GetData("9ddc264e-9d50-42e0-92b1-1a8e0681badd", "Supplier/Exporter", "[13 01 000 000] Supplier/Exporter");
			}
			else
			{
				if (isImport)
				{
					RegionOfDestinationDropEdit.TabIndex = 8;

					GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 113, true);
					GoodsDescriptionTextBox.TabIndex = 9;

					JE_ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 137, true);
					JE_ShipmentIncoTermPlaceTextBox.TabIndex = 10;

					ZG_AgreedPlaceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 137, true);
					ZG_AgreedPlaceCodeDropEdit.TabIndex = 11;

					ZG_AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 137, true);
					ZG_AgreedPlaceCodeFindBox.TabIndex = 11;

					IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 161, true);
					IncoTermDropEdit.TabIndex = 13;

					IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 161, true);
					IncoTermExplainButton.TabIndex = 14;

					OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 184, true);
					OwnersReferenceTextBox.TabIndex = 17;
					TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 184, true);
					TotalNoOfPacksCalcDropEdit.TabIndex = 18;

					WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 207, true);
					WeightzCalcDropEdit.TabIndex = 19;

					VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 232, true);
					VolumeCalcDropEdit.TabIndex = 22;

					JE_DateOfArrivalBoundDateEdit2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long;
				}
				else
				{
					GoodsDescriptionTextBox.TabIndex = 8;

					JE_ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 207, true);
					JE_ShipmentIncoTermPlaceTextBox.TabIndex = 20;

					ZG_AgreedPlaceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 207, true);
					ZG_AgreedPlaceCodeDropEdit.TabIndex = 21;

					ZG_AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 207, true);
					ZG_AgreedPlaceCodeFindBox.TabIndex = 21;

					IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 184, true);
					IncoTermDropEdit.TabIndex = 17;

					IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 183, true);
					IncoTermExplainButton.TabIndex = 18;

					JE_LocationOtherInformationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 136, true);
					JE_LocationOtherInformationDropEdit.TabIndex = 12;

					JE_LocationQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 136, true);
					JE_LocationQualifierDropEdit.TabIndex = 11;

					WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 184, true);
					WeightzCalcDropEdit.TabIndex = 16;
				}

				JE_LocationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 111, true);
				JE_LocationOfGoodsCodeFindBox.TabIndex = 9;

				JE_ExportDateBoundDateEdit2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
				if (JobDeclaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport)
				{
					ImporterDocAddress.CaptionResourceString = Res.GetData("D4C3F696-2EA9-4DCE-A016-F9E30DA35C56", "Importer", "[3/15] Importer", "[3/15 && 3/16] Importer (ID)", "[3/15] Importer & [3/16] Importer Identification Number");
					SupplierDocAddress.CaptionResourceString = Res.GetData("D1CCC8CC-C0ED-409F-82ED-5502EE65B3D4", "Exporter", "[3/1] Exporter", "[3/1 && 3/2] Exporter (ID)", "[3/1] Exporter & [3/2] Exporter Identification Number");
				}
				else
				{
					ImporterDocAddress.CaptionResourceString = Res.GetData("5c8b8f33-e47d-40d2-b879-0da86afd99b6", "Importer");
					SupplierDocAddress.CaptionResourceString = Res.GetData("9ddc264e-9d50-42e0-92b1-1a8e0681badc", "Supplier");
				}
			}
			SetAgreedPlaceCodeVisibility(null, null);
			RefreshDocAddressCaptions();
		}

		protected override void Declaration_MultipleKeyToUseChanged(object sender, EventArgs e)
		{
			base.Declaration_MultipleKeyToUseChanged(sender, e);
			RefreshCaption(IncoTermDropEdit);
			RefreshCaption(JE_ShipmentIncoTermPlaceTextBox);
		}

		protected override void SetRightTabControlSelectTab()
		{
			RightTabControl.SelectedTab = OrganisationsTabPage;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (JobDeclaration != null)
			{
				JobDeclaration.JE_ShipmentIncoTermPlaceInfo.ValueChanged += SetAgreedPlaceCodeVisibility;
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (JobDeclaration != null)
			{
				JobDeclaration.JE_ShipmentIncoTermPlaceInfo.ValueChanged -= SetAgreedPlaceCodeVisibility;
			}
		}

		void SetAgreedPlaceCodeVisibility(object sender, EventArgs e)
		{
			if (JobDeclaration is JobDeclaration declaration && declaration.AgreedPlaceUsesUNLOCO)
			{
				ZG_AgreedPlaceCodeDropEdit.Visible = false;
				ZG_AgreedPlaceCodeFindBox.Visible = true;
			}
			else
			{
				ZG_AgreedPlaceCodeDropEdit.Visible = !JobDeclaration.IsExport && !JobDeclaration.JE_ShipmentIncoTermPlace.IsEmpty;
				ZG_AgreedPlaceCodeFindBox.Visible = false;
			}
		}
		void RefreshDocAddressCaptions()
		{
			ImporterDocAddress.SetLabelCaptionVisible(false);
			ImporterDocAddress.SetLabelCaptionVisible(true);
			SupplierDocAddress.SetLabelCaptionVisible(false);
			SupplierDocAddress.SetLabelCaptionVisible(true);
		}

		void ReorderTabPages()
		{
			RightTabControl.TabPages.Remove(DocsTabPage);
			RightTabControl.TabPages.Insert(DocsTabPage, 1);
		}
	}
}
