using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP.Testing;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILConfigurationResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage_Valid()
		{
			var logger = new LoggingInformation();
			var processor = new ILConfigurationResponseMessageProcessor();
			message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.wisetechglobal.com/Schemas/ConfigurationResponse"">
	<Group Type=""System"" Reference=""WTLDPL"">
		<Group Type=""Company"" Reference=""{GlbCompany.CurrentCompany.GC_Code}"" Status=""VAL"">
			<Credential>
				<CredentialPath>123|WTLDPL.CH1</CredentialPath>
			</Credential>
		</Group>
	</Group>
</Configuration>";

			processor.ProcessMessage(message, logger);
			Factory.Save();

			var wrapper = new GlbCompanyWrapper(GlbCompany.CurrentCompany);
			AssertEquals("Password status should be Registered", "Registered", wrapper.GlbExternalPassword.PasswordStatus);
		}

		public void TestProcessMessage_Invalid()
		{
			var logger = new LoggingInformation();
			var processor = new ILConfigurationResponseMessageProcessor();
			message.EM_MessageText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>WTLDPL</SenderID>
    <RecipientID>CustomsConfiguration</RecipientID>
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Event>
        <EventTime>2024-09-25 16:43:13.286</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
          <MessageType>XER</MessageType>
          <Type>PreProcessingError</Type>
          <Reason>Value cannot be null. (Parameter 'UserName')</Reason>
        </EventParameters>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

			processor.ProcessMessage(message, logger);
			Factory.Save();

			var wrapper = new GlbCompanyWrapper(GlbCompany.CurrentCompany);
			AssertEquals("Password status should be Invalid", "Invalid", wrapper.GlbExternalPassword.PasswordStatus);
		}

		public void TestGetLinkedBusinessObjectMetaData_WithLinkedObject_ReturnsFromLinkedObject()
		{
			var processor = new ILConfigurationResponseMessageProcessor();

			var result = processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());
			AssertEquals("LinkTableName should match the company table name", GlbCompany.Schema.TableName, result.ReturnValue.LinkTableName);
			AssertEquals("LinkUniqueId should match the company PK", company.PK, result.ReturnValue.LinkUniqueID);
			AssertEquals("BranchPk should match the message.EM_GB", message.EM_GB, result.ReturnValue.BranchPk);
			AssertEquals("JobNumber should match the company code", company.GC_Code, result.ReturnValue.JobNumber);
		}

		public void TestLinkedObjectNotAssigned()
		{
			var logger = new LoggingInformation();
			var processor = new ILConfigurationResponseMessageProcessor();
			message.EM_LinkedObject = null;
			message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.wisetechglobal.com/Schemas/ConfigurationResponse"">
	<Group Type=""System"" Reference=""WTLDPL"">
		<Group Type=""Company"" Reference=""{GlbCompany.CurrentCompany.GC_Code}"" Status=""VAL"">
			<Credential>
				<CredentialPath>123|WTLDPL.CH1</CredentialPath>
			</Credential>
		</Group>
	</Group>
</Configuration>";

			processor.ProcessMessage(message, logger);
			AssertNull("Linked object should not be assigned", message.EM_LinkedObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var sessionGuid = Guid.NewGuid();
			outgoingInterchange = Factory.New<ILEDIInterchange>();
			var incomingInterchange = Factory.New<ILEDIInterchange>();
			incomingInterchange.EI_SessionGUID = sessionGuid;
			incomingInterchange.EI_ApplicationCode = "CFG";
			incomingInterchange.EI_To = "WTLDCHCTU";
			incomingInterchange.EI_TransportType = ILEDIInterchange.TransportType.xT;
			incomingInterchange.EI_From = "CustomsConfiguration";
			outgoingInterchange.EI_SessionGUID = sessionGuid;
			outgoingInterchange.EI_ApplicationCode = incomingInterchange.EI_ApplicationCode;
			outgoingInterchange.EI_ReceiveTransmit = ILEDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_TransportType = ILEDIInterchange.TransportType.xT;
			outgoingInterchange.EI_From = "WTLDCHCTU";
			outgoingInterchange.EI_To = "CustomsConfiguration";
			outgoingInterchange.EI_InterchangeType = EDIMessage.ApplicationCodes.CHCustomsEdec;
			outgoingInterchange.EI_BodyText = $@"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""CHCustomId"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""WTLXYZ"">
    <Group Type=""Company"" Reference=""{GlbCompany.CurrentCompany.GC_Code}"" Status=""VAL"">
      <Credential>
        <UserName>customsID</UserName>
        <Password />
      </Credential>
    </Group>
  </Group>
</Configuration>";

			message = Factory.New<EDIMessageForTest>();
			message.EM_EI = incomingInterchange.PK;

			company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, GlbCompany.CurrentCompany.GC_Code)) ?? Factory.New<GlbCompany>();
			company.GC_Code = GlbCompany.CurrentCompany.GC_Code;

			message.EM_LinkedObject = company;

			Factory.Save();
		}

		EDIMessage message;
		GlbCompany company;
		ILEDIInterchange outgoingInterchange;
	}
}
