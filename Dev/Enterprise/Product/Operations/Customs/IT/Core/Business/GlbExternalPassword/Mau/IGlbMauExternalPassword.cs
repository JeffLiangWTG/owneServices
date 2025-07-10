using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business;

public interface IGlbMauExternalPassword : IGlbExternalPassword
{
	ZString DeclarantTaxNumber { get; }
}
