using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterAndTest : TestCase
	{
		public void TestListSubExpressions()
		{
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			AssertContainsExactElementsInAnyOrder(new[] { left.Object, right.Object },
				new FilterAnd(left.Object, right.Object).GetSubExpressions());
		}

		[ExpectNoExceptions]
		public void TestCollectConstraints()
		{
			var list = new List<string>();
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();

			left.Setup(m => m.CollectConstraints(list));
			right.Setup(m => m.CollectConstraints(list));
			new FilterAnd(left.Object, right.Object).CollectConstraints(list);
		}

		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();

			left.Setup(m => m.Evaluate(provider.Object)).Returns(true);
			right.Setup(m => m.Evaluate(provider.Object)).Returns(true);
			AssertEquals("true && true", true, new FilterAnd(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object)).Returns(true);
			right.Setup(m => m.Evaluate(provider.Object)).Returns(false);
			AssertEquals("true && false", false, new FilterAnd(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object)).Returns(false);
			AssertEquals("false && true", false, new FilterAnd(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestIsRequirementEnforced()
		{
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			var requirement = new FilterRequirement("Blat");
			left.Setup(m => m.IsRequirementEnforced(requirement)).Returns(false);
			right.Setup(m => m.IsRequirementEnforced(requirement)).Returns(false);

			AssertEquals(false, new FilterAnd(left.Object, right.Object).IsRequirementEnforced(requirement));

			left.Setup(m => m.IsRequirementEnforced(requirement)).Returns(false);
			right.Setup(m => m.IsRequirementEnforced(requirement)).Returns(true);

			AssertEquals(true, new FilterAnd(left.Object, right.Object).IsRequirementEnforced(requirement));

			left.Setup(m => m.IsRequirementEnforced(requirement)).Returns(true);

			AssertEquals(true, new FilterAnd(left.Object, right.Object).IsRequirementEnforced(requirement));
		}

		public void TestIsEmpty()
		{
			var left = new Mock<IFilterExpression>();
			var right = new Mock<IFilterExpression>();
			var exp = new FilterAnd(left.Object, right.Object) as IFilterExpression;
			AssertEquals(false, exp.IsEmpty);
		}

		public void TestAppend()
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
			left.Setup(m => m.Precidence).Returns(FilterPrecedence.And);
			right.Setup(m => m.Precidence).Returns(FilterPrecedence.And + 1);
			left.Setup(m => m.Append(builder)).Callback(leftDelegate);
			right.Setup(m => m.Append(builder)).Callback(rightDelegate);

			new FilterAnd(left.Object, right.Object).Append(builder);
			AssertEquals("<left> && (<right>)", builder.ToString());

			builder.Length = 0;
			left.Setup(m => m.Precidence).Returns(FilterPrecedence.And + 1);
			right.Setup(m => m.Precidence).Returns(FilterPrecedence.And);

			left.Setup(m => m.Append(builder)).Callback(leftDelegate);
			right.Setup(m => m.Append(builder)).Callback(rightDelegate);

			new FilterAnd(left.Object, right.Object).Append(builder);
			AssertEquals("(<left>) && <right>", builder.ToString());
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
			var result = new FilterAnd(left.Object, right.Object).AddConstraintPrefix(prefix);
			AssertType(typeof(FilterAnd), result);
			var and = (FilterAnd)result;
			AssertSame("Left", newLeft.Object, and.Left);
			AssertSame("Right", newRight.Object, and.Right);
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
			var result = new FilterAnd(left.Object, right.Object).RemoveConstraintPrefix(prefix);
			AssertType(typeof(FilterAnd), result);
			FilterAnd and = (FilterAnd)result;
			AssertSame("Left", newLeft.Object, and.Left);
			AssertSame("Right", newRight.Object, and.Right);
		}
	}
}
