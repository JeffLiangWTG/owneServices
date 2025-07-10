using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryConstantsTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestHostingSupport()
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var hostingSupport = factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "SUP"));
				if (hostingSupport == null)
				{
					hostingSupport = factory.New<IGlbGroup>();
					hostingSupport.GG_Code = "SUP";
					factory.Save();
				}

				EnvProxy.SetHostedLocationForTest("ABC");
				RegistryItemDictionary.Instance.PurgeAll();
				var value = RegistryConstants.GroupPKs.HostingSupportSingleton.Instance.GetHostingSupport();
				AssertEquals("HostingSupport should be initialized", hostingSupport.PK, value);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(null);
			}
		}
	}
}
