using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public interface IAdditionalDocumentCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>
		where T : AdditionalDocument
	{
	}

	public class AdditionalDocumentCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, IAdditionalDocumentCollection<T>
		where T : AdditionalDocument
	{
		public AdditionalDocumentCollection(BusinessObject parent)
			: base(parent, H7CusSupportingInfoTypeList.Codes.AdditionalDocument)
		{
		}
	}
}
