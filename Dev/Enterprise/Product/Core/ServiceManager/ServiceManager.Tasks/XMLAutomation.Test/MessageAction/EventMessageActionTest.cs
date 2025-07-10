using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class EventMessageActionTest : ImportMessageActionTest
	{
		public void TestExecuteAction_ImportEvent()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00001223";

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00001224";

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S00001225";

			Factory.Save();

			int noOfEvents = Factory.GetDatabaseCount(typeof(StmALog));

			var buffer = new NotificationBuffer();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageText = messageBody;
			var action = new EventMessageAction(factoryProvider);
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = interchangeHeader;
			interchange.EI_FooterText = interchangeFooter;
			interchange.EI_From = "blah";
			interchange.EI_To = "blah blah";
			message.EM_EI = interchange.PK;
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));
			Factory.Save();
			AssertEquals(noOfEvents + 4, Factory.GetDatabaseCount(typeof(StmALog))); // No longer logging ADD events for EDIMessage and EDIInterchange, hence -2
		}

		#region XML

		const string interchangeHeader = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2011-02-03T12:15:11</Date></InterchangeInfo><Payload><Events>";
		const string interchangeFooter = @"</Events></Payload></XmlInterchange>";
		const string messageBody = @"<Event><Source>NACCS</Source><Code>IES</Code><DateTime>2011-01-02T09:31:00+11:00</DateTime><Information>Remark 1</Information><ReferenceKeys><ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">S00001223</ReferenceKey></ReferenceKeys></Event><Event><Source>NACCS</Source><Code>CLR</Code><DateTime>2010-12-30T10:00:00+11:00</DateTime><Information>JP Permit1</Information><ReferenceKeys><ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">S00001224</ReferenceKey><ReferenceKey ReferenceKeyCountry=""JP"" ReferenceKeyName=""CustomsEntryNumber"" ReferenceKeyType=""PMT"">JP Permit1</ReferenceKey></ReferenceKeys></Event><Event><Source>NACCS</Source><Code>CCC</Code><DateTime>2011-01-03T10:00:00+11:00</DateTime><ReferenceKeys><ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">S00001225</ReferenceKey></ReferenceKeys></Event>";

		#endregion
	}
}
