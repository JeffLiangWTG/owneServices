using System;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class DpsConfidenceThresholdsValidation : ZValidation
	{
		public DpsConfidenceThresholdsValidation(DpsConfidenceThresholdsBusinessObject parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DpsConfidenceThresholdsBusinessObject parent;

		public override Type AutoValidationType => typeof(DpsConfidenceThresholdsValidation);

		public override void ValidateAll()
		{
			ValidateHighThreshold();
			ValidateMediumThreshold();
		}

		public void ValidateHighThreshold()
		{
			ValidateCalculatedProperty(parent.HighThresholdInfo);
		}

		protected void CheckHighThreshold()
		{
			ValidateRange(parent.HighThresholdInfo, parent.HighThreshold, DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMinimum, DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum);

			if (parent.HighThreshold < parent.MediumThreshold + DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum)
			{
				parent.HighThresholdInfo.AddError(Res.GetString("03e6c137-5fad-4801-b43e-c85b640716ea",
					"High confidence threshold must be at least {0}% greater than the medium confidence threshold.",
					DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum)
				);
			}
		}

		public void ValidateMediumThreshold()
		{
			ValidateCalculatedProperty(parent.MediumThresholdInfo);
		}

		protected void CheckMediumThreshold()
		{
			ValidateRange(parent.MediumThresholdInfo, parent.MediumThreshold, DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMinimum, DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum);

			if (parent.MediumThreshold > parent.HighThreshold - DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum)
			{
				parent.MediumThresholdInfo.AddError(Res.GetString("44ca4730-91cd-4bc1-b904-65fee6c90f6d",
					"Medium confidence threshold must be at least {0}% less than the high confidence threshold.",
					DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum)
				);
			}
		}

		#region Implementation

		void ValidateRange(ZPropertyInfo propertyInfo, int propertyValue, int lowValue, int highValue)
		{
			if (propertyValue < lowValue || propertyValue > highValue)
			{
				propertyInfo.AddError(Res.GetString("f94dc813-f52f-469c-bb91-7be2031a3be1", "Threshold must be between {0} and {1} inclusive.", lowValue, highValue));
			}
		}

		#endregion
	}
}
