using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;

namespace Enterprise.Client.MFI.GUI
{
	public partial class MFIDataImporterForm : DataImporterForm
	{
		internal protected MFIDataImporterForm()
		{
			InitializeComponent();
		}

		public MFIDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption)
			: base(businessEntity, formCaption, BillingInterfaceName.ClientSpecifiedImport)
		{
			InitializeComponent();
		}

		public static new MFIDataImporterForm Create(BillingInterfaceName interfaceName)
		{
			return new MFIDataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null);
		}

		protected override string ImportFileFilter
		{
			get { return "All (*.*)|*.*"; }
		}

		#region Test internal variables
		internal string InternalImportFileFilterTest => ImportFileFilter;
		#endregion
	}
}
