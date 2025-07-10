using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStorageSupportingDocumentCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>
		where T : TemporaryStorageSupportingDocument
	{
	}

	public class TemporaryStorageSupportingDocumentCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, ITemporaryStorageSupportingDocumentCollection<T>
		where T : TemporaryStorageSupportingDocument
	{
		const int maxAllowedSupportingDocuments = 99;

		public TemporaryStorageSupportingDocumentCollection(BusinessObject parent) : base(parent, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument)
		{
			MaxCountValidationEnable(maxAllowedSupportingDocuments);
		}

		protected override bool AllowNewCore => Count < MaxCount;
	}
}
