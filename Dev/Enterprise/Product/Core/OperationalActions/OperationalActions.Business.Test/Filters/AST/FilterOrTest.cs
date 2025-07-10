using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterOrTest : TestCase
	{
		public void TestListSubExpressions()
		{
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			AssertContainsExactElementsInAnyOrder(new[] { left.Object, right.Object }, new FilterOr(left.Object, right.Object).GetSubExpressions());
		}

		[ExpectNoExceptions]
		public void TestCollectConstraints()
		{
			var list = new List<string>();
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			left.Setup(m => m.CollectConstraints(list));
			right.Setup(m => m.CollectConstraints(list));
			new FilterOr(left.Object, right.Object).CollectConstraints(list);
		}

		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			left.Setup(m => m.Evaluate(provider.Object)).Returns(true);

			AssertEquals("true || true", true, new FilterOr(left.Object, right.Object).Evaluate(provider.Object));

			left.Setup(m => m.Evaluate(provider.Object)).Returns(false);
			right.Setup(m => m.Evaluate(provider.Object)).Returns(false);
			AssertEquals("false || false", false, new FilterOr(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object)).Returns(false);
			right.Setup(m => m.Evaluate(provider.Object)).Returns(true);
			AssertEquals("false || true", true, new FilterOr(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestIsRequirementEnforced()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			var requirement = new FilterRequirement("Blat");
			left.Setup(m => m.IsRequirementEnforced(requirement)).Returns(true);
			right.Setup(m => m.IsRequirementEnforced(requirement)).Returns(true);
			AssertEquals(true, new FilterOr(left.Object, right.Object).IsRequirementEnforced(requirement));
			left.Setup(m => m.IsRequirementEnforced(requirement)).Returns(true);
			right.Setup(m => m.IsRequirementEnforced(requirement)).Returns(false);
			AssertEquals(false, new FilterOr(left.Object, right.Object).IsRequirementEnforced(requirement));
			left.Setup(m => m.IsRequirementEnforced(requirement)).Returns(false);
			AssertEquals(false, new FilterOr(left.Object, right.Object).IsRequirementEnforced(requirement));
		}

		public void TestIsEmpty()
		{
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			IFilterExpression exp = new FilterOr(left.Object, right.Object);
			AssertEquals(false, exp.IsEmpty);
		}

		public void TestToString()
		{
			Action<StringBuilder> leftDelegate = delegate(StringBuilder strBuilder)
			{
				strBuilder.Append("<left>");
			};
			Action<StringBuilder> rightDelegate = delegate(StringBuilder strBuilder)
			{
				strBuilder.Append("<right>");
			};
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			var builder = new StringBuilder();
			left.Setup(m => m.Precidence).Returns(FilterPrecedence.Or);
			right.Setup(m => m.Precidence).Returns(FilterPrecedence.Or + 1);

			left.Setup(m => m.Append(builder)).Callback(leftDelegate);
			right.Setup(m => m.Append(builder)).Callback(rightDelegate);

			new FilterOr(left.Object, right.Object).Append(builder);
			AssertEquals("<left> || (<right>)", builder.ToString());
			builder.Length = 0;

			left.Setup(m => m.Precidence).Returns(FilterPrecedence.Or + 1);
			right.Setup(m => m.Precidence).Returns(FilterPrecedence.Or);

			left.Setup(m => m.Append(builder)).Callback(leftDelegate);
			right.Setup(m => m.Append(builder)).Callback(rightDelegate);
			new FilterOr(left.Object, right.Object).Append(builder);
			AssertEquals("(<left>) || <right>", builder.ToString());
		}

		public void TestAddConstraintPrefix()
		{
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			var newLeft = new Mock<IFilterExpression>();
			var newRight = new Mock<IFilterExpression>();
			var prefix = "Prefix";
			left.Setup(m => m.AddConstraintPrefix(prefix)).Returns(newLeft.Object);
			right.Setup(m => m.AddConstraintPrefix(prefix)).Returns(newRight.Object);

			var result = new FilterOr(left.Object, right.Object).AddConstraintPrefix(prefix);

			AssertType(typeof(FilterOr), result);
			var or = (FilterOr)result;
			AssertSame("Left", newLeft.Object, or.Left);
			AssertSame("Right", newRight.Object, or.Right);
		}

		public void TestRemoveConstraintPrefix()
		{
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			var newLeft = new Mock<IFilterExpression>();
			var newRight = new Mock<IFilterExpression>();
			var prefix = "Prefix";
			left.Setup(m => m.RemoveConstraintPrefix(prefix)).Returns(newLeft.Object);
			right.Setup(m => m.RemoveConstraintPrefix(prefix)).Returns(newRight.Object);

			var result = new FilterOr(left.Object, right.Object).RemoveConstraintPrefix(prefix);

			AssertType(typeof(FilterOr), result);
			var or = (FilterOr)result;
			AssertSame("Left", newLeft.Object, or.Left);
			AssertSame("Right", newRight.Object, or.Right);
		}
	}
}
