using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public class ItineraryCountryCollection : EU.Business.ItineraryCountryCollection
	{
		public ItineraryCountryCollection(BusinessObject parent) : base(parent)
		{
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			MarkAsNeedingValidation();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(ItineraryCountry);

		public new ItineraryCountry this[int index] => (ItineraryCountry)base[index];

		public new ItineraryCountry AddNew() => (ItineraryCountry)base.AddNew();

		public new ItineraryCountry AddNew(Type type) => (ItineraryCountry)base.AddNew(type);

		protected override BusinessObject AddNewCore()
		{
			return AddNewCore(typeof(ItineraryCountry));
		}
	}
}
