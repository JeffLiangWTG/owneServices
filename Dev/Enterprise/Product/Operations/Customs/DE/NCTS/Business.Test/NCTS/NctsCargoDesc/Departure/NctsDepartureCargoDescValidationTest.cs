using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		[DeveloperOnlyTest]
		public void TestPreSaveValidationValidatesPreviousProcedureMaster()
		{
			detail.PreviousProcedureMaster.CSI_Procedure = "N337";
			detail.PreviousDocuments.RemoveAndDeleteAll();
			AssertNoMessageErrorContaining(detail.PreviousProcedureMaster.CSI_ProcedureInfo, "NR0052");
			detail.RunPreSaveValidation();
			AssertHasMessageErrorContaining(detail.PreviousProcedureMaster.CSI_ProcedureInfo, "NR0052");
		}

		public void TestCheckBY_GrossWeightIsValidZDecimal()
		{
			const string message = "is too large";
			CombineAssertions(() =>
			{
				detail.BY_GrossWeight = 123456789012.999;
				AssertHasErrorContaining("Above Maximum", detail.BY_GrossWeightInfo, message);

				detail.BY_GrossWeight = 12345678901.999;
				AssertNoErrorContaining("Below Maximum", detail.BY_GrossWeightInfo, message);
			});
		}

		public void TestCheckBY_FormattedHarmonisedTariff_ValidDidgitCount()
		{
			const string message = "You have not entered a valid 6-, 8- or 11-digit Code.";

			CombineAssertions(() =>
			{
				detail.BY_FormattedHarmonisedTariff = "12345";
				AssertHasMessageError("5 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "1234567";
				AssertHasMessageError("7 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "1234567890";
				AssertHasMessageError("10 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "123456";
				AssertNoMessageError("6 - valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "12345678";
				AssertNoMessageError("8 - valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "12345678901";
				AssertNoMessageError("11 - valid", detail.BY_FormattedHarmonisedTariffInfo, message);
			});
		}

		public void TestCheckBY_FormattedHarmonisedTariff_ValidDidgitCount_WhenBillHasN830PreDocument()
		{
			const string message = "You have not entered a valid 8- or 11-digit Code.";

			CombineAssertions(() =>
			{
				bill.PreviousDocuments.AddNew().CSI_Code = "N830";
				detail.BY_FormattedHarmonisedTariff = "12345";
				AssertHasMessageError("5 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "1234567";
				AssertHasMessageError("7 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "1234567890";
				AssertHasMessageError("10 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "123456";
				AssertHasMessageError("6 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "12345678";
				AssertNoMessageError("8 - valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "12345678901";
				AssertNoMessageError("11 - valid", detail.BY_FormattedHarmonisedTariffInfo, message);
			});
		}

		public void TestCheckBY_FormattedHarmonisedTariff_ValidDidgitCount_WhenHasN830PreDocument()
		{
			const string message = "You have not entered a valid 8- or 11-digit Code.";

			CombineAssertions(() =>
			{
				detail.PreviousDocuments.AddNew().CSI_Code = "N830";
				detail.BY_FormattedHarmonisedTariff = "12345";
				AssertHasMessageError("5 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "1234567";
				AssertHasMessageError("7 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "1234567890";
				AssertHasMessageError("10 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "123456";
				AssertHasMessageError("6 - not valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "12345678";
				AssertNoMessageError("8 - valid", detail.BY_FormattedHarmonisedTariffInfo, message);

				detail.BY_FormattedHarmonisedTariff = "12345678901";
				AssertNoMessageError("11 - valid", detail.BY_FormattedHarmonisedTariffInfo, message);
			});
		}

		public void TestCheckBY_FormattedHarmonisedTariff_Mandatory_BM_InbondEntryTypeIsNotTIR()
		{
			CombineAssertions(() =>
			{
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
				detail.Validation.ValidateBY_HarmonisedTariff();
				AssertNoMessageErrorContaining("BM_InbondEntryType == 'TIR'", detail.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				detail.Validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageErrorContaining("BM_InbondEntryType != 'TIR'", detail.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

				detail.BY_FormattedHarmonisedTariff = "12345678";
				AssertNoMessageErrorContaining("BY_FormattedHarmonisedTariff isn't empty", detail.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			}
			);
		}

		public void TestCheckBY_FormattedHarmonisedTariff_Mandatory_Has830PreviousDocument()
		{
			movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			CombineAssertions(() =>
			{
				var previousDocument = detail.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = DE.Business.UniversalReferenceConstants.SupportingDocumentTypes.N830;

				detail.Validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageErrorContaining("Has N830 PreviousDocument", detail.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = DE.Business.UniversalReferenceConstants.SupportingDocumentTypes.C034;
				detail.Validation.ValidateBY_HarmonisedTariff();
				AssertNoMessageErrorContaining("Not N830 PreviousDocument", detail.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			}
			);
		}

		public void TestCheckBY_FormattedHarmonisedTariff_Mandatory_BillHas830PreviousDocument()
		{
			movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			CombineAssertions(() =>
			{
				var billPreviousDocument = bill.PreviousDocuments.AddNew();
				billPreviousDocument.CSI_Code = DE.Business.UniversalReferenceConstants.SupportingDocumentTypes.N830;

				detail.Validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageErrorContaining("Has N830 PreviousDocument", detail.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

				billPreviousDocument.CSI_Code = DE.Business.UniversalReferenceConstants.SupportingDocumentTypes.C034;
				detail.Validation.ValidateBY_HarmonisedTariff();
				AssertNoMessageErrorContaining("Not N830 PreviousDocument", detail.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckBY_FormattedHarmonisedTariff_Exists() => CombineAssertions(() =>
		{
			var info = detail.BY_FormattedHarmonisedTariffInfo;
			const string msgError = "The Commodity Code you have entered is not valid for the current context.";

			detail.BY_FormattedHarmonisedTariff = ZString.Empty;
			AssertNoMessageErrorContaining(info, msgError);

			detail.BY_FormattedHarmonisedTariff = "01234567";
			AssertHasMessageErrorContaining("No match at all", info, msgError);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeComodity = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeComodity.PK, "12345678", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			detail.BY_FormattedHarmonisedTariff = tariff1.ZZ1_TariffCode;
			AssertHasMessageErrorContaining("No match on date", info, msgError);

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeComodity.PK, "23456789000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeComodity.PK, "23456789100", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));

			detail.BY_FormattedHarmonisedTariff = "23456789";
			AssertNoMessageErrorContaining("At least one has to start with", info, msgError);
		});

		public void TestCheckBY_TransportChargesMethodOfPayment()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(detail.BY_TransportChargesMethodOfPaymentInfo, "#", TransportChargesModeOfPayment.Codes.Cash);
		}

		public void TestCheckDestinationSanMarinoValidType()
		{
			const string messageError = "Destination Country/Region San Marino requires a Declaration Type of 'T2' or 'T2F'.";
			var targetInfo = detail.BY_TypeInfo;
			CombineAssertions(() =>
			{
				detail.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.SanMarino;
				movementHeader.BM_InBondEntryType = "T1";
				detail.BY_Type = "T2";
				AssertNoMessageError("SanMarino, BM_InBondEntryType=T1, BY_Type=T2", targetInfo, messageError);

				detail.BY_Type = "T2F";
				AssertNoMessageError("SanMarino, BM_InBondEntryType=T1, BY_Type=T2F", targetInfo, messageError);

				detail.BY_Type = "T1";
				AssertHasMessageError("SanMarino, BM_InBondEntryType=T1, BY_Type=T1", targetInfo, messageError);

				movementHeader.BM_InBondEntryType = "T2";
				detail.Validation.ValidateBY_Type();
				AssertNoMessageError("SanMarino, BM_InBondEntryType=T2, BY_Type=T1", targetInfo, messageError);

				movementHeader.BM_InBondEntryType = "T2F";
				detail.Validation.ValidateBY_Type();
				AssertNoMessageError("SanMarino, BM_InBondEntryType=T2F, BY_Type=T1", targetInfo, messageError);

				movementHeader.BM_InBondEntryType = "T1";
				detail.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
				detail.Validation.ValidateBY_Type();
				AssertNoMessageError("Germany, BM_InBondEntryType=T1, BY_Type=T1", targetInfo, messageError);
			});
		}

		public void TestCheckBY_Type_Mandatory_BM_InBondEntryTypeIsT()
		{
			var targetInfo = detail.BY_TypeInfo;
			CombineAssertions(() =>
			{
				movementHeader.BM_InBondEntryType = "T";
				detail.BY_Type = ZString.Empty;
				detail.Validation.ValidateBY_Type();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				movementHeader.BM_InBondEntryType = "T2";
				detail.Validation.ValidateBY_Type();
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = header.MovementHeader;
			bill = header.Bills.AddNew();
			detail = bill.GoodsItems.AddNew();
		}
		NctsHeader header;
		NctsDepartureMovementHeader movementHeader;
		NctsBill bill;
		NctsDepartureCargoDesc detail;
	}
}
