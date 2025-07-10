using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNDeclarationDeadlineWarningThresholdValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll_RowError()
		{
			CombineAssertions(() =>
			{
				deadlineWarning.Validation.ValidateAll();
				AssertHasRowError("RowData", deadlineWarning, "The three thresholds must be non-negative and not all zero.");
				AssertHasErrorContaining("TransportMode", deadlineWarning.TransportModeInfo, MandatoryValidation.MustBeEntered);
				AssertEquals("FirstLevelThreshold", true, !deadlineWarning.FirstLevelThresholdInfo.Notifications.HasErrors());
				AssertEquals("FirstLevelWarningColor", true, !deadlineWarning.FirstLevelWarningColorInfo.Notifications.HasErrors());
				AssertEquals("SecondLevelThreshold", true, !deadlineWarning.SecondLevelThresholdInfo.Notifications.HasErrors());
				AssertEquals("SecondLevelWarningColor", true, !deadlineWarning.SecondLevelWarningColorInfo.Notifications.HasErrors());
				AssertEquals("ThirdLevelThreshold", true, !deadlineWarning.ThirdLevelThresholdInfo.Notifications.HasErrors());
				AssertEquals("ThirdLevelWarningColor", true, !deadlineWarning.ThirdLevelWarningColorInfo.Notifications.HasErrors());
			});
		}

		public void TestValidateAll_NoRowError()
		{
			deadlineWarning.FirstLevelThreshold = 16;
			deadlineWarning.SecondLevelThreshold = 16;
			deadlineWarning.ThirdLevelThreshold = 16;
			CombineAssertions(() =>
			{
				deadlineWarning.Validation.ValidateAll();
				AssertNoRowError("RowData", deadlineWarning, "The three thresholds must be non-negative and not all zero.");
				AssertHasErrorContaining("TransportMode", deadlineWarning.TransportModeInfo, MandatoryValidation.MustBeEntered);
				AssertHasError("FirstLevelThreshold", deadlineWarning.FirstLevelThresholdInfo, "1st Level Threshold should be less than or equal to 15.");
				AssertHasErrorContaining("FirstLevelWarningColor", deadlineWarning.FirstLevelWarningColorInfo, MandatoryValidation.MustBeEntered);
				AssertHasError("SecondLevelThreshold", deadlineWarning.SecondLevelThresholdInfo, "2nd Level Threshold should be less than or equal to 15.");
				AssertHasErrorContaining("SecondLevelWarningColor", deadlineWarning.SecondLevelWarningColorInfo, MandatoryValidation.MustBeEntered);
				AssertHasError("ThirdLevelThreshold", deadlineWarning.ThirdLevelThresholdInfo, "3rd Level Threshold should be between 2nd level threshold and 15.");
				AssertHasErrorContaining("ThirdLevelWarningColor", deadlineWarning.ThirdLevelWarningColorInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestValidateTransportMode_CheckEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(deadlineWarning.TransportModeInfo);
		}

		public void TestValidateTransportMode_ErrorIfInvalidCode()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(deadlineWarning.TransportModeInfo, "~", "ALL");
		}

		public void TestValidateTransportMode_IsUnique()
		{
			const string message = "must be unique";
			CombineAssertions(() =>
			{
				deadlineWarning.TransportMode = "ALL";
				var deadlineWarning2 = collection.AddNew();
				deadlineWarning2.TransportMode = "ALL";
				AssertHasErrorContaining("Duplicate", deadlineWarning2.TransportModeInfo, message);
				deadlineWarning2.TransportMode = "AIR";
				AssertNoErrorContaining("Unique", deadlineWarning2.TransportModeInfo, message);
			});
		}

		public void TestValidateFirstLevelThreshold_CheckNotNegative()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(deadlineWarning.FirstLevelThresholdInfo);
		}

		public void TestValidateFirstLevelThresholdInfo_Between0And2ndLevelThreshold()
		{
			const string message = "1st Level Threshold should be between 0 and 2nd level threshold.";
			CombineAssertions(() =>
			{
				deadlineWarning.SecondLevelThreshold = 3;
				deadlineWarning.FirstLevelThreshold = 4;

				AssertHasError("SecondLevelThreshold < FirstLevelThreshold", deadlineWarning.FirstLevelThresholdInfo, message);

				deadlineWarning.FirstLevelThreshold = 3;

				AssertHasError("SecondLevelThreshold = FirstLevelThreshold", deadlineWarning.FirstLevelThresholdInfo, message);

				deadlineWarning.FirstLevelThreshold = 2;
				AssertNoError("SecondLevelThreshold > FirstLevelThreshold", deadlineWarning.FirstLevelThresholdInfo, message);
			});
		}

		public void TestValidateFirstLevelThresholdInfo_LessThanOrEqualTo15()
		{
			const string message = "1st Level Threshold should be less than or equal to 15.";
			CombineAssertions(() =>
			{
				deadlineWarning.FirstLevelThreshold = 16;
				AssertHasError(">15", deadlineWarning.FirstLevelThresholdInfo, message);

				deadlineWarning.FirstLevelThreshold = 15;
				AssertNoError("<=15", deadlineWarning.FirstLevelThresholdInfo, message);
			});
		}

		public void TestValidateFirstLevelWarningColor_CheckEntered_IfFirstLevelThresholdEntered()
		{
			deadlineWarning.FirstLevelThreshold = 1;
			ValidationTestHelper.AssertErrorIfNotEntered(deadlineWarning.FirstLevelWarningColorInfo);
		}

		public void TestValidateFirstLevelWarningColor_CheckEntered_IfFirstLevelThresholdNotEntered()
		{
			deadlineWarning.FirstLevelThreshold = 0;
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(deadlineWarning.FirstLevelWarningColorInfo);
		}

		public void TestValidateFirstLevelWarningColor()
		{
			AssertValidRGBValue(deadlineWarning.FirstLevelWarningColorInfo);
		}

		public void TestValidateSecondLevelThreshold_CheckNotNegative()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(deadlineWarning.SecondLevelThresholdInfo);
		}

		public void TestValidateSecondLevelThreshold_Between1And3rdLevelThreshold()
		{
			const string message = "2nd Level Threshold should be between 1st and 3rd level threshold.";
			CombineAssertions(() =>
			{
				deadlineWarning.FirstLevelThreshold = 2;
				deadlineWarning.ThirdLevelThreshold = 4;

				deadlineWarning.SecondLevelThreshold = 1;
				AssertHasError("SecondLevelThreshold < FirstLevelThreshold", deadlineWarning.SecondLevelThresholdInfo, message);

				deadlineWarning.SecondLevelThreshold = 2;

				AssertHasError("SecondLevelThreshold = FirstLevelThreshold", deadlineWarning.SecondLevelThresholdInfo, message);

				deadlineWarning.SecondLevelThreshold = 4;
				AssertHasError("SecondLevelThreshold = ThirdLevelThreshold", deadlineWarning.SecondLevelThresholdInfo, message);

				deadlineWarning.SecondLevelThreshold = 5;
				AssertHasError("SecondLevelThreshold > ThirdLevelThreshold", deadlineWarning.SecondLevelThresholdInfo, message);

				deadlineWarning.SecondLevelThreshold = 3;
				AssertNoError("(FirstLevelThreshold, ThirdLevelThreshold)", deadlineWarning.SecondLevelThresholdInfo, message);
			});
		}

		public void TestValidateSecondLevelThreshold_LessThanOrEqualTo15()
		{
			const string message = "2nd Level Threshold should be less than or equal to 15.";
			CombineAssertions(() =>
			{
				deadlineWarning.SecondLevelThreshold = 16;
				AssertHasError(">15", deadlineWarning.SecondLevelThresholdInfo, message);

				deadlineWarning.SecondLevelThreshold = 15;
				AssertNoError("<=15", deadlineWarning.SecondLevelThresholdInfo, message);
			});
		}

		public void TestValidateSecondLevelWarningColor_CheckEntered_IfSecondLevelThresholdEntered()
		{
			deadlineWarning.SecondLevelThreshold = 1;
			ValidationTestHelper.AssertErrorIfNotEntered(deadlineWarning.SecondLevelWarningColorInfo);
		}

		public void TestValidateSecondLevelWarningColor_CheckEntered_IfSecondLevelThresholdNotEntered()
		{
			deadlineWarning.SecondLevelThreshold = 0;
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(deadlineWarning.SecondLevelWarningColorInfo);
		}

		public void TestValidateSecondLevelWarningColor()
		{
			AssertValidRGBValue(deadlineWarning.SecondLevelWarningColorInfo);
		}

		public void TestValidateThirdLevelThreshold_CheckNotNegative()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(deadlineWarning.ThirdLevelThresholdInfo);
		}

		public void TestValidateThirdLevelThreshold_Between2ndLevelThresholdAnd15()
		{
			const string message = "3rd Level Threshold should be between 2nd level threshold and 15.";
			CombineAssertions(() =>
			{
				deadlineWarning.SecondLevelThreshold = 2;

				deadlineWarning.ThirdLevelThreshold = 1;
				AssertHasError("ThirdLevelThreshold < SecondLevelThreshold", deadlineWarning.ThirdLevelThresholdInfo, message);

				deadlineWarning.ThirdLevelThreshold = 2;

				AssertHasError("ThirdLevelThreshold = SecondLevelThreshold", deadlineWarning.ThirdLevelThresholdInfo, message);

				deadlineWarning.ThirdLevelThreshold = 16;
				AssertHasError("ThirdLevelThreshold > 16", deadlineWarning.ThirdLevelThresholdInfo, message);

				deadlineWarning.ThirdLevelThreshold = 15;
				AssertNoError("(SecondLevelThreshold, 16)", deadlineWarning.ThirdLevelThresholdInfo, message);
			});
		}

		public void TestValidateThirdLevelThreshold_GreaterThan1stLevelThreshold()
		{
			const string message = "3rd Level Threshold should be greater than 1st level threshold.";
			CombineAssertions(() =>
			{
				deadlineWarning.FirstLevelThreshold = 2;

				deadlineWarning.ThirdLevelThreshold = 1;
				AssertHasError("ThirdLevelThreshold < FirstLevelThreshold", deadlineWarning.ThirdLevelThresholdInfo, message);

				deadlineWarning.ThirdLevelThreshold = 2;
				AssertHasError("ThirdLevelThreshold = FirstLevelThreshold", deadlineWarning.ThirdLevelThresholdInfo, message);

				deadlineWarning.ThirdLevelThreshold = 3;
				AssertNoError("ThirdLevelThreshold > FirstLevelThreshold", deadlineWarning.ThirdLevelThresholdInfo, message);
			});
		}

		public void TestValidateThirdLevelWarningColor_CheckEntered_IfThirdLevelThresholdEntered()
		{
			deadlineWarning.ThirdLevelThreshold = 1;
			ValidationTestHelper.AssertErrorIfNotEntered(deadlineWarning.ThirdLevelWarningColorInfo);
		}

		public void TestValidateThirdLevelWarningColor_CheckEntered_IfThirdLevelThresholdNotEntered()
		{
			deadlineWarning.ThirdLevelThreshold = 0;
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(deadlineWarning.ThirdLevelWarningColorInfo);
		}

		public void TestValidateThirdLevelWarningColor()
		{
			AssertValidRGBValue(deadlineWarning.ThirdLevelWarningColorInfo);
		}

		public void TestValidateDelayedWarningColor()
		{
			AssertValidRGBValue(deadlineWarning.DelayedWarningColorInfo);
		}

		public void TestValidateRowData()
		{
			const string message = "The three thresholds must be non-negative and not all zero.";
			CombineAssertions(() =>
			{
				deadlineWarning.FirstLevelThreshold = 0;
				deadlineWarning.SecondLevelThreshold = 0;
				deadlineWarning.ThirdLevelThreshold = 0;
				deadlineWarning.Validation.ValidateRowData();
				AssertHasRowError("All are empty", deadlineWarning, message);

				deadlineWarning.FirstLevelThreshold = 1;
				deadlineWarning.Validation.ValidateRowData();
				AssertNoRowError("Not all empty", deadlineWarning, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			collection = new CNDeclarationDeadlineWarningThresholdCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			deadlineWarning = collection.AddNew();
		}
		CNDeclarationDeadlineWarningThreshold deadlineWarning;
		CNDeclarationDeadlineWarningThresholdCollection collection;

		void AssertValidRGBValue(ZPropertyInfo property)
		{
			const string rgbFormat = "RGB value should be in the format of R,G,B";
			const string invalidColor = "Invalid color";
			CombineAssertions(() =>
			{
				property.Value = (ZString)"256,111,1,1";
				AssertHasError("More than 2 separators", property, rgbFormat);

				property.Value = (ZString)"12345678901";
				AssertHasError("No separator", property, rgbFormat);

				property.Value = (ZString)"2561,1111,0";
				AssertNoError("Correct format but invalid color", property, rgbFormat);
				AssertHasError("Invalid color", property, invalidColor);

				property.Value = (ZString)"250,250,0";
				AssertNoError("Valid", deadlineWarning.FirstLevelWarningColorInfo, invalidColor);
			});
		}
	}
}
