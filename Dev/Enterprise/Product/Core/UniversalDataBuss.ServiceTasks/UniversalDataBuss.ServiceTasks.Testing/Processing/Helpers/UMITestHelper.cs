using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.Scheduler.GraphEngine.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	public static class UMITestHelper
	{
		#region Assertions

		public static void AssertIsProcessed(string message, BusinessObjectFactory factory, params EDIMessage[] messages)
		{
			AssertStatus(message, factory, messages, QueueStatusCodes.Codes.Processed);
		}

		static void AssertStatus(string message, BusinessObjectFactory factory, EDIMessage[] messages, string status)
		{
			GraphEngineTestExtensions.AssertStatusInQueue<EDIMessage>(message ?? string.Format(CultureInfo.InvariantCulture, "Messages should be [{0}]", status), factory, status, EDIMessageSchema.PK, messages.Select(b => b.PK).ToArray());
		}

		public static void AssertNotInQueue(string message, BusinessObjectFactory factory, params EDIMessage[] messages)
		{
			var query = GraphEngineTestExtensions.GetNotInQueueQuery(EDIMessageSchema.PK, messages);
			Assertion.AssertContainsExactElementsInAnyOrder(message ?? "Messages should not be in the queue", messages, factory.Load<EDIMessage>(query));
		}

		#endregion

		#region Gen

		public static Task CompletedTask()
		{
			var source = new TaskCompletionSource<object>();
			source.SetResult(new object());
			return source.Task;
		}

		public static EDIMessage GetMessageRowWithValidMessageContent(BusinessObjectFactory factory)
		{
			return GetMessageRowWithValidMessageContent(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				XmlEDIMessage.Status.Queued);
		}

		public static EDIMessage GetMessageRowWithUniquelyKeyedMessageContent(BusinessObjectFactory factory, int counter)
		{
			var shipment = factory.New<IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_HouseBill = "FRED235478923" + counter;

			var consol = factory.New<IForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			((BusinessObject)consol)[JobConsolSchema.JK_BookingReference] = "20257654321";

			factory.Save();

			return GetMessage(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				XmlEDIMessage.Status.Queued, string.Format(ValidKeyedXmlContent, shipment.JS_HouseBill));
		}

		public static EDIMessage GetMessageRowWithValidMessageContent(BusinessObjectFactory factory, string applicationCode, string messageType, string messageSubType, string status)
		{
			return GetMessage(factory, applicationCode, messageType, messageSubType, status, ValidXmlContent);
		}

		[ThreadSafe]
		static readonly Overridable<int> messageId = new Overridable<int>();

		public static EDIMessage GetMessage(BusinessObjectFactory factory, string applicationCode, string messageType, string messageSubType, string status, string message)
		{
			var message1 = factory.New<EDIMessage>();

			message1.EM_ApplicationCode = applicationCode;
			message1.EM_MessageType = messageType;
			message1.EM_MessageSubType = messageSubType;
			message1.EM_Status = status;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = message;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			lock (messageId)
			{
				messageId.Value += 1;
				message1.EM_MessageNum = messageId.Value.ToString("D20");
			}
			return message1;
		}

		#region XML

		const string ValidXmlContent =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalEvent>
   <Event>
      <EventType>FMA</EventType>
      <EventTime>2015-08-27T04:23:36.3801949</EventTime>
      <EventReference />
      <DataProvider>eHub</DataProvider>
      <ContextCollection>
         <Context>
            <Type>MAWBNumber</Type>
            <Value>235-07943460</Value>
         </Context>
         <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>MIA</Value>
         </Context>
         <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>ANK</Value>
         </Context>
         <Context>
            <Type>OriginalFWBFHLMessage</Type>
            <Value>FWB/16 235-07943460MIAANK/T10K254</Value>
         </Context>
      </ContextCollection>
   </Event>
</UniversalEvent>
";

		const string ValidKeyedXmlContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent Version = ""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Event>
    <DataContext>
      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2011-04-18T14:41:28.79</EventTime>
    <EventType>ATH</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>10137654321</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>{0}</Value>
      </Context>
      <Context>
        <Type>HAWBOriginIATAAirportCode</Type>
        <Value>FRA</Value>
      </Context>
      <Context>
        <Type>HAWBDestinationIATAAirportCode</Type>
        <Value>HKG</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>20257654321</Value>
      </Context>
      <Context>
        <Type>ShippersReference</Type>
        <Value>FUL423189120</Value>
      </Context>
      <Context>
        <Type>InterimReceipt</Type>
        <Value>INTERIM</Value>
      </Context>
      <Context>
        <Type Description=""AMS Number"">AMS</Type>
				<Value>134FREGT</Value>
      </Context>
      <Context>
        <Type Description="""">COC</Type>
				<Value>267AIRGT</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		#endregion

		#endregion
	}
}
