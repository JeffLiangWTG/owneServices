using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	public class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollection(Factory);
		}

		public void TestAddPivotWithAdditionalLineDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_Tariff = "1234567890";
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Tariff Code for new pivot", "1234567890", pivot.CI_TariffNum);
			invoiceLine.JI_CC = cusClass.PK;
			pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Class. Lookup for new pivot", cusClass.PK, pivot.CI_CC);
			invoiceLine.JI_NDescription = "CI_NDescription test";
			invoiceLine.ComplementaryDescription = "ComplementaryDescription test";
			invoiceLine.JI_InvoiceUQ = "PKG";
			invoiceLine.JI_CountryOfOrigin = "BR";
			pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("ComplementaryDescription for new pivot", "ComplementaryDescription test", pivot.ComplementaryDescription);
		}

		public void TestAddPivotWithCatalogInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "0121323122";

			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			invoiceLine.JI_CGC_Catalog = cusGoodsCatalog.PK;
			invoiceLine.JI_Tariff = "1234567890";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var pivot = collection.AddNew().PivotsForBinding[0];

			AssertEquals("Set Tariff Code for new pivot should be empty", ZString.Empty, pivot.CI_TariffNum);
			AssertEquals("Set Class. Lookup for new pivot should be empty", ZGuid.Empty, pivot.CI_CC);
			AssertEquals("Set Catalog for new pivot", invoiceLine.JI_CGC_Catalog, pivot.CI_CGC_Catalog);
		}
	}
}
