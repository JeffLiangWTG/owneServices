#if DEBUG
using System;
using System.ComponentModel;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class SafeMethodInvokerTests : TestCase
	{
		public void TestGetPropertiesIncludingPrivate()
		{
			PropertyDescriptorCollection privateProperties = KPropertyDescriptorCollection.FromType(typeof(TestComponent), true);
			AssertEquals("PublicProperty", "PublicProperty", privateProperties["PublicProperty"].Name);
			AssertEquals("PrivateProperty", "PrivateProperty", privateProperties["PrivateProperty"].Name);

			TestComponent c = new TestComponent();
			AssertEquals("PublicProperty", "PublicProperty", privateProperties["PublicProperty"].Name);
			AssertEquals("PublicProperty", typeof(KReflectPropertyDescriptor), ((KPropertyDescriptor)privateProperties["PublicProperty"]).Inner.GetType());
			AssertEquals("PrivateProperty", "PrivateProperty", privateProperties["PrivateProperty"].Name);
			AssertEquals("PrivateProperty", typeof(KReflectPropertyDescriptor), ((KPropertyDescriptor)privateProperties["PrivateProperty"]).Inner.GetType());

			AssertEquals("PrivateProperty", "private", privateProperties["PrivateProperty"].GetValue(c));
		}

		#region SafeCreateInstance / SafeMethodInvoke / SafeCreateDelegate

		public void TestSafeCreateInstance()
		{
			ConstructorInfo constructor = typeof(ObjectToCreateInPartialTrust).GetConstructor(new Type[] { typeof(string) });
			ObjectToCreateInPartialTrust obj = (ObjectToCreateInPartialTrust)SafeMethodInvoker.SafeCreateInstance(constructor, "ConstructorParameter");
			AssertEquals("Constructor is called with parameter", "ConstructorParameter", obj.ConstructorParameter);
		}

		public void TestSafeMethodInvoke()
		{
			ObjectToCreateInPartialTrust obj = new ObjectToCreateInPartialTrust("");
			SafeMethodInvoker.SafeMethodInvoke(ObjectToCreateInPartialTrust.MethodInfo, obj, null);
			AssertEquals("Method is called", true, obj.MethodCalled);
		}

		public void TestSafeCreateDelegate()
		{
			ObjectToCreateInPartialTrust obj = new ObjectToCreateInPartialTrust("");
			ObjectToCreateInPartialTrust.MethodDelegate d = (ObjectToCreateInPartialTrust.MethodDelegate)SafeMethodInvoker.SafeCreateDelegate(typeof(ObjectToCreateInPartialTrust.MethodDelegate), obj, ObjectToCreateInPartialTrust.MethodInfo, true);
			d();
			AssertEquals("Delegate constructed and invoked", true, obj.MethodCalled);
		}

		internal class ObjectToCreateInPartialTrust
		{
			public ObjectToCreateInPartialTrust(string constructorParameter)
			{ this.ConstructorParameter = constructorParameter; }

			public readonly string ConstructorParameter;
			public bool MethodCalled;

			public static MethodInfo MethodInfo
			{ get { return typeof(ObjectToCreateInPartialTrust).GetMethod("Method", BindingFlags.NonPublic | BindingFlags.Instance); } }

			public delegate void MethodDelegate();

			internal void Method()
			{ MethodCalled = true; }
		}

		#endregion

		#region CreateLightWeightMethodInvoker

		public void TestCreateLightWeightMethodInvoker()
		{
			StaticMethodDelegate staticDelegate = (StaticMethodDelegate)SafeMethodInvoker.CreateLightWeightMethodInvoker("StaticMethod", GetType().GetMethod("StaticMethod", BindingFlags.NonPublic | BindingFlags.Static), typeof(StaticMethodDelegate), typeof(int), new Type[] { typeof(int) });
			AssertEquals(1, staticDelegate(1));

			StaticMethodWithBoxAndUnboxDelegate staticWithBoxingDelegate = (StaticMethodWithBoxAndUnboxDelegate)SafeMethodInvoker.CreateLightWeightMethodInvoker("StaticMethodWithBoxAndUnbox", GetType().GetMethod("StaticMethodWithBoxAndUnbox", BindingFlags.NonPublic | BindingFlags.Static), typeof(StaticMethodWithBoxAndUnboxDelegate), typeof(object), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(Exception) });
			AssertEquals(10, staticWithBoxingDelegate(1, 2, 3, 4, new ApplicationException("No cast required")));

			InstanceMethodDelegate instanceDelegate = (InstanceMethodDelegate)SafeMethodInvoker.CreateLightWeightMethodInvoker("StaticMethod", GetType().GetMethod("InstanceMethod", BindingFlags.NonPublic | BindingFlags.Instance), typeof(InstanceMethodDelegate), typeof(int), new Type[] { typeof(SafeMethodInvokerTests), typeof(int) });
			AssertEquals(2, instanceDelegate(this, 2));
		}

		delegate int StaticMethodDelegate(int argument);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test scenario")]
		static int StaticMethod(int argument)
		{ return argument; }

		delegate object StaticMethodWithBoxAndUnboxDelegate(int a1, int a2, int a3, int a4, Exception a5);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test scenario")]
		static int StaticMethodWithBoxAndUnbox(int a1, int a2, int a3, int a4, ApplicationException a5)
		{
			AssertNotNull(a5);
			return a1 + a2 + a3 + a4;
		}

		delegate int InstanceMethodDelegate(SafeMethodInvokerTests instance, int argument);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test scenario")]
		int InstanceMethod(int argument)
		{ return argument; }

		#endregion

		#region Implementation

		internal class TestComponent
		{
			public string PublicProperty
			{ get { return "public"; } }

			protected string PrivateProperty
			{ get { return "private"; } }

			public static object Invoke(MethodBase method, object obj, object[] parameters)
			{ return method.Invoke(obj, parameters); }
		}

		#endregion
	}
}
#endif
