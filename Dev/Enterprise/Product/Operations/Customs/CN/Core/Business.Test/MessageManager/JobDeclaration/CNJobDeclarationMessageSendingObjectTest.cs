using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNJobDeclarationMessageSendingObject))]
	class CNJobDeclarationMessageSendingObjectTest : Customs.Business.Testing.JobDeclarationMessageSendingObjectTest
	{
		public override void TestProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "Customs Status");
			Factory.Save();

			var code09 = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "09", "09", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			code09.ZZD_Description = "已放行";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = "11";
			instruction.CEI_SubStyle = IntelligentDeclarationTypeList.Codes.Add;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.CH_MessageType = "CUS";
			entry.CH_EntryStatus = "09";
			entry.CH_Status = "CLO";
			entry.CH_BGMReference = "REF000000001";
			entry.ManuallySetEntryNumber(Common.CusEntryNumberTypes.China.DeclarationUnifiedNumber, "000000000000000001", new ZDateTime(2019, 6, 26));
			entry.EntryNumber = "ENT00000001";

			Factory.Save();
			var messageSendingObject = new CNJobDeclarationMessageSendingObject(entry);

			CombineAssertions(() =>
			{
				AssertEquals("MessageTypeDescription", "报关单", messageSendingObject.MessageTypeDescription);
				AssertEquals("LocalReferenceNumber", "REF000000001", messageSendingObject.LocalReferenceNumber);
				AssertEquals("EntryNumber", "ENT00000001", messageSendingObject.EntryNumber);
				AssertEquals("DeclarationUnifiedNumber", "000000000000000001", messageSendingObject.DeclarationUnifiedNumber);
				AssertEquals("EntryStatus", "已放行", messageSendingObject.EntryStatus);
				AssertEquals("MessageStatusDescription", "已放行(整合申报)", messageSendingObject.MessageStatusDescription);
				AssertEquals("IntelligentDeclarationType", "添加智能辅助申报", messageSendingObject.IntelligentDeclarationType);
			});
		}

		public void TestDeclarationType()
		{
			AssertDeclarationType(false, "", "", DeclarationTypeList.Codes.IntegratedDeclaration);
			AssertDeclarationType(true, "", "", DeclarationTypeList.Codes.IntegratedDeclaration);
			AssertDeclarationType(true, ClearanceModeList.Codes.Integrated, "", DeclarationTypeList.Codes.IntegratedDeclaration);
			AssertDeclarationType(true, ClearanceModeList.Codes.TwoStep, "", DeclarationTypeList.Codes.PreliminaryDeclaration);
			AssertDeclarationType(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration, DeclarationTypeList.Codes.CompleteDeclaration);
			AssertDeclarationType(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration, DeclarationTypeList.Codes.CompleteDeclaration);
			AssertDeclarationType(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.AcknowledgedCompletedDeclaration, DeclarationTypeList.Codes.CompleteDeclaration);
			AssertDeclarationType(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.ErrorCompletedDeclaration, DeclarationTypeList.Codes.CompleteDeclaration);
			AssertDeclarationType(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.ClearedCompletedDeclaration, DeclarationTypeList.Codes.CompleteDeclaration);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClearanceMode = "XXX";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = "";
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testItem = new CNJobDeclarationMessageSendingObject(entry);
				AssertNoExceptionThrown(() =>
				{
					_ = testItem.DeclarationType;
					_ = testItem.DeclarationTypeDescription;
				});
			}
		}

		void AssertDeclarationType(bool twoSetup, string clearanceMode, string chStatus, string expectedDeclaration)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClearanceMode = clearanceMode;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = chStatus;
			var testItem = new CNJobDeclarationMessageSendingObject(entry);
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, twoSetup))
			{
				AssertEquals("DeclarationType", expectedDeclaration, testItem.DeclarationType);
				AssertEquals("DeclarationTypeDescription", Factory.GetCachedValue<DeclarationTypeList>().GetDescriptionFromCode(testItem.DeclarationType), testItem.DeclarationTypeDescription);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => new CNJobDeclarationMessageSendingObject(Factory.New<CusEntryHeader>());
	}
}
