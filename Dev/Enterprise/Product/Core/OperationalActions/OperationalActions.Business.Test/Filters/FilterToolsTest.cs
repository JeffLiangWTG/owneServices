using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Enterprise.Services.OperationalActions.Business.AST;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FilterToolsTest : TestCase
	{
		public void TestAddRequirement_Empty()
		{
			var expression = new Mock<IFilterExpression>();
			var requirement = new FilterRequirement("constraint") { "A" };
			var builder = new StringBuilder();
			expression.Setup(m => m.IsEmpty).Returns(true);
			var combined = FilterTools.AddRequirement(expression.Object, requirement);
			combined.Append(builder);
			AssertEquals(@"constraint == ""A""", builder.ToString());
			requirement.Add("B");
			builder.Length = 0;
			expression.Setup(m => m.IsEmpty).Returns(true);
			combined = FilterTools.AddRequirement(expression.Object, requirement);
			combined.Append(builder);
			AssertEquals(@"constraint in (""A"", ""B"")", builder.ToString());
		}

		public void TestAddRequirement_NotEmpty()
		{
			Action<StringBuilder> expressionAction = delegate(StringBuilder b)
			{
				b.Append("<Expression>");
			};

			var expression = new Mock<IFilterExpression>();
			var requirement = new FilterRequirement("constraint") { "A" };
			var builder = new StringBuilder();
			expression.Setup(m => m.IsEmpty).Returns(false);
			expression.Setup(m => m.Precidence).Returns(FilterPrecedence.Comparison);

			expression.Setup(m => m.Append(builder)).Callback(expressionAction);

			var combined = FilterTools.AddRequirement(expression.Object, requirement);
			combined.Append(builder);
			AssertEquals(@"constraint == ""A"" && <Expression>", builder.ToString());
			requirement.Add("B");
			builder.Length = 0;
			expression.Setup(m => m.IsEmpty).Returns(false);
			expression.Setup(m => m.Precidence).Returns(FilterPrecedence.Comparison);

			expression.Setup(m => m.Append(builder)).Callback(expressionAction);

			combined = FilterTools.AddRequirement(expression.Object, requirement);
			combined.Append(builder);

			AssertEquals(@"constraint in (""A"", ""B"") && <Expression>", builder.ToString());
		}

		public void TestSplitByInfoChain_Empty()
		{
			PropertyInfo[] infoChain = ReflectionHelper.FieldTextToPath(typeof(DummyBusinessObjectWithDocumentSupport), "AnotherFilteredCollection.Other.Z0_Number");
			AssertNotNull("precondition:", infoChain);

			var expression = new Mock<IFilterExpression>();
			IFilterExpression[] result;
			expression.Setup(m => m.IsEmpty).Returns(true);
			result = FilterTools.SplitByInfoChain(expression.Object, infoChain);

			AssertEquals(3, result.Length);
			AssertType(typeof(FilterEmpty), result[0]);
			AssertType(typeof(FilterEmpty), result[1]);
			AssertType(typeof(FilterEmpty), result[2]);
		}

		public void TestSplitByInfoChain_NotEmpty()
		{
			PropertyInfo[] infoChain = ReflectionHelper.FieldTextToPath(typeof(DummyBusinessObjectWithDocumentSupport), "AnotherFilteredCollection.Other.Z0_Number");
			AssertNotNull("precondition:", infoChain);
			var root = new Mock<IFilterExpression>();
			var and1 = new Mock<IFilterExpression>();
			var and2 = new Mock<IFilterExpression>();
			var sub1 = new Mock<IFilterExpression>();
			var sub2 = new Mock<IFilterExpression>();
			var sub3 = new Mock<IFilterExpression>();
			var sub4 = new Mock<IFilterExpression>();
			var newSub2 = new Mock<IFilterExpression>();
			var newSub3 = new Mock<IFilterExpression>();
			IFilterExpression[] resultExpressions;
			string[] resultStrings;
			root.Setup(m => m.IsEmpty).Returns(false);
			root.Setup(m => m.Precidence).Returns(FilterPrecedence.And);
			root.Setup(m => m.GetSubExpressions()).Returns(new[] { and1.Object, and2.Object });
			and1.Setup(m => m.Precidence).Returns(FilterPrecedence.And);
			and1.Setup(m => m.GetSubExpressions()).Returns(new[] { sub1.Object, sub2.Object });
			and2.Setup(m => m.Precidence).Returns(FilterPrecedence.And);
			and2.Setup(m => m.GetSubExpressions()).Returns(new[] { sub3.Object, sub4.Object });
			sub1.Setup(m => m.Precidence).Returns(FilterPrecedence.Comparison);
			sub2.Setup(m => m.Precidence).Returns(FilterPrecedence.Or);
			sub2.Setup(m => m.RemoveConstraintPrefix("AnotherFilteredCollection")).Returns(newSub2.Object);
			sub3.Setup(m => m.Precidence).Returns(FilterPrecedence.Comparison);
			sub3.Setup(m => m.RemoveConstraintPrefix("AnotherFilteredCollection.Other")).Returns(newSub3.Object);
			sub4.Setup(m => m.Precidence).Returns(FilterPrecedence.Or);

			sub1.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate(""));
			sub2.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("AnotherFilteredCollection.Z0_Code", "AnotherFilteredCollection.Other.Z0_Guid"));
			sub3.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("AnotherFilteredCollection.Other.Collection.Z0_VarCharMax"));
			sub4.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("Z0_Code"));

			resultExpressions = FilterTools.SplitByInfoChain(root.Object, infoChain);

			sub1.Setup(m => m.Precidence).Returns(FilterPrecedence.Comparison);
			sub4.Setup(m => m.Precidence).Returns(FilterPrecedence.Or);

			sub1.Setup(m => m.Append(It.IsAny<StringBuilder>())).Callback(GetAppendDelegate("<sub1>"));
			newSub2.Setup(m => m.Append(It.IsAny<StringBuilder>())).Callback(GetAppendDelegate("<newSub2>"));
			newSub3.Setup(m => m.Append(It.IsAny<StringBuilder>())).Callback(GetAppendDelegate("<newSub3>"));
			sub4.Setup(m => m.Append(It.IsAny<StringBuilder>())).Callback(GetAppendDelegate("<sub4>"));

			resultStrings = Array.ConvertAll(resultExpressions, (e) =>
			{
				var builder = new StringBuilder();
				e.Append(builder);
				return builder.ToString();
			});

			AssertEquals(3, resultStrings.Length);
			AssertEquals("<sub1> && (<sub4>)", resultStrings[0]);
			AssertEquals("<newSub2>", resultStrings[1]);
			AssertEquals("<newSub3>", resultStrings[2]);
		}

		#region Implementation
		static Action<IList<string>> GetCollectDelegate(params string[] constraints)
		{
			return delegate(IList<string> list)
			{
				((List<string>)list).AddRange(constraints);
			};
		}

		static Action<StringBuilder> GetAppendDelegate(string text)
		{
			return delegate(StringBuilder builder)
			{
				builder.Append(text);
			};
		}
		#endregion
	}
}
