using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

public interface IDateOfValuationProvider
{
	ZDateTime DateOfValuation { get; }
}
