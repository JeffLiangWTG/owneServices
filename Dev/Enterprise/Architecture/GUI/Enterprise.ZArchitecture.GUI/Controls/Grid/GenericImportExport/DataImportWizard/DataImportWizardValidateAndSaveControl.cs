using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	partial class DataImportWizardValidateAndSaveControl : ZUserControl
	{
		public DataImportWizardValidateAndSaveControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var wizard = (ImportWizard)dataSource;
			if (wizard != null)
			{
				base.SetDataBinding(((ImportWizard)dataSource).CollectionInfo.Collection, "");
			}
		}
	}
}
