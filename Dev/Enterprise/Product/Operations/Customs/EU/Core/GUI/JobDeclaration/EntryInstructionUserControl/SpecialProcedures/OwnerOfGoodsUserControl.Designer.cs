namespace Enterprise.Customs.EU.GUI
{
	partial class OwnerOfGoodsUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.OwnersOfGoodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OwnersOfGoodsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OwnersOfGoodsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OwnersOfGoodsGrid)).BeginInit();
			this.OwnersOfGoodsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// OwnersOfGoodsGroupBox
			//
			this.OwnersOfGoodsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("023ee088-b5a4-4c4f-a4c7-2179b292c7ac", "Owners of Goods");
			this.OwnersOfGoodsGroupBox.Controls.Add(this.OwnersOfGoodsGrid);
			this.OwnersOfGoodsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OwnersOfGoodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OwnersOfGoodsGroupBox.Name = "OwnersOfGoodsGroupBox";
			this.OwnersOfGoodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 142, true);
			this.OwnersOfGoodsGroupBox.TabIndex = 1;
			this.OwnersOfGoodsGroupBox.TabStop = false;
			// 
			// OwnersOfGoodsGrid
			// 
			this.OwnersOfGoodsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OwnersOfGoodsGrid, "CustomsEntryInstructions.OwnerOfGoodsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).OwnerOfGoodsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.OwnerOfGoods)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).OwnerOfGoodsCollection)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.OwnerOfGoods)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).OwnerOfGoodsCollection)).SyncRoot)).E2_OA_Address)));
			this.OwnersOfGoodsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo1.ColumnName = "E2_OA_Address";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.OwnersOfGoodsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OwnersOfGoodsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.OwnersOfGoodsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OwnersOfGoodsGrid.GridId = "5e8e37c5-4eb2-4edd-a063-350a47f194ea";
			this.OwnersOfGoodsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OwnersOfGoodsGrid.LayoutKey = "OwnersOfGoodsGrid";
			this.OwnersOfGoodsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OwnersOfGoodsGrid.Name = "OwnersOfGoodsGrid";
			this.OwnersOfGoodsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 123, true);
			this.OwnersOfGoodsGrid.TabIndex = 0;
			// 
			// OwnerOfGoodsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OwnersOfGoodsGroupBox);
			this.Name = "OwnerOfGoodsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 142, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OwnersOfGoodsGroupBox.ResumeLayout(false);
			this.OwnersOfGoodsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OwnersOfGoodsGrid)).EndInit();
			this.OwnersOfGoodsGrid.ResumeLayout(false);
			this.OwnersOfGoodsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox OwnersOfGoodsGroupBox;
		internal ZArchitecture.ZGrid OwnersOfGoodsGrid;
	}
}
