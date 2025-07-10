using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.Misc;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	[UseSnapshotProtection]
	public class EndToEndTests : TestCase
	{
		public void TestSendThenReceive_Legacy()
		{
			RunTestEndToEndSendOutboundMessage();
			RunTestEndToEndInboundMessageFromCcsuk(GetRandomLoopbackIpAndSaveAsCcsukRemoteAddress());
		}

		public void TestSendThenReceiveForCDS()
		{
			RunTestEndToEndSendOutboundMessageForCDS();
			RunTestEndToEndInboundMessageFromCDS(GetRandomLoopbackIpAndSaveAsCcsukRemoteAddress());
		}

		public void TestReceiveUtf8()
		{
			RunTestEndToEndInboundMessageWithUtf8();
		}

		public void TestReceiveInboundMessageInChunks()
		{
			RunTestEndToEndInboundMessageInChunks();
		}

		public void TestReceiveNothing()
		{
			RunTestEndToEndWithNothingReceived();
		}

		public void TestBadHandshake()
		{
			SetUpForFailoverApproach();
			RunTestBadHandshake(FirstIpV4InternetworkAddress);
		}

		void RunTestBadHandshake(string listenIp)
		{
			AssertExceptionThrown(typeof(NoMoreAcceptableProfilesLeftException), delegate
			{
				using (TempDirectory tempDir = new TempDirectory())
				{
					string mode = "BADHANDSHAKE";
					SpawnAndLogOn(mode, tempDir.DirectoryName, GetFreePortAbove(50000), listenIp);
					if (!proc.HasExited)
					{
						proc.Kill();
					}
					Assert("We expect a bad handshake report to be in the logs.",
							testLogger.ToString().Contains("Calling I.P. not the same as Handshake Request"));
					ErrorReporter.Clear();
				}
			});
		}

		public void TestSendThenReceive_FailoverAware()
		{
			SetUpForFailoverApproach();
			RunTestEndToEndInboundMessageFromCcsuk(FirstIpV4InternetworkAddress);
		}

		public void TestNoErrorReports()
		{
			using (Env.Instance.TemporaryServiceTaskContext("ABC", canRunInAnyBranch: true))
			{
				WaitForInboundMessage(mode: "SENDMESOMETHING", isCDS: false);

				var ri = ReceivedInterchange;
				AssertNotNull("Pre-requisite: ReceivedInterchange", ri);
				AssertEquals("ErrorReporter.TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestNoErrorReportsCDS()
		{
			using (Env.Instance.TemporaryServiceTaskContext("ABC", canRunInAnyBranch: true))
			{
				WaitForInboundMessage(mode: "SENDMESOMETHINGUTF8");

				var ri = ReceivedCDSInterchange;
				AssertNotNull("Pre-requisite: ReceivedCDSInterchange", ri);
				AssertEquals("ErrorReporter.TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			}
		}

		void RunTestEndToEndInboundMessageFromCcsuk(string listenIp)
		{
			var branchThatHasPima = GetDunstableBranchPkForTest(new BusinessObjectFactory(), "DJC", "GBMIK");
			branchThatHasPima.Factory.Save();
			using (DisposableEnvironment.ForBranch(branchThatHasPima.PK.ToGuid()))
			{
				LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, false);
			}
			AssertNotEquals(branchThatHasPima.PK, GlbBranch.CurrentBranch.PK);

			WaitForInboundMessage(mode: "SENDMESOMETHING", listenIp, isCDS: false);

			var ri = ReceivedInterchange;
			AssertNotNull(ri);
			AssertEquals("UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKFFW98000LXA:IATA+100621:1028+101AA102846000+++A'", ri.EI_HeaderText);
			AssertEquals(1, ri.ContainedMessages.Count);
			Assert("Expected EM_MessageText to contain \"UNH+09615869266168+CONTRL:4:1:UN+E0B15CA2C31845A48A5322359EE28DDB'\" but was:\r\n" + ri.ContainedMessages[0].EM_MessageText, ri.ContainedMessages[0].EM_MessageText.Contains("UNH+09615869266168+CONTRL:4:1:UN+E0B15CA2C31845A48A5322359EE28DDB'"));
			AssertEquals(branchThatHasPima.PK, ri.EI_GB);
			AssertEquals(branchThatHasPima.PK, ri.ContainedMessages[0].EM_GB);
		}

		void RunTestEndToEndInboundMessageFromCDS(string listenIp)
		{
			var initialDateTime = ZDateTime.UtcNow.ToDateTime();

			var branchThatHasPima = GetDunstableBranchPkForTest(new BusinessObjectFactory(), "DJC", "GBMIK");
			branchThatHasPima.Factory.Save();
			using (DisposableEnvironment.ForBranch(branchThatHasPima.PK.ToGuid()))
			{
				LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, false);
			}
			AssertNotEquals(branchThatHasPima.PK, GlbBranch.CurrentBranch.PK);

			WaitForInboundMessage(mode: "SENDMESOMETHINGCDS", listenIp);

			var ri = ReceivedCDSInterchange;
			AssertNotNull(ri);
			AssertEquals(@"<?ccsuk senderid=""CUKSYS98HELP01"" recipientid=""CUKFFW98000LXA""?>", ri.EI_FooterText);
			AssertEquals("", ri.EI_HeaderText);
			AssertEquals(@"<GBCustomsBusinessResponse>
<ResponseHeader>
	<ConversationID>00000000000000000000000000000000</ConversationID>
</ResponseHeader>
<ResponseBody><MetaData></MetaData></ResponseBody>
</GBCustomsBusinessResponse>", ri.EI_BodyText);
			AssertEquals(branchThatHasPima.PK, ri.EI_GB);

			var nowDateTime = ZDateTime.UtcNow.ToDateTime();
			AssertEquals("CcsukLastConnectedProfile", $"Profile: LEGACY SETTING; Participant:127.44.1.1; Local:127.44.1.1; Transport:; Remote: {listenIp}", GBCustomsDataRegistry.Instance.CcsukLastConnectedProfile.Value);
			Assert("CcsukLastReceivedMessageDateTime should have been updated", GBCustomsDataRegistry.Instance.CcsukLastReceivedMessageDateTime.Value >= initialDateTime && GBCustomsDataRegistry.Instance.CcsukLastReceivedMessageDateTime.Value <= nowDateTime);
		}

		void RunTestEndToEndInboundMessageWithUtf8()
		{
			WaitForInboundMessage(mode: "SENDMESOMETHINGUTF8");

			var ri = ReceivedCDSInterchange;
			AssertEquals(@"<GBCustomsBusinessResponse>
<ResponseHeader>
	<ConversationID>00000000000000000000000000000000</ConversationID>
</ResponseHeader>
<ResponseBody><MetaData><foo>m³</foo></MetaData></ResponseBody>
</GBCustomsBusinessResponse>", ri.EI_BodyText);
		}

		void RunTestEndToEndInboundMessageInChunks()
		{
			WaitForInboundMessage(mode: "SENDMESOMETHINGINCHUNKS");

			var ri = ReceivedCDSInterchange;
			AssertEquals(@"<GBCustomsBusinessResponse>
<ResponseHeader>
	<ConversationID>00000000000000000000000000000000</ConversationID>
</ResponseHeader>
<ResponseBody><MetaData><Data>Very long long long long long long long long long long long long long long long long message split in chunks</Data></MetaData></ResponseBody>
</GBCustomsBusinessResponse>", ri.EI_BodyText);
		}

		void RunTestEndToEndWithNothingReceived()
		{
			WaitForInboundMessage(mode: "SENDMENOTHING", waitAttempts: 1);
			AssertEquals(null, ReceivedCDSInterchange);
		}

		void RunTestEndToEndSendOutboundMessage()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				string mode = "YOUWILLRECEIVESOMETHING";
				try
				{
					SaveInterchangeToDbForUpload(true);
					SpawnAndLogOn(mode, tempDir.DirectoryName, GetFreePortAbove(50000), GetRandomLoopbackIpAndSaveAsCcsukRemoteAddress());
					session.SendAllWaitingInterchanges();
					session.ShutdownAndDisposeInstance();
					session = null;
					LookOnFileSystemForInterchange(tempDir.DirectoryName, OutboundInterchangeText);
					mawb.Reload();
					AssertEquals(PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection, mawb.PresenceOnNetworkStatus);
				}
				catch (Exception ex)
				{
					var sockets = GetActiveSocketsListForDebuggingAfterCrash();
					throw new Exception("CCSUK tests failed to execute. STDOUT from program to follow. " + outputFromExe + sockets, ex);
				}
				finally
				{
					if (proc != null && !proc.HasExited)
					{
						proc.Kill();
					}
				}
			}
		}
		
		void RunTestEndToEndSendOutboundMessageForCDS()
		{
			var initialDateTime = ZDateTime.UtcNow.ToDateTime();

			var listenIp = GetRandomLoopbackIpAndSaveAsCcsukRemoteAddress();
			using (TempDirectory tempDir = new TempDirectory())
			{
				string mode = "YOUWILLRECEIVESOMETHING";
				try
				{
					SaveCDSInterchangeToDbForUpload();
					SpawnAndLogOn(mode, tempDir.DirectoryName, GetFreePortAbove(50000), listenIp);
					session.SendAllWaitingInterchanges();
					session.ShutdownAndDisposeInstance();
					session = null;
					LookOnFileSystemForInterchange(tempDir.DirectoryName, OutboundXMLInterchangeText);
				}
				catch (Exception ex)
				{
					var sockets = GetActiveSocketsListForDebuggingAfterCrash();
					throw new Exception("CCSUK tests failed to execute. STDOUT from program to follow. " + outputFromExe + sockets, ex);
				}
				finally
				{
					if (proc != null && !proc.HasExited)
					{
						proc.Kill();
					}
				}
			}

			var nowDateTime = ZDateTime.UtcNow.ToDateTime();
			AssertEquals("CcsukLastConnectedProfile", $"Profile: LEGACY SETTING; Participant:127.44.1.1; Local:127.44.1.1; Transport:; Remote: {listenIp}", GBCustomsDataRegistry.Instance.CcsukLastConnectedProfile.Value);
			Assert("CcsukLastSentMessageDateTime should have been updated", GBCustomsDataRegistry.Instance.CcsukLastSentMessageDateTime.Value >= initialDateTime && GBCustomsDataRegistry.Instance.CcsukLastSentMessageDateTime.Value <= nowDateTime);
		}

		string GetActiveSocketsListForDebuggingAfterCrash()
		{
			string result = "";
			try
			{
				result = "\r\n\r\nActive TCP connections (local-->remote)\r\n";
				var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
				var endPoints = ipProperties.GetActiveTcpListeners();
				var tcpConnections = ipProperties.GetActiveTcpConnections();

				foreach (var info in tcpConnections)
				{
					result += info.LocalEndPoint.Address.ToString() + "\t:\t" + info.LocalEndPoint.Port.ToString()
								+ "\t\t-->\t\t" + info.RemoteEndPoint.Address.ToString() + "\t:\t" + info.RemoteEndPoint.Port.ToString()
								+ "\t\t" + info.State.ToString() + "\r\n";
				}
				result += "\r\n\r\nEnd Points\r\n";
				foreach (var info in endPoints)
				{
					result += info.Address.ToString() + "\t:\t" + info.Port.ToString() + "\r\n";
				}
			}
			catch { }
			return result;
		}

		void LookOnFileSystemForInterchange(string dirName, string expected)
		{
			AssertFileSameAsString(Path.Combine(dirName, ".interchange.txt"), expected);
		}

		void SaveInterchangeToDbForUpload(bool linkOutboundMessageToMawb = false, string interchangeNum = "14")
		{
			var factory = new BusinessObjectFactory();
			var sendThisInterchange = EDIInterchange.CreateNewInterchangeFromString(factory, OutboundInterchangeText.Replace("1124+14", "1124+" + interchangeNum), ApplicationCodeList.Codes.GbCcsuk);
			factory.Save();
			sendThisInterchange.Reload();
			sendThisInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sendThisInterchange.EI_Status = EDIInterchange.Status.Queued;

			if (linkOutboundMessageToMawb)
			{
				var messageToSend = sendThisInterchange.ContainedMessages[0];
				mawb = factory.New<CusMAWB>();
				mawb.Messages.Add(messageToSend);
			}
			factory.Save();
		}

		void SaveCDSInterchangeToDbForUpload()
		{
			var factory = new BusinessObjectFactory();

			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = "<MetaData></MetaData>";
			interchange.EI_From = "CUKFFW98000LXA";
			interchange.EI_To = "CUKSYS98HELP01";
			interchange.EI_FooterText = @"<?ccsuk senderid=""CUKFFW98000LXA"" recipientid=""CUKSYS98HELP01""?>";

			factory.Save();
			interchange.Reload();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;

			factory.Save();
		}

		void SpawnAndLogOn(string mode, string tempDirName, int portThatCcsukShouldListenOn, string ipThatFakerShouldListenOn)
		{
			session = null;

			var executableDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
			var pathToRun = Path.Combine(executableDirectory, exeName);

			if (!File.Exists(pathToRun))
			{
				throw new FileNotFoundException("The CCSUK runner executable was not found at " + pathToRun);
			}

			var args = string.Format("{0} {1} {2} {3} {4} {5}", mode, tempDirName, portThatCcsukShouldListenOn, CargoWise.Data.Db.DatabaseName, CargoWise.Data.Db.ServerName, ipThatFakerShouldListenOn);
			var startInfo = new ProcessStartInfo(pathToRun, args);
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;

			proc = new Process();
			proc.OutputDataReceived += new DataReceivedEventHandler(proc_OutputDataReceived);
			proc.StartInfo = startInfo;
			proc.Start();
			proc.BeginOutputReadLine();

			if (!processes.IsNullOrEmpty()) // we spawn two lots of the child program, one for each scenario
			{
				ClearProcesses();
			}

			processes.Add(proc);

			if (proc != null && proc.HasExited)
			{
				throw new Exception("The CCSUK runner failed to start properly. ExitCode=" + proc.ExitCode.ToString());
			}
			Thread.Sleep(3000); // wait for program to launch
			testLogger = new PasswordChangeTests.TestLogger();
			session = CcsukSession.GetSession(testLogger); // logs us on and allows receipt.
		}

		PasswordChangeTests.TestLogger testLogger;

		void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			outputFromExe += e.Data + System.Environment.NewLine;
		}
		string outputFromExe;

		void WaitForInboundMessage(string mode, string listenIp = null, bool isCDS = true, int waitAttempts = 10)
		{
			using (var tempDir = new TempDirectory())
			{
				try
				{
					var i = 0;
					SpawnAndLogOn(mode, tempDir.DirectoryName, GetFreePortAbove(50000), listenIp ?? GetRandomLoopbackIpAndSaveAsCcsukRemoteAddress());
					while ((isCDS ? ReceivedCDSInterchange : ReceivedInterchange) == null && ++i <= waitAttempts)
					{
						Thread.Sleep(3000); // gives us a chance to receive a message
					}
					session.ShutdownAndDisposeInstance();
					session = null;
				}
				catch (Exception ex)
				{
					Fail($"CCSUK tests failed to execute.\n\nException = {ex}\n\nSTDOUT from program was:\n{outputFromExe}");
				}
				finally
				{
					if (proc != null && !proc.HasExited)
					{
						try
						{
							proc.Kill();
						}
						catch { }
					}
				}
			}
		}

		EDIInterchange ReceivedInterchange
		{
			get
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
				query.AddToFilter(EDIInterchangeSchema.EI_BodyText, SQLComparisonOperator.Contains, "E0B15CA2C31845A48A5322359EE28DDB");
				return new BusinessObjectFactory().LoadTop1<EDIInterchange>(query);
			}
		}

		EDIInterchange ReceivedCDSInterchange
		{
			get
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GbCustomsDeclarationServices);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
				query.AddToFilter(EDIInterchangeSchema.EI_BodyText, SQLComparisonOperator.Contains, "<MetaData>");
				return new BusinessObjectFactory().LoadTop1<EDIInterchange>(query);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DANIELROXX");
			GBCustomsDataRegistry.Instance.CcsukLocalIpForBindingListener_Legacy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "127.44.1.1");
			GBCustomsDataRegistry.Instance.CcsukNetworkIpToDeclareForCallBack_Legacy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "127.44.1.1");
			GBCustomsDataRegistry.Instance.CcsukPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "anything");
			GBCustomsDataRegistry.Instance.CcsukIpAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CcsukIpAddressesSettingCollection());
		}

		void SetUpForFailoverApproach()
		{
			base.SetUp();
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL = FirstIpV4InternetworkAddress;
			var ipAddressesCollection = new CcsukIpAddressesSettingCollection();
			var addressOneNotUsed = ipAddressesCollection.AddNew();
			var addressTwoNotUsed = ipAddressesCollection.AddNew();
			var addressThreeUseThis = ipAddressesCollection.AddNew();
			addressOneNotUsed.Sequence = 1;
			addressOneNotUsed.FriendlyName = "One";
			addressOneNotUsed.LocalIpAddress = "5.5.5.5";  // We assume that the DAT client does not have this IP address
			addressOneNotUsed.CcsukParticipantIpAddress = "5.5.5.5";
			addressThreeUseThis.Sequence = 2;
			addressTwoNotUsed.Sequence = 1;
			addressTwoNotUsed.FriendlyName = "Two";
			addressTwoNotUsed.LocalIpAddress = "127.0.0.1";  // This address is available to client but is NOT listed as an address that it owns, so we don't see it
			addressTwoNotUsed.CcsukParticipantIpAddress = "6.6.6.6";
			addressThreeUseThis.FriendlyName = "Three";
			addressThreeUseThis.LocalIpAddress = FirstIpV4InternetworkAddress;
			addressThreeUseThis.CcsukParticipantIpAddress = addressThreeUseThis.LocalIpAddress;
			GBCustomsDataRegistry.Instance.CcsukIpAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ipAddressesCollection);
		}

		string firstIpV4InternetworkAddress;
		string FirstIpV4InternetworkAddress
		{
			get
			{
				if (string.IsNullOrEmpty(firstIpV4InternetworkAddress))
				{
					var localMachinesIpAddresses = Dns.GetHostEntry(System.Environment.MachineName).AddressList;
					firstIpV4InternetworkAddress = (from IPAddress a in localMachinesIpAddresses where a.AddressFamily == AddressFamily.InterNetwork select a.ToString()).First();  // v4
				}
				return firstIpV4InternetworkAddress;
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			ClearProcesses();
		}

		int GetFreePortAbove(int p)
		{
			var port = IpAddress.FindNextAvailablePortInRangeOnAddress(p, p + 2000, IPAddress.Parse(FirstIpV4InternetworkAddress));
			GBCustomsDataRegistry.Instance.CcsukRemotePort = port;
			return port;
		}

		string GetRandomLoopbackIpAndSaveAsCcsukRemoteAddress()
		{
			string firstTwoBytes = "127.44";  // Local, UK
			var random = new Random();
			var byteThree = random.Next(255);
			var byteFour = random.Next(255);
			var fullIp = string.Format("{0}.{1}.{2}", firstTwoBytes, byteThree, byteFour);
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL = fullIp;
			return fullIp;
		}

		void ClearProcesses()
		{
			foreach (var process in processes)
			{
				process.Close();
			}

			processes.Clear();
		}

		static GlbBranch GetDunstableBranchPkForTest(BusinessObjectFactory factory, string branchCode = "ZZZ", string port = "GBDTE")
		{
			var company = factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			company.GC_Code = branchCode;
			var dunstableBranch = company.Branches.AddNew();
			dunstableBranch.GB_Code = branchCode;
			dunstableBranch.GB_RL_NKHomePort = port;
			dunstableBranch.GB_BranchName = "Test Branch";
			factory.Save();
			return dunstableBranch;
		}

		public const string OutboundInterchangeText = "UNB+UNOA:2+CUKFFW98000CAR:IATA+CUKCTM98CHFEXP:IATA+100621:1124+14'UNH+23+UKCINV:D:00A:UN:109001+E0B15CA2C31845A48A5322359EE28DDB'BGM+EAC:105:109'UNS+D'UNS+S'UNT+5+23'UNZ+1+14'";
		public const string OutboundXMLInterchangeText = @"<MetaData></MetaData><?ccsuk senderid=""CUKFFW98000LXA"" recipientid=""CUKSYS98HELP01""?>";
		static readonly string exeName = "Enterprise.Customs.GB.Ccsuk.TestProgramExe.exe";
		CcsukSession session;
		Process proc;
		CusMAWB mawb;
		readonly List<Process> processes = new List<Process>();
	}
}
