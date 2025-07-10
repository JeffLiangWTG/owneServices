using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class JobManagementAmountFilterControl
	{

		private ZDropEdit ComparisonOperatorDropEdit;
		private ZArchitecture.ZCalcEdit AmountCalcEdit;
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ComparisonOperatorDropEdit = new ZDropEdit();
			this.AmountCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobManagementAmountFilter);
			// 
			// ComparisonOperatorDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ComparisonOperatorDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobManagementAmountFilter)(null)).ComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobManagementAmountFilter)(null)).ComparisonOperator_List)));
			this.ComparisonOperatorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ComparisonOperatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 1, true);
			this.ComparisonOperatorDropEdit.Name = "ComparisonOperatorDropEdit";
			this.ComparisonOperatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.ComparisonOperatorDropEdit.TabIndex = 0;
			// 
			// AmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountCalcEdit, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobManagementAmountFilter)(null)).Property)));
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 1, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AmountCalcEdit.TabIndex = 1;
			this.AmountCalcEdit.Text = "0";
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobManagementAmountFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ComparisonOperatorDropEdit);
			this.Controls.Add(this.AmountCalcEdit);
			this.Name = "JobManagementAmountFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
