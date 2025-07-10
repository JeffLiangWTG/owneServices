namespace Enterprise.Registry.GUI
{
	partial class VerboseLoggingControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.codeDescriptionTimeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.codeDescriptionTimeGrid)).BeginInit();
			this.codeDescriptionTimeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.VerboseLoggingBusinessObject);
			// 
			// codeDescriptionTimeGrid
			// 
			this.codeDescriptionTimeGrid.AllowNavigation = false;
			this.codeDescriptionTimeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.codeDescriptionTimeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.VerboseLoggingBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.VerboseLoggingBusinessObject)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.VerboseLoggingBusinessObject)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.VerboseLoggingBusinessObject)(null)).Value)));
			this.codeDescriptionTimeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("866f9e7f-acf2-4687-a767-300b4f2ad4b9", "Code", "Task code", "Service task code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e10ce5f5-3b3a-46f8-8606-25ec76c90b82", "Description", "Service task description");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b9ecbb38-1ee6-4c58-ac9e-d3b723688214", "Time (UTC)", "Date and Time (UTC)", "Verbose logging is enabled up to this time (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "Value";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.codeDescriptionTimeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.codeDescriptionTimeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.codeDescriptionTimeGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.codeDescriptionTimeGrid.GridId = "58468f2f-36ae-48a3-802e-8ce3ef1055db";
			this.codeDescriptionTimeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.codeDescriptionTimeGrid.LayoutKey = "zGrid1";
			this.codeDescriptionTimeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.codeDescriptionTimeGrid.Name = "codeDescriptionTimeGrid";
			this.codeDescriptionTimeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 348, true);
			this.codeDescriptionTimeGrid.TabIndex = 2;
			// 
			// VerboseLoggingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.codeDescriptionTimeGrid);
			this.Name = "VerboseLoggingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 348, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.codeDescriptionTimeGrid)).EndInit();
			this.codeDescriptionTimeGrid.ResumeLayout(false);
			this.codeDescriptionTimeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid codeDescriptionTimeGrid;
	}
}
