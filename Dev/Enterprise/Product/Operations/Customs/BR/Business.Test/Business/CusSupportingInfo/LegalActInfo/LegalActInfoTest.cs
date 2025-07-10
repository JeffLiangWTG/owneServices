using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LegalActInfo))]
	class LegalActInfoTest : Customs.Business.Testing.CusSupportingInfoTest<LegalActInfo>
	{
		public void TestSetDefaultValues()
		{
			var legalAct = Factory.New<LegalActInfo>();
			AssertEquals(CusSupportingInfoTypeList.Codes.LegalAct, legalAct.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, legalAct.CSI_ParentTableCode);
		}

		public void TestAdditionalTariffsCusSupportingSettingValues()
		{
			var date = ZDateTime.Now;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			var additionalCusLineTariffDetails = invoiceLine.AdditionalTariffs.AddNew();
			additionalCusLineTariffDetails.TariffType = ChildTariffTypeList.Codes.LETEC;
			additionalCusLineTariffDetails.ExNumber = "001";
			var legalAct = additionalCusLineTariffDetails.LegalAct;
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.ExIPITariff;
			legalAct.CSI_Tariff = "TST";
			legalAct.CSI_Code = "1";
			legalAct.CSI_IssuerType = "2";
			legalAct.CSI_ReferenceNumber = "123456";
			legalAct.CSI_YearOfIssue = date.Year.ToString();

			AssertEquals("2", legalAct.CSI_SubType);
			AssertEquals("TST", legalAct.CSI_Tariff);
			AssertEquals("1", legalAct.CSI_Code);
			AssertEquals("2", legalAct.CSI_IssuerType);
			AssertEquals("123456", legalAct.CSI_ReferenceNumber);
			AssertEquals(date.Year.ToString(), legalAct.CSI_YearOfIssue);
			AssertEquals(new ZDateTime(date.Year, 1, 1), legalAct.CSI_DateOfIssue);
		}

		public void TestCSI_YearOfIssue()
		{
			var legalAct = Factory.New<LegalActInfo>();
			AssertEquals(ZDateTime.Empty, legalAct.CSI_DateOfIssue);
			AssertEquals(ZString.Empty, legalAct.CSI_YearOfIssue);
			legalAct.CSI_YearOfIssue = "ASD";
			AssertEquals(ZDateTime.Empty, legalAct.CSI_DateOfIssue);
			AssertEquals(ZString.Empty, legalAct.CSI_YearOfIssue);
			legalAct.CSI_YearOfIssue = "1899";
			AssertEquals(new ZDateTime(1900, 1, 1), legalAct.CSI_DateOfIssue);
			AssertEquals("1900", legalAct.CSI_YearOfIssue);
			legalAct.CSI_YearOfIssue = "2080";
			AssertEquals(new ZDateTime(2079, 1, 1), legalAct.CSI_DateOfIssue);
			AssertEquals("2079", legalAct.CSI_YearOfIssue);
			legalAct.CSI_YearOfIssue = "2010";
			AssertEquals(new ZDateTime(2010, 1, 1), legalAct.CSI_DateOfIssue);
			AssertEquals("2010", legalAct.CSI_YearOfIssue);
		}

		public void TestIsSavedByFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var legalAct = invoiceLine.LegalActInfos.AddNew();
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.ExDutyTariff;
			AssertEquals("Should be saved when CSI_SubType = 1", true, legalAct.IsSavedByFactory);
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.ExIPITariff;
			AssertEquals("Should be saved when CSI_SubType = 2", true, legalAct.IsSavedByFactory);
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.TariffAgreement;
			AssertEquals("Should be saved when CSI_SubType = 3", true, legalAct.IsSavedByFactory);
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.IPITaxBenefit;
			AssertEquals("Should NOT be saved when CSI_SubType = 4", false, legalAct.IsSavedByFactory);
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.Antidumping;
			AssertEquals("Should NOT be saved when CSI_SubType = 5", false, legalAct.IsSavedByFactory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var cusLineTariffDetails = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().AdditionalTariffs.AddNew();
			cusLineTariffDetails.TariffType = ChildTariffTypeList.Codes.LETEC;
			cusLineTariffDetails.ExNumber = "001";
			return cusLineTariffDetails.LegalAct;
		}

		protected override IEnumerable<LegalActInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			var additionalCusLineTariffDetails = invoiceLine.AdditionalTariffs.FirstOrDefault() as AdditionalTariff;
			additionalCusLineTariffDetails.TariffType = ChildTariffTypeList.Codes.LETEC;
			additionalCusLineTariffDetails.ExNumber = "001";
			var legalActCusSupporting = additionalCusLineTariffDetails.LegalAct;
			legalActCusSupporting.CSI_Tariff = "TST";
			legalActCusSupporting.CSI_Code = "1";
			legalActCusSupporting.CSI_IssuerType = "2";
			legalActCusSupporting.CSI_ReferenceNumber = "123456";
			legalActCusSupporting.CSI_YearOfIssue = ZDateTime.Now.Year.ToString();
			yield return legalActCusSupporting;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}
	}
}
