using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL.Testing
{
	class GenralTests : TestCaseWithFactory
	{
		public void TestFallbackInvokedImport()
		{
			InisialiseFallbackRegistry(false, false);
			FallbackRunner(false, true, @"UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN'BGM+BCM::109'MSG+CCS-UK'FTX+AAA+++***FALLBACK*** IMPORT INVOKED EDATE 01052011 ETIME 080159:***FALLBACK ANNOUNCED*** ADATE 01052011 ATIME 192029:CHIEF UNAVAILABLE DUE TO FAILED HARDWARE IN HMRC:THE NETWORK CARD DONE BROKE:I NEVER TOUCHED IT'UNT+5+<<MSGNO PLACEHOLDER>>'");
			MakeFurtherFallbackAssertionsForOneRun();
		}

		public void TestFallbackInvokedExport()
		{
			InisialiseFallbackRegistry(false, false);
			FallbackRunner(true, false, @"UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN'BGM+BCM::109'MSG+CCS-UK'FTX+AAA+++***FALLBACK*** EXPORT INVOKED EDATE 01052011 ETIME 080159:***FALLBACK ANNOUNCED*** ADATE 01052011 ATIME 092029:CHIEF UNAVAILABLE DUE TO FAILED HARDWARE IN HMRC'UNT+5+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestFallbackRevokedImport()
		{
			InisialiseFallbackRegistry(true, true);
			FallbackRunner(true, false, @"UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN'BGM+BCM::109'MSG+CCS-UK'FTX+AAA+++***FALLBACK*** IMPORT REVOKED EDATE 01052011 ETIME 080159:***FALLBACK ANNOUNCED*** ADATE 01052011 ATIME 092029:CHIEF NOW OK'UNT+5+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestFallbackRevokedExport()
		{
			InisialiseFallbackRegistry(true, true);
			FallbackRunner(false, true, @"UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN'BGM+BCM::109'MSG+CCS-UK'FTX+AAA+++***FALLBACK*** EXPORT REVOKED EDATE 01052011 ETIME 080159:***FALLBACK ANNOUNCED*** ADATE 01052011 ATIME 092029:CHIEF NOW OK'UNT+5+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestFallbackInvokedBoth()
		{
			InisialiseFallbackRegistry(false, false);
			FallbackRunner(true, true, @"UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN'BGM+BCM::109'MSG+CCS-UK'FTX+AAA+++***FALLBACK*** IMPORT AND EXPORT INVOKED EDATE 01052011 ETIME 080159:***FALLBACK ANNOUNCED*** ADATE 01052011 ATIME 092029:CHIEF UNAVAILABLE DUE TO FAILED HARDWARE IN HMRC'UNT+5+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestFallbackRevokedBoth()
		{
			InisialiseFallbackRegistry(true, true);
			FallbackRunner(false, false, @"UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN'BGM+BCM::109'MSG+CCS-UK'FTX+AAA+++***FALLBACK*** IMPORT AND EXPORT REVOKED EDATE 01052011 ETIME 080159:***FALLBACK ANNOUNCED*** ADATE 01052011 ATIME 092029:CHIEF NOW OK'UNT+5+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestMakeGenralToParticipant_Preformatted()
		{
			GenralMessageGenerator generator = new GenralMessageGenerator();
			string result = generator.MakeMessageToParticipant(weaselPreformattedSource, new UkCharSet(), true);
			AssertEquals(genralMessageEdifact_TextPreformatted, result);
		}

		public void TestMakeGenralToParticipant_Unformatted()
		{
			GenralMessageGenerator generator = new GenralMessageGenerator();
			string result = generator.MakeMessageToParticipant(thisIsAnHomageToOneVeryAnnoyingGuyAtAMajorClientWhoLooksLikeAMoleButBehavesLikeAMustela, new UkCharSet(), false);
			AssertEquals(genralMessageEdifact_Text, result);
		}

		public void TestParseGenralFromParticipant_PoorUnhVersion()
		{
			RunGenralFromParticipantTest(genralMessageEdifact_Text.Replace(EDIMessage.MessageNumberPlaceHolder, "123")
																  .Replace("GENRAL:0:912:UN+", "GENRAL:0:912:BT+")
										);
		}

		public void TestParseGenralFromParticipant()
		{
			RunGenralFromParticipantTest(genralMessageEdifact_Text.Replace(EDIMessage.MessageNumberPlaceHolder, "123"));
		}

		EDIMessage RunGenralFromParticipantTest(string bodyOfGenral)
		{
			string interchangeHeader = "UNB+UNOA:2+CUKFFW98000ABC:IATA+CUKFFW98000CAR:IATA+100715:1533+1028C153305000+++A'";
			string interchangeFooter = "UNZ+1+1028C153305000'";

			EDIInterchange receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(this.Factory, interchangeHeader + bodyOfGenral + interchangeFooter, "CUK");
			GenralMessageParser parser = new GenralMessageParser();
			var receivedMessage = receivedInterchange.ContainedMessages[0];
			receivedMessage.EM_MessageNum = "123";
			parser.ParseMessage(receivedMessage);
			Factory.Save();
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains(@"<h3>GENRAL message</h3>
<p>Sender: CUKFFW98000ABC (Another CCS-UK participant)<br/>
Recipient: CUKFFW98000CAR<br/>
Purpose: Text<br/>
Payload:</p>
<p><b><pre><i>WEASELS ARE MAMMALS", relevantEMail.Body);
			AssertEquals("GEN", receivedMessage.EM_MessageType);
			AssertEquals("TXT", receivedMessage.EM_MessageSubType);
			AssertEquals("CUKFFW98000ABC", receivedMessage.EM_ApplicationReference);
			AssertEquals("CUKFFW98000CAR", receivedMessage.EM_MessageOwner);
			AssertContains("To open the job", relevantEMail.Body);
			AssertContainsExactElementsInAnyOrder(new string[] { "daniel@wisetechglobal.com", "yawn@soPointless.com" }, relevantEMail.Recipients.RecipientsAsDelimitedString(";").Split(';'));
			return receivedMessage;
		}

		public void TestReceivedGenralGoesAgainstCorrectBranchBasedOnRecipientPima()
		{
			var dunstableBranch = CuscarInboundParserTests.GetDunstableBranchPkForTest(Factory);
			BadgeCodeSetting badgeDan;
			CredentialsSetting credentialCla;
			using (DisposableEnvironment.ForBranch(dunstableBranch.PK.ToGuid()))
			{
				CuscarInboundParserTests.MakeCredentials(out badgeDan, out credentialCla, "CUKFFW98000CAR");
			}

			var receivedMessage = RunGenralFromParticipantTest(genralMessageEdifact_Text.Replace(EDIMessage.MessageNumberPlaceHolder, "123"));
			AssertEquals("Genral is saved against correct brnach based on PIMA", dunstableBranch.PK, receivedMessage.EM_GB);
			AssertNotEquals(GlbBranch.CurrentBranch.PK, receivedMessage.EM_GB);
		}

		public void TestParseGenralFromChief()
		{
			string interchangeHeader = "UNB+UNOA:2+CUKCTM98CHFIMP:IATA+CUKFFW98000CAR:IATA+100715:1533+1028C153305000+++A'";
			string interchangeFooter = "UNZ+1+1028C153305000'";

			EDIInterchange receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(this.Factory, interchangeHeader + genralMessageEdifact_Broadcast.Replace(EDIMessage.MessageNumberPlaceHolder, "123") + interchangeFooter, "CUK");
			GenralMessageParser parser = new GenralMessageParser();
			var receivedMessage = receivedInterchange.ContainedMessages[0];
			parser.ParseMessage(receivedMessage);
			Factory.Save();
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");

			AssertContains(@"<h3>GENRAL message</h3>
<p>Sender: CUKCTM98CHFIMP (CHIEF)<br/>
Recipient: CUKFFW98000CAR<br/>
Purpose: Broadcast<br/>
Payload:</p>
<p><b><pre><i>WEASELS ARE MAMMALS", relevantEMail.Body);
			AssertEquals("GEN", receivedMessage.EM_MessageType);
			AssertEquals("BCM", receivedMessage.EM_MessageSubType);
			AssertEquals("RCV", receivedMessage.EM_Status);
		}

		public void TestParseFallbackNarrativeUpdateGenralFromCcsuk()
		{
			string interchangeHeader = "UNB+UNOA:2+CUKCTM98CHFIMP:IATA+CUKFFW98000CAR:IATA+100715:1533+1028C153305000+++A'";
			var narrativeUpdate = "UNH+20140613120554+GENRAL:0:912:UN'BGM+TXT::109'MSG+CCS-UK'FTX+AAA+++MUCR A?:12513061401 ROUTE 2 FOLLOWING FALLBACK CLEARANCE:DATE/TIME OF CUSTOMS STATUS 2014-06-13?:12?:05:AIRPORT LHR SHED CODE ABS:PLEASE CONTACT NCH HELPDESK FOR FURTHER DETAILS:'UNT+5+20140613120554'";
			string interchangeFooter = "UNZ+1+1028C153305000'";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_MasterBillNum = "12513061401";
			consol1.JK_RL_NKLoadPort = "GBLHR";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now;
			var wrapper1 = new CustomsExportConsolIntegrationWrapper(consol1, new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			wrapper1.MawbExportHelper.ME_ExportLocation = "LHR";
			wrapper1.MawbExportHelper.ME_ExportShed = "XXX";  // The goods may have been arrived at the next location, but our consol may still note the last departure. 			 

			EDIInterchange receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeHeader + narrativeUpdate + interchangeFooter, "CUK");
			var parser = new GenralMessageParser();
			var receivedMessage = receivedInterchange.ContainedMessages[0];
			parser.ParseMessage(receivedMessage);
			AssertEquals("GEN", receivedMessage.EM_MessageType);
			AssertEquals("TXT", receivedMessage.EM_MessageSubType);
			AssertContains("ROUTE 2 FOLLOWING FALLBACK CLEARANCE", receivedMessage.EM_MessageInterpretation);
			AssertEquals("", wrapper1.MawbExportHelper.ME_ChiefCustomsActionTextFromFsn);
			AssertEquals(ZDateTime.Empty, wrapper1.MawbExportHelper.ME_ChiefCustomsActionDateFromFsn);
			AssertEquals(1, consol1.Messages.Count);
		}

		public void TestParseFallbackStatusUpdateGenralFromCcsuk()
		{
			string interchangeHeader = "UNB+UNOA:2+CUKCTM98CHFIMP:IATA+CUKFFW98000CAR:IATA+100715:1533+1028C153305000+++A'";
			var statusUpdate = "UNH+123+GENRAL:0:912:UN'BGM+TXT::109'MSG+CCS-UK'FTX+AAA+++MUCR A?:12512345671 FALLBACK RELEASED 29 May 12 - 11?:30 LHRBAC'UNT+5+123'"; // NB the specs use a smart hyphen between the date and time, not a real hyphen.  This test swaps for a real hyphen.
			string interchangeFooter = "UNZ+1+1028C153305000'";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_MasterBillNum = "12512345671";
			consol1.JK_RL_NKLoadPort = "GBLHR";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now;
			var wrapper1 = new CustomsExportConsolIntegrationWrapper(consol1, new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			wrapper1.MawbExportHelper.ME_ExportLocation = "LHR";
			wrapper1.MawbExportHelper.ME_ExportShed = "XXX";  // The goods may have been arrived at the next location, but our consol may still note the last departure. 			 

			EDIInterchange receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeHeader + statusUpdate + interchangeFooter, "CUK");
			var parser = new GenralMessageParser();
			var receivedMessage = receivedInterchange.ContainedMessages[0];
			parser.ParseMessage(receivedMessage);
			AssertEquals("GEN", receivedMessage.EM_MessageType);
			AssertEquals("TXT", receivedMessage.EM_MessageSubType);
			AssertContains("FALLBACK RELEASED", receivedMessage.EM_MessageInterpretation);
			AssertEquals("FBK RELEASED LHRBAC", wrapper1.MawbExportHelper.ME_ChiefCustomsActionTextFromFsn);
			AssertEquals(new ZDateTime(2012, 5, 29, 11, 30, 0), wrapper1.MawbExportHelper.ME_ChiefCustomsActionDateFromFsn);
			AssertEquals(1, consol1.Messages.Count);
		}

		void InisialiseFallbackRegistry(bool expectExports, bool expectImports)
		{
			GBCustomsDataRegistry.Instance.ChiefFallbackExports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, expectExports);
			GBCustomsDataRegistry.Instance.ChiefFallbackImports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, expectImports);
		}

		void FallbackRunner(bool expectExports, bool exportImports, string text)
		{
			string interchangeHeader = "UNB+UNOA:2+ANYSENDER:IATA+ANYRECIPIENT:IATA+100715:1533+1028C153305000+++A'";
			string interchangeFooter = "UNZ+1+1028C153305000'";
			var receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(this.Factory, interchangeHeader + text.Replace(EDIMessage.MessageNumberPlaceHolder, "123") + interchangeFooter, "CUK");
			var parser = new GenralMessageParser();
			var receivedMessage = receivedInterchange.ContainedMessages[0];
			parser.ParseMessage(receivedMessage);
			AssertEquals(expectExports, GBCustomsDataRegistry.Instance.ChiefFallbackExports.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(exportImports, GBCustomsDataRegistry.Instance.ChiefFallbackImports.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		void MakeFurtherFallbackAssertionsForOneRun()
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains(@"
FALLBACK ANNOUNCEMENT
---------------------
Fallback for CHIEF IMPORT is being INVOKED
Time that CHIEF lost/gained responsiveness: 01/05/2011 08:01:59
Time of announcement by CCS-UK: 01/05/2011 19:20:29
Additional information: 
	CHIEF UNAVAILABLE DUE TO FAILED HARDWARE IN HMRC
	THE NETWORK CARD DONE BROKE
	I NEVER TOUCHED IT", email.Body);
			AssertContains("Purpose: Fallback announcement", email.Body);
			AssertContains("CCSUK general Fallback announcement message from ANYSENDER (CCS-UK)", email.Subject);
			var inboundMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals("RCV", inboundMessage.EM_Status);
			AssertEquals("GEN", inboundMessage.EM_MessageType);
			AssertEquals("FBK", inboundMessage.EM_MessageSubType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			// YAWN.....
			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_EmailAddress = "daniel@wisetechglobal.com";
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			var staffGroup = Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup);
			staffGroup.Staff.Add(currentUserInCurrentFactory);
			staffGroup.Staff.Add(anotherStaff);
			Factory.Save();
		}

		readonly string genralMessageEdifact_Text = "UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN+<<SYSCAR>>'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++WEASELS ARE MAMMALS FORMING THE GENUS MUSTELA OF THE MUSTELIDAE FAMILY:. THEY ARE SMALL, ACTIVE PREDATORS, LONG AND SLENDER WITH SHORT LEGS. :WEASELS VARY IN LENGTH FROM 12 TO 45 CENTIMETRES (5 TO 18 IN), AND USU:ALLY HAVE A RED OR BROWN UPPER COAT AND A WHITE BELLY; SOME POPULATION:S OF SOME SPECIES MOULT TO A WHOLLY WHITE COAT IN WINTER. THEY HAVE LO'FTX+AAA+++NG SLENDER BODIES, WHICH ENABLE THEM TO FOLLOW THEIR PREY INTO BURROWS:. THEIR TAILS MAY BE FROM 22 TO 33 CENTIMETRES (9 TO 13 IN) LONG. AS I:S TYPICAL OF SMALL CARNIVORES, WEASELS HAVE A REPUTATION FOR CLEVERNES:S AND GUILE. WEASELS FEED ON SMALL MAMMALS, AND HAVE FROM TIME TO TIME: BEEN CONSIDERED VERMIN SINCE SOME SPECIES TOOK POULTRY FROM FARMS, OR'FTX+AAA+++ RABBITS FROM COMMERCIAL WARRENS. CERTAIN SPECIES OF WEASEL AND FERRET:S HAVE BEEN REPORTED TO PERFORM THE MESMERIZING WEASEL WAR DANCE, AFTE:R FIGHTING OTHER CREATURES, OR ACQUIRING FOOD FROM COMPETING CREATURES:. IN FOLKLORE AT LEAST, THIS DANCE IS PARTICULARLY ASSOCIATED WITH THE: STOAT CITATION NEEDED  . WEASELS OCCUR ALL ACROSS THE WORLD EXCEPT FO'FTX+AAA+++R ANTARCTICA, AUSTRALIA, AND NEIGHBOURING ISLANDS.  THE ENGLISH WORD ?':WEASEL?' WAS ORIGINALLY APPLIED TO ONE SPECIES OF THE GENUS, THE EUROPE:AN FORM OF THE LEAST WEASEL (MUSTELA NIVALIS). THIS USAGE IS RETAINED :IN BRITISH ENGLISH, WHERE THE NAME IS ALSO EXTENDED TO COVER SEVERAL O:THER SMALL SPECIES OF THE GENUS. HOWEVER, IN TECHNICAL DISCOURSE AND I'UNT+8+<<MSGNO PLACEHOLDER>>'";
		string genralMessageEdifact_Broadcast
		{
			get { return genralMessageEdifact_Text.Replace("BGM+TXT:ZZZ'MSG+USER'", "BGM+BCM'MSG+CHIEF'"); }
		}
		readonly string genralMessageEdifact_TextPreformatted = "UNH+<<MSGNO PLACEHOLDER>>+GENRAL:0:912:UN+<<SYSCAR>>'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++THE WEASELS, A ROCK BAND:WEASEL GAP, PRINCE CHARLES MOUNTAINS, ANTARCTICA:WEASEL HILL, GRAHAM LAND, ANTARCTICA:PC WEASEL 2000, COMPUTER GRAPHICS PRODUCT:WEASEL PROGRAM, SIMULATION AND THOUGHT EXPERIMENT BY RICHARD DAWKINS'FTX+AAA+++HMS WEAZEL, A LIST OF BRITISH ROYAL NAVY SHIPS:I.M. WEASEL, A CHARACTER IN I AM WEASEL'UNT+6+<<MSGNO PLACEHOLDER>>'";

		readonly string thisIsAnHomageToOneVeryAnnoyingGuyAtAMajorClientWhoLooksLikeAMoleButBehavesLikeAMustela = @"Weasels are mammals forming the genus Mustela of the Mustelidae family. They are small, active predators, long and slender with short legs.
Weasels vary in length from 12 to 45 centimetres (5 to 18 in), and usually have a red or brown upper coat and a white belly; some populations of some species moult to a wholly white coat in winter. They have long slender bodies, which enable them to follow their prey into burrows. Their tails may be from 22 to 33 centimetres (9 to 13 in) long. As is typical of small carnivores, weasels have a reputation for cleverness and guile.
Weasels feed on small mammals, and have from time to time been considered vermin since some species took poultry from farms, or rabbits from commercial warrens. Certain species of weasel and ferrets have been reported to perform the mesmerizing weasel war dance, after fighting other creatures, or acquiring food from competing creatures. In folklore at least, this dance is particularly associated with the stoat[citation needed] .
Weasels occur all across the world except for Antarctica, Australia, and neighbouring islands.
 
The English word 'weasel' was originally applied to one species of the genus, the European form of the Least Weasel (Mustela nivalis). This usage is retained in British English, where the name is also extended to cover several other small species of the genus. However, in technical discourse and in American usage the term 'weasel' can refer to any member of the genus, or to the genus as a whole. Of the 17 extant species currently classified in the genus Mustela, ten have 'weasel' in their common name. Among those that do not are the stoat or ermine, the polecats or ferrets, and the European Mink (the superficially similar American Mink is now regarded as belonging in another genus, Neovison).
Collective nouns for a group of weasels include boogle, gang, pack, sneak and confusion.";
		readonly string weaselPreformattedSource = @"The Weasels, a rock band
Weasel Gap, Prince Charles Mountains, Antarctica
Weasel Hill, Graham Land, Antarctica
PC Weasel 2000, computer graphics product
Weasel program, simulation and thought experiment by Richard Dawkins
HMS Weazel, a list of British Royal Navy ships
I.M. Weasel, a character in I Am Weasel";
	}
}
