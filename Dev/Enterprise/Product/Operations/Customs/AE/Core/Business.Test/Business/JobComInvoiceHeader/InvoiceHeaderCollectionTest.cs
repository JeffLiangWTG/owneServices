using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(InvoiceHeaderActiveCollection))]
public class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
{
}
