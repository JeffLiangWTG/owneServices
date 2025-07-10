using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.MessageBuilders.Testing
{
	#region EDIFACTMessageStatusCalculatorTestCase

	[TestsSubclassesOf(typeof(EDIFACTMessageStatusCalculator))]
	public abstract class EDIFACTMessageStatusCalculatorTestCase : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public virtual void TestIsWithdrawn()
		{
			NUnit.Framework.Assert.That(calculator.IsWithdrawn(EntryStatusList.Codes.Cancelled), Is.True, "IsWithdrawn");
			NUnit.Framework.Assert.That(!calculator.IsWithdrawn(EntryStatusList.Codes.Clear), Is.True, "IsWithdrawn");
			NUnit.Framework.Assert.That(!calculator.IsWithdrawn(""), Is.True, "IsWithdrawn");
		}

		[ExpectNoExceptions]
		public virtual void TestIsClear()
		{
			NUnit.Framework.Assert.That(calculator.IsClear(EntryStatusList.Codes.Clear), Is.True, "IsClear");
			NUnit.Framework.Assert.That(!calculator.IsClear(EntryStatusList.Codes.Cancelled), Is.True, "IsClear");
			NUnit.Framework.Assert.That(!calculator.IsClear(""), Is.True, "IsClear");
		}

		[ExpectNoExceptions]
		public virtual void TestIsLodged()
		{
			NUnit.Framework.Assert.That(calculator.IsLodged(EntryStatusList.Codes.Clear), Is.True, "IsLodged");
			NUnit.Framework.Assert.That(calculator.IsLodged(EntryStatusList.Codes.Error), Is.True, "IsLodged");
			NUnit.Framework.Assert.That(!calculator.IsLodged(EntryStatusList.Codes.Cancelled), Is.True, "IsLodged");
			NUnit.Framework.Assert.That(!calculator.IsLodged(""), Is.True, "IsLodged");
		}

		public abstract void TestMessageTypeDescription();
		public abstract void TestCalculatedJobStatus();
		protected abstract EDIFACTMessageStatusCalculator GetCalculator();

		protected override void SetUp()
		{
			base.SetUp();
			calculator = GetCalculator();
			message = Factory.New<EDIMessage>();
		}
		protected EDIFACTMessageStatusCalculator calculator;
		protected EDIMessage message;
	}

	#endregion

	#region EDIFACTMessageStatusCalculatorTest

	[TestedType(typeof(MessageStatusCalculatorForTesting))]
	class EDIFACTMessageStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		[ExpectNoExceptions]
		public void TestIsAwaitingReply()
		{
			NUnit.Framework.Assert.That(calculator.IsAwaitingReply(MessageStatusList.Codes.AwaitingOriginal), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(calculator.IsAwaitingReply(MessageStatusList.Codes.AwaitingDelete), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(calculator.IsAwaitingReply(MessageStatusList.Codes.AwaitingChange), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(calculator.IsAwaitingReply(MessageStatusList.Codes.AwaitingReplace), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(!calculator.IsAwaitingReply(MessageStatusList.Codes.AcknowledgedOriginal), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(!calculator.IsAwaitingReply(MessageStatusList.Codes.AcknowledgedDelete), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(!calculator.IsAwaitingReply(MessageStatusList.Codes.AcknowledgedChange), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(!calculator.IsAwaitingReply(MessageStatusList.Codes.ErrorOriginal), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(!calculator.IsAwaitingReply(MessageStatusList.Codes.ClearChange), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(!calculator.IsAwaitingReply(MessageStatusList.Codes.NotSent), Is.True, "IsAwaitingReply");
			NUnit.Framework.Assert.That(!calculator.IsAwaitingReply(""), Is.True, "IsAwaitingReply");
		}

		[ExpectNoExceptions]
		public void TestGetMessageSubType()
		{
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.ClearOriginal), Is.EqualTo(MessageSubTypeCodes.Codes.Original).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingOriginal), Is.EqualTo(MessageSubTypeCodes.Codes.Original).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.AcknowledgedOriginal), Is.EqualTo(MessageSubTypeCodes.Codes.Original).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.ErrorOriginal), Is.EqualTo(MessageSubTypeCodes.Codes.Original).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingChange), Is.EqualTo(MessageSubTypeCodes.Codes.Change).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.AcknowledgedChange), Is.EqualTo(MessageSubTypeCodes.Codes.Change).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.ClearChange), Is.EqualTo(MessageSubTypeCodes.Codes.Change).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.ErrorChange), Is.EqualTo(MessageSubTypeCodes.Codes.Change).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingDelete), Is.EqualTo(MessageSubTypeCodes.Codes.Cancellation).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.AcknowledgedDelete), Is.EqualTo(MessageSubTypeCodes.Codes.Cancellation).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.ClearDelete), Is.EqualTo(MessageSubTypeCodes.Codes.Cancellation).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(MessageStatusList.Codes.ErrorDelete), Is.EqualTo(MessageSubTypeCodes.Codes.Cancellation).Using(CustomComparers.TypeComparison), "MessageSubType");
			NUnit.Framework.Assert.That(calculator.GetMessageSubType(""), Is.EqualTo(MessageSubTypeCodes.Codes.Undefined).Using(CustomComparers.TypeComparison), "MessageSubType");
		}

		#region TestGetMessageStatus

		[ExpectNoExceptions]
		public void TestGetMessageAcknowledgedStatus()
		{
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			NUnit.Framework.Assert.That(calculator.GetMessageAcknowledgedStatus(message), Is.EqualTo(MessageStatusList.Codes.AcknowledgedOriginal).Using(CustomComparers.TypeComparison), "GetAcknowledgedStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Change;
			NUnit.Framework.Assert.That(calculator.GetMessageAcknowledgedStatus(message), Is.EqualTo(MessageStatusList.Codes.AcknowledgedChange).Using(CustomComparers.TypeComparison), "GetAcknowledgedStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			NUnit.Framework.Assert.That(calculator.GetMessageAcknowledgedStatus(message), Is.EqualTo(MessageStatusList.Codes.AcknowledgedDelete).Using(CustomComparers.TypeComparison), "GetAcknowledgedStatus");
			message.EM_MessageSubType = ZString.Empty;
			NUnit.Framework.Assert.That(calculator.GetMessageAcknowledgedStatus(message), Is.EqualTo(ZString.Empty), "GetAcknowledgedStatus");
		}

		[ExpectNoExceptions]
		public void TestGetMessageAwaitingStatus()
		{
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			NUnit.Framework.Assert.That(calculator.GetMessageAwaitingStatus(message), Is.EqualTo(MessageStatusList.Codes.AwaitingOriginal).Using(CustomComparers.TypeComparison), "GetAwaitingStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Change;
			NUnit.Framework.Assert.That(calculator.GetMessageAwaitingStatus(message), Is.EqualTo(MessageStatusList.Codes.AwaitingChange).Using(CustomComparers.TypeComparison), "GetAwaitingStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			NUnit.Framework.Assert.That(calculator.GetMessageAwaitingStatus(message), Is.EqualTo(MessageStatusList.Codes.AwaitingDelete).Using(CustomComparers.TypeComparison), "GetAwaitingStatus");
			message.EM_MessageSubType = ZString.Empty;
			NUnit.Framework.Assert.That(calculator.GetMessageAwaitingStatus(message), Is.EqualTo(ZString.Empty), "GetAwaitingStatus");
		}

		[ExpectNoExceptions]
		public void TestGetMessageClearedStatus()
		{
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			NUnit.Framework.Assert.That(calculator.GetMessageClearedStatus(message), Is.EqualTo(MessageStatusList.Codes.ClearOriginal).Using(CustomComparers.TypeComparison), "GetClearedStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Change;
			NUnit.Framework.Assert.That(calculator.GetMessageClearedStatus(message), Is.EqualTo(MessageStatusList.Codes.ClearChange).Using(CustomComparers.TypeComparison), "GetClearedStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			NUnit.Framework.Assert.That(calculator.GetMessageClearedStatus(message), Is.EqualTo(MessageStatusList.Codes.ClearDelete).Using(CustomComparers.TypeComparison), "GetClearedStatus");
			message.EM_MessageSubType = ZString.Empty;
			NUnit.Framework.Assert.That(calculator.GetMessageClearedStatus(message), Is.EqualTo(ZString.Empty), "GetClearedStatus");
		}

		[ExpectNoExceptions]
		public void TestGetMessageRejectedStatus()
		{
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			NUnit.Framework.Assert.That(calculator.GetMessageRejectedStatus(message), Is.EqualTo(MessageStatusList.Codes.ErrorOriginal).Using(CustomComparers.TypeComparison), "GetRejectedStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Change;
			NUnit.Framework.Assert.That(calculator.GetMessageRejectedStatus(message), Is.EqualTo(MessageStatusList.Codes.ErrorChange).Using(CustomComparers.TypeComparison), "GetRejectedStatus");
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			NUnit.Framework.Assert.That(calculator.GetMessageRejectedStatus(message), Is.EqualTo(MessageStatusList.Codes.ErrorDelete).Using(CustomComparers.TypeComparison), "GetRejectedStatus");
			message.EM_MessageSubType = ZString.Empty;
			NUnit.Framework.Assert.That(calculator.GetMessageRejectedStatus(message), Is.EqualTo(ZString.Empty), "GetRejectedStatus");
		}

		#endregion

		[ExpectNoExceptions]
		public override void TestMessageTypeDescription()
		{
			NUnit.Framework.Assert.That(calculator.MessageTypeDescription, Is.EqualTo("Test Calculator").Using(CustomComparers.TypeComparison), "MessageTypeDescription");
		}

		[ExpectNoExceptions]
		public override void TestCalculatedJobStatus()
		{
			NUnit.Framework.Assert.That(calculator.CalculatedJobStatus(null), Is.EqualTo(ZString.Empty), "CalculatedJobStatus");
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new MessageStatusCalculatorForTesting();
		}
	}

	#endregion

	#region MessageStatusCalculatorForTesting

	class MessageStatusCalculatorForTesting : EDIFACTMessageStatusCalculator
	{
		#region Overrides of EDIFACTMessageStatusCalculator

		public override ZString MessageTypeDescription
		{
			get { return "Test Calculator"; }
		}

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			return ZString.Empty;
		}

		#endregion
	}

	#endregion
}
