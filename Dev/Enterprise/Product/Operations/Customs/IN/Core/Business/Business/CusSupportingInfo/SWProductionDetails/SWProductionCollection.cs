using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class SWProductionDetailsCollection : CusSupportingInfoCollection<SWProduction>
{
	public SWProductionDetailsCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.SingleWindowProduction)
	{
		const int maxAllowed = 9999;
		this.EnableMaxCountValidationWithMessageError(maxAllowed, warnAtHalfway: false, Res.GetString("8A4D83D2-9082-4C0C-8567-8D8569ED33FA", "The maximum number of {0} SW Production Details has been exceeded.", maxAllowed));
	}
}

