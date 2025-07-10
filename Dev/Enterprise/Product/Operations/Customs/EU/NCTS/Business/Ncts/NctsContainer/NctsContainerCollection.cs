using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerCollection<T> : CusInBondContainerCollection<NctsContainer> where T : BusinessObject
	{
		public NctsContainerCollection(T master)
			: base(master)
		{
			this.master = master;
		}

		readonly T master;

		public T Master => master;

		public bool HasDuplicatesForSealNumber(ZString sealNumber) => AllSeals.Where(x => x == sealNumber).Skip(1).Any();

		IEnumerable<ZString> AllSeals
		{
			get
			{
				foreach (NctsContainer container in this)
				{
					if (!container.BC_Seal1.IsEmpty)
					{
						yield return container.BC_Seal1;
					}

					if (!container.BC_Seal2.IsEmpty)
					{
						yield return container.BC_Seal2;
					}

					foreach (var seal in container.Seals.Cast<CusSeal>())
					{
						if (!seal.BK_SealNumber.IsEmpty)
						{
							yield return seal.BK_SealNumber;
						}
					}
				}
			}
		}
	}
}
