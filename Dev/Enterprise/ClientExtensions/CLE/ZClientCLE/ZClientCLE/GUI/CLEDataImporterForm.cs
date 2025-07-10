using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;

namespace Enterprise.Client.CLE
{
	internal partial class CLEDataImporterForm : DataImporterForm
	{
		internal protected CLEDataImporterForm()
		{
			InitializeComponent();
		}

		public CLEDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption)
			: base(businessEntity, formCaption, BillingInterfaceName.ClientSpecifiedImport)
		{
			InitializeComponent();
		}

		public new static CLEDataImporterForm Create(BillingInterfaceName interfaceName)
		{
			return new CLEDataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null);
		}

		protected override string ImportFileFilter
		{
			get { return "Csv files (*.csv)|*.csv"; }
		}
		internal string InternalImportFileFilter => ImportFileFilter;
	}
}
