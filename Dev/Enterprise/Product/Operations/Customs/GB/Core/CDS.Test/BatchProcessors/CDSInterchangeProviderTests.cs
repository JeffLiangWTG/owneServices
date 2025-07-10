using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class CDSInterchangeProviderTests : InterchangeProviderTestCase
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public override void TestMessagesPopulateNewInterchange()
		{
			CDSMessageSenderTestHelper.TestProcess(null, entry =>
			{
				var messages = new NonDependentEDIMessageCollection(entry.Factory);
				messages.AddRange(entry.Messages);
				return ((CDSInterchangeProvider)GetInterchangeProvider(messages)).Interchanges;
			});
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new CDSInterchangeProvider(new LoggingInformation(), collection);
		}

		public void TestNoInterchangeOnFailedMessages()
		{
			AssertNoExceptionThrown(() =>
			{
				var messages = new NonDependentEDIMessageCollection(Factory);
				var provider = GetInterchangeProvider(messages);
				provider.PackCollatedMessagesIntoInterchanges();
				AssertEquals("Provider Interchanges[] should be 0", 0, provider.Interchanges.Length);
			});
		}

		public void TestMessageBody()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message = CreateAndPopulateMessage(entry);
			message.EM_MessageOwner = "B";
			var messageCollection = new NonDependentEDIMessageCollection(Factory)
			{
				message
			};
			var interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			AssertEquals("EI_BodyText should be wrapped by pnt:InventoryMessage, and the pnt namespace prefix is expressly defined (otherwise the upload to eHub can't understand it, and escapes it to &lt;pnt etc, and it arrives into eHub as text not XML, which Pentant rightly refuse)",
							"<pnt:InventoryMessage xmlns:pnt='http://www.myvan.descartes.com/pnt/2018/r1'>FOO~BAR~</pnt:InventoryMessage>", interchangeProvider.Interchanges[0].EI_BodyText);

			message = CreateAndPopulateMessage(entry);
			message.EM_MessageType = string.Empty;
			message.EM_MessageOwner = "B";
			messageCollection = new NonDependentEDIMessageCollection(Factory)
			{
				message
			};
			interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			AssertEquals("EI_BodyText should not be wrapped by pnt:InventoryMessage", "FOO~BAR~", interchangeProvider.Interchanges[0].EI_BodyText);
		}

		public void TestNoInterchangeCreatedForCCSUKMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<CDSInventoryLinkingQueryRequestEDIMessage>();
			message.EM_MessageOwner = "ABC";
			message.EM_MessageNum = "0001";
			entry.Messages.Add(message);
			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			AssertEquals(0, interchangeProvider.Interchanges.Length);
			AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("EDIMessage number 0001 could not be delivered due to a mismatch between its properties and its parent declaration's properties; " +
				"this is most likely due to changing the declaration's profile and gateway after message generation.", GetLastNote(message).ST_NoteDataAsText);
		}

		public void TestLinkedConsolInterchangeHeader()
		{
			MakeCCSUKBadgeAndCredential();
			MawbTestHelper.MakeBadge("DFF", GatewayList.Codes.CDS, "CUKFFW98000", true, "DFF", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", MucrGenerationStyles.Codes.Air);

			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new Chief.ChiefExportConsolIntegration.CustomsExportConsolIntegrationWrapper(consol, null);
			wrapper.MawbExportHelper.ME_Profile = "DFF";
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			var message = Factory.New<CDSInventoryLinkingConsolidationRequestEDIMessage>();
			message.EM_MessageOwner = "DFE";
			message.EM_LinkedObject = consol;
			var messageCollection = new NonDependentEDIMessageCollection(Factory) { message };
			var interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			AssertEquals(1, interchangeProvider.Interchanges.Length);
			AssertEquals("Header should not be empty", "<GBCustomsRequest xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <Provider>Direct</Provider>\r\n  <Service>ExportInventory</Service>\r\n  <Credentials Key=\"EDIDAT.GB12345678.DFF\" />\r\n  <JobNumber />\r\n</GBCustomsRequest>", interchangeProvider.Interchanges[0].EI_HeaderText);

			wrapper.MawbExportHelper.ME_Profile = "DFE";
			message = Factory.New<CDSInventoryLinkingConsolidationRequestEDIMessage>();
			message.EM_MessageOwner = "DFE";
			message.EM_MessageNum = "0001";
			message.EM_LinkedObject = consol;
			messageCollection = new NonDependentEDIMessageCollection(Factory) { message };
			interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			AssertEquals(0, interchangeProvider.Interchanges.Length);
			AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("EDIMessage number 0001 could not be delivered due to a mismatch between its properties and its parent declaration's properties; " +
				"this is most likely due to changing the declaration's profile and gateway after message generation.", GetLastNote(message).ST_NoteDataAsText);
		}

		public void TestLinkedAsycudaBillInterchangeHeader()
		{
			MakeCCSUKBadgeAndCredential();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsProfile = "XXX";
			var bill = header.Bills.AddNew();
			var message = Factory.New<CDSInventoryLinkingConsolidationRequestEDIMessage>();
			message.EM_LinkedObject = bill;
			var credentialsKey = GBExtensions.GetCredentialsKey("", "XXX");
			var messageCollection = new NonDependentEDIMessageCollection(Factory) { message };
			var interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			AssertEquals(1, interchangeProvider.Interchanges.Length);
			AssertEquals("Header should not be empty", $"<GBCustomsRequest xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <Provider>Direct</Provider>\r\n  <Service>ExportInventory</Service>\r\n  <Credentials Key=\"{credentialsKey}\" />\r\n  <JobNumber />\r\n</GBCustomsRequest>", interchangeProvider.Interchanges[0].EI_HeaderText);
		}

		public void TestLinkedEntryInterchangeIsValid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_Gateway = GatewayList.Codes.CDS;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<CDSInventoryLinkingQueryRequestEDIMessage>();
			message.EM_MessageOwner = "ABC";
			entry.Messages.Add(message);
			var messageCollection = new NonDependentEDIMessageCollection(Factory) { message };

			var interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			AssertEquals(1, interchangeProvider.Interchanges.Length);
			AssertEquals(EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestLinkedEntryInterchangeIsInvalid()
		{
			MakeCCSUKBadgeAndCredential();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "DFE";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<CDSInventoryLinkingQueryRequestEDIMessage>();
			message.EM_MessageOwner = "DEF";
			message.EM_MessageNum = "0001";
			entry.Messages.Add(message);
			var messageCollection = new NonDependentEDIMessageCollection(Factory) { message };

			var interchangeProvider = new CDSInterchangeProvider(new LoggingInformation(), messageCollection);
			AssertEquals(0, interchangeProvider.Interchanges.Length);
			AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("EDIMessage number 0001 could not be delivered due to a mismatch between its properties and its parent declaration's properties; " +
				"this is most likely due to changing the declaration's profile and gateway after message generation.", GetLastNote(message).ST_NoteDataAsText);
		}

		EDIMessage CreateAndPopulateMessage(CusEntryHeader header)
		{
			var message = Factory.New<CDSPentantAcaMessage>();
			message.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "FOO~BAR~";
			message.EM_MessageOwner = "ABC";
			message.EM_LinkedObject = header;
			return message;
		}

		void MakeCCSUKBadgeAndCredential()
		{
			var badge = new BadgeCodeSetting();
			badge.BadgeCode = "DFE";
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badge.ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var existingBadges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			existingBadges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, existingBadges);
			Factory.Save();
			var cred = new CredentialsSetting();
			cred.BadgeCode = "DFE";
			var existingCreds = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			existingCreds.Add(cred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, existingCreds);
			Factory.Save();
		}

		StmNote GetLastNote(EDIMessage message) => message.Notes.GetAllNotes().Cast<StmNote>().LastOrDefault();
	}
}
