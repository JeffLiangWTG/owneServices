using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierIncidentTransportEquipmentData : INCTSTransportEquipment, IEquatable<NCTSPrettierIncidentTransportEquipmentData>
	{
		public NCTSPrettierIncidentTransportEquipmentData(ZString transportEquipmentSequenceNumber, ZString transportEquipmentContainerIdentificationNumber, ZString transportEquipmentNumberOfSeals, IReadOnlyCollection<INCTSSeal> transportEquipmentSeals, IReadOnlyCollection<INCTSGoodsReference> goodsReferences)
		{
			SequenceNumber = int.TryParse(transportEquipmentSequenceNumber, out var sequenceNo) ? sequenceNo : SequenceNumber;
			ContainerIdentificationNumber = transportEquipmentContainerIdentificationNumber;
			NumberOfSeals = int.TryParse(transportEquipmentNumberOfSeals, out var noOfSeals) ? noOfSeals : NumberOfSeals;
			Seals = transportEquipmentSeals;
			GoodsReferences = goodsReferences;
		}

		public override bool Equals(object obj)
			=> obj is NCTSPrettierIncidentTransportEquipmentData other && Equals(other);

		public bool Equals(NCTSPrettierIncidentTransportEquipmentData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return SequenceNumber.Equals(other.SequenceNumber)
				&& ContainerIdentificationNumber.Equals(other.ContainerIdentificationNumber)
				&& NumberOfSeals.Equals(other.NumberOfSeals)
				&& Seals.SequenceEqual(other.Seals)
				&& GoodsReferences.SequenceEqual(other.GoodsReferences);
		}

		public override int GetHashCode() => (SequenceNumber, ContainerIdentificationNumber, NumberOfSeals, Seals, GoodsReferences).GetHashCode();

		public int SequenceNumber { get; }

		public string ContainerIdentificationNumber { get; }

		public int? NumberOfSeals { get; }

		public IReadOnlyCollection<INCTSSeal> Seals { get; }

		public IReadOnlyCollection<INCTSGoodsReference> GoodsReferences { get; }
	}
}
