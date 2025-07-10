using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSInvoiceLineViewCollection))]
	class EMCSInvoiceLineViewCollectionTest : InvoiceLineCollectionTest<EMCSInvoiceLineViewCollection, EMCSJobComInvoiceLine>
	{
		public void TestAllowNew()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var collection = declaration.FilteredInvoiceLines;

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();

			Assert("Should be false when the message status of declaration is SNT.", !collection.AllowNew);

			declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
			Factory.InvalidateCachedProperties();

			Assert("Should be false when the message status of declaration is ACK.", !collection.AllowNew);

			declaration.JE_MessageStatus = string.Empty;
			Factory.InvalidateCachedProperties();

			Assert("Should be true when the message status of declaration is not AND or ACK.", collection.AllowNew);
		}

		#region Implementation

		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<EMCSJobDeclaration>();
		}

		protected override EMCSInvoiceLineViewCollection GetCollectionToTest()
		{
			return new EMCSInvoiceLineViewCollection(DeclarationForBizOCollectionTest);
		}

		protected override BaseJobDeclaration GetDeclarationForBizOCollectionTest()
		{
			return Factory.New<EMCSJobDeclaration>();
		}

		new EMCSJobDeclaration DeclarationForBizOCollectionTest => (EMCSJobDeclaration)base.DeclarationForBizOCollectionTest;

		protected override BusinessObject GetNewElementToAddToTheCollection() => DeclarationForBizOCollectionTest.InvoiceHeader.InvoiceLines.AddNew();

		#endregion
	}
}
