using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageRegHeaderAssemblyDataTest : TestCase
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader), assemblyData.BusinessObjectType);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		public void TestGetEDocsViaUniversalXmlSupport()
		{
			AssertType<CusTempStorageRegHeaderEDocsViaUniversalXmlSupport>(assemblyData.GetEDocsViaUniversalXmlSupport());
		}

		protected override void SetUp()
		{
			base.SetUp();
			assemblyData = new CusTempStorageRegHeaderAssemblyData();
		}
		CusTempStorageRegHeaderAssemblyData assemblyData;
	}
}
