using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AccountingOnFormFilterControl
	{
		#region Component Designer generated code

		readonly Container components = null;

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "JH_JobNum";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 76, true);
			this.FilteredGrid.TabIndex = 19;
			this.FilteredGrid.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ARAP.Invoicing.PeriodicInvoiceSelectableJobCollection);
			// 
			// AccountingOnFormFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "AccountingOnFormFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 145, true);
			((ISupportInitialize)(this.FilteredGrid)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
