using System;
using CargoWise.BuildTools;
using CargoWise.Common;
using Enterprise.ReflectionTest.Utilities;
using Microsoft.Build.Locator;
using NUnit.Framework;

namespace Enterprise.ReflectionTest
{
	public abstract class ReflectionTestBase : TransactionedTestCase
	{
		protected override void MasterSetUp()
		{
			if (MSBuildLocator.CanRegister)
			{
				var instance = DevMSBuildLocator.GetMSBuildInstance();
				MSBuildLocator.RegisterInstance(instance);
			}

#if NETFRAMEWORK
			AssembliesContext.EnsureInstance(AssemblyLoader.GetBinPath(), AssembliesUnderTest.AllAssemblyPaths);
#else
			AssembliesContext.EnsureInstance(AssemblyLoader.GetParentBinPath(), AssembliesUnderTest.AllAssemblyPaths);
#endif
		}

		//Using reflection causes .Net to load the meta data for the reflected types. This is loaded outside the managed heap of the process
		//in the loader heap, and is kept for the duration of the process. As some of the reflection tests reflect on every type in
		//Enterprise, the amount of meta data loaded is large - several hundred MB. Using a new AppDomain for each test causes the
		//meta data to be allocated in the new AppDomain, which is then unloaded, reclaiming the memory.

		protected bool UseNewAppDomainForTests
		{
			get
			{
				return TestingState.IsRunningOnDAT;
			}
		}

		protected virtual ReflectionTestHelper GetHelper() => new ReflectionTestHelper();

		protected void InvokeTest(string testMethodName)
		{
			InvokeTest(testMethodName, Array.Empty<object>());
		}

		protected void InvokeTest(string testMethodName, object[] args)
		{
			using (((ITransactionalTestInternals)this).SetInReflectionTestTemporary())
			{
				var testHelper = GetHelper();
				ReflectionTestHelper.Assemblies = AssembliesUnderTest.AllAssemblies;
				testHelper.SetUpHelper();

				try
				{
					testHelper.GetType().GetMethod(testMethodName).Invoke(testHelper, args);
				}
				finally
				{
					testHelper.TearDownHelper();
				}
			}
		}
	}
}
