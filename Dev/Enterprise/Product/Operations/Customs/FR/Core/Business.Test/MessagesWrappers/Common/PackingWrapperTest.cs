using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class PackingWrapperTest : TestCaseWithFactory
	{
		public void TestWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 9999.99;
			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 99;
			package1.CW_PackType = "AA";
			package1.CW_MarksAndNos = "AAAAAAAAAAAA";

			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 99;

			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine1.JI_CL = entryLine.PK;

			var wrapper = new PackingWrapper(entryLine);
			AssertEquals("AA", wrapper.Type);
			AssertEquals("AAAAAAAAAAAA", wrapper.MarksAndNos);
			AssertEquals(99, wrapper.Count);
			AssertEquals(0, wrapper.ItemsCount);

			package1.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
			package1.CW_MarksAndNos = "NNNNNNNN";
			var wrapper2 = new PackingWrapper(entryLine);
			AssertEquals("NE", wrapper2.Type);
			AssertEquals("NNNNNNNN", wrapper2.MarksAndNos);
			AssertEquals(0, wrapper2.Count);
			AssertEquals(9999, wrapper2.ItemsCount);
		}
	}
}
