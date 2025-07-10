using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class InvoicingLineBaseTaxableTest : TestCaseWithFactory
	{
		public void TestITaxableTransactionLineValues()
		{
			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(invoice, 100M);
			var objForTest = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);
			var reversingFactory = new ReversingFactory();

			invoice.AH_OH = creator.AALSHI.PK;
			invoice.AH_RX_NKTransactionCurrency = creator.USD.Code;
			invoice.AH_ExchangeRate = 3M;
			line.AL_OSExTaxAmount = 108M;
			var taxDate = new ZDate(2020, 03, 03);
			line.AL_TaxDate = taxDate;
			line.AL_AC = creator.CC1.PK;
			line.AL_GB = creator.NonCurrentBranch.PK;

			var taxableLine = objForTest as ITaxableTransactionLine;
			AssertEquals(creator.USD.Code, taxableLine.Currency);
			AssertEquals(-108M, taxableLine.BaseOSAmount);
			AssertEquals(-36M, taxableLine.LocalAmount);
			AssertEquals(taxDate, taxableLine.TaxDate);
			AssertEquals(line.PK, taxableLine.PK);
			AssertEquals(line.Factory, taxableLine.Factory);
			AssertEquals(creator.NonCurrentBranch, taxableLine.Branch);
			AssertEquals(creator.CC1, taxableLine.ChargeCode);

			line.AL_TaxDate = ZDate.Empty;
			AssertEquals(ZDate.Today, taxableLine.TaxDate);

			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			var reversedTransaction = invoice.ReverseInvoice;
			var reversedLine = (InvoicingLineBase)reversedTransaction.Lines.First();
			AssertEquals(taxableLine.PK, reversedLine.CopiedFromPK);

			taxableLine = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(reversedLine);
			AssertEquals(reversedLine.CopiedFromPK, taxableLine.CopiedFromPK);
		}

		public void TestBranch()
		{
			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(invoice, 100M);
			var objForTest = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);

			var branch1 = creator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = creator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			line.AL_GB = branch1.PK;

			var taxableLine = objForTest as ITaxableTransactionLine;
			AssertNull("Precondition:", line.TaxBranch);
			AssertEquals(line.Branch, taxableLine.Branch);

			line.AL_GB_TaxBranch = branch2.PK;
			AssertNotNull("Precondition: ", line.TaxBranch);
			AssertEquals(line.TaxBranch, taxableLine.Branch);
		}

		public void TestSupplyType()
		{
			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(invoice, 100M);
			var taxableLine = (ITaxableTransactionLine)TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);

			var expectedSupplyType = "DDD";
			line.AL_SupplyType = expectedSupplyType;
			AssertEquals(expectedSupplyType, taxableLine.SupplyType);

			line.AL_SupplyType = ZString.Empty;
			AssertEquals(ZString.Empty, taxableLine.SupplyType);
		}

		protected override void SetUp()
		{
			creator = new TestObjectCreator(Factory);
			creator.CreateBranch("TST", GlbCompany.CurrentCompany);
		}

		TestObjectCreator creator;
	}
}
