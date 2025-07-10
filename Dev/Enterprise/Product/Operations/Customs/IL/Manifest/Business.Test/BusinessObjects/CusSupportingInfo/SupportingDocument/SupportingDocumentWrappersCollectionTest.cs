using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocumentWrappersCollection))]
	sealed class SupportingDocumentWrappersCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SupportingDocumentWrappersCollection>
	{
		public void TestAllowNewCore() => AssertEquals("Adding new elements should not be allowed", false, Collection.AllowNew);

		public void TestCreateNonPersistentBusinessObject()
		{
			_ = AssertExceptionThrown<NotSupportedException>("Cannot AddNew", "SupportingDocumentWrapper cannot be created by users in grid.", () => Collection.AddNew());
		}

		protected override SupportingDocumentWrappersCollection GetCollectionToTest() => new SupportingDocumentWrappersCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new SupportingDocumentWrapper(bill.SupportingDocuments.AddNew());

		protected override void SetUp()
		{
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();

			base.SetUp();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
