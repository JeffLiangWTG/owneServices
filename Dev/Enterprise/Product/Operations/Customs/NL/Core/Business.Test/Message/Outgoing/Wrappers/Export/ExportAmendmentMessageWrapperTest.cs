using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Customs.NL.Business.Common.NLConstants;
using EntryStatus = Enterprise.Customs.NL.Business.Common.NLConstants.EntryStatus;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ExportAmendmentMessageWrapperTest : DataProviderTestCase<ExportAmendmentMessageWrapper>
{
	public void TestComparedMessage_NoOldMessage()
	{
		AssertExceptionThrown<ApplicationException>("No ComparedMetaData", () => _ = GetProvider().ComparedMessageXSDType);
	}

	public void TestComparedMessageText_CC515C()
	{
		AssertComparedMessage_CC515C("<TypeCode>EXA</TypeCode>");
	}

	public void TestComparedMessageXSDType_CC515C()
	{
		AssertComparedMessage_CC515C(expectedComparedMessageXSDType: ComparedMessageXSDType.Declaration);
	}

	void AssertComparedMessage_CC515C(string expectedComparedMessageText = null, ComparedMessageXSDType? expectedComparedMessageXSDType = null)
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.CRE, "EXB");

		var customsStatusCompareWithCC515C = new[] {
			EntryStatus.AdvanceDeclarationReceived,
			EntryStatus._220,
			EntryStatus.Accepted,
			EntryStatus.DocumentsControl,
			EntryStatus.NuclearMaterials,
			EntryStatus.NonIntrusiveInspection,
			EntryStatus.PhysicalInspection,
			EntryStatus.IdentificationOfShipment,
			EntryStatus.IntrusiveInspection,
			EntryStatus.QualityControl,
			EntryStatus.CharacteristicsOfGoods,
			EntryStatus.Sampling,
			EntryStatus.RequestForInformation,
			EntryStatus.ExportReminder_NoExitInformationReceived,
		};

		CombineAssertions(() =>
		{
			foreach (var entryStatus in customsStatusCompareWithCC515C)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var wrapper = GetProvider();
				if (!string.IsNullOrEmpty(expectedComparedMessageText))
				{
					AssertContains($"CH_EntryStatus:{entryStatus}-ComparedMessageText", expectedComparedMessageText, wrapper.ComparedMessageText);
				}
				if (expectedComparedMessageXSDType != null)
				{
					AssertEquals($"CH_EntryStatus:{entryStatus}-ComparedMessageXSDType", expectedComparedMessageXSDType, wrapper.ComparedMessageXSDType);
				}
			}

			entryHeader.CH_EntryStatus = "XXX";
			AssertExceptionThrown<ApplicationException>("CH_EntryStatus is invalid", "The old message was not captured, so we cannot send an amendment message.", () => _ = GetProvider().ComparedMessageText);
		});
	}

	public void TestComparedMessage_CC529C_CheckEntryStatus500()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.CRE, "EXB");
		WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC529C, StatusNameCodes.ProvisionalRelease);

		var customsStatusCompareWithCC529C_CheckEntryStatus500 = new[] {
			EntryStatus.ProvisionalRelease,
			EntryStatus.SupplementReminder,
			EntryStatus.SupplementSent,
			EntryStatus.SupplementReceivedByCustoms,
			EntryStatus.ExportRejection_515_for_SubStyle_XY,
		};

		CombineAssertions(() =>
		{
			foreach (var entryStatus in customsStatusCompareWithCC529C_CheckEntryStatus500)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var wrapper = GetProvider();
				AssertContains($"CH_EntryStatus:{entryStatus}-ComparedMessageText", "<TypeCode>EXB</TypeCode>", wrapper.ComparedMessageText);
				AssertEquals($"CH_EntryStatus:{entryStatus}-ComparedMessageXSDType", ComparedMessageXSDType.AdditionalMessage, wrapper.ComparedMessageXSDType);
			}

			entryHeader.CH_EntryStatus = EntryStatus.ReleasedAndTaxed;
			AssertExceptionThrown<ApplicationException>("CH_EntryStatus:550", "The old message was not captured, so we cannot send an amendment message.", () => _ = GetProvider().ComparedMessageText);
		});
	}

	public void TestComparedMessage_CC529C_CheckEntryStatus550()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.CRE, "EXB");
		WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC529C, StatusNameCodes.Released);

		CombineAssertions(() =>
		{
			entryHeader.CH_EntryStatus = EntryStatus.ReleasedAndTaxed;
			var wrapper = GetProvider();
			AssertContains("CH_EntryStatus:550-ComparedMessageText", "<TypeCode>EXB</TypeCode>", wrapper.ComparedMessageText);
			AssertEquals("CH_EntryStatus:550-ComparedMessageXSDType", ComparedMessageXSDType.AdditionalMessage, wrapper.ComparedMessageXSDType);

			entryHeader.CH_EntryStatus = EntryStatus.ProvisionalRelease;
			AssertExceptionThrown<ApplicationException>("CH_EntryStatus:500", "The old message was not captured, so we cannot send an amendment message.", () => _ = GetProvider().ComparedMessageText);
		});
	}

	public void TestComparedMessage_CC529C_CheckEntryStatus500Or550_When500ReleasedMessageExists()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.CRE, "EXB");
		WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC529C, StatusNameCodes.ProvisionalRelease);

		var customsStatusCompareWithCC529C_CheckEntryStatus500Or550 = new[] {
			EntryStatus._800,
			EntryStatus.NoExitInformationRecievedYet,
			EntryStatus.ExitInformationDetailsSent,
			EntryStatus.ExitInformationDetailsReceived,
			EntryStatus.ExportRejection_583,
			EntryStatus.ExportCancellation,
		};

		CombineAssertions(() =>
		{
			foreach (var entryStatus in customsStatusCompareWithCC529C_CheckEntryStatus500Or550)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var wrapper = GetProvider();
				AssertContains($"CH_EntryStatus:{entryStatus}-ComparedMessageText", "<TypeCode>EXB</TypeCode>", wrapper.ComparedMessageText);
				AssertEquals($"CH_EntryStatus:{entryStatus}-ComparedMessageXSDType", ComparedMessageXSDType.AdditionalMessage, wrapper.ComparedMessageXSDType);
			}
		});
	}

	public void TestComparedMessage_CC529C_CheckEntryStatus500Or550_When550ReleasedMessageExists()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.CRE, "EXB");
		WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC529C, StatusNameCodes.Released);

		var customsStatusCompareWithCC529C_CheckEntryStatus500Or550 = new[] {
			EntryStatus._800,
			EntryStatus.NoExitInformationRecievedYet,
			EntryStatus.ExitInformationDetailsSent,
			EntryStatus.ExitInformationDetailsReceived,
			EntryStatus.ExportRejection_583,
			EntryStatus.ExportCancellation,
		};

		CombineAssertions(() =>
		{
			foreach (var entryStatus in customsStatusCompareWithCC529C_CheckEntryStatus500Or550)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var wrapper = GetProvider();
				AssertContains($"CH_EntryStatus:{entryStatus}-ComparedMessageText", "<TypeCode>EXB</TypeCode>", wrapper.ComparedMessageText);
				AssertEquals($"CH_EntryStatus:{entryStatus}-ComparedMessageXSDType", ComparedMessageXSDType.AdditionalMessage, wrapper.ComparedMessageXSDType);
			}
		});
	}

	public void TestComparedMessage_Cached()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");

		entryHeader.CH_EntryStatus = EntryStatus.AdvanceDeclarationReceived;
		_ = GetProvider().ComparedMessageText;

		Factory.TryGetValueFromCacheOnly<ValueTuple<ZString, ComparedMessageXSDType>>("GetComparedMessageInfo_True_False_False_False", out var result);
		AssertEquals(ComparedMessageXSDType.Declaration, result.Item2);
	}

	protected override ExportAmendmentMessageWrapper GetProvider() => new ExportAmendmentMessageWrapper(sendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
	}
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObject sendingObject;
}
