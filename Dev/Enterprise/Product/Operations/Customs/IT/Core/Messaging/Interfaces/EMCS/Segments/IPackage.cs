using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IPackage
{
	ZString KindOfPackages { get; }

	ZInt NumberOfPackages { get; }

	ZString CommercialSealIdentification { get; }

	ZString SealInformation { get; }

	ZString SealInformationLanguage { get; }
}
