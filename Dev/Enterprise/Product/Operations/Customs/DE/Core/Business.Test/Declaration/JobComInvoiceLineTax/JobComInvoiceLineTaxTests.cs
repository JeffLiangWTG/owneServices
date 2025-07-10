using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	public class JobComInvoiceLineTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Special Case", tax.HumanReadableName);
		}

		public void TestData()
		{
			AssertType<JobComInvoiceLineTax>(tax.Data);
			AssertType<JobComInvoiceLineTax>(tax);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return tax;
		}

		public void TestJLT_Rate()
		{
			AssertEquals(5, JobComInvoiceLineTax.Schema.JLT_RateDecimalPlaces);

			tax.JLT_Rate = 1;
			tax.JLT_MethodOfCalculation = SpecialCaseGroupList.Codes._07;
			AssertEquals(false, tax.JLT_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)1, tax.JLT_Rate);

			tax.JLT_Rate = 1;
			tax.JLT_MethodOfCalculation = SpecialCaseGroupList.Codes._08;
			AssertEquals(true, tax.JLT_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, tax.JLT_Rate);

			tax.JLT_Rate = 1;
			tax.JLT_MethodOfCalculation = SpecialCaseGroupList.Codes._09;
			AssertEquals(true, tax.JLT_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, tax.JLT_Rate);

			tax.JLT_Rate = 1;
			tax.JLT_MethodOfCalculation = SpecialCaseGroupList.Codes._10;
			AssertEquals(false, tax.JLT_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)1, tax.JLT_Rate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invHeader = dec.Invoices.AddNew();
			invLine = invHeader.InvoiceLines.AddNew();
			tax = invLine.Taxes.AddNew();
		}

		JobDeclaration dec;
		JobComInvoiceHeader invHeader;
		JobComInvoiceLine invLine;
		JobComInvoiceLineTax tax;
	}
}
