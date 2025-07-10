using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class RadionuclideComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var undgDataItem = wrapper?.DGData;

			if (undgDataItem != null)
			{
				var radionuclideElement = undgDataItem.DI_RadionuclideElement;
				var radionuclideElementSuffix = undgDataItem.DI_RadionuclideElementSuffix;
				var maximumActivity = undgDataItem.DI_RadioactiveMaximumActivity;
				var activityUnit = undgDataItem.DI_RadioactiveMaximumActivityUnit;

				var radionuclideComponentLines = new List<ZString>();
				if (!radionuclideElement.IsEmpty && !radionuclideElementSuffix.IsEmpty)
				{
					if (char.IsDigit(radionuclideElementSuffix[0]))
					{
						radionuclideComponentLines.Add(string.Format("{0}-{1}", radionuclideElement, radionuclideElementSuffix));
					}
					else
					{
						radionuclideComponentLines.Add(string.Format("{0} {1}", radionuclideElement, radionuclideElementSuffix));
					}
				}

				if (!activityUnit.IsEmpty)
				{
					(var becquerelQuantity, var becquerelUnits) = ConvertToAppropriateBecquerelUnits(maximumActivity, activityUnit);
					(var curieQuantity, var curieUnits) = ConvertToAppropriateCurieUnits(maximumActivity, activityUnit);

					var becquerelUnitsSymbol = Core.Constants.RadioactiveUnits.GetSymbol(becquerelUnits);
					var curieUnitsSymbol = Core.Constants.RadioactiveUnits.GetSymbol(curieUnits);
					if (!string.IsNullOrEmpty(becquerelUnitsSymbol) && !string.IsNullOrEmpty(curieUnitsSymbol))
					{
						radionuclideComponentLines.Add(string.Format("{0} {1} ({2} {3})", becquerelQuantity, becquerelUnitsSymbol, curieQuantity, curieUnitsSymbol));
					}
				}

				if (radionuclideComponentLines.Any())
				{
					var radionuclideComponentResult = string.Join(", ", radionuclideComponentLines);
					return radionuclideComponentResult;
				}
			}

			return ZString.Empty;
		}

		(ZDecimal quantity, ZString unit) ConvertToAppropriateCurieUnits(ZDecimal maximumActivity, ZString activityUnit)
		{
			var curie = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Curie);
			if (curie < 0.001m)
			{
				var microCurie = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Microcurie);
				microCurie = Utilities.Round(microCurie, 2);
				return (microCurie, Core.Constants.RadioactiveUnits.Microcurie);
			}
			else if (curie < 0.1m)
			{
				var milliCurie = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Millicurie);
				milliCurie = Utilities.Round(milliCurie, 2);
				return (milliCurie, Core.Constants.RadioactiveUnits.Millicurie);
			}

			curie = Utilities.Round(curie, 2);
			return (curie, Core.Constants.RadioactiveUnits.Curie);
		}

		(ZDecimal quantity, ZString unit) ConvertToAppropriateBecquerelUnits(ZDecimal maximumActivity, ZString activityUnit)
		{
			var megabecquerel = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Megabecquerel);
			if (megabecquerel >= 100000m)
			{
				var terabecquerel = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Terabecquerel);
				terabecquerel = Utilities.Round(terabecquerel, 2);
				return (terabecquerel, Core.Constants.RadioactiveUnits.Terabecquerel);
			}
			else if (megabecquerel >= 100m)
			{
				var gigabecquerel = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Gigabecquerel);
				gigabecquerel = Utilities.Round(gigabecquerel, 2);
				return (gigabecquerel, Core.Constants.RadioactiveUnits.Gigabecquerel);
			}

			megabecquerel = Utilities.Round(megabecquerel, 2);
			return (megabecquerel, Core.Constants.RadioactiveUnits.Megabecquerel);
		}
	}
}
