using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
	}

	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class HouseBillLevelInvoiceCollectionTest : Customs.Business.Testing.HouseBillLevelInvoiceCollectionTest
	{
	}

	sealed class JobComInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCollectionTest
	{
	}
}
