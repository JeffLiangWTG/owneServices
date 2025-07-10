using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Testing;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	public class CDSTransmissionMessageGeneratorTests : TestCaseWithFactory
	{
		public void TestAfterFullSuccess_UpdateStatus()
		{
			var generator = new CDSTransmissionMessageGenerator(null);
			var entry = Factory.New<CusEntryHeader>();
			generator.AfterFullSuccess(new BuilderResultForTest(entry, Factory.New<CDSNewDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.AwaitingOriginal, entry.CH_Status);

			generator.AfterFullSuccess(new BuilderResultForTest(entry, Factory.New<CDSAmendDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.AwaitingChange, entry.CH_Status);

			generator.AfterFullSuccess(new BuilderResultForTest(entry, Factory.New<CDSCancelDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.AwaitingDelete, entry.CH_Status);

			generator.AfterFullSuccess(new BuilderResultForTest(entry, Factory.New<CDSEDIMessage>()));
			AssertEquals("Status is unchanged", MessageStatusList.Codes.AwaitingDelete, entry.CH_Status);
		}

		public void TestMakePrettyForInterpretation()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entry.LRN = "8GB123456789000-S0001000";

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var messageSendingObjects = new[] { messageSendingObject };

			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message = generator.Generate(entry).Message;
			message.EM_MessageText = message.EM_MessageText.Replace(CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entry.CH_BGMReference); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			message.EM_MessageText = message.EM_MessageText.Replace(CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entry.DeclarationUCR); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			generator.PutReferenceNumberIntoMessageFromPlaceholder(message, message.EM_MessageText, entry);
			var interpretation = generator.MakePrettyForInterpretation(message);
			AssertEquals(GetExpectedH1Interpretation(), interpretation);
		}

		public void TestMessageWorksWithInvalidXmlCharactersInCertainFields()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entry.LRN = "<<>1234>";
			entry.Declaration.JE_DeclarationReference = "&decref";
			entry.Declaration.JE_OwnerRef = "&ownref";
			entry.CH_BGMReference = "5&6&7";

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var messageSendingObjects = new[] { messageSendingObject };

			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var messageBuilder = new DeclarationMessageBuilder(entry.Declaration, generator);
			messageBuilder.PopulateMessages();

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestMakePrettyForInterpretation_Cancellation()
		{
			var entry = Factory.New<CusEntryHeader>();

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			var messageSendingObjects = new[] { messageSendingObject };

			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());

			var message = (CDSCancelDeclarationEDIMessage)entry.Messages.AddNew(typeof(CDSCancelDeclarationEDIMessage));
			message.EM_MessageText = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <WCODataModelVersionCode>3.6</WCODataModelVersionCode>
  <WCOTypeName>DEC</WCOTypeName>
  <ResponsibleCountryCode>GB</ResponsibleCountryCode>
  <ResponsibleAgencyName>HMRC</ResponsibleAgencyName>
  <AgencyAssignedCustomizationVersionCode>v2.1</AgencyAssignedCustomizationVersionCode>
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>13</FunctionCode>
    <FunctionalReferenceID>7-B00001311</FunctionalReferenceID>
    <ID>MRN123</ID>
    <TypeCode>INV</TypeCode>
    <AdditionalInformation>
      <StatementDescription>RRR</StatementDescription>
      <StatementTypeCode>AES</StatementTypeCode>
    </AdditionalInformation>
    <Amendment>
      <ChangeReasonCode>1</ChangeReasonCode>
    </Amendment>
  </Declaration>
</MetaData>";
			message.EM_MessageText = message.EM_MessageText.Replace(CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entry.CH_BGMReference); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			message.EM_MessageText = message.EM_MessageText.Replace(CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entry.DeclarationUCR); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			var interpretation = generator.MakePrettyForInterpretation(message);
			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H1>Request to Cancel</H1><p><strong>MRN: </strong>MRN123<br><strong>Functional Reference ID: </strong>7-B00001311<br><strong>Reason Code: </strong>1 - Cancel - Declaration is no longer required<br><strong>Reason Description: </strong>RRR</p>", interpretation);
		}

		public void TestGenerator_InventoryLinkingMovement()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation;
			var messageSendingObjects = new[] { messageSendingObject };
			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message = generator.Generate(entry).Message;

			message.EM_MessageText = @"<inventoryLinkingMovementRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAA</messageCode>
  <ucrBlock>
    <ucr>8GB945390992000-B00173969</ucr>
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsLocation>GBLHR</goodsLocation>
  <shedOPID>ABD</shedOPID>
  <masterUCR xsi:nil=""true"" />
  <movementReference />
  <transportDetails>
    <transportID>BB123</transportID>
    <transportMode>4</transportMode>
    <transportNationality>US</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>";
			AssertEquals("LMQ", message.EM_MessageType);
			AssertEquals("EAA", message.EM_MessageSubType);
			var interpretation = generator.MakePrettyForInterpretation(message);
			AssertContains("EAA", interpretation);
		}

		[TestDate(2015, 8, 22)]
		public void TestMakePrettyForInterpretation_Amendment()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";
			var addDoc = declaration.PreviousDocuments.AddNew();
			addDoc.CSI_Code = "123";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2015, 9, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 20);
			password.IsTokenForCDS = true;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = cei.PK;
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);
			AssertEquals(1, entry1.Messages.Count);
			AssertEquals("NEW", entry1.Messages[0].EM_MessageType);

			declaration.PreviousDocuments.RemoveAndDeleteAll();
			var invoice = entry1.Declaration.Invoices[0];
			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();

			invoice.JZ_Weight = 100;

			var container3 = entry1.Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CNT789";

			var container4 = entry1.Declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CNT999";

			entry1.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry1.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entry1.CH_EntryStatus = ThreeCharFunctionCodes.DeclarationAccepted;
			Factory.Save();

			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);
			AssertEquals(3, entry1.Messages.Count);
			AssertEquals("NAM", entry1.Messages[1].EM_MessageType);
			AssertEquals("AMD", entry1.Messages[2].EM_MessageType);

			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" + FormattableString.Invariant($"This is a snapshot of the state of your entry at {entry1.Messages[1].EM_SystemCreateTimeUtc} UTC, which was used to create an amendment request.  The details of what was sent to CDS should be viewed on the AMD message."), entry1.Messages[1].EM_MessageInterpretation);
			AssertEquals($$"""<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H1>Request to Amend</H1><p><strong>MRN: </strong>mrn123<br><strong>Functional Reference ID: </strong>{{entry1.LRN}}<br><strong>Reason Code: </strong>20 - Amend commodity code</p><p><H3>Additions</H3><p><strong>WCOID Path: </strong>42A[1]/67A[1]/28A[1]/31B[1]<br><strong>Name Path: </strong>Declaration/GoodsShipment/Consignment/TransportEquipment<br><strong>Value: </strong>SequenceNumeric = 1, ID = CNT789</p><p><strong>WCOID Path: </strong>42A[1]/67A[1]/28A[1]/31B[1]<br><strong>Name Path: </strong>Declaration/GoodsShipment/Consignment/TransportEquipment<br><strong>Value: </strong>SequenceNumeric = 2, ID = CNT999</p><H3>Changes</H3><p><strong>WCOID Path: </strong>42A[1]/67A[1]/28A[1]/096[1]<br><strong>Name Path: </strong>Declaration/GoodsShipment/Consignment/ContainerCode<br><strong>New Value: </strong>1</p><H3>Deletions</H3><p><strong>WCOID Path: </strong>42A[1]/67A[1]/99A[1]<br><strong>Name Path: </strong>Declaration/GoodsShipment/PreviousDocument</p></p>""", entry1.Messages[2].EM_MessageInterpretation);
		}

		static ZString GetExpectedH1Interpretation()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1Interpretation.html").Trim();
		}

		public void TestTypeOfGeneratedMessage_CDSNewDeclarationEDIMessage()
		{
			var entry1 = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var messageSendingObject1 = new JobDeclarationMessageSendingObject(entry1);
			messageSendingObject1.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var messageSendingObjects = new[] { messageSendingObject1 };
			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message1 = generator.Generate(entry1).Message;
			AssertType<CDSNewDeclarationEDIMessage>("If the entry has no MRN, make a NEW message.", message1);
		}

		public void TestTypeOfGeneratedMessage_CDSAmendDeclarationEDIMessage()
		{
			var entry2 = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var messageSendingObject2 = new JobDeclarationMessageSendingObject(entry2);
			messageSendingObject2.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var messageSendingObjects = new[] { messageSendingObject2 };
			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var newMessage = generator.Generate(entry2).Message;
			entry2.Messages.Add(newMessage);
			entry2.MovementReferenceNumberSetter("Mrn123", ZDateTime.BrettsBirthday);
			messageSendingObject2 = new JobDeclarationMessageSendingObject(entry2);
			messageSendingObject2.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			AmendmentMessageHelper.Instance.CanBeAmended(messageSendingObject2);
			messageSendingObjects[0] = messageSendingObject2;
			generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message2 = generator.Generate(entry2).Message;
			AssertType<CDSAmendDeclarationEDIMessage>("If the entry has MRN, is pre lodged but not clear and has Amendment Code, make a AMEND message.", message2);
		}

		public void TestTypeOfGeneratedMessage_CDSCancelDeclarationEDIMessage()
		{
			var entry3 = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var messageSendingObject3 = new JobDeclarationMessageSendingObject(entry3);
			messageSendingObject3.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			var messageSendingObjects = new[] { messageSendingObject3 };
			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message3 = generator.Generate(entry3).Message;
			AssertType<CDSCancelDeclarationEDIMessage>("If the entry has MRN, is pre lodged, clear and has Cancellation Code, make a CANCEL message.", message3);
		}

		public void TestTypeOfGeneratedMessage_CDSNewDeclarationEDIMessage_NotPrelodged()
		{
			var entry4 = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var messageSendingObject4 = new JobDeclarationMessageSendingObject(entry4);
			messageSendingObject4.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var messageSendingObjects = new[] { messageSendingObject4 };
			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			entry4.MovementReferenceNumberSetter("Mrn123", ZDateTime.BrettsBirthday);
			var message4 = generator.Generate(entry4).Message;
			AssertType<CDSNewDeclarationEDIMessage>("if the entry has MRN but is not pre lodged at customs, make a NEW message.", message4);
		}

		public void TestAmendmentComparisonMessageHasCorrectApplicationCode()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entry.Declaration.ZG_Gateway = "CCSUK";

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var messageSendingObjects = new[] { messageSendingObject };

			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());

			var newMessage = generator.Generate(entry).Message;
			entry.Messages.Add(newMessage);
			entry.MovementReferenceNumberSetter("Mrn123", ZDateTime.BrettsBirthday);
			messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			AmendmentMessageHelper.Instance.CanBeAmended(messageSendingObject);
			messageSendingObjects[0] = messageSendingObject;

			AssertEquals("CVC", entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == "NAM")?.EM_ApplicationCode ?? ZString.Empty);
			generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message2 = generator.Generate(entry).Message;
			AssertType<CDSAmendDeclarationEDIMessage>("If the entry has MRN, is pre lodged but not clear and has Amendment Code, make a AMEND message.", message2);
			AssertEquals("CVC", message2.EM_ApplicationCode);
		}

		public void TestMessageOwnerOfGeneratedMessage()
		{
			var entry = CreateCusEntryHeaderForTest(hasMRN: false);
			entry.Declaration.JE_CustomsProfile = "ABC";

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var messageSendingObjects = new[] { messageSendingObject };

			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message = generator.Generate(entry).Message;
			AssertEquals("MessageOwner should be set as CustomsProfile.", "ABC", message.EM_MessageOwner);
		}

		[TestDate(2018, 08, 09)]
		public void TestDucrGoesIntoMessageEvenIfNotSavedBeforeSendingFullEndToEndTest()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory, setDucrExplicitly: false);
			if (!entry.Declaration.CustomsEntryInstructions.Any())
			{
				var cei = entry.Declaration.CustomsEntryInstructions.AddNew();
				cei.CEI_Style = EU.Business.EntryStyleListImport.Codes.ImportNormal;
				cei.CEI_SubStyle = Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.C21GoodsArrived;
			}
			entry.CH_CEI_Instruction = entry.Declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault().PK;
			entry.Declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(MasterFiles.Business.OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", Core.Constants.CountryCodes.UnitedKingdom);
			foreach (JobComInvoiceLine invLine in entry.Declaration.InvoiceLines)
			{
				invLine.AdditionalProcedureCodes.RemoveAndDeleteAll();// invoice line is created when dec's app code is CHF, so it has validation preventing the extra codes, which stops the message being generated
				invLine.JI_Procedure = "4012123"; // Some inv lines lack a CPC
			}
			AssertEquals("PreReq - no DUCR yet", "", entry.CH_BGMReference);
			entry.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			shutUp.AnswerToContinueWithAction = true;

			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var abc = badgeCodeSettings.AddNew();
			abc.BadgeCode = "ABC";
			abc.RL_PortCode = "GBLBA";
			abc.Direction = entry.Declaration.JE_MessageType;
			abc.CSPCode = "CCSUK";
			abc.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk;
			abc.ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings))
			{
				var credential = new CredentialsSetting
				{
					BadgeCode = "ABC",
					Printer = "Location",
					Company = "Role"
				};
				var credentials = GBCustomsDataRegistry.Instance.Credentials.GetValueWithoutFallback(entry.Declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
				credentials.Add(credential);
				using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(entry.Declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, credentials))
				{
					entry.Declaration.JE_CustomsProfile = "ABC";
					var decWrapper = new JobDeclarationMessageSendingObjectParentForTest(entry.Declaration);
					if (!decWrapper.ParentDeclaration.CustomsEntryInstructions.Any())
					{
						decWrapper.ParentDeclaration.CustomsEntryInstructions.AddNew();
					}
					var sender = new CDSMessageSenderForTest(decWrapper);
					sender.Send(shutUp);
				}
			}

			var message = entry.Messages.LastOutgoingMessage;
			Assert(message.IsInDatabase); // saved for us
			AssertEquals("Pre-req - DUCR now set", "8GB123456789000-B00001216", entry.CH_BGMReference);
			AssertContains("<TraderAssignedReferenceID>8GB123456789000-B00001216</TraderAssignedReferenceID>", message.EM_MessageText);
			AssertContains($"<FunctionalReferenceID>{entry.LRN}</FunctionalReferenceID>", message.EM_MessageText);
			AssertContains(@"<PreviousDocument>
        <CategoryCode>Z</CategoryCode>
        <ID>8GB123456789000-B00001216</ID>
        <TypeCode>DCR</TypeCode>
      </PreviousDocument>", message.EM_MessageText);
			AssertNotContains(CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, message.EM_MessageText);
			AssertNotContains(CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, message.EM_MessageText);
		}

		public void TestGenerator_InventoryLinkingMasterQuery()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entry.CH_MasterUCR = "0GB000000001-Z00000001";
			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.MasterQueryDeclaration;
			var messageSendingObjects = new[] { messageSendingObject };
			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			var message = generator.Generate(entry).Message;

			message.EM_MessageText = @"<inventoryLinkingQueryRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <queryUCR>
    <ucr>0GB000000001-Z00000001</ucr>
    <ucrType>M</ucrType>
  </queryUCR>
