using System;

namespace CargoWise.Types.Tests
{
	public class ZBoolTest : IZTypeTest
	{
		public void TestStringNullConstructor()
		{
			string obj = null;
			AssertEquals(false, new ZBool(obj));
		}

		public void TestObjectNullConstructor()
		{
			object obj = null;
			AssertEquals(false, new ZBool(obj));
		}

		public void TestEqualityOperator()
		{
			Assert("When value is false", new ZBool(false) == new ZBool(false));
			Assert("When value is true", new ZBool(true) == new ZBool(true));
		}

		public void TestInequalityOperator()
		{
			AssertEquals("When value is false", false, new ZBool(false) != new ZBool(false));
			AssertEquals("When value is true", false, new ZBool(true) != new ZBool(true));
		}

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>true</a>", ZBool.True);
			AssertZTypeSerializesToXml("<a>false</a>", ZBool.False);
		}

		public void TestBoolConverter()
		{
			var incorrect = true;
			AssertExceptionThrown<ArgumentException>(() => ZBoolTypeConverter.Instance.ConvertTo(incorrect, typeof(bool)));
		}

		public void TestTryParse()
		{
			AssertTryParse("tRue", true, ZBool.True);
			AssertTryParse("YeS", true, ZBool.True);
			AssertTryParse("1", true, ZBool.True);
			AssertTryParse("t", true, ZBool.True);
			AssertTryParse("Y", true, ZBool.True);
			AssertTryParse("False", true, ZBool.False);
			AssertTryParse("nO", true, ZBool.False);
			AssertTryParse("n", true, ZBool.False);
			AssertTryParse("F", true, ZBool.False);
			AssertTryParse(" ", true, ZBool.False);
			AssertTryParse("0", true, ZBool.False);
		}

		public void TestParseSafe()
		{
			AssertEquals("37 as string", ZBool.True, ZBool.ParseSafe("37", ZBool.True));
			AssertEquals("0 as string", ZBool.False, ZBool.ParseSafe("0", ZBool.True));
			AssertEquals("f as string", ZBool.False, ZBool.ParseSafe("f", ZBool.True));
		}

		void AssertTryParse(string value, bool expectSuccess, ZBool expectedResult)
		{
			bool success = ZBool.TryParse(value, out var result);
			AssertEquals("TryParse success.", expectSuccess, success);
			AssertEquals("TryParse result.", expectedResult, result);
		}

		//public void TestNotImplementation()
		//{
		//  AssertEquals(ZBool.True, !ZBool.False);
		//  AssertEquals(ZBool.False, !ZBool.True);
		//}

		#region IZTypeTest Overrides

		public override void TestToString()
		{
			foreach (object value in ValidValues)
			{
				ZBool z = (ZBool)NewZ(value);
				AssertEquals(MessageForValue(value), z ? "Y" : "N", z.ToString());
			}
		}

		public override void TestEquals()
		{
			foreach (object value in AllValues)
			{
				string message = MessageForValue(value);
				IZType z = NewZ(value);

				AssertEquals(message, true, z.Equals(NewZ(value)));
				AssertEquals(message, value is bool || value is ZBool, z.Equals(value));
			}
		}

		public override void TestGetValue()
		{
			foreach (object value1 in AllValues)
			{
				string message = MessageForValue(value1) + " ";
				IZType z = NewZ(value1);

				object actualFalse = ((IZTypeInternals)z).GetValueForLogicalDataLayer(false);
				object actualTrue = ((IZTypeInternals)z).GetValueForLogicalDataLayer(true);

				bool value = new ZBool(value1);
				AssertEquals(message + "GetValue(false)", value, actualFalse);
				AssertEquals(message + "GetValue(true)", !value ? DBNull.Value : actualFalse, actualTrue);
			}
		}

		protected override IZType NewZ(object value)
		{
			return new ZBool(value);
		}

		protected override object[] ValidValues
		{
			get { return new object[] { true, false, "Y", "y", 'y', "N", "n", 'n', " ", ' ', "1", "0", 1, 0 }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0.0, 0M, "Yes!", "No." }; }
		}

		#endregion
	}
}
