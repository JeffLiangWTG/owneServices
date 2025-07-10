using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[NUnit.Framework.TestsSubclassesOf(typeof(BusinessObject.Loader))]
	public abstract class LoaderTestCase : TestCaseWithFactory
	{
		protected abstract BusinessObject.Loader GetNewLoaderToTest();

		public void TestConstruction()
		{
			AssertEquals("Constructed with correct factory", Factory, GetNewLoaderToTest().Factory);
		}

		public void TestAllPublicMethodsReturnSameTypeAsGetTypeOfBusinessObjectToLoad()
		{
			ZStringBuilder errorList = new ZStringBuilder();
			BusinessObject.Loader loader = GetNewLoaderToTest();

			Dictionary<string, MethodInfo> publicLoadMethodsDefinedInLoader = GetPublicLoadMethodsHashedByName(loader.GetType(), false);
			Dictionary<string, MethodInfo> publicLoadMethodsIncludingInheritedMethods = GetPublicLoadMethodsHashedByName(loader.GetType(), true);

			foreach (string methodName in publicLoadMethodsIncludingInheritedMethods.Keys)
			{
				Type returnTypeOfLoadMethod = publicLoadMethodsIncludingInheritedMethods[methodName].ReturnType;

				if (returnTypeOfLoadMethod.IsSubclassOf(typeof(BusinessObject)) && // TODO: Support checking arrays of business objects
					returnTypeOfLoadMethod != loader.GetTypeOfBusinessObjectToLoad() &&
					(publicLoadMethodsDefinedInLoader[methodName] == null ||
					publicLoadMethodsDefinedInLoader[methodName].ReturnType != loader.GetTypeOfBusinessObjectToLoad()))
				{
					MethodInfo methodInfo = publicLoadMethodsIncludingInheritedMethods[methodName];
					errorList.Append("\r\n\r\n- Method '" + methodInfo.Name + "' returns '" + methodInfo.ReturnType.FullName + ". ");
					errorList.Append("\r\nIt should have a public new-ed method which returns '" + loader.GetTypeOfBusinessObjectToLoad().FullName + "'");
				}
			}

			Assert("\r\n\r\n** Missing Public New Methods ***" + errorList.ToString() + "\r\n\r\n", errorList.IsEmpty);
		}

		Dictionary<string, MethodInfo> GetPublicLoadMethodsHashedByName(Type type, bool includeInheritedMethods)
		{
			Dictionary<string, MethodInfo> result = new Dictionary<string, MethodInfo>();

			foreach (MethodInfo methodInfo in type.GetMethods())
			{
				if (methodInfo.Name.IndexOf("Load") != -1 &&
					(includeInheritedMethods || (!includeInheritedMethods && methodInfo.DeclaringType == type)))
				{
					result[methodInfo.Name] = methodInfo;
				}
			}

			return result;
		}
	}
}
