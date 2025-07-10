using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackLineStatus : IPackLineStatus, Integration.Customs.AU.IPackLineStatus
	{
		ZString IPackLineStatus.GetCustomsStatusDescription(ZString oceanBill, ZString houseBill, ZString containerNumber)
		{
			ZString result = ZString.Empty;

			return result;
		}
	}
}
