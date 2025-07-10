using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal abstract class FilterCheckTest : TestCase
	{
		public void TestListSubExpressions()
		{
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<IFilterExpression>(), NewCheck(left.Object, right.Object).GetSubExpressions());
		}

		[ExpectNoExceptions]
		public void TestCollectConstraints()
		{
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var list = new List<string>();
			left.Setup(m => m.CollectConstraints(list));
			right.Setup(m => m.CollectConstraints(list));
			NewCheck(left.Object, right.Object).CollectConstraints(list);
		}

		public void TestIsEmpty()
		{
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			IFilterExpression exp = NewCheck(left.Object, right.Object);
			AssertEquals(false, exp.IsEmpty);
		}

		public void TestAddConstraintPrefix()
		{
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var newLeft = new Mock<IFilterOperand>();
			var newRight = new Mock<IFilterOperand>();
			var prefix = "Prefix";
			IFilterExpression inExp = NewCheck(left.Object, right.Object);
			left.Setup(m => m.AddConstraintPrefix(prefix)).Returns(newLeft.Object);
			right.Setup(m => m.AddConstraintPrefix(prefix)).Returns(newRight.Object);
			var result = inExp.AddConstraintPrefix(prefix);

			AssertType(inExp.GetType(), result);
			var check = (FilterCheck)result;
			AssertSame("Left", newLeft.Object, check.Left);
			AssertSame("Right", newRight.Object, check.Right);
		}

		public void TestRemoveConstraintPrefix()
		{
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var newLeft = new Mock<IFilterOperand>();
			var newRight = new Mock<IFilterOperand>();
			var prefix = "Prefix";
			IFilterExpression inExp = NewCheck(left.Object, right.Object);
			IFilterExpression result;
			left.Setup(m => m.RemoveConstraintPrefix(prefix)).Returns(newLeft.Object);
			right.Setup(m => m.RemoveConstraintPrefix(prefix)).Returns(newRight.Object);
			result = inExp.RemoveConstraintPrefix(prefix);
			AssertType(inExp.GetType(), result);
			var check = (FilterCheck)result;
			AssertSame("Left", newLeft.Object, check.Left);
			AssertSame("Right", newRight.Object, check.Right);
		}

		#region Implementation
		protected abstract FilterCheck NewCheck(IFilterOperand lhs, IFilterOperand rhs);
		#endregion
	}
}
