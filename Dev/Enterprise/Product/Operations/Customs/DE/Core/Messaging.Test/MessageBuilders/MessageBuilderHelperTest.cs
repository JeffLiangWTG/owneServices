using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	sealed class MessageBuilderHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLeftOrNull()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(ZString.Empty.LeftOrNull(2), Is.EqualTo(default(string)), "Empty - should be [null]");
				NUnit.Framework.Assert.That(new ZString("DE DE").LeftOrNull(2), Is.EqualTo("DE"), "Normal");
			});
		}

		[ExpectNoExceptions]
		public void TestFormatDecimal()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MessageBuilderHelper.FormatDecimal(12.52661m, 3).ToString(), Is.EqualTo("12.527"), "Round up");
				NUnit.Framework.Assert.That(MessageBuilderHelper.FormatDecimal(4.72388m, 2).ToString(), Is.EqualTo("4.72"), "Round down");
				NUnit.Framework.Assert.That(MessageBuilderHelper.FormatDecimal(4.70000m, 2).ToString(), Is.EqualTo("4.7"), "No trailing zeros");
			});
		}

		[ExpectNoExceptions]
		public void TestMapZBoolTo10()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolTo10(ZBool.True), Is.EqualTo("1"), "ZBool.True");
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolTo10(ZBool.False), Is.EqualTo("0"), "ZBool.False");
			});
		}

		[ExpectNoExceptions]
		public void TestMapBoolTo10()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolTo10(true), Is.EqualTo("1"), "True");
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolTo10(false), Is.EqualTo("0"), "False");
			});
		}

		[ExpectNoExceptions]
		public void TestMapZBoolToJN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolToJN(ZBool.True), Is.EqualTo("J"), "ZBool.True");
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolToJN(ZBool.False), Is.EqualTo("N"), "ZBool.False");
			});
		}

		[ExpectNoExceptions]
		public void TestMapBoolToJN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolToJN(true), Is.EqualTo("J"), "True");
				NUnit.Framework.Assert.That(MessageBuilderHelper.MapBoolToJN(false), Is.EqualTo("N"), "False");
			});
		}
	}
}
