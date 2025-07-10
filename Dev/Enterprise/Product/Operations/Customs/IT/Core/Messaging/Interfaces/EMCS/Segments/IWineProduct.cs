using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IWineProduct
{
	ZInt WineProductCategory { get; }
	ZString WineGrowingZoneCode { get; }
	ZString ThirdCountryOfOrigin { get; }
	ZString OtherInformation { get; }
	ZString OtherInformationLanguage { get; }
	IWineOperation WineOperation { get; }
	IEnumerable<IWineOperation> WineOperations { get; }
}

public interface IWineOperation
{
	ZInt TreatmentCode { get; }
}
