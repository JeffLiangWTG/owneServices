namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DisbursementJobsClosureConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DisbursementJobsClosureConfigurationGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.AggregatedLevelLabel = new ZArchitecture.ZLabel();
			this.JobLevelLabel = new ZArchitecture.ZLabel();
			this.JobLevelShortfallCalcLabel = new ZArchitecture.ZLabel();
			this.JobLevelShortfallCalcEdit = new ZArchitecture.ZCalcEdit();
			this.JobLevelSurplusLabel = new ZArchitecture.ZLabel();
			this.JobLevelSurplusCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AggregatedLevelShortfallLabel = new ZArchitecture.ZLabel();
			this.AggregatedLevelShortfallCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AggregatedLevelSurplusLabel = new ZArchitecture.ZLabel();
			this.AggregatedLevelSurplusCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DisbursementJobsClosureConfigurationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.DisbursementJobsClosureConfiguration);
			// 
			// DisbursementJobsClosureConfigurationGroupBox
			// 
			this.DisbursementJobsClosureConfigurationGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("89ca2422-8042-4981-a753-09891cea13f5", "Disbursement Jobs Closure Configuration");
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.AggregatedLevelLabel);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.JobLevelLabel);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.JobLevelShortfallCalcLabel);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.JobLevelShortfallCalcEdit);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.JobLevelSurplusLabel);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.JobLevelSurplusCalcEdit);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.AggregatedLevelShortfallLabel);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.AggregatedLevelShortfallCalcEdit);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.AggregatedLevelSurplusLabel);
			this.DisbursementJobsClosureConfigurationGroupBox.Controls.Add(this.AggregatedLevelSurplusCalcEdit);
			this.DisbursementJobsClosureConfigurationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DisbursementJobsClosureConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DisbursementJobsClosureConfigurationGroupBox.Name = "DisbursementJobsClosureConfigurationGroupBox";
			this.DisbursementJobsClosureConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 133, true);
			this.DisbursementJobsClosureConfigurationGroupBox.TabIndex = 1;
			this.DisbursementJobsClosureConfigurationGroupBox.TabStop = false;
			// 
			// AggregatedLevelLabel
			// 
			this.AggregatedLevelLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("aa53eca8-28b3-4233-8ed3-5fbc28ebc686", "Aggregated Level");
			this.AggregatedLevelLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AggregatedLevelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 71, true);
			this.AggregatedLevelLabel.Name = "AggregatedLevelLabel";
			this.AggregatedLevelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.AggregatedLevelLabel.TabIndex = 0;
			this.AggregatedLevelLabel.Text = Enterprise.Accounting.GUI.Res.GetString("5C333795-F49B-4B19-93AE-5E99A334C9FE", "Aggregated Level:");
			// 
			// JobLevelLabel
			// 
			this.JobLevelLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b5d232df-8d89-4056-8af6-4bd894416aee", "Job Level");
			this.JobLevelLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.JobLevelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 15, true);
			this.JobLevelLabel.Name = "JobLevelLabel";
			this.JobLevelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.JobLevelLabel.TabIndex = 0;
			this.JobLevelLabel.Text = Enterprise.Accounting.GUI.Res.GetString("A36BA245-58B9-418F-A16F-549B98CCD54F", "Job Level:");
			// 
			// JobLevelShortfallCalcLabel
			// 
			this.JobLevelShortfallCalcLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f1bb6e25-def3-47d0-baaa-c10c6b21b128", "Shortfall of up to");
			this.JobLevelShortfallCalcLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.JobLevelShortfallCalcLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 38, true);
			this.JobLevelShortfallCalcLabel.Name = "JobLevelShortfallCalcLabel";
			this.JobLevelShortfallCalcLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.JobLevelShortfallCalcLabel.TabIndex = 0;
			this.JobLevelShortfallCalcLabel.Text = Enterprise.Accounting.GUI.Res.GetString("4B9F8DDC-0B8F-41BC-8F79-2F9B7A6700BC", "Shortfall of up to");
			// 
			// JobLevelShortfallCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JobLevelShortfallCalcEdit, "JobLevelOfShortfallUpTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.DisbursementJobsClosureConfiguration)(null)).JobLevelOfShortfallUpTo)));
			this.JobLevelShortfallCalcEdit.CaptionResourceString = null;
			this.JobLevelShortfallCalcEdit.DecimalPlaces = 0;
			this.JobLevelShortfallCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobLevelShortfallCalcEdit, false);
			this.JobLevelShortfallCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 41, true);
			this.JobLevelShortfallCalcEdit.Name = "JobLevelShortfallCalcEdit";
			this.JobLevelShortfallCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.JobLevelShortfallCalcEdit.TabIndex = 0;
			this.JobLevelShortfallCalcEdit.Text = "0";
			this.JobLevelShortfallCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobLevelSurplusLabel
			// 
			this.JobLevelSurplusLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4fbff1ca-9923-48bd-8f08-821b60c870b0", "Surplus of up to");
			this.JobLevelSurplusLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.JobLevelSurplusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 38, true);
			this.JobLevelSurplusLabel.Name = "JobLevelSurplusLabel";
			this.JobLevelSurplusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.JobLevelSurplusLabel.TabIndex = 0;
			this.JobLevelSurplusLabel.Text = Enterprise.Accounting.GUI.Res.GetString("4669F6CF-C704-4F5F-95BB-480E84509E32", "Surplus of up to");
			// 
			// JobLevelSurplusCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JobLevelSurplusCalcEdit, "JobLevelOfSurplusUpTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.DisbursementJobsClosureConfiguration)(null)).JobLevelOfSurplusUpTo)));
			this.JobLevelSurplusCalcEdit.CaptionResourceString = null;
			this.JobLevelSurplusCalcEdit.DecimalPlaces = 0;
			this.JobLevelSurplusCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobLevelSurplusCalcEdit, false);
			this.JobLevelSurplusCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 41, true);
			this.JobLevelSurplusCalcEdit.Name = "JobLevelSurplusCalcEdit";
			this.JobLevelSurplusCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.JobLevelSurplusCalcEdit.TabIndex = 1;
			this.JobLevelSurplusCalcEdit.Text = "0";
			this.JobLevelSurplusCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AggregatedLevelShortfallLabel
			// 
			this.AggregatedLevelShortfallLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("30fecf16-8797-4005-86ea-f17cf4f5ee44", "Shortfall of up to");
			this.AggregatedLevelShortfallLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AggregatedLevelShortfallLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 94, true);
			this.AggregatedLevelShortfallLabel.Name = "AggregatedLevelShortfallLabel";
			this.AggregatedLevelShortfallLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.AggregatedLevelShortfallLabel.TabIndex = 0;
			this.AggregatedLevelShortfallLabel.Text = Enterprise.Accounting.GUI.Res.GetString("1E4CFA5B-2237-4ADD-A39A-CBB947E69305", "Shortfall of up to");
			// 
			// AggregatedLevelShortfallCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AggregatedLevelShortfallCalcEdit, "AggregatedLevelOfShortfallUpTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.DisbursementJobsClosureConfiguration)(null)).AggregatedLevelOfShortfallUpTo)));
			this.AggregatedLevelShortfallCalcEdit.CaptionResourceString = null;
			this.AggregatedLevelShortfallCalcEdit.DecimalPlaces = 0;
			this.AggregatedLevelShortfallCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AggregatedLevelShortfallCalcEdit, false);
			this.AggregatedLevelShortfallCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 97, true);
			this.AggregatedLevelShortfallCalcEdit.Name = "AggregatedLevelShortfallCalcEdit";
			this.AggregatedLevelShortfallCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.AggregatedLevelShortfallCalcEdit.TabIndex = 2;
			this.AggregatedLevelShortfallCalcEdit.Text = "0";
			this.AggregatedLevelShortfallCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AggregatedLevelSurplusLabel
			// 
			this.AggregatedLevelSurplusLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("78cca6c2-11a6-426f-9008-3df200edd6e4", "Surplus of up to");
			this.AggregatedLevelSurplusLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AggregatedLevelSurplusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 94, true);
			this.AggregatedLevelSurplusLabel.Name = "AggregatedLevelSurplusLabel";
			this.AggregatedLevelSurplusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.AggregatedLevelSurplusLabel.TabIndex = 0;
			this.AggregatedLevelSurplusLabel.Text = Enterprise.Accounting.GUI.Res.GetString("DC1A83F3-A3BC-4454-A777-B2E3AEDC7442", "Surplus of up to");
			// 
			// AggregatedLevelSurplusCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AggregatedLevelSurplusCalcEdit, "AggregatedLevelOfSurplusUpTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.DisbursementJobsClosureConfiguration)(null)).AggregatedLevelOfSurplusUpTo)));
			this.AggregatedLevelSurplusCalcEdit.CaptionResourceString = null;
			this.AggregatedLevelSurplusCalcEdit.DecimalPlaces = 0;
			this.AggregatedLevelSurplusCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AggregatedLevelSurplusCalcEdit, false);
			this.AggregatedLevelSurplusCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 97, true);
			this.AggregatedLevelSurplusCalcEdit.Name = "AggregatedLevelSurplusCalcEdit";
			this.AggregatedLevelSurplusCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.AggregatedLevelSurplusCalcEdit.TabIndex = 3;
			this.AggregatedLevelSurplusCalcEdit.Text = "0";
			this.AggregatedLevelSurplusCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DisbursementJobsClosureConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DisbursementJobsClosureConfigurationGroupBox);
			this.Name = "DisbursementJobsClosureConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 133, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DisbursementJobsClosureConfigurationGroupBox.ResumeLayout(false);
			this.DisbursementJobsClosureConfigurationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZGroupBox DisbursementJobsClosureConfigurationGroupBox;
		ZArchitecture.ZLabel JobLevelLabel;
		ZArchitecture.ZLabel AggregatedLevelLabel;
		ZArchitecture.ZLabel JobLevelShortfallCalcLabel;
		ZArchitecture.ZCalcEdit JobLevelShortfallCalcEdit;
		ZArchitecture.ZLabel JobLevelSurplusLabel;
		ZArchitecture.ZCalcEdit JobLevelSurplusCalcEdit;
		ZArchitecture.ZLabel AggregatedLevelShortfallLabel;
		ZArchitecture.ZCalcEdit AggregatedLevelShortfallCalcEdit;
		ZArchitecture.ZLabel AggregatedLevelSurplusLabel;
		ZArchitecture.ZCalcEdit AggregatedLevelSurplusCalcEdit;

	}
}
