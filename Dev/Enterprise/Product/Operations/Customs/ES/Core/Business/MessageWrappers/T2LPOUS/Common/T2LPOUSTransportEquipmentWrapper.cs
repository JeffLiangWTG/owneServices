using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSTransportEquipmentWrapper : IT2LPOUSTransportEquipment
	{
		T2LPOUSTransportEquipmentWrapper(ZString containerNum, List<ZShort> goodsReference)
		{
			ContainerIdentificationNumber = containerNum;
			goodsReferenceList = goodsReference;
		}
		readonly List<ZShort> goodsReferenceList;

		public ZString ContainerIdentificationNumber { get; }

		public IReadOnlyCollection<ZInt> GoodsReference
		{
			get
			{
				if (goodsReference == null)
				{
					var goodsRefList = new List<ZInt>();

					goodsReferenceList.ForEach(x => goodsRefList.Add(x));

					goodsReference = goodsRefList.AsReadOnly();
				}
				return goodsReference;
			}
		}
		ReadOnlyCollection<ZInt> goodsReference;

		public static IReadOnlyCollection<T2LPOUSTransportEquipmentWrapper> GetTransportEquipmentList(CusEntryHeader entryHeader)
		{
			var transportEquipments = new List<T2LPOUSTransportEquipmentWrapper>();
			if (entryHeader != null)
			{
				var (containerToEntryLineMapping, _) = entryHeader.GetContainerOrEquipmentToEntryLineMapping();
				var entryLineCount = entryHeader.MergedLines.Count;

				foreach (var contDict in containerToEntryLineMapping.OrderBy(x => x.Key.CO_ContainerNumber))
				{
					var goodsReferenceList = contDict.Value.Select(x => x.CL_LineNumber).OrderBy(x => x).ToList();
					if (entryLineCount == goodsReferenceList.Count)
					{
						goodsReferenceList = new List<ZShort> { 0 };
					}
					transportEquipments.Add(new T2LPOUSTransportEquipmentWrapper(contDict.Key.CO_ContainerNumber, goodsReferenceList));
				}
			}

			return transportEquipments.AsReadOnly();
		}
	}
}
