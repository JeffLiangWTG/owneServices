using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaGInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestDOAResponse()
		{
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "EASYLOG2TEST_EAD";
			intchg.EI_To = "HYEDFRCMT";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg.EI_InterchangeNum = "238";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = @"<Message><APPLUS><Enveloppe_APPLUS><JobNumber>BH_JOBREFERENCE</JobNumber><TYPE>DOA</TYPE><STATUT>V</STATUT><AP_DATE>20220218</AP_DATE><AP_HEURE>0806</AP_HEURE><REF_TRC>171614308</REF_TRC></Enveloppe_APPLUS></APPLUS></Message>";
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			AssertEquals(1, msg.Length);

			AssertEquals(1, intchg.ContainedMessages.Count);
			AssertEquals(msg[0].PK, intchg.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg.PK, msg[0].EM_EI);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.FRPortMessage, msg[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.POR, msg[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.DOA, msg[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "238", msg[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, msg[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", intchg.EI_BodyText, msg[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg[0].EM_LinkUniqueID);
		}

		public void TestDeltaTMessages()
		{
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "EASYLOG2TEST_EAD";
			intchg.EI_To = "HYEDFRCMT";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg.EI_InterchangeNum = "238";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = "<Message><EnveloppeMessage><schemaID>DELTAT_MESSAGE_CC016A</schemaID><schemaVersion>1.0</schemaVersion><partyId>EASYLOG_1</partyId><transactionId>HYEDFRCMT+DT_DEC+00050283</transactionId><numseq>0</numseq></EnveloppeMessage><ReponseDeclaration><CC016A><SynIdeMES1>UNOC</SynIdeMES1><SynVerNumMES2>3</SynVerNumMES2><MesSenMES3>NTA.FR</MesSenMES3><MesRecMES6>OPE.FR</MesRecMES6><DatOfPreMES9>200915</DatOfPreMES9><TimOfPreMES10>0852</TimOfPreMES10><IntConRefMES11>eyGiPZTQgjiid7</IntConRefMES11><TesIndMES18>1</TesIndMES18><MesIdeMES19>NCT00050283</MesIdeMES19><MesTypMES20>CC016A</MesTypMES20><HEAHEA><RefNumHEA4>NCT00050283</RefNumHEA4><TypOfDecHEA24>T1</TypOfDecHEA24><DecAntHEA1002>0</DecAntHEA1002><DecRejDatHEA159>20200915</DecRejDatHEA159><DecRejReaHEA252LNG>FR</DecRejReaHEA252LNG></HEAHEA><FUNERRER1><ErrTypER11>15</ErrTypER11><ErrPoiER12>HEA.Numero agrement</ErrPoiER12><ErrReaER13>NAT070</ErrReaER13><OriAttValER14>00000004</OriAttValER14></FUNERRER1></CC016A></ReponseDeclaration></Message>";
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			AssertEquals(1, msg.Length);

			AssertEquals(1, intchg.ContainedMessages.Count);
			AssertEquals(msg[0].PK, intchg.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg.PK, msg[0].EM_EI);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.EuNcts, msg[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", Core.Constants.CountryCodes.France, msg[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", "XXX", msg[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "238", msg[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, msg[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", intchg.EI_BodyText, msg[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg[0].EM_LinkUniqueID);
		}

		public void TestEDIMessageCreatedCorrectly()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "USC";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = Factory.New<GlbBranch>();
			usBranch.GB_Code = "USB";
			usBranch.GB_GC = usCompany.PK;

			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "FRCUS";
			intchg.EI_To = "TEST";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg.EI_InterchangeNum = "0000000000000000001";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = getMessage();
			Factory.Save();

			var intchg1 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg1.EI_From = "FRCUS";
			intchg1.EI_To = "TEST";
			intchg1.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg1.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg1.EI_InterchangeNum = "0000000000000000002";
			intchg1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg1.EI_Status = EDIInterchange.Status.Queued;
			intchg1.EI_IsActive = true;
			intchg1.EI_GB = usBranch.PK;
			intchg1.EI_BodyText = "<test>US Branch Wont be processed</test>";
			Factory.Save();

			var intchg2 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg2.EI_From = "FRCUS";
			intchg2.EI_To = "TEST";
			intchg2.EI_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			intchg2.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg2.EI_InterchangeNum = "0000000000000000003";
			intchg2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg2.EI_Status = EDIInterchange.Status.Queued;
			intchg2.EI_IsActive = true;
			intchg2.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg2.EI_BodyText = "<test>EI_ApplicationCode FRC Wont be processed</test>";
			Factory.Save();

			var intchg3 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg3.EI_From = "FRCUS";
			intchg3.EI_To = "TEST";
			intchg3.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg3.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAtlasSystem;
			intchg3.EI_InterchangeNum = "0000000000000000004";
			intchg3.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg3.EI_Status = EDIInterchange.Status.Queued;
			intchg3.EI_IsActive = true;
			intchg3.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg3.EI_BodyText = "<test>EI_InterchangeType ATS Wont be processed</test>";
			Factory.Save();

			var intchg4 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg4.EI_From = "FRCUS";
			intchg4.EI_To = "TEST";
			intchg4.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg4.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg4.EI_InterchangeNum = "0000000000000000005";
			intchg4.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			intchg4.EI_Status = EDIInterchange.Status.Queued;
			intchg4.EI_IsActive = true;
			intchg4.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg4.EI_BodyText = "<test>EI_ReceiveTransmit Transmit Wont be processed</test>";
			Factory.Save();

			var intchg5 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg5.EI_From = "FRCUS";
			intchg5.EI_To = "TEST";
			intchg5.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg5.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg5.EI_InterchangeNum = "0000000000000000006";
			intchg5.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg5.EI_Status = EDIInterchange.Status.Received;
			intchg5.EI_IsActive = true;
			intchg5.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg5.EI_BodyText = "<test>EI_Status Received Wont be processed</test>";
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();
			intchg1.Reload();
			intchg2.Reload();
			intchg3.Reload();
			intchg4.Reload();
			intchg5.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg1.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg2.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg3.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg4.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg5.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			var msg2 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg2.PK));
			var msg3 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg3.PK));
			var msg4 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg4.PK));
			var msg5 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg5.PK));
			AssertEquals(1, msg.Length);
			AssertEquals(0, msg1.Length);
			AssertEquals(0, msg2.Length);
			AssertEquals(0, msg3.Length);
			AssertEquals(0, msg4.Length);
			AssertEquals(0, msg5.Length);

			AssertEquals(1, intchg.ContainedMessages.Count);
			AssertEquals(msg[0].PK, intchg.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg.PK, msg[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.EXC, msg[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.EXC, msg[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "0000000000000000001", msg[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", getMessage(), msg[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg[0].EM_LinkUniqueID);
		}
		public void TestDeltaCExpResponseWithoutMetaDataCreateEXCEdiMessageType()
		{
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "EASYLOG2TEST_EAD";
			intchg.EI_To = "HYEDFRCMT";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg.EI_InterchangeNum = "237";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = "<Message><EnveloppeMessage><schemaID>Unknown</schemaID><schemaVersion>01032013</schemaVersion><partyId>33159700500064</partyId><transactionId>HYEDFRCMT+DSE+13256</transactionId><numseq>12</numseq></EnveloppeMessage><ReponseDeclaration><Entete><refdos>13256-B00178554</refdos></Entete><ReponseDatas><Erreur><ErreurGen><erreurCode>CORE1981</erreurCode><erreurDescription>CORE1981 : Le pays d'exportation/expédition et le pays de destination ne doivent pas figurer dans l'itinéraire.(AU)</erreurDescription></ErreurGen><ErreurGen><erreurCode>CORE1953</erreurCode><erreurDescription>CORE1953 : Si le bureau de sortie est différent du bureau de rattachement/présentation,le type de sortie ne doit pas être servi.</erreurDescription></ErreurGen><ErreurGen><erreurCode>COR1093</erreurCode><erreurDescription>[Article n°1] COR1093 : La masse nette doit être inférieure ou égale à la masse brute</erreurDescription></ErreurGen></Erreur></ReponseDatas></ReponseDeclaration></Message>";
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			AssertEquals(0, msg.Length);

			var intchg1 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg1.EI_From = "EASYLOG2TEST_EAD";
			intchg1.EI_To = "HYEDFRCMT";
			intchg1.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg1.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg1.EI_InterchangeNum = "238";
			intchg1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg1.EI_Status = EDIInterchange.Status.Queued;
			intchg1.EI_IsActive = true;
			intchg1.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg1.EI_BodyText = "<Message><EnveloppeMessage><schemaID>MessageReponseCDecExp</schemaID><schemaVersion>01032013</schemaVersion><partyId>33159700500064</partyId><transactionId>HYEDFRCMT+DSE+13256</transactionId><numseq>12</numseq></EnveloppeMessage><ReponseDeclaration><Entete><refdos>0000000001</refdos></Entete><ReponseDatas><Erreur><ErreurGen><erreurCode>CORE1981</erreurCode><erreurDescription>CORE1981 : Le pays d'exportation/expédition et le pays de destination ne doivent pas figurer dans l'itinéraire.(AU)</erreurDescription></ErreurGen><ErreurGen><erreurCode>CORE1953</erreurCode><erreurDescription>CORE1953 : Si le bureau de sortie est différent du bureau de rattachement/présentation,le type de sortie ne doit pas être servi.</erreurDescription></ErreurGen><ErreurGen><erreurCode>COR1093</erreurCode><erreurDescription>[Article n°1] COR1093 : La masse nette doit être inférieure ou égale à la masse brute</erreurDescription></ErreurGen></Erreur></ReponseDatas></ReponseDeclaration></Message>";
			Factory.Save();

			var processor1 = new DeltaGIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor1.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg1.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg1.EI_Status);

			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			AssertEquals(1, msg1.Length);

			AssertEquals(1, intchg1.ContainedMessages.Count);
			AssertEquals(msg1[0].PK, intchg1.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg1.PK, msg1[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg1[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.EXC, msg1[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.EXC, msg1[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg1[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "238", msg1[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg1[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg1[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg1[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", intchg1.EI_BodyText, msg1[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg1[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg1[0].EM_LinkUniqueID);
		}

		public void TestExportMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			var cusEntryHeader = dec.ActiveEntryHeaders.AddNew();

			cusEntryHeader.CH_BGMReference = "BGMTEST";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			cusEntryHeader.Messages.Add(outgoingMessage);

			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "FRCUS";
			intchg.EI_To = "TEST";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg.EI_InterchangeNum = "0000000000000000001";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = getMessage();
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			AssertEquals(1, msg.Length);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.EXC, msg[0].EM_MessageType);

			AssertEquals("MRN", "19FRD617C073961303", cusEntryHeader.MovementReferenceNumber);
		}
		string getMessage()
		{
			return @"<Message>
		<MetaData>
			<Application>DELTAC</Application>
			<DeclarationReference>B00456982314</DeclarationReference>
			<EntryNumberType>EXP</EntryNumberType>
		</MetaData>
		<EnveloppeMessage>
			<schemaID>MessageReponseCDecExp</schemaID>
			<schemaVersion>01032013</schemaVersion>
			<partyId>41138023100026</partyId>
			<transactionId>AAAAAAAAA+BBB+0000000001</transactionId>
			<numseq>0</numseq>
		</EnveloppeMessage>
		<ReponseDeclaration>
			<Entete>
				<refdec>1907396130</refdec>
				<mrnecs>19FRD617C073961303</mrnecs>
				<refdos>9-B00001000</refdos>
			</Entete>
			<ReponseDatas>
			</ReponseDatas>
		</ReponseDeclaration>
	   </Message>";
		}
	}
}
