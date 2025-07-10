using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTaxRate))]
	sealed class DocTaxRateTest : DocumentWrapperTestCase
	{
		public void TestConstructor()
		{
			DocTaxRate docTaxRate = DocTaxRate.New(TaxRate, Factory);
			int hashCode = docTaxRate.GetHashCode();
			docTaxRate = DocTaxRate.New(TaxRate, Factory);

			AssertEquals("Hash code must be the same", hashCode, docTaxRate.GetHashCode());
		}

		public void TestIsRatedTax()
		{
			AssertEquals(true, DocTaxRate.IsRatedTax(AccTaxRate.Types.Rated));
			AssertEquals(false, DocTaxRate.IsRatedTax(AccTaxRate.Types.ServiceTax));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AssertEquals(true, DocTaxRate.IsRatedTax(AccTaxRate.Types.ServiceTax));
			}

			AssertEquals(false, DocTaxRate.IsRatedTax(AccTaxRate.Types.CapitalRated));
			AssertEquals(true, DocTaxRate.IsRatedTax(AccTaxRate.Types.IntegratedGST));
		}

		public void TestARInvoiceTaxMessages()
		{
			AccInvMsg message1 = Factory.NewWithValidTestData<AccInvMsg>();
			TaxRate.AT_A9_DefaultVatClass = message1.PK;

			DocTaxRate rateWrapper = (DocTaxRate)GetDocumentWrappers()[0];
			AssertEquals(1, rateWrapper.ARInvoiceTaxMessages.Count);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocTaxRate.New(TaxRate, Factory)
			};
		}

		AccTaxRate TaxRate;
		protected override void SetUp()
		{
			TaxRate = Factory.New<AccTaxRate>();
			base.SetUp();
		}
	}
}
