using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	sealed class PBNInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType("IESUBPCPBT", "TEST DESCRIPTION", "TEST LONG DESCRIPTION");
			helper.CreateRefSysConfig(
				"IESUBPCPBT",
				"https://www.ros.ie/customs/webservice/v1/rest/roro-control/pbn",
				ZDateTime.BrettsBirthday,
				ZDateTime.Empty
				);
			Factory.Save();

			var messages = new NonDependentEDIMessageCollection(Factory);

			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsPBN;
			message1.EM_MessageType = "CPB";
			message1.EM_ApplicationReference = "AA11GH99";
			messages.Add(message1);

			var interchangeProvider = new PBNInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("There should be one interchange", 1, messages.Select(x => x.Interchange).Distinct().Count());

			AssertMessageAndInterchange(
				message1,
				interchangeType: "CPB",
				headerText: "{\"custom.IE.Endpoint\":\"https://www.ros.ie/customs/webservice/v1/rest/roro-control/pbn\",\"custom.IE.SigningOption\":\"Rest\"}"
			);

			void AssertMessageAndInterchange(EDIMessage message, string interchangeType, string headerText)
			{
				var interchange = message.Interchange;
				CombineAssertions("", () =>
				{
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
					AssertEquals("EI_InterchangeType", interchangeType, interchange.EI_InterchangeType);
					AssertEquals("EI_HeaderText", headerText, interchange.EI_HeaderText);
					AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.IECustomsPBN, interchange.EI_ApplicationCode);
					AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
					AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
					AssertEquals("EI_SessionGUID", true, interchange.EI_SessionGUID.IsValid);
					AssertEquals("EI_Priority", EDIInterchangePriorityList.Codes.High, interchange.EI_Priority);
					AssertEquals("EI_To", "IECustomsTest", interchange.EI_To);
					AssertEquals("EI_From", interchange.Company.LicenceKeyIdentifier, interchange.EI_From);
					AssertEquals("EI_GB", message.EM_GB, interchange.EI_GB);
					AssertEquals("EI_GP", message.EM_GP, interchange.EI_GP);
				});
			}
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new PBNInterchangeProvider(collection);
	}
}
