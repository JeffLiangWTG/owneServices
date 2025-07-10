using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public interface IAutHeaderWithCusOfficeProvider
{
	CusAuthorisationHeader Authorization { get; }
	ZString AuthorizationNumber { get; }
	ZString CustomsOffice { get; }
	ZBool IsExport { get; }
}
