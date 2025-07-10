namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceCopiesControl
	{

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.InvoiceCopiesGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceCopiesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.InvoiceCopyCollection);
			// 
			// InvoiceCopiesGrid
			// 
			this.InvoiceCopiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceCopiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.InvoiceCopy)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Accounting.Registry.Business.InvoiceCopy)(null)).Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.InvoiceCopy)(null)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.InvoiceCopy)(null)).DeliveryMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.InvoiceCopy)(null)).DeliveryMethodList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.InvoiceCopy)(null)).IncludeTradingTerms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.InvoiceCopy)(null)).Message)));
			this.InvoiceCopiesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceCopiesControl|ff4f5056-4e7b-44e1-b46d-e478f47ea8a5", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceCopiesControl|26dfd500-4514-4831-b212-ac71c0a5e786", "Delivery Method");
			zDropEditColumnStyleInfo1.ColumnName = "DeliveryMethod";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceCopiesControl|b6851ba9-ff63-42ae-adf1-f1a4ad2b178b", "Include Trading Terms");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeTradingTerms";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "Message";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "Order";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.InvoiceCopiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoiceCopiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoiceCopiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoiceCopiesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoiceCopiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoiceCopiesGrid.GridId = "841f0a58-4246-479f-8f9a-9639dba902e4";
			this.InvoiceCopiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceCopiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceCopiesGrid.LayoutKey = "InvoiceCopiesGrid";
			this.InvoiceCopiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceCopiesGrid.Name = "InvoiceCopiesGrid";
			this.InvoiceCopiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			this.InvoiceCopiesGrid.TabIndex = 0;
			// 
			// InvoiceCopiesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceCopiesGrid);
			this.Name = "InvoiceCopiesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceCopiesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		ZArchitecture.ZGrid InvoiceCopiesGrid;
	}
}
