using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Billing.Testing
{
	[TestedType(typeof(AccBillingItem))]
	public class AccBillingItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var item = Factory.New<AccBillingItem>();
			item.ABI_ParentId = new ZGuid();
			item.ABI_ParentTableCode = JobConsolSchema.Constants.Prefix;
			return item;
		}

		public void TestABH_ParentReferenceNumberMaxLength()
		{
			AssertEquals(JobShipmentSchema.JS_UniqueConsignRef.MaxLength, AccBillingItemSchema.ABI_ParentReferenceNumber.MaxLength);
		}
	}
}
