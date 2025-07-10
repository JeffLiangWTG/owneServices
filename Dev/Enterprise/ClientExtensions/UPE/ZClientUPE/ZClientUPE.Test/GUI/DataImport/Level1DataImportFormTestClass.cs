using System.Windows.Forms;
using Enterprise.Client.UPE.Business.DataImport;

namespace Enterprise.Client.UPE.GUI.DataImport
{
	abstract internal class Level1DataImportFormTestClass : Level1DataImportForm
	{
		public Level1DataImportFormTestClass(Level1DataImport businessEntity)
			: base(businessEntity)
		{
		}

		public new Level1DataImportDetailControl detailControl
		{
			get { return base.detailControl; }
			set { base.detailControl = value; }
		}

		public new Level1DataImportDetailForManifestControl detailForManifestControl
		{
			get { return base.detailForManifestControl; }
			set { base.detailForManifestControl = value; }
		}

		public new Level1DataImportReasonsControl reasonsControl
		{
			get { return base.reasonsControl; }
			set { base.reasonsControl = value; }
		}

		public new Level1DataImportReasonsForManifestControl reasonsForManifestControl
		{
			get { return base.reasonsForManifestControl; }
			set { base.reasonsForManifestControl = value; }
		}

		public new Level1DataFileImporterCore DataImporter
		{
			get { return base.DataImporter; }
			set { base.DataImporter = value; }
		}

		protected override DialogResult OpenFileDialogResult
		{
			get
			{
				base.Dialog.FileName = Level1FileName;
				return DialogResult.OK;
			}
		}

		public abstract string Level1FileName { get; set; }
	}
}
