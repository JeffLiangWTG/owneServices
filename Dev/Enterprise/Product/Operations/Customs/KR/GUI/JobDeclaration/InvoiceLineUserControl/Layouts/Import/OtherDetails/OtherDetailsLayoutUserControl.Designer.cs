
namespace Enterprise.Customs.KR.GUI
{
	partial class OtherDetailsLayoutUserControl
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
            this.InspectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DeliveryCompanyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ProductDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.Agency3CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.Agency2CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.Agency1CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.InspectionDropEdit.SuspendLayout();
            this.DeliveryCompanyDropEdit.SuspendLayout();
            this.ProductDropEdit.SuspendLayout();
            this.Agency3CodeFindBox.SuspendLayout();
            this.Agency2CodeFindBox.SuspendLayout();
            this.Agency1CodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // InspectionDropEdit
            // 
            this.InspectionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.InspectionDropEdit, "FilteredInvoiceLines.JI_MightRequireInspection");
            this.InspectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 150, true);
            this.InspectionDropEdit.Name = "InspectionDropEdit";
            this.InspectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.InspectionDropEdit.TabIndex = 0;
            // 
            // DeliveryCompanyDropEdit
            // 
            this.DeliveryCompanyDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeliveryCompanyDropEdit, "FilteredInvoiceLines.JI_CourierCargoSelectivityIndicator");
            this.DeliveryCompanyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 176, true);
            this.DeliveryCompanyDropEdit.Name = "DeliveryCompanyDropEdit";
            this.DeliveryCompanyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.DeliveryCompanyDropEdit.TabIndex = 1;
            // 
            // LineNoCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "FilteredInvoiceLines.JI_ParentLine");
            this.LineNoCalcEdit.DecimalPlaces = 2;
            this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 46, true);
            this.LineNoCalcEdit.Name = "LineNoCalcEdit";
            this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.LineNoCalcEdit.TabIndex = 3;
            this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.LineNoCalcEdit.TrackDisposedAccess = true;
            // 
            // ProductDropEdit
            // 
            this.ProductDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ProductDropEdit, "FilteredInvoiceLines.JI_ProductTypeCode");
            this.ProductDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 20, true);
            this.ProductDropEdit.Name = "ProductDropEdit";
            this.ProductDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.ProductDropEdit.TabIndex = 2;
            // 
            // Agency3CodeFindBox
            // 
            this.Agency3CodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.Agency3CodeFindBox, "FilteredInvoiceLines.JI_PostClearanceProcedureGA3");
            this.Agency3CodeFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0227b01d-de28-48c2-9cb1-5fe320d156b0", "Post Clearance Agency 3");
            this.Agency3CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 124, true);
            this.Agency3CodeFindBox.Name = "Agency3CodeFindBox";
            this.Agency3CodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.Agency3CodeFindBox.ParentType = null;
            this.Agency3CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.Agency3CodeFindBox.TabIndex = 6;
            // 
            // Agency2CodeFindBox
            // 
            this.Agency2CodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.Agency2CodeFindBox, "FilteredInvoiceLines.JI_PostClearanceProcedureGA2");
            this.Agency2CodeFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e140abd7-a240-4db8-b57e-7dbee14cb857", "Post Clearance Agency 2");
            this.Agency2CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 98, true);
            this.Agency2CodeFindBox.Name = "Agency2CodeFindBox";
            this.Agency2CodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.Agency2CodeFindBox.ParentType = null;
            this.Agency2CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.Agency2CodeFindBox.TabIndex = 5;
            // 
            // Agency1CodeFindBox
            // 
            this.Agency1CodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.Agency1CodeFindBox, "FilteredInvoiceLines.JI_PostClearanceProcedureGA1");
            this.Agency1CodeFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("f58a814b-57bc-4e99-ba12-91e65a1cd703", "Post Clearance Agency 1");
            this.Agency1CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 72, true);
            this.Agency1CodeFindBox.Name = "Agency1CodeFindBox";
            this.Agency1CodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.Agency1CodeFindBox.ParentType = null;
            this.Agency1CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.Agency1CodeFindBox.TabIndex = 4;
            // 
            // OtherDetailsLayoutUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.Agency3CodeFindBox);
            this.Controls.Add(this.Agency2CodeFindBox);
            this.Controls.Add(this.Agency1CodeFindBox);
            this.Controls.Add(this.LineNoCalcEdit);
            this.Controls.Add(this.ProductDropEdit);
            this.Controls.Add(this.DeliveryCompanyDropEdit);
            this.Controls.Add(this.InspectionDropEdit);
            this.Name = "OtherDetailsLayoutUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 229, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.InspectionDropEdit.ResumeLayout(true);
            this.InspectionDropEdit.PerformLayout();
            this.DeliveryCompanyDropEdit.ResumeLayout(true);
            this.DeliveryCompanyDropEdit.PerformLayout();
            this.ProductDropEdit.ResumeLayout(true);
            this.ProductDropEdit.PerformLayout();
            this.Agency3CodeFindBox.ResumeLayout(true);
            this.Agency3CodeFindBox.PerformLayout();
            this.Agency2CodeFindBox.ResumeLayout(true);
            this.Agency2CodeFindBox.PerformLayout();
            this.Agency1CodeFindBox.ResumeLayout(true);
            this.Agency1CodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit InspectionDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DeliveryCompanyDropEdit;
		internal ZArchitecture.ZCalcEdit LineNoCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit ProductDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox Agency3CodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox Agency2CodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox Agency1CodeFindBox;
	}
}
