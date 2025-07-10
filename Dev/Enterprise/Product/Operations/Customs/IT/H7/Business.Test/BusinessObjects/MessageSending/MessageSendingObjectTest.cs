using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(MessageSendingObject))]
sealed class MessageSendingObjectTest : EU.H7.Business.Testing.MessageSendingObjectTest
{
	public void TestActionDefaultValue()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);

		AssertEquals("Action's default value is H7D", ITH7MessageTypes.Codes.H7D, messageSendingObject.Action);
	}

	public void TestAmendmentReasonCode()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);
		messageSendingObject.Action = string.Empty;

		var allMessageTypes = new[] { ITH7MessageTypes.Codes.H7D, ITH7MessageTypes.Codes.H7Q, ITH7MessageTypes.Codes.H7C, ITH7MessageTypes.Codes.H7M };

		var readonlyMessageTypes = new[] { ITH7MessageTypes.Codes.H7D, ITH7MessageTypes.Codes.H7Q };

		CombineAssertions(() =>
		{
			foreach (var messageType in allMessageTypes)
			{
				messageSendingObject.AmendmentReasonCode = "R1";
				messageSendingObject.Action = messageType;

				if (readonlyMessageTypes.Contains(messageType))
				{
					Assert($"AmendmentReason should be read only for {messageType}", messageSendingObject.AmendmentReasonCodeInfo.ReadOnly);
				}
				else
				{
					Assert($"AmendmentReason should not be read only for {messageType}", !messageSendingObject.AmendmentReasonCodeInfo.ReadOnly);
				}

				AssertEquals($"AmendmentReason should be empty when Action is changed", string.Empty, messageSendingObject.AmendmentReasonCode);
			}
		});
	}

	public void TestLegislativeReference()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);

		var allMessageTypes = new[] { ITH7MessageTypes.Codes.H7D, ITH7MessageTypes.Codes.H7Q, ITH7MessageTypes.Codes.H7C, ITH7MessageTypes.Codes.H7M };

		var readonlyMessageTypes = new[] { ITH7MessageTypes.Codes.H7D, ITH7MessageTypes.Codes.H7Q };

		CombineAssertions(() =>
		{
			foreach (var messageType in allMessageTypes)
			{
				messageSendingObject.Action = messageType;

				if (readonlyMessageTypes.Contains(messageType))
				{
					Assert($"LegislativeReference should be read only for {messageType}", messageSendingObject.LegislativeReferenceInfo.ReadOnly);
				}
				else
				{
					Assert($"LegislativeReference should not be read only for {messageType}", !messageSendingObject.LegislativeReferenceInfo.ReadOnly);
				}
			}
		});
	}

	public void TestDutyAmount()
	{
		CombineAssertions(() =>
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(MessageSendingObject), nameof(MessageSendingObject.DutyAmount), false, x => x.MaxLength == 17);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(MessageSendingObject), nameof(MessageSendingObject.DutyAmount), false, x => x.DecimalPlaces == 2);
			AssertHasCustomAttribute<DecimalPrecisionAttribute>(typeof(MessageSendingObject), nameof(MessageSendingObject.DutyAmount), false, x => x.DecimalPrecision == 16);
		});
	}

	public void TestCurrency()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);

		AssertEquals("Currency is set to EUR", "EUR", messageSendingObject.Currency);
	}

	public void TestAmendmentCancellationReasonCodeList()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);

		CombineAssertions(() =>
		{
			messageSendingObject.Action = ZString.Empty;
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(), messageSendingObject.AmendmentReasonCodeList);

			messageSendingObject.Action = ITH7MessageTypes.Codes.H7M;
			AssertContainsExactElementsInAnyOrder(new AmendmentReasonList(), messageSendingObject.AmendmentReasonCodeList);

			messageSendingObject.Action = ITH7MessageTypes.Codes.H7C;
			AssertContainsExactElementsInAnyOrder(new Ucc6ImportCancellationReasonList(), messageSendingObject.AmendmentReasonCodeList);
		});
	}

	public void TestLegislativeReferenceCodeList()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);

		CombineAssertions(() =>
		{
			messageSendingObject.Action = ITH7MessageTypes.Codes.H7M;
			AssertContainsExactElementsInAnyOrder(ExpectedLegislativeReferenceCodeList, messageSendingObject.LegislativeReferenceCodeList);

			messageSendingObject.Action = ITH7MessageTypes.Codes.H7C;
			AssertContainsExactElementsInAnyOrder(ExpectedLegislativeReferenceCodeList, messageSendingObject.LegislativeReferenceCodeList);
		});
	}

	public override void TestIsAmendmentAction()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);

		CombineAssertions(() =>
		{
			Assert("Should be false when Action is empty", !messageSendingObject.IsAmendmentAction);

			messageSendingObject.Action = ITH7MessageTypes.Codes.H7M;
			Assert("Should be true when Action is H7M", messageSendingObject.IsAmendmentAction);

			messageSendingObject.Action = ITH7MessageTypes.Codes.H7C;
			Assert("Should be false when Action is not H7M", !messageSendingObject.IsAmendmentAction);
		});
	}

	public override void TestIsCancellationAction()
	{
		var bill = Factory.New<AsycudaBill>();
		var messageSendingObject = new MessageSendingObject(bill);

		CombineAssertions(() =>
		{
			Assert("Should be false when Action is empty", !messageSendingObject.IsCancellationAction);

			messageSendingObject.Action = ITH7MessageTypes.Codes.H7C;
			Assert("Should be true when Action is H7C", messageSendingObject.IsCancellationAction);

			messageSendingObject.Action = ITH7MessageTypes.Codes.H7M;
			Assert("Should be false when Action is not H7C", !messageSendingObject.IsCancellationAction);
		});
	}

	protected override CodeDescriptionPairList ExpectedActionList => new ITH7MessageTypes();

	protected override CodeDescriptionPairList ExpectedSubStyleList => new CodeDescriptionPairList();

	CodeDescriptionPairList ExpectedLegislativeReferenceCodeList => new Ucc6ImportCancellationAndAmendmentLegislativeReferenceList();

	protected override string AmendmentReasonCodeCaption => "Amendment/Cancellation Reason";

	protected override EU.H7.Business.AsycudaBill GetMessageSendingObjectParentBill()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();

		return bill;
	}

	protected override EU.H7.Business.MessageSendingObject GetMessageSendingObject(EU.H7.Business.AsycudaBill bill) => new MessageSendingObject(bill);

	protected override Type ExpectedMessageSenderType => typeof(MessageSender);
}
