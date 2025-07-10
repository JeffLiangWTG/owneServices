using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AdditionalProcedureCode))]
	public class AdditionalProcedureCodeTest : Customs.Business.Testing.CusCodeDataTest<AdditionalProcedureCode>
	{
		public void TestParent()
		{
			var bill = Factory.New<AsycudaBill>();
			var additionalProcedueCode = bill.AdditionalProcedureCodes.AddNew();
			AssertType<AsycudaBill>(additionalProcedueCode.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<AdditionalProcedureCode>();
		}

		protected override IEnumerable<AdditionalProcedureCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<AdditionalProcedureCode>();

			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.AdditionalProcedureCodes.Add(result);

			yield return result;
		}
	}
}
