using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.CNS.Testing
{
	public partial class CnsStatusUpdateMessageTest
	{
		public void TestTextSoakTestTryAllKindsOfMessages()
		{
			var fakeDec = Factory.New<JobDeclaration>();
			var fakeEntry = fakeDec.CustomsEntryHeaders.AddNew();
			fakeEntry.CH_BGMReference = "1GB945390992000-B00001002";
			int numberOfItems = 0;
			var resourceFileNames = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList();
			foreach (var resourceFileName in resourceFileNames)
			{
				if (resourceFileName.Contains(@".TextScraperSampleFiles.Good.") && resourceFileName.EndsWith(".txt"))
				{
					using (var stream = GetType().Assembly.GetManifestResourceStream(resourceFileName))
					{
						using (var sr = new StreamReader(stream))
						{
							SetupReceivedEmail(sr.ReadToEnd());
							numberOfItems++;
						}
					}
				}
			}

			Factory.Save();
			InitialiseAndRunTaskSchedule(new CnsCompassServiceTask());

			var allMessagesInDatabase = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("Processed all sample files", numberOfItems, allMessagesInDatabase.Length);
			AssertEquals("No reports where we errored in processing (FAL is OK - not every report will contain a reference number)", false, (from EDIMessage m in allMessagesInDatabase where m.EM_Status == EDIMessage.Status.Error select m).Any());

			// Check every test message has a test method
			var thisType = this.GetType();
			var methodInfos = thisType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			var untestedReportTypes = new ZStringBuilder();
			foreach (var applicationReference in (from EDIMessage m in allMessagesInDatabase select m.EM_ApplicationReference).Distinct())
			{
				var tidied = applicationReference.Replace("-", "").ToLower();
				if (!(from MethodInfo m in methodInfos where m.Name.ToLower().Contains(tidied) select m).Any())
				{
					untestedReportTypes.Append(applicationReference);
				}
			}

			if (untestedReportTypes.Length > 0)
			{
				Fail("Untested report types: " + untestedReportTypes.ToStringWithNewLineBetweenAppends());
			}
		}

		public void TestTextEmails()
		{
			SetUpEmailGroup();
			var currentBaseGroup = GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);
			GBCustomsDataRegistry.Instance.NotificationCnsTextUpdates.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, currentBaseGroup);
			var entry = SetUpEntry("SCT1TXC4M00100");
			entry.CH_BGMReference = "Daniel";
			SetUpOneEmailFromFileAndRunTask("DBEA95ED-A957-432F-ABC8-A84605A993DC.txt");
			entry.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(1, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("CNS COMPASS AGENT RENOMINATION NOTIFICATION 09-10-13 10:24 Daniel", email.Subject);
			AssertContains("has  been reassigned to agent code SENATOR ", email.Body);
			AssertContains("disney.com", email.Recipients.RecipientsAsDelimitedString());
			AssertContains("has  been reassigned to agent code SENATOR ", entry.Messages[0].EM_MessageInterpretation);
			AssertContains("<pre>", entry.Messages[0].EM_MessageInterpretation);
		}

		public void TestTextCannotFindJob()
		{
			SetUpEmailGroup();
			var currentBaseGroup = GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);
			GBCustomsDataRegistry.Instance.NotificationCnsErrors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, currentBaseGroup);
			SetUpOneEmailFromFileAndRunTask("DBEA95ED-A957-432F-ABC8-A84605A993DC.txt");
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals("FAL", message.EM_Status);
			AssertNotEquals("QUE", message.Interchange.EI_Status);
			AssertEquals(1, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Reference or job could not be found - CNS COMPASS AGENT RENOMINATION NOTIFICATION 09-10-13 10:24", email.Subject);
			AssertContains("has  been reassigned to agent code SENATOR ", email.Body);
			AssertContains("disney.com", email.Recipients.RecipientsAsDelimitedString());
		}

		public void TestTextCannotParseMessage_CannotGetId()
		{
			SetUpEmailGroup();
			SetUpOneEmailFromFileAndRunTask("Unparsable_CannotGetReportId.txt", "Bad");
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals("ERR", message.EM_Status);
			AssertNotEquals("QUE", message.Interchange.EI_Status);
			var email = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Could not determine report type - CNS COMPASS Something 01-10-13 15:55", email.Subject);
			AssertContains("Blah - meaningless report ID", email.Body);
			AssertContains("disney.com", email.Recipients.RecipientsAsDelimitedString());
		}

		public void TestTextCannotParseMessage_UnknownId()
		{
			SetUpEmailGroup();
			SetUpOneEmailFromFileAndRunTask("Unparsable_UnknownReportId.txt", "Bad");
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals("ERR", message.EM_Status);
			AssertNotEquals("QUE", message.Interchange.EI_Status);
			var email = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Unknown report type XXX-YYY-1 - CNS COMPASS Something 01-10-13 15:55", email.Subject);
			AssertContains("Blah unknwon report ID", email.Body);
			AssertContains("disney.com", email.Recipients.RecipientsAsDelimitedString());
			AssertEquals("XXX-YYY-1", message.EM_ApplicationReference);
		}

		public void TestTextCmiSlr2()
		{
			var entry = SetUpEntry("SCT1TBHHY00000");
			var cont = AddContainerToDeclaration(entry.Declaration, "TEMU3519649");
			SetUpOneEmailFromFileAndRunTask("CNS_Compass_-_SL_UNIT_RELEASED_20130407111400_4736594.txt");
			entry.Reload();
			cont.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.Released, cont.CO_MessageStatus);
		}

		public void TestTextCmiH31()
		{
			var entry = SetUpEntry("290-026187M", new ZDate(2013, 9, 25));
			SetUpOneEmailFromFileAndRunTask("8A8D3E12-281B-4527-8EBB-809443AB63A9.txt");
			entry.Reload();
			entry.Declaration.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals("01", entry.CH_ImportClearanceStatusICS);

			entry = SetUpEntry("19GB81HPQHSTSFGVR9", new ZDate(2013, 09, 25));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR9 CmiH31.txt");
			entry.Reload();
			AssertEquals("CDS example1 colons space", "RCV", entry.Messages[0].EM_Status);

			entry = SetUpEntry("19GB81HPQHSTSFGVR8", new ZDate(2013, 09, 25));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR8 CmiH31.txt");
			entry.Reload();
			AssertEquals("CDS example2 colons space", "RCV", entry.Messages[0].EM_Status);

			entry = SetUpEntry("23GB627CY77IGHUAR9", new ZDate(2023, 06, 02));
			SetUpOneEmailFromFileAndRunTask("CDS example 23GB627CY77IGHUAR9 CmiH31.txt");
			entry.Reload();
			AssertEquals("CDS example 3 with 3-digit prefix", "RCV", entry.Messages[0].EM_Status);
		}

		public void TestTextCerOff2()
		{
			var entry = SetUpEntry("PZL1B05XJ00000");
			SetUpOneEmailFromFileAndRunTask("428E8FDF-1C5F-48EF-8A43-E21B4AAA9930.txt");
			AssertEquals(1, entry.Messages.Count);
		}

		public void TestTextDtiIf1()
		{
			var entry = SetUpEntry("290-026187M", new ZDate(2013, 9, 25));
			SetUpOneEmailFromFileAndRunTask("178F5D05-E16B-4D2B-AF01-2B9DDC63C77B.txt");
			entry.Reload();
			AssertEquals("space hyphen", "RCV", entry.Messages[0].EM_Status);
			entry.Declaration.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals("009", entry.CH_IrcInventoryReturnCode);
			AssertEquals("01", entry.CH_ImportClearanceStatusICS);

			entry = SetUpEntry("19GB81HPQHSTSFGVR9", new ZDate(2013, 09, 25));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR9 DtiIf1.txt");
			entry.Reload();
			AssertEquals("CDS example1 colons space", "RCV", entry.Messages[0].EM_Status);

			entry = SetUpEntry("19GB81HPQHSTSFGVR8", new ZDate(2013, 09, 25));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR8 DtiIf1.txt");
			entry.Reload();
			AssertEquals("CDS example2 colons space", "RCV", entry.Messages[0].EM_Status);

			entry = SetUpEntry("23GB627CY77IGHUAR9", new ZDate(2023, 06, 02));
			SetUpOneEmailFromFileAndRunTask("CDS example 23GB627CY77IGHUAR9 DtiIf1.txt");
			entry.Reload();
			AssertEquals("CDS example 3 with 3-digit prefix", "RCV", entry.Messages[0].EM_Status);
		}

		public void TestTextCmiClr1()
		{
			var entry = SetUpEntry("150-011691T", new ZDate(2013, 09, 23));
			SetUpOneEmailFromFileAndRunTask("1BB3DBA6-81F1-4FBC-8E69-5EF669231038.txt");
			entry.Reload();
			AssertEquals("space hyphen", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
			AssertEquals(new ZDateTime(2013, 09, 25, 10, 36, 0), entry.CH_EntryReleaseDate);
			AssertEquals(1, entry.Messages.Count);

			entry = SetUpEntry("19GB81HPQHSTSFGVR9", new ZDate(2014, 09, 23));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR9 CmiClr1.txt");
			entry.Reload();
			AssertEquals("CDS example1 colons space", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);

			entry = SetUpEntry("19GB81HPQHSTSFGVR8", new ZDate(2014, 09, 23));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR8 CmiClr1.txt");
			entry.Reload();
			AssertEquals("CDS example2 space space", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);

			entry = SetUpEntry("23GB627CY77IGHUAR9", new ZDate(2023, 06, 02));
			SetUpOneEmailFromFileAndRunTask("CDS example 23GB627CY77IGHUAR9 CmiClr1.txt");
			entry.Reload();
			AssertEquals("CDS example 3 with 3-digit prefix", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
		}

		public void TestTextCmiClr2()
		{
			var entry = SetUpEntry("290-028031L", new ZDate(2013, 09, 26));

			SetUpOneEmailFromFileAndRunTask("B9E78FDB-C286-4EB4-8C21-B4822C1DB1E0.txt");
			entry.Reload();
			AssertEquals("colons hyphen", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
			AssertEquals(new ZDateTime(2013, 10, 2, 23, 48, 0), entry.CH_EntryReleaseDate);
			AssertEquals(1, entry.Messages.Count);

			entry = SetUpEntry("19GB81HPQHSTSFGVR9", new ZDate(2019, 07, 29));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR9 CmiClr2.txt");
			entry.Reload();
			AssertEquals("CDS example1 space space", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);

			entry = SetUpEntry("19GB81HPQHSTSFGVR8", new ZDate(2019, 07, 29));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR8 CmiClr2.txt");
			entry.Reload();
			AssertEquals("CDS example2 space space", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);

			entry = SetUpEntry("23GB627CY77IGHUAR9", new ZDate(2023, 06, 02));
			SetUpOneEmailFromFileAndRunTask("CDS example 23GB627CY77IGHUAR9 CmiClr2.txt");
			entry.Reload();
			AssertEquals("CDS example 3 with 3-digit prefix", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
		}

		public void TestTextCmiNom1()
		{
			var entry = SetUpEntry("PZQ1B056C00900");
			SetUpOneEmailFromFileAndRunTask("24E31FEA-462E-4130-AEEB-B2D5679D1A66.txt");
			AssertEquals(1, entry.Messages.Count);
		}

		public void TestTextCmiAdv1()
		{
			var entry = SetUpEntry("JWQ1J11T500100");
			SetUpOneEmailFromFileAndRunTask("9B1E0F13-9C08-44AF-A28C-2208772BD593.txt");
			AssertEquals(1, entry.Messages.Count);
		}
		public void TestTextCmiNom4()
		{
			var entry = SetUpEntry("SCT1TWKN000100");
			SetUpOneEmailFromFileAndRunTask("1B1785DD-AFD7-429C-995B-4A104BDEEC4F.txt");
			AssertEquals(1, entry.Messages.Count);
		}

		public void TestTextCmiExc1()
		{
			var entry = SetUpEntry("290-001198N", new ZDate(2013, 10, 1));
			SetUpOneEmailFromFileAndRunTask("000C7AB8-AE8B-434A-89DB-5B1A929BF9F1.txt");
			entry.Reload();
			AssertEquals("colons", "RCV", entry.Messages[0].EM_Status);
			AssertEquals(1, entry.Messages.Count);

			entry = SetUpEntry("290-001198T", new ZDate(2014, 10, 1));
			SetUpOneEmailFromFileAndRunTask("000C7AB8-AE8B-434A-89DB-5B1A929BF9F2.txt");
			entry.Reload();
			AssertEquals("space", "RCV", entry.Messages[0].EM_Status);
		}

		public void TestTextCerRel2_ChangeOfReleasee()
		{
			var entry = SetUpEntry("PGM1B093700500");
			var cont = AddContainerToDeclaration(entry.Declaration, "DRYU9373420");
			SetUpOneEmailFromFileAndRunTask("1107419D-FA97-4C5F-8F8F-988625B8E5BB.txt");
			entry.Reload();
			cont.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.NoInformation, cont.CO_MessageStatus);
		}

		public void TestTextCerRel2_Released()
		{
			var entry = SetUpEntry("SCT1TVVN300000");
			var cont = AddContainerToDeclaration(entry.Declaration, "MEDU8690116");
			SetUpOneEmailFromFileAndRunTask("5373A58D-7DB3-48D8-AAA4-005648221B53.txt");
			entry.Reload();
			cont.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.Released, cont.CO_MessageStatus);
		}

		public void TestTextCmiHld1_AddHold()
		{
			var entry = SetUpEntry("RSL10046Y00500");
			var container1 = AddContainerToDeclaration(entry.Declaration, "NYKU4055649");
			var container2 = AddContainerToDeclaration(entry.Declaration, "POOP1234567");
			SetUpOneEmailFromFileAndRunTask("3B37F5AE-5EA7-4708-9488-872FBA63DC1C.txt");
			entry.Reload();
			container1.Reload();
			container2.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.HoldIsAdded, container1.CO_MessageStatus);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.NoInformation, container2.CO_MessageStatus);
			AssertMostRecentLog("Hold-B", entry, new ZDateTime(2013, 10, 07, 12, 27, 0));
		}

		public void TestTextCmiHld1_RemoveHold()
		{
			var entry = SetUpEntry("JWQ1J1MH000000");
			var container1 = AddContainerToDeclaration(entry.Declaration, "TCLU2722079");
			var container2 = AddContainerToDeclaration(entry.Declaration, "POOP1234567");
			SetUpOneEmailFromFileAndRunTask("0473569D-F448-4F3A-ADA0-E510D170AB86.txt");
			entry.Reload();
			container1.Reload();
			container2.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.AtLeastOneHoldRemovedOthersOrNoneMayRemain, container1.CO_MessageStatus);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.NoInformation, container2.CO_MessageStatus);
			AssertMostRecentLog("HoldRemoved-C3", entry, new ZDateTime(2013, 10, 07, 10, 46, 0));
		}

		public void TestTextCmiHld2()
		{
			var entry = SetUpEntry("PXN1B058600500");
			var container1 = AddContainerToDeclaration(entry.Declaration, "CCLU3917762");
			SetUpOneEmailFromFileAndRunTask("82A14ECE-FDD1-42EA-A3E0-D4D577EFF92E.txt");
			entry.Reload();
			container1.Reload();
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.AtLeastOneHoldRemovedOthersOrNoneMayRemain, container1.CO_MessageStatus);
			AssertMostRecentLog("HoldRemoved-C", entry, new ZDateTime(2013, 9, 24, 18, 21, 0));
		}

		public void TestTextCerClr2()
		{
			var entry = SetUpEntry("290-024130L", new ZDate(2013, 09, 23));
			SetUpOneEmailFromFileAndRunTask("051C0599-4A15-4849-99B1-0A28F72F7ADC.txt");
			entry.Reload();
			AssertEquals("space hyphen", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
			AssertEquals(new ZDateTime(2013, 09, 23, 20, 22, 0), entry.CH_EntryReleaseDate);
			AssertEquals(1, entry.Messages.Count);

			entry = SetUpEntry("19GB81HPQHSTSFGVR9", new ZDate(2014, 09, 23));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR9 CerClr.txt");
			entry.Reload();
			AssertEquals("CDS example1 colons space", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);

			entry = SetUpEntry("19GB81HPQHSTSFGVR8", new ZDate(2014, 09, 23));
			SetUpOneEmailFromFileAndRunTask("CDS example 19GB81HPQHSTSFGVR8 CerClr.txt");
			entry.Reload();
			AssertEquals("CDS example2 colons space", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);

			entry = SetUpEntry("23GB60E8K2ZSXIMAR1", new ZDate(2023, 06, 01));
			SetUpOneEmailFromFileAndRunTask("CDS example 23GB60E8K2ZSXIMAR1 CerClr.txt");
			entry.Reload();
			AssertEquals("CDS example 3 with 3-digit prefix", EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
		}

		public void TestTextCmiRel4_MatchByEntryNumberNoAmbiguity()
		{
			GBCustomsDataRegistry.Instance.CnsSaveStatusNotificationsToEDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var entry = SetUpEntry("XXX-002152M", new ZDate(2016, 1, 1), "BTM2PBLWK00100");
			SetUpOneEmailFromFileAndRunTask("CNS_Compass_-_MI_Gate_Pass_20161230122325_2 (2).txt");
			entry.Reload();
			AssertEquals(EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
			AssertEquals(new ZDate(2016, 12, 30), entry.CH_EntryReleaseDate.Date);
			AssertEquals(1, entry.Messages.Count);
			var docManagerInfo = ((IDocManagerSupport)entry.Declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			var eDoc = docManagerInfo.Files[0];
			using (var streamReader = new StreamReader(eDoc.GetImageDataReader(), Encoding.ASCII))
			{
				AssertContains("CARGO  HANDLER  - REMOVAL  NOTE", streamReader.ReadToEnd());
			}
		}

		public void TestTextCmiRel4_MultipleLines()
		{
			GBCustomsDataRegistry.Instance.CnsSaveStatusNotificationsToEDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var entry1 = SetUpEntry("XXX-001869K", new ZDate(2016, 1, 1), "BTM2PBLY500100");
			var entry2 = SetUpEntry("XXX-002869K", new ZDate(2016, 1, 1), "BTM2PBLY500200");
			var entry3 = SetUpEntry("XXX-003869K", new ZDate(2016, 1, 1), "BTM2PBLY500300");
			SetUpOneEmailFromFileAndRunTask("CNS_Compass_-_MI_Gate_Pass_20161223091409_2 (4).txt");
			entry1.Reload();
			entry2.Reload();
			entry3.Reload();
			AssertEquals(EntryStatusList.Codes.Clear, entry1.CH_EntryStatus);
			AssertEquals(EntryStatusList.Codes.Clear, entry2.CH_EntryStatus);
			AssertEquals(EntryStatusList.Codes.Clear, entry3.CH_EntryStatus);
			AssertEquals(new ZDate(2016, 12, 21), entry1.CH_EntryReleaseDate.Date);
			AssertEquals(new ZDate(2016, 12, 22), entry2.CH_EntryReleaseDate.Date);
			AssertEquals(new ZDate(2016, 12, 23), entry3.CH_EntryReleaseDate.Date);
			AssertEquals(1, entry1.Messages.Count + entry2.Messages.Count + entry3.Messages.Count);
			foreach (var entry in new[] { entry1, entry2, entry3 })
			{
				var docManagerInfo = ((IDocManagerSupport)entry.Declaration).DocManagerInfo;
				AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
				var eDoc = docManagerInfo.Files[0];

				using (var streamReader = new StreamReader(eDoc.GetImageDataReader(), Encoding.ASCII))
				{
					AssertContains("CARGO  HANDLER  - REMOVAL  NOTE", streamReader.ReadToEnd());
				}
			}
		}

		public void TestTextCmiRel4_MatchByEntryNumberMultipleNumbersExistInSameMonthDifferentiateByUcn()
		{
			var entry1 = SetUpEntry("XXX-002152M", new ZDate(2016, 12, 30), "BADUCN");
			var entry2 = SetUpEntry("YYY-002152M", new ZDate(2016, 12, 30), "STILLBAD");
			var entry3 = SetUpEntry("ZZZ-002152M", new ZDate(2016, 12, 30), "BTM2PBLWK00100"); // this is the one we'll match
			SetUpOneEmailFromFileAndRunTask("CNS_Compass_-_MI_Gate_Pass_20161230122325_2 (2).txt");
			entry1.Reload();
			entry2.Reload();
			entry3.Reload();
			AssertNotEquals(EntryStatusList.Codes.Clear, entry1.CH_EntryStatus);
			AssertNotEquals(EntryStatusList.Codes.Clear, entry2.CH_EntryStatus);
			AssertEquals(EntryStatusList.Codes.Clear, entry3.CH_EntryStatus);
			AssertEquals(new ZDate(2016, 12, 30), entry3.CH_EntryReleaseDate.Date);
			AssertEquals(1, entry1.Messages.Count + entry2.Messages.Count + entry3.Messages.Count);
		}

		public void TestTextAddToEDocs()
		{
			GBCustomsDataRegistry.Instance.CnsSaveStatusNotificationsToEDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var entry = SetUpEntry("065-000127J", new ZDate(2013, 10, 1));
			SetUpOneEmailFromFileAndRunTask("AB6CAF1A-8F47-43CD-B432-2976B69CAF6E.txt");
			entry.Reload();
			AssertEquals(EntryStatusList.Codes.Clear, entry.CH_EntryStatus);
			AssertEquals(1, entry.Messages.Count);
			AssertEquals("RCV", entry.Messages[0].Interchange.EI_Status);
			var docManagerInfo = ((IDocManagerSupport)entry.Declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			var eDoc = docManagerInfo.Files[0];
			AssertEquals("CNS COMPASS CLEARANCE ADVICE REPORT Issued 04-10-13 11 06 entry 065-000127J.txt", eDoc.FileName);
			using (var streamReader = new StreamReader(eDoc.GetImageDataReader(), Encoding.ASCII))
			{
				AssertContains("WOODEN DECORATIONS / GARMENTS", streamReader.ReadToEnd());
			}
		}

		void SetUpOneEmailFromFileAndRunTask(string resourceId, string goodOrBad = "Good")
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.GB.CNS.Testing.TextScraperSampleFiles." + goodOrBad + "." + resourceId))
			{
				using (var sr = new StreamReader(stream))
				{
					SetupReceivedEmail(sr.ReadToEnd());
				}
			}
			Factory.Save();
			InitialiseAndRunTaskSchedule(new CnsCompassServiceTask());
		}

		CusEntryHeader SetUpEntry(string entryNumber, ZDate entryDate, string masterUCR = "")
		{
			var dec = Factory.New<JobDeclaration>();
			if (!string.IsNullOrEmpty(masterUCR))
			{
				dec.JE_MasterUCR = masterUCR;
			}

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = entryNumber;
			entry.CusEntryNumber.CE_IssueDate = entryDate;
			return entry;
		}

		CusEntryHeader SetUpEntry(string ucn)
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_MasterUCR = ucn;
			return entry;
		}

		EU.Business.Declaration.CusContainer AddContainerToDeclaration(JobDeclaration dec, string containerNumber)
		{
			var container = dec.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			return container;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			branchEnvironment = Enterprise.Environment.DisposableEnvironment.ForBranch(Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDownCore()
		{
			base.TearDownCore();
			branchEnvironment.Dispose();
		}

		void AssertMostRecentLog(string p, CusEntryHeader entry, ZDateTime eventTime)
		{
			((ILogsInternals)entry.Logs).ReloadFromDB();
			var log = entry.GetLogs().MostRecentLog;
			AssertEquals(p, log.SL_Reference);
			AssertEquals(eventTime, log.SL_EventTime);
		}
	}
}
