using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class ContainerPackingFlatteningExtensions
	{
		public static DataObjectList<Container> GetContainersReferencedBy(this DataObjectList<Container> containers, IEnumerable<PackingLine> packingLines)
		{
			var containerList = new DataObjectList<Container>();
			containerList.Content = CollectionContent.Partial;

			if (packingLines != null && containers != null)
			{
				var containersIDsFromPacking = packingLines.Where(p => p.ContainerLink.HasValue).Select(p => p.ContainerLink).Distinct().ToArray();
				if (containersIDsFromPacking.Any())
				{
					containerList.AddRange(containers.Where(c => c.Link.HasValue && containersIDsFromPacking.Contains(c.Link.Value)));
				}
			}

			return containerList;
		}

		public static IEnumerable<ZString> GetDistinctContainerNumbers(this DataObjectList<PackingLine> packingLines)
		{
			if (packingLines != null)
			{
				return packingLines
					.Where(p => p.ContainerNumber.HasValue && !p.ContainerNumber.Value.IsEmpty)
					.Select(p => p.ContainerNumber.Value)
					.Distinct()
					.ToArray();
			}

			return Enumerable.Empty<ZString>();
		}
	}
}
