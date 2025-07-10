using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using EDIMessage = Enterprise.Customs.CA.Business.EDIMessage;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class IntegratedImportDeclarationEventParentFinderTest : TestCaseWithFactory
	{
		public void TestCusEntryHeaderByNumberQuery()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader.CH_BGMReference = "12345000000011";
			Factory.Save();

			var eventXmlText = GetEventXmlText("12345000000011");
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusEntryHeader;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "12345000000011", relatedObj.CH_BGMReference);

			eventXmlText = GetEventXmlText("12345000000011", "CustomsDeclaration");
			xmlEvent = eventDeserializer.Parse(eventXmlText);
			logParents = finder.GetLogParentsForEvent(xmlEvent);
			Assert(logParents == null || logParents.Length == 0);
		}

		public void TestThrowDuplicateMessageException()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message.EM_ApplicationReference = "00000000";
			Factory.Save();

			var eventXmlText = GetEventXmlText("ccn88884444");
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			AssertExceptionThrown<DuplicateMessageException>(() => finder.GetLogParentsForEvent(xmlEvent));
		}

		protected override void SetUp()
		{
			base.SetUp();
			eventDeserializer = new XmlEventDeserializer();
			finder = new IntegratedImportDeclarationEventParentFinder(Factory, new IntegratedImportDeclarationDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));
		}
		IntegratedImportDeclarationEventParentFinder finder;
		XmlEventDeserializer eventDeserializer;

		ZString GetEventXmlText(ZString key, string dataTargetType = "CAIntegratedImportDeclaration")
		{
			return ZString.Format(@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>{0}</Type>
					<Key>{1}</Key>
				</DataTarget>
			</DataTargetCollection>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>CD4</Code>
					<Description>CA Customs IID/D4 Status Notice</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
	</Event>
</UniversalEvent>
", dataTargetType, key);
		}
	}
}
