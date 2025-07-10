using System;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.Common.Testing.Data
{
	class UpgradeManagerExceptionTest : TestCase
	{
		public void TestMessageDefault()
		{
			var result = new UpgradeManagerException().Message;
			AssertEquals("Upgrade manager exception.", result);
		}

		public void TestMessageForInnerException()
		{
			CombineAssertions(() =>
			{
				Test(new Exception());
				Test(new InvalidOperationException());
				Test(new AccessViolationException());
			});

			void Test(Exception exception)
			{
				var result = new UpgradeManagerException(exception).Message;
				AssertEquals("Upgrade manager exception.", result);
			}
		}

		public void TestInnerException()
		{
			CombineAssertions(() =>
			{
				Test(new Exception());
				Test(new InvalidOperationException());
				Test(new AccessViolationException());
			});

			void Test(Exception exception)
			{
				var result = new UpgradeManagerException(exception).InnerException;
				AssertEquals(exception, result);
			}
		}
	}
}
