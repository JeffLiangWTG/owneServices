using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface ITransportEquipment
	{
		ZString ContainerNo { get; }
		ZString SealNo { get; }
		ZString SecondSealNo { get; }
	}

	class TransportEquipmentWrapper : ITransportEquipment
	{
		TransportEquipmentWrapper(ZString containerNo, ZString sealNo, ZString secondSealNo)
		{
			this.containerNo = containerNo;
			this.sealNo = sealNo;
			this.secondSealNo = secondSealNo;
		}

		public static TransportEquipmentWrapper New(ZString containerNo, ZString sealNo, ZString secondSealNo)
		{
			return new TransportEquipmentWrapper(containerNo, sealNo, secondSealNo);
		}

		ZString ITransportEquipment.ContainerNo => containerNo.StripNewlineCharacters(CDSDataElementsLengths.TransportEquipmentContainerNoMaxLength);
		ZString ITransportEquipment.SealNo => sealNo.StripNewlineCharacters();
		ZString ITransportEquipment.SecondSealNo => secondSealNo.StripNewlineCharacters();

		readonly ZString containerNo;
		readonly ZString sealNo;
		readonly ZString secondSealNo;
	}
}
