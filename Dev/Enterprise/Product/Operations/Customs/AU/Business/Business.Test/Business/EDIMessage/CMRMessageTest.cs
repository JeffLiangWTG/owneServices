using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRMessage))]
	public class CMRMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestResetToQueue()
		{
			var message = Factory.New<CMRIMDRMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Sent;

			message.ResetToQueuedStatus();
			Factory.Save();

			var messageLoaded = Factory.Load<CMRMessage>(message.PK);
			AssertEquals(typeof(CMRIMDRMessage), messageLoaded.GetType());
		}

		public void TestFilterForValidEDIFACTCharacters()
		{
			AssertEquals("Valid EDIFACT Characters", "ABCDEABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,-()=?+?:?'/",
						CMRMessage.FilterForValidEDIFACTCharacters("abcdeABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,-()=+:'/!@#$%^&*"));
		}

		public void TestIsRejected()
		{
			CMRMessage message = (CMRMessage)GetNewBusinessObject();
			message.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertEquals("Is rejected", true, message.IsRejected);
		}

		public void TestCollationKey()
		{
			CMRMessage message = Factory.New<CMRMessage>();
			message.EM_MessageOwner = "MESSAGE_";
			message.EM_MessageType = "TYP";
			AssertEquals("MESSAGE_TYP", message.CollationKey);
		}

		public void TestDefaultValues()
		{
			bool isTestMode = Env.Registry.CMRTestMode;
			CMRMessage message1 = Factory.New<CMRMessage>();
			AssertEquals("TestMessage", isTestMode, message1.EM_IsTestMessage);
		}

		public void TestGetAgentReference()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarationReference = "B99999999";

			CMRMessage message1 = Factory.New<CMRMessage>();
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			var getAgentReference = message1.GetType().GetMethod("GetAgentReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

			AssertEquals("Agent Reference", "B99999999/1", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "Agent Reference";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "B99999999/1 AGENT RE", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "+:'";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "B99999999/1 ?+?:?'", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = ".,-()=/";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "B99999999/1 .,-()=/", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "Agent+:'";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "B99999999/1 AGENT?+?:?'", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "Agent{}#$^%";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "B99999999/1 AGENT", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "1234567:";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "B99999999/1 1234567?:", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "1234567:x";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "B99999999/1 1234567?:", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			message1.EM_LinkedObject = null;
			message1.ResetCachedValuesForTest();
			AssertEquals(string.Empty, getAgentReference.Invoke(message1, System.Array.Empty<object>()));
		}

		public void TestGetAgentReferenceWhereLinkedObjectIsJobDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_DeclarationReference = "B99999999";

			CMRMessage message1 = Factory.New<CMRMessage>();
			message1.EM_LinkedObject = declaration;
			message1.ResetCachedValuesForTest();
			declaration.JE_AgentsReference = "Agent{}#$^%";
			var getAgentReference = message1.GetType().GetMethod("GetAgentReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("Agent Reference", "AGENT", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "1234567890123456789'x";
			message1.EM_LinkedObject = declaration;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "1234567890123456789?'", getAgentReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "1234567890123456789'";
			message1.EM_LinkedObject = declaration;
			message1.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "1234567890123456789?'", getAgentReference.Invoke(message1, System.Array.Empty<object>()));
		}

		public void TestGetAgentReferenceWhereLinkedObjectIsConsolidatedDeclaration()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			consolidatedDeclaration.CRD_JobReferenceNumber = "CE00000001";
			var declaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_DeclarationReference = "B99999999";

			var message = Factory.New<CMRMessage>();
			message.EM_LinkedObject = consolidatedDeclaration;
			message.ResetCachedValuesForTest();
			var getAgentReference = message.GetType().GetMethod("GetAgentReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

			AssertEquals("Agent Reference", "CE00000001", getAgentReference.Invoke(message, System.Array.Empty<object>()));

			declaration.JE_AgentsReference = "Agent Reference";
			message.EM_LinkedObject = consolidatedDeclaration;
			message.ResetCachedValuesForTest();
			AssertEquals("Agent Reference", "CE00000001 AGENT REF", getAgentReference.Invoke(message, System.Array.Empty<object>()));
		}

		public void TestGetOwnerReference()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_OwnerRef = "ABCD";

			CMRMessage message1 = Factory.New<CMRMessage>();
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			var getOwnerReference = message1.GetType().GetMethod("GetOwnerReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("Owner Reference", "ABCD", getOwnerReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_OwnerRef = " .,-()=+:'/!@#$";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Owner Reference", " .,-()=?+?:?'/", getOwnerReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_OwnerRef = "12345678901234567890";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Owner Reference", "12345678901234567890", getOwnerReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_OwnerRef = "123456789012345678901";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Owner Reference", "12345678901234567890", getOwnerReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_OwnerRef = "1234567890123456789+x";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Owner Reference", "1234567890123456789?+", getOwnerReference.Invoke(message1, System.Array.Empty<object>()));

			declaration.JE_OwnerRef = "1234567890123456789+";
			message1.EM_LinkedObject = entryHeader;
			message1.ResetCachedValuesForTest();
			AssertEquals("Owner Reference", "1234567890123456789?+", getOwnerReference.Invoke(message1, System.Array.Empty<object>()));
		}

		public void TestGetOwnerReferenceWhereLinkedObjectIsJobDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_OwnerRef = "ABCD";

			CMRMessage message1 = Factory.New<CMRMessage>();
			message1.EM_LinkedObject = declaration;
			message1.ResetCachedValuesForTest();
			var getOwnerReference = message1.GetType().GetMethod("GetOwnerReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("Owner Reference", "ABCD", getOwnerReference.Invoke(message1, System.Array.Empty<object>()));
		}

		public void TestGetOwnerReferenceWhereLinkedObjectIsConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration = consolidatedDeclaration.LeadDeclaration;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_OwnerRef = "ABCD";

			var message = Factory.New<CMRMessage>();
			message.EM_LinkedObject = consolidatedDeclaration;
			message.ResetCachedValuesForTest();
			var getOwnerReference = message.GetType().GetMethod("GetOwnerReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("Owner Reference", "ABCD", getOwnerReference.Invoke(message, System.Array.Empty<object>()));
		}

		public void TestGetOwnerReferenceWhereAutoGeneratedLate()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			OrgHeader importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importer.OH_Code = "FRED";
			importer.MiscServ.OM_IMAutoImpJobRefered = false;
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();
			AssertEquals("Pre-condition", ZString.Empty, declaration.JE_OwnerRef);
			importer.MiscServ.OM_IMAutoImpJobRefered = true;
			CMRMessage message = Factory.New<CMRMessage>();
			message.EM_LinkedObject = declaration;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + " OwnersRef=(" + EDIMessage.OwnerReferencePlaceHolder + ")";
			Factory.Save();
			Assert("Owner Reference", message.EM_MessageText.Contains("OwnersRef=(1)"));
		}

		public void TestGetSendersReference()
		{
			CMRMessage message1 = Factory.New<CMRMessage>();
			message1.EM_LinkedObject = Factory.New(typeof(CusUnderbond));
			((CusUnderbond)message1.EM_LinkedObject).C4_SendersMessageReference = "1234";
			var getSendersReference = message1.GetType().GetMethod("GetSendersReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("SendersReference", "1234", getSendersReference.Invoke(message1, System.Array.Empty<object>()));
		}

		public void TestGetSendersReferenceWhereLinkedObjectIsConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			consolidatedDeclaration.CRD_JobReferenceNumber = "CE00000001";
			var declaration = consolidatedDeclaration.LeadDeclaration;
			declaration.JE_DeclarationReference = "B111111";

			var message = Factory.New<CMRMessage>();
			message.EM_LinkedObject = consolidatedDeclaration;
			var getSendersReference = message.GetType().GetMethod("GetSendersReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("SendersReference", "CE00000001", getSendersReference.Invoke(message, System.Array.Empty<object>()));
		}

		public void TestGetSendersReferenceForPartShip()
		{
			CMRMessage message1 = Factory.New<CMRMessage>();

			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MessageReference = "M00001000";
			CusPartShip partShip = hAWB.PartShips.AddNew();
			partShip.CG_MessageReference = "000001";

			message1.EM_LinkedObject = partShip;
			var getSendersReference = message1.GetType().GetMethod("GetSendersReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("SendersReference", "PM00001000/000001", getSendersReference.Invoke(message1, System.Array.Empty<object>()));
		}

		public void TestGetBGMReferenceForCUSDECMessage()
		{
			TestBGMReference(eXDText, "S00059260/SYD1");
		}

		public void TestGetBGMReferenceForCUSCARMessage()
		{
			TestBGMReference(eMMText, "K00001213/1");
		}

		public void TestGetBGMReferenceForCUSREPMessage()
		{
			TestBGMReference(dEPARTText, "K00001165/BNE1");
		}

		public void TestGetBGMReferenceForCUSRESMessage()
		{
			TestBGMReference(cUSRESText, "");
		}

		public void TestGetBGMMessageTypeForCUSDECMessage()
		{
			TestBGMMessageType(eXDText, "EXD");
		}

		public void TestBGMMessageTypeForCUSCARMessage()
		{
			TestBGMMessageType(eMMText, "EMM");
		}

		public void TestBGMMessageTypeCUSREPMessage()
		{
			TestBGMMessageType(dEPARTText, "DEPART");
		}

		public void TestBGMMessageTypeCUSRESMessage()
		{
			TestBGMMessageType(cUSRESText, "DEPARTR");
		}

		public void TestGetBGMFunctionCodeForCUSDECMessage()
		{
			TestBGMMessageFunctionCode(eXDText, MessageFunctionCodeList.Original);
		}

		public void TestGetBGMFunctionCodeeForCUSCARMessage()
		{
			TestBGMMessageFunctionCode(eMMText, MessageFunctionCodeList.Original);
		}

		public void TestGetBGMFunctionCodeeForCUSREPMessage()
		{
			TestBGMMessageFunctionCode(dEPARTText, MessageFunctionCodeList.Replace);
		}

		public void TestGetBGMFunctionCodeForCUSRESMessage()
		{
			TestBGMMessageFunctionCode(cUSRESText, "11");
		}

		public void TestGetSendersReferenceFromISendersMessageReferenceProvider()
		{
			CMRMessage message = Factory.New<CMRMessage>();
			DummySendersMessageReferenceProvider bizo = Factory.New<DummySendersMessageReferenceProvider>();
			message.EM_LinkedObject = bizo;
			var getSendersReference = message.GetType().GetMethod("GetSendersReference", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			AssertEquals("SendersReference", "12345", getSendersReference.Invoke(message, System.Array.Empty<object>()));
			AssertEquals("PopulateSendersReferenceIfNeededCallCount", 1, bizo.PopulateSendersReferenceIfNeededCallCount);
		}

		public void TestLineKeys()
		{
			var messageWithNoGroup7 = Factory.New<CMRMessage>();
			messageWithNoGroup7.EM_MessageText = eMMText;
			var keys = messageWithNoGroup7.LineKeys();
			AssertEquals("Line Keys", 0, keys.Count());

			var esmMessage = Factory.New<CMRMessage>();
			esmMessage.EM_MessageText = eSMText;
			keys = esmMessage.LineKeys();
			AssertEquals("Line Keys - Hwbs only", 2, keys.Count());

			var esmWithBlankHwbsMessage = Factory.New<CMRMessage>();
			esmWithBlankHwbsMessage.EM_MessageText = eSMTextWithEDNInsteadOfHWB;
			keys = esmWithBlankHwbsMessage.LineKeys();
			AssertEquals("Line Keys - with EDN", 4, keys.Count());
			int keyCount = 0;
			foreach (CUSCARLineKey key in keys)
			{
				var result = false;
				keyCount++;
				if (keyCount == 1)
				{
					AssertEquals("S00047551", key.houseBill);
					result = true;
				}
				else if (keyCount == 2)
				{
					AssertEquals("AAAC3RN9X", key.EDN);
					result = true;
				}
				else if (keyCount == 3)
				{
					AssertEquals("AAAC3RPAF", key.EDN);
					result = true;
				}
				else if (keyCount == 4)
				{
					AssertEquals("AAAC3RPC6", key.EDN);
					result = true;
				}

				Assert("Key values as expected ", result);
			}
		}

		#region Implementation

		#region TestHelper

		class DummySendersMessageReferenceProvider : DummyBusinessObject, ISendersMessageReferenceProvider
		{
			public DummySendersMessageReferenceProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ZString ISendersMessageReferenceProvider.SendersReference
			{
				get
				{
					return "12345";
				}
			}

			void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
			{
				PopulateSendersReferenceIfNeededCallCount++;
			}

			public int PopulateSendersReferenceIfNeededCallCount;
		}

		#endregion
		readonly string eXDText = @"UNH+1+CUSDEC:D:99B:UN'
BGM+830:::EXD+S00059260/SYD1:1+9'
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
MOA+39::AUD'4
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
UNT+29+1'";

		readonly string eMMText = @"UNH+1+CUSCAR:D:99B:UN'
BGM+85:::EMM+K00001213/1:1+9'
TDT+20+954++11++++9122447::11'
LOC+5+AUBNE::5'
LOC+28+AU::6'
DTM+136:20050426:102'
GIS+NIL:109:95'
UNT+8+1'";

		readonly string dEPARTText = @"UNH+1+CUSREP:D:99B:UN'
BGM+124:::DEPART+K00001165/BNE1:3+5'
DTM+136:20050428:102'
DTM+136:2154:401'
LOC+5+FK79B::95'
TDT+20+07B5++11+13096334771::95+++8122581::11'
LOC+8+AUPKL::6'
UNT+8+1'";

		readonly string cUSRESText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::DEPARTR+182C 5G3G 8CFF:001+11'
FTX+AHN+++CLEAR:THE NOTICE HAS BEEN ACCEPTED BY CUSTOMS.'
NAD+MR+60077306435::95'
RFF+ABO:EAGLE 1 0047::001'
RFF+ACW:DEPART'
RFF+AFM:9'
ERP+::0000'
ERC+XM0001::95'
FTX+AAO+++LODGE OR AMEND DEPARTURE REPORT FROM REPORTING PARTY ID RPID=60077306435 HAS BEEN PROCESSED'
CNT+55:01'
UNT+12+000001'";

		readonly string eSMText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:HWB0001'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:68349924'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+25+1'";
		readonly string eSMTextWithEDNInsteadOfHWB = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00027025/CMT1:1+9'TDT+20+++6'DTM+136:20150317:102'GIS+C:121:95'CNT+11:4'CNI+1'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047551'LOC+28+NZ::5'NAD+GO+++CARGOWISE AUSTRALIA'GID+1'FTX+AAA+++*HAZ* CLASS 3 UN1170'CNI+2'CNT+11:1'RFF+TN:AAAC3RN9X'GID+1'CNI+3'CNT+11:1'RFF+TN:AAAC3RPAF'GID+1'RFF+HWB:S01786236'GID+1'CNI+4'CNT+11:1'RFF+TN:AAAC3RPC6'GID+1'RFF+HWB:S00047554'GID+1'UNT+32+1'";

		void TestBGMReference(string messageText, string expectedBGMReference)
		{
			CMRMessage message = Factory.New<CMRMessage>();
			message.EM_MessageText = messageText.Replace("\r\n", "");
			AssertEquals("BGMReference", expectedBGMReference, message.BGMReference);
		}

		void TestBGMMessageType(string messageText, string expectedMessageType)
		{
			CMRMessage message = Factory.New<CMRMessage>();
			message.EM_MessageText = messageText.Replace("\r\n", "");
			AssertEquals("MessageType", expectedMessageType, message.BGMMessageType);
		}

		void TestBGMMessageFunctionCode(string messageText, string expectedMessageType)
		{
			CMRMessage message = Factory.New<CMRMessage>();
			message.EM_MessageText = messageText.Replace("\r\n", "");
			AssertEquals("MessageFunctionCode", expectedMessageType, message.BGMMessageFunctionCode);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIMessage result = (EDIMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CMRMessage>();
		}

		#endregion
	}
}
