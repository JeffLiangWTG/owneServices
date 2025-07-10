using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectLookups))]
	sealed class MessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcedureCodeList()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var messageSendingObject = new MessageSendingObject(cusEntryHeader);
			AssertEquals(0, messageSendingObject.Lookups.ProcedureCodeList.Count);

			cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			messageSendingObject = new MessageSendingObject(cusEntryHeader);
			var declaration = cusEntryHeader.Declaration;

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			AssertType<JPProcedureCodeList.ExportMessageSendingObjectProcedureCodeList>(messageSendingObject.Lookups.ProcedureCodeList);

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			AssertType<JPProcedureCodeList.ImportMessageSendingObjectProcedureCodeList>(messageSendingObject.Lookups.ProcedureCodeList);
		}

		public void TestMessageActionList()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var messageSendingObject = new MessageSendingObject(cusEntryHeader);
			var actionList = messageSendingObject.Lookups.MessageActionList;

			CombineAssertions(() =>
			{
				AssertEquals(1, actionList.Count);
				AssertEquals("9", actionList[0].Code);
			});

			cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var declaration = cusEntryHeader.Declaration;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ExportControlNumber = "123";
			cusEntryHeader.CH_CEI_Instruction = instruction.PK;
			messageSendingObject = new MessageSendingObject(cusEntryHeader);

			CombineAssertions(() =>
			{
				actionList = messageSendingObject.Lookups.MessageActionList;
				AssertEquals(2, actionList.Count);
				AssertEquals("5", actionList[0].Code);
				AssertEquals("1", actionList[1].Code);
			});
		}
	}
}
