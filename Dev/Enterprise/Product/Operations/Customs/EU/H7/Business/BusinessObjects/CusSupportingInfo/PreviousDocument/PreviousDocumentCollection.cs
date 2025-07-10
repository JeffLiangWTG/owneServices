using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public interface IPreviousDocumentCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>
		where T : PreviousDocument
	{
	}

	public class PreviousDocumentCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, IPreviousDocumentCollection<T>
		where T : PreviousDocument
	{
		public PreviousDocumentCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument)
		{
		}
	}
}
