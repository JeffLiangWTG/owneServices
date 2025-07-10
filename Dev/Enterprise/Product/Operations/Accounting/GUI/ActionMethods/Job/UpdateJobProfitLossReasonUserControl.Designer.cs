
namespace Enterprise.Accounting.GUI
{
	partial class UpdateJobProfitLossReasonUserControl
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
			this.ProfitLossReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UpdateJobStatusActionMethodApplicator);
			// 
			// StatusCodeDropEdit
			// 
			this.ProfitLossReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProfitLossReasonDropEdit, "ProfitLossReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UpdateJobProfitLossReasonActionMethodApplicator)(null)).ProfitLossReason)));
			this.ProfitLossReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 2, true);
			this.ProfitLossReasonDropEdit.Name = "ProfitLossReasonDropEdit";
			this.ProfitLossReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.ProfitLossReasonDropEdit.PreBoundMaxLength = 3;
			this.ProfitLossReasonDropEdit.TabIndex = 0;
			this.ProfitLossReasonDropEdit.CaptionResourceString = Res.GetData("74f20200-6470-4f55-9715-c5a40c404d46", "Profit / Loss Reason");
			// 
			// UpdateHVLVStatusUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ProfitLossReasonDropEdit);
			this.Name = "UpdateJobProfitLossReasonUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit ProfitLossReasonDropEdit;
	}
}
