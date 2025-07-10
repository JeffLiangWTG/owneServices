using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class ImportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_RegionOfDestination()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(GetInvoiceLine().ZG_RegionOfDestinationInfo, "!", BERegionList.Codes.BrusselsRegion);
	}

	public void TestValidateZG_CountryOfDestination()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium");
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "US", "United States", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "AU", "Australia", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "DE", "Germany", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		var invoiceLine = GetInvoiceLine();
		CombineAssertions(() =>
		{
			new List<string> { "", "FR", "DE", "IT" }.ForEach(v =>
			{
				invoiceLine.ZG_CountryOfDestination = v;
				AssertNoMessageErrorContaining(v + " should be valid", invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
			new List<string> { "US", "AU", "XX" }.ForEach(v =>
			{
				invoiceLine.ZG_CountryOfDestination = v;
				AssertHasMessageErrorContaining(v + " should be invalid", invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		});
	}

	public void TestValidateZG_RegionOfDestination()
	{
		var invoiceLine = GetInvoiceLine();
		CombineAssertions(() =>
		{
			invoiceLine.ZG_RegionOfDestination = "5";
			AssertListValidationInvalidCodeMessageError(invoiceLine.ZG_RegionOfDestinationInfo, true);

			new BERegionList().GetAllCodes().ToList().ForEach(validCode =>
			{
				invoiceLine.ZG_RegionOfDestination = validCode;
				AssertListValidationInvalidCodeMessageError(invoiceLine.ZG_RegionOfDestinationInfo, false);
			});
		});
	}

	public void TestRegionOfDestinationValidation()
	{
		var invoiceLine = GetInvoiceLine();

		CombineAssertions(() =>
		{
			foreach (var codeDescription in invoiceLine.AddInfoLookups.RegionOfDestinationList)
			{
				invoiceLine.ZG_RegionOfDestination = ((ICodeDescription)codeDescription).Code;
				AssertNoNotifications("Correct input of Region of Destination should not trigger error", invoiceLine.ZG_RegionOfDestinationInfo);
			}
			invoiceLine.ZG_RegionOfDestination = "(";
			AssertHasMessageErrors("Region of Destination should report a message about invalid input '('", invoiceLine.ZG_RegionOfDestinationInfo);
		});
	}

	public void TestValidateCountryOfSupply()
	{
		var invoiceLine = GetInvoiceLine();
		invoiceLine.JI_CEI = invoiceLine.Declaration.CustomsEntryInstructions.AddNew().PK;
		CombineAssertions(() =>
		{
			foreach (var combination in PreferenceAndStyleTestCombination.CreateCombinations("Country of Origin (=Country of Supply)", GetPrimaryPreferences(), GetStyles()))
			{
				invoiceLine.EntryInstruction.CEI_Style = combination.Style;
				invoiceLine.JI_PrimaryPreference = combination.PrimaryPreference;

				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				AssertHasMessageError("JE_CEI = " + combination.Style + " Primary preference = " + combination.PrimaryPreference + ", CountryOfSupply should have a message error", invoiceLine.ZG_CountryOfSupplyInfo, combination.ErrorMessage);
				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Belgium;
				AssertNoMessageError("JE_CEI = " + combination.Style + " Primary preference = " + combination.PrimaryPreference + ", CountryOfSupply should not have a message error", invoiceLine.ZG_CountryOfSupplyInfo, combination.ErrorMessage);
			}
		});

		List<string> GetPrimaryPreferences() => new List<string>
		{
			"252",
			"304",
			"476",
		};

		List<string> GetStyles() => new List<string>
		{
			ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified,
		};
	}

	JobComInvoiceLine GetInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		return declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
}
