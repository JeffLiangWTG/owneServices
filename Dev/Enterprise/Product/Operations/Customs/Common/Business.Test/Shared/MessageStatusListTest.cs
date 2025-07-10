using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Shared.Testing
{
	class MessageStatusListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetCategoryForStatus()
		{
			var messageStatusCategoryList = new MessageStatusCategoryList();

			NUnit.Framework.Assert.That(MessageStatusList.GetCategoryForStatus(MessageStatusList.Codes.AcknowledgedChange, messageStatusCategoryList), Is.EqualTo("AC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(MessageStatusList.GetCategoryForStatus(MessageStatusList.Codes.AwaitingOriginal, messageStatusCategoryList), Is.EqualTo("AW").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(MessageStatusList.GetCategoryForStatus(MessageStatusList.Codes.NotSent, messageStatusCategoryList), Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(MessageStatusList.GetCategoryForStatus(MessageStatusList.Codes.Sent, messageStatusCategoryList), Is.EqualTo("UN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(MessageStatusList.GetCategoryForStatus(MessageStatusList.Codes.Unknown, messageStatusCategoryList), Is.EqualTo("UN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsError()
		{
			var list = new MessageStatusList();
			list.RemoveCode(MessageStatusList.Codes.ErrorChange);
			list.RemoveCode(MessageStatusList.Codes.ErrorDelete);
			list.RemoveCode(MessageStatusList.Codes.ErrorOriginal);
			list.RemoveCode(MessageStatusList.Codes.ErrorReplace);
			foreach (ICodeDescription pair in list)
			{
				NUnit.Framework.Assert.That(MessageStatusList.IsError(pair.Code), Is.EqualTo(false), pair.Code);
			}
			NUnit.Framework.Assert.That(MessageStatusList.IsError(MessageStatusList.Codes.ErrorChange), Is.EqualTo(true), MessageStatusList.Codes.ErrorChange);
			NUnit.Framework.Assert.That(MessageStatusList.IsError(MessageStatusList.Codes.ErrorDelete), Is.EqualTo(true), MessageStatusList.Codes.ErrorDelete);
			NUnit.Framework.Assert.That(MessageStatusList.IsError(MessageStatusList.Codes.ErrorOriginal), Is.EqualTo(true), MessageStatusList.Codes.ErrorOriginal);
			NUnit.Framework.Assert.That(MessageStatusList.IsError(MessageStatusList.Codes.ErrorReplace), Is.EqualTo(true), MessageStatusList.Codes.ErrorReplace);
		}

		[ExpectNoExceptions]
		public void TestIsAwaiting()
		{
			var list = new MessageStatusList();
			list.RemoveCode(MessageStatusList.Codes.AwaitingChange);
			list.RemoveCode(MessageStatusList.Codes.AwaitingDelete);
			list.RemoveCode(MessageStatusList.Codes.AwaitingOriginal);
			list.RemoveCode(MessageStatusList.Codes.AwaitingReplace);
			foreach (ICodeDescription pair in list)
			{
				NUnit.Framework.Assert.That(MessageStatusList.IsAwaiting(pair.Code), Is.EqualTo(false), pair.Code);
			}
			NUnit.Framework.Assert.That(MessageStatusList.IsAwaiting(MessageStatusList.Codes.AwaitingChange), Is.EqualTo(true), MessageStatusList.Codes.AwaitingChange);
			NUnit.Framework.Assert.That(MessageStatusList.IsAwaiting(MessageStatusList.Codes.AwaitingDelete), Is.EqualTo(true), MessageStatusList.Codes.AwaitingDelete);
			NUnit.Framework.Assert.That(MessageStatusList.IsAwaiting(MessageStatusList.Codes.AwaitingOriginal), Is.EqualTo(true), MessageStatusList.Codes.AwaitingOriginal);
			NUnit.Framework.Assert.That(MessageStatusList.IsAwaiting(MessageStatusList.Codes.AwaitingReplace), Is.EqualTo(true), MessageStatusList.Codes.AwaitingReplace);
		}

		[ExpectNoExceptions]
		public void TestIsAcknowledged()
		{
			var list = new MessageStatusList();
			list.RemoveCode(MessageStatusList.Codes.AcknowledgedChange);
			list.RemoveCode(MessageStatusList.Codes.AcknowledgedDelete);
			list.RemoveCode(MessageStatusList.Codes.AcknowledgedOriginal);
			list.RemoveCode(MessageStatusList.Codes.AcknowledgedReplace);
			foreach (ICodeDescription pair in list)
			{
				NUnit.Framework.Assert.That(MessageStatusList.IsAcknowledged(pair.Code), Is.EqualTo(false), pair.Code);
			}
			NUnit.Framework.Assert.That(MessageStatusList.IsAcknowledged(MessageStatusList.Codes.AcknowledgedChange), Is.EqualTo(true), MessageStatusList.Codes.AcknowledgedChange);
			NUnit.Framework.Assert.That(MessageStatusList.IsAcknowledged(MessageStatusList.Codes.AcknowledgedDelete), Is.EqualTo(true), MessageStatusList.Codes.AcknowledgedDelete);
			NUnit.Framework.Assert.That(MessageStatusList.IsAcknowledged(MessageStatusList.Codes.AcknowledgedOriginal), Is.EqualTo(true), MessageStatusList.Codes.AcknowledgedOriginal);
			NUnit.Framework.Assert.That(MessageStatusList.IsAcknowledged(MessageStatusList.Codes.AcknowledgedReplace), Is.EqualTo(true), MessageStatusList.Codes.AcknowledgedReplace);
		}
	}
}
