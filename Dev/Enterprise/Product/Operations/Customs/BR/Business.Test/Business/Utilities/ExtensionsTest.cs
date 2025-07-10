using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ExtensionsTest : TestCaseWithFactory
	{
		public void TestGetCNPJOrCPF()
		{
			var supplier = Factory.New<OrgHeader>();
			AssertEquals("CNPJ should be", ZString.Empty, supplier.GetCNPJOrCPF());

			supplier.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";
			AssertEquals("CNPJ should be", "58500398000105", supplier.GetCNPJOrCPF());
		}

		public void TestGetRootCNPJ()
		{
			OrgHeader header = null;
			AssertEquals("Root CNPJ should be", ZString.Empty, header.GetRootCNPJ());

			header = Factory.New<OrgHeader>();
			AssertEquals("Root CNPJ should be", ZString.Empty, header.GetRootCNPJ());

			header.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "27.094.734/0001-33", Core.Constants.CountryCodes.Brazil);
			AssertEquals("Root CNPJ should be", ZString.Empty, header.GetRootCNPJ());

			header.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "27094734", Core.Constants.CountryCodes.Brazil);
			AssertEquals("Root CNPJ should be", "27094734", header.GetRootCNPJ());
		}

		public void TestGetRootCNPJFromCNPJ()
		{
			OrgHeader supplier = null;
			AssertEquals("Root CNPJ should be", ZString.Empty, supplier.GetRootCNPJFromCNPJ());

			supplier = Factory.New<OrgHeader>();
			AssertEquals("Root CNPJ should be", ZString.Empty, supplier.GetRootCNPJFromCNPJ());

			supplier.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";
			AssertEquals("Root CNPJ should be", "58500398", supplier.GetRootCNPJFromCNPJ());

			supplier.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			supplier.PrimaryRegistrationNumber.Number = "123.456.789-01";
			AssertEquals("Root CPF should be", "12345678901", supplier.GetRootCNPJFromCNPJ());
		}

		public void TestGetTinCode()
		{
			OrgHeader header = null;
			AssertEquals("TIN Code should be", ZString.Empty, header.GetTinCode());

			header = Factory.New<OrgHeader>();
			AssertEquals("TIN Code should be", ZString.Empty, header.GetTinCode());

			header.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.TIN, "269020", Core.Constants.CountryCodes.Brazil);
			AssertEquals("TIN Code should be", "269020", header.GetTinCode());
		}

		public void TestGetInternal()
		{
			OrgHeader header = null;
			AssertEquals("Internal Code should be", ZString.Empty, header.GetInternalCode());

			header = Factory.New<OrgHeader>();
			AssertEquals("Internal Code should be", ZString.Empty, header.GetInternalCode());

			header.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode, "587481", Core.Constants.CountryCodes.Brazil);
			AssertEquals("Internal Code should be", "587481", header.GetInternalCode());
		}

		public void TestGetForeignOperatorEmail()
		{
			OrgHeader header = null;
			AssertEquals("Email should be", ZString.Empty, header.GetForeignOperatorEmail());

			header = Factory.New<OrgHeader>();
			AssertEquals("Email should be", ZString.Empty, header.GetForeignOperatorEmail());

			var contact = header.Contacts.AddNew();
			contact.OC_ContactName = "ContactName";
			contact.OC_Email = "allocationNFO@email.com";

			var contact2 = header.Contacts.AddNew();
			contact2.OC_ContactName = "ContactName2";
			contact2.OC_Email = "allocationCUS@email.com";

			var allocationNFO = contact.Allocations.AddNew();
			allocationNFO.PC_Type = OrgConstants.ContactAllocationType.BRForeignOperator;

			var allocationCUS = contact2.Allocations.AddNew();
			allocationCUS.PC_Type = OrgConstants.ContactAllocationType.CUS;

			AssertEquals("Email should be", "allocationNFO@email.com", header.GetForeignOperatorEmail());
		}

		public void TestGetNearestWorkingdayBefore()
		{
			var testDate = new ZDateTime(2021, 5, 10);
			AssertEquals("Nearest Working day Before", new ZDateTime(2021, 5, 10), testDate.GetNearestWorkingdayBefore());

			testDate = new ZDateTime(2021, 5, 9);
			AssertEquals("Nearest Working day Before", new ZDateTime(2021, 5, 7), testDate.GetNearestWorkingdayBefore());

			testDate = new ZDateTime(2021, 5, 8);
			AssertEquals("Nearest Working day Before", new ZDateTime(2021, 5, 7), testDate.GetNearestWorkingdayBefore());

			testDate = new ZDateTime(2021, 4, 21);
			AssertEquals("Nearest Working day Before", new ZDateTime(2021, 4, 20), testDate.GetNearestWorkingdayBefore());

			testDate = new ZDateTime(2021, 1, 1);
			AssertEquals("Nearest Working day Before", new ZDateTime(2020, 12, 31), testDate.GetNearestWorkingdayBefore());
		}

		public void TestConvertToYesNoList()
		{
			AssertEquals("True value", YesNoList.Codes.Yes, ZBool.True.ConvertToYesNoList());
			AssertEquals("False value", YesNoList.Codes.No, ZBool.False.ConvertToYesNoList());
		}

		public void TestConvertYesNoToBool()
		{
			AssertEquals("True value", "true", ((ZString)Profile.AnswerValues.Yes).ConvertYesNoToBool());
			AssertEquals("False value", "false", ((ZString)Profile.AnswerValues.No).ConvertYesNoToBool());
			AssertEquals("Empty", string.Empty, ZString.Empty.ConvertYesNoToBool());
		}

		public void TestConvertBoolToYesNo()
		{
			AssertEquals("S value", Profile.AnswerValues.Yes, ((ZString)true.ToString().ToLower()).ConvertBoolToYesNo());
			AssertEquals("N value", Profile.AnswerValues.No, ((ZString)false.ToString().ToLower()).ConvertBoolToYesNo());
			AssertEquals("Empty", string.Empty, ZString.Empty.ConvertBoolToYesNo());
		}

		public void TestReturnNullIfEmpty_ZDecimal()
		{
			CombineAssertions(() =>
			{
				ZDecimal inputValue = 0m;
				AssertNull("Return Null if empty", inputValue.ReturnNullIfEmpty());

				inputValue = 123m;
				AssertEquals("Return input value if not empty", 123m, inputValue.ReturnNullIfEmpty());
			});
		}

		public void TestReturnNullIfEmpty_ZString()
		{
			CombineAssertions(() =>
			{
				var inputValue = ZString.Empty;
				AssertNull("Return Null if empty", inputValue.ReturnNullIfEmpty());

				inputValue = "TEST";
				AssertEquals("Return input value if not empty", "TEST", inputValue.ReturnNullIfEmpty());
			});
		}

		public void TestCombineValuesAsString()
		{
			var multipleText = "Multiple";
			AssertEquals("", ((ZString[])null).CombineValuesAsString(multipleText));
			AssertEquals("", Enumerable.Empty<ZString>().CombineValuesAsString(multipleText));
			AssertEquals("", System.Array.Empty<ZString>().CombineValuesAsString(multipleText));
			AssertEquals("", new ZString[] { "" }.CombineValuesAsString(multipleText));
			AssertEquals("", new ZString[] { "", "" }.CombineValuesAsString(multipleText));
			AssertEquals("1", new ZString[] { "", "1" }.CombineValuesAsString(multipleText));
			AssertEquals("1", new ZString[] { "", "1", "1" }.CombineValuesAsString(multipleText));
			AssertEquals(multipleText, new ZString[] { "", "1", "2" }.CombineValuesAsString(multipleText));
			AssertEquals(multipleText, new ZString[] { "1", "1", "2", "2" }.CombineValuesAsString(multipleText));
		}

		public void TestGetInvoiceUQDescriptions()
		{
			var invoiceUQList = RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Box", Factory.GetInvoiceUQDescriptions(invoiceUQList, "BOX").DescInEnglish);
				AssertEquals("Caixa", Factory.GetInvoiceUQDescriptions(invoiceUQList, "BOX").DescInPortugueseBrazil);

				AssertEquals("Kilograms", Factory.GetInvoiceUQDescriptions(invoiceUQList, "KG").DescInEnglish);
				AssertEquals("Quilogramas", Factory.GetInvoiceUQDescriptions(invoiceUQList, "KG").DescInPortugueseBrazil);
			});
		}

		public void TestGetTotalChargesAmountOnInvoiceLinesAndGetFirstCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_NetWeight = 60;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 150m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.UnitedStates);

			var insuranceCurrency = invoice.InvoiceLines.Cast<JobComInvoiceLine>().GetFirstChargeCurrency(Common.CustomsChargeTypeList.Codes.OverseasInsurance);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, insuranceCurrency.Code);
			AssertEquals(250m, invoice.InvoiceLines.Cast<JobComInvoiceLine>().GetTotalChargesAmountOnInvoiceLines(c => c.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance, insuranceCurrency));
			AssertEquals(250m, invoice.InvoiceLines.Cast<JobComInvoiceLine>().GetTotalChargesAmountOnInvoiceLines(Common.CustomsChargeTypeList.Codes.OverseasInsurance, insuranceCurrency));
		}
	}
}
