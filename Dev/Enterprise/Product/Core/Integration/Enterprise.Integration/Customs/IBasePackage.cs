using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBasePackage
		{
			ZGuid PK { get; }
			ZGuid CW_CR_HouseContainer { get; set; }
			ZInt CW_InBondPackQty { get; set; }
			ZString CW_MarksAndNos { get; set; }
			ZInt CW_OuterPacks { get; set; }
			ZInt CW_PackQty { get; set; }
			ZString CW_PackType { get; set; }
			ZString CW_ShippingSymbol { get; set; }
		}
	}
}
