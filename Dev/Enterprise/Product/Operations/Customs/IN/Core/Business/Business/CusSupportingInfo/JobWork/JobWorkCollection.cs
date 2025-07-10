using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class JobWorkCollection : CusSupportingInfoCollection<JobWork>
{
	public JobWorkCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.JobWork)
	{
		const int maxAllowed = 99;
		this.EnableMaxCountValidationWithMessageError(maxAllowed, warnAtHalfway: false, Res.GetString("48C91BBC-D087-4113-9717-E628DA7fBE8F1", "The maximum number of {0} Job Works has been exceeded.", maxAllowed));
	}
}
