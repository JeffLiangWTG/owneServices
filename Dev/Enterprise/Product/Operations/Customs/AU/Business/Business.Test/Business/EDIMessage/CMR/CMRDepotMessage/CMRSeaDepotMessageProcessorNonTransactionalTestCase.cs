using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[UseSnapshotProtection]
	class CMRSeaDepotMessageProcessorNonTransactionalTestCase : TestCase
	{
		public void TestSeaConcurrencyDoesNotCauseExceptionInSameProcess()
		{
			GlbGroup postMasters = null;
			GlbStaff newStaff = null;
			ZString oldEmailAddress;
			var factory = new BusinessObjectFactory();
			postMasters = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				newStaff = postMasters.Staff.AddNew();
			}
			oldEmailAddress = postMasters.Staff[0].GS_EmailAddress;
			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var currentCompany = GlbCompany.GetCurrentCompany(factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			var header = factory.New<CusOutturnHeader>();
			header.C6_SendersMessageReference = "O00000007";
			header.C6_OutturningPremiseID = "9914N";
			header.C6_LloydsIMO = "9227297";
			header.C6_VoyageNum = "101S";

			var interchange = factory.New<EDIInterchange>();
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";

			var message1 = factory.New<CMRCARSTMessage>();
			message1.EM_MessageNum = "1";
			message1.EM_MessageText = CARSTMessage1;
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_EI = interchange.PK;

			var message2 = factory.New<CMRCARSTMessage>();
			message2.EM_MessageNum = "2";
			message2.EM_MessageText = CARSTMessage2.Replace("TDT+20+987S++11++++7654321", "TDT+20+101S++11++++9227297");
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			factory.Save();

			var processor = new AUCCRSMessageProcessor();
			processor.ExecuteBatch();

			AssertLogs("Debug Log from MessageProcessor", processor.Logger, @"Processing Message #1
Processing Message #2
Saving...
2 messages processed");
		}

		public void TestSeaConcurrencyConflictWithMutex()
		{
			string log1 = @"Pre-Process Message #1
Saving...
";
			string log2 = @"
Starting quick scan.
Selected 2 messages out of first 2 messages in ???ms.
Processing Message #2
Exception processing message #2 individually: [There is another message being processed for the same outturn with Vessel Lloyds = 9227297, Premise ID = 9914N, Voyage Number = 101S.]
Starting quick scan.
Selected 1 messages out of first 1 messages in ???ms.
Processing Message #3
Saving...
1 message processed
Starting quick scan.
Selected 0 messages out of first 0 messages in ???ms.
Starting deep scan.
Selected 0 messages out of first 0 messages in ???ms.
			".Trim();
			string log3 = @"Processing Message #2
Saving...
1 message processed";

			AssertConcurrencyConflict(log1, log2, log3, true, CARSTMessage1, CARSTMessage2, SEIMessage);
		}

		public void TestSeaConcurrencyConflictWithMutexResolved()
		{
			string log1 = @"
Starting quick scan.
Selected 1 messages out of first 1 messages in ???ms.
Pre-Process Message #1
Saving...
			".Trim();

			string log2 = @"
Starting quick scan.
Selected 0 messages out of first 2 messages in ???ms.
Starting deep scan.
Selected 0 messages out of first 2 messages in ???ms.
			".Trim();

			string log3 = @"
Starting quick scan.
Selected 2 messages out of first 2 messages in ???ms.
Processing Message #2
Processing Message #3
Saving...
2 messages processed
Starting quick scan.
Selected 0 messages out of first 0 messages in ???ms.
Starting deep scan.
Selected 0 messages out of first 0 messages in ???ms.
			".Trim();

			AssertConcurrencyConflict(log1, log2, log3, false, CARSTMessage1, CARSTMessage2, SEIMessage);
		}

		public void TestAirConcurrencyConflictWithMutex()
		{
			string log1 = @"
Starting quick scan.
Selected 1 messages out of first 1 messages in ???ms.
Pre-Process Message #1
Saving...
1 message pre-processed
Processing Message #1
Saving...
1 message processed
Starting quick scan.
Selected 0 messages out of first 0 messages in ???ms.
Starting deep scan.
Selected 0 messages out of first 0 messages in ???ms.
			".Trim();
			string log2 = @"Processing Message #2
Processing Message #3
Saving...
2 messages processed";
			string log3 = @"";

			AssertConcurrencyConflict(log1, log2, log3, true,
				CARSTMessage1.Replace("TDT+20+101S++11", "TDT+20+101S++6"),
				CARSTMessage2.Replace("TDT+20+987S++11", "TDT+20+987S++6"),
				SEIMessage.Replace("TDT+20+101S++11", "TDT+20+101S++6")
			);
		}

		public void TestAirConcurrencyConflictWithMutexResolved()
		{
			string log1 = @"
Starting quick scan.
Selected 1 messages out of first 1 messages in ???ms.
Pre-Process Message #1
Saving...
1 message pre-processed
Processing Message #1
Saving...
1 message processed
Starting quick scan.
Selected 0 messages out of first 0 messages in ???ms.
Starting deep scan.
Selected 0 messages out of first 0 messages in ???ms.";

			string log2 = @"Starting quick scan.
Selected 0 messages out of first 2 messages in ???ms.
Starting deep scan.
Selected 0 messages out of first 2 messages in ???ms.
";

			string log3 = @"Processing Message #2
Processing Message #3
Saving...
2 messages processed";

			AssertConcurrencyConflict(log1, log2, log3, false,
				CARSTMessage1.Replace("TDT+20+101S++11", "TDT+20+101S++6"),
				CARSTMessage2.Replace("TDT+20+987S++11", "TDT+20+987S++6"),
				SEIMessage.Replace("TDT+20+101S++11", "TDT+20+101S++6")
			);
		}

		public void AssertConcurrencyConflict(string log1, string log2, string log3, bool disableLockingQueue, string msg1, string msg2, string msg3)
		{
			GlbGroup postMasters = null;
			GlbStaff newStaff = null;
			var oldEmailAddress = ZString.Empty;

			try
			{
				var factory = new BusinessObjectFactory();
				postMasters = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				if (postMasters.Staff.Count == 0)
				{
					newStaff = postMasters.Staff.AddNew();
				}
				oldEmailAddress = postMasters.Staff[0].GS_EmailAddress;
				postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

				var currentCompany = GlbCompany.GetCurrentCompany(factory);
				currentCompany.OrgProxy.OH_IsUnpackDepot = true;
				currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
				var header = factory.New<CusOutturnHeader>();
				header.C6_SendersMessageReference = "O00000007";
				header.C6_OutturningPremiseID = "9914N";
				header.C6_LloydsIMO = "9227297";
				header.C6_VoyageNum = "101S";

				header = factory.New<CusOutturnHeader>();
				header.C6_OutturningPremiseID = "9914N";
				header.C6_LloydsIMO = "7654321";
				header.C6_VoyageNum = "987S";

				var interchange = factory.New<EDIInterchange>();
				interchange.EI_From = "FROM";
				interchange.EI_To = "TO";

				var message1 = factory.New<CMRSEIMessage>();
				message1.EM_MessageNum = "1";
				message1.EM_MessageText = msg3;
				message1.EM_Status = EDIMessage.Status.Queued;
				message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message1.EM_EI = interchange.PK;

				var message2 = factory.New<CMRCARSTMessage>();
				message2.EM_MessageNum = "2";
				message2.EM_MessageText = msg1;
				message2.EM_Status = EDIMessage.Status.Queued;
				message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				var message3 = factory.New<CMRCARSTMessage>();
				message3.EM_MessageNum = "3";
				message3.EM_MessageText = msg2;
				message3.EM_Status = EDIMessage.Status.Queued;
				message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				factory.Save();

				var processor1 = new AUCConcurrentMessageProcessorForTest();
				processor1.DisableLockingQueue = disableLockingQueue;
				var threadExceptions = new ConcurrentBag<Exception>();

				var thread = new Thread(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							processor1.ExecuteBatch();
						}
					}
					catch (Exception ex)
					{
						threadExceptions.Add(ex);
					}
				});
				thread.IsBackground = true;
				thread.Start();

				Assert("Thread should start within 10 seconds", processor1.ProcessMessageInitiatingEvent.WaitOne(10000));
				var processor2 = new AUCCRSMessageProcessor();
				processor2.ExecuteBatch();

				Assert("message should be processed within a minute", thread.Join(60_000));

				CombineAssertions(() =>
				{
					AssertLogs("Debug Log1 from MessageProcessor", processor1.Logger, log1);
					AssertLogs("Debug Log2 from MessageProcessor", processor2.Logger, log2);
				});

				if (threadExceptions.Any())
				{
					throw new AssertionFailedError("Error in the parallel thread.", threadExceptions.First());
				}

				processor2.ExecuteBatch();

				AssertLogs("Debug Log3 from MessageProcessor", processor2.Logger, log3);
			}
			finally
			{
				if (postMasters != null && postMasters.Staff.Count > 0)
				{
					postMasters.Staff[0].GS_EmailAddress = oldEmailAddress;
				}

				if (newStaff != null)
				{
					newStaff.Delete();
				}

				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
				TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
				TestCaseHelper.ClearTable(CusOutturnSchema.Constants.TableName);
				TestCaseHelper.ClearTable(CusUnderbondSchema.Constants.TableName);
				TestCaseHelper.ClearTable(CusOutturnHeaderSchema.Constants.TableName);
			}
		}

		static void AssertLogs(string errorMessage, LoggingInformation logger, string expectedResult)
		{
			var timeRegex = new Regex(@" in \d+ms");
			var actualResults = new ZStringBuilder();
			foreach (var line in logger.UserLogStrings)
			{
				if (line.Length > 1)
				{
					actualResults.Append(timeRegex.Replace(line.Substring(1), " in ???ms"));
				}
			}

			var actualResult = actualResults.ToStringWithNewLineBetweenAppends();
			AssertContains(errorMessage, expectedResult.Trim(), actualResult);
			logger.ClearLogs();
		}

		const string CARSTMessage1 = "UNH+000010+CUSRES:D:99B:UN'BGM+34:::CARST+281B CDEH 60FG:1+8'DTM+9:20060518165938923729:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'FTX+AHN+++LCL UNDERBOND SATISFIED:YES'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:L60001007/CM21::1'RFF+MB:OBL111'RFF+BH:HOUSE333'RFF+AAQ:UBUB6969696'DOC+1'PAC+++LCL:67:95'UNT+35+000010'";
		const string CARSTMessage2 = "UNH+000010+CUSRES:D:99B:UN'BGM+34:::CARST+281B CDEH 60FG:1+8'DTM+9:20060518165938923729:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'FTX+AHN+++LCL UNDERBOND SATISFIED:YES'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+987S++11++++7654321::11'LOC+12+AUSYD::6'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:L60001007/CM21::1'RFF+MB:OBL111'RFF+BH:HOUSE333'RFF+AAQ:UBUB6969696'DOC+1'PAC+++LCL:67:95'UNT+35+000010'";
		const string SEIMessage = "UNH+000001+CUSRES:D:99B:UN\'BGM+961:::SEI+1G79 7IAF 71F9:1++11\'DTM+9:20090317154645464587:ZZZ\'TDT+20+101S++11++++9227297::11\'TDT+1++ROA\'NAD+MR+AAA374M::95\'NAD+VW+41065894724::95\'RFF+ABO:O00000007/DAT8::8\'DOC+1\'PAC+++FCL:67:95\'PAC+100++BX:185:95\'RFF+MB:OBLDPT001\'RFF+AAQ:OCLU8911239\'FTX+AAA+++CONSOLIDATED CARGO\'GIS+FFO:109:95\'MEA+AAE+G+KG:0000000023000.00\'MEA+AAE+AAL+KG:0000000020000.00\'MEA+AAE+ABJ+CM:0000000000020.00\'NAD+CN++LOCAL FORWARDER\'DOC+1\'PAC+++LCL:67:95\'PAC+20++BX:185:95\'RFF+MB:OBLDPT001\'RFF+BH:HBL001\'RFF+AAQ:OCLU8911239\'FTX+AAA+++STUFF TYPE 1\'MEA+AAE+G+KG:0000000005000.00\'MEA+AAE+AAL+KG:0000000005000.00\'MEA+AAE+ABJ+CM:0000000000005.00\'NAD+CN++CONSIGNEE\'DOC+1\'PAC+++LCL:67:95\'PAC+30++BX:185:95\'RFF+MB:OBLDPT001\'RFF+BH:HBL002\'RFF+AAQ:OCLU8911239\'PCI+28+MARKS1:MARKS2:MARKS3:MARKS4:MARKS5:MARKS6:MARKS7:MARKS8:MARKS9\'FTX+AAA+++STUFF TYPE 2\'MEA+AAE+G+KG:0000000007000.00\'MEA+AAE+AAL+KG:0000000007001.00\'MEA+AAE+ABJ+CM:0000000000007.00\'NAD+CN++CONSIGNEE 2\'UNT+43+000001\'";

		class AUCConcurrentMessageProcessorForTest : AUCConcurrentMessageProcessor
		{
			public bool DisableLockingQueue { get; set; }

			private protected override ILockMechanism GetLockMechanism()
			{
				return DisableLockingQueue ? new NoLockMechanism() : base.GetLockMechanism();
			}

			public AutoResetEvent ProcessMessageInitiatingEvent = new AutoResetEvent(false);

			protected override void ProcessMessageCore(ApplicationTypeMessageProcessor processor, EDIMessage message)
			{
				ProcessMessageInitiatingEvent.Set();
				Thread.Sleep(5000);

				base.ProcessMessageCore(processor, message);
			}
		}

		class NoLockMechanism : ILockMechanism
		{
			public bool TryGetLock(string key, out IDisposable appLock)
			{
				appLock = new DisposableAction(() => { });
				return true;
			}
		}
	}
}
