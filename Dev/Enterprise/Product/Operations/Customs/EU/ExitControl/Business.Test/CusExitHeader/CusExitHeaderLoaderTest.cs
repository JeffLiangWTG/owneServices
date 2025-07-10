using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeader.Loader))]
	sealed class CusExitHeaderLoaderTest : LoaderTestCase
	{
		public void TestLoaderResults()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			header.CXH_ParentID = declaration.PK;
			header.CXH_ParentTableCode = "JE";
			var consignment = Factory.NewWithValidTestData<CusExitConsignment>();
			consignment.CXC_CXH_Header = header.PK;

			Factory.Save();
			var loader = new CusExitHeader.Loader(Factory);
			var headers = loader.Load(false, declaration.PK, "JE"); // TODO: Use JE_ClusterKey when CXH_ClusterKey has been updated to use parent clusterkey
			AssertEquals("One CusExitHeader returned", 1, headers.Length);
			AssertEquals("Returned object type", typeof(CusExitHeader), headers[0].GetType());
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusExitHeader.Loader(Factory);
		}
	}
}
