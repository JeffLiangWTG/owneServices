using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.Edifact.D96A.Messages.CUSREP;
using Enterprise.Edifact.D96A.Messages.CUSRES;
using Enterprise.Edifact.D96A.Segments;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class D96AMessageUtilitiesGetDetailsTest : TestCaseWithFactory
	{
		public void TestGetReferenceCode()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var request = helper.GetRNSRequest("CCN 123456", "B99999998", RNSMessageTypes.Codes.StatusQuery, ZDateTime.UtcNow);
			var cusrep = (CUSREPMessage)request.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());

			var rff = D96AMessageUtilities.GetReferenceCode(cusrep.Group1, ReferenceQualifierList.CustomsDeclarationNumber);
			AssertEquals("CCN", "CCN123456", rff);
			rff = D96AMessageUtilities.GetReferenceCode(cusrep.Group1, ReferenceQualifierList.TransactionReferenceNumber);
			AssertEquals("Transaction Number", "B99999998", rff);
		}

		public void TestGetOriginalMessage()
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = testCompany.Branches.AddNew();
			testBranch.FillWithValidTestData();
			testBranch.GB_RL_NKHomePort = "CABLO";
			Factory.Save();
			var currentBranch = GlbBranch.CurrentBranch;

			var helper = new DeclarationTestHelper(Factory, true);
			var request = helper.GetRNSRequest("CCN123456", "B99999998", RNSMessageTypes.Codes.StatusQuery, ZDateTime.UtcNow);
			var request2 = helper.GetRNSRequest("CCN123456", "B99999998", RNSMessageTypes.Codes.StatusQuery, ZDateTime.UtcNow);
			var request2_1 = helper.GetRNSRequest("CCN123456", "B99999998", RNSMessageTypes.Codes.StatusQuery, ZDateTime.UtcNow.AddMinutes(1));
			var request3 = helper.GetRNSRequest("CCN123456", "B99999998", RNSMessageTypes.Codes.StatusQuery, ZDateTime.UtcNow);
			request.EM_GB = currentBranch.PK;
			request2.EM_GB = currentBranch.PK;
			request2_1.EM_GB = currentBranch.PK;
			request3.EM_GB = testBranch.PK;
			Factory.Save();
			request2.EM_MessageNum = "0002";
			request2_1.EM_MessageNum = "0002";
			Factory.Save();

			const string messageFormat = "UNH+257+CUSRES:D:96A:UN'BGM+:::489+B99999998+11'DTM+9:201006221028:203'GIS+2'ERP+2:{0}'ERC+09'RFF+XC:CCN123456'UNT+8+1'";
			string messageText = ZString.Format(messageFormat, request.EM_MessageNum);

			var ediMessage = helper.GetEDIReleaseMessage(messageText, ZDateTime.Now.AddDays(-5));
			var cusresmessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			AssertEquals("Load the match message", request, D96AMessageUtilities.GetOriginalRNSMessage(Factory, cusresmessage));

			AssertEquals(request2.EM_MessageNum, request2_1.EM_MessageNum);
			messageText = ZString.Format(messageFormat, request2.EM_MessageNum);
			ediMessage = helper.GetEDIReleaseMessage(messageText, ZDateTime.Now.AddDays(-5));
			cusresmessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			AssertEquals("Load the latest match message", request2_1, D96AMessageUtilities.GetOriginalRNSMessage(Factory, cusresmessage));

			messageText = ZString.Format(messageFormat, "0003");
			ediMessage = helper.GetEDIReleaseMessage(messageText, ZDateTime.Now.AddDays(-5));
			cusresmessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			AssertNull("No Request with message number = 0003", D96AMessageUtilities.GetOriginalRNSMessage(Factory, cusresmessage));

			messageText = ZString.Format(messageFormat, request3.EM_MessageNum);
			ediMessage = helper.GetEDIReleaseMessage(messageText, ZDateTime.Now.AddDays(-5));
			cusresmessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			AssertEquals(request3, D96AMessageUtilities.GetOriginalRNSMessage(Factory, cusresmessage));
		}

		public void TestGetServiceOption()
		{
			var bgmSection = new BGMSegmentMessageSection(1);
			var bgm = bgmSection.InstantiateAChildAndAddItToChildrenCollection();
			bgm.Parse(characterSet, "BGM+:::125+12345000067897+11");
			AssertEquals("125", D96AMessageUtilities.GetServiceOption(bgmSection));
		}

		public void TestGetProcessingIndicator()
		{
			var gisSection = new GISSegmentMessageSection(1);
			var gis = gisSection.InstantiateAChildAndAddItToChildrenCollection();
			gis.Parse(characterSet, "GIS+9");
			AssertEquals("9", D96AMessageUtilities.GetProcessingIndicator(gisSection));
		}

		public void TestGetContainers()
		{
			var eqdSection = new EQDSegmentMessageSection(1);
			eqdSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "EQD+CN+CONTAINER 1");
			eqdSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "EQD+CN+CONTAINER 2");
			eqdSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "EQD+CN+CONTAINER 3");

			var containers = D96AMessageUtilities.GetContainers(eqdSection);
			AssertEquals("Container count", 3, containers.Count());
			AssertEquals("CONTAINER 1", containers.ElementAt(0));
			AssertEquals("CONTAINER 2", containers.ElementAt(1));
			AssertEquals("CONTAINER 3", containers.ElementAt(2));
		}

		public void TestGetFreeText()
		{
			var ftxSection = new FTXSegmentMessageSection(1);
			var ftx = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
			ftx.Parse(characterSet, "FTX+AAG+++TEXT 1:TEXT 2: TEXT 3:TEXT 4:TEXT 5");
			AssertEquals("TEXT 1\r\nTEXT 2\r\n TEXT 3\r\nTEXT 4\r\nTEXT 5", D96AMessageUtilities.GetFreeText(ftxSection, TextSubjectQualifierList.PartyInstructions));
		}

		public void TestGetLocation()
		{
			var locSection = new LOCSegmentMessageSection(1);
			var loc = locSection.InstantiateAChildAndAddItToChildrenCollection();
			loc.Parse(characterSet, "LOC+22+0351:129::3021");
			AssertEquals("0351", D96AMessageUtilities.GetLocation(locSection, PlaceLocationQualifierList.CustomsOfficeOfClearance));
			AssertEquals("3021", D96AMessageUtilities.GetRelatedLocation(locSection, PlaceLocationQualifierList.CustomsOfficeOfClearance));
		}

		public void TestGetDate()
		{
			var dtmSection = new DTMSegmentMessageSection(1);
			var dtm = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
			dtm.Parse(characterSet, "DTM+9:200912221030:203");
			AssertEquals(new ZDateTime(2009, 12, 22, 10, 30, 0), D96AMessageUtilities.GetDate(dtmSection, DateTimePeriodQualifierList.ProcessingDateTime));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new CACharSet();
		}

		CACharSet characterSet;

		#endregion
	}
}
