using System.Reflection;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	sealed class GroupInvoiceChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseGroupInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestLookups()
		{
			var groupInvoiceCharge = Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].Charges.AddNew();
			AssertType<GroupInvoiceChargeLookups>(groupInvoiceCharge.Lookups);
		}

		public void TestFullDescription()
		{
			var chargeTypeProperty = typeof(GroupInvoiceCharge).GetProperty(nameof(GroupInvoiceCharge.J7_ChargeType));
			var fullDescription = chargeTypeProperty.GetCustomAttribute<ResourceStringDataAttribute>().FullDescription;
			AssertEquals(fullDescription, "Select the charge code type related to the commercial invoice to consider in valuation. Example: A CIF Invoice, requires Freight, and Insurance Charge codes to correctly value goods for customs purposes.");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].Charges.AddNew();
	}
}
