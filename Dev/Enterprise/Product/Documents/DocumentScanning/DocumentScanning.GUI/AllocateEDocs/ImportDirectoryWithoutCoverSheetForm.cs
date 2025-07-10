using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ImportDirectoryWithoutCoverSheetForm : ImportDirectoryForm
	{
		public ImportDirectoryWithoutCoverSheetForm()
			: base()
		{
			InitializeComponent();
		}

		public ImportDirectoryWithoutCoverSheetForm(FileImporter importer)
			: base(importer)
		{
			InitializeComponent();
		}

		protected override bool ValidateForm()
		{
			if (string.IsNullOrEmpty(JobTypeDropDownEdit.Text) || string.IsNullOrEmpty(DocTypeDropDownEdit.Text))
			{
				Globals.Message.Show(Res.GetString("6D325C6D-6F86-4B1E-98BB-9EA50057B9E0", "Job type or/and Document type is not specified. {0} will use type from barcode if it exists.", BrandingFactory.Instance.ProductName),
					Res.GetString("4EFCB906-1891-4657-8B87-52B350F6BC8A", "Import Document"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			return base.ValidateForm();
		}
	}
}
