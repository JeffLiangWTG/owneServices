using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	public sealed class PreviousDocumentHelperTest : TestCaseWithFactory
	{
		public void TestGetCachedSubTypeList_ATNEU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var airCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE185104", "SHYZAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var airAttribute = helper.CreateCusCodeListAttribute(airCodeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
			helper.CreateTransportModeForCusCodeAttribute(airAttribute.PK, RefTransportModeList.Codes.AIR);
			Factory.Save();
			var list = Factory.GetCachedSubTypeList_ATNEU("DE185104");
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "AWB, REG, ULD", list.CodesAsString);
				AssertSame("Cached", list, Factory.GetCachedSubTypeList_ATNEU("DE185104"));
			});
		}

		public void TestGetCachedSubTypeList_ATNEU_Empty()
		{
			var list = Factory.GetCachedSubTypeList_ATNEU(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "REG", list.CodesAsString);
				AssertSame("Cached", list, Factory.GetCachedSubTypeList_ATNEU(ZString.Empty));
			});
		}

		public void TestGetCachedSubTypeList_ATNEU_OfficeIsNotAIR()
		{
			var list = Factory.GetCachedSubTypeList_ATNEU("DE185104");
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "REG", list.CodesAsString);
				AssertSame("Cached", list, Factory.GetCachedSubTypeList_ATNEU("DE185104"));
			});
		}

		public void TestPreviousProceduresRequiringReference()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[] {
				PreviousProcedureList.Codes._ATA,
				PreviousProcedureList.Codes._ESUMA,
				PreviousProcedureList.Codes._GB,
				PreviousProcedureList.Codes._PUEB,
				PreviousProcedureList.Codes._T1,
				PreviousProcedureList.Codes._T2,
				PreviousProcedureList.Codes._TIR,
				PreviousProcedureList.Codes._VO
			}, PreviousDocumentHelper.PreviousProceduresRequiringReference);
		}

		public void TestNeeds21CharactersReference_Import()
		{
			CombineAssertions(() =>
			{
				foreach (var procedure in new PreviousProcedureList().GetAllCodes())
				{
					previousDocument.CSI_Procedure = procedure;
					if (procedure == PreviousProcedureList.Codes._ATNEU)
					{
						foreach (var subType in new PreviousDocSubTypeList().GetAllCodes())
						{
							previousDocument.CSI_SubType = subType;
							AssertEquals($"ATNEU, CSI_SubType {subType}", subType == PreviousDocSubTypeList.Codes.REG, previousDocument.Requires18Or21CharactersReference());
						}
					}
					else if (procedure == PreviousProcedureList.Codes._ATAV || procedure == PreviousProcedureList.Codes._ATZL)
					{
						foreach (var status in new[] { true, false })
						{
							previousDocument.Status = status;
							AssertEquals($"{procedure}, CSI_Status {previousDocument.CSI_Status}", status, previousDocument.Requires18Or21CharactersReference());
						}
					}
					else
					{
						AssertEquals($"{procedure}", false, previousDocument.Requires18Or21CharactersReference());
					}
				}
			});
		}

		public void TestNeeds21CharactersReference_Export()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				foreach (var procedure in PreviousDocumentLookups.GetProcedureList(false).GetAllCodes())
				{
					previousDocument.CSI_Procedure = procedure;
					AssertEquals($"{procedure}", false, previousDocument.Requires18Or21CharactersReference());
				}
			});
		}

		public void TestIsValidAtlasReferenceForBondedWarehouse()
		{
			AssertIsValidAtlasReferenceForBondedWarehouse(PreviousDocumentHelper.IsValidAtlasReferenceForBondedWarehouse);
		}

		public static void AssertIsValidAtlasReferenceForBondedWarehouse(Func<ZString,bool> isValidAtlasReferenceForBondedWarehouse, bool expectAutomaticalTruncationWhenPopulatingCSI_ReferenceNumber = false)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid, 3rd character C", expected: true, isValidAtlasReferenceForBondedWarehouse("ATC710123456789012345"));
				AssertEquals("Valid, 3rd character D", expected: true, isValidAtlasReferenceForBondedWarehouse("ATD710123456789012345"));
				AssertEquals("Valid, 3rd character E", expected: true, isValidAtlasReferenceForBondedWarehouse("ATE710123456789012345"));
				AssertEquals("Valid, 3rd character H", expected: true, isValidAtlasReferenceForBondedWarehouse("ATH710123456789012345"));
				AssertEquals("Valid, 3rd character T", expected: true, isValidAtlasReferenceForBondedWarehouse("ATT710123456789012345"));
				AssertEquals("Doesn't start with AT", expected: false, isValidAtlasReferenceForBondedWarehouse("ASC710123456789012345"));
				AssertEquals("3rd char not in ('C', 'D', 'E', 'H', 'T')", expected: false, isValidAtlasReferenceForBondedWarehouse("ATA710123456789012345"));
				AssertEquals("4-5 chars not '71'", expected: false, isValidAtlasReferenceForBondedWarehouse("ATC720123456789012345"));
				AssertEquals("Doesn't end with 16 digits", expected: false, isValidAtlasReferenceForBondedWarehouse("ATC71012345678901234A"));
				AssertEquals("Shorter than 21 chars",	expected: false, isValidAtlasReferenceForBondedWarehouse("ATC71012345678901234"));
				AssertEquals("Longer than 21 chars", expected: expectAutomaticalTruncationWhenPopulatingCSI_ReferenceNumber, isValidAtlasReferenceForBondedWarehouse("ATC7101234567890123456"));
			});
		}

		public void TestIsValidAtlasReferenceForBondedWarehouse_MRN()
		{
			AssertIsValidAtlasReferenceForBondedWarehouse_MRN(PreviousDocumentHelper.IsValidAtlasReferenceForBondedWarehouse);
		}

		public static void AssertIsValidAtlasReferenceForBondedWarehouse_MRN(Func<ZString, bool> isValidAtlasReferenceForBondedWarehouse)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid 10 - C", true, isValidAtlasReferenceForBondedWarehouse("24DE5875GCM0002XR7"));
				AssertEquals("Valid 10 - D", true, isValidAtlasReferenceForBondedWarehouse("24DE5875GDM0002XR2"));
				AssertEquals("Valid 10 - E", true, isValidAtlasReferenceForBondedWarehouse("24DE5875GEM0002XR8"));
				AssertEquals("Valid 10 - H", true, isValidAtlasReferenceForBondedWarehouse("24DE5875GHM0002XR4"));
				AssertEquals("Valid 10 - T", true, isValidAtlasReferenceForBondedWarehouse("24DE5875GTM0002XR5"));

				AssertEquals("NotValid - No DE", false, isValidAtlasReferenceForBondedWarehouse("24ES5875GCM0002XR0"));
				AssertEquals("24ES5875GCM0002X0", 0, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24ES5875GCM0002XR0").calculatedCheckDigit);

				AssertEquals("NotValid - 1-2 not digits", false, isValidAtlasReferenceForBondedWarehouse("AADE5875GCM0002XR5"));
				AssertEquals("AADE5875GCM0002XR5", 5, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("AADE5875GCM0002XR5").calculatedCheckDigit);

				AssertEquals("NotValid - 11 not M", false, isValidAtlasReferenceForBondedWarehouse("24DE5875GCN0002XR8"));
				AssertEquals("24DE5875GCN0002XR8", 8, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24DE5875GCN0002XR8").calculatedCheckDigit);

				AssertEquals("NotValid - 10 not in ('C', 'D', 'E', 'H', 'T')", false, isValidAtlasReferenceForBondedWarehouse("24DE5875GPM0002XR3"));
				AssertEquals("24DE5875GPM0002XR3", 3, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24DE5875GPM0002XR3").calculatedCheckDigit);

				AssertEquals("NotValid - wrong check digit", false, isValidAtlasReferenceForBondedWarehouse("24DE5875GCM0002XR6"));
				AssertEquals("24DE5875GCM0002XR6", 7, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24DE5875GCM0002XR6").calculatedCheckDigit);

				AssertEquals("Shorter than 18 chars", false, isValidAtlasReferenceForBondedWarehouse("24DE5875GCM0002X"));
				AssertEquals("Longer than 18 chars", false, isValidAtlasReferenceForBondedWarehouse("24DE5875GCM0002XR711"));
			});
		}

		public void TestIsValidAtlasReferenceForInwardProcessing()
		{
			AssertIsValidAtlasReferenceForInwardProcessing(PreviousDocumentHelper.IsValidAtlasReferenceForInwardProcessing);
		}

		public static void AssertIsValidAtlasReferenceForInwardProcessing(Func<ZString, bool> isValidAtlasReferenceForInwardProcessing, bool expectAutomaticalTruncationWhenPopulatingCSI_ReferenceNumber = false)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid, 3rd character C", expected: true, isValidAtlasReferenceForInwardProcessing("ATC020123456789012345"));
				AssertEquals("Valid, 3rd character D", expected: true, isValidAtlasReferenceForInwardProcessing("ATD020123456789012345"));
				AssertEquals("Valid, 3rd character E", expected: true, isValidAtlasReferenceForInwardProcessing("ATE020123456789012345"));
				AssertEquals("Valid, 3rd character P", expected: true, isValidAtlasReferenceForInwardProcessing("ATP020123456789012345"));
				AssertEquals("Valid, 4-5 chars '41'", expected: true, isValidAtlasReferenceForInwardProcessing("ATC410123456789012345"));
				AssertEquals("Valid, 4-5 chars '51'", expected: true, isValidAtlasReferenceForInwardProcessing("ATC510123456789012345"));
				AssertEquals("Valid, 4-5 chars '91'", expected: true, isValidAtlasReferenceForInwardProcessing("ATC910123456789012345"));
				AssertEquals("Doesn't start with AT", expected: false, isValidAtlasReferenceForInwardProcessing("ASC020123456789012345"));
				AssertEquals("Valid, 3rd char not in ('C', 'D', 'E', 'P')", expected: false, isValidAtlasReferenceForInwardProcessing("ATA020123456789012345"));
				AssertEquals("Valid, 4-5 chars not in ('02', '41', '51', '91')", expected: false, isValidAtlasReferenceForInwardProcessing("ATC040123456789012345"));
				AssertEquals("Doesn't end with 16 digits", expected: false, isValidAtlasReferenceForInwardProcessing("ATC02012345678901234A"));
				AssertEquals("Shorter than 21 chars", expected: false, isValidAtlasReferenceForInwardProcessing("ATC02012345678901234"));
				AssertEquals("Longer than 21 chars", expected: expectAutomaticalTruncationWhenPopulatingCSI_ReferenceNumber, isValidAtlasReferenceForInwardProcessing("ATC0201234567890123456"));
			});
		}

		public void TestIsValidAtlasReferenceForInwardProcessing_MRN()
		{
			AssertIsValidAtlasReferenceForInwardProcessing_MRN(PreviousDocumentHelper.IsValidAtlasReferenceForInwardProcessing);
		}

		public static void AssertIsValidAtlasReferenceForInwardProcessing_MRN(Func<ZString, bool> isValidAtlasReferenceForInwardProcessing)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid 10 - C", true, isValidAtlasReferenceForInwardProcessing("24DE5875GCH0002ER0"));
				AssertEquals("Valid 10 - D", true, isValidAtlasReferenceForInwardProcessing("24DE5875GDH0002ER6"));
				AssertEquals("Valid 10 - E", true, isValidAtlasReferenceForInwardProcessing("24DE5875GEH0002ER1"));
				AssertEquals("Valid 10 - P", true, isValidAtlasReferenceForInwardProcessing("24DE5875GPH0002ER7"));

				AssertEquals("NotValid - No DE", false, isValidAtlasReferenceForInwardProcessing("24ES5875GCH0002ER3"));
				AssertEquals("24ES5875GCH0002ER3", 3, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24ES5875GCH0002ER3").calculatedCheckDigit);

				AssertEquals("NotValid - 1-2 not digits", false, isValidAtlasReferenceForInwardProcessing("AADE5875GCH0002XR6"));
				AssertEquals("AADE5875GCH0002XR0", 0, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("AADE5875GCH0002XR0").calculatedCheckDigit);

				AssertEquals("NotValid - 11 not H", false, isValidAtlasReferenceForInwardProcessing("24DE5875GCN0002XR8"));
				AssertEquals("24DE5875GCN0002XR0", 8, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24DE5875GCN0002XR8").calculatedCheckDigit);

				AssertEquals("NotValid - 10 not in ('C', 'D', 'E', 'P')", false, isValidAtlasReferenceForInwardProcessing("24DE5875G1H0002XR6"));
				AssertEquals("24DE5875G1H0002XR6", 6, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24DE5875G1H0002XR6").calculatedCheckDigit);

				AssertEquals("NotValid - wrong check digit", false, isValidAtlasReferenceForInwardProcessing("24DE5875GCH0002ER1"));
				AssertEquals("24DE5875GCH0002ER1", 0, MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("24DE5875GCH0002ER1").calculatedCheckDigit);

				AssertEquals("Shorter than 18 chars", false, isValidAtlasReferenceForInwardProcessing("24DE5875GCM0002X"));
				AssertEquals("Longer than 18 chars", false, isValidAtlasReferenceForInwardProcessing("24DE5875GCM0002ER011"));
			});
		}

		public void TestMaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLineDuringTransitionPeriod()
		{
			AssertEquals(9, PreviousDocumentHelper.MaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLineDuringTransitionPeriod);
		}

		public void TestMaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLine()
		{
			AssertEquals(99, PreviousDocumentHelper.MaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLine);
		}

		public static string[] UnitOfQuantitiesThatRequireIntegerValues => new string[]
		{
			Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems,
			Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells,
			Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs
		};

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			previousDocument = instruction.PreviousDocuments.AddNew();
		}
		JobDeclaration declaration;
		PreviousDocument previousDocument;
	}
}
