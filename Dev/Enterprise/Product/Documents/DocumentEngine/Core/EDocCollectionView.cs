using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	public class EDocCollectionView : DeliverableCollectionView
	{
		public EDocCollectionView(IDeliverableCollection deliverables, DocDeliveryContactCollection recipients)
			: base(deliverables, recipients)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return element is IeDoc;
		}

		internal void ResetEDocs()
		{
			ResetEDocsToClearAllNotifications();

			using (SuspendRecipientsValidation())
			{
				foreach (IDeliverable edoc in this)
				{
					using (new DisposableAction(() => edoc.ShouldPrintByDefault = true, () => edoc.ShouldPrintByDefault = false))
					{
						edoc.IncludedInPrint = true;
					}
				}
			}
		}

		void ResetEDocsToClearAllNotifications()
		{
			foreach (IDeliverable edoc in this)
			{
				edoc.IncludedInPrint = false;
			}
		}
	}
}
