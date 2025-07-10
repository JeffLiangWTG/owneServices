using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FixedWidthLineBuilderTest : TestCase
	{
		public void TestAppendToLine_StringIsSmallerInLengthThanMaxLength()
		{
			Builder.AddDataToLine("hello", 10, 6);
			string data = Builder.ToString();
			AssertEquals("string was not truncated", "hello ", data.Substring(10, 6));
		}

		public void TestAppendToLine_TruncatesString()
		{
			Builder.AddDataToLine("hello", 5, 4);
			string data = Builder.ToString();
			AssertEquals("string was not truncated", "hell", data.Substring(5, 4));
			AssertEquals(50, Builder.ToString().Length);
		}

		public void TestPadLeftRight()
		{
			Builder.AddDataToLine("123", 5, 4);
			AssertEquals("string was shifted left", "123 ", Builder.ToString().Substring(5, 4));

			Builder.AddDataToLine("123", 5, 4, true);
			AssertEquals("string was shifted right", " 123", Builder.ToString().Substring(5, 4));

			AssertEquals(50, Builder.ToString().Length);
		}

		public void TestPadLeftRight_WhiteChar()
		{
			Builder.AddDataToLine("123", 5, 4);
			AssertEquals("string was shifted left", "123 ", Builder.ToString().Substring(5, 4));

			Builder.AddDataToLine("123", 5, 4, true, 'A');
			AssertEquals("string was shifted right", "A123", Builder.ToString().Substring(5, 4));

			AssertEquals(50, Builder.ToString().Length);
		}

		public void TestCreateBlankLine()
		{
			string data = Builder.ToString();
			AssertEquals("Incorrect string length", 50, data.Length);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			Builder = new FixedWidthLineBuilder(' ', 50);
		}

		FixedWidthLineBuilder Builder;

		#endregion
	}
}
