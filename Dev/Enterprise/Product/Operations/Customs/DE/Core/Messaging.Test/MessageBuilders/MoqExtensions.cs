using System;
using System.Linq.Expressions;

namespace Moq
{
	internal static class MoqExtensions
	{
		public static Mock<T> Update<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression, TResult arg) where T : class
			=> mock.Setup(expression, arg);

		public static Mock<T> Setup<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression, TResult arg) where T : class
		{
			mock.Setup(expression).Returns(arg);
			return mock;
		}
	}
}
