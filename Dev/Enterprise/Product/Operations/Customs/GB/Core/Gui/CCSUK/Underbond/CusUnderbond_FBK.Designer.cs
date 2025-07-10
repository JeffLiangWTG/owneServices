namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CusUnderbond_FBK
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.GridForFBK = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridForFBK)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB);
			// 
			// GridForFBK
			// 
			this.GridForFBK.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GridForFBK, "FBKs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Fallback)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)).SyncRoot)).C4_SendersMessageReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Fallback)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)).SyncRoot)).HouseWaybillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Fallback)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)).SyncRoot)).AirportOfReceipt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Fallback)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)).SyncRoot)).Shed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Fallback)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)).SyncRoot)).NoPackagesExpected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Fallback)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)).SyncRoot)).C4_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Fallback)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).FBKs)).SyncRoot)).SplitReferenceToWhichThisRemovalPertains)));
			this.GridForFBK.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("46c12443-382a-402f-9236-e2127ef502cc", "Reference Number");
			zTextBoxColumnStyleInfo1.ColumnName = "C4_SendersMessageReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("4f3dc011-dd5b-4a43-b9d4-0702e2403625", "HAWB");
			zTextBoxColumnStyleInfo2.ColumnName = "HouseWaybillNumber";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3418e29c-f25e-4a3e-91ea-79b682f63dda", "Airport");
			zTextBoxColumnStyleInfo3.ColumnName = "AirportOfReceipt";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6056ec81-c0d2-4887-859c-c7f397b62851", "Current Shed");
			zTextBoxColumnStyleInfo4.ColumnName = "Shed";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("4c7fa2f5-ee3b-413c-82b5-8e041d73ec2a", "NPX");
			zTextBoxColumnStyleInfo5.ColumnName = "NoPackagesExpected";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("296f65b9-3937-4e6e-a977-b70c34510070", "Status");
			zTextBoxColumnStyleInfo6.ColumnName = "C4_Status";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zDropEditColumnStyleInfo1.ColumnName = "SplitReferenceToWhichThisRemovalPertains";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CusUnderbond.SplitReferenceToWhichThisRemovalPertains", "SRF", "Split", "Split Reference", "Split to which this request pertains");
			this.GridForFBK.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GridForFBK.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GridForFBK.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GridForFBK.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.GridForFBK.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.GridForFBK.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.GridForFBK.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GridForFBK.GridId = "b24057fc-c514-49fd-9be2-92cae36fcc4e";
			this.GridForFBK.CopySelectedRowsAllowed = true;
			this.GridForFBK.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridForFBK.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GridForFBK.LayoutKey = "GridForFBK";
			this.GridForFBK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridForFBK.Name = "GridForFBK";
			this.GridForFBK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 219, true);
			this.GridForFBK.TabIndex = 0;
			// 
			// CusUnderbond_FBK
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridForFBK);
			this.Name = "CusUnderbond_FBK";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 219, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridForFBK)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		ZArchitecture.ZGrid GridForFBK;
	}
}
