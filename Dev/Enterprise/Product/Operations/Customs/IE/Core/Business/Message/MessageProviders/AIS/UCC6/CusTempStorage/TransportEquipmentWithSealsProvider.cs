using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	[CodeAlive("Part of a message builder in development")]
	public class TransportEquipmentWithSealsProvider : ITransportEquipmentWithSeals
	{
		readonly TemporaryStorageContainer container;

		public TransportEquipmentWithSealsProvider(TemporaryStorageContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}
		public IReadOnlyCollection<string> Seals
		{
			get
			{
				if (sealsCached == null)
				{
					var seals = new List<string>();
					if (!container.ACN_Seal1.IsEmpty)
					{
						seals.Add(container.ACN_Seal1);
					}
					if (!container.ACN_Seal2.IsEmpty)
					{
						seals.Add(container.ACN_Seal2);
					}
					if (!container.ACN_Seal3.IsEmpty)
					{
						seals.Add(container.ACN_Seal3);
					}
					foreach (var s in container.AdditionalSeals)
					{
						seals.Add(s.BK_SealNumber);
					}
					if (seals.Count > 0)
					{
						sealsCached = seals;
					}
				}
				return sealsCached;
			}
		}
		IReadOnlyCollection<string> sealsCached;

		public string ContainerIdentificationNumber => container.ACN_ContainerNumber;

		public IReadOnlyCollection<string> GoodsReferences => Array.Empty<string>();

		public bool ContainerIsFull => container.ACN_EmptyFullIndicator.EqualsIgnoringCase(EmptyFullIndicatorList.Codes.NotEmpty);
	}
}
