using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class KoreaSouthEInvoicingDataElementConfigurationControl : RegistryZUserControl
	{
		ZArchitecture.GUI.ZPanel ButtonPanel;
		ZArchitecture.GUI.ZButton MacroButton;
		ZArchitecture.ZGrid ConfigurationGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.ConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MacroButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).BeginInit();
			this.ConfigurationGrid.SuspendLayout();
			this.ButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDataElementConfigurationCollection);
			// 
			// ConfigurationGrid
			// 
			this.ConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDataElementConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDataElementConfiguration)(null)).InvoiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDataElementConfiguration)(null)).DataElement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDataElementConfiguration)(null)).Configuration)));
			this.ConfigurationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "InvoiceType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "DataElement";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zMultiLineTextBoxColumnInfo1.ColumnName = "Configuration";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.ConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ConfigurationGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationGrid.GridId = "5A98F68D-BB23-428B-9428-05AB228DF1AF";
			this.ConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfigurationGrid.LayoutKey = "ConfigurationGrid";
			this.ConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.ConfigurationGrid.Name = "ConfigurationGrid";
			this.ConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 193, true);
			this.ConfigurationGrid.TabIndex = 3;
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
			this.MacroButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("KoreaSouthEInvoicingDataElementConfigurationControl|202BE969-2604-461C-9891-3C5D4FC9A981", "Insert Field", "Displays a list of all available fields that can be included on the Invoice Description.\r\nYou can choose a field and it will automatically be inserted into the registry item value.");
			this.MacroButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 3, true);
			this.MacroButton.Name = "MacroButton";
			this.MacroButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MacroButton.TabIndex = 2;
			this.MacroButton.ToolTipCaption = null;
			this.MacroButton.UseVisualStyleBackColor = true;
			this.MacroButton.Click += new System.EventHandler(this.ShowMapTreePresenter);
			// 
			// KoreaSouthEInvoicingDataElementConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConfigurationGrid);
			this.Controls.Add(this.ButtonPanel);
			this.Name = "KoreaSouthEInvoicingDataElementConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).EndInit();
			this.ConfigurationGrid.ResumeLayout(false);
			this.ConfigurationGrid.PerformLayout();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
