using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	static class SealsHelper
	{
		public static IReadOnlyCollection<string> GetSeals(this EU.Business.CusTempStorage.TemporaryStorageContainer container)
		=> new ZString[]
			{
				container.ACN_Seal1,
				container.ACN_Seal2,
				container.ACN_Seal3,
			}
			.Union(
				container.AdditionalSeals.Select(seal => seal.BK_SealNumber)
			)
			.Where(seal => !seal.IsEmpty)
			.Select(seal => seal.ToString())
			.ToArray();
	}
}
