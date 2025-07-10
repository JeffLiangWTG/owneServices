using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CusSupportingDocumentCollectionView : BusinessObjectCollectionView<CusSupportingDocument>
	{
		public CusSupportingDocumentCollectionView(BusinessObjectCollection collectionToFilter) : base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var cusSupportingDocument = element as CusSupportingDocument;
			return !cusSupportingDocument?.IsCertificateOfOrigin ?? false;
		}
	}
}
