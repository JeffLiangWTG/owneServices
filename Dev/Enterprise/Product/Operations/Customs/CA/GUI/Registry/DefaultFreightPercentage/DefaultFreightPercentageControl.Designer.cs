namespace Enterprise.Customs.CA.GUI
{
	public partial class DefaultFreightPercentageControl
	{
		internal ZArchitecture.ZGrid DefaultFreightPercentageGrid;

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DefaultFreightPercentageGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultFreightPercentageGrid)).BeginInit();
			this.DefaultFreightPercentageGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Registry.DefaultFreightPercentage);
			// 
			// DefaultFreightPercentageGrid
			// 
			this.DefaultFreightPercentageGrid.AllowNavigation = false;
			this.DefaultFreightPercentageGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.DefaultFreightPercentageGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Registry.DefaultFreightPercentage)(null)).ModeofTransport);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Registry.DefaultFreightPercentage)(null)).TransportTypeList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Registry.DefaultFreightPercentage)(null)).FreightPercentage);
			this.DefaultFreightPercentageGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "TransportTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("349bd07b-54b4-4210-9449-8abc71411649", "Mode of Transport");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ModeofTransport";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1f19e62b-e041-418e-9443-c449bbe54ea4", "Freight Percentage");
			zCalcEditColumnStyleInfo1.ColumnName = "FreightPercentage";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DefaultFreightPercentageGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DefaultFreightPercentageGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DefaultFreightPercentageGrid.GridId = "e314bef2-048b-4bd0-8301-e41bf9740163";
			this.DefaultFreightPercentageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultFreightPercentageGrid.LayoutKey = "DefaultFreightPercentageGrid";
			this.DefaultFreightPercentageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultFreightPercentageGrid.Name = "DefaultFreightPercentageGrid";
			this.DefaultFreightPercentageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 96, true);
			this.DefaultFreightPercentageGrid.TabIndex = 0;
			// 
			// DefaultFreightPercentageControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultFreightPercentageGrid);
			this.Name = "DefaultFreightPercentageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 96, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultFreightPercentageGrid)).EndInit();
			this.DefaultFreightPercentageGrid.ResumeLayout(false);
			this.DefaultFreightPercentageGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
