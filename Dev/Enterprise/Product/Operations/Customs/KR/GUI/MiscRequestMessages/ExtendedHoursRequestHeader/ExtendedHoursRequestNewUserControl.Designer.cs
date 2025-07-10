namespace Enterprise.Customs.KR.GUI
{
	partial class ExtendedHoursRequestNewUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ExtendedHoursRequestHeaderPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.LineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ExtenedHoursRequestLinesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.HeaderGroupBox.SuspendLayout();
            this.LineGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExtenedHoursRequestLinesBoundGrid)).BeginInit();
            this.ExtenedHoursRequestLinesBoundGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader);
            // 
            // HeaderGroupBox
            // 
            this.HeaderGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("F6C43EB6-777C-444A-A05F-615C2782C09E", "Details");
            this.HeaderGroupBox.Controls.Add(this.ExtendedHoursRequestHeaderPanel);
            this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.HeaderGroupBox.Name = "HeaderGroupBox";
            this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 163, true);
            this.HeaderGroupBox.TabIndex = 0;
            this.HeaderGroupBox.TabStop = false;
            // 
            // ExtendedHoursRequestHeaderPanel
            // 
            this.ExtendedHoursRequestHeaderPanel.AllowDrop = true;
            this.ExtendedHoursRequestHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtendedHoursRequestHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
            this.ExtendedHoursRequestHeaderPanel.Name = "ExtendedHoursRequestHeaderPanel";
            this.ExtendedHoursRequestHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 144, true);
            this.ExtendedHoursRequestHeaderPanel.TabIndex = 0;
            // 
            // LineGroupBox
            // 
            this.LineGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("18F276B1-E21A-439A-B588-1B9FEFF182FA", "Related Entries");
            this.LineGroupBox.Controls.Add(this.ExtenedHoursRequestLinesBoundGrid);
            this.LineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 163, true);
            this.LineGroupBox.Name = "LineGroupBox";
            this.LineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 282, true);
            this.LineGroupBox.TabIndex = 1;
            this.LineGroupBox.TabStop = false;
            // 
            // ExtenedHoursRequestLinesBoundGrid
            // 
            this.ExtenedHoursRequestLinesBoundGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ExtenedHoursRequestLinesBoundGrid, "ExtendedHoursRequestLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).ExtendedHoursRequestLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).ExtendedHoursRequestLines)).SyncRoot)).FormattedReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).ExtendedHoursRequestLines)).SyncRoot)).CustomsValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).ExtendedHoursRequestLines)).SyncRoot)).PackageCount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).ExtendedHoursRequestLines)).SyncRoot)).TotalWeight)));
            this.ExtenedHoursRequestLinesBoundGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "FormattedReferenceNumber";
						zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
						zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "CustomsValue";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "PackageCount";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "TotalWeight";
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            this.ExtenedHoursRequestLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ExtenedHoursRequestLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ExtenedHoursRequestLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.ExtenedHoursRequestLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.ExtenedHoursRequestLinesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtenedHoursRequestLinesBoundGrid.GridId = "b2a0d480-086c-4fd0-ad57-462b58daaeb4";
            this.ExtenedHoursRequestLinesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ExtenedHoursRequestLinesBoundGrid.LayoutKey = "ExtenedHoursRequestLinesBoundGrid";
            this.ExtenedHoursRequestLinesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
            this.ExtenedHoursRequestLinesBoundGrid.Name = "ExtenedHoursRequestLinesBoundGrid";
            this.ExtenedHoursRequestLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 262, true);
            this.ExtenedHoursRequestLinesBoundGrid.TabIndex = 0;
            // 
            // ExtendedHoursRequestNewUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.LineGroupBox);
            this.Controls.Add(this.HeaderGroupBox);
            this.Name = "ExtendedHoursRequestNewUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 445, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.HeaderGroupBox.ResumeLayout(false);
            this.HeaderGroupBox.PerformLayout();
            this.LineGroupBox.ResumeLayout(false);
            this.LineGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExtenedHoursRequestLinesBoundGrid)).EndInit();
            this.ExtenedHoursRequestLinesBoundGrid.ResumeLayout(false);
            this.ExtenedHoursRequestLinesBoundGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
		private ZArchitecture.GUI.ZGroupBox LineGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel ExtendedHoursRequestHeaderPanel;
		public ZArchitecture.ZGrid ExtenedHoursRequestLinesBoundGrid;
	}
}
