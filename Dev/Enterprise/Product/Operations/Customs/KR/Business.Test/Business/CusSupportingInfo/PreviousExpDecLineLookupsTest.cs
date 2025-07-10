using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PreviousExpDecLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitOfQuantityList()
		{
			var uqList = previousExpDecLine.Lookups.UnitOfQuantityList;
			AssertEquals(94, uqList.Count);
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			previousExpDecLine = invoiceLine.PreviousExpDecLineCollection.AddNew();
		}
		JobDeclaration declaration;
		PreviousExpDecLine previousExpDecLine;
	}
}
