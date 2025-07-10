using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierTranshipmentData : INCTSTranshipmentData, IEquatable<NCTSPrettierTranshipmentData>
	{
		public NCTSPrettierTranshipmentData(ZString containerIndicator, ZString transportMeansNationality, ZString transportMeansIdentificationNumber, ZString transportMeansTypeOfIdentification)
		{
			ContainerIndicator = ZBool.ParseSafe(containerIndicator, defaultValue: false);
			TransportMeansNationality = transportMeansNationality;
			TransportMeansIdentificationNumber = transportMeansIdentificationNumber;
			TransportMeansTypeOfIdentification = transportMeansTypeOfIdentification;
		}

		public override bool Equals(object obj)
			=> obj is NCTSPrettierTranshipmentData other && Equals(other);

		public bool Equals(NCTSPrettierTranshipmentData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return ContainerIndicator.Equals(other.ContainerIndicator)
				&& TransportMeansNationality.Equals(other.TransportMeansNationality)
				&& TransportMeansIdentificationNumber.Equals(other.TransportMeansIdentificationNumber)
				&& TransportMeansTypeOfIdentification.Equals(other.TransportMeansTypeOfIdentification);
		}

		public override int GetHashCode() => (ContainerIndicator, TransportMeansNationality, TransportMeansIdentificationNumber, TransportMeansTypeOfIdentification).GetHashCode();

		public bool ContainerIndicator { get; }

		public ZString TransportMeansNationality { get; set; }

		public ZString TransportMeansIdentificationNumber { get; set; }

		public ZString TransportMeansTypeOfIdentification { get; set; }
	}
}
