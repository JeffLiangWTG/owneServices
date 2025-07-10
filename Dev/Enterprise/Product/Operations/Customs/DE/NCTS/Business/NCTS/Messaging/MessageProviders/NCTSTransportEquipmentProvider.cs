using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSTransportEquipmentProvider : CargoWise.Customs.DE.MessageContracts.NCTS.INCTSTransportEquipment
	{
		public static NCTSTransportEquipmentProvider NewOrNull(NctsDepartureHeaderContainer headerContainer, string declarationType) => headerContainer != null && declarationType != null ? new NCTSTransportEquipmentProvider(headerContainer, declarationType) : null;

		NCTSTransportEquipmentProvider(NctsDepartureHeaderContainer headerContainer, string declarationType)
		{
			this.headerContainer = Argument.NotNull(headerContainer, nameof(headerContainer));
			this.declarationType = Argument.NotNull(declarationType, nameof(declarationType));
		}

		public string IdentificationNumber => headerContainer.BC_Mode == Core.Constants.ContainerModes.Containerised ? (string)headerContainer.BC_ContainerNum : null;

		public INCTSSeals Seals => CachedValueHelper.GetValue(ref seals, () =>
		{
			INCTSSeals result = null;
			var seal1 = headerContainer.BC_Seal1;
			var seal2 = headerContainer.BC_Seal2;
			if (declarationType == "10" || declarationType == "11")
			{
				var sealList = headerContainer.AdditionalSeals.Select(x => x.BK_SealNumber).ToList();
				if (!seal1.IsEmpty)
				{
					sealList.Add(seal1);
				}
				if (!seal2.IsEmpty)
				{
					sealList.Add(seal2);
				}
				result = NCTSSealsProvider.NewOrNull(sealList);
			}
			return result;
		});
		CachedValue<INCTSSeals> seals;

		public IReadOnlyCollection<int> GoodsReferences
		{
			get
			{
				if (goodsReferences == null)
				{
					if (headerContainer.BC_Mode == Core.Constants.ContainerModes.Containerised)
					{
						goodsReferences = headerContainer.Header.Bills.SelectMany(
							x => x.GoodsItems.Cast<NctsDepartureCargoDesc>().Where(y => y.ContainersSelected.Contains(headerContainer.BC_ContainerNum))).Select(i => (int)i.BY_DeclarationGoodsItemNumber).OrderBy(l => l).ToArray();
					}
					else
					{
						goodsReferences = Array.Empty<int>();
					}
				}
				return goodsReferences;
			}
		}
		IReadOnlyCollection<int> goodsReferences;

		readonly NctsDepartureHeaderContainer headerContainer;
		readonly string declarationType;
	}
}
