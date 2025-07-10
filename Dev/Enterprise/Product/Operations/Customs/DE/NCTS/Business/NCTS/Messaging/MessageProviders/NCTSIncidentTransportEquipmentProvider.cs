using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSIncidentTransportEquipmentProvider : INCTSTransportEquipment
	{
		public static NCTSIncidentTransportEquipmentProvider NewOrNull(EU.NCTS.Business.NctsContainer container) => container != null ? new NCTSIncidentTransportEquipmentProvider(container) : null;

		NCTSIncidentTransportEquipmentProvider(EU.NCTS.Business.NctsContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		public string IdentificationNumber => container.BC_ContainerNum;

		public INCTSSeals Seals => CachedValueHelper.GetValue(ref seals, () =>
		{
			var seal1 = container.BC_Seal1;
			var seal2 = container.BC_Seal2;
			var sealList = container.SealsForMessaging.Select(x => x.BK_SealNumber).ToList();
			if (!seal1.IsEmpty)
			{
				sealList.Add(seal1);
			}
			if (!seal2.IsEmpty)
			{
				sealList.Add(seal2);
			}
			return NCTSSealsProvider.NewOrNull(sealList);
		});
		CachedValue<INCTSSeals> seals;

		public IReadOnlyCollection<int> GoodsReferences => goodsReferences ?? (goodsReferences = container.ItemNumbers.Select(i => (int)i.CY_DataNumeric).OrderBy(l => l).ToArray());
		IReadOnlyCollection<int> goodsReferences;

		readonly EU.NCTS.Business.NctsContainer container;
	}
}
