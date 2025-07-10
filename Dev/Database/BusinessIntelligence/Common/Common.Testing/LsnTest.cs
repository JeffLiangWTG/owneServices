using System;
using NUnit.Framework;

namespace CargoWise.Bi.Common.Testing
{
	public class LsnTest : TestCase
	{
		public void TestLsn()
		{
			var expectedValue = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var lsn = new Lsn(expectedValue);

			AssertEquals("lsn's length should be 10", 10, lsn.Value.Length);

			for (int i = 0; i < 10; i++)
			{
				AssertEquals("lsn should be equal with expectedValue", expectedValue[i], lsn.Value[i]);
			}
		}

		public void TestLsn_NullLsn()
		{
			var nullLsn = new Lsn((byte[])null);
			AssertNull("nullLsn's value should be null", nullLsn.Value);

			nullLsn = new Lsn((string)null);
			AssertNull("nullLsn's value should be null", nullLsn.Value);

			nullLsn = new Lsn(DBNull.Value.ToString());
			AssertNull("nullLsn's value should be null", nullLsn.Value);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestLsn_WrongValue()
		{
			var lsn = new Lsn(new byte[] { 1 });
		}

		public void TestCompareTo()
		{
			var lsn = new Lsn(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			var smallerLsn = new Lsn(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 8 });
			var sameLsn = new Lsn(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			var biggerLsn = new Lsn(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10 });

			AssertEquals("Should return 1 when compared obj is smaller than this", 1, lsn.CompareTo(smallerLsn));
			AssertEquals("Should return 0 when compared obj is equal to this", 0, lsn.CompareTo(sameLsn));
			AssertEquals("Should return -1 when compared obj is bigger than this", -1, lsn.CompareTo(biggerLsn));

			var nullLsn1 = new Lsn((byte[])null);
			var nullLsn2 = new Lsn((byte[])null);

			AssertEquals("Should return 1 when compared obj's value is null and this one's value is not null", 1, lsn.CompareTo(nullLsn1));
			AssertEquals("Should return 0 when compared obj's value is null and this one's value is null", 0, nullLsn1.CompareTo(nullLsn2));
			AssertEquals("Should return -1 when compared obj's value is not null and this one's value is null", -1, nullLsn1.CompareTo(lsn));
		}

		public void TestLsnFormat()
		{
			CombineAssertions(() =>
			{
				AssertEquals("0xABCD000000000000002C", new Lsn("0xABCD000000000000002c").ToString());
				AssertEquals("0x00300000000000000000", new Lsn("0x003").ToString());
				AssertEquals("0x0000000000000000000A", new Lsn(10).ToString());
				AssertEquals("0x0000CCDDCCDDAABBAABB", new Lsn(0xCCDDCCDDAABBAABB).ToString());
				AssertEquals("null", new Lsn("").ToString());
				AssertEquals("null", new Lsn((byte[])null).ToString());
			});
		}
	}
}
