using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.AWB.Testing
{
	[TestedType(typeof(JASShipmentExportAWBRateLineCollection))]
	public class JASShipmentExportAWBRateLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			JASShipmentExportAWBHeader awbHeader = Factory.New<JASShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			return new JASShipmentExportAWBRateLineCollection(awbHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JASShipmentExportAWBRateLine>();
		}
	}
}
