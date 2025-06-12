using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[TestFixture]
	class XHubMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestXMLMessageSuccess_ARL()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(ARLMessgae)))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, ARLSender, eHubSystemID, MessageSchemaType.Xml, ApplicationCode.SYS, "", messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = StreamExtensions.CompressAndEncode(messageStream).ReadToEnd();
				AssertInboxMessage(messagePK, ARLSenderPK, ApplicationCode.SYS, eHubSystemPK, "http://cargowise.com/ehub/clients/arl/2013/07#ARLPurchaseOrder", "", "", content, 0, 2);
				var x = new MemoryStream(Encoding.UTF8.GetBytes(messageStream.ReadToEnd().Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "").Trim()));
				AssertInboxXmlContent(messagePK, ARLMessageTypePK, x.ReadToEnd());
			}
		}

		const string eHubSystemID = "eHubSystem";
		static readonly Guid eHubSystemPK = new Guid("139C9874-CA99-4616-95C6-0440B9DB4C54");
		const string ARLSender = "ENTTSTSVR";
		static readonly Guid ARLSenderPK = new Guid("A0D7B0BE-BC1C-4935-B987-378831EEB0A3");
		static readonly Guid ARLMessageTypePK = new Guid("b5115688-4bb4-451e-809d-ca3fe0662ce8");

		string ARLMessgae = @"<ARLPurchaseOrder xmlns=""http://cargowise.com/ehub/clients/arl/2013/07"">
  <PurchaseOrder>
<CountryCode>20</CountryCode>
	<Priority>4.Sea Freight or Road Freight</Priority>   
<PONumber>PO-0048734</PONumber>
    <PODate>20130818</PODate>
    <SupplierNumber>20-DES01-ZAR</SupplierNumber>
      <SupplierName>Drilling Equipment Services CC</SupplierName>
    <SupplierCurrency>ZAR</SupplierCurrency>
      <ShipToLocation>20LUM</ShipToLocation>
      <ShipToLocationDescription>Zambia - Lumwana</ShipToLocationDescription>
      <StockCode>1-02-02-04-60548-R</StockCode>
      <ItemDescription>AIR END, SECOND STAGE SULLAIR 350/900 (RECONDITIONED)</ItemDescription>
    <QtyOrdered>1.00</QtyOrdered>
    <UnitOfMeasurement>Ea</UnitOfMeasurement>
      <UnitCost>189,980.25</UnitCost>
    <ExtendedCost>189,980.25</ExtendedCost>
    <ExpectedDeliveryDate>20130920</ExpectedDeliveryDate>
    <ISPM15Requirement>No</ISPM15Requirement>
      <IsCompleted>No</IsCompleted>
      <QtyCancelled>0.00</QtyCancelled>
    <QtyOS>1.00</QtyOS>
    <OrderLineNo>1,000.00</OrderLineNo>
    <Incoterm>FCA</Incoterm>
   </PurchaseOrder>
</ARLPurchaseOrder>";
	}
}
