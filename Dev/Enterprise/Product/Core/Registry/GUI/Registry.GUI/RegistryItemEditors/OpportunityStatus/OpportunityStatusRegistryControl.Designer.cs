namespace Enterprise.Registry.GUI
{
	partial class OpportunityStatusRegistryControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.OpportunityStatusGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OpportunityStatusGrid)).BeginInit();
			this.OpportunityStatusGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.OpportunityStatusCollection);
			// 
			// OpportunityStatusGrid
			// 
			this.OpportunityStatusGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OpportunityStatusGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OpportunityStatus)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OpportunityStatus)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OpportunityStatus)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.OpportunityStatus)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.OpportunityStatus)(null)).EffectiveAgreement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.OpportunityStatus)(null)).Enabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OpportunityStatus)(null)).TradeStatus)));
			this.OpportunityStatusGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("363e43b8-d41c-490e-b894-d37376eefaee", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59e2a0fb-f930-4034-972b-664bdbba312e", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d884de07-4b8c-4774-bcc1-8d8a076b84b3", "Auto Close");
			zCheckBoxColumnStyleInfo1.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8bc82ea4-47f7-4f4c-bcce-be87b3bdeefe", "Effective Agreement");
			zCheckBoxColumnStyleInfo2.ColumnName = "EffectiveAgreement";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59781893-EE53-4ECE-A402-61F2227B8501", "Enabled");
			zCheckBoxColumnStyleInfo3.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("53fa511c-4170-4ef2-972e-06f6c6e7a722", "Trade Status");
			zDropEditColumnStyleInfo1.ColumnName = "TradeStatus";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OpportunityStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OpportunityStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OpportunityStatusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OpportunityStatusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.OpportunityStatusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.OpportunityStatusGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OpportunityStatusGrid.CopySelectedRowsAllowed = true;
			this.OpportunityStatusGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OpportunityStatusGrid.GridId = "69af8564-ea26-4069-a663-42d715ceadd7";
			this.OpportunityStatusGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OpportunityStatusGrid.LayoutKey = "OpportunityStatusGrid";
			this.OpportunityStatusGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OpportunityStatusGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OpportunityStatusGrid.Name = "OpportunityStatusGrid";
			this.OpportunityStatusGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.OpportunityStatusGrid.TabIndex = 0;
			// 
			// OpportunityStatusRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OpportunityStatusGrid);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "OpportunityStatusRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OpportunityStatusGrid)).EndInit();
			this.OpportunityStatusGrid.ResumeLayout(false);
			this.OpportunityStatusGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid OpportunityStatusGrid;
	}
}
