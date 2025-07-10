using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class IDutyAndTaxDataExtensionsTest : TestCaseWithFactory
	{
		public void TestGetClassHeader()
		{
			AssertEquals(classHeader, invoiceLine.GetClassHeader());
		}

		public void TestGetTaxRefNumHeader()
		{
			AssertEquals(refNumHeader, invoiceLine.GetTaxRefNumHeader());

			classHeader.ZA_AreaCode = "XXX";
			AssertEquals(legacyRefNumHeader, invoiceLine.GetTaxRefNumHeader());
		}

		public void TestIsCigars()
		{
			Assert(!invoiceLine.IsCigars());
			refNum.ZE_ExciseTaxRefNumber = "E01";
			Assert(invoiceLine.IsCigars());
		}

		public void TestIsSimaAmountPayable()
		{
			Assert(IDutyAndTaxDataExtensions.IsSimaAmountPayable("321"));
			Assert(!IDutyAndTaxDataExtensions.IsSimaAmountPayable("320"));
		}

		JobComInvoiceLine invoiceLine;
		CACTaxRefNumber refNum;
		CACClassHeader classHeader;
		CACTaxRefNumHeader refNumHeader;
		CACTaxRefNumHeader legacyRefNumHeader;

		protected override void SetUp()
		{
			base.SetUp();
			classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1234567890";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);
			refNum = refNumHeader.RefNumbers.AddNew();
			refNum.ZE_ExciseTaxRefNumber = "XXX";

			var legacyClassHeader = Factory.New<CACClassHeader>();
			legacyClassHeader.ZA_ClassificationNumber = "1234567890";
			legacyClassHeader.ZA_EffectiveDate = new ZDateTime(2023, 10, 03);
			legacyClassHeader.ZA_ExpiryDate = new ZDateTime(2024, 10, 03, 23, 59, 00);

			legacyRefNumHeader = Factory.New<CACTaxRefNumHeader>();
			legacyRefNumHeader.ZD_ZA_ClassNumber = legacyClassHeader.PK;
			legacyRefNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			legacyRefNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);

			invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			invoiceLine.JI_Tariff = classHeader.ZA_ClassificationNumber;
		}
	}
}
