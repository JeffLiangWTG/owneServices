using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Diagnostics
{
	public partial class DocumentSectionsForm : ZChildForm
	{
		public DocumentSectionsForm()
			: base(new DocumentSections(new BusinessObjectFactory()))
		{
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}
	}
}
