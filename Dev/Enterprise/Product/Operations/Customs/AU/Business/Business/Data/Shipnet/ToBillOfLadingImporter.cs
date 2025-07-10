using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ToBillOfLadingImporter : FlatFileDataImporter, Integration.Customs.AU.IShipnetToBillOfLadingImporter
	{
		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new ToBillOfLadingConverter(notifications, FactoryProvider.Current);
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new ShipnetFlatFileFormat(); }
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.AgencyBillsOfLading();
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.AgencyBillsOfLading valueBills = (Xsd.AgencyBillsOfLading)xSD;
			foreach (Xsd.AgencyBillOfLading valueBill in valueBills.AgencyBillOfLading)
			{
				AgencyShipmentValueObjectDataAdapter<BillOfLading> adaptor = new AgencyShipmentValueObjectDataAdapter<BillOfLading>();
				ValueObjectImportContext context = new ValueObjectImportContext(FactoryProvider, notifications);
				adaptor.CreateOrUpdateFromValueObject(valueBill, context);
			}
			return true;
		}
	}
}
