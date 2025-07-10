using System;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	sealed class MessageTimeHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestZeroFromSecond()
		{
			NUnit.Framework.Assert.That(new DateTime(2020, 5, 18, 9, 2, 3, 111).ZeroFromSecond(), Is.EqualTo(new DateTime(2020, 5, 18, 9, 2, 00)));
		}

		[ExpectNoExceptions]
		public void TestZeroFromSecond_ZDateTime()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(new ZDateTime(2020, 5, 18, 9, 2, 3, 111).ZeroFromSecond(), Is.EqualTo(new ZDateTime(2020, 5, 18, 9, 2, 00)), "Valid input");
				NUnit.Framework.Assert.That(ZDateTime.Invalid.ZeroFromSecond(), Is.EqualTo(ZDateTime.Invalid), "Invalid input");
			});
		}

		[TestDate(2019, 12, 22, 16, 43, 51)]
		[ExpectNoExceptions]
		public void TestSafeDateTime()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(ZDateTime.Now.SafeDateTime(), Is.EqualTo(new DateTime(2019, 12, 22, 16, 43, 51)), "Valid input");
				NUnit.Framework.Assert.That(ZDateTime.Empty.SafeDateTime(), Is.EqualTo(default(DateTime)), "Invalid input");
			});
		}

		[TestDate(2019, 12, 22, 16, 43, 51)]
		[ExpectNoExceptions]
		public void TestSafeDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(ZDate.Today.SafeDate(), Is.EqualTo(new DateTime(2019, 12, 22)), "Valid input");
				NUnit.Framework.Assert.That(ZDate.Empty.SafeDate(), Is.EqualTo(default(DateTime)), "Invalid input");
			});
		}

		[TestDate(2019, 12, 22, 16, 43, 51)]
		[ExpectNoExceptions]
		public void TestToNullableDateTime_ZDateTime()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(ZDateTime.Now.ToNullableDateTime(), Is.EqualTo(new DateTime(2019, 12, 22, 16, 43, 51)), "Valid input");
				NUnit.Framework.Assert.That(ZDateTime.Empty.ToNullableDateTime(), Is.Null, "Invalid input");
			});
		}

		[ExpectNoExceptions]
		public void TestToNullableDateTime_ZDate()
		{
			CombineAssertions(() =>
			{
				var date = new ZDate(2022, 11, 28);
				NUnit.Framework.Assert.That(date.ToNullableDateTime(), Is.EqualTo(date).Using(CustomComparers.TypeComparison), "Valid input");
				NUnit.Framework.Assert.That(ZDate.Empty.ToNullableDateTime(), Is.Null, "Invalid input");
			});
		}
	}
}
