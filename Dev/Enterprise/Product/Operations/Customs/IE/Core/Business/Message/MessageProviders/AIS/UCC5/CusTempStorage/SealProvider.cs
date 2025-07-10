using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class SealProvider : ISeal
	{
		internal SealProvider(TemporaryStorageHeader header)
		{
			this.header = header;
		}
		readonly TemporaryStorageHeader header;

		public string SealNumber => SealIds.Count.ToString();

		public IReadOnlyCollection<string> SealIds => sealIds ??= header.Containers.SelectMany(GetSeals).Distinct().OrderBy(p => p).ToArray();
		IReadOnlyCollection<string> sealIds;

		List<string> GetSeals(TemporaryStorageContainer container)
		{
			var seals = new List<string>();

			var seal1 = container.ACN_Seal1;
			if (!seal1.IsEmpty)
			{
				seals.Add(seal1); 
			}

			var seal2 = container.ACN_Seal2;
			if (!seal2.IsEmpty)
			{
				seals.Add(seal2);
			}

			var seal3 = container.ACN_Seal3;
			if (!seal3.IsEmpty)
			{
				seals.Add(seal3);
			}

			seals.AddRange(container.AdditionalSeals.Where(p => !p.BK_SealNumber.IsEmpty).Select(p => p.BK_SealNumber.ToString()));
			return seals;
		}
	}
}
