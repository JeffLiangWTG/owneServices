using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnHeaderDataObjectWriterTest : DataTransfer.Universal.Outturn.Testing.OutturnDataObjectWriterTestHelper
	{
		public void TestSubShipmentIsPopulateCorrectly()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_SendersMessageReference = "O00000340";

			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_HouseBill = "HB1";

			var creator = new CFSShipmentCreator(outturn, Factory.BOFactory);
			var shipment = creator.Shipment;
			shipment.JS_UniqueConsignRef = "JS123";

			var writer = new CusOutturnHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, outturnHeader)));
			var outturnHeaderData = writer.GetDataObject(outturnHeader);

			AssertEquals("SubShipment Count", 1, outturnHeaderData.SubShipmentCollection.Count);
			AssertEquals("SubShipment WayBillNumber", "HB1", outturnHeaderData.SubShipmentCollection[0].WayBillNumber);

			var addinfoJS = outturnHeaderData.SubShipmentCollection[0].AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Outturn.Constants.AddInfoType.ShipmentOrContainerNumber);
			AssertEquals("SubShipment ShipmentOrContainerNumber", "JS123", addinfoJS.Value);
		}
	}
}
