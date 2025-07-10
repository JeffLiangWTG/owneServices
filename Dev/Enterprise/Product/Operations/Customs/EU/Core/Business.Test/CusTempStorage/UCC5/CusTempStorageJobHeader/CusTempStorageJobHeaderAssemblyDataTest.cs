using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderAssemblyDataTest : TestCase
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(CusTempStorageJobHeader), assemblyData.BusinessObjectType);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Temporary Storage", assemblyData.HumanReadableName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			assemblyData = new CusTempStorageJobHeaderAssemblyData();
		}
		CusTempStorageJobHeaderAssemblyData assemblyData;
	}
}
