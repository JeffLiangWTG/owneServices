using System;
using System.Linq;
using System.Reflection;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Everybody
{
	sealed class ZModuleRegistrationTest : TestCase
	{
		public void TestModuleRegistration()
		{
			Type type = Type.GetType("Enterprise.ZArchitecture.Modules.Testing.ZModuleFactoryTest,Enterprise.ZArchitecture.GUI.Test");
			AssertNotNull("Test broken due to reorganisation of Enterprise.ZArchitecture.Business.dll name or namespaces.", type);

			MethodInfo methodInfo = type.GetMethod("TestAllModules");
			AssertNotNull("Test broken due rename of TestAllModules.", methodInfo);

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

		public void TestDummyRegistrationAvailable()
		{
			var dummyEnumValue = (Enum)ModuleId.Dummy;
			var moduleIdentifier = ModuleIDs.AllIncludingClientModules.SingleOrDefault(id => id.ID.Equals(dummyEnumValue));
			AssertNotNull(moduleIdentifier);
			var moduleInfo = new ModuleList().All.SingleOrDefault(info => info.ID == moduleIdentifier && string.IsNullOrEmpty(info.CountryCode));
			AssertNotNull(moduleInfo);
		}
	}
}
