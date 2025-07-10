using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CommercialInvoiceWrapperCollection))]
	sealed class CommercialInvoiceWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<CommercialInvoiceWrapperCollection>
	{
		public void TestConstructionFromShipment()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CommercialInvoiceWrapperCollection collection = new CommercialInvoiceWrapperCollection(shipment, Factory);
			AssertEquals("collection.Count", 0, collection.Count);

			declaration.JE_JS = shipment.PK;
			collection = new CommercialInvoiceWrapperCollection(shipment, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestConstructionFromJobDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			CommercialInvoiceWrapperCollection collection = new CommercialInvoiceWrapperCollection(declaration, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		#region Implementation
		protected override CommercialInvoiceWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CommercialInvoiceWrapperCollection((BaseJobDeclaration)null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CommercialInvoiceWrapper(null, Factory);
		}
		#endregion
	}
}
