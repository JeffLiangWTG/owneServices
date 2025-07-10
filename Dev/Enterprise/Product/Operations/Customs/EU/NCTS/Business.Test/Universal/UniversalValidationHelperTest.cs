using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class UniversalValidationHelperTest : TestCaseWithFactory
	{
		public void TestCheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code()
		{
			const string errorMessage = "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties).";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			var dummyBO = Factory.New<DummyBusinessObject>();
			var propertyInfo = dummyBO.Z0_DescriptionInfo;
			using (dummyBO.SuspendValidationTesting())
			{
				CombineAssertions(() =>
				{
					var china = Core.Constants.CountryCodes.China;
					var france = Core.Constants.CountryCodes.France;
					UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(Factory, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure, france, china, Core.Constants.CountryCodes.Latvia, propertyInfo);
					AssertNoMessageErrorContaining("T2 Declaration Type, Country Of Dispatch is in List 9", propertyInfo, errorMessage);
					propertyInfo.ClearAllNotifications();

					UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(Factory, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure, china, france, Core.Constants.CountryCodes.Latvia, propertyInfo);
					AssertNoMessageErrorContaining("T2 Declaration Type, Country Of Destination is in List 9", propertyInfo, errorMessage);
					propertyInfo.ClearAllNotifications();

					UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(Factory, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure, china, china, Core.Constants.CountryCodes.Latvia, propertyInfo);
					AssertHasMessageErrorContaining("T2 Declaration, both countries not in list", propertyInfo, errorMessage);
					propertyInfo.ClearAllNotifications();

					UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(Factory, NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4, china, china, Core.Constants.CountryCodes.Latvia, propertyInfo);
					AssertNoMessageErrorContaining("Not T2 Declaration Type", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckNoLowerCaseLetters()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			var propertyInfo = dummyBO.Z0_DescriptionInfo;

			using (dummyBO.SuspendValidationTesting())
			{
				CombineAssertions(() =>
				{
					propertyInfo.Value = (ZString)"ABC123";
					UniversalValidationHelper.CheckNoLowerCaseLetters(propertyInfo);
					AssertNoMessageErrors("No lowercase letters", propertyInfo);

					propertyInfo.Value = (ZString)"abc123";
					UniversalValidationHelper.CheckNoLowerCaseLetters(propertyInfo);
					AssertHasMessageError("Contains lowercase letters", propertyInfo, "Must not contain lower case letters.");
				});
			}
		}

		public void TestIsDocumentTypeIsMRN()
		{
			AssertEquals("Type is in the list [ N820, N821, N822, N830]", true, UniversalValidationHelper.IsDocumentTypeIsMRN(NctsConstants.NctsTypeOfPreviousDocument.Codes.N820));
			AssertEquals("Type is in the list [ N820, N821, N822, N830]", true, UniversalValidationHelper.IsDocumentTypeIsMRN(NctsConstants.NctsTypeOfPreviousDocument.Codes.N821));
			AssertEquals("Type is in the list [ N820, N821, N822, N830]", true, UniversalValidationHelper.IsDocumentTypeIsMRN(NctsConstants.NctsTypeOfPreviousDocument.Codes.N822));
			AssertEquals("Type is in the list [ N820, N821, N822, N830]", true, UniversalValidationHelper.IsDocumentTypeIsMRN(NctsConstants.NctsTypeOfPreviousDocument.Codes.N830));

			AssertEquals("Type is NOT in the list [ N820, N821, N822, N830]", false, UniversalValidationHelper.IsDocumentTypeIsMRN(NctsConstants.NctsTypeOfPreviousDocument.Codes.C651));
		}

		public static void AssertMaxLengthDuringTransitionPeriod(ZPropertyInfo propertyInfo, int maxLength, string rulePrefix = "")
		{
			var message = $"{rulePrefix}Length of {propertyInfo.HumanReadableName} must not exceed {maxLength} characters.";

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, true))
			{
				propertyInfo.Value = new ZString('X', maxLength + 1);
				AssertHasMessageError($"When {propertyInfo.HumanReadableName} is {maxLength + 1} characters in Transition Period", propertyInfo, message);
				propertyInfo.Value = new ZString('X', maxLength);
				AssertNoMessageError($"When {propertyInfo.HumanReadableName} is {maxLength} characters in Transition Period", propertyInfo, message);
			}
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, false))
			{
				propertyInfo.Value = new ZString('X', maxLength + 1);
				AssertNoMessageError($"When {propertyInfo.HumanReadableName} is {maxLength + 1} characters not in Transition Period", propertyInfo, message);
			}
		}

		public static void AssertMaxLengthForPhase5AndTransitionPeriod(ZPropertyInfo propertyInfo, int maxLengthPhase5, int maxLengthPhase5TP)
		{
			AssertMaxLength(propertyInfo, maxLengthPhase5);
			AssertMaxLengthDuringTransitionPeriod(propertyInfo, maxLengthPhase5TP);
		}

		public static void AssertMaxLength(ZPropertyInfo propertyInfo, int maxLength)
		{
			var message = $"Length of {propertyInfo.HumanReadableName} must not exceed {maxLength} characters.";

			propertyInfo.Value = new ZString('X', maxLength + 1);
			AssertHasMessageError($"MaxLength exceeded", propertyInfo, message);
			propertyInfo.Value = new ZString('X', maxLength);
			AssertNoMessageError($"MaxLength not exceeded", propertyInfo, message);
		}

		public static void AssertMaxLengthE1103(NctsHeader nctsHeader, ZPropertyInfo info, bool isBMInlandTransportMode, params string[] list)
		{
			const string message35 = "Maximum allowed length is 35";
			const string message27 = "Maximum allowed length is 27";

			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var departureMovement = nctsHeader.MovementHeader;

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, true))
			{
				if (isBMInlandTransportMode)
				{
					departureMovement.BM_InlandTransportMode = list.Last();
					AssertEquals(message35, 35, info.MaxLength);

					var rightNumOfBMInlandTransportMode = list.Length - 1;
					for (int i = 0; i < rightNumOfBMInlandTransportMode; i++)
					{
						departureMovement.BM_InlandTransportMode = list[i];
						AssertEquals(message27, 27, info.MaxLength);
					}
				}

				AssertEquals(message27, 27, info.MaxLength);
			}

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, false))
			{
				if (isBMInlandTransportMode)
				{
					departureMovement.BM_InlandTransportMode = list.Last();
					AssertEquals(message35, 35, info.MaxLength);

					var rightNumOfBMInlandTransportMode = list.Length - 1;
					for (int i = 0; i < rightNumOfBMInlandTransportMode; i++)
					{
						departureMovement.BM_InlandTransportMode = list[i];
						AssertEquals(message35, 35, info.MaxLength);
					}
				}

				AssertEquals(message35, 35, info.MaxLength);
			}
		}

		public static void AssertCheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(ZPropertyInfo propertyInfo, string message)
		{
			CombineAssertions(() =>
			{
				using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, true))
				{
					propertyInfo.Value = new ZDecimal(1234567890.1);
					AssertNoMessageErrorContaining("Length of Max value is not exceeding 11, max length of decimal is not exceeding 3", propertyInfo, message);

					propertyInfo.Value = new ZDecimal(1234567890.12);
					AssertHasMessageErrorContaining("Length of Max value is exceeding 11, max length of decimal is not exceeding 3", propertyInfo, message);

					propertyInfo.Value = new ZDecimal(12345678901.1);
					AssertHasMessageErrorContaining("Length of Max value is exceeding 11, max length of decimal is not exceeding 3", propertyInfo, message);

					propertyInfo.Value = new ZDecimal(123456789.12);
					AssertNoMessageErrorContaining("Length of Max value is not exceeding 11, max length of decimal is not exceeding 3", propertyInfo, message);

					propertyInfo.Value = new ZDecimal(12345678.123);
					AssertNoMessageErrorContaining("Length of Max value is not exceeding 11, max length of decimal is not exceeding 3", propertyInfo, message);

					propertyInfo.Value = new ZDecimal(1234567.1234);
					AssertHasMessageErrorContaining("Length of Max value is not exceeding 11, max length of decimal is exceeding 3", propertyInfo, message);
				}
				using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, false))
				{
					propertyInfo.Value = new ZDecimal(12345678.1234);
					AssertNoMessageErrorContaining("Length of Max value is exceeding 11, max length of decimal is exceeding 3, but is not Inside Transition Period", propertyInfo, message);
				}
			});
		}
	}
}
