using System.Text;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterEmptyTest : TestCase
	{
		public void TestListSubExpressions()
		{
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<IFilterExpression>(), new FilterEmpty().GetSubExpressions());
		}

		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			AssertEquals(true, new FilterEmpty().Evaluate(provider.Object));
		}

		public void TestIsEmpty()
		{
			IFilterExpression exp = new FilterEmpty();
			AssertEquals(true, exp.IsEmpty);
		}

		public void TestAppend()
		{
			StringBuilder builder = new StringBuilder();
			new FilterEmpty().Append(builder);
			AssertEquals("", builder.ToString());
		}

		public void TestAddConstraintPrefix()
		{
			IFilterExpression empty = new FilterEmpty();
			AssertSame(empty, empty.AddConstraintPrefix("bob"));
		}

		public void TestRemoveConstraintPrefix()
		{
			IFilterExpression empty = new FilterEmpty();
			AssertSame(empty, empty.RemoveConstraintPrefix("bob"));
		}
	}
}
