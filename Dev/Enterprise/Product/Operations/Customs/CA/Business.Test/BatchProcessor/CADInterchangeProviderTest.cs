using System;
using System.Linq;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CADInterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestDate(2021, 10, 08, 12, 6, 25)]
		public override void TestMessagesPopulateNewInterchange()
		{
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");

			var company = Env.CurrentCompany;
			var branch1_1 = company.ActiveBranches.First();
			var branch1_2 = company.ActiveBranches.Last();

			var collection = new NonDependentEDIMessageCollection(Factory);
			var message1 = collection.AddNew();
			message1.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message1.EM_GB = branch1_1.PK;
			message1.EM_MessageText = @"<root>Message 1 text</root>";

			var message2 = collection.AddNew();
			message2.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message2.EM_GB = branch1_1.PK;
			message2.EM_MessageText = @"<root>Message 2 text</root>";

			var message3 = collection.AddNew();
			message3.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message3.EM_GB = branch1_2.PK;
			message3.EM_MessageText = @"<root>Message 3 text</root>";

			var interchanges = new CADInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message3.EM_Status);

			AssertEquals(3, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.CAIMP, interchange.EI_ApplicationCode);
				AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			}
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new CADInterchangeProvider(collection);
		}
	}
}
