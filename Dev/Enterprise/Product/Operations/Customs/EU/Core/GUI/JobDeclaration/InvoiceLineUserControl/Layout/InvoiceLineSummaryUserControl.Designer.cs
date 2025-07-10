namespace Enterprise.Customs.EU.GUI
{
    partial class InvoiceLineSummaryUserControl
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
            this.CustomsValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.GSTVATDeferredConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.StatisticalValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.ValueForVatConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CustomsValueConvertToLocalCurrencyControl.SuspendLayout();
            this.GSTVATDeferredConvertToLocalCurrencyControl.SuspendLayout();
            this.StatisticalValueConvertToLocalCurrencyControl.SuspendLayout();
            this.ValueForVatConvertToLocalCurrencyControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
            // 
            // CustomsValueConvertToLocalCurrencyControl
            // 
            this.CustomsValueConvertToLocalCurrencyControl.AllowDrop = true;
            this.CustomsValueConvertToLocalCurrencyControl.BindToAmount = "JI_CustomsValue";
            this.CustomsValueConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.CustomsValueConvertToLocalCurrencyControl.BindToUnit = "JI_RX_LocalCurrency";
            this.CustomsValueConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|2E2C6A85-89F9-41B1-86DB-442C5FAD20DA", "Customs Value");
            this.CustomsValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 113, true);
            this.CustomsValueConvertToLocalCurrencyControl.Name = "CustomsValueConvertToLocalCurrencyControl";
            this.CustomsValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.CustomsValueConvertToLocalCurrencyControl.TabIndex = 3;
            // 
            // GSTVATDeferredConvertToLocalCurrencyControl
            // 
            this.GSTVATDeferredConvertToLocalCurrencyControl.AllowDrop = true;
            this.GSTVATDeferredConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_GSTVATDeferred";
            this.GSTVATDeferredConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.GSTVATDeferredConvertToLocalCurrencyControl.BindToUnit = "JI_RX_LocalCurrency";
            this.GSTVATDeferredConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|2C748B70-682B-435E-BFBB-E1715AFE073A", "Def. VAT");
            this.GSTVATDeferredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 35, true);
            this.GSTVATDeferredConvertToLocalCurrencyControl.Name = "GSTVATDeferredConvertToLocalCurrencyControl";
            this.GSTVATDeferredConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.GSTVATDeferredConvertToLocalCurrencyControl.TabIndex = 0;
            // 
            // StatisticalValueConvertToLocalCurrencyControl
            // 
            this.StatisticalValueConvertToLocalCurrencyControl.AllowDrop = true;
            this.StatisticalValueConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_StatisticalValue";
            this.StatisticalValueConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.StatisticalValueConvertToLocalCurrencyControl.BindToUnit = "JI_RX_LocalCurrency";
            this.StatisticalValueConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|330C3C09-F384-4DAF-ABB3-AD8371042A3B", "Stat. Value");
            this.StatisticalValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 61, true);
            this.StatisticalValueConvertToLocalCurrencyControl.Name = "StatisticalValueConvertToLocalCurrencyControl";
            this.StatisticalValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.StatisticalValueConvertToLocalCurrencyControl.TabIndex = 1;
            // 
            // ValueForVatConvertToLocalCurrencyControl
            // 
            this.ValueForVatConvertToLocalCurrencyControl.AllowDrop = true;
            this.ValueForVatConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_ValueForVat";
            this.ValueForVatConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.ValueForVatConvertToLocalCurrencyControl.BindToUnit = "JI_RX_LocalCurrency";
            this.ValueForVatConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|28987458-C939-4504-9894-1A8D0DE19F18", "VAT Value");
            this.ValueForVatConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 87, true);
            this.ValueForVatConvertToLocalCurrencyControl.Name = "ValueForVatConvertToLocalCurrencyControl";
            this.ValueForVatConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.ValueForVatConvertToLocalCurrencyControl.TabIndex = 2;
            // 
            // InvoiceLineSummaryUserControl
            // 
            this.Controls.Add(this.CustomsValueConvertToLocalCurrencyControl);
            this.Controls.Add(this.GSTVATDeferredConvertToLocalCurrencyControl);
            this.Controls.Add(this.StatisticalValueConvertToLocalCurrencyControl);
            this.Controls.Add(this.ValueForVatConvertToLocalCurrencyControl);
            this.Name = "InvoiceLineSummaryUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 167, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CustomsValueConvertToLocalCurrencyControl.ResumeLayout(true);
            this.CustomsValueConvertToLocalCurrencyControl.PerformLayout();
            this.GSTVATDeferredConvertToLocalCurrencyControl.ResumeLayout(true);
            this.GSTVATDeferredConvertToLocalCurrencyControl.PerformLayout();
            this.StatisticalValueConvertToLocalCurrencyControl.ResumeLayout(true);
            this.StatisticalValueConvertToLocalCurrencyControl.PerformLayout();
            this.ValueForVatConvertToLocalCurrencyControl.ResumeLayout(true);
            this.ValueForVatConvertToLocalCurrencyControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl GSTVATDeferredConvertToLocalCurrencyControl;
        internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl StatisticalValueConvertToLocalCurrencyControl;
        internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl ValueForVatConvertToLocalCurrencyControl;
        internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl CustomsValueConvertToLocalCurrencyControl;
    }
}
