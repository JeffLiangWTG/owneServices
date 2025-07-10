using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class ZDateTimeExtensionsTest : TestCase
	{
		public void TestToNACCSDate()
		{
			var date = new DateTime(2025, 5, 7, 13, 1, 45);
			AssertEquals("2025/05/07", date.ToNACCSDate());

			date = default;
			AssertEquals(ZString.Empty, date.ToNACCSDate());
		}

		public void TestToNACCSDateTime()
		{
			var date = new DateTime(2025, 5, 7, 13, 1, 45);
			AssertEquals("2025/05/07 13:01", date.ToNACCSDateTime());

			date = default;
			AssertEquals(ZString.Empty, date.ToNACCSDateTime());
		}

		public void TestToNACCSTime()
		{
			var date = new DateTime(2025, 5, 7, 13, 1, 45);
			AssertEquals("13:01:45", date.ToNACCSTime());

			date = default;
			AssertEquals(ZString.Empty, date.ToNACCSTime());
		}
	}
}
