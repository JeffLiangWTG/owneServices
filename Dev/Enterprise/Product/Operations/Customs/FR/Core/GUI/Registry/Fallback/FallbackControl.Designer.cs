namespace Enterprise.Customs.FR.GUI.Registry
{
	partial class FallbackControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zfallbackGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDateEditEnd = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEditRegularisation = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCalcEditPeriod = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zTextBoxInvocation = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBoxRevocation = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEditStart = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCalcEditCount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEditBatchSize = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zfallbackGroupbox.SuspendLayout();
			this.zDateEditEnd.SuspendLayout();
			this.zDateEditRegularisation.SuspendLayout();
			this.zDateEditStart.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Registry.FallbackSettings);
			// 
			// zfallbackGroupbox
			// 
			this.zfallbackGroupbox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|624D8153-8A24-43BE-9429-349DC6FE7262", "Fallback settings");
			this.zfallbackGroupbox.Controls.Add(this.zDateEditEnd);
			this.zfallbackGroupbox.Controls.Add(this.zDateEditRegularisation);
			this.zfallbackGroupbox.Controls.Add(this.zCalcEditPeriod);
			this.zfallbackGroupbox.Controls.Add(this.zTextBoxInvocation);
			this.zfallbackGroupbox.Controls.Add(this.zTextBoxRevocation);
			this.zfallbackGroupbox.Controls.Add(this.zDateEditStart);
			this.zfallbackGroupbox.Controls.Add(this.zCalcEditCount);
			this.zfallbackGroupbox.Controls.Add(this.zCalcEditBatchSize);
			this.zfallbackGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zfallbackGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zfallbackGroupbox.Name = "zfallbackGroupbox";
			this.zfallbackGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.zfallbackGroupbox.TabIndex = 0;
			this.zfallbackGroupbox.TabStop = false;
			// 
			// zDateEditEnd
			// 
			this.zDateEditEnd.AllowDrop = true;
			this.zDateEditEnd.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditEnd, "End");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).End)));
			this.zDateEditEnd.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|15C06CE4-BE99-4AF1-A407-DC4EFB072554", "End");
			this.zDateEditEnd.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditEnd.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 27, true);
			this.zDateEditEnd.Name = "zDateEditEnd";
			this.zDateEditEnd.TabIndex = 2;
			// 
			// zDateEditRegularisation
			// 
			this.zDateEditRegularisation.AllowDrop = true;
			this.zDateEditRegularisation.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditRegularisation, "Regularisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).Regularisation)));
			this.zDateEditRegularisation.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|70984095-FCDA-4CAF-B7BA-DB6506AE4248", "Regularization date");
			this.zDateEditRegularisation.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditRegularisation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 54, true);
			this.zDateEditRegularisation.Name = "zDateEditRegularisation";
			this.zDateEditRegularisation.TabIndex = 3;
			// 
			// zCalcEditPeriod
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditPeriod, "RegularisationPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).RegularisationPeriod)));
			this.zCalcEditPeriod.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("77B858AE-E1CF-4723-81C6-E2F7D64B7FA1", "Period");
			this.zCalcEditPeriod.DecimalPlaces = 0;
			this.zCalcEditPeriod.Decimals = 0;
			this.zCalcEditPeriod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 54, true);
			this.zCalcEditPeriod.Name = "zCalcEditPeriod";
			this.zCalcEditPeriod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zCalcEditPeriod.TabIndex = 4;
			this.zCalcEditPeriod.Text = "0";
			this.zCalcEditPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBoxInvocation
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxInvocation, "InvocationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).InvocationReason)));
			this.zTextBoxInvocation.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|1AD61010-B7C4-4DF7-9C90-6F19DC3036FB", "Invocation");
			this.zTextBoxInvocation.IsDynamicMultiline = true;
			this.zTextBoxInvocation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 81, true);
			this.zTextBoxInvocation.Multiline = true;
			this.zTextBoxInvocation.Name = "zTextBoxInvocation";
			this.zTextBoxInvocation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			this.zTextBoxInvocation.TabIndex = 5;
			// 
			// zTextBoxRevocation
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxRevocation, "RevocationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).RevocationReason)));
			this.zTextBoxRevocation.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|BF5F4F09-B985-4CC0-8275-C8B3A1A994D9", "Revocation");
			this.zTextBoxRevocation.IsDynamicMultiline = true;
			this.zTextBoxRevocation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 116, true);
			this.zTextBoxRevocation.Multiline = true;
			this.zTextBoxRevocation.Name = "zTextBoxRevocation";
			this.zTextBoxRevocation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			this.zTextBoxRevocation.TabIndex = 6;
			// 
			// zDateEditStart
			// 
			this.zDateEditStart.AllowDrop = true;
			this.zDateEditStart.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditStart, "Start");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).Start)));
			this.zDateEditStart.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|F1BAFCC8-A872-4468-9701-ECAC7BC0BCB6", "Start");
			this.zDateEditStart.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditStart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 27, true);
			this.zDateEditStart.Name = "zDateEditStart";
			this.zDateEditStart.TabIndex = 1;
			// 
			// zCalcEditCount
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditCount, "RegularisationCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).RegularisationCount)));
			this.zCalcEditCount.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|82900B89-B2CF-44B7-807D-2033A3A21A86", "Count");
			this.zCalcEditCount.DecimalPlaces = 2;
			this.zCalcEditCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 150, true);
			this.zCalcEditCount.Name = "zCalcEditCount";
			this.zCalcEditCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEditCount.TabIndex = 7;
			this.zCalcEditCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEditCount.Visible = false;
			// 
			// zCalcEditBatchSize
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditBatchSize, "RegularisationBatchSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Registry.FallbackSettings)(null)).RegularisationBatchSize)));
			this.zCalcEditBatchSize.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FallbackControl|5AECFE23-BE06-4F35-AD0B-370CCC9EFC8D", "Size");
			this.zCalcEditBatchSize.DecimalPlaces = 2;
			this.zCalcEditBatchSize.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 174, true);
			this.zCalcEditBatchSize.Name = "zCalcEditBatchSize";
			this.zCalcEditBatchSize.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEditBatchSize.TabIndex = 8;
			this.zCalcEditBatchSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEditBatchSize.Visible = false;
			// 
			// FallbackControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zfallbackGroupbox);
			this.Name = "FallbackControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zfallbackGroupbox.ResumeLayout(false);
			this.zfallbackGroupbox.PerformLayout();
			this.zDateEditEnd.ResumeLayout(true);
			this.zDateEditEnd.PerformLayout();
			this.zDateEditRegularisation.ResumeLayout(true);
			this.zDateEditRegularisation.PerformLayout();
			this.zDateEditStart.ResumeLayout(true);
			this.zDateEditStart.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zfallbackGroupbox;
		private ZArchitecture.GUI.ZDateEdit zDateEditEnd;
		private ZArchitecture.GUI.ZDateEdit zDateEditRegularisation;
		private ZArchitecture.ZCalcEdit zCalcEditPeriod;
		private ZArchitecture.ZTextBox zTextBoxInvocation;
		private ZArchitecture.ZTextBox zTextBoxRevocation;
		private ZArchitecture.GUI.ZDateEdit zDateEditStart;
		private ZArchitecture.ZCalcEdit zCalcEditCount;
		private ZArchitecture.ZCalcEdit zCalcEditBatchSize;
	}
}

