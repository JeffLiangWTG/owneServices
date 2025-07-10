using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class ViewTest : TestCase
	{
		public void TestSearchCondition_ShouldTrim()
		{
			CombineAssertions(() =>
			{
				AssertSearchCondition_ShouldTrim(null, null);
				AssertSearchCondition_ShouldTrim("", "");
				AssertSearchCondition_ShouldTrim(" ", "");
				AssertSearchCondition_ShouldTrim(" SomeSearch Condition \r\n ", "SomeSearch Condition");
			});
		}

		void AssertSearchCondition_ShouldTrim(string searchCondition, string expectedSearchCondition)
		{
			var view = new View { SearchCondition = searchCondition };

			AssertEquals(expectedSearchCondition, view.SearchCondition);
		}
	}
}
