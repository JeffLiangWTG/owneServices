using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public interface ISupportingDocumentCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>
	where T : SupportingDocument
	{
	}

	public class SupportingDocumentCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, ISupportingDocumentCollection<T>
		where T : SupportingDocument
	{
		public SupportingDocumentCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument)
		{
		}
	}
}
