using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EDIMessageSupporterTest : TestCaseWithFactory
	{
		public void TestGetOriginalSender()
		{
			var user1 = Factory.New<GlbStaff>();
			user1.GS_Code = "UR1";
			var user2 = Factory.New<GlbStaff>();
			user2.GS_Code = "UR2";
			var user3 = Factory.New<GlbStaff>();
			user3.GS_Code = "UR3";
			var user4 = Factory.New<GlbStaff>();
			user4.GS_Code = "UR4";
			var exportMessage1 = Factory.New<EDIMessage>();
			exportMessage1.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			exportMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(1);
			exportMessage1.EM_SystemCreateUser = user1.GS_Code;
			var exportMessage2 = Factory.New<EDIMessage>();
			exportMessage2.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			exportMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(2);
			exportMessage2.EM_SystemCreateUser = user2.GS_Code;
			var amendMessage1 = Factory.New<EDIMessage>();
			amendMessage1.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
			amendMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(3);
			amendMessage1.EM_SystemCreateUser = user3.GS_Code;
			var amendMessage2 = Factory.New<EDIMessage>();
			amendMessage2.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
			amendMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(4);
			amendMessage2.EM_SystemCreateUser = user4.GS_Code;
			var responsedMessage1 = Factory.New<EDIMessage>();
			responsedMessage1.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			responsedMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(5);
			var responsedMessage2 = Factory.New<EDIMessage>();
			responsedMessage2.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			responsedMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(6);
			var allMessages = new EDIMessage[] { exportMessage1, exportMessage2, amendMessage1, amendMessage2, responsedMessage1, responsedMessage2 };
			AssertEquals(user2, allMessages.GetOriginalSender(ElectronicDocumentTypeList.Codes._830));
		}
	}
}
