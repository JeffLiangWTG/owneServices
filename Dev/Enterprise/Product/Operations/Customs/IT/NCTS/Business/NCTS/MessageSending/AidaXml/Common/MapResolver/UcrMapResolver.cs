using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

static class UcrMapResolver
{
	#region ResolveUcr Overloads

	public static ZString ResolveUcr(this NctsHeader header) => header.MovementHeader.ResolveUcr();
	public static ZString ResolveUcr(this CusInBondMoveHeader header) => UcrMapHeaderResolver.GetValueForHeader(header);
	public static ZString ResolveUcr(this NctsBill bill) => UcrMapHeaderResolver.GetValueForLine(bill);
	public static ZString ResolveUcr(this NctsCommonCargoDesc goodsItem) => UcrMapItemResolver.GetValueForLine(goodsItem);

	#endregion

	#region Value Map Resolvers

	static IHeaderOrLineValueMapResolver<CusInBondMoveHeader, NctsBill, ZString> UcrMapHeaderResolver =>
		HeaderOrLineValueMapResolverFluent
			.Configure<CusInBondMoveHeader, NctsBill, ZString>()
			.SetLineGetter(x => x.Header.Bills.Cast<NctsBill>())
			.SetHeaderGetter(x => x.Header.MovementHeader)
			.SetLineValueGetter(x => UcrMapItemResolver.GetValueForHeader(x))
			.SetHeaderValueGetter(x => x.BM_UniqueConsignmentReference)
			.IsEmptyWhen(x => x.IsEmpty)
			.Build();

	static IHeaderOrLineValueMapResolver<NctsBill, NctsCommonCargoDesc, ZString> UcrMapItemResolver =>
		HeaderOrLineValueMapResolverFluent
			.Configure<NctsBill, NctsCommonCargoDesc, ZString>()
			.SetLineGetter(x => x.GoodsItems)
			.SetHeaderGetter(x => (NctsBill)x.Bill)
			.SetLineValueGetter(x => x.BY_CommercialReferenceNumber)
			.SetHeaderValueGetter(x => x.B0_ReferenceID.FallbackTo(x.Header.MovementHeader.BM_UniqueConsignmentReference))
			.IsEmptyWhen(x => x.IsEmpty)
			.UseFallBack()
			.Build();

	#endregion
}
