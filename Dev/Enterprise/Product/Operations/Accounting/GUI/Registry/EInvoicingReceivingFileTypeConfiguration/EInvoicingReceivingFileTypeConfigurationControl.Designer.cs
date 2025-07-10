using System.ComponentModel;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class EInvoicingReceivingFileTypeConfigurationControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		IContainer components = null;

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

		internal Enterprise.ZArchitecture.ZGrid EInvoicingReceivingFileTypeConfigurationGrid;

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			EInvoicingReceivingFileTypeConfigurationGrid.ReadOnly = readOnly;
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.EInvoicingReceivingFileTypeConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EInvoicingReceivingFileTypeConfigurationGrid)).BeginInit();
			this.EInvoicingReceivingFileTypeConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(EInvoicingReceivingFileTypeConfigurationCollection);
			//
			// EInvoicingReceivingFileTypeConfigurationGrid
			//
			this.EInvoicingReceivingFileTypeConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EInvoicingReceivingFileTypeConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Registry.Business.EInvoicingReceivingFileTypeConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Registry.Business.EInvoicingReceivingFileTypeConfiguration)(null)).FileFormat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Registry.Business.EInvoicingReceivingFileTypeConfiguration)(null)).DebtorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Registry.Business.EInvoicingReceivingFileTypeConfiguration)(null)).DebtorCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Registry.Business.EInvoicingReceivingFileTypeConfiguration)(null)).DebtorDescription)));
			this.EInvoicingReceivingFileTypeConfigurationGrid.CaptionVisible = false;

			zDropEditColumnStyleInfo1.ColumnName = "FileFormat";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B7B346EA-E3D0-42EF-80C6-9C6BFC8DBB83", "File Format");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDropEditColumnStyleInfo2.ColumnName = "DebtorType";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("49CCD54E-F670-4E9F-8F8B-67A4C20355A3", "Type");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zGuidFindBoxColumnStyleInfo1.ColumnName = "DebtorCode";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("78114298-8643-4F2E-9631-5AAB23BD1C20", "Code");
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo1.ColumnName = "DebtorDescription";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A270F7B1-501D-4E63-AFBD-09FCB57C696C", "Debtor Name / Debtor Group Description");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

			this.EInvoicingReceivingFileTypeConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EInvoicingReceivingFileTypeConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EInvoicingReceivingFileTypeConfigurationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.EInvoicingReceivingFileTypeConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EInvoicingReceivingFileTypeConfigurationGrid.CopySelectedRowsAllowed = true;
			this.EInvoicingReceivingFileTypeConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EInvoicingReceivingFileTypeConfigurationGrid.GridId = "13FD21BD-C6C9-4086-94BC-2D33790AC46F";
			this.EInvoicingReceivingFileTypeConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EInvoicingReceivingFileTypeConfigurationGrid.LayoutKey = "EInvoicingReceivingFileTypeConfigurationGrid";
			this.EInvoicingReceivingFileTypeConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EInvoicingReceivingFileTypeConfigurationGrid.Name = "EInvoicingReceivingFileTypeConfigurationGrid";
			this.EInvoicingReceivingFileTypeConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.EInvoicingReceivingFileTypeConfigurationGrid.TabIndex = 0;
			//
			// EInvoicingReceivingFileTypeConfigurationControl
			//
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.EInvoicingReceivingFileTypeConfigurationGrid);
			this.Name = "EInvoicingReceivingFileTypeConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EInvoicingReceivingFileTypeConfigurationGrid)).EndInit();
			this.EInvoicingReceivingFileTypeConfigurationGrid.ResumeLayout(false);
			this.EInvoicingReceivingFileTypeConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

