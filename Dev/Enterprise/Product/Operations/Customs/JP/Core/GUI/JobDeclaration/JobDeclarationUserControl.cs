using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			Controls.Remove(base.ImporterOrganisationControl);
			Controls.Remove(base.SupplierOrganisationControl);
			JE_MessageSubTypeBoundDropDownEdit.Visible = false;
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			SetFieldVisibility();
			SetImporterAndSupplierControlLabels();
		}

		void SetImporterAndSupplierControlLabels()
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				if (declaration.IsImport)
				{
					ImporterDocAddressControl.CaptionResourceString = Res.GetData("1D8F092B-4B06-4D0E-842E-0C270F68916A", "Importer");
					SupplierDocAddressControl.CaptionResourceString = Res.GetData("740FFB36-A0F9-4D6C-A84F-33A27223A2E6", "Shipper");
				}
				else if (declaration.IsExport)
				{
					ImporterDocAddressControl.CaptionResourceString = Res.GetData("6D42CB09-4186-4C4D-9334-C63E15EEBD8B", "Consignee");
					SupplierDocAddressControl.CaptionResourceString = Res.GetData("DB66B00A-A34F-4891-A9B1-FBBA2D0BDF02", "Exporter");
				}
				UpdateCaption();
			}
		}

		void UpdateCaption()
		{
			ImporterDocAddressControl.SetLabelCaptionVisible(false);
			ImporterDocAddressControl.SetLabelCaptionVisible(true);
			SupplierDocAddressControl.SetLabelCaptionVisible(false);
			SupplierDocAddressControl.SetLabelCaptionVisible(true);
		}

		void SetFieldVisibility()
		{
			var isExport = Declaration.IsExport;
			FinalDestinationFindBox.ShowDescriptionBox = !isExport;
			FinalDestinationNameTextBox.Visible = isExport;
			ArrivalAtLoadingDateEdit.Visible = isExport;
			if (isExport)
			{
				JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 114, true);
			}
			else
			{
				JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 88, true);
			}
			var isSea = Declaration.IsSea;
			ReceiptModeDropEdit.Visible = isExport && isSea;
			DeliveryModeDropEdit.Visible = isExport && isSea;
			RadioCallSignCodeFindBox.Visible = VesselFindBox.Visible;
		}

		JobDeclaration Declaration => (JobDeclaration)JobDeclaration;
	}
}
