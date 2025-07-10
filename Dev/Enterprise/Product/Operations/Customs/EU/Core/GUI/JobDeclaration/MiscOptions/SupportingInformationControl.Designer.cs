namespace Enterprise.Customs.EU.GUI
{
	partial class SupportingInformationControl
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
			this.components = new System.ComponentModel.Container();
			this.SupportingInformationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.SupportingDocumentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.AdditionalInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.additionalInfosUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.PreviousDocumentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviousDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.GuaranteesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GuaranteesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingInformationTabControl.SuspendLayout();
			this.SupportingDocumentTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.PreviousDocumentTabPage.SuspendLayout();
			this.GuaranteesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// SupportingInformationTabControl
			// 
			this.SupportingInformationTabControl.Controls.Add(this.SupportingDocumentTabPage);
			this.SupportingInformationTabControl.Controls.Add(this.AdditionalInfoTabPage);
			this.SupportingInformationTabControl.Controls.Add(this.PreviousDocumentTabPage);
			this.SupportingInformationTabControl.Controls.Add(this.GuaranteesTabPage);
			this.SupportingInformationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupportingInformationTabControl.Name = "SupportingInformationTabControl";
			this.SupportingInformationTabControl.SelectedIndex = 0;
			this.SupportingInformationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 305, true);
			this.SupportingInformationTabControl.TabIndex = 6;
			// 
			// SupportingDocumentTabPage
			// 
			this.SupportingDocumentTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("75c0a8ae-95a9-4f53-ac70-805c1c4b1946", "[44] Supporting Documents");
			this.SupportingDocumentTabPage.Controls.Add(this.SupportingDocumentsUserControl);
			this.SupportingDocumentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentTabPage.Name = "SupportingDocumentTabPage";
			this.SupportingDocumentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 278, true);
			this.SupportingDocumentTabPage.TabIndex = 0;
			this.SupportingDocumentTabPage.UseVisualStyleBackColor = true;
			// 
			// SupportingDocumentsUserControl
			// 
			this.SupportingDocumentsUserControl.AllowDrop = true;
			this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
			this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 272, true);
			this.SupportingDocumentsUserControl.TabIndex = 0;
			// 
			// AdditionalInfoTabPage
			// 
			this.AdditionalInfoTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("80485898-164f-4820-bce5-86f30231eb95", "[44] Additional Info");
			this.AdditionalInfoTabPage.Controls.Add(this.additionalInfosUserControl);
			this.AdditionalInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalInfoTabPage.Name = "AdditionalInfoTabPage";
			this.AdditionalInfoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 278, true);
			this.AdditionalInfoTabPage.TabIndex = 1;
			this.AdditionalInfoTabPage.UseVisualStyleBackColor = true;
			// 
			// additionalInfosUserControl
			// 
			this.additionalInfosUserControl.AllowDrop = true;
			this.additionalInfosUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalInfosUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.additionalInfosUserControl.Name = "additionalInfosUserControl";
			this.additionalInfosUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 272, true);
			this.additionalInfosUserControl.TabIndex = 0;
			// 
			// PreviousDocumentTabPage
			// 
			this.PreviousDocumentTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("245f85ed-cc6c-499e-a7d3-569033b8bff5", "[40] Previous Docs");
			this.PreviousDocumentTabPage.Controls.Add(this.PreviousDocumentsUserControl);
			this.PreviousDocumentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviousDocumentTabPage.Name = "PreviousDocumentTabPage";
			this.PreviousDocumentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviousDocumentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 278, true);
			this.PreviousDocumentTabPage.TabIndex = 2;
			this.PreviousDocumentTabPage.UseVisualStyleBackColor = true;
			// 
			// previousDocumentsUserControl
			// 
			this.PreviousDocumentsUserControl.AllowDrop = true;
			this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
			this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 272, true);
			this.PreviousDocumentsUserControl.TabIndex = 0;
			// 
			// GuaranteesTabPage
			// 
			this.GuaranteesTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("ddd9e6d2-9ae9-473c-8237-ccb128407684", "[52] Guarantees");
			this.GuaranteesTabPage.Controls.Add(this.GuaranteesUserControl);
			this.GuaranteesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GuaranteesTabPage.Name = "GuaranteesTabPage";
			this.GuaranteesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GuaranteesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 278, true);
			this.GuaranteesTabPage.TabIndex = 3;
			this.GuaranteesTabPage.UseVisualStyleBackColor = true;
			// 
			// GuaranteesUserControl
			// 
			this.GuaranteesUserControl.AllowDrop = true;
			this.GuaranteesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GuaranteesUserControl.Name = "GuaranteesUserControl";
			this.GuaranteesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 272, true);
			this.GuaranteesUserControl.TabIndex = 0;
			// 
			// SupportingInformationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingInformationTabControl);
			this.Name = "SupportingInformationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 311, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingInformationTabControl.ResumeLayout(false);
			this.SupportingInformationTabControl.PerformLayout();
			this.SupportingDocumentTabPage.ResumeLayout(false);
			this.SupportingDocumentTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.PreviousDocumentTabPage.ResumeLayout(false);
			this.PreviousDocumentTabPage.PerformLayout();
			this.GuaranteesTabPage.ResumeLayout(false);
			this.GuaranteesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZTabControl SupportingInformationTabControl;
		protected ZArchitecture.GUI.ZTabPage SupportingDocumentTabPage;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentsUserControl;
		protected ZArchitecture.GUI.ZTabPage AdditionalInfoTabPage;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl additionalInfosUserControl;
		protected ZArchitecture.GUI.ZTabPage PreviousDocumentTabPage;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl PreviousDocumentsUserControl;
		protected ZArchitecture.GUI.ZTabPage GuaranteesTabPage;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl GuaranteesUserControl;
	}
}
