using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class PreviousDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection
		, Integration.Customs.GB.IPreviousDocumentCollection
	{
		public PreviousDocumentCollection(BusinessObject parent)
			: base(parent)
		{ }

		public new PreviousDocument this[int i] => (PreviousDocument)base[i];

		public new PreviousDocument AddNew() => (PreviousDocument)base.AddNew();

		#region IPreviousDocumentCollection Implements

		Integration.Customs.GB.IPreviousDocument Integration.Customs.GB.IPreviousDocumentCollection.this[int index] => this[index];

		Integration.Customs.GB.IPreviousDocument Integration.Customs.GB.IPreviousDocumentCollection.AddNew() => AddNew();

		#endregion
	}
}
