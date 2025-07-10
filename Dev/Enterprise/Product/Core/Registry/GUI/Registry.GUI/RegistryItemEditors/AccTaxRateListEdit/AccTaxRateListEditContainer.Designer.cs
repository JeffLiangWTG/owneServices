namespace Enterprise.Registry.GUI
{
	public partial class AccTaxRateListEditContainer
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AccTaxRateGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AccTaxRateGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.AccTaxRateListCollectionWrapper);
			// 
			// AccTaxRateGrid
			// 
			this.AccTaxRateGrid.AllowNavigation = false;
			this.AccTaxRateGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AccTaxRateGrid, "AccTaxRateList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.AccTaxRateListCollectionWrapper)(null)).AccTaxRateList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.GUI.AccTaxRateListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.AccTaxRateListCollectionWrapper)(null)).AccTaxRateList)).SyncRoot)).TaxRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.AccTaxRateListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.AccTaxRateListCollectionWrapper)(null)).AccTaxRateList)).SyncRoot)).AccTaxRateCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.GUI.AccTaxRateListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.AccTaxRateListCollectionWrapper)(null)).AccTaxRateList)).SyncRoot)).TaxRateDescription)));
			this.AccTaxRateGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "AccTaxRateCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AccTaxRateListEditContainer|efd77623-572e-428d-a1ef-1f081021bc88", "Tax ID");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TaxRate";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccTaxRate;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AccTaxRateListEditContainer|b9065dfc-ada2-4880-b57b-e646daf5f064", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "TaxRateDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.AccTaxRateGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AccTaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AccTaxRateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccTaxRateGrid.LayoutKey = "zGrid1";
			this.AccTaxRateGrid.DisableImportDataMenuItem = true;
			this.AccTaxRateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccTaxRateGrid.Name = "AccTaxRateGrid";
			this.AccTaxRateGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.AccTaxRateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 144, true);
			this.AccTaxRateGrid.TabIndex = 0;
			// 
			// AccTaxRateListEditContainer
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AccTaxRateGrid);
			this.Name = "AccTaxRateListEditContainer";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AccTaxRateGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
