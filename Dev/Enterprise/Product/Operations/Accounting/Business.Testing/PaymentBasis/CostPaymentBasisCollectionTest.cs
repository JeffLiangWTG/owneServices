using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[TestedType(typeof(CostPaymentBasisCollection))]
	public class CostPaymentBasisCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var consolCost = Factory.NewWithValidTestData<JobConsolCost>();
			return new CostPaymentBasisCollection(consolCost);
		}
	}
}
