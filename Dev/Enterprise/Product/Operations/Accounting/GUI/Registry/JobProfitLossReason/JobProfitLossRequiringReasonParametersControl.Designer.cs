namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobProfitLossRequiringReasonParametersControl
	{

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.JobStatusControl = new Enterprise.Registry.GUI.CodeSelectionCollectionControl();
			this.TopPanel = new ZArchitecture.GUI.ZPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MarginThresholdNegativeProfitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MarginThresholdPositiveProfitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JobProfitLossRequiringReasonParameters);
			// 
			// JobStatusControl
			// 
			this.BindingSource.SetBindingMember(this.JobStatusControl, "JobStatusCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.CodeSelectionCollection)(((Enterprise.Accounting.Registry.Business.JobProfitLossRequiringReasonParameters)(null)).JobStatusCollection)));
			this.JobStatusControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobStatusControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 89, true);
			this.JobStatusControl.Name = "JobStatusControl";
			this.JobStatusControl.ReadOnly = false;
			this.JobStatusControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 309, true);
			this.JobStatusControl.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.zLabel1);
			this.TopPanel.Controls.Add(this.MarginThresholdNegativeProfitCalcEdit);
			this.TopPanel.Controls.Add(this.MarginThresholdPositiveProfitCalcEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 89, true);
			this.TopPanel.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossRequiringReasonParametersControl|ec53416e-a14f-4737-b068-062c093c2188", "Reason Required WHEN:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 23, true);
			this.zLabel1.TabIndex = 0;
			// 
			// MarginThresholdNegativeProfitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MarginThresholdNegativeProfitCalcEdit, "LossThreshold");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.JobProfitLossRequiringReasonParameters)(null)).LossThreshold)));
			this.MarginThresholdNegativeProfitCalcEdit.BindToDecimalPlaces = "DecimalPlaces";
			this.MarginThresholdNegativeProfitCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossRequiringReasonParametersControl|bad15ef2-3e93-4590-baa0-11406d1daacc", "PROFIT / REVENUE percentage falls below");
			this.MarginThresholdNegativeProfitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 56, true);
			this.MarginThresholdNegativeProfitCalcEdit.Name = "MarginThresholdNegativeProfitCalcEdit";
			this.MarginThresholdNegativeProfitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginThresholdNegativeProfitCalcEdit.TabIndex = 2;
			this.MarginThresholdNegativeProfitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarginThresholdPositiveProfitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MarginThresholdPositiveProfitCalcEdit, "ProfitThreshold");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.JobProfitLossRequiringReasonParameters)(null)).ProfitThreshold)));
			this.MarginThresholdPositiveProfitCalcEdit.BindToDecimalPlaces = "DecimalPlaces";
			this.MarginThresholdPositiveProfitCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossRequiringReasonParametersControl|41b0fcaa-e170-41c2-b046-8cdbc0fb5e41", "PROFIT / REVENUE percentage exceeds");
			this.MarginThresholdPositiveProfitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 30, true);
			this.MarginThresholdPositiveProfitCalcEdit.Name = "MarginThresholdPositiveProfitCalcEdit";
			this.MarginThresholdPositiveProfitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginThresholdPositiveProfitCalcEdit.TabIndex = 1;
			this.MarginThresholdPositiveProfitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobProfitLossRequiringReasonParametersControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobStatusControl);
			this.Controls.Add(this.TopPanel);
			this.Name = "JobProfitLossRequiringReasonParametersControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 398, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		ZArchitecture.GUI.ZPanel TopPanel;
		internal Enterprise.ZArchitecture.ZCalcEdit MarginThresholdPositiveProfitCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit MarginThresholdNegativeProfitCalcEdit;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		internal Enterprise.Registry.GUI.CodeSelectionCollectionControl JobStatusControl;
	}
}
