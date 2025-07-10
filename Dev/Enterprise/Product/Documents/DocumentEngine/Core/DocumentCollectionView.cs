using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	public class DocumentCollectionView : DeliverableCollectionView
	{
		public DocumentCollectionView(IDeliverableCollection deliverables, DocDeliveryContactCollection recipients)
			: base(deliverables, recipients)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return !(element is IeDoc);
		}
	}
}
