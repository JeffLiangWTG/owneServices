using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	public class MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			return new MessageSendingObject(entryHeader);
		}

		public void TestActionReadOnly()
		{
			var sendingObject = (MessageSendingObject)GetNewBusinessObject();
			Assert("Default to false.", !sendingObject.ActionInfo.ReadOnly);
			Assert("Default to false.", !sendingObject.Action_ReadOnly);

			sendingObject.Action_ReadOnly = true;
			Assert("Should be readonly.", sendingObject.ActionInfo.ReadOnly);
		}

		public void TestDefaultValue()
		{
			var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var declaration = cusEntryHeader.Declaration;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;

				AssertActionDefaultValue(string.Empty, string.Empty, JPProcedureCodeList.Codes.EDA);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Rejected, JPProcedureCodeList.Codes.EDA, JPProcedureCodeList.Codes.EDA);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Acknowledged, JPProcedureCodeList.Codes.EDA, JPProcedureCodeList.Codes.EDC);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Rejected, JPProcedureCodeList.Codes.EDA01, JPProcedureCodeList.Codes.EDA01);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Acknowledged, JPProcedureCodeList.Codes.EDA01, JPProcedureCodeList.Codes.EDE);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Acknowledged, JPProcedureCodeList.Codes.EDC, JPProcedureCodeList.Codes.EDA01);

				declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;

				AssertActionDefaultValue(string.Empty, string.Empty, JPProcedureCodeList.Codes.IDA);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Rejected, JPProcedureCodeList.Codes.IDA, JPProcedureCodeList.Codes.IDA);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Acknowledged, JPProcedureCodeList.Codes.IDA, JPProcedureCodeList.Codes.IDC);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Rejected, JPProcedureCodeList.Codes.IDA01, JPProcedureCodeList.Codes.IDA01);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Acknowledged, JPProcedureCodeList.Codes.IDA01, JPProcedureCodeList.Codes.IDE);
				AssertActionDefaultValue(JPMessageStatusList.Codes.Acknowledged, JPProcedureCodeList.Codes.IDC, JPProcedureCodeList.Codes.IDA01);

				AssertNullOrEmpty("ECRAction should be empty", new MessageSendingObject(cusEntryHeader).Action);

				var cusEntryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
				cusEntryInstruction.ExportControlNumber = "";
				cusEntryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
				AssertEquals("ECRAction should be default as 9.", ActionList.Codes.Nine, new MessageSendingObject(cusEntryHeader).Action);

				cusEntryInstruction.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "1", true);
				AssertEquals("ECRAction should be default as 5", ActionList.Codes.Five, new MessageSendingObject(cusEntryHeader).Action);
			});

			void AssertActionDefaultValue(string messgaStatus, string phase, string defaultValue)
			{
				cusEntryHeader.CH_Status = messgaStatus;
				cusEntryHeader.CH_PhaseStatus = phase;

				var messageSendingObject = new MessageSendingObject(cusEntryHeader);
				AssertEquals(defaultValue, messageSendingObject.ProcedureCode);
			}
		}
	}
}
