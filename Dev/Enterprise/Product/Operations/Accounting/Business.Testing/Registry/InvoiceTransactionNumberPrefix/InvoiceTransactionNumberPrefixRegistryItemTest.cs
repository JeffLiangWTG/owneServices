using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceTransactionNumberPrefixRegistryItem))]
	class InvoiceTransactionNumberPrefixRegistryItemTest : StronglyTypedRegistryItemTestCase<String>
	{
		public void TestProposedValueSetWhenRegistryOverriden()
		{
			IRegistryItemInternals registryItem = (InvoiceTransactionNumberPrefixRegistryItem)GetNewRegistryItem();
			registryItem.SetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, String.Empty);
			registryItem.SetCurrentValueToUse(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			AssertEquals("Proposed value", Env.CurrentCompany.Code, registryItem.GetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestProposedValueSetWhenRegistryOverridenWithNullCurrentValue()
		{
			IRegistryItemInternals registryItem = (InvoiceTransactionNumberPrefixRegistryItem)GetNewRegistryItem();
			registryItem.SetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, null);
			registryItem.SetCurrentValueToUse(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			AssertEquals("Proposed value", Env.CurrentCompany.Code, registryItem.GetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestExistingValueNotOverwrittenWhenRegistryOverriden()
		{
			IRegistryItemInternals registryItem = (InvoiceTransactionNumberPrefixRegistryItem)GetNewRegistryItem();
			registryItem.SetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "XYZ");
			registryItem.SetCurrentValueToUse(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			AssertEquals("Proposed value", "XYZ", registryItem.GetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<String, String> GetNewRegistryItem()
		{
			return new InvoiceTransactionNumberPrefixRegistryItem(String.Empty, null, null, null, RegistryStorageFlags.Company);
		}

		#endregion
	}
}
