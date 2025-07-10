namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class DefaultDestinationPremiseIDControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.DefaultDestinationPremiseIDGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultDestinationPremiseIDGrid)).BeginInit();
			this.DefaultDestinationPremiseIDGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.DefaultDestinationPremiseID);
			// 
			// DefaultDestinationPremiseIDGrid
			// 
			this.DefaultDestinationPremiseIDGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DefaultDestinationPremiseIDGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DefaultDestinationPremiseID)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DefaultDestinationPremiseID)(null)).AirlineCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DefaultDestinationPremiseID)(null)).PortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DefaultDestinationPremiseID)(null)).UNLocoCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DefaultDestinationPremiseID)(null)).PremiseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.DefaultDestinationPremiseID)(null)).UseDischargePort)));
			this.DefaultDestinationPremiseIDGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("f3dc4d8c-dfd1-4da2-b1ba-68ee63f79d27", "Airline");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "AirlineCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.BindToList = "UNLocoCollection";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("8cde403c-8dbc-433b-9bb0-47b9e5c6cd9a", "Port");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PortOfDischarge";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("373f1709-fb9b-4d34-a7a5-935511d1e7b8", "Premise ID");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "PremiseID";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ff7b303d-a193-4681-97b6-6c3d2cbe052d", "Match against Discharge");
			zCheckBoxColumnStyleInfo1.ColumnName = "UseDischargePort";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			this.DefaultDestinationPremiseIDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DefaultDestinationPremiseIDGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DefaultDestinationPremiseIDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DefaultDestinationPremiseIDGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DefaultDestinationPremiseIDGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultDestinationPremiseIDGrid.GridId = "e314bef2-048b-4bd0-8301-e41bf9740163";
			this.DefaultDestinationPremiseIDGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultDestinationPremiseIDGrid.LayoutKey = "DefaultDestinationPremiseIDGrid";
			this.DefaultDestinationPremiseIDGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultDestinationPremiseIDGrid.Name = "DefaultDestinationPremiseIDGrid";
			this.DefaultDestinationPremiseIDGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 93, true);
			this.DefaultDestinationPremiseIDGrid.TabIndex = 1;
			// 
			// DefaultDestinationPremiseIDControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultDestinationPremiseIDGrid);
			this.Name = "DefaultDestinationPremiseIDControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 93, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultDestinationPremiseIDGrid)).EndInit();
			this.DefaultDestinationPremiseIDGrid.ResumeLayout(false);
			this.DefaultDestinationPremiseIDGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid DefaultDestinationPremiseIDGrid;
	}
}
