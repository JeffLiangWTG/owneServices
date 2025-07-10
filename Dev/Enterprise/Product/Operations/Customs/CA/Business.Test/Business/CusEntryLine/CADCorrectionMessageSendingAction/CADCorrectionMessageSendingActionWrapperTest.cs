using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CADCorrectionMessageSendingActionWrapper))]
	sealed class CADCorrectionMessageSendingActionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateReasonAndAppealsProgramCodeCombination()
		{
			var expectedError = "Only up to 3 unique reason code and appeals program can be submitted at a time.";
			var actions = wrapper.SendingActions;
			var action1 = actions.AddNew();
			action1.CSI_Code = "001";
			action1.CSI_SubType = "1";
			var action2 = actions.AddNew();
			action2.CSI_Code = "001";
			action2.CSI_SubType = "2";
			var action3 = actions.AddNew();
			action3.CSI_Code = "001";
			action3.CSI_SubType = "3";
			wrapper.RunPreSaveValidation();
			AssertNoError(wrapper.SendMessageInfo, expectedError);

			var action4 = actions.AddNew();
			action4.CSI_Code = "001";
			action4.CSI_SubType = "4";
			wrapper.RunPreSaveValidation();
			AssertHasError(wrapper.SendMessageInfo, expectedError);

			action4.CSI_SubType = "3";
			wrapper.RunPreSaveValidation();
			AssertNoError(wrapper.SendMessageInfo, expectedError);
		}

		public void TestAtLeastCheckOneRadioButton()
		{
			var expectedMessage = "Please select an option before click 'OK'.";
			wrapper.RunPreSaveValidation();
			AssertHasErrorContaining(wrapper.SendMessageInfo, expectedMessage);
			AssertHasErrorContaining(wrapper.SaveWithoutSendMessageInfo, expectedMessage);
			wrapper.SendMessage = true;
			wrapper.RunPreSaveValidation();
			AssertNoError(wrapper.SendMessageInfo, expectedMessage);
			AssertNoError(wrapper.SaveWithoutSendMessageInfo, expectedMessage);

			wrapper.SendMessage = false;
			wrapper.SaveWithoutSendMessage = true;
			wrapper.RunPreSaveValidation();
			AssertNoError(wrapper.SendMessageInfo, expectedMessage);
			AssertNoError(wrapper.SaveWithoutSendMessageInfo, expectedMessage);

			wrapper.SendMessage = true;
			wrapper.SaveWithoutSendMessage = true;
			wrapper.RunPreSaveValidation();
			AssertNoError(wrapper.SendMessageInfo, expectedMessage);
			AssertNoError(wrapper.SaveWithoutSendMessageInfo, expectedMessage);

			wrapper.SendMessage = false;
			wrapper.SaveWithoutSendMessage = false;
			wrapper.RunPreSaveValidation();
			AssertHasErrorContaining(wrapper.SendMessageInfo, expectedMessage);
			AssertHasErrorContaining(wrapper.SaveWithoutSendMessageInfo, expectedMessage);
		}

		public void TestValidateSendingActionsCount()
		{
			var expectedMessage = "At least one Amendment Detail is required.";
			wrapper.SendMessage = true;
			wrapper.ValidateSendingActionsCount();
			AssertHasRowErrorContaining(wrapper, expectedMessage);
			wrapper.SendingActions.AddNew();
			wrapper.ValidateSendingActionsCount();
			AssertNoRowError(wrapper, expectedMessage);
			wrapper.SendingActions.RemoveAndDeleteAll();
			AssertEquals(0, wrapper.SendingActions.Count);
		}

		public void TestSendingActionsClearedWhenSetSaveWithoutSendMessage()
		{
			wrapper.SendingActions.RemoveAndDeleteAll();
			wrapper.SendingActions.AddNew();
			AssertEquals(1, wrapper.SendingActions.Count);
			wrapper.SaveWithoutSendMessage = true;
			AssertEquals(0, wrapper.SendingActions.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper;
		}

		#endregion

		CADCorrectionMessageSendingActionWrapper wrapper;
		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
		}
	}
}
