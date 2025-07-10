using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class CertificateLayoutTemplate : ZUserControl
	{
		public CertificateLayoutTemplate()
		{
			InitializeComponent();
			OtherLawsGrid.MaximumRows = CusOtherLawReferenceCollection<CusOtherLawReference>.MaxRowCount;
			ApprovalCertificateInfoGrid.MaximumRows = ApprovalCertificateInfoCollection.MaxCountForImport;
		}
	}
}
