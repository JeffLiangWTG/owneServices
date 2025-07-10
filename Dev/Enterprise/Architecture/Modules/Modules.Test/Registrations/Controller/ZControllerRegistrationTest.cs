using System;
using System.Reflection;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Everybody
{
	sealed class ZControllerRegistrationTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAllControllersResolve()
		{
			foreach (var item in new ControllerList().All)
			{
				if (Type.GetType(item.TypePath, false) == null)
				{
					Fail("Could not resolve: " + item.TypePath + " for ID : " + item.ID);
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestControllerRegistration()
		{
			Type type = Type.GetType("Enterprise.ZArchitecture.Modules.Testing.ZControllerFactoryTest,Enterprise.ZArchitecture.GUI.Test");
			AssertNotNull("Test broken due to reorganisation of Enterprise.ZArchitecture.Business.dll name or namespaces.", type);

			MethodInfo methodInfo = type.GetMethod("TestAllControllers");
			AssertNotNull("Test broken due rename of TestAllControllers.", methodInfo);

			object instance = Activator.CreateInstance(type, null);
			AssertNotNull("Unable to create instance.", instance);

			try
			{
				methodInfo.Invoke(instance, null);
			}
			catch (Exception e)
			{
				throw e.InnerException;
			}
		}
	}
}
