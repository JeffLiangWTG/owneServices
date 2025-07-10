using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CC044ADeclarationWrapper : DeclarationWrapper, ICC044ADeclaration
	{
		public CC044ADeclarationWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public IUnloadingRemarkInterface UnloadingRemark => CachedValueHelper.GetValue(ref unloadingRemark, () => new UnloadingRemarkWrapper(nctsHeader.UnloadingRemark));
		CachedValue<IUnloadingRemarkInterface> unloadingRemark;

		public ZString HeaderUnloadingNotes => nctsHeader.HeaderUnloadingNotes;

		public ZString HeaderUnloadingNotesLanguage => ZString.Empty;

		public IReadOnlyCollection<IControlResult> ControlResultList => controlResultList ?? (controlResultList = nctsHeader.ResultsOfControlCollection.Cast<CusAddInfo<ResultsOfControlAddInfo>>().Select(x => new ControlResultWrapper(x.Data)).ToArray());
		IReadOnlyCollection<IControlResult> controlResultList;

		public IReadOnlyCollection<IEnRouteEvent> EnRouteEvents
		{
			get
			{
				if (enRouteEvents == null)
				{
					enRouteEvents = new List<EnRouteEventWrapper>();
					var enRouteTransshipments = nctsHeader.EnRouteTransshipments;
					var enRouteTransshipmentsCount = enRouteTransshipments.Count;

					var enRouteIncidents = nctsHeader.EnRouteIncidents;
					var enRouteIncidentsCount = enRouteIncidents.Count;

					var enRouteSeals = nctsHeader.EnRouteSeals;
					var enRouteSealsCount = enRouteSeals.Count;

					var maxCount = new int[] { enRouteIncidentsCount, enRouteTransshipmentsCount, enRouteSealsCount }.Max();

					for (var i = 0; i < maxCount; i++)
					{
						var transshipment = i < enRouteTransshipmentsCount ? enRouteTransshipments[i] : null;
						var incident = i < enRouteIncidentsCount ? enRouteIncidents[i] : null;
						var seal = i < enRouteSealsCount ? enRouteSeals[i] : null;
						enRouteEvents.Add(new EnRouteEventWrapper(transshipment, incident, seal));
					}
				}

				return enRouteEvents;
			}
		}
		List<EnRouteEventWrapper> enRouteEvents;

		public ZString IdentityOfMeansOfTransportAtDeparture => nctsHeader.UnloadedMeansOfTransportAtDepartureIdentity;

		public ZString IdentityOfMeansOfTransportAtDepartureLanguage => ZString.Empty;

		public ZString NationalityOfMeansOfTransportAtDeparture => nctsHeader.UnloadedMeansOfTransportAtDepartureNationality;

		public ZInt TotalNumberOfItems => nctsHeader.UnloadingMovementHeader.TotalNumberOfItems;

		public ZLong TotalNumberOfPackages => nctsHeader.UnloadingMovementHeader.TotalNumberOfPackages;

		public ZDecimal TotalGrossMass => nctsHeader.UnloadingMovementHeader.TotalGrossMassInKilograms;

		public ITrader DestinationTrader => CachedValueHelper.GetValue(ref destinationTrader, () => TraderWrapper.New(nctsHeader.DestinationTrader, false, IsAddressExtended));
		CachedValue<ITrader> destinationTrader;

		public IReadOnlyCollection<ICommonGoodsItem> ExpectedGoodsItems
		{
			get
			{
				if (expectedGoodsItems == null)
				{
					expectedGoodsItems = nctsHeader.ArrivalMovementHeader?.GoodsItems.Select(x => new CommonGoodsItemWrapper(x)).OrderBy(x => x.ItemNumber).ToArray() ?? Array.Empty<ICommonGoodsItem>();
				}
				return expectedGoodsItems;
			}
		}
		IReadOnlyCollection<ICommonGoodsItem> expectedGoodsItems;
	}
}
