using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaTax))]
	sealed class AsycudaTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULUTEST01234";
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();
			AssertEquals(bill, asycudaTax.Bill);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();
			asycudaTax.AET_MethodOfCalculation = "%";
			return asycudaTax;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues() => new List<ZString>() { AsycudaTax.Schema.AET_ABL };
	}
}
