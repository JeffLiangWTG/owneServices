using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(IntercompanyCostsApportionmentInvoiceLineCollection))]
	public class IntercompanyCostsApportionmentInvoiceLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IntercompanyCostsApportionmentInvoiceLineCollection>
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testInvoice = new IntercompanyCostsApportionmentInvoice(Factory);
		}

		IntercompanyCostsApportionmentInvoice testInvoice;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice);
		}

		protected override IntercompanyCostsApportionmentInvoiceLineCollection GetCollectionToTest()
		{
			return new IntercompanyCostsApportionmentInvoiceLineCollection(Factory, testInvoice);
		}

		#endregion

		public void TestShowGLAccountsForImportAction()
		{
			var collection = GetCollectionToTest();
			collection.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			var line = collection.AddNew();
			AssertNotNull(line.ShowGLAccountsForImportAction);
			AssertEquals(collection.ShowGLAccountsForImportAction, line.ShowGLAccountsForImportAction);
		}
	}
}
