using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI
{
	partial class InvoiceDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AgreedPlaceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AgreedPlaceCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobComInvoiceHeader);
			// 
			// AgreedPlaceCodeDropEdit
			// 
			this.AgreedPlaceCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPlaceCodeDropEdit, "ZG_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceHeader)(null)).ZG_AgreedPlaceCode)));
			this.AgreedPlaceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 12, true);
			this.AgreedPlaceCodeDropEdit.Name = "AgreedPlaceCodeDropEdit";
			this.AgreedPlaceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.AgreedPlaceCodeDropEdit.TabIndex = 0;
			// 
			// InvoiceDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AgreedPlaceCodeDropEdit);
			this.Name = "InvoiceDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 76, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AgreedPlaceCodeDropEdit.ResumeLayout(true);
			this.AgreedPlaceCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDropEdit AgreedPlaceCodeDropEdit;
	}
}
