using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CFIARegistrationNumber))]
	sealed class CFIARegistrationNumberTest : Customs.Business.Testing.CusCodeDataTest<CFIARegistrationNumber>
	{
		public void TestValidation()
		{
			var number = Factory.New<CFIARegistrationNumber>();
			AssertEquals("Validation", typeof(CFIARegistrationNumberValidation), number.Validation.GetType());
		}

		public void TestLookups()
		{
			CFIARegistrationNumber regNo = Factory.New<CFIARegistrationNumber>();
			AssertEquals("Validation", typeof(CFIARegistrationNumberLookups), regNo.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			var number = Factory.New<CFIARegistrationNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.CFIANumber, number.CY_Type);
		}

		public void TestParents()
		{
			var number1 = Factory.New<CFIARegistrationNumber>();
			number1.CY_ParentID = InvoiceLine.PK;
			number1.CY_ParentTableCode = InvoiceLine.TablePrefix;
			AssertEquals(InvoiceLine, number1.Parent);
			var pivot = Factory.New<CusClassPartPivot>();
			var number2 = pivot.CFIARegistrationNumbers.AddNew();
			AssertEquals(pivot.TablePrefix, number2.CY_ParentTableCode);
			AssertEquals(pivot.PK, number2.CY_ParentID);
			AssertEquals(pivot, number2.Parent);
		}

		public void TestDefaultCY_CodeWhenSafeFoodForCanadiansLicence()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMPORTER";
			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			var license = orgImpAddInfo.SafeFoodLicenses.AddNew();
			license.CY_Code = "SFC";
			license.CY_Data = "SafeFoodLicense";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var number = invoiceLine.CFIARegistrationNumbers.AddNew();
			AssertEquals("", number.CY_Data);

			number.CY_Code = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("SFC", number.CY_Data);

			number.CY_Data = "TTT";
			number.CY_Code = "";
			number.CY_Code = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("TTT", number.CY_Data);
		}

		protected override IEnumerable<CFIARegistrationNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.CFIARegistrationNumbers.AddNew();

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			yield return pivot.CFIARegistrationNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.CFIARegistrationNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine.CFIARegistrationNumbers.AddNew();
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
