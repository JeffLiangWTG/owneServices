using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	class CFRReportableQuantityComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var undgDataItem = wrapper?.DGData;
			if (undgDataItem == null)
			{
				return ZString.Empty;
			}

			var substance = undgDataItem.Substance;
			if (substance == null ||
				substance.DG_Standard != UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR)
			{
				return ZString.Empty;
			}

			if (TryGetReportableQuantityInPounds(substance.StandardSubstance as UNDGSubstanceCFR, out var reportableQuantityInPounds))
			{
				var dataItemWeightInPounds = Constants.Weight
					.ConvertSafe(
						sourceValue: undgDataItem.DI_DGWeight,
						sourceUnitCode: undgDataItem.DI_UnitOfWeight.ToUpperInvariant(),
						targetUnitCode: Constants.Weight.Pounds);

				if (dataItemWeightInPounds >= reportableQuantityInPounds)
				{
					return ReportableQuantityAcronym;
				}
			}

			return ZString.Empty;
		}

		bool TryGetReportableQuantityInPounds(UNDGSubstanceCFR substance, out ZDecimal reportableQuantity)
		{
			if (substance == null ||
				substance.CFR_ReportableQuantity == ZDecimal.Zero)
			{
				reportableQuantity = 0;
				return false;
			}

			reportableQuantity = Constants.Weight
				.ConvertSafe(
					sourceValue: substance.CFR_ReportableQuantity,
					sourceUnitCode: substance.CFR_ReportableQuantityUnit.ToUpperInvariant(),
					targetUnitCode: Constants.Weight.Pounds);

			return true;
		}

		const string ReportableQuantityAcronym = "RQ";
	}
}
