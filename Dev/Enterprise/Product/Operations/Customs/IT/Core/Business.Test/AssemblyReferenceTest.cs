using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AssemblyReferenceTest : TestCase
{
	public void TestNoNctsReferencesInCoreSolution()
	{
		CombineAssertions("In 'IT.Core' solution no 'NCTS' assembly references are expected. Please remove them and move the code in 'IT.NCTS' solution.", () =>
		{
			foreach (var assembly in LoadITCoreAssemblies().ToArray())
			{
				AssertNoNctsReferencesInAssembly(assembly);
			}
		});
	}

	IEnumerable<Assembly> LoadITCoreAssemblies()
	{
		var assemblyNameCollection = new string[]
		{
			"Enterprise.Customs.IT.Business",
			"Enterprise.Customs.IT.Business.Test",
			"Enterprise.Customs.IT.DataTransfer",
			"Enterprise.Customs.IT.DataTransfer.Test",
			"Enterprise.Customs.IT.GUI",
			"Enterprise.Customs.IT.GUI.Test",
			"Enterprise.Customs.IT.Messaging",
			"Enterprise.Customs.IT.Messaging.Test",
			"Enterprise.Customs.IT.Module",
			"Enterprise.Customs.IT.Module.Test",
			"Enterprise.Customs.IT.ServiceTasks",
			"Enterprise.Customs.IT.ServiceTasks.Test"
		};

		foreach (var assemblyName in assemblyNameCollection)
		{
			yield return Assembly.Load(assemblyName);
		}
	}

	void AssertNoNctsReferencesInAssembly(Assembly assemblyToInspect)
	{
		var referencedAssemblyes = assemblyToInspect.GetReferencedAssemblies().Cast<AssemblyName>();
		var nctsReferences = referencedAssemblyes.Where(x => x.Name.IndexOf("NCTS", StringComparison.OrdinalIgnoreCase) >= 0).Select(x => x.Name).ToArray();
		AssertEquals($"List of unexpected assembly references in '{assemblyToInspect.GetName().Name}'", "", string.Join(System.Environment.NewLine, nctsReferences));
	}
}
