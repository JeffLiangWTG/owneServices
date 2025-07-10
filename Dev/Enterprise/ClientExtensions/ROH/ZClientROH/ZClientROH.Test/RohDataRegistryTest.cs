using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig
{
	[TestedType(typeof(RohDataRegistry))]
	class RohDataRegistryTest : RegistryItemSetTestCaseWithFactory<RohDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertVisible(ItemSet.HouseBillSignedByRaw);
			AssertVisible(ItemSet.CoreFreightEmailAddressRaw);
			AssertVisible(ItemSet.UseRohligSpecificInvoiceLayoutItem);
		}

		public void TestUseRohligSpecificInvoiceLayout()
		{
			ItemSet.UseRohligSpecificInvoiceLayout = true;
			Assert(ItemSet.UseRohligSpecificInvoiceLayout);
		}

		public void TestHouseBillsSignedBy()
		{
			ItemSet.HouseBillSignedBy = "blah blah";
			AssertEquals("blah blah", ItemSet.HouseBillSignedBy);
		}

		public void TestCoreFreightEmailAddress()
		{
			ItemSet.CoreFreightEmailAddress = "test@edi.com.au";
			AssertEquals("test@edi.com.au", ItemSet.CoreFreightEmailAddress);
		}
	}
}
