namespace Enterprise.Registry.GUI
{
	partial class DbHealthWarningListControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.warningZGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.warningZGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DbHealthWarningRegistryCollection);
			// 
			// warningZGrid
			// 
			this.warningZGrid.AllowNavigation = false;
			this.warningZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.warningZGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DbHealthWarningRegistryElement)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DbHealthWarningRegistryElement)(null)).WarningType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DbHealthWarningRegistryElement)(null)).Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.DbHealthWarningRegistryElement)(null)).IsAcknowledged)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DbHealthWarningRegistryElement)(null)).AcknowledgedByUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.DbHealthWarningRegistryElement)(null)).AcknowledgedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DbHealthWarningRegistryElement)(null)).Description)));
			this.warningZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e177e9d8-f7dc-4a4e-9b4f-bb1676c9c818", "Warning Type");
			zTextBoxColumnStyleInfo1.ColumnName = "WarningType";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("254ede8b-beb4-4582-ad12-b1a96e72f5af", "Source");
			zTextBoxColumnStyleInfo2.ColumnName = "Source";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f7873175-d9d7-4faa-8051-f69b993ceac7", "ACK");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsAcknowledged";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("06842a73-9e58-4dd6-9a94-5e0b2f1abd1c", "ACK User");
			zTextBoxColumnStyleInfo3.ColumnName = "AcknowledgedByUser";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("28a1e578-d3e8-46d8-953f-2c33a66848a6", "ACK Date");
			zDateEditColumnStyleInfo1.ColumnName = "AcknowledgedDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8c1b52d0-bfab-4091-976f-137a3e20d58e", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.warningZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.warningZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.warningZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.warningZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.warningZGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.warningZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.warningZGrid.CopySelectedRowsAllowed = true;
			this.warningZGrid.GridId = "d7855bad-3e15-45a5-944e-95742ac826e0";
			this.warningZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.warningZGrid.LayoutKey = "warningZGrid";
			this.warningZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.warningZGrid.Name = "warningZGrid";
			this.warningZGrid.ReadOnly = true;
			this.warningZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 250, true);
			this.warningZGrid.TabIndex = 0;
			// 
			// DbHealthWarningListControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.warningZGrid);
			this.Name = "DbHealthWarningListControl";
			this.ReadOnly = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 256, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.warningZGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid warningZGrid;

	}
}
