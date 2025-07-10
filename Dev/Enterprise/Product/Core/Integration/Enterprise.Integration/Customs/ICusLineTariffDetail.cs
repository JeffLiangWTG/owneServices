using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusLineTariffDetail
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZGuid BZ_ParentID { get; set; }
			ZString BZ_ParentTableCode { get; set; }
			ZDecimal BZ_Qty1 { get; set; }
			ZString BZ_Tariff { get; set; }
			ZString BZ_Type { get; set; }
			ZString BZ_UQ1 { get; set; }
			ZDecimal BZ_Value { get; set; }
		}
	}
}