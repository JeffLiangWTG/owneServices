using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI
{
	public static class ClassificationTariffUserControlHelper
	{
		public static void UpdateTariffColumnStyleInfoAndTariffFindBoxToGetTariffFromSRDb(
			ZGrid grid,
			ZString columnName,
			ZArchitecture.GUI.ZCodeFindBox tariffFindBox,
			ZString newTariffFindBoxName,
			Func<ZDateTime> getEffectiveAssessmentDateForUniversalTariff,
			Action<Universal.GUI.TariffFindBox> setBindingMember)
		{
			UpdateTariffColumnStyleInfoToGetTariffFromSRDb(grid, columnName, getEffectiveAssessmentDateForUniversalTariff);
			UpdateTariffFindBoxToGetTariffFromSRDb(tariffFindBox, newTariffFindBoxName, getEffectiveAssessmentDateForUniversalTariff, setBindingMember);
		}

		public static void UpdateTariffColumnStyleInfoToGetTariffFromSRDb(
			ZGrid grid,
			ZString columnName,
			Func<ZDateTime> getEffectiveAssessmentDateForUniversalTariff)
		{
			if (grid != null)
			{
				var tariffColumnInfo = grid.GetColumnStyle(columnName);
				if (tariffColumnInfo is Customs.GUI.TariffColumnStyleInfo columnInfo)
				{
					var tariffFromRSDbColumnInfo = new Universal.GUI.TariffColumnStyleInfo()
					{
						TariffType = Universal.Constants.TariffTypes.HarmonizedSystem,
						ColumnName = columnName,
						GetEffectiveDate = getEffectiveAssessmentDateForUniversalTariff,
						GetCountryCode = () => Core.Constants.CountryCodes.Canada,
						GetDataGrouping = () => Core.Constants.CountryCodes.Canada,
					};
					tariffFromRSDbColumnInfo.ColumnName = columnInfo.ColumnName;
					tariffFromRSDbColumnInfo.CaptionResourceString = columnInfo.CaptionResourceString;
					grid.ColumnStyles.Add(tariffFromRSDbColumnInfo);
					grid.ColumnStyles.Remove(columnInfo);
				}
			}
		}

		public static void UpdateTariffFindBoxToGetTariffFromSRDb(
			ZArchitecture.GUI.ZCodeFindBox tariffFindBox,
			ZString newTariffFindBoxName,
			Func<ZDateTime> getEffectiveAssessmentDateForUniversalTariff,
			Action<Universal.GUI.TariffFindBox> setBindingMember)
		{
			if (tariffFindBox != null)
			{
				tariffFindBox.Visible = false;
				var tariffFromSRDbFindBox = new Universal.GUI.TariffFindBox();
				var parentControl = tariffFindBox.Parent;
				parentControl.Controls.Add(tariffFromSRDbFindBox);
				parentControl.Controls.SetChildIndex(tariffFromSRDbFindBox, 0);
				tariffFromSRDbFindBox.AllowDrop = true;
				setBindingMember(tariffFromSRDbFindBox);
				tariffFromSRDbFindBox.Anchor = tariffFindBox.Anchor;
				tariffFromSRDbFindBox.CaptionResourceString = tariffFindBox.CaptionResourceString;
				tariffFromSRDbFindBox.Location = tariffFindBox.Location;
				tariffFromSRDbFindBox.Name = newTariffFindBoxName;
				tariffFromSRDbFindBox.Size = tariffFindBox.Size;
				tariffFromSRDbFindBox.TabIndex = tariffFindBox.TabIndex;
				tariffFromSRDbFindBox.GetEffectiveDate = getEffectiveAssessmentDateForUniversalTariff;
				tariffFromSRDbFindBox.GetCountryCode = () => Core.Constants.CountryCodes.Canada;
				tariffFromSRDbFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.Canada;
				tariffFromSRDbFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
				tariffFromSRDbFindBox.Visible = true;
			}
		}
	}
}
