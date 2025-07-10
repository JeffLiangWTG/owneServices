using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CNDeclarationDeadlineWarningThresholdValidation
	{
		public CNDeclarationDeadlineWarningThresholdValidation(CNDeclarationDeadlineWarningThreshold parent)
		{
			this.parent = parent;
		}

		readonly CNDeclarationDeadlineWarningThreshold parent;

		public void ValidateAll()
		{
			ValidateRowData();
			ValidateTransportMode();
			ValidateFirstLevelThresholdInfo();
			ValidateFirstLevelWarningColor();
			ValidateSecondLevelThresholdInfo();
			ValidateSecondLevelWarningColor();
			ValidateThirdLevelThresholdInfo();
			ValidateThirdLevelWarningColor();
			ValidateDelayedWarningColor();
		}

		#region Validate properties
		public void ValidateTransportMode()
		{
			parent.TransportModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(parent.TransportModeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(parent.TransportModeInfo);
		}

		public void ValidateFirstLevelThresholdInfo()
		{
			parent.FirstLevelThresholdInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(parent.FirstLevelThresholdInfo);
			if (parent.FirstLevelThreshold > 0)
			{
				if (parent.SecondLevelThreshold > 0 && parent.SecondLevelThreshold <= parent.FirstLevelThreshold)
				{
					parent.FirstLevelThresholdInfo.AddError(Res.GetString("35E31F58-054B-4489-AEBE-D0EE7F09FE09", "1st Level Threshold should be between 0 and 2nd level threshold."));
				}
				if (parent.FirstLevelThreshold > 15)
				{
					parent.FirstLevelThresholdInfo.AddError(Res.GetString("29A55875-7047-4185-BF16-671F9577FD2E", "1st Level Threshold should be less than or equal to 15."));
				}
			}
			ValidateFirstLevelWarningColor();
		}

		public void ValidateFirstLevelWarningColor()
		{
			parent.FirstLevelWarningColorInfo.ClearAllNotifications();
			ValidateRGBValue(parent.FirstLevelWarningColorInfo);
			if (parent.FirstLevelThreshold > 0)
			{
				MandatoryValidation.CheckEntered(parent.FirstLevelWarningColorInfo);
			}
		}

		public void ValidateSecondLevelThresholdInfo()
		{
			parent.SecondLevelThresholdInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(parent.SecondLevelThresholdInfo);
			if (parent.SecondLevelThreshold > 0)
			{
				if ((parent.ThirdLevelThreshold > 0 && parent.ThirdLevelThreshold <= parent.SecondLevelThreshold) || parent.SecondLevelThreshold <= parent.FirstLevelThreshold)
				{
					parent.SecondLevelThresholdInfo.AddError(Res.GetString("C726810D-84B2-4701-B324-3C5D87C21128", "2nd Level Threshold should be between 1st and 3rd level threshold."));
				}
				if (parent.SecondLevelThreshold > 15)
				{
					parent.SecondLevelThresholdInfo.AddError(Res.GetString("6F03C98F-EEB8-45C9-819A-87CB7265136F", "2nd Level Threshold should be less than or equal to 15."));
				}
			}
			ValidateSecondLevelWarningColor();
		}

		public void ValidateSecondLevelWarningColor()
		{
			parent.SecondLevelWarningColorInfo.ClearAllNotifications();
			ValidateRGBValue(parent.SecondLevelWarningColorInfo);
			if (parent.SecondLevelThreshold > 0)
			{
				MandatoryValidation.CheckEntered(parent.SecondLevelWarningColorInfo);
			}
		}

		public void ValidateThirdLevelThresholdInfo()
		{
			parent.ThirdLevelThresholdInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(parent.ThirdLevelThresholdInfo);
			if (parent.ThirdLevelThreshold > 0)
			{
				if (parent.ThirdLevelThreshold > 15 || parent.ThirdLevelThreshold <= parent.SecondLevelThreshold)
				{
					parent.ThirdLevelThresholdInfo.AddError(Res.GetString("EC02987A-EFA8-4357-9A37-160557CBB4E9", "3rd Level Threshold should be between 2nd level threshold and 15."));
				}
			}
			if (parent.ThirdLevelThreshold <= parent.FirstLevelThreshold && parent.ThirdLevelThreshold > 0)
			{
				parent.ThirdLevelThresholdInfo.AddError(Res.GetString("363182F9-A497-4BC6-BE50-C2FEE7EF9441", "3rd Level Threshold should be greater than 1st level threshold."));
			}
			ValidateThirdLevelWarningColor();
		}

		public void ValidateThirdLevelWarningColor()
		{
			parent.ThirdLevelWarningColorInfo.ClearAllNotifications();
			ValidateRGBValue(parent.ThirdLevelWarningColorInfo);
			if (parent.ThirdLevelThreshold > 0)
			{
				MandatoryValidation.CheckEntered(parent.ThirdLevelWarningColorInfo);
			}
		}

		public void ValidateDelayedWarningColor()
		{
			parent.DelayedWarningColorInfo.ClearAllNotifications();
			ValidateRGBValue(parent.DelayedWarningColorInfo);
		}
		#endregion

		#region ValidateRowData
		public void ValidateRowData()
		{
			parent.ClearRowNotifications();
			if (parent.FirstLevelThreshold == 0 && parent.SecondLevelThreshold == 0 && parent.ThirdLevelThreshold == 0)
			{
				parent.AddRowError(Res.GetString("D3255831-8ADD-4EDE-A95A-50C378825F64", "The three thresholds must be non-negative and not all zero."));
			}
		}
		#endregion

		#region ValidateRGB
		public static void ValidateRGBValue(ZPropertyInfo color)
		{
			var rgbValue = (ZString)color.Value;
			if (!rgbValue.IsEmpty)
			{
				if (!ColorHelper.HasCorrectFormat(rgbValue))
				{
					color.AddError(Res.GetString("3B9848F8-E3D1-4D04-9F44-BCE9AB29DB89", "RGB value should be in the format of R,G,B"));
				}

				if (!color.HasErrors())
				{
					if (!ColorHelper.IsValidColor(rgbValue))
					{
						color.AddError(Res.GetString("33E02269-51FC-4F58-9273-39ED3E1CA75F", "Invalid color"));
					}
				}
			}
		}
		#endregion
	}
}
