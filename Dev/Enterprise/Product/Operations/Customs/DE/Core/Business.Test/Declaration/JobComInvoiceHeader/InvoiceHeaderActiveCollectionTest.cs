using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class InvoiceHeaderActiveCollectionTest : EU.Business.Declaration.Testing.InvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		public void TestGetDefaultSetterForInvoiceHeader()
		{
			var collection = new InvoiceHeaderActiveCollectionForTest(Factory.New<JobDeclaration>());
			AssertType<DefaultSetterForInvoiceHeader>(collection.GetDefaultSetterForInvoiceHeaderExposed(null, null));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			return invoice;
		}

		class InvoiceHeaderActiveCollectionForTest : InvoiceHeaderActiveCollection
		{
			public InvoiceHeaderActiveCollectionForTest(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeaderExposed(Customs.Business.BaseJobComInvoiceHeader newElement, Customs.Business.BaseJobDeclaration declaration)
			{
				return base.GetDefaultSetterForInvoiceHeader(newElement, declaration);
			}
		}
	}
}
