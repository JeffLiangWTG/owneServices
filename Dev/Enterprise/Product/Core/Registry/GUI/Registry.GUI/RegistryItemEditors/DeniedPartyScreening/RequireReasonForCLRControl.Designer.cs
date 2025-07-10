namespace Enterprise.Registry.GUI
{
	partial class RequireReasonForCLRControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.TableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OptionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.NoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.YesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RequireReasonForCLRGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TableLayoutPanel.SuspendLayout();
			this.OptionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RequireReasonForCLRGrid)).BeginInit();
			this.RequireReasonForCLRGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.RequireReasonForCLRWrapper);
			// 
			// TableLayoutPanel
			// 
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableLayoutPanel.Controls.Add(this.OptionGroupBox);
			this.TableLayoutPanel.Controls.Add(this.RequireReasonForCLRGrid);
			this.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TableLayoutPanel.Name = "TableLayoutPanel";
			this.TableLayoutPanel.RowCount = 1;
			this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60)));
			this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 400, true);
			this.TableLayoutPanel.TabIndex = 0;
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.Controls.Add(this.NoRadioButton);
			this.OptionGroupBox.Controls.Add(this.YesRadioButton);
			this.OptionGroupBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 3, true);
			this.OptionGroupBox.Name = "OptionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 52, true);
			this.OptionGroupBox.TabIndex = 0;
			this.OptionGroupBox.TabStop = false;
			// 
			// NoRadioButton
			// 
			this.NoRadioButton.AutoCheck = false;
			this.NoRadioButton.AutoSize = true;
			this.NoRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.NoRadioButton, "NoRadioButtonSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.RequireReasonForCLRWrapper)(null)).NoRadioButtonSelection)));
			this.NoRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("632B6072-1E1B-453B-8284-991F4A3CB55D", "No");
			this.NoRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 13, true);
			this.NoRadioButton.Name = "NoRadioButton";
			this.NoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.NoRadioButton.TabIndex = 1;
			this.NoRadioButton.TabStop = true;
			this.NoRadioButton.UseVisualStyleBackColor = false;
			// 
			// YesRadioButton
			// 
			this.YesRadioButton.AutoCheck = false;
			this.YesRadioButton.AutoSize = true;
			this.YesRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.YesRadioButton, "YesRadioButtonSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.RequireReasonForCLRWrapper)(null)).YesRadioButtonSelection)));
			this.YesRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ECDD2028-F46C-4823-9CA0-3C40891413FC", "Yes");
			this.YesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.YesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 13, true);
			this.YesRadioButton.Name = "YesRadioButton";
			this.YesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 17, true);
			this.YesRadioButton.TabIndex = 2;
			this.YesRadioButton.TabStop = true;
			this.YesRadioButton.UseVisualStyleBackColor = false;
			this.YesRadioButton.CheckedChanged += new System.EventHandler(this.YesRadioButton_CheckedChanged);
			// 
			// RequireReasonForCLRGrid
			// 
			this.RequireReasonForCLRGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RequireReasonForCLRGrid, "ItemCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RequireReasonForCLRWrapper)(null)).ItemCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RequireReasonForCLRItem)(((System.Collections.IList)(((Enterprise.Registry.Business.RequireReasonForCLRWrapper)(null)).ItemCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RequireReasonForCLRItem)(((System.Collections.IList)(((Enterprise.Registry.Business.RequireReasonForCLRWrapper)(null)).ItemCollection)).SyncRoot)).Title)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RequireReasonForCLRItem)(((System.Collections.IList)(((Enterprise.Registry.Business.RequireReasonForCLRWrapper)(null)).ItemCollection)).SyncRoot)).ClearingReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.RequireReasonForCLRItem)(((System.Collections.IList)(((Enterprise.Registry.Business.RequireReasonForCLRWrapper)(null)).ItemCollection)).SyncRoot)).IsMandatory)));
			this.RequireReasonForCLRGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("D5396C87-B0C6-4FDA-9A20-22580A48292C", "Code");
			zTextBoxColumnStyleInfo4.ColumnName = "Code";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2537EF0A-8CF1-43FC-8B8A-1A45F3E17074", "Title");
			zTextBoxColumnStyleInfo5.ColumnName = "Title";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("F7244013-3AF2-4B9F-8615-9F3AA01E25CB", "Clearing Reason");
			zTextBoxColumnStyleInfo6.ColumnName = "ClearingReason";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(700);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("66C38E58-7DFE-49D8-B412-F76449469FCF", "Mandatory");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsMandatory";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RequireReasonForCLRGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RequireReasonForCLRGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RequireReasonForCLRGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RequireReasonForCLRGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.RequireReasonForCLRGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequireReasonForCLRGrid.GridId = "cff6a447-6379-4bb2-a0e5-f24148bb6bde";
			this.RequireReasonForCLRGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RequireReasonForCLRGrid.LayoutKey = "RequireReasonForCLRGrid";
			this.RequireReasonForCLRGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
			this.RequireReasonForCLRGrid.Name = "RequireReasonForCLRGrid";
			this.RequireReasonForCLRGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 334, true);
			this.RequireReasonForCLRGrid.TabIndex = 2;
			// 
			// RequireReasonForCLRControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TableLayoutPanel);
			this.Name = "RequireReasonForCLRControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TableLayoutPanel.ResumeLayout(false);
			this.TableLayoutPanel.PerformLayout();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RequireReasonForCLRGrid)).EndInit();
			this.RequireReasonForCLRGrid.ResumeLayout(false);
			this.RequireReasonForCLRGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KGroupBox OptionGroupBox;
		CargoWise.Windows.UI.KTableLayoutPanel TableLayoutPanel;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton NoRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton YesRadioButton;
		internal Enterprise.ZArchitecture.ZGrid RequireReasonForCLRGrid;
	}
}
