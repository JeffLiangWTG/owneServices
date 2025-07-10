using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInBondMoveHeader
		{
			ZGuid PK { get; }
			ZGuid BM_BH { get; set; }
			ZString BM_SubApplicationCode { get; set; }
			ZGuid BM_OA_WarehouseAddress { get; set; }
			ZString BM_CustomsStatus { get; set; }
			ZString BM_WarehouseTransactionStatus { get; set; }
		}
	}
}
