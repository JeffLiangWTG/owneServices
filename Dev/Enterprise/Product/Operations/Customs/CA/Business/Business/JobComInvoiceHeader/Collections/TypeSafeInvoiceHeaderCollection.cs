using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.CA.Business
{
	partial class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection, IImportWizardProvider
	{
		public new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		public new JobComInvoiceHeader this[int index]
		{
			get { return (JobComInvoiceHeader)(base[index]); }
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader newElement, Customs.Business.BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader(newElement, JobDeclaration);
		}

		protected override bool AllowNew
		{
			get
			{
				var declaration = JobDeclaration;
				return base.AllowNew && declaration.CA_LVSCloseDate.IsEmpty
					&& !(declaration.IsConsolidatedLVS && declaration.HasAB3AcceptedOrWaiting) && !declaration.IsIM2;
			}
		}

		ImportWizard IImportWizardProvider.GetImportWizard(IImportCollectionInfo collectionInfo,
			ISettingsStorage settingsStorage, IFileMapper fileMapper)
		{
			return new InvoiceHeaderImportWizard(collectionInfo, settingsStorage, fileMapper);
		}
	}
}
