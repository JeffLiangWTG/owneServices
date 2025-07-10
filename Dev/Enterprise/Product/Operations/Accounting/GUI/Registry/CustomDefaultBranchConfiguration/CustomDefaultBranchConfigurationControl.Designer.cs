using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class CustomDefaultBranchConfigurationControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ConfigTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.EvaluateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ValidationResultLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TimeTakenResultLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JobFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SetBranchDefaultingRulesLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigTextBox.SuspendLayout();
			this.JobFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.CustomDefaultDepartmentConfiguration);
			// 
			// ConfigTextBox
			// 
			this.ConfigTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfigTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ConfigTextBox, "Config");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Accounting.Registry.Business.CustomDefaultDepartmentConfiguration)(null)).Config)));
			this.ConfigTextBox.IsToolBarVisible = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConfigTextBox, false);
			this.ConfigTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 41, true);
			this.ConfigTextBox.MaxLength = 10000000;
			this.ConfigTextBox.Name = "ConfigTextBox";
			this.ConfigTextBox.ParentZForm = null;
			this.ConfigTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 406, true);
			this.ConfigTextBox.TabIndex = 0;
			// 
			// EvaluateButton
			// 
			this.EvaluateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EvaluateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("Evaluate|75DB934B-3DC2-4AD0-8F39-2944FC202F8D", "Evaluate");
			this.EvaluateButton.IsCaptionOverridden = false;
			this.EvaluateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 453, true);
			this.EvaluateButton.Name = "EvaluateButton";
			this.EvaluateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.EvaluateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.EvaluateButton.TabIndex = 1;
			this.EvaluateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.EvaluateButton.ToolTipCaption = null;
			this.EvaluateButton.Click += new System.EventHandler(this.EvaluateButton_Click);
			// 
			// ValidationResultLabel
			// 
			this.ValidationResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ValidationResultLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0516AC96-B3E5-4356-8481-03C35449AF3F", "Default Branch:");
			this.ValidationResultLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ValidationResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 479, true);
			this.ValidationResultLabel.Name = "ValidationResultLabel";
			this.ValidationResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 23, true);
			this.ValidationResultLabel.TabIndex = 0;
			// 
			// TimeTakenResultLabel
			// 
			this.TimeTakenResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TimeTakenResultLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9BA33C2A-E40C-4ADC-8BA8-E0F536CAFF8C", "Time taken:");
			this.TimeTakenResultLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TimeTakenResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 503, true);
			this.TimeTakenResultLabel.Name = "TimeTakenResultLabel";
			this.TimeTakenResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 23, true);
			this.TimeTakenResultLabel.TabIndex = 0;
			// 
			// JobFindBox
			// 
			this.JobFindBox.AllowDrop = true;
			this.JobFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JobFindBox.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.JobFindBox, "JobHeaderPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.CustomDefaultDepartmentConfiguration)(null)).JobHeaderPK)));
			this.JobFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AD7E6751-0D72-4E8E-BA1A-55901AE9CD4E", "Job Number");
			this.JobFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 453, true);
			this.JobFindBox.Name = "JobFindBox";
			this.JobFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JobFindBox.ParentType = null;
			this.JobFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.JobFindBox.TabIndex = 0;
			// 
			// SetBranchDefaultingRulesLabel
			// 
			this.SetBranchDefaultingRulesLabel.AutoSize = true;
			this.SetBranchDefaultingRulesLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e780ad73-7f02-4f3b-aa0a-86044a1d3e41", "Set Branch Defaulting Rules");
			this.SetBranchDefaultingRulesLabel.IsFontBold = false;
			this.SetBranchDefaultingRulesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.SetBranchDefaultingRulesLabel.Name = "SetBranchDefaultingRulesLabel";
			this.SetBranchDefaultingRulesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 13, true);
			this.SetBranchDefaultingRulesLabel.TabIndex = 2;
			this.SetBranchDefaultingRulesLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetBranchDefaultingRulesLabel_LinkClicked);
			// 
			// CustomDefaultBranchConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SetBranchDefaultingRulesLabel);
			this.Controls.Add(this.ConfigTextBox);
			this.Controls.Add(this.EvaluateButton);
			this.Controls.Add(this.ValidationResultLabel);
			this.Controls.Add(this.TimeTakenResultLabel);
			this.Controls.Add(this.JobFindBox);
			this.Name = "CustomDefaultBranchConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 537, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigTextBox.ResumeLayout(true);
			this.ConfigTextBox.PerformLayout();
			this.JobFindBox.ResumeLayout(true);
			this.JobFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZRichTextBox ConfigTextBox;
		ZButton EvaluateButton;
		ZLabel ValidationResultLabel;
		ZLabel TimeTakenResultLabel;
		ZGuidFindBox JobFindBox;
		private ZLinkLabel SetBranchDefaultingRulesLabel;
	}
}
