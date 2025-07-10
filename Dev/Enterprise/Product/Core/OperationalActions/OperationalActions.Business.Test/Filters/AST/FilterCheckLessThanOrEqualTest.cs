using System;
using Moq;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterCheckLessThanOrEqualTest : FilterCheckTest
	{
		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			AssertEquals("'valueA' <= 'valueA'", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			AssertEquals("'valueA' <= 'valueB'", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			AssertEquals("'valueB' <= valueA", false, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("N/A");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("n/a");
			AssertEquals("'N/A' <= 'n/a'", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("n/a");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("N/A");
			AssertEquals("'n/a' <= 'N/A'", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateDates()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var date = new DateTime(2017, 6, 23);
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("22-JUN-17 23:59");
			AssertEquals("23-JUN-17 <= 22-JUN-17 23:59 (should be false)", false, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23-JUN-17");
			AssertEquals("23-JUN-17 <= 23-JUN-17 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("24-JUN-17 17:32");
			AssertEquals("23-JUN-17 <= 24-JUN-17 17:32 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateInts()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var intValue = 3;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(intValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("2");
			AssertEquals("3 <= 2 (should be false)", false, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(intValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3");
			AssertEquals("3 <= 3 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(intValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.1");
			AssertEquals("3 <= 3.1 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateDoubles()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var doubleValue = 3.0;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("2.9");
			AssertEquals("3.0 <= 2.9 (should be false)", false, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3");
			AssertEquals("3.0 <= 3 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.1");
			AssertEquals("3.0 <= 3.0 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateDecimal()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(3.1m);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.0");
			AssertEquals("3.1 <= 3.0 (should be false)", false, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(3.1m);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.2");
			AssertEquals("3.0 <= 3.1 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(3.1m);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.1");
			AssertEquals("3.1 <= 3.1 (should be true)", true, new FilterCheckLessThanOrEqual(left.Object, right.Object).Evaluate(provider.Object));
		}

		#region Implementation
		protected override FilterCheck NewCheck(IFilterOperand lhs, IFilterOperand rhs)
		{
			return new FilterCheckEquality(lhs, rhs);
		}
		#endregion
	}
}
