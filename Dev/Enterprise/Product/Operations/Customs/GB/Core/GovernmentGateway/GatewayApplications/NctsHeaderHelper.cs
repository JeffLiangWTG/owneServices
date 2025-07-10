using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.GovernmentGateway
{
	public static class NctsHeaderHelper
	{
		public static bool IsConsignorDefinedAtGoodsItemLevel(NctsHeader nctsHeader) => nctsHeader.IsDepartureMovement && nctsHeader.GetDepartureGoodsItems().Any(x => !x.Consignor.IsEmpty);

		public static bool IsConsigneeDefinedAtGoodsItemLevel(NctsHeader nctsHeader) => nctsHeader.IsDepartureMovement && nctsHeader.GetDepartureGoodsItems().Any(x => !x.Consignee.IsEmpty);

		public static bool HasSecurityAtGoodsItemLevel(NctsHeader nctsHeader) => nctsHeader.IsDepartureMovement && nctsHeader.HasSecurityAtGoodsItemLevel;
	}
}
