using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class DpsConfidenceThresholdsValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation

		public void TestValidateMediumThresholdRange()
		{
			var expectedError = $"Threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMinimum} and {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum} inclusive.";

			var confidenceThresholds1 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMinimum - 1 };
			var confidenceThresholds2 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum + 1 };
			var confidenceThresholds3 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum - 1 };

			CombineAssertions(() =>
			{
				AssertHasError("Less than minimum", confidenceThresholds1.MediumThresholdInfo, expectedError);
				AssertHasError("Greater than maximum", confidenceThresholds2.MediumThresholdInfo, expectedError);
				AssertNoError("Within bounds", confidenceThresholds3.MediumThresholdInfo, expectedError);
			});
		}

		public void TestValidateHighThresholdRange()
		{
			var expectedError = $"Threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMinimum} and {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum} inclusive.";

			var confidenceThresholds1 = new DpsConfidenceThresholdsBusinessObject { HighThreshold = DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMinimum - 1 };
			var confidenceThresholds2 = new DpsConfidenceThresholdsBusinessObject { HighThreshold = DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum + 1 };
			var confidenceThresholds3 = new DpsConfidenceThresholdsBusinessObject { HighThreshold = DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum - 1 };

			CombineAssertions(() =>
			{
				AssertHasError("Less than minimum", confidenceThresholds1.HighThresholdInfo, expectedError);
				AssertHasError("Greater than maximum", confidenceThresholds2.HighThresholdInfo, expectedError);
				AssertNoError("Within bounds", confidenceThresholds3.HighThresholdInfo, expectedError);
			});
		}

		public void TestValidateThresholdOrdering()
		{
			const int testValue = 80;

			var expectedMediumError = $"Medium confidence threshold must be at least {DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum}% less than the high confidence threshold.";
			var expectedHighError = $"High confidence threshold must be at least {DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum}% greater than the medium confidence threshold.";

			var confidenceThresholds_IncorrectOrder = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = testValue + 1, HighThreshold = testValue };
			var confidenceThresholds_ExactlyEqual = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = testValue, HighThreshold = testValue };
			var confidenceThresholds_NotEnoughDifference = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = testValue, HighThreshold = testValue + DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum - 1 };
			var confidenceThresholds_ExactlyEnoughDifference = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = testValue, HighThreshold = testValue + DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum };
			var confidenceThresholds_MoreThanEnoughDifference = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = testValue, HighThreshold = testValue + DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum + 1 };

			CombineAssertions(() =>
			{
				AssertHasError("Medium greater than High", confidenceThresholds_IncorrectOrder.MediumThresholdInfo, expectedMediumError);
				AssertHasError("High less than low", confidenceThresholds_IncorrectOrder.HighThresholdInfo, expectedHighError);

				AssertHasError("Medium equal to High", confidenceThresholds_ExactlyEqual.MediumThresholdInfo, expectedMediumError);
				AssertHasError("High equal to medium", confidenceThresholds_ExactlyEqual.HighThresholdInfo, expectedHighError);

				AssertHasError("Medium not low enough", confidenceThresholds_NotEnoughDifference.MediumThresholdInfo, expectedMediumError);
				AssertHasError("High not large enough", confidenceThresholds_NotEnoughDifference.HighThresholdInfo, expectedHighError);

				AssertNoErrors("Exactly enough difference", confidenceThresholds_ExactlyEnoughDifference.MediumThresholdInfo);
				AssertNoErrors("Exactly enough difference", confidenceThresholds_ExactlyEnoughDifference.HighThresholdInfo);

				AssertNoErrors("Enough difference", confidenceThresholds_MoreThanEnoughDifference.MediumThresholdInfo);
				AssertNoErrors("Enough difference", confidenceThresholds_MoreThanEnoughDifference.HighThresholdInfo);
			});
		}

		#endregion

	}
}
