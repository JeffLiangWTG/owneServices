using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ExportJobComInvoiceLineValidation))]
sealed class ExportJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJI_Description()
	{
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;

		var warningMessage = "Goods Description is more than 120 Char. Only first 120 Characters will be used for SB filling.";

		InvoiceLine.JI_Description = new string('A', 121);
		AssertHasWarning(InvoiceLine.JI_DescriptionInfo, warningMessage);

		InvoiceLine.JI_Description = new string('A', 120);
		AssertNoWarning(InvoiceLine.JI_DescriptionInfo, warningMessage);
	}

	public void TestJI_PMV()
	{
		InvoiceLine.JI_ValuationMarkup = 0;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_PMVInfo);
		InvoiceLine.JI_ValuationMarkup = 1;
		AssertNoMessageErrors(InvoiceLine.JI_PMVInfo);
	}

	public void TestCheckJI_EndUse()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_EndUseInfo);
	}

	public void TestCheckJI_StateOrRegionOfOrigin()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceLine.JI_StateOrRegionOfOriginInfo, new ZString[] { "x", "xx" }, InvoiceLine.Lookups.OriginStateList.GetAllCodesZString());
	}

	public void TestCheckJI_UnitPrice()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_UnitPriceInfo);
	}

	public void TestCheckJI_UnitQuantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_UnitQuantityInfo);
	}

	public void TestCheckJI_UnitUQ()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_UnitUQInfo);
	}

	public void TestCheckJI_AccessoryStatus()
	{
		RefDataSetupTestHelper.SetupExportAccessoryStatusCodes(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceLine.JI_AccessoryStatusInfo, new ZString[] { "4", "5" }, new ZString[] { "0", "1" });
	}

	public void TestValidateAccessoryDescription()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.JI_AccessoryStatus = "1";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.AccessoryDescriptionInfo);

			InvoiceLine.JI_AccessoryStatus = "2";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.AccessoryDescriptionInfo);

			InvoiceLine.JI_AccessoryStatus = "3";
			ValidationTestHelper.AssertFieldIsNotMandatory(InvoiceLine.AccessoryDescriptionInfo);
		});
	}

	public void TestJI_RewardItem()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_RewardItemInfo, new ZString[] { "x" }, InvoiceLine.Lookups.RewardItemList.GetAllCodesZString());
	}

	public void TestCheck_TransitCountry()
	{
		var validcodes = InvoiceLine.Lookups.CountryOfTransits;
		ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceLine.JI_RN_NKCountryOfTransitInfo, new ZString[] { "x", "xx" }, validcodes.OfType<ICodeDescription>().Select(x => new ZString(x.Code)).ToArray());
	}

	public void TestCheckJI_InvoiceQuantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_InvoiceQuantityInfo);
	}

	public void TestCheckJI_InvoiceUQ_Mandatory()
	{
		var info = InvoiceLine.JI_InvoiceUQInfo;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(info, MandatoryValidation.YouHaveNotEnteredMessage("UOM"));
	}

	public void TestCheckJI_InvoiceUQ_PackMapping()
	{
		CombineAssertions(() =>
		{
			RefDataSetupTestHelper.SetupCustomsUnitOfQuantityCode(Factory);
			var expectedPartialMessage = "does not map to a Customs Package Type for country IN. Please add the mapping via Maintain > Customs > Customs Files > Packs Conversion.";
			InvoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageErrorContaining("When Invoice UQ empty", InvoiceLine.JI_InvoiceUQInfo, expectedPartialMessage);

			InvoiceLine.JI_InvoiceUQ = "CR1";
			AssertHasMessageErrorContaining("When Pack conversion does not exist", InvoiceLine.JI_InvoiceUQInfo, expectedPartialMessage);

			InvoiceLine.JI_InvoiceUQ = "CM";
			AssertNoMessageErrorContaining("InvoiceUQ is not in CUSUQ but in CMS", InvoiceLine.JI_InvoiceUQInfo, expectedPartialMessage);

			RefDataSetupTestHelper.SetupCusPack(Factory, "CR1", "CR1", 1);
			InvoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageErrorContaining("When pack conversion exist", InvoiceLine.JI_InvoiceUQInfo, expectedPartialMessage);

			InvoiceLine.JI_InvoiceUQ = "SMM";
			AssertHasMessageErrorContaining("When unknown pack", InvoiceLine.JI_InvoiceUQInfo, expectedPartialMessage);

			InvoiceLine.JI_InvoiceUQ = "KGS";
			AssertNoMessageErrorContaining("When known pack from CUSUQ", InvoiceLine.JI_InvoiceUQInfo, expectedPartialMessage);
		});
	}
	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= GetInvoiceLine();
	JobComInvoiceLine invoiceLine;

	JobComInvoiceLine GetInvoiceLine()
	{
		var invoice = Declaration.Invoices.AddNew();
		return invoice.InvoiceLines.AddNew();
	}

	JobDeclaration GetJobDeclaration()
	{
		var result = Factory.New<JobDeclaration>();
		result.JE_MessageType = JobMessageTypeList.Codes.Export;
		return result;
	}
}
