using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AdditionalTariffCollection))]
	class AdditionalTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalTariffCollection>
	{
		public void TestLoad()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.CusLineTariffDetails.AddNew(Constants.TariffTypes.NCCA, "001");
			invoiceLine.CusLineTariffDetails.AddNew(Constants.TariffTypes.NALADIHS, "001");

			var additionalTariffCollection = new AdditionalTariffCollection(invoiceLine);
			additionalTariffCollection.Load();
			Assert("IsLoaded", additionalTariffCollection.IsLoaded);
			AssertEquals(0, additionalTariffCollection.Count);

			var tariffDetailLETEC = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetailLETEC.BZ_LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			var tariffDetailIPI = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetailIPI.BZ_LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
			var tariffDetailAGM = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetailAGM.BZ_LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;

			var legalAct1 = invoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.ExDutyTariff);
			legalAct1.CSI_IssuerType = "123";
			legalAct1.CSI_ReferenceNumber = "456";
			legalAct1.CSI_YearOfIssue = ZDateTime.Now.Year.ToString();

			var legalAct2 = invoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.ExIPITariff);
			legalAct2.CSI_IssuerType = "123";
			legalAct2.CSI_ReferenceNumber = "456";
			legalAct2.CSI_YearOfIssue = ZDateTime.Now.Year.ToString();

			var legalAct3 = invoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.TariffAgreement);
			legalAct3.CSI_IssuerType = "123";
			legalAct3.CSI_ReferenceNumber = "456";
			legalAct3.CSI_YearOfIssue = ZDateTime.Now.Year.ToString();

			additionalTariffCollection.Rebuild();
			Assert("IsLoaded", additionalTariffCollection.IsLoaded);
			AssertEquals(3, additionalTariffCollection.Count);

			var addTariffLETEC = additionalTariffCollection.Cast<AdditionalTariff>().Single(x => x.LegalActSubject == AdditionalTaxTypeList.Codes.ExDutyTariff);
			AssertSame(tariffDetailLETEC, addTariffLETEC.TariffDetail);

			var addTariffIPI = additionalTariffCollection.Cast<AdditionalTariff>().Single(x => x.LegalActSubject == AdditionalTaxTypeList.Codes.ExIPITariff);
			AssertSame(tariffDetailIPI, addTariffIPI.TariffDetail);

			var addTariffAGM = additionalTariffCollection.Cast<AdditionalTariff>().Single(x => x.LegalActSubject == AdditionalTaxTypeList.Codes.TariffAgreement);
			AssertSame(tariffDetailAGM, addTariffAGM.TariffDetail);

			AssertEquals(3, invoiceLine.LegalActInfos.Count);

			var dutyTariffLegalActs = invoiceLine.LegalActInfos.Cast<LegalActInfo>().Where(x => x.CSI_SubType == AdditionalTaxTypeList.Codes.ExDutyTariff);
			AssertCollectionContains(legalAct1, dutyTariffLegalActs);
			AssertContainsExactElementsInAnyOrder("Duty Tariff & Legal Act '1' matches", dutyTariffLegalActs, new[] { addTariffLETEC.LegalAct });

			AssertSame("IPI Tariff & Legal Act '2' matches", legalAct2, addTariffIPI.LegalAct);
			AssertSame("AGM Tariff & Legal Act '3' matches", legalAct3, addTariffAGM.LegalAct);

			additionalTariffCollection.Rebuild();
			AssertEquals(3, additionalTariffCollection.Count);
		}

		public void TestLoadWhenCusLineTariffDetailNotExists()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			var additional = invoiceLine.AdditionalTariffs.AddNew();
			additional.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			invoiceLine.AdditionalTariffs.Load();

			AssertEquals("TariffDetail.BZ_LegalActSubject should be", AdditionalTaxTypeList.Codes.ExDutyTariff, additional.TariffDetail.BZ_LegalActSubject);
		}

		public void TestCreateNonPersistentBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("Precondition:", 0, invoiceLine.AdditionalTariffs.Count);
			AssertEquals("Precondition:", 0, invoiceLine.CusLineTariffDetails.Count);
			AssertEquals("Precondition:", 0, invoiceLine.LegalActInfos.Count);

			var additionalTariff = invoiceLine.AdditionalTariffs.AddNew();

			AssertEquals("A new AdditionTariff added", 1, invoiceLine.AdditionalTariffs.Count);
			AssertEquals("A new CusLineTariffDetail added", 1, invoiceLine.CusLineTariffDetails.Count);
			AssertEquals("A new LegalAct added", 1, invoiceLine.LegalActInfos.Count);

			AssertSame(invoiceLine.CusLineTariffDetails.First(), additionalTariff.TariffDetail);
			AssertSame(invoiceLine.LegalActInfos.First(), additionalTariff.LegalAct);
		}

		public override void TestDelete()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionalTariff1 = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff1.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			var additionalTariff2 = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff2.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;

			AssertEquals("Collection count equal 2", 2, invoiceLine.AdditionalTariffs.Count);

			var tariffDetail1 = additionalTariff1.TariffDetail;
			var legalAct1 = additionalTariff1.LegalAct;

			additionalTariff1.Delete();

			CombineAssertions(() =>
			{
				Assert("additionalTariff1.LegalAct must be deleted", legalAct1.IsDeleted);
				Assert("additionalTariff1.TariffCode must be deleted", tariffDetail1.IsDeleted);
				Assert("additionalTariff2.LegalAct must NOT be deleted", !additionalTariff2.LegalAct.IsDeleted);
				Assert("additionalTariff2.TariffCode must NOT be deleted", !additionalTariff2.TariffDetail.IsDeleted);
			});
		}

		public void TestFindBySubject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionalTariff1 = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff1.LegalAct.CSI_SubType = "1";

			var additionalTariffBySubject = invoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff);

			AssertNotNull("Collection should not be null", additionalTariffBySubject);
		}

		protected override AdditionalTariffCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var additionalTariffCollection = new AdditionalTariffCollection(declaration.Invoices.AddNew().InvoiceLines.AddNew());
			return additionalTariffCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			var legalActInfo = invoiceLine.LegalActInfos.AddNew();
			return new AdditionalTariff(legalActInfo, cusLineTariffDetail);
		}
	}
}
