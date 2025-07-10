namespace Enterprise.UniversalCopy.Module
{
	partial class UniversalCopyScheduleFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.StmUniversalCopy)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.UniversalCopy.Business.StmUniversalCopy)(null)).Template.ModuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.StmUniversalCopy)(null)).ScheduleTask.S5_ScheduleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.StmUniversalCopy)(null)).CopyObjectDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.StmUniversalCopy)(null)).Template.S9_FilterName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UniversalCopy.Business.StmUniversalCopy)(null)).ScheduleTask.S5_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.UniversalCopy.Business.StmUniversalCopy)(null)).ScheduleTask.CalcNextRunTimeLocal)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.UniversalCopy.Module.Res.GetData("a4cab5f1-ad74-4ba2-a950-e42c0f738cab", "Module");
			zTextBoxColumnStyleInfo1.ColumnName = "Template+ModuleDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.UniversalCopy.Module.Res.GetData("60807711-691f-451b-ad21-61b84d4e9c5f", "Task Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ScheduleTask+S5_ScheduleDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.UniversalCopy.Module.Res.GetData("fad4cd46-4b44-48e8-b9ec-ebf0ff61628b", "Copy Template Name");
			zTextBoxColumnStyleInfo3.ColumnName = "Template+S9_FilterNameMultilingual";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.UniversalCopy.Module.Res.GetData("40D16B98-D09B-4BF0-BC5B-E882427D3BEB", "Object");
			zTextBoxColumnStyleInfo4.ColumnName = "CopyObjectDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.ColumnName = "ScheduleTask+S5_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.ColumnName = "ScheduleTask+CalcNextRunTimeLocal";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.StmUniversalCopy);
			// 
			// UniversalCopyScheduleFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "UniversalCopyScheduleFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