</inventoryLinkingQueryRequest>";
			AssertEquals("LQM", message.EM_MessageType);

			var expectedInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Master Query Request to CDS:</H3><p><strong>UCR: </strong>0GB000000001-Z00000001<br><strong>UCR Type: </strong>M</p>";
			var interpretation = generator.MakePrettyForInterpretation(message);
			AssertContains(expectedInterpretation, interpretation);
		}

		public void TestReportErrorOnBuilderReturnEmptyMessage()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entry.EntryInstruction.CEI_Style = "B1";

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;

			var generator = new CDSTransmissionMessageGenerator(new[] { messageSendingObject });
			var mockBuilderManager = new MockMessageBuilderManager();
			var builderManagerField = generator.GetType().GetField("<BuilderManager>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
			builderManagerField.SetValue(generator, mockBuilderManager);
			var result = generator.Generate(entry);

			var errorMessage = "Failed to generate EDI message (EntryType: B1 (IM), MessageType: NEW)";
			AssertCollectionContains(errorMessage, result.Errors);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);

			Factory.Save();
			errorMessage = $"Failed to save EM_MessageText (PK: {result.Message.PK}, LinkUniqueID: {result.Message.EM_LinkUniqueID})";
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestErrorOnInvalidEntryType()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entry.EntryInstruction.CEI_Style = "XX";

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;

			var generator = new CDSTransmissionMessageGenerator(new[] { messageSendingObject });
			var result = generator.Generate(entry);

			var errorMessage = "Failed to generate EDI message (EntryType: XX (IM), MessageType: NEW)";
			AssertCollectionNotContains(errorMessage, result.Errors);
			errorMessage = "CW1 does not support building message type XX";
			AssertCollectionContains(errorMessage, result.Errors);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			Factory.Save();
			errorMessage = $"Failed to save EM_MessageText (PK: {result.Message.PK}, LinkUniqueID: {result.Message.EM_LinkUniqueID})";
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGenerateDigitalPrompts()
		{
			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);

			_ = entry.Declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "457394695000", Core.Constants.CountryCodes.UnitedKingdom);
			entry.LRN = "ZDMCVD717014558147638984780";

			var generator = new CDSTransmissionMessageGenerator(new[] { messageSendingObject });
			using var funcs = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true);
			using var attrib = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Universal.Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "16");

			using (GBCustomsDataRegistry.Instance.DigitalPromptsTrialParticipateInNudgeTrial.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("DigitalPromptsTrialParticipateInNudgeTrial", true, GBCustomsDataRegistry.Instance.DigitalPromptsTrialParticipateInNudgeTrial.Value);
				AssertEquals("HmrcDigitalPrompts", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today));
				var warningFactor = EnhancedValidationParticipationHelper.GetWarningFactor();
				AssertEquals("WarningFactor", EnhancedValidationParticipationHelper.WarningFactor.Sixteen, warningFactor);

				var result = generator.Generate(entry);
				Factory.Save();

				var expected = $@"
