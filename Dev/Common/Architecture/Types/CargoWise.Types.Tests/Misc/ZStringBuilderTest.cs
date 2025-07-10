using System;

namespace CargoWise.Types.Tests
{
	using NUnit.Framework;

	public class ZStringBuilderTest : TestCase
	{
		public void ToStringWithNewLineBetweenAppends()
		{
			ZStringBuilder builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(string.Empty);
			builder.AppendIfNotEmpty("One Line Here");
			builder.AppendIfNotEmpty(string.Empty);
			builder.AppendIfNotEmpty(string.Empty);
			builder.AppendIfNotEmpty("Another Line Here");
			builder.AppendIfNotEmpty(string.Empty);
			AssertEquals("One Line Here, Another Line Here", builder.ToStringWithDelimiterBetweenAppends(", "));
		}

		public void TestAppendIfNotEmpty_WithPrefix()
		{
			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty("Colour: ", "Black");
			builder.AppendIfNotEmpty("Size: ", "");
			builder.AppendIfNotEmpty("", "Value without prefix");
			builder.AppendIfNotEmpty(null, null);
			builder.AppendIfNotEmpty(null, "");
			builder.AppendIfNotEmpty("", null);

			AssertEquals("Colour: Black|Value without prefix", builder.ToStringWithDelimiterBetweenAppends("|"));
			AssertEquals(33, builder.Length);
		}

		public void TestConstructor()
		{
			ZStringBuilder builder = new ZStringBuilder();
			AssertEquals(0, builder.Length);
			builder = new ZStringBuilder(new string[] { "Hello", "world" });
			AssertEquals("Hello, world", builder.ToStringWithDelimiterBetweenAppends(", "));
		}

		public void TestLength()
		{
			ZStringBuilder builder = new ZStringBuilder();
			AssertEquals(0, builder.Length);
			builder.Prepend("Hello");
			AssertEquals(5, builder.Length);
			builder.Append("Goodbye");
			AssertEquals(12, builder.Length);
		}

		public void TestToStringWithDelimiterBetweenAppends()
		{
			ZStringBuilder builder = new ZStringBuilder();
			AssertEquals("ToStringWithDelimiterBetweenAppends", string.Empty, builder.ToStringWithDelimiterBetweenAppends("|"));

			builder.Append("A");
			builder.Append("BastardWantsItLonger");
			builder.Append("C");

			AssertEquals("ToStringWithDelimiterBetweenAppends", "A|BastardWantsItLonger|C", builder.ToStringWithDelimiterBetweenAppends("|"));
		}

		public void TestToStringWithNewLineBetweenAppends()
		{
			ZStringBuilder builder = new ZStringBuilder();
			AssertEquals("ToStringWithNewLineBetweenAppends", string.Empty, builder.ToStringWithNewLineBetweenAppends());

			builder.Append("A");
			builder.Append("BastardWantsItLonger");
			builder.Append("C");

			AssertEquals("ToStringWithNewLineBetweenAppends", "A" + Environment.NewLine + "BastardWantsItLonger" + Environment.NewLine + "C", builder.ToStringWithNewLineBetweenAppends());
		}

		public void TestZStringBuilder()
		{
			ZStringBuilder builder = new ZStringBuilder();
			AssertEquals("ToString", string.Empty, builder.ToString());

			builder.Append("A");
			builder.Append("B");
			builder.Append("C");

			AssertEquals("ToString", "ABC", builder.ToString());
		}

		public void TestIsEmpty()
		{
			ZStringBuilder builder = new ZStringBuilder();
			AssertEquals("IsEmpty", true, builder.IsEmpty);

			builder.Append(string.Empty);
			AssertEquals("IsEmpty", true, builder.IsEmpty);

			builder.Append(string.Empty);
			AssertEquals("IsEmpty", true, builder.IsEmpty);

			builder.Append("X");
			AssertEquals("IsEmpty", false, builder.IsEmpty);
		}

		public void TestMultipleAppends()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Assert("Precondtion - Builder should be empty", builder.IsEmpty);

			builder.Append("TEST1").Append("TEST2").Append("TEST3");
			AssertEquals("All text should be appended to the original Builder", "TEST1TEST2TEST3", builder.ToString());
		}

		public void TestPrepend()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Assert("Precondition - Builder should be empty", builder.IsEmpty);

			builder.Append("MEH");
			builder.Append("TEAPOT");
			builder.Prepend("BLAh");
			AssertEquals("BLAhMEHTEAPOT", builder.ToString());
		}

		public void TestAppendZStringBuilder()
		{
			ZStringBuilder sb1 = new ZStringBuilder();
			ZStringBuilder sb2 = new ZStringBuilder();

			sb1.Append("aaa");
			sb1.Append("bbb");

			sb2.Append("ccc");
			sb2.Append("ddd");

			sb1.Append(sb2);

			sb1.Append("eee");
			sb1.Append("fff");

			AssertEquals(18, sb1.Length);
			AssertEquals("aaabbbcccdddeeefff", sb1.ToString());
			AssertEquals("aaa,bbb,ccc,ddd,eee,fff", sb1.ToStringWithDelimiterBetweenAppends(","));
		}

		public void TestAppendFormat()
		{
			var sb1 = new ZStringBuilder();
			sb1.AppendFormat("This is {0}", "boring");
			AssertEquals("This is boring", sb1.ToString());
		}

		public void TestClear()
		{
			var builder = new ZStringBuilder("this is not empty");
			Assert(!builder.IsEmpty);
			Assert("Clear should return the emptied builder", builder.Clear().IsEmpty);
			Assert("Clear should empty the original builder", builder.IsEmpty);
		}
	}
}
