namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CommunityHandlingCodesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.zGrid2 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			// 
			// zGrid2
			// 
			this.zGrid2.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid2, "CommunityHandlingCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).CommunityHandlingCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfo<Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CommunityHandlingCode>)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).CommunityHandlingCodes)).SyncRoot)).Data.C4_CommunityHandlingCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfo<Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CommunityHandlingCode>)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).CommunityHandlingCodes)).SyncRoot)).Data.SpecialHandlingCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfo<Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CommunityHandlingCode>)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).CommunityHandlingCodes)).SyncRoot)).Data.C4_SplitReferenceToWhichThisPertains)));
			this.zGrid2.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d4282f16-cba9-475d-b4b2-3ec2a302249f", "Handling Code");
			zDropEditColumnStyleInfo1.ColumnName = "Data+C4_CommunityHandlingCode";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("acc2aa0b-be49-479d-a011-cf53b4c10bd7", "Handling Code Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Data+SpecialHandlingCodeDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("4f15e2b5-a4a9-422f-9953-0ff95104c53e", "SRF", "Split Reference", "Split", "Split Reference to which this CHC pertains");
			zDropEditColumnStyleInfo2.ColumnName = "Data+C4_SplitReferenceToWhichThisPertains";
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGrid2.CopySelectedRowsAllowed = true;
			this.zGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid2.GridId = "5189574c-0c8e-4f3b-ae72-121284a713f3";
			this.zGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid2.LayoutKey = "zGrid2";
			this.zGrid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid2.Name = "zGrid2";
			this.zGrid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.zGrid2.TabIndex = 0;
			// 
			// CommunityHandlingCodesUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid2);
			this.Name = "CommunityHandlingCodesUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		 
		private ZArchitecture.ZGrid zGrid2;
	}
}
