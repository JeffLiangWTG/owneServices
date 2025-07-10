namespace Enterprise.Customs.EU.GUI.CommercialInvoice
{
	partial class InvoiceHeaderDetailsLayouUserControl
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
			this.AgreedPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AgreedPlaceCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader);
			// 
			// AgreedPlaceCodeFindBox
			// 
			this.AgreedPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPlaceCodeFindBox, "ZG_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).ZG_AgreedPlaceCode)));
			this.AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 2, true);
			this.AgreedPlaceCodeFindBox.Name = "AgreedPlaceCodeFindBox";
			this.AgreedPlaceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AgreedPlaceCodeFindBox.ParentType = null;
			this.AgreedPlaceCodeFindBox.PreBoundMaxLength = 5;
			this.AgreedPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 25, true);
			this.AgreedPlaceCodeFindBox.TabIndex = 0;
			// 
			// InvoiceHeaderDetailsLayouUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AgreedPlaceCodeFindBox);
			this.Name = "InvoiceHeaderDetailsLayouUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AgreedPlaceCodeFindBox.ResumeLayout(true);
			this.AgreedPlaceCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox AgreedPlaceCodeFindBox;

		#endregion
	}
}
