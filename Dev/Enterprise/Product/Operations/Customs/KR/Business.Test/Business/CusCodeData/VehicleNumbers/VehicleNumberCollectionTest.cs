using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(VehicleNumberCollection))]
	sealed class VehicleNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllDeletedWhenInvoiceLineIsDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var vehicleNo = invoiceLine.VehicleNumbers.AddNew();
			invoiceLine.Delete();
			Assert(vehicleNo.IsDeleted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new VehicleNumberCollection(invoiceLine);
		}
	}
}
