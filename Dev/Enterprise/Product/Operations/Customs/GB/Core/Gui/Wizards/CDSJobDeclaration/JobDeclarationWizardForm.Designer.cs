using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.GB.GUI.Wizards
{
	public partial class JobDeclarationWizardForm
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

		new void InitializeComponent()
		{
            this.ButtonNext = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ButtonPrevious = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.GroupBoxOptions = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LabelQuestion = new Enterprise.ZArchitecture.ZLabel();
            this.ButtonFinish = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 475, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.CDS.DeclarationWizard);
            // 
            // ButtonNext
            // 
            this.ButtonNext.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("AA789C60-A612-40D0-A0FA-5E0A8BF474DF", "Next");
            this.ButtonNext.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 447, true);
            this.ButtonNext.Name = "ButtonNext";
            this.ButtonNext.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
            this.ButtonNext.TabIndex = 4;
            this.ButtonNext.ToolTipCaption = null;
            this.ButtonNext.UseVisualStyleBackColor = true;
            this.ButtonNext.Click += new System.EventHandler(this.ButtonNext_Click);
            // 
            // ButtonPrevious
            // 
            this.ButtonPrevious.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0C545759-9B03-415F-9A0D-A7B9C55ADFB5", "Previous");
            this.ButtonPrevious.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 447, true);
            this.ButtonPrevious.Name = "ButtonPrevious";
            this.ButtonPrevious.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
            this.ButtonPrevious.TabIndex = 3;
            this.ButtonPrevious.ToolTipCaption = null;
            this.ButtonPrevious.UseVisualStyleBackColor = true;
            this.ButtonPrevious.Click += new System.EventHandler(this.ButtonPrevious_Click);
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.GroupBoxOptions);
            this.zPanel1.Controls.Add(this.LabelQuestion);
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 425, true);
            this.zPanel1.TabIndex = 1;
            // 
            // GroupBoxOptions
            // 
            this.GroupBoxOptions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 69, true);
            this.GroupBoxOptions.Name = "GroupBoxOptions";
            this.GroupBoxOptions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 291, true);
            this.GroupBoxOptions.TabIndex = 2;
            this.GroupBoxOptions.TabStop = false;
            this.GroupBoxOptions.Text = "1";
            // 
            // LabelQuestion
            // 
            this.BindingSource.SetBindingMember(this.LabelQuestion, "QuestionText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.CDS.DeclarationWizard)(null)).QuestionText)));
            this.LabelQuestion.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
            this.LabelQuestion.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 11, true);
            this.LabelQuestion.Name = "LabelQuestion";
            this.LabelQuestion.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 35, true);
            this.LabelQuestion.TabIndex = 0;
            this.LabelQuestion.UseMnemonic = false;
            // 
            // ButtonFinish
            // 
            this.ButtonFinish.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f636c0bd-9a81-4771-858c-e52fd3b899c8", "Finish");
            this.ButtonFinish.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 448, true);
            this.ButtonFinish.Name = "ButtonFinish";
            this.ButtonFinish.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
            this.ButtonFinish.TabIndex = 6;
            this.ButtonFinish.ToolTipCaption = null;
            this.ButtonFinish.UseVisualStyleBackColor = true;
            this.ButtonFinish.Visible = false;
            this.ButtonFinish.Click += new System.EventHandler(this.ButtonFinish_Click);
            // 
            // JobDeclarationWizardForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5f5d49f8-6ed6-4a67-ab5b-b34f476fc406", "Job Declaration Wizard");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 499, true);
            this.Controls.Add(this.ButtonFinish);
            this.Controls.Add(this.zPanel1);
            this.Controls.Add(this.ButtonPrevious);
            this.Controls.Add(this.ButtonNext);
            this.DataSourceAssemblyName = "Enterprise.Customs.GB.CDS";
            this.DataSourceType = typeof(Enterprise.Customs.GB.CDS.DeclarationWizard);
            this.DataSourceTypeName = "Enterprise.Customs.GB.CDS.DeclarationWizard";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 309, true);
            this.Name = "JobDeclarationWizardForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.ButtonNext, 0);
            this.Controls.SetChildIndex(this.ButtonPrevious, 0);
            this.Controls.SetChildIndex(this.zPanel1, 0);
            this.Controls.SetChildIndex(this.ButtonFinish, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZButton ButtonNext;
		protected ZArchitecture.GUI.ZButton ButtonPrevious;
		private ZArchitecture.GUI.ZPanel zPanel1;
		protected ZArchitecture.ZLabel LabelQuestion;
		protected ZArchitecture.GUI.ZGroupBox GroupBoxOptions;
		protected ZArchitecture.GUI.ZButton ButtonFinish;
	}
}
