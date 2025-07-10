using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	class CFRPoisonInhalationHazardComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var substance = wrapper?.DGData?.Substance?.StandardSubstance as UNDGSubstanceCFR;
			if (substance == null
				|| substance.CFR_PoisonInhalationHazard.IsEmpty)
			{
				return ZString.Empty;
			}

			return $"Poison-Inhalation Hazard Zone {substance.CFR_PoisonInhalationHazard}";
		}
	}
}
