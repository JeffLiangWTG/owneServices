using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class SharedValueMapResolverProvider
{
	public static IHeaderOrLineValueMapResolver<NctsDepartureMovementHeader, NctsDepartureCargoDesc, ZString> GetCountryOfDestinationMapResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsDepartureMovementHeader, NctsDepartureCargoDesc, ZString>()
			.SetLineGetter(x => x.Header.Bills.SelectMany(b => b.GoodsItems))
			.SetHeaderGetter(x => x.MoveHeader)
			.SetHeaderValueGetter(m => m.BM_RL_NKDestinationPort)
			.SetLineValueGetter(g => g.BY_RN_NKCountryOfDestination)
			.IsEmptyWhen(x => x.IsEmpty)
			.UseFallBack()
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsDepartureMovementHeader, NctsBill, ZString> GetCountryOfDispatchMapHeaderResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsDepartureMovementHeader, NctsBill, ZString>()
			.SetLineGetter(x => x.Header.Bills.Cast<NctsBill>())
			.SetHeaderGetter(x => x.Header.MovementHeader)
			.SetLineValueGetter(x => GetCountryOfDispatchMapLineResolver().GetValueForHeader(x))
			.SetHeaderValueGetter(x => x.BM_RN_NKCountryOfDispatch)
			.IsEmptyWhen(x => x.IsEmpty)
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsBill, NctsDepartureCargoDesc, ZString> GetCountryOfDispatchMapLineResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsBill, NctsDepartureCargoDesc, ZString>()
			.SetLineGetter(x => x.GoodsItems)
			.SetHeaderGetter(x => x.Bill)
			.SetLineValueGetter(x => x.BY_RN_NKCountryOfDispatch)
			.SetHeaderValueGetter(x => x.B0_RN_NKCountryOfExport.FallbackTo(x.Header.MovementHeader.BM_RN_NKCountryOfDispatch))
			.IsEmptyWhen(x => x.IsEmpty)
			.UseFallBack()
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsDepartureMovementHeader, NctsBill, ZString> GetCountryOfDestinationMapHeaderResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsDepartureMovementHeader, NctsBill, ZString>()
			.SetLineGetter(x => x.Header.Bills.Cast<NctsBill>())
			.SetHeaderGetter(x => x.Header.MovementHeader)
			.SetLineValueGetter(x => GetCountryOfDestinationMapLineResolver().GetValueForHeader(x))
			.SetHeaderValueGetter(x => x.BM_RL_NKDestinationPort)
			.IsEmptyWhen(x => x.IsEmpty)
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsBill, NctsDepartureCargoDesc, ZString> GetCountryOfDestinationMapLineResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsBill, NctsDepartureCargoDesc, ZString>()
			.SetLineGetter(x => x.GoodsItems)
			.SetHeaderGetter(x => x.Bill)
			.SetLineValueGetter(x => x.BY_RN_NKCountryOfDestination)
			.SetHeaderValueGetter(x => x.B0_RN_NKCountryOfDestination.FallbackTo(x.Header.MovementHeader.BM_RL_NKDestinationPort))
			.IsEmptyWhen(x => x.IsEmpty)
			.UseFallBack()
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsDepartureMovementHeader, NctsDepartureCargoDesc, ZString> GetTransportChargesMethodOfPaymentMapResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsDepartureMovementHeader, NctsDepartureCargoDesc, ZString>()
			.SetLineGetter(x => x.Header.Bills.SelectMany(b => b.GoodsItems))
			.SetHeaderGetter(x => x.MoveHeader)
			.SetHeaderValueGetter(m => m.BM_MethodOfPayment)
			.SetLineValueGetter(g => g.BY_TransportChargesMethodOfPayment)
			.IsEmptyWhen(x => x.IsEmpty)
			.UseFallBack()
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsDepartureMovementHeader, NctsBill, ZString> GetTransportBillMethodOfPaymentMapResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsDepartureMovementHeader, NctsBill, ZString>()
			.SetLineGetter(x => x.Header.Bills)
			.SetHeaderGetter(x => x.Header.MovementHeader)
			.SetHeaderValueGetter(x => x.BM_MethodOfPayment)
			.SetLineValueGetter(x => x.B0_TransportPaymentMethod)
			.IsEmptyWhen(x => x.IsEmpty)
			.UseFallBack()
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsHeader, NctsBill, EoriOrTcuTraderWrapper> GetConsigneeMapResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsHeader, NctsBill, EoriOrTcuTraderWrapper>()
			.SetLineGetter(x => x.Bills)
			.SetHeaderGetter(x => x.Header)
			.SetHeaderValueGetter(x => GetTrader(x.Consignee))
			.SetLineValueGetter(x => GetTrader(x.Consignee))
			.WithEqualityComparer(new TraderEqualityComparer())
			.UseFallBack()
			.Build();
	}

	public static IHeaderOrLineValueMapResolver<NctsHeader, NctsBill, IReadOnlyCollection<DepartureMeansOfTransportWrapper>> GetDepartureTransportMeansMapResolver()
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<NctsHeader, NctsBill, IReadOnlyCollection<DepartureMeansOfTransportWrapper>>()
			.SetLineGetter(x => x.Bills)
			.SetHeaderGetter(x => x.Header)
			.SetHeaderValueGetter(x => DepartureMeansOfTransportWrapper.CollectFromMovementHeader(x.MovementHeader))
			.SetLineValueGetter(DepartureMeansOfTransportWrapper.CollectFromBill)
			.WithEqualityComparer(new DepartureMeansOfTransportCollectionComparer())
			.IsEmptyWhen(x => x.Count == 0)
			.UseFallBack()
			.Build();
	}

	static EoriOrTcuTraderWrapper GetTrader(JobDocAddress jobDocAddress) => jobDocAddress is null || jobDocAddress.IsEmpty
		? null
		: new EoriOrTcuTraderWrapper(jobDocAddress);
}
