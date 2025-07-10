namespace Enterprise.Customs.KR.GUI
{
	partial class ImportValuationDeclarationUserControl
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
      this.ValuationDeclarationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
      this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.ImportValuationSendingDetailsUserControl = new Enterprise.Customs.KR.GUI.ImportValuationSendingDetailsUserControl();
      this.QuestionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.QuestionGroupBoxUserControl = new Enterprise.Customs.KR.GUI.Question5To7GroupBoxUserControl();
      this.PriceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.PriceGroupBoxUserControl = new Enterprise.Customs.KR.GUI.PriceGroupBoxUserControl();
      this.MethodTwoToSixTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.ValuationDeclarationMethodTwoToSixUserControl = new Enterprise.Customs.KR.GUI.ValuationDeclarationMethodTwoToSixUserControl();
      this.MethodTwoToThreeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.MethodTwoToThreeUserControl = new Enterprise.Customs.KR.GUI.MethodTwoToThreeUserControl();
      this.MethodFourTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.MethodFourUserControl = new Enterprise.Customs.KR.GUI.MethodFourUserControl();
      this.MethodFiveToSixTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.MethodFiveToSixUserControl = new Enterprise.Customs.KR.GUI.MethodFiveToSixUserControl();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.ValuationDeclarationTabControl.SuspendLayout();
      this.DetailsTabPage.SuspendLayout();
      this.ImportValuationSendingDetailsUserControl.SuspendLayout();
      this.QuestionTabPage.SuspendLayout();
      this.QuestionGroupBoxUserControl.SuspendLayout();
      this.PriceTabPage.SuspendLayout();
      this.PriceGroupBoxUserControl.SuspendLayout();
      this.MethodTwoToSixTabPage.SuspendLayout();
      this.ValuationDeclarationMethodTwoToSixUserControl.SuspendLayout();
      this.MethodTwoToThreeTabPage.SuspendLayout();
      this.MethodTwoToThreeUserControl.SuspendLayout();
      this.MethodFourTabPage.SuspendLayout();
      this.MethodFourUserControl.SuspendLayout();
      this.MethodFiveToSixTabPage.SuspendLayout();
      this.MethodFiveToSixUserControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.ValuationDeclarationMessageSendingObjectParent);
      // 
      // ValuationDeclarationTabControl
      // 
      this.ValuationDeclarationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
      this.ValuationDeclarationTabControl.Controls.Add(this.DetailsTabPage);
      this.ValuationDeclarationTabControl.Controls.Add(this.QuestionTabPage);
      this.ValuationDeclarationTabControl.Controls.Add(this.PriceTabPage);
      this.ValuationDeclarationTabControl.Controls.Add(this.MethodTwoToSixTabPage);
      this.ValuationDeclarationTabControl.Controls.Add(this.MethodTwoToThreeTabPage);
      this.ValuationDeclarationTabControl.Controls.Add(this.MethodFourTabPage);
      this.ValuationDeclarationTabControl.Controls.Add(this.MethodFiveToSixTabPage);
      this.ValuationDeclarationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ValuationDeclarationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.ValuationDeclarationTabControl.Name = "ValuationDeclarationTabControl";
      this.ValuationDeclarationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 595, true);
      this.ValuationDeclarationTabControl.TabIndex = 0;
      // 
      // DetailsTabPage
      // 
      this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("cb649f58-9e0e-4110-9d5a-97c6d1c45bd9", "Details");
      this.DetailsTabPage.Controls.Add(this.ImportValuationSendingDetailsUserControl);
      this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
      this.DetailsTabPage.Name = "DetailsTabPage";
      this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
      this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.DetailsTabPage.TabIndex = 1;
      // 
      // ImportValuationSendingDetailsUserControl
      // 
      this.ImportValuationSendingDetailsUserControl.AllowDrop = true;
      this.ImportValuationSendingDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ImportValuationSendingDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
      this.ImportValuationSendingDetailsUserControl.Name = "ImportValuationSendingDetailsUserControl";
      this.ImportValuationSendingDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1194, 562, true);
      this.ImportValuationSendingDetailsUserControl.TabIndex = 0;
      // 
      // QuestionTabPage
      // 
      this.QuestionTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("28b3eab9-2cee-4045-95b6-613a4adc3861", "Question");
      this.QuestionTabPage.Controls.Add(this.QuestionGroupBoxUserControl);
      this.QuestionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
      this.QuestionTabPage.Name = "QuestionTabPage";
      this.QuestionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.QuestionTabPage.TabIndex = 2;
      // 
      // QuestionGroupBoxUserControl
      // 
      this.QuestionGroupBoxUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.QuestionGroupBoxUserControl, ".");
      this.QuestionGroupBoxUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.QuestionGroupBoxUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.QuestionGroupBoxUserControl.Name = "QuestionGroupBoxUserControl";
      this.QuestionGroupBoxUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.QuestionGroupBoxUserControl.TabIndex = 0;
      // 
      // PriceTabPage
      // 
      this.PriceTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("b7c485af-ed6e-48db-bc5e-c8b8d295f155", "Price Information");
      this.PriceTabPage.Controls.Add(this.PriceGroupBoxUserControl);
      this.PriceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
      this.PriceTabPage.Name = "PriceTabPage";
      this.PriceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.PriceTabPage.TabIndex = 3;
      // 
      // PriceGroupBoxUserControl
      // 
      this.PriceGroupBoxUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.PriceGroupBoxUserControl, ".");
      this.PriceGroupBoxUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.PriceGroupBoxUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.PriceGroupBoxUserControl.Name = "PriceGroupBoxUserControl";
      this.PriceGroupBoxUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.PriceGroupBoxUserControl.TabIndex = 0;
      // 
      // MethodTwoToSixTabPage
      // 
      this.MethodTwoToSixTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("ef5bab61-f6eb-4d5a-9f24-3d123d3ca863", "Method Two to Six");
      this.MethodTwoToSixTabPage.Controls.Add(this.ValuationDeclarationMethodTwoToSixUserControl);
      this.MethodTwoToSixTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
      this.MethodTwoToSixTabPage.Name = "MethodTwoToSixTabPage";
      this.MethodTwoToSixTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.MethodTwoToSixTabPage.TabIndex = 4;
      // 
      // ValuationDeclarationMethodTwoToSixUserControl
      // 
      this.ValuationDeclarationMethodTwoToSixUserControl.AllowDrop = true;
      this.ValuationDeclarationMethodTwoToSixUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ValuationDeclarationMethodTwoToSixUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.ValuationDeclarationMethodTwoToSixUserControl.Name = "ValuationDeclarationMethodTwoToSixUserControl";
      this.ValuationDeclarationMethodTwoToSixUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.ValuationDeclarationMethodTwoToSixUserControl.TabIndex = 0;
      // 
      // MethodTwoToThreeTabPage
      // 
      this.MethodTwoToThreeTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("78f9d336-b44c-49b3-9c2a-9e1973626f82", "Method Two to Three");
      this.MethodTwoToThreeTabPage.Controls.Add(this.MethodTwoToThreeUserControl);
      this.MethodTwoToThreeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
      this.MethodTwoToThreeTabPage.Name = "MethodTwoToThreeTabPage";
      this.MethodTwoToThreeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.MethodTwoToThreeTabPage.TabIndex = 5;
      // 
      // MethodTwoToThreeUserControl
      // 
      this.MethodTwoToThreeUserControl.AllowDrop = true;
      this.MethodTwoToThreeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MethodTwoToThreeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.MethodTwoToThreeUserControl.Name = "MethodTwoToThreeUserControl";
      this.MethodTwoToThreeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.MethodTwoToThreeUserControl.TabIndex = 0;
      // 
      // MethodFourTabPage
      // 
      this.MethodFourTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d0b0e8f0-a50f-46c9-bd81-5324f549c0df", "Method Four");
      this.MethodFourTabPage.Controls.Add(this.MethodFourUserControl);
      this.MethodFourTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
      this.MethodFourTabPage.Name = "MethodFourTabPage";
      this.MethodFourTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.MethodFourTabPage.TabIndex = 6;
      // 
      // MethodFourUserControl
      // 
      this.MethodFourUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.MethodFourUserControl, ".");
      this.MethodFourUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MethodFourUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.MethodFourUserControl.Name = "MethodFourUserControl";
      this.MethodFourUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.MethodFourUserControl.TabIndex = 0;
      // 
      // MethodFiveToSixTabPage
      // 
      this.MethodFiveToSixTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("57c9de8f-e3ef-4969-80c3-6aed5a962533", "Method Five to Six");
      this.MethodFiveToSixTabPage.Controls.Add(this.MethodFiveToSixUserControl);
      this.MethodFiveToSixTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
      this.MethodFiveToSixTabPage.Name = "MethodFiveToSixTabPage";
      this.MethodFiveToSixTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.MethodFiveToSixTabPage.TabIndex = 7;
      // 
      // MethodFiveToSixUserControl
      // 
      this.MethodFiveToSixUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.MethodFiveToSixUserControl, ".");
      this.MethodFiveToSixUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MethodFiveToSixUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.MethodFiveToSixUserControl.Name = "MethodFiveToSixUserControl";
      this.MethodFiveToSixUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 568, true);
      this.MethodFiveToSixUserControl.TabIndex = 0;
      // 
      // ImportValuationDeclarationUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.ValuationDeclarationTabControl);
      this.Name = "ImportValuationDeclarationUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 595, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.ValuationDeclarationTabControl.ResumeLayout(false);
      this.ValuationDeclarationTabControl.PerformLayout();
      this.DetailsTabPage.ResumeLayout(false);
      this.DetailsTabPage.PerformLayout();
      this.ImportValuationSendingDetailsUserControl.ResumeLayout(true);
      this.ImportValuationSendingDetailsUserControl.PerformLayout();
      this.QuestionTabPage.ResumeLayout(false);
      this.QuestionTabPage.PerformLayout();
      this.QuestionGroupBoxUserControl.ResumeLayout(true);
      this.QuestionGroupBoxUserControl.PerformLayout();
      this.PriceTabPage.ResumeLayout(false);
      this.PriceTabPage.PerformLayout();
      this.PriceGroupBoxUserControl.ResumeLayout(true);
      this.PriceGroupBoxUserControl.PerformLayout();
      this.MethodTwoToSixTabPage.ResumeLayout(false);
      this.MethodTwoToSixTabPage.PerformLayout();
      this.ValuationDeclarationMethodTwoToSixUserControl.ResumeLayout(true);
      this.ValuationDeclarationMethodTwoToSixUserControl.PerformLayout();
      this.MethodTwoToThreeTabPage.ResumeLayout(false);
      this.MethodTwoToThreeTabPage.PerformLayout();
      this.MethodTwoToThreeUserControl.ResumeLayout(true);
      this.MethodTwoToThreeUserControl.PerformLayout();
      this.MethodFourTabPage.ResumeLayout(false);
      this.MethodFourTabPage.PerformLayout();
      this.MethodFourUserControl.ResumeLayout(true);
      this.MethodFourUserControl.PerformLayout();
      this.MethodFiveToSixTabPage.ResumeLayout(false);
      this.MethodFiveToSixTabPage.PerformLayout();
      this.MethodFiveToSixUserControl.ResumeLayout(true);
      this.MethodFiveToSixUserControl.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl ValuationDeclarationTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private ZArchitecture.GUI.ZTabPage QuestionTabPage;
		private ZArchitecture.GUI.ZTabPage PriceTabPage;
		private ZArchitecture.GUI.ZTabPage MethodTwoToSixTabPage;
		private ZArchitecture.GUI.ZTabPage MethodTwoToThreeTabPage;
		private ZArchitecture.GUI.ZTabPage MethodFourTabPage;
		private ZArchitecture.GUI.ZTabPage MethodFiveToSixTabPage;
		private ValuationDeclarationMethodTwoToSixUserControl ValuationDeclarationMethodTwoToSixUserControl;
		private MethodTwoToThreeUserControl MethodTwoToThreeUserControl;
		private MethodFourUserControl MethodFourUserControl;
		private MethodFiveToSixUserControl MethodFiveToSixUserControl;
		private ImportValuationSendingDetailsUserControl ImportValuationSendingDetailsUserControl;
		private PriceGroupBoxUserControl PriceGroupBoxUserControl;
		private Question5To7GroupBoxUserControl QuestionGroupBoxUserControl;
	}
}
