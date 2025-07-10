using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

class DepartureMeansOfTransportCollectionComparer : IEqualityComparer<IReadOnlyCollection<DepartureMeansOfTransportWrapper>>
{
	public bool Equals(IReadOnlyCollection<DepartureMeansOfTransportWrapper> wrappers1, IReadOnlyCollection<DepartureMeansOfTransportWrapper> wrappers2)
	{
		if (ReferenceEquals(wrappers1, wrappers2))
		{
			return true;
		}

		if (wrappers1 == null || wrappers2 == null)
		{
			return false;
		}

		if (wrappers1.Count != wrappers2.Count)
		{
			return false;
		}

		var sortedWrappers1 = wrappers1
			.OrderBy(item => item.TypeOfIdentification)
			.ThenBy(item => item.IdentificationNumber)
			.ThenBy(item => item.Nationality)
			.ToList();
		var sortedWrappers2 = wrappers2
			.OrderBy(item => item.TypeOfIdentification)
			.ThenBy(item => item.IdentificationNumber)
			.ThenBy(item => item.Nationality)
			.ToList();

		for (var i = 0; i < sortedWrappers1.Count; i++)
		{
			var wrapper1 = sortedWrappers1[i];
			var wrapper2 = sortedWrappers2[i];
			if (wrapper1.TypeOfIdentification != wrapper2.TypeOfIdentification || wrapper1.IdentificationNumber != wrapper2.IdentificationNumber || wrapper1.Nationality != wrapper2.Nationality)
			{
				return false;
			}
		}

		return true;
	}

	public int GetHashCode(IReadOnlyCollection<DepartureMeansOfTransportWrapper> wrappers)
	{
		if (wrappers == null)
		{
			return 0;
		}

		var sortedWrappers = wrappers
			.OrderBy(item => item.TypeOfIdentification)
			.ThenBy(item => item.IdentificationNumber)
			.ThenBy(item => item.Nationality);

		var combinedString = string.Join(";", sortedWrappers.Select(wrapper =>
			$"{wrapper.TypeOfIdentification},{wrapper.IdentificationNumber},{wrapper.Nationality}"));

		return combinedString.GetHashCode();
	}
}
