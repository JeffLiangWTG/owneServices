using Enterprise.DocumentVisualizer.Presentation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ProxyBuilderTest : TestCase
	{
		#region TestProxyForInterfaceImplementingReadOnlyProperty

		public void TestProxyForInterfaceImplementingReadOnlyProperty()
		{
			var proxy = ProxyBuilder.Instance.CreateProxy<ITestA>();

			AssertNotNull("Proxy created", proxy);

			AssertEquals("Property value", 0, proxy.Property);
		}

		#endregion

		#region TestProxyForInterfaceImplementingReadWriteProperty

		public void TestProxyForInterfaceImplementingReadWriteProperty()
		{
			var proxy = ProxyBuilder.Instance.CreateProxy<ITestB>();

			AssertNotNull("Proxy created", proxy);

			AssertEquals("Property value", 0, proxy.Property);

			proxy.Property = 3;

			AssertEquals("Property value", 3, proxy.Property);
		}

		#endregion

		#region TestProxyForInterfaceImplementingReadWritePropertyHavingComplexType

		public void TestProxyForInterfaceImplementingReadWritePropertyHavingComplexType()
		{
			var proxy = ProxyBuilder.Instance.CreateProxy<ITestC>();

			AssertNotNull("Proxy created", proxy);

			AssertEquals("Property value", null, proxy.Property);

			var a = new A();
			proxy.Property = a;

			AssertEquals("Property value", a, proxy.Property);
		}

		#endregion

		#region TestProxyForInterfaceImplementingMethodNoParametersReturningVoid

		public void TestProxyForInterfaceImplementingMethodNoParametersReturningVoid()
		{
			var proxy = ProxyBuilder.Instance.CreateProxy<ITestD>();

			AssertNotNull("Proxy created", proxy);

			AssertNoExceptionThrown("Calling method", proxy.Method);
		}

		#endregion

		#region TestProxyForInterfaceImplementingMethodNoParametersRetuningValue

		public void TestProxyForInterfaceImplementingMethodNoParametersRetuningValue()
		{
			var proxy = ProxyBuilder.Instance.CreateProxy<ITestE>();

			AssertNotNull("Proxy created", proxy);

			AssertEquals("Calling method", 0, proxy.Method());
		}

		#endregion

		#region TestProxyForInterfaceImplementingMethodWithParameterReturningVoid

		public void TestProxyForInterfaceImplementingMethodWithParameterReturningVoid()
		{
			var proxy = ProxyBuilder.Instance.CreateProxy<ITestF>();

			AssertNotNull("Proxy created", proxy);

			AssertNoExceptionThrown("Calling method", () => proxy.Method(3));
		}

		#endregion

		#region TestProxyForInterfaceImplementingMethodWithParametersReturningValue

		public void TestProxyForInterfaceImplementingMethodWithParametersReturningValue()
		{
			var proxy = ProxyBuilder.Instance.CreateProxy<ITestG>();

			AssertNotNull("Proxy created", proxy);

			AssertEquals("Calling method", 0, proxy.Method(3, new A()));
		}

		#endregion
	}

	#region Types

	public interface ITestA
	{
		int Property { get; }
	}

	public interface ITestB
	{
		int Property { get; set; }
	}

	public interface ITestC
	{
		A Property { get; set; }
	}

	public class A
	{
	}

	public interface ITestD
	{
		void Method();
	}

	public interface ITestE
	{
		int Method();
	}

	public interface ITestF
	{
		void Method(int number);
	}

	public interface ITestG
	{
		int Method(int number, object o);
	}

	#endregion
}