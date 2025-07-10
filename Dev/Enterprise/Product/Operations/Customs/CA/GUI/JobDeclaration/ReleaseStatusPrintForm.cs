using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ReleaseStatusPrintForm : ZChildForm
	{
		public ReleaseStatusPrintForm(JobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
		}
	}
}
