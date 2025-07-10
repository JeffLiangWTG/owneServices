using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public sealed class TransportEquipmentWithSealsProvider : TransportEquipmentWithSealsBaseProvider
	{
		public static IReadOnlyCollection<ITransportEquipmentWithSeals> GetEquipments(CusEntryHeader entry)
		{
			var result = new List<ITransportEquipmentWithSeals>();
			var mappings = entry?.GetContainerOrEquipmentToEntryLineMapping();
			if (mappings?.containers is IDictionary<Customs.Business.BaseCusContainer, IReadOnlyCollection<Customs.Business.CusEntryLine>> containerMappings
				&& containerMappings.Count > 0)
			{
				result.AddRange(
					containerMappings
					.Select(x => (
							ContainerNumber: x.Key.CO_ContainerNumber,
							Seals: GetSealsArray(x.Key),
							LineNumbers: x.Value.Select(y => y.CL_LineNumber).OrderBy(l => l)
						))
					.OrderBy(o => o.ContainerNumber)
					.Select(p => new TransportEquipmentWithSealsProvider()
					{
						ContainerIdentificationNumber = p.ContainerNumber,
						Seals = GetSeals(p.Seals),
						GoodsReferences = p.LineNumbers.Select(l => l.ToString()).ToArray()
					})
				);
			}

			if (mappings?.equipments is IDictionary<Customs.Business.CusEquipment, IReadOnlyCollection<Customs.Business.CusEntryLine>> equipmentMappings
				&& equipmentMappings.Count > 0)
			{
				result.AddRange(equipmentMappings.
					Select(x => (
						Seals: ((EU.Business.Declaration.CusEquipment)x.Key).Seals.OrderBy(o => o.BK_SequenceNumber).Select(p => p.BK_SealNumber).ToArray(),
						LineNumbers: x.Value.Select(y => y.CL_LineNumber).OrderBy(l => l))
						).
					Where(o => o.Seals.Length > 0).
					Select(p => new TransportEquipmentWithSealsProvider()
					{
						ContainerIdentificationNumber = string.Empty,
						Seals = GetSeals(p.Seals),
						GoodsReferences = p.LineNumbers.Select(l => l.ToString()).ToArray()
					}));
			}
			return result.ToArray();
		}

		static ZString[] GetSealsArray(Customs.Business.BaseCusContainer container)
		{
			var result = new List<ZString> { container.CO_Seal, container.CO_SecondSeal };
			if (container is EU.Business.Declaration.CusContainer euContainer)
			{
				result.AddRange(euContainer.AdditionalSeals.OrderBy(x => x.BK_SequenceNumber).Select(seal => seal.BK_SealNumber));
			}

			return result.ToArray();
		}
	}
}
