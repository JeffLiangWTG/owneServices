using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestTypedIndexer()
		{
			var collection = new InvoiceLineCompleteCollection(Declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override BaseJobDeclaration GetMeANewJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
