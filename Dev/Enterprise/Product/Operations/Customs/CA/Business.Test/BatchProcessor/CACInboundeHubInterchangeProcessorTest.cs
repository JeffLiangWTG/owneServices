using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Business.MessageProcessors.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CACInboundeHubInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestSupportCARMSOAMessage()
		{
			var text = CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PAMessageText();

			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = Enterprise.Messaging.Integration.EDIInterchangeTypeList.Codes.CanadianCustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "00000000020449";
			interchangeToProcess.EI_From = "INETCECPT";
			interchangeToProcess.EI_To = "YUSAIRXPN";
			interchangeToProcess.EI_BodyText = text;
			interchangeToProcess.EI_GB = BranchFromOtherCompany.PK;
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);
			AssertEquals("EI_InterchangeNum", "00000000020449", interchangeToProcess.EI_InterchangeNum);
			AssertEquals("EI_InterchangeType", MessageTypeList.Codes.CARMStatementOfAccount, interchangeToProcess.EI_InterchangeType);
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.CACustoms, interchangeToProcess.EI_ApplicationCode);
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "00000000020449"));
			AssertNotNull("Messages spawned", message as CARMStatementOfAccountMessage);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", MessageTypeList.Codes.CARMStatementOfAccount, message.EM_MessageType);
			AssertEquals("Message sub type", "XXX", message.EM_MessageSubType);
		}

		public void TestSupportCARMDailyNoticeImporterMessage()
		{
			var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText();

			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = Enterprise.Messaging.Integration.EDIInterchangeTypeList.Codes.CanadianCustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "00000000020449";
			interchangeToProcess.EI_From = "INETCECPT";
			interchangeToProcess.EI_To = "YUSAIRXPN";
			interchangeToProcess.EI_BodyText = text;
			interchangeToProcess.EI_GB = BranchFromOtherCompany.PK;
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);
			AssertEquals("EI_InterchangeNum", "00000000020449", interchangeToProcess.EI_InterchangeNum);
			AssertEquals("EI_InterchangeType", MessageTypeList.Codes.CARMDailyNotice, interchangeToProcess.EI_InterchangeType);
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.CACustoms, interchangeToProcess.EI_ApplicationCode);
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "00000000020449"));
			AssertNotNull("Messages spawned", message as CARMDailyNoticeMessage);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", MessageTypeList.Codes.CARMDailyNotice, message.EM_MessageType);
			AssertEquals("Message sub type", CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, message.EM_MessageSubType);
		}

		public void TestSupportCARMDailyNoticeBrokerMessage()
		{
			var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeBrokerMessageText();

			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = Enterprise.Messaging.Integration.EDIInterchangeTypeList.Codes.CanadianCustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "00000000020449";
			interchangeToProcess.EI_From = "INETCECPT";
			interchangeToProcess.EI_To = "YUSAIRXPN";
			interchangeToProcess.EI_BodyText = text;
			interchangeToProcess.EI_GB = BranchFromOtherCompany.PK;
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);
			AssertEquals("EI_InterchangeNum", "00000000020449", interchangeToProcess.EI_InterchangeNum);
			AssertEquals("EI_InterchangeType", MessageTypeList.Codes.CARMDailyNotice, interchangeToProcess.EI_InterchangeType);
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.CACustoms, interchangeToProcess.EI_ApplicationCode);
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "00000000020449"));
			AssertNotNull("Messages spawned", message as CARMDailyNoticeMessage);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", MessageTypeList.Codes.CARMDailyNotice, message.EM_MessageType);
			AssertEquals("Message sub type", CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker, message.EM_MessageSubType);
		}

		public void TestSupportCADInboundInterchange()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.BatchProcessor.TestFiles.IncomingCADInterchange.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}

			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = Enterprise.Messaging.Integration.EDIInterchangeTypeList.Codes.CanadianCustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "00000000020449";
			interchangeToProcess.EI_From = "INETCECPT";
			interchangeToProcess.EI_To = "YUSAIRXPN";
			interchangeToProcess.EI_BodyText = text;
			interchangeToProcess.EI_GB = BranchFromOtherCompany.PK;
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);
			AssertEquals("EI_InterchangeNum", "2020032615254200000000020449", interchangeToProcess.EI_InterchangeNum);
			AssertEquals("EI_InterchangeType", MessageTypeList.Codes.CommercialAccountingDeclaration, interchangeToProcess.EI_InterchangeType);
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.CAIMP, interchangeToProcess.EI_ApplicationCode);
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "2020032615254200000000020449"));
			AssertNotNull("Messages spawned", message as CADMessage);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", MessageTypeList.Codes.CommercialAccountingDeclaration, message.EM_MessageType);
		}

		public void TestProcessDuplicateInterchange_CAD()
		{
			var duplicatedInterchange = Factory.New<EDIInterchange>();
			duplicatedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			duplicatedInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			duplicatedInterchange.EI_InterchangeType = Enterprise.Messaging.Integration.EDIInterchangeTypeList.Codes.CanadianCustoms;
			duplicatedInterchange.EI_Status = EDIInterchange.Status.Received;
			duplicatedInterchange.EI_InterchangeNum = "2020032615254200000000020449";
			duplicatedInterchange.EI_From = "INETCECPT";
			duplicatedInterchange.EI_To = "XXXXXXX";

			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.BatchProcessor.TestFiles.IncomingCADInterchange.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}

			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = Enterprise.Messaging.Integration.EDIInterchangeTypeList.Codes.CanadianCustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "00000000020449";
			interchangeToProcess.EI_From = "INETCECPT";
			interchangeToProcess.EI_To = "XXXXXXX";
			interchangeToProcess.EI_BodyText = text;
			interchangeToProcess.EI_GB = BranchFromOtherCompany.PK;
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Should not save duplicate interchange", EDIInterchange.Status.Error, interchangeToProcess.EI_Status);
			AssertEquals("00000000020449", interchangeToProcess.EI_InterchangeNum);
			Assert("New interchange not deleted", !interchangeToProcess.IsDeleted);
			AssertEquals(1, interchangeToProcess.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "DUPLICATE")).Length);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "2020032615254200000000020449"));
			AssertNull("Messages not spawned", message);
			Assert(interchangeProcessor.Logger.UserLogStrings.Contains("\tInbound interchange #2020032615254200000000020449 ignored, as it is a duplicate"));
		}

		public void TestProcessInterchange()
		{
			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "1234";
			interchangeToProcess.EI_From = "CACustoms";
			interchangeToProcess.EI_To = "XXXXXXX";
			interchangeToProcess.EI_BodyText = "UNB+UNOA:3+INETCECPT+XXXXXXX+090224:0604+106++++++1'UNG+CUSRES+CCR+U41091N1+090224:0604+108+UN+D:00A'UNH+103000001+CUSRES:D:00A:UN'BGM+:::687+8010S00001256D+11'DTM+9:200902240032:203'GIS+1'UNT+5+103000001'UNE+1+108'UNZ+1+106'";
			interchangeToProcess.EI_GB = BranchFromOtherCompany.PK;
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);
			AssertEquals("EI_From", "INETCECPT", interchangeToProcess.EI_From);
			AssertEquals("EI_InterchangeNum", "20090224060400000000000106", interchangeToProcess.EI_InterchangeNum);
			AssertEquals("EI_BodyText", "UNH+103000001+CUSRES:D:00A:UN'BGM+:::687+8010S00001256D+11'DTM+9:200902240032:203'GIS+1'UNT+5+103000001'", interchangeToProcess.EI_BodyText);
			AssertEquals("EI_HeaderText", "UNB+UNOA:3+INETCECPT+XXXXXXX+090224:0604+106++++++1'UNG+CUSRES+CCR+U41091N1+090224:0604+108+UN+D:00A'", interchangeToProcess.EI_HeaderText);
			AssertEquals("EI_FooterText", "UNE+1+108'UNZ+1+106'", interchangeToProcess.EI_FooterText);
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.CAACI, interchangeToProcess.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", EDIInterchange.ApplicationCodes.CACustoms, interchangeToProcess.EI_InterchangeType);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "103000001"));
			AssertNotNull("Messages spawned", message);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", "ACI", message.EM_MessageType);
		}

		protected GlbBranch BranchFromOtherCompany
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
				return Factory.LoadTop1<GlbBranch>(filter);
			}
		}

		[TestDate(2013, 9, 25, 16, 12, 42)]
		public void TestProcessDuplicateInterchange()
		{
			var duplicatedInterchange = Factory.New<EDIInterchange>();
			duplicatedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			duplicatedInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CAACI;
			duplicatedInterchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.CAACI;
			duplicatedInterchange.EI_Status = EDIInterchange.Status.Received;
			duplicatedInterchange.EI_InterchangeNum = "20090224060400000000000106";
			duplicatedInterchange.EI_From = "INETCECPT";
			duplicatedInterchange.EI_To = "XXXXXXX";

			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "1234";
			interchangeToProcess.EI_From = "CACustoms";
			interchangeToProcess.EI_To = "XXXXXXX";
			interchangeToProcess.EI_BodyText = "UNA:+.? 'UNB+UNOA:3+INETCECPT+XXXXXXX+090224:0604+106++++++1'UNG+CUSRES+CCR+U41091N1+090224:0604+108+UN+D:00A'UNH+103000001+CUSRES:D:00A:UN'BGM+:::687+8010S00001256D+11'DTM+9:200902240032:203'GIS+1'UNT+5+103000001'UNE+1+108'UNZ+1+106'";
			Factory.Save();

			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Should not save duplicate interchange", EDIInterchange.Status.Error, interchangeToProcess.EI_Status);
			AssertEquals("CACustoms", interchangeToProcess.EI_From);
			AssertEquals("1234", interchangeToProcess.EI_InterchangeNum);
			Assert("New interchange not deleted", !interchangeToProcess.IsDeleted);
			AssertEquals(1, interchangeToProcess.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "DUPLICATE")).Length);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "103000001"));
			AssertNull("Messages not spawned", message);
			Assert(interchangeProcessor.Logger.UserLogStrings.Contains("\tInbound interchange #20090224060400000000000106 ignored, as it is a duplicate"));
		}

		[TestDate(2014, 9, 25, 16, 12, 42)]
		public void TestProcessMalFormedInterchange()
		{
			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "1234";
			interchangeToProcess.EI_From = "CACustoms";
			interchangeToProcess.EI_To = "XXXXXXX";
			interchangeToProcess.EI_BodyText = "UNB+UNOA:3+INETCECPT+ZZZZZZZZ+100727:0808+50++++++1'UNG+CUSRES+CCR+U10207V1+100727:0808+50+UN+D:96A:UN'UNH+1+CUSRES:D:96A:UN:UN'BGM+:::125+10207000000044+11'DTM+9:201007270805:203'GIS+14'ERP+2:82:22'ERC+B90'UNT+7+1'UNE+1+50'UNZ+1+50'";
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);
			AssertEquals("EI_From", "INETCECPT", interchangeToProcess.EI_From);
			AssertEquals("EI_InterchangeNum", "20100727080800000000000050", interchangeToProcess.EI_InterchangeNum);
			AssertEquals("EI_BodyText", "UNH+1+CUSRES:D:96A:UN'BGM+:::125+10207000000044+11'DTM+9:201007270805:203'GIS+14'ERP+2:82:22'ERC+B90'UNT+7+1'", interchangeToProcess.EI_BodyText);
			AssertEquals("EI_HeaderText", "UNB+UNOA:3+INETCECPT+ZZZZZZZZ+100727:0808+50++++++1'UNG+CUSRES+CCR+U10207V1+100727:0808+50+UN+D:96A'", interchangeToProcess.EI_HeaderText);
			AssertEquals("EI_FooterText", "UNE+1+50'UNZ+1+50'", interchangeToProcess.EI_FooterText);
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.CAIMP, interchangeToProcess.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", MessageTypeList.Codes.EDIRelease, interchangeToProcess.EI_InterchangeType);
		}

		public void TestTypeOfImportInterchange()
		{
			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "1234";
			interchangeToProcess.EI_From = "CACustoms";
			interchangeToProcess.EI_To = "XXXXXXX";
			interchangeToProcess.EI_BodyText = "UNB+UNOA:3+INETCECPT+YUSAIRXPN+141214:2206+4078++++++1'UNG+CUSRES+CCR+U10207V1+141214:2206+813+UN+D:96A'UNH+103000001+CUSRES:D:96A:UN'BGM+:::489+80362015G+11'DTM+9:201412142204:203'GIS+2'ERP+2:1756'ERC+01'RFF+XC:80362015G'UNT+8+103000001'UNE+1+813'UNZ+1+4078'";

			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "103000001"));
			AssertNotNull("Messages spawned", message);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", "REL", message.EM_MessageType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			interchangeProcessor = new CACInboundeHubInterchangeProcessor();
		}
		CACInboundeHubInterchangeProcessor interchangeProcessor;
	}
}
