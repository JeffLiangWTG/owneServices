using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaManifestHeaderAssemblyDataTest : TransactionedTestCase
	{
		public void TestHumanReadableName()
		{
			var data = new AsycudaManifestHeaderAssemblyData();
			AssertEquals("Manifest", data.HumanReadableName.GetUnresolvedString());
		}

		public void TestOverrides()
		{
			var data = new AsycudaManifestHeaderAssemblyData();
			AssertEquals("BusinessObjectType", typeof(AsycudaManifestHeader), data.BusinessObjectType);
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.SupplyChainLogistics, data.ReferenceType);
		}

		public void TestGetEDocsViaUniversalXmlSupport()
		{
			AssertType<AsycudaManifestHeaderAssemblyDataEDocsViaUniversalXmlSupport>(new AsycudaManifestHeaderAssemblyData().GetEDocsViaUniversalXmlSupport());
		}
	}
}
