using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using EDIMessage = Enterprise.Customs.CA.Business.EDIMessage;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class DuplicateUniversalEventCheckerTest : TestCaseWithFactory
	{
		public void TestGetApplicationReference()
		{
			UniversalEvent xmlEvent = (UniversalEvent)eventDeserializer.Parse(GetEventXmlText());
			var applicationReference = DuplicateUniversalEventChecker.GetApplicationReference(xmlEvent);
			AssertEquals("29-Dec-16 06:3900000000", applicationReference);
		}

		public void TestCheckDuplicateMessages()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message.EM_ApplicationReference = "29-Dec-16 06:3900000000";
			Factory.Save();

			UniversalEvent xmlEvent = (UniversalEvent)eventDeserializer.Parse(GetEventXmlText());
			var applicationReference = DuplicateUniversalEventChecker.GetApplicationReference(xmlEvent);
			var isDuplicated = DuplicateUniversalEventChecker.CheckDuplicateMessages(applicationReference, Factory, new[] { UniversalEventMessageTypes.Codes.IIDResponses, UniversalEventMessageTypes.Codes.D4Notices });
			Assert(isDuplicated);
		}

		ZString GetEventXmlText()
		{
			return @"
				<UniversalEvent>
					<Event>
						<DataContext>
							<DataTargetCollection>
								<DataTarget>
									<Type>CAIntegratedImportDeclaration</Type>
									<Key>10207000013506</Key>
								</DataTarget>
							</DataTargetCollection>
							<RecipientRoleCollection>
								<RecipientRole>
									<Code>CD4</Code>
									<Description>CA Customs IID/D4 Status Notice</Description>
								</RecipientRole>
							</RecipientRoleCollection>
						</DataContext>
						<EventTime>2016-12-29T06:39:10</EventTime>
						<EventType>MAA</EventType>
						<EventReference></EventReference>
						<ContextCollection>
							<Context>
								<Type>IsTest</Type>
								<Value>Y</Value>
							</Context>
							<Context>
								<Type>OrganizationReference</Type>
								<Value>857477707RM0001</Value>
							</Context>
						</ContextCollection>
					</Event>
				</UniversalEvent>";
		}

		protected override void SetUp()
		{
			base.SetUp();
			eventDeserializer = new XmlEventDeserializer();
		}
		XmlEventDeserializer eventDeserializer;
	}
}
