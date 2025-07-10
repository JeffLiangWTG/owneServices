using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using ShipmentRec = Enterprise.Client.MFI.CaroTrans.Constants.ShipmentRecord;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransShipmentConverterTest : TestCaseWithFactory
	{
		public void TestMapExport()
		{
			CommonShipment shipment = CreateShipmentAndRelatedObjects();
			CommonConsol consol = shipment.Consols[0];
			JobDeclaration jobDec = (JobDeclaration)shipment.Declarations[0];
			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			xsdShipment.ShipmentDetails.AgentReference = shipment.JS_UniqueConsignRef;
			xsdShipment.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();
			Xsd.ShipmentIdentifier xsdIdentifier = xsdShipment.ShipmentIdentifier.AddNew();
			xsdIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			xsdIdentifier.Value = "333";
			xsdShipment.ShipmentDetails.Deliver.GoodsDelivered = new ZDateTime(2005, 11, 28);
			MockCaroTransShipmentConverter converter = new MockCaroTransShipmentConverter(new NotificationBuffer(), Factory);
			FlatFileDataRowCollection exportedShipments = converter.MapExport(xsdShipment);
			AssertEquals("ExportedShipments.Count", 1, exportedShipments.Count);
			FlatFileDataRow exportedShipment = exportedShipments[0];
			AssertEquals("BLNumber", "333", exportedShipment[ShipmentRec.BLNumber]);
			AssertEquals("AgentReference", consol.JK_UniqueConsignRef, exportedShipment[ShipmentRec.AgentReference]);
			AssertEquals("Vessel", "Vessel", exportedShipment[ShipmentRec.Vessel]);
			AssertEquals("Voyage", "Voyage", exportedShipment[ShipmentRec.Voyage]);
			AssertEquals("Container", "222", exportedShipment[ShipmentRec.Container]);
			AssertEquals("ArrivalDate", "20051125", exportedShipment[ShipmentRec.ArrivalDate]);
			AssertEquals("AvailableDate", "20051126", exportedShipment[ShipmentRec.AvailableDate]);
			AssertEquals("DeliveryPickupDate", "20051128", exportedShipment[ShipmentRec.DeliveryPickupDate]);
			AssertEquals("CustomsClearanceDate", "", exportedShipment[ShipmentRec.CustomsClearanceDate]);
			consol.Containers[0].JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			jobDec.Logs.AddNew(Events.CustomsEntryStatus, CustomsEntryStatus.ClearCreate.Code, new ZDateTimeOffset(2005, 11, 15));
			jobDec.Logs.AddNew(Events.CustomsEntryStatus, CustomsEntryStatus.ClearCreate.Code, new ZDateTimeOffset(2005, 11, 20));
			Factory.Save();
			Xsd.Event xsdEvent = xsdShipment.Events.Event.AddNew();
			xsdEvent.Code = Events.DeliveryOrderHandedOver.Code;
			xsdEvent.DateTime = new ZDateTime(2005, 11, 29);
			exportedShipments = converter.MapExport(xsdShipment);
			AssertEquals("ExportedShipments.Count", 1, exportedShipments.Count);
			exportedShipment = exportedShipments[0];
			AssertEquals("AvailableDate", "20051127", exportedShipment[ShipmentRec.AvailableDate]);
			AssertEquals("DeliveryPickupDate", "20051129", exportedShipment[ShipmentRec.DeliveryPickupDate]);
			AssertEquals("CustomsClearanceDate", "20051115", exportedShipment[ShipmentRec.CustomsClearanceDate]);
		}

		#region Implementation
		CommonShipment CreateShipmentAndRelatedObjects()
		{
			CommonShipment result = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol = result.Consols.AddNew();
			consol.FillWithValidTestData();
			Transport transport = consol.Transports[0];
			transport.JW_Vessel = "Vessel";
			transport.JW_VoyageFlight = "Voyage";
			transport.JW_ATA = new ZDateTime(2005, 11, 25);
			CommonContainer container = consol.Containers.AddNew();
			container.FillWithValidTestData();
			container.JC_ContainerNum = "222";
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_LCLAvailable = new ZDateTime(2005, 11, 26);
			container.JC_FCLAvailable = new ZDateTime(2005, 11, 27);
			JobDeclaration jobDec = Factory.NewWithValidTestData<JobDeclaration>();
			jobDec.JE_JS = result.PK;
			Factory.Save();
			return result;
		}

		class MockCaroTransShipmentConverter : CaroTransShipmentConverter
		{
			public MockCaroTransShipmentConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
		#endregion
	}
}
