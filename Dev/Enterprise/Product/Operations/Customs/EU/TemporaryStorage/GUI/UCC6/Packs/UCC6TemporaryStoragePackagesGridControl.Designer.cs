namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePackagesGridControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo textBoxColumnStyleInfoLineNo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo guidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo calcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo dropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo multiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.GridPacks = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridPacks)).BeginInit();
			this.GridPacks.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// zGridPacks
			// 
			this.GridPacks.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GridPacks, "Bills.Packs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).ContainerPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_MarksAndNumbers)));
			this.GridPacks.CaptionVisible = false;
			textBoxColumnStyleInfoLineNo.ColumnName = "APA_LineNo";
			textBoxColumnStyleInfoLineNo.IsReadOnly = true;
			textBoxColumnStyleInfoLineNo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			guidDropEditColumnStyleInfo1.ColumnName = "ContainerPK";
			guidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			calcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			calcEditColumnStyleInfo1.ColumnName = "APA_PackQty";
			calcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			dropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			dropEditColumnStyleInfo1.ColumnName = "APA_PackUQ";
			dropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			multiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			multiLineTextBoxColumnInfo1.ColumnName = "APA_MarksAndNumbers";
			multiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GridPacks.ColumnStyles.Add(textBoxColumnStyleInfoLineNo);
			this.GridPacks.ColumnStyles.Add(guidDropEditColumnStyleInfo1);
			this.GridPacks.ColumnStyles.Add(calcEditColumnStyleInfo1);
			this.GridPacks.ColumnStyles.Add(dropEditColumnStyleInfo1);
			this.GridPacks.ColumnStyles.Add(multiLineTextBoxColumnInfo1);
			this.GridPacks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridPacks.GridId = "520B1DC7-883B-4278-96AB-7C7499191A74";
			this.GridPacks.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GridPacks.LayoutKey = "gridPacks";
			this.GridPacks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridPacks.Name = "gridPacks";
			this.GridPacks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 224, true);
			this.GridPacks.TabIndex = 0;
			// 
			// UCC6TemporaryStoragePackagesGridControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridPacks);
			this.Name = "UCC6TemporaryStoragePackagesGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 166, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridPacks)).EndInit();
			this.GridPacks.ResumeLayout(false);
			this.GridPacks.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		protected ZArchitecture.ZGrid GridPacks;
	}
}
