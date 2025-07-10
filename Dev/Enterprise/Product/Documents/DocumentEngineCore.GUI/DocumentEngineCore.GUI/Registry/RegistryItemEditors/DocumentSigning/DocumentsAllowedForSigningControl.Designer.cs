using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	partial class DocumentsAllowedForSigningControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DocumentsAllowedForSigningItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsAllowedForSigningItemsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigning);
			// 
			// DocumentsAllowedForSigningItemsGrid
			// 
			this.DocumentsAllowedForSigningItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentsAllowedForSigningItemsGrid, "DocumentsAllowedForSigningItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigning)(null)).DocumentsAllowedForSigningItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigningItem)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigning)(null)).DocumentsAllowedForSigningItems)).SyncRoot)).DocumentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigningItem)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigning)(null)).DocumentsAllowedForSigningItems)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigningItem)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DocumentsAllowedForSigning)(null)).DocumentsAllowedForSigningItems)).SyncRoot)).DocumentTypePairList)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DocumentsAllowedForSigningControl|4FAB59A3-91C0-452B-817D-5BEAFB64282C", "Document Name");
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.BindToList = "DocumentTypePairList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DocumentsAllowedForSigningControl|3DD7383E-1A83-459C-9581-A51D9EC4B9BC", "Is System");
			zDropEditColumnStyleInfo1.ColumnName = "DocumentType";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.DocumentsAllowedForSigningItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentsAllowedForSigningItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DocumentsAllowedForSigningItemsGrid.GridId = "47A2F4A4-CDCF-4256-9151-FE1187A35C3F";
			this.DocumentsAllowedForSigningItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsAllowedForSigningItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsAllowedForSigningItemsGrid.LayoutKey = "DocumentsAllowedForSigningItemsGrid";
			this.DocumentsAllowedForSigningItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentsAllowedForSigningItemsGrid.Name = "DocumentsAllowedForSigningItemsGrid";
			this.DocumentsAllowedForSigningItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 350, true);
			this.DocumentsAllowedForSigningItemsGrid.TabIndex = 0;
			// 
			// DocumentsAllowedForSigningControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DocumentsAllowedForSigningItemsGrid);
			this.Name = "DocumentsAllowedForSigningControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 350, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsAllowedForSigningItemsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZGrid DocumentsAllowedForSigningItemsGrid;
	}
}
