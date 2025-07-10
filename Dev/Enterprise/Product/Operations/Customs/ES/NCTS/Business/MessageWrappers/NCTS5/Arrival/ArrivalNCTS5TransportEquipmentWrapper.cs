using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5TransportEquipmentWrapper : NCTS5CommonTransportEquipmentWrapper
	{
		public ArrivalNCTS5TransportEquipmentWrapper(NctsContainer container, ZShort seqNum)
			: base(container, seqNum)
		{
		}

		protected override IReadOnlyCollection<INCTSCommonGoodsReference> GoodsReferenceCore
		{
			get
			{
				if (goodsReference == null)
				{
					var goodsReferenceList = new List<NCTS5CommonGoodsReferenceWrapper>();

					ZShort seqNum = 1;
					foreach (var item in container.ItemNumbers)
					{
						goodsReferenceList.Add(new NCTS5CommonGoodsReferenceWrapper(seqNum, item.CY_DataNumeric));
						seqNum++;
					}

					goodsReference = goodsReferenceList.AsReadOnly();
				}
				return goodsReference;
			}
		}
		IReadOnlyCollection<NCTS5CommonGoodsReferenceWrapper> goodsReference;
	}
}
