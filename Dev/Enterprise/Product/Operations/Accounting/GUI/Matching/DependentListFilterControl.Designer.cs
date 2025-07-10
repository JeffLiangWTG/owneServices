using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class DependentListFilterControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zDropEdit1 = new ZDropEdit();
			this.zDropEdit2 = new ZDropEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDropEdit1.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Base.Filters.DependentListFilter);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "Property1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.Base.Filters.DependentListFilter)(null)).Property1)));
			this.zDropEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DependentListFilterControl|de5a2048-00e9-403a-b42e-bc20920eca51", "Ledger");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 1, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.ShowDescriptionBox = false;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.zDropEdit2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEdit2, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.Base.Filters.DependentListFilter)(null)).Property2)));
			this.zDropEdit2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DependentListFilterControl|cad6f243-2415-41c1-827c-608f5d096a02", "Transaction Type");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 1, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.zDropEdit2.TabIndex = 1;
			// 
			// DependentListFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zDropEdit2);
			this.Controls.Add(this.zDropEdit1);
			this.Name = "DependentListFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 22, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}