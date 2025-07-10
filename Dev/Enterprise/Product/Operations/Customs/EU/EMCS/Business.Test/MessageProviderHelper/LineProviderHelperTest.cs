using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class LineProviderHelperTest : TestCaseWithFactory
	{
		public void TestLineNumber()
		{
			var invoice = emcsInvoiceLine.Declaration.InvoiceHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition Line 1", emcsInvoiceLine.JI_LineNo, new ZShort(1));
				AssertEquals("Line 1", 1, helper.LineNumber);

				var invLine2 = invoice.InvoiceLines.AddNew();
				var lineProvider2 = new LineProviderHelper(invLine2);
				AssertEquals("Pre-condition Line 2", invLine2.JI_LineNo, new ZShort(2));
				AssertEquals("Line 2", 2, lineProvider2.LineNumber);
			});
		}

		public void TestExciseProductCode()
		{
			emcsInvoiceLine.ZG_ExciseProductCode = "E300";
			AssertEquals("E300", helper.ExciseProductCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			helper = new LineProviderHelper(emcsInvoiceLine);
		}
		EMCSJobDeclaration emcsDeclaration;
		EMCSJobComInvoiceLine emcsInvoiceLine;
		LineProviderHelper helper;
	}
}
