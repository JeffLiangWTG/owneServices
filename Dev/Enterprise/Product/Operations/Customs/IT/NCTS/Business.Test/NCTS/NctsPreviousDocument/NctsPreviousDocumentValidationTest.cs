using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsPreviousDocumentValidationTest : BasePreviousDocumentValidationTest
{
	public void TestCheckCSI_DateOfIssue_NCTS()
	{
		var message = ValidationCaptions.NctsPreviousDocument.InvalidDateOfIssue;

		var previousDocument = GetPreviousDocumentForTesting();
		CombineAssertions(nameof(previousDocument.CSI_DateOfIssue), () =>
		{
			previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
			AssertNoMessageErrorContaining("when empty", previousDocument.CSI_DateOfIssueInfo, message);
			previousDocument.CSI_DateOfIssue = ZDateTime.Now;
			AssertNoMessageErrorContaining("when the date is today", previousDocument.CSI_DateOfIssueInfo, message);
			previousDocument.CSI_DateOfIssue = ZDateTime.Now.AddDays(-1);
			AssertNoMessageErrorContaining("when the date is in the past", previousDocument.CSI_DateOfIssueInfo, message);
			previousDocument.CSI_DateOfIssue = ZDateTime.Now.AddDays(1);
			AssertHasMessageErrorContaining("when the date is in the future", previousDocument.CSI_DateOfIssueInfo, message);
		});
	}

	protected override IPreviousDocumentForTesting GetPreviousDocumentForTesting() => previousDocument;

	public void TestLineNoAndTariffEmptyWhenRP()
	{
		var previousDocument = Factory.New<NctsPreviousDocumentForTest>();
		string assertionMessage;

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
		CombineAssertions("When procedure is RP", () =>
		{
			assertionMessage = "When no LineNo and no Tariff";
			previousDocument.CSI_LineNo = ZShort.Zero;
			previousDocument.CSI_Tariff = ZString.Empty;
			AssertNoMessageErrors(assertionMessage, previousDocument.CSI_LineNoInfo);
			AssertNoMessageErrors(assertionMessage, previousDocument.CSI_TariffInfo);
			AssertHasWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertHasWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);

			assertionMessage = "When no LineNo and but a Tariff";
			previousDocument.CSI_LineNo = ZShort.Zero;
			previousDocument.CSI_Tariff = "1001.1001";
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);

			assertionMessage = "When a LineNo and but no Tariff";
			previousDocument.CSI_LineNo = 1;
			previousDocument.CSI_Tariff = ZString.Empty;
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);

			assertionMessage = "When a LineNo and a Tariff";
			previousDocument.CSI_LineNo = 1;
			previousDocument.CSI_Tariff = "1001.1001";
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
		});

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3;
		CombineAssertions("When procedure is PA", () =>
		{
			assertionMessage = "When no LineNo and no Tariff";
			previousDocument.CSI_LineNo = ZShort.Zero;
			previousDocument.CSI_Tariff = ZString.Empty;
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);

			assertionMessage = "When no LineNo and but a Tariff";
			previousDocument.CSI_LineNo = ZShort.Zero;
			previousDocument.CSI_Tariff = "1001.1001";
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);

			assertionMessage = "When a LineNo and but no Tariff";
			previousDocument.CSI_LineNo = 1;
			previousDocument.CSI_Tariff = ZString.Empty;
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);

			assertionMessage = "When a LineNo and a Tariff";
			previousDocument.CSI_LineNo = 1;
			previousDocument.CSI_Tariff = "1001.1001";
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_LineNoInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
			AssertNoWarningContaining(assertionMessage, previousDocument.CSI_TariffInfo, ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
		});
	}

	public void TestReferenceNumberCheckDigitValidation()
	{
		var checkDigitMessageErrorPrefix = "The check digit entered is incorrect.";
		AssertEquals("Check Digit error prefix", checkDigitMessageErrorPrefix, ValidationCaptions.PreviousDocument.CheckDigitIsIncorrectErrorPrefix);

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaFerroviariaModCim;
		AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		CombineAssertions("ReferenceNumber = 123456Z", () =>
		{
			previousDocument.CSI_ReferenceNumber = "123456Z";
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, $"{checkDigitMessageErrorPrefix} Current value is: 'Z', expected 'A'.");
		});

		previousDocument.CSI_ReferenceNumber = "123456A";
		AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);

		CombineAssertions("ReferenceNumber = 30919Z", () =>
		{
			previousDocument.CSI_ReferenceNumber = "30919Z";
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, $"{checkDigitMessageErrorPrefix} Current value is: 'Z', expected 'X'.");
		});

		previousDocument.CSI_ReferenceNumber = "30919X";
		AssertNoMessageErrorContaining("ReferenceNumber = 30919X", previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);

		previousDocument.CSI_Procedure = "";
		previousDocument.CSI_ReferenceNumber = "30919Z";
		AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);
	}

	public void TestMoreThanOnePaAndOneOrMoreRpDocuments()
	{
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertNoWarningContaining(previousDocument.CSI_ProcedureInfo, ValidationCaptions.NctsPreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);

		var previousDocumentRp = goodsItem.PreviousDocuments.AddNew();
		previousDocumentRp.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
		AssertNoWarningContaining(previousDocumentRp.CSI_ProcedureInfo, ValidationCaptions.NctsPreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);

		var previousDocumentPa = goodsItem.PreviousDocuments.AddNew();
		previousDocumentPa.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertHasWarningContaining(previousDocumentPa.CSI_ProcedureInfo, ValidationCaptions.NctsPreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);

		var previousDocumentRp2 = goodsItem.PreviousDocuments.AddNew();
		previousDocumentRp2.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertHasWarningContaining(previousDocumentRp2.CSI_ProcedureInfo, ValidationCaptions.NctsPreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);
	}

	public void TestCSI_Quantity3MandatoryForProcedurePa()
	{
		AssertNoMessageErrorContaining("CSI_Quantity3 is not required", previousDocument.CSI_Quantity3Info, MandatoryValidation.YouHaveNotEntered);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertHasMessageErrorContaining("CSI_Procedure is PA, CSI_Quantity3 must be entered", previousDocument.CSI_Quantity3Info, MandatoryValidation.YouHaveNotEntered);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
		AssertNoMessageErrorContaining("CSI_Procedure is RP, CSI_Quantity3 is not required", previousDocument.CSI_Quantity3Info, MandatoryValidation.YouHaveNotEntered);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		previousDocument.CSI_Quantity3 = 20;
		AssertNoMessageErrorContaining("CSI_Procedure is PA and CSI_Quantity3 is entered", previousDocument.CSI_Quantity3Info, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestGrossWeightQuantitiesDoesNotMatchGoodsItemQuantity()
	{
		AssertNoMessageErrorContaining("CSI_Procedure is not PA", previousDocument.CSI_Quantity3Info, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertNoMessageErrorContaining("CSI_Procedure is PA but last 2 chars of BY_Procedure != '00'", previousDocument.CSI_Quantity3Info, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		goodsItem.BY_Procedure = "4000";
		previousDocument.Validation.ValidateCSI_Quantity3();
		AssertNoMessageErrorContaining("CSI_Procedure is PA and last 2 chars of BY_Procedure == '00', but CSI_Quantity3 == BY_GrossWeight", previousDocument.CSI_Quantity3Info, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		previousDocument.CSI_Quantity3 = 100;
		AssertHasMessageErrorContaining("CSI_Procedure is PA, last 2 chars of BY_Procedure == '00' and CSI_Quantity3 != BY_GrossWeight", previousDocument.CSI_Quantity3Info, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
		previousDocument2.CSI_Quantity3 = 100;
		AssertHasMessageErrorContaining("CSI_Procedure is PA, last 2 chars of BY_Procedure == '00' and SUM(CSI_Quantity3) != BY_GrossWeight", previousDocument.CSI_Quantity3Info, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		goodsItem.BY_GrossWeight = 200;
		previousDocument.Validation.ValidateCSI_Quantity3();
		AssertNoMessageErrorContaining("CSI_Procedure is PA, last 2 chars of BY_Procedure == '00' and SUM(CSI_Quantity3) == BY_GrossWeight", previousDocument.CSI_Quantity3Info, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
	}

	public void TestPackageQuantitiesDoesNotMatchGoodsItemQuantity()
	{
		AssertNoMessageErrorContaining("Package Quantity is not required", previousDocument.CSI_PackQtyInfo, MandatoryValidation.YouHaveNotEntered);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertHasMessageErrorContaining("CSI_Procedure is PA, Package Quantity must be entered", previousDocument.CSI_PackQtyInfo, MandatoryValidation.YouHaveNotEntered);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
		AssertNoMessageErrorContaining("CSI_Procedure is RP, Package Quantity is not required", previousDocument.CSI_PackQtyInfo, MandatoryValidation.YouHaveNotEntered);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		previousDocument.CSI_PackQty = 20;
		AssertNoMessageErrorContaining("CSI_Procedure is PA and Package Quantity is entered", previousDocument.CSI_PackQtyInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestPreviousDocumentsQuantitiesNotMatchGoodsItemQuantity()
	{
		var package = goodsItem.Packages.AddNew();
		AssertNoMessageErrorContaining("CSI_Procedure is not PA", previousDocument.CSI_PackQtyInfo, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertNoMessageErrorContaining("CSI_Procedure is PA but last 2 chars of BY_Procedure != '00'", previousDocument.CSI_PackQtyInfo, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		goodsItem.BY_Procedure = "4000";
		AssertNoMessageErrorContaining("CSI_Procedure is PA and last 2 chars of BY_Procedure == '00', but PackageQuantity == SUM(B5_UnitCount", previousDocument.CSI_PackQtyInfo, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		previousDocument.CSI_PackQty = 100;
		AssertHasMessageErrorContaining("CSI_Procedure is PA, last 2 chars of BY_Procedure == '00' and PackageQuantity != SUM(B5_UnitCount", previousDocument.CSI_PackQtyInfo, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
		previousDocument2.CSI_PackQty = 100;
		AssertHasMessageErrorContaining("CSI_Procedure is PA, last 2 chars of BY_Procedure == '00' and SUM(PackageQuantity) != SUM(B5_UnitCount", previousDocument.CSI_PackQtyInfo, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		package.B5_UnitCount = 200;
		previousDocument.Validation.ValidateCSI_PackQty();
		AssertNoMessageErrorContaining("CSI_Procedure is PA, last 2 chars of BY_Procedure == '00' and SUM(PackageQuantity) == SUM(B5_UnitCount)", previousDocument.CSI_PackQtyInfo, ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
	}

	[ExpectNoExceptions]
	public void TestValidateAllTriggersPackageQuantityValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_PackQty");
		mockNctsPreviousDocumentValidation.Object.ValidateAll();
		mockNctsPreviousDocumentValidation.Protected().Verify("CheckCSI_PackQty", Times.Never());
	}

	public void TestCheckCSI_LineNo()
	{
		var previousDocument = GetPreviousDocumentForTesting();
		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_LineNo validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_LineNo = 1;
			AssertHasMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_LineNo = ZShort.Zero;
			AssertNoMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "MRN";
		CombineAssertions("Checking CSI_LineNo validation when CSI_Procedure is MRN", delegate
		{
			previousDocument.CSI_LineNo = ZShort.Zero;
			AssertNoMessageErrors("CSI_LineNoInfo Validation messages, Empty LineNo", previousDocument.CSI_LineNoInfo);
			previousDocument.CSI_LineNo = 1;
			AssertNoMessageErrors("CSI_LineNoInfo Validation messages, LineNo filled", previousDocument.CSI_LineNoInfo);
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_LineNo validation when CSI_Procedure is LC", delegate
		{
			previousDocument.CSI_LineNo = 1;
			AssertHasMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_LineNo = ZShort.Zero;
			AssertNoMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);
		});
	}

	public void TestCheckCSI_Quantity2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var importTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		Factory.Save();

		var tariffWithUOM = helper.CreateTariff(Core.Constants.CountryCodes.Italy, importTariffType.PK, "6402121000 ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithUOM, UnitOfMeasureTypes.AdditionalUOMType, "NPR");
		helper.CreateTariff(Core.Constants.CountryCodes.Italy, importTariffType.PK, "8001100000 ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		CombineAssertions("When it is a summary declaration document", () =>
		{
			previousDocument.CSI_Procedure = "A3";
			Assert("PRE-CONDITION:", previousDocument.IsSummaryDeclarationDocument);

			previousDocument.FormattedTariff = "8001100000";
			previousDocument.CSI_Quantity2 = 0m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.FormattedTariff = "6402121000";
			previousDocument.CSI_Quantity2 = 0m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Quantity2 = 1m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("When it is a previous procedure document and import declaration", () =>
		{
			previousDocument.CSI_Procedure = "2";
			Assert("PRE-CONDITION:", previousDocument.IsPreviousProcedureDocument);

			previousDocument.FormattedTariff = "8001100000";
			previousDocument.CSI_Quantity2 = 0m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.FormattedTariff = "6402121000";
			previousDocument.CSI_Quantity2 = 0m;
			AssertHasMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Quantity2 = 1m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		previousDocument = Factory.New<NctsPreviousDocumentForTest>();
		goodsItem.PreviousDocuments.Add(previousDocument);
	}

	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;
	NctsPreviousDocumentForTest previousDocument;
}
