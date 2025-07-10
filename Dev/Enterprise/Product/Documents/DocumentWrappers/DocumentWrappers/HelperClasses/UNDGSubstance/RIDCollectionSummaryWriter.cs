using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	class RIDCollectionSummaryWriter : IUNDGSubstanceCollectionSummaryWriter
	{
		ZString IUNDGSubstanceCollectionSummaryWriter.GetSummary(IReadOnlyCollection<UNDGSubstanceWrapper> collection)
		{
			var summarys = new List<string>();
			var ridCollection = collection.Where(IsRID).ToList();

			var class1TotalWeightSummary = GetUNDGTotalWeightSummaryForRIDClass1(ridCollection);
			if (!class1TotalWeightSummary.IsEmpty)
			{
				summarys.Add(class1TotalWeightSummary);
			}

			var class1MixedPackingSummary = GetUNDGSummaryForRIDClass1MixedPackingProvisions(ridCollection);
			if (!class1MixedPackingSummary.IsEmpty)
			{
				summarys.Add(class1MixedPackingSummary);
			}

			if (summarys.Count == 0)
			{
				return ZString.Empty;
			}
			else
			{
				return string.Join(System.Environment.NewLine, summarys.ToArray());
			}
		}

		bool IsRID(UNDGSubstanceWrapper undg)
		{
			return (undg.DGData?.Substance?.DG_Standard ?? ZString.Empty) == UNDGSubstanceStandardTypes.RID;
		}

		bool IsClass1(UNDGSubstanceWrapper undg)
		{
			return (undg.DGData?.Substance?.DG_Class ?? ZString.Empty) == "1";
		}

		ZString GetUNDGTotalWeightSummaryForRIDClass1(IReadOnlyCollection<UNDGSubstanceWrapper> undgs)
		{
			var class1UNDGs = undgs
				.Where(IsClass1)
				.ToList();

			var totalWeight = class1UNDGs.Sum(undg => undg.Weight.InKilograms.Value);

			if (class1UNDGs.Count > 0)
			{
				return Res.GetString("28a31fb2-4fd9-5ebd-446e-1de9dd53ea39", "Total Net Weight: {0} KG", totalWeight);
			}

			return ZString.Empty;
		}

		ZString GetUNDGSummaryForRIDClass1MixedPackingProvisions(IReadOnlyCollection<UNDGSubstanceWrapper> substanceWrappers)
		{
			var unnos = substanceWrappers
				.Where(substanceWrapper => IsClass1(substanceWrapper) && SubstanceHasMixedPackingProvisions(substanceWrapper))
				.Select(uNDG => uNDG.UNNumber).ToList();

			if (unnos.Count > 0)
			{
				return Res.GetString("d789645d-baa7-4295-4e0d-e037ce9558f8", "GOODS ON UN NOS: {0}", string.Join(", ", unnos.ToArray()));
			}

			return ZString.Empty;
		}

		bool SubstanceHasMixedPackingProvisions(UNDGSubstanceWrapper substanceWrapper)
		{
			return MixedPackingProvisionCodes.Contains((substanceWrapper.DGData?.Subs?.StandardSubstance as UNDGSubstanceRID)?.RID_MixedPackProv ?? ZString.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string value.")]
		List<ZString> MixedPackingProvisionCodes => mixedPackingProvisionCodes ?? (mixedPackingProvisionCodes = new List<ZString>
		{
			"MP1",
			"MP2",
			"MP20",
			"MP21",
			"MP22",
			"MP23",
			(NoResString)"MP22 MP24",
			(NoResString)"MP23 MP24",
			"MP20 MP24"
		});
		List<ZString> mixedPackingProvisionCodes;
	}
}
