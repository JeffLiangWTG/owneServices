using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class SupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection
	{
		public SupportingDocumentCollection(BusinessObject parent) : base(parent)
		{
		}

		public new SupportingDocument this[int i] => (SupportingDocument)base[i];

		public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();
	}
}
