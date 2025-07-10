using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IPackage
	{
		ZString KindOfPackages { get; set; }

		ZString NumberOfPackages { get; set; }

		ZString CommercialSealIdentification { get; set; }

		ZString SealInformation { get; set; }
	}
}
