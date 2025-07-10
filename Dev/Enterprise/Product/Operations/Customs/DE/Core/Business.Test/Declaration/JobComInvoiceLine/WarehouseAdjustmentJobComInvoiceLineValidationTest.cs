using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using CustomsUq = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class WarehouseAdjustmentJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_Procedure_SpecificRate()
		{
			const string messageError = "This CPC requires a Charge Code of Type SRC or SRN or SRS.";
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "4900C22";
				AssertNoMessageError("JI_Procedure 4900C22", invoiceLine.JI_ProcedureInfo, messageError);

				invoiceLine.JI_Procedure = "40718E6";
				AssertHasMessageError("JI_Procedure 40718E6", invoiceLine.JI_ProcedureInfo, messageError);

				var charge = invoiceLine.Charges.AddNew();
				charge.J7_ChargeType = ImportChargeCodeList.Codes.SRC;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Has SRC", invoiceLine.JI_ProcedureInfo, messageError);
			});
		}

		public void TestFieldsNotValidated()
		{
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
			invoiceLine.JI_Description = ZString.Empty;
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;

			invoiceLine.JI_LinePrice = ZDecimal.Zero;
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_RH_NKCommodity_Code = ZString.Empty;
			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			invoiceLine.SupplementaryInformation = ZString.Empty;

			invoiceLine.JI_Weight = ZDecimal.Zero;
			invoiceLine.JI_WeightUQ = ZString.Empty;
			invoiceLine.JI_NetWeight = ZDecimal.Zero;
			invoiceLine.JI_NetWeightUQ = ZString.Empty;
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsSecondQuantity = ZDecimal.Zero;
			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
			invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;

			CombineAssertions("Fields shouldn't be validated", () =>
			{
				AssertNoMessageErrors("JI_Tariff", invoiceLine.JI_TariffInfo);
				AssertNoMessageErrors("JI_SupplementaryCode1", invoiceLine.JI_SupplementaryCode1Info);
				AssertNoMessageErrors("JI_SupplementaryCode2", invoiceLine.JI_SupplementaryCode2Info);
				AssertNoMessageErrors("JI_Description", invoiceLine.JI_DescriptionInfo);
				AssertNoMessageErrors("JI_CountryOfOrigin", invoiceLine.JI_CountryOfOriginInfo);
				AssertNoMessageErrors("JI_PrimaryPreference", invoiceLine.JI_PrimaryPreferenceInfo);

				AssertNoMessageErrors("JI_LinePrice", invoiceLine.JI_LinePriceInfo);
				AssertNoMessageErrors("JI_PartNo", invoiceLine.JI_PartNoInfo);
				AssertNoMessageErrors("JI_RH_NKCommodity_Code", invoiceLine.JI_RH_NKCommodity_CodeInfo);
				AssertNoMessageErrors("JI_ConcessionOrder", invoiceLine.JI_ConcessionOrderInfo);
				AssertNoMessageErrors("SupplementaryInformation", invoiceLine.SupplementaryInformationInfo);

				AssertNoMessageErrors("JI_Weight", invoiceLine.JI_WeightInfo);
				AssertNoMessageErrors("JI_WeightUQ", invoiceLine.JI_WeightUQInfo);
				AssertNoMessageErrors("JI_NetWeight", invoiceLine.JI_NetWeightInfo);
				AssertNoMessageErrors("JI_NetWeightUQ", invoiceLine.JI_NetWeightUQInfo);
				AssertNoMessageErrors("JI_CustomsQuantity", invoiceLine.JI_CustomsQuantityInfo);
				AssertNoMessageErrors("JI_CustomsUnitQty", invoiceLine.JI_CustomsUnitQtyInfo);
				AssertNoMessageErrors("JI_CustomsSecondQuantity", invoiceLine.JI_CustomsSecondQuantityInfo);
				AssertNoMessageErrors("JI_CustomsSecondUnitQty", invoiceLine.JI_CustomsSecondUnitQtyInfo);
				AssertNoMessageErrors("JI_CustomsThirdQuantity", invoiceLine.JI_CustomsThirdQuantityInfo);
				AssertNoMessageErrors("JI_CustomsThirdUnitQty", invoiceLine.JI_CustomsThirdUnitQtyInfo);
			});
		}

		public void TestCheckOutwardMRN()
		{
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasMessageErrorContaining("OutwardMRN empty", invoiceLine.OutwardMRNInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.OutwardMRN = "DE00012345";
				AssertNoMessageErrorContaining("OutwardMRN set", invoiceLine.OutwardMRNInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckOutwardDecisiveDate_Mandatory()
		{
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasMessageErrorContaining("DecisiveDate empty", invoiceLine.OutwardDecisiveDateInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.OutwardDecisiveDate = new ZDateTime(2020, 01, 01);
				AssertNoMessageErrorContaining("DecisiveDate set", invoiceLine.OutwardDecisiveDateInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		[TestDate(2021, 01, 12, 10, 21, 59)]
		public void TestCheckOutwardDecisiveDate_NotLaterThanToday()
		{
			const string message = "Decisive Date cannot be greater than current date.";
			CombineAssertions(() =>
			{
				invoiceLine.OutwardDecisiveDate = new ZDateTime(2021, 01, 13);
				AssertHasMessageError("After today", invoiceLine.OutwardDecisiveDateInfo, message);

				invoiceLine.OutwardDecisiveDate = new ZDateTime(2021, 01, 12, 23, 59, 59);
				AssertNoMessageError("Same day as today, ignoring time component", invoiceLine.OutwardDecisiveDateInfo, message);

				invoiceLine.OutwardDecisiveDate = new ZDateTime(2021, 01, 11);
				AssertNoMessageError("Before today", invoiceLine.OutwardDecisiveDateInfo, message);
			});
		}

		public void TestCheckJI_BondedWhsQuantity_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_BondedWhsQuantityInfo, MandatoryValidation.YouHaveNotEnteredMessage("Outward Qty."));
		}

		public void TestCheckJI_BondedWhsQuantity_Negative()
		{
			const string message = "Outward Qty. cannot be negative.";
			var targetInfo = invoiceLine.JI_BondedWhsQuantityInfo;
			CombineAssertions(() =>
			{
				foreach (var procedureCode in new[]
				{
					CustomsProcedureCodeList.Import.ProcedureCode._14,
					CustomsProcedureCodeList.Import.ProcedureCode._15,
					CustomsProcedureCodeList.Import.ProcedureCode._16,
					CustomsProcedureCodeList.Import.ProcedureCode._17
				})
				{
					invoiceLine.JI_Procedure = $"{procedureCode}00C07";
					invoiceLine.JI_BondedWhsQuantity = -1;
					AssertNoMessageError($"JI_Procedure starts with '{procedureCode}', negative quantity", targetInfo, message);
				}

				invoiceLine.JI_Procedure = "4200C07";
				invoiceLine.JI_BondedWhsQuantity = -1;
				AssertHasMessageError("JI_Procedure not starts with ('14', '15', '16', '17'), negative quantity", targetInfo, message);
				invoiceLine.JI_BondedWhsQuantity = 1;
				AssertNoMessageError("JI_Procedure not starts with ('14', '15', '16', '17'), positive quantity", targetInfo, message);
			});
		}

		public void TestCheckJI_BondedWhsQuantity_NotInteger()
		{
			const string message = "Outward Qty. must be integer.";
			var targetInfo = invoiceLine.JI_BondedWhsQuantityInfo;
			CombineAssertions(() =>
			{
				invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_BondedWhsQuantity = 1.5m;
				AssertNoMessageError("JI_BondedWhsUnitQty not in ('NAR', 'NCL', 'NPR'), decimal quantity", targetInfo, message);

				foreach (var unit in new[] { CustomsUq.Number.NumberOfItems, CustomsUq.Number.NumberOfCells, CustomsUq.Number.NumberOfPairs })
				{
					invoiceLine.JI_BondedWhsUnitQty = unit;
					invoiceLine.JI_BondedWhsQuantity = 1.5m;
					AssertHasMessageError($"JI_BondedWhsUnitQty '{unit}', decimal quantity", targetInfo, message);
					invoiceLine.JI_BondedWhsQuantity = 1;
					AssertNoMessageError($"JI_BondedWhsUnitQty '{unit}', integer quantity", targetInfo, message);
				}
			});
		}

		public void TestCheckJI_BondedWhsUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "DE", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			CombineAssertions(() =>
			{
				var targetInfo = invoiceLine.JI_BondedWhsUnitQtyInfo;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage("Outward Qty. Unit"));
				ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XYZ", "KGM");
			});
		}

		public void TestCheckJI_PreviousEntryNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryNumberInfo);
		}

		public void TestCheckJI_PreviousEntryNumber_Format()
		{
			var targetInfo = invoiceLine.JI_PreviousEntryNumberInfo;
			DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("", targetInfo);
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			var message = "Inbound Reg. Pos. cannot be zero.";
			var targetInfo = invoiceLine.JI_PreviousEntryLineNumberInfo;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
				AssertHasMessageError("JI_PreviousEntryLineNumber is empty", targetInfo, message);

				invoiceLine.JI_PreviousEntryLineNumber = 1;
				AssertNoMessageError("JI_PreviousEntryLineNumber is positive", targetInfo, message);

				invoiceLine.JI_PreviousEntryLineNumber = -1;
				AssertNoMessageError("JI_PreviousEntryLineNumber is negative", targetInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
	}
}
