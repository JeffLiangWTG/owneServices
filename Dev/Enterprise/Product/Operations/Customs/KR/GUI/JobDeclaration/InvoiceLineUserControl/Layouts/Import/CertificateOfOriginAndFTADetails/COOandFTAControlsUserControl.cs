using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class COOandFTAControlsUserControl : ZUserControl
	{
		public COOandFTAControlsUserControl()
		{
			InitializeComponent();
		}

		public void BindToInvoiceHeader()
		{
			BindingSource.DataSourceType = typeof(JobDeclaration);
			BindingSource.SetBindingMember(GoodsOriginCodeFindBox, "Invoices.JZ_RN_NKDefaultOrigin");
			BindingSource.SetBindingMember(CODeterminationRuleDropEdit, "Invoices.CriteriaForDeterminingCountryOfOrigin");
			BindingSource.SetBindingMember(COLabelLocationDropEdit, "Invoices.JZ_COOLabelLocation");
			BindingSource.SetBindingMember(COLabelTypeDropEdit, "Invoices.JZ_COOLabelType");
			BindingSource.SetBindingMember(COLabelExemptionReasonDropEdit, "Invoices.JZ_COOExemptionReason");

			BindingSource.SetBindingMember(COReferenceNumberTextBox, "Invoices.CertificateOfOriginNo");
			COReferenceNumberTextBox.CaptionResourceString = Res.GetData("69DFDF3E-8331-43BA-B64D-DCF661AEE2CE", "Reference Number");
			BindingSource.SetBindingMember(COIssuingCountryCodeFindBox, "Invoices.CertificateOfOriginIssuingCountry");
			BindingSource.SetBindingMember(COIssueDateEdit, "Invoices.CertificateOfOriginIssueDate");
			BindingSource.SetBindingMember(IssuingAgencyNameTextBox, "Invoices.CertificateOfOriginAgencyName");
			IssuingAgencyNameTextBox.CaptionResourceString = Res.GetData("8CA56FC2-A3D4-434D-B1A0-FEE86AECD902", "Issuing Agency Name");
			BindingSource.SetBindingMember(IssuingAreaNameTextBox, "Invoices.CertificateOfOriginAreaName");
			IssuingAreaNameTextBox.CaptionResourceString = Res.GetData("54BECE7E-953C-4C1E-8501-5087998F0AA0", "Issuing Area Name");
			BindingSource.SetBindingMember(IssuingPersonNameTextBox, "Invoices.CertificateOfOriginPersonName");
			IssuingPersonNameTextBox.CaptionResourceString = Res.GetData("36978D1D-1EBC-4BF7-AC88-C02E9E255BF6", "Issuing Person Name");
			BindingSource.SetBindingMember(COSplitYNDropEdit, "Invoices.CertificateOfOriginStatus");
		}
	}
}
