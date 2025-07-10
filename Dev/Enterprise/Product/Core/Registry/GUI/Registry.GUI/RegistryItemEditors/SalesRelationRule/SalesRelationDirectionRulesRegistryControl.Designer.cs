namespace Enterprise.Registry.GUI
{
	partial class SalesRelationDirectionRulesRegistryControl
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
			this.directionRuleControl = new Enterprise.Registry.GUI.SalesRelationDirectionRuleControl();
			this.directionRuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.directionRulesListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.newButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ruleEditorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.directionRuleGrid)).BeginInit();
			this.directionRulesListGroupBox.SuspendLayout();
			this.ruleEditorGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SalesRelationDirectionRuleCollection);
			// 
			// directionRuleControl
			// 
			this.directionRuleControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.directionRuleControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.SalesRelationDirectionRule)(((Enterprise.Registry.Business.SalesRelationDirectionRule)(null)))));
			this.directionRuleControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.directionRuleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.directionRuleControl.Name = "directionRuleControl";
			this.directionRuleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 342, true);
			this.directionRuleControl.TabIndex = 0;
			// 
			// directionRuleGrid
			// 
			this.directionRuleGrid.AllowNavigation = false;
			this.directionRuleGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.directionRuleGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SalesRelationDirectionRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SalesRelationDirectionRule)(null)).NodeSequenceAsString)));
			this.directionRuleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ac1d5996-3d14-4c06-b850-e4a58f95a813", "Sequence");
			zTextBoxColumnStyleInfo1.ColumnName = "NodeSequenceAsString";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.directionRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.directionRuleGrid.CopySelectedRowsAllowed = true;
			this.directionRuleGrid.GridId = "7d5456d1-fccc-46df-b4fb-aad08804f5c3";
			this.directionRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.directionRuleGrid.LayoutKey = "salesRelationsRuleGrid";
			this.directionRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.directionRuleGrid.Name = "directionRuleGrid";
			this.directionRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 361, true);
			this.directionRuleGrid.TabIndex = 1;
			// 
			// directionRulesListGroupBox
			// 
			this.directionRulesListGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3a4987c3-996a-4aeb-b53a-ace37cb2c916", "Direction Rules List");
			this.directionRulesListGroupBox.Controls.Add(this.newButton);
			this.directionRulesListGroupBox.Controls.Add(this.directionRuleGrid);
			this.directionRulesListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.directionRulesListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.directionRulesListGroupBox.Name = "directionRulesListGroupBox";
			this.directionRulesListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 415, true);
			this.directionRulesListGroupBox.TabIndex = 2;
			this.directionRulesListGroupBox.TabStop = false;
			// 
			// newButton
			// 
			this.newButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.newButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("28f0b502-b3e5-4c49-94a6-d7264d34955b", "New");
			this.newButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 386, true);
			this.newButton.Name = "newButton";
			this.newButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.newButton.TabIndex = 2;
			this.newButton.Click += new System.EventHandler(this.newButton_Click);
			// 
			// ruleEditorGroupBox
			// 
			this.ruleEditorGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ruleEditorGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("026305a0-8535-4bcf-9f1f-a439bd99d629", "Selected Direction Rule Editor");
			this.ruleEditorGroupBox.Controls.Add(this.directionRuleControl);
			this.ruleEditorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.ruleEditorGroupBox.Name = "ruleEditorGroupBox";
			this.ruleEditorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 361, true);
			this.ruleEditorGroupBox.TabIndex = 3;
			this.ruleEditorGroupBox.TabStop = false;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.directionRulesListGroupBox);
			this.mainSplitContainer.Panel1MinSize = 200;
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.ruleEditorGroupBox);
			this.mainSplitContainer.Panel2MinSize = 200;
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 415, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(308);
			this.mainSplitContainer.TabIndex = 4;
			// 
			// SalesRelationDirectionRulesRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "SalesRelationDirectionRulesRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 415, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.directionRuleGrid)).EndInit();
			this.directionRulesListGroupBox.ResumeLayout(false);
			this.ruleEditorGroupBox.ResumeLayout(false);
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private SalesRelationDirectionRuleControl directionRuleControl;
		protected ZArchitecture.ZGrid directionRuleGrid;
		private ZArchitecture.GUI.ZGroupBox directionRulesListGroupBox;
		private ZArchitecture.GUI.ZGroupBox ruleEditorGroupBox;
		private ZArchitecture.GUI.ZButton newButton;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
	}
}
