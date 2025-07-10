using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Dynamic.Test
{
	sealed class DlrProxyTest : TestCase
	{
		public void TestCreateLambda()
		{
			DlrProxy proxy = new DlrProxy();
			var lambda = proxy.CreateLambda<Func<object, object, object>>("a+b", "a", "b");
			AssertEquals(4, lambda(2, 2));
			AssertEquals("Test", lambda("Te", "st"));
		}

		public void TestHelpers()
		{
			DlrProxy proxy = new DlrProxy();
			var lambda1 = proxy.CreateLambda<Func<string, string, string>>("left(a, 2) + right(b, 2) + mid(a, 2) + mid(b, 1, 2)", "a", "b");
			AssertEquals("abDEcdeBC", lambda1("abcde", "ABCDE"));

			var lambda2 = proxy.CreateLambda<Func<object, object, object>>("iif(a > b, a, b)", "a", "b");
			AssertEquals(20, lambda2(10, 20));
			AssertEquals("B", lambda2("B", "A"));
		}

		public void TestCreateLambdaException()
		{
			AssertCreateLambdaException("unexpected EOF while parsing", "#?@", "a", "b");
			AssertCreateLambdaException("object has no attribute", "str.ErrorAttrNT", "a", "b");
			AssertCreateLambdaException("out of range", "a[20]", "a", "b");
		}

		void AssertCreateLambdaException(string expectPartofMessage, string expression, params string[] parameters)
		{
			DlrProxy proxy = new DlrProxy();
			try
			{
				var lambda = proxy.CreateLambda<Func<string, string, string>>(expression, parameters);
				lambda("A", "B");
				Assert("No errors thrown", false);
			}
			catch (Exception ex)
			{
				Assert($"This error is not recognized by DlrProxy.(type:{ex.GetType()})", proxy.IsDlrException(ex));
				Assert("The expected error message was not received.", ex.Message.Contains(expectPartofMessage));
			}
		}

		public void TestRunScript()
		{
			DlrProxy proxy = new DlrProxy();
			string funText = @"
def textFun(a, b):
	return a * b + 5
";
			proxy.RunScript(funText);
			var lambda = proxy.CreateLambda<Func<int, int, int>>("textFun(a, b)", "a", "b");
			AssertEquals(47, lambda(6, 7));
		}
	}
}
