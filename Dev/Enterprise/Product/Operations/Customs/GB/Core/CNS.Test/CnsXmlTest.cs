using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.CNS.Testing
{
	[TestedType(typeof(CnsCompassServiceTask))]
	public partial class CnsStatusUpdateMessageTest : ServiceTaskTestCase<CnsCompassServiceTask>
	{
		// Test removed - an invalid date in the file will be caught by the "invalid schema" processing part

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingVersionTwoFormatMultipleEntriesWithSameEntryNumberDifferingByDate()
		{
			string xml = @" 
							 <CNScargoStatus>
								 <MessageHeader>
									  <HeaderID>20113271303913216355</HeaderID> 
									  <MessageType>CLEARED</MessageType> 
								 </MessageHeader>
								 <MessageDetail>
									  <Site>PNT1</Site> 
									  <Container>LIGT1104272</Container> 
									  <UCN>CRAP</UCN> 
									  <Clearance>CL</Clearance> 
									  <BillOfLading>BL-32146</BillOfLading> 
									  <Packages>1</Packages> 
									  <Weight>1234.00</Weight> 
									  <Hold /> 
									  <CHIEFEntryNo>000038M</CHIEFEntryNo> 
									  <CHIEFEntryRoute>6</CHIEFEntryRoute> 
									  <CHIEFEntryDate>20110427</CHIEFEntryDate> 
									  <CHIEFepu>290</CHIEFepu> 
								  </MessageDetail>
							  </CNScargoStatus>
								";

			SetUpXmlAndDec(xml);
			cusEntryHeader.EntryNumber = "290-000038M";
			cusEntryHeader.CH_EntryStatus = "ABC";

			var declarationTwo = Factory.New<JobDeclaration>();
			declarationTwo.JE_MessageType = "IMP";
			declarationTwo.JE_MasterUCR = "POOP";

			var cusEntryHeaderTwo = declarationTwo.CustomsEntryHeaders.AddNew();
			cusEntryHeaderTwo.EntryNumber = "290-000038M";
			cusEntryHeaderTwo.CusEntryNumber.CE_IssueDate = new ZDateTime(2011, 04, 27);  // from XML

			Factory.Save();
			RunAndReload(Factory);
			cusEntryHeaderTwo.Reload();
			AssertEquals(EntryStatusList.Codes.Clear, cusEntryHeaderTwo.CH_EntryStatus);
			AssertEquals("ABC", cusEntryHeader.CH_EntryStatus);
		}

		// Test removed - an invalid date in the file will be caught by the "invalid schema" processing part

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingVersionTwoFormat()
		{
			string xml = @" 
							 <CNScargoStatus>
								 <MessageHeader>
									  <HeaderID>20113271303913216355XYZ</HeaderID> 
									  <MessageType>CLEARED</MessageType> 
								 </MessageHeader>
								 <MessageDetail>
									  <Site>PNT1</Site> 
									  <Container>LIGT1104272</Container> 
									  <UCN>PNT10029G00100</UCN> 
									  <Clearance>CL</Clearance> 
									  <BillOfLading>BL-32146</BillOfLading> 
									  <Packages>1</Packages> 
									  <Weight>1234.00</Weight> 
									  <Hold /> 
									  <CHIEFEntryNo>000038M</CHIEFEntryNo> 
									  <CHIEFEntryRoute>6</CHIEFEntryRoute> 
									  <CHIEFEntryDate>20110427</CHIEFEntryDate> 
									  <CHIEFepu>290</CHIEFepu> 
								  </MessageDetail>
							  </CNScargoStatus>
								";

			SetUpXmlAndDec(xml);
			cusEntryHeader.EntryNumber = "290-000038M";  // Job will be found on entry num - UCN mismatches deliberately
			cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDateTime(2011, 04, 27);
			Factory.Save();
			RunAndReload(Factory);

			AssertEquals("Entry number unchanged", "290-000038M", cusEntryHeader.EntryNumber);
			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("<h2>Customs CLEARED advice for B000069 / 290-000038M", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("<tr><td>Entry Number</td><td>290-000038M</td></tr><tr><td>Entry Route</td><td>6</td></tr><tr><td>Entry Date</td><td>27-Apr-11</td></tr>", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("Customs CLEARED advice for  B000069 / 290-000038M / UCN=PNT10029G001", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("20113271303913216355XYZ", cusEntryHeader.Messages[0].EM_MessageNum); // The exception reporter listener will handle any maxlength issues for us
			AssertEquals("RCV", cusEntryHeader.Messages[0].Interchange.EI_Status);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingNotSchemaValid()
		{
			string xml = @" 
							 <CNScargoStatus>
								 <MessageHeader>
									  <HeaderID>20113271303913216355</HeaderID> 
									  <MessageType>POOP</MessageType> 
								 </MessageHeader>
								 <MessageDetail>
									  <Site>PNT1</Site> 
									  <Container>LIGT1104272</Container> 
									  <UCN>PNT10029G00100</UCN> 
									  <Clearance>CL</Clearance> 
									  <BillOfLading>BL-32146</BillOfLading> 
									  <Packages>1</Packages> 
									  <Weight>1.23</Weight> 
									  <Hold /> 
									  <CHIEFEntryNo>000038M</CHIEFEntryNo> 
									  <CHIEFEntryRoute>6</CHIEFEntryRoute> 
									  <CHIEFEntryDate>20110427</CHIEFEntryDate> 
									  <CHIEFepu>290</CHIEFepu> 
								  </MessageDetail>
							  </CNScargoStatus>
								";

			SetUpXmlAndDec(xml);
			cusEntryHeader.EntryNumber = "290-000038M";  // Job will be found on entry num - UCN mismatches deliberately
			cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDateTime(2011, 04, 27);
			Factory.Save();
			var originalUserName = GlbStaff.CurrentUser.GS_LoginName;
			var originalUserCode = GlbStaff.CurrentUser.GS_Code;
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;
			GlbStaff.CurrentUser.GS_LoginName = User.ServiceUserName;
			RunAndReload(Factory);

			// First run, message marked as ERRor.
			var receivedMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals("ERR", receivedMessage.EM_Status);
			var sentEmail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("CNS XML message filename was not schema-valid", sentEmail.Body);
			AssertContains("POOP", sentEmail.Body);
			Assert(sentEmail.Recipients.Contains("m.mouse@disney.com"));
			Assert(sentEmail.Recipients.Contains("d.duck@disney.com"));

			// User re-queues the message
			GlbStaff.CurrentUser.GS_Code = "DAN";
			receivedMessage.EM_Status = "QUE";  // saves with last-edited-user=DAN
			Factory.Save();

			// Second run, we properly fail as before.
			RunAndReload(Factory);
			AssertEquals("QUE", receivedMessage.EM_Status);
			AssertContains("'POOP' is not a valid value for MessageType", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			GlbStaff.CurrentUser.GS_Code = originalUserCode;
			GlbStaff.CurrentUser.GS_LoginName = originalUserName;
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingVersionTwoFormat_UpdatesToNewRouteIfNotClear()
		{
			string xml = @" 
							 <CNScargoStatus>
								 <MessageHeader>
									  <HeaderID>20113271303913216355</HeaderID> 
									  <MessageType>HOLDREMOVE</MessageType> 
								 </MessageHeader>
								 <MessageDetail>
									  <Site>PNT1</Site> 
									  <Container>LIGT1104272</Container> 
									  <UCN>PNT10029G00100</UCN>  

									  <BillOfLading>BL-32146</BillOfLading> 
									  <Packages>1</Packages> 
									  <Weight>1234.00</Weight> 
									  <Hold>B</Hold>
									  <CHIEFEntryNo>000038M</CHIEFEntryNo> 
									  <CHIEFEntryRoute>6</CHIEFEntryRoute> 
									  <CHIEFEntryDate>20110427</CHIEFEntryDate> 
									  <CHIEFepu>290</CHIEFepu> 
								  </MessageDetail>
							  </CNScargoStatus>
								";

			SetUpXmlAndDec(xml);
			cusEntryHeader.EntryNumber = "290-000038M";  // Job will be found on entry num - UCN mismatches deliberately
			cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDateTime(2011, 04, 27);
			Factory.Save();
			RunAndReload(Factory);

			AssertEquals("Route updated to RT6 because clearance is not being advised", EntryStatusList.Codes.Route6, cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingVersionTwoFormat_DoesntUpdateToNewRouteIfAlreadyClear()
		{
			string xml = @" 
							 <CNScargoStatus>
								 <MessageHeader>
									  <HeaderID>20113271303913216355</HeaderID> 
									  <MessageType>HOLDREMOVE</MessageType> 
								 </MessageHeader>
								 <MessageDetail>
									  <Site>PNT1</Site> 
									  <Container>LIGT1104272</Container> 
									  <UCN>PNT10029G00100</UCN>  

									  <BillOfLading>BL-32146</BillOfLading> 
									  <Packages>1</Packages> 
									  <Weight>1234.00</Weight> 
									  <Hold>B</Hold>
									  <CHIEFEntryNo>000038M</CHIEFEntryNo> 
									  <CHIEFEntryRoute>6</CHIEFEntryRoute> 
									  <CHIEFEntryDate>20110427</CHIEFEntryDate> 
									  <CHIEFepu>290</CHIEFepu> 
								  </MessageDetail>
							  </CNScargoStatus>
								";

			SetUpXmlAndDec(xml);
			cusEntryHeader.EntryNumber = "290-000038M";  // Job will be found on entry num - UCN mismatches deliberately
			cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDateTime(2011, 04, 27);
			cusEntryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			Factory.Save();
			RunAndReload(Factory);

			AssertEquals("Route unchanged because clearance already granted even though this message states nothing about clearance and advises of RT6. Example - if the email containing the XML arrives late.",
				EntryStatusList.Codes.Clear, cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingHoldRemovedWithoutClearance()
		{
			string xml = @"
									<CNScargoStatus>
										<MessageHeader>
											<HeaderID>100217104521</HeaderID>
											<MessageType>HOLDREMOVE</MessageType>
										</MessageHeader>
										<MessageDetail>
											<Site>ELF1</Site>
											<Container>ELFT9021204</Container>		
											<UCN>ELF1A001200100</UCN>
											<BillOfLading>BOL33</BillOfLading>
											<Packages>10</Packages>
											<Weight>24350</Weight>
											<Hold>B</Hold>
										</MessageDetail>
									</CNScargoStatus>

								";

			SetUpXmlAndDec(xml);
			this.cusEntryHeader.CH_EntryStatus = "ABC";
			RunAndReload(Factory);

			AssertEquals("ABC", cusEntryHeader.CH_EntryStatus);  // Unchanged from whatever it was before (usually HLD). 
			AssertEquals("PRS", mailItem.MI_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("<h2>Customs HOLDREMOVE advice for B000069 / 191-69696969</h1> <h2>UCN=ELF1A0012001</h2>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<td>HOLDREMOVE</td></tr><tr><td>Advice datetime (header ID)</td><td>17-Feb-10 10:45:21</td></tr><tr><td>UCN</td><td>ELF1A0012001</td></tr><tr><td>Container</td><td>ELFT9021204</td></tr><tr><td>BoL</td><td>BOL33</td></tr><tr><td>Clearance code</td><td>&nbsp;</td></tr><tr><td>Hold type</td><td>ICD hold</td></tr><tr><td>NoP</td><td>10</td></tr><tr><td>Weight</td><td>24350</td></tr><tr><td>Site</td><td>ELF1</td></tr></table>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<p><b>N.B.</b> A <font color='red'>HOLDREMOVE</font> of type B (ICD hold) was toggled for this entry</p>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("Customs HOLDREMOVE advice for  B000069 / 191-69696969 / UCN=ELF1A0012001", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			ZQuery q = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CnsCompassServiceTask.Code);
			q.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] messages = Factory.Load<EDIMessage>(q);
			AssertEquals(1, messages.Length);

			AssertNotEquals(EDIMessage.Status.Queued, messages[0].EM_Status);
			AssertContains("<UCN>ELF1A001200100</UCN>", messages[0].EM_MessageText);
			AssertEquals("100217104521", messages[0].EM_MessageNum);

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			IeDoc eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Clearance advice (XML) from CNS", eDoc.Description);
			AssertEquals("eDoc.DocType", Core.Constants.RefDocTypes.ClearanceAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", "Clearance advice 191-69696969.CnsXml.xml", eDoc.FileName);

			ZQuery logQ = new ZQuery(StmALogSchema.SL_Parent, cusEntryHeader.PK);
			logQ.AddToFilter(StmALogSchema.SL_Reference, "HoldRemoved-B");
			AssertEquals(1, cusEntryHeader.GetLogs().Find(logQ).Length);

			AssertContains("Interchange's header is email's header", "Daniel Rocks: Yes", messages[0].Interchange.EI_HeaderText);
			AssertEquals("Emails sent = 1. 1 to user.", 1, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingHoldRemovedWithSimultaneousClearance()
		{
			string xml = @"
									<CNScargoStatus>
										<MessageHeader>
											<HeaderID>100217104521</HeaderID>
											<MessageType>HOLDREMOVE</MessageType>
										</MessageHeader>
										<MessageDetail>
											<Site>ELF1</Site>
											<Container>ELFT9021204</Container>		
											<UCN>ELF1A001200100</UCN>
											<Clearance>CL</Clearance>
											<BillOfLading>BOL33</BillOfLading>
											<Packages>10</Packages>
											<Weight>24350</Weight>
											<Hold>U</Hold>
										</MessageDetail>
									</CNScargoStatus>

								";

			SetUpXmlAndDec(xml);
			RunAndReload(Factory);

			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("<h2>Customs HOLDREMOVE/Cleared advice for B000069 / 191-69696969</h1> <h2>UCN=ELF1A0012001</h2>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			string expectedBody = "<td>HOLDREMOVE</td></tr><tr><td>Advice datetime (header ID)</td><td>17-Feb-10 10:45:21</td></tr><tr><td>UCN</td><td>ELF1A0012001</td></tr><tr><td>Container</td><td>ELFT9021204</td></tr><tr><td>BoL</td><td>BOL33</td></tr><tr><td>Clearance code</td><td>CL</td></tr><tr><td>Hold type</td><td>Unit hold</td></tr><tr><td>NoP</td><td>10</td></tr><tr><td>Weight</td><td>24350</td></tr><tr><td>Site</td><td>ELF1</td></tr></table>";
			AssertContains(expectedBody, Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			cusEntryHeader.Messages.Load();
			EDIMessage ediMessageIn = cusEntryHeader.Messages.LastIncomingMessage;
			AssertContains(expectedBody, ediMessageIn.EM_MessageInterpretation);
			AssertEquals("100217104521", ediMessageIn.EM_MessageNum);

			AssertContains("<p><b>N.B.</b> A <font color='red'>HOLDREMOVE</font> of type U (Unit hold) was toggled for this entry</p>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("Customs HOLDREMOVE/Cleared advice for  B000069 / 191-69696969 / UCN=ELF1A0012001", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			ZQuery q = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CnsCompassServiceTask.Code);
			q.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] messages = Factory.Load<EDIMessage>(q);
			AssertEquals(1, messages.Length);

			AssertNotEquals(EDIMessage.Status.Queued, messages[0].EM_Status);
			AssertContains("<UCN>ELF1A001200100</UCN>", messages[0].EM_MessageText);

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			IeDoc eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Clearance advice (XML) from CNS", eDoc.Description);
			AssertEquals("eDoc.DocType", Core.Constants.RefDocTypes.ClearanceAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", "Clearance advice 191-69696969.CnsXml.xml", eDoc.FileName);

			ZQuery logQ = new ZQuery(StmALogSchema.SL_Parent, cusEntryHeader.PK);
			logQ.AddToFilter(StmALogSchema.SL_Reference, "HoldRemoved-U");
			AssertEquals(1, cusEntryHeader.GetLogs().Find(logQ).Length);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingHoldAddedWithoutClearance()
		{
			string xml = @"
							<CNScargoStatus>
								<MessageHeader>
									<HeaderID>100217104521</HeaderID>
									<MessageType>HOLDADD</MessageType>
								</MessageHeader>
								<MessageDetail>
									<Site>ELF1</Site>
									<Container>ELFT9021204</Container>		
									<UCN>ELF1A001200100</UCN>
									<BillOfLading>BOL33</BillOfLading>
									<Packages>10</Packages>
									<Weight>24350</Weight>
									<Hold>B</Hold>
								</MessageDetail>
							</CNScargoStatus>
								";

			SetUpXmlAndDec(xml);
			RunAndReload(Factory);

			AssertEquals("HLD", cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("<h2>Customs HOLDADD advice for B000069 / 191-69696969</h1> <h2>UCN=ELF1A0012001</h2>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<td>HOLDADD</td></tr><tr><td>Advice datetime (header ID)</td><td>17-Feb-10 10:45:21</td></tr><tr><td>UCN</td><td>ELF1A0012001</td></tr><tr><td>Container</td><td>ELFT9021204</td></tr><tr><td>BoL</td><td>BOL33</td></tr><tr><td>Clearance code</td><td>&nbsp;</td></tr><tr><td>Hold type</td><td>ICD hold</td></tr><tr><td>NoP</td><td>10</td></tr><tr><td>Weight</td><td>24350</td></tr><tr><td>Site</td><td>ELF1</td></tr></table>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<p><b>N.B.</b> A <font color='red'>HOLDADD</font> of type B (ICD hold) was toggled for this entry</p>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("Customs HOLDADD advice for  B000069 / 191-69696969 / UCN=ELF1A0012001", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			ZQuery q = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CnsCompassServiceTask.Code);
			q.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] messages = Factory.Load<EDIMessage>(q);
			AssertEquals(1, messages.Length);

			AssertNotEquals(EDIMessage.Status.Queued, messages[0].EM_Status);
			AssertContains("<UCN>ELF1A001200100</UCN>", messages[0].EM_MessageText);

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			IeDoc eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Clearance advice (XML) from CNS", eDoc.Description);
			AssertEquals("eDoc.DocType", Core.Constants.RefDocTypes.ClearanceAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", "Clearance advice 191-69696969.CnsXml.xml", eDoc.FileName);

			AssertEquals("Hold-B", cusEntryHeader.GetLogs().MostRecentLog.SL_Reference);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingHoldAddedWithSimultaneousClearance()
		{
			string xml = @"
							<CNScargoStatus>
								<MessageHeader>
									<HeaderID>100217104521</HeaderID>
									<MessageType>HOLDADD</MessageType>
								</MessageHeader>
								<MessageDetail>
									<Site>ELF1</Site>
									<Container>ELFT9021204</Container>		
									<UCN>ELF1A001200100</UCN>
									<Clearance>CL</Clearance>
									<BillOfLading>BOL33</BillOfLading>
									<Packages>10</Packages>
									<Weight>24350</Weight>
									<Hold>B</Hold>
								</MessageDetail>
							</CNScargoStatus>
								";

			SetUpXmlAndDec(xml);
			RunAndReload(Factory);

			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("<h2>Customs HOLDADD/Cleared advice for B000069 / 191-69696969</h1> <h2>UCN=ELF1A0012001</h2>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<td>HOLDADD</td></tr><tr><td>Advice datetime (header ID)</td><td>17-Feb-10 10:45:21</td></tr><tr><td>UCN</td><td>ELF1A0012001</td></tr><tr><td>Container</td><td>ELFT9021204</td></tr><tr><td>BoL</td><td>BOL33</td></tr><tr><td>Clearance code</td><td>CL</td></tr><tr><td>Hold type</td><td>ICD hold</td></tr><tr><td>NoP</td><td>10</td></tr><tr><td>Weight</td><td>24350</td></tr><tr><td>Site</td><td>ELF1</td></tr></table>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<p><b>N.B.</b> A <font color='red'>HOLDADD</font> of type B (ICD hold) was toggled for this entry</p>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("Customs HOLDADD/Cleared advice for  B000069 / 191-69696969 / UCN=ELF1A0012001", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			ZQuery q = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CnsCompassServiceTask.Code);
			q.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] messages = Factory.Load<EDIMessage>(q);
			AssertEquals(1, messages.Length);

			AssertNotEquals(EDIMessage.Status.Queued, messages[0].EM_Status);
			AssertContains("<UCN>ELF1A001200100</UCN>", messages[0].EM_MessageText);

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			IeDoc eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Clearance advice (XML) from CNS", eDoc.Description);
			AssertEquals("eDoc.DocType", Core.Constants.RefDocTypes.ClearanceAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", "Clearance advice 191-69696969.CnsXml.xml", eDoc.FileName);

			// Why can't we GetLogs() return an StmALog[], hmmm?
			ZQuery qLog = new ZQuery();
			qLog.AddToFilter(StmALogSchema.SL_Parent, cusEntryHeader.PK);
			qLog.AddToFilter(StmALogSchema.SL_Reference, "Hold-B");
			AssertEquals(1, Factory.Load<StmALog>(qLog).Length);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingCleared()
		{
			string xml = @"
								<CNScargoStatus>
										<MessageHeader>
											<HeaderID>101112131415</HeaderID>
											<MessageType>CLEARED</MessageType>
										</MessageHeader>
									<MessageDetail>
										<Site>ELF1</Site>
										<Container>ELFT9021204</Container>		
										<UCN>ELF1A001200100</UCN>
										<Clearance>CL</Clearance>
										<Packages>10</Packages>
										<Weight>24350</Weight>
									</MessageDetail>
								</CNScargoStatus>

								";

			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var bbbBranch = Factory.New<GlbBranch>();
			bbbBranch.GB_Code = "BBB";
			bbbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(bbbBranch.PK.ToGuid()))
			{
				SetUpXmlAndDec(xml);
			}

			RunAndReload(Factory);

			AssertEquals(new ZDateTime(2010, 11, 12, 13, 14, 00), cusEntryHeader.CH_EntryReleaseDate);  // from header ID... without seconds (bah!)
			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);

			AssertEquals("1 email", 1, Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("<h2>Customs CLEARED advice for B000069 / 191-69696969</h1> <h2>UCN=ELF1A0012001</h2>",
				Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<td>CLEARED</td></tr><tr><td>Advice datetime (header ID)</td><td>12-Nov-10 13:14:15</td></tr><tr><td>UCN</td><td>ELF1A0012001</td></tr><tr><td>Container</td><td>ELFT9021204</td></tr><tr><td>BoL</td><td>&nbsp;</td></tr><tr><td>Clearance code</td><td>CL</td></tr><tr><td>Hold type</td><td>&nbsp;</td></tr><tr><td>NoP</td><td>10</td></tr><tr><td>Weight</td><td>24350</td></tr><tr><td>Site</td><td>ELF1</td></tr></table>",
				Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("Customs CLEARED advice for  B000069 / 191-69696969 / UCN=ELF1A0012001", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			ZQuery q = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CnsCompassServiceTask.Code);
			q.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] messages = Factory.Load<EDIMessage>(q);
			AssertEquals(1, messages.Length);

			AssertNotEquals(EDIMessage.Status.Queued, messages[0].EM_Status);
			AssertContains("<UCN>ELF1A001200100</UCN>", messages[0].EM_MessageText);
			AssertEquals(bbbBranch.PK, messages[0].EM_GB);

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			IeDoc eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Clearance advice (XML) from CNS", eDoc.Description);
			AssertEquals("eDoc.DocType", Core.Constants.RefDocTypes.ClearanceAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", "Clearance advice 191-69696969.CnsXml.xml", eDoc.FileName);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingSplitUcnButUserPutInVerbatimMucr_Mismatch()
		{
			string xml = @"
								<CNScargoStatus>
										<MessageHeader>
											<HeaderID>101112131415</HeaderID>
											<MessageType>CLEARED</MessageType>
										</MessageHeader>
									<MessageDetail>
										<Site>ELF1</Site>
										<Container>ELFT9021204</Container>		
										<UCN>ELF1A001200100</UCN>
										<Clearance>CL</Clearance>
										<Packages>10</Packages>
										<Weight>24350</Weight>
									</MessageDetail>
								</CNScargoStatus>

								";

			// Note UCN in file is ELF1A001200100, and MUCR on job is also ELF1A001200100 (same). 
			// Compare to TestCnsParsingClearedWithCargoSplit() where the MUCR on the job is ELF1A001200100 (ie. trimmed)
			SetUpXmlAndDec(xml);
			declaration.JE_MasterUCR = "ELF1A001200100";
			GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			RunAndReload(Factory);
			AssertNotEquals("Rego demands a mismatch, job is not updated", "CLR", cusEntryHeader.CH_EntryStatus);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingSplitUcnButUserPutInVerbatimMucr_Match()
		{
			string xml = @"
								<CNScargoStatus>
										<MessageHeader>
											<HeaderID>101112131415</HeaderID>
											<MessageType>CLEARED</MessageType>
										</MessageHeader>
									<MessageDetail>
										<Site>ELF1</Site>
										<Container>ELFT9021204</Container>		
										<UCN>ELF1A001200100</UCN>
										<Clearance>CL</Clearance>
										<Packages>10</Packages>
										<Weight>24350</Weight>
									</MessageDetail>
								</CNScargoStatus>

								";

			// Note UCN in file is ELF1A001200100, and MUCR on job is also ELF1A001200100 (same). 
			// Compare to TestCnsParsingClearedWithCargoSplit() where the MUCR on the job is ELF1A001200100 (ie. trimmed)
			SetUpXmlAndDec(xml);
			declaration.JE_MasterUCR = "ELF1A001200100";
			GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RunAndReload(Factory);
			AssertEquals("Rego demand a MATCH, job is updated", "CLR", cusEntryHeader.CH_EntryStatus);
		}

		[TestDate(2009, 12, 11)]
		public void TestXmlCnsParsingClearedWithCargoSplit()
		{
			string xml = @"
								<CNScargoStatus>
										<MessageHeader>
											<HeaderID>101112131415</HeaderID>
											<MessageType>CLEARED</MessageType>
										</MessageHeader>
									<MessageDetail>
										<Site>ELF1</Site>
										<Container>ELFT9021204</Container>
										<UCN>ELF1A001200109</UCN>
										<Clearance>CL</Clearance>
										<Packages>10</Packages>
										<Weight>24350</Weight>
									</MessageDetail>
								</CNScargoStatus>

								";

			SetUpXmlAndDec(xml);
			declaration.JE_MasterUCR = "ELF1A001200109";
			declaration.Factory.Save();
			RunAndReload(Factory);

			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var x = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertContains("<h2>Customs CLEARED advice for B000069 / 191-69696969</h1> <h2>UCN=ELF1A001200109</h2>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("<td>CLEARED</td></tr><tr><td>Advice datetime (header ID)</td><td>12-Nov-10 13:14:15</td></tr><tr><td>UCN</td><td>ELF1A001200109</td></tr><tr><td>Container</td><td>ELFT9021204</td></tr><tr><td>BoL</td><td>&nbsp;</td></tr><tr><td>Clearance code</td><td>CL</td></tr><tr><td>Hold type</td><td>&nbsp;</td></tr><tr><td>NoP</td><td>10</td></tr><tr><td>Weight</td><td>24350</td></tr><tr><td>Site</td><td>ELF1</td></tr></table>",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("Customs CLEARED advice for  B000069 / 191-69696969 / UCN=ELF1A001200109", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			ZQuery q = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CnsCompassServiceTask.Code);
			q.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] messages = Factory.Load<EDIMessage>(q);
			AssertEquals(1, messages.Length);

			AssertNotEquals(EDIMessage.Status.Queued, messages[0].EM_Status);
			AssertContains("<UCN>ELF1A001200109</UCN>", messages[0].EM_MessageText);

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			IeDoc eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Clearance advice (XML) from CNS", eDoc.Description);
			AssertEquals("eDoc.DocType", Core.Constants.RefDocTypes.ClearanceAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", "Clearance advice 191-69696969.CnsXml.xml", eDoc.FileName);
		}

		[TestDate(2009, 12, 11, 09, 56, 01)]
		public void TestXmlCnsParsingUnknownJob()
		{
			string xml = @"<CNScargoStatus>
										<MessageHeader>
											<HeaderID>101112131415</HeaderID>
											<MessageType>CLEARED</MessageType>
										</MessageHeader>
									<MessageDetail>
										<Site>ELF1</Site>
										<Container>ELFT9021204</Container>		
										<UCN>My cat's breath smells like cat food</UCN>
										<Clearance>CL</Clearance>
										<Packages>10</Packages>
										<Weight>24350</Weight>
									</MessageDetail>
								</CNScargoStatus>";

			SetUpXmlAndDec(xml);
			RunAndReload(Factory);

			AssertEquals("", cusEntryHeader.CH_EntryStatus);
			AssertEquals("PRS", mailItem.MI_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			AssertContains("A clearance advice email was received from CNS, but the job could not be found. The entry number in the message did not match any entry in your system.  The advice message is attached.",
				Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertEquals(3, Env.OutgoingCustomsMailManager.EmailsCreated[0].Attachments.Count);  // two image, one payload

			ArrayList arrayList = new ArrayList(Env.OutgoingCustomsMailManager.EmailsCreated[0].Attachments);
			AttachmentDef[] atts = (AttachmentDef[])arrayList.ToArray(typeof(AttachmentDef));
			AttachmentDef xmlAttachment = Array.Find(atts, x => !x.DisplayName.Contains(".jpg"));
			AssertEquals("Invalid CnsXml 2009-12-11 095601.xml", xmlAttachment.DisplayName);
			byte[] data = xmlAttachment.Data;
			AssertContains("<UCN>My cat's breath smells like cat food</UCN>", BytesToString(data));

			AssertContains("Customs clearance advice for UCN=My cat's breath smells like cat food - entry and job not found", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			ZQuery q = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CnsCompassServiceTask.Code);
			q.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] messages = Factory.Load<EDIMessage>(q);
			AssertEquals(1, messages.Length);

			AssertEquals(EDIMessage.Status.Failed, messages[0].EM_Status);
			AssertContains("<UCN>My cat's breath smells like cat food</UCN>", messages[0].EM_MessageText);

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 0, docManagerInfo.Files.Count);
		}

		MailItem mailItem;
		JobDeclaration declaration;
		CusEntryHeader cusEntryHeader;

		string BytesToString(byte[] bytes)
		{
			ASCIIEncoding encoding = new ASCIIEncoding();
			return encoding.GetString(bytes);
		}

		void RunAndReload(BusinessObjectFactory factory)
		{
			ErrorReporter.Clear();

			// Here is the main work:
			CnsCompassServiceTask serviceTask = new CnsCompassServiceTask();
			factory.Save();

			TestServiceLogger log = InitialiseAndRunTaskSchedule(serviceTask);

			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			mailItem.Reload();
		}

		protected void SetUpEmailGroup()
		{
			var customsNotificationGroup = Factory.New<GlbGroup>();
			customsNotificationGroup.GG_Code = "ZZZ";
			var user1 = customsNotificationGroup.Staff.AddNew();
			var user2 = customsNotificationGroup.Staff.AddNew();
			user1.GS_Code = "XXX";
			user1.GS_LoginName = "XXX";
			user1.GS_EmailAddress = "m.mouse@disney.com";
			user2.GS_Code = "YYY";
			user2.GS_LoginName = "YYY";
			user2.GS_EmailAddress = "d.duck@disney.com";
			Factory.Save();
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsNotificationGroup.PK.ToGuid());
		}

		protected void SetupReceivedEmail(string attachmentText)
		{
			mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "King Ninad I <ninad@cnsonline.net>";
			mailItem.MI_Subject = "X400123456789";
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.MI_Header = "Daniel Rocks: Yes" + System.Environment.NewLine + mailItem.MI_Header;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii(attachmentText);
			attachment.MA_FileName = "filename.anything";
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbCNS);
		}

		void SetUpXmlAndDec(string inboundXmlFromCns)
		{
			SetUpEmailGroup();
			SetupReceivedEmail("<?xml version=\"1.0\"?>  " + inboundXmlFromCns);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MasterUCR = "ELF1A0012001";  // According to Elite and CNS both, when the UCN ends in "00" then the MUCR should not contain this. So here the UCN is "really" ELF1A001200100 (as will be returned in the XML) but the broker will supply ELF1A0012001 on the dec. 
			declaration.JE_DeclarationReference = "B000069";
			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_EntrySubmittedDate = ZDateTime.Now.AddMinutes(-60);
			cusEntryHeader.CH_MessageType = "GBG";
			cusEntryHeader.EntryNumber = "191-69696969";
			cusEntryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
			Factory.Save();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"UK Customs Compass mail inbound",
						MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbCNS,
						MailDBItemsSchema.Constants.MI_Status + "=" + StatusCodeList.Codes.Queued,
						MailDBItemsSchema.Constants.MI_Direction + "=" + DirectionList.Codes.Receive),
				};
			}
		}
	}
}
