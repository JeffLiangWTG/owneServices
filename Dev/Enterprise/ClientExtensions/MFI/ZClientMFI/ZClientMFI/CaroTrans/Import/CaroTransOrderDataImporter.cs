using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransOrderDataImporter : FlatFileDataImporter
	{
		public CaroTransOrderDataImporter()
		{
		}

		protected CaroTransOrderDataImporter(BusinessObjectFactory factory)
			: base(new SingleBusinessObjectFactoryProvider(factory))
		{
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new CaroTransOrderFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.Orders();
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			bool result = false;

			Xsd.Orders xsdOrders = xSD as Xsd.Orders;
			if (xsdOrders != null)
			{
				Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
				interchange.InterchangeInfo = new Xsd.InterchangeInfo();
				interchange.InterchangeInfo.EDIOrganisation = new Xsd.Organisation();
				interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "CTI";

				OrderValueObjectDataAdapter<Order, Xsd.Order> adapter = new OrderValueObjectDataAdapter<Order, Xsd.Order>();
				foreach (Xsd.Order xsdOrder in xsdOrders.Order)
				{
					ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, interchange, notifications);
					Order orderBizObj = adapter.CreateOrUpdateFromValueObject(xsdOrder, importContext);
					if (orderBizObj != null)
					{
						if (orderBizObj.BuyerPK.IsValid && orderBizObj.Shipment == null)
						{
							result = true;
						}
						else
						{
							orderBizObj.ClearHasChanges();
						}
					}
				}
			}

			return result;
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CaroTransOrderFlatFileFormat(); }
		}
	}
}
