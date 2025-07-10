using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class SupportingDocumentMetaDataCollection : IL.Business.SupportingDocumentMetaDataCollection
	{
		public SupportingDocumentMetaDataCollection(BusinessObject parent)
			: base(parent)
		{
		}

		public new SupportingDocumentMetaData this[int index] => (SupportingDocumentMetaData)base[index];

		public new SupportingDocumentMetaData AddNew() => (SupportingDocumentMetaData)base.AddNew();
	}
}
