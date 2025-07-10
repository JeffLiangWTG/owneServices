using System;

namespace CargoWise.Types.Tests
{
	public class ZByteTest : IZTypeTest
	{
		public void TestNullConstructor()
		{
			AssertEquals(ZByte.Zero, new ZByte(null));
		}

		public void TestImplicitZByteZInt()
		{
			ZByte z = 4;
			ZInt asInt = z;
			AssertEquals(4, asInt);
		}

		public void TestToZInt()
		{
			ZByte z = 4;
			AssertEquals(new ZInt(4), z.ToZInt());
		}

		public void TestEqualOperator()
		{
			ZByte lhs = 2;
			ZByte rhs = 2;

			AssertEquals(true, lhs == rhs);

			lhs = 1;

			AssertEquals(false, lhs == rhs);
		}

		public void TestNotEqualOperator()
		{
			ZByte lhs = 2;
			ZByte rhs = 2;

			AssertEquals(false, lhs != rhs);

			lhs = 1;

			AssertEquals(true, lhs != rhs);
		}

		public void TestPlusPlusOperator()
		{
			byte b = 0;
			b++;
			ZByte z = 0;
			z++;

			AssertEquals("ZByte Z = 0;   Z++;   -   Z should be 1.", (byte)1, z);
			Assert("ZByte++ does not match byte++ operator.", z == b);
		}

		public void TestMinusMinusOperator()
		{
			byte b = 2;
			b--;
			ZByte z = 2;
			z--;

			AssertEquals("ZByte Z = 2;   Z--;   -   Z should be 1.", (byte)1, z);
			Assert("ZByte++ does not match byte++ operator.", z == b);
		}

		public void TestZero()
		{
			ZByte z = 0;
			AssertEquals(ZByte.Zero, z);
		}

		public void TestTryParse()
		{
			AssertTryParse(byte.MinValue.ToString(), true, byte.MinValue);
			AssertTryParse("100", true, 100);
			AssertTryParse(byte.MaxValue.ToString(), true, byte.MaxValue);

			AssertTryParse((byte.MinValue - 1).ToString(), false, 0);
			AssertTryParse((byte.MaxValue + 1).ToString(), false, 0);

			AssertTryParse("some junk", false, 0);
		}

		public void TestParseSafe()
		{
			AssertEquals("37 as string", (ZByte)37, ZByte.ParseSafe("37", 2));
			AssertEquals("0 as string", ZByte.Zero, ZByte.ParseSafe("0", 1));
			AssertEquals("-12 as string", ZByte.Zero, ZByte.ParseSafe("-12", 0));
			AssertEquals("37 as ZString", (ZByte)37, ZByte.ParseSafe(new ZString("37"), 2));
			AssertEquals("0 as ZString", ZByte.Zero, ZByte.ParseSafe(new ZString("0"), 5));
			AssertEquals("-12 as ZString", (ZByte)72, ZByte.ParseSafe(new ZString("-12"), 72));

			foreach (string stringValue in new string[] { "", "NotNumber!" })
			{
				AssertEquals(stringValue + " as string", ZByte.Zero, ZByte.ParseSafe(stringValue, 0));
				AssertEquals(stringValue + " as ZString", (ZByte)2, ZByte.ParseSafe(new ZString(stringValue), 2));
			}
		}

		void AssertTryParse(string value, bool expectSuccess, byte expectedResult)
		{
			bool success = ZByte.TryParse(value, out var result);
			AssertEquals("TryParse success.", expectSuccess, success);
			AssertEquals("TryParse result.", expectedResult, result);
		}

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>2</a>", new ZByte(2));
		}

		#region IZTypeTest Overrides

		protected override IZType NewZ(object value)
		{
			return new ZByte(value);
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { new ZByte(), DBNull.Value, (byte)0 }; }
		}

		protected override object[] ValidValues
		{
			get { return new object[] { (byte)1, byte.MaxValue }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0.0, 0M, 0, (short)0 }; }
		}

		#endregion
	}
}
