using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();

			ChangeControlsVisibility();

			SupplierOrganisationControl.OrgAddressFormatter = GetAddressFormatter();
			ImporterOrganisationControl.OrgAddressFormatter = GetAddressFormatter();
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (JobDeclaration != null)
			{
				JobDeclaration.JE_MessageTypeInfo.ValueChanged -= DetailsVisibility_ValueChanged;
				JobDeclaration.JE_MessageSubTypeInfo.ValueChanged -= DetailsVisibility_ValueChanged;
				JobDeclaration.JE_TransportModeInfo.ValueChanged -= DetailsVisibility_ValueChanged;
				
				JobDeclaration.JE_MessageTypeInfo.ValueChanged += DetailsVisibility_ValueChanged;
				JobDeclaration.JE_MessageSubTypeInfo.ValueChanged += DetailsVisibility_ValueChanged;
				JobDeclaration.JE_TransportModeInfo.ValueChanged += DetailsVisibility_ValueChanged;

				ChangeLayoutAndSizeLocation();
				UpdateCaptionsForControlsWithMultipleKeys();
			}
		}
		Func<BusinessObjectFactory, OrgAddress, AddressFormatter> GetAddressFormatter()
		{
			return (factory, orgAddress) => new KRAddressFormatter(factory, orgAddress);
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			SetImporterAndSupplierControlLabels();
			UpdateShipmentDetailsLayout();
		}

		protected override void ChangeControlsVisibility()
		{
			DeclarationDetailsGroupBox.Visible = false;
		}

		void SetImporterAndSupplierControlLabels()
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				if (declaration.IsLocalExport)
				{
					ImporterOrganisationControl.CaptionResourceString = Res.GetData("C044C7F5-A8C9-4F42-AFA6-FC304765D0CA", "Importer");
					SupplierOrganisationControl.CaptionResourceString = Res.GetData("F60BE9C1-9567-4968-80D8-12AF1EE2AC4F", "Main Supplier");
				}
				else if (declaration.IsExport)
				{
					ImporterOrganisationControl.CaptionResourceString = Res.GetData("7B7CA986-0E0A-4C98-B28D-BACAE1E1D106", "Importer");
					SupplierOrganisationControl.CaptionResourceString = Res.GetData("4062CD64-AF8F-42A4-B2DC-EB54D043BE42", "Main Supplier");
				}
				else
				{
					ImporterOrganisationControl.CaptionResourceString = Res.GetData("3A7935BD-A958-4554-8172-6F4CE035EE34", "Importer");
					SupplierOrganisationControl.CaptionResourceString = Res.GetData("3F2405CA-8EA3-410D-BE9A-B1E9A5829738", "Main Supplier");
				}
			}
			ImporterOrganisationControl.Text = ImporterOrganisationControl.CaptionResourceString.Caption;
			SupplierOrganisationControl.Text = SupplierOrganisationControl.CaptionResourceString.Caption;
		}

		protected override void SetContainerCountAndNoOfPieces()
		{
			JE_ContainerCountCalcEdit.Visible = false;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
			if (JobDeclaration.IsImport)
			{
				JE_TotalNoOfPiecesBoundCalcEdit.Visible = true;
				JE_TotalNoOfPiecesBoundCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("B6016F5C-69DC-4B8F-84EA-99EBAEA618C7", "Units");
			}
		}

		void DetailsVisibility_ValueChanged(object sender, EventArgs e)
		{
			ChangeLayoutAndSizeLocation();
		}

		void ChangeLayoutAndSizeLocation()
		{
			AdjustSize();
			AdjustLocation();
			UpdateTransportDetailsLayout();
			UpdateShipmentDetailsLayout();
			UpdateExportSEDDetailObjectVisibility();
			UpdateCustomsDetailObjectVisibility();
		}

		void UpdateTransportDetailsLayout()
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				if (declaration.IsImport)
				{
					TransportDetailsLayoutPanel?.UpdateLayout(new IMPDeclarationTransportDetailsLayout(declaration));
				}
				else if (declaration.IsLocalExport)
				{
					TransportDetailsLayoutPanel?.UpdateLayout(new LEXDeclarationTransportDetailsLayout(declaration));
				}
				else
				{
					TransportDetailsLayoutPanel?.UpdateLayout(new EXPDeclarationTransportDetailsLayout(declaration));
				}
			}
		}

		void UpdateShipmentDetailsLayout()
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				if (declaration.IsLocalExport)
				{
					ShipmentDetailsLayoutPanel?.UpdateLayout(new LEXShipmentDetailsLayout());
				}
				else
				{
					ShipmentDetailsLayoutPanel?.UpdateLayout(new ShipmentDetailsLayout());
				}
			}
		}

		void UpdateCustomsDetailObjectVisibility()
		{
			var declaration = (JobDeclaration)JobDeclaration;
			if (declaration.IsImport)
			{
				CustomsDetailsPanel.UpdateLayout(new DeclarationCustomsDetailsLayout());
			}
			CustomsDetailsGroupBox.Visible = declaration.IsImport;
			SEDDetailsPanel.Visible = !declaration.IsImport;
		}

		void UpdateExportSEDDetailObjectVisibility()
		{
			if (JobDeclaration is JobDeclaration declaration && declaration.IsExport)
			{
				ExportSEDDetailUserControl.InspectionDateEdit.Visible = JobDeclaration.JE_MessageSubType != ExportTypeCodeList.Codes.G;
			}
		}

		protected override void Declaration_MultipleKeyToUseChanged(object sender, EventArgs e)
		{
			base.Declaration_MultipleKeyToUseChanged(sender, e);
			UpdateCaptionsForControlsWithMultipleKeys();
		}

		void UpdateCaptionsForControlsWithMultipleKeys()
		{
			RefreshCaption(ShipmentTypeGroupBox.FindSingleOrDefault<ZDropEdit>("TransactionTypeDropEdit"));
			RefreshCaption(ShipmentTypeGroupBox.FindSingleOrDefault<ZDropEdit>("MessageSubTypeDropEdit"));
		}

		protected void RefreshCaption(System.Windows.Forms.Control control)
		{
			if (control != null && !control.IsDisposed)
			{
				control.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
		}

		void AdjustSize()
		{
			var declaration = (JobDeclaration)JobDeclaration;
			if (declaration.IsExport)
			{
				ShipmentTypeGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(235);
				TransportDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(165);
				ShipmentDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(240);
				SEDDetailsPanel.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(190);
				ExportSEDDetailUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}
			else if (declaration.IsLocalExport)
			{
				ShipmentTypeGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
				TransportDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
				ShipmentDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(170);
				SEDDetailsPanel.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(270);
				LocalExportSEDDetailUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}
			else
			{
				ShipmentTypeGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(285);
				TransportDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(215);
				ShipmentDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(240);
			}
			ExportSEDDetailUserControl.Visible = declaration.IsExport;
			LocalExportSEDDetailUserControl.Visible = declaration.IsLocalExport;
		}

		void AdjustLocation()
		{
			var declaration = (JobDeclaration)JobDeclaration;

			var transportDetailsGroupBoxHeight = TransportDetailsGroupBox.Height + DeclarationDetailsGroupBox.Location.Y;

			TransportDetailsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(TransportDetailsGroupBox.Location.X, DeclarationDetailsGroupBox.Location.Y, false);
			ShipmentDetailsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(ShipmentDetailsGroupBox.Location.X, transportDetailsGroupBoxHeight, false);
			RightTabControl.Location = ControlDpiScalingHelper.NewScaledPoint(RightTabControl.Location.X, DeclarationDetailsGroupBox.Location.Y, false);
			SEDDetailsPanel.Location = ControlDpiScalingHelper.NewScaledPoint(SEDDetailsPanel.Location.X, ShipmentDetailsGroupBox.Location.Y + ShipmentDetailsGroupBox.Height, false);
			CustomsDetailsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(CustomsDetailsGroupBox.Location.X, ShipmentDetailsGroupBox.Location.Y + ShipmentDetailsGroupBox.Height, false);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ShipmentTypeLayoutPanel.TabStop = true;
			OrganisationDetailsLayoutPanel.TabStop = true;
		}
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (JobDeclaration is JobDeclaration declaration)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= DetailsVisibility_ValueChanged;
					declaration.JE_MessageSubTypeInfo.ValueChanged -= DetailsVisibility_ValueChanged;
					declaration.JE_TransportModeInfo.ValueChanged -= DetailsVisibility_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}
	}
}
