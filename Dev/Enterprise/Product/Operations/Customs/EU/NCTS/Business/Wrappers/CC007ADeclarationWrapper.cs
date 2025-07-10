using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CC007ADeclarationWrapper : DeclarationWrapper, ICC007ADeclaration
	{
		public CC007ADeclarationWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{ }

		public ITrader Destination => CachedValueHelper.GetValue(ref destination, () => TraderWrapper.New(nctsHeader.DestinationTrader, false, IsAddressExtended));
		CachedValue<ITrader> destination;

		public ZString ArrivalNotificationPlace => ArrivalNotificationPlaceCore;
		protected virtual ZString ArrivalNotificationPlaceCore => nctsHeader.DeclarationPlace;

		public ZString ArrivalNotificationPlaceLanguage => ZString.Empty;

		public ZString ArrivalAgreedLocationOfGoodsCode => nctsHeader.ArrivalMovementHeader.BM_LocationOfGoodsCode;

		public ZString ArrivalAgreedLocationOfGoodsLanguage => ZString.Empty;

		public ZString DialogLanguageIndicatorAtDestination => ZString.Empty;

		public ZString ArrivalNotificationDate
		{
			get
			{
				var entryDate = nctsHeader.ArrivalMovementHeader.BM_EntryDate;
				var arrivalDate = entryDate.IsEmpty ? ZDateTime.Now : entryDate;
				return arrivalDate.GetLongDate();
			}
		}

		public ZBool IsSimplifiedArrivalProcedure => nctsHeader.ArrivalMovementHeader.IsSimplifiedNctsProcedure;

		public ZString CustomsPresentationOfficeRefNumber => nctsHeader.PresentationCustomsOffice;

		public ZString CustomsSubPlace => nctsHeader.ArrivalMovementHeader.BM_CustomsSubPlace;

		public ZString DeclarantTIN => CachedValueHelper.GetValue(ref declarantTIN, () => nctsHeader.ArrivalDeclarantTIN());
		CachedValue<ZString> declarantTIN;

		public IReadOnlyCollection<IEnRouteEvent> EnRouteEvents
		{
			get
			{
				if (enRouteEvents == null)
				{
					enRouteEvents = new List<EnRouteEventWrapper>();
					var incidentEventCount = nctsHeader.EnRouteIncidents.Count;
					var transhipEventCount = nctsHeader.EnRouteTransshipments.Count;
					var sealEventCount = nctsHeader.EnRouteSeals.Count;
					var maxCount = new[] { incidentEventCount, transhipEventCount, sealEventCount }.Max();

					for (var i = 0; i < maxCount; i++)
					{
						var incident = i < incidentEventCount ? nctsHeader.EnRouteIncidents.ElementAt(i) : null;
						var tranship = i < transhipEventCount ? nctsHeader.EnRouteTransshipments.ElementAt(i) : null;
						var seal = i < sealEventCount ? nctsHeader.EnRouteSeals.ElementAt(i) : null;

						enRouteEvents.Add(new EnRouteEventWrapper(tranship, incident, seal));
					}
				}
				return enRouteEvents;
			}
		}
		List<EnRouteEventWrapper> enRouteEvents;
	}
}
