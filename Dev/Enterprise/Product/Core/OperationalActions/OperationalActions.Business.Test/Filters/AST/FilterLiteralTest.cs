using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterLiteralTest : TestCase
	{
		public void TestCollectConstraints()
		{
			var list = new List<string>();
			var literal = new FilterLiteral("value");
			literal.CollectConstraints(list);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), list);
		}

		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			AssertEquals("value1", new FilterLiteral("value1").Evaluate(provider.Object));
			AssertEquals("value2", new FilterLiteral("value2").Evaluate(provider.Object));
		}

		public void TestAppend()
		{
			var literal = new FilterLiteral("val\"ue");
			var builder = new StringBuilder();
			literal.Append(builder);
			AssertEquals("\"val\"\"ue\"", builder.ToString());
		}

		public void TestGetValuesForRequirement()
		{
			var inRequirement = new FilterRequirement("Blat") { "Value1" };
			var outRequirement = new FilterLiteral("Value2").GetValuesForRequirement(inRequirement);
			CombineAssertions(delegate
			{
				AssertEquals("Constraint Name", "Blat", outRequirement.ConstraintName);
				AssertContainsExactElementsInAnyOrder("Values", new[] { "Value2" }, outRequirement);
			});
		}

		public void TestAddConstraintPrefix()
		{
			IFilterOperand filter = new FilterLiteral("value");
			AssertSame(filter, filter.AddConstraintPrefix("prefix"));
		}

		public void TestRemoveConstraintPrefix()
		{
			IFilterOperand filter = new FilterLiteral("value");
			AssertSame(filter, filter.RemoveConstraintPrefix("prefix"));
		}
	}
}
