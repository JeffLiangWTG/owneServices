using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterInTest : TestCase
	{
		public void TestListSubExpressions()
		{
			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();

			AssertContainsExactElementsInAnyOrder(Array.Empty<IFilterExpression>(),
				new FilterIn(left.Object, new[] { right1.Object, right2.Object }).GetSubExpressions());
		}

		[ExpectNoExceptions]
		public void TestCollectConstraints()
		{
			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();
			var list = new List<string>();

			left.Setup(m => m.CollectConstraints(list));
			right1.Setup(m => m.CollectConstraints(list));
			right2.Setup(m => m.CollectConstraints(list));

			new FilterIn(left.Object, new[] { right1.Object, right2.Object }).CollectConstraints(list);
		}

		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();
			IFilterExpression filter = new FilterIn(left.Object, new IFilterOperand[] { right1.Object, right2.Object });
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");

			AssertEquals("'valueA' in ( 'valueA', ??? )", true, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			right2.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			AssertEquals("'valueA' in ( 'valueB', 'valueA' )", true, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			right2.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			AssertEquals("'valueA' in ( 'valueB', 'valueB' )", false, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(null);
			AssertEquals("null in ( 'valueA', 'valueA' )", false, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(null);
			right2.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(null);
			AssertEquals("null in ( null, null )", false, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(null);
			right2.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			AssertEquals("null in ( null, 'valueA' )", true, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			right2.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("value?");
			AssertEquals("'valueA' in ( 'valueB', 'value?' )", true, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			right2.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("*A");
			AssertEquals("'valueA' in ( 'valueB', '*A' )", true, filter.Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("VALUEa");
			right1.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			right2.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			AssertEquals("'VALUEa' in ( 'valueB', 'valueA' )", true, filter.Evaluate(provider.Object));
		}

		public void TestIsRequirementEnforced()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();
			var requirement = new FilterRequirement("Blat");
			var aB = new FilterRequirement("Blat") { "A", "B" };
			var b = new FilterRequirement("Blat") { "B" };
			var bC = new FilterRequirement("Blat") { "B", "C" };
			left.Setup(m => m.GetValuesForRequirement(requirement)).Returns(aB);
			right1.Setup(m => m.GetValuesForRequirement(requirement)).Returns(bC);
			AssertEquals(false, new FilterIn(left.Object, new IFilterOperand[] { right1.Object, right2.Object }).IsRequirementEnforced(requirement));
			left.Setup(m => m.GetValuesForRequirement(requirement)).Returns(aB);
			right1.Setup(m => m.GetValuesForRequirement(requirement)).Returns(b);
			right2.Setup(m => m.GetValuesForRequirement(requirement)).Returns(bC);
			AssertEquals(false, new FilterIn(left.Object, new IFilterOperand[] { right1.Object, right2.Object }).IsRequirementEnforced(requirement));
			left.Setup(m => m.GetValuesForRequirement(requirement)).Returns(aB);
			right1.Setup(m => m.GetValuesForRequirement(requirement)).Returns(b);
			right2.Setup(m => m.GetValuesForRequirement(requirement)).Returns(b);
			AssertEquals(true, new FilterIn(left.Object, new IFilterOperand[] { right1.Object, right2.Object }).IsRequirementEnforced(requirement));
		}

		public void TestIsEmpty()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();
			IFilterExpression filter = new FilterIn(left.Object, new[] { right1.Object, right2.Object });
			AssertEquals(false, filter.IsEmpty);
		}

		public void TestToString()
		{
			Action<StringBuilder> leftDelegate = delegate(StringBuilder strBuilder)
			{
				strBuilder.Append("<left>");
			};
			Action<StringBuilder> rightDelegate1 = delegate(StringBuilder strBuilder)
			{
				strBuilder.Append("<right1>");
			};
			Action<StringBuilder> rightDelegate2 = delegate(StringBuilder strBuilder)
			{
				strBuilder.Append("<right2>");
			};

			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();
			var builder = new StringBuilder();

			left.Setup(m => m.Append(builder)).Callback(leftDelegate);
			right1.Setup(m => m.Append(builder)).Callback(rightDelegate1);

			new FilterIn(left.Object, new IFilterOperand[] { right1.Object }).Append(builder);
			AssertEquals("<left> in (<right1>)", builder.ToString());
			builder.Length = 0;

			left.Setup(m => m.Append(builder)).Callback(leftDelegate);
			right1.Setup(m => m.Append(builder)).Callback(rightDelegate1);
			right2.Setup(m => m.Append(builder)).Callback(rightDelegate2);

			new FilterIn(left.Object, new IFilterOperand[] { right1.Object, right2.Object }).Append(builder);
			AssertEquals("<left> in (<right1>, <right2>)", builder.ToString());
		}

		public void TestAddConstraintPrefix()
		{
			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();
			var newLeft = new Mock<IFilterOperand>();
			var newRight1 = new Mock<IFilterOperand>();
			var newRight2 = new Mock<IFilterOperand>();
			var prefix = "Prefix";
			IFilterExpression result;
			left.Setup(m => m.AddConstraintPrefix(prefix)).Returns(newLeft.Object);
			right1.Setup(m => m.AddConstraintPrefix(prefix)).Returns(newRight1.Object);
			right2.Setup(m => m.AddConstraintPrefix(prefix)).Returns(newRight2.Object);

			result = new FilterIn(left.Object, new IFilterOperand[] { right1.Object, right2.Object }).AddConstraintPrefix(prefix);

			AssertType(typeof(FilterIn), result);
			var filterIn = (FilterIn)result;
			AssertSame("Left", newLeft.Object, filterIn.Left);
			var right = new List<IFilterOperand>(filterIn.Right);
			AssertEquals("Right.Count", 2, right.Count);
			AssertSame("Right1", newRight1.Object, right[0]);
			AssertSame("Right2", newRight2.Object, right[1]);
		}

		public void TestRemoveConstraintPrefix()
		{
			var left = new Mock<IFilterOperand>();
			var right1 = new Mock<IFilterOperand>();
			var right2 = new Mock<IFilterOperand>();
			var newLeft = new Mock<IFilterOperand>();
			var newRight1 = new Mock<IFilterOperand>();
			var newRight2 = new Mock<IFilterOperand>();
			var prefix = "Prefix";

			IFilterExpression result;
			left.Setup(m => m.RemoveConstraintPrefix(prefix)).Returns(newLeft.Object);
			right1.Setup(m => m.RemoveConstraintPrefix(prefix)).Returns(newRight1.Object);
			right2.Setup(m => m.RemoveConstraintPrefix(prefix)).Returns(newRight2.Object);

			result = new FilterIn(left.Object, new[] { right1.Object, right2.Object }).RemoveConstraintPrefix(prefix);
			AssertType(typeof(FilterIn), result);
			var filterIn = (FilterIn)result;
			AssertSame("Left", newLeft.Object, filterIn.Left);
			var right = new List<IFilterOperand>(filterIn.Right);
			AssertEquals("Right.Count", 2, right.Count);
			AssertSame("Right1", newRight1.Object, right[0]);
			AssertSame("Right2", newRight2.Object, right[1]);
		}
	}
}
