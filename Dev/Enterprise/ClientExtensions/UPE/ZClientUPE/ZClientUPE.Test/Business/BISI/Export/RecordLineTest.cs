using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class RecordLineTest : TestCase
	{
		[TestDate(2002, 02, 20, 14, 21, 22)]
		public void TestLineAsString()
		{
			AssertEquals("00234502002-02-20 14:21:22  12NEWF0", Line.LineAsString);
		}

		#region Test Append Methods
		public void TestAppendDecimalField()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Line.AppendFixedLengthField(builder, 23.3847m, 2, 7);
			AssertEquals("0002338", builder.ToString());
			Line.AppendFixedLengthField(builder, 23.3847m, 4, 7);
			AssertEquals("00023380233847", builder.ToString());
			Line.AppendFixedLengthField(builder, 23.3847m, 4, 4);
			AssertEquals("000233802338472338", builder.ToString());
			Line.AppendFixedLengthField(builder, 23.3847m, 3, 6);
			AssertEquals("000233802338472338023384", builder.ToString());
			Line.AppendFixedLengthField(builder, 23.3847m, 1, 6);
			AssertEquals("000233802338472338023384000233", builder.ToString());
			Line.AppendFixedLengthField(builder, 23.3847m, 0, 6);
			AssertEquals("000233802338472338023384000233000023", builder.ToString());
			Line.AppendFixedLengthField(builder, 123m, 3, 7);
			AssertEquals("0002338023384723380233840002330000230123000", builder.ToString());
			Line.AppendFixedLengthField(builder, 123m, 3, 4);
			AssertEquals("00023380233847233802338400023300002301230001230", builder.ToString());
		}

		public void TestAppendDateTimeField()
		{
			ZStringBuilder builder = new ZStringBuilder();
			ZDateTime testDateTime = new ZDateTime(2005, 12, 11, 23, 10, 12);
			Line.AppendFixedLengthField(builder, testDateTime, false, 10);
			AssertEquals("2005-12-11", builder.ToString());
			Line.AppendFixedLengthField(builder, testDateTime, true, 10);
			AssertEquals("2005-12-1123:10:12  ", builder.ToString());
			Line.AppendFixedLengthField(builder, testDateTime, true, 2);
			AssertEquals("2005-12-1123:10:12  23", builder.ToString());
		}

		public void TestAppendIntField()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Line.AppendFixedLengthField(builder, 1, 1);
			AssertEquals("1", builder.ToString());
			Line.AppendFixedLengthField(builder, 7890, 5);
			AssertEquals("107890", builder.ToString());
			Line.AppendFixedLengthField(builder, 8898, 3);
			AssertEquals("107890889", builder.ToString());
		}

		public void TestAppendStringField()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Line.AppendFixedLengthField(builder, "test", 5);
			AssertEquals("test ", builder.ToString());
			Line.AppendFixedLengthField(builder, "t", 2);
			AssertEquals("test t ", builder.ToString());
			Line.AppendFixedLengthField(builder, "", 2);
			AssertEquals("test t   ", builder.ToString());
			Line.AppendFixedLengthField(builder, "xxx", 1);
			AssertEquals("test t   x", builder.ToString());
		}

		public void TestAppendBoolField()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Line.AppendFixedLengthField(builder, true, 1);
			AssertEquals("1", builder.ToString());
			Line.AppendFixedLengthField(builder, false, 1);
			AssertEquals("10", builder.ToString());
			Line.AppendFixedLengthField(builder, true, 3);
			AssertEquals("101  ", builder.ToString());
		}

		#endregion
		#region RecordLineForTest
		RecordLineForTest Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new RecordLineForTest();
				}

				return fLine;
			}
		}

		RecordLineForTest fLine;
		class RecordLineForTest : RecordLine
		{
			public new void AppendFixedLengthField(ZStringBuilder lineBuilder, ZDecimal value, int decimalPlace, int fieldLength)
			{
				base.AppendFixedLengthField(lineBuilder, value, decimalPlace, fieldLength);
			}

			public new void AppendFixedLengthField(ZStringBuilder lineBuilder, ZDateTime value, bool asTime, int fieldLength)
			{
				base.AppendFixedLengthField(lineBuilder, value, asTime, fieldLength);
			}

			public new void AppendFixedLengthField(ZStringBuilder lineBuilder, ZInt value, int fieldLength)
			{
				base.AppendFixedLengthField(lineBuilder, value, fieldLength);
			}

			public new void AppendFixedLengthField(ZStringBuilder lineBuilder, ZString value, int fieldLength)
			{
				base.AppendFixedLengthField(lineBuilder, value, fieldLength);
			}

			public new void AppendFixedLengthField(ZStringBuilder lineBuilder, ZBool value, int fieldLength)
			{
				base.AppendFixedLengthField(lineBuilder, value, fieldLength);
			}

			protected override void AppendFields(ZStringBuilder builder)
			{
				AppendFixedLengthField(builder, 23.45m, 3, 7);
				AppendFixedLengthField(builder, ZDateTime.Now, false, 11);
				AppendFixedLengthField(builder, ZDateTime.Now, true, 10);
				AppendFixedLengthField(builder, 12, 2);
				AppendFixedLengthField(builder, "NEWFIELD", 4);
				AppendFixedLengthField(builder, false, 1);
			}
		}
		#endregion
	}
}
