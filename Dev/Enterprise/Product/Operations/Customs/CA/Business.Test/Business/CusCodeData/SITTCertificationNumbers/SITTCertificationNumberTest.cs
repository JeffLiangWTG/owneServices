using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(SITTCertificationNumber))]
	sealed class SITTCertificationNumberTest : Customs.Business.Testing.CusCodeDataTest<SITTCertificationNumber>
	{
		public void TestValidation()
		{
			var number = Factory.New<SITTCertificationNumber>();
			AssertEquals("Validation", typeof(SITTCertificationNumberValidation), number.Validation.GetType());
		}

		public void TestLookups()
		{
			var number = Factory.New<SITTCertificationNumber>();
			AssertEquals("Lookups", typeof(CusCodeDataLookups), number.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			var number = Factory.New<SITTCertificationNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.SITTNumber, number.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.SITTNumber, number.CY_Code);
		}

		public void TestParent()
		{
			var number1 = Factory.New<CFIARegistrationNumber>();
			number1.CY_ParentID = invoiceLine.PK;
			number1.CY_ParentTableCode = invoiceLine.TablePrefix;
			AssertEquals(invoiceLine, number1.Parent);
			var pivot = Factory.New<CusClassPartPivot>();
			var number2 = pivot.SITTCertificationNumbers.AddNew();
			AssertEquals(pivot.TablePrefix, number2.CY_ParentTableCode);
			AssertEquals(pivot.PK, number2.CY_ParentID);
			AssertEquals(pivot, number2.Parent);
		}

		protected override IEnumerable<SITTCertificationNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.SITTCertificationNumbers.AddNew();

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			yield return pivot.SITTCertificationNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.SITTCertificationNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return invoiceLine.SITTCertificationNumbers.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		JobComInvoiceLine invoiceLine;
	}
}
