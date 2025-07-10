using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing
{
	sealed class CusTempStorageRegPremisesAssemblyDataTest : TestCase
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(CusTempStorageRegPremises), assemblyData.BusinessObjectType);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Temporary Storage Premises", assemblyData.HumanReadableName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			assemblyData = new CusTempStorageRegPremisesAssemblyData();
		}
		CusTempStorageRegPremisesAssemblyData assemblyData;
	}
}
