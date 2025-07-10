using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class PreviousDocumentCollection : Customs.Business.CusSupportingInfoCollection<PreviousDocument>
	{
		public PreviousDocumentCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument)
		{
		}
	}
}
