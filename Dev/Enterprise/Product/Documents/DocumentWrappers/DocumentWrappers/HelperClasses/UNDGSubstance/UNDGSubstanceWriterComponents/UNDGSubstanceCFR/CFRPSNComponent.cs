using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class CFRPSNComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var properShippingNameWithTechnicalName = UNDGSubstanceWrapperHelper
				.GetPSNWithTechnicalNameOfSubstance(wrapper);

			var undgDataItem = wrapper.DGData as ForwardingUNDGDataItem;
			var substance = undgDataItem?.Substance;

			if (substance == null ||
				substance.DG_PSN.Contains(RawMoltenString, System.StringComparison.InvariantCultureIgnoreCase) ||
				substance.DG_PSN.Contains(RawElevatedTemperatureString, System.StringComparison.InvariantCultureIgnoreCase))
			{
				return properShippingNameWithTechnicalName;
			}

			var stringBuilder = new ZStringBuilder();

			if (CheckIfSubstanceIsInElevatedTemperatures(undgDataItem))
			{
				stringBuilder.Append((NoResString)"HOT - ");
			}

			stringBuilder.Append(properShippingNameWithTechnicalName);

			return stringBuilder.ToString();
		}

		#region Implementation

		bool CheckIfSubstanceIsInElevatedTemperatures(ForwardingUNDGDataItem undgDataItem)
		{
			var substance = undgDataItem?.Substance?.StandardSubstance as UNDGSubstanceCFR;
			if (substance == null)
			{
				return false;
			}

			switch (substance.CFR_State)
			{
				case UNDGSubstanceLookups.StateTypes.Code.Liquid:
					return IsLiquidSubstanceInElevatedTemperature(undgDataItem);

				case UNDGSubstanceLookups.StateTypes.Code.Solid:
					return IsSolidSubstanceInElevatedTemperature(undgDataItem);

				default:
					return false;
			}
		}

		bool IsLiquidSubstanceInElevatedTemperature(ForwardingUNDGDataItem undgDataItem)
		{
			var parentPackline = undgDataItem?.ParentPackLine;
			if (parentPackline == null ||
				!parentPackline.JL_RequiresTemperatureControl)
			{
				return false;
			}

			var maximumTemperatureInCentigrade = GetRequiredMaximumTemperatureInCentigrade(parentPackline);
			var flashPointOfDG = undgDataItem.DI_DGFlashPoint;

			var waterBoilingTemperatureInCentigrade = 100;
			var oshaFlashPointLimitInCentigrade = 38;

			if (maximumTemperatureInCentigrade >= waterBoilingTemperatureInCentigrade ||
				(flashPointOfDG >= oshaFlashPointLimitInCentigrade &&
				maximumTemperatureInCentigrade >= flashPointOfDG))
			{
				return true;
			}

			return false;
		}

		bool IsSolidSubstanceInElevatedTemperature(ForwardingUNDGDataItem undgDataItem)
		{
			var parentPackline = undgDataItem?.ParentPackLine;
			if (parentPackline == null ||
				!parentPackline.JL_RequiresTemperatureControl)
			{
				return false;
			}

			var solidPhaseTemperatureLimitInCentigrade = 240;
			var maximumTemperatureInCentigrade = GetRequiredMaximumTemperatureInCentigrade(parentPackline);
			if (maximumTemperatureInCentigrade >= solidPhaseTemperatureLimitInCentigrade)
			{
				return true;
			}

			return false;
		}

		decimal GetRequiredMaximumTemperatureInCentigrade(ForwardingPackLine packline)
		{
			return Constants.Temperature.Convert(
				sourceValue: packline.JL_RequiredTemperatureMaximum,
				sourceUnitCode: packline.JL_RequiredTemperatureUnit,
				targetUnitCode: Constants.Temperature.Centigrade);
		}

		const string RawMoltenString = "MOLTEN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translateable constant")]
		const string RawElevatedTemperatureString = "ELEVATED TEMPERATURE";

		#endregion
	}
}
