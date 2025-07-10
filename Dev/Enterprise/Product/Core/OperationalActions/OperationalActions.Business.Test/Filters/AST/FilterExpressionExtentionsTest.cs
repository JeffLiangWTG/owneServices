using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business
{
	internal sealed class FilterExpressionExtentionsTest : TestCase
	{
		public void TestGetConstraints()
		{
			var expression = new Mock<IFilterExpression>(MockBehavior.Loose) { CallBase = true };
			expression.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("alpha"));
			var constraints = expression.Object.GetConstraints();
			AssertContainsExactElementsInAnyOrder(new[] { "alpha" }, constraints);
		}

		public void TestGetLongestCommonPrefix()
		{
			var expression = new Mock<IFilterExpression>();
			expression.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate());
			AssertEquals("", expression.Object.GetLongestCommonPrefix());

			expression.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("alpha"));
			AssertEquals("", expression.Object.GetLongestCommonPrefix());

			expression.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("alpha.beta"));
			AssertEquals("alpha", expression.Object.GetLongestCommonPrefix());

			expression.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("alpha.beta.gamma", "alpha.beta+delta", "alpha.beta.gamma.delta"));
			AssertEquals("alpha.beta", expression.Object.GetLongestCommonPrefix());

			expression.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>())).Callback(GetCollectDelegate("alpha", "alpha.beta"));
			AssertEquals("", expression.Object.GetLongestCommonPrefix());
		}

		public void TestGetLongestCommonPrefixOutofIndex()
		{
			var expression = new Mock<IFilterExpression>();
			expression.Setup(m => m.CollectConstraints(It.IsAny<IList<string>>()))
				.Callback(GetCollectDelegate(".", ".alpha"));
			AssertEquals("", expression.Object.GetLongestCommonPrefix());
		}

		#region Implementation
		static Action<IList<string>> GetCollectDelegate(params string[] constraints)
		{
			return delegate(IList<string> list)
			{
				foreach (var constraint in constraints)
				{
					list.Add(constraint);
				}
			};
		}
		#endregion
	}
}
