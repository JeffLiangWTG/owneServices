using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.WCB.GUI
{
	/// <summary>
	/// WCBDataImportForm needs to provide a selection option to distinguish the invoice format being imported
	/// </summary>
	public partial class WCBDataImporterForm : DataImporterForm
	{
		protected WCBDataImporterForm()
		{
			InitializeComponent();
		}

		public WCBDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption)
			: base(businessEntity, formCaption, BillingInterfaceName.ClientSpecifiedImport)
		{
			InitializeComponent();
		}

		public new static WCBDataImporterForm Create(BillingInterfaceName interfaceName)
		{
			return new WCBDataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null);
		}

		protected override void ImportFromFile(ZString fileName)
		{
			FileFormat format = GetFileFormat();
			if (format != FileFormat.Unknown)
			{
				((WCBDataImporter)Importer).SetFileFormat(format);
				base.ImportFromFile(fileName);
			}
			else
			{
				Globals.Message.ShowError("Please indicate which format of Invoice Data is being imported.");
			}
		}

		FileFormat GetFileFormat()
		{
			FileFormat result = FileFormat.Unknown;
			if (DaimlerFormatRadioButton.Checked)
			{
				result = FileFormat.Mercedes;
			}
			else if (FreightlinerFormatRadioButton.Checked)
			{
				result = FileFormat.Freightliner;
			}

			return result;
		}
	}
}
