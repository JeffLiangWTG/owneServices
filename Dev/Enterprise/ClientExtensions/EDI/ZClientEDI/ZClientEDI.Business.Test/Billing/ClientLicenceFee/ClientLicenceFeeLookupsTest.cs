using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFeeTypes()
		{
			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();
			var expected = new CodeDescriptionPairList();
			expected.AddRange(EDIDataRegistry.Instance.LicenceFeeTypes.Value);
			var feeTypes = licenceFee.Lookups.FeeTypes;
			AssertEquals(expected.Count, feeTypes.Count);
			for (int i = 0; i < expected.Count; ++i)
			{
				AssertEquals("Code " + i.ToString(), expected[i].Code, feeTypes[i].Code);
				AssertEquals("Desc " + i.ToString(), expected[i].Description, feeTypes[i].Description);
			}
		}

		public void TestSystemCode()
		{
			ClientLicenceFee fee = Factory.New<ClientLicenceFee>();
			CodeDescriptionPairList expected = new CodeDescriptionPairList();
			expected.AddPair(BillingConstants.BillingSystem.ODM, "Show on Billing ODPL/STL Report/Invoice");
			expected.AddPair(BillingConstants.BillingSystem.Maintenance, "Show on Billing Maintenance Report/Invoice");
			AssertContainsExactElementsInAnyOrder(expected, fee.Lookups.SystemCodes);
		}
	}
}