using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class PreviousDocumentCollection : Customs.Business.CusSupportingInfoCollection<PreviousDocument>
	{
		public PreviousDocumentCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PreviousDocument)
		{
		}
	}
}
