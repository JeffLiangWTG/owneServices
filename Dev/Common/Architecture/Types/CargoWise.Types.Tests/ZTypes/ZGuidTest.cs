using System;
using CargoWise.Common;

namespace CargoWise.Types.Tests
{
	class ZGuidTest : IZTypeTest
	{
		public void TestNullConstructor()
		{
			AssertEquals(ZGuid.Empty, new ZGuid(null));
		}

		public void TestDefaultGuids()
		{
			var empty = new ZGuid(new Guid("00000000-0000-0000-0000-000000000000"));
			AssertEquals(true, empty.IsEmpty);

			var invalid = new ZGuid(new Guid("0000ffff-ffff-ffff-ffff-ffffffffffff"));
			AssertEquals(false, invalid.IsValid);

			var missing = new ZGuid(new Guid("1111ffff-ffff-ffff-ffff-ffffffffffff"));
			AssertEquals(true, missing.IsMissing);
		}

		public void TestFormatException()
		{
			foreach (object value in BadGuidStringValues)
			{
				bool wasThrown = false;

				try
				{
					NewZ(value);
				}
				catch (FormatException)
				{
					wasThrown = true;
				}

				Assert(MessageForValue(value), wasThrown);
			}
		}

		public void TestEqualsBadGuidString()
		{
			foreach (object value in BadGuidStringValues)
			{
				AssertEquals(false, ZGuid.Empty.Equals(value));
			}
		}

		public void TestNewZGuid()
		{
			foreach (object value in AllValues)
			{
				string message = MessageForValue(value);
				ZGuid z = new ZGuid(value);
				AssertEquals(message, false, z.Equals(ZGuid.NewZGuid()));
			}
		}

		public void TestToGuid()
		{
			foreach (object value in ValidValues)
			{
				string message = MessageForValue(value);
				Guid expected = new Guid(value.ToString());
				Guid actual = new ZGuid(value).ToGuid();
				AssertEquals(message, expected, actual);
			}
		}

		public void TestEmpty()
		{
			AssertEquals("IsEmpty", true, ZGuid.Empty.IsEmpty);
			AssertEquals("IsValid", false, ZGuid.Empty.IsValid);
			AssertEquals("IsMissing", false, ZGuid.Empty.IsMissing);
			Assert("Equals", ZGuid.Empty.Equals(ZGuid.Empty));
			Assert("Equals Guid.Empty", ZGuid.Empty.Equals(Guid.Empty));
		}

		public void TestInvalid()
		{
			AssertEquals("IsEmpty", false, ZGuid.Invalid.IsEmpty);
			AssertEquals("IsValid", false, ZGuid.Invalid.IsValid);
			AssertEquals("IsMissing", false, ZGuid.Invalid.IsMissing);
			Assert("Equals", ZGuid.Invalid.Equals(ZGuid.Invalid));
		}

		public void TestMissing()
		{
			AssertEquals("IsEmpty", false, ZGuid.Missing.IsEmpty);
			AssertEquals("IsValid", false, ZGuid.Missing.IsValid);
			AssertEquals("IsMissing", true, ZGuid.Missing.IsMissing);
			Assert("Equals", ZGuid.Missing.Equals(ZGuid.Missing));
		}

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>12345678-1234-1234-1234-123456789012</a>", new ZGuid("{12345678-1234-1234-1234-123456789012}"));
		}

		public void TestParseSafe()
		{
			AssertEquals(ZGuid.Empty, ZGuid.ParseSafe(""));
			AssertEquals(ZGuid.Empty, ZGuid.ParseSafe(null));
			AssertEquals(ZGuid.Invalid, ZGuid.ParseSafe("asdasdasd"));
			var valid = Guid.NewGuid();
			AssertEquals(valid, ZGuid.ParseSafe(valid.ToString()));
		}

		public void TestTryParse()
		{
			bool successfull;

			successfull = ZGuid.TryParse("asdasdasd", out var guid);
			Assert(!successfull);
			AssertEquals(ZGuid.Empty, guid);

			successfull = ZGuid.TryParse(null, out guid);
			Assert(successfull);
			AssertEquals(ZGuid.Empty, guid);

			Guid valid = Guid.NewGuid();

			successfull = ZGuid.TryParse(valid, out guid);
			Assert(successfull);
			AssertEquals(valid, guid);

			successfull = ZGuid.TryParse(valid.ToString(), out guid);
			Assert(successfull);
			AssertEquals(valid, guid);
		}

		public void TestIsGuid()
		{
			ZString value = "D859BB18-15FA-4ce6-A27D-4A720B1DB60E";
			Assert(ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce6-A27D-4A720B1DB60";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce6-A27-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15F-4ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB1-15FA-4ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce6-A27D-4A720B1DB60E1";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce6-A27D1-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA1-4ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB181-15FA-4ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce6-A27D24A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce62A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA24ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18215FA-4ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "X859BB18-15FA-4ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-X5FA-4ce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-Xce6-A27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce6-X27D-4A720B1DB60E";
			Assert(!ZGuid.IsGuid(value));

			value = "D859BB18-15FA-4ce6-A27D-XA720B1DB60E";
			Assert(!ZGuid.IsGuid(value));
		}

		public void TestGuidName_MissAndInvalid()
		{
			AssertEquals("name of Missing Guid", nameof(ZGuid.Missing), ZTypeConstants.MissingGuidName);
			AssertEquals("name of Invalid Guid", nameof(ZGuid.Invalid), ZTypeConstants.InvalidGuidName);
		}

		#region IZTypeTest Overrides

		protected static readonly Guid ValidGuidValue = Guid.NewGuid();

		protected override IZType NewZ(object value)
		{
			return new ZGuid(value);
		}

		protected override object[] ValidValues
		{
			get { return new object[] { ValidGuidValue }; }
		}

		protected override object[] InvalidValues
		{
			get { return new object[] { ZGuid.Invalid, ZGuid.Missing }; }
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { "", ZString.Empty, null, DBNull.Value, Guid.Empty, Guid.Empty.ToString(), new Guid(), ZGuid.Empty, new ZGuid(), new ZGuid(new Guid()), new ZString(new Guid().ToString()) }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0, Guid.Empty.ToByteArray() }; }
		}

		protected object[] BadGuidStringValues
		{
			get { return new object[] { "1234", "This string is not a Guid" }; }
		}

		protected override bool ValueIsValid(object value)
		{
			if (value is IZType && value.GetType() == AnyZ.GetType())
			{
				return ((IZType)value).IsValid;
			}
			else
			{
				return ArrayContainsValue(ValidValues, value);
			}
		}

		protected override bool ArrayContainsValue(Array values, object value)
		{
			if (value is ZString)
			{
				value = (string)(ZString)value;
			}
			return base.ArrayContainsValue(values, value);
		}

		#endregion
	}
}
