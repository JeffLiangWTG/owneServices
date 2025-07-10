using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	sealed class CustomsAndExciseReportInterchangeProviderTest : InterchangeProviderTestCase
	{
		public void TestGetEndPoint()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message.MessageDate = new ZDateTime(2024, 9, 1);
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.PSR, "/transactions/periods/20240901/payer-summary-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.PCT, "/transactions/periods/20240901/payer-combined-taxes-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.PTT, "/transactions/periods/20240901/payer-tax-types-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.PCI, "/transactions/periods/20240901/importer-combined-taxes-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.DSR, "/transactions/daily/20240901/payer-summary-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.DCT, "/transactions/daily/20240901/payer-combined-taxes-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.DTT, "/transactions/daily/20240901/payer-tax-types-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.UDR, "/transactions/payer-unpaids-report");
			AssertGetEndPoint(CustomsAndExciseReportTypeList.Codes.BAL, "/transactions/balance");
			AssertGetEndPoint("XXX", "20240901");

			void AssertGetEndPoint(string messageType, string expectedEndPoint)
			{
				message.EM_MessageType = messageType;
				AssertEquals(messageType, expectedEndPoint, CustomsAndExciseReportInterchangeProvider.GetEndPoint(message));
			}
		}

		public void TestGetWebServiceEndPoint()
		{
			var endPoint = "/transactions/balance";
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			AssertEquals("When IsProductionSystem is true", "https://www.ros.ie/customs/webservice/v1/rest/transactions/balance", CustomsAndExciseReportInterchangeProvider.GetWebServiceEndPoint(endPoint));

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			AssertEquals("When IsProductionSystem is false", "https://softwaretestnextversion.ros.ie/customs/webservice/v1/rest/transactions/balance", CustomsAndExciseReportInterchangeProvider.GetWebServiceEndPoint(endPoint));
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);

			var message1 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message1.EM_MessageType = CustomsAndExciseReportTypeList.Codes.DSR;
			message1.MessageDate = new ZDateTime(2024, 09, 06);
			messages.Add(message1);

			var message2 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message2.EM_MessageType = CustomsAndExciseReportTypeList.Codes.DSR;
			message2.MessageDate = new ZDateTime(2024, 09, 06);
			messages.Add(message2);

			var message3 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message3.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PCI;
			message3.MessageDate = new ZDateTime(2024, 09, 06);
			messages.Add(message3);

			var message4 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message4.EM_MessageType = CustomsAndExciseReportTypeList.Codes.DSR;
			message4.MessageDate = new ZDateTime(2024, 10, 09);
			messages.Add(message4);

			var interchangeProvider = new CustomsAndExciseReportInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();

			AssertSame("The two messages with the same EM_MessageType and MessageDate should share the same interchange", message1.Interchange, message2.Interchange);
			AssertEquals("There should be three interchanges", 3, messages.Select(x => x.Interchange).Distinct().Count());

			AssertMessageAndInterchange(message1, interchangeType: CustomsAndExciseReportTypeList.Codes.DSR, headerText: GetHeaderText("daily/20240906/payer-summary-report"));
			AssertMessageAndInterchange(message2, interchangeType: CustomsAndExciseReportTypeList.Codes.DSR, headerText: GetHeaderText("daily/20240906/payer-summary-report"));
			AssertMessageAndInterchange(message3, interchangeType: CustomsAndExciseReportTypeList.Codes.PCI, headerText: GetHeaderText("periods/20240901/importer-combined-taxes-report"));
			AssertMessageAndInterchange(message4, interchangeType: CustomsAndExciseReportTypeList.Codes.DSR, headerText: GetHeaderText("daily/20241009/payer-summary-report"));

			void AssertMessageAndInterchange(CustomsAndExciseReportOutboundMessage message, string interchangeType, string headerText)
			{
				var interchange = message.Interchange;
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("EI_InterchangeType", interchangeType, interchange.EI_InterchangeType);
				AssertEquals("EI_HeaderText", headerText, interchange.EI_HeaderText);
				AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.IECustomsAndExcise, interchange.EI_ApplicationCode);
				AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
				AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
				AssertEquals("EI_SessionGUID", true, interchange.EI_SessionGUID.IsValid);
				AssertEquals("EI_Priority", EDIInterchangePriorityList.Codes.High, interchange.EI_Priority);
				AssertEquals("EI_To(GetToRecipient)", "IECustomsTest", interchange.EI_To);
				AssertEquals("EI_From", interchange.Company.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_GB", message.EM_GB, interchange.EI_GB);
				AssertEquals("EI_GP", message.EM_GP, interchange.EI_GP);
			}

			string GetHeaderText(string expectedEndPoint) => $"{{\"custom.IE.Endpoint\":\"https://softwaretestnextversion.ros.ie/customs/webservice/v1/rest/transactions/{expectedEndPoint}\",\"custom.IE.SigningOption\":\"Rest\"}}";
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new CustomsAndExciseReportInterchangeProvider(collection);
	}
}
