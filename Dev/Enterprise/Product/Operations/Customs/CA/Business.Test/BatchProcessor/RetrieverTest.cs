using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.BatchProcessor.Testing
{
	sealed class RetrieverTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRetrieveG7Interchange()
		{
			string text = File.ReadAllText(testPath + "IncomingG7Interchange.txt");
			EDIInterchange interchange = TestRetrieveIncomingInterchange(text.Replace("\r\n", ""), EDIInterchange.ApplicationCodes.CAEXP);
			AssertEquals("MessageCount", 1, interchange.ContainedMessages.Count);
			AssertEquals("HeaderText", "UNB+UNOA:3+CBSANETWORKID+CLIENTSNETWORKID+020925:1015+12345678901234'", interchange.EI_HeaderText);
			AssertEquals("FooterText", "UNZ+1+12345678901234'", interchange.EI_FooterText);
			AssertEquals("MessageText", "UNH+12345600REFNBR+CUSRES:D:00A:UN'BGM+:::661+54321X8000002+11'DTM+9:200209251015:203'GIS+1'RFF+ED:RC123420021100001'UNT+6+12345600REFNBR'", interchange.ContainedMessages[0].EM_MessageText);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRetrieveACInterchange()
		{
			string text = File.ReadAllText(testPath + "IncomingACIInterchange.txt");
			EDIInterchange interchange = TestRetrieveIncomingInterchange(text.Replace("\r\n", ""), EDIInterchange.ApplicationCodes.CAACI);
			AssertEquals("MessageCount", 2, interchange.ContainedMessages.Count);
			AssertEquals("HeaderText", "UNB+UNOA:3+CBSANETWORKID+CLIENTSNETWORKID+040612:0855+12345678901234'", interchange.EI_HeaderText);
			AssertEquals("FooterText", "UNZ+1+12345678901234'", interchange.EI_FooterText);
			AssertEquals("Message1Text", "UNH+MSGREFNO123+CUSRES:D:00A:UN'BGM+:::687+ABCD25234+11'DTM+9:200406161523:203'GIS+17'UNT+5+MSGREFNO124'", interchange.ContainedMessages[0].EM_MessageText);
			AssertEquals("Message2Text", "UNH+MSGREFNO124+CUSRES:D:00A:UN'BGM+:::687+ABCD25234+11'DTM+9:200406122210:203'GIS+14'ERP+2:AB123456:20'ERC+312'FTX+AAO+++CB'ERP+2:AB123456:20'ERC+D40'FTX+AAO+++V2S.5F3'ERP+2:AB123456:20'ERC+1'ERC+2'FTX+AAO+++E1'FTX+AAO+++E2A'FTX+AAO+++E2B'ERP+2:AB123456:20'ERC+3'UNT+19+MSGREFNO124'", interchange.ContainedMessages[1].EM_MessageText);
		}

		[TestDate(2009, 1, 1, 10, 30, 25)]
		public void TestInvalidPath()
		{
			retriever.ResetLastRetrieveFailureTimeForTesting();
			GlbGroup group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_FullName = "blah";
			staff.GS_EmailAddress = "blah@blah.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
			CACustomsDataRegistry.Instance.MessageInputDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "!@#$%^&*(()");
			retriever.ExecuteBatch();
			AssertEquals("1 Email Sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		EDIInterchange TestRetrieveIncomingInterchange(string interchangeText, string applicationCode)
		{
			byte[] outBytes = Encoding.ASCII.GetBytes(interchangeText);
			string filename = Path.Combine(CACustomsDataRegistry.Instance.MessageInputDirectory.Value, "test1.edi");
			using (FileStream outStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				outStream.Write(outBytes, 0, outBytes.Length);
			}

			retriever.ExecuteBatch();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_From, "CBSANETWORKID");
			filter.AddToFilter(EDIInterchangeSchema.EI_To, "CLIENTSNETWORKID");
			filter.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, "12345678901234");
			var interchange = Factory.LoadTop1<EDIInterchange>(filter);
			AssertNotNull("Interchange", interchange);
			AssertEquals("ApplicationCode", applicationCode, interchange.EI_ApplicationCode);
			AssertEquals("Direction", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals("Status", EDIInterchange.Status.Received, interchange.EI_Status);
			Assert("MessageCount", interchange.ContainedMessages.Count > 0);

			Enterprise.Messaging.Business.EDIMessage message = interchange.ContainedMessages[0];
			AssertEquals("ApplicationCode", applicationCode, message.EM_ApplicationCode);
			AssertEquals("Direction", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("Status", EDIMessage.Status.Queued, message.EM_Status);
			Assert("File has been deleted", !File.Exists(filename));
			return interchange;
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			tempCIGInPath = Path.Combine(Env.TempPath, "CIGIn");
			Directory.CreateDirectory(tempCIGInPath);
			CACustomsDataRegistry.Instance.MessageInputDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, tempCIGInPath);
			retriever = new Retriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (tempCIGInPath.Contains("CIGIn"))
			{
				TempDirectory.DeleteDirectory(tempCIGInPath);
			}
		}

		Retriever retriever;
		string tempCIGInPath = "";
		readonly string testPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\BatchProcessor\TestFiles\";

		#endregion
	}
}
