using System;
using System.IO;
using System.Reflection;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DeletedClientDllTest : TestCase
	{
		public void TestDeletedClientDll()
		{
			var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var values = Enum.GetNames(typeof(DeletedClientDll));
			foreach (var clientDll in values)
			{
				var filePath = Path.Combine(rootPath, clientDll + ".dll");
				AssertEquals("Deleted " + clientDll + ".dll exists?", false, File.Exists(filePath));
			}
		}
	}
}
