using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBillDocManagerInfo))]
	sealed class AsycudaBillDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestDocManagerCodeIsValid()
		{
			Assert(AssemblyDataLookup.IsDocManagerCodeValid(GetDocManagerInfo(false).DocManagerCode));
		}

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();
	}
}
