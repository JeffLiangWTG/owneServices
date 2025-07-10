using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public sealed class SupportingDocumentCollection : CusSupportingInfoCollection<SupportingDocument>
{
	public SupportingDocumentCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.SupportingDocument)
	{
		const int maxAllowed = 9999;
		this.EnableMaxCountValidationWithMessageError(maxAllowed, warnAtHalfway: false, Res.GetString("D83CECED-2CF0-467D-A702-7A3C5AA8D6D2", "The maximum number of {0} Supporting Documents has been exceeded.", maxAllowed));
	}
}
