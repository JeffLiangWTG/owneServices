namespace Enterprise.Registry.GUI
{
	partial class SubscriptionRulesRegistryControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.subscriptionRuleControl = new Enterprise.Registry.GUI.SubscriptionRuleControl();
			this.subscriptionRuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.subscriptionRulesListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.newButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ruleEditorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.subscriptionRuleGrid)).BeginInit();

			this.subscriptionRulesListGroupBox.SuspendLayout();
			this.ruleEditorGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SubscriptionRuleCollection);

			// 
			// subscriptionRuleControl
			// 
			this.subscriptionRuleControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.subscriptionRuleControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.SubscriptionRule)(((Enterprise.Registry.Business.SubscriptionRule)(null)))));
			this.subscriptionRuleControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.subscriptionRuleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.subscriptionRuleControl.Name = "subscriptionRuleControl";
			this.subscriptionRuleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 385, true);
			this.subscriptionRuleControl.TabIndex = 0;

			// 
			// subscriptionRuleGrid
			// 
			this.subscriptionRuleGrid.AllowNavigation = false;
			this.subscriptionRuleGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.subscriptionRuleGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SubscriptionRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionRule)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionRule)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.SubscriptionRule)(null)).IsDefaultForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionRule)(null)).CampaignType)));
			this.subscriptionRuleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c25e9f41-c247-47d5-89af-db02da80de37", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("22df3967-d7f8-459f-9e9d-e80a1a900d7f", "Published List Name");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4e30022b-96e9-45a6-8005-f05e6af22f53", "Default");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsDefaultForBinding";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("19c759d4-1f9c-43a2-abdd-af82ccfcd279", "Campaign Type");
			zDropEditColumnStyleInfo1.ColumnName = "CampaignType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			this.subscriptionRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.subscriptionRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.subscriptionRuleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.subscriptionRuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.subscriptionRuleGrid.CopySelectedRowsAllowed = true;
			this.subscriptionRuleGrid.GridId = "22f603a6-2e3e-4dd7-a7fe-1c6f4a4ad50c";
			this.subscriptionRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.subscriptionRuleGrid.LayoutKey = "subscriptionNameGrid";
			this.subscriptionRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.subscriptionRuleGrid.Name = "subscriptionRuleGrid";
			this.subscriptionRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 361, true);
			this.subscriptionRuleGrid.TabIndex = 1;
			// 
			// subscriptionRulesListGroupBox
			// 
			this.subscriptionRulesListGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("77c88ab3-0ffc-4c07-b567-27d2d4a835a1", "Published List");
			this.subscriptionRulesListGroupBox.Controls.Add(this.newButton);
			this.subscriptionRulesListGroupBox.Controls.Add(this.subscriptionRuleGrid);
			this.subscriptionRulesListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.subscriptionRulesListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.subscriptionRulesListGroupBox.Name = "subscriptionRulesListGroupBox";
			this.subscriptionRulesListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 415, true);
			this.subscriptionRulesListGroupBox.TabIndex = 2;
			this.subscriptionRulesListGroupBox.TabStop = false;
			// 
			// newButton
			// 
			this.newButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.newButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("28f0b502-b3e5-4c49-94a6-d7264d34955b", "New");
			this.newButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 386, true);
			this.newButton.Name = "newButton";
			this.newButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.newButton.TabIndex = 2;
			this.newButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// ruleEditorGroupBox
			// 
			this.ruleEditorGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ruleEditorGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("48cab363-f3b3-411c-9a35-750b454d19bd", "Subscription Rules");
			this.ruleEditorGroupBox.Controls.Add(this.subscriptionRuleControl);
			this.ruleEditorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.ruleEditorGroupBox.Name = "ruleEditorGroupBox";
			this.ruleEditorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 361, true);
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
			this.mainSplitContainer.Panel1.Controls.Add(this.subscriptionRulesListGroupBox);
			this.mainSplitContainer.Panel1MinSize = 200;
			this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
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
			this.Name = "SubscriptionListRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 415, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.subscriptionRuleGrid)).EndInit();
			this.subscriptionRulesListGroupBox.ResumeLayout(false);
			this.ruleEditorGroupBox.ResumeLayout(false);
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		protected ZArchitecture.ZGrid subscriptionRuleGrid;
		private SubscriptionRuleControl subscriptionRuleControl;
		private ZArchitecture.GUI.ZGroupBox subscriptionRulesListGroupBox;
		private ZArchitecture.GUI.ZGroupBox ruleEditorGroupBox;
		private ZArchitecture.GUI.ZButton newButton;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;

	}
}
