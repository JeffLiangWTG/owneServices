using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class CustomDefaultDepartmentConfigurationControl
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
			this.SetDepartmentDefaultingRulesLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
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
			this.ConfigTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 32, true);
			this.ConfigTextBox.MaxLength = 10000000;
			this.ConfigTextBox.Name = "ConfigTextBox";
			this.ConfigTextBox.ParentZForm = null;
			this.ConfigTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 393, true);
			this.ConfigTextBox.TabIndex = 0;
			// 
			// EvaluateButton
			// 
			this.EvaluateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EvaluateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("Evaluate|FC2695A5-4EA7-491D-871F-C3D1CE97E821", "Evaluate");
			this.EvaluateButton.IsCaptionOverridden = false;
			this.EvaluateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 431, true);
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
			this.ValidationResultLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3858a17a-fbb4-42e4-bda1-ec17f233ed34", "Default Department:");
			this.ValidationResultLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ValidationResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 457, true);
			this.ValidationResultLabel.Name = "ValidationResultLabel";
			this.ValidationResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 23, true);
			this.ValidationResultLabel.TabIndex = 0;
			// 
			// TimeTakenResultLabel
			// 
			this.TimeTakenResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TimeTakenResultLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5f6ab6a6-e82f-4513-bfd0-1931c32b1810", "Time taken:");
			this.TimeTakenResultLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TimeTakenResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 481, true);
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
			this.JobFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C2DFB162-A703-4927-AFB1-116C7F787A67", "Job Number");
			this.JobFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 431, true);
			this.JobFindBox.Name = "JobFindBox";
			this.JobFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JobFindBox.ParentType = null;
			this.JobFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.JobFindBox.TabIndex = 0;
			// 
			// SetDepartmentDefaultingRulesLabel
			// 
			this.SetDepartmentDefaultingRulesLabel.AutoSize = true;
			this.SetDepartmentDefaultingRulesLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0345abdb-3714-4e51-bc85-c46a9e4132f3", "Set Department Defaulting Rules");
			this.SetDepartmentDefaultingRulesLabel.IsFontBold = false;
			this.SetDepartmentDefaultingRulesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 11, true);
			this.SetDepartmentDefaultingRulesLabel.Name = "SetDepartmentDefaultingRulesLabel";
			this.SetDepartmentDefaultingRulesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 13, true);
			this.SetDepartmentDefaultingRulesLabel.TabIndex = 2;
			this.SetDepartmentDefaultingRulesLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetDepartmentDefaultingRulesLabel_LinkClicked);
			// 
			// CustomDefaultDepartmentConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SetDepartmentDefaultingRulesLabel);
			this.Controls.Add(this.ConfigTextBox);
			this.Controls.Add(this.EvaluateButton);
			this.Controls.Add(this.ValidationResultLabel);
			this.Controls.Add(this.TimeTakenResultLabel);
			this.Controls.Add(this.JobFindBox);
			this.Name = "CustomDefaultDepartmentConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 515, true);
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
		private ZLinkLabel SetDepartmentDefaultingRulesLabel;
	}
}
