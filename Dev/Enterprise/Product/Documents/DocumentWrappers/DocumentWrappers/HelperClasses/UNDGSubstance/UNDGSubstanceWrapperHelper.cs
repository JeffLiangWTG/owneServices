using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	public class UNDGSubstanceWrapperHelper
	{
		public static ZString GetPSNWithTechnicalNameOfSubstance(UNDGSubstanceWrapper wrapper)
		{
			var properShippingName = wrapper?.DGData?.ProperShippingName ?? ZString.Empty;

			if (!properShippingName.IsEmpty
				&& !wrapper.TechnicalName.IsEmpty)
			{
				return properShippingName + " (" + wrapper.TechnicalName + ")";
			}

			return properShippingName;
		}

		public ZString GetUNDGPackagesSummary(IReadOnlyCollection<UNDGSubstanceWrapper> undgs)
		{
			var summaries = DatasetSummaryWriters
				.Select(writer => writer.GetSummary(undgs))
				.Where(summary => !summary.IsEmpty)
				.ToList();

			if (summaries.Count > 0)
			{
				return string.Join(System.Environment.NewLine, summaries);
			}

			return ZString.Empty;
		}

		IEnumerable<IUNDGSubstanceCollectionSummaryWriter> DatasetSummaryWriters
		{
			get
			{
				yield return new RIDCollectionSummaryWriter();
				yield return new ADNCollectionSummaryWriter();
				yield return new CFRCollectionSummaryWriter();
			}
		}
	}
}
