using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
	}

	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	class HouseBillLevelInvoiceCollectionTest : Customs.Business.Testing.HouseBillLevelInvoiceCollectionTest
	{
	}

	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	class GroupInvoiceDirectChildInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseGroupInvoiceDirectChildInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection>
	{
	}

	class JobComInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCollectionTest
	{
	}
}
