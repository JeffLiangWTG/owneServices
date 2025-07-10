using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Chief.NES.Testing
{
	class NesEmailReceiverTests : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22, 14, 01, 00)]
		public void TestWtgEdcsAlert()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Nobody@notOurDomain.com";
			mailItem.MI_Subject = NesConstants.WtgAlert + " " + NesEmailReceiver.WtgAlertTypes.SET + " " + ZDateTime.BrettsBirthday.ToString("yyyy-MM-dd HH:mm");
			mailItem.MI_Body = @"This is a note form DJC telling you that EDCS is down.\r\nWith two lines";
			var receiver = new NesEmailReceiverForTest();
			receiver.GetInterchangeTextForTest(mailItem, false);
			AssertEquals(MailStatus.Processed, mailItem.MI_Status);
			AssertEquals(ZDateTime.Empty, GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.Value);
			AssertEquals("", GBCustomsDataRegistry.Instance.EdcsWtgAlertString.Value);

			mailItem.MI_From = "Authorised Wtg User <trusted.user@wisetechglobal.com>";
			mailItem.MI_Status = MailStatus.Queued;
			receiver = new NesEmailReceiverForTest();
			receiver.GetInterchangeTextForTest(mailItem, false);
			AssertEquals(MailStatus.Processed, mailItem.MI_Status);
			AssertEquals(ZDateTime.BrettsBirthday, GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.Value);
			AssertEquals(mailItem.MI_Body, GBCustomsDataRegistry.Instance.EdcsWtgAlertString.Value);

			mailItem.MI_Subject = NesConstants.WtgAlert + " " + NesEmailReceiver.WtgAlertTypes.UNSET;
			mailItem.MI_Status = MailStatus.Queued;
			receiver = new NesEmailReceiverForTest();
			receiver.GetInterchangeTextForTest(mailItem, false);
			AssertEquals(MailStatus.Processed, mailItem.MI_Status);
			AssertEquals(ZDateTime.Empty, GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.Value);
			AssertEquals("", GBCustomsDataRegistry.Instance.EdcsWtgAlertString.Value);

			mailItem.MI_Subject = NesConstants.WtgAlert + " " + NesEmailReceiver.WtgAlertTypes.SET + " 1234-Nonsense-Date-567";
			mailItem.MI_Status = MailStatus.Queued;
			receiver = new NesEmailReceiverForTest();
			receiver.GetInterchangeTextForTest(mailItem, false);
			AssertEquals(MailStatus.Processed, mailItem.MI_Status);
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 01, 00), GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.Value);
			AssertEquals(mailItem.MI_Body, GBCustomsDataRegistry.Instance.EdcsWtgAlertString.Value);
		}

		public void TestAcknowledgement()
		{
			RunTest("Acknowledgement.txt", "Acknowlegement text", @"Your message to
was received at 24 Aug 2012 08:52:08 +0100

This notification was generated
Manually
The following extra information was given:
d73a91d8-5bf6-4e4f-8a97-76dad60d31e1

", "NESACK");
		}

		public void TestQuotedPrintableSolicitedResponse()
		{
			// Ot's OK to see newline characters... Enterprise.Messaging.Biz cleans these up for us. 
			RunTest("QuotedPrintable.txt",
					"Response emails received in quoted-printtable format with = signs in body should be understood",
@"UNB+UNOA:2+EDRCHIEF+THSAZCJ::LOCEDCAZCJ+100921:1211+0010672810+18+CHIEFLIVE'U
NH+09695382942520+CUSRES:D:04A:UN:109730+6A5F56E702444A068EDB09B40400934D'BGM
+EFD::109++27'RFF+ABO:0GB945390992000-B00010282'ERP+:0+CST:3:5'ERC+6::109'FTX
+AAO+++E2929 CONSIGNOR TIDS MUST BE ENTERED AT EITHER ENTRY OR ITEM LEVEL
NOT'ERP+:0+CST:3:5'ERC+6::109'FTX+AAO+++E468 DECLARANT MUST BE
PRESENT'ERP+:5:1+NAD:14:5'ERC+6::109'FTX+AAO+++E2369 CONSIGNOR DETAILS MUST
NOT BE SUPPLIED WITHOUT A CORRESPONDING
T'DOC+960+B00010282'UNT+14+09695382942520'UNZ+1+0010672810'",
					"NESRES");
		}

		public void TestResentReport()
		{
			RunTest("Base64ReportResent.txt",
					"Report emails that have a subject line containing 'resent' should also be processed",
					"UNB+UNOA:2+EDRCHIEF+THS1ZEG::LOCEDC1ZEG+100917:1012+DTICHIEFEDI'UNH+09691855362728+CUSRES:D:04A:UN:109790'BGM+RPA::109:DTI-P2++6'LOC+43+555::109'NAD+DT+GB945390992000'NAD+PB+945390992000::109'NAD+CZ+GB945390992000++CARGOWISE'RFF+ABT:555-A00504R-17/09/2010:01'RFF+ACD:FDE'RFF+TN:THS1ZEG'RFF+ABO:0GB945390992000-B10010277:H'RFF+AAE:10GB09X32503673019'RFF+ABS:A1'RFF+AHZ:1:H'DOC+960+B00010277'MOA+123:500.00'CST+1'TAX+5'MOA+123:500.00'CNT+11:1'UNT+20+09691855362728'UNZ+1+DTICHIEFEDI'",
					"NESRES");
		}

		public void TestEmbeddedBase64Report()
		{
			RunTest("Base64Report.txt",
					"Report emails with a b64 'embedded' attachment, NOT as a .bin file, should be understood",
					"UNB+UNOA:2+EDRCHIEF+THS1ZEG::LOCEDC1ZEG+100917:1012+DTICHIEFEDI'UNH+09691855362728+CUSRES:D:04A:UN:109790'BGM+RPA::109:DTI-P2++6'LOC+43+555::109'NAD+DT+GB945390992000'NAD+PB+945390992000::109'NAD+CZ+GB945390992000++CARGOWISE'RFF+ABT:555-A00504R-17/09/2010:01'RFF+ACD:FDE'RFF+TN:THS1ZEG'RFF+ABO:0GB945390992000-B10010277:H'RFF+AAE:10GB09X32503673019'RFF+ABS:A1'RFF+AHZ:1:H'DOC+960+B00010277'MOA+123:500.00'CST+1'TAX+5'MOA+123:500.00'CNT+11:1'UNT+20+09691855362728'UNZ+1+DTICHIEFEDI'",
					"NESRES");
		}

		public void TestRegularBinAttachedReport()
		{
			RunTest("BinReport.txt",
				"Report emails with proper .bin attachments",
				"UNB+UNOA:2+EDRCHIEF+THSAZCJ::LOCEDCAZCJ+100921:1216+DTICHIEFEDI'UNH+09695386831195+CUSRES:D:04A:UN:109790'BGM+RPA::109:DTI-P2++6'LOC+43+555::109'NAD+DT+GB945390992000'NAD+PB+945390992000::109'NAD+CZ+GB945390992000++CARGOWISE EDI (UK)'RFF+ABT:555-A80619R-21/09/2010:01'RFF+ACD:FDE'RFF+TN:THSAZCJ'RFF+ABO:0GB945390992000-B00010282:Z'RFF+AAE:10GB09X69272562010'RFF+ABS:A1'RFF+AHZ:1:H'DOC+960+B00010282'MOA+123:500.00'CST+1'TAX+5'MOA+123:500.00'CNT+11:1'UNT+20+09695386831195'UNZ+1+DTICHIEFEDI'",
				"NESRES");
		}

		public void TestExportAccompanyingDocument()
		{
			RunTest("ExportAccompanyingDocumentEmail.txt",
				"Report emails with proper .dat attachments but poor subject line",
				"UNB+UNOA:2+EDRCHIEF+THSAIBL::LOCEDCAIBL+120824:1012+DTICHIEFEDI'UNH+10302703305306+CUSDEC:D:04A:UN:109793'BGM+ACD::109++6'CST++EX'LOC+36+MA'LOC+35+GB'LOC+114+GB000051'LOC+115+BE101000'DTM+7:20120724:102'DTM+268:20121123:102'FTX+AJA++A2'RFF+ABO:2GB399426986000-SLDSSE00001630:P'RFF+AAE:12GB07X30120052012'TDT+13++1'TDT+1+++++++:::BF EUPHORIA+EN'NAD+CZ+FR18383184728++UNITED PHARMACEUTICALS+55 AVENUE HOCHE+PARIS+EN+75008+FR'NAD+CN+++LABORATOIRES SOTHEMA+BP1+BOUSKOURA+EN+20180+MA'NAD+DT+GB399426986000++FUTURE FORWARDING+UNIT 4 HAWTHORNE HOUSE+WEST YORKSHIRE+EN+WF17 9LW+GB'UNS+D'CST+1+1000001+19011000'FTX+ACB++LIC99'MEA+AAR++KGM:7329.600'PAC+20++CR'PCI++LABORATOIRES SOTHEMA+EN'MOA+123:34214.34'RFF+ZZZ'IMD+++:::BABY MILK POWDER::EN'DOC+998:::380+6917:::EN'UNS+S'CNT+5:1'UNT+30+10302703305306'UNZ+1+DTICHIEFEDI'",
				"NESRES");
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]  // freeze the clock to get the same ticks each time
		public void TestReceivedIterchangeAndMessageStoredAgainstCorrectCompanyBasedOnReceipientRole()
		{
			// Don't change environemtn company context duriong this test, yet check that message comes out under right branch still.
			var branchOne = Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory, "ONE");
			var branchTwo = Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory, "TWO");
			var badgeOne = new BadgeCodeSetting();
			badgeOne.CSPCode = GatewayList.Codes.NES;
			badgeOne.BadgeCode = GatewayList.Codes.NES;
			var badgeTwo = new BadgeCodeSetting();
			badgeTwo.CSPCode = GatewayList.Codes.NES;
			badgeTwo.BadgeCode = GatewayList.Codes.NES;
			var badgeCollectionOne = new BadgeCodeSettingCollection();
			var badgeCollectionTwo = new BadgeCodeSettingCollection();
			badgeCollectionOne.Add(badgeOne);
			badgeCollectionTwo.Add(badgeTwo);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, branchOne.PK.ToGuid(), Guid.Empty, badgeCollectionOne);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, branchTwo.PK.ToGuid(), Guid.Empty, badgeCollectionTwo);
			using (DisposableEnvironment.ForBranch(branchOne.PK.ToGuid()))
			{
				var credentialOne = new CredentialsSetting();
				credentialOne.NesRole = "THS1ONE";
				credentialOne.NesLocation = "LOCEDCONE";
				credentialOne.BadgeCode = badgeOne.BadgeCode;
				var credentialSettingCollectionOne = new CredentialsSettingCollection();
				credentialSettingCollectionOne.Add(credentialOne);
				GBCustomsDataRegistry.Instance.Credentials.SetValue(branchOne.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, credentialSettingCollectionOne);
			}
			using (DisposableEnvironment.ForBranch(branchTwo.PK.ToGuid()))
			{
				var credentialTwo = new CredentialsSetting();
				credentialTwo.NesRole = "THS1TWO";
				credentialTwo.NesLocation = "LOCEDCTWO";
				credentialTwo.BadgeCode = badgeTwo.BadgeCode;
				var credentialSettingCollectionTwo = new CredentialsSettingCollection();
				credentialSettingCollectionTwo.Add(credentialTwo);
				GBCustomsDataRegistry.Instance.Credentials.SetValue(branchTwo.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, credentialSettingCollectionTwo);
			}
			var interchangeReportTextOne = "UNB+UNOA:2+EDRCHIEF+THS1" + "ONE" + "::LOCEDC1ZEG+100917:1012+DTICHIEFEDI'UNH+MESSAGE1+CUSRES:D:04A:UN:109790'BGM+RPA::109:DTI-P2++6'LOC+43+555::109'NAD+DT+GB945390992000'NAD+PB+945390992000::109'NAD+CZ+GB945390992000++CARGOWISE'RFF+ABT:555-A00504R-17/09/2010:01'RFF+ACD:FDE'RFF+TN:THS1ZEG'RFF+ABO:0GB945390992000-B10010277:H'RFF+AAE:10GB09X32503673019'RFF+ABS:A1'RFF+AHZ:1:H'DOC+960+B00010277'MOA+123:500.00'CST+1'TAX+5'MOA+123:500.00'CNT+11:1'UNT+20+09691855362728'UNZ+1+DTICHIEFEDI'";
			var interchangeReportTextTwo = "UNB+UNOA:2+EDRCHIEF+THS1" + "TWO" + "::LOCEDC1ZEG+100917:1012+DTICHIEFEDI'UNH+MESSAGE2+CUSRES:D:04A:UN:109790'BGM+RPA::109:DTI-P2++6'LOC+43+555::109'NAD+DT+GB945390992000'NAD+PB+945390992000::109'NAD+CZ+GB945390992000++CARGOWISE'RFF+ABT:555-A00504R-17/09/2010:01'RFF+ACD:FDE'RFF+TN:THS1ZEG'RFF+ABO:0GB945390992000-B10010277:H'RFF+AAE:10GB09X32503673019'RFF+ABS:A1'RFF+AHZ:1:H'DOC+960+B00010277'MOA+123:500.00'CST+1'TAX+5'MOA+123:500.00'CNT+11:1'UNT+20+09691855362728'UNZ+1+DTICHIEFEDI'";
			var interchangeContrlOne = "UNB+UNOA:2+EDRCHIEF+THS1" + "ONE" + "::LOCXML1ZEG+090918:1606+0009662587+15+CHIEFLIVE'UNH+MESSAGE3+CONTRL:2:2:UN'UCI+1+THS1ZEG::LOCXML1ZEG+EDRCHIEF+4+G46'UNT+3+090918160656'UNZ+1+0009662587'";
			var interchangeCusResTwo = "UNB+UNOA:2+EDRCHIEF+THS1" + "TWO" + "::LOCEDCAZCJ+090923:1021+0009670646+169+CHIEFLIVE'UNH+MESSAGE4+CUSRES:D:04A:UN:109730'BGM+EFD::109++29'DTM+7:200908141636:203'LOC+44+071::109'RFF+ABT:A00528L:1'RFF+ABO:9GB945390992000-B00152488:R'RFF+AAE:09GB08X32343757015'RFF+ABS:A1'RFF+AHZ:1:H'DOC+960+B00152488'MOA+40:10.00'MOA+55:0.00'MOA+9:0.00'MOA+74:0.00'MOA+176:0.00'CST+1'TAX+5'MOA+123:10.00'UNT+19+09081416363422'UNZ+1+0009662587'";

			SetUpEmail(interchangeReportTextOne);
			SetUpEmail(interchangeReportTextTwo);
			SetUpEmail(interchangeContrlOne);
			SetUpEmail(interchangeCusResTwo);
			Factory.Save();
			var receiver = new NesEmailReceiver(null);
			receiver.ExecuteBatch();
			var messageReportOne = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "MESSAGE1"));
			var messageReportTwo = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "MESSAGE2"));
			var messageControlOne = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "MESSAGE3"));
			var messageCusresTwo = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "MESSAGE4"));
			AssertEquals(branchOne.PK, messageReportOne.EM_GB);
			AssertEquals(branchTwo.PK, messageReportTwo.EM_GB);
			AssertEquals(branchOne.PK, messageControlOne.EM_GB);
			AssertEquals(branchTwo.PK, messageCusresTwo.EM_GB);

			AssertNotEquals("Interchange number doesn't match even when the timestamp is identical (due to [TestDate] attribute)", messageReportOne.Interchange.EI_InterchangeNum, messageReportTwo.Interchange.EI_InterchangeNum);
			AssertEquals("Interchange should have a number that comprises the message number, a fixed, trimmed timestamp, then part of a GUID", true, System.Text.RegularExpressions.Regex.IsMatch(messageReportOne.Interchange.EI_InterchangeNum, @"MESSAGE1_635758488_[A-F0-9a-f-].*"));
			AssertEquals("Interchange should have a number that comprises the message number, a fixed, trimmed timestamp, then part of a GUID", true, System.Text.RegularExpressions.Regex.IsMatch(messageReportTwo.Interchange.EI_InterchangeNum, @"MESSAGE2_635758488_[A-F0-9a-f-].*"));
		}

		void SetUpEmail(string interchangeText)
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Header = @"from: 'stest@smtptest.hmce.gov.uk' <stest@smtptest.hmce.gov.uk>
to: Cargowise Test <cargowise.test@5-1-2.com>
date: Tue, 21 Sep 2010 12:11:38 +0100
subject: Anything";
			mailItem.MI_Header = interchangeText;
			mailItem.MI_Body = interchangeText;
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.ExtractAttachments();
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbChiefNesEmail);
		}

		void RunTest(string testFileName, string commentForAssertion, string expectedEdifact, string expectedMessageTypeAndSubtype)
		{
			string twoNewLines = System.Environment.NewLine + System.Environment.NewLine;
			string wholeFile = "";
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.GB.Chief.Testing.NES." + testFileName))
			{
				wholeFile = new StreamReader(stream).ReadToEnd();
			}
			string emailHeader = System.Text.RegularExpressions.Regex.Split(wholeFile, twoNewLines)[0];
			string emailBody = wholeFile.Replace(emailHeader + twoNewLines, "");
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Body = emailBody;
			mailItem.MI_Header = emailHeader;
			mailItem.ExtractAttachments();
			NesEmailReceiverForTest receiver = new NesEmailReceiverForTest();
			var actualExtractedEdifact = receiver.GetInterchangeTextForTest(mailItem, false);
			AssertEquals(commentForAssertion, expectedEdifact, actualExtractedEdifact);
			var interchange = receiver.CreateInterchangeAndMessagesForTest(Factory, actualExtractedEdifact, mailItem);
			AssertEquals(1, interchange.ContainedMessages.Count);
			var message = interchange.ContainedMessages[0];
			AssertEquals("Application code of message created", "NES", message.EM_ApplicationCode);
			AssertEquals("Type/subtype of message created", expectedMessageTypeAndSubtype, message.EM_MessageType + message.EM_MessageSubType);
		}
	}

	class NesEmailReceiverForTest : NesEmailReceiver
	{
		public NesEmailReceiverForTest() : base(new TestServiceLogger()) { }

		public ZString GetInterchangeTextForTest(MailItem emailItem, bool processInNewThread)
		{
			return base.GetInterchangeText(emailItem, processInNewThread);
		}

		public EDIInterchange CreateInterchangeAndMessagesForTest(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			return base.CreateInterchangeAndMessages(factory, interchangeString, mailItem);
		}
	}
}
