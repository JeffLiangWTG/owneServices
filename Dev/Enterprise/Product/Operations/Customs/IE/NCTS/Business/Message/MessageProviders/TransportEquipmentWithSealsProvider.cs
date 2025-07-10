using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business.AES;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class TransportEquipmentWithSealsProvider : TransportEquipmentWithSealsBaseProvider
	{
		public static IReadOnlyCollection<ITransportEquipmentWithSeals> GetEquipments(NctsHeader nctsHeader)
		{
			var result = new List<ITransportEquipmentWithSeals>();

			var containers = nctsHeader.DepartureHeaderContainers;
			var singleContainer = containers.Count == 1;
			var goodsReferences = new Dictionary<ZString, HashSet<ZShort>>();
			HashSet<ZShort> singleContainerList = null;
			if (singleContainer)
			{
				singleContainerList = new HashSet<ZShort>();
				goodsReferences.Add(containers.Cast<NctsDepartureHeaderContainer>().Single().BC_ContainerNum, singleContainerList);
			}

			foreach (var bill in nctsHeader.Bills)
			{
				foreach (var goodsItem in bill.GoodsItems)
				{
					if (singleContainer)
					{
						if (goodsItem.Packages.Count > 0)
						{
							singleContainerList.Add(goodsItem.BY_LineNo);
						}
					}
					else
					{
						foreach (NctsPackage package in goodsItem.Packages)
						{
							foreach (var container in package.ContainersPivot.Containers)
							{
								goodsReferences.GetOrAdd(container.BC_ContainerNum, () => new HashSet<ZShort>())
									.Add(goodsItem.BY_LineNo);
							}
						}
					}
				}
			}

			if (singleContainerList != null && singleContainerList.Count == 0)
			{
				goodsReferences.Clear();
			}

			result.AddRange(
				containers
				.Select(x => (
						ContainerNumber: x.BC_ContainerNum,
						Seals: GetSealsArray(x),
						LineNumbers: goodsReferences.GetOrAdd(x.BC_ContainerNum, () => new HashSet<ZShort>())
					))
				.OrderBy(o => o.ContainerNumber)
				.Select(p => new TransportEquipmentWithSealsProvider()
				{
					ContainerIdentificationNumber = p.ContainerNumber,
					Seals = GetSeals(p.Seals),
					GoodsReferences = p.LineNumbers.Select(l => l.ToString()).ToArray()
				})
			);
			return result.ToArray();
		}

		public static IReadOnlyCollection<ITransportEquipmentWithSeals> GetEquipments(EnRouteIncident incident)
		{
			var result = new List<ITransportEquipmentWithSeals>();
			var containers = incident.IncidentContainers;

			result.AddRange(containers.OrderBy(container => container.ContainerNumber).Select(container => new TransportEquipmentWithSealsProvider()
			{
				ContainerIdentificationNumber = container.ContainerNumber,
				Seals = GetSealsArray(container),
				GoodsReferences = container.ItemNumbers.Select(itemNumber => itemNumber.CY_DataNumeric.ToString()).ToArray()
			}));

			return result.ToArray();
		}

		static ZString[] GetSealsArray(NctsDepartureHeaderContainer container)
		{
			var result = new List<ZString> { container.Seal1, container.Seal2 };
			result.AddRange(container.AdditionalSeals.Select(seal => seal.BK_SealNumber));
			return result.ToArray();
		}

		static IReadOnlyCollection<string> GetSealsArray(NctsContainer container)
		{
			var result = new List<ZString> { container.BC_Seal1, container.BC_Seal2 };
			result.AddRange(container.SealsForMessaging.Select(seal => seal.BK_SealNumber));
			return result.Where(sealNumber => !sealNumber.IsEmpty).Select(sealNumber => sealNumber.ToString()).ToArray();
		}
	}
}
