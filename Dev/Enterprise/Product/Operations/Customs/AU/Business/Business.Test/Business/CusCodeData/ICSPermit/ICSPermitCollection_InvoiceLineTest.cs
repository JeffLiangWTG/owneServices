using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ICSPermitCollection))]
	public class ICSPermitCollection_InvoiceLineTest : CusCodeDataCollectionTest<ICSPermit>
	{
		public void TestGetSortedPermitNumbers()
		{
			var permit1 = InvoiceLine.ICSPermits.AddNew();
			permit1.CY_Data = "PERT2";
			AssertEquals("PERT2", InvoiceLine.ICSPermits.GetSortedPermitNumbers());
			var permit2 = InvoiceLine.ICSPermits.AddNew();
			permit2.CY_Data = "PERT1";
			AssertEquals("PERT1, PERT2", InvoiceLine.ICSPermits.GetSortedPermitNumbers());
			var permit3 = InvoiceLine.ICSPermits.AddNew();
			permit3.CY_Data = "PERT3";
			AssertEquals("PERT1, PERT2, PERT3", InvoiceLine.ICSPermits.GetSortedPermitNumbers());
		}

		protected override CusCodeDataCollection<ICSPermit> GetCusCodeDataCollection()
		{
			return new ICSPermitCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var esmLineSequence = Factory.New<ICSPermit>();
			esmLineSequence.CY_ParentID = InvoiceLine.PK;
			esmLineSequence.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return esmLineSequence;
		}

		JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew());
		JobComInvoiceLine invoiceLine;
	}
}
