namespace Enterprise.Registry.GUI
{
	partial class SalesRelationDirectionRuleControl
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
			this.moveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.moveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.nodeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nodeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SalesRelationDirectionRule);
			// 
			// moveUpButton
			// 
			this.moveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moveUpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b095e42c-a99b-4403-9a30-ef344c82c23c", "Move Up");
			this.moveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 3, true);
			this.moveUpButton.Name = "moveUpButton";
			this.moveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.moveUpButton.TabIndex = 1;
			this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
			// 
			// moveDownButton
			// 
			this.moveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moveDownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3dadce11-08eb-4e0c-a814-4938ca357f67", "Move Down");
			this.moveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 32, true);
			this.moveDownButton.Name = "moveDownButton";
			this.moveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.moveDownButton.TabIndex = 2;
			this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
			// 
			// nodeGrid
			// 
			this.nodeGrid.AllowNavigation = false;
			this.nodeGrid.AllowSorting = false;
			this.nodeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.nodeGrid, "Nodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SalesRelationDirectionRule)(null)).Nodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SalesRelationRuleNode)(((System.Collections.IList)(((Enterprise.Registry.Business.SalesRelationDirectionRule)(null)).Nodes)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SalesRelationRuleNode)(((System.Collections.IList)(((Enterprise.Registry.Business.SalesRelationDirectionRule)(null)).Nodes)).SyncRoot)).TypeDescription)));
			this.nodeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("340d07ec-e42c-4a1b-bc39-c69baa9af5f8", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "Type";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.IsSortable = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("31aa6082-80f9-4917-9ef8-a9c5cf0162ba", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo1.IsSortable = false;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.nodeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.nodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.nodeGrid.CopySelectedRowsAllowed = true;
			this.nodeGrid.GridId = "5dcacd63-e254-4220-9972-f94cc118f766";
			this.nodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.nodeGrid.LayoutKey = "ruleSequenceGrid";
			this.nodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 6, true);
			this.nodeGrid.Name = "nodeGrid";
			this.nodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 318, true);
			this.nodeGrid.TabIndex = 0;
			// 
			// SalesRelationDirectionRuleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.nodeGrid);
			this.Controls.Add(this.moveDownButton);
			this.Controls.Add(this.moveUpButton);
			this.Name = "SalesRelationDirectionRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 324, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nodeGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZButton moveUpButton;
		private ZArchitecture.GUI.ZButton moveDownButton;
		private ZArchitecture.ZGrid nodeGrid;
	}
}
