namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class DeliveryUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo isBeingReleasedNowColumnInfo  = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "OutTurns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_C4_Underbond)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).RemovalsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).SplitReferenceToWhichThisPertains)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_PackagesOutturned)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).IsDelivered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).IsReleasedAlready)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).IsBeingReleasedNow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_PackagesUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).WarehouseLocationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_CargoReceiptDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_CargoUnpackDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_ContainerSeal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_SealIntactIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusOutTurn)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).OutTurns)).SyncRoot)).C5_DamageIndicator)));
			this.zGrid1.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.BindToList = "RemovalsList";
			zGuidDropEditColumnStyleInfo1.Caption = "Removal/Fallback";
			zGuidDropEditColumnStyleInfo1.ColumnName = "C5_C4_Underbond";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zDropEditColumnStyleInfo1.Caption = "Split";
			zDropEditColumnStyleInfo1.ColumnName = "SplitReferenceToWhichThisPertains";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(44);
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Packages";
			zCalcEditColumnStyleInfo1.ColumnName = "C5_PackagesOutturned";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Caption = "Delivered?";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsDelivered";
			zCheckBoxColumnStyleInfo2.Caption = "Released Already?";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zCheckBoxColumnStyleInfo2.ColumnName = "IsReleasedAlready";
			zDropEditColumnStyleInfo2.Caption = "Package Type";
			zDropEditColumnStyleInfo2.ColumnName = "C5_PackagesUnits";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.Caption = "Marks & Numbers";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zTextBoxColumnStyleInfo1.ColumnName = "C5_MarksAndNumbers";
			zTextBoxColumnStyleInfo2.Caption = "Goods\' Description";
			zTextBoxColumnStyleInfo2.ColumnName = "C5_GoodsDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WarehouseLocationID";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigLocation;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(124);
			zGuidFindBoxColumnStyleInfo1.Caption = "Shed Storage Location";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.Caption = "Received Date";
			zDateEditColumnStyleInfo1.ColumnName = "C5_CargoReceiptDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zDateEditColumnStyleInfo2.Caption = "Unpacked Date";
			zDateEditColumnStyleInfo2.ColumnName = "C5_CargoUnpackDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zTextBoxColumnStyleInfo4.Caption = "Container";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.ColumnName = "C5_ContainerNumber";
			zTextBoxColumnStyleInfo5.Caption = "Seal";
			zTextBoxColumnStyleInfo5.ColumnName = "C5_ContainerSeal";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zCheckBoxColumnStyleInfo4.Caption = "Damaged?";
			zCheckBoxColumnStyleInfo4.ColumnName = "C5_DamageIndicator";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);

			isBeingReleasedNowColumnInfo.Caption = "Release now";
			isBeingReleasedNowColumnInfo.ColumnName = "IsBeingReleasedNow";


			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(isBeingReleasedNowColumnInfo);			
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.zGrid1.CopySelectedRowsAllowed = true;
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "51bad310-20fe-41c2-8606-c454bfb8d2a8";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 181, true);
			this.zGrid1.TabIndex = 0;
			// 
			// DeliveryUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zGrid1);
			this.Name = "DeliveryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 181, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid zGrid1;
	}
}
