using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public interface IDateOfValuationProvider
{
	ZDateTime DateOfValuation { get; }
}
