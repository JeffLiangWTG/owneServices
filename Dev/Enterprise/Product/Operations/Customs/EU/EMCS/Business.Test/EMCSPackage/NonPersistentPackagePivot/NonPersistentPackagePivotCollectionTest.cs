using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(NonPersistentPackagePivotCollection))]
	class NonPersistentPackagePivotCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentPackagePivotCollection>
	{
		public void TestConstructorPopulatesCollection()
		{
			declaration.EMCSPackages.AddNew();
			var newInvoiceLine = invoice.InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Packages Count same as Pivots Count", 2, newInvoiceLine.EMCSPackagePivots.Count);
				AssertEquals("Has Changes", false, newInvoiceLine.EMCSPackagePivots.HasChanges);
			});
		}

		public void TestAdditionOfPackageOnDeclarationAdds()
		{
			var packagesPivots = invoiceLine.EMCSPackagePivots;
			CombineAssertions(() =>
			{
				AssertEquals("Packages Count same as Pivots Count", 1, invoiceLine.EMCSPackagePivots.Count);
				var newPackage = declaration.EMCSPackages.AddNew();
				AssertEquals("Pivots increased when Dec has new Package Added", 2, invoiceLine.EMCSPackagePivots.Count);
			});
		}

		public void TestRemovalOfPackageOnDeclarationRemoves()
		{
			var newPackage = declaration.EMCSPackages.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Packages Count same as Pivots Count", 2, invoiceLine.EMCSPackagePivots.Count);
				declaration.EMCSPackages.Remove(newPackage);
				AssertEquals("Pivots decreased when Dec has Package deleted", 1, invoiceLine.EMCSPackagePivots.Count);
			});
		}

		protected override NonPersistentPackagePivotCollection GetCollectionToTest() => new NonPersistentPackagePivotCollection(invoiceLine);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new NonPersistentPackagePivot(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			declaration.EMCSPackages.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine invoiceLine;
		EMCSJobComInvoiceHeader invoice;
	}
}
