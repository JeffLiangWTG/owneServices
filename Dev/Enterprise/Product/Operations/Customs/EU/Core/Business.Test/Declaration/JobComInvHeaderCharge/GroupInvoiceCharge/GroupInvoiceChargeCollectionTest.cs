using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceChargeCollection<GroupInvoiceCharge>))]
	public class GroupInvoiceChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<GroupInvoiceChargeCollection<GroupInvoiceCharge>, GroupInvoiceCharge>
	{
		protected override GroupInvoiceChargeCollection<GroupInvoiceCharge> GetCollectionToTest()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			return (GroupInvoiceChargeCollection<GroupInvoiceCharge>)testDec.JobComInvoiceGroupHeaders[0].Charges;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(GroupInvoiceCharge));
		}
	}
}
