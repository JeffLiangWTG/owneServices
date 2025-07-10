using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditCardFeeRegistryItem))]
	class CreditCardFeeRegistryItemTest : StronglyTypedRegistryItemTestCase<CreditCardFeeCollection>
	{
		protected override StronglyTypedRegistryItem<CreditCardFeeCollection, CreditCardFeeCollection> GetNewRegistryItem()
		{
			return new CreditCardFeeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		public void TestRegistryItemIsHiddenForFutureUse()
		{
			// This registry item was implemented for the Seven Seas project, which didn't go ahead.
			// This registry is being hidden in case that project is recommenced, or needs to be used in a later work item
			AssertEquals(RegistryOptions.IsHidden, GetNewRegistryItem().Options);
		}
	}
}
