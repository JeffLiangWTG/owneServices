using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public static class ATLASMessageHelper
	{
		public static EDIMessage CreateAnyKnownATLASMessage(BusinessObjectFactory factory)
		{
			var applicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCTSTJ);
			var message = factory.New<AtlasInboundEDIMessage<ICUSTST>>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_ApplicationReference = applicationReference;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			message.EM_MessageNum = MessageNumber;
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.ChangeCustodianEntitledTrader;
			message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DECustomsData>
	<LogbookTime>2019-02-25T16:29:00.2347048+01:00</LogbookTime>
	<CustomsData>
		<{applicationReference}>
			<MetaData>
				<Preparation>
					<Date>2019-02-25</Date>
					<Time>16:00:00</Time>
				</Preparation>
				<InterchangeControlReference>0000000375302</InterchangeControlReference>
				<MessageReferenceNumber>1</MessageReferenceNumber>
				<MessageIdentifier>{MessageNumber}</MessageIdentifier>
				<MessageGroup>SVM</MessageGroup>
				<MessageType>{applicationReference}</MessageType>
				<InterchangeSender>
					<Identification>
						<ReferenceNumber>DE005875</ReferenceNumber>
					</Identification>
				</InterchangeSender>
			</MetaData>
			<Header>
				<ReferenceNumber>ATB150002930220195875</ReferenceNumber>
			</Header>
			<Body />
		</{applicationReference}>
	</CustomsData>
	<AttachedDocumentCollection>
		<AttachedDocument>
			<FileName>SVM-1-DE9000348-0001-DE005866_58660000003234841.pdf</FileName>
			<Type>
				<Code>CAU</Code>
				<Description>Report SCPRLI</Description>
			</Type>
			<ImageData>MkE5UlZFQUMtQVpQMlU0RjItSkhKRVNDUkMtWllSSFpOUEYtTFREODQ2TFYtMzlRRDZGTk0tM1Y3VzZNRDgtUTdVMjk5VTI=</ImageData>
		</AttachedDocument>
	</AttachedDocumentCollection>
</DECustomsData>";
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		public static SendAcknowledgementsRegistryCollection CreateSendAcknowledgementsRegistryCollection(BusinessObjectFactory factory)
		{
			var emailGroup = factory.NewWithValidTestData<GlbGroup>();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@mail.com";
			emailGroup.Staff.Add(staff);
			factory.Save();

			var sendToRegistryCollection = new SendAcknowledgementsRegistryCollection();
			var sendToRegistry = sendToRegistryCollection.AddNew();
			sendToRegistry.EBSCode = "0001";
			sendToRegistry.SendGroupPK = emailGroup.PK;

			return sendToRegistryCollection;
		}

		const string MessageNumber = "CUSTST58750000000375302250219160050";
	}
}
