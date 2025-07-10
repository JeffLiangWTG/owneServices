using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.AES
{
	public class TransportEquipmentWithSealsBaseProvider : ITransportEquipmentWithSeals
	{
		protected static IReadOnlyCollection<string> GetSeals(params ZString[] seals)
		{
			var list = new List<string>();
			foreach (var seal in seals)
			{
				if (!seal.IsEmpty)
				{
					list.Add(seal);
				}
			}
			return list.ToArray();
		}

		public string ContainerIdentificationNumber { get; set; }

		public IReadOnlyCollection<string> Seals
		{
			get => seals ?? (seals = Array.Empty<string>());
			set => seals = value;
		}
		IReadOnlyCollection<string> seals;

		public IReadOnlyCollection<string> GoodsReferences
		{
			get => goodsReferences ?? (goodsReferences = Array.Empty<string>());
			set => goodsReferences = value;
		}
		IReadOnlyCollection<string> goodsReferences;

		public bool ContainerIsFull => false;
	}
}
