using System;
using System.Text;
using Enterprise.Services.OperationalActions.Support;
using Moq;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterCheckEqualityTest : FilterCheckTest
	{
		public void TestEvaluateWhenLeftValueIsNull()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(null);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			AssertEquals(false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			AssertEquals("'valueA' == 'valueA'", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			AssertEquals("'valueA' == 'valueB'", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(null);
			AssertEquals("'valueA' == null", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(null);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueB");
			AssertEquals("null == 'valueB'", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("N/A");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("n/a");
			AssertEquals("'N/A' == 'n/a'", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("value?");
			AssertEquals("'valueA' == 'value?'", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("*A");
			AssertEquals("'valueA' == '*A'", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("valueA");
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("VALUEa");
			AssertEquals("'valueA' == 'VALUEa'", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateDates()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var date = new DateTime(2017, 6, 23);
			#region ShortDateFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23-JUN-17");
			AssertEquals("23-JUN-17 == 23-JUN-17 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23-JUN-2017");
			AssertEquals("23-JUN-17 == 23-JUN-2017 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("??-JUN-17");
			AssertEquals("23-JUN-17 == ??-JUN-17 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("??-Jun-17");
			AssertEquals("23-JUN-17 == ??-Jun-17 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			#endregion
			#region ISO8601ShortDateFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("2017-06-23");
			AssertEquals("23-JUN-17 == 2017-06-23 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date.AddDays(8));
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("2017-7-1");
			AssertEquals("01-JUL-17 == 2017-7-1 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("????-06-23");
			AssertEquals("23-JUN-17 == ????-06-23 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			#endregion
			#region LongTimeFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23-JUN-17 00:00");
			AssertEquals("23-JUN-17 == 23-JUN-17 00:00 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23-JUN-17 00:01");
			AssertEquals("23-JUN-17 == 23-JUN-17 00:01 (should be false)", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("??-JUN-17 00:00");
			AssertEquals("23-JUN-17 == ??-JUN-17 00:00 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			#endregion
			#region BestReadableDateFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23 JUN 2017");
			AssertEquals("23-JUN-17 == 23 JUN 2017 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23 ??? 2017");
			AssertEquals("23-JUN-17 == 23 ??? 2017 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			#endregion
			#region BestReadableDateTimeFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23 JUN 2017 00:00");
			AssertEquals("23-JUN-17 == 23 JUN 2017 00:00 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23 JUN 2017 00:01");
			AssertEquals("23-JUN-17 == 23 JUN 2017 00:01 (should be false)", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(date);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23 JUN 201? 00:00");
			AssertEquals("23-JUN-17 == 23 JUN 201? 00:00 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			#endregion
		}

		public void TestEvaluateDateTime()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var time = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 7, 52, 0);
			var dateTime = new DateTime(2017, 6, 23, 7, 52, 0);
			#region LongTimeFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(dateTime);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23-JUN-17 07:52");
			AssertEquals("23-JUN-17 07:52 == 23-JUN-17 07:52 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));

			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(dateTime);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23-JUN-17 00:01");
			AssertEquals("23-JUN-17 07:52 == 23-JUN-17 00:01 (should be false)", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));

			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(dateTime);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("??-Jun-17 07:52");

			AssertEquals("23-JUN-17 07:52 == ??-Jun-17 07:52 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			#endregion
			#region ShortTimeFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(time);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("07:52");
			AssertEquals("07:52 == 07:52 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(time);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("07:??");
			AssertEquals("07:52 == 07:?? (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(time);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("7:52");
			AssertEquals("07:52 == 7:52 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
		#endregion
		#region BestReadableDateTimeFormat
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(dateTime);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23 JUN 2017 07:52");
			AssertEquals("23-JUN-17 07:52 == 23 JUN 2017 07:52 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(dateTime);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("23 JUN 201? 07:5?");
			AssertEquals("23-JUN-17 07:52 == 23 JUN 201? 07:5? (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			#endregion
		}

		public void TestEvaluateInts()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var intValue = 3;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(intValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.0");
			AssertEquals("3 == 3.0 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(intValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3");
			AssertEquals("3 == 3 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(intValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.1");
			AssertEquals("3 == 3.1 (should be false)", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));

			intValue = 30;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(intValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3?");
			AssertEquals("30 == 3? (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateDoubles()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var doubleValue = 3.0;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.0");
			AssertEquals("3.0 == 3.0 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3");
			AssertEquals("3.0 == 3 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.1");
			AssertEquals("3.0 == 3.1 (should be false)", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));

			doubleValue = 3.7;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.?");
			AssertEquals("3.7 == 3.? (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(doubleValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("?.7");
			AssertEquals("3.7 == ?.7 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateDecimal()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var decValue = 3.0m;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(decValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.0");
			AssertEquals("3.0 == 3.0 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(decValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3");
			AssertEquals("3.0 == 3 (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(decValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.1");
			AssertEquals("3.0 == 3.1 (should be false)", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));

			decValue = 3.7m;
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(decValue);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("3.?");
			AssertEquals("3.7 == 3.? (should be true)", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestEvaluateBools()
		{
			var provider = new Mock<IFilterValueProvider>();
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(true);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("true");
			AssertEquals("true = true", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(true);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("True");
			AssertEquals("true = True", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(true);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("TRUE");
			AssertEquals("true = TRUE", true, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
			left.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns(true);
			right.Setup(m => m.Evaluate(provider.Object, It.IsAny<bool>())).Returns("false");
			AssertEquals("true = false", false, new FilterCheckEquality(left.Object, right.Object).Evaluate(provider.Object));
		}

		public void TestIsRequirementEnforced()
		{
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var requirement = new FilterRequirement("Blat");
			var aB = new FilterRequirement("Blat");
			aB.Add("A");
			aB.Add("B");
			var b = new FilterRequirement("Blat") { "B" };
			var bC = new FilterRequirement("Blat") { "B", "C" };
			left.Setup(m => m.GetValuesForRequirement(requirement)).Returns(aB);
			right.Setup(m => m.GetValuesForRequirement(requirement)).Returns(b);
			AssertEquals(true, new FilterCheckEquality(left.Object, right.Object).IsRequirementEnforced(requirement));
			left.Setup(m => m.GetValuesForRequirement(requirement)).Returns(aB);
			right.Setup(m => m.GetValuesForRequirement(requirement)).Returns(bC);
			AssertEquals(false, new FilterCheckEquality(left.Object, right.Object).IsRequirementEnforced(requirement));
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
			var left = new Mock<IFilterOperand>();
			var right = new Mock<IFilterOperand>();
			var builder = new StringBuilder();
			left.Setup(m => m.Append(builder)).Callback(leftDelegate);
			right.Setup(m => m.Append(builder)).Callback(rightDelegate);

			new FilterCheckEquality(left.Object, right.Object).Append(builder);
			AssertEquals("<left> == <right>", builder.ToString());
		}

		#region Implementation
		protected override FilterCheck NewCheck(IFilterOperand lhs, IFilterOperand rhs)
		{
			return new FilterCheckEquality(lhs, rhs);
		}
		#endregion
	}
}
