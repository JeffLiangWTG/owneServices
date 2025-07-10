using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class SupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection
		, Integration.Customs.GB.ISupportingDocumentCollection
	{
		public SupportingDocumentCollection(BusinessObject parent)
			: base(parent)
		{ }

		public new SupportingDocument this[int i] => (SupportingDocument)base[i];

		public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();

		#region ISupportingDocumentCollection Implements

		Integration.Customs.GB.ISupportingDocument Integration.Customs.GB.ISupportingDocumentCollection.this[int index] => this[index];

		Integration.Customs.GB.ISupportingDocument Integration.Customs.GB.ISupportingDocumentCollection.AddNew() => AddNew();

		#endregion
	}
}
