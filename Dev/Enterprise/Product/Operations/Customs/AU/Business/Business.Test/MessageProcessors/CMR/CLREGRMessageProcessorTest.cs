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
	sealed class CLREGRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestClearResponseNoOutgoingMessageFound()
		{
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: no CCID number for the organization", "", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));

			outgoingMessage.EM_MessageText = ZString.Empty;
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			AssertEquals("Precondition: incomingMessage SendersReference", docMessageNumber, IncomingMessage.SendersReference);
			AssertEquals("Precondition: outgoingMessage BGMReference", ZString.Empty, OutgoingMessage.BGMReference);

			processor.ProcessMessage(incomingMessage);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("CCID Customs Code should be updated for the organization", "AAA3366766M", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
		}

		public void TestClearResponseNoMatchingAddressFound()
		{
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: no CCID number for the organization", "", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			AssertEquals("Precondition: incomingMessage SendersReference", docMessageNumber, IncomingMessage.SendersReference);
			AssertEquals("Precondition: outgoingMessage BGMReference", docMessageNumber, OutgoingMessage.BGMReference);
			AssertEquals("Precondition: incomingMessage BGMReferenceVersion", 1, IncomingMessage.SendersReferenceVersion);
			AssertEquals("Precondition: outgoingMessage BGMReferenceVersion", 1, OutgoingMessage.BGMReferenceVersion);

			processor.ProcessMessage(incomingMessage);
			AssertEquals("Organisation is found but no matching address", organization.PK, IncomingMessage.EM_LinkedObject.PK);

			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("CCID Customs Code should be added to the organization Customs Codes", "AAA3366766M", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
		}

		public void TestClearResponseNoMatchingAddressFound_HasExistingAddressCID()
		{
			var address = organization.Addresses.AddNew();
			address.Address1 = "200 Sydney St";
			address.City = "Alexandria";
			address.Postcode = "2015";
			address.State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			var cusCode = organization.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			cusCode.OK_OA_PremisesAddress = address.PK;
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: no organisation CCID", ZString.Empty, organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
			AssertEquals("Precondition: CCID exists for the address", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, address.PK));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			AssertEquals("Precondition: incomingMessage SendersReference", docMessageNumber, IncomingMessage.SendersReference);
			AssertEquals("Precondition: outgoingMessage BGMReference", docMessageNumber, OutgoingMessage.BGMReference);
			AssertEquals("Precondition: incomingMessage BGMReferenceVersion", 1, IncomingMessage.SendersReferenceVersion);
			AssertEquals("Precondition: outgoingMessage BGMReferenceVersion", 1, OutgoingMessage.BGMReferenceVersion);

			processor.ProcessMessage(incomingMessage);
			AssertEquals("Organisation is found", organization.PK, IncomingMessage.EM_LinkedObject.PK);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("Organisation CCID not created", ZString.Empty, organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
			AssertEquals("Address CCID not updated", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, address.PK));
		}

		public void TestClearResponseNoMatchingAddressFound_HasExistingOrganisationCID()
		{
			organization.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: CCID exists for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			AssertEquals("Precondition: incomingMessage SendersReference", docMessageNumber, IncomingMessage.SendersReference);
			AssertEquals("Precondition: outgoingMessage BGMReference", docMessageNumber, OutgoingMessage.BGMReference);
			AssertEquals("Precondition: incomingMessage BGMReferenceVersion", 1, IncomingMessage.SendersReferenceVersion);
			AssertEquals("Precondition: outgoingMessage BGMReferenceVersion", 1, OutgoingMessage.BGMReferenceVersion);

			processor.ProcessMessage(incomingMessage);
			AssertEquals("Organisation is found", organization.PK, IncomingMessage.EM_LinkedObject.PK);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("CCID should not be updated for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
		}

		public void TestClearResponseWithMatchingAddressAndNoCCID() => CombineAssertions(() =>
		{
			var address = organization.Addresses.AddNew();
			address.Address1 = "456 Stix Street";
			address.City = "Cockle Bay";
			address.Postcode = "2014";
			address.State = "AUK";
			address.OA_RN_NKCountryCode = "NZ";

			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertNull("Precondition: no CCID", organization.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			AssertEquals("Precondition: incomingMessage SendersReference", docMessageNumber, IncomingMessage.SendersReference);
			AssertEquals("Precondition: outgoingMessage BGMReference", docMessageNumber, OutgoingMessage.BGMReference);
			AssertEquals("Precondition: incomingMessage BGMReferenceVersion", 1, IncomingMessage.SendersReferenceVersion);
			AssertEquals("Precondition: outgoingMessage BGMReferenceVersion", 1, OutgoingMessage.BGMReferenceVersion);

			processor.ProcessMessage(incomingMessage);
			AssertEquals("Organisation is found", organization.PK, IncomingMessage.EM_LinkedObject.PK);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);

			AssertEquals("Organisation CCID added", "AAA3366766M", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
			AssertEquals("Address CCID not added", ZString.Empty, organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, address.PK));
		});

		public void TestClearResponseWithMatchingAddressAndNoAddressCCID() => CombineAssertions(() =>
		{
			var address = organization.Addresses.AddNew();
			address.Address1 = "456 Stix Street";
			address.City = "Cockle Bay";
			address.Postcode = "2014";
			address.State = "AUK";
			address.OA_RN_NKCountryCode = "NZ";

			organization.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: CCID exists for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
			AssertEquals("Precondition: no address CCID", ZString.Empty, organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, address.PK));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			AssertEquals("Precondition: incomingMessage SendersReference", docMessageNumber, IncomingMessage.SendersReference);
			AssertEquals("Precondition: outgoingMessage BGMReference", docMessageNumber, OutgoingMessage.BGMReference);
			AssertEquals("Precondition: incomingMessage BGMReferenceVersion", 1, IncomingMessage.SendersReferenceVersion);
			AssertEquals("Precondition: outgoingMessage BGMReferenceVersion", 1, OutgoingMessage.BGMReferenceVersion);

			processor.ProcessMessage(incomingMessage);
			AssertEquals("Organisation is found", organization.PK, IncomingMessage.EM_LinkedObject.PK);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("Organisation CCID remains unchanged", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
			AssertEquals("Address CCID added", "AAA3366766M", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, address.PK));
		});

		public void TestClearResponseWithMatchingAddressAndExistingAddressCCID()
		{
			var address = organization.Addresses.AddNew();
			address.Address1 = "456 Stix Street";
			address.City = "Cockle Bay";
			address.Postcode = "2014";
			address.State = "AUK";
			address.OA_RN_NKCountryCode = "NZ";

			var cusCode = organization.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			cusCode.OK_OA_PremisesAddress = address.PK;
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: no organisation CCID", ZString.Empty, organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
			AssertEquals("Precondition: CCID exists for the address", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, address.PK));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			AssertEquals("Precondition: incomingMessage SendersReference", docMessageNumber, IncomingMessage.SendersReference);
			AssertEquals("Precondition: outgoingMessage BGMReference", docMessageNumber, OutgoingMessage.BGMReference);
			AssertEquals("Precondition: incomingMessage BGMReferenceVersion", 1, IncomingMessage.SendersReferenceVersion);
			AssertEquals("Precondition: outgoingMessage BGMReferenceVersion", 1, OutgoingMessage.BGMReferenceVersion);

			processor.ProcessMessage(incomingMessage);
			AssertEquals("Organisation is found", organization.PK, IncomingMessage.EM_LinkedObject.PK);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("No organisation CCID", ZString.Empty, organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
			AssertEquals("Address CCID updated", "AAA3366766M", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, address.PK));
		}

		public void TestClearResponseForExistingCCID()
		{
			organization.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: CCID exists for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGClearMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Emails Count", 1, processor.ErrorEmailSendCount);
			orgWrapper.Messages.Load();
			AssertEquals("Response Message should be added to the Organisation", 2, orgWrapper.Messages.Count);
			AssertEquals("CCID should not be updated for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
		}

		public void TestErrorResponse()
		{
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGErrorMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
			AssertEquals("Message should be added to the Organisation", 1, orgWrapper.Messages.Count);
		}

		public void TestErrorResponseForExistingCCID()
		{
			organization.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			orgWrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			AssertEquals("Precondition: CCID exists for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGErrorMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
			AssertEquals("Message should be added to the Organisation", 1, orgWrapper.Messages.Count);
			AssertEquals("CCID should not be updated for the organization", "1234567890", organization.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia, ZGuid.Empty));
		}

		public void TestProcessDuplicateRejectedResponse()
		{
			orgWrapper.CLREGInfoProvider.ZA_IsOrg = true;

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CLREGDuplicateRejectedMessage.txt")).Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
			AssertEquals("Message should be added to the Organisation", 1, orgWrapper.Messages.Count);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.CLREG;

		protected override ZString GetExpectedMessageName() => "Client Registration Response (CLREGR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();

			organization = Factory.New<OrgHeader>();
			organization.OH_Code = "TestORG";
			orgWrapper = new OrgHeaderWrapper(organization);
			docMessageNumber = AddGenAddOnColumnData();
			outgoingMessage = orgWrapper.Messages.AddNew(typeof(CMRCLREGMessage));
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+101:::CLREG+" + docMessageNumber + ":1+9'RFF+AQU:IND'FTX+AFM+++MRS:TEST:TEST1:USER:SF'FTX+CNP+++TEST USER:CP'GIS+EVI::95'GIS+EXD::95'CNI+1'RFF+1'LOC+DN+123456'LOC+ISS+SGP'RFF+1'LOC+DN+8546'LOC+ISS+MDA'GID+1'FTX+ATY+BA++456 STIX STREET::COCKLE BAY:2014:AUK+NZ'FTX+ATY+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BP+02+123456789456123:CONTACT PH COMMENT'FTX+CAT+FA+123+456789123:CONTACT FAX COMMENT'FTX+CAT+AP+654+987654321:CONTACT AH COMMENT'FTX+CAT+MO++04122526321:CONTACT MOBILE COMMENT'FTX+CAT+EA++TEST@TEST.COM'MEA+RN+:::EXPORTER'MEA+RN+:::IMPORTER'MEA+RN+:::SEA_CG_RPT'AUT+GE+M'DTM+329:20110101:102'UNT+30+<<MSGNO PLACEHOLDER>>'";
			processor = new TestHelperCLREGRMessageProcessor(logger);
		}

		protected override Type IncomingMessageType => typeof(CMRCLREGRMessage);

		string docMessageNumber;
		CLREGRMessageProcessor processor;
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

		sealed class TestHelperCLREGRMessageProcessor : CLREGRMessageProcessor
		{
			public TestHelperCLREGRMessageProcessor(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}

		CMRCLREGMessage OutgoingMessage => clregOutgoingMessage ?? (clregOutgoingMessage = (CMRCLREGMessage)outgoingMessage);
		CMRCLREGMessage clregOutgoingMessage;

		CMRCLREGRMessage IncomingMessage => clregIncomingMessage ?? (clregIncomingMessage = (CMRCLREGRMessage)incomingMessage);
		CMRCLREGRMessage clregIncomingMessage;
	}
}
