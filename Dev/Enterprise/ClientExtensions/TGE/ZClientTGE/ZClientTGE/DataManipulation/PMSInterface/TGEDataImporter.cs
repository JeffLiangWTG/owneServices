using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.TGE.PMS
{
	public class TGEDataImporter : FlatFileDataImporter
	{
		#region Override

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.Consols consols = (Xsd.Consols)xSD;

			Xsd.XmlInterchange interchange = CreateXmlInterchange(notifications);

			foreach (Xsd.Consol consolXSD in consols.Consol)
			{
				NotificationBuffer buffer = new NotificationBuffer(notifications);
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				ValueObjectImportContext importContext = new ValueObjectImportContext(newFactory, interchange, buffer);
				CommonConsol consol = ConsolDataAdapter.CreateOrUpdateFromValueObject(consolXSD, importContext);

				if (buffer.HasErrors)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, "Error Importing Data from line with MAWB " + consol.JK_MasterBillNum + "\n"));
				}
				else
				{
					foreach (CommonShipment shipment in consol.Shipments)
					{
						foreach (CusEntryNumber entry in shipment.CusEntryNumbers)
						{
							entry.CE_EntryIsSystemGenerated = false;
						}
					}

					newFactory.Save();
				}
			}

			return true;
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new TGEFlatFileFormat(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notify)
		{
			return new DataConverter(notify, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.Consols();
		}

		protected override System.Text.Encoding Encoding
		{
			get { return System.Text.Encoding.GetEncoding("windows-1252"); }
		}

		#endregion

		Xsd.XmlInterchange CreateXmlInterchange(INotifications notify)
		{
			Xsd.XmlInterchange result = new Xsd.XmlInterchange();

			OrgHeader pMSOrg = (OrgHeader)FactoryProvider.Current.Load(typeof(OrgHeader), TGEDataRegistry.Instance.CodeMapPMSOrganisation);
			if (pMSOrg != null)
			{
				result.InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(pMSOrg, new ValueObjectExportContext(notify));
			}
			return result;
		}

		#region ConsolDataAdapter

		TGEConsolValueObjectDataAdapter ConsolDataAdapter
		{
			get
			{
				if (fConsolDataAdapter == null)
				{
					fConsolDataAdapter = new TGEConsolValueObjectDataAdapter();
				}
				return fConsolDataAdapter;
			}
		}
		TGEConsolValueObjectDataAdapter fConsolDataAdapter;

		#endregion
	}
}
