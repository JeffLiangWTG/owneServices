using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRMessageBuilderBaseOnlyTest : CMRMessageBuilderAbstractTest
	{
		public void TestGetLastBGMReference()
		{
			var messages = new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory);
			var builder = new CMRMessageBuilderTestHelper();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			builder.Messages = messages;
			AssertEquals(ZString.Empty, builder.LastOriginalBGMReference);
			var outgoingMessage = (CMREXDMessage)messages.AddNew(typeof(CMREXDMessage));
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;

			#region LastOutgoingMessage

			outgoingMessage.EM_MessageText = @"UNH+1+CUSDEC:D:99B:UN'
BGM+830:::EXD+S00059260/1:1+9'
LOC+9+AUSYD::6'
LOC+12+ZAJNB::6'
LOC+28+ZA::6'
DTM+129:20050505:102'
GIS+N:79:95'
GIS+N:107:95'
GIS+N:141:95'
RFF+AWH:A'
PAC+++N:67:95'
PAC+++OT:146:95'
TDT+20+++11'
NAD+CN+++SWAVET++JOHANNESBURG'
NAD+GO+36000082002::95'
MOA+39::AUD'
MOA+63:89983:AUD'
UNS+D'
CST+1+I::95'
FTX+AAA+++MISXED GOODS'
LOC+27++AU-NS::6'
MEA+WT++KG:1195'
MEA+ABW++NR:0'
MOA+63:89983'
RFF+HS:98090001'
UNS+S'
CNT+11:7'
CNT+36:0'
UNT+29+1'".Replace("\r\n", "");

			#endregion

			AssertEquals("S00059260/1", builder.LastOriginalBGMReference);
		}

		public void TestBGMReference()
		{
			var messages = new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory);
			var outgoingMessage = (CMREXDMessage)messages.AddNew(typeof(CMREXDMessage));
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;

			#region LastOutgoingMessage

			outgoingMessage.EM_MessageText = @"UNH+1+CUSDEC:D:99B:UN'
