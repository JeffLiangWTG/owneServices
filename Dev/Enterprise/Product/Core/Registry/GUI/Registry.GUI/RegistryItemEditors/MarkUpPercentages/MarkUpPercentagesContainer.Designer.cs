using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class MarkUpPercentagesContainer : ZUserControl
	{
		Enterprise.ZArchitecture.ZGrid MarkUpGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MarkUpGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MarkUpGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper);
			// 
			// MarkUpGrid
			// 
			this.MarkUpGrid.AllowNavigation = false;
			this.MarkUpGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MarkUpGrid, "MarkUpPercentages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MarkUpPercentage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)).SyncRoot)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)).SyncRoot)).Modes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MarkUpPercentage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)).SyncRoot)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)).SyncRoot)).Locations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.MarkUpPercentage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)).SyncRoot)).Percentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.MarkUpPercentage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)).SyncRoot)).Minimum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.MarkUpPercentage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MarkUpPercentagesCollectionWrapper)(null)).MarkUpPercentages)).SyncRoot)).PerUnit)));
			this.MarkUpGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Modes";
			zDropEditColumnStyleInfo1.Caption = "Mode";
			zDropEditColumnStyleInfo1.ColumnName = "Mode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Locations";
			zCodeFindBoxColumnStyleInfo1.Caption = "Location";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Location";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Location;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Markup %";
			zCalcEditColumnStyleInfo1.ColumnName = "Percentage";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Minimum";
			zCalcEditColumnStyleInfo2.ColumnName = "Minimum";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Per Unit";
			zCalcEditColumnStyleInfo3.ColumnName = "PerUnit";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.MarkUpGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MarkUpGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MarkUpGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MarkUpGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MarkUpGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MarkUpGrid.GridId = "5f015ef5-2e36-4953-857b-3cc6639ded9e";
			this.MarkUpGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MarkUpGrid.LayoutKey = "zGrid1";
			this.MarkUpGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MarkUpGrid.Name = "MarkUpGrid";
			this.MarkUpGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.MarkUpGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 192, true);
			this.MarkUpGrid.TabIndex = 0;
			// 
			// MarkUpPercentagesContainer
			// 
			this.Controls.Add(this.MarkUpGrid);
			this.Name = "MarkUpPercentagesContainer";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 192, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MarkUpGrid)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
