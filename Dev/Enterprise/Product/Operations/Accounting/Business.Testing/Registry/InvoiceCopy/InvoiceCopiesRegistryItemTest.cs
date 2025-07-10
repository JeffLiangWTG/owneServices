using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceCopiesRegistryItem))]
	class InvoiceCopiesRegistryItemTest : StronglyTypedRegistryItemTestCase<InvoiceCopyCollection>
	{
		protected override StronglyTypedRegistryItem<InvoiceCopyCollection, InvoiceCopyCollection> GetNewRegistryItem()
		{
			return new InvoiceCopiesRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceCopyCollection());
		}

		public void TestUpdateMissingInvoiceCopyOrder()
		{
			var collection = new InvoiceCopyCollection();
			var invoiceCopy1 = collection.AddNew();
			invoiceCopy1.Name = (NoResString)"Original";
			invoiceCopy1.IsOriginal = true;
			invoiceCopy1.DeliveryMethod = "ALL";
			invoiceCopy1.Order = 0;

			var invoiceCopy2 = collection.AddNew();
			invoiceCopy2.Name = (NoResString)"Copy 1";
			invoiceCopy2.DeliveryMethod = "ALL";
			invoiceCopy2.Order = 0;

			var invoiceCopy3 = collection.AddNew();
			invoiceCopy3.Name = (NoResString)"Copy 2";
			invoiceCopy3.DeliveryMethod = "ALL";
			invoiceCopy3.Order = 0;

			Assert("Existing legacy collection don't have order values", collection.Cast<InvoiceCopy>().All(x => x.Order == 0));

			var registryItem = GetNewRegistryItem();
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var collectionCopy = registryItem.Value;

			AssertEquals(1, collectionCopy.Cast<InvoiceCopy>().First(x => x.Name == "Original").Order);
			AssertEquals(2, collectionCopy.Cast<InvoiceCopy>().First(x => x.Name == "Copy 1").Order);
			AssertEquals(3, collectionCopy.Cast<InvoiceCopy>().First(x => x.Name == "Copy 2").Order);
		}
	}
}
