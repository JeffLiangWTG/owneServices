namespace Enterprise.Customs.CN.GUI
{
	partial class PackagesAndTypeUserControl
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
			this.PackTypeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NoOfPacksCalcBox = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackTypeCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CusEntryInstruction);
			// 
			// PackTypeCodeDropEdit
			// 
			this.PackTypeCodeDropEdit.AllowDrop = true;
			this.PackTypeCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PackTypeCodeDropEdit, "CEI_PackageUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_PackageUQ)));
			this.PackTypeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 0, true);
			this.PackTypeCodeDropEdit.Name = "PackTypeCodeDropEdit";
			this.PackTypeCodeDropEdit.PreBoundMaxLength = 1;
			this.PackTypeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.PackTypeCodeDropEdit.TabIndex = 8;
			// 
			// NoOfPacksCalcBox
			// 
			this.BindingSource.SetBindingMember(this.NoOfPacksCalcBox, "CEI_Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_Packages)));
			this.NoOfPacksCalcBox.DecimalPlaces = 0;
			this.NoOfPacksCalcBox.Decimals = 0;
			this.NoOfPacksCalcBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NoOfPacksCalcBox.Name = "NoOfPacksCalcBox";
			this.NoOfPacksCalcBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.NoOfPacksCalcBox.TabIndex = 7;
			this.NoOfPacksCalcBox.Text = "0";
			this.NoOfPacksCalcBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NoOfPacksCalcBox.TrackDisposedAccess = true;
			// 
			// PackagesAndTypeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackTypeCodeDropEdit);
			this.Controls.Add(this.NoOfPacksCalcBox);
			this.Name = "PackagesAndTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackTypeCodeDropEdit.ResumeLayout(true);
			this.PackTypeCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit NoOfPacksCalcBox;
		internal ZArchitecture.GUI.ZDropEdit PackTypeCodeDropEdit;
	}
}
