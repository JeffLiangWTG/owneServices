using System;
using System.Reflection;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SubClassRetrieverTest : TestCase
	{
		public void TestDontIncludeClientDlls()
		{
			Assembly clientAssembly = Assembly.Load("ZClientEDI");
			Assembly resourcesAssembly = typeof(Constants).Assembly;
			SubClassRetriever retriever = new SubClassRetriever(new string[]
			{
				clientAssembly.GetName().Name,
				resourcesAssembly.GetName().Name,
			}, typeof(object));

			Type[] clientBusinessObjectTypes = retriever.Retrieve();
			bool hasClientDllTypes = ContainsTypeFromAssembly(clientBusinessObjectTypes, clientAssembly);
			bool hasResourcesTypes = ContainsTypeFromAssembly(clientBusinessObjectTypes, resourcesAssembly);
			AssertEquals("Should find all types by default", true, hasClientDllTypes);
			AssertEquals("Should find all types by default", true, hasResourcesTypes);

			retriever.IncludeClientDlls = false;
			Type[] clientBusinessObjectTypes_WithClientDllsExcluded = retriever.Retrieve();
			hasClientDllTypes = ContainsTypeFromAssembly(clientBusinessObjectTypes_WithClientDllsExcluded, clientAssembly);
			hasResourcesTypes = ContainsTypeFromAssembly(clientBusinessObjectTypes_WithClientDllsExcluded, resourcesAssembly);
			AssertEquals("Should not find types in client dll when excluded", false, hasClientDllTypes);
			AssertEquals("Should find types in other dlls even when client dlls excluded", true, hasResourcesTypes);
		}

		bool ContainsTypeFromAssembly(Type[] types, Assembly ass)
		{
			foreach (Type type in types)
			{
				if (type.Assembly == ass)
				{
					return true;
				}
			}
			return false;
		}
	}
}