BGM+830:::EXD+S00059260/1:1+9'
LOC+9+AUSYD::6'
LOC+12+ZAJNB::6'
LOC+28+ZA::6'
DTM+129:20050505:102'
GIS+N:79:95'
GIS+N:107:95'
GIS+N:141:95'
RFF+AWH:A'
PAC+++N:67:95'
PAC+++OT:146:95'
TDT+20+++11'
NAD+CN+++SWAVET++JOHANNESBURG'
NAD+GO+36000082002::95'
MOA+39::AUD'
MOA+63:89983:AUD'
UNS+D'
CST+1+I::95'
FTX+AAA+++MISXED GOODS'
LOC+27++AU-NS::6'
MEA+WT++KG:1195'
MEA+ABW++NR:0'
MOA+63:89983'
RFF+HS:98090001'
UNS+S'
CNT+11:7'
CNT+36:0'
UNT+29+1'".Replace("\r\n", "");

			#endregion

			var createBuilder = new CMRMessageBuilderTestHelper();
			createBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			createBuilder.Messages = messages;
			var replaceBuilder = new CMRMessageBuilderTestHelper();
			replaceBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			replaceBuilder.Messages = messages;

			AssertEquals("<<SENDERS REFERENCE PLACE HOLDER>>/DAT1", createBuilder.BGMReference);
			AssertEquals("S00059260/1", replaceBuilder.BGMReference);
		}

		public void TestLastOriginalMessage()
		{
			var messages = new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory);
			var outgoingOriginalEXD = (CMREXDMessage)messages.AddNew(typeof(CMREXDMessage));
			outgoingOriginalEXD.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var outgoingEXDReplace = (CMREXDMessage)messages.AddNew(typeof(CMREXDMessage));
			outgoingEXDReplace.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var outgoingOriginalEMM = (CMREMMMessage)messages.AddNew(typeof(CMREMMMessage));
			outgoingOriginalEMM.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;

			var builder = new CMRMessageBuilderTestHelper();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			builder.Messages = messages;
			AssertEquals(outgoingOriginalEXD, builder.LastOriginalMessage);
		}

		public void TestLastOriginalMessage2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messages = declaration.Messages;
			var firstOutgoingOriginalEXD = (CMREXDMessage)messages.AddNew(typeof(CMREXDMessage));
			firstOutgoingOriginalEXD.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			firstOutgoingOriginalEXD.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			System.Threading.Thread.Sleep(1000);
			var secondOutgoingOriginalEXD = (CMREXDMessage)messages.AddNew(typeof(CMREXDMessage));
			secondOutgoingOriginalEXD.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			secondOutgoingOriginalEXD.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();

			var builder = new CMRMessageBuilderTestHelper();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			builder.Messages = messages;
			AssertEquals(secondOutgoingOriginalEXD, builder.LastOriginalMessage);
		}

		public void TestVersion()
		{
			var builder = new CMRMessageBuilderTestHelper();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory);
			AssertEquals(1, builder.Version);
			builder.Messages.AddNew(typeof(CMREXDMessage));
			AssertEquals(2, builder.Version);
		}

		public void TestVersionOffest()
		{
			var builder = new CMRMessageBuilderTestHelper();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory);
			AssertEquals(1, builder.Version);
			builder.VersionOffset = 1;
			AssertEquals(2, builder.Version);
		}

		public void TestServerIDGoesInBGMReference()
		{
			var builder = GetMessageBuilderToTest();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory);
			builder.PopulateBGM();
			var bgmString = builder.BGM.ToString(new Edifact.UNOACharacterSet());
			AssertEquals("Server ID Included", true, bgmString.Contains("DAT"));
		}

		public void TestCMRMessageIsBureau()
		{
			const string LocalSiteID = "AAA447Y";
			var builder = GetMessageBuilderToTest(LocalSiteID);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			var dummy = Factory.New<DummyBusinessObject>();
			builder.Messages = new EDIMessageCollection(dummy, Factory);
			var message = builder.PopulateMessagesReturningResult();
			AssertEquals("EM_MessageOwner should be the Organisation passed into the message builder", LocalSiteID, message.EM_MessageOwner);
		}

		public void TestDoNotCreateAmendmentMessagewhenGenerateMessageTextHasException()
		{
			var testDec = Factory.New<JobDeclaration>();
			var cusEntryHeader = testDec.CustomsEntryHeaders.AddNew();
			var builder = new IMDMessageBuilder(cusEntryHeader, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.Messages = new EDIMessageCollection(cusEntryHeader, Factory);
			builder.Messages.CountChanged += (s, e) =>
			{
				var newMessage = (EDIMessage)e.BizObject;
				newMessage.EM_MessageTextInfo.ValueChanged += (s, e) => throw new ApplicationException("Test exception to trigger the error flow");
			};

			AssertExceptionThrown<ApplicationException>(() => builder.PopulateMessagesReturningResult());
			AssertEquals(0, builder.Messages.Count);
		}

		CMRMessageBuilder GetMessageBuilderToTest(ZString abn) => new CMRMessageBuilderTestHelper(abn);

		protected override CMRMessageBuilder GetMessageBuilderToTest() => GetMessageBuilderToTest("");

		sealed class CMRMessageBuilderTestHelper : CMRMessageBuilder
		{
			public CMRMessageBuilderTestHelper() : this("") { }

			public CMRMessageBuilderTestHelper(ZString messageOwnerSiteID)
				: base(messageOwnerSiteID)
			{
			}

			protected override bool IsBureau => !MessageOwnerSiteID.IsEmpty;

			protected internal override void GenerateMessageText() { }

			protected internal override UNTSegment UNT => null;

			protected internal override BGMSegment BGM => bgm ?? (bgm = new BGMSegment());
			BGMSegment bgm;

			protected internal override UNHSegment UNH => null;

			protected internal override SegmentGroup EdifactMessage => new Edifact.D99B.Messages.CUSCAR.CUSCARMessage();

			protected internal override MessageTypeList UNHMessageType => null;

			protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.EXD;

			protected internal override ZString DocumentName => null;

			protected internal override DocumentNameCodeList DocumentNameCode => null;

			protected internal override Type TypeOfMessage => typeof(CMRMessage);

			protected internal override ZString MessageInterpretation => "MessageInterpretation Sample";
		}
	}
}
