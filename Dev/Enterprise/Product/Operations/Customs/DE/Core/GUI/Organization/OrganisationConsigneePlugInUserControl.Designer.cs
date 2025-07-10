using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	partial class OrganisationConsigneePlugInUserControl : ZUserControl
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
			this.DEPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.VatClaimBackDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DEPanel.SuspendLayout();
			this.VatClaimBackDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.DEOrgImpAddInfo);
			// 
			// DEPanel
			// 
			this.DEPanel.Controls.Add(this.VatClaimBackDropEdit);
			this.DEPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DEPanel.Name = "DEPanel";
			this.DEPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DEPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
			this.DEPanel.TabIndex = 2;
			this.DEPanel.Text = "DE";
			// 
			// VatClaimBackDropEdit
			// 
			this.VatClaimBackDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VatClaimBackDropEdit, "ZO_VATClaimBack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.DEOrgImpAddInfo)(null)).ZO_VATClaimBack)));
			this.VatClaimBackDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("C9377597-5C17-460B-9AEA-679325C7476F", "VAT Claim Back");
			this.VatClaimBackDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 6, true);
			this.VatClaimBackDropEdit.Name = "VatClaimBackDropEdit";
			this.VatClaimBackDropEdit.PreBoundMaxLength = 1;
			this.VatClaimBackDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.VatClaimBackDropEdit.TabIndex = 5;
			// 
			// OrganisationConsigneePlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DEPanel);
			this.Name = "OrganisationConsigneePlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DEPanel.ResumeLayout(false);
			this.DEPanel.PerformLayout();
			this.VatClaimBackDropEdit.ResumeLayout(true);
			this.VatClaimBackDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel DEPanel;
		ZDropEdit VatClaimBackDropEdit;
	}
}
