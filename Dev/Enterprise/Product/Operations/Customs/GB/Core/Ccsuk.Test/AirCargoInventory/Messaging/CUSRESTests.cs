using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class CUSRESTests : CcsukNonChiefResponseBaseMessageProcessorTest
	{
		[TestDate(1987, 12, 11)]
		public void TestParseCUSRES_FBK_DeclarationAndShipmentDoNotExist()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCusresResponsesToCusdecFbk, Guid.Empty, Factory);
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "", false);
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESReplyToTheFallbackEntryMessage_WholeHouse, hawb.FBKs.AddNew(), hawb);
			TestParseCUSRES_FBK_Asserts(outboundMessage);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);

			var sentEmail = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("A customs declaration has been created for this job", sentEmail.Body);
			AssertContains("<td>Entry Number</td><td>9876543</td>", sentEmail.Body);
			AssertContains("CCS-UK House Bill 111-77777777-12345678 - response to FBK request", sentEmail.Subject);
			AssertContains(emailAddress, sentEmail.Recipients[0].Email);
		}

		public void TestParseCUSRES_FBK_ShipmentExistsAndItHasAGBDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-6);
			shipment.JS_HouseBill = "12345678";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKDestination = Core.Constants.CountryCodes.UnitedKingdom;

			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "", false);
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESReplyToTheFallbackEntryMessage_WholeHouse, hawb.FBKs.AddNew(), hawb);
			TestParseCUSRES_FBK_Asserts(outboundMessage);
		}

		public void TestParseCUSRES_FBK_DeclarationExistsAndIsLinkedOnlyByNK()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("123", "11177777777", "12345678", "", false);
			hawb.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			Factory.Save();
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESReplyToTheFallbackEntryMessage_WholeHouse, hawb.FBKs.AddNew(), hawb);
			dec.Reload();
			TestParseCUSRES_FBK_Asserts(outboundMessage);
		}

		public void TestParseCUSRES_FBK_ShipmentExistsAndItHasADeclarationByFK()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "1234", false);
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESReplyToTheFallbackEntryMessage_WholeHouse, hawb.FBKs.AddNew(), hawb);
			TestParseCUSRES_FBK_Asserts(outboundMessage);
		}

		public void TestParseCUSRES_FBK_DeclarationExistsAndIsLinkedToHawbByFK_And_F2Printing()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("123", "11177777777", "12345678", "1234", false);
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESReplyToTheFallbackEntryMessage_WholeHouse, hawb.FBKs.AddNew(), hawb);
			dec.Reload();
			TestParseCUSRES_FBK_Asserts(outboundMessage);

			var query = new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "F2 Fallback clearance request 111-77777777-12345678");
			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull("F2 document should be queued for printing", printJob);
		}

		public void TestParseCUSRES_FBK_Split()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "", false);
			var split = hawb.Splits.AddNew();
			split.SplitReference = "69";
			var fbkUnderbond = hawb.FBKs.AddNew();
			fbkUnderbond.SplitReferenceToWhichThisRemovalPertains = "69";
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESReplyToTheFallbackEntryMessage_Split, fbkUnderbond, hawb);
			var query = new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "F2 Fallback clearance request 111-77777777-12345678/69");
			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull("F2 document should be queued for printing", printJob);
			AssertEquals("Inbound message EM_ApplicationReference", "69", hawb.Messages.LastIncomingMessage.EM_ApplicationReference);
		}

		public void TestParseCUSRES_FBK_DeclarationExistsAndIsLinkedToHawbByFK_And_F2Printing_SplitHouse()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("123", "11177777777", "12345678", "1234", false);
			var splitHouse = hawb.Splits.AddNew();
			splitHouse.SplitReference = "03";
			var irrelevantSplitHouse = hawb.Splits.AddNew();
			irrelevantSplitHouse.SplitReference = "01";
			var fbk = hawb.FBKs.AddNew();
			fbk.SplitReferenceToWhichThisRemovalPertains = "03";
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESReplyToTheFallbackEntryMessage_SplitHouse, fbk, splitHouse);
			dec.Reload();
			splitHouse.Reload();
			AssertEquals("Cusres for FBK still updates job's CAC", "ENTRY/REQUEST ACCEPTED", splitHouse.LatestCustomsActionText);
			AssertEquals("CA", splitHouse.CustomsActionCode);
			AssertEquals("", hawb.LatestCustomsActionText);
			AssertEquals("--", hawb.CustomsActionCode);
			var query = new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "F2 Fallback clearance request 111-77777777-12345678/03");
			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull("F2 document should be queued for printing", printJob);
			AssertEquals("03", hawb.Messages.LastIncomingMessage.EM_ApplicationReference);
			AssertEquals(EDIInterchange.Status.Received, hawb.Messages.LastIncomingMessage.Interchange.EI_Status);
		}

		public void TestParseCUSRES_IAR_House()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "123", false);
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_House, hawb.IARs.AddNew(), hawb);
			AssertContains("IAR html message not as expected.", @"<h3>IAR Response</h3>
							<h4>111-77777777-12345678: QUERIED OR DETAINED</h4>", hawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertEquals("Status of message is invalid.", EDIMessage.Status.Received, hawb.Messages.LastIncomingMessage.EM_Status);
			AssertEquals("Outbound message not acknowledged.", EDIMessage.Status.Acknowledged, outboundMessage.EM_Status);
			AssertContains("Custom Actions Code is NOT UPDATED by cusres", "", hawb.LatestCustomsActionText);
			AssertContains("Customs Action Text is NOT UPDATED.", "", hawb.CustomsActionCode);
			AssertEquals("Inbound message EM_MessageType.", "IAR", hawb.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals("Inbound message EM_MessageSubType", "RES", hawb.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals("Inbound message EM_ApplicationReference", "", hawb.Messages.LastIncomingMessage.EM_ApplicationReference);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);

			var sentEmail = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains(">Response to IAR Request<", sentEmail.Body);
			AssertContains("QUERIED OR DETAINED", sentEmail.Body);
			AssertContains("CCS-UK House Bill 111-77777777-12345678 - response to IAR request", sentEmail.Subject);
		}

		public void TestParseCUSRES_IAR_Basic()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "", "123", false);
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_Basic, mawb.IARs.AddNew(), mawb);
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertContains("IAR html message not as expected.", @"<h3>IAR Response</h3>
							<h4>111-77777777: QUERIED OR DETAINED</h4>", mawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertEquals("Status of message is invalid.", EDIMessage.Status.Received, mawb.Messages.LastIncomingMessage.EM_Status);
			AssertEquals("Outbound message not acknowledged.", EDIMessage.Status.Acknowledged, outboundMessage.EM_Status);
			AssertContains("Custom Actions Code is invalid", "", mawb.LatestCustomsActionText);
			AssertContains("Customs Action Text is invalid.", "", mawb.CustomsActionCode);
			AssertEquals("Inbound message EM_MessageType.", "IAR", mawb.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals("Inbound message EM_MessageSubType", "RES", mawb.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals("Inbound message EM_ApplicationReference", "", mawb.Messages.LastIncomingMessage.EM_ApplicationReference);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, mawb.PresenceOnNetworkStatus);
		}

		public void TestParseCUSRES_IAR_SplitBasic()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "", "123", false);
			var splitBasic = mawb.Splits.AddNew();
			splitBasic.SplitReference = "69";
			Factory.Save();
			var splitIar = mawb.IARs.AddNew();
			var irrelevantUnderbond = mawb.IARs.AddNew();
			irrelevantUnderbond.C4_Status = "POO";
			splitIar.SplitReferenceToWhichThisRemovalPertains = "69";
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_SplitBasic, splitIar, splitBasic);
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			splitBasic.Reload();
			AssertContains("IAR html message not as expected.", @"<h3>IAR Response</h3>
							<h4>111-77777777/69: QUERIED OR DETAINED</h4>", mawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertEquals("Status of message is invalid.", EDIMessage.Status.Received, mawb.Messages.LastIncomingMessage.EM_Status);
			AssertEquals("Outbound message not acknowledged.", EDIMessage.Status.Acknowledged, outboundMessage.EM_Status);
			AssertContains("Custom Actions Code is invalid", "", mawb.LatestCustomsActionText);
			AssertContains("Customs Action Text is invalid.", "", mawb.CustomsActionCode);
			AssertContains("Custom Actions Code is invalid", "", splitBasic.LatestCustomsActionText);
			AssertContains("Customs Action Text is invalid.", "", splitBasic.CustomsActionCode);
			AssertEquals("Inbound message EM_MessageType.", "IAR", mawb.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals("Inbound message EM_MessageSubType", "RES", mawb.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals("Inbound message EM_ApplicationReference is split number", "69", mawb.Messages.LastIncomingMessage.EM_ApplicationReference);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, splitBasic.PresenceOnNetworkStatus);
			irrelevantUnderbond.Reload();
			splitIar.Reload();
			AssertEquals("ACK", splitIar.C4_Status);
			AssertEquals("POO", irrelevantUnderbond.C4_Status);
		}

		public void TestParseCUSRES_IAR_SplitHouse()
		{
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());

			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "123", false);
			var splitHouse = hawb.Splits.AddNew();
			splitHouse.SplitReference = "69";
			Factory.Save();
			var splitIar = hawb.IARs.AddNew();
			splitIar.SplitReferenceToWhichThisRemovalPertains = "69";
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_SplitHouse, splitIar, splitHouse);
			splitHouse.Reload();
			AssertContains(@"<h3>IAR Response</h3>
							<h4>111-77777777-12345678/69: QUERIED OR DETAINED</h4>", hawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertEquals("Status of message is invalid.", EDIMessage.Status.Received, hawb.Messages.LastIncomingMessage.EM_Status);
			AssertEquals("Outbound message not acknowledged.", EDIMessage.Status.Acknowledged, outboundMessage.EM_Status);
			AssertContains("Custom Actions Code is invalid", "", hawb.LatestCustomsActionText);
			AssertContains("Customs Action Text is invalid.", "", hawb.CustomsActionCode);
			AssertContains("Custom Actions Code is invalid", "", splitHouse.LatestCustomsActionText);
			AssertContains("Customs Action Text is invalid.", "", splitHouse.CustomsActionCode);
			AssertEquals("Inbound message EM_MessageType.", "IAR", hawb.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals("Inbound message EM_MessageSubType", "RES", hawb.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals("Inbound message EM_ApplicationReference is split number", "69", hawb.Messages.LastIncomingMessage.EM_ApplicationReference);

			var printJobs = printer.Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("CAC=CR, no GR produced", 0, printJobs.Length);
		}

		public void TestParseCUSRES_TSR_StatusCA()
		{
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());

			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "123", false);
			var tsr = hawb.TSRs.AddNew();
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(tsrResponse.Replace("<<CAC>>", "CA"), tsr, hawb);
			AssertContains(@"<h3>TSR Response</h3>
							<h4>111-77777777-12345678: REQUEST TFR CDG</h4>", hawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertContains(@"<tr><td>Transit Reference Number</td><td>T001579</td></tr><tr><td>Customs Action Code</td><td>CA</td></tr><tr><td>Airport of Receipt</td><td>LHR</td></tr><tr><td>Shed Id</td><td>CWE</td></tr><tr><td>Agent&#39;s Reference Number</td><td>&nbsp;</td></tr><tr><td>Number of Packages Entered</td><td>10</td></tr></table>"
							, hawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			hawb.Reload();
			tsr.Reload();
			AssertEquals("T001579", hawb.CS_TranshipmentEntryNum);
			AssertEquals("T001579", tsr.TranshipmentEntryNumber);

			var printJobs = printer.Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "GR Advice of selected removal request 111-77777777-12345678"));
			AssertEquals("GR produced for CUSRES/TSR/CA", 1, printJobs.Length);
		}

		public void TestParseCUSRES_TSR_StatusCT_NprIsZero()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "123", false);
			hawb.CS_PiecesLanded = 0;
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(tsrResponse.Replace("<<CAC>>", "CT"), hawb.TSRs.AddNew(), hawb);
			AssertNull(Factory.LoadTop1<StmPrintJob>(new ZQuery())); // no print from cusres except for FBK
		}

		public void TestParseCUSRES_TSR_StatusCT_NprIsSet()
		{
			TestParseCUSRES_PrepareDecMawbHawbAndShipment("", "11177777777", "12345678", "123", false);
			hawb.CS_PiecesLanded = 69;
			TestParseCUSRES_PrepareMockMessagesAndRunProcessor(tsrResponse.Replace("<<CAC>>", "CT"), hawb.TSRs.AddNew(), hawb);
			AssertNull(Factory.LoadTop1<StmPrintJob>(new ZQuery())); // no print from cusres except for FBK
		}

		void TestParseCUSRES_FBK_Asserts(EDIMessage outboundMessage)
		{
			dec = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			var pd = dec.PreviousDocuments[0];
			AssertEquals("Previous document for entry number not added by reply to Fallback entry.", "Z/ZZZ/FALLBACK=987-6543", pd.CSI_SubType + "/" + pd.CSI_Code + "/" + pd.CSI_ReferenceNumber);
			AssertEquals("Airport of origin not updated by reply to Fallback entry.", "USLAX", dec.JE_RL_NKOrigin);
			AssertEquals("Airport of receipt not updated by reply to Fallback entry.", "GBLGW", dec.JE_RL_NKFinalDestination);
			AssertEquals("Owner Reference not updated by reply to Fallback entry.", "4321", dec.JE_OwnerRef);
			AssertEquals("Entry Status not updated by reply to Fallback entry.", "FBK", dec.JE_EntryStatus);
			AssertEquals("Badge not updated by reply to Fallback entry.", "FRF", dec.JE_CustomsProfile);
			AssertEquals("Transport Mode not updated by reply to Fallback entry.", Customs.Business.TransportTypeList.Codes.Air, dec.JE_TransportMode);
			AssertEquals("Master Bill Number not updated by reply to Fallback entry.", "11177777777", dec.JE_MasterBill);
			AssertEquals("Flight Number not updated by reply to Fallback entry.", "", dec.JE_VoyageFlightNo);
			AssertEquals("Goods Description not updated by reply to Fallback entry.", "HOME BREW", dec.JE_GoodsDescription);
			AssertEquals("House Bill Number not updated by reply to Fallback entry.", "12345678", dec.JE_HouseBill);
			AssertEquals("Total Number Of Packs not updated by reply to Fallback entry.", 80, dec.JE_TotalNoOfPacks);
			AssertEquals("Declaration Type not updated by reply to Fallback entry.", "", dec.JE_DeclarationType);
			AssertEquals("Entry Sub Style not updated by reply to Fallback entry.", "", dec.JE_EntrySubStyle);
			AssertEquals("Airport of Shed not updated by reply to Fallback entry.", "LGW", dec.JE_LocationOfGoods);
			AssertEquals("Split number not updated by reply to Fallback entry.", "", dec.ZG_HouseSplitReference);
			AssertEquals("Shed Code not updated by reply to Fallback entry.", "SID", dec.SubLocation);
			AssertEquals("MUCR updated by reply to Fallback entry.", "", dec.JE_MasterUCR);
			ZQuery logQ = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			logQ.AddToFilter(StmALogSchema.SL_Reference, "CA");
			AssertEquals(1, hawb.GetLogs().Find(logQ).Length);
			AssertContains(@"<h3>FBK Response</h3>
							<h4>111-77777777-12345678: ENTRY/REQUEST ACCEPTED</h4>", hawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertContains(@"<tr><td>Entry Number</td><td>9876543</td></tr><tr><td>Customs Action Code</td><td>CA</td></tr><tr><td>Airport of Receipt</td><td>LGW</td></tr><tr><td>Shed Id</td><td>SID</td></tr><tr><td>Agent&#39;s Reference Number</td><td>4321</td></tr><tr><td>Number of Packages Entered</td><td>80</td></tr></table>",
							hawb.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertEquals("Status of message is invalid.", EDIMessage.Status.Received, hawb.Messages.LastIncomingMessage.EM_Status);
			AssertEquals("Outbound message not acknowledged.", EDIMessage.Status.Acknowledged, outboundMessage.EM_Status);
			AssertEquals("Inbound message sub type not set.", "FBK", hawb.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals("Inbound message type not set.", "RES", hawb.Messages.LastIncomingMessage.EM_MessageType);
			if (shipment != null)
			{
				AssertEquals(shipment.PK, hawb.CS_JS);
			}
			AssertEquals(dec.PK, hawb.CS_JE_CustomsFormalEntry);
		}

		void TestParseCUSRES_PrepareDecMawbHawbAndShipment(string decNo, string mawbNo, string hawbNo, string shipmentNo, bool attachShipmentToDec)
		{
			dec = null;
			mawb = null;
			hawb = null;
			shipment = null;
			outboundMessage = null;

			if (mawbNo.Length > 0)
			{
				mawb = Factory.New<CusMAWB>();
				mawb.CM_MAWB = mawbNo;
			}
			if (hawbNo.Length > 0)
			{
				hawb = mawb.ChildBills.AddNew();
				hawb.CS_HAWB = hawbNo;
			}
			if (decNo.Length > 0)
			{
				dec = Factory.New<JobDeclaration>();
				dec.CustomsEntryHeaders.AddNew();
				dec.JE_MessageType = MessageTypeList.Codes.Import;
				dec.JE_MasterBill = mawbNo;
				dec.JE_HouseBill = hawbNo;
				dec.JE_DateOfFirstArrival = ZDateTime.Now;
				dec.JE_DeclarationReference = decNo;
				dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				hawb.CS_JE_CustomsFormalEntry = dec.PK;
			}
			if (shipmentNo.Length > 0)
			{
				shipment = Factory.New<ForwardingShipment>();
				if (hawb != null)
				{
					hawb.CS_JS = shipment.PK;
				}
				if (attachShipmentToDec)
				{
					if (dec != null && attachShipmentToDec)
					{
						dec.JE_JS = shipment.PK;
					}
				}
				else
				{
					if (hawb != null && !attachShipmentToDec)
					{
						hawb.CS_JS = shipment.PK;
					}
				}
			}
			Factory.Save();
		}

		void TestParseCUSRES_PrepareMockMessagesAndRunProcessor(string inboundMessage, CusUnderbond cusUnderbond, ICcsukCusAwb awb)
		{
			var mockOutboundEdiMessage = Factory.New<DummyEDIMessage_CUSRESTests>();
			mockOutboundEdiMessage.GetMessageReferenceNumberReturns = "84";
			outboundMessage = mockOutboundEdiMessage;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			outboundMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;
			awb.Messages.Add(outboundMessage);
			Factory.Save();

			var msg = inboundMessage.Replace("COMREF1", string.Format("{0}/{1}", outboundMessage.EM_MessageNum, cusUnderbond.C4_SendersMessageReference));
			var mockInboundEdiMessage = Factory.NewMoq<EDIMessage>();
			var message = mockInboundEdiMessage.Object;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = "CUK";
			message.EM_MessageText = msg.Replace(System.Environment.NewLine, "");
			var inboundInterchange = Factory.New<EDIInterchange>();
			message.EM_EI = inboundInterchange.PK;
			inboundInterchange.EI_From = "POOP";
			inboundInterchange.EI_To = "FART";
			inboundInterchange.EI_BodyText = "X";
			Factory.Save();

			RunProcessors();
			mawb.Reload();
			((BusinessObject)awb).Reload();
			if (hawb != null)
			{
				hawb = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
				dec = Factory.Load<JobDeclaration>(hawb.CS_JE_CustomsFormalEntry);
			}
			outboundMessage.Reload();
		}

		sealed class DummyEDIMessage_CUSRESTests : EDIMessage
		{
			public DummyEDIMessage_CUSRESTests(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			public string GetMessageReferenceNumberReturns { get; set; } = string.Empty;

			protected override string GetMessageReferenceNumber()
			{
				return GetMessageReferenceNumberReturns;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_EmailAddress = "daniel@wisetechglobal.com";
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			var staffGroup = Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup);
			staffGroup.Staff.Add(currentUserInCurrentFactory);
			staffGroup.Staff.Add(anotherStaff);
			branchEnvironment = DisposableEnvironment.ForBranch(Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}

		JobDeclaration dec;
		CusMAWB mawb;
		CusHAWB hawb;
		ForwardingShipment shipment;
		EDIMessage outboundMessage;

		readonly string cUSRESReplyToTheFallbackEntryMessage_Split = @"
UNH+MSGREF+CUSRES:2:912:UN:109602+COMREF1'
BGM+FBK+11177777777+++HWB:12345678:69'
NAD+CB+FRF+FREDS FORWARDING:0812461234'
LOC+11:LGW:145:3::SID:129:ZZZ+27:US+84:LAX:145:3'
PAC+80'
RFF+ABE:4321'
RFF+TN:9876543+141:20101213:102'
GIS+CA:120:ZZZ'
FTX+CAT+++ENTRY/REQUEST ACCEPTED'
FTX+AAA+++HOME BREW'
UNT+8+MSGREF'";

		readonly string cUSRESReplyToTheFallbackEntryMessage_WholeHouse = @"
UNH+MSGREF+CUSRES:2:912:UN:109602+COMREF1'
BGM+FBK+11177777777+++HWB:12345678'
NAD+CB+FRF+FREDS FORWARDING:0812461234'
LOC+11:LGW:145:3::SID:129:ZZZ+27:US+84:LAX:145:3'
PAC+80'
RFF+ABE:4321'
RFF+TN:9876543+141:20101213:102'
GIS+CA:120:ZZZ'
FTX+CAT+++ENTRY/REQUEST ACCEPTED'
FTX+AAA+++HOME BREW'
UNT+8+MSGREF'";

		readonly string cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_House = @"
UNH+MSGREF+CUSRES:2:912:UN:109606+COMREF1'
BGM+IAR+11177777777+++HWB:12345678'
LOC+11:MAN:145:3::SID:129:ZZZ'
PAC+78'
RFF+ABE:4321'
GIS+CR:120:ZZZ'
FTX+CAT+++QUERIED OR DETAINED'
UNT+7+MSGREF'";

		readonly string cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_SplitHouse = @"
UNH+MSGREF+CUSRES:2:912:UN:109606+COMREF1'
BGM+IAR+11177777777+++HWB:12345678:69'
LOC+11:MAN:145:3::SID:129:ZZZ'
PAC+78'
RFF+ABE:4321'
GIS+CR:120:ZZZ'
FTX+CAT+++QUERIED OR DETAINED'
UNT+7+MSGREF'";

		readonly string cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_Basic = @"
UNH+MSGREF+CUSRES:2:912:UN:109606+COMREF1'
BGM+IAR+11177777777'
LOC+11:MAN:145:3::SID:129:ZZZ'
PAC+78'
RFF+ABE:4321'
GIS+CR:120:ZZZ'
FTX+CAT+++QUERIED OR DETAINED'
UNT+7+MSGREF'";

		readonly string cUSRESResponseToRemovalRequestFromTheTsoOrAnAgent_SplitBasic = @"
UNH+MSGREF+CUSRES:2:912:UN:109606+COMREF1'
BGM+IAR+11177777777+++ACD::69'
LOC+11:MAN:145:3::SID:129:ZZZ'
PAC+78'
RFF+ABE:4321'
GIS+CR:120:ZZZ'
FTX+CAT+++QUERIED OR DETAINED'
UNT+7+MSGREF'";

		readonly string tsrResponse = @"
UNH+318+CUSRES:2:912:UN:109606+COMREF1'
BGM+TSR+11177777777+++HWB:12345678'
NAD+CB+CAR+CARGOWISE'
LOC+11:LHR:145:3::CWE:129:ZZZ+27:US+84:ATL:145:3'
PAC+10'
RFF+TN:T001579'
GIS+<<CAC>>:120:ZZZ'
FTX+CAT+++REQUEST TFR CDG'
FTX+AAA+++MORE TOYS
'UNT+10+318'";

		readonly string cUSRESReplyToTheFallbackEntryMessage_SplitHouse = @"
UNH+MSGREF+CUSRES:2:912:UN:109602+COMREF1'
BGM+FBK+11177777777+++HWB:12345678:03'
NAD+CB+FRF+FREDS FORWARDING:0812461234'
LOC+11:LGW:145:3::SID:129:ZZZ+27:US+84:LAX:145:3'
PAC+80'
RFF+ABE:4321'
RFF+TN:9876543+141:20101213:102'
GIS+CA:120:ZZZ'
FTX+CAT+++ENTRY/REQUEST ACCEPTED'
FTX+AAA+++HOME BREW'
UNT+8+MSGREF'";
	}
}
