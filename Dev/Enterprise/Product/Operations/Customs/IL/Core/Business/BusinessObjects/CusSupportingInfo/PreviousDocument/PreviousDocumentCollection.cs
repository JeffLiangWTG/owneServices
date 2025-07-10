using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class PreviousDocumentCollection : CusSupportingInfoCollection<PreviousDocument>
	{
		public PreviousDocumentCollection(BusinessObject parent)
			: base(parent, Common.IL.CusSupportingInfoTypeList.Codes.PreviousDocument)
		{
		}
	}
}
