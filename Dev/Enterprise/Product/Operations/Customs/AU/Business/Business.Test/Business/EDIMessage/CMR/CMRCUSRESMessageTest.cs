using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCUSRESMessage))]
	class CMRCUSRESMessageTest : CMRIncomingMessageTest
	{
		public void TestLinkOrCloneMessage()
		{
			CMRMessageRespondeeDummyBizO dummy = Factory.New<CMRMessageRespondeeDummyBizO>();
			CMRCUSRESMessage message = (CMRCUSRESMessage)Factory.New(ExpectedBusinessObjectType);

			AssertNull(message.EM_LinkedObject);
			AssertEquals(0, dummy.Messages.Count);

			CMRCUSRESMessage linkedMessage = message.LinkOrCloneMessage(dummy);
			dummy.Messages.Load();
			AssertEquals(1, dummy.Messages.Count);
			AssertEquals(message, dummy.Messages[0]);
			AssertEquals(message, linkedMessage);

			linkedMessage = message.LinkOrCloneMessage(dummy);
			dummy.Messages.Load();
			AssertEquals(1, dummy.Messages.Count);
			AssertEquals(message, dummy.Messages[0]);
			AssertEquals(message, linkedMessage);

			CMRMessageRespondeeDummyBizO dummy2 = Factory.New<CMRMessageRespondeeDummyBizO>();
			dummy2.Messages.Load();
			AssertEquals(0, dummy2.Messages.Count);
			AssertNotNull(message.EM_LinkedObject);

			linkedMessage = message.LinkOrCloneMessage(dummy2);
			dummy2.Messages.Load();
			AssertEquals(1, dummy2.Messages.Count);
			AssertEquals(linkedMessage, dummy2.Messages[0]);
			Assert(linkedMessage != message);

			AssertEquals(dummy, message.EM_LinkedObject);
			AssertEquals(dummy2, linkedMessage.EM_LinkedObject);
			AssertEquals(EDIMessage.Status.Received, linkedMessage.EM_Status);
		}

		public void TestMessageIdTypeDecidedWhenLoadingEDIMessage()
		{
			EDIMessage message = (EDIMessage)Factory.New(ExpectedBusinessObjectType);
			Factory.Save();
			TestLoadType(typeof(EDIMessage), ExpectedBusinessObjectType, message.PK);
			TestLoadType(typeof(CMRMessage), ExpectedBusinessObjectType, message.PK);
			TestLoadType(typeof(CMRIncomingMessage), ExpectedBusinessObjectType, message.PK);
			TestLoadType(typeof(CMRCUSRESMessage), ExpectedBusinessObjectType, message.PK);
		}

		public void TestIncomingInterchangeCreatesMessageOfCorrectType()
		{
			string incomingInterchangeString = incomingInterchange.Replace("<<DOCUMENT NAME PLACEHOLDER>>", DocumentName);
			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, incomingInterchangeString);
			AssertEquals("Created Message Type", ExpectedBusinessObjectType, interchange.ContainedMessages[0].GetType());
		}

		public void TestGetMessageNameFromCode()
		{
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();
			AssertEquals("CHANGE", message.GetMessageNameFromCode("4"));
			AssertEquals("REPLACE", message.GetMessageNameFromCode("5"));
			AssertEquals("ORIGINAL", message.GetMessageNameFromCode("9"));
			AssertEquals("REPLACE-HEADING-SECTION-ONLY", message.GetMessageNameFromCode("20"));
			AssertEquals("REPLACE-ITEM-DETAIL-AND-SUMMARY-ONLY", message.GetMessageNameFromCode("21"));
			AssertEquals("WITHDRAW", message.GetMessageNameFromCode("50"));
		}

		public void TestGetWrappedObjectForCusUnderbond()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_SendersMessageReference = "U00006666";
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();
			message.EM_MessageText = uBMREQEMessageText;
			message.SetEM_LinkedObject();
			AssertEquals("LinkedObject", underbond, message.EM_LinkedObject);
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			consolidatedDeclaration.CRD_JobReferenceNumber = "CE00000002";
			var message = Factory.New<CMRCUSRESMessage>();
			var messageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::EXDR+A6J4 F9GA EFD:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:CE00000002/CMT1::001'RFF+ACW:EXD'RFF+AFM:9'RFF+ED:AAAGELWTA'CNT+5:0002'UNT+10+000001'";
			message.EM_MessageText = messageText;
			message.SetEM_LinkedObject();
			AssertEquals("LinkedObject", consolidatedDeclaration, message.EM_LinkedObject);
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForExportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001111";
			var messageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::EXDR+A6J4 F9GA EFD:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:B00001111/CMT1::001'RFF+ACW:EXD'RFF+AFM:9'RFF+ED:AAAGELWTA'CNT+5:0002'UNT+10+000001'";
			var message1 = Factory.New<CMRCUSRESMessage>();
			message1.EM_MessageText = messageText;
			message1.SetEM_LinkedObject();
			AssertEquals("LinkedObject is JobDeclaration when no CusEntryHeader", declaration, message1.EM_LinkedObject);

			declaration.ActiveEntryHeaders.AddNew();
			var message2 = Factory.New<CMRCUSRESMessage>();
			message2.EM_MessageText = messageText;
			message2.SetEM_LinkedObject();
			AssertEquals("LinkedObject is CusEntryHeader when has one CusEntryHeader", declaration.ActiveEntryHeaders[0], message2.EM_LinkedObject);
		}

		public void TestCusUnderbondMessagesDetails()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_DischargePremiseID = "12345";
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();
			message.EM_LinkedObject = underbond;
			AssertEquals("Details", underbond.Details, message.GetEM_LinkedObjectDetails(false));
		}

		public void TestProcessingDate()
		{
			var message = Factory.New<CMRCUSRESMessage>();
			message.EM_MessageText = "UNH+000003+CUSRES:D:99B:UN'BGM+34:::CARST+3J9J A8C6 G8GE:1+8'DTM+9:20141217094157760565:ZZZ'DTM+132:20141217:102'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:YES'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+765++6+QF::3'LOC+12+AUBNE::6'LOC+4+9912J::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:A00266308/CMT1::1'RFF+MWB:08177661776'RFF+HWB:1999999'UNT+34+000003'";
			AssertEquals(new ZDateTime(2014, 12, 17, 09, 41, 57), message.ProcessingDate);
		}

		#region Implementation

		public class CMRMessageRespondeeDummyBizO : DummyBusinessObject, ICMRMessageRespondee
		{
			public CMRMessageRespondeeDummyBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ICMRMessageRespondee Members

			public ZString Details
			{
				get { return "Details"; }
			}

			public EDIMessageCollection Messages
			{
				get
				{
					if (messages == null)
					{
						messages = new EDIMessageCollection(this);
						this.RegisterEditableChildObject(messages);
						messages.Load();
					}
					return messages;
				}
			}
			EDIMessageCollection messages;

			public ZString ShortDescription
			{
				get { return "ShortDescription"; }
			}

			#endregion
		}

		protected const string TestMessageNumber = "9300";

		protected string DocumentName
		{
			get
			{
				return ExpectedBusinessObjectType.Name.Substring(3, ExpectedBusinessObjectType.Name.Length - 10);
			}
		}

		readonly string incomingInterchange = "UNA:+.? 'UNB+UNOC:3+AAA336C::AAA336C+AAA374M+041210:1354+00000000261664++++1++1'UNH+000001+CUSRES:D:99B:UN'BGM+961:::<<DOCUMENT NAME PLACEHOLDER>>+CCF_AAA374M_160476_ACR_1:1+11'UNT+3+000001'UNZ+1+00000000261664'";

		protected void TestLoadType(Type loadingType, Type expectedType, ZGuid pK)
		{
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertEquals("Loaded Type", ExpectedBusinessObjectType, secondFactory.Load(loadingType, pK).GetType());
		}

		protected string uBMREQEMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQE+3F2F B2A6 8JF5:001+11'
NAD+MR+AAA374M:110:95'
RFF+ACW:UBMREQ'
RFF+AFM:9'
RFF+ABO:U00006666/1::001'
DTM+310:20050401030134:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");

		#endregion
	}
}
