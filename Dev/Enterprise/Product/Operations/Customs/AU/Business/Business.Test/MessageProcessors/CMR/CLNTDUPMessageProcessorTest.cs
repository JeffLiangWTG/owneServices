using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CLNTDUPMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestNormalResponse()
		{
			orgWrapper.CLREGInfoProvider.ZA_IsOrg = true;
			AssertEquals("Precondition: no CCID number for the organization", "", organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia));

			var docMessageNumber = AddGenAddOnColumnData();

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGDuplicateMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);

			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("CCID should be added to the organization Customs Codes", "AAA3366939P", organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia));
		}

		public void TestNormalResponseForExistingCCID()
		{
			organization.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			orgWrapper.CLREGInfoProvider.ZA_IsOrg = true;
			AssertEquals("Precondition: CCID exists for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia));

			var docMessageNumber = AddGenAddOnColumnData();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGDuplicateMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);

			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("CCID should be updated for the organization", "AAA3366939P", organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia));
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.CLNTDUP;

		protected override ZString GetExpectedMessageName() => "Client Duplicate Response - (CLNTDUP)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();

			organization = Factory.New<OrgHeader>();
			organization.OH_Code = "TestORG";
			orgWrapper = new OrgHeaderWrapper(organization);
			outgoingMessage = orgWrapper.Messages.AddNew(typeof(CMRCLREGMessage));
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			processor = new TestHelperCLNTDUPMessageProcessor(logger);
		}

		protected override Type IncomingMessageType => typeof(CMRCLNTDUPMessage);

		CLNTDUPMessageProcessor processor;
		OrgHeader organization;
		OrgHeaderWrapper orgWrapper;

		string AddGenAddOnColumnData()
		{
			var docMessageNumber = Guid.NewGuid().ToString("N");
			var addOn = Factory.New<GenAddOnColumn>();
			addOn.XA_ParentID = organization.PK;
			addOn.XA_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			addOn.XA_Name = docMessageNumber;
			addOn.XA_Type = AddOnColumnDataType.Codes.String;
			addOn.XA_Data = docMessageNumber;

			return docMessageNumber;
		}

		sealed class TestHelperCLNTDUPMessageProcessor : CLNTDUPMessageProcessor
		{
			public TestHelperCLNTDUPMessageProcessor(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
