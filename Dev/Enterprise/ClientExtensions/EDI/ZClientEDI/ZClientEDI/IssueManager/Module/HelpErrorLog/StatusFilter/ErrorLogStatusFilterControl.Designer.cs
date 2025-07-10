
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	partial class ErrorLogStatusFilterControl : ZUserControl
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
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PrefixLabel = new Enterprise.ZArchitecture.ZLabel();
			this.suffixLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TimeFrameZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IssueManager.Module.ErrorLogStatusFilter);
			// 
			// StatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IssueManager.Module.ErrorLogStatusFilter)(null)).Property)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Module.ErrorLogStatusFilter)(null)).StatusList)));
			this.StatusDropEdit.BindToList = "StatusList";
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 1, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.StatusDropEdit.TabIndex = 0;
			// 
			// PrefixLabel
			// 
			this.PrefixLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrefixLabel, "PrefixText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.IssueManager.Module.ErrorLogStatusFilter)(null)).PrefixText)));
			this.PrefixLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 4, true);
			this.PrefixLabel.Name = "PrefixLabel";
			this.PrefixLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.PrefixLabel.TabIndex = 2;
			// 
			// suffixLabel
			// 
			this.suffixLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.suffixLabel, "SuffixText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.IssueManager.Module.ErrorLogStatusFilter)(null)).SuffixText)));
			this.suffixLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 4, true);
			this.suffixLabel.Name = "suffixLabel";
			this.suffixLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.suffixLabel.TabIndex = 3;
			// 
			// TimeFrameZCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TimeFrameZCalcEdit, "RelatedTimeFrame");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IssueManager.Module.ErrorLogStatusFilter)(null)).RelatedTimeFrame)));
			this.TimeFrameZCalcEdit.Decimals = 0;
			this.TimeFrameZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 1, true);
			this.TimeFrameZCalcEdit.Name = "TimeFrameZCalcEdit";
			this.TimeFrameZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TimeFrameZCalcEdit.TabIndex = 4;
			this.TimeFrameZCalcEdit.Text = "0";
			this.TimeFrameZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ErrorLogStatusFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TimeFrameZCalcEdit);
			this.Controls.Add(this.suffixLabel);
			this.Controls.Add(this.PrefixLabel);
			this.Controls.Add(this.StatusDropEdit);
			this.Name = "ErrorLogStatusFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private Enterprise.ZArchitecture.ZLabel PrefixLabel;
		private Enterprise.ZArchitecture.ZLabel suffixLabel;
		private ZCalcEdit TimeFrameZCalcEdit;
	}
}
