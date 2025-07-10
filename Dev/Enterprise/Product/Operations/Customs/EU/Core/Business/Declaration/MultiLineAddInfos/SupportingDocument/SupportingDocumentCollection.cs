using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface ISupportingDocumentCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>
		where T : SupportingDocument
	{
		new T this[int index] { get; }
		new T AddNew();
		T AddNew(ZString code, ZString referenceNumber);
		T AddNewInvoiceDocumentAndCopyDataFromInvoice(ZString code);
		T AddNewIfNotExsistWithSameCodeAndReference(ZString code, ZString referenceNumber);
		void DeleteAllDocumentsHavingCode(ZString code);
		EUSupportingDocumentHelper Helper { get; }
	}

	public class SupportingDocumentCollection : SupportingDocumentCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(BusinessObject parent) : base(parent)
		{
		}
	}
}
