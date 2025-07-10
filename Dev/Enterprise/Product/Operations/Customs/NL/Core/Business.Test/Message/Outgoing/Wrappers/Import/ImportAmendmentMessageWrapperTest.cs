using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ImportAmendmentMessageWrapperTest : DataProviderTestCase<ImportAmendmentMessageWrapper>
{
	public void TestComparedMessageText_CC415A()
	{
		AssertComparedMessage_CC415A("<TypeCode>EXA</TypeCode>");
	}

	public void TestComparedMessageXSDType_CC415A()
	{
		AssertComparedMessage_CC415A(expectedComparedMessageXSDType: ComparedMessageXSDType.Declaration);
	}

	void AssertComparedMessage_CC415A(string expectedComparedMessageText = null, ComparedMessageXSDType? expectedComparedMessageXSDType = null)
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.CRI, "EXB");

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
			EntryStatus.OtherControl,
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

	public void TestComparedMessage_CC429A()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.CRI, "EXB");

		var customsStatusCompareWithCC515C = new[] {
			EntryStatus.ProvisionalRelease,
			EntryStatus.SupplementReminder,
			EntryStatus.SupplementSent,
			EntryStatus.SupplementReceivedByCustoms,
			EntryStatus.Rejection_415_for_SubStyle_XY,
			EntryStatus.ReleasedAndTaxed,
		};

		CombineAssertions(() =>
		{
			foreach (var entryStatus in customsStatusCompareWithCC515C)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var wrapper = GetProvider();
				AssertContains($"CH_EntryStatus:{entryStatus}-ComparedMessageText", "<TypeCode>EXB</TypeCode>", wrapper.ComparedMessageText);
				AssertEquals($"CH_EntryStatus:{entryStatus}-ComparedMessageXSDType", ComparedMessageXSDType.AdditionalMessage, wrapper.ComparedMessageXSDType);
			}
		});
	}

	public void TestComparedMessage_CC429AElseCC415A_WhenNo500ReleasedMessage()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.CRI, "EXB");
		WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC429A, StatusNameCodes.Released);

		var customsStatusCompareWithCC515C = new[] {
			EntryStatus._400,
			EntryStatus._410,
			EntryStatus._420
		};

		CombineAssertions(() =>
		{
			foreach (var entryStatus in customsStatusCompareWithCC515C)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var wrapper = GetProvider();
				AssertContains($"CH_EntryStatus:{entryStatus}-ComparedMessageText", "<TypeCode>EXA</TypeCode>", wrapper.ComparedMessageText);
				AssertEquals($"CH_EntryStatus:{entryStatus}-ComparedMessageXSDType", ComparedMessageXSDType.Declaration, wrapper.ComparedMessageXSDType);
			}
		});
	}

	public void TestComparedMessage_CC429AElseCC415A_When500ReleasedMessageExists()
	{
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.DEC, "EXA");
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.CRI, "EXB");
		WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC429A, StatusNameCodes.ProvisionalRelease);

		var customsStatusCompareWithCC515C = new[] {
			EntryStatus._400,
			EntryStatus._410,
			EntryStatus._420
		};

		CombineAssertions(() =>
		{
			foreach (var entryStatus in customsStatusCompareWithCC515C)
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
		WrapperTestHelper.CreateSentMessage(entryHeader, ImportSendMessageTypes.Codes.DEC, "EXA");

		entryHeader.CH_EntryStatus = EntryStatus.AdvanceDeclarationReceived;
		_ = GetProvider().ComparedMessageText;

		Factory.TryGetValueFromCacheOnly<ValueTuple<ZString, ComparedMessageXSDType>>("GetComparedMessageInfo_False_True_False", out var result);
		AssertEquals(ComparedMessageXSDType.Declaration, result.Item2);
	}

	protected override ImportAmendmentMessageWrapper GetProvider() => new ImportAmendmentMessageWrapper(sendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
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
