using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;
using UniversalConstants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckJI_AddInfoIsWesternEuropean()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AddInfo = "String=123456*NString=一二三四五六";
			AssertHasError(invoiceLine.JI_AddInfoInfo, "Info only accepts Western European languages characters.");

			invoiceLine.JI_AddInfo = "String=123456";
			Assert(!invoiceLine.JI_AddInfoInfo.HasErrors());
		}

		public void TestWhenInvoiceUQNotEqualCustomsUQ()
		{
			var errMsg = "Due to the Customs requirement, the Customs Quantity, UQ, and Customs Unit Price are to be sent to the Customs instead of the Invoice Quantity, UQ, and its Unit Price.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2402200000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateTariffAttribute(ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Tariff = "1234500000";
			invoiceLine.JI_InvoiceUQ = "EA";
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.Validation.ValidateCustomsUnitPrice();
			AssertNoWarningContaining(invoiceLine.CustomsUnitPriceInfo, errMsg);

			invoiceLine.JI_Tariff = "2402200000";
			invoiceLine.JI_InvoiceUQ = "U";
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.Validation.ValidateCustomsUnitPrice();
			AssertNoWarningContaining(invoiceLine.CustomsUnitPriceInfo, errMsg);

			invoiceLine.JI_InvoiceUQ = "EA";
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.Validation.ValidateCustomsUnitPrice();
			AssertHasWarning(invoiceLine.CustomsUnitPriceInfo, errMsg);
		}

		public void TestTariffExportOGARegulationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2402200000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateTariffAttribute(ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff);
			var ogaCondition = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", ZZ.RefCusConditionType.OGA, "Other Government Agency Requirement Details");
			var simpledrawbackCondition = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", ZZ.RefCusConditionType.SimpleDrawback, "Simple drawback");
			var regulationNumber = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGARegulationNumber, "OGA Regulation Number");
			var documentName = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGADocumentName, "OGA Document Name");

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101211000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "5305003000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff2, "71", "수출허가서", ogaCondition, regulationNumber, documentName, true, false);

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9305101010", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "34", "수출허가서", ogaCondition, regulationNumber, documentName, true, false);
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "55", "수출허가증", ogaCondition, regulationNumber, documentName, true, false);

			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertNoErrors(invoiceLine.JI_TariffInfo);
			invoiceLine.GAApprovalDataCollection.RemoveAll();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoErrors(invoiceLine.JI_TariffInfo);

			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertNoErrors(invoiceLine.JI_TariffInfo);
			invoiceLine.GAApprovalDataCollection.RemoveAll();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulation 71 but the details are not in Other Details > Approval Document");

			invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
			AssertNoErrors(invoiceLine.JI_TariffInfo);
			invoiceLine.GAApprovalDataCollection.Remove(invoiceLine.GAApprovalDataCollection.First());
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulation 34 but the details are not in Other Details > Approval Document");

			invoiceLine.GAApprovalDataCollection.RemoveAll();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulations 34, 55 but the details are not in Other Details > Approval Document");
		}

		public void TestCusEntryInstruction()
		{
			var errMsg = "Entry Instruction should be selected on an Invoice Line";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);

			instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);

			instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, errMsg);
		}

		public void TestJI_NetWeightUQ()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 0;
			invoiceLine.Validation.ValidateJI_NetWeightUQ();
			AssertNoMessageErrors(invoiceLine.JI_NetWeightUQInfo);

			invoiceLine.JI_NetWeight = 1;
			invoiceLine.JI_NetWeightUQ = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_NetWeightUQ = "99";
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageErrors(invoiceLine.JI_NetWeightUQInfo);
		}

		public void TestJI_WeightUQ()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 0;
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertNoMessageErrors(invoiceLine.JI_WeightUQInfo);

			invoiceLine.JI_Weight = 1;
			invoiceLine.JI_WeightUQ = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_WeightUQ = "99";
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageErrors(invoiceLine.JI_WeightUQInfo);
		}

		public void TestCheckJI_Description()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Description = "Test Model";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_ZZF_NKTaxType()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoMessageErrors(invoiceLine.JI_ZZF_NKTaxTypeInfo);

			invoiceLine.JI_ZZF_NKTaxType = "VTA";
			AssertNoMessageErrors(invoiceLine.JI_ZZF_NKTaxTypeInfo);

			invoiceLine.JI_ZZF_NKTaxType = "VTB";
			AssertNoMessageErrors(invoiceLine.JI_ZZF_NKTaxTypeInfo);

			invoiceLine.JI_ZZF_NKTaxType = "VTC";
			AssertNoMessageErrors(invoiceLine.JI_ZZF_NKTaxTypeInfo);

			invoiceLine.JI_ZZF_NKTaxType = "VTD";
			AssertHasMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
