using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsVoyageDestinationWrapperCollection : NonPersistentBusinessObjectCollection<CustomsVoyageDestinationWrapper>
	{
		public CustomsVoyageDestinationWrapperCollection(CustomsJobVoyageWrapper voyageWrapper)
			: base(voyageWrapper.Factory)
		{
			this.voyageWrapper = voyageWrapper;

			foreach (VoyageDestination destination in voyageWrapper.Voyage.Destinations)
			{
				Add(new CustomsVoyageDestinationWrapper(voyageWrapper, destination));
			}
			voyageWrapper.Voyage.Destinations.CountChanged += new CollectionCountChangedEventHandler(Destinations_CountChanged);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomsVoyageDestinationWrapper(voyageWrapper, null);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region Implementation

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("The collection is a readonly wrapper of Voyage.Destinations");
		}

		void Destinations_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				Add(new CustomsVoyageDestinationWrapper(voyageWrapper, e.BizObject as VoyageDestination));
			}
			else
			{
				CustomsVoyageDestinationWrapper wrapperToRemove = FindDestinationWrapper(e.BizObject as VoyageDestination);
				if (wrapperToRemove != null)
				{
					Remove(wrapperToRemove);
				}
			}
		}

		CustomsVoyageDestinationWrapper FindDestinationWrapper(VoyageDestination destination)
		{
			foreach (CustomsVoyageDestinationWrapper wrapper in this)
			{
				if (wrapper.Destination == destination)
				{
					return wrapper;
				}
			}
			return null;
		}

		readonly CustomsJobVoyageWrapper voyageWrapper;

		#endregion
	}
}
