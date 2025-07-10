using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(IntercompanyCostsApportionmentCollection))]
	public class IntercompanyCostsApportionmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IntercompanyCostsApportionmentCollection>
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testInvoice = new IntercompanyCostsApportionmentInvoice(Factory);
			testInvoiceLine = new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice);
		}

		IntercompanyCostsApportionmentInvoice testInvoice;
		IntercompanyCostsApportionmentInvoiceLine testInvoiceLine;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IntercompanyCostsApportionment(Factory, testInvoiceLine);
		}

		protected override IntercompanyCostsApportionmentCollection GetCollectionToTest()
		{
			return new IntercompanyCostsApportionmentCollection(Factory, testInvoiceLine);
		}

		#endregion

	}
}