<!--HMRC Digital Prompts: control-->
<!--Profile: {entry.Declaration.JE_CustomsProfile}-->
<!--Gateway: {entry.Declaration.ZG_Gateway}-->
<!--Declaration Type: {entry.EntryInstruction.CEI_Style}-->
<!--Message Number: {result.Message.EM_MessageNum}-->";
				var messageXML = result.Message.EM_MessageText;
				AssertEndsWith("Message generated with digital prompts control", expected, messageXML);

				_ = entry.Declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "358522637000", Core.Constants.CountryCodes.UnitedKingdom);
				entry.LRN = "ZGBLGK390322513645514837201";

				result = generator.Generate(entry);
				Factory.Save();

				expected = $@"
<!--HMRC Digital Prompts: treatment-->
<!--Profile: {entry.Declaration.JE_CustomsProfile}-->
<!--Gateway: {entry.Declaration.ZG_Gateway}-->
<!--Declaration Type: {entry.EntryInstruction.CEI_Style}-->
<!--Message Number: {result.Message.EM_MessageNum}-->";
				messageXML = result.Message.EM_MessageText;
				AssertEndsWith("Message generated with digital prompts treatment", expected, messageXML);

				messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
				result = generator.Generate(entry);
				Factory.Save();

				messageXML = result.Message.EM_MessageText;
				AssertNotContains("Message generated without digital prompts", "HMRC Digital Prompts", messageXML);
			}

			using (GBCustomsDataRegistry.Instance.DigitalPromptsTrialParticipateInNudgeTrial.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var result = generator.Generate(entry);
				Factory.Save();
				var messageXML = result.Message.EM_MessageText;
				AssertNotContains("Message generated with digital prompts disabled", "HMRC Digital Prompts", messageXML);
			}
		}

		CusEntryHeader CreateCusEntryHeaderForTest(bool hasMRN, string amendmentReason = "", string entryStatus = "")
		{
			var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);
			if (!entry.Declaration.CustomsEntryInstructions.Any())
			{
				var cei = entry.Declaration.CustomsEntryInstructions.AddNew();
			}
			entry.CH_CEI_Instruction = entry.Declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault().PK;
			entry.Declaration.JE_DeclarationType = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			entry.MovementReferenceNumberSetter(hasMRN ? "TESTMRN" : string.Empty, hasMRN ? ZDateTime.BrettsBirthday : ZDateTime.Empty);
			entry.ZG_AmendmentReasonCode = amendmentReason;
			return entry;
		}
	}

	class MockMessageBuilderManager : IMessageBuilderManager
	{
		public IGbCDSMessageBuilder NewMessageBuilder(JobDeclarationMessageSendingObject objectToSend, EU.Business.ErrorCollector errorCollector, Customs.Business.CusdecMessageFunction newAmendDelete)
		{
			var mockBuilder = new Mock<IGbCDSMessageBuilder>();
			mockBuilder.Setup(builder => builder.Build()).Returns(ZString.Empty);
			return mockBuilder.Object;
		}
	}

	class BuilderResultForTest : IBuilderResult
	{
		readonly CusEntryHeader entry;
		readonly EDIMessage message;

		public BuilderResultForTest(CusEntryHeader entry, EDIMessage message)
		{
			this.entry = entry;
			this.message = message;
		}

		public void AfterFullSuccess()
		{
		}

		public EDIMessage Message => message;

		public string[] Errors => null;

		public AfterFullSuccessDelegate AfterFullSuccessDelegate => null;

		public BusinessObject Owner => entry;
	}
}
