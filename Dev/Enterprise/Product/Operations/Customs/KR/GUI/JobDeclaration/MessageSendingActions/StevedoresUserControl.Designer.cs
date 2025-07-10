namespace Enterprise.Customs.KR.GUI
{
	partial class StevedoresUserControl
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
			this.StevedoresGrid = new Enterprise.ZArchitecture.ZGrid();
			this.StevedoresGrid.ReadOnly = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StevedoresGrid)).BeginInit();
			this.StevedoresGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationLoadingCompletionMessageSendingObject);
			// 
			// StevedoresGrid
			// 
			this.StevedoresGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StevedoresGrid, "Stevedores");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationLoadingCompletionMessageSendingObject)(null)).Stevedores)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationLoadingCompletionMessageSendingObject)(null)).Stevedores)).SyncRoot)).PersonFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationLoadingCompletionMessageSendingObject)(null)).Stevedores)).SyncRoot)).PersonHomePhoneNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationLoadingCompletionMessageSendingObject)(null)).Stevedores)).SyncRoot)).PersonMobilePhoneNumber)));
			this.StevedoresGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9feaa26a-544f-4445-8b4a-e423a343c0ca", "Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "PersonFullName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("1cdbf68d-d457-4836-9283-2f394bf49014", "Phone Number");
			zTextBoxColumnStyleInfo2.ColumnName = "PersonHomePhoneNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("39334e03-4f0e-4fdf-8909-e96a364d3366", "Mobile Number");
			zTextBoxColumnStyleInfo3.ColumnName = "PersonMobilePhoneNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			this.StevedoresGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StevedoresGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StevedoresGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StevedoresGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StevedoresGrid.GridId = "8e17f5ed-7fb5-45a9-b308-8c8428bdf023";
			this.StevedoresGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StevedoresGrid.LayoutKey = "StevedoresGrid";
			this.StevedoresGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StevedoresGrid.Name = "StevedoresGrid";
			this.StevedoresGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.StevedoresGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 120, true);
			this.StevedoresGrid.TabIndex = 0;
			// 
			// StevedoresUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StevedoresGrid);
			this.Name = "StevedoresUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StevedoresGrid)).EndInit();
			this.StevedoresGrid.ResumeLayout(false);
			this.StevedoresGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid StevedoresGrid;
	}
}
