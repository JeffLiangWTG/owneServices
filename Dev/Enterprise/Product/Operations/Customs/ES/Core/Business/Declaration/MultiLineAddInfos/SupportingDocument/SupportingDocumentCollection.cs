using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class SupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection
	{
		public SupportingDocumentCollection(BusinessObject parent)
			: base(parent)
		{
		}
		public new SupportingDocument this[int i] => (SupportingDocument)base[i];

		public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();

		public new SupportingDocument AddNew(System.Type bizOType) => (SupportingDocument)base.AddNew(bizOType);

		public new ESSupportingDocumentHelper Helper => (ESSupportingDocumentHelper)base.Helper;

		protected override EUSupportingDocumentHelper GetNewSupportingDocumentHelper(ICanBeImportOrExport parentImportOrExpot)
		{
			return new ESSupportingDocumentHelper(parentImportOrExpot);
		}
	}
}
