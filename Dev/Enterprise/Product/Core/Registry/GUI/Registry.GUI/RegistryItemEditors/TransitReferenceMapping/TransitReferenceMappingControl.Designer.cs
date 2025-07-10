
namespace Enterprise.Registry.GUI
{
	partial class TransitReferenceMappingControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TransitReferenceMappingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TransitReferenceMappingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransitReferenceMappingGrid)).BeginInit();
			this.TransitReferenceMappingGrid.SuspendLayout();
			this.TransitReferenceMappingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.TransitReferenceMappingConfiguration);
			// 
			// TransitReferenceMappingGrid
			// 
			this.TransitReferenceMappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransitReferenceMappingGrid, "TransitReferenceMappingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMappingConfiguration)(null)).TransitReferenceMappingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMapping)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMappingConfiguration)(null)).TransitReferenceMappingCollection)).SyncRoot)).SourceCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMapping)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMappingConfiguration)(null)).TransitReferenceMappingCollection)).SyncRoot)).SourceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMapping)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMappingConfiguration)(null)).TransitReferenceMappingCollection)).SyncRoot)).TargetCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMapping)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMappingConfiguration)(null)).TransitReferenceMappingCollection)).SyncRoot)).TargetType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMapping)(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.TransitReferenceMappingConfiguration)(null)).TransitReferenceMappingCollection)).SyncRoot)).Direction)));
			this.TransitReferenceMappingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransitReferenceMappingControl|7deb52e4-b71c-490e-9edc-5c4aaa42065a", "Source Category");
			zDropEditColumnStyleInfo1.ColumnName = "SourceCategory";
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransitReferenceMappingControl|ec490d3e-a712-46d7-99e5-af3e924d46fd", "Source Type");
			zDropEditColumnStyleInfo2.ColumnName = "SourceType";
			zDropEditColumnStyleInfo3.Caption = null;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransitReferenceMappingControl|6dbdd4e9-2f2f-49d4-951a-7ae3ddcaad43", "Target Category");
			zDropEditColumnStyleInfo3.ColumnName = "TargetCategory";
			zDropEditColumnStyleInfo4.Caption = null;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransitReferenceMappingControl|9eeecf00-2e52-42b3-857f-123d5c993cac", "Target Type");
			zDropEditColumnStyleInfo4.ColumnName = "TargetType";
			zDropEditColumnStyleInfo5.Caption = null;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransitReferenceMappingControl|fbd35977-21d5-4459-8195-508d07dc278b", "Direction");
			zDropEditColumnStyleInfo5.ColumnName = "Direction";
			this.TransitReferenceMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TransitReferenceMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransitReferenceMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TransitReferenceMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TransitReferenceMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TransitReferenceMappingGrid.CopySelectedRowsAllowed = true;
			this.TransitReferenceMappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransitReferenceMappingGrid.GridId = "5d90e4a5-62ab-4e75-b631-d481238ef88c";
			this.TransitReferenceMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransitReferenceMappingGrid.LayoutKey = "TransitReferenceMappingGrid";
			this.TransitReferenceMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransitReferenceMappingGrid.Name = "TransitReferenceMappingGrid";
			this.TransitReferenceMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 276, true);
			this.TransitReferenceMappingGrid.TabIndex = 6;
			// 
			// TransitReferenceMappingGroupBox
			// 
			this.TransitReferenceMappingGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("939c0c6b-28fd-4feb-a10d-9e9dc3f2bf8a", "Transit Reference Mapping");
			this.TransitReferenceMappingGroupBox.Controls.Add(this.TransitReferenceMappingGrid);
			this.TransitReferenceMappingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransitReferenceMappingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransitReferenceMappingGroupBox.Name = "TransitReferenceMappingGroupBox";
			this.TransitReferenceMappingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			this.TransitReferenceMappingGroupBox.TabIndex = 7;
			this.TransitReferenceMappingGroupBox.TabStop = false;
			// 
			// TransitReferenceMappingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransitReferenceMappingGroupBox);
			this.Name = "TransitReferenceMappingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TransitReferenceMappingGrid)).EndInit();
			this.TransitReferenceMappingGrid.ResumeLayout(false);
			this.TransitReferenceMappingGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		ZArchitecture.ZGrid TransitReferenceMappingGrid;
		ZArchitecture.GUI.ZGroupBox TransitReferenceMappingGroupBox;
	}
}
