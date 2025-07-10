using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class ECCCWasteUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.LPCOGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LPCOGridUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.IntendedUseCodeDropEdit.SuspendLayout();
			this.ConsigneeAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.LPCOGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ECCCPGAHeader);
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 198, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.DetailsGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.LPCOGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 330, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(129);
			this.SplitContainer.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.IntendedUseCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ConsigneeAddressControl);
			this.DetailsGroupBox.Controls.Add(this.ManufacturerAddressControl);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 109, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			// 
			// IntendedUseCodeDropEdit
			// 
			this.IntendedUseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_IntendedUseCode)));
			this.IntendedUseCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5d88beca-870a-4468-8cdf-14dd8deaacc9", "Intended Use Code", "Not mandatory, but if a code is provided under base codes 050 and 160, then that triggers that the goods/cargo are hazardous and therefore a notice number and movement document number must be provided.");
			this.IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 19, true);
			this.IntendedUseCodeDropEdit.Name = "IntendedUseCodeDropEdit";
			this.IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.IntendedUseCodeDropEdit.TabIndex = 0;
			// 
			// ConsigneeAddressControl
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeAddressControl, "InvoiceLine.JI_OA_ConsigneeAddress");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).InvoiceLine.JI_OA_ConsigneeAddress)));
			this.ConsigneeAddressControl.AllowDrop = true;
			this.ConsigneeAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("24b69819-0a37-4565-a650-22711e8af53a", "Ultimate Consignee", "Information is used to track shipments of hazardous waste or recyclable material as ECCC must know where the shipment is destined and who is supposed to take ownership of it at destination. This information can be provided at the invoice level or commodity level.");
			this.ConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 45, true);
			this.ConsigneeAddressControl.Name = "ConsigneeAddressControl";
			this.ConsigneeAddressControl.PopupCaption = "";
			this.ConsigneeAddressControl.ReadOnly = false;
			this.ConsigneeAddressControl.ShowAddress = false;
			this.ConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ConsigneeAddressControl.TabIndex = 1;
			// 
			// ManufacturerAddressControl
			// 
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "OA_Manufacturer");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).OA_Manufacturer)));
			this.ManufacturerAddressControl.BindToOrgList = "RequirementsParent.ManufacturersLookup";
			this.ManufacturerAddressControl.AllowDrop = true;
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a5d4723c-2607-4165-82d3-46dee88ab337", "Generator", "Information is used to track shipments of hazardous waste or recyclable material as ECCC must know the origin of the shipment and who has ownership at the shipping site. This information can be provided at the invoice level or commodity level.");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 71, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ReadOnly = false;
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ManufacturerAddressControl.TabIndex = 2;
			// 
			// LPCOGroupBox
			// 
			this.LPCOGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LPCOGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCOGroupBox.Name = "LPCOGroupBox";
			this.LPCOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 157, true);
			this.LPCOGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGroupBox.TabIndex = 1;
			this.LPCOGroupBox.TabStop = false;
			this.LPCOGroupBox.Text = "LPCOs";
			// 
			// ECCCWasteUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "ECCCWasteUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 330, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.IntendedUseCodeDropEdit.ResumeLayout(true);
			this.IntendedUseCodeDropEdit.PerformLayout();
			this.ConsigneeAddressControl.ResumeLayout(true);
			this.ConsigneeAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.LPCOGroupBox.ResumeLayout(false);
			this.LPCOGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox LPCOGroupBox;
		private ZAddressControl ManufacturerAddressControl;
		private ZAddressControl ConsigneeAddressControl;
		private ZDropEdit IntendedUseCodeDropEdit;
		internal LPCOGridUserControl LPCOGridUserControl;
	}
}
