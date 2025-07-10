using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaTaxTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaTax), new AsycudaTaxTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaTax), new AsycudaTaxTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var asycudaTax = Factory.NewWithValidTestData<AsycudaTax>();
			var asycudaBill = Factory.NewWithValidTestData<AsycudaBill>();
			asycudaTax.AET_ABL = asycudaBill.PK;
			Factory.Save();
			AssertGetTypeForLoad<AsycudaTax>(asycudaTax);

			var asycudaPackedItem = Factory.NewWithValidTestData<AsycudaPackedItemForTesting>();
			asycudaTax.AET_ABL = ZGuid.Empty;
			asycudaTax.AET_API_AsycudaPackedItem = asycudaPackedItem.PK;
			Factory.Save();
			AssertGetTypeForLoad<AsycudaPackedItemTaxForTesting>(asycudaTax);
		}

		void AssertGetTypeForLoad<T>(AsycudaTax asycudaTax)
		{
			var row = (asycudaTax as INeedRow)?.Row;
			AssertEquals(typeof(T), new AsycudaTaxTypeDecider().GetTypeForLoad(row, Factory));
		}

		class AsycudaPackedItemForTesting : AsycudaPackedItem
		{
			public AsycudaPackedItemForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override Type GetAsycudaTaxTypeCore() => typeof(AsycudaPackedItemTaxForTesting);
		}

		class AsycudaPackedItemTaxForTesting : AsycudaTax
		{
			public AsycudaPackedItemTaxForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
