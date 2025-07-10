using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ListFormatterTest : TestCase
	{
		public void TestGetCommaSeparatedText()
		{
			List<int> list = new List<int>();

			list.Add(1);
			AssertEquals("GetCommaSeparatedText<>()", "'1'", ListFormatter.GetCommaSeparatedText(list));

			list.Add(2);
			AssertEquals("GetCommaSeparatedText<>()", "'1' and '2'", ListFormatter.GetCommaSeparatedText(list));

			list.Add(3);
			AssertEquals("GetCommaSeparatedText<>()", "'1', '2' and '3'", ListFormatter.GetCommaSeparatedText(list));
		}
	}
}
