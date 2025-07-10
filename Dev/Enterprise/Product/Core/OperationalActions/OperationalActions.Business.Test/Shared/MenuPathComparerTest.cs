using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class MenuPathComparerTest : TestCase
	{
		public void TestCompare()
		{
			AssertPathEqual("Foo/Bar", "foo/bar", "Foo//Bar", "Foo / Bar", "/ Foo / Bar /");
			AssertPathEqual("Foo Bar", "foo bar", "Foo\tBar", "Foo   Bar");
			AssertPathEqual("", " ", "\t", "\n", "/");
			AssertPathOrder("Foo", "");
			AssertPathOrder("Foo", "             ");
			AssertPathOrder("bar", "FOO");
			AssertPathOrder("BAR", "foo");
			AssertPathOrder("Foo/Bar", "FooBar");
			AssertPathOrder("Foo/Bar", "Foo Bar");
			AssertPathOrder("Foo Bar", "FooBar");
			AssertPathOrder("Bar/Foo", "Foo/Bar");
		}

		public void TestNormalise()
		{
			AssertEquals("Foo/Bar/Foo Bar", MenuPathComparer.NormalisePath(" / Foo / / Bar / Foo\tBar / "));
		}

		#region Implementation
		void AssertPathEqual(string path1, params string[] otherPaths)
		{
			foreach (string path2 in otherPaths)
			{
				AssertPathEqual(path1, path2);
			}
		}

		void AssertPathEqual(string path1, string path2)
		{
			AssertEquals(string.Format("ComparePath('{0}', '{1}') == 0", path1, path2), 0, MenuPathComparer.ComparePath(path1, path2));
			AssertEquals(string.Format("HashCode('{0}') == HashCode('{1}')", path1, path2), MenuPathComparer.HashCode(path1), MenuPathComparer.HashCode(path2));
		}

		void AssertPathOrder(string firstPath, string secondPath)
		{
			int result;
			if ((result = MenuPathComparer.ComparePath(firstPath, secondPath)) >= 0)
			{
				HtmlFail(Html(string.Format("ComparePath('{0}', '{1}') < 0", firstPath, secondPath)) + "</br>" + HtmlFormatBadValue(result));
			}

			if ((result = MenuPathComparer.ComparePath(secondPath, firstPath)) <= 0)
			{
				HtmlFail(Html(string.Format("ComparePath('{0}', '{1}') > 0", secondPath, firstPath)) + "</br>" + HtmlFormatBadValue(result));
			}

			Assert(true);
		}
		#endregion
	}
}
