namespace Enterprise.Registry.GUI
{
	partial class GlowOpportunityStatusRegistryControl
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
			this.GlowOpportunityStatusGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowOpportunityStatusGrid)).BeginInit();
			this.GlowOpportunityStatusGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.GlowOpportunityStatusCollection);
			// 
			// GLowOpportunityStatusGrid
			// 
			this.GlowOpportunityStatusGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GlowOpportunityStatusGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.GlowOpportunityStatus)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowOpportunityStatus)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowOpportunityStatus)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.GlowOpportunityStatus)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowOpportunityStatus)(null)).TradeStatus)));
			this.GlowOpportunityStatusGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("363e43b8-d41c-490e-b894-d37376eefaee", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59e2a0fb-f930-4034-972b-664bdbba312e", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59781893-EE53-4ECE-A402-61F2227B8501", "Enabled");
			zCheckBoxColumnStyleInfo1.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("53fa511c-4170-4ef2-972e-06f6c6e7a722", "Trade Status");
			zDropEditColumnStyleInfo1.ColumnName = "TradeStatus";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GlowOpportunityStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GlowOpportunityStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GlowOpportunityStatusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GlowOpportunityStatusGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GlowOpportunityStatusGrid.CopySelectedRowsAllowed = true;
			this.GlowOpportunityStatusGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.GlowOpportunityStatusGrid.GridId = "C4B51D5F-9374-4198-94BB-14678797F912";
			this.GlowOpportunityStatusGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GlowOpportunityStatusGrid.LayoutKey = "GlowOpportunityStatusGrid";
			this.GlowOpportunityStatusGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GlowOpportunityStatusGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GlowOpportunityStatusGrid.Name = "GlowOpportunityStatusGrid";
			this.GlowOpportunityStatusGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 300, true);
			this.GlowOpportunityStatusGrid.TabIndex = 0;
			// 
			// UpButton
			// 
			this.UpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d1834e3b-ddec-4073-890e-88d743ca5978", "Move Up");
			this.UpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 3, true);
			this.UpButton.Name = "UpButton";
			this.UpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpButton.TabIndex = 1;
			this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
			// 
			// DownButton
			// 
			this.DownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4be81931-8f8f-4257-9493-0909c791c6ad", "Move Down");
			this.DownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 32, true);
			this.DownButton.Name = "DownButton";
			this.DownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DownButton.TabIndex = 2;
			this.DownButton.Click += new System.EventHandler(this.DownButton_Click);
			// 
			// OpportunityStatusRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GlowOpportunityStatusGrid);
			this.Controls.Add(this.DownButton);
			this.Controls.Add(this.UpButton);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "GlowOpportunityStatusRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowOpportunityStatusGrid)).EndInit();
			this.GlowOpportunityStatusGrid.ResumeLayout(false);
			this.GlowOpportunityStatusGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid GlowOpportunityStatusGrid;
		private Enterprise.ZArchitecture.GUI.ZButton UpButton;
		private Enterprise.ZArchitecture.GUI.ZButton DownButton;
	}
}
