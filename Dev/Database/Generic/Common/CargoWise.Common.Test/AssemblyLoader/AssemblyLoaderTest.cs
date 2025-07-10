using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class AssemblyLoaderTest : TestCase
	{
		public void TestLoadAssembly()
		{
			AssertEquals(typeof(AssemblyLoaderTest).Assembly, AssemblyLoader.LoadAssembly("CargoWise.Common.Testing"));
		}

		public void TestInstanceIsNotThreadStatic()
		{
			var originalInstance = AssemblyLoader.Instance;
			try
			{
				AssemblyLoader.Instance = new AnotherAssemblyLoader();
				IAssemblyLoader instanceFromOtherThread = null;
				var thread = new Thread(new ThreadStart(delegate
				{
					instanceFromOtherThread = AssemblyLoader.Instance;
				}));
				thread.Start();
				thread.Join();
				AssertType(typeof(AnotherAssemblyLoader), instanceFromOtherThread);
				Assert(ReferenceEquals(AssemblyLoader.Instance, instanceFromOtherThread));
			}
			finally
			{
				AssemblyLoader.Instance = originalInstance;
			}
		}

		class AnotherAssemblyLoader : IAssemblyLoader
		{
			public Assembly LoadAssembly(AssemblyName assemblyName)
			{
				throw new NotImplementedException();
			}

			public string GetBinPath()
			{
				throw new NotImplementedException();
			}

			public string GetParentBinPath()
			{
				throw new NotImplementedException();
			}
		}
	}
}
