using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISynchroniserDeletableBusinessObject
	{
		event EventHandler Deleted;
	}

	public class DestinationDeletableBusinessObjectSynchroniser : BusinessObjectSynchroniser
	{
		public DestinationDeletableBusinessObjectSynchroniser(ISynchroniserDeletableBusinessObject destination, BusinessObject source)
			: base((BusinessObject)destination, source)
		{
			destination.Deleted += new EventHandler(Destination_Deleted);
		}

		#region Implementation

		void Destination_Deleted(object sender, EventArgs e)
		{
			UnHookSynchronisers();
			SetEnabled(false, DetectEnabled);
			OnDestinationDeleted(e);
		}

		public event EventHandler DestinationDeleted;
		protected virtual void OnDestinationDeleted(EventArgs e)
		{
			if (DestinationDeleted != null)
			{
				DestinationDeleted(this, e);
			}
		}

		public static bool IsSenderRefreshingByDataRefreshBus(object sender)
		{
			return (sender as BusinessObjectCollection)?.IsRefreshingByDataRefreshBus ?? false;
		}

		#endregion
	}
}
