using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class AESCommonTransportEquipmentWrapper : CommonTransportEquipmentWrapper, IAESCommonTransportEquipment
{
	protected AESCommonTransportEquipmentWrapper(ZShort seqNum, ZString containerNum, ZString sealAmount, List<ZString> seals, List<ZShort> goodsReference) : base(seqNum, containerNum, goodsReference)
	{
		NumberOfSeals = sealAmount;
		sealsList = seals;
	}
	readonly List<ZString> sealsList;

	public ZString NumberOfSeals { get; }

	public IReadOnlyCollection<ISealCommon> Seals => SealsCore;

	protected virtual IReadOnlyCollection<ISealCommon> SealsCore
	{
		get
		{
			if (seals == null)
			{
				var sealCommonWrapperList = new List<SealCommonWrapper>();

				ZShort seqNum = 1;
				foreach (var seal in sealsList)
				{
					sealCommonWrapperList.Add(new SealCommonWrapper(seqNum, seal));
					seqNum++;
				}

				seals = sealCommonWrapperList.AsReadOnly();
			}
			return seals;
		}
	}
	IReadOnlyCollection<SealCommonWrapper> seals;

	public static new IReadOnlyCollection<AESCommonTransportEquipmentWrapper> GetTransportEquipmentList(CusEntryHeader entryHeader)
	{
		var transportEquipments = new List<AESCommonTransportEquipmentWrapper>();
		if (entryHeader != null)
		{
			(var containerToEntryLineMapping, var equipmentToEntryLineMapping) = entryHeader.GetContainerOrEquipmentToEntryLineMapping();

			ZShort seqNum = 1;
			foreach (var contDict in containerToEntryLineMapping.OrderBy(x => x.Key.CO_ContainerNumber))
			{
				var sealList = GetSealsContainerList(contDict.Key);
				var goodsReferenceList = contDict.Value.Select(x => x.CL_LineNumber).OrderBy(x => x).ToList();
				transportEquipments.Add(new AESCommonTransportEquipmentWrapper(seqNum, contDict.Key.CO_ContainerNumber, sealList.Count.ToString(), sealList, goodsReferenceList));
				seqNum++;
			}

			foreach (var equipDict in equipmentToEntryLineMapping.OrderBy(x => x.Key.CEQ_IdentificationNumber))
			{
				var sealList = GetSealList(((EU.Business.Declaration.CusEquipment)equipDict.Key).Seals.Select(p => p.BK_SealNumber).ToArray());
				sealList.Sort();
				var goodsReferenceList = equipDict.Value.Select(x => x.CL_LineNumber).OrderBy(x => x).ToList();
				transportEquipments.Add(new AESCommonTransportEquipmentWrapper(seqNum, ZString.Empty, sealList.Count.ToString(), sealList, goodsReferenceList));
				seqNum++;
			}
		}
		return transportEquipments.AsReadOnly();

		List<ZString> GetSealsContainerList(Customs.Business.BaseCusContainer container)
		{
			var result = GetSealList(container.CO_Seal, container.CO_SecondSeal);
			if (container is EU.Business.Declaration.CusContainer euContainer)
			{
				var resultForSort = GetSealList(euContainer.AdditionalSeals.Select(p => p.BK_SealNumber).ToArray());
				resultForSort.Sort();
				result.AddRange(resultForSort);
			}
			return result.Distinct().Select(x => x).ToList();
		}

		List<ZString> GetSealList(params ZString[] seals)
		{
			var list = new List<ZString>();
			foreach (var seal in seals)
			{
				if (!seal.IsEmpty)
				{
					list.Add(seal);
				}
			}
			return list.Distinct().ToList();
		}
	}
}
