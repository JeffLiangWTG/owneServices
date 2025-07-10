namespace Enterprise.Customs.KR.GUI
{
	partial class ValuationMethodCUserControl
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
			this.Question5To11TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.Question5To7TabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.question5To7GroupBoxUserControl = new Enterprise.Customs.KR.GUI.Question5To7GroupBoxUserControl();
			this.Question8To11TabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.question8To11GroupBoxUserControl = new Enterprise.Customs.KR.GUI.Question8To11GroupBoxUserControl();
			this.ValuationQuestionCGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Question5To11TabControl.SuspendLayout();
			this.Question5To7TabPage.SuspendLayout();
			this.question5To7GroupBoxUserControl.SuspendLayout();
			this.Question8To11TabPage.SuspendLayout();
			this.question8To11GroupBoxUserControl.SuspendLayout();
			this.ValuationQuestionCGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// Question5To11TabControl
			// 
			this.Question5To11TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.Question5To11TabControl.Controls.Add(this.Question5To7TabPage);
			this.Question5To11TabControl.Controls.Add(this.Question8To11TabPage);
			this.Question5To11TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Question5To11TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.Question5To11TabControl.Name = "Question5To11TabControl";
			this.Question5To11TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 544, true);
			this.Question5To11TabControl.TabIndex = 1;
			// 
			// Question5To7TabPage
			// 
			this.Question5To7TabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("cbfd5a3f-37cf-4db2-bf73-7bdbc4b183da", "Question 5~7");
			this.Question5To7TabPage.Controls.Add(this.question5To7GroupBoxUserControl);
			this.Question5To7TabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.Question5To7TabPage.Name = "Question5To7TabPage";
			this.Question5To7TabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 517, true);
			this.Question5To7TabPage.TabIndex = 0;
			this.Question5To7TabPage.UseVisualStyleBackColor = true;
			// 
			// question5To7GroupBoxUserControl
			// 
			this.question5To7GroupBoxUserControl.AllowDrop = true;
			this.question5To7GroupBoxUserControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.question5To7GroupBoxUserControl, "Invoices");
			this.question5To7GroupBoxUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.question5To7GroupBoxUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.question5To7GroupBoxUserControl.Name = "question5To7GroupBoxUserControl";
			this.question5To7GroupBoxUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 517, true);
			this.question5To7GroupBoxUserControl.TabIndex = 0;
			// 
			// Question8To11TabPage
			// 
			this.Question8To11TabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("ded5eb76-e80c-44ce-af6b-6b96b2edb87f", "Question 8~11");
			this.Question8To11TabPage.Controls.Add(this.question8To11GroupBoxUserControl);
			this.Question8To11TabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.Question8To11TabPage.Name = "Question8To11TabPage";
			this.Question8To11TabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 517, true);
			this.Question8To11TabPage.TabIndex = 1;
			this.Question8To11TabPage.UseVisualStyleBackColor = true;
			// 
			// question8To11GroupBoxUserControl
			// 
			this.question8To11GroupBoxUserControl.AllowDrop = true;
			this.question8To11GroupBoxUserControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.question8To11GroupBoxUserControl, ".");
			this.question8To11GroupBoxUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.question8To11GroupBoxUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.question8To11GroupBoxUserControl.Name = "question8To11GroupBoxUserControl";
			this.question8To11GroupBoxUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 517, true);
			this.question8To11GroupBoxUserControl.TabIndex = 0;
			// 
			// ValuationQuestionCGroupBox
			// 
			this.ValuationQuestionCGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("C8502F53-78BE-446D-A724-C7857D8F5E1F", "Valuation Method C");
			this.ValuationQuestionCGroupBox.Controls.Add(this.Question5To11TabControl);
			this.ValuationQuestionCGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValuationQuestionCGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValuationQuestionCGroupBox.Name = "ValuationQuestionCGroupBox";
			this.ValuationQuestionCGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 563, true);
			this.ValuationQuestionCGroupBox.TabIndex = 1;
			this.ValuationQuestionCGroupBox.TabStop = false;
			// 
			// ValuationMethodCUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ValuationQuestionCGroupBox);
			this.Name = "ValuationMethodCUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 563, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Question5To11TabControl.ResumeLayout(false);
			this.Question5To11TabControl.PerformLayout();
			this.Question5To7TabPage.ResumeLayout(false);
			this.Question5To7TabPage.PerformLayout();
			this.question5To7GroupBoxUserControl.ResumeLayout(true);
			this.question5To7GroupBoxUserControl.PerformLayout();
			this.Question8To11TabPage.ResumeLayout(false);
			this.Question8To11TabPage.PerformLayout();
			this.question8To11GroupBoxUserControl.ResumeLayout(true);
			this.question8To11GroupBoxUserControl.PerformLayout();
			this.ValuationQuestionCGroupBox.ResumeLayout(false);
			this.ValuationQuestionCGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl Question5To11TabControl;
		private ZArchitecture.GUI.ZTabPage Question5To7TabPage;
		private Question5To7GroupBoxUserControl question5To7GroupBoxUserControl;
		private ZArchitecture.GUI.ZTabPage Question8To11TabPage;
		private Question8To11GroupBoxUserControl question8To11GroupBoxUserControl;
		private ZArchitecture.GUI.ZGroupBox ValuationQuestionCGroupBox;
	}
}
