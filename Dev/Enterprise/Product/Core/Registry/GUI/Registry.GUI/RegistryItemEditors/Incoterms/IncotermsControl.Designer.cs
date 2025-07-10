using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class IncotermsControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.IncoTermChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncoTermChargesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.IncoTermChargeCodesCollection);
			// 
			// IncoTermChargesGrid
			// 
			this.IncoTermChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IncoTermChargesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).IncoTermDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).OriginBrokerage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Loading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Freight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Insurance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Unloading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Brokerage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).CustomsDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IncoTermChargeCodes)(null)).Parties)));
			this.IncoTermChargesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|b0d3f4cb-6f68-a199-4159-26998c0e2491", "Incoterm");
			zTextBoxColumnStyleInfo1.ColumnName = "IncoTerm";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|7dbd595b-5b1d-4962-9a32-7ed9e5598b7c", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "IncoTermDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.BindToList = "Parties";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|39dd03b2-82df-411b-95f0-8fc6d2946d6c", "Origin Brokerage");
			zDropEditColumnStyleInfo1.ColumnName = "OriginBrokerage";
			zDropEditColumnStyleInfo2.BindToList = "Parties";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|04d0186a-6b4c-4af5-adc1-a69341641147", "Origin");
			zDropEditColumnStyleInfo2.ColumnName = "Origin";
			zDropEditColumnStyleInfo3.BindToList = "Parties";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|763ff799-3f88-4ed5-8e17-fe4edd11b7e1", "Loading");
			zDropEditColumnStyleInfo3.ColumnName = "Loading";
			zDropEditColumnStyleInfo4.BindToList = "Parties";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|91636ba2-7969-4084-9c67-ee1186ddbe5f", "Freight");
			zDropEditColumnStyleInfo4.ColumnName = "Freight";
			zDropEditColumnStyleInfo5.BindToList = "Parties";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|0d4c41f2-c9a9-435f-93e5-b31e54c4e800", "Insurance");
			zDropEditColumnStyleInfo5.ColumnName = "Insurance";
			zDropEditColumnStyleInfo6.BindToList = "Parties";
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|b426d6ac-cd8c-4cad-a0e2-09432cb05e29", "Unloading");
			zDropEditColumnStyleInfo6.ColumnName = "Unloading";
			zDropEditColumnStyleInfo7.BindToList = "Parties";
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|698e46df-a30d-4ada-8a68-eb2b5040ac96", "Destination");
			zDropEditColumnStyleInfo7.ColumnName = "Destination";
			zDropEditColumnStyleInfo8.BindToList = "Parties";
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|16c72c2e-ff91-4302-adae-28b3b4c0e929", "Destination Brokerage");
			zDropEditColumnStyleInfo8.ColumnName = "Brokerage";
			zDropEditColumnStyleInfo9.BindToList = "Parties";
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("IncotermsControl|6d1aa4aa-6738-4a21-adbf-383f065412c0", "Customs Duty/Tax");
			zDropEditColumnStyleInfo9.ColumnName = "CustomsDuty";
			this.IncoTermChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IncoTermChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.IncoTermChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.IncoTermChargesGrid.GridId = "f1dbdef4-0899-4811-863b-b5a67f17eda0";
			this.IncoTermChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncoTermChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IncoTermChargesGrid.LayoutKey = "IncoTermChargesGrid";
			this.IncoTermChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncoTermChargesGrid.Name = "IncoTermChargesGrid";
			this.IncoTermChargesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.IncoTermChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.IncoTermChargesGrid.TabIndex = 0;
			// 
			// IncotermsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncoTermChargesGrid);
			this.Name = "IncotermsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncoTermChargesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid IncoTermChargesGrid;
	}
}
