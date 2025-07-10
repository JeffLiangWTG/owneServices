using System;
using CargoWise.Customs.DE.MessageDefinitions.ZHub;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
	{
		public void TestSetDefaultMessageType_Arrival()
		{
			const string anyValue = "any value";
			const string rejectedMessageStatus = nameof(CUSINFReferencedMessageStatus.REJ);

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var sendingObject = new NctsHeaderMessageSendingObject(header);

			var testCases = new[]
			{
				new { MessageStatus = string.Empty, CustomsStatus = anyValue, Message = "BM_MessageStatus is empty", ExpectedMessageType = NctsMessageTypeList.Codes.DESNOT },
				new { MessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent, CustomsStatus = anyValue, Message = "BM_MessageStatus is MAN", ExpectedMessageType = NctsMessageTypeList.Codes.DESNOT },
				new { MessageStatus = "123", CustomsStatus = anyValue, Message = "BM_MessageStatus is not empty and not MAN", ExpectedMessageType = "ABC" },
				new { MessageStatus = LogicalStatusList.Codes.Error, CustomsStatus = string.Empty, Message = "BM_MessageStatus is ERR, BM_CustomsStatus is empty", ExpectedMessageType = NctsMessageTypeList.Codes.DESNOT },
				new { MessageStatus = LogicalStatusList.Codes.Failed, CustomsStatus = string.Empty, Message = "BM_MessageStatus is FAL, BM_CustomsStatus is empty", ExpectedMessageType = NctsMessageTypeList.Codes.DESNOT },
				new { MessageStatus = rejectedMessageStatus, CustomsStatus = string.Empty, Message = "BM_MessageStatus is REJ, BM_CustomsStatus is empty", ExpectedMessageType = NctsMessageTypeList.Codes.DESNOT },
				new { MessageStatus = rejectedMessageStatus, CustomsStatus = "123", Message = "BM_MessageStatus is ERR, BM_CustomsStatus is not empty", ExpectedMessageType = "ABC" },
				new { MessageStatus = LogicalStatusList.Codes.Accepted, CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, Message = "BM_CustomsStatus is UAP, BM_MessageStatus is ACC", ExpectedMessageType = NctsMessageTypeList.Codes.DESREM },
				new { MessageStatus = LogicalStatusList.Codes.Error, CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, Message = "BM_CustomsStatus is UAP, BM_MessageStatus is ERR", ExpectedMessageType = NctsMessageTypeList.Codes.DESREM },
				new { MessageStatus = LogicalStatusList.Codes.Failed , CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, Message = "BM_CustomsStatus is UAP, BM_MessageStatus is FAL", ExpectedMessageType = NctsMessageTypeList.Codes.DESREM },
				new { MessageStatus = rejectedMessageStatus, CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, Message = "BM_CustomsStatus is UAP, BM_MessageStatus is REJ", ExpectedMessageType = NctsMessageTypeList.Codes.DESREM },
				new { MessageStatus = "abc", CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, Message = "BM_CustomsStatus is UAP, BM_MessageStatus is not ACC, ERR, FAL or REJ", ExpectedMessageType = "ABC" },
				new { MessageStatus = LogicalStatusList.Codes.Error, CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, Message = "BM_CustomsStatus is ULR, BM_MessageStatus is ERR", ExpectedMessageType = NctsMessageTypeList.Codes.DESREM },
				new { MessageStatus = LogicalStatusList.Codes.Failed , CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, Message = "BM_CustomsStatus is ULR, BM_MessageStatus is FAL", ExpectedMessageType = NctsMessageTypeList.Codes.DESREM },
				new { MessageStatus = rejectedMessageStatus, CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, Message = "BM_CustomsStatus is ULR, BM_MessageStatus is REJ", ExpectedMessageType = NctsMessageTypeList.Codes.DESREM },
				new { MessageStatus = "abc", CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, Message = "BM_CustomsStatus is ULR, BM_MessageStatus is not ERR, FAL or REJ", ExpectedMessageType = "ABC" },
			};

			foreach (var testCase in testCases)
			{
				sendingObject.MessageType = "ABC";
				header.ArrivalMovementHeader.BM_MessageStatus = testCase.MessageStatus;
				if (testCase.CustomsStatus != anyValue)
				{
					header.ArrivalMovementHeader.BM_CustomsStatus = testCase.CustomsStatus;
				}
				configuration.SetDefaultMessageType(sendingObject);
				AssertEquals(testCase.Message, testCase.ExpectedMessageType, sendingObject.MessageType);
			}
		}

		public void TestShouldFillAdditionalWarningsOnSendScreen()
		{
			Assert(configuration.ShouldFillAdditionalWarningsOnSendScreen);
		}

		public void TestShouldHideSendWithAdditionalWarningCheckBox()
		{
			AssertEquals(expected: true, configuration.ShouldHideSendWithAdditionalWarningCheckBox);
		}

		public override void TestGetNewNctsMessageSendingObjectParent()
		{
			AssertType<NctsHeaderMessageSendingObjectParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(header));
		}

		public override void TestGetShouldSendDefault() => CombineAssertions(() =>
		{
			var sendingObject = new NctsHeaderMessageSendingObject(header);
			AssertEquals("Departure|!SNT", expected: true, configuration.GetShouldSendDefault(sendingObject));
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("Departure|SNT", expected: false, configuration.GetShouldSendDefault(sendingObject));
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("!Departure|SNT", expected: true, configuration.GetShouldSendDefault(sendingObject));
		});

		public void TestGetShouldSendDefault_DepDat() => CombineAssertions(() =>
		{
			var sendingObject = new NctsHeaderMessageSendingObject(header);
			sendingObject.MessageType = NctsMessageTypeList.Codes.DEPDAT;
			AssertEquals("Departure|Empty", expected: true, configuration.GetShouldSendDefault(sendingObject));

			header.EffectiveMessageStatus = NctsMessageStatusList.Codes.Rejected;
			AssertEquals("Departure|Rej", expected: true, configuration.GetShouldSendDefault(sendingObject));

			header.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
			AssertEquals("Departure|INV and MovementReferenceNumber|Empty", expected: true, configuration.GetShouldSendDefault(sendingObject));

			header.MovementReferenceEntryNumber.CE_EntryNum = "NUM";
			AssertEquals("Departure|INV and !MovementReferenceNumber|Empty", expected: false, configuration.GetShouldSendDefault(sendingObject));
		});

		public override void TestMessageTypeList()
		{
			AssertEquals("DEPDAT", configuration.MessageTypeList(header).CodesAsString);
		}

		public override void TestSetDefaultMessageType()
		{
			var sendingObject = new NctsHeaderMessageSendingObject(header);
			configuration.SetDefaultMessageType(sendingObject);
			AssertEquals("DEPDAT", sendingObject.MessageType);
		}

		public override void TestShowJustification()
		{
			AssertEquals(expected: false, configuration.ShowJustification(header));
		}

		protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
