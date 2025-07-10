using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;

namespace Enterprise.Client.WCB.DaimlerChrysler.GUI
{
	public partial class DCDataImporterForm : DataImporterForm
	{
		protected DCDataImporterForm()
		{
			InitializeComponent();
		}

		public DCDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption)
			: base(businessEntity, formCaption, BillingInterfaceName.ClientSpecifiedImport)
		{
			InitializeComponent();
		}

		public new static DCDataImporterForm Create(BillingInterfaceName interfaceName)
		{
			return new DCDataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null);
		}

		protected override string ImportFileFilter
		{
			get
			{
				string filterClause = ZString.Empty;

				if (((DCFlatFileDataImporter)Importer).IsMercedes)
				{
					string prefix = WCBDataRegistry.Instance.MercedesImportFileNamePrefix;
					filterClause = "(" + prefix + "*.txt)|" + prefix + "*.txt";
				}
				else
				{
					string prefix = WCBDataRegistry.Instance.ChryslerImportFileNamePrefix;
					filterClause = "(" + prefix + "*.txt)|" + prefix + "*.txt";
				}

				return filterClause;
			}
		}
	}
}
