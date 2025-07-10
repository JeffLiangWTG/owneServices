using System.Data;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CONTRLMessageProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestMultiMessageInterchangeAndCTLClonedToJob()
		{
			var interchange = EDIInterchange.New(Factory);
			interchange.EI_From = "AAA374M";
			interchange.EI_To = "AAA336C";
			interchange.EI_Priority = "HGH";
			interchange.EI_Status = "RCV";
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			interchange.EI_InterchangeNum = "8445";
			interchange.EI_HeaderText = "UNA:+.? 'UNB+UNOC:3+AAA374M::AAA374M+AAA336C+111020:1855+8445++++1++1'";
			interchange.EI_FooterText = "UNZ+3+8445'";

			var job1 = Factory.New<JobDeclaration>();
			var entry1 = job1.CustomsEntryHeaders.AddNew();
			var message1 = entry1.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message1.EM_EI = interchange.PK;
			message1.EM_MessageText = @"UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00001342/2/DAT1:4+4'CST++N10::95'LOC+8+AUMEL::6'LOC+12+AUSYD::6'LOC+9+GBLHR::6'LOC+79+AUSYD::6'DTM+178:20110716:102'DTM+260:20110715:102'" +
				"DTM+252:20110716:102'GIS+Y:153:95'GIS+POR:109:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:20.00000'FTX+DEL+++DELIVERY NAME'FTX+CHG+++XXX'RFF+ABQ:234983549785'RFF+ADU:                  AP'RFF+APH:FOB'RFF+AMG:0011'" +
				"RFF+ABT:AAACPAPYG'TDT+20++A++QF::3'NAD+AT+66015286036::95'NAD+VT+AA33HF::95'NAD+DP++MASCOT++831 BOTANY ROAD++:::NSW+2020+AU'NAD+CB+54321::95'MOA+63:2000.00:USD'MOA+141:1861.60:AUD'MOA+39:2000.00:USD'" +
				"MOA+68:2.34:AUD'MOA+313:1.42:AUD'MOA+71:1.00:USD'UNS+D'DMS+1'LIN+1+A'PAC+1+1'PCI+1'RFF+MWB:08126255552'PCI+1'RFF+HWB:22625535'CST+1+A::95+N10::95'FTX+AAA+++ABCDEFGHIJK'LOC+27+DE::5'MEA+AAA++NO:1.00000'" +
				"NAD+SU+AAA3336646Y::95'MOA+38:1000.00:USD'MOA+68:0.51:AUD'RFF+ABD:73182200'RFF+AED:05'RFF+AGW:GEN'RFF+AWA:001'RFF+AFV:IG'GIS+REL:109:95'CST+2+A::95+N10::95'FTX+AAA+++ABCDEFGHIJK'LOC+27+DE::5'MEA+AAA++NO:1.00000'" +
				"NAD+SU+AAA3336646Y::95'MOA+38:1000.00:USD'MOA+68:1.83:AUD'RFF+ABD:73182200'RFF+AED:05'RFF+AGW:GEN'RFF+AWA:001'RFF+AFV:IG'GIS+REL:109:95'UNS+S'UNT+69+1'";
			message1.EM_MessageNum = "1";

			var job2 = Factory.New<JobDeclaration>();
			var entry2 = job2.CustomsEntryHeaders.AddNew();
			var message2 = entry2.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message2.EM_EI = interchange.PK;
			message2.EM_MessageText = @"UNH+2+CUSDEC:D:99B:UN'BGM+929:::IMD+B00001339/3/DAT1:13+4'" +
				"CST++N10::95'LOC+8+AUMEL::6'LOC+12+AUMEL::6'LOC+9+GBLON::6'LOC+79+AUMEL::6'DTM+178:20110302:102'DTM+260:20110301:102'DTM+252:20110302:102'GIS+Y:153:95'GIS+POR:109:95'GIS+LLB:109:95'GIS+TLB:109:95'" +
				"FII+COQ+323232+:::242200::215'MEA+AAE+G+KG:100.00000'FTX+DEL+++DELIVERY NAME'FTX+CHG+++XXX'RFF+ABQ:225444'RFF+ADU:B00001339/3'RFF+ANU:B'RFF+APH:FOB'RFF+ADP:0014::N'RFF+AMG:0011'RFF+ADP:0015::Y'RFF+ABT:AAACNFYHJ'" +
				"TDT+20++A++QF::3'NAD+AT+66015286036::95'NAD+VT+AA33HF::95'NAD+DP++MASCOT++831 BOTANY ROAD++:::NSW+2020+AU'NAD+CB+54321::95'MOA+63:5000.00:USD'MOA+141:5100.00:USD'MOA+39:5000.00:USD'MOA+313:50.00:USD'" +
				"MOA+71:50.00:USD'UNS+D'DMS+1'LIN+1+A'PAC+10+1'PCI+1'RFF+MWB:08165281123'PCI+1'RFF+HWB:33298423636'CST+1+A::95+N10::95'FTX+AAA+++ABCDEFGHIJK'LOC+27+DE::5'MEA+AAA++NO:5.00000'NAD+SU+AAA3336646Y::95'" +
				"MOA+38:1000.00:USD'RFF+ABD:73182200'RFF+AED:05'RFF+AGW:GEN'RFF+AWA:001'RFF+AFV:IG'GIS+REL:109:95'CST+2+I::95+N10::95'FTX+AAA+++METAL FURNITURE OF A KIND USED IN OFFICES'LOC+27+DE::5'MEA+AAA++NO:10000000000.00000'" +
				"NAD+SU+AAA3336646Y::95'PAC++1'PCI+1'FTX+RAH+++00147:00162:N'PCI+1'FTX+RAH+++00314:00313:N'MOA+38:4000.00:USD'RFF+ABD:94031000'RFF+AED:40'RFF+AGW:GEN'RFF+AWA:001'RFF+AFV:IG'GIS+REL:109:95'UNS+S'UNT+75+2'";
			message2.EM_MessageNum = "2";

			var job3 = Factory.New<JobDeclaration>();
			var entry3 = job3.CustomsEntryHeaders.AddNew();
			var message3 = entry3.Messages.AddNew();
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message3.EM_EI = interchange.PK;
			message3.EM_MessageText = @"UNH+3+CUSDEC:D:99B:UN'BGM+929:::IMD+B00001147/6/DAT2:7+4'CST++N10::95'LOC+8+AUMEL::6'LOC+12+AUMEL::6'LOC+9+GBLON::6'LOC+79+AUMEL::6'DTM+178:20091111:102'DTM+260:20091109:102'DTM+252:20091111:102'GIS+Y:153:95'" +
				"GIS+POR:109:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:100.00000'FTX+DEL+++TREETOYS PTY LTD'FTX+CHG+++XXXXXXXXX'FTX+ABA++ORIGIN::95+THI IS THE AMBER STATEMENT. 'RFF+ABQ:2636'RFF+ADU:                    '" +
				"RFF+APH:FOB'RFF+ADP:0009::Y'RFF+AMG:0011'RFF+ADP:0008::Y'RFF+ABT:AAACKJWWY'TDT+20+1221+S+++++7631456::11'NAD+AT+66015286036::95'NAD+VT+AA33HF::95'NAD+DP++BRISBANE++SOMEWHERE IN BNE++:::QLD+7001+AU'NAD+CB+54321::95'" +
				"MOA+63:2000.00:AUD'MOA+141:2250.00:AUD'MOA+39:2000.00:AUD'MOA+313:150.00:AUD'MOA+71:100.00:AUD'UNS+D'DMS+1'LIN+1+A'PAC+++LCL:67:95'PAC+100+1'PCI+1'RFF+AAQ:OCLU1113330'PCI+1'RFF+MB:162165233'PCI+1'RFF+BH:3636B'" +
				"CST+1+A::95+N10::95'FTX+AAA+++ABCDEFGHIJK'LOC+27+CA::5'MEA+AAA++NO:100.00000'NAD+SU+AAA3336646Y::95'MOA+38:2000.00:AUD'RFF+ABD:73182200'RFF+AED:05'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:OCLU1113330'RFF+AFV:IG'" +
				"GIS+REL:109:95'UNS+S'UNT+61+3'";
			message3.EM_MessageNum = "3";

			interchange.EI_BodyText = message1.EM_MessageText + message2.EM_MessageText + message3.EM_MessageText;

			var message = Factory.New<CMRCONTRLMessage>();
			message.EM_MessageText = "UNH+000001+CONTRL:D:3:UN'UCI+8445+AAA374M::AAA374M+AAA336C+7'UCM+1+CUSDEC:D:99B:UN+7'UCM+2+CUSDEC:D:99B:UN+4+18'UCS+60+18'UCD+12+4:2'UCM+3+CUSDEC:D:99B:UN+7'UNT+8+000001'";
			processor.ProcessMessage(message);

			AssertNotEquals("job1-no syntax error", EDIMessage.Status.Rejected, message1.EM_Status);
			AssertEquals("job1-only the outgoing message", 1, entry1.Messages.Count);
			AssertEquals("job2-syntax error", EDIMessage.Status.Rejected, message2.EM_Status);
			AssertEquals("job2-extra message", 2, entry2.Messages.Count);
			AssertEquals("job2-is cloned CTL", CMRMessage.CMRMessageTypes.CONTRL, entry2.Messages[1].EM_MessageType);
			AssertNotEquals("job3-no syntax error", EDIMessage.Status.Rejected, message3.EM_Status);
			AssertEquals("job3-only the outgoing message", 1, entry3.Messages.Count);
		}

		public void TestDuplicateInterchangeCONTRLMessagePreservesInterchangeStatus()
		{
			string cONTRLMessageText = "UNH+000001+CONTRL:D:3:UN'UCI+104715+AAA374M::AAA374M+AAA336C+4+26'UNT+3+000001'";
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = cONTRLMessageText;

			interchange.EI_Status = EDIInterchange.Status.eHubPending;
			AssertEquals("Precondition, ShouldSendViaEHub", true, interchange.ShouldSendViaEHub);

			processor.ProcessMessage(message);
			AssertEquals("Interchange EI_Status", EDIInterchange.Status.eHubPending, interchange.EI_Status);
		}

		public void TestProcessCONTRLRejectionMessage()
		{
			string cONTRLMessageText = "UNH+000001+CONTRL:D:3:UN'UCI+104715+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSDEC:D:99B:UN+4+18'UCS+10'UCD+13+4:1'UNT+6+000001'";
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = cONTRLMessageText;

			interchange.EI_Status = EDIInterchange.Status.eHubPending;
			AssertEquals("Precondition, ShouldSendViaEHub", true, interchange.ShouldSendViaEHub);

			processor.ProcessMessage(message);
			AssertEquals("Interchange EI_Status", EDIInterchange.Status.eHubPending, interchange.EI_Status);

			var originalMessage = interchange.ContainedMessages[0];
			AssertEquals("Original Message EM_Status", EDIMessage.Status.Rejected, originalMessage.EM_Status);
		}

		public void TestCancelPendingMessagesFromCONTRLRejection()
		{
			var interchange = Factory.New<CMRInterchange>();
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "AAA336C";
			interchange.EI_From = "AAA374M";
			interchange.EI_InterchangeNum = "382591";
			interchange.EI_Status = EDIMessage.Status.Sent;
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			interchange.EI_InterchangeType = "SUT";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_HeaderText = "";
			interchange.EI_BodyText = @"Mime-Version: 1.0  Content-Transfer-Encoding: base64  Content-Type: application/pkcs7-mime; smime-type=signed-data; name = ""smime.p7m""    MIIMOwYJKoZIhvcNAQcCoIIMLDCCDCgCAQExDjAMBggqhkiG9w0CBQUAMIIE8QYJKoZIhvcNAQcB  oIIE4gSCBN5NaW1lLVZlcnNpb246IDEuMA0KQ29udGVudC1UcmFuc2Zlci1FbmNvZGluZzogYmFz  ZTY0DQpDb250ZW50LVR5cGU6IGFwcGxpY2F0aW9uL2VkaWZhY3Q7IG5hbWU9QUFBMzc0TV8zODI1  OTEuZWRpDQpDb250ZW50LURpc3Bvc2l0aW9uOiBhdHRhY2htZW50OyBmaWxlbmFtZT1BQUEzNzRN  XzM4MjU5MS5lZGkNCg0KVlU1Qk9pc3VQeUFuVlU1Q0sxVk9UME02TXl0QlFVRXpOelJOT2pwQlFV  RXpOelJOSzBGQlFUTXpOa01yTVRnd05qQXhPakUwTVRZcg0KTXpneU5Ua3hLeXNyS3pFckt6RW5W  VTVJS3pFclExVlRRMEZTT2tRNk9UbENPbFZPSjBKSFRTc3lOak02T2pwVFJVRlBWVlFyVHpBdw0K  TURBd05qSXpMME5OVkRJNk9TczBKMDVCUkN0V1Z5czBNVEEyTlRnNU5EY3lORG82T1RVblZFUlVL  ekl3S3pNeE1EVlFWeXNyTVRFcg0KS3lzck9UQTBORGMwT0RvNk1URW5URTlES3pRck9Ua3hORTQ2  T2prMUowTk9TU3NyT2pvNlJDZFNSa1lyUVVGUk9reERURlUyTWpjMg0KTWpjMkowZEpSQ3N4SjFK  R1JpdEJRMVU2VGtsTUowZEpSQ3N4SjFKR1JpdENTRHBOV1VKSFNEY25SMGxFS3pFblVrWkdLMDFD  T2sxWg0KUWtsSFFURW5SMGxUSzA0Nk5qSTZPVFVuUjBsVEsxVTZOak02T1RVblIwbFRLMVU2TnpF  Nk9UVW5SMGxUSzA0Nk1UZzJPamsxSjBkSg0KVXl0T09qRTRPRG81TlNkVVJGUXJNU2RFVkUwck5E  SXdPakl3TVRnd05qQXhPakV3TWlkRVZFMHJOREl3T2pBek16UTZOREF4SjBSVQ0KVFNzMU56QTZN  akF4T0RBMk1ERTZNVEF5SjBSVVRTczFOekE2TURReE16bzBNREVuUjBsRUt6RW5VRUZES3pFblVF  RkRLeXNyV0RJNg0KTVRnMU9qazFKMUJCUXlzcksweERURG8yTnpvNU5TZERUa2tyS3pvNk9ra25V  a1pHSzBGQlVUcE1RMHhWTmpJM05qSTNOaWRIU1VRcg0KTVNkU1JrWXJRVU5WT2s1SlRDZEhTVVFy  TVNkU1JrWXJRa2c2VFZsQ1IwZzNKMGRKUkNzeEoxSkdSaXROUWpwTldVSkpSMEV4SjBkSg0KVXl0  T09qWXlPamsxSjBkSlV5dFZPall6T2prMUowZEpVeXRWT2pjeE9qazFKMGRKVXl0T09qRTROam81  TlNkSFNWTXJUam94T0RnNg0KT1RVblZFUlVLekVuUkZSTkt6UXlNRG95TURFNE1EWXdNVG94TURJ  blJGUk5LelF5TURvd016TTBPalF3TVNkRVZFMHJOVGN3T2pJdw0KTVRnd05qQXhPakV3TWlkRVZF  MHJOVGN3T2pBME1UTTZOREF4SjBkSlJDc3hKMUJCUXlzeEoxQkJReXNySzFneU9qRTROVG81TlNk  UQ0KUVVNckt5dE1RMHc2TmpjNk9UVW5WVTVVS3pVd0t6RW5WVTVhS3pFck16Z3lOVGt4Snc9PQ0K  oIIFSTCCBUUwggSuoAMCAQICEGOdASTLN0aW+QSJACnXNt0wDQYJKoZIhvcNAQEFBQAwgY8xGzAZ  BgNVBAoTElZlcmlTaWduIEF1c3RyYWxpYTEXMBUGA1UECxMOR2F0ZWtlZXBlciBQS0kxODA2BgNV  BAsTL1Rlcm1zIG9mIHVzZSBhdCBodHRwczovL3d3dy5lc2lnbi5jb20uYXUvR0tSUEEvMR0wGwYD  VQQDExRHYXRla2VlcGVyIFRZUEUgMyBDQTAeFw0xNzA2MTMwMDAwMDBaFw0xOTA3MTEyMzU5NTla  MHUxCzAJBgNVBAYTAkFVMRgwFgYDVQQIEw9OZXcgU291dGggV2FsZXMxIDAeBgNVBAoUF1dJU0VU  RUNIIEdMT0JBTCBMSU1JVEVEMRIwEAYDVQQLFAlDYXJnb1dpc2UxFjAUBgNVBAMTDWVkaUVudGVy  cHJpc2UwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC+maeXf7tuM3wd13r0LpMlZr1D  noWyZQY9LKW97CoBtQP4Lgiy+OJMKnkYaxOaJ+29kIyD0kme2x7zSBm7yaiJDHbXds0a0CLk8JFz  8uU/MyDaDNCM3BBu7AbUrpP4Wem02KCZeHhuW8PdQceI4ROjIefRPjwaWE9CkE6utNr9lGCLsLXl  y98HzMGPZsYZuiCWNvI1icW3QBVR/xaLkyBsP9p2OMyE6LHGAwz5t53PoVQZmQajY3UOKzJWWO2S  ci6saaLLTnYcohqehKeluezHoPr39GnygVTRX3szTr1kGBOFHp+LX26qr8nqhQoTYxi3P9udQmZH  vQTtiSFXee95AgMBAAGjggI1MIICMTAMBgNVHRMBAf8EAjAAMIIBBgYDVR0fBIH+MIH7MIH4oIH1  oIHyhj1odHRwOi8vb25zaXRlY3JsLmVzaWduLmNvbS5hdS9HYXRla2VlcGVyVHlwZTNDQS9MYXRl  c3RDUkwuY3JshoGwbGRhcDovL2RpcmVjdG9yeS5lc2lnbi5jb20uYXUvY249R2F0ZWtlZXBlciBU  WVBFIDMgQ0Esb3U9VGVybXMgb2YgdXNlIGF0IGh0dHBzOi8vd3d3LmVzaWduLmNvbS5hdS9HS1JQ  QS8sb3U9R2F0ZWtlZXBlciBQS0ksbz1WZXJpU2lnbiBBdXN0cmFsaWE/Y2VydGlmaWNhdGVyZXZv  Y2F0aW9ubGlzdDtiaW5hcnkwHwYDVR0jBBgwFoAUtdsZxo7xZOfcgKfpIo4eJgQltDQwHQYDVR0O  BBYEFDQvSFIHMwYBYSxhYqYO/eV+O8mSMDUGCCsGAQUFBwEBBCkwJzAlBggrBgEFBQcwAYYZaHR0  cHM6Ly9vY3NwLmVzaWduLmNvbS5hdTAOBgNVHQ8BAf8EBAMCBPAwHAYDVR0RBBUwE4ERY21yQGNh  cmdvd2lzZS5jb20wFwYGKiQBgk0BBA0WCzQxMDY1ODk0NzI0MEYGA1UdIAQ/MD0wOwYKKiSp/LRj  gk0CCDAtMCsGCCsGAQUFBwIBFh9odHRwczovL3d3dy5lc2lnbi5jb20uYXUvR0tSUEEvMBEGCWCG  SAGG+EIBAQQEAwIHgDANBgkqhkiG9w0BAQUFAAOBgQB0gtaf5kifuI/NOorC7rDHYJ3s+N9bZTAv  zQ8It6NgdnVQ8GrzKH+I9ZYYobQzCGmrCVQ/NPRr9CN7x0yC8H2Z5FSoBZU66dJUEiIkwNtWGUKR  4WF4H9Z8/JhPK1urImse6sKXobBR9V8Yp2ibww69w8hHgMlJuRHWodKgIgs7YTGCAc8wggHLAgEB  MIGkMIGPMRswGQYDVQQKExJWZXJpU2lnbiBBdXN0cmFsaWExFzAVBgNVBAsTDkdhdGVrZWVwZXIg  UEtJMTgwNgYDVQQLEy9UZXJtcyBvZiB1c2UgYXQgaHR0cHM6Ly93d3cuZXNpZ24uY29tLmF1L0dL  UlBBLzEdMBsGA1UEAxMUR2F0ZWtlZXBlciBUWVBFIDMgQ0ECEGOdASTLN0aW+QSJACnXNt0wDAYI  KoZIhvcNAgUFADANBgkqhkiG9w0BAQEFAASCAQCAy0SSHBx0OyqdbUr/Bqt3ZuAoGW0sxLyPF8lO  xCefxyDX7Lr+YbD6RNngjBPA52mMdyLksJI61mLDTQ+VwkewZd5zBspXVJIfjB3GeamSWDe04+4j  Ghn2ixd0SEvHoBIcGI2O+IDxVW2/t8rdz3YpVly9SNx5eCaqDOYhe+p/njVF40oaSTFbpjPv3mkO  urIIJigQOFCsvCNt/Q2GvXof+t+qecPNNG7vyhPur8wbQcsKTuDUeCEtT4CFU1O16JqLgQCVehCS  0wt/vhfDVU6vMPxErpkqjBVCrBqha4Lp7P2fo6vyChTrBv5YaZ11PZ+N54eD24ZmeavQ/HUvxyrG  ";
			interchange.EI_FooterText = "";

			var outgoingMessage = interchange.ContainedMessages.AddNew(typeof(EDIMessageTestHelper));
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "SMI1";
			outgoingMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+O00000409/CMT1:1+9'NAD+VW+41065894724::95'TDT+20+901++11++++8811924::11'LOC+4+1399K::95'CNI++:::I'RFF+AAQ:AAAA1111117'GID+1'RFF+ACU:SH'GIS+Y:62:95'GIS+U:63:95'GIS+U:71:95'GIS+N:186:95'GIS+N:188:95'TDT+1'DTM+420:20160218:102'DTM+420:2318:401'DTM+570:20160218:102'DTM+570:2318:401'GID+1'PAC+3'PAC+++YC:185:95'PAC+++FCL:67:95'UNT+24+1'";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			AssertEquals("Pre-conditon - Outgoing msg/interchange linked", outgoingMessage.EM_EI, interchange.PK);

			var outturnHeader = Factory.New<CusOutturnHeader>();

			var splitMessage3 = Factory.New<CMRSEAOUTMessage>();
			splitMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_ApplicationReference = "SMI3";

			var splitMessage2 = Factory.New<CMRSEAOUTMessage>();
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_ApplicationReference = "SMI2";
			Factory.Save();

			outturnHeader.Messages.Add(outgoingMessage);
			outgoingMessage.EM_LinkUniqueID = outturnHeader.PK;
			outturnHeader.Messages.Add(splitMessage3);
			splitMessage3.EM_LinkUniqueID = outturnHeader.PK;
			outturnHeader.Messages.Add(splitMessage2);
			splitMessage2.EM_LinkUniqueID = outturnHeader.PK;

			var responseInterchange = Factory.New<CMRInterchange>();
			responseInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			responseInterchange.EI_To = "AAA374M";
			responseInterchange.EI_From = "AAA336C";
			responseInterchange.EI_InterchangeNum = "00000000400737";
			responseInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseInterchange.EI_InterchangeType = "CMR";
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_BodyText = @"UNH+000001+CONTRL:D:3:UN'UCI+382591+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+18'UCS+26+18'UCD+12+4:1'UCS+48+18'UCD+12+4:1'UNT+8+000001'";
			responseInterchange.EI_Status = EDIInterchange.Status.eHubPending;
			AssertEquals("Precondition, ShouldSendViaEHub", true, responseInterchange.ShouldSendViaEHub);

			var responseMessage = Factory.New<CMRCONTRLMessage>();
			responseMessage.EM_ApplicationCode = "CMR";
			responseMessage.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage.EM_MessageNum = "00000000400737000001";
			responseMessage.EM_ApplicationReference = "000001";
			responseMessage.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+382591+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+18'UCS+26+18'UCD+12+4:1'UCS+48+18'UCD+12+4:1'UNT+8+000001'";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_Status = EDIInterchange.Status.Queued;
			responseMessage.EM_LinkTable = "EDIInterchange";
			responseMessage.EM_LinkUniqueID = responseInterchange.PK;
			responseInterchange.InterchangeAcknowledgementMessages.Add(responseMessage);

			logger = new LoggingInformation();
			processor = new TestHelperCONTRLMessageProcessor(logger);
			processor.ProcessMessage(responseMessage);
			AssertEquals("Interchange EI_Status", EDIInterchange.Status.eHubPending, responseInterchange.EI_Status);

			AssertEquals("Split message 1 has been rejected with CONTRL message", EDIMessage.Status.Rejected, outgoingMessage.EM_Status);
			AssertEquals("All pending split messages for this SEA Outturn should be cancelled", EDIMessage.Status.Cancelled, splitMessage2.EM_Status);
			AssertEquals("All pending split messages for this SEA Outturn should be cancelled", EDIMessage.Status.Cancelled, splitMessage3.EM_Status);

			AssertEquals("Cancelled log should have been created on the Underbond", CMRCUSRESMessage.splitMessageOriginalCancelled, outturnHeader.Logs.MostRecentLog.SL_Reference);
			AssertEquals("Cancelled log SL_SE_NKEvent", AutoEvents.UnderbondSplitOutturnOriginalRejectedCode, outturnHeader.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals("Outturn Status", CMRBaseStatuses.Codes.OriginalRejected, outturnHeader.OutturnStatus.Code);
		}

		public void TestProcessEXDMessageInterchangeRejection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.EXD;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_LinkedObject = declaration;

			outgoingMessage.EM_MessageSubType = "ORG";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Original EXD Message Status should now be rejected", CustomsEntryStatus.FailOriginal.Code, declaration.JE_EntryStatus);

			outgoingMessage.EM_MessageSubType = "AMD";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Amend EXD Message Status should now be rejected", CustomsEntryStatus.FailReplacement.Code, declaration.JE_EntryStatus);

			outgoingMessage.EM_MessageSubType = "WDW";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Withdraw EXD Message Status should now be rejected", CustomsEntryStatus.FailWithdrawal.Code, declaration.JE_EntryStatus);
		}

		public void TestOnlyOutturnPendingMessagesAreCancelledOnRejection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var interchange = Factory.New<CMRInterchange>();
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "AAA336C";
			interchange.EI_From = "AAA374M";
			interchange.EI_InterchangeNum = "381942";
			interchange.EI_Status = EDIMessage.Status.Sent;
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			interchange.EI_InterchangeType = "EXD";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_HeaderText = "";
			interchange.EI_BodyText = @"Mime-Version: 1.0  Content-Transfer-Encoding: base64  Content-Type: application/pkcs7-mime; smime-type=signed-data; name = ""smime.p7m""    MIILRQYJKoZIhvcNAQcCoIILNjCCCzICAQExDjAMBggqhkiG9w0CBQUAMIID+wYJKoZIhvcNAQcB  oIID7ASCA+hNaW1lLVZlcnNpb246IDEuMA0KQ29udGVudC1UcmFuc2Zlci1FbmNvZGluZzogYmFz  ZTY0DQpDb250ZW50LVR5cGU6IGFwcGxpY2F0aW9uL2VkaWZhY3Q7IG5hbWU9QUFBMzc0TV8zODIw  MDUuZWRpDQpDb250ZW50LURpc3Bvc2l0aW9uOiBhdHRhY2htZW50OyBmaWxlbmFtZT1BQUEzNzRN  XzM4MjAwNS5lZGkNCg0KVlU1Qk9pc3VQeUFuVlU1Q0sxVk9UME02TXl0QlFVRXpOelJOT2pwQlFV  RXpOelJOSzBGQlFUTXpOa01yTVRjeE1qQTBPakEzTURrcg0KTXpneU1EQTFLeXNyS3pFckt6RW5W  VTVJS3pFclExVlRSRVZET2tRNk9UbENPbFZPSjBKSFRTczRNekE2T2pwRldFUXJRakF3TVRjeA0K  TlRVM0wwTk5WREU2TVNzNUoweFBReXM1SzBGVlRVVk1Pam8ySjB4UFF5c3hNaXRPV2tGTFREbzZO  aWRNVDBNck1qZ3JUbG82T2pZbg0KUkZSTkt6RXlPVG95TURFM01USXdORG94TURJblIwbFRLMDQ2  TnprNk9UVW5SMGxUSzA0Nk1UQTNPamsxSjBkSlV5dE9PakUwTVRvNQ0KTlNkU1JrWXJRVmRJT2tF  blVFRkRLeXNyVGpvMk56bzVOU2RRUVVNckt5dFBWRG94TkRZNk9UVW5WRVJVS3pJd0t5c3JOaWRP  UVVRcg0KUTA0ckt5dEJSRlZNUVZSSlQwNGdRazlQUzFNZ1RGUkVJQ2hPV2lCRFZWTlVUMDFUS1Nz  clUxbEVUa1ZaSjA1QlJDdEhUeXN5TXpFeA0KTWprek5qazVNUzh3TURFNk9qazFKMDFQUVNzek9U  bzZRVlZFSjAxUFFTczJNem8xTURBd09rRlZSQ2RWVGxNclJDZERVMVFyTVN0Sg0KT2pvNU5TZEdW  RmdyUVVGQkt5c3JRMDlPVGtWRFZFOVNVeUJHVDFJZ1QxQlVTVU5CVENCR1NVSlNSVk1zSUU5UVZF  bERRVXdnUmtsQw0KVWtVZ1FsVk9SRXhGVXlCUFVpQkRRVUpNUlZNblRFOURLekkzS3l0QlZTMVdT  VG82TmlkTlJVRXJWMVFySzB0SE9qRXdKMDFGUVN0Qg0KUWxjckswNVBPakV3SjAxUFFTczJNem8x  TURBd0oxSkdSaXRJVXpvNE5UTTJOekF4TUNkVlRsTXJVeWREVGxRck1URTZNVEFuUTA1VQ0KS3pN  Mk9qQW5WVTVVS3pJNUt6RW5WVTVhS3pFck16Z3lNREExSnc9PQ0KoIIFSTCCBUUwggSuoAMCAQIC  EGOdASTLN0aW+QSJACnXNt0wDQYJKoZIhvcNAQEFBQAwgY8xGzAZBgNVBAoTElZlcmlTaWduIEF1  c3RyYWxpYTEXMBUGA1UECxMOR2F0ZWtlZXBlciBQS0kxODA2BgNVBAsTL1Rlcm1zIG9mIHVzZSBh  dCBodHRwczovL3d3dy5lc2lnbi5jb20uYXUvR0tSUEEvMR0wGwYDVQQDExRHYXRla2VlcGVyIFRZ  UEUgMyBDQTAeFw0xNzA2MTMwMDAwMDBaFw0xOTA3MTEyMzU5NTlaMHUxCzAJBgNVBAYTAkFVMRgw  FgYDVQQIEw9OZXcgU291dGggV2FsZXMxIDAeBgNVBAoUF1dJU0VURUNIIEdMT0JBTCBMSU1JVEVE  MRIwEAYDVQQLFAlDYXJnb1dpc2UxFjAUBgNVBAMTDWVkaUVudGVycHJpc2UwggEiMA0GCSqGSIb3  DQEBAQUAA4IBDwAwggEKAoIBAQC+maeXf7tuM3wd13r0LpMlZr1DnoWyZQY9LKW97CoBtQP4Lgiy  +OJMKnkYaxOaJ+29kIyD0kme2x7zSBm7yaiJDHbXds0a0CLk8JFz8uU/MyDaDNCM3BBu7AbUrpP4  Wem02KCZeHhuW8PdQceI4ROjIefRPjwaWE9CkE6utNr9lGCLsLXly98HzMGPZsYZuiCWNvI1icW3  QBVR/xaLkyBsP9p2OMyE6LHGAwz5t53PoVQZmQajY3UOKzJWWO2Sci6saaLLTnYcohqehKeluezH  oPr39GnygVTRX3szTr1kGBOFHp+LX26qr8nqhQoTYxi3P9udQmZHvQTtiSFXee95AgMBAAGjggI1  MIICMTAMBgNVHRMBAf8EAjAAMIIBBgYDVR0fBIH+MIH7MIH4oIH1oIHyhj1odHRwOi8vb25zaXRl  Y3JsLmVzaWduLmNvbS5hdS9HYXRla2VlcGVyVHlwZTNDQS9MYXRlc3RDUkwuY3JshoGwbGRhcDov  L2RpcmVjdG9yeS5lc2lnbi5jb20uYXUvY249R2F0ZWtlZXBlciBUWVBFIDMgQ0Esb3U9VGVybXMg  b2YgdXNlIGF0IGh0dHBzOi8vd3d3LmVzaWduLmNvbS5hdS9HS1JQQS8sb3U9R2F0ZWtlZXBlciBQ  S0ksbz1WZXJpU2lnbiBBdXN0cmFsaWE/Y2VydGlmaWNhdGVyZXZvY2F0aW9ubGlzdDtiaW5hcnkw  HwYDVR0jBBgwFoAUtdsZxo7xZOfcgKfpIo4eJgQltDQwHQYDVR0OBBYEFDQvSFIHMwYBYSxhYqYO  /eV+O8mSMDUGCCsGAQUFBwEBBCkwJzAlBggrBgEFBQcwAYYZaHR0cHM6Ly9vY3NwLmVzaWduLmNv  bS5hdTAOBgNVHQ8BAf8EBAMCBPAwHAYDVR0RBBUwE4ERY21yQGNhcmdvd2lzZS5jb20wFwYGKiQB  gk0BBA0WCzQxMDY1ODk0NzI0MEYGA1UdIAQ/MD0wOwYKKiSp/LRjgk0CCDAtMCsGCCsGAQUFBwIB  Fh9odHRwczovL3d3dy5lc2lnbi5jb20uYXUvR0tSUEEvMBEGCWCGSAGG+EIBAQQEAwIHgDANBgkq  hkiG9w0BAQUFAAOBgQB0gtaf5kifuI/NOorC7rDHYJ3s+N9bZTAvzQ8It6NgdnVQ8GrzKH+I9ZYY  obQzCGmrCVQ/NPRr9CN7x0yC8H2Z5FSoBZU66dJUEiIkwNtWGUKR4WF4H9Z8/JhPK1urImse6sKX  obBR9V8Yp2ibww69w8hHgMlJuRHWodKgIgs7YTGCAc8wggHLAgEBMIGkMIGPMRswGQYDVQQKExJW  ZXJpU2lnbiBBdXN0cmFsaWExFzAVBgNVBAsTDkdhdGVrZWVwZXIgUEtJMTgwNgYDVQQLEy9UZXJt  cyBvZiB1c2UgYXQgaHR0cHM6Ly93d3cuZXNpZ24uY29tLmF1L0dLUlBBLzEdMBsGA1UEAxMUR2F0  ZWtlZXBlciBUWVBFIDMgQ0ECEGOdASTLN0aW+QSJACnXNt0wDAYIKoZIhvcNAgUFADANBgkqhkiG  9w0BAQEFAASCAQBixpSqBv/6+53qt88/r5rx1WOdrq77S1tOd+8jG6e5gnCPFkOCVYAtuddKoKea  iCRsaim5fTDJD0ctKTaGaacRA3twhvTejx9OJNeImcN3r9RuSzKb6b5BIKwUx84wtvLhU7KoFKKq  N7fpIk/mohaoyhgkEF9/5MSBzVpIoNtoXMtzP8+L74zhSb4tuL/7yEO9j8FSL8vP7ZtWEIXzdGym  +z12vbDJGdezBj3s1pTLgksF0nU1My72JTq8YRCYK3N/7hOKwdZbnnSJJUABahyad8pSPjz1cY3m  gTJojznSK78ezLDZNqXB7j1EpEnswEQD2d3oYfTHBfsaCgE4JrmJ  ";
			interchange.EI_FooterText = "";
			Factory.Save();

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.EXD;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_LinkedObject = declaration;
			outgoingMessage.EM_EI = interchange.PK;

			var outgoingMessage2 = Factory.New<EDIMessage>();
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.EXD;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_Status = EDIMessage.Status.Pending;
			outgoingMessage2.EM_LinkedObject = declaration;

			var responseInterchange = Factory.New<CMRInterchange>();
			responseInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			responseInterchange.EI_To = "AAA374M";
			responseInterchange.EI_From = "AAA336C";
			responseInterchange.EI_InterchangeNum = "00000000400026";
			responseInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseInterchange.EI_InterchangeType = "CMR";
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_BodyText = @"UNH+000001+CONTRL:D:3:UN'UCI+381942+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+18'UCS+3+18'UCD+13+2:2'UCM+2+CUSCAR:D:99B:UN+4+18'UCS+3+18'UCD+13+2:2'UNT+9+000001'";
			responseInterchange.EI_Status = EDIInterchange.Status.eHubPending;
			AssertEquals("Precondition, ShouldSendViaEHub", true, responseInterchange.ShouldSendViaEHub);

			var responseMessage = Factory.New<CMRCONTRLMessage>();
			responseMessage.EM_ApplicationCode = "CMR";
			responseMessage.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage.EM_MessageNum = "00000000400026000001";
			responseMessage.EM_ApplicationReference = "000001";
			responseMessage.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+381942+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+18'UCS+3+18'UCD+13+2:2'UCM+2+CUSCAR:D:99B:UN+4+18'UCS+3+18'UCD+13+2:2'UNT+9+000001'";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_Status = EDIInterchange.Status.Queued;
			responseMessage.EM_LinkTable = "EDIInterchange";
			responseMessage.EM_LinkUniqueID = responseInterchange.PK;
			responseInterchange.InterchangeAcknowledgementMessages.Add(responseMessage);

			logger = new LoggingInformation();
			processor = new TestHelperCONTRLMessageProcessor(logger);
			processor.ProcessMessage(responseMessage);
			AssertEquals("Interchange EI_Status", EDIInterchange.Status.eHubPending, responseInterchange.EI_Status);
			AssertEquals("Original message has been rejected with CONTRL message", EDIMessage.Status.Rejected, outgoingMessage.EM_Status);
			AssertEquals("Pending message should not be affected as it is not an Outturn", EDIMessage.Status.Pending, outgoingMessage2.EM_Status);
		}

		public void TestProcessWARRELMessageInterchangeRejection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.WARREL;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_LinkedObject = declaration;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;

			outgoingMessage.EM_MessageSubType = "ORG";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Original WARREL Message Status should now be rejected", CustomsEntryStatus.FailWARRELOriginal.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = "AMD";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Replace WARREL Message Status should now be rejected", CustomsEntryStatus.FailWARRELReplacement.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = "WDW";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Withdraw WARREL Message Status should now be rejected", CustomsEntryStatus.FailWARRELWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestProcessWARRETMessageInterchangeRejection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.WARRET;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_LinkedObject = declaration;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;

			outgoingMessage.EM_MessageSubType = "ORG";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Original WARRET Message Status should now be rejected", CustomsEntryStatus.FailWARRETOriginal.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = "AMD";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Replace WARRET Message Status should now be rejected", CustomsEntryStatus.FailWARRETReplacement.Code, declaration.JE_MessageStatus);
		}

		public void TestProcessDEPRECMessageInterchangeRejection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.DEPREC;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_LinkedObject = declaration;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;

			outgoingMessage.EM_MessageSubType = "ORG";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Original DEPREC Message Status should now be rejected", CustomsEntryStatus.FailDEPRECOriginal.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = "AMD";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Replace DEPREC Message Status should now be rejected", CustomsEntryStatus.FailDEPRECReplacement.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = "WDW";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Withdraw DEPREC Message Status should now be rejected", CustomsEntryStatus.FailDEPRECWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestProcessDEPRELMessageInterchangeRejection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.DEPREL;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_LinkedObject = declaration;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;

			outgoingMessage.EM_MessageSubType = "ORG";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Original DEPREL Message Status should now be rejected", CustomsEntryStatus.FailDEPRELOriginal.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = "AMD";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Replace DEPREL Message Status should now be rejected", CustomsEntryStatus.FailDEPRELReplacement.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = "WDW";
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Entry status should not change", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Withdraw DEPREL Message Status should now be rejected", CustomsEntryStatus.FailDEPRELWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestProcessIMDMessageInterchangeRejection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader testHeader = declaration.CustomsEntryHeaders.AddNew();
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_LinkedObject = testHeader;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;

			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			testHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;

			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Outgoing Message Status should be rejected", "REJ", outgoingMessage.EM_Status);
			AssertEquals("Formal Entry Header Message Status should now be failed", CustomsEntryStatus.FailFormalLodge.Code, testHeader.CH_Status);
			AssertEquals("Formal Declaration Message Status should now be failed", CustomsEntryStatus.FailFormalLodge.Code, declaration.JE_MessageStatus);

			testHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Pre-lodge Entry Header Message Status should now be failed", CustomsEntryStatus.FailPreLodge.Code, testHeader.CH_Status);
			AssertEquals("Pre-lodge Declaration Message Status should now be failed", CustomsEntryStatus.FailPreLodge.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.PAYSTD;

			testHeader.CH_Status = CustomsEntryStatus.AwaitingPayment.Code;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Payment Entry Header Message Status should now be failed", CustomsEntryStatus.FailPayment.Code, testHeader.CH_Status);
			AssertEquals("Payment Declaration Message Status should now be failed", CustomsEntryStatus.FailPayment.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SAC;

			testHeader.CH_Status = CustomsEntryStatus.AwaitingSAC.Code;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("SAC Entry Header Message Status should now be failed", CustomsEntryStatus.FailSAC.Code, testHeader.CH_Status);
			AssertEquals("SAC Declaration Message Status should now be failed", CustomsEntryStatus.FailSAC.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;

			testHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Amendment Entry Header Message Status should now be failed", CustomsEntryStatus.FailAmendment.Code, testHeader.CH_Status);
			AssertEquals("Amendment Declaration Message Status should now be failed", CustomsEntryStatus.FailAmendment.Code, declaration.JE_MessageStatus);

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;

			testHeader.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Withdrawl Entry Header Message Status should now be failed", CustomsEntryStatus.FailWithdrawal.Code, testHeader.CH_Status);
			AssertEquals("Withdrawl Declaration Message Status should now be failed", CustomsEntryStatus.FailWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestProcessAIRCRMessageInterchangeRejection()
		{
			CusHAWB cusHAWB = Factory.New<CusHAWB>();
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.AIRCR;
			outgoingMessage.EM_LinkedObject = cusHAWB;

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Original AIRCR Message Status should now be rejected", CMRBaseStatuses.Codes.OriginalRejected, cusHAWB.CS_MsgStatus);

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Amend AIRCR Message Status should now be rejected", CMRBaseStatuses.Codes.AmendmentRejected, cusHAWB.CS_MsgStatus);

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Withdraw AIRCR Message Status should now be rejected", CMRBaseStatuses.Codes.WithdrawalRejected, cusHAWB.CS_MsgStatus);
		}

		public void TestProcessUnderbondMessageInterchangeRejection()
		{
			CusUnderbond underbond = CusUnderbond.New(Factory);
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.UBMREQ;
			outgoingMessage.EM_LinkedObject = underbond;

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Original UBMREQ Message Status should now be rejected", CMRBaseStatuses.Codes.OriginalRejected, underbond.UnderbondStatus.Code);

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Amend UBMREQ Message Status should now be rejected", CMRBaseStatuses.Codes.AmendmentRejected, underbond.UnderbondStatus.Code);

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Original UBMREQ Message Status should now be rejected", CMRBaseStatuses.Codes.WithdrawalRejected, underbond.UnderbondStatus.Code);
		}

		public void TestProcessAIROUTMessageInterchangeRejection()
		{
			CusUnderbond underbond = CusUnderbond.New(Factory);
			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			outgoingMessage.EM_LinkedObject = underbond;

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Original AIROUT Message Status should now be rejected", CMRBaseStatuses.Codes.OriginalRejected, underbond.OutturnStatus.Code);

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Amend AIROUT Message Status should now be rejected", CMRBaseStatuses.Codes.AmendmentRejected, underbond.OutturnStatus.Code);

			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			AssertEquals("Original AIROUT Message Status should now be rejected", CMRBaseStatuses.Codes.WithdrawalRejected, underbond.OutturnStatus.Code);
		}

		public void TestProcessESMMessageInterchangeRejection()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();

			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			header.Messages.Add(outgoingMessage);

			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage.EM_MessageSubType = "ORG";
			Assert(header.IsWaitingForManifestResponse);
			Assert(header.ManifestStatus != "Not Sent");
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			Assert(!header.IsWaitingForManifestResponse);
			Assert(header.ManifestStatus.Contains("rejected by Customs"));
		}

		public void TestProcessDEPARTMessageInterchangeRejection()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();

			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			EDIMessage outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			header.Messages.Add(outgoingMessage);

			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.DEPART;
			outgoingMessage.EM_MessageSubType = "ORG";
			Assert(header.IsWaitingForDepartureReportResponse);
			Assert(header.DepartureStatus != "Not Sent");
			processor.ProcessMessageRejection(incomingMessage, outgoingMessage);
			Assert(!header.IsWaitingForDepartureReportResponse);
			Assert(header.DepartureStatus.Contains("rejected by Customs"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			interchange = Factory.New<CMRInterchange>();
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "AAA336C";
			interchange.EI_From = "AAA374M";
			interchange.EI_InterchangeNum = "104715";
			interchange.EI_Status = EDIMessage.Status.Sent;
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_HeaderText = "UNB+UNOC:3+AAA374M::AAA374M+AAA336C+031101:2247+132365++++1++1'";
			interchange.EI_BodyText = @"UNH+1+CUSDEC:D:99B:UN'BGM+830:::EXD+S00001760:1+9'LOC+9+AUSYD::6'LOC+12+NZAKL::6'LOC+28+NZ::6'DTM+129:20040430:102'GIS+N:79:95'GIS+N:107:95'RFF+AWH:A'PAC+++:67:95'PAC+++OT:146:95'TDT+20+++11'NAD+CN+++TEST CONSIGEE++AUCKLAND'NAD+GO+66015286036::95'MOA+39::AUD'MOA+63:100:AUD'UNS+D'CST+1+I::95'FTX+AAA+++CHALK'LOC+27++AU-NS::6'MEA+WT++KG:100.000'MEA+ABW++KG:100.0000'MOA+63:100'RFF+HS:25090000'UNS+S'CNT+11:0'CNT+36:10'UNT+28+1'";
			interchange.EI_FooterText = "UNZ+1+132365'";

			EDIMessage outgoingMessage = interchange.ContainedMessages.AddNew(typeof(EDIMessageTestHelper));
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_MessageText = interchange.EI_BodyText;

			parent = Factory.New<JobDeclaration>();
			parent.Messages.Add(outgoingMessage);
			logger = new LoggingInformation();
			processor = new TestHelperCONTRLMessageProcessor(logger);
			Factory.Save();
		}

		JobDeclaration parent;
		CMRInterchange interchange;
		LoggingInformation logger;
		TestHelperCONTRLMessageProcessor processor;

		#region EDIMessageTestHelper
		class EDIMessageTestHelper : EDIMessage
		{
			public EDIMessageTestHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
				//do nothing
			}
		}
		#endregion

		#region CONTRL Processor Test Helper

		class TestHelperCONTRLMessageProcessor : CONTRLMessageProcessor
		{
			public TestHelperCONTRLMessageProcessor(LoggingInformation logger) : base(logger) { }

			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
		#endregion
		#endregion
	}
}
