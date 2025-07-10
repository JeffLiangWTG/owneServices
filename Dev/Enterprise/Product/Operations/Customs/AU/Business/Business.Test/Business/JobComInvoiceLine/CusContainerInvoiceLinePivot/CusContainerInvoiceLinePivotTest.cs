using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusContainerInvoiceLinePivot))]
	public class CusContainerInvoiceLinePivotTest : Customs.Business.Testing.CusContainerInvoiceLinePivotTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Factory);
		}

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = declaration.CusContainers.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			return invoiceLine.ContainersPivot[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject(factory);
		}

		public void TestIsDeclarationPersistent()
		{
			var container = Factory.New<CusContainer>();
			var pivot = (CusContainerInvoiceLinePivot)container.InvoiceLinePivotCollection.AddNew();
			AssertEquals(true, pivot.IsDeclarationPersistent);
			AssertEquals(true, pivot.IsSavedByFactory);

			var declaration = Factory.New<JobDeclaration>();
			container.CO_JE = declaration.PK;
			AssertEquals("IsDeclarationPersistent with persistent declaration", true, pivot.IsDeclarationPersistent);
			AssertEquals("IsSavedByFactory with persistent declaration", true, pivot.IsSavedByFactory);

			declaration.MakeNonPersistent();
			AssertEquals("IsDeclarationPersistent with non-persistent declaration", false, pivot.IsDeclarationPersistent);
			AssertEquals("IsSavedByFactory with non-persistent declaration", false, pivot.IsSavedByFactory);

			container.Delete();
			AssertEquals(true, pivot.IsDeclarationPersistent);
			AssertEquals(true, pivot.IsSavedByFactory);

			pivot.Delete();
			AssertEquals(true, pivot.IsDeclarationPersistent);
			AssertEquals(true, pivot.IsSavedByFactory);
		}
	}
}
