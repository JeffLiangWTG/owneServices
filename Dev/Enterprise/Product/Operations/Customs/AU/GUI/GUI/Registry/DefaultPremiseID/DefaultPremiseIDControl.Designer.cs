namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class DefaultPremiseIDControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DefaultPremiseIDGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultPremiseIDGrid)).BeginInit();
			this.DefaultPremiseIDGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.DefaultPremiseID);
			// 
			// DefaultPremiseIDGrid
			// 
			this.DefaultPremiseIDGrid.AllowNavigation = false;
			this.DefaultPremiseIDGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DefaultPremiseIDGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DefaultPremiseID)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DefaultPremiseID)(null)).AirlineCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DefaultPremiseID)(null)).PortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DefaultPremiseID)(null)).UNLocoCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DefaultPremiseID)(null)).PremiseID)));
			this.DefaultPremiseIDGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("DefaultPremiseIDControl|b29813cd-43b5-4ddb-856c-71fa130ec361", "Airline Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "AirlineCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.BindToList = "UNLocoCollection";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("DefaultPremiseIDControl|917a2737-72f8-44bd-8905-235afabb9e5a", "Port Of Discharge");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PortOfDischarge";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("DefaultPremiseIDControl|12844d46-deb6-48ee-b709-1c886c76bd88", "Premise ID");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "PremiseID";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DefaultPremiseIDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DefaultPremiseIDGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DefaultPremiseIDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DefaultPremiseIDGrid.GridId = "e314bef2-048b-4bd0-8301-e41bf9740163";
			this.DefaultPremiseIDGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultPremiseIDGrid.LayoutKey = "DefaultPremiseIDGrid";
			this.DefaultPremiseIDGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultPremiseIDGrid.Name = "DefaultPremiseIDGrid";
			this.DefaultPremiseIDGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 96, true);
			this.DefaultPremiseIDGrid.TabIndex = 0;
			// 
			// DefaultPremiseIDControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultPremiseIDGrid);
			this.Name = "DefaultPremiseIDControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 96, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultPremiseIDGrid)).EndInit();
			this.DefaultPremiseIDGrid.ResumeLayout(false);
			this.DefaultPremiseIDGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid DefaultPremiseIDGrid;
	}
}
