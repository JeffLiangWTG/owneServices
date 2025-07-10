using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI.DataMapping;

namespace Enterprise.CommissionManagement.Module
{
	public partial class CommissionImportWizardForm : MultistepDataImportWizardForm
	{
		public CommissionImportWizardForm(CommissionImportCollectionInfo collectionImportInfo, string contextKey, CommissionFlattenedDataTransferProcessor flattenedProcessor, CommissionSaveProcessor saveProcessor)
			: base(new CommissionImportWizardFactory(), collectionImportInfo, contextKey, new DataTransferProcessor[] { flattenedProcessor, saveProcessor })
		{
			this.collectionImportInfo = collectionImportInfo;

			InitializeComponent();
		}

		new CommissionImportWizard Wizard
		{
			get { return (CommissionImportWizard)base.Wizard; }
		}

		readonly CommissionImportCollectionInfo collectionImportInfo;

		protected override bool DoProcessing()
		{
			collectionImportInfo.ImportType = Wizard.ImportType;

			return base.DoProcessing();
		}
	}
}
