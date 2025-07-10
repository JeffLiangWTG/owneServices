using System.Linq;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageHeaderAssemblyDataTest : TestCase
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(TemporaryStorageHeader), assemblyData.BusinessObjectType);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Temporary Storage UCC6", assemblyData.HumanReadableName);
		}

		public void TestAssemblyDataProviderAttribute()
		{
			var assembly = typeof(TemporaryStorageHeaderAssemblyData).Assembly;
			var attributes = assembly.GetCustomAttributes(typeof(AssemblyDataProviderAttribute), false).Cast<AssemblyDataProviderAttribute>().ToArray();

			AssertNotNull(attributes);
			AssertCollectionContains("Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeaderAssemblyData", attributes, a => a.Type == typeof(TemporaryStorageHeaderAssemblyData));
			AssertCollectionContains("TS6", attributes, a => a.DocManagerCode == "TS6");
		}

		protected override void SetUp()
		{
			base.SetUp();
			assemblyData = new TemporaryStorageHeaderAssemblyData();
		}
		TemporaryStorageHeaderAssemblyData assemblyData;
	}
}
