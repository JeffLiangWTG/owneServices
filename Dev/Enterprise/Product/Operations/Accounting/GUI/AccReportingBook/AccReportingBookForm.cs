using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting
{
	public partial class AccReportingBookForm : ZTemplateForm
	{
		public AccReportingBookForm(AccReportingBook reportingBook) : base(reportingBook)
		{
		}

		public new AccReportingBook BusinessEntity
		{
			get { return (AccReportingBook)base.BusinessEntity; }
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		protected override bool ShowAuditTab => true;

		protected override void OnShown(System.EventArgs e)
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.AccountingReportingBookFeature);
			
			if (featureData != null && featureData.TryDeserializeParameterAsJson<ReportingBookFeatureControlData>(out var reportingBookFeatureControlData))
			{
				ARB_RX_NKCurrency.Visible = reportingBookFeatureControlData?.EnableCurrencyTranslation ?? false;
			}
		}

		void ReportingBookCodeTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			e.Handled = !char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
		}
	}
}
