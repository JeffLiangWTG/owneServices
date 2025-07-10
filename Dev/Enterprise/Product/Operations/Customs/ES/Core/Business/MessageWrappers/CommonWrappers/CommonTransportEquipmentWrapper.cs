using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonTransportEquipmentWrapper : ICommonTransportEquipment
{
	protected CommonTransportEquipmentWrapper(ZShort seqNum, ZString containerNum, List<ZShort> goodsReference)
	{
		SequenceNumber = seqNum.ToString();
		ContainerNumber = containerNum;
		goodsReferenceList = goodsReference;
	}
	readonly List<ZShort> goodsReferenceList;

	public ZString SequenceNumber { get; }

	public ZString ContainerNumber { get; }

	public IReadOnlyCollection<ICommonGoodsReference> GoodsReference
	{
		get
		{
			if (goodsReference == null)
			{
				var commonGoodsReferenceList = new List<CommonGoodsReferenceWrapper>();

				ZShort seqNum = 1;
				foreach (var reference in goodsReferenceList)
				{
					commonGoodsReferenceList.Add(new CommonGoodsReferenceWrapper(seqNum, reference));
					seqNum++;
				}
				goodsReference = commonGoodsReferenceList.AsReadOnly();
			}
			return goodsReference;
		}
	}
	IReadOnlyCollection<CommonGoodsReferenceWrapper> goodsReference;

	public static IReadOnlyCollection<CommonTransportEquipmentWrapper> GetTransportEquipmentList(CusEntryHeader entryHeader)
	{
		var transportEquipments = new List<CommonTransportEquipmentWrapper>();
		if (entryHeader != null)
		{
			(var containerToEntryLineMapping, var _) = entryHeader.GetContainerOrEquipmentToEntryLineMapping();

			ZShort seqNum = 1;
			foreach (var contDict in containerToEntryLineMapping.OrderBy(x => x.Key.CO_ContainerNumber))
			{
				var goodsReferenceList = contDict.Value.Select(x => x.CL_LineNumber).OrderBy(x => x).ToList();
				transportEquipments.Add(new CommonTransportEquipmentWrapper(seqNum, contDict.Key.CO_ContainerNumber, goodsReferenceList));
				seqNum++;
			}
		}
		return transportEquipments.AsReadOnly();
	}
}
