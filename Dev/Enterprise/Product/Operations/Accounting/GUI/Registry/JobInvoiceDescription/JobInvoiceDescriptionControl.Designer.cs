namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobInvoiceDescriptionControl
	{
		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.ConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MacroButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JobInvoiceDescriptionCollection);
			// 
			// ConfigurationGrid
			// 
			this.ConfigurationGrid.AllowNavigation = false;
			this.ConfigurationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.JobInvoiceDescription)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JobInvoiceDescription)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JobInvoiceDescription)(null)).DirectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JobInvoiceDescription)(null)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JobInvoiceDescription)(null)).InvoiceDescription)));
			this.ConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoiceDescriptionControl|febe2667-d543-4b0f-9e56-6fa9ff3cbf38", "Job Type");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "JobType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoiceDescriptionControl|61a2c64e-4181-40b8-b400-695a35f9d62e", "Direction");
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoiceDescriptionControl|bdfcedf7-41d5-4bbe-a736-068a63d093f7", "Mode");
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "Mode";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoiceDescriptionControl|f17c099d-f112-4cba-8a67-5959892d2226", "Inv. Description", "Invoice Description", "Job Invoice Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "InvoiceDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.ConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ConfigurationGrid.CopySelectedRowsAllowed = true;
			this.ConfigurationGrid.GridId = "5E272366-6CB7-480A-A460-A48606AE16F6";
			this.ConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfigurationGrid.LayoutKey = "ConfigurationGrid";
			this.ConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.ConfigurationGrid.Name = "ConfigurationGrid";
			this.ConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 192, true);
			this.ConfigurationGrid.TabIndex = 0;
			this.ConfigurationGrid.Enter += new System.EventHandler(this.RememberCursorPosition);
			this.ConfigurationGrid.CursorChanged += new System.EventHandler(this.RememberCursorPosition);
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.MacroButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 31, true);
			this.ButtonPanel.TabIndex = 1;
			// 
			// MacroButton
			// 
			this.MacroButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MacroButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoiceDescriptionControl|14b6d44a-5e1d-489c-be83-388144f3954e", "Insert Field", "Displays a list of all available fields that can be included on the Invoice Description.\r\nYou can choose a field and it will automatically be inserted into the registry item value.");
			this.MacroButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 3, true);
			this.MacroButton.Name = "MacroButton";
			this.MacroButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MacroButton.TabIndex = 0;
			this.MacroButton.UseVisualStyleBackColor = true;
			this.MacroButton.Click += new System.EventHandler(this.ShowMapTreePresenter);
			// 
			// JobInvoiceDescriptionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ButtonPanel);
			this.Controls.Add(this.ConfigurationGrid);
			this.Name = "JobInvoiceDescriptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton MacroButton;
		internal Enterprise.ZArchitecture.ZGrid ConfigurationGrid;
	}
}
